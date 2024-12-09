using System;
using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.Models.ShoppingCart;

namespace PototoTrade.Repository.Cart;

public class ShoppingCartRrepositoryImp : ShoppingCartRepository
{
    DBC _context;
    public ShoppingCartRrepositoryImp(DBC context)
    {
        _context = context;
    }
    public async Task<ShoppingCarts>? GetShoppingCartByUserId(int id)
    {
        try
        {
            return await _context.ShoppingCarts
                .Include(sc => sc.ShoppingCartItems)
                .ThenInclude(item => item.Product)
                .Where(sc => sc.UserId == id)
                .OrderByDescending(sc => sc.CreatedAt)
                .FirstOrDefaultAsync();
        }
        catch (Exception e)
        {
            return null;
        }

    }

    public async Task<ShoppingCarts>? GetShoppingCartsById(int id)
    {
        try
        {
            return await _context.ShoppingCarts
                .Include(sc => sc.User)
                .Include(sc => sc.ShoppingCartItems)
                .ThenInclude(item => item.Product)
                .Where(sc => sc.Id == id)
                .FirstOrDefaultAsync();
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task CreateShoppingCart(ShoppingCarts shoppingCarts)
    {
        try
        {
            await _context.ShoppingCarts.AddAsync(shoppingCarts);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
        }
    }

    public async Task<List<ShoppingCarts>> GetShoppingCarts()
    {
        try
        {
            return await _context.ShoppingCarts.Include(cart => cart.User)
                                                .Include(Cart => Cart.ShoppingCartItems)
                                               .ToListAsync();
        }
        catch (Exception e)
        {
            return null;

        }
    }

    public async Task<bool> DeleteShoppingCart(ShoppingCarts shoppingCart)
    {
        try
        {
            _context.ShoppingCarts.Remove(shoppingCart);
            await _context.SaveChangesAsync();
            return true;

        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task DeleteShoppingCartItems(List<ShoppingCartItem> items)
    {
        _context.ShoppingCartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }

}
