using PototoTrade.Data;
using PototoTrade.DTO.Wallet;
using PototoTrade.Models.User;
using Stripe;

namespace PototoTrade.Repository.Wallet{
    public interface WalletTransactionRepository{

      Task CreateTransaction(WalletTransaction transaction);
    }

}