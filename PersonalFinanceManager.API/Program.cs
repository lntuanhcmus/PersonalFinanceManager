using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.API.Model;
using PersonalFinanceManager.API.Services;
using PersonalFinanceManager.Shared.Services;
using System.Globalization;
using PersonalFinanceManager.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GmailServiceSettings>(
    builder.Configuration.GetSection("GmailService"));

// Set culture to vi-VN
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("vi-VN");
// Thêm dịch vụ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy.WithOrigins("https://localhost:7204") // Origin của UI
              .AllowAnyMethod() // GET, POST, PUT, DELETE, etc.
              .AllowAnyHeader(); // Content-Type, etc.
    });
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IGmailService, GmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
//builder.Services.AddScoped<ExcelService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<BudgetService>();
builder.Services.AddScoped<RepaymentTransactionService>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.MigrationsAssembly("PersonalFinanceManager.Shared"));
});

var app = builder.Build();

// Sử dụng CORS trước các middleware khác
app.UseCors("AllowUI");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
// Cấu hình chạy trên port 8000 HTTP
app.Urls.Add("http://localhost:8000");


app.Run();

