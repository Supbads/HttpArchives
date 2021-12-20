using HttpArchivesService.Features.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HttpArchivesService.Features.Shared.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UserFriendlyException ex)
            {
                await context.Response.WriteAsJsonAsync(new ErrorDetails 
                { 
                    Message = ex.Message,
                    StatusCode = ex.StatusCode
                }.ToString());
            }
        }
    }
}
