using Azure;
using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.Models.User;
using System.Threading.Tasks;

namespace PototoTrade.Repository.Wallet
{
    public class WalletTransactionRepositoryImpl : WalletTransactionRepository
    {
        private readonly DBC _context;

        public WalletTransactionRepositoryImpl(DBC context)
        {
            _context = context;
        }
        
        public async Task CreateTransaction(WalletTransaction transaction)
    {
        _context.WalletTransactions.Add(transaction);
        await _context.SaveChangesAsync();
    }
    }
}
