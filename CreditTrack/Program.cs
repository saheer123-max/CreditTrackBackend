using CreditTrack.Application.Interfaces;
using CreditTrack.Application.Service;
using CreditTrack.Application.IRepo;
using CreditTrack.Infrastructure.RepoService;
using CreditTrack.Infrastructure.Services;
using Npgsql;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using CreditTrack.Middleware;
using CreditTrack.Infrastructure.Services;
using CreditTrack.Chat;
using YourProject.WebAPI.Hubs;
using YourProject.Application.Services;
using YourProject.Infrastructure.Repositories;
using MediatR;
using CreditTrack.Application.Commands.Products;
using CreditTrack.Application.Handlers.Products;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// -----------------------
// Add services
// -----------------------
builder.Services.AddControllers();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});



builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, CreditTransactionRepository>();


builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddScoped< ProductService>();
builder.Services.AddScoped<CreditTransactionService>(); 
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly)
);




builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddTransient<IDbConnection>(sp =>
{
    var connStr = sp.GetRequiredService<IConfiguration>().GetConnectionString("Default");
    return new NpgsqlConnection(connStr); // ✅ FIXED
});



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


var app = builder.Build();


 
    app.UseSwagger();
    app.UseSwaggerUI();
 

app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();


app.UseCors("AllowReactApp");
app.MapHub<ChatHub>("/chathub");
app.MapHub<SearchHub>("/searchhub");
app.MapHub<AnnouncementHub>("/announcementHub");
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var svc = scope.ServiceProvider.GetRequiredService<IAdminService>();
    await svc.EnsureSeedAdminAsync();
}

app.Run();
