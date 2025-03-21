using System;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using PotatoTrade.DTO.Notification;
using PotatoTrade.Service.Notification;
using PototoTrade.DTO.Common;
using PototoTrade.DTO.Product;
using PototoTrade.Models.BuyerItem;
using PototoTrade.Models.Media;
using PototoTrade.Models.Product;
using PototoTrade.Models.ShoppingCart;
using PototoTrade.Models.User;
using PototoTrade.Repository.BuyerItem;
using PototoTrade.Repository.Cart;
using PototoTrade.Repository.MediaRepo;
using PototoTrade.Repository.Product;
using PototoTrade.Repository.Users;
using PototoTrade.Repository.Wallet;
using PototoTrade.Service.Utilities.Exceptions;
using PototoTrade.Service.Utilities.Response;
using PototoTrade.Service.Wallet;

namespace PototoTrade.Service.Product
{
    public class ProductSrvBsn
    {
        private readonly ProductRepository _productRepository;

        private readonly MediaRepository _mediaRepository;

        private readonly UserAccountRepository _userRepository;

        private readonly WalletRepository _walletRepository;

        private readonly UserWalletService _userWalletService;

        private readonly MediaSrv _mediaService;

        private readonly ShoppingCartRepository _shoppingCartRepository;

        private readonly PurchaseOrderRepository _purchaseOrderRepository;

        private readonly BuyerItemRepository _buyerItemRepository;

        private readonly WalletTransactionRepository _walletTransactionRepository;

        private readonly NotificationService _notificationService;

        private readonly IHubContext<NotificationHub> _notificationHubContext;

        public ProductSrvBsn(ProductRepository productRepository, MediaRepository mediaRepository, UserAccountRepository userRepository,
        UserWalletService userWalletService, WalletRepository walletRepository, MediaSrv mediaSrv, PurchaseOrderRepository purchaseOrderRepository,NotificationService notificationService,
         ShoppingCartRepository shoppingCartRepository, BuyerItemRepository buyerItemRepository,
         WalletTransactionRepository walletTransactionRepository, ILogger<ProductSrv> logger, IHubContext<NotificationHub> notificationHubContext)
        {
            _productRepository = productRepository;
            _mediaRepository = mediaRepository;
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _userWalletService = userWalletService;
            _mediaService = mediaSrv;
            _purchaseOrderRepository = purchaseOrderRepository;
            _shoppingCartRepository = shoppingCartRepository;
            _buyerItemRepository = buyerItemRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _notificationService = notificationService;
            _notificationHubContext = notificationHubContext;
        }

        public async Task<ResponseModel<T>> CreateProduct<T>(ClaimsPrincipal userClaims, CreateProductDTO createProductDto)
        {
            try
            {

                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                var userRole = userClaims.FindFirst(ClaimTypes.Role)?.Value;
                // Check if UserId exists
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("USER_NOT_FOUND"),
                        new GeneralMessageDTO
                        {
                            Message = $"User with ID {userId} does not exist.",
                            Success = false
                        }
                    );
                }

                var category = await _productRepository.GetCategoryById(createProductDto.CategoryId);
                if (category == null)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("CATEGORY_NOT_FOUND"),
                        new GeneralMessageDTO
                        {
                            Message = $"Category with ID {createProductDto.CategoryId} does not exist.",
                            Success = false
                        }
                    );
                }

                var chargeFee = createProductDto.Price * (decimal)category.ChargeRate;

                await _userWalletService.ChargeFee(userId, chargeFee);

                var newProduct = new Products
                {
                    UserId = userId,
                    CategoryId = createProductDto.CategoryId,
                    MediaBoolean = createProductDto.MediaBoolean,
                    Title = createProductDto.Title,
                    Description = createProductDto.Description,
                    Price = createProductDto.Price,
                    RefundGuaranteedDuration = createProductDto.RefundGuaranteedDuration,
                    Status = "available",
                    CreatedAt = DateTime.UtcNow
                };

                await _productRepository.CreateProduct(newProduct);

                if (createProductDto.MediaBoolean && createProductDto.Media != null)
                {
                    foreach (var mediaDto in createProductDto.Media)
                    {
                        await _mediaService.CreateMedia(newProduct.Id, "PRODUCT", mediaDto.MediaUrl);
                    }
                }

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Product created successfully, and charge fee applied.",
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
                Console.Error.WriteLine($"Error creating product: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while creating the product.",
                    Data = default
                };
            }
        }

        public async Task<ResponseModel<T>> PlaceOrder<T>(ClaimsPrincipal userClaims)
        {
            try
            {
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);

                // Retrieve shopping cart
                var shoppingCart = await _shoppingCartRepository.GetShoppingCartByUserId(userId);
                if (shoppingCart == null)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("CART_NOT_FOUND"),
                        new GeneralMessageDTO
                        {
                            Message = "Shopping cart not found.",
                            Success = false
                        }
                    );
                }

                var availableItems = shoppingCart.ShoppingCartItems
                    .Where(item => item.Product.Status == "available")
                    .ToList();

                if (!availableItems.Any())
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("NO_AVAILABLE_ITEMS"),
                        new GeneralMessageDTO
                        {
                            Message = "No available items in the shopping cart.",
                            Success = false
                        }
                    );
                }

                // Check for an existing pending order
                var pendingOrder = await _purchaseOrderRepository.GetPendingOrderByUserId(userId);
                if (pendingOrder != null)
                {
                    await AddItemsToPendingOrder(pendingOrder, availableItems, userId);

                    return new ResponseModel<T>
                    {
                        Success = true,
                        Code = "200",
                        Message = "Items added to the existing pending order.",
                        Data = (T)(object)new PlaceOrderResponseDTO
                        {
                            PurchaseOrderId = pendingOrder.Id,
                            OrderDate = pendingOrder.OrderCreatedAt
                        }
                    };
                }

                // Create a new order
                var newOrder = new PurchaseOrder
                {
                    UserId = userId,
                    CartId = shoppingCart.Id,
                    TotalAmount = availableItems.Sum(item => item.Product.Price),
                    Status = "pending",
                    OrderCreatedAt = DateTime.UtcNow
                };

                var newOrderId = await _purchaseOrderRepository.CreatePurchaseOrder(newOrder);

                await AddItemsToNewOrder(newOrderId, availableItems, userId);

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Order placed successfully.",
                    Data = (T)(object)new PlaceOrderResponseDTO
                    {
                        PurchaseOrderId = newOrderId,
                        OrderDate = newOrder.OrderCreatedAt
                    }
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
                    Data = default
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error placing order: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while placing the order.",
                    Data = default
                };
            }
        }

        // Helper method to handle adding items to an existing order
        private async Task AddItemsToPendingOrder(PurchaseOrder pendingOrder, List<ShoppingCartItem> availableItems, int userId)
        {
            foreach (var cartItem in availableItems)
            {
                cartItem.Product.Status = "not available";
                cartItem.Product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateProduct(cartItem.Product);

                var buyerItem = new BuyerItems
                {
                    OrderId = pendingOrder.Id,
                    ProductId = cartItem.ProductId,
                    BuyerId = userId,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _buyerItemRepository.CreateBuyerItem(buyerItem);
            }

            pendingOrder.TotalAmount += availableItems.Sum(item => item.Product.Price);
            await _purchaseOrderRepository.UpdatePurchaseOrder(pendingOrder);
            await _shoppingCartRepository.DeleteShoppingCartItems(availableItems);
        }

        // Helper method to handle adding items to a new order
        private async Task AddItemsToNewOrder(int newOrderId, List<ShoppingCartItem> availableItems, int userId)
        {
            foreach (var cartItem in availableItems)
            {
                cartItem.Product.Status = "not available";
                cartItem.Product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateProduct(cartItem.Product);

                var buyerItem = new BuyerItems
                {
                    OrderId = newOrderId,
                    ProductId = cartItem.ProductId,
                    BuyerId = userId,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                };

                await _buyerItemRepository.CreateBuyerItem(buyerItem);
            }

            await _shoppingCartRepository.DeleteShoppingCartItems(availableItems);
        }


        public async Task<ResponseModel<T>> CancelItemInOrder<T>(CancelItemRequestDTO cancelItemRequest)
        {
            try
            {

                var purchaseOrder = await _purchaseOrderRepository.GetPurchaseOrderById(cancelItemRequest.PurchaseOrderId);
                if (purchaseOrder == null)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("PURCHASE_ORDER_NOT_FOUND")
                    );
                }

                var buyerItem = await _buyerItemRepository.GetBuyerItemByOrderAndProduct(cancelItemRequest.PurchaseOrderId, cancelItemRequest.ProductId);
                if (buyerItem == null)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("BUYER_ITEM_NOT_FOUND")
                    );
                }

                var product = buyerItem.Product;
                product.Status = "available";
                product.UpdatedAt = DateTime.UtcNow;

                await _productRepository.UpdateProduct(product);

                purchaseOrder.TotalAmount -= buyerItem.Product.Price;

                await _buyerItemRepository.RemoveBuyerItem(buyerItem);

                await _purchaseOrderRepository.UpdatePurchaseOrder(purchaseOrder);


                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Item removed from the order successfully.",
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
                Console.Error.WriteLine($"Error cancelling item in order: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while cancelling the item in the order.",
                    Data = default
                };
            }
        }

        public async Task<ResponseModel<T>> FetchRebateAmountList<T>(int orderId)
        {
            try
            {
                var purchaseOrder = await _purchaseOrderRepository.GetPurchaseOrderById(orderId);
                if (purchaseOrder == null)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("PURCHASE_ORDER_NOT_FOUND")
                    );
                }

                if (purchaseOrder.BuyerItems == null || !purchaseOrder.BuyerItems.Any())
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("PURCHASE_ORDER_ITEMS_NOT_FOUND")
                    );
                }

                var rebateItems = new List<RebateAmountDTO>();
                decimal totalDiscountedPrice = 0;

                foreach (var buyerItem in purchaseOrder.BuyerItems)
                {
                    var product = buyerItem.Product;
                    var category = await _productRepository.GetProductCategoryByProductIdAsync(product.Id);
                    if (product == null)
                    {
                        throw new Exception($"BuyerItem {buyerItem.Id} has no associated product.");
                    }

                    if (category == null)
                    {
                        throw new Exception($"Product {product.Id} has no associated category.");
                    }

                    var rebateRate = (decimal)category.RebateRate;
                    var discountedPrice = product.Price - (product.Price * rebateRate);
                    var deliveryCost = product.Price * 0.05m;

                    rebateItems.Add(new RebateAmountDTO
                    {
                        ProductName = product.Title,
                        ProductId = product.Id,
                        RebateRate = rebateRate * 100,
                        FinalPrice = discountedPrice,
                        DeliveryFee = deliveryCost
                    });

                    totalDiscountedPrice += discountedPrice + deliveryCost;
                }

                var rebateListResponse = new RebateAmountListDTO
                {
                    PurchaseOrderId = orderId,
                    FinalPrice = totalDiscountedPrice,
                    Items = rebateItems
                };

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Rebate amount list fetched successfully.",
                    Data = (T)(object)rebateListResponse
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
                Console.Error.WriteLine($"Error fetching rebate amount list: {ex}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while fetching the rebate amount list.",
                    Data = default
                };
            }
        }


        public async Task<ResponseModel<T>> MakePayment<T>(MakePaymentRequestDTO paymentRequest, ClaimsPrincipal userClaims)
        {
            try
            {
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                var purchaseOrder = await _purchaseOrderRepository.GetPurchaseOrderById(paymentRequest.PurchaseOrderId);
                if (purchaseOrder == null)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("PURCHASE_ORDER_NOT_FOUND")
                    );
                }
                var userWallet = await _walletRepository.GetWalletByUserIdAsync(userId);
                if (userWallet == null || userWallet.AvailableBalance < paymentRequest.FinalPrice)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("INSUFFICIENT_BALANCE")
                    );
                }
                userWallet.AvailableBalance -= paymentRequest.FinalPrice;
                await _walletRepository.UpdateWalletAsync(userWallet);

                var userTransaction = new WalletTransaction
                {
                    WalletId = userWallet.Id,
                    Amount = -paymentRequest.FinalPrice,
                    TransactionType = "Order Payment",
                    CreatedAt = DateTime.UtcNow
                };
                await _walletTransactionRepository.CreateTransaction(userTransaction);
                

                foreach (var item in paymentRequest.RebateItems)
                {
                    var buyerItem = await _buyerItemRepository.GetBuyerItemByOrderAndProduct(paymentRequest.PurchaseOrderId, item.ProductId);
                    if (buyerItem == null) continue;
                    buyerItem.Status = "done payment";
                    buyerItem.UpdatedAt = DateTime.UtcNow;
                    await _buyerItemRepository.UpdateBuyerItem(buyerItem);

                    buyerItem.Product.UpdatedAt = DateTime.UtcNow;
                    buyerItem.Product.Status = "sold out";
                    await _productRepository.UpdateProduct(buyerItem.Product);

                    var product = buyerItem.Product;

                    var adminWallet = await _walletRepository.GetPlatformWallet();
                    var category = await _productRepository.GetProductCategoryByProductIdAsync(product.Id);

                    if (category == null)
                    {
                        throw new Exception($"Product {product.Id} has no associated category.");
                    }
                    decimal rebateAmount = buyerItem.Product.Price * (decimal)category.RebateRate;


                    adminWallet.AvailableBalance = adminWallet.AvailableBalance - rebateAmount + item.DeliveryFee;
                    await _walletRepository.UpdateWalletAsync(adminWallet);

                    var AdminRebateToUserTransaction = new WalletTransaction
                    {
                        WalletId = adminWallet.Id,
                        Amount = -rebateAmount,
                        TransactionType = "Cost",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _walletTransactionRepository.CreateTransaction(AdminRebateToUserTransaction);
                    var deliveryTransaction = new WalletTransaction
                    {
                        WalletId = adminWallet.Id,
                        Amount = item.DeliveryFee,
                        TransactionType = "Revenue",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _walletTransactionRepository.CreateTransaction(deliveryTransaction);

                    var productOwnerWallet = await _walletRepository.GetWalletByUserIdAsync(buyerItem.Product.UserId);
                    if (productOwnerWallet == null)
                    {
                        throw new CustomException<GeneralMessageDTO>(
                            ExceptionEnum.GetException("WALLET_NOT_FOUND")
                        );
                    }

                    if (buyerItem.Product.RefundGuaranteedDuration > 0)
                    {

                        productOwnerWallet.OnHoldBalance += buyerItem.Product.Price;
                        await _walletRepository.UpdateWalletAsync(productOwnerWallet);

                        var OnHoldTransaction = new WalletTransaction
                        {
                            WalletId = productOwnerWallet.Id,
                            Amount = buyerItem.Product.Price,
                            TransactionType = "On Hold On Refundable Product",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _walletTransactionRepository.CreateTransaction(OnHoldTransaction);


                    }
                    else
                    {
                        productOwnerWallet.AvailableBalance += buyerItem.Product.Price;
                        await _walletRepository.UpdateWalletAsync(productOwnerWallet);

                        var ownerTransaction = new WalletTransaction
                        {
                            WalletId = productOwnerWallet.Id,
                            Amount = buyerItem.Product.Price,
                            TransactionType = "Earning from Product",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _walletTransactionRepository.CreateTransaction(ownerTransaction);
                    }

                    var delivery = new BuyerItemDelivery
                    {
                        BuyerItemId = buyerItem.Id,
                        StageTypes = "on packaging",
                        StageDescription = $"{buyerItem.Product.Title} is on packaging at {DateTime.UtcNow}.",
                        Status = "delivering",
                        StageDate = DateTime.UtcNow
                    };
                    await _buyerItemRepository.CreateBuyerItemDelivery(delivery);
                    List<SystemInnerNotificationDto> orderNotifactionList = new List<SystemInnerNotificationDto>();

                    SystemInnerNotificationDto currentOrderNotification = new SystemInnerNotificationDto();
                    var sellerDetails = await _userRepository.GetUserByIdAsync(product.UserId);
                    currentOrderNotification.ReceiverId = sellerDetails.Id;
                    currentOrderNotification.SenderUsername = "System";
                    currentOrderNotification.ReceiverUsername = sellerDetails.Username;
                    currentOrderNotification.Title = $"Product Sold!";
                    currentOrderNotification.MessageText = $"{product.Title} has been purchased by a buyer!";
                    orderNotifactionList.Add(currentOrderNotification);
                    await _notificationService.createOrderPurchasedNotificationandSaveToDB(userClaims, orderNotifactionList);
                    //var orderAA = await _notificationService.createOrderPurchasedNotificationandSaveToDB(userClaims, orderNotifactionList);
                // Console.WriteLine($"==============: {orderAA.Data}");
                //                 Console.WriteLine($"HERE HERE HERE: {sellerDetails.Id}");
                // await _notificationHubContext.Clients.Group($"User-{sellerDetails.Id}").SendAsync("ReceivePurchasedNotification", orderAA.Data);
                }

                
                
                purchaseOrder.TotalAmount = paymentRequest.FinalPrice;
                purchaseOrder.Status = "done payment";
                await _purchaseOrderRepository.UpdatePurchaseOrder(purchaseOrder);

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Payment processed successfully.",
                    Data = (T)(object)new GeneralMessageDTO
                    {
                        Message = "Payment completed, items are being processed for delivery.",
                        Success = true
                    }
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
                Console.Error.WriteLine($"Error processing payment: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while processing the payment.",
                    Data = default
                };
            }
        }

        public async Task<ResponseModel<T>> ViewPendingOrder<T>(ClaimsPrincipal userClaims)
        {
            try
            {
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);

                // Fetch the pending order
                var pendingOrder = await _purchaseOrderRepository.GetPendingOrderByUserId(userId);
                if (pendingOrder == null)
                {
                    // Handle when no pending order is found
                    return new ResponseModel<T>
                    {
                        Success = false,
                        Code = "404",
                        Message = "No pending purchase order found.",
                        Data = default // Explicitly return default for type T
                    };
                }

                // Map the pending order to PurchaseOrderDTO
                var responseDTO = new PurchaseOrderDTO
                {
                    PurchaseOrderId = pendingOrder.Id,
                    TotalAmount = pendingOrder.TotalAmount,
                    OrderCreatedAt = pendingOrder.OrderCreatedAt,
                    BuyerItems = (await Task.WhenAll(pendingOrder.BuyerItems.Select(async item =>
                    {
                        var media = item.Product.MediaBoolean
                            ? await _mediaService.GetFirstMediaBySourceIdAndType(item.Product.Id, "PRODUCT")
                            : null;

                        var productUrl = media?.MediaUrl;
                        return new BuyerItemDTO
                        {
                            ProductName = item.Product.Title,
                            ProductPrice = item.Product.Price,
                            ProductUrl = productUrl,
                            ProductOwner = item.Product.User?.Name ?? "Unknown",
                            BuyerItemStatus = item.Status
                        };
                    }))).ToList()
                };

                // Return the mapped DTO
                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Pending purchase order fetched successfully.",
                    Data = (T)(object)responseDTO // Cast to generic type T
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching pending purchase order: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while fetching the pending purchase order.",
                    Data = default // Ensure consistent type handling
                };
            }
        }


        public async Task<ResponseModel<T>> ViewPurchaseOrderItems<T>(int purchaseOrderId)
        {
            try
            {
                var purchaseOrder = await _purchaseOrderRepository.GetPurchaseOrderById(purchaseOrderId);
                if (purchaseOrder == null)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("PURCHASE_ORDER_NOT_FOUND"),
                        new GeneralMessageDTO
                        {
                            Message = "Purchase order not found.",
                            Success = false
                        }
                    );
                }

                var responseDTO = new PurchaseOrderDTO
                {
                    PurchaseOrderId = purchaseOrder.Id,
                    TotalAmount = purchaseOrder.TotalAmount,
                    OrderCreatedAt = purchaseOrder.OrderCreatedAt,
                    BuyerItems = (await Task.WhenAll(purchaseOrder.BuyerItems.Select(async item =>
                    {
                        var media = item.Product.MediaBoolean
                            ? await _mediaService.GetFirstMediaBySourceIdAndType(item.Product.Id, "PRODUCT")
                            : null;

                        var productUrl = media?.MediaUrl;

                        return new BuyerItemDTO
                        {
                            ProductName = item.Product.Title,
                            ProductPrice = item.Product.Price,
                            ProductUrl = productUrl,
                            ProductOwner = item.Product.User.Name,
                            BuyerItemStatus = item.Status
                        };
                    }))).ToList()
                };

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Purchase order items fetched successfully.",
                    Data = (T)(object)responseDTO
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
                Console.Error.WriteLine($"Error fetching purchase order items: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while fetching the purchase order items.",
                    Data = default
                };
            }
        }

        public async Task<ResponseModel<T>> AcceptRefund<T>(RefundDTO refundDto, ClaimsPrincipal userClaims)
        {
            try
            {
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);

                var buyerItem = await _buyerItemRepository.GetBuyerItemById(refundDto.BuyerItemId);

                var product = buyerItem.Product;
                product.Status = "available";
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateProduct(product);

                buyerItem.Status = "refunded";
                buyerItem.UpdatedAt = DateTime.UtcNow;
                await _buyerItemRepository.UpdateBuyerItem(buyerItem);
                await _userWalletService.RefundAmount(
                    buyerItem.Product.UserId,
                    buyerItem.Order.UserId,
                    buyerItem.Product.Price
                );

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = $"{buyerItem.Product.Title} has been refunded to the buyer.",
                    Data = (T)(object)new GeneralMessageDTO
                    {
                        Message = $"{buyerItem.Product.Title} has been refunded successfully.",
                        Success = true
                    }
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
                Console.Error.WriteLine($"Error processing refund: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while processing the refund.",
                    Data = default
                };
            }
        }

        public async Task<ResponseModel<T>> RejectOrCancelRefund<T>(RefundDTO refundDto, ClaimsPrincipal userClaims)
        {
            try
            {
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                var buyerItem = await _buyerItemRepository.GetBuyerItemById(refundDto.BuyerItemId);
                var order = await _purchaseOrderRepository.GetPurchaseOrderById(buyerItem.OrderId);

                if (buyerItem == null)
                {
                    throw new Exception($"BuyerItem with ID {refundDto.BuyerItemId} not found.");
                }
                if (buyerItem.Product == null)
                {
                    throw new Exception($"Product associated with BuyerItem ID {refundDto.BuyerItemId} is null.");
                }
                if (order == null)
                {
                    throw new Exception($"Order associated with BuyerItem ID {refundDto.BuyerItemId} is null.");
                }

                var product = buyerItem.Product;
                int whoIsBeingRejected = userId == product.UserId ? order.UserId : product.UserId;

                buyerItem.Status = "received";
                buyerItem.UpdatedAt = DateTime.UtcNow;
                await _buyerItemRepository.UpdateBuyerItem(buyerItem);

                product.Status = "sold out";
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateProduct(product);

                string responseMessage = whoIsBeingRejected == buyerItem.Order.UserId
                    ? $"{product.Title} refund request has been cancelled"
                    : $"{product.Title} refund request has been rejected ";

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = responseMessage,
                    Data = (T)(object)new GeneralMessageDTO
                    {
                        Message = responseMessage,
                        Success = true
                    }
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error rejecting refund request: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while rejecting the refund request.",
                    Data = default
                };
            }
        }
    }
}
