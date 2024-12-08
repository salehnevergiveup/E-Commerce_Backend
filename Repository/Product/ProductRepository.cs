using System;
using PototoTrade.Models.Product;

namespace PototoTrade.Repository.Product;

public interface ProductRepository
{
    Task<Products> GetProduct(int id); 
    Task<bool> CategoryExists(string categoryName);
    Task CreateCategory(ProductCategory productCategory);
    
    Task<ProductCategory?> GetCategoryById(int categoryId);
    Task UpdateCategory(ProductCategory productCategory);
    Task<List<ProductCategory>> GetProductCategories();
    Task<List<Products>> GetProductsByCategoryId(int categoryId);
    Task<List<Products>> GetAllProducts();
    Task CreateProduct(Products product);

    Task<List<Products>> GetProductsByStatusAndUserId(string status, int userId);

    Task UpdateProduct(Products product) ;

    Task DeleteProductAsync(Products product);

    Task<Products?> GetProductById(int productId);


}
