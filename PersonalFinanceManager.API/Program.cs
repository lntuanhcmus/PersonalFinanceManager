using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.API.Data;
using PersonalFinanceManager.API.Services;
using PersonalFinanceManager.Shared.Services;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<GmailService>();
//builder.Services.AddScoped<ExcelService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<BudgetService>();


// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

//using (var scope = app.Services.CreateScope())
//{
//    var excelService = scope.ServiceProvider.GetRequiredService<ExcelService>();
//    var transactionService = scope.ServiceProvider.GetRequiredService<TransactionService>();
//    var migrator = new ExcelToSqlMigrator(excelService, transactionService);
//    await migrator.Migrate();
//}

app.Run();

