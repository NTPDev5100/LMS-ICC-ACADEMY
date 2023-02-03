using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppZim.Areas.Admin.Controllers
{
    public class CertificatceController : BaseController
    {
        // GET: Admin/Certificatce
        [HttpGet]
        public ActionResult Certificatce(int CourseStudentId)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                tbl_CoursesStudent cs = CoursesStudentTable.getbyid(CourseStudentId);
                if (cs == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "e");
                    return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                }
                var ck = CertificatceCustomerTable.GetByStudentIdCoursesstudentId(cs.StudentUID.Value, cs.CourseID.Value);
                if (ck != null)
                {
                    if (ck.isAccept == true)
                    {
                        SetAlert("Học viên đã có chứng nhận!", "s");
                        return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                    }
                    else
                    {
                        SetAlert("Chứng nhận đang phê duyệt!", "s");
                        return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                    }
                }
                return View(cs);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("CertificatceController", "Certificatce", Session["UID"].ToString().ToInt(0), ex.Message);
                SetAlert("Sự cố bất ngờ, vui lòng thử lại", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddCertificatceCustomer(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int idcs = f.Get("hdf-CourseStudentId").ToInt(0);
            var cs = CoursesStudentTable.getbyid(idcs);
            if (cs == null)
            {
                SetAlert("Không tìm thấy dữ liệu!", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            tbl_CertificatceCustomer cc = CertificatceCustomerTable.insert(cs.StudentUID.Value, idcs, aclog.UserName);
            if (cc == null)
            {
                SetAlert("Sự cố bất ngờ, vui lòng thử lại", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            SetAlert("Thành công", "s");
            CoursesStudentTable.UpdateIsCertificatce(cs.ID, true);
            return RedirectToAction("CourseStudent", "Customer", new { area = "Admin" });
        }

        [HttpGet]
        public ActionResult CensorshipCertificatce(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var df = DateTime.ParseExact("01/01/1001", "dd/MM/yyyy", null).Date;
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                string search = "";
                ViewBag.Search = Request.QueryString["search"];
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                    search = Request.QueryString["search"].ToString().Trim();
                int sort = 2;
                ViewBag.Sort = Request.QueryString["sort"].ToInt(2);
                if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
                    sort = Request.QueryString["sort"].ToInt(0);
                //page size              
                int pageSize = 20;
                List<VietCoCo_certificate_browsing_Result> l = new List<VietCoCo_certificate_browsing_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.VietCoCo_certificate_browsing(search, sort, page - 1, pageSize, aclog.ID, aclog.RoleID).ToList();
                }
                ViewBag.StartIndex = (page - 1) * pageSize;
                int total = 0;
                if (l.Count > 0)
                {
                    total = l[0].TotalRow.Value;
                }
                ViewBag.Total = total;
                int PageCount = 0;
                if (total % pageSize == 0)
                    PageCount = total / pageSize;
                else
                    PageCount = total / pageSize + 1;
                ViewBag.Page = page;
                ViewBag.PageCount = PageCount;
                return View(l);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CertificatceController", "CensorshipCertificatce", uid, ex.Message);
                SetAlert("Sự cố bất ngờ, vui lòng thử lại", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }

        /// <summary>
        /// Duyệt chứng chỉ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AcceptCertificatce(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var cc = CertificatceCustomerTable.getbyid(id);
            if (cc.isAccept == false)//chấp nhận
                CertificatceCustomerTable.updateaccept(cc.ID, true, aclog.UserName);
            else// ko duyệt
                CertificatceCustomerTable.updateaccept(cc.ID, false, aclog.UserName);
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult PrintCertificatce(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });
                tbl_CertificatceCustomer check = CertificatceCustomerTable.getbyid(id);
                if (check == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "w");
                    return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                }
                else
                {
                    if (check.isAccept == false && aclog.RoleID != 1)
                    {
                        SetAlert("Chứng chỉ chưa được duyệt!", "w");
                        return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                    }
                }
                return View(check);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CertificatceController", "PrintCertificatce", uid, ex.Message);
                SetAlert("Sự cố bất ngờ, vui lòng thử lại", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }
    }
}