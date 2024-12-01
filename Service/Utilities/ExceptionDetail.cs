namespace PototoTrade.Service.Utilities.Exceptions
{
    public static class ExceptionEnum
    {
        private static readonly Dictionary<string, ExceptionDetail> _exceptionDetails = new Dictionary<string, ExceptionDetail>
        {
            { "PRODUCT_CATEGORY_EXISTED", new ExceptionDetail { Success = false, Message = "Product Category Name Already Existed", Code = "1001" }},
            { "PRODUCT_CATEGORY_NOT_FOUND", new ExceptionDetail { Success = false, Message = "Product Category Not Found", Code = "1002" }},
            { "PRODUCT_NOT_FOUND", new ExceptionDetail { Success = false, Message = "Product Category Not Found", Code = "1003" }},
            { "USER_NOT_FOUND", new ExceptionDetail { Success = false, Message = "User not found.", Code = "001" } },
            { "WALLET_NOT_FOUND", new ExceptionDetail { Success = false, Message = "Wallet not found.", Code = "201" } },
            { "INSUFFICIENT_BALANCE", new ExceptionDetail { Success = false, Message = "Insufficient balance.", Code = "202" } },
            { "PLATFORM_WALLET_NOT_FOUND", new ExceptionDetail { Success = false, Message = "Platform wallet not found.", Code = "203" } },
            { "INVALID_MEDIA_INPUT", new ExceptionDetail { Success = false, Message = "Media Input Error", Code = "301" } },
            { "MEDIA_NOT_FOUND", new ExceptionDetail { Success = false, Message = "Media not found.", Code = "302" } },
            { "CART_NOT_FOUND", new ExceptionDetail { Success = false, Message = "Shopping cart not found.", Code = "601" } },
            { "NO_AVAILABLE_ITEMS", new ExceptionDetail { Success = false, Message = "No available items in the shopping cart.", Code = "602" } },
            { "PURCHASE_ORDER_NOT_FOUND", new ExceptionDetail { Success = false, Message = "Purchase order not found.", Code = "603" } },
            { "BUYER_ITEM_NOT_FOUND", new ExceptionDetail { Success = false, Message = "Buyer item not found.", Code = "604" } },
            { "INVALID_STAGE_TRANSITION", new ExceptionDetail { Success = false, Message = "Invalid stage transition for buyer item.", Code = "605" } },
            { "ITEM_NOT_REFUNDABLE", new ExceptionDetail { Success = false, Message = "Item is not eligible for a refund.", Code = "606" } }

        };

        public static ExceptionDetail GetException(string key)
        {
            return _exceptionDetails.ContainsKey(key) ? _exceptionDetails[key] : null!;
        }
    }

    public class ExceptionDetail
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }
    }
}