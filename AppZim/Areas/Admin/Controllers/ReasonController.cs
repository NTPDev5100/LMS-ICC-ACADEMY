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
    public class ReasonController : BaseController
    {
        // GET: Admin/Reason
        public ActionResult Reason()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var l = new List<tbl_Reason>();
            using(var db = new ZimEntities())
            {
                l = db.tbl_Reason.Where(x => x.isDelete != true).ToList();
            }
            return View(l);
        }
        public ActionResult Delete(int id)
        {
            try
            {
                using(var db = new ZimEntities())
                {
                    var ck = db.tbl_Reason.Where(x => x.ID == id).FirstOrDefault();
                    ck.isDelete = true;
                    db.SaveChanges();
                }
                SetAlert("Thành công!", "s");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            catch(Exception e) 
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
                    var type = f.Get("type").ToInt(0);
                    var typename = "";
                    var name = f.Get("name").ToString();
                    if(type == 1)
                    {
                        typename = "Lý do học";
                    }
                    else if(type == 2)
                    {
                        typename = "Lý do không học";
                    }

                    if(id == 0)
                    {
                        var ck = new tbl_Reason();
                        ck.Type = type;
                        ck.TypeName = typename;
                        ck.Name = name;
                        ck.CreateBy = aclog.UserName;
                        ck.CreateDate = DateTime.Now;
                        db.tbl_Reason.Add(ck);
                        db.SaveChanges();
                    }
                    else
                    {
                        var ck = db.tbl_Reason.Where(x => x.ID == id).FirstOrDefault();
                        ck.Type = type;
                        ck.TypeName = typename;
                        ck.Name = name;
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
        public ActionResult GetAllByType(int type)
        {
            var l = ReasonTable.GetAllByType(type);
            return Json(l, JsonRequestBehavior.AllowGet);
        }
    }
}