var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.Use(async (context, next) =>
{
    // Litet bonustips: Använd app.Configuration istället för builder.Configuration här!
    var configuredApiKey = app.Configuration.GetValue<string>("KatalogApiKey");
    var extractedApiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();

    if (extractedApiKey != configuredApiKey)
    {
        context.Response.StatusCode = 401; 
        await context.Response.WriteAsync("Ogiltig eller saknad API-nyckel.");
        return; 
    }
    
    // Fixen är här nere:
    await next(context); 
});

app.MapControllers();

app.Run();