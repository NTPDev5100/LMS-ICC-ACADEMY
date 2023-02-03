using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppZim.Controllers
{
    public class StudentPrizeController : Controller
    {
        // GET: StudentPrize
        public ActionResult PrizeList()
        {
            return View();
        }
    }
}