using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.Net.Http.Headers;
namespace MedicalData.API.Results
{
    public class FileCallbackResult : FileResult
    {
        private readonly Func<Stream, CancellationToken, Task> _callback;

        public FileCallbackResult(string contentType, Func<Stream, CancellationToken, Task> callback) : base(contentType)
        {
            _callback = callback;
        }
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.ContentType = ContentType;

            if(!string.IsNullOrEmpty(FileDownloadName))
            {
                var cd = new ContentDispositionHeaderValue("attatchment")
                {
                    FileNameStar = FileDownloadName,
                };
                response.Headers[HeaderNames.ContentDisposition] = cd.ToString();

            }

            await _callback(response.Body, context.HttpContext.RequestAborted);
        }
    }
}
