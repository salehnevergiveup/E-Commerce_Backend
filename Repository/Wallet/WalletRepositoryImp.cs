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

    }
}
