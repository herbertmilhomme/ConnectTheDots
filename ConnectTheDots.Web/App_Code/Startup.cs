using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ConnectTheDots.Web.Startup))]

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
