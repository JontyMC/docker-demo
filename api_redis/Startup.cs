using System;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;

namespace HelloApi
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            var ip = Dns.GetHostAddressesAsync("redis").Result[0].ToString();          
            var redis = ConnectionMultiplexer.ConnectAsync(ip).Result;
 
            app.Run(async x => {
                var db = redis.GetDatabase();
                var message = await db.StringGetAsync("message");
                await x.Response.WriteAsync("Redis says: " + message);
            });
        }
    }
}