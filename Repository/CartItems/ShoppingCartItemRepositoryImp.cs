using System;
using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.Models.ShoppingCart;

namespace PototoTrade.Repository.CartItems;

public class ShoppingCartItemRepositoryImp : ShoppingCartItemRepository
{
  public DBC _context;

  public ShoppingCartItemRepositoryImp(DBC context)
  {

    _context = context;

  }
  public async Task<int> CreateItem(ShoppingCartItem shoppingCartItem)
  {
    try
    {
      await _context.ShoppingCartItems.AddRangeAsync(shoppingCartItem);

      await _context.SaveChangesAsync();

      return shoppingCartItem.Id;

    }
    catch (Exception e)
    {
      return 0;
    }
  }

  public async Task<ShoppingCartItem> GetShoppingCartItemById(int Id)
  {
    try
    {
      return await _context.ShoppingCartItems.Include(shi => shi.Product).FirstOrDefaultAsync(shi => shi.Id == Id);
    }
    catch (Exception e)
    {
      return null;
    }
  }

  public async Task<bool> UpdateShoppingCartItem(ShoppingCartItem shoppingCartItem)
  {
    try
    {
      _context.ShoppingCartItems.Update(shoppingCartItem);
      await _context.SaveChangesAsync();
      return true;
    }
    catch (Exception e)
    {
      return false;
    }
  }

  public async Task<bool> DeleteShoppingCartItem(ShoppingCartItem shoppingCartItem)
  {
    try
    {
      _context.ShoppingCartItems.Remove(shoppingCartItem);
      await _context.SaveChangesAsync();
      return true;
    }
    catch (Exception e)
    {
      return false;
    }

  }

}
