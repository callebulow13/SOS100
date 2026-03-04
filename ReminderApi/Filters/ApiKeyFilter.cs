using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ReminderApi.Filters;

public class ApiKeyFilter : IActionFilter
{
    private readonly IConfiguration _config;

    public ApiKeyFilter(IConfiguration config)
    {
        _config = config;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var hasHeader = context.HttpContext.Request.Headers
            .TryGetValue("X-Api-Key", out var receivedKey);

        if (!hasHeader)
        {
            context.Result = new UnauthorizedObjectResult(
                new { error = "API-nyckel saknas." });
            return;
        }

        if (receivedKey != _config["ApiKey"])
        {
            context.Result = new UnauthorizedObjectResult(
                new { error = "Ogiltig API-nyckel." });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}