namespace PototoTrade.Middleware.Filter
{
    public class FilterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        //        Simpler Example:
        //Imagine you run a store:

        //The store building is like middleware: it stays the same, open for everyone.
        //        Customers are like HTTP requests: new people(requests) come in every time.
        //        Cashiers are like scoped services: each customer gets their own cashier (service instance) to help them.
        //If you tried to use the same cashier (service) for everyone (like singleton middleware), they would have trouble keeping track of different orders(requests). So, you need to hire a new cashier(use IServiceScopeFactory) for each customer.

        //Why IServiceScopeFactory Fixes the Problem:
        //Without IServiceScopeFactory, the middleware would try to use the same instance of the scoped service for every request, which could cause data conflicts or errors.
        //With IServiceScopeFactory, the middleware creates a new "scope" for every request, so that you get a fresh instance of the scoped service for each HTTP request.
        //By using IServiceScopeFactory, you're making sure the scoped services are created properly for each request, even though the middleware itself is singleton. That's why it works.
        public FilterMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            bool isAllowed = false;
            if (context.Request.Path.StartsWithSegments("/public/"))
            {
                await _next(context); // Bypass the filter chain for "/public" APIs
                return;
            }


            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var filters = scope.ServiceProvider.GetServices<IFilter>();

                foreach (var filter in filters)
                {
                    // Execute the filter logic
                    bool result = await filter.ExecuteAsync(context);

                    // Track if any filter passes
                    if (result)
                    {
                        isAllowed = true;
                    }
                }
            }

            if (isAllowed)
            {
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Request blocked.");
            }
        }
    }
}
