using ReportApi.DataProviders;
using ReportApi.DataProviders.Interfaces;
using ReportApi.Services;
using ReportApi.Services.Interfaces;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using ReportApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddHttpClient<ILoanDataProvider, LoanDataProvider>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:LoanApiBaseUrl"]!);
    client.DefaultRequestHeaders.Add("X-Api-Key",
        builder.Configuration["ExternalApis:LoanApiKey"]!);
});

builder.Services.AddHttpClient<IItemDataProvider, ItemDataProvider>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:KatalogApiBaseUrl"]!);
    client.DefaultRequestHeaders.Add("X-Api-Key",
        builder.Configuration["ExternalApis:KatalogApiKey"]!);
});

builder.Services.AddHttpClient<IUserDataProvider, UserDataProvider>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:UserApiBaseUrl"]!);
});

builder.Services.AddDbContext<ReportDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
    dbContext.Database.Migrate();
}

app.Run();


