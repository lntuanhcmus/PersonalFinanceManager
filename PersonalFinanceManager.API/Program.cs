using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PersonalFinanceManager.API.Extensions;
using PersonalFinanceManager.API.Model;
using Serilog;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console() // Ghi log ra console cho docker logs
    .WriteTo.File(
        path: "logs/PersonalFinanceManager/app-.log", // File log với tên theo ngày
        rollingInterval: RollingInterval.Day, // Tạo file mới mỗi ngày
        rollOnFileSizeLimit: true, // Tạo file mới nếu vượt kích thước
        fileSizeLimitBytes: 10_000_000, // Giới hạn 10MB mỗi file
        retainedFileCountLimit: 3, // Giữ 7 file log (7 ngày)
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}") // Định dạng log
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.Configure<GmailServiceSettings>(
    builder.Configuration.GetSection("GmailService"));

// Sử dụng Serilog
builder.Host.UseSerilog();

// Set culture to vi-VN 
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("vi-VN");

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services
    .AddAppServices(builder.Configuration)
    .AddCorsPolicy()
    .AddJwtAuthentication(builder.Configuration);



var app = builder.Build();

// Sử dụng CORS trước các middleware khác
app.UseCors("AllowUI");

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

app.Run();

