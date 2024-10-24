using System.Diagnostics;

namespace PototoTrade.Middleware.Filter
{
    public class JwtFilter : IFilter
    {
        public Task<bool> ExecuteAsync(HttpContext context)
        {
            bool isAuthenticated = true;

            if (!context.User.Identity.IsAuthenticated)
            {
                isAuthenticated = false; 

            }

            return Task.FromResult(isAuthenticated);
        }
    }
}
