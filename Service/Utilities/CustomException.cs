using PototoTrade.Service.Utilities.Response;
using PototoTrade.Service.Utilities.Exceptions;

namespace PototoTrade.Service.Utilities.Exceptions
{
    public class CustomException<T> : Exception
    {
        public ResponseModel<T> Response { get; }

        public CustomException(ExceptionDetail exceptionDetail, T? data = default)
        {
            Response = new ResponseModel<T>
            {
                Success = exceptionDetail.Success,
                Code = exceptionDetail.Code,
                Message = exceptionDetail.Message,
                Data = data
            };
        }
    }
}