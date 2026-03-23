using ReportApi.DataProviders;
using ReportApi.DataProviders.Interfaces;
using ReportApi.Services;
using ReportApi.Services.Interfaces;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddHttpClient<ILoanDataProvider, LoanDataProvider>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:LoanApiBaseUrl"]!);
});

builder.Services.AddHttpClient<IItemDataProvider, ItemDataProvider>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:KatalogApiBaseUrl"]!);
});

builder.Services.AddHttpClient<IUserDataProvider, UserDataProvider>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:UserApiBaseUrl"]!);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();


