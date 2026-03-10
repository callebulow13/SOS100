using Microsoft.EntityFrameworkCore;
using ReportApi.Data;
using ReportApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ReportDbContext>(options =>
    options.UseSqlite("Data Source=report.db"));

builder.Services.AddHttpClient<IReportDataProvider, MixedReportDataProvider>(client =>
{
    client.BaseAddress = new Uri("https://localhost:5001/");
});

builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
    context.Database.EnsureCreated();
    SeedData.Initialize(context);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();