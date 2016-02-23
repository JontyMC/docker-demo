using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Api
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