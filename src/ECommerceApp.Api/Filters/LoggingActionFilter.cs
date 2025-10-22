using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace ECommerceApp.Api.Filters
{
    public class LoggingActionFilter : IAsyncActionFilter
    {
        private readonly ILogger<LoggingActionFilter> _logger;

        public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("Inicio acción {Action} - Args: {@Args}", context.ActionDescriptor.DisplayName, context.ActionArguments);

            // Ejecuta la acción
            var executed = await next();

            sw.Stop();
            _logger.LogInformation("Fin acción {Action} - Estado: {StatusCode} - Tiempo: {Elapsed}ms",
                context.ActionDescriptor.DisplayName,
                context.HttpContext.Response?.StatusCode,
                sw.ElapsedMilliseconds);
        }
    }
}
