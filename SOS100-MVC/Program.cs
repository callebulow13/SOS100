using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace SOS100_MVC;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            
            options.Filters.Add(new AuthorizeFilter(policy));
        });

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Index";
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });
        
        builder.Services.AddHttpClient();
        
        builder.Services.AddHttpClient("LoanApi", client =>
        {
            client.BaseAddress = new Uri(
                builder.Configuration["LoanApiBaseUrl"]!);

            client.DefaultRequestHeaders.Add("X-Api-Key",
                builder.Configuration["LoanApiKey"]!);
        });

        builder.Services.AddHttpClient("UserApi", client =>
        {
            client.BaseAddress = new Uri(
                builder.Configuration["UserServiceBaseUrl"]!);
        });
        
        builder.Services.AddHttpClient<SOS100_MVC.Services.ReminderServiceClient>(client =>
        {
            client.BaseAddress = new Uri(
                builder.Configuration["ReminderServiceBaseUrl"]!);
            client.DefaultRequestHeaders.Add("X-Api-Key",
                builder.Configuration["ReminderApiKey"]!);
        });
        
        builder.Services.AddHttpClient<SOS100_MVC.Services.ReportApiService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["ReportApiBaseUrl"]!);
        });
        
        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto
        });
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}