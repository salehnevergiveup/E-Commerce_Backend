using System;
using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.Models.BuyerItem;
using PototoTrade.Repository.BuyerItem;

namespace PototoTrade.Repository.Cart;

public class BuyerItemRepositoryImpl : BuyerItemRepository
{
    DBC _context;
    public BuyerItemRepositoryImpl(DBC context)
    {
        _context = context;
    }

    public async Task CreateBuyerItem(BuyerItems buyerItem)
    {
        _context.BuyerItems.Add(buyerItem);
        await _context.SaveChangesAsync();
    }

    public async Task<BuyerItems?> GetBuyerItemByOrderAndProduct(int orderId, int productId)
    {
        return await _context.BuyerItems
            .Include(bi => bi.Product)
            .FirstOrDefaultAsync(bi => bi.OrderId == orderId && bi.ProductId == productId);
    }

    public async Task RemoveBuyerItem(BuyerItems buyerItem)
    {
        _context.BuyerItems.Remove(buyerItem);
        await _context.SaveChangesAsync();
    }

    public async Task CreateBuyerItemDelivery(BuyerItemDelivery delivery)
    {
        _context.BuyerItemDeliveries.Add(delivery);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateBuyerItem(BuyerItems buyerItem)
    {
        _context.BuyerItems.Update(buyerItem);
        await _context.SaveChangesAsync();
    }

    public async Task<List<BuyerItems>> GetBuyerItemsByStatus(string status)
    {
        return await _context.BuyerItems
            .Include(bi => bi.Product)
            .ThenInclude(p => p.User)
            .Include(bi => bi.BuyerItemDeliveries)
            .Where(bi => bi.Status == status)
            .ToListAsync();
    }

    public async Task<BuyerItems?> GetBuyerItemById(int buyerItemId)
    {
        return await _context.BuyerItems
            .Include(b => b.Product)
            .ThenInclude(p => p.User)
            .Include(b => b.BuyerItemDeliveries)
            .FirstOrDefaultAsync(b => b.Id == buyerItemId);
    }

    public async Task<List<BuyerItems>> GetAllBuyerItems()
    {
        return await _context.BuyerItems
            .Include(b => b.Product)
            .ThenInclude(p => p.User)
            .Include(b => b.BuyerItemDeliveries)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<BuyerItems>> GetBuyerItemsByStatusAndUserId(string status, int userId)
    {
        return await _context.BuyerItems
            .Include(b => b.Product)
                .ThenInclude(p => p.User)
            .Include(b => b.BuyerItemDeliveries)
            .Where(b => b.Status == status && b.BuyerId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<BuyerItems>> GetBuyerItemsByStatusesAndUserId(IEnumerable<string> statuses, int userId)
    {
        return await _context.BuyerItems
            .Include(b => b.Product)
                .ThenInclude(p => p.User)
            .Include(b => b.BuyerItemDeliveries)
            .Where(b => statuses.Contains(b.Status) && b.BuyerId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<BuyerItems?> GetLatestBuyerItemByProductId(int productId)
    {
        return await _context.BuyerItems
            .Where(bi => bi.ProductId == productId)
            .OrderByDescending(bi => bi.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<BuyerItems?> GetBuyerItemByProductIdAndStatus(int productId, string status)
    {

        return await _context.BuyerItems
            .Include(b => b.Product) // Include related entities if needed
            .FirstOrDefaultAsync(b => b.ProductId == productId && b.Status == status);

    }
}

