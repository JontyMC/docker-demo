using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace aspnetcoreapp
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            var redis = ConnectionMultiplexer.Connect("redis");
 
            app.Run(async x => {
                var db = redis.GetDatabase();
                var message = await db.StringGetAsync("message");
                await x.Response.WriteAsync(message);
            });
        }
    }
}