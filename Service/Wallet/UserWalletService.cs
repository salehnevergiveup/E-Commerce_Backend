using PototoTrade.Repository.Wallet;
using Stripe.Checkout;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using PototoTrade.DTO.Wallet;
using PototoTrade.Service.Utilities.Response;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.AspNetCore.Identity;
using Stripe;
using PototoTrade.Models.User;
using Stripe.TestHelpers.Treasury;
using PototoTrade.Service.Utilities.Exceptions;
using PototoTrade.DTO.Common;
using PototoTrade.Models;
using Microsoft.Extensions.Options;
using PototoTrade.Repository.User;
using PototoTrade.Repository.Users;

namespace PototoTrade.Service.Wallet
{
    public class UserWalletService
    {

        private readonly WalletRepository _walletRepository;
        private readonly UserAccountRepository _userAccountRepository;
        private readonly StripeSettings _stripeSettings;

        private readonly WalletTransactionRepository _walletTransactionRepository;

        public UserWalletService(WalletRepository walletRepository, WalletTransactionRepository walletTransactionRepository, IOptions<StripeSettings> stripeSettings, UserAccountRepository userAccountRepository)
        {
            _walletRepository = walletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _stripeSettings = stripeSettings.Value;
            _userAccountRepository = userAccountRepository;
        }

        // 1. Get User Wallet Balance
        public async Task<ResponseModel<WalletBalanceDTO?>> GetWalletBalanceAsync(ClaimsPrincipal userClaims)
        {
            var response = new ResponseModel<WalletBalanceDTO?>
            {
                Success = false,
                Data = null,
                Message = "Failed to retrieve user wallet balance"
            };

            try
            {
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                if (userId == 0)
                {
                    response.Message = "Invalid Request";
                    return response;
                }

                var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                {
                    response.Message = "Wallet cannot be found";
                    return response;
                }

                var walletBalanceDTO = new WalletBalanceDTO
                {
                    //UserId = wallet.UserId,
                    AvailableBalance = wallet.AvailableBalance,
                    OnHoldBalance = wallet.OnHoldBalance
                };
                response.Data = walletBalanceDTO;
                response.Success = true;
                response.Message = "User wallet balance retrieved successfully";
            }
            catch (Exception e)
            {
                response.Message = $"An error occurred: {e.Message}";
            }
            return response;
        }


        // 2. Create Stripe Checkout Session
        public async Task<ResponseModel<string>> CreateTopUpSessionAsync(ClaimsPrincipal userClaims, decimal amount, string currency)
        {
            var response = new ResponseModel<string>{
                Success = false,
                Data = "",
                Message = "Failed to create wallet top up session"
            };

            var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                if (userId == 0){
                    response.Message = "Invalid Request";
                    return response;
                }
            //validate inputs
            if (amount <= 0)
                throw new Exception("Amount must be greater than zero.");

            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null)
                throw new Exception("User wallet not found.");

             var userAccount = await _userAccountRepository.GetUserByIdAsync(userId);
            if (userAccount == null)
            {
                response.Message = "User account not found.";
                return response;
            }

            var userName = userAccount.Name;

            // Generate a unique reference ID for the transaction
            var referenceId = Guid.NewGuid().ToString();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card", "fpx", "grabpay" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = currency,
                            UnitAmount = (long)(amount * 100), // Amount in cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Confirm Top up amount for {userName}'s wallet: \n",
                                Description = "Add funds to your wallet for future purchases."
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = "http://localhost:3000/user/profile", //$"https://yourdomain.com/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = "http://localhost:3000/user/profile",
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId.ToString() },
                    { "referenceId", referenceId },
                    { "transactionType", "wallet-topup" }
                }
            };

            try
            {
                var service = new SessionService();
                var session = await service.CreateAsync(options);
                Console.WriteLine(session.Id);
                response.Data = session.Url;
                response.Success = true;
                response.Message = "Stripe session created: {session.Id}";
                return response;
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"Stripe error: {ex.Message}");
                throw new Exception("Failed to create Stripe Checkout session. Please try again later.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw;
            }
        }

        // 3. Update Wallet Balance After Payment
        public async Task<ResponseModel<bool>> TopUpWalletAsync(string userId, decimal amount) //if doesnt work in future, change to claimsprincipal to fetch userid
        {
            var response = new ResponseModel<bool>
            {
                Success = false,
                Data = false,
                Message = "Failed to topup user wallet balance"
            };

            try{
                 var parsedUserId = int.Parse(userId);
                if (parsedUserId == 0){
                    response.Message = "Invalid Request";
                    return response;
                }
                var wallet = await _walletRepository.GetWalletByUserIdAsync(parsedUserId);
                if (wallet == null){
                    response.Message = "Wallet cannot be found";
                    return response;
                }
                wallet.AvailableBalance += amount;
                wallet.UpdatedAt = DateTime.Now;
                await _walletRepository.UpdateWalletAsync(wallet);

                response.Data = true;
                response.Success = true;
                response.Message = "User wallet updated successfully";

            }
            catch (Exception e)
            {
                response.Message = $"An error occurred: {e.Message}";
            }
            return response;
            }
    

        public async Task<ResponseModel<bool>> RequestRefund(ClaimsPrincipal userClaims, int buyerId, decimal amount)
        {
            var response = new ResponseModel<bool>
            {
                Success = false,
                Data = false,
                Message = "Failed to request refund"
            };

            try
            {
                // Extract userId from ClaimsPrincipal
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                if (userId == 0)
                {
                    response.Message = "Invalid User";
                    return response;
                }

                // Get buyer and seller wallets
                var (buyerWallet, sellerWallet) = await _walletRepository.GetBuyerandSellerWalletByUserId(buyerId, userId);
                if (buyerWallet == null || sellerWallet == null)
                {
                    response.Message = "One or both wallets not found.";
                    return response;
                }
                Console.WriteLine($"{amount}");
                // Update wallet balances
                sellerWallet.AvailableBalance -= amount;
                sellerWallet.OnHoldBalance += amount;
                sellerWallet.UpdatedAt = DateTime.Now;

                buyerWallet.OnHoldBalance += amount;
                buyerWallet.UpdatedAt = DateTime.Now;

                await _walletRepository.UpdateWalletAsync(sellerWallet);
                await _walletRepository.UpdateWalletAsync(buyerWallet);

                // Create and save refund request
                var refundRequest = new RefundRequest
                {
                    BuyerId = buyerId,
                    SellerId = userId,
                    Amount = amount,
                    Status = "Pending", // Set default status as "Pending"
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var succ = await _walletRepository.CreateRefundRequest(refundRequest);

                Console.WriteLine($"RefundRequestId after saving: {succ}");

                if (succ == 0)
                {
                    response.Message = "Failed to save refund request.";
                    return response;
                }

                // Set response to success
                response.Data = true;
                response.Success = true;
                response.Message = $"Refund requested successfully. RefundRequestId: {succ}";
            }
            catch (Exception e)
            {
                response.Message = $"An error occurred: {e.Message}";
                Console.WriteLine($"Error: {e}");
            }

            return response;
        }


        public async Task RefundAmount( int sellerUserId, int buyerUserId, decimal refundAmount)
        {
            var sellerWallet = await _walletRepository.GetWalletByUserIdAsync(sellerUserId);
            var buyerWallet = await _walletRepository.GetWalletByUserIdAsync(buyerUserId);

            if (sellerWallet == null || buyerWallet == null)
            {
                throw new CustomException<GeneralMessageDTO>(
                    ExceptionEnum.GetException("WALLET_NOT_FOUND"),
                    new GeneralMessageDTO
                    {
                        Message = "Seller or buyer wallet not found.",
                        Success = false
                    }
                );
            }


            sellerWallet.OnHoldBalance -= refundAmount;
            await _walletRepository.UpdateWalletAsync(sellerWallet);

            var sellerTransaction = new WalletTransaction
            {
                WalletId = sellerWallet.Id,
                Amount = -refundAmount,
                TransactionType = "Product Refund",
                CreatedAt = DateTime.UtcNow
            };
            await _walletTransactionRepository.CreateTransaction(sellerTransaction);

            buyerWallet.AvailableBalance += refundAmount;
            await _walletRepository.UpdateWalletAsync(buyerWallet);

            var buyerTransaction = new WalletTransaction
            {
                WalletId = buyerWallet.Id,
                Amount = refundAmount,
                TransactionType = "Fund from Seller Refund",
                CreatedAt = DateTime.UtcNow
            };
            await _walletTransactionRepository.CreateTransaction(buyerTransaction);
        }

        public async Task<ResponseModel<bool>> RejectRefund(int refundRequestId)
        {
            var response = new ResponseModel<bool>
            {
                Success = false,
                Data = false,
                Message = "Failed to reject refund"
            };

            try
            {
                //fetch refund request from db
                var refundRequest = await _walletRepository.GetRefundRequestByIdAsync(refundRequestId);
                if (refundRequest == null || refundRequest.Status != "Pending")
                {
                    response.Message = "Refund request not found or it is not pending.";
                    return response;
                }

                //get both buyer and seller wallet
                var (buyerWallet, sellerWallet) = await _walletRepository.GetBuyerandSellerWalletByUserId(refundRequest.BuyerId, refundRequest.SellerId);
                if (buyerWallet == null || sellerWallet == null)
                {
                    response.Message = "Failed to allow refund as one or both wallets could not be found.";
                    return response;
                }

                //deduct amount from seller balance

                sellerWallet.OnHoldBalance -= refundRequest.Amount;
                sellerWallet.AvailableBalance += refundRequest.Amount;
                sellerWallet.UpdatedAt = DateTime.Now;

                //add amount to buyer wallet
                buyerWallet.OnHoldBalance -= refundRequest.Amount;
                buyerWallet.UpdatedAt = DateTime.Now;
                //save
                await _walletRepository.UpdateWalletAsync(sellerWallet);
                await _walletRepository.UpdateWalletAsync(buyerWallet);

                //update status
                refundRequest.Status = "Rejected";
                refundRequest.UpdatedAt = DateTime.UtcNow;

                await _walletRepository.UpdateRefundRequestAsync(refundRequest);

                response.Data = true;
                response.Success = true;
                response.Message = "Refund rejected successfully";
            }
            catch (Exception e)
            {
                // Log the error
                Console.WriteLine($"An error occurred while processing the refund: {e.Message}");
                response.Message = $"An error occurred: {e.Message}";
            }

            return response;
        }

        public async Task ChargeFee(int userId, decimal feeAmount)
        {
            var userWallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (userWallet == null)
            {
                throw new CustomException<GeneralMessageDTO>(
                    ExceptionEnum.GetException("WALLET_NOT_FOUND")
                );
            }

            if (userWallet.AvailableBalance < feeAmount)
            {
                throw new CustomException<GeneralMessageDTO>(
                    ExceptionEnum.GetException("INSUFFICIENT_BALANCE")
                );
            }

            userWallet.AvailableBalance -= feeAmount;
            await _walletRepository.UpdateWalletAsync(userWallet);

            var adminWallet = await _walletRepository.GetPlatformWallet();
            if (adminWallet == null)
            {
                throw new CustomException<GeneralMessageDTO>(
                    ExceptionEnum.GetException("PLATFORM_WALLET_NOT_FOUND")
                );
            }

            adminWallet.AvailableBalance += feeAmount;
            await _walletRepository.UpdateWalletAsync(adminWallet);

            var transaction = new WalletTransaction
            {
                WalletId = userWallet.Id,
                Amount = -feeAmount,
                TransactionType = "Charge Fee",
                CreatedAt = DateTime.UtcNow
            };
            await _walletTransactionRepository.CreateTransaction(transaction);

            var AdminAddFundtransaction = new WalletTransaction
            {
                WalletId = adminWallet.Id,
                Amount = feeAmount,
                TransactionType = "Revenue",
                CreatedAt = DateTime.UtcNow
            };
            await _walletTransactionRepository.CreateTransaction(AdminAddFundtransaction);
        }

    }
}

