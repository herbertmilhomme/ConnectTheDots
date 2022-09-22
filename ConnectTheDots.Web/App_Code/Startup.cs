using ConnectTheDots.Web;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]//, "Configuration"

namespace ConnectTheDots.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
