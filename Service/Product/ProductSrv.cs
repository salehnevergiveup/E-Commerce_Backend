using System;
using System.Security.Claims;
using PototoTrade.DTO.Common;
using PototoTrade.DTO.Product;
using PototoTrade.Models.BuyerItem;
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
                        ProductCategoryName = product.Category.ProductCategoryName,
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
                    Data = default(T) ?? (T)(object)productList
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
                    Data = customEx.Response.Data == null ? default! : (T)(object)customEx.Response.Data
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
                    Data = default!
                };
            }
        }

        public async Task<ResponseModel<T>> ViewAvailableProductsByCategory<T>(ViewProductsRequestDTO request)
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
                    if (product.Status != "available")
                    {
                        continue;
                    }

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
                        ProductCategoryName = product.Category.ProductCategoryName,
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

        public async Task<ResponseModel<T>> ViewAllAvailableProducts<T>()
        {
            try
            {
                var products = await _productRepository.GetAllProducts();

                if (products == null || !products.Any())
                {
                    Console.Error.WriteLine("Products is null or empty");
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
                    var category = await _productRepository.GetProductCategoryByProductIdAsync(product.Id);
                    if (product == null)
                    {
                        Console.Error.WriteLine("A product is null");
                        continue;
                    }

                    if (product.Status != "available")
                    {
                        continue;
                    }

                    if (category == null)
                    {
                        throw new Exception($"Product {product.Id} has no category.");
                    }

                    if (product.User == null)
                    {
                        throw new Exception($"Product {product.Id} has no associated user.");
                    }

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
                        ProductCategoryName = category.ProductCategoryName ?? "Unknown",
                        RefundGuaranteedDuration = product.RefundGuaranteedDuration,
                        CreatedAt = product.CreatedAt,
                        UserId = product.User.Id,
                        UserName = product.User.Name ?? "Unknown",
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
                Console.WriteLine("Starting EditProduct method.");

                // Extract user information
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                var userRole = userClaims.FindFirst(ClaimTypes.Role)?.Value;
                Console.WriteLine($"User ID: {userId}, Role: {userRole}");

                var buyerItem = await _buyerItemRepository.GetBuyerItemByProductIdAndStatus(editProductDto.ProductId, "pending");
                if (buyerItem != null)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("Product_Update_Not_Available"),
                        new GeneralMessageDTO
                        {
                            Message = $"Product : {editProductDto.Title} has been ordered by someone, changes cannot be applied",
                            Success = false
                        }
                    );
                }

                // Fetch the existing product
                var existingProduct = await _productRepository.GetProduct(editProductDto.ProductId);
                if (existingProduct == null)
                {
                    Console.WriteLine($"Product with ID {editProductDto.ProductId} not found.");
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("PRODUCT_NOT_FOUND"),
                        new GeneralMessageDTO
                        {
                            Message = $"Product with ID {editProductDto.ProductId} not found.",
                            Success = false
                        }
                    );
                }

                Console.WriteLine("Existing product fetched successfully.");
                Console.WriteLine($"Product Details: ID={existingProduct.Id}, Title={existingProduct.Title}");

                // Update product fields
                existingProduct.Title = editProductDto.Title;
                existingProduct.Description = editProductDto.Description;
                existingProduct.Price = editProductDto.Price;
                existingProduct.CategoryId = editProductDto.CategoryId;
                existingProduct.RefundGuaranteedDuration = editProductDto.RefundGuaranteedDuration;
                existingProduct.MediaBoolean = editProductDto.MediaBoolean;
                existingProduct.Status = editProductDto.ProductStatus;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                Console.WriteLine("Product fields updated.");

                // Handle media updates
                if (editProductDto.updateMediaBoolean)
                {
                    Console.WriteLine("Updating media...");
                    if (editProductDto.Media == null || !editProductDto.Media.Any())
                    {
                        Console.WriteLine("Media list is null or empty.");
                    }
                    else
                    {
                        if (existingProduct.MediaBoolean)
                        {
                            Console.WriteLine($"Deleting existing media for product ID {existingProduct.Id}.");
                            var existmedia = await _mediaService.GetFirstMediaBySourceIdAndType(editProductDto.ProductId, "PRODUCT");

                            if (existmedia != null)
                            {
                                await _mediaService.DeleteMedia(existingProduct.Id, "PRODUCT");
                            }
                        }

                        foreach (var mediaDto in editProductDto.Media)
                        {
                            Console.WriteLine($"Creating media: {mediaDto.MediaUrl} for product ID {existingProduct.Id}.");
                            await _mediaService.CreateMedia(existingProduct.Id, "PRODUCT", mediaDto.MediaUrl);
                        }
                    }
                }

                // Save changes
                Console.WriteLine($"Updating product in the database. Product ID: {existingProduct.Id}");
                await _productRepository.UpdateProduct(existingProduct);

                Console.WriteLine("Product updated successfully.");

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
                Console.Error.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while updating the product.",
                    Data = default
                };
            }
        }

        public async Task<ResponseModel<T>> DeleteProduct<T>(int productId, ClaimsPrincipal userClaims)
        {
            try
            {
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                var product = await _productRepository.GetProductById(productId);
                if (product == null)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("PRODUCT_NOT_FOUND"),
                        new GeneralMessageDTO
                        {
                            Message = "Product not found.",
                            Success = false
                        }
                    );
                }

                if (product.Status != "available")
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("PRODUCT_NOT_DELETABLE")

                    );
                }
                if (product.MediaBoolean)
                {
                    await _mediaService.DeleteMedia(product.Id, "PRODUCT");
                }

                await _productRepository.DeleteProductAsync(product);

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Product deleted successfully.",
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
                Console.Error.WriteLine($"Error deleting product: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while deleting the product.",
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
                var productDetails = new List<ProductResponseDTO>();

                foreach (var product in products)
                {
                    var productUrl = product.MediaBoolean
                        ? (await _mediaService.GetFirstMediaBySourceIdAndType(product.Id, "PRODUCT"))?.MediaUrl
                        : null;

                    var latestBuyerItem = await _buyerItemRepository.GetLatestBuyerItemByProductId(product.Id);

                    string paymentStatus = DeterminePaymentStatus(status, latestBuyerItem);

                    productDetails.Add(new ProductResponseDTO
                    {
                        ProductId = product.Id,
                        CategoryId = product.CategoryId,
                        ProductTitle = product.Title,
                        ProductPrice = product.Price,
                        ProductStatus = product.Status,
                        ProductPaymentStatus = paymentStatus,
                        MediaUrl = productUrl
                    });
                }
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

        private string DeterminePaymentStatus(string status, BuyerItems latestBuyerItem)
        {
            return status switch
            {
                "available" => latestBuyerItem?.Status == "pending" ? "order placed" : "none",
                "sold out" => latestBuyerItem != null && latestBuyerItem.ValidRefundDate != null &&
                               DateOnly.FromDateTime(DateTime.UtcNow) <= latestBuyerItem.ValidRefundDate ? "on hold" : "payment done",
                "request refund" => "processing refund",
                _ => "none"
            };
        }

        public async Task<ResponseModel<T>> ViewProductDetails<T>(int productId, ClaimsPrincipal userClaims)
        {
            try
            {
                Console.WriteLine($"Start ViewProductDetails - ProductId: {productId}");

                // Retrieve the user ID from claims
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                Console.WriteLine($"User ID from claims: {userId}");

                // Fetch the product details by ID
                var product = await _productRepository.GetProductById(productId);
                if (product == null || product.UserId != userId)
                {
                    Console.WriteLine($"Product not found or access denied - ProductId: {productId}, UserId: {userId}");
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("PRODUCT_NOT_FOUND"),
                        new GeneralMessageDTO
                        {
                            Message = "Product not found or access denied.",
                            Success = false
                        }
                    );
                }

                Console.WriteLine($"Product found - ProductId: {product.Id}, UserId: {product.UserId}");

                // Fetch associated media if MediaBoolean is true
                List<MediaDTO> mediaList = new List<MediaDTO>();
                if (product.MediaBoolean)
                {
                    Console.WriteLine($"Fetching media for ProductId: {product.Id}");
                    var media = await _mediaRepository.GetMediaListBySourceIdAndType(product.Id, "PRODUCT");

                    if (media != null && media.Any())
                    {
                        Console.WriteLine($"Media found for ProductId: {product.Id}, Count: {media.Count}");
                        mediaList = media.Select(m => new MediaDTO
                        {
                            Id = m.Id,
                            MediaUrl = m.MediaUrl,
                            CreatedAt = m.CreatedAt,
                            UpdatedAt = m.UpdatedAt
                        }).ToList();
                    }
                    else
                    {
                        Console.WriteLine($"No media found for ProductId: {product.Id}");
                    }
                }
                var category = await _productRepository.GetProductCategoryByProductIdAsync(product.Id);
                // Construct the product details DTO
                Console.WriteLine($"Constructing ProductDetailsDTO for ProductId: {product.Id}");

                if (category == null)
                {
                    throw new Exception($"Product {product.Id} has no category.");
                }
                var productDetails = new ProductDetailsDTO
                {
                    ProductId = product.Id,
                    Title = product.Title,
                    Description = product.Description,
                    Price = product.Price,
                    CategoryId = category.Id,
                    RefundGuaranteedDuration = product.RefundGuaranteedDuration,
                    ProductCategoryName = category.ProductCategoryName ?? "Unknown",
                    CreatedAt = product.CreatedAt,
                    UserId = product.User.Id,
                    UserName = product.User.Name,
                    Media = mediaList
                };

                Console.WriteLine($"Returning product details for ProductId: {product.Id}");
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
        public async Task<ResponseModel<T>> getRefundbuyeritemid<T>(int productId, ClaimsPrincipal userClaims)
        {
            try
            {
                // Retrieve the user ID from the ClaimsPrincipal
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);

                // Call repository to get the latest buyer item by product ID
                var latestBuyerItem = await _buyerItemRepository.GetBuyerItemByProductIdAndStatus(productId, "refunding");

                // Check if buyer item exists and if it belongs to the user
                if (latestBuyerItem == null)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("BUYER_ITEM_NOT_FOUND"),
                        new GeneralMessageDTO
                        {
                            Message = "No refunding buyer item found for this product.",
                            Success = false
                        }
                    );
                }

                // Return the buyer item ID as the response
                var responseDto = new BuyerItemIdResponseDTO
                {
                    BuyerItemId = latestBuyerItem.Id
                };

                // Return the buyer item ID as the response
                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Refunding buyer item found successfully.",
                    Data = (T)(object)responseDto
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
                Console.Error.WriteLine($"Error fetching refunding buyer item: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while fetching the refunding buyer item.",
                    Data = default
                };
            }
        }


    }

}
