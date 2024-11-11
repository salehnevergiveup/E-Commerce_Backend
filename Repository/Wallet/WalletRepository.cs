using PototoTrade.Data;
using PototoTrade.Models.User;

namespace PototoTrade.Repository.Wallet{
    public interface WalletRepository{

        Task<int> CreateWallet(UserWallet userWallet);
        Task<UserWallet?> GetWalletByUserIdAsync(int userId);
        Task UpdateWalletAsync(UserWallet wallet);
        Task<(UserWallet? BuyerWallet, UserWallet? SellerWallet)> GetBuyerandSellerWalletByUserId (int buyerId, int sellerId);
    }

}