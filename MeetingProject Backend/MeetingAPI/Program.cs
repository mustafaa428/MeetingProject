using DataBusiness.Interface;
using DataBusiness.Repository;
using Entities.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Services.Hubs;
using Services.Interface;
using Services.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");

// Veritabaný baðlantýsýný yapýyoruz
builder.Services.AddDbContext<ProjectConnect>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository ve Service sýnýflarýný DI container'a ekliyoruz
builder.Services.AddScoped<IRepository<User>, Repository<User>>();
builder.Services.AddScoped<IRepository<Meeting>, Repository<Meeting>>();
builder.Services.AddScoped<IRepository<MeetingParticipant>, Repository<MeetingParticipant>>(); // Add IRepository<MeetingParticipant> ve Repository<MeetingParticipant>
builder.Services.AddScoped<IMeetingService, MeetingService>(); // MeetingService ekle
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
// CORS yapýlandýrmasýný ekliyoruz
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .WithOrigins("http://127.0.0.1:5500") // Burada sadece frontend'inizin URL'sini ekleyin
            .AllowCredentials() // Kimlik doðrulama bilgilerine izin ver
            .AllowAnyHeader()
            .AllowAnyMethod());
});


// Authentication ve JWT'yi ekle
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
    };
});

// Þifreleme iþlemi için PasswordHasher servisini ekliyoruz
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddSignalR();


// Swagger yapýlandýrmasýný ekliyoruz
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();
app.UseAuthentication(); // JWT doðrulama eklendi

// CORS politikasýný uyguluyoruz
app.UseCors("AllowSpecificOrigin");

// SignalR Hub'ý ekle
app.MapHub<ChatHub>("/chatHub");


// Swagger UI'i kullanýma açýyoruz
app.UseSwagger();
app.UseSwaggerUI();

// HTTPS yönlendirmesini aktif ediyoruz
app.UseHttpsRedirection();

// Controller'larý eþliyoruz
app.MapControllers();

app.Run();
