using ECommerceApp.Api.Filters;
using ECommerceApp.Api.Services;
using ECommerceApp.Core.Interfaces;
using ECommerceApp.Infrastructure.Data;
using ECommerceApp.Infrastructure.HttpHandlers;
using ECommerceApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Experimental;
using System.Text;

// crea el builder de la aplicación minimal-host (configuración, logging, DI).
// args vienen del Main.
var builder = WebApplication.CreateBuilder(args);

// atajo a IServiceCollection donde registras dependencias (inyección de dependencias).
var services = builder.Services;

// atajo a la configuración (appsettings.json, variables de entorno, etc.).
var config = builder.Configuration;

// obtiene la sección JwtSettings de la configuración (ej.: Issuer, Audience, SecretKey).
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

// toma el SecretKey como texto y lo transforma a byte[] para crear la clave simétrica
// que valida la firma del token. (Nota: conviene usar Encoding.UTF8 y una SecretKey
// larga/aleatoria.)
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

// registra los controladores MVC (model binding, filtros, etc.). Habilita endpoints de
// controllers ([ApiController]).
services.AddControllers()
    .AddNewtonsoftJson();

// agrega/usa Newtonsoft.Json para serialización JSON en lugar del System.Text.Json por
// defecto. Útil si dependes de features/atributos de Newtonsoft.
services.AddEndpointsApiExplorer();

// registra el generador de Swagger/OpenAPI para crear documentación y UI (Swagger UI).
services.AddSwaggerGen();

// registra tu AppDbContext en DI. DbContext por defecto es scoped (una instancia por
// petición).
services.AddDbContext<AppDbContext>(options =>
//configura EF Core para usar SQL Server con la cadena de conexión DefaultConnection
//tomada de la configuración.
    options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

// registra una implementación de IDistributedCache usando StackExchange.Redis (cache
// distribuida).
services.AddStackExchangeRedisCache(options =>
{
    // establece la conexión a Redis (ej. hostname:port,password=...) sacada de la
    // configuración.
    options.Configuration = config.GetSection("Redis:Configuration").Value;
    // prefijo que se añadirá a las claves almacenadas en Redis (útil para separar
    // datos entre aplicaciones/instancias).
    options.InstanceName = "ECommerce_";
});

// registra CacheService (tu clase) con lifetime scoped (una instancia por petición).
// Probablemente envuelve IDistributedCache con lógica de conveniencia.
services.AddScoped<CacheService>();

// añade servicios de autenticación y configura esquemas por defecto.
services.AddAuthentication(options =>
{
    // establece que el esquema por defecto para autenticar es Bearer (JWT).
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    // establece que los challenges (cuando se requiere auth) usan el esquema Bearer.
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
// registra el handler JWT-Bearer y sus opciones.
.AddJwtBearer(options =>
{
    // define cómo se validan los tokens entrantes.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // exige que el iss del token coincida con ValidIssuer.
        ValidateAudience = true, // exige que el aud del token coincida con ValidAudience.
        ValidateIssuerSigningKey = true, // exige validar la firma con la clave proporcionada.
        ValidIssuer = jwtSettings["Issuer"], // valor esperado del iss (tomado de JwtSettings).
        ValidAudience = jwtSettings["Audience"], // valor esperado del aud.
        IssuerSigningKey = new SymmetricSecurityKey(key) // clave simétrica creada a partir del byte[] key para validar la firma HMAC del token.
        //(Observación: no estás explicitando ValidateLifetime = true ni ClockSkew; por defecto ValidateLifetime está en true — sería buena práctica ponerlo explícito y ajustar ClockSkew si hace falta.)
    };
});

// registra tu servicio que probablemente emite/gestiona tokens JWT (una instancia
// por petición).
services.AddScoped<JwtService>();

// Registrar filtros como servicios
services.AddScoped<ValidateModelAttribute>();
services.AddScoped<LoggingActionFilter>();
services.AddScoped<ExceptionToProblemDetailsFilter>();

services.AddTransient<LoggingHttpHandler>();

// Registramos un HttpClient con el handler
services.AddHttpClient("PaymentService", client =>
{
    client.BaseAddress = new Uri("https://api.fakepayment.com");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<LoggingHttpHandler>();

// Registrar controladores y añadir filtros globales (opcional)
services.AddControllers(options =>
{
    // Añadir filtro global de excepciones y validación de modelo
    options.Filters.Add<ExceptionToProblemDetailsFilter>(); // captura excepciones en MVC y las transforma
    options.Filters.Add<ValidateModelAttribute>(); // valida ModelState automáticamente antes de actions
    // Si quieres que el logging sea global:
    options.Filters.Add<LoggingActionFilter>();
});

services.AddScoped<PaymentService>();

services.AddHttpClient<IPaymentService, PaymentService>();

// construye la instancia de WebApplication: crea el IServiceProvider final y prepara
// el pipeline/middleware. A partir de aquí se usan app.Use... y app.Map....
var app = builder.Build();

// comprueba si la app corre en entorno Development.
if (app.Environment.IsDevelopment())
{
    // habilita el middleware que expone el JSON OpenAPI (habitualmente /swagger/v1/swagger.json).
    app.UseSwagger();
    // habilita la interfaz web de Swagger (UI) que permite interactuar con la API desde
    // el navegador. Generalmente se activa solo en desarrollo.
    app.UseSwaggerUI();
}

// middleware que redirige peticiones HTTP (puerto 80) a HTTPS (443). Ponte esto temprano
// en el pipeline para forzar tráfico seguro.
app.UseHttpsRedirection();

// Primero autenticación
// inserta el middleware de autenticación. Antes de que el request llegue a los endpoints,
// este middleware intenta autenticar el usuario (ej.: leer Authorization: Bearer ...,
// validar token) y establecer HttpContext.User.
// Importante: debe ir antes de UseAuthorization().
app.UseAuthentication();

// Luego autorización
// middleware que aplica políticas de autorización (atributos [Authorize], políticas,
// roles). Depende de que HttpContext.User ya esté poblado por UseAuthentication().
app.UseAuthorization();

// conecta las rutas basadas en atributos de los controladores ([Route], [ApiController])
// con el pipeline; define los endpoints finales que manejarán las peticiones.
app.MapControllers();

// arranca la aplicación web (bloqueante) y comienza a escuchar solicitudes (inicia
// Kestrel u otro servidor configurado).
app.Run();