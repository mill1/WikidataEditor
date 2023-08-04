using Newtonsoft.Json;
using System.Net;

namespace WikidataEditor.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, RequestDelegate next)
        {
            this.next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        private Task HandleException(HttpContext context, Exception e)
        {
            var id = Guid.NewGuid().ToString();
            _logger.LogError(e, $"Exception thrown by API controller method {context.Request.Path}. Error id: {id}", context);

            var message = e.Message;
            var statusCode = ((HttpRequestException?)e?.InnerException)?.StatusCode;

            if (statusCode == null)
                statusCode = HttpStatusCode.InternalServerError;

            if (statusCode == HttpStatusCode.InternalServerError)
                message = "An unexpected error has occurred";

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            string result = JsonConvert.SerializeObject(new WikidataEditorErrorResponse((int)statusCode, message, id));

            return context.Response.WriteAsync(result);
        }

        public class WikidataEditorErrorResponse
        {
            public WikidataEditorErrorResponse(int statusCode, string message, string id)
            {
                Id = id;
                dateTime = DateTime.Now;
                StatusCode = statusCode;
                Message = message;
            }

            public string Id { get; }
            public DateTime dateTime { get; }
            public int StatusCode { get; }
            public string Message { get; }
        }
    }
}
