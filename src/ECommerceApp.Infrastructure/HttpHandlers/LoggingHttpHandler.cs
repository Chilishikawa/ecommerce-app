using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceApp.Infrastructure.HttpHandlers
{
    public class LoggingHttpHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingHttpHandler> _logger;

        public LoggingHttpHandler(ILogger<LoggingHttpHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("🌐 Enviando solicitud HTTP → {Method} {Url}", request.Method, request.RequestUri);

            var response = await base.SendAsync(request, cancellationToken);
            sw.Stop();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("⚠ Respuesta HTTP con error ({StatusCode}) desde {Url} - Tiempo {Elapsed}ms",
                response.StatusCode, request.RequestUri, sw.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation("✅ Respuesta exitosa ({StatusCode}) desde {Url} - Tiempo {Elapsed}ms",
                response.StatusCode, request.RequestUri, sw.ElapsedMilliseconds);
            }

            return response;
        }
    }
}
