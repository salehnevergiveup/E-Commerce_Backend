using System;
using PototoTrade.Models.Product;  


namespace PototoTrade.Repository.ReivewRepo;

public interface ReviewRepository
{
    public Task<List<ProductReview>> GetAllReviews();

    public Task<List<ProductReview>> GetReviewByProductId(int id);

    public Task<List<ProductReview>> GetReviewsBySalerId(int id); 
    public Task<List<ProductReview>> GetReviewsByBuyerId(int id); 

    public Task<ProductReview?> GetReview(int id);


    public Task<ProductReview?> CreateReview(ProductReview review);

    public Task<bool> DeleteReview(ProductReview review);
}
