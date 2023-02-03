using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AppZim
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
              name: "set-password",
              url: "set-password",
              defaults: new { controller = "Login", action = "SetPass", id = UrlParameter.Optional }
          );
            routes.MapRoute(
              name: "log-in",
              url: "log-in",
              defaults: new { controller = "Login", action = "Signout", id = UrlParameter.Optional }
          );
            routes.MapRoute(
             name: "forgot-password",
             url: "forgot-password",
             defaults: new { controller = "Login", action = "ForgotPassword", id = UrlParameter.Optional }
         );
            routes.MapRoute(
            name: "login-not-password",
            url: "login-not-password",
            defaults: new { controller = "Login", action = "SigninNotPassword", id = UrlParameter.Optional }
        );
            routes.MapRoute(
             name: "doing-test",
             url: "doing-test/{id}/{metatitle}",
             defaults: new { controller = "StudentSet", action = "DoingTest", id = UrlParameter.Optional }
         );
            routes.MapRoute(
                name: "done-test",
                url: "done-test/{id}/{metatitle}",
                defaults: new { controller = "StudentSet", action = "DoneTest", id = UrlParameter.Optional }
            );
            routes.MapRoute(
              name: "my-set",
              url: "my-set",
              defaults: new { controller = "StudentSet", action = "MySet", id = UrlParameter.Optional }
          );
            routes.MapRoute(
               name: "Default",
               url: "{controller}/{action}/{id}",
               defaults: new { controller = "Login", action = "Signin", id = UrlParameter.Optional },
               namespaces: new string[] { "AppZim.Controllers" }
           );
        }
    }
}
