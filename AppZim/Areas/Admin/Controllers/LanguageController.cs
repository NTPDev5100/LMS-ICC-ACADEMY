using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AppZim.Areas.Admin.Controllers
{
    public class LanguageController : BaseController
    {
        // GET: Admin/Language
        public ActionResult Language()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var l = new List<tbl_Language>();
            using (var db = new ZimEntities())
            {
                l = db.tbl_Language.Where(x => x.isDelete != true).ToList();
            }
            return View(l);
        }
        public ActionResult Delete(int id)
        {
            try
            {
                using (var db = new ZimEntities())
                {
                    var ck = db.tbl_Language.Where(x => x.ID == id).FirstOrDefault();
                    if (ck != null)
                    {
                        ck.isDelete = true;
                        db.SaveChanges();
                    }
                }
                SetAlert("Thành công!", "s");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception e)
            {
                SetAlert("Có lỗi", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }

        }
        [HttpPost]
        public ActionResult AddEditReason(FormCollection f)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                using (var db = new ZimEntities())
                {
                    var id = f.Get("id").ToInt(0);
                    var name = f.Get("name").ToString();
                    var code = f.Get("language-code").ToString();

                    if (id == 0)
                    {
                        var ck = new tbl_Language();
                        ck.LanguageCode = code;
                        ck.Name = name;
                        ck.CreateBy = aclog.UserName;
                        ck.CreateDate = DateTime.Now;
                        db.tbl_Language.Add(ck);
                        db.SaveChanges();
                    }
                    else
                    {
                        var ck = db.tbl_Language.Where(x => x.ID == id).FirstOrDefault();
                        ck.Name = name;
                        ck.LanguageCode = code;
                        db.SaveChanges();
                    }


                }
                SetAlert("Thành công!", "s");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception e)
            {
                SetAlert("Có lỗi", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
        }
        //public JsonResult 
    }
}