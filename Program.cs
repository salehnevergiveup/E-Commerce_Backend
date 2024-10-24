using MySqlConnector;
using PototoTrade;
using Microsoft.EntityFrameworkCore;
using PototoTrade.Middleware.Filter;
using PototoTrade.Service.User;
using PototoTrade.ServiceBusiness.LiveChat;
using PototoTrade.Repository.User;
using PototoTrade.Repository.Users;
using PototoTrade.Models;



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
        builder
            .WithOrigins("http://127.0.0.1:5500") // Explicitly allow this origin
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // This is required if credentials are involved
    });
});

builder.Services.AddSingleton<SharedDb>(); //connection to db
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

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
app.UseAuthorization();
app.MapControllers();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<FilterMiddleware>();

app.MapHub<ChatHub>("/Chat"); //connection to chat hub

app.Run();
