using AppZim.Areas.Admin.Controllers;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppZim.Controllers
{
    public class StudentSetCustomController : BaseController
    {
        // GET: StudentSetCustom
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SetPackages(int ID = 0)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 5)
                {
                    return RedirectToAction("Signin", "Login", new { area = "" });
                }
                var pack = SetPackageStudentTable.get_by_setID_and_studentID(ID, aclog.ID);
                if (pack == null)
                {
                    SetAlert("You not have this set!", "e");
                    return RedirectToAction("MySet", "StudentSet", new { area = "" });
                }
                ViewBag.SetID = ID;
                return View(pack);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("StudentSetCustomController", "SetPackages", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
            }

        }
    }
}