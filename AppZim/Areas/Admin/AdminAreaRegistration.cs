using System.Web.Mvc;

namespace AppZim.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
             name: "invoice",
             url: "invoice",
             defaults: new { controller = "Cashier", action = "InvoiceNew", id = UrlParameter.Optional }
         );
            context.MapRoute(
             name: "vouchers",
             url: "vouchers",
             defaults: new { controller = "Cashier", action = "Vouchers", id = UrlParameter.Optional }
         );
            context.MapRoute(
             name: "not-found",
             url: "not-found",
             defaults: new { controller = "Home", action = "ErrorNotFound", id = UrlParameter.Optional }
         );
            context.MapRoute(
            name: "demon",
            url: "demon",
            defaults: new { controller = "NamDemo", action = "demon", id = UrlParameter.Optional }
        );
            context.MapRoute(
              name: "feedback-detail",
              url: "admin/phan-hoi-chi-tiet/{metatitle}-{id}",
              defaults: new { controller = "FeedBack", action = "FeedBackDetail", id = UrlParameter.Optional }
          );
            context.MapRoute(
             name: "news-feed",
             url: "news-feed",
             defaults: new { controller = "Account", action = "ProfileUser", id = UrlParameter.Optional }
         );
            context.MapRoute(
             name: "post-detail",
             url: "news/bai-viet/{metatitle}-{id}",
             defaults: new { controller = "Account", action = "PostDetail", id = UrlParameter.Optional }
         );
            context.MapRoute(
            name: "print-contract",
            url: "print-contract",
            defaults: new { controller = "Contract", action = "PrintContract", id = UrlParameter.Optional }
        );
            context.MapRoute(
            name: "store",
            url: "store",
            defaults: new { controller = "SetPackage", action = "Store", id = UrlParameter.Optional }
        );
            context.MapRoute(
            name: "checkout",
            url: "checkout",
            defaults: new { controller = "SetPackage", action = "CheckOut", id = UrlParameter.Optional }
        );

            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new string[] { "AppZim.Areas.Admin.Controllers" }
            );
        }
    }
}