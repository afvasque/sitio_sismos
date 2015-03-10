using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Sismos.Startup))]
namespace Sismos
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
