using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Last_Dance_System.Startup))]
namespace Last_Dance_System
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
