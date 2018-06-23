using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Pedidos.Startup))]
namespace Pedidos
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
