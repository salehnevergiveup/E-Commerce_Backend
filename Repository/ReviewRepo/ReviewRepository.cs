using System;
using PototoTrade.Models.Product;  


namespace PototoTrade.Repository.ReivewRepo;

public interface ReviewRepository
{
    public Task<List<ProductReview>> GetAllReviews();

    public Task<List<ProductReview>> GetReviewByProductId(int id);

    public Task<List<ProductReview>> GetReviewsByUserId(int id);

    public Task<ProductReview?> GetReview(int id);


    public Task<ProductReview?> CreateReview(ProductReview review);

    public Task<bool> DeleteReview(ProductReview review);

    public Task<ProductReview?> EditReview(ProductReview review);
}
