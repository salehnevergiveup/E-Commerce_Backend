using System;
using PototoTrade.Models.Product;

namespace PototoTrade.Repository.Product;

public interface PurchaseOrderRepository
{
    Task<PurchaseOrder?> GetPendingOrderByUserId(int userId);
    Task<int> CreatePurchaseOrder(PurchaseOrder order);
    Task UpdatePurchaseOrder(PurchaseOrder order);
    
    Task<PurchaseOrder?> GetPurchaseOrderById(int orderId);
}
