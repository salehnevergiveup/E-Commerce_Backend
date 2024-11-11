using System;
using PototoTrade.Models.ShoppingCart;

namespace PototoTrade.Repository.CartItems;

public interface ShoppingCartItemRepository
{
   Task<int> CreateItem(ShoppingCartItem shoppingCartItem);
   Task<bool> UpdateShoppingCartItem(ShoppingCartItem shoppingCartItem);
   Task<bool> DeleteShoppingCartItem(ShoppingCartItem shoppingCartItem);
   Task<ShoppingCartItem> GetShoppingCartItemById(int Id);


}
