using System.Diagnostics;

namespace PototoTrade.Middleware.Filter
{
    public class JwtFilter : IFilter
    {
        public Task<bool> ExecuteAsync(HttpContext context)
        {
            // Your authentication logic
            bool isAuthenticated = true;

            if (isAuthenticated)
            {

                context.Items["User"] = "aaaa";
            }


            return Task.FromResult(isAuthenticated);
        }
    }
}
