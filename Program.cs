using MySqlConnector;
using PototoTrade;
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

//User
builder.Services.AddScoped<IUserAccountService, UserAccountServiceImpl>();
builder.Services.AddScoped<UserAccountRepository, UserAccountRepositoryImpl>();
builder.Services.AddScoped<SessionRepository, SessionRepositoryImp>(); // Session repo

builder.Services.AddScoped<IHashing, Hashing>();
builder.Services.AddTransient<SeederFacade>();

//services  
builder.Services.AddScoped<Authentication>();

//MiddleWare
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
