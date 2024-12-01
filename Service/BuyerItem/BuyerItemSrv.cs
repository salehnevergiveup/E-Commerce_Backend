using System;
using System.Security.Claims;
using PototoTrade.DTO.Common;
using PototoTrade.Models.BuyerItem;
using PototoTrade.Models.Media;
using PototoTrade.Models.User;
using PototoTrade.Repository.BuyerItem;
using PototoTrade.Repository.MediaRepo;
using PototoTrade.Repository.OnHoldingPayment;
using PototoTrade.Repository.Product;
using PototoTrade.Repository.Wallet;
using PototoTrade.Service.Product;
using PototoTrade.Service.Utilities.Exceptions;
using PototoTrade.Service.Utilities.Response;

namespace PototoTrade.Service.BuyerItem
{
    public class BuyerItemService
    {
        private readonly BuyerItemRepository _buyerItemRepository;
        private readonly MediaSrv _mediaService;

        private readonly ProductRepository _productRepository;

        private readonly WalletRepository _walletRepository;

        private readonly OnHoldingPaymentHistoryRepository _onHoldingPaymentHistoryRepository;

        private readonly WalletTransactionRepository _walletTransactionRepository;

        public BuyerItemService(BuyerItemRepository buyerItemRepository, ProductRepository productRepository, 
        MediaSrv mediaService,OnHoldingPaymentHistoryRepository onHoldingPaymentHistoryRepository, WalletRepository walletRepository,
        WalletTransactionRepository walletTransactionRepository)
        {
            _buyerItemRepository = buyerItemRepository;
            _mediaService = mediaService;
            _productRepository = productRepository;
            _onHoldingPaymentHistoryRepository = onHoldingPaymentHistoryRepository;
            _walletRepository = walletRepository;
            _walletTransactionRepository = walletTransactionRepository;
        }

        public async Task<ResponseModel<List<T>>> ViewItemsByStatus<T>(string status, ClaimsPrincipal userClaims)
        {
            try
            {

                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                var buyerItems = await _buyerItemRepository.GetBuyerItemsByStatusAndUserId(status, userId);
                if (buyerItems == null || !buyerItems.Any())
                {
                    return new ResponseModel<List<T>>
                    {
                        Success = false,
                        Code = "450",
                        Message = $"No buyer items found with status '{status}'.",
                        Data = new List<T>()
                    };
                }

                var buyerItemDetails = await Task.WhenAll(buyerItems.Select(async item =>
                {
                    var productUrl = item.Product.MediaBoolean
                        ? (await _mediaService.GetFirstMediaBySourceIdAndType(item.Product.Id, "PRODUCT"))?.MediaUrl
                        : null;

                    var deliveries = item.BuyerItemDeliveries.Select(delivery => new BuyerItemDeliveryDTO
                    {
                        Stage = delivery.StageTypes,
                        StageDescription = delivery.StageDescription,
                        StageDate = delivery.StageDate
                    }).ToList();

                    return new BuyerItemDetailsDTO
                    {
                        PurchaseOrderId = item.OrderId,
                        BuyerItemId = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.Product.Title,
                        ProductUrl = productUrl,
                        ProductOwner = item.Product.User.Name,
                        BuyerItemStatus = item.Status,
                        RefundableBoolean = item.Product.RefundGuaranteedDuration > 0,
                        BuyerItemDelivery = deliveries
                    };
                }));

                return new ResponseModel<List<T>>
                {
                    Success = true,
                    Code = "200",
                    Message = "Buyer items fetched successfully.",
                    Data = buyerItemDetails.Cast<T>().ToList()
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching buyer items: {ex.Message}");
                return new ResponseModel<List<T>>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while fetching buyer items.",
                    Data = default
                };
            }
        }

        public async Task<ResponseModel<List<T>>> ViewRefundItems<T>(ClaimsPrincipal userClaims)
        {
            try
            {
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);

                var buyerItems = await _buyerItemRepository.GetBuyerItemsByStatusesAndUserId(new[] { "refunded", "refunding" }, userId);

                if (buyerItems == null || !buyerItems.Any())
                {
                    return new ResponseModel<List<T>>
                    {
                        Success = false,
                        Code = "450",
                        Message = $"No buyer items found with statuses 'refunded' or 'refunding'.",
                        Data = new List<T>()
                    };
                }
                var sortedBuyerItems = buyerItems.OrderBy(item => item.Status == "refunding" ? 0 : 1).ThenByDescending(item => item.CreatedAt);

                var buyerItemDetails = await Task.WhenAll(sortedBuyerItems.Select(async item =>
                {
                    var productUrl = item.Product.MediaBoolean
                        ? (await _mediaService.GetFirstMediaBySourceIdAndType(item.Product.Id, "PRODUCT"))?.MediaUrl
                        : null;

                    bool isRefundable = item.Product.RefundGuaranteedDuration > 0;
                    int remainingRefundDays = 0;

                    if (isRefundable && item.ValidRefundDate.HasValue)
                    {
                        var currentDate = DateTime.UtcNow.Date;
                        var validRefundDate = item.ValidRefundDate.Value.ToDateTime(TimeOnly.MinValue);
                        if (validRefundDate < currentDate)
                        {
                            isRefundable = false;
                        }
                        else
                        {
                            remainingRefundDays = (validRefundDate - currentDate).Days;
                        }
                    }

                    var deliveries = item.BuyerItemDeliveries.Select(delivery => new BuyerItemDeliveryDTO
                    {
                        Stage = delivery.StageTypes,
                        StageDescription = delivery.StageDescription,
                        StageDate = delivery.StageDate
                    }).ToList();

                    return new BuyerItemDetailsDTO
                    {
                        PurchaseOrderId = item.OrderId,
                        BuyerItemId = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.Product.Title,
                        ProductUrl = productUrl,
                        ProductOwner = item.Product.User.Name,
                        BuyerItemStatus = item.Status,
                        RefundableBoolean = isRefundable,
                        RemainingRefundDays = remainingRefundDays,
                        BuyerItemDelivery = deliveries
                    };
                }));

                return new ResponseModel<List<T>>
                {
                    Success = true,
                    Code = "200",
                    Message = "Buyer items fetched successfully.",
                    Data = buyerItemDetails.Cast<T>().ToList()
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching buyer items: {ex.Message}");
                return new ResponseModel<List<T>>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while fetching buyer items.",
                    Data = default
                };
            }
        }


        public async Task<ResponseModel<T>> CreateDeliveryStage<T>(int buyerItemId, string stageType)
        {
            try
            {
                var buyerItem = await _buyerItemRepository.GetBuyerItemById(buyerItemId);

                if (buyerItem == null)
                {
                    throw new CustomException<GeneralMessageDTO>(ExceptionEnum.GetException("BUYER_ITEM_NOT_FOUND"));
                }
                var lastStage = buyerItem.BuyerItemDeliveries.OrderByDescending(d => d.StageDate).FirstOrDefault();
                if (lastStage != null)
                {
                    if (!IsValidStageTransition(lastStage.StageTypes, stageType))
                    {
                        throw new CustomException<GeneralMessageDTO>(ExceptionEnum.GetException("INVALID_STAGE_TRANSITION"));
                    }
                }
                var newDeliveryStage = new BuyerItemDelivery
                {
                    BuyerItemId = buyerItem.Id,
                    StageTypes = stageType,
                    StageDescription = $"{buyerItem.Product.Title} {stageType} at {DateTime.UtcNow}.",
                    Status = "delivering",
                    StageDate = DateTime.UtcNow
                };

                await _buyerItemRepository.CreateBuyerItemDelivery(newDeliveryStage);

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = $"Buyer item delivery stage '{stageType}' created successfully.",
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
                Console.Error.WriteLine($"Error creating delivery stage: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while creating the delivery stage.",
                    Data = default
                };
            }
        }

        private bool IsValidStageTransition(string lastStage, string currentStage)
        {
            var validOrder = new List<string>
            {
                "on packaging",
                "arrived in sorting facility",
                "arrived in sorting delivery hub",
                "out of delivery",
                "item delivered"
            };

            var lastIndex = validOrder.IndexOf(lastStage);
            var currentIndex = validOrder.IndexOf(currentStage);

            return currentIndex == lastIndex + 1; // Ensure proper order
        }

        public async Task<ResponseModel<T>> CreateDeliveredStage<T>(int buyerItemId)
        {
            try
            {
                var response = await CreateDeliveryStage<GeneralMessageDTO>(buyerItemId, "item delivered");
                if (!response.Success)
                {
                    return new ResponseModel<T>

                    {
                        Success = response.Success,
                        Code = response.Code,
                        Message = response.Message,
                        Data = default
                    };
                }
                var buyerItem = await _buyerItemRepository.GetBuyerItemById(buyerItemId);

                buyerItem.Status = "received";
                if (buyerItem.Product.RefundGuaranteedDuration > 0)
                {
                    buyerItem.ValidRefundDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(buyerItem.Product.RefundGuaranteedDuration));
                }
                else
                {
                    buyerItem.ValidRefundDate = DateOnly.FromDateTime(DateTime.UtcNow); ;
                }

                buyerItem.UpdatedAt = DateTime.UtcNow;
                buyerItem.ArrivedDate = DateOnly.FromDateTime(DateTime.UtcNow);
                await _buyerItemRepository.UpdateBuyerItem(buyerItem);

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Buyer item marked as delivered and status updated to 'received'.",
                    Data = default
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating 'item delivered' stage: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while creating the 'item delivered' stage.",
                    Data = default
                };
            }
        }

        public async Task<ResponseModel<List<T>>> ViewAllBuyerItems<T>()
        {
            try
            {
                var buyerItems = await _buyerItemRepository.GetAllBuyerItems();

                if (!buyerItems.Any())
                {
                    return new ResponseModel<List<T>>
                    {
                        Success = false,
                        Code = "404",
                        Message = "No buyer items found.",
                        Data = new List<T>()
                    };
                }

                var buyerItemDetails = buyerItems
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(item => new BuyerItemDetailsDTO
                    {
                        PurchaseOrderId = item.OrderId,
                        BuyerItemId = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.Product.Title,
                        ProductUrl = item.Product.MediaBoolean
                            ? _mediaService.GetFirstMediaBySourceIdAndType(item.Product.Id, "PRODUCT")?.Result?.MediaUrl
                            : null,
                        ProductOwner = item.Product.User.Name,
                        BuyerItemStatus = item.Status,
                        RefundableBoolean = item.Product.RefundGuaranteedDuration > 0,
                        BuyerItemDelivery = item.BuyerItemDeliveries
                            .Select(delivery => new BuyerItemDeliveryDTO
                            {
                                Stage = delivery.StageTypes,
                                StageDescription = delivery.StageDescription,
                                StageDate = delivery.StageDate
                            }).ToList()
                    }).ToList();

                return new ResponseModel<List<T>>
                {
                    Success = true,
                    Code = "200",
                    Message = "Buyer items fetched successfully.",
                    Data = buyerItemDetails.Cast<T>().ToList()
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching buyer items: {ex.Message}");
                return new ResponseModel<List<T>>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while fetching buyer items.",
                    Data = default
                };
            }
        }

        public async Task<ResponseModel<T>> RequestRefund<T>(int buyerItemId)
        {
            try
            {
                var buyerItem = await _buyerItemRepository.GetBuyerItemById(buyerItemId);
                if (buyerItem == null)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("BUYER_ITEM_NOT_FOUND")
                    );
                }

                if (buyerItem.Status != "done payment" || buyerItem.Product.RefundGuaranteedDuration <= 0)
                {
                    throw new CustomException<GeneralMessageDTO>(
                        ExceptionEnum.GetException("ITEM_NOT_REFUNDABLE"),
                        new GeneralMessageDTO
                        {
                            Message = "Item is not eligible for a refund.",
                            Success = false
                        }
                    );
                }
                buyerItem.Status = "refunding";
                buyerItem.UpdatedAt = DateTime.UtcNow;
                await _buyerItemRepository.UpdateBuyerItem(buyerItem);

                buyerItem.Product.Status = "request refund";
                buyerItem.Product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateProduct(buyerItem.Product);

                return new ResponseModel<T>
                {
                    Success = true,
                    Code = "200",
                    Message = "Refund request successfully initiated.",
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
                Console.Error.WriteLine($"Error processing refund: {ex.Message}");
                return new ResponseModel<T>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while processing the refund request.",
                    Data = default
                };
            }
        }

        public async Task<ResponseModel<GeneralMessageDTO>> MakePayToUser()
        {
            try
            {
                var buyerItems = await _buyerItemRepository.GetBuyerItemsByStatus("received");

                if (buyerItems == null || !buyerItems.Any())
                {
                    return new ResponseModel<GeneralMessageDTO>
                    {
                        Success = true,
                        Code = "200",
                        Message = "No items to process for payment.",
                        Data = new GeneralMessageDTO { Message = "No items to process.", Success = true }
                    };
                }

                foreach (var item in buyerItems)
                {
                    if (item.ValidRefundDate.HasValue && item.ValidRefundDate.Value.ToDateTime(TimeOnly.MinValue) > DateTime.UtcNow)
                    {
                        continue; 
                    }

                    var sellerId = item.Product.UserId;
                    var buyerId = item.BuyerId;
                    var productId = item.ProductId;

                    var existingPayment = await _onHoldingPaymentHistoryRepository.GetPaymentHistoryByDetails(buyerId, sellerId, productId);
                    if (existingPayment != null)
                    {
                        continue; 
                    }

                    var sellerWallet = await _walletRepository.GetWalletByUserIdAsync(sellerId);
                    if (sellerWallet == null)
                    {
                        throw new CustomException<GeneralMessageDTO>(
                            ExceptionEnum.GetException("WALLET_NOT_FOUND"),
                            new GeneralMessageDTO { Message = $"Wallet for seller ID {sellerId} not found.", Success = false }
                        );
                    }

                    sellerWallet.OnHoldBalance -= item.Product.Price;
                    sellerWallet.AvailableBalance += item.Product.Price;
                    await _walletRepository.UpdateWalletAsync(sellerWallet);

                    var paymentHistory = new OnHoldingPaymentHistory
                    {
                        ProductId = productId,
                        BuyerItemId = item.Id,
                        SellerId = sellerId,
                        BuyerId = buyerId,
                        PaymentAmount = item.Product.Price,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _onHoldingPaymentHistoryRepository.CreatePaymentHistory(paymentHistory);

                   
                    var walletTransaction = new WalletTransaction
                    {
                        WalletId = sellerWallet.Id,
                        Amount = item.Product.Price,
                        TransactionType = "Release Payment",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _walletTransactionRepository.CreateTransaction(walletTransaction);
                }

                return new ResponseModel<GeneralMessageDTO>
                {
                    Success = true,
                    Code = "200",
                    Message = "Payments processed successfully.",
                    Data = new GeneralMessageDTO { Message = "Payments updated successfully.", Success = true }
                };
            }
            catch (CustomException<GeneralMessageDTO> customEx)
            {
                Console.Error.WriteLine($"CustomException: {customEx.Response.Message}");
                return new ResponseModel<GeneralMessageDTO>
                {
                    Success = customEx.Response.Success,
                    Code = customEx.Response.Code,
                    Message = customEx.Response.Message,
                    Data = customEx.Response.Data
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error processing payments: {ex.Message}");
                return new ResponseModel<GeneralMessageDTO>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while processing payments.",
                    Data = new GeneralMessageDTO { Message = "Error processing payments.", Success = false }
                };
            }
        }


    }
}
