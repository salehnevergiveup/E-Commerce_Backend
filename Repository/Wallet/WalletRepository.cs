using PototoTrade.Data;
using PototoTrade.DTO.Wallet;
using PototoTrade.Models.User;
using Stripe;

namespace PototoTrade.Repository.Wallet{
    public interface WalletRepository{

        Task<int> CreateWallet(UserWallet userWallet);
        Task<UserWallet?> GetWalletByUserIdAsync(int userId);
        Task UpdateWalletAsync(UserWallet wallet);
        Task<(UserWallet? BuyerWallet, UserWallet? SellerWallet)> GetBuyerandSellerWalletByUserId (int buyerId, int sellerId);


        //refund
        Task<int> CreateRefundRequest(RefundRequest refundRequest);
        Task<RefundRequest?> GetRefundRequestByIdAsync(int refundRequestId);
        Task UpdateRefundRequestAsync(RefundRequest refundRequest);
    }

}