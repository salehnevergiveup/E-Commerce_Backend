using System;

namespace PototoTrade.Service.Utilities.Response;

public class ResponseModel<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public string Code { get; set;}
    public T? Data { get; set; }
}
