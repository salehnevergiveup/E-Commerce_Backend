using Microsoft.EntityFrameworkCore;
using PototoTrade.Middleware.Filter;
using PototoTrade.Service.User;
using PototoTrade.Repository.User;
using PototoTrade.Repository.Users;
using PototoTrade.Data;
using PototoTrade.Service.Utilites.Hash;
using PototoTrade.Data.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PototoTrade.ServiceBusiness.Authentication;
using PototoTrade.Repository.Role;
using PototoTrade.Repository.MediaRepo;
using PototoTrade.Service.Role;
using PototoTrade.Repository.CartItems;
using PototoTrade.Repository.Cart;
using PototoTrade.Repository.Product;
using PototoTrade.Service.CartItem;
using PototoTrade.Service.ShoppingCart;
using PototoTrade.Integrations;
using PototoTrade.ServiceBusiness.LLM;
using PototoTrade.Repositories;
using PototoTrade.Repository.Report;
using PototoTrade.Service.Report;



var builder = WebApplication.CreateBuilder(args);


// builder.WebHost.ConfigureKestrel(serverOptions =>
// {
//     serverOptions.ListenAnyIP(8080); // HTTP
//     serverOptions.ListenAnyIP(8081, listenOptions => listenOptions.UseHttps()); // HTTPS
// });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials(); // Allow cookies to be sent
    });
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

//Repos 
builder.Services.AddScoped<UserAccountRepository, UserAccountRepositoryImpl>();
builder.Services.AddScoped<SessionRepository, SessionRepositoryImp>();
builder.Services.AddScoped<RoleRepository, RoleRepositoryImp>();
builder.Services.AddScoped<MediaRepository, MediaRepositoryImp>();
builder.Services.AddScoped<UserDetailsRepository, UserDetailsRepositoryImp>();
builder.Services.AddScoped<ShoppingCartItemRepository, ShoppingCartItemRepositoryImp>();
builder.Services.AddScoped<ShoppingCartRepository, ShoppingCartRrepositoryImp>();
builder.Services.AddScoped<ProductRepository, ProductRepositoryImp>();
builder.Services.AddScoped<ReportRepository,ReportRepositoryImp>(); 
builder.Services.AddScoped<IHashing, Hashing>();
builder.Services.AddTransient<SeederFacade>();

//services & Business Service
builder.Services.AddScoped<Authentication>();
builder.Services.AddScoped<UserAccountService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<ShoppingCartItemsService>();
builder.Services.AddScoped<ShoppingCartService>();
builder.Services.AddScoped<LlamaIntegration>();
builder.Services.AddScoped<LLMService>();  
builder.Services.AddScoped<ReportsService>(); 
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor(); // for the websocket


//MiddleWare Filters
builder.Services.AddScoped<IFilter, JwtFilter>();

builder.Services.AddDbContext<DBC>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        new MySqlServerVersion(new Version(9, 0, 1)) // Adjust based on the MySQL version you're using
    )
);

var configuration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .Build();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidAudience = configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"])),
            ClockSkew = TimeSpan.Zero // no addational default time 5min 
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for one of the hubs
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/cartHub")  /*used for the shoppig cart realtime update */ 
                    || path.StartsWithSegments("/chatHub") /*for the chathub used for the live chatting feature*/
                    || path.StartsWithSegments("/notificationHub")) /*for the notification feature */
                    )
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });



var app = builder.Build();

//swagger configruation for the dev env
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<FilterMiddleware>();
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
    Secure = CookieSecurePolicy.Always
});
// Map SignalR hubs
app.MapHub<CartHub>("/cartHub");
app.MapHub<ChatHub>("/chatHub");// this  will be add when you create the live chathub 
app.MapHub<NotificationHub>("/notificationHub"); //this is will be added when you create the notification  hub 

//seeder for init data
if (args.Length == 1 && args[0].ToLower() == "init")
    SeedData(app);

void SeedData(IHost app)
{
    var scopedFactory = app.Services.GetService<IServiceScopeFactory>();

    using (var scope = scopedFactory.CreateScope())
    {
        var service = scope.ServiceProvider.GetService<SeederFacade>();
        service.SeedInitialData();
    }
}
app.Run();
