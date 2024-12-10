using PotatoTrade.DTO.Review;
using PototoTrade.Repository.MediaRepo;
using PototoTrade.Repository.ReivewRepo;
using PototoTrade.Service.Utilities.Response;
using PototoTrade.Models.Product;
using PototoTrade.Models.Media;
using System.Security.Claims;
using PotatoTrade.DTO.MediaDTO;
using PototoTrade.DTO.Product;
using PotatoTrade.DTO.User;
using PototoTrade.DTO.Review;

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

    public async Task<ResponseModel<List<GetAllReviewsDTO>>> GetAllReviews()
    {
        var response = new ResponseModel<List<GetAllReviewsDTO>>
        {
            Data = new List<GetAllReviewsDTO>(),
            Message = "No reviews found.",
            Success = false
        };

        try
        {
            // Step 1: Fetch All Reviews with Related Data
            var reviews = await _review.GetAllReviews();

            if (reviews == null || !reviews.Any())
            {
                return response;
            }

            var reviewDTOs = new List<GetAllReviewsDTO>();

            // Step 2: Extract Necessary IDs
            var reviewIds = reviews.Select(r => r.Id).ToList();
            var buyerIds = reviews.Select(r => r.User.Id).Distinct().ToList(); // Buyers are the users who wrote the reviews
            var sellerIds = reviews.Select(r => r.Product.User.Id).Distinct().ToList(); // Sellers are the users who own the products

            // Step 3: Fetch All Required Medias in Bulk
            var reviewMedias = await _media.GetMediaListBySourceIdsAndType(reviewIds, "Review");
            var buyerMedias = await _media.GetMediaListBySourceIdsAndType(buyerIds, "User_Profile");
            var sellerMedias = await _media.GetMediaListBySourceIdsAndType(sellerIds, "User_Profile");
            var productMedias = await _media.GetMediaListBySourceIdsAndType(reviews.Select(r => r.Product.Id).ToList(), "Product_Image");

            // Step 4: Group Medias for Efficient Lookup
            var reviewMediasGrouped = reviewMedias
                .GroupBy(m => m.SourceId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var buyerMediasGrouped = buyerMedias
                .GroupBy(m => m.SourceId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var sellerMediasGrouped = sellerMedias
                .GroupBy(m => m.SourceId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var productMediasGrouped = productMedias
                .GroupBy(m => m.SourceId)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault());

            // Step 5: Iterate Through Each Review and Map to DTO
            foreach (var review in reviews)
            {
                // Map Buyer
                GetUserListDTO buyerDTO = null;
                if (review.User != null)
                {
                    var buyer = review.User;
                    buyerDTO = new GetUserListDTO
                    {
                        Id = buyer.Id,
                        Name = buyer.Name,
                        Username = buyer.Username,
                        Status = buyer.Status,
                        CreatedAt = buyer.CreatedAt,
                        Medias = buyerMediasGrouped.TryGetValue(buyer.Id, out var buyerMediaList)
                            ? buyerMediaList.Select(media => new HandleMedia
                            {
                                Id = media.Id,
                                SourceId = media.SourceId,
                                Type = media.SourceType,
                                MediaUrl = media.MediaUrl,
                                CreatedAt = media.CreatedAt,
                                UpdatedAt = media.UpdatedAt
                            }).ToList()
                            : new List<HandleMedia>()
                    };
                }

                // Map Seller
                GetUserListDTO sellerDTO = null;
                if (review.Product?.User != null)
                {
                    var seller = review.Product.User;
                    sellerDTO = new GetUserListDTO
                    {
                        Id = seller.Id,
                        Name = seller.Name,
                        Username = seller.Username,
                        Status = seller.Status,
                        CreatedAt = seller.CreatedAt,
                        Medias = sellerMediasGrouped.TryGetValue(seller.Id, out var sellerMediaList)
                            ? sellerMediaList.Select(media => new HandleMedia
                            {
                                Id = media.Id,
                                SourceId = media.SourceId,
                                Type = media.SourceType,
                                MediaUrl = media.MediaUrl,
                                CreatedAt = media.CreatedAt,
                                UpdatedAt = media.UpdatedAt
                            }).ToList()
                            : new List<HandleMedia>()
                    };
                }

                // Map Product
                ProductDTO productDTO = null;
                if (review.Product != null)
                {
                    var product = review.Product;
                    var mainImage = productMediasGrouped.TryGetValue(product.Id, out var productMedia) && productMedia != null
                        ? new HandleMedia
                        {
                            Id = productMedia.Id,
                            SourceId = productMedia.SourceId,
                            Type = productMedia.SourceType,
                            MediaUrl = productMedia.MediaUrl,
                            CreatedAt = productMedia.CreatedAt,
                            UpdatedAt = productMedia.UpdatedAt
                        }
                        : null;

                    productDTO = new ProductDTO
                    {
                        Id = product.Id,
                        Price = product.Price,
                        Image = mainImage != null ? mainImage.MediaUrl : "/placeholder-product.png",
                        CreatedAt = product.CreatedAt,
                        RefundGuaranteedDuration = 0,
                        Title = product.Title
                    };
                }

                // Map Review Medias
                var reviewMediasList = reviewMediasGrouped.TryGetValue(review.Id, out var reviewMediaItems)
                    ? reviewMediaItems.Select(media => new HandleMedia
                    {
                        Id = media.Id,
                        SourceId = media.SourceId,
                        Type = media.SourceType,
                        MediaUrl = media.MediaUrl,
                        CreatedAt = media.CreatedAt,
                        UpdatedAt = media.UpdatedAt
                    }).ToList()
                    : new List<HandleMedia>();

                // Create GetAllReviewsDTO
                var getAllReviewsDTO = new GetAllReviewsDTO
                {
                    Id = review.Id,
                    ProductId = review.ProductId,
                    Rating = review.Rating,
                    ReviewComment = review.ReviewComment,
                    Buyer = buyerDTO,
                    Saler = sellerDTO,
                    Product = productDTO,
                    Medias = reviewMediasList
                };

                reviewDTOs.Add(getAllReviewsDTO);
            }

            // Step 6: Populate the Response
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

    public async Task<ResponseModel<List<GetReviewDTO>>> GetReviews(int id, string type)
    {
        var response = new ResponseModel<List<GetReviewDTO>> { Success = false };

        try
        {
            List<ProductReview> reviews = new List<ProductReview>();

            // Step 1: Fetch Reviews Based on Type
            switch (type.ToLower())
            {
                case "product":
                    reviews = await _review.GetReviewByProductId(id);
                    break;
                case "saler":
                    reviews = await _review.GetReviewsBySalerId(id);
                    break;
                case "user_buyer":
                    reviews = await _review.GetReviewsByBuyerId(id);
                    break;
                case "review":
                    var singleReview = await _review.GetReview(id);
                    if (singleReview != null)
                    {
                        reviews.Add(singleReview);
                    }
                    break;
                default:
                    response.Message = $"Invalid type parameter. Allowed values are 'review', 'product', 'saler', or 'user_buyer'.{type}";
                    return response;
            }

            // Step 2: Check if Reviews Exist
            if (reviews == null || !reviews.Any())
            {
                response.Message = "No reviews found.";
                return response;
            }

            var reviewDTOs = new List<GetReviewDTO>();

            // Step 3: Extract Necessary IDs
            var reviewIds = reviews.Select(r => r.Id).ToList();
            var userIds = reviews.Select(r => r.User.Id).Distinct().ToList();
            var productIds = reviews.Select(r => r.Product.Id).Distinct().ToList();

            // Step 4: Fetch Review Medias
            var allReviewMedias = await _media.GetMediaListBySourceIdsAndType(reviewIds, "Review");

            // Step 5: Fetch User Profile Medias
            var allUserMedias = await _media.GetMediaListBySourceIdsAndType(userIds, "User_Profile");

            // Step 6: Fetch Product Images (First Image per Product)
            var firstProductMedias = new List<Media>();
            foreach (var productId in productIds)
            {
                var media = await _media.GetFirstMediaBySourceIdAndType(productId, "Product_Image");
                if (media != null)
                {
                    firstProductMedias.Add(media);
                }
            }

            // Step 7: Group Medias for Efficient Lookup
            var reviewMediasGrouped = allReviewMedias
                .GroupBy(m => m.SourceId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var userMediasGrouped = allUserMedias
                .GroupBy(m => m.SourceId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var productMediasGrouped = firstProductMedias
                .GroupBy(m => m.SourceId)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault());

            // Step 8: Map to DTOs
            foreach (var review in reviews)
            {
                // Map User
                GetUserListDTO userDTO = null;
                if (review.User != null)
                {
                    var user = review.User;
                    userDTO = new GetUserListDTO
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Username = user.Username,
                        Status = user.Status,
                        CreatedAt = user.CreatedAt,
                        Medias = userMediasGrouped.TryGetValue(user.Id, out var userMedias)
                            ? userMedias.Select(media => new HandleMedia
                            {
                                Id = media.Id,
                                SourceId = media.SourceId,
                                Type = media.SourceType,
                                MediaUrl = media.MediaUrl,
                                CreatedAt = media.CreatedAt,
                                UpdatedAt = media.UpdatedAt
                            }).ToList()
                            : new List<HandleMedia>()
                    };
                }

                // Map Product
                ProductDTO productDTO = null;
                if (review.Product != null)
                {
                    var product = review.Product;
                    HandleMedia mainImage = null;
                    if (productMediasGrouped.TryGetValue(product.Id, out var productMedia) && productMedia != null)
                    {
                        mainImage = new HandleMedia
                        {
                            Id = productMedia.Id,
                            SourceId = productMedia.SourceId,
                            Type = productMedia.SourceType,
                            MediaUrl = productMedia.MediaUrl,
                            CreatedAt = productMedia.CreatedAt,
                            UpdatedAt = productMedia.UpdatedAt
                        };
                    }

                    productDTO = new ProductDTO
                    {
                        Id = product.Id,
                        Price = product.Price,
                        Image = mainImage != null ? mainImage.MediaUrl : "",
                        CreatedAt = product.CreatedAt,
                        RefundGuaranteedDuration = 0,
                        Title = product.Title
                    };
                }

                // Map Review Medias
                var reviewMedias = reviewMediasGrouped.TryGetValue(review.Id, out var medias)
                    ? medias.Select(media => new HandleMedia
                    {
                        Id = media.Id,
                        SourceId = media.SourceId,
                        Type = media.SourceType,
                        MediaUrl = media.MediaUrl,
                        CreatedAt = media.CreatedAt,
                        UpdatedAt = media.UpdatedAt
                    }).ToList()
                    : new List<HandleMedia>();

                // Create UpdateReviewDTO
                var reviewDTO = new GetReviewDTO
                {
                    Id = review.Id,
                    ProductId = review.ProductId,
                    Rating = review.Rating,
                    ReviewComment = review.ReviewComment,
                    User = userDTO,
                    Product = productDTO,
                    Medias = reviewMedias
                };

                reviewDTOs.Add(reviewDTO);
            }

            // Step 9: Populate the Response
            response.Data = reviewDTOs;
            response.Success = true;
            response.Message = "Reviews retrieved successfully.";
            return response;
        }
        catch (Exception ex)
        {
            // Optionally log the exception here using your logging framework
            response.Message = "An error occurred while retrieving reviews.";
            return response;
        }
    }

    public async Task<ResponseModel<int>> CreateReview(CreateReviewDTO createReviewDto, ClaimsPrincipal userClaims)
    {
        var response = new ResponseModel<int> { Success = false };

        try
        {
            var userIdClaim = userClaims.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                response.Message = "Invalid user.";
                return response;
            }

            var newReview = new ProductReview
            {
                ProductId = createReviewDto.ProductId,
                UserId = int.Parse(userIdClaim),
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

            // Extract user information from claims
            var userIdClaim = userClaims.FindFirst(ClaimTypes.Name)?.Value;
            var userRole = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            var canDelete = userClaims.HasClaim("Permission", "CanDelete");

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                response.Message = "Invalid user.";
                return response;
            }

            if (userId != review.UserId &&
                (userRole != "SuperAdmin" && userRole != "Admin" || !canDelete))
            {
                response.Message = "You are not authorized to delete this review.";
                return response;
            }

            await _media.DeleteMediaBySourceIdAndType(review.Id, "Review");

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
