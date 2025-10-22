using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace ECommerceApp.Api.Filters
{
    public class ExceptionToProblemDetailsFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionToProblemDetailsFilter> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionToProblemDetailsFilter(ILogger<ExceptionToProblemDetailsFilter> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Excepción no manejada en {Action}", context.ActionDescriptor.DisplayName);

            var detail = _env.IsDevelopment() ? context.Exception.ToString() : "Se produjo un error interno.";
            var pd = new ProblemDetails
            {
                Title = "Error interno del servidor",
                Detail = detail,
                Status = StatusCodes.Status500InternalServerError
            };

            context.Result = new ObjectResult(pd) { StatusCode = pd.Status };
            context.ExceptionHandled = true; // marcamos como manejada para que no se propague más
        }
    }
}
