using System;
using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.Models.Product;
using ZstdSharp.Unsafe;

namespace PototoTrade.Repository.Product;

public class ProductRepositoryImp : ProductRepository
{
    public readonly DBC _context;

    public ProductRepositoryImp(DBC context)
    {
        _context = context;
    }
    public async Task<Products> GetProduct(int id)
    {
        try
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception)
        { 
            return null; 
        }
    }

}
