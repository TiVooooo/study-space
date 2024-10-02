using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudySpace.Service;
using StudySpace.Service.Services;
using System.Text;
using static StudySpace.Service.Configuration.ConfigurationModel;
using Microsoft.OpenApi.Models;
using StudySpace.Data.Helper;
using System.Configuration;
using StudySpace.Service.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Enable support for file uploads (multipart/form-data)

    // Configure swagger to use form parameters correctly for file upload
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});


// JWT authentication configuration
var key = Encoding.ASCII.GetBytes("s3cr3tKeyF0rJWT@2024!MustBe32Char$");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Register services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IFirebaseService, FirebaseService>();
builder.Services.AddScoped<ISpaceService, SpaceService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<IAmityService, AmityService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddScoped<FirebasePathName>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Firebase configuration
var firebaseConfigSection = builder.Configuration.GetSection("Firebase");
var firebaseConfig = firebaseConfigSection.Get<FirebaseConfiguration>();
builder.Services.Configure<FirebaseConfiguration>(firebaseConfigSection);
builder.Services.AddSingleton(firebaseConfig);


// Email service configuration
var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddTransient<EmailService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAnyOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
