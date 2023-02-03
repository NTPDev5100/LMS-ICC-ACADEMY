using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace AppZim.Areas.Admin.Controllers
{
    public class BaseController : Controller
    {
        public static string ProjectName = "ICC ACADEMY";
        public static string ProjectDomain = "https://crm.iccacademy.edu.vn/";
        protected void SetAlert(string message, string type)
        {
            TempData["AlertMessage"] = message;
            if (type == "s")
            {
                TempData["Icon"] = "check";
                TempData["Class"] = "noti-success";
            }
            else if (type == "w")
            {
                TempData["Icon"] = "warning";
                TempData["Class"] = "noti-warning";
            }
            else if (type == "e")
            {
                TempData["Icon"] = "error_outline";
                TempData["Class"] = "noti-error";
            }
        }

        protected void SetlinkInvoice(string url)
        {
            TempData["InvoiceURL"] = url;            
        }
        protected void SetLinkOnePay(string url)
        {
            TempData["OnePayURL"] = url;
        }
        public static bool Like(string toSearch, string toFind)
        {
            return new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(toFind, ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z", RegexOptions.Singleline).IsMatch(toSearch);
        }
    }
}