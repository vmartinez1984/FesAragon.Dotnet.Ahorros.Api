namespace DuckBank.Ahorros.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionMiddleware> logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger
        )
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new { Mensaje = "El minge ya esta reiniciado el servidor", Guid = Guid.NewGuid() });
                // DO SOMETHING
            }
        }
    }
}