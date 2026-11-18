using EcoRutas.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using EcoRutas.Data.Context;

[assembly: OwinStartup(typeof(EcoRutas.Web.Startup))]

namespace EcoRutas.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            app.CreatePerOwinContext(EcoRutasContext.Create);

            app.CreatePerOwinContext<ApplicationUserManager>(
                ApplicationUserManager.Create);

            app.UseCookieAuthentication(
                new CookieAuthenticationOptions
                {
                    AuthenticationType =
                        DefaultAuthenticationTypes.ApplicationCookie,

                    LoginPath = new PathString("/Account/Login"),

                    CookieHttpOnly = true,

                    SlidingExpiration = true
                });
        }
    }
}