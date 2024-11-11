using System;
using PototoTrade.Models.Product;

namespace PototoTrade.Repository.Product;

public interface ProductRepository
{
    Task<Products> GetProduct(int id); 

}
