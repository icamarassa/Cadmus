using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Cadmus.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireCollectorApiKeyAttribute : Attribute, IAsyncActionFilter
{
    public const string HeaderName = "X-Cadmus-Collector-Key";

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var configuration = context.HttpContext.RequestServices
            .GetRequiredService<IConfiguration>();

        var expectedApiKey = configuration[
            "CollectorAuthentication:ApiKey"];

        if (string.IsNullOrWhiteSpace(expectedApiKey))
        {
            context.Result = new StatusCodeResult(
                StatusCodes.Status500InternalServerError);

            return;
        }

        var receivedApiKey = context.HttpContext.Request.Headers[
            HeaderName].ToString();

        if (!HasValidApiKey(receivedApiKey, expectedApiKey))
        {
            context.Result = new UnauthorizedResult();

            return;
        }

        await next();
    }

    private static bool HasValidApiKey(
        string receivedApiKey,
        string expectedApiKey)
    {
        var receivedBytes = Encoding.UTF8.GetBytes(receivedApiKey);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedApiKey);

        return CryptographicOperations.FixedTimeEquals(
            receivedBytes,
            expectedBytes);
    }
}