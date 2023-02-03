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
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace AppZim.Areas.Admin.Controllers
{
    public class PostNewFeedController : BaseController
    {
        // GET: Admin/PostNewFeed
        public ActionResult PostList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5 || aclog.RoleID == 6)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //page size
            int pageSize = 20;

            //lấy bài mới 
            //List<tbl_ExerciseAddonStudent> l = new List<tbl_ExerciseAddonStudent>();
            IEnumerable<tbl_PostContent> l = PostContentTable.getbyall().Where(n => n.StatusPost != true).OrderByDescending(n => n.ModifiedDate).ToList();
            //Khác admin và quản lý thì lấy bài của nó thôi
            if (aclog.RoleID != 1 && aclog.RoleID != 2)
                l = l.Where(n => n.CreatedBy == aclog.UserName).ToList();
            //key search
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from ex in l
                    where AssetCRM.RemoveUnicode(ex.TitlePost).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.CreatedBy).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select ex;
            }
            // lấy ngày đầu và ngày cuối tháng
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            // thời gian bắt đầu từ
            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                try
                {
                    DateTime fromdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                    l = from ex in l
                        where ex.CreatedDate.Value.Date >= fromdate.Date
                        select ex;
                    ViewBag.From = fromdate.ToString("dd/MM/yyyy");
                }
                catch { }
            }
            else
            {
                try
                {
                    DateTime fromdate = firstDayOfMonth;
                    l = from ex in l
                        where ex.CreatedDate.Value.Date >= fromdate.Date
                        select ex;
                    ViewBag.From = fromdate.ToString("dd/MM/yyyy");
                }
                catch { }
            }
            // đến
            if (!string.IsNullOrEmpty(Request.QueryString["to"]))
            {
                try
                {
                    DateTime todate = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null);
                    l = from ex in l
                        where ex.CreatedDate.Value.Date <= todate.Date
                        select ex;
                    ViewBag.To = todate.ToString("dd/MM/yyyy");
                }
                catch { }
            }
            else
            {
                try
                {
                    DateTime todate = lastDayOfMonth;
                    l = from ex in l
                        where ex.CreatedDate.Value.Date <= todate.Date
                        select ex;
                    ViewBag.To = todate.ToString("dd/MM/yyyy");
                }
                catch { }
            }
            //sort
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);
                if (sort == 1)
                    l = l.OrderBy(n => n.CreatedDate).ToList();
                if (sort == 2)
                    l = l.OrderByDescending(n => n.CreatedDate).ToList();
                if (sort == 4)
                    l = l.OrderBy(n => n.isHideStaff).ToList();
                if (sort == 3)
                    l = l.OrderByDescending(n => n.isHideStaff).ToList();
                if (sort == 6)
                    l = l.OrderBy(n => n.isHideStudent).ToList();
                if (sort == 5)
                    l = l.OrderByDescending(n => n.isHideStudent).ToList();
            }
            ViewBag.Sort = sort;
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }

        public ActionResult PostNew()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            return View();
        }

        [HttpPost]
        public ActionResult UploadFilePost()
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
                        var data = file.InputStream.Read(tempFileBytes, 0, Convert.ToInt32(file.ContentLength));
                        string fileName = Guid.NewGuid() + ext; // getting File Name
                        string fileExtension = Path.GetExtension(fileName).ToLower();
                        var result = AssetCRM.isValidFile(tempFileBytes, fileExtension, fileContentType); // Validate Header
                        if (result)
                        {
                            fileName = Guid.NewGuid() + ext;
                            var path = Path.Combine(Server.MapPath("~/Upload/newfeed/"), fileName);

                            string strPathAndQuery = Request.Url.PathAndQuery;
                            string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");

                            link = strUrl + "/Upload/newfeed/" + fileName;
                            file.SaveAs(path);
                        }
                    }
                }
            }
            return Json(new { rs = true, l = link }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddPost(FormCollection f, HttpPostedFileBase filetitle)
        {
            string page1 = Request.Url.ToString();
            string page2 = Request.UrlReferrer.ToString();
            if (page1 != page2)
                Session["PrePage"] = page2;

            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            string linkimg = "";
            string linkimgthumb = "";
            int filetype = 1;//1 imgage,2 video
            int ptype = f.Get("txt-type").ToInt(0);
            int groupid = f.Get("hdfGroup").ToInt(0);
            if (ptype == 0)
            {
                groupid = 0;
                if (ModelState.IsValid)
                {   //iterating through multiple file collection                  
                    if (filetitle != null)
                    {
                        string name = Path.GetFileNameWithoutExtension(filetitle.FileName);
                        string ext = Path.GetExtension(filetitle.FileName);
                        if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                        {
                            SetAlert("File của bạn đã bị chặn, vui lòng không tải lên file .php, .asp, .aspx, .ps1", "e");
                            return RedirectToAction("PostNew", "PostNewFeed");
                        }
                        string fileContentType = filetitle.ContentType; // getting ContentType
                        byte[] tempFileBytes = new byte[filetitle.ContentLength];
                        var data = filetitle.InputStream.Read(tempFileBytes, 0, Convert.ToInt32(filetitle.ContentLength));
                        string fileName = Guid.NewGuid() + ext; // getting File Name
                        string fileExtension = Path.GetExtension(fileName).ToLower();

                        if (Path.GetExtension(fileName).ToLower().Contains(".mp4") || Path.GetExtension(fileName).ToLower().Contains(".webm") || Path.GetExtension(fileName).ToLower().Contains(".ogg"))
                        {
                            SetAlert("Vui lòng chọn file ảnh", "e");
                            return RedirectToAction("PostNew", "PostNewFeed");
                        }
                        else
                        {
                            var result = AssetCRM.isValidFile(tempFileBytes, fileExtension, fileContentType); // Validate Header
                            if (result)
                            {
                                var path = Path.Combine(Server.MapPath("~/Upload/newfeed/"), fileName);
                                linkimg = "/Upload/newfeed/" + fileName;
                                linkimgthumb = "/Upload/thumbnails/newfeed/" + fileName;
                                filetitle.SaveAs(path);
                                filetype = 1;
                                AssetCRM.GenerateThumbNail("~/Upload/newfeed//" + fileName, "~/Upload/thumbnails/newfeed//" + fileName, 150, false);
                            }
                            else
                            {
                                SetAlert("Vui lòng chọn file đại diện", "e");
                                return RedirectToAction("PostNew", "PostNewFeed");
                            }
                        }

                    }
                }
                var title = f.Get("txt-title").ToString();
                var content = f.Get("txt-content").ToString();
                if (string.IsNullOrEmpty(AssetCRM.RemoveHTMLTags(content)))
                {
                    SetAlert("Vui lòng nhập nội dung", "e");
                    return RedirectToAction("PostNew", "PostNewFeed");
                }

                if (string.IsNullOrEmpty(f.Get("hdfID")))
                    PostContentTable.insert(title, linkimg, linkimgthumb, content, aclog.UserName, false, false, groupid, filetype);
                else
                {
                    if (aclog.RoleID != 1 && aclog.RoleID != 2)
                    {
                        var po = PostContentTable.getbyid(f.Get("hdfID").ToInt(0));
                        if (aclog.UserName == po.CreatedBy && po.Status != 3)
                            PostContentTable.update(f.Get("hdfID").ToInt(0), title, linkimg, linkimgthumb, content, aclog.UserName);
                    }
                    else
                        PostContentTable.update(f.Get("hdfID").ToInt(0), title, linkimg, linkimgthumb, content, aclog.UserName);
                }
                SetAlert("Thành công", "s");
                return RedirectToAction("PostNew", "PostNewFeed");
            }
            else
            {
                var bg = f.Get("hdfbg").ToInt(0);
                bool bgt = false;
                if (bg == 0)
                {
                    if (filetitle != null)
                    {
                        string name = Path.GetFileNameWithoutExtension(filetitle.FileName);
                        string ext = Path.GetExtension(filetitle.FileName);
                        if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                        {
                            SetAlert("File của bạn đã bị chặn, vui lòng không tải lên file .php, .asp, .aspx, .ps1", "e");
                            return RedirectToAction("PostNew", "PostNewFeed");
                        }
                        string fileContentType = filetitle.ContentType; // getting ContentType
                        byte[] tempFileBytes = new byte[filetitle.ContentLength];
                        var data = filetitle.InputStream.Read(tempFileBytes, 0, Convert.ToInt32(filetitle.ContentLength));
                        string fileName = Guid.NewGuid() + ext; // getting File Name
                        string fileExtension = Path.GetExtension(fileName).ToLower();
                        if (Path.GetExtension(fileName).ToLower().Contains(".mp4") || Path.GetExtension(fileName).ToLower().Contains(".webm") || Path.GetExtension(fileName).ToLower().Contains(".ogg"))
                        {
                            var path = Path.Combine(Server.MapPath("~/Upload/newfeed/"), fileName);
                            linkimg = "/Upload/newfeed/" + fileName;
                            filetitle.SaveAs(path);
                            filetype = 2;
                        }
                        else
                        {
                            var result = AssetCRM.isValidFile(tempFileBytes, fileExtension, fileContentType); // Validate Header
                            if (result)
                            {
                                var path = Path.Combine(Server.MapPath("~/Upload/newfeed/"), fileName);
                                linkimg = "/Upload/newfeed/" + fileName;
                                linkimgthumb = "/Upload/thumbnails/newfeed/" + fileName;
                                filetitle.SaveAs(path);
                                AssetCRM.GenerateThumbNail("~/Upload/newfeed//" + fileName, "~/Upload/thumbnails/newfeed//" + fileName, 650, true);
                            }
                            else
                            {
                                SetAlert("Vui lòng chọn file đại diện", "e");
                                return RedirectToAction("ProfileUser", "Account", new { area = "Admin" });
                            }
                        }
                    }
                }
                else
                {
                    bgt = true;
                    if (bg == 1)
                        linkimg = "/app-assets/zimv2/assets/img/bg-status1.jpg";
                    if (bg == 2)
                        linkimg = "/app-assets/zimv2/assets/img/bg-status2.jpg";
                    if (bg == 3)
                        linkimg = "/app-assets/zimv2/assets/img/bg-status3.jpg";
                    if (bg == 4)
                        linkimg = "/app-assets/zimv2/assets/img/bg-status4.jpg";
                    if (bg == 5)
                        linkimg = "/app-assets/zimv2/assets/img/bg-status5.jpg";
                    if (bg == 6)
                        linkimg = "/app-assets/zimv2/assets/img/bg-status6.jpg";
                    if (bg == 7)
                        linkimg = "/app-assets/zimv2/assets/img/bg-status7.jpg";
                    if (bg == 8)
                        linkimg = "/app-assets/zimv2/assets/img/bg-status8.jpg";
                    if (bg == 9)
                        linkimg = "/app-assets/zimv2/assets/img/bg-status9.jpg";
                    if (bg == 10)
                        linkimg = "/app-assets/zimv2/assets/img/bg-status10.jpg";
                    if (bg == 11)
                        linkimg = "/app-assets/zimv2/assets/img/bg-status11.jpg";
                    if (bg == 12)
                        linkimg = "/app-assets/zimv2/assets/img/bg-status12.jpg";
                    if (bg == 13)
                        linkimg = "/app-assets/zimv2/assets/img/bg-status13.jpg";
                    if (bg == 14)
                        linkimg = "/app-assets/zimv2/assets/img/bg-status14.jpg";
                    if (bg == 15)
                        linkimg = "/app-assets/zimv2/assets/img/bg-status15.jpg";
                    filetype = 1;
                    linkimgthumb = linkimg;
                }
                var title = f.Get("txt-title").ToString();
                var id = f.Get("hdfid").ToInt(0);
                tbl_Group gr = GroupTable.getbyid(groupid);
                if (gr != null)
                {
                    var member = MemeberTable.getbymenberid(aclog.ID, gr.ID);

                    if (aclog.ID != 1)
                    {
                        if (member != null)
                        {
                            if (member.Status == 2)
                            {
                                SetAlert("Access is denied", "e");
                                if (!string.IsNullOrEmpty(Session["PrePage"].ToString()))
                                {
                                    return Redirect(Session["PrePage"].ToString());
                                }
                                else
                                {
                                    return RedirectToAction("ProfileUser", "Account", new { area = "Admin" });
                                }
                            }
                        }
                        else
                        {
                            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.UserName != gr.CreatedBy)
                            {
                                SetAlert("Access is denied", "e");
                                if (!string.IsNullOrEmpty(Session["PrePage"].ToString()))
                                {
                                    return Redirect(Session["PrePage"].ToString());
                                }
                                else
                                {
                                    return RedirectToAction("ProfileUser", "Account", new { area = "Admin" });
                                }
                            }
                        }
                    }
                }
                if (id == 0)
                    PostContentTable.insert(title, linkimg, linkimgthumb, "", aclog.UserName, true, bgt, groupid, filetype);
                else
                {
                    var p = PostContentTable.getbyid(id);
                    if (p != null)
                    {
                        if (p.CreatedBy == aclog.UserName)
                            PostContentTable.update(p.ID, title, linkimg, linkimgthumb, "", aclog.UserName, true, bgt);
                    }
                }

                if (groupid != 0)
                {
                    SendEmailGroupTable.insert(4, groupid, aclog.ID, 0);
                    Thread sendmail = new Thread(AppZim.Areas.Admin.Controllers.AccountController.SendNotiGroup);
                    sendmail.Start();
                    if (!string.IsNullOrEmpty(Session["PrePage"].ToString()))
                    {
                        return Redirect(Session["PrePage"].ToString());
                    }
                    else
                    {
                        return RedirectToAction("ProfileUser", "Account", new { area = "Admin" });
                    }
                }
                else
                {
                    return RedirectToAction("ProfileUser", "Account", new { area = "Admin" });
                }

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AcceptOffRehect(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var d = PostContentTable.getbyid(f.Get("hdfID").ToInt(0));
            if (d == null)
            {
                SetAlert("Không tìm thấy", "s");
                return RedirectToAction("PostNew", "PostNewFeed");
            }
            PostContentTable.updatestatus(d.ID, f.Get("hdfStatus").ToInt(0), aclog.UserName);
            SetAlert("Thành công", "s");
            return RedirectToAction("PostList", "PostNewFeed");
        }

        public ActionResult EditPost(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var d = PostContentTable.getbyid(id);
            if (d == null)
            {
                SetAlert("Không tìm thấy", "s");
                return RedirectToAction("PostNew", "PostNewFeed");
            }
            if (aclog.RoleID != 1 && aclog.RoleID != 2)
            {
                if (aclog.UserName != d.CreatedBy || d.Status == 3)
                {
                    SetAlert("Bạn không được phép sửa", "s");
                    return RedirectToAction("PostNew", "PostNewFeed");
                }
            }
            ViewBag.ID = d.ID;
            return View();
        }

        [HttpPost]
        public ActionResult ShowVsHidden(int id, int type)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var d = PostContentTable.getbyid(id);
            if (type == 0)
                PostContentTable.updatehideshowstaff(d.ID, !d.isHideStaff.Value, aclog.UserName);
            else
                PostContentTable.updatehideshowstudent(d.ID, !d.isHideStudent.Value, aclog.UserName);
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Xóa bài đăng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeletePost(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 1)
            {
                PostContentTable.deletestatus(id, aclog.UserName);
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var p = PostContentTable.getbyid(id);
                if (p.CreatedBy == aclog.UserName)
                {
                    PostContentTable.deletestatus(id, aclog.UserName);
                    return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Lấy thông tin bài đăng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetPost(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var p = PostContentTable.getbyid(id);
            return Json(new { rs = true, p = p }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Thêm cmt
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cmtid"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        //bình luận bài viết
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult CmtPost(int id, int cmtid, string content)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            tbl_PostContent po = new tbl_PostContent();
            if (id != 0)
                po = PostContentTable.getbyid(id);
            else
            {
                var cmt = PostCommentTable.getbyid(cmtid);
                po = PostContentTable.getbyid(cmt.PostContentID.Value);
            }
            var base64EncodedBytes = System.Convert.FromBase64String(content);
            content = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            StringBuilder ap = new StringBuilder();
            if (!string.IsNullOrEmpty(content))
            {
                var p = PostCommentTable.insert(id, cmtid, aclog.ID, content, aclog.UserName);
                string cmt = "/app-assets/zimv2/images/invis-user.png";
                var acmt = AccountTable.getbyusername(p.CreatedBy);
                if (!string.IsNullOrEmpty(acmt.Avatar))
                {
                    cmt = acmt.Avatar;
                }
                if (id != 0)
                {
                    ap.Append("<li class=\"comment-item-wrap\">");
                    ap.Append(" <div class=\"comment-item\">");
                    ap.Append("     <div class=\"comment-avatar\">");
                    ap.Append("         <a href = \"javascript:;\" class=\"avatar\">");
                    ap.Append("             <img src = \"" + cmt + "\" alt=\"avatar\">");
                    ap.Append("         </a>");
                    ap.Append("     </div>");
                    ap.Append("     <div class=\"comment-content\">");
                    ap.Append("         <div class=\"comment-info\">");
                    ap.Append("             <a href = \"javascript:;\" > " + acmt.FullName + " </a>");
                    ap.Append("             <span class=\"content-cmt\"  style=\"white-space:pre-line;\">" + p.CMT + "</span>");
                    ap.Append("         </div>");
                    ap.Append("         <div class=\"comment-info-hourt\">");
                    ap.Append("             <span class=\"content-cmt-time\">" + p.CreatedDate.Value.ToString("dd/MM/yyyy HH:mm") + "</span>");
                    ap.Append("         </div>");
                    ap.Append("     </div>");
                    ap.Append(" </div>");
                    ap.Append(" </li>");
                }
                else//rep cmt
                {
                    ap.Append("<li class=\"comment-item-wrap\">");
                    ap.Append(" <div class=\"comment-item\">");
                    ap.Append("     <div class=\"comment-avatar\">");
                    ap.Append("         <a href = \"#\" class=\"avatar\">");
                    ap.Append("             <img src=\"" + cmt + "\" alt=\"avatar\" style=\"width:25px; height:25px; min-height: 25px;\">");
                    ap.Append("         </a>");
                    ap.Append("     </div>");
                    ap.Append("     <div class=\"comment-content\">");
                    ap.Append("         <div class=\"comment-info\">");
                    ap.Append("             <a href =\"#\" >" + acmt.FullName + "</a>");
                    ap.Append("             <span class=\"content-cmt\" style=\"white-space:pre-line;\">" + p.CMT + "</span>");
                    ap.Append("         </div>");
                    ap.Append("         <div class=\"comment-info-hourt\">");
                    ap.Append("             <span class=\"content-cmt-time\">" + p.CreatedDate.Value.ToString("dd/MM/yyyy HH:mm") + "</span>");
                    ap.Append("         </div>");
                    ap.Append("     </div>");
                    ap.Append(" </div>");
                    ap.Append("</li>");

                    var lrep = PostCommentTable.getbypostcmtid(cmtid);
                    var userep = lrep.Select(n => n.CreatedBy).Distinct();
                    foreach (var item in userep)
                    {
                        if (item == aclog.UserName)
                            continue;
                        var itemac = AccountTable.getbyusername(item);
                        var nt = NotificationTable.insertsingle("", "Trả lời bình luận", "[ " + aclog.FullName + " ] > " + p.CMT, itemac.ID, aclog.UserName, 11, po.ID);
                    }
                }

            }
            return Json(new { d = ap.ToString() }, JsonRequestBehavior.AllowGet);
        }
    }
}