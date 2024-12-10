using System;
using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.Models.Product;


namespace PototoTrade.Repository.Product;

public class PurchaseOrderRepositoryImpl : PurchaseOrderRepository
{
    public readonly DBC _context;

    public PurchaseOrderRepositoryImpl(DBC context)
    {
        _context = context;
    }

    public async Task<PurchaseOrder?> GetPendingOrderByUserId(int userId)
    {
        return await _context.PurchaseOrders
        .Include(o => o.BuyerItems) // Include BuyerItems
        .ThenInclude(bi => bi.Product) // Include the Product related to the BuyerItem
        .ThenInclude(p => p.User) // Include the User related to the Product
        .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "pending");
    }

    public async Task<int> CreatePurchaseOrder(PurchaseOrder order)
    {
        _context.PurchaseOrders.Add(order);
        await _context.SaveChangesAsync();
        return order.Id;
    }

    public async Task UpdatePurchaseOrder(PurchaseOrder order)
    {
        _context.PurchaseOrders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task<PurchaseOrder?> GetPurchaseOrderById(int orderId)
    {
        return await _context.PurchaseOrders
            .Include(po => po.BuyerItems) 
            .ThenInclude(bi => bi.Product) 
            .FirstOrDefaultAsync(po => po.Id == orderId);
    }
}
