using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System.Threading.Tasks;

namespace Api
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.Run(async x => await x.Response.WriteAsync("Hello, world!"));
        }
    }
}