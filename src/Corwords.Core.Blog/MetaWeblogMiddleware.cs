﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Corwords.Core.Blog
{
    public class MetaWeblogMiddleware<TBlog, TBlogPost, TPostTag>
        where TBlog : class, IBlog<TBlog, TBlogPost, TPostTag>
        where TBlogPost : class, IBlogPost<TBlog, TBlogPost, TPostTag>
        where TPostTag : class, IPostTag<TBlog, TBlogPost, TPostTag>
    {
        private ILogger _logger;
        private readonly RequestDelegate _next;
        private MetaWeblogService<TBlog, TBlogPost, TPostTag> _service;
        private string _urlEndpoint;

        public MetaWeblogMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, string urlEndpoint, MetaWeblogService<TBlog, TBlogPost, TPostTag> service)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<MetaWeblogMiddleware<TBlog, TBlogPost, TPostTag>>(); ;
            _urlEndpoint = urlEndpoint;
            _service = service;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method == "POST" &&
              context.Request.Path.StartsWithSegments(_urlEndpoint) &&
              context.Request != null &&
              context.Request.ContentType.ToLower().Contains("text/xml"))
            {
                var rdr = new StreamReader(context.Request.Body);
                var xml = rdr.ReadToEnd();
                _logger.LogInformation($"Request XMLRPC: {xml}");
                var result = _service.Invoke(xml);
                _logger.LogInformation($"Result XMLRPC: {result}");
                await context.Response.WriteAsync(result, Encoding.UTF8);
                context.Response.ContentType = "text/xml";
            }

            // Continue On
            await _next.Invoke(context);
        }
    }
}