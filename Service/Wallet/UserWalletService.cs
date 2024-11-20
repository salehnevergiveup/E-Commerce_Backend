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

namespace PototoTrade.Service.Wallet
{
    public class UserWalletService{
    
        private readonly WalletRepository _walletRepository;

        public UserWalletService(WalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        // 1. Get User Wallet Balance
        public async Task<ResponseModel<WalletBalanceDTO?>> GetWalletBalanceAsync(ClaimsPrincipal userClaims)
        {
            var response = new ResponseModel<WalletBalanceDTO?>{
                Success = false,
                Data = null,
                Message = "Failed to retrieve user wallet balance"
            };

            try{
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                if (userId == 0){
                    response.Message = "Invalid Request";
                    return response;
                }

                var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
                if (wallet == null){
                    response.Message = "Wallet cannot be found";
                    return response;
                }

                var walletBalanceDTO = new WalletBalanceDTO
                {
                    UserId = wallet.UserId,
                    AvailableBalance = wallet.AvailableBalance,
                    OnHoldBalance = wallet.OnHoldBalance
                };
                response.Data = walletBalanceDTO;
                response.Success = true;
                response.Message = "User wallet balance retrieved successfully";
            }catch (Exception e){
                response.Message = $"An error occurred: {e.Message}";
            }
            return response;
            }


        // 2. Create Stripe Checkout Session
        public async Task<string> CreateTopUpSessionAsync(int userId, decimal amount, string currency)
        {
            // var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedUserId))
            // {
            //     throw new Exception("Invalid user ID.");
            // }

            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null)
                throw new Exception("User wallet not found.");

            var options = new SessionCreateOptions
            { 
                PaymentMethodTypes = new List<string> { "card" },
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
                                Name = "Wallet Top-Up"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"https://yourdomain.com/api/wallet/payment-success?userId={userId}",
                CancelUrl = "https://yourdomain.com/payment-cancel"
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return session.Url;
        }

        // 3. Update Wallet Balance After Payment
        public async Task<ResponseModel<bool>> TopUpWalletAsync(ClaimsPrincipal userClaims, decimal amount)
        {
            var response = new ResponseModel<bool>{
                Success = false,
                Data = false,
                Message = "Failed to topup user wallet balance"
            };

            try{
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                if (userId == 0){
                    response.Message = "Invalid Request";
                    return response;
                }

                var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
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
 
            }catch (Exception e){
                response.Message = $"An error occurred: {e.Message}";
            }
            return response;
            }
    
        //     var userId
        //     try{
        //     var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
        //     if (wallet == null)
        //     {
        //         return false;                 // Wallet not found, return false or handle accordingly
        //     }
        //     wallet.AvailableBalance += amount;
        //     wallet.UpdatedAt = DateTime.Now;

        //     await _walletRepository.UpdateWalletAsync(wallet);
        //     return true;

        //     }catch(Exception e){
                
        //     // Log the exception (you can use a logging framework here)
        //     Console.WriteLine($"An error occurred while topping up the wallet: {e.Message}");

        //     // Return false in case of failure
        //     return false;
        //     }
            
        // }

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

    
    public async Task<ResponseModel<bool>> AllowRefund(int refundRequestId)
    {
        var response = new ResponseModel<bool>
        {
            Success = false,
            Data = false,
            Message = "Failed to allow refund"
        };

        try
        {
            //fetch refund request from db
            var refundRequest = await _walletRepository.GetRefundRequestByIdAsync(refundRequestId);
            if (refundRequest == null || refundRequest.Status != "Pending")
            {
                Console.WriteLine($"{refundRequest}");
                response.Message = $"Refund request not found or it is not pending. {refundRequest}";
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
            sellerWallet.UpdatedAt = DateTime.Now;

            //add amount to buyer wallet
            buyerWallet.AvailableBalance += refundRequest.Amount;
            buyerWallet.OnHoldBalance -= refundRequest.Amount;
            buyerWallet.UpdatedAt = DateTime.Now;

            //save
            await _walletRepository.UpdateWalletAsync(sellerWallet);
            await _walletRepository.UpdateWalletAsync(buyerWallet);

            //update status
            refundRequest.Status = "Approved";
            refundRequest.UpdatedAt = DateTime.UtcNow;

            await _walletRepository.UpdateRefundRequestAsync(refundRequest);

            response.Data = true;
            response.Success = true;
            response.Message = "Refund allowed successfully";
        }
        catch (Exception e)
        {
            // Log the error
            Console.WriteLine($"An error occurred while processing the refund: {e.Message}");
            response.Message = $"An error occurred: {e.Message}";
        }

        return response;
    }


        public async Task<ResponseModel<bool>> RejectRefund (int refundRequestId)
        {var response = new ResponseModel<bool>
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

        return response;}
        }
        }

