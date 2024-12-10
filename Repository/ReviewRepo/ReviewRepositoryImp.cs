using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.Models.Product;
using PototoTrade.Repository.ReivewRepo;

namespace PotatoTrade.Repository.ReviewRepo
{
    public class ReviewRepositoryImpl : ReviewRepository
    {
        private readonly DBC _context;

        public ReviewRepositoryImpl(DBC context)
        {
            _context = context;
        }

        public async Task<List<ProductReview>> GetAllReviews()
        {
            return await _context.ProductReviews
                                .Include(p => p.Product)
                                .Include(u => u.User).ToListAsync();
        }

        public async Task<List<ProductReview>> GetReviewByProductId(int id)
        {
            return await _context.ProductReviews.Where(p => p.ProductId == id).ToListAsync();
        }

        // get the reviews of the saler
        public async Task<List<ProductReview>> GetReviewsBySalerId(int id)
        {
            return await _context.ProductReviews.Where(p => p.Product.UserId == id)
                                .Include(p => p.Product)
                                .Include(u => u.User).ToListAsync();
        }

        //get the reviews of the buyer 
        public async Task<List<ProductReview>> GetReviewsByBuyerId(int id)
        {
            return await _context.ProductReviews.Where(p => p.UserId == id).ToListAsync();
        }

        public async Task<ProductReview?> GetReview(int id)
        {

            return await _context.ProductReviews.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<ProductReview?> CreateReview(ProductReview review)
        {
            try
            {
                await _context.ProductReviews.AddAsync(review);
                await _context.SaveChangesAsync();
                return review;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<bool> DeleteReview(ProductReview review)
        {
            try
            {
                _context.ProductReviews.Remove(review);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }

}
