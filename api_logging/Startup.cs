using System;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using StackExchange.Redis;

namespace HelloApi
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            Console.WriteLine("test");
            var ip = Dns.GetHostAddressesAsync("redis").Result[0].ToString();          
            var redis = ConnectionMultiplexer.ConnectAsync(ip).Result;
 
            app.Run(async x => {
                var logger = loggerFactory.CreateLogger("HelloApi");
                logger.LogInformation("Api call with path: {path}", x.Request.Path);
                logger.LogInformation("Calling Redis");
                var db = redis.GetDatabase();
                var message = await db.StringGetAsync("message");
                await x.Response.WriteAsync("Redis says: " + message);
                logger.LogInformation("Request complete");
            });
        }
    }
}