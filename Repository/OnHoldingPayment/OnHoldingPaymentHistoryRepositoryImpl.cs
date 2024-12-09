using System;
using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.Models.Product;
using PototoTrade.Repository.OnHoldingPayment;
using ZstdSharp.Unsafe;

namespace PototoTrade.Repository.Product;
public class OnHoldingPaymentHistoryRepositoryImpl : OnHoldingPaymentHistoryRepository
{
    private readonly DBC _context;

    public OnHoldingPaymentHistoryRepositoryImpl(DBC context)
    {
        _context = context;
    }

    public async Task<OnHoldingPaymentHistory?> GetPaymentHistoryByDetails(int buyerId, int sellerId, int productId)
    {
        return await _context.OnHoldingPaymentHistories
            .FirstOrDefaultAsync(p => p.BuyerId == buyerId && p.SellerId == sellerId && p.ProductId == productId);
    }

    public async Task CreatePaymentHistory(OnHoldingPaymentHistory paymentHistory)
    {
        await _context.OnHoldingPaymentHistories.AddAsync(paymentHistory);
        await _context.SaveChangesAsync();
    }
}