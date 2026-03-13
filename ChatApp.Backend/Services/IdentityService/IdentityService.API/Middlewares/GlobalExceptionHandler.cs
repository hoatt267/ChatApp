using System.ComponentModel.DataAnnotations;
using IdentityService.Application.Wrappers;
using IdentityService.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace IdentityService.API.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var response = new ApiResponse<object> { Success = false };

            // Phân loại lỗi
            switch (exception)
            {
                case CustomValidationException validationEx:
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = "Bad request. Please check the errors and try again.";
                    // Biến Dictionary lỗi thành List<string> để trả về frontend
                    response.Errors = validationEx.Errors.SelectMany(kvp => kvp.Value.Select(v => $"{kvp.Key}: {v}")).ToList();
                    break;

                case NotFoundException notFoundEx:
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = notFoundEx.Message;
                    break;

                case BadRequestException badRequestEx:
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = badRequestEx.Message;
                    break;

                default: // Các lỗi crash code, lỗi DB... (Lỗi 500)
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.Message = "An unexpected error occurred. Please try again later.";
                    response.Errors = new List<string> { exception.Message }; // Ở Production nên ẩn dòng này đi
                    break;
            }

            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }
    }
}