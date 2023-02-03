using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using Org.BouncyCastle.Ocsp;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TagLib.Ape;
using WebUI.Business;

namespace AppZim.Areas.Admin.Controllers
{
    public class FeedBackController : BaseController
    {
        // GET: Admin/FeedBack
        public ActionResult FeedBackList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            //page size
            int pageSize = 20;

            //key search
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {

                search = Request.QueryString["search"].ToString().Trim();
            }
            ViewBag.Search = search;
            int cate = 0;
            if (Request.QueryString["cate"].ToInt(0) != 0)
            {

                cate = Request.QueryString["cate"].ToInt(0);
            }
            ViewBag.Cate = cate;
            int tvv = 0;
            if (aclog.RoleID == 6)// tư vấn viên lấy nhưng ng nó support
            {
                tvv = aclog.ID;
            }
            if (aclog.RoleID != 6)
            {
                if (Request.QueryString["sp"].ToInt(0) != 0)
                {

                    tvv = Request.QueryString["sp"].ToInt(0);
                }
            }
            ViewBag.Sp = tvv;
            // lấy ngày đầu và ngày cuối tháng
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            string fd = firstDayOfMonth.ToString();
            ViewBag.From = firstDayOfMonth.ToString("dd/MM/yyyy");
            // thời gian bắt đầu từ
            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                try
                {
                    DateTime fromdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                    fd = fromdate.ToString();
                    ViewBag.From = fromdate.ToString("dd/MM/yyyy");
                }
                catch { }
            }
            // đến
            string td = lastDayOfMonth.ToString();
            ViewBag.To = lastDayOfMonth.ToString("dd/MM/yyyy");
            if (!string.IsNullOrEmpty(Request.QueryString["to"]))
            {
                try
                {
                    DateTime todate = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null).AddHours(23).AddMinutes(59).AddSeconds(59);
                    td = todate.ToString();
                    ViewBag.To = todate.ToString("dd/MM/yyyy");
                }
                catch { }
            }
            //sort
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);
            }
            ViewBag.Sort = sort;
            List<get_list_feedback_Result> l = new List<get_list_feedback_Result>();
            int total = 0;
            string data = "";
            using (var db = new ZimEntities())
            {
                l = db.get_list_feedback(search, sort, cate, fd, td, page - 1, pageSize, tvv).ToList();
                if (l.Count > 0)
                {
                    total = l[0].TotalRow.Value;
                }
            }
            ViewBag.Total = total;
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;

            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            ViewBag.Data = data.ToString();

            return View(l);
        }

        public ActionResult FeedBack()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            int type = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["type-nav"]))
                type = Request.QueryString["type-nav"].ToInt(0);
            ViewBag.Type = type;
            return View();
        }

        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddFeedBack(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

            var cate = FeedBackCategoryTable.getbyID(f.Get("ddl-type").ToInt(0));
            if (cate == null)
            {
                SetAlert("Không tìm thấy loại phản hồi", "e");
                return RedirectToAction("FeedBack", "FeedBack");
            }
            string title = f.Get("txt-title").ToString();
            string content = f.Get("hdfanswer").ToString();
            int studentid = 0;
            if (!string.IsNullOrEmpty(f.Get("ddl-student")))
            {
                studentid = f.Get("ddl-student").ToInt(0);
            }
            bool check = false;
            if (!string.IsNullOrEmpty(f.Get("sendtostudent")))
            {
                check = true;
            }
            tbl_Account sp = new tbl_Account();
            if (aclog.SupportUID != null)
                sp = AccountTable.getbyID(aclog.SupportUID.Value);
            if (sp == null)
            {
                sp = new tbl_Account();
                sp.ID = 0;
                sp.FullName = "";
            }
            string titlefeed = "Phải hồi mới";
            if (aclog.RoleID == 6)
            {
                if (check)
                {
                    titlefeed = "Hỗ trợ học viên";
                    cate.ID = -1;
                    cate.CategoryFeedback = "Hỗ trợ học viên";
                    tbl_Account student = AccountTable.getbyID(studentid);
                    if (student != null)
                    {
                        sp.ID = studentid;
                        sp.FullName = student.FullName;
                    }
                }
            }
            var rs = FeedBackTable.insert(aclog.ID, aclog.FullName, sp.ID, sp.FullName, cate.ID, cate.CategoryFeedback, title, content, aclog.UserName);
            if (rs != null)
            {
                //gửi phàn hồi mới cho admin manager và support
                List<tbl_Account> ad = AccountTable.getall().Where(n => (n.RoleID == 1 || n.RoleID == 2) && n.Status == 2).ToList();
                if (aclog.SupportUID != null)
                    ad.Add(AccountTable.getbyID(aclog.SupportUID.Value));
                //đẩy thông báo đi
                foreach (var itemac in ad)
                {
                    if (itemac != null)
                    {
                        string strPathAndQuery = Request.Url.PathAndQuery;
                        string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                        string datalink = "" + strUrl + "Admin/FeedBack/FeedBackDetail/" + rs.ID;
                        var nt = NotificationTable.insertsingle("", titlefeed, "Có phản hồi mới từ [ " + aclog.FullName + " ].", itemac.ID, aclog.UserName, 8, rs.ID);
                        
                        
                    }
                }
                if (aclog.RoleID == 6 && check == true)
                {
                    string strPathAndQuery = Request.Url.PathAndQuery;
                    string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                    string datalink = "" + strUrl + "Admin/FeedBack/FeedBackDetail/" + rs.ID;
                    var nt = NotificationTable.insertsingle("", titlefeed, "Có phản hồi mới từ [ " + aclog.FullName + " ].", sp.ID, aclog.UserName, 8, rs.ID);
                    
                    
                }
                // Nếu loại phản hồi có ID = 15
                // tự động gửi mail đến tuyensinh@ieclanguage.edu.vn
                AssetCRM.SendMail("tuyensinh@iccacademy.edu.vn", title, content);

                SetAlert("Thành công", "s");
            }
            else
                SetAlert("Không hành công", "e");
            return RedirectToAction("FeedBack", "FeedBack", new { page = 0 });
        }


        [HttpPost]
        public ActionResult UploadFileImageFeedBack()
        {
            string link = "";
            var httpContext = HttpContext;
            if (httpContext.Request.Files.Count > 0)
            {
                for (int i = 0; i < httpContext.Request.Files.Count; i++)
                {
                    var file = httpContext.Request.Files[i];
                    if (file != null)
                    {
                        //string name = Path.GetFileNameWithoutExtension(file.FileName);
                        string ext = Path.GetExtension(file.FileName);
                        if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                        {
                            return Json(new { rs = false, l = "" }, JsonRequestBehavior.AllowGet);
                        }

                        string fileContentType = file.ContentType; // getting ContentType
                        byte[] tempFileBytes = new byte[file.ContentLength];
                        //var data = file.InputStream.Read(tempFileBytes, 0, Convert.ToInt32(file.ContentLength));
                        string fileName = file.FileName; // getting File Name
                        string fileExtension = Path.GetExtension(fileName).ToLower();
                        var result = AssetCRM.isValidFile(tempFileBytes, fileExtension, fileContentType); // Validate Header
                        if (result)
                        {
                            fileName = Guid.NewGuid() + ext;
                            var path = Path.Combine(Server.MapPath("~/Upload/feedback/"), fileName);

                            string strPathAndQuery = Request.Url.PathAndQuery;
                            string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");

                            link = "" + strUrl + "Upload/feedback/" + fileName;
                            file.SaveAs(path);
                        }
                        else
                            return Json(new { rs = false, l = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new { rs = true, l = link }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadContentFeedback(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            StringBuilder h = new StringBuilder();
            StringBuilder b = new StringBuilder();

            var f = FeedBackTable.getbyid(id);
            if (f != null)
            {
                var acc = AccountTable.getbyusername(f.CreatedBy);
                var l = FeedBackDetailTable.getbyfeedbackid(id);

                h.Append("<div class=\"avatar avatar-sm\"><img src = \"\" class=\"rounded-circle\" onerror=\"this.onerror = null; this.src = '/app-assets/zimv2/images/invis-user.png'; \"  alt=\"\"></div>");
                h.Append("<div class=\"media-body mg-l-10\">");
                h.Append("  <h6 class=\"mg-b-2 tx-13\">" + acc?.FullName + "</h6>");
                h.Append("  <span class=\"d-block tx-11 tx-color-03\">" + f.CreatedDate.Value.ToString("dd/MM/yyyy HH:mm") + "</span>");
                h.Append("</div>");


                b.Append("<div class=\"pd-20 pd-lg-25 report-content\">");
                b.Append("  <h5 class=\"mg-b-20\">" + f.TypeName + "</h5>");
                b.Append(f.ContentFeedBack);
                b.Append("</div>");
                b.Append("<div class=\"report-reply\">");
                foreach (var item in l)
                {
                    var accre = AccountTable.getbyusername(item.CreatedBy);
                    if (accre != null)
                    {
                        string av = "/app-assets/zimv2/images/invis-user.png";
                        if (!string.IsNullOrEmpty(accre?.Avatar))
                            av = accre.Avatar;

                        b.Append("<div class=\"pd-20 pd-lg-25 reply-block\">");
                        b.Append("  <div class=\"media reply-header\">");
                        b.Append("      <div class=\"avatar avatar-sm\"><img src = \"" + av + "\" class=\"rounded-circle\"onerror=\"this.onerror = null; this.src = '/app-assets/zimv2/images/invis-user.png'; \"  alt=\"\"></div>");
                        b.Append("      <div class=\"media-body mg-l-10\">");
                        b.Append("          <h6 class=\"mg-b-2 tx-13\">" + accre?.FullName + "</h6>");
                        b.Append("          <span class=\"d-block tx-11 tx-color-03\">" + item.CreatedDate.Value.ToString("dd/MM/yyyy HH:mm") + "</span>");
                        b.Append("      </div>");
                        b.Append("  </div>");
                        b.Append("  <div class=\"reply-content pd-t-20\">");
                        b.Append(item.ContentFeedback);
                        b.Append("  </div>");
                        b.Append("</div>");
                    }
                }
                b.Append("</div>");
            }
            //fb quan trọng
            var imp = false;
            int fid = 0;
            if (f != null)
                fid = f.ID;
            var ck = FeedBackImportantTable.getbyuidvsfeedback(aclog.ID, fid);
            if (ck != null)
            {
                if (!ck.IsHidden.Value)
                    imp = true;
            }

            return Json(new { rs = true, l = f, h = h.ToString(), b = b.ToString(), imp = imp }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetFeedback(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            var f = FeedBackTable.getbyid(id);
            if (f == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            else
            {
                f = FeedBackTable.updateview(f.ID, aclog.UserName);
                return Json(new { rs = true, l = f }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SumitRateFeedback(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

            var fe = FeedBackTable.getbyid(f.Get("hdffeedbackid").ToInt(0));
            if (fe == null)
            {
                SetAlert("Không tìm thấy phản hồi", "e");
                return RedirectToAction("FeedBack", "FeedBack");
            }
            int rate = f.Get("hdfRate").ToInt(5);
            if (aclog.UserName != fe.CreatedBy)
            {
                SetAlert("Bạn không có quyền chấm sao", "e");
                return RedirectToAction("FeedBackDetail", "FeedBack", new { id = fe.ID });
            }
            if (!fe.IsHidden.Value)
            {
                SetAlert("Chưa thể chấm sao, phản hồi đang xử lý", "e");
                return RedirectToAction("FeedBackDetail", "FeedBack", new { id = fe.ID });
            }
            if (fe.Rate == null)
            {
                var rs = FeedBackTable.updaterate(fe.ID, rate, aclog.UserName);
                if (rs != null)
                {
                    if (fe.SupportID != 0)
                    {
                        //gửi thông báo cho thằng sp
                        var nt = NotificationTable.insertsingle("", "Feedback bạn xử lý được chấm điểm", "Số điểm được chấm cho feedbacj bạn xử lý: " + rate + ", " + fe.Title, fe.SupportID.Value, aclog.UserName, 8, fe.ID);

                    }
                    SetAlert("Thành công", "s");
                }
                else
                    SetAlert("Không thành công", "e");
            }
            else
                SetAlert("Vé hỗi trợ đã được đánh giá", "e");
            return RedirectToAction("FeedBackDetail", "FeedBack", new { id = fe.ID });
        }

        [HttpPost]
        public ActionResult ReviewDoneFeedback(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 6 && aclog.RoleID != 1 && aclog.RoleID != 3 && aclog.RoleID != 2 && aclog.RoleID != 8)
            {
                SetAlert("Bạn không có quyền", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
            var fe = FeedBackTable.getbyid(f.Get("hdffeedbackid").ToInt(0));
            if (fe == null)
            {
                SetAlert("Không tìm thấy phản hồi", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            if (fe.IsHidden.Value)
            {
                SetAlert("Không thành công", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            var rs = FeedBackTable.updatedone(fe.ID, aclog.UserName);
            if (rs != null)
            {
                //gửi cho thằng gửi báo khi đã xong
                if (fe.UID != 0)
                {
                    //gửi thông báo cho thằng sp
                    var nt = NotificationTable.insertsingle("", "Feedback của bạn đã được xử lý xong", fe.Title, fe.UID.Value, aclog.UserName, 8, fe.ID);

                }
                SetAlert("Thành công", "s");
            }
            else
                SetAlert("Không thành công", "e");
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        public ActionResult FeedBackDetail(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var f = FeedBackTable.getbyid(id);
            if (f == null)
            {
                SetAlert("Không tìm thấy phản hồi", "e");
                return RedirectToAction("FeedBackList", "FeedBack");
            }
            FeedBackTable.updateview(id, aclog.UserName);
            return View(f);
        }

        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult RepFeedback(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var fe = FeedBackTable.getbyid(f.Get("hdffeedbackid").ToInt(0));
            if (fe == null)
            {
                SetAlert("Không tìm thấy phản hồi", "e");
                return RedirectToAction("FeedBackList", "FeedBack");
            }
            string content = f.Get("hdfcontent").ToString();
            var rs = FeedBackDetailTable.insert(fe.ID, content, aclog.UserName);
            if (rs != null)
            {
                string username = "";
                if (aclog.UserName != fe.CreatedBy)
                    username = fe.CreatedBy;
                else
                {
                    if (fe.SupportID != 0)
                        username = AccountTable.getbyID(fe.SupportID.Value).UserName;
                }
                if (!string.IsNullOrEmpty(username))
                {
                    var acpush = AccountTable.getbyusername(username);
                    //gửi thông báo cho thằng sp
                    var nt = NotificationTable.insertsingle("", "Feedback có nội dung mới", fe.Title, acpush.ID, aclog.UserName, 8, fe.ID);

                }
                SetAlert("Thành công", "s");
            }
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("FeedBackDetail", "FeedBack", new { id = fe.ID });
        }
        //reply feedback qua ajax
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ReplyFeedback(int id, string content)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var fe = FeedBackTable.getbyid(id);
            if (fe == null)
                return Json(new { rs = false, noti = "Fail" }, JsonRequestBehavior.AllowGet);

            var dataencode = System.Convert.FromBase64String(content);
            var datadecode = System.Text.Encoding.UTF8.GetString(dataencode);

            var rs = FeedBackDetailTable.insert(fe.ID, datadecode, aclog.UserName);
            if (rs != null)
            {
                FeedBackTable.updatemodified(fe.ID, aclog.UserName);
                string username = "";
                if (aclog.UserName != fe.CreatedBy)
                    username = fe.CreatedBy;
                else
                {
                    if (fe.SupportID != 0)
                        username = AccountTable.getbyID(fe.SupportID.Value).UserName;
                }
                if (!string.IsNullOrEmpty(username))
                {
                    var acpush = AccountTable.getbyusername(username);
                    //gửi thông báo cho thằng sp
                    var nt = NotificationTable.insertsingle("", "Reply feedback", fe.Title, acpush.ID, aclog.UserName, 8, fe.ID);

                }
                StringBuilder b = new StringBuilder();
                string av = "/app-assets/zimv2/images/invis-user.png";
                if (!string.IsNullOrEmpty(aclog.Avatar))
                    av = aclog.Avatar;

                b.Append("<div class=\"pd-20 pd-lg-25 reply-block\">");
                b.Append("  <div class=\"media reply-header\">");
                b.Append("      <div class=\"avatar avatar-sm\"><img src = \"" + av + "\" class=\"rounded-circle\" onerror=\"this.onerror = null; this.src = '/app-assets/zimv2/images/invis-user.png'; \"  alt=\"\"></div>");
                b.Append("      <div class=\"media-body mg-l-10\">");
                b.Append("          <h6 class=\"mg-b-2 tx-13\">" + aclog.FullName + "</h6>");
                b.Append("          <span class=\"d-block tx-11 tx-color-03\">" + rs.CreatedDate.Value.ToString("dd/MM/yyyy HH:mm") + "</span>");
                b.Append("      </div>");
                b.Append("  </div>");
                b.Append("  <div class=\"reply-content pd-t-20\">");
                b.Append(rs.ContentFeedback);
                b.Append("  </div>");
                b.Append("</div>");
                return Json(new { rs = true, d = b.ToString() }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { rs = false, noti = "Fail" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateRate(int id, int rate)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

            var fe = FeedBackTable.getbyid(id);
            if (fe == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            if (aclog.UserName != fe.CreatedBy)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            if (fe.Rate == null)
            {
                var rs = FeedBackTable.updaterate(fe.ID, rate, aclog.UserName);
                if (rs != null)
                {
                    if (fe.SupportID != 0)
                    {
                        //gửi thông báo cho thằng sp
                        var nt = NotificationTable.insertsingle("", "Feedback bạn xử lý được chấm điểm", "Số điểm được chấm cho feedback bạn xử lý: " + rate + "," + fe.Title, fe.SupportID.Value, aclog.UserName, 8, fe.ID);

                    }
                    return Json(new { rs = true, noti = "Success" }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { rs = false, noti = "Fail" }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { rs = false, noti = "Fail" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateImportant(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

            var fe = FeedBackTable.getbyid(id);
            if (fe == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            var rs = FeedBackImportantTable.insert(aclog.ID, id, aclog.UserName);
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SearchFeedback(string search)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            IEnumerable<tbl_FeedBack> l = FeedBackTable.getbyuid(aclog.ID);


            var fs = from f in l
                     where AssetCRM.RemoveUnicode(f.SupportName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                     || AssetCRM.RemoveUnicode(f.TypeName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                     || AssetCRM.RemoveUnicode(f.Title).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                     select f;

            l = fs;
            StringBuilder html = new StringBuilder();
            StringBuilder loadmore = new StringBuilder();
            if (l.Count() > 0)
            {
                l = l.OrderBy(n => n.FullName);
                if (l.Count() > 20)
                {
                    l = l.Skip(0).Take(20).ToList();
                    loadmore.Append("<div class=\"pd-t-1 pd-b-5\">");
                    loadmore.Append("    <a href = \"javascript:;\" class=\"btn btn-xs btn-block btn-light bd-0 tx-uppercase tx-10 tx-spacing-1 tx-medium mn-ht-0 btn-loadmore\">Load more</a>");
                    loadmore.Append("</div>");
                }
                html.Append("<ul class=\"list-unstyled media-list mg-b-0\">");
                foreach (var item in l)
                {
                    string n = "";
                    for (int i = item.SupportName.Length - 1; i >= 0; i--)
                    {
                        if (item.SupportName[i].ToString().Contains(" "))
                        {
                            n = item.SupportName[i + 1].ToString();
                            break;
                        }
                    }

                    html.Append("<li class=\"media class-load\" data-id=\"" + item.ID + "\">");
                    html.Append("   <div class=\"avatar\">");
                    html.Append("       <div class=\"avatar\"><span class=\"avatar-initial rounded-circle bg-indigo\">" + n + "</span></div>");
                    html.Append("   </div>");
                    html.Append("   <div class=\"media-body mg-l-15\">");
                    html.Append("       <div class=\"tx-color-03 d-flex align-items-center justify-content-between mg-b-2\">");
                    html.Append("           <span class=\"tx-12\">" + item.SupportName + "</span>");
                    html.Append("           <span class=\"tx-11\">" + item.CreatedDate.Value.ToString("dd/MM/yyyy") + "</span>");
                    html.Append("       </div>");
                    html.Append("       <h6 class=\"tx-13\">" + item.TypeName + "</h6>");
                    html.Append("       <p class=\"tx-12 tx-color-03 mg-b-0\">" + item.Title + "</p>");
                    html.Append("   </div>");
                    html.Append("</li>");
                }
                html.Append("</ul>");
            }
            return Json(new { rs = true, html = html.ToString(), Loadmore = loadmore.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadMore(string search, int type, int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            var lastfb = FeedBackTable.getbyid(id);
            IEnumerable<tbl_FeedBack> l = FeedBackTable.getbyuid(aclog.ID).Where(n => n.ModifiedDate < lastfb.ModifiedDate && n.ID > id).OrderByDescending(n => n.ModifiedDate).ToList();

            var fs = from f in l
                     where AssetCRM.RemoveUnicode(f.SupportName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                     || AssetCRM.RemoveUnicode(f.TypeName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                     || AssetCRM.RemoveUnicode(f.Title).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                     select f;

            l = fs;
            StringBuilder html = new StringBuilder();
            if (l.Count() > 0)
            {
                l = l.OrderBy(n => n.FullName);
                if (l.Count() > 20)
                {
                    l = l.Skip(0).Take(20).ToList();
                }
                html.Append("<ul class=\"list-unstyled media-list mg-b-0\">");
                foreach (var item in l)
                {
                    string n = "";
                    for (int i = item.SupportName.Length - 1; i >= 0; i--)
                    {
                        if (item.SupportName[i].ToString().Contains(" "))
                        {
                            n = item.SupportName[i + 1].ToString();
                            break;
                        }
                    }
                    html.Append("<li class=\"media class-load\" data-id=\"" + item.ID + "\">");
                    html.Append("   <div class=\"avatar\">");
                    html.Append("       <div class=\"avatar\"><span class=\"avatar-initial rounded-circle bg-indigo\">" + n + "</span></div>");
                    html.Append("   </div>");
                    html.Append("   <div class=\"media-body mg-l-15\">");
                    html.Append("       <div class=\"tx-color-03 d-flex align-items-center justify-content-between mg-b-2\">");
                    html.Append("           <span class=\"tx-12\">" + item.SupportName + "</span>");
                    html.Append("           <span class=\"tx-11\">" + item.CreatedDate.Value.ToString("dd/MM/yyyy") + "</span>");
                    html.Append("       </div>");
                    html.Append("       <h6 class=\"tx-13\">" + item.TypeName + "</h6>");
                    html.Append("       <p class=\"tx-12 tx-color-03 mg-b-0\">" + item.Title + "</p>");
                    html.Append("   </div>");
                    html.Append("</li>");
                }
                html.Append("</ul>");
            }
            if (l.Count() > 0)
                id = l.OrderBy(n => n.ModifiedDate).FirstOrDefault().ID;
            return Json(new { rs = true, html = html.ToString(), skipid = id }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Danh sách phản hồi lấy ra check
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult FeedbackCheckSolution(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //page size
            int pageSize = 20;
            List<tbl_FeedBackCheckSolution> l = new List<tbl_FeedBackCheckSolution>();
            //key search
            string search = "";
            int status = 0;

            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                search = Request.QueryString["search"].ToString().Trim();
            ViewBag.Search = search;

            if (!string.IsNullOrEmpty(Request.QueryString["status"]))
                status = Request.QueryString["status"].ToInt(0);
            ViewBag.Status = status;
            if (Request.QueryString["search"].ToInt(0) == 0)
            {
                //tạo tham số và gọi stored
                SqlParameter[] parameters =
                    {
                    new SqlParameter("@qsearch", SqlDbType.NVarChar, 2000) { Value = search },
                    new SqlParameter("@status", SqlDbType.Int) { Value = status }
                };
                var rs = SqlHelper.ExecuteDataTable("lay_giai_phap_cua_tu_van_vien_kiem_tra", parameters);
                l = (from rw in rs.AsEnumerable()
                     select new tbl_FeedBackCheckSolution()
                     {
                         ID = Convert.ToInt32(rw["ID"]),
                         FeedBackID = Convert.ToInt32(rw["FeedBackID"]),
                         StatusAcceptReject = Convert.ToInt32(rw["StatusAcceptReject"]),
                         indexName = rw["indexName"].ToString(),
                         UIDCheckFixName = rw["UIDCheckFixName"].ToString(),
                         UIDCheckFix = Convert.ToInt32(rw["UIDCheckFix"]),
                         ModifiedBy = rw["SupportName"].ToString()
                     }).OrderBy(n => n.StatusAcceptReject).ThenByDescending(n => n.CreatedDate).ToList();
            }
            else
            {
                tbl_FeedBackCheckSolution t = FeedBackCheckSolutionTable.getbyFeedBackID(Request.QueryString["search"].ToInt(0));
                l.Add(t);
            }
            if (aclog.RoleID == 2)
                l = l.Where(n => n.UIDCheckFix == aclog.ID).ToList();
            ViewBag.ToTal = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }
        [HttpPost]
        public ActionResult CheckSolution(int id, int status)
        {
            if (Session["UID"] == null)
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 7 && aclog.RoleID != 8)// khác giáo viên bị đá
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            var ck = FeedBackCheckSolutionTable.getbyFeedBackID(id);
            if (aclog.RoleID == 8)
            {
                if (ck.UIDCheckFix != aclog.ID)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            var l = FeedBackCheckSolutionTable.updateaccept(ck.FeedBackID, status, aclog.UserName);
            if (l == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            bool reject = false;
            if (status == 3)//reject
                reject = true;
            FeedBackTable.rejectfix(l.FeedBackID, reject, aclog.UserName);
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }
    }
}