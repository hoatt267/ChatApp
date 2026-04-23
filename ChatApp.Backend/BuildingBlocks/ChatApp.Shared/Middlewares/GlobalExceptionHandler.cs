using System.ComponentModel.DataAnnotations;
using ChatApp.Shared.Wrappers;
using ChatApp.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChatApp.Shared.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (
                exception is not CustomValidationException &&
                exception is not BadRequestException &&
                exception is not NotFoundException &&
                exception is not ForbiddenException
                )
            {
                var rootCause = exception.GetBaseException().Message;

                _logger.LogError(exception, "🚨 Lỗi hệ thống nghiêm trọng: {Message} | Lỗi gốc: {RootCause}", exception.Message, rootCause);
            }
            else
            {
                // 🌟 NẾU LÀ LỖI NGHIỆP VỤ (400, 403, 404) -> GHI LẠI MỨC ĐỘ WARNING (Màu Vàng)
                _logger.LogWarning(exception, "⚠️ Lỗi nghiệp vụ: {Message}", exception.Message);
            }

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

                case ForbiddenException forbiddenEx:
                    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    response.Message = forbiddenEx.Message;
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