using PotatoTrade.DTO.Review;
using PototoTrade.Repository.MediaRepo;
using PototoTrade.Repository.ReivewRepo;
using PototoTrade.Service.Utilities.Response;
using PototoTrade.Models.Product;
using PototoTrade.Models.Media;
using System.Security.Claims;

namespace PototoTrade.Service.Reivew;

public class ReviewService
{

    private readonly ReviewRepository _review;
    private readonly MediaRepository _media;

    public ReviewService(ReviewRepository review, MediaRepository media)
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
                Media? media = null;

                if (review.MediaBoolean)
                {
                    media = await _media.GetMediaBySourceIdAndType(review.Id, "Review");
                }

                var reviewDTO = this.castToReviewDTO(review, media);
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

    private ReviewDTO castToReviewDTO(ProductReview review, Media media)
    {
        return new ReviewDTO
        {
            Id = review.Id,
            ProductId = review.ProductId,
            UserId = review.UserId,
            Rating = review.Rating,
            ReviewComment = review.ReviewComment,
            ReviewDate = review.ReviewDate,
            UpdatedAt = review.UpdatedAt,
            MediaBoolean = review.MediaBoolean,
            MediaUrl = media?.MediaUrl,
        };
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

            foreach (var review in reviews)
            {
                Media? media = null;

                if (review.MediaBoolean)
                {
                    media = await _media.GetMediaBySourceIdAndType(review.Id, "Review");
                }

                var reviewDTO = this.castToReviewDTO(review, media);
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
                UserId = 23,
                Rating = createReviewDto.Rating,
                ReviewComment = createReviewDto.ReviewComment,
                ReviewDate = DateTime.UtcNow,
                MediaBoolean = !string.IsNullOrEmpty(createReviewDto.MediaUrl)
            };

            await _review.CreateReview(newReview);

            if (!string.IsNullOrEmpty(createReviewDto.MediaUrl))
            {
                var newMedia = new Media
                {
                    SourceType = "Review",
                    SourceId = newReview.Id,
                    MediaUrl = createReviewDto.MediaUrl,
                    CreatedAt = DateTime.UtcNow
                };
                await _media.CreateMedias(newReview.Id, new List<Media> { newMedia });
            }

            response.Data = newReview.Id;
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

    public async Task<ResponseModel<bool>> UpdateReview(int reviewId, UpdateReviewDTO updateReviewDto, ClaimsPrincipal userClaims)
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
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId) || userId != review.UserId)
            {
                response.Message = "You are not authorized to update this review.";
                return response;
            }

            review.Rating = updateReviewDto.Rating ?? review.Rating;
            review.ReviewComment = updateReviewDto.ReviewComment ?? review.ReviewComment;
            review.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(updateReviewDto.MediaUrl))
            {
                var existingMedia = await _media.GetMediaBySourceIdAndType(review.Id, "Review");

                if (existingMedia != null)
                {
                    existingMedia.MediaUrl = updateReviewDto.MediaUrl;
                    existingMedia.UpdatedAt = DateTime.UtcNow;
                    await _media.UpdateMedias(review.Id, new List<Media> { existingMedia });
                }
                else
                {
                    var newMedia = new Media
                    {
                        SourceType = "Review",
                        SourceId = review.Id,
                        MediaUrl = updateReviewDto.MediaUrl,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _media.CreateMedias(review.Id, new List<Media> { newMedia });
                }

                review.MediaBoolean = true;
            }

            await _review.EditReview(review);

            response.Data = true;
            response.Success = true;
            response.Message = "Review updated successfully.";
            return response;
        }
        catch (Exception ex)
        {
            response.Message = "An error occurred while updating the review.";
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
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId) || userId != review.UserId)
            {
                response.Message = "You are not authorized to delete this review.";
                return response;
            }

            await _media.DeleteMediaBySourceId(review.Id);

            await _review.DeleteReview(review);

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
