using AppZim.TableSql;
using AppZim.Models;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AppZim.ZIM;
using PagedList;

namespace AppZim.Areas.Admin.Controllers
{
    public class ClassController : BaseController
    {
        #region danh sách lớp
        public ActionResult ClassList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //page size
            int pageSize = 20;
            IEnumerable<tbl_Class> l = ClassTable.getall();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                string search = ViewBag.Search = Request.QueryString["search"].ToString();
                l = from r in l
                    where AssetCRM.RemoveUnicode(r.ClassName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(r.GradeName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select r;
            }
            int language = 2;
            if(!string.IsNullOrEmpty(Request.QueryString["slLanguage"]))
            {
                language = Request.QueryString["slLanguage"].ToInt(7);
            }
            l = l.Where(x => x.Language == language).ToList();
            ViewBag.Language = language;
            l = l.OrderBy(n => n.isHide).ThenByDescending(n => n.GradeID).ThenByDescending(n => n.ClassName).ToList();
            int total = l.Count();
            ViewBag.Total = total;
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            return View(l.Skip((page-1)*pageSize).Take(pageSize).ToList());
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddClass(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            int gradeid = f.Get("ddl-grade").ToInt(0);
            var grade = GradeTable.getbyID(gradeid);
            string classname = f.Get("txt-class-name").ToString();
            int language = f.Get("dll-language").ToInt(0);
            int coefficient = 0;
            //int coefficient = f.Get("slCoefficient").ToInt(0);
            int type = 2;
            //int type = f.Get("ddl-type").ToInt(1);
            int price = f.Get("txt-min").ToString().Trim().Replace(",", "").ToInt(0);
            int giavon = f.Get("txt-max").ToString().Trim().Replace(",", "").ToInt(0);
            if(price > giavon)
            {
                SetAlert("Giá tối đa không được thấp hơn giá tối thiểu", "e");
                return RedirectToAction("ClassList", "Class");
            }
            var rs = ClassTable.insert(classname, grade.ID, grade.GradeName, type, price, giavon, aclog.UserName, language, coefficient);

            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("ClassList", "Class");
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditClass(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            bool ishide = false;
            if (!string.IsNullOrEmpty(f.Get("chk-cb")))
                ishide = true;
            int ID = f.Get("hdfID").ToInt(0);
            int gradeid = f.Get("ddl-edit-grade").ToInt(0);
            var grade = GradeTable.getbyID(gradeid);
            string classname = f.Get("txt-edit-class-name").ToString();
            //int totallensson = f.Get("txt-edit-total-lensson").ToString().ToInt(0);
            int price = f.Get("txt-min-edit").ToString().Trim().Replace(",", "").ToInt(0);
            int giavon = f.Get("txt-max-edit").ToString().Trim().Replace(",", "").ToInt(0);
            if (price > giavon)
            {
                SetAlert("Giá tối đa không được thấp hơn giá tối thiểu", "e");
                return RedirectToAction("ClassList", "Class");
            }
            int language = f.Get("dll-language-edit").ToInt(0);
            //int coefficient = f.Get("slCoefficient-edit").ToInt(0);
            int coefficient = 0;
            int type = 2;
            //int type = f.Get("ddl-edit-type").ToInt(1);
            var rs = ClassTable.update(ID, classname, grade.ID, grade.GradeName, ishide, type, price, giavon, aclog.UserName, language, coefficient);

            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("ClassList", "Class");
        }
        #endregion

        //danh sách giáo trình
        public ActionResult AllCurriculumList()
        {
            return View();
        }

        [HttpPost]// tải tiết học
        public ActionResult LoadHistoryChangePrice(int classid)
        {
            StringBuilder ap = new StringBuilder();
            var l = ClassHistoryTable.getbyclassid(classid).OrderByDescending(n => n.CreatedDate).ToList();
            foreach (var item in l)
            {
                var ac = AccountTable.getbyusername(item.CreatedBy);
                if (ac == null)
                    continue;
                ap.Append("<div class=\"timeline-item mg-b-20\">");
                ap.Append(" <div class=\"timeline-body\">");
                ap.Append("     <h6 class=\"mg-b-0\">" + ac.FullName + "</h6>");
                ap.Append("     <p style = \"margin-bottom:0px;\" ><a href=\"javascript:;\">" + AccountTable.getrolehtml(ac.RoleID.Value) + "</a> " + item.CreatedDate.Value.ToString("dd/MM/yyyy HH:mm") + "</p>");
                ap.Append("     " + item.HistoryContent + "");
                ap.Append(" </div>");
                ap.Append("</div>");
            }
            return Json(new { d = ap.ToString() }, JsonRequestBehavior.AllowGet);
        }
    }
}