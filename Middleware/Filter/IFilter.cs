namespace PototoTrade.Middleware.Filter
{
    public interface IFilter
    {
        Task<bool> ExecuteAsync(HttpContext context);
    }
}
