using System;
using System.Security.Claims;
using PototoTrade.DTO.Common;
using PototoTrade.DTO.Product;
using PototoTrade.Models.Product;
using PototoTrade.Repository.BuyerItem;
using PototoTrade.Repository.MediaRepo;
using PototoTrade.Repository.Product;
using PototoTrade.Service.Utilities.Exceptions;
using PototoTrade.Service.Utilities.Response;

namespace PototoTrade.Service.Product
{
    public class ProductSrv
    {
        private readonly ProductRepository _productRepository;

        private readonly MediaRepository _mediaRepository;

        private readonly MediaSrv _mediaService;

        private readonly BuyerItemRepository _buyerItemRepository;

        public ProductSrv(ProductRepository productRepository, BuyerItemRepository buyerItemRepository, MediaRepository mediaRepository, MediaSrv mediaSrv, ILogger<ProductSrv> logger)
        {
            _productRepository = productRepository;
            _mediaRepository = mediaRepository;
            _mediaService = mediaSrv;
            _buyerItemRepository = buyerItemRepository;
        }

        public async Task<ResponseModel<T>> CreateCategory<T>(CreateProductCategoryDTO categoryDto)
        {
            try
            {
                if (categoryDto.ChargeRate < 0 || categoryDto.RebateRate < 0)
                {
                    return new ResponseModel<T>
                    {
                        Success = false,
                        Code = "999",
                        Message = "Invalid input: ChargeRate and RebateRate must be non-negative.",
                        Data = default
                    };
                }

                if (await _productRepository.CategoryExists(categoryDto.ProductCategoryName))
                {

                    throw new CustomException<GeneralMessageDTO>(ExceptionEnum.GetException("PRODUCT_CATEGORY_EXISTED"));

                }

                var chargeRate = categoryDto.ChargeRate / 100.0;
                var rebateRate = categoryDto.RebateRate / 100.0;
                var newCategory = new ProductCategory
                {
                    ProductCategoryName = categoryDto.ProductCategoryName,
                    Description = categoryDto.Description,
                    ChargeRate = chargeRate,
                    RebateRate = rebateRate,
                    CreatedAt = DateTime.UtcNow
                };

                await _productRepository.CreateCategory(newCategory);

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = $"Product category :{categoryDto.ProductCategoryName} created successfully.",
                    Data = default
                };
            }
            catch (CustomException<GeneralMessageDTO> customEx)
            {
                Console.Error.WriteLine($"CustomException: {customEx.Response.Message}");
                return new ResponseModel<T>
                {
                    Success = customEx.Response.Success,
                    Code = customEx.Response.Code,
                    Message = customEx.Response.Message,
                    Data = customEx.Response.Data == null ? default(T)! : (T)(object)customEx.Response.Data
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error updating product category (New Product Name: {categoryDto.ProductCategoryName}): {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = $"An error occurred while creating the product category: {categoryDto.ProductCategoryName}",
                    Data = default
                };
            }

        }



        public async Task<ResponseModel<T>> EditCategory<T>(EditProductCategoryDTO categoryDto)
        {
            try
            {

                var existingCategory = await _productRepository.GetCategoryById(categoryDto.ProductCategoryId);
                if (existingCategory == null)
                {
                    throw new CustomException<GeneralMessageDTO>(ExceptionEnum.GetException("PRODUCT_CATEGORY_NOT_FOUND"));
                }


                existingCategory.ProductCategoryName = categoryDto.ProductCategoryName;
                existingCategory.ChargeRate = categoryDto.ChargeRate / 100.0;
                existingCategory.RebateRate = categoryDto.RebateRate / 100.0;
                await _productRepository.UpdateCategory(existingCategory);

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = $"Product category :{categoryDto.ProductCategoryName} update successfully.",
                    Data = default
                };
            }
            catch (CustomException<GeneralMessageDTO> customEx)
            {
                Console.Error.WriteLine($"CustomException: {customEx.Response.Message}");
                return new ResponseModel<T>
                {
                    Success = customEx.Response.Success,
                    Code = customEx.Response.Code,
                    Message = customEx.Response.Message,
                    Data = customEx.Response.Data == null ? default(T)! : (T)(object)customEx.Response.Data
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error updating product category (ID: {categoryDto.ProductCategoryId}): {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = $"An error occurred while updating the product category: {categoryDto.ProductCategoryName}",
                    Data = default
                };
            }

        }

        public async Task<ResponseModel<T>> GetProductCategoryList<T>()
        {
            try
            {
                var productCategories = await _productRepository.GetProductCategories();

                if (productCategories == null || !productCategories.Any())
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("PRODUCT_CATEGORY_NOT_FOUND")

                    );
                }

                var categoryList = productCategories.Select(category => new ProductCategoryListDTO
                {
                    ProductCategoryId = category.Id,
                    ProductCategoryName = category.ProductCategoryName,
                    ChargeRate = category.ChargeRate * 100,
                    RebateRate = category.RebateRate * 100,
                    NumberOfItems = category.Products.Count(product => product.Status == "available")
                }).ToList();

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Product categories retrieved successfully.",
                    Data = default(T) ?? (T)(object)categoryList
                };
            }
            catch (CustomException<GeneralMessageDTO> customEx)
            {
                Console.Error.WriteLine($"CustomException: {customEx.Response.Message}");
                return new ResponseModel<T>
                {
                    Success = customEx.Response.Success,
                    Code = customEx.Response.Code,
                    Message = customEx.Response.Message,
                    Data = customEx.Response.Data == null ? default(T)! : (T)(object)customEx.Response.Data
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving product categories: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while retrieving the product categories.",
                    Data = default
                };
            }
        }

        public async Task<ResponseModel<T>> ViewProductsByCategory<T>(ViewProductsRequestDTO request)
        {
            try
            {
                if (request.ProductCategoryId <= 0)
                {
                    return new ResponseModel<T>
                    {
                        Success = false,
                        Code = "400",
                        Message = "Invalid ProductCategoryId.",
                        Data = default
                    };
                }

                var productCategory = await _productRepository.GetCategoryById(request.ProductCategoryId);
                if (productCategory == null)
                {
                    throw new CustomException<GeneralMessageDTO>(ExceptionEnum.GetException("PRODUCT_CATEGORY_NOT_FOUND"));
                }
                var products = await _productRepository.GetProductsByCategoryId(request.ProductCategoryId);

                if (products == null || !products.Any())
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("PRODUCT_NOT_FOUND"),
                        new GeneralMessageDTO
                        {
                            Message = $"The Category :{productCategory.ProductCategoryName} has no any product",
                            Success = false
                        }
                    );
                }

                var productList = new List<ProductDetailsDTO>();

                foreach (var product in products)
                {
                    var mediaList = new List<MediaDTO>();

                    if (product.MediaBoolean)
                    {
                        var media = await _mediaRepository.GetMediaListBySourceIdAndType(product.Id, "PRODUCT");
                        mediaList = media != null && media.Any()
                        ? media.Select(m => new MediaDTO
                        {
                            Id = m.Id,
                            MediaUrl = m.MediaUrl,
                            CreatedAt = m.CreatedAt,
                            UpdatedAt = m.UpdatedAt
                        }).ToList()
                        : null;
                    }

                    productList.Add(new ProductDetailsDTO
                    {
                        ProductId = product.Id,
                        Title = product.Title,
                        Description = product.Description,
                        Price = product.Price,
                        RefundGuaranteedDuration = product.RefundGuaranteedDuration,
                        CreatedAt = product.CreatedAt,
                        UserId = product.User.Id,
                        UserName = product.User.Name,
                        Media = mediaList
                    });
                }

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Products retrieved successfully.",
                    Data = (T)(object)productList
                };
            }
            catch (CustomException<GeneralMessageDTO> customEx)
            {
                Console.Error.WriteLine($"CustomException: {customEx.Response.Message}");
                return new ResponseModel<T>
                {
                    Success = customEx.Response.Success,
                    Code = customEx.Response.Code,
                    Message = customEx.Response.Message,
                    Data = customEx.Response.Data == null ? default(T)! : (T)(object)customEx.Response.Data
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving products for category ID {request.ProductCategoryId}: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while retrieving products.",
                    Data = default
                };
            }
        }

        public async Task<ResponseModel<T>> ViewAllProducts<T>()
        {
            try
            {
                var products = await _productRepository.GetAllProducts();
                if (products == null || !products.Any())
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("PRODUCT_NOT_FOUND"),
                        new GeneralMessageDTO
                        {
                            Message = "No products found.",
                            Success = false
                        }
                    );
                }

                var productList = new List<ProductDetailsDTO>();

                foreach (var product in products)
                {
                    var mediaList = new List<MediaDTO>();

                    if (product.MediaBoolean)
                    {
                        var media = await _mediaRepository.GetMediaListBySourceIdAndType(product.Id, "PRODUCT");
                        mediaList = media != null && media.Any()
                            ? media.Select(m => new MediaDTO
                            {
                                Id = m.Id,
                                MediaUrl = m.MediaUrl,
                                CreatedAt = m.CreatedAt,
                                UpdatedAt = m.UpdatedAt
                            }).ToList()
                            : null;
                    }

                    productList.Add(new ProductDetailsDTO
                    {
                        ProductId = product.Id,
                        Title = product.Title,
                        Description = product.Description,
                        Price = product.Price,
                        RefundGuaranteedDuration = product.RefundGuaranteedDuration,
                        CreatedAt = product.CreatedAt,
                        UserId = product.User.Id,
                        UserName = product.User.Name,
                        Media = mediaList
                    });
                }
                productList = productList.OrderByDescending(p => p.CreatedAt).ToList();

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Products retrieved successfully.",
                    Data = (T)(object)productList
                };
            }
            catch (CustomException<GeneralMessageDTO> customEx)
            {
                Console.Error.WriteLine($"CustomException: {customEx.Response.Message}");
                return new ResponseModel<T>
                {
                    Success = customEx.Response.Success,
                    Code = customEx.Response.Code,
                    Message = customEx.Response.Message,
                    Data = customEx.Response.Data == null ? default(T)! : (T)(object)customEx.Response.Data
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving all products: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while retrieving products.",
                    Data = default
                };
            }
        }

        public async Task<ResponseModel<T>> EditProduct<T>(ClaimsPrincipal userClaims, EditProductDetailDto editProductDto)
        {
            try
            {

                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                var userRole = userClaims.FindFirst(ClaimTypes.Role)?.Value;

                var existingProduct = await _productRepository.GetProduct(editProductDto.ProductId);
                if (existingProduct == null)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("PRODUCT_NOT_FOUND")
                    );
                }

                existingProduct.Title = editProductDto.Title;
                existingProduct.Description = editProductDto.Description;
                existingProduct.Price = editProductDto.Price;
                existingProduct.CategoryId = editProductDto.CategoryId;
                existingProduct.RefundGuaranteedDuration = editProductDto.RefundGuaranteedDuration;
                existingProduct.MediaBoolean = editProductDto.MediaBoolean;
                existingProduct.Status = editProductDto.ProductStatus;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                if (editProductDto.updateMediaBoolean && editProductDto.Media != null)
                {
                    await _mediaService.DeleteMedia(existingProduct.Id, "PRODUCT");

                    foreach (var mediaDto in editProductDto.Media)
                    {
                        await _mediaService.CreateMedia(existingProduct.Id, "PRODUCT", mediaDto.MediaUrl);
                    }
                }

                await _productRepository.UpdateProduct(existingProduct);

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Product updated successfully.",
                    Data = default
                };
            }
            catch (CustomException<GeneralMessageDTO> customEx)
            {
                Console.Error.WriteLine($"CustomException: {customEx.Response.Message}");
                return new ResponseModel<T>
                {
                    Success = customEx.Response.Success,
                    Code = customEx.Response.Code,
                    Message = customEx.Response.Message,
                    Data = customEx.Response.Data == null ? default(T)! : (T)(object)customEx.Response.Data
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error updating product: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while updating the product.",
                    Data = default
                };
            }
        }



        public async Task<ResponseModel<List<T>>> GetUserProductsByStatus<T>(string status, ClaimsPrincipal userClaims)
        {
            try
            {
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);

                var products = await _productRepository.GetProductsByStatusAndUserId(status, userId);

                if (products == null || !products.Any())
                {
                    return new ResponseModel<List<T>>
                    {
                        Success = false,
                        Code = "404",
                        Message = $"No products found with status '{status}'.",
                        Data = new List<T>()
                    };
                }

                var productDetails = await Task.WhenAll(products.Select(async product =>
                {
                    var productUrl = product.MediaBoolean
                        ? (await _mediaService.GetFirstMediaBySourceIdAndType(product.Id, "PRODUCT"))?.MediaUrl
                        : null;
                    var latestBuyerItem = await _buyerItemRepository.GetLatestBuyerItemByProductId(product.Id);

                    string paymentStatus = "none";
                    if (status == "available")
                    {
                        if (latestBuyerItem != null)
                        {
                            paymentStatus = latestBuyerItem.Status == "pending" ? "order placed" : "none";
                        }
                    }
                    else if (status == "sold out")
                    {
                        if (latestBuyerItem != null && latestBuyerItem.ValidRefundDate != null)
                        {
                            paymentStatus = DateOnly.FromDateTime(DateTime.UtcNow) <= latestBuyerItem.ValidRefundDate ? "on hold" : "payment done";
                        }
                    }
                    else if (status == "request refund")
                    {
                        paymentStatus = "processing refund";
                    }

                    return new ProductResponseDTO
                    {
                        ProductId = product.Id,
                        CategoryId = product.CategoryId,
                        ProductTitle = product.Title,
                        ProductPrice = product.Price,
                        ProductStatus = product.Status,
                        ProductPaymentStatus = paymentStatus,
                        MediaUrl = productUrl
                    };
                }));

                return new ResponseModel<List<T>>
                {
                    Success = true,
                    Code = "200",
                    Message = $"Products with status '{status}' fetched successfully.",
                    Data = productDetails.Cast<T>().ToList()
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching products by status: {ex.Message}");
                return new ResponseModel<List<T>>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while fetching products by status.",
                    Data = default
                };
            }
        }

        public async Task<ResponseModel<T>> ViewProductDetails<T>(int productId, ClaimsPrincipal userClaims)
        {
            try
            {
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);

                var product = await _productRepository.GetProductById(productId);
                if (product == null || product.UserId != userId)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("PRODUCT_NOT_FOUND"),
                        new GeneralMessageDTO
                        {
                            Message = "Product not found or access denied.",
                            Success = false
                        }
                    );
                }

                List<MediaDTO> mediaList = new List<MediaDTO>();
                if (product.MediaBoolean)
                {
                    var media = await _mediaRepository.GetMediaListBySourceIdAndType(product.Id, "PRODUCT");
                    mediaList = media != null && media.Any()
                        ? media.Select(m => new MediaDTO
                        {
                            Id = m.Id,
                            MediaUrl = m.MediaUrl,
                            CreatedAt = m.CreatedAt,
                            UpdatedAt = m.UpdatedAt
                        }).ToList()
                        : new List<MediaDTO>();
                }

                var productDetails = new ProductDetailsDTO
                {
                    ProductId = product.Id,
                    Title = product.Title,
                    Description = product.Description,
                    Price = product.Price,
                    RefundGuaranteedDuration = product.RefundGuaranteedDuration,
                    CreatedAt = product.CreatedAt,
                    UserId = product.User.Id,
                    UserName = product.User.Name,
                    Media = mediaList
                };

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Product details retrieved successfully.",
                    Data = (T)(object)productDetails
                };
            }
            catch (CustomException<GeneralMessageDTO> customEx)
            {
                Console.Error.WriteLine($"CustomException: {customEx.Response.Message}");
                return new ResponseModel<T>
                {
                    Success = customEx.Response.Success,
                    Code = customEx.Response.Code,
                    Message = customEx.Response.Message,
                    Data = customEx.Response.Data == null ? default(T)! : (T)(object)customEx.Response.Data
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving product details: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while retrieving the product details.",
                    Data = default
                };
            }
        }
    }

}
