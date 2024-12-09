using PotatoTrade.DTO.Review;
using PototoTrade.Repository.MediaRepo;
using PototoTrade.Repository.ReivewRepo;
using PototoTrade.Service.Utilities.Response;
using PototoTrade.Models.Product;
using PototoTrade.Models.Media;
using System.Security.Claims;
using PotatoTrade.DTO.MediaDTO;

namespace PototoTrade.Service.Review;

public class ProductReviewService
{

    private readonly ReviewRepository _review;
    private readonly MediaRepository _media;

    public ProductReviewService(ReviewRepository review, MediaRepository media)
    {
        _review = review;
        _media = media;
    }

    public async Task<ResponseModel<List<ReviewDTO>>> GetAllReviews()
    {
        var response = new ResponseModel<List<ReviewDTO>>
        {
            Data = new List<ReviewDTO>(),
            Message = "No reviews found.",
            Success = false
        };

        try
        {
            var reviews = await _review.GetAllReviews();

            if (reviews == null || !reviews.Any())
            {
                return response;
            }

            var reviewDTOs = new List<ReviewDTO>();

            foreach (var review in reviews)
            {
                var medias = await _media.GetMediaListBySourceIdAndType(review.Id, "Review");

                var reviewDTO = new ReviewDTO
                {
                    Id = review.Id,
                    ProductId = review.ProductId,
                    UserId = review.UserId,
                    Rating = review.Rating,
                    ReviewComment = review.ReviewComment,
                    ReviewDate = review.ReviewDate,
                    UpdatedAt = review.UpdatedAt,
                    Medias = medias.Select(media => new HandleMedia
                    {
                        Id = media.Id,
                        SourceId = media.SourceId,
                        Type = media.SourceType,
                        MediaUrl = media.MediaUrl,
                        CreatedAt = media.CreatedAt,
                        UpdatedAt = media.UpdatedAt
                    }).ToList()
                };

                reviewDTOs.Add(reviewDTO);
            }

            response.Data = reviewDTOs;
            response.Success = true;
            response.Message = "Reviews retrieved successfully.";
            return response;
        }
        catch (Exception ex)
        {
            response.Message = "An error occurred while retrieving reviews.";
            return response;
        }

    }

    public async Task<ResponseModel<List<ReviewDTO>>> GetReviews(int id, string type)
    {
        var response = new ResponseModel<List<ReviewDTO>> { Success = false };

        try
        {
            List<ProductReview> reviews = new List<ProductReview>();

            switch (type.ToLower())
            {
                case "product":
                    reviews = await _review.GetReviewByProductId(id);
                    break;
                case "user":
                    reviews = await _review.GetReviewsByUserId(id);
                    break;
                case "review":
                    var singleReview = await _review.GetReview(id);
                    if (singleReview != null)
                    {
                        reviews.Add(singleReview);
                    }
                    break;
                default:
                    response.Message = "Invalid type parameter. Allowed values are 'review', 'product', or 'user'.";
                    return response;
            }

            if (reviews == null || !reviews.Any())
            {
                response.Message = "No reviews found.";
                return response;
            }

            var reviewDTOs = new List<ReviewDTO>();

            // Create a list of tasks to fetch media for each review
            var mediaTasks = reviews.Select(review => _media.GetMediaListBySourceIdAndType(review.Id, "Review")).ToList();

            // Execute all media fetching tasks concurrently
            var allMedias = await Task.WhenAll(mediaTasks);

            // Iterate through each review and map to ReviewDTO
            for (int i = 0; i < reviews.Count; i++)
            {
                var review = reviews[i];
                var medias = allMedias[i];

                var reviewDTO = new ReviewDTO
                {
                    Id = review.Id,
                    ProductId = review.ProductId,
                    UserId = review.UserId,
                    Rating = review.Rating,
                    ReviewComment = review.ReviewComment,
                    ReviewDate = review.ReviewDate,
                    UpdatedAt = review.UpdatedAt,
                    Medias = medias.Select(media => new HandleMedia
                    {
                        Id = media.Id,
                        SourceId = media.SourceId,
                        Type = media.SourceType,
                        MediaUrl = media.MediaUrl,
                        CreatedAt = media.CreatedAt,
                        UpdatedAt = media.UpdatedAt
                    }).ToList()
                };

                reviewDTOs.Add(reviewDTO);
            }

            // Populate the response
            response.Data = reviewDTOs;
            response.Success = true;
            response.Message = "Reviews retrieved successfully.";
            return response;
        }
        catch (Exception ex)
        {
            // Optionally log the exception here using your logging framework
            // _logger.LogError(ex, "Error retrieving reviews.");

            response.Message = "An error occurred while retrieving reviews.";
            return response;
        }
    }


    public async Task<ResponseModel<int>> CreateReview(CreateReviewDTO createReviewDto, ClaimsPrincipal userClaims)
    {
        var response = new ResponseModel<int> { Success = false };

        try
        {
            var userIdClaim = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                response.Message = "Invalid user.";
                return response;
            }

            var newReview = new ProductReview
            {
                ProductId = createReviewDto.ProductId,
                UserId = ,
                Rating = createReviewDto.Rating,
                ReviewComment = createReviewDto.ReviewComment,
                ReviewDate = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdReview = await _review.CreateReview(newReview);
            if (createdReview == null)
            {
                response.Message = "Failed to create review.";
                return response;
            }

            if (createReviewDto.Medias != null && createReviewDto.Medias.Any())
            {
                var medias = createReviewDto.Medias.Select(mediaDto => new Media
                {
                    SourceType = "Review",
                    SourceId = createdReview.Id,
                    MediaUrl = mediaDto.MediaUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                await _media.CreateMedias(createdReview.Id, medias);
            }

            response.Data = createdReview.Id;
            response.Success = true;
            response.Message = "Review created successfully.";
            return response;
        }
        catch (Exception ex)
        {
            response.Message = "An error occurred while creating the review.";
            return response;
        }
    }

    public async Task<ResponseModel<bool>> DeleteReview(int reviewId, ClaimsPrincipal userClaims)
    {
        var response = new ResponseModel<bool> { Success = false };

        try
        {
            var review = await _review.GetReview(reviewId);

            if (review == null)
            {
                response.Message = "Review not found.";
                return response;
            }

            var userIdClaim = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                response.Message = "Invalid user.";
                return response;
            }

            if (userId != review.UserId)
            {
                response.Message = "You are not authorized to delete this review.";
                return response;
            }

            var mediaList = await _media.GetMediaBySourceId(review.Id);

            foreach (var media in mediaList)
            {
                await _media.DeleteMedia(media);
            }

            var deleteResult = await _review.DeleteReview(review);
            if (!deleteResult)
            {
                response.Message = "Failed to delete the review.";
                return response;
            }

            response.Data = true;
            response.Success = true;
            response.Message = "Review deleted successfully.";
            return response;
        }
        catch (Exception ex)
        {
            response.Message = "An error occurred while deleting the review.";
            return response;
        }
    }
}
