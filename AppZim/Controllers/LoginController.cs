using AppZim.Areas.Admin.Controllers;
using AppZim.Hubs;
using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using Microsoft.AspNet.SignalR;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppZim.Controllers
{
    public class LoginController : BaseController
    {
        // GET: Login
        [HttpGet]
        public ActionResult Signin()
        {
            if (Request.Cookies["_ckun"] != null)
            {
                if (Response.Cookies["_ckun"].Expires > GetDateTime.Now)
                {
                    string username = Encryptor.Decrypt(Request.Cookies["_ckun"].Value);
                    var ck = AccountTable.getbyusername(username);
                    if (ck != null)
                    {
                        if (ck.Status == 2)
                        {
                            Session["UID"] = ck.ID.ToString();
                        }
                    }
                }
            }


            if (Session["UID"] != null)
            {
                var ck = AccountTable.getbyID(Session["UID"].ToString().ToInt());
                if (ck != null)
                {
                    
                    if (ck.Status == 2)
                    {
                        Session["UID"] = ck.ID.ToString();
                        if (ck.RoleID == 5)
                        {
                            return RedirectToAction("Dashboard", "Home", new { area = "" });
                        }
                        else if (ck.RoleID == 4)
                        {
                            return RedirectToAction("CourseList", "TeacherCourse", new { area = "Admin" });
                        }
                        else if (ck.RoleID == 1)
                        {
                            return RedirectToAction("Dashboard", "Home", new { area = "Admin" });
                        }
                        else if (ck.RoleID == 7)
                        {
                            return RedirectToAction("Dashboard", "Home", new { area = "Admin" });
                        }
                        else
                        {
                            return RedirectToAction("Dashboard", "Home", new { area = "Admin" });
                        }
                    }
                    else
                    {
                        if (ck.Status == 1)
                            ModelState.AddModelError("", "Tài khoản chưa kích hoạt!");
                        else
                            ModelState.AddModelError("", "Tài khoản đang khóa!");
                    }
                }
            }
            return View();
        }
        [HttpGet]
        public ActionResult Signout()
        {
            if (Session["UID"] == null)
                return View("Signin");
            Response.Cookies["_ckun"].Expires = GetDateTime.Now.AddDays(-5);
            AccountTable.updatestatussnline(Session["UID"].ToString().ToInt(0), false);
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ZimRealTime>();
            hubContext.Clients.All.getstatusonline(Session["UID"].ToString().ToInt(0), "avatar-off");
            Session["UID"] = null;
            return View("Signin");
        }
        [HttpPost]
        public ActionResult Signin(FormCollection f)
        {
            string user = f.Get("txt-username").ToString().Trim();
            string pass = f.Get("txt-password").ToString().Trim();
            tbl_Account acc = AccountTable.getbyusername(user);
            if (acc != null)
            {
                var ck = AccountTable.login(user, pass);
                if (ck != null)
                {
                    if (ck.Status == 2)
                    {
                        AccountTable.updatestatussnline(ck.ID, true);
                        var hubContext = GlobalHost.ConnectionManager.GetHubContext<ZimRealTime>();
                        hubContext.Clients.All.getstatusonline(ck.ID, "avatar-online");

                        Session["UID"] = ck.ID.ToString();
                        if (ck.RoleID == 5)
                        {
                            return RedirectToAction("Dashboard", "Home", new { area = "" });
                        }
                        else if (ck.RoleID == 4)
                        {
                            return RedirectToAction("Schedule", "TeacherCourse", new { area = "Admin" });
                        }
                        else if (ck.RoleID == 1)
                        {
                            return RedirectToAction("Dashboard", "Home", new { area = "Admin" });
                        }
                        else if (ck.RoleID == 7)
                        {
                            return RedirectToAction("ScheduleSchool", "Cashier", new { area = "Admin" });
                        }
                        else
                        {
                            return RedirectToAction("Dashboard", "Home", new { area = "Admin" });
                        }
                    }
                    else
                    {
                        if (ck.Status == 1)
                            ModelState.AddModelError("", "Tài khoản chưa kích hoạt!");
                        else
                            ModelState.AddModelError("", "Tài khoản đang khóa!");
                    }
                }
                else
                    ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng!");
            }
            else
                ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng!");
            return View("Signin");
        }
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SetPassword(FormCollection f)
        {
            string email = f.Get("txt-email").ToString();
            var ac = AccountTable.getall().Where(n => n.UserName == email || n.Email == email).FirstOrDefault();
            if (ac != null)
            {
                var code = AppZim.ZIM.AssetCRM.RandomStringWithText(50);
                AccountTable.setkeyforgotpass(ac.ID, code);
                string encodecode = AppZim.ZIM.Encryptor.EncryptURL(code);

                string strPathAndQuery = Request.Url.PathAndQuery;
                string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                string link = "" + strUrl + "set-password/?user=" + ac.UserName + "&code=" + encodecode;

                AppZim.ZIM.AssetCRM.SendMail(ac.Email, "Reset your password", "<a href=\"" + link + "\" target=\"_blank\">Reset your password</a>");
                ModelState.AddModelError("", "Mật khẩu đã được gửi lại email của bạn.");
            }
            else
                ModelState.AddModelError("", "Không tìm thấy!");
            return View("ForgotPassword");
        }
        /// <summary>
        /// Trang đi đến để set mật khẩu mới
        /// </summary>
        /// <param name="user"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SetPass(string user, string code)
        {
            string decode = AppZim.ZIM.Encryptor.DecryptURL(code);
            var ac = AccountTable.checkkeyforgotpass(user, decode);
            if (ac == null)
                return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });

            var time = (GetDateTime.Now - ac.CreatedDateKeyForgot.Value).Minutes;
            if (time > 30)
                return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });
            ViewBag.DataID = ac.ID;
            return View();
        }
        /// <summary>
        /// Tạo mật khẩu mới khi quên
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetPass(FormCollection f)
        {
            var ac = AccountTable.getbyID(f.Get("hdfid").ToInt(0));
            if (ac == null)
                ModelState.AddModelError("", "Fail!");
            else
            {
                AccountTable.setpassnewforgotpass(ac.ID, f.Get("txt-pass-new").ToString());
                ModelState.AddModelError("", "Success!");
            }
            return View("SetPass");
        }
        /// <summary>
        /// Đăng nhập qua link, ko cần mật khẩu
        /// </summary>
        /// <param name="user"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SigninNotPassword(string user, string code)
        {
            string decode = AppZim.ZIM.Encryptor.DecryptURL(code);
            var ac = AccountTable.checkkeyforgotpass(user, decode);
            if (ac == null)
                return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });

            var time = (GetDateTime.Now - ac.CreatedDateKeyForgot.Value).Minutes;
            if (time > 10 || ac.Status != 2)
                return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });

            AccountTable.updatestatussnline(ac.ID, true);
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ZimRealTime>();
            //thông báo trạng thái cho thằng khác
            hubContext.Clients.All.getstatusonline(ac.ID, "avatar-online");
            Session["UID"] = ac.ID.ToString();
            AccountTable.setkeyforgotpass(ac.ID, "");

            HttpCookie cookie = new HttpCookie("_ckun");
            cookie.Value = Encryptor.Encrypt(ac.UserName);
            cookie.Expires = DateTime.Now.AddDays(30);
            Response.Cookies.Add(cookie);


            return RedirectToAction("ProfileUser", "Account", new { area = "Admin" });
        }
    }
}