using PototoTrade.Repository.Wallet;
using Stripe.Checkout;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using PototoTrade.DTO.Wallet;

namespace PototoTrade.Service.Wallet
{
    public class UserWalletService{
    
        private readonly WalletRepository _walletRepository;

        public UserWalletService(WalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        // 1. Get User Wallet Balance
        public async Task<WalletBalanceDTO?> GetWalletBalanceAsync(int userId)
        {
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null) return null;

             return new WalletBalanceDTO
                {
                    UserId = wallet.UserId,
                    AvailableBalance = wallet.AvailableBalance,
                    OnHoldBalance = wallet.OnHoldBalance
                };
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
        public async Task<bool> TopUpWalletAsync(int userId, decimal amount)
        {
            try{
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                return false;                 // Wallet not found, return false or handle accordingly
            }
            wallet.AvailableBalance += amount;
            wallet.UpdatedAt = DateTime.Now;

            await _walletRepository.UpdateWalletAsync(wallet);
            return true;

            }catch(Exception e){
                
            // Log the exception (you can use a logging framework here)
            Console.WriteLine($"An error occurred while topping up the wallet: {e.Message}");

            // Return false in case of failure
            return false;
            }
            
        }

        public async Task<bool> RequestRefund(int userId, decimal amount){
            try{
                var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
                if (wallet == null){
                    return false;
                }
                wallet.OnHoldBalance += amount;
                wallet.AvailableBalance -= amount;
                wallet.UpdatedAt = DateTime.Now;

                await _walletRepository.UpdateWalletAsync(wallet);
                return true;
            }catch(Exception e){
                
            // Log the exception (you can use a logging framework here)
            Console.WriteLine($"An error occurred while processing refund request: {e.Message}");

            // Return false in case of failure
            return false;
            }
        }

            
    }
    
}

