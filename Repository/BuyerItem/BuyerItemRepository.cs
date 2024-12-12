using System;
using PototoTrade.Models.BuyerItem;

namespace PototoTrade.Repository.BuyerItem;

public interface BuyerItemRepository
{
    Task CreateBuyerItem(BuyerItems buyerItem);
    Task<BuyerItems?> GetBuyerItemByOrderAndProduct(int orderId, int productId);
    Task RemoveBuyerItem(BuyerItems buyerItem);

    Task CreateBuyerItemDelivery(BuyerItemDelivery delivery);

    Task UpdateBuyerItem(BuyerItems buyerItem);

    Task<List<BuyerItems>> GetBuyerItemsByStatus(string status);

    Task<BuyerItems?> GetBuyerItemById(int buyerItemId);
    Task<List<BuyerItems>> GetAllBuyerItems();
    Task<List<BuyerItems>> GetBuyerItemsByStatusAndUserId(string status, int userId);
    Task<List<BuyerItems>> GetBuyerItemsByStatusesAndUserId(IEnumerable<string> statuses, int userId);

    Task<BuyerItems?> GetLatestBuyerItemByProductId(int productId);

    Task<BuyerItems?> GetBuyerItemByProductIdAndStatus(int productId , string status);


}