using Microsoft.AspNetCore.Identity;
using PersonalFinanceManager.API.Extensions;
using PersonalFinanceManager.API.Model;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GmailServiceSettings>(
    builder.Configuration.GetSection("GmailService"));

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// Cấu hình chạy trên port 8000 HTTP
app.Urls.Add("http://localhost:8000");


app.Run();

