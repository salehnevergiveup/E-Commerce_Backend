using System;
using PototoTrade.Models.Product;

namespace PototoTrade.Repository.OnHoldingPayment;
public interface OnHoldingPaymentHistoryRepository
{
    Task<OnHoldingPaymentHistory?> GetPaymentHistoryByDetails(int buyerId, int sellerId, int productId);
    Task CreatePaymentHistory(OnHoldingPaymentHistory paymentHistory);
}