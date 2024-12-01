using System;
using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.Models.Product;
using ZstdSharp.Unsafe;

namespace PototoTrade.Repository.Product;

public class ProductRepositoryImpl : ProductRepository
{
    public readonly DBC _context;

    public ProductRepositoryImpl(DBC context)
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

    public async Task<bool> CategoryExists(string categoryName)
    {
        return await _context.ProductCategories
            .AnyAsync(c => c.ProductCategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
    }

    // Create a new product category
    public async Task CreateCategory(ProductCategory productCategory)
    {
        _context.ProductCategories.Add(productCategory);
        await _context.SaveChangesAsync();
    }

    public async Task<ProductCategory?> GetCategoryById(int categoryId)
    {
        return await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Id == categoryId);
    }

    public async Task UpdateCategory(ProductCategory productCategory)
    {
        _context.ProductCategories.Update(productCategory);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ProductCategory>> GetProductCategories()
    {
        return await _context.ProductCategories
            .Include(category => category.Products)
            .ToListAsync();
    }

    public async Task<List<Products>> GetProductsByCategoryId(int categoryId)
    {
        return await _context.Products
            .Include(product => product.User)
            .Where(product => product.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<List<Products>> GetAllProducts()
    {
        return await _context.Products
            .Include(product => product.User)
            .ToListAsync();
    }

    public async Task CreateProduct(Products product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }


    public async Task UpdateProduct(Products product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Products>> GetProductsByStatusAndUserId(string status, int userId)
    {
        return await _context.Products
            .Where(p => p.Status == status && p.UserId == userId)
            .ToListAsync();
    }

    public async Task<Products?> GetProductById(int productId)
    {
        return await _context.Products
            .Include(p => p.User) 
            .FirstOrDefaultAsync(p => p.Id == productId);
    }

}
