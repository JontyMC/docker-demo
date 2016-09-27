using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HelloApi
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.Run(x =>
            {
                return x.Response.WriteAsync("Hello from ASP.NET Core!");
            });
        }
    }
}