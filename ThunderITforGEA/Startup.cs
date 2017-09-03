using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ThunderITforGEA.Startup))]
namespace ThunderITforGEA
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
