using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using EmailTemplate = AppZim.ZIM.EmailTemplate;

namespace AppZim.Areas.Admin.Controllers
{
    public class NotificationController : BaseController
    {
        // GET: Admin/Notification
        public ActionResult CreateNotification(int page = 0)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            var l = NotificationTable.GetAllPT();
            if (page == 0)
                page = 1;
            return View(l.ToPagedList(page, pageSize));
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AddNotification(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            List<int> role = new List<int>();
            role = f.Get("ddl-role").ToString().Split(',').Select(x => Convert.ToInt32(x)).ToList();
            bool email = f.Get("hdfemail").ToBool(false);
            string title = f.Get("txt-title").ToString();
            string content = f.Get("hdfcontent").ToString();
            string[] arschool = f.Get("ddl-school").ToString().Split(',');
            List<int> accounts = new List<int>();
            if (string.IsNullOrEmpty(f.Get("slAccount")))
            {
                SetAlert("Vui lòng chọn 1 tài khoản", "e");
                return RedirectToAction("CreateNotification", "Notification");
            }

            accounts = f.Get("slAccount").ToString().Split(',').Select(x => Convert.ToInt32(x)).ToList();

            string roles = "";
            if (!string.IsNullOrEmpty(f.Get("ddl-role")))
            {
                roles = f.Get("ddl-role");
            }

            if (roles.Contains("0"))
                roles = "2,3,4,5,6,7";

            string languages = "";
            if (!string.IsNullOrEmpty(f.Get("slLangue")))
            {
                languages = f.Get("slLangue");
            }

            if (f.Get("slAccount").Contains("0"))
            {
                accounts = AccountTable.GetByRolesAndLanguage(roles, languages).Select(x => x.ID).ToList();
            }
            
            var rs = NotificationTable.insert(title, content, 0, email, aclog.UserName);
            if (rs != null)
            {
                for (int i = 0; i < arschool.Length; i++)
                {
                    NotificationOptionShcoolTable.insert(rs.ID, arschool[i].ToInt(0), aclog.UserName);
                }

                foreach (var item in accounts)
                {
                    var nt = NotificationTable.insertsingle(rs.ID.ToString(), title, content, item, aclog.UserName, 0, 0);
                }

                Thread tnotify = new Thread(() => SendMobile(accounts, "[" + title + "]" + " - " + content));
                tnotify.Start();

                if (rs.PushEmail.Value)
                {
                    var contentEmail = EmailTemplate.Notify(rs.NotificationContent);
                    Thread tmail = new Thread(() => SendMail(accounts, rs.NotificationTitle, contentEmail));
                    tmail.Start();
                }

                SetAlert("Thành công", "s");
                NotificationTable.updatepushdoneparent(rs.ID);
            }
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("CreateNotification", "Notification");
        }

        public ActionResult Notification(int page = 0)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

            int pageSize = 50;
            var l = NotificationTable.GetByUID(aclog.ID).OrderBy(n => n.Status).ThenByDescending(n => n.CreatedDate).ToList();
            if (page == 0)
                page = 1;
            return View(l.ToPagedList(page, pageSize));
        }

        #region upload file imgage notification
        [HttpPost]
        public ActionResult UploadFileImageNoti()
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
                            return Json(new { rs = false, l = "File của bạn không được hỗi trợ" }, JsonRequestBehavior.AllowGet);
                        }
                        var fileName = Guid.NewGuid() + ext;
                        var path = Path.Combine(Server.MapPath("~/Upload/imagenotification/"), fileName);

                        string strPathAndQuery = Request.Url.PathAndQuery;
                        string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");

                        link = "" + strUrl + "Upload/imagenotification/" + fileName;
                        file.SaveAs(path);
                    }
                }
            }
            return Json(new { rs = true, l = link }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Gửi thông báo đến app
        //public ActionResult NotificationToApp()
        //{
        //    using (var db = new ZimEntities())
        //    {
        //        var ck = db.tbl_NotificationSingleLog.ToList();
        //        return View(ck);
        //    }

        //}

        //[HttpPost]
        //public ActionResult NotificationToApp(FormCollection f)
        //{
        //    string content = f.Get("content").ToString();
        //    string title = f.Get("title").ToString();
        //    int GuiChoAi = f.Get("who").ToInt(0);// 0 gửi all 4 gửi giáo viên và học vụ 5 gửi học viên
        //    string AccRoleName = "";
        //    switch (GuiChoAi)
        //    {
        //        case 0:
        //            using (var db = new ZimEntities())
        //            {
        //                var acc = db.tbl_Account.Select(x => new { x.ID }).ToList();
        //                foreach (var item in acc)
        //                {
        //                    var nt = NotificationTable.insertsingle("", title, content, item.ID, "admin", 0, 0);
        //                }
        //            }
        //            AccRoleName = "Tất cả";
        //            break;
        //        case 4:
        //            using (var db = new ZimEntities())
        //            {
        //                var acc = db.tbl_Account.Where(x => x.RoleID == 4 || x.RoleID == 7).Select(x => new { x.ID }).ToList();
        //                foreach (var item in acc)
        //                {
        //                    var nt = NotificationTable.insertsingle("", title, content, item.ID, "admin", 0, 0);
        //                }
        //            }
        //            AccRoleName = "Giáo viên và học vụ";
        //            break;
        //        case 5:
        //            using (var db = new ZimEntities())
        //            {
        //                var acc = db.tbl_Account.Where(x => x.RoleID == 5).Select(x => new { x.ID }).ToList();
        //                foreach (var item in acc)
        //                {
        //                    var nt = NotificationTable.insertsingle("", title, content, item.ID, "admin", 0, 0);
        //                }
        //            }
        //            AccRoleName = "Học viên";
        //            break;
        //    }
        //    // lưu vào lịch sử
        //    using (var db = new ZimEntities())
        //    {
        //        var ck = new tbl_NotificationSingleLog();
        //        ck.AccRoleName = AccRoleName;
        //        ck.Title = title;
        //        ck.Content = content;
        //        ck.CreateBy = "admin";
        //        ck.CreateDate = DateTime.Now;
        //        db.tbl_NotificationSingleLog.Add(ck);
        //        db.SaveChanges();
        //    }
        //    return Redirect(Request.UrlReferrer.AbsoluteUri);
        //}
        #endregion
        public void SendMail(List<int> list, string title, string content)
        {
            try
            {
                using (var db = new ZimEntities())
                {
                    var accounts = db.tbl_Account.Where(x => list.Contains(x.ID) && string.IsNullOrEmpty(x.Email) == false).ToArray();
                    if (accounts.Count() > 0)
                    {
                        var ccList = accounts.Where(x => x.ID != accounts[0].ID).Select(x => x.Email).ToString();
                        AssetCRM.SendMailMultiple(accounts[0].Email, title, content, ccList);
                    }
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("NotificationController", "SendMail", 0, e.Message + "||" + e.InnerException);
            }
        }
        public void SendMobile(List<int> list, string content)
        {
            try
            {
                using (var db = new ZimEntities())
                {
                    var includeID = db.tbl_DeviceMobile.Where(x => list.Contains(x.UID ?? 0)).Select(x => x.IncludePlayerID).ToArray();
                    PushNotifyMobile.SendNotifyMobile(content, includeID);
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("NotificationController", "SendMobile", 0, e.Message + "||" + e.InnerException);
            }
        }
    }
}