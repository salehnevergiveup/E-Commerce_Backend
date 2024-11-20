using Azure;
using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.Models.User;
using System.Threading.Tasks;

namespace PototoTrade.Repository.Wallet
{
    public class WalletRepositoryImp : WalletRepository
    {
        private readonly DBC _context;

        public WalletRepositoryImp(DBC context)
        {
            _context = context;
        }
        public async Task<int> CreateWallet(UserWallet newWallet){
            try{
                await _context.UserWallets.AddAsync(newWallet);
                await _context.SaveChangesAsync();
            Console.WriteLine("success");

            return newWallet.Id;
            }catch(Exception e){
                Console.WriteLine("create wallet error"+ e);
                return 0;
            };
            
        }
        public async Task<UserWallet?> GetWalletByUserIdAsync(int userId)
        {
            return await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task UpdateWalletAsync(UserWallet wallet)
        {
                _context.UserWallets.Update(wallet);
                await _context.SaveChangesAsync();
        
        }

        public async Task<(UserWallet? BuyerWallet, UserWallet? SellerWallet)> GetBuyerandSellerWalletByUserId(int buyerId, int sellerId)
        {
            var wallets = await _context.UserWallets
                                        .Where(w => w.UserId == buyerId || w.UserId == sellerId)
                                        .ToListAsync();

            var buyerWallet = wallets.FirstOrDefault(w => w.UserId == buyerId);
            var sellerWallet = wallets.FirstOrDefault(w => w.UserId == sellerId);

            return (buyerWallet, sellerWallet);
        }

       public async Task<int> CreateRefundRequest(RefundRequest refundRequest)
        {
            await _context.RefundRequests.AddAsync(refundRequest);
            var result = await _context.SaveChangesAsync();
            Console.WriteLine($"SaveChanges result: {result}");
            return refundRequest.RefundRequestId;
        }

        public async Task<RefundRequest?> GetRefundRequestByIdAsync(int refundRequestId)
        {
            return await _context.RefundRequests.FirstOrDefaultAsync(r => r.RefundRequestId == refundRequestId);
        }

        public async Task UpdateRefundRequestAsync(RefundRequest refundRequest)
        {
            _context.RefundRequests.Update(refundRequest);
            await _context.SaveChangesAsync();
        }

    }
}
