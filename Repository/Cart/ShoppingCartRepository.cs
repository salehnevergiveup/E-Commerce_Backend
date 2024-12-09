using System;
using PototoTrade.Models.ShoppingCart;

namespace PototoTrade.Repository.Cart;

public interface ShoppingCartRepository
{
    Task<ShoppingCarts>? GetShoppingCartByUserId(int id);
    Task CreateShoppingCart(ShoppingCarts shoppingCarts);
    Task<ShoppingCarts>? GetShoppingCartsById(int id);

    Task<bool> DeleteShoppingCart(ShoppingCarts shoppingCart); 

    Task DeleteShoppingCartItems(List<ShoppingCartItem> items);

    Task<List<ShoppingCarts>> GetShoppingCarts(); 
}
