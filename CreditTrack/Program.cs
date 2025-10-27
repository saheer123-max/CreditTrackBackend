using CreditTrack.Application.Interfaces;
using CreditTrack.Application.Service;
using CreditTrack.Domain.IRepo;
using CreditTrack.Infrastructure.RepoService;
using CreditTrack.Infrastructure.Services;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using CreditTrack.Middleware;
using CreditTrack.Infrastructure.Services;
using CreditTrack.Chat;

var builder = WebApplication.CreateBuilder(args);

// -----------------------
// Add services
// -----------------------
builder.Services.AddControllers();

// CORS: React frontend allow ചെയ്യാൻ
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});


// -----------------------
// Repositories
// -----------------------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, CreditTransactionRepository>();

// -----------------------
// Services
// -----------------------
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddScoped< ProductService>();
builder.Services.AddScoped<CreditTransactionService>(); // ✅ Register your service
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
// -----------------------
// Swagger
// -----------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
// -----------------------
// Dapper IDbConnection
// -----------------------
builder.Services.AddTransient<IDbConnection>(sp =>
{
    var connStr = sp.GetRequiredService<IConfiguration>().GetConnectionString("Default");
    return new SqlConnection(connStr);
});

// -----------------------
// JWT Authentication
// -----------------------
var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt.GetValue<string>("Key")!;
var issuer = jwt.GetValue<string>("Issuer");
var audience = jwt.GetValue<string>("Audience");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

// -----------------------
// Build app
// -----------------------
var app = builder.Build();

// -----------------------
// Configure pipeline
// -----------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();
// ✅ CORS must come BEFORE Authentication & Authorization

app.UseCors("AllowReactApp");
app.MapHub<ChatHub>("/chathub");
app.MapHub<SearchHub>("/searchhub");
app.UseAuthentication();   // 🔑 must come before UseAuthorization
app.UseAuthorization();

app.MapControllers();

// -----------------------
// Seed admin at startup
// -----------------------
using (var scope = app.Services.CreateScope())
{
    var svc = scope.ServiceProvider.GetRequiredService<IAdminService>();
    await svc.EnsureSeedAdminAsync();
}

app.Run();
