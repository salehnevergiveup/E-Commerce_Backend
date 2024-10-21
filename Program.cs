using MySqlConnector;
using PototoTrade;
using Microsoft.EntityFrameworkCore;
using PototoTrade.Middleware.Filter;
using PototoTrade.Service.User;
using PototoTrade.Repository.User;
using PototoTrade.Repository.Users;
using PototoTrade.Models;



var builder = WebApplication.CreateBuilder(args);


builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8080); // HTTP
    serverOptions.ListenAnyIP(8081, listenOptions => listenOptions.UseHttps()); // HTTPS
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()  // Allow all origins (any domain can make a request)
               .AllowAnyMethod()  // Allow all HTTP methods (GET, POST, PUT, DELETE, etc.)
               .AllowAnyHeader(); // Allow all headers
    });
});

builder.Services.AddControllers();

//User
builder.Services.AddScoped<IUserAccountService, UserAccountServiceImpl>();
builder.Services.AddScoped<UserAccountRepository, UserAccountRepositoryImpl>();

//MiddleWare
builder.Services.AddScoped<IFilter, JwtFilter>();
builder.Services.AddDbContext<DBC>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        new MySqlServerVersion(new Version(9, 0, 1)) // Adjust based on the MySQL version you're using
    )
);
var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseMiddleware<FilterMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();
