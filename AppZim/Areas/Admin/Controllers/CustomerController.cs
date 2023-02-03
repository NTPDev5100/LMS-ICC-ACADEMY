using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using Microsoft.Office.Interop.Excel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebUI.Business;

namespace AppZim.Areas.Admin.Controllers
{
    public class CustomerController : BaseController
    {
        // danh sách học viên
        [HttpGet]
        public ActionResult CustomerList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3 && aclog.RoleID != 6)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                //page size
                int pageSize = 20;

                string search = "";
                ViewBag.Search = Request.QueryString["search"];
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                    search = Request.QueryString["search"].ToString().Trim();

                int status = 0;
                //status
                if (Request.QueryString["status"].ToInt(0) != 0)
                {
                    status = ViewBag.StatusID = Request.QueryString["status"].ToInt(0);
                }

                //study
                int study = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["slStudy"]))
                {
                    study = Request.QueryString["slStudy"].ToInt(0);
                }
                ViewBag.Study = study;
                var l = new List<getallcustomernew_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.getallcustomernew(search, status, study, (page - 1) * pageSize, pageSize).ToList();
                }

                int total = 0;
                if (l.Count > 0)
                    total = l[0].TotalRow.Value;
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

                AssetCRM.Writelog("CustomerController", "CustomerList", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }
        // tạo mưới học viên
        public ActionResult CreateCustomer()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {

                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                return View();
            }
            catch (Exception ex)
            {

                AssetCRM.Writelog("CustomerController", "CreateCustomer", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// Thêm mới học viên
        /// </summary>
        /// <returns></returns>
        public ActionResult AddCustomer()
        {
            return View();
        }

        // học viên nợ học phí
        public ActionResult CustomerDebts(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {

                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                string search = "";
                ViewBag.Search = Request.QueryString["search"];
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                    search = Request.QueryString["search"].ToString().Trim();
                //center
                int center = 0;
                if (Request.QueryString["center"].ToInt(0) != 0)
                {
                    center = ViewBag.CenterID = Request.QueryString["center"].ToInt(0);
                }
                //sắp xếp
                int sort = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
                {
                    sort = Request.QueryString["sort"].ToInt(0);
                }
                ViewBag.Sort = sort;
                // lấy ngày đầu và ngày cuối tháng
                DateTime date = DateTime.Now;
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                // thời gian bắt đầu từ
                string fromdate = firstDayOfMonth.ToString("dd/MM/yyyy");
                string todate = lastDayOfMonth.ToString("dd/MM/yyyy");
                string fromdateleft = firstDayOfMonth.ToString("dd/MM/yyyy");
                string todateleft = lastDayOfMonth.ToString("dd/MM/yyyy");
                ViewBag.FromLeft = firstDayOfMonth.ToString("dd/MM/yyyy");
                ViewBag.ToLeft = lastDayOfMonth.ToString("dd/MM/yyyy");
                ViewBag.From = firstDayOfMonth.ToString("dd/MM/yyyy");
                ViewBag.To = lastDayOfMonth.ToString("dd/MM/yyyy");
                if (!string.IsNullOrEmpty(Request.QueryString["fromleft"]))
                    try
                    {
                        DateTime fdateleft = DateTime.ParseExact(Request.QueryString["fromleft"].ToString(), "dd/MM/yyyy", null);
                        ViewBag.FromLeft = fdateleft.ToString("dd/MM/yyyy");
                        fromdateleft = fdateleft.ToString("dd/MM/yyyy");
                    }
                    catch { }
                // đến
                if (!string.IsNullOrEmpty(Request.QueryString["toleft"]))
                    try
                    {
                        DateTime tdateleft = DateTime.ParseExact(Request.QueryString["toleft"].ToString(), "dd/MM/yyyy", null);
                        ViewBag.ToLeft = tdateleft.ToString("dd/MM/yyyy");
                        todateleft = tdateleft.ToString("dd/MM/yyyy");
                    }
                    catch { }

                // thời gian bắt đầu từ
                if (!string.IsNullOrEmpty(Request.QueryString["from"]))
                {
                    try
                    {
                        DateTime fdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                        ViewBag.From = fdate.ToString("dd/MM/yyyy");
                        fromdate = fdate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                // đến
                if (!string.IsNullOrEmpty(Request.QueryString["to"]))
                {
                    try
                    {
                        DateTime tdate = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null);
                        ViewBag.To = tdate.ToString("dd/MM/yyyy");
                        todate = tdate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }

                int pageSize = 20;
                List<get_list_of_CustomerDebts_Result> l = new List<get_list_of_CustomerDebts_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.get_list_of_CustomerDebts(search, center, fromdateleft, todateleft, fromdate, todate, sort, pageSize, page - 1).ToList();
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

                AssetCRM.Writelog("CustomerController", "CustomerDebts", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        //thanh toán học phí
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult PaidDebts(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var cs = CoursesStudentTable.getbyid(f.Get("hdfID").ToInt(0));
                if (cs == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "e");
                    return RedirectToAction("CustomerDebts", "Customer");
                }
                var cource = CoursesTable.getbyid(cs.CourseID.Value);
                var choolinvoice = SchoolTable.getbyID(f.Get("ddl-school").ToInt(0));
                if (choolinvoice == null)
                {
                    SetAlert("Không tìm thấy trung tâm tạo phiếu", "e");
                    return RedirectToAction("CustomerDebts", "Customer");
                }

                int payment = f.Get("ddl-payment").ToInt(1);
                string paymentname = CashierController.Paymentmethod(payment);
                string dateleft = f.Get("txt-date-left").ToString().Replace(" ", "");
                var stu = AccountTable.getbyID(cs.StudentUID.Value);
                int price = f.Get("txt-paid").ToString().Replace(",", "").ToInt(0);

                string note = f.Get("txt-ghi-chu").ToString();
                string ghichu = "";
                if (!string.IsNullOrEmpty(note))
                    ghichu += " [Ghi chú: " + note + "]";

                if (price > 0)
                {


                    //cập nhật dữ liệu khóa
                    var sc = CoursesStudentTable.updatepaiddebts(cs.ID, price, dateleft, aclog.UserName);
                    var cus = AccountTable.getbyID(sc.StudentUID.Value);
                    if (sc != null)
                    {

                        if (sc.ComboID != null && sc.ComboID != 0)
                        {

                            ComboTable.UpdatePriceLeft(sc.ComboID.Value, price, aclog.UserName);
                            string name = "6-" + sc.ComboID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";//6 cọc thêm thuộc combo
                            string nameencode = AppZim.ZIM.Encryptor.EncryptURL(name);
                            string strPathAndQuery = Request.Url.PathAndQuery;
                            string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                            string datalink = "" + strUrl + "invoice/?code=" + nameencode;
                            var barcode = AppZim.ZIM.AssetCRM.CreateQRCode(datalink, name);
                            string gcpt = "";
                            if (!string.IsNullOrEmpty(ghichu))
                                gcpt += " [Ghi chú: " + ghichu + "]";

                            //tbl_Invoices iv = new tbl_Invoices();
                            ////đăng ký combo                      
                            //iv = InvoicesTable.insert(false, 0, 0, 0, 0, sc.ComboID.Value, cus, price, ComboTable.GetCurrentPriceLeft(sc.ComboID.Value), ghichu, barcode, nameencode, aclog, dateleft, payment, paymentname, choolinvoice, 0);
                            var iv = InvoicesTable.insert(0, false, sc.ID, 0, 0, 0, sc.ComboID.Value, cus, price, ComboTable.GetCurrentPriceLeft(sc.ComboID.Value), gcpt, "", barcode, nameencode, aclog, dateleft, payment, paymentname, choolinvoice, 0);
                            //lấy danh sách các cái khác thuộc combo
                            List<tbl_ComboDetail> lbdetail = ComboDetailTable.getbycomboid(sc.ComboID.Value);
                            foreach (var item in lbdetail)
                            {
                                if (item.TypeID == 1)
                                {
                                    tbl_CoursesStudent coursestudent = CoursesStudentTable.getbyCourseIDdandStudentID(item.CourseID.Value, cus.ID);
                                    tbl_InvoiceDetail ivd = InvoiceDetailTable.insert(iv.ID, coursestudent.ComboID.Value, coursestudent.CourseID.Value, 1, coursestudent.PriceCourse.Value, aclog.UserName);
                                }
                                else
                                {
                                    tbl_StudentAppointment stapp = StudentAppointmentTable.getbycomboclass(item.ComboID.Value, item.CourseID.Value);
                                    if (stapp.isHide.Value)
                                    {
                                        tbl_CoursesStudent coursestudent = CoursesStudentTable.getbystudentid(cus.ID).Where(n => n.AppointmentID == stapp.ID).FirstOrDefault();
                                        tbl_InvoiceDetail ivd = InvoiceDetailTable.insert(iv.ID, coursestudent.ComboID.Value, coursestudent.ID, 3, coursestudent.PriceCourse.Value, aclog.UserName);
                                    }
                                    else
                                    {
                                        tbl_InvoiceDetail ivd = InvoiceDetailTable.insert(iv.ID, stapp.ComboID.Value, stapp.ClassID.Value, 2, stapp.ClassPrice.Value, aclog.UserName);
                                    }
                                }
                            }
                            //đẩy thông báo cho admin
                            var ladmin = AccountTable.getbyrole(1).Where(n => n.Status == 2 && n.ID == 1).ToList();
                            foreach (var item in ladmin)
                            {
                                string title = "[" + choolinvoice.SchoolName + "] - Phiếu thu " + String.Format("{0:0,0 VNĐ}", iv.Price) + "";

                                string content = "Phát sinh phiếu thu có ID = \"" + iv.ID;
                                var nt = NotificationTable.insertsingle("", title, content, item.ID, aclog.UserName, 9, 0);

                                // gửi mail
                                if (!string.IsNullOrEmpty(item.Email) && item.ID == 1)
                                    AppZim.ZIM.AssetCRM.SendMail(item.Email, nt.NotificationTitle, nt.NotificationContent);
                            }
                            //==============>>>>

                            SetlinkInvoice(datalink);
                        }
                        else
                        {
                            string name = "3-" + cs.ID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";//3 thanh toán số tiền còn lại
                            string nameencode = AppZim.ZIM.Encryptor.EncryptURL(name);
                            string strPathAndQuery = Request.Url.PathAndQuery;
                            string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                            string datalink = "" + strUrl + "invoice/?code=" + nameencode;
                            var barcode = AppZim.ZIM.AssetCRM.CreateQRCode(datalink, name);
                            var iv = InvoicesTable.insert(0, false, cs.ID, 0, 0, 0, 0, stu, price, sc.PriceLeft.Value, cs.CourseName + ghichu, "", barcode, nameencode, aclog, dateleft, payment, paymentname, choolinvoice, 0);
                            //đẩy thông báo cho admin
                            var ladmin = AccountTable.getbyrole(1).Where(n => n.Status == 2 && n.ID == 1).ToList();
                            foreach (var item in ladmin)
                            {
                                string title = "[" + choolinvoice.SchoolName + "] - Phiếu thu " + String.Format("{0:0,0 VNĐ}", iv.Price) + "";

                                string content = "Phát sinh phiếu thu có ID = \"" + iv.ID;
                                var nt = NotificationTable.insertsingle("", title, content, item.ID, aclog.UserName, 9, 0);
                                // gửi mail
                                if (!string.IsNullOrEmpty(item.Email) && item.ID == 1)
                                    AppZim.ZIM.AssetCRM.SendMail(item.Email, nt.NotificationTitle, nt.NotificationContent);
                            }
                            //==============>>>>
                            SetlinkInvoice(datalink);
                        }

                    }
                }
                SetAlert("Thành công", "s");
                return RedirectToAction("CustomerDebts", "Customer");
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("CustomerController", "PaidDebts", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        // học viên chi tiết
        public ActionResult CustomerDetai(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {

                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên,giaos vien bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });
                if (aclog.RoleID == 4)
                {
                    return RedirectToAction("WarningDetail", "Customer", new { id = id });
                }
                var t = AccountTable.getbyID(id);
                if (t == null)
                {
                    SetAlert("Không tìm thấy học viên này :(", "e");
                    return RedirectToAction("CustomerList", "Customer");
                }
                else if (t.RoleID != 5)// học viên
                {
                    SetAlert("Không tìm thấy học viên này :(", "e");
                    return RedirectToAction("CustomerList", "Customer");
                }
                bool check = false;
                SqlParameter[] parameters =
                    {
                      new SqlParameter("@aclogID", SqlDbType.Int) { Value = aclog.ID },
                      new SqlParameter("@roleID", SqlDbType.Int) { Value = aclog.RoleID },
                      new SqlParameter("@studentID", SqlDbType.Int) { Value = id },
                };
                var rs = SqlHelper.ExecuteDataTable("check_permission", parameters);
                check = Convert.ToBoolean((from rw in rs.AsEnumerable() select rw["KT"]).FirstOrDefault().ToString().ToInt(0));
                if (!check)
                {
                    SetAlert("Xin lỗi bạn không có quyền xem thông tin học viên này", "e");
                    if (HttpContext.Request.UrlReferrer != null)
                    {
                        return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
                    }
                    else
                    {
                        return RedirectToAction("CustomerList", "Customer", new { area = "Admin" });
                    }
                }
                return View(t);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("CustomerController", "CustomerDetai", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        // học viên hẹn đăng ký
        public ActionResult Appointment(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {

                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });


                string search = "";
                ViewBag.Search = Request.QueryString["search"];
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                    search = Request.QueryString["search"].ToString().Trim();

                //lớp
                int lop = 0;
                if (Request.QueryString["lop"].ToInt(0) != 0)
                {
                    lop = ViewBag.LopID = Request.QueryString["lop"].ToInt(0);
                }
                //ca
                int ca = 0;
                if (Request.QueryString["ca"].ToInt(0) != 0)
                {
                    ca = ViewBag.CaID = Request.QueryString["ca"].ToInt(0);
                }
                //language
                int language = 0;
                if (Request.QueryString["slLanguage"].ToInt(0) != 0)
                {
                    language = ViewBag.Language = Request.QueryString["slLanguage"].ToInt(0);
                }

                // thời gian bắt đầu từ
                // lấy ngày đầu và ngày cuối tháng
                DateTime date = DateTime.Now;
                var firstDayOfMonth = DateTime.Now.FirstDayOfMonth();
                var lastDayOfMonth = DateTime.Now.LastDayOfMonth();

                string fromdate = firstDayOfMonth.ToString("yyyy/MM/dd");
                ViewBag.From = firstDayOfMonth.ToString("dd/MM/yyyy");
                if (!string.IsNullOrEmpty(Request.QueryString["from"]))
                {
                    try
                    {
                        DateTime fdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                        ViewBag.From = fdate.ToString("dd/MM/yyyy");
                        fromdate = fdate.ToString("yyyy/MM/dd");
                    }
                    catch { }
                }
                string todate = lastDayOfMonth.ToString("yyyy/MM/dd");
                ViewBag.To = lastDayOfMonth.ToString("dd/MM/yyyy");
                // đến
                if (!string.IsNullOrEmpty(Request.QueryString["to"]))
                {
                    try
                    {
                        DateTime tdate = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null);
                        ViewBag.To = tdate.ToString("dd/MM/yyyy");
                        todate = tdate.ToString("yyyy/MM/dd");
                    }
                    catch { }
                }
                int pageSize = 80;
                List<get_list_of_student_appointment_Result> l = new List<get_list_of_student_appointment_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.get_list_of_student_appointment(search, lop, ca, fromdate, todate, pageSize, page - 1, aclog.ID, aclog.RoleID, language).ToList();
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
                AssetCRM.Writelog("CustomerController", "Appointment", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        //Cập nhật thông tin khách hàng và thông tin đằng ký
        //Cập nhật thông tin đăng ký v2
        [HttpPost]
        public ActionResult UpdateAppointmentNotProfile(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var appoint = StudentAppointmentTable.getbyid(f.Get("hdfAppointID").ToInt(0));
                if (appoint == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "e");
                    return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
                }
                var cus = AccountTable.getbyID(appoint.StudentUID.Value);
                if (cus != null)
                {
                    var diadiem = LocationTable.getbyID(f.Get("ddl-city").ToInt(0));
                    if (diadiem == null)
                    {
                        diadiem = new Models.tbl_City();
                        diadiem.ID = 0;
                        diadiem.CityName = "";
                    }
                    var lop = "";
                    if (!string.IsNullOrEmpty(f.Get("ddl-class")))
                    {
                        lop = f.Get("ddl-class").ToString();
                    }

                    int lesson = 0;
                    if (!string.IsNullOrEmpty(f.Get("txt-lesson")))
                    {
                        lesson = f.Get("txt-lesson").ToString().ToInt(0);
                    }

                    var trungtam = SchoolTable.getbyID(f.Get("ddl-school").ToInt(0));
                    if (trungtam == null)
                    {
                        trungtam = new Models.tbl_School();
                        trungtam.ID = 0;
                        trungtam.SchoolName = "";
                    }
                    var cahoc = StudyTimeTable.getbyid(f.Get("ddl-study").ToInt(0));
                    if (cahoc == null)
                    {
                        cahoc = new Models.tbl_StudyTime();
                        cahoc.ID = 0;
                        cahoc.StudyTimeName = "";
                    }

                    string datedeposit = "";

                    if (appoint != null)
                    {
                        if (appoint.DateAppointment != null)
                            datedeposit = appoint.DateAppointment.Value.ToString("dd/MM/yyyy");
                    }
                    if (!string.IsNullOrEmpty(f.Get("txt-appointment-date").ToString()))
                    {
                        try
                        {
                            datedeposit = DateTime.ParseExact(f.Get("txt-appointment-date").ToString().Replace(" ", ""), "dd/MM/yyyy", null).ToString("dd/MM/yyyy");
                        }
                        catch { }
                    }

                    string noteappoint = f.Get("txt-note-appoment").ToString();

                    if (appoint != null)
                    {
                        int[] lops = lop.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
                        if (lops.Count() == 1)
                        {
                            var classobject = ClassTable.getbyID(lops[0]);
                            appoint = StudentAppointmentTable.update(appoint.ID, cus, diadiem, trungtam, classobject, cahoc, 0, noteappoint,
                                                        appoint.PaymentMethodID.Value, appoint.PaymentMethodName, aclog.UserName, datedeposit, lesson);
                        }
                        else
                        {
                            var comboDB = ComboDetailTable.getbycomboid(appoint.ComboID ?? 0).Select(x => x.ClassID ?? 0).ToArray();
                            var inserts = lops.Except(comboDB).ToList();
                            var deletes = comboDB.Except(lops).ToList();
                            foreach (var i in inserts)
                            {
                                ComboDetailTable.insert(appoint.ComboID ?? 0, i, aclog.UserName);
                            }

                            foreach (var d in deletes)
                            {
                                ComboDetailTable.Delete(appoint.ComboID ?? 0, d, aclog.UserName);
                            }
                        }
                    }

                    ////thêm tiền
                    //int payment = f.Get("ddl-payment-deposit-add").ToInt(1);
                    //string paymentname = CashierController.Paymentmethod(payment);
                    //int desposit = f.Get("txt-deposit-add").Replace(",", "").ToInt(0);
                    //var trungtampaid = SchoolTable.getbyID(f.Get("ddl-school-deposit-add").ToInt(0));
                    //if (appoint.ComboID != null && appoint.ComboID != 0)
                    //{
                    //    ComboTable.UpdatePriceLeft(appoint.ComboID.Value, desposit, aclog.UserName);
                    //}
                    //cập nhật ngôn ngữ
                    int languageID = f.Get("ddl-language-deposit-add").ToInt(0);
                    using (var db = new ZimEntities())
                    {
                        var account = db.tbl_Account.SingleOrDefault(x => x.ID == appoint.StudentUID);
                        if (languageID != account.Language) account.Language = languageID;
                        db.SaveChanges();
                    }
                    //if (desposit > 0)
                    //{
                    //    if (appoint.CloseAppointment == 1)
                    //    {
                    //        SetAlert("Đăng ký đã hủy, không thể cọc thêm", "e");
                    //        if (appoint.CourseStudentID == 0)
                    //            return RedirectToAction("Appointment", "Customer");
                    //        else
                    //            return RedirectToAction("CustomerReserve", "Customer");
                    //    }
                    //    if (trungtampaid == null)
                    //    {
                    //        SetAlert("Vui lòng chọn trung tâm thu tiền", "e");
                    //        if (appoint.CourseStudentID == 0)
                    //            return RedirectToAction("Appointment", "Customer");
                    //        else
                    //            return RedirectToAction("CustomerReserve", "Customer");
                    //    }
                    //    //cập nhật thêm số tiền cọc hẹn
                    //    var rs = StudentAppointmentTable.update(appoint.ID, cus, diadiem, trungtam, lop, cahoc, desposit, noteappoint,
                    //       payment, paymentname, aclog.UserName, "");

                    //    if (rs != null)
                    //    {

                    //        if (rs.ComboID != null && rs.ComboID != 0)
                    //        {

                    //            string name = "6-" + rs.ComboID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";//6 cọc thêm thuộc combo
                    //            string nameencode = AppZim.ZIM.Encryptor.EncryptURL(name);
                    //            string strPathAndQuery = Request.Url.PathAndQuery;
                    //            string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                    //            string datalink = "" + strUrl + "invoice/?code=" + nameencode;
                    //            var barcode = AppZim.ZIM.AssetCRM.CreateQRCode(datalink, name);
                    //            string ghichuphieuthu = f.Get("txt-note-paid-appoment").ToString();
                    //            string gcpt = "";
                    //            if (!string.IsNullOrEmpty(ghichuphieuthu))
                    //                gcpt += " [Ghi chú: " + ghichuphieuthu + "]";

                    //            //tbl_Invoices iv = new tbl_Invoices();
                    //            //đăng ký combo                      
                    //            var iv = InvoicesTable.insert(0, false, 0, rs.ID, 0, 0, rs.ComboID.Value, cus, desposit, ComboTable.GetCurrentPriceLeft(rs.ComboID.Value), gcpt, "", barcode, nameencode, aclog, datedeposit, payment, paymentname, trungtam, 0);
                    //            //lấy danh sách các cái khác thuộc combo
                    //            List<tbl_ComboDetail> lbdetail = ComboDetailTable.getbycomboid(rs.ComboID.Value);
                    //            foreach (var item in lbdetail)
                    //            {
                    //                if (item.TypeID == 1)
                    //                {
                    //                    tbl_CoursesStudent coursestudent = CoursesStudentTable.getbyCourseIDdandStudentID(item.CourseID.Value, cus.ID);
                    //                    tbl_InvoiceDetail ivd = InvoiceDetailTable.insert(iv.ID, coursestudent.ComboID.Value, coursestudent.CourseID.Value, 1, coursestudent.PriceCourse.Value, aclog.UserName);
                    //                }
                    //                else
                    //                {
                    //                    tbl_StudentAppointment stapp = StudentAppointmentTable.getbycomboclass(item.ComboID.Value, item.CourseID.Value);
                    //                    if (stapp.isHide.Value)
                    //                    {
                    //                        tbl_CoursesStudent coursestudent = CoursesStudentTable.getbystudentid(cus.ID).Where(n => n.AppointmentID == stapp.ID).FirstOrDefault();
                    //                        tbl_InvoiceDetail ivd = InvoiceDetailTable.insert(iv.ID, coursestudent.ComboID.Value, coursestudent.ID, 3, coursestudent.PriceCourse.Value, aclog.UserName);
                    //                    }
                    //                    else
                    //                    {
                    //                        tbl_InvoiceDetail ivd = InvoiceDetailTable.insert(iv.ID, stapp.ComboID.Value, stapp.ClassID.Value, 2, stapp.ClassPrice.Value, aclog.UserName);
                    //                    }
                    //                }
                    //            }
                    //            var ladmin = AccountTable.getbyrole(1).Where(n => n.Status == 2 && n.ID == 1).ToList();
                    //            foreach (var item in ladmin)
                    //            {
                    //                string title = "[" + trungtampaid.SchoolName + "] - Phiếu thu " + String.Format("{0:0,0 VNĐ}", iv.Price) + "";

                    //                string content = "Phát sinh phiếu thu có ID = \"" + iv.ID;
                    //                var nt = NotificationTable.insertsingle("", title, content, item.ID, aclog.UserName, 9, 0);
                    //                // gửi mail
                    //                if (!string.IsNullOrEmpty(item.Email) && item.ID == 1)
                    //                    AppZim.ZIM.AssetCRM.SendMail(item.Email, nt.NotificationTitle, nt.NotificationContent);
                    //            }
                    //            //==============>>>>

                    //            SetlinkInvoice(datalink);
                    //        }
                    //        else
                    //        {
                    //            string name = "5-" + appoint.ID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";//5 cọc thêm
                    //            string nameencode = AppZim.ZIM.Encryptor.EncryptURL(name);
                    //            string strPathAndQuery = Request.Url.PathAndQuery;
                    //            string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                    //            string datalink = "" + strUrl + "invoice/?code=" + nameencode;
                    //            var barcode = AppZim.ZIM.AssetCRM.CreateQRCode(datalink, name);

                    //            string ghichuphieuthu = f.Get("txt-note-paid-appoment").ToString();
                    //            string gcpt = "";
                    //            if (!string.IsNullOrEmpty(ghichuphieuthu))
                    //                gcpt += " [Ghi chú: " + ghichuphieuthu + "]";
                    //            var iv = InvoicesTable.insert(0, false, 0, appoint.ID, 0, 0, 0, cus, desposit, 0, "Đóng thêm tiền giữ chỗ" + gcpt, "", barcode, nameencode, aclog, "", payment, paymentname, trungtampaid, 0);

                    //            //đẩy thông báo cho admin
                    //            var ladmin = AccountTable.getbyrole(1).Where(n => n.Status == 2 && n.ID == 1).ToList();
                    //            foreach (var item in ladmin)
                    //            {
                    //                string title = "[" + trungtampaid.SchoolName + "] - Phiếu thu " + String.Format("{0:0,0 VNĐ}", iv.Price) + "";

                    //                string content = "Phát sinh phiếu thu có ID = \"" + iv.ID;
                    //                var nt = NotificationTable.insertsingle("", title, content, item.ID, aclog.UserName, 9, 0);
                    //                // gửi mail
                    //                if (!string.IsNullOrEmpty(item.Email) && item.ID == 1)
                    //                    AppZim.ZIM.AssetCRM.SendMail(item.Email, nt.NotificationTitle, nt.NotificationContent);
                    //            }
                    //            //==============>>>>
                    //            SetlinkInvoice(datalink);
                    //        }

                    //    }
                    //}

                    SetAlert("Thành công", "s");
                }
                return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("CustomerController", "Appointment", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getlanguageOfStudent(int accountID)
        {
            try
            {
                int languageID = (int)AccountTable.getbyID(accountID).Language;
                return Json(new { rs = true, languageID = languageID });
            }
            catch { return Json(new { rs = false }); }

        }

        //đẩy khách đăng ký dữ chỗ vào khóa
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AssignCusAppointmentToCourse(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var app = StudentAppointmentTable.getbyid(f.Get("hdfAsignAppointID").ToInt(0));
                if (app == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "e");
                    return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
                }
                if (app.CloseAppointment == 1)
                {
                    SetAlert("Lịch chờ xếp lớp đã bị hủy", "e");
                    return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
                }

                var course = CoursesTable.getbyid(f.Get("ddl-cource").ToInt(0));
                if (course == null)
                {
                    SetAlert("Không tìm thấy khóa học", "e");
                    return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
                }

                var commitmentpackage = ClassTable.getbyID(app.CommitmentID ?? 0);
                if (commitmentpackage == null)
                {
                    commitmentpackage = new tbl_Class() { ID = 0, ClassName = "", PriceClass = 0 };
                }

                var stu = AccountTable.getbyID(app.StudentUID.Value);

                tbl_CoursesStudent r = CoursesStudentTable.insert(app.RewardTeachers ?? 0, stu, course, app.CustomPrice ?? 0,
                        app.ID, app.CustomPrice ?? 0, app.Note, app.PaymentMethodID ?? 0, app.PaymentMethodName,
                        aclog.UserName, 0, true, app.DateAppointment == null ? "" : app.DateAppointment.Value.ToString("dd/MM/yyyy"),
                        "", 0, app.ComboID ?? 0, commitmentpackage, app.DiscountID ?? 0, app.DiscountPrice ?? 0);

                if (r != null)
                {
                    if (app.ComboID > 0)// combo
                    {
                        ComboDetailTable.UpdateDone(app.ID, app.ComboID ?? 0, course.ClassID ?? 0, aclog.UserName, 0);
                    }
                    else
                    {
                        StudentAppointmentTable.updatedone(app.ID, aclog.UserName);
                    }

                    string codex = "";
                    if (DeviceMobileTable.getbyUID(r.StudentUID.Value) != null)
                    {
                        codex = DeviceMobileTable.getbyUID(r.StudentUID.Value).IncludePlayerID;
                    }
                    string[] includeID = new[] { codex };
                    PushNotifyMobile.SendNotifyMobile("Bạn được thêm vào khóa học mới" + r.CourseName, includeID);

                    int count = CoursesStudentTable.getbycourseid(r.CourseID.Value).Where(n => n.isHide != true).ToList().Count;
                    CoursesTable.updatecountstudent(course.ID, count, aclog.UserName);
                    var first = CoursesScheduleTable.GetFirstLesson(course.ID);
                    var time = StudyTimeTable.getbyid(first.StudyTimeID.Value);
                    string tt = f.Get("hdtt").ToString();
                    string ct = f.Get("hdct").ToString();
                    // gửi mail theo form
                    if (!string.IsNullOrEmpty(stu.Email))
                    {
                        ct = ct.Replace("[ten-hoc-vien]", stu.FullName);
                        ct = ct.Replace("[thoi-gian-hoc]", time.sTime + " - " + time.eTime);
                        ct = ct.Replace("[tai-khoan]", stu.UserName);
                        ct = ct.Replace("[mat-khau]", "iccacademy");
                        AssetCRM.SendMail(stu.Email, tt, ct);
                    }
                }
                else
                    SetAlert("Không thành công", "e");
                if (app.CourseStudentID == 0)
                    return RedirectToAction("Appointment", "Customer");
                else
                    return RedirectToAction("CustomerReserve", "Customer");
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("CustomerController", "AssignCusAppointmentToCourse", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }


        //hủy lịch hẹn
        [HttpPost]
        public ActionResult CloseCusAppointmentToCourse(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {

                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var app = StudentAppointmentTable.getbyid(f.Get("hdfCloseAppointID").ToInt(0));
                if (app == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "e");
                    return RedirectToAction("Appointment", "Customer");
                }

                string note = f.Get("txt-note-close").ToString();
                var rs = StudentAppointmentTable.updateclose(app.ID, note, aclog.UserName);
                if (rs.ComboID != null && rs.ComboID != 0)
                {
                    ComboTable.UpdateTotalPrice(rs.ComboID.Value, rs.ClassPrice.Value * (-1), aclog.UserName);
                    ComboTable.UpdatePriceLeft(rs.ComboID.Value, rs.Deposit.Value * (-1), aclog.UserName);
                }
                if (rs != null)
                    SetAlert("Thành công", "s");
                else
                    SetAlert("Không thành công", "e");
                if (app.CourseStudentID == 0)
                    return RedirectToAction("Appointment", "Customer");
                else
                    return RedirectToAction("CustomerReserve", "Customer");
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("CustomerController", "CloseCusAppointmentToCourse", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        //yêu cầu hoàn tiền hẹn dữ chỗ
        [HttpPost]
        public ActionResult RefundAppointment(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {

                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var app = StudentAppointmentTable.getbyid(f.Get("hdfRefundAppointID").ToInt(0));
                if (app == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "e");
                    if (app.CourseStudentID == 0)
                        return RedirectToAction("Appointment", "Customer");
                    else
                        return RedirectToAction("CustomerReserve", "Customer");
                }
                var ck = RefundsTable.getbyappointmentid(app.ID);
                if (ck != null)
                {
                    SetAlert("Đã gửi yêu cầu cho admin, không thể gửi thêm", "e");
                    if (app.CourseStudentID == 0)
                        return RedirectToAction("Appointment", "Customer");
                    else
                        return RedirectToAction("CustomerReserve", "Customer");
                }
                var cus = AccountTable.getbyID(app.StudentUID.Value);
                if (cus == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "e");
                    if (app.CourseStudentID == 0)
                        return RedirectToAction("Appointment", "Customer");
                    else
                        return RedirectToAction("CustomerReserve", "Customer");
                }
                string note = f.Get("txt-note-refund").ToString();
                int price = f.Get("txt-refund").ToString().Replace(",", "").ToInt(0);
                if (price == 0 || price > app.Deposit)
                {
                    SetAlert("Vui lòng nhập số tiền đúng <= số tiền đã đóng", "e");
                    if (app.CourseStudentID == 0)
                        return RedirectToAction("Appointment", "Customer");
                    else
                        return RedirectToAction("CustomerReserve", "Customer");
                }
                var chool = SchoolTable.getbyID(f.Get("ddl-school-refund").ToInt(0));
                if (chool == null)
                {
                    SetAlert("Vui lòng nhập số tiền", "e");
                    if (app.CourseStudentID == 0)
                        return RedirectToAction("Appointment", "Customer");
                    else
                        return RedirectToAction("CustomerReserve", "Customer");
                }

                bool kichout = true;
                //if (!string.IsNullOrEmpty(f.Get("chk-kick-out")))
                //    kichout = true;

                var rs = RefundsTable.insert(0, app.ID, 0, cus, price, note, chool, kichout, aclog.UserName);
                if (rs != null)
                {
                    // gửi thông báo cho admin               
                    var ladmin = AccountTable.getbyrole(1).Where(n => n.Status == 2 && n.ID == 1).ToList();
                    string strPathAndQuery = Request.Url.PathAndQuery;
                    string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                    string datalink = "" + strUrl + "Admin/Cashier/RefundList";
                    foreach (var item in ladmin)
                    {
                        string title = "[" + chool.SchoolName + "] - Yêu cầu hoàn tiền " + String.Format("{0:0,0 VNĐ}", rs.Price) + "";

                        string content = "Phát sinh yêu cầu hoàn tiền có ID = \"" + rs.ID;
                        var nt = NotificationTable.insertsingle("", title, content, item.ID, aclog.UserName, 9, 0);
                        // gửi mail
                        if (!string.IsNullOrEmpty(item.Email))
                            AppZim.ZIM.AssetCRM.SendMail(item.Email, nt.NotificationTitle, nt.NotificationContent);
                    }
                    SetAlert("Thành công", "s");
                }
                else
                    SetAlert("Không thành công", "e");
                if (app.CourseStudentID == 0)
                    return RedirectToAction("Appointment", "Customer");
                else
                    return RedirectToAction("CustomerReserve", "Customer");
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("CustomerController", "RefundAppointment", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]// lấy thông tin hẹn đăng ký về refund
        public ActionResult CheckRefundAppointmentByID(int ID)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var rs = StudentAppointmentTable.getbyid(ID);
                if (rs == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                else
                {
                    var ck = RefundsTable.getbyappointmentid(rs.ID);
                    if (ck == null)
                        return Json(new { rs = true, fullname = rs.StudentName, deposit = String.Format("{0:0,0}", rs.Deposit) }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                int id = 0;
                if (Session["UID"] != null)
                    id = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "CheckRefundAppointmentByID", id, ex.Message);
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        ///// <summary>
        ///// Kiếm tra lịch hẹn đăng kí
        ///// </summary>
        ///// <param name="ID"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult CheckAppointment(int ID)
        //{
        //    try
        //    {
        //        var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
        //        var rs = StudentAppointmentTable.getbyid(ID);
        //        if (rs == null)
        //            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        //        else
        //        {
        //            if (rs.ComboID != null)
        //            {
        //                if (rs.ComboID != 0)
        //                {
        //                    var combo = ComboTable.getbyid(rs.ComboID.Value);
        //                    if (combo == null)
        //                    {
        //                        return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        //                    }
        //                    if (combo.DiscountID != null)
        //                    {
        //                        if (combo.DiscountID != 0)
        //                        {
        //                            var cl = ClassTable.getbyID(rs.ClassID.Value);
        //                            var combodetail = ComboDetailTable.getbycomboidvscourseid(combo.ID, cl.ID);
        //                            var dt = ComboDetailTable.getbycomboid(combo.ID);
        //                            if (dt.Count == 1)
        //                                combodetail.DiscountPrice = combo.PriceDiscount.Value;
        //                            return Json(new { rs = true, discountValue = String.Format("{0:0,0}", combodetail.DiscountPrice) }, JsonRequestBehavior.AllowGet);
        //                        }
        //                    }
        //                    return Json(new { rs = false, message = "Có Combo" }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            return Json(new { rs = false, message = "Ko Combo" }, JsonRequestBehavior.AllowGet);

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        int id = 0;
        //        if (Session["UID"] != null)
        //            id = Session["UID"].ToString().ToInt(0);
        //        AssetCRM.Writelog("CustomerController", "CheckAppointment", id, ex.Message);
        //        //return RedirectToAction("Signin", "Login", new { area = "" });
        //        return Json(new { rs = false, message = "Ko Combo" }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        [HttpPost]// lấy thông tin buổi test
        public ActionResult GetAppointmentByID(int ID)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var rs = StudentAppointmentTable.getbyid(ID);
                if (rs == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                else
                {
                    var desireDates = DesiredStudyTimeTable.getbyStudentAppointmentID(rs.ID);
                    var cahoc = StudyTimeTable.getall().Where(x => x.isHide == false).OrderBy(x => x.sTime).ToList();
                    StringBuilder html = new StringBuilder();
                    for (var i = 0; i < desireDates.Count; i++)
                    {
                        html.Append("   <div class=\"col-sm-6 form-group\">");
                        html.Append("   <select name=\"slDay\" class=\"form-control select2\">");
                        for (var d = 0; d < 7; d++)
                        {
                            if (d == desireDates[i].DayOfWeekID)
                            {
                                html.Append("<option value=\"" + d + "\" selected >" + Enum.GetName(typeof(DayOfWeek), d) + "</option>");
                            }
                            else
                            {
                                html.Append("<option value=\"" + d + "\">" + Enum.GetName(typeof(DayOfWeek), d) + "</option>");
                            }
                        }
                        html.Append("   </select>");
                        html.Append("   </div>");
                        html.Append("   <div class=\"col-sm-6 form-group\">");
                        html.Append("   <select name=\"slStudytime\" class=\"form-control select2\">");
                        foreach (var s in cahoc)
                        {
                            if (s.ID == desireDates[i].StudyTimeID)
                            {
                                html.Append("<option value=\"" + s.ID + "\" selected >" + s.sTime + " - " + s.eTime + "</option>");
                            }
                            else
                            {
                                html.Append("<option value=\"" + s.ID + "\">" + s.sTime + " - " + s.eTime + "</option>");
                            }
                        }
                        html.Append("   </select>");
                        html.Append("   </div>");
                    }
                    string linkfb = "";
                    tbl_Account academic = null;
                    try
                    {
                        var student = AccountTable.getbyID(rs.StudentUID.Value);
                        var tvv = AccountTable.getbyID(student.SupportUID.Value);
                        academic = tvv;
                        linkfb = tvv.LinkFB;
                    }
                    catch { }
                    StringBuilder combostring = new StringBuilder();
                    var comboDetails = ComboDetailTable.getbycomboid(rs.ComboID ?? 0);
                    return Json(new { rs = true, academic, app = rs, dates = html.ToString(), linkfb, combos = string.Join(",", comboDetails.Select(x => x.ClassID).ToList()) }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                int id = 0;
                if (Session["UID"] != null)
                    id = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "GetAppointmentByID", id, ex.Message);
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult AddCus(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {

                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var sc = SourceOfCustomerTable.getbyID(f.Get("ddl-source").ToInt(0));

                var district = DistrictTable.getbyID(f.Get("ddl-district").ToInt(1));
                string fullname = f.Get("txt-full-name").ToString();
                string email = f.Get("txt-email").ToString();
                string phone = f.Get("txt-phone").ToString();
                string address = f.Get("txt-address").ToString();
                int language = f.Get("dll-language").ToInt(0);
                string cmnd = f.Get("txt-identity-card").ToString();
                var cmndcity = LocationTable.getbyID(f.Get("ddl-identity-card-city").ToInt(1));
                string cmnddate = f.Get("txt-identity-card-date").ToString();

                string birth = f.Get("txt-birthday").ToString().Replace(" ", "");

                var job = JobTable.getbyid(f.Get("ddl-job").ToInt(0));
                var workplace = f.Get("txt-work-place").ToString();

                var pur = AcademicPurposesTable.getbyID(f.Get("ddl-academic-purpose").ToInt(0));
                if (pur == null)
                {
                    pur = new Models.tbl_AcademicPurposes();
                    pur.ID = 0;
                    pur.AcademicPurposesName = "";
                }

                string note = f.Get("txt-note-home").ToString();
                string typeedu = f.Get("ddl-type-edu").ToString();
                string scorein = f.Get("txt-in").ToString();
                string scoreout = f.Get("txt-out").ToString();
                string dateexam = f.Get("txt-date-exam").ToString().Replace(" ", "");

                int spid = 0;
                if (aclog.RoleID == 6)//saler
                    spid = aclog.ID;

                if (sc == null)
                    sc = SourceOfCustomerTable.getall().OrderBy(n => n.ID).FirstOrDefault();
                if (aclog.RoleID == 6)//saler
                {
                    spid = aclog.ID;
                    sc.ID = 0;
                    sc.SourceOfCustomer = "TVV";
                }

                tbl_Ward w = new tbl_Ward();
                w.ID = 0;
                w.WardName = "";
                tbl_Street s = new tbl_Street();
                s.ID = 0;
                s.StreetName = "";

                var rs = AccountTable.insertcustomer(district, fullname, email, phone, address, 1, "", spid, sc.ID, sc.SourceOfCustomer, birth, job.ID, job.JobName, workplace,
                    pur.ID, pur.AcademicPurposesName, scorein, scoreout, dateexam, typeedu, cmnd, cmndcity.ID, cmndcity.CityName, cmnddate, note, aclog.UserName, false, w, s, "", language, "", 0);

                //if (!string.IsNullOrEmpty(note) && rs != null)// lưu lại ghi chú học viên
                //    StudentNoteTable.insert(rs.ID, rs.FullName, note, aclog.UserName);

                if (rs != null)
                    SetAlert("Thành công", "s");
                else
                    SetAlert("Không thành công, tài khoản đã tồn tại", "e");
                return RedirectToAction("CustomerList", "Customer");
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("CustomerController", "AddCus", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        ////cập nhật khách
        //[HttpPost]
        //public ActionResult UpdateCus(FormCollection f)
        //{
        //    if (Session["UID"] == null)
        //        return RedirectToAction("Signin", "Login", new { area = "" });
        //    var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
        //    if (aclog.RoleID == 5)
        //        return RedirectToAction("Signin", "Login", new { area = "" });

        //    var sc = SourceOfCustomerTable.getbyID(f.Get("ddl-source-edit").ToInt(0));
        //    if (sc == null)
        //    {
        //        sc = new tbl_SourceOfCustomer();
        //        sc.ID = 0;
        //        sc.SourceOfCustomer = "TVV";
        //    }
        //    var rs = AccountTable.getbyID(f.Get("hdfID").ToInt(0));
        //    if (rs == null)
        //    {
        //        SetAlert("Không tìm thấy học viên", "e");
        //        return RedirectToAction("CustomerList", "Customer");
        //    }

        //    //kiểm tra xem có phải ở trang đổi tvv khi tvv nghỉ ko
        //    bool checksupoff = false;
        //    if (rs.SupportUID != null)
        //    {
        //        var sup = AccountTable.getbyID(rs.SupportUID.Value);
        //        if (sup != null)
        //        {
        //            if (sup.Status != 2)
        //                checksupoff = true;
        //        }
        //    }

        //    //var district = DistrictTable.getbyID(f.Get("ddl-district-edit").ToInt(1));
        //    tbl_District district = new tbl_District();
        //    var citydis = f.Get("ddl-district-edit").ToString();
        //    if (!citydis.Contains("0-"))// là quận huyện
        //    {
        //        district = DistrictTable.getbyID(citydis.ToInt(0));
        //        if (district == null)
        //        {
        //            SetAlert("Không tìm thấy tỉnh thành", "e");
        //            if (checksupoff)
        //                return RedirectToAction("SupportOffList", "Customer");
        //            else
        //                return RedirectToAction("CustomerList", "Customer");
        //        }
        //    }
        //    else
        //    {
        //        int cityid = citydis.Replace("0-", "").ToInt(0);
        //        var city = LocationTable.getbyID(cityid);
        //        if (city == null)
        //        {
        //            SetAlert("Không tìm thấy tỉnh thành", "e");
        //            if (checksupoff)
        //                return RedirectToAction("SupportOffList", "Customer");
        //            else
        //                return RedirectToAction("CustomerList", "Customer");
        //        }
        //        district.ID = 0;
        //        district.DistrictName = "";
        //        district.LocationID = city.ID;
        //        district.LocationName = city.CityName;
        //    }

        //    string fullname = f.Get("txt-full-name-edit").ToString();
        //    string email = f.Get("txt-email-edit").ToString();
        //    string phone = f.Get("txt-phone-edit").ToString();
        //    string address = f.Get("txt-address-edit").ToString();

        //    string cmnd = f.Get("txt-identity-card-edit").ToString();
        //    var cmndcity = LocationTable.getbyID(f.Get("ddl-identity-card-city-edit").ToInt(1));
        //    string cmnddate = f.Get("txt-identity-card-date-edit").ToString();

        //    string birth = f.Get("txt-birthday-edit").ToString().Replace(" ", "");

        //    var job = JobTable.getbyid(f.Get("ddl-job-edit").ToInt(0));
        //    if (job == null)
        //    {
        //        job = new Models.tbl_Job();
        //        job.ID = 0;
        //        job.JobName = "";
        //    }
        //    var workplace = f.Get("txt-work-place-edit").ToString();

        //    var pur = AcademicPurposesTable.getbyID(f.Get("ddl-academic-purpose-edit").ToInt(0));
        //    if (pur == null)
        //    {
        //        pur = new Models.tbl_AcademicPurposes();
        //        pur.ID = 0;
        //        pur.AcademicPurposesName = "";
        //    }

        //    string note = f.Get("txt-note-home-edit").ToString();
        //    string typeedu = "Academic";
        //    if (!string.IsNullOrEmpty(f.Get("ddl-type-edu-edit")))
        //        typeedu = f.Get("ddl-type-edu-edit").ToString();
        //    string scorein = f.Get("txt-in-edit").ToString();
        //    string scoreout = f.Get("txt-out-edit").ToString();
        //    string dateexam = f.Get("txt-date-exam-edit").ToString().Replace(" ", "");

        //    int status = rs.Status.Value;
        //    string pass = "";
        //    if (!string.IsNullOrEmpty(f.Get("hdfDetail")))
        //    {
        //        status = f.Get("ddl-status").ToInt(1);
        //        pass = f.Get("txt-pass-edit").ToString();

        //        //lưu lại ghi chú
        //        string noteacc = f.Get("txt-note-account").ToString();
        //        if (!string.IsNullOrEmpty(noteacc))
        //            AccountHistoryChangeTable.insert(rs.ID, rs.FullName, noteacc, aclog.UserName);

        //    }

        //    int spid = f.Get("ddl-saler").ToInt(0);
        //    rs = AccountTable.updatecustomer(rs.ID, district, fullname, email, phone, address, status, pass, spid, sc.ID, sc.SourceOfCustomer, birth, job.ID, job.JobName, workplace,
        //        pur.ID, pur.AcademicPurposesName, scorein, scoreout, dateexam, typeedu, cmnd, cmndcity.ID, cmndcity.CityName, cmnddate, note, aclog.UserName);
        //    if (rs != null)
        //        SetAlert("Thành công", "s");
        //    else
        //        SetAlert("Không thành công", "e");
        //    if (!string.IsNullOrEmpty(f.Get("hdfDetail")))
        //        return RedirectToAction("CustomerDetai", "Customer", new { id = rs.ID });
        //    else
        //    {
        //        if (checksupoff)
        //            return RedirectToAction("SupportOffList", "Customer");
        //        else
        //            return RedirectToAction("CustomerList", "Customer");
        //    }
        //}
        //Cap naht customer ajax zim v2
        //cập nhật khách
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult UpdateCusAjax(int id, int sourceid, int citydis, string fullname, string phone, string email,
            string address, string cmnd, int cmndcity, string cmnddate, string birth, int jobid, string workplace,
            int mucdicid, string note, string typeedu, int scorein, int scoreout, string dateexam, int status,
            int supportid, string pass, int district = 0, int ward = 0, int street = 0, string home = "",
            int language = 0, string linkfb = "", int nation = 0)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 6)// Superadmin/Manager/Saler
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

                var rs = AccountTable.getbyID(id);
                if (rs == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

                var sc = SourceOfCustomerTable.getbyID(sourceid);
                if (sc == null)
                {
                    sc = new tbl_SourceOfCustomer();
                    sc.ID = 0;
                    sc.SourceOfCustomer = "TVV";
                }

                tbl_District dis = DistrictTable.getbyID(district);
                if (dis == null)// là quận huyện
                {
                    dis = new tbl_District();
                    dis.ID = 0;
                    dis.DistrictName = "";
                    dis.LocationID = 0;
                    dis.LocationName = "";
                }
                tbl_Ward wa = WardTable.getbyid(ward);
                if (wa == null)
                {
                    wa = new tbl_Ward();
                    wa.ID = 0;
                    wa.WardName = "";
                }
                tbl_Street st = StreetTable.getbyid(street);
                if (st == null)
                {
                    st = new tbl_Street();
                    st.ID = 0;
                    st.StreetName = "";
                }

                var job = JobTable.getbyid(jobid);
                if (job == null)
                {
                    job = new Models.tbl_Job();
                    job.ID = 0;
                    job.JobName = "";
                }
                var pur = AcademicPurposesTable.getbyID(mucdicid);
                if (pur == null)
                {
                    pur = new Models.tbl_AcademicPurposes();
                    pur.ID = 0;
                    pur.AcademicPurposesName = "";
                }

                var cmndcityoj = LocationTable.getbyID(cmndcity);
                if (cmndcityoj == null)
                {
                    cmndcityoj = new tbl_City();
                    cmndcityoj.ID = 0;
                    cmndcityoj.CityName = "";
                }

                rs = AccountTable.updatecustomer(rs.ID, dis, fullname, email, phone, address, status, pass, supportid, sc.ID, sc.SourceOfCustomer, birth, job.ID, job.JobName, workplace,
                    pur.ID, pur.AcademicPurposesName, scorein.ToString(), scoreout.ToString(), dateexam, typeedu, cmnd, cmndcityoj.ID, cmndcityoj.CityName, cmnddate, note, aclog.UserName, wa, st, home, language, linkfb, nation);
                if (rs != null)
                    return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "UpdateCusAjax", uid, ex.Message);
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AddCusAjax(int sourceid, int citydis, string fullname, string phone, string email, string address, string cmnd,
           int cmndcity, string cmnddate, string birth, int jobid, string workplace, int mucdicid, string note, string typeedu, string scorein,
           string scoreout, string dateexam, int status, int supportid, string pass, int district, int ward, int street, string home, int language)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 6)// Superadmin/Manager/Saler
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

                var sc = SourceOfCustomerTable.getbyID(sourceid);
                if (sc == null)
                {
                    sc = new tbl_SourceOfCustomer();
                    sc.ID = 0;
                    sc.SourceOfCustomer = "TVV";
                }

                tbl_District dis = DistrictTable.getbyID(district);
                if (dis == null)// là quận huyện
                {
                    dis = new tbl_District();
                    dis.ID = 0;
                    dis.DistrictName = "";
                    dis.LocationID = 0;
                    dis.LocationName = "";
                }
                tbl_Ward wa = WardTable.getbyid(ward);
                if (wa == null)
                {
                    wa = new tbl_Ward();
                    wa.ID = 0;
                    wa.WardName = "";
                }
                tbl_Street st = StreetTable.getbyid(street);
                if (st == null)
                {
                    st = new tbl_Street();
                    st.ID = 0;
                    st.StreetName = "";
                }

                var job = JobTable.getbyid(jobid);
                if (job == null)
                {
                    job = new Models.tbl_Job();
                    job.ID = 0;
                    job.JobName = "";
                }
                var pur = AcademicPurposesTable.getbyID(mucdicid);
                if (pur == null)
                {
                    pur = new Models.tbl_AcademicPurposes();
                    pur.ID = 0;
                    pur.AcademicPurposesName = "";
                }

                var cmndcityoj = LocationTable.getbyID(cmndcity);
                if (cmndcityoj == null)
                {
                    cmndcityoj = new tbl_City();
                    cmndcityoj.ID = 0;
                    cmndcityoj.CityName = "";
                }
                var rs = AccountTable.insertcustomer(dis, fullname, email, phone, address, status, "", supportid,
                    sc.ID, sc.SourceOfCustomer, birth, job.ID, job.JobName, workplace, pur.ID, pur.AcademicPurposesName, scorein, scoreout,
                    dateexam, typeedu, cmnd, cmndcityoj.ID, cmndcityoj.CityName, cmnddate, note, aclog.UserName, false, wa, st, home, language, "", 0);
                if (rs != null)
                    return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "AddCusAjax", uid, ex.Message);
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult EditCus(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var ID = f.Get("hdfID").ToInt(0);
                var acc = AccountTable.getbyID(ID);
                if (acc == null)
                {
                    SetAlert("Không tìm thấy tài khoản này", "e");
                    return RedirectToAction("CustomerList", "Customer");
                }
                int status = f.Get("ddl-status").ToInt(0);
                string fullname = f.Get("txt-full-name").ToString();
                string email = f.Get("txt-email").ToString();
                string phone = f.Get("txt-phone").ToString();
                string pass = f.Get("txt-password").ToString();
                string address = f.Get("txt-address").ToString();
                int language = f.Get("dll-language").ToInt(0);
                DateTime bd = GetDateTime.Now.Date;
                try
                {
                    bd = DateTime.ParseExact(f.Get("txt-birthday").ToString(), "dd/MM/yyyy", null);
                }
                catch
                {
                    bd = GetDateTime.Now.Date;
                }

                var rs = AccountTable.updatestaff(acc.ID, acc.LocationID.Value, acc.LocationName, 0, "", fullname, pass,
                    email, phone, address, acc.RoleID.Value, status, bd, 3, aclog.UserName, acc.DistrictID.Value,
                    acc.DistrictName, language, "", "", "", "", "", "", "", acc.Position ?? 0, acc.Revenue ?? 0);
                if (rs != null)
                    SetAlert("Thành công", "s");
                else
                    SetAlert("Không thành công", "e");
                return RedirectToAction("ConfigurationTeacher", "Staff", new { id = acc.ID });
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "EditCus", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        // học viên sử dụng dịch vụ
        public ActionResult ServiceCustomer(int page = 1)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                //page size
                int pageSize = 20;
                //lấy hoc viên
                IEnumerable<tbl_ServicesStudent> l = ServicesStudentTable.getall().OrderByDescending(n => n.ID).ToList();
                //key search
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                {
                    ViewBag.Search = Request.QueryString["search"];
                    string search = Request.QueryString["search"].ToString().Trim();

                    l = from ex in l
                        where AssetCRM.RemoveUnicode(ex.StudentName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        || AssetCRM.RemoveUnicode(ex.ServiceName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        || AssetCRM.RemoveUnicode(ex.SupplierServices).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        select ex;
                }
                //dịch vụ
                if (Request.QueryString["service"].ToInt(0) != 0)
                {
                    int service = ViewBag.ServiceID = Request.QueryString["service"].ToInt(0);
                    l = from ex in l
                        where ex.ServiceID == service
                        select ex;
                }
                //nhà cung cấp
                if (Request.QueryString["supplier"].ToInt(0) != 0)
                {
                    int supplier = ViewBag.SupplierID = Request.QueryString["supplier"].ToInt(0);
                    l = from ex in l
                        where ex.SupplierServicesID == supplier
                        select ex;
                }
                // thời gian thi từ
                if (!string.IsNullOrEmpty(Request.QueryString["fromexam"]))
                {
                    try
                    {
                        l = l.Where(n => n.ExamDate != null).ToList();
                        DateTime fromdate = DateTime.ParseExact(Request.QueryString["fromexam"].ToString(), "dd/MM/yyyy", null);
                        l = from ex in l
                            where ex.ExamDate.Value.Date >= fromdate.Date
                            select ex;
                        ViewBag.FromExam = fromdate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                // đến
                if (!string.IsNullOrEmpty(Request.QueryString["toexam"]))
                {
                    try
                    {
                        l = l.Where(n => n.ExamDate != null).ToList();
                        DateTime todate = DateTime.ParseExact(Request.QueryString["toexam"].ToString(), "dd/MM/yyyy", null);
                        l = from ex in l
                            where ex.ExamDate.Value.Date <= todate.Date
                            select ex;
                        ViewBag.ToExam = todate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
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
                //sort
                int sort = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
                {
                    sort = Request.QueryString["sort"].ToInt(0);

                    if (sort == 1)
                        l = l.OrderBy(n => n.StudentName.Trim().Substring(n.StudentName.Trim().LastIndexOf(' ') + 1).ToString()).ToList();
                    if (sort == 2)
                        l = l.OrderByDescending(n => n.StudentName.Substring(n.StudentName.Trim().LastIndexOf(' ') + 1).ToString()).ToList();
                    if (sort == 3)
                        l = l.OrderBy(n => n.ExamDate).ToList();
                    if (sort == 4)
                        l = l.OrderByDescending(n => n.ExamDate).ToList();
                    if (sort == 5)
                        l = l.OrderBy(n => n.PriceService).ToList();
                    if (sort == 6)
                        l = l.OrderByDescending(n => n.PriceService).ToList();
                }
                ViewBag.Sort = sort;
                ViewBag.Total = l.Count();
                return View(l.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "ServiceCustomer", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }


        }
        [HttpPost]// lấy thông tin học viên mua dịch vụ
        public ActionResult CheckRefundServiceStudent(int ID)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var sc = ServicesStudentTable.getbyid(ID);
                if (sc == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                else
                {
                    var ck = RefundsTable.getbyservicestudentid(sc.ID);
                    if (ck == null)
                        return Json(new { rs = true, fullname = sc.StudentName, deposit = String.Format("{0:0,0}", sc.PriceService) }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "CheckRefundServiceStudent", uid, ex.Message);
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        //yêu cầu hoàn tiền dịch vụ
        [HttpPost]
        public ActionResult RefundService(FormCollection f)
        {
            try
            {
                var url = f.Get("hdf-url").ToString();
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var sv = ServicesStudentTable.getbyid(f.Get("hdfRefundAppointID").ToInt(0));
                if (sv == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "e");
                    return Redirect(url);
                }
                var ck = RefundsTable.getbyservicestudentid(sv.ID);
                if (ck != null)
                {
                    SetAlert("Đã gửi yêu cầu cho admin, không thể gửi thêm", "e");
                    return Redirect(url);
                }
                var cus = AccountTable.getbyID(sv.StudentUID.Value);
                if (cus == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "e");
                    return Redirect(url);
                }
                string note = f.Get("txt-note-refund").ToString();
                int price = f.Get("txt-refund").ToString().Replace(",", "").ToInt(0);
                if (price == 0 || price > sv.PriceService)
                {
                    SetAlert("Vui lòng nhập số tiền đúng <= số tiền đã đóng", "e");
                    return Redirect(url);
                }
                var chool = SchoolTable.getbyID(f.Get("ddl-school-refund").ToInt(0));
                if (chool == null)
                {
                    SetAlert("Vui lòng nhập số tiền", "e");
                    return Redirect(url);
                }

                bool kichout = true;
                //if (!string.IsNullOrEmpty(f.Get("chk-kick-out")))
                //    kichout = true;

                var rs = RefundsTable.insert(0, 0, sv.ID, cus, price, note, chool, kichout, aclog.UserName);
                if (rs != null)
                {
                    // gửi thông báo cho admin               
                    var ladmin = AccountTable.getbyrole(1).Where(n => n.Status == 2 && n.ID == 1).ToList();
                    string strPathAndQuery = Request.Url.PathAndQuery;
                    string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                    string datalink = "" + strUrl + "Admin/Cashier/RefundList";
                    foreach (var item in ladmin)
                    {
                        string title = "[" + chool.SchoolName + "] - Yêu cầu hoàn tiền " + String.Format("{0:0,0 VNĐ}", rs.Price) + "";

                        string content = "Phát sinh yêu cầu hoàn tiền có ID = \"" + rs.ID;
                        var nt = NotificationTable.insertsingle("", title, content, item.ID, aclog.UserName, 9, 0);
                        // gửi mail
                        if (!string.IsNullOrEmpty(item.Email))
                            AppZim.ZIM.AssetCRM.SendMail(item.Email, nt.NotificationTitle, nt.NotificationContent);
                    }
                    SetAlert("Thành công", "s");
                }
                else
                    SetAlert("Không thành công", "e");
                return Redirect(url);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "RefundService", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateScoreExam(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var url = f.Get("hdf-url").ToString();
                var sv = ServicesStudentTable.getbyid(f.Get("hdfID").ToInt(0));
                if (sv == null)
                {
                    SetAlert("Không tìm thấy tài dữ liệu", "e");
                    return Redirect(url);
                }
                double lis = f.Get("txt-listening").Replace(",", ".").ToFloat(100000);
                if (lis == 100000)
                {
                    SetAlert("Vui lòng nhập đúng số điểm, kiểu số", "e");
                    return Redirect(url);
                }
                double spe = f.Get("txt-speking").Replace(",", ".").ToFloat(100000);
                if (spe == 100000)
                {
                    SetAlert("Vui lòng nhập đúng số điểm, kiểu số", "e");
                    return Redirect(url);
                }
                double read = f.Get("txt-reading").Replace(",", ".").ToFloat(100000);
                if (read == 100000)
                {
                    SetAlert("Vui lòng nhập đúng số điểm, kiểu số", "e");
                    return Redirect(url);
                }
                double one = f.Get("txt-task-one").Replace(",", ".").ToFloat(100000);
                if (one == 100000)
                {
                    SetAlert("Vui lòng nhập đúng số điểm, kiểu số", "e");
                    return Redirect(url);
                }
                double two = f.Get("txt-task-two").Replace(",", ".").ToFloat(100000);
                if (two == 100000)
                {
                    SetAlert("Vui lòng nhập đúng số điểm, kiểu số", "e");
                    return Redirect(url);
                }
                var rs = ServicesStudentTable.updatescore(sv.ID, lis, spe, read, one, two, aclog.UserName);
                if (rs != null)
                    SetAlert("Thành công", "s");
                else
                    SetAlert("Không thành công", "e");
                return Redirect(url);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "UpdateScoreExam", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// Cập nhật trạng thái cmt của giáo viên chủ nhiệm
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult UpdateStatusCMTwarning(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 7 && aclog.RoleID != 8)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var url = f.Get("hdf-url").ToString();
                var rs = CoursesAttendanceStudentWarningTable.updateaccept(f.Get("hdfID").ToInt(0), f.Get("hdfStatus").ToInt(0), aclog.UserName);
                if (rs != null)
                    SetAlert("Thành công", "s");
                else
                    SetAlert("Không thành công", "e");
                return Redirect(url);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "UpdateStatusCMTwarning", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]// tìm kiếm theo số điện thoại
        public ActionResult GetScoreService(int id)
        {
            try
            {

                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var l = ServicesStudentTable.getbyid(id);
                if (l.OverAll != null)
                    return Json(new { rs = true, l = l }, JsonRequestBehavior.AllowGet);
                return Json(new { rs = false, l = l }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "GetScoreService", uid, ex.Message);
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        // danh sách học viên đăng ký học
        public ActionResult CourseStudent(int page = 1)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                //page size
                int pageSize = 20;
                string search = "";
                //key search
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                {
                    ViewBag.Search = Request.QueryString["search"];
                    search = Request.QueryString["search"].ToString().Trim();
                }

                int language = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["cblang"]))
                {
                    language = ViewBag.Language = Request.QueryString["cblang"].ToInt(0);
                }

                int program = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["hdProgram"]))
                {
                    program = ViewBag.Program = Request.QueryString["hdProgram"].ToInt(0);
                }

                int grade = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["hdGrade"]))
                {
                    grade = ViewBag.Grade = Request.QueryString["hdGrade"].ToInt(0);
                }

                int tvv = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["slTVV"]))
                {
                    tvv = Request.QueryString["slTVV"].ToInt(0);
                }
                if(aclog.RoleID == 6)
                {
                    tvv = aclog.ID;
                }

                ViewBag.TVV = tvv;

                //List<getCustomerInCourse_Result> l = new List<getCustomerInCourse_Result>();
                //using (var db = new ZimEntities())
                //{
                //    l = db.getCustomerInCourse(search, program, (page - 1) * pageSize, pageSize, tvv).ToList();
                //}
                //ViewBag.StartIndex = (page - 1) * pageSize;
                //int total = 0;
                //if (l.Count > 0)
                //{
                //    total = l[0].TotalRow.Value;
                //}
                //ViewBag.Total = total;
                //int PageCount = 0;
                //if (total % pageSize == 0)
                //    PageCount = total / pageSize;
                //else
                //    PageCount = total / pageSize + 1;
                //ViewBag.Page = page;
                //ViewBag.PageCount = PageCount;
                return View();
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "CourseStudent", uid, ex.Message + ex.InnerException);
                return RedirectToAction("Signin", "Login", new { area = "" });
            }

        }
        public ActionResult ExportCourseStudentExcel()
        {
            try
            {
                var df = DateTime.ParseExact("01/01/1001", "dd/MM/yyyy", null).Date;
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                //page size
                string search = "";
                //key search
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                {
                    search = Request.QueryString["search"].ToString().Trim();
                }
                //center
                int center = 0;
                if (Request.QueryString["center"].ToInt(0) != 0)
                {
                    center = Request.QueryString["center"].ToInt(0);

                }
                // thời gian bắt đầu từ
                string fromdate = null;
                if (!string.IsNullOrEmpty(Request.QueryString["from"]))
                {
                    try
                    {
                        DateTime fdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                        fromdate = fdate.Date.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                string todate = null;
                // đến
                if (!string.IsNullOrEmpty(Request.QueryString["to"]))
                {
                    try
                    {
                        DateTime tdate = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null);
                        todate = tdate.Date.ToString("dd/MM/yyyy 23:59:59");
                    }
                    catch { }
                }
                //sort
                int sort = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
                {
                    sort = Request.QueryString["sort"].ToInt(0);

                }
                List<getCustomerInCourse_Export_Result> l = new List<getCustomerInCourse_Export_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.getCustomerInCourse_Export(search, center, fromdate, todate, sort).ToList();
                }
                var dataTable = new System.Data.DataTable("HV Trong Khoa");
                dataTable.Columns.Add("Họ và tên", typeof(string));
                dataTable.Columns.Add("Điện thoại", typeof(string));
                dataTable.Columns.Add("Email", typeof(string));
                dataTable.Columns.Add("Địa chỉ", typeof(string));
                dataTable.Columns.Add("Quận/Huyện", typeof(string));
                dataTable.Columns.Add("Tỉnh/Thành phố", typeof(string));
                dataTable.Columns.Add("Khóa học", typeof(string));
                dataTable.Columns.Add("Trung tâm", typeof(string));
                dataTable.Columns.Add("Cam kết", typeof(string));
                dataTable.Columns.Add("Giá tiền", typeof(string));
                dataTable.Columns.Add("Đã đóng", typeof(string));
                dataTable.Columns.Add("Giảm giá", typeof(string));
                dataTable.Columns.Add("Còn lại", typeof(string));
                dataTable.Columns.Add("Nhập ngày", typeof(string));
                dataTable.Columns.Add("Cảnh báo", typeof(string));
                dataTable.Columns.Add("Kết quả thi", typeof(string));
                dataTable.Columns.Add("Ghi chú", typeof(string));
                foreach (var item in l)
                {
                    try
                    {
                        dataTable.Rows.Add(
                            item.StudentName,
                            item.Phone,
                            item.Email,
                            item.Address,
                            item.DistrictName,
                            item.LocationName,
                            item.CourseName,
                            item.SchoolName,
                            item.Commitment,
                            string.Format("{0:0,0}", item.PriceCourse),
                            string.Format("{0:0,0}", item.TotalPaid),
                            string.Format("{0:0,0}", item.PriceDiscout),
                            string.Format("{0:0,0}", item.PriceLeft),
                            item.CreatedDate.Value.ToString("dd/MM/yyyy"),
                            item.Warning,
                            item.Result,
                            item.Note);
                    }
                    catch
                    {
                        continue;
                    }
                }
                var grid = new GridView();
                grid.DataSource = dataTable;
                grid.DataBind();
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=ExcelCourseStudent.xls");
                Response.ContentType = "application/ms-excel";
                Response.ContentEncoding = System.Text.Encoding.Unicode;
                Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
                Response.Charset = "";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                grid.RenderControl(htw);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
                //return Json(new { rs = true, message = "Success" }, JsonRequestBehavior.AllowGet);
                return RedirectToAction("CourseStudent", "Customer", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "CourseStudent", uid, ex.Message);
                return RedirectToAction("CourseStudent", "Customer", new { area = "Admin" });
            }
        }
        [HttpPost]// lấy thông tin học viên trong khóa
        public ActionResult CheckRefundCourseStudent(int ID)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var rs = CoursesStudentTable.getbyid(ID);
                if (rs == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                else
                {
                    var ck = RefundsTable.getbycoursestudentid(rs.ID);
                    if (ck == null)
                        return Json(new { rs = true, fullname = rs.StudentName, deposit = String.Format("{0:0,0}", rs.TotalPaid) }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "CheckRefundCourseStudent", uid, ex.Message);
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]// lấy thông tin học viên trong khóa
        public ActionResult CheckCourseStudentChange(int ID)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var rs = CoursesStudentTable.getbyid(ID);
                var course = new tbl_Courses();
                if (rs != null)
                {
                    course = CoursesTable.getbyid(rs.CourseID.Value);
                }            
                if (rs == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                return Json(new { rs = true, l = rs, course = course }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "CheckCourseStudentChange", uid, ex.Message);
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]// tính discout từ mã khóa cũ cho khóa mới
        public ActionResult DiscountCourseChange(int ID, int courseid)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var rs = CoursesStudentTable.getbyid(ID);
                var course = CoursesTable.getbyid(courseid);
                if (rs == null || course == null)
                    return Json(new { rs = 0 }, JsonRequestBehavior.AllowGet);
                if (rs.DiscountID == 0)
                    return Json(new { rs = 0 }, JsonRequestBehavior.AllowGet);

                return Json(new { rs = String.Format("{0:0,0}", rs.PriceDiscout) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "DiscountCourseChange", uid, ex.Message);
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        //Chuyển lớp
        [HttpPost]
        public ActionResult ChangeCourseStudent(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var r = CoursesStudentTable.getbyid(f.Get("hdfChangerID").ToInt(0));
                if (r == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "e");
                    return RedirectToAction("CourseStudent", "Customer");
                }
                var cource = CoursesTable.getbyid(f.Get("ddl-cource").ToInt(0));
                if (cource == null)
                {
                    SetAlert("Không tìm thấy khóa học chuyển đến", "e");
                    return RedirectToAction("CourseStudent", "Customer");
                }

                var choolinvoice = SchoolTable.getbyID(cource.SchoolID.Value);
                if (choolinvoice == null)
                {
                    SetAlert("Không tìm thấy trung tâm tạo phiếu", "e");
                    return RedirectToAction("RegisterCourse", "Cashier");
                }

                string note = f.Get("txt-note-change").ToString();
                int money = f.Get("txt-add-price").Replace(",", "").ToInt(0);

                //lưu lại lịch sử thằng trước khi chuyển
                CoursesStudentHistoryChangeTable.insert(r, cource.ID, aclog.UserName);
                //cập nhật
                var kq = CoursesStudentTable.updatechangecourse(r.ID, cource, r.DiscountID ?? 0, r.PriceDiscout ?? 0, money, note, aclog.UserName);
                if (r.ComboID > 0)//combo thì update lại combodetail
                {
                    var preCourse = CoursesTable.getbyid(r.CourseID.Value);
                    ComboDetailTable.ReActive(r.ComboID ?? 0, preCourse.ClassID ?? 0, aclog.UserName);// mở lại combodetail cũ
                    ComboDetailTable.UpdateDone(0, r.ComboID ?? 0, cource.ClassID ?? 0, aclog.UserName, preCourse.ClassID ?? 0);// isDone combodetail mới
                }

                if (money > 0 && kq != null)//có tiền thì update lại hóa đơn, hẹn đăng ký
                {
                    var ac = AccountTable.getbyID(kq.StudentUID.Value);
                    InvoicesTable.UpdatePrice(ac.ID, r.AppointmentID ?? 0, money, aclog.UserName);
                    StudentAppointmentTable.UpdatePrice(kq.AppointmentID ?? 0, money, aclog.UserName);
                }

                if (kq == null)
                    SetAlert("Không thành công", "e");
                else
                    SetAlert("Thành công", "s");
                return RedirectToAction("CourseStudent", "Customer");
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "ChangeCourseStudent", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        //yêu cầu hoàn tiền học
        [HttpPost]
        public ActionResult RefundCourseStudent(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var sc = CoursesStudentTable.getbyid(f.Get("hdfRefundAppointID").ToInt(0));
                if (sc == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "e");
                    return RedirectToAction("CourseStudent", "Customer");
                }
                var ck = RefundsTable.getbycoursestudentid(sc.ID);
                if (ck != null)
                {
                    SetAlert("Đã gửi yêu cầu cho admin, không thể gửi thêm", "e");
                    return RedirectToAction("CourseStudent", "Customer");
                }
                var cus = AccountTable.getbyID(sc.StudentUID.Value);
                if (cus == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "e");
                    return RedirectToAction("CourseStudent", "Customer");
                }
                string note = f.Get("txt-note-refund").ToString();
                int price = f.Get("txt-refund").ToString().Replace(",", "").ToInt(0);
                if (price == 0 || price > sc.TotalPaid)
                {
                    SetAlert("Vui lòng nhập số tiền đúng <= số tiền đã đóng", "e");
                    return RedirectToAction("CourseStudent", "Customer");
                }
                var chool = SchoolTable.getbyID(sc.SchoolID.Value);
                if (chool == null)
                {
                    SetAlert("Vui lòng nhập số tiền", "e");
                    return RedirectToAction("CourseStudent", "Customer");
                }

                bool kichout = false;
                if (!string.IsNullOrEmpty(f.Get("chk-kick-out")))
                    kichout = true;

                var rs = RefundsTable.insert(sc.ID, 0, 0, cus, price, note, chool, kichout, aclog.UserName);
                if (rs != null)
                {
                    // gửi thông báo cho admin               
                    var ladmin = AccountTable.getbyrole(1).Where(n => n.Status == 2 && n.ID == 1).ToList();
                    string strPathAndQuery = Request.Url.PathAndQuery;
                    string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                    string datalink = "" + strUrl + "Admin/Cashier/RefundList";
                    foreach (var item in ladmin)
                    {
                        string title = "[" + chool.SchoolName + "] - Yêu cầu hoàn tiền " + String.Format("{0:0,0 VNĐ}", rs.Price) + "";

                        string content = "Phát sinh yêu cầu hoàn tiền có ID = \"" + rs.ID;
                        var nt = NotificationTable.insertsingle("", title, content, item.ID, aclog.UserName, 0, 0);


                        // gửi mail
                        if (!string.IsNullOrEmpty(item.Email))
                            AppZim.ZIM.AssetCRM.SendMail(item.Email, nt.NotificationTitle, nt.NotificationContent);
                    }
                    SetAlert("Thành công", "s");
                }
                else
                    SetAlert("Không thành công", "e");
                return RedirectToAction("CourseStudent", "Customer");
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "RefundCourseStudent", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        //Bảo lưu gói học
        [HttpPost]
        public ActionResult ReserveCourseStudent(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var sc = CoursesStudentTable.getbyid(f.Get("hdfReserveID").ToInt(0));
                if (sc == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "e");
                    return RedirectToAction("CourseStudent", "Customer");
                }

                if (sc.CourseReserve ?? false)
                {
                    SetAlert("Đã bảo lưu", "e");
                    return RedirectToAction("CourseStudent", "Customer");
                }
                var cus = AccountTable.getbyID(sc.StudentUID.Value);
                if (cus == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "e");
                    return RedirectToAction("CourseStudent", "Customer");
                }
                var commitment = ClassTable.getbyID(sc.CommitmentID ?? 0);
                string dateline = f.Get("txt-date-reserve").ToString().Replace(" ", "");
                string note = "[BẢO LƯU] - [" + dateline + "] " + f.Get("txt-note-reserve").ToString();

                var rs = CoursesStudentTable.updatereserve(sc.ID, note, aclog.UserName);
                if (rs != null)
                {
                    var app = StudentAppointmentTable.ReActive(sc.AppointmentID ?? 0, aclog.UserName, dateline);
                    //nếu là combo thì mở lại chương trình học đã đóng
                    var course = CoursesTable.getbyid(sc.CourseID.Value);
                    if (app.ComboID > 0)
                    {
                        ComboDetailTable.ReActive(app.ComboID ?? 0, course.ClassID ?? 0, aclog.UserName);
                    }
                    var school = SchoolTable.getbyID(course.SchoolID ?? 0);
                    // gửi thông báo cho admin               
                    var ladmin = AccountTable.getbyrole(1).Where(n => n.Status == 2).ToList();
                    foreach (var item in ladmin)
                    {
                        string title = "[" + school?.SchoolName + "] - Bảo lưu";

                        string content = "Học viên [" + cus.FullName + "] bảo lưu khóa học [" + course.CourseName + "]";
                        var nt = NotificationTable.insertsingle("", title, content, item.ID, aclog.UserName, 14, 0);
                    }
                    SetAlert("Thành công", "s");
                }
                else
                    SetAlert("Không thành công", "e");
                return RedirectToAction("CourseStudent", "Customer");
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "ReserveCourseStudent", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        //Khách hàng bảo lưu khóa học
        public ActionResult CustomerReserve(int page = 1)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                string search = "";
                ViewBag.Search = Request.QueryString["search"];
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                    search = Request.QueryString["search"].ToString().Trim();

                //lớp
                int lop = 0;
                if (Request.QueryString["lop"].ToInt(0) != 0)
                {
                    lop = ViewBag.LopID = Request.QueryString["lop"].ToInt(0);
                }

                // thời gian bắt đầu từ
                // lấy ngày đầu và ngày cuối tháng
                DateTime date = DateTime.Now;
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                string fromdate = firstDayOfMonth.ToString("yyyy-MM-dd");
                ViewBag.From = firstDayOfMonth.ToString("dd/MM/yyyy");
                if (!string.IsNullOrEmpty(Request.QueryString["from"]))
                {
                    try
                    {
                        DateTime fdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                        ViewBag.From = fdate.ToString("dd/MM/yyyy");
                        fromdate = fdate.ToString("yyyy-MM-dd");
                    }
                    catch { }
                }
                string todate = lastDayOfMonth.ToString("yyyy-MM-dd");
                ViewBag.To = lastDayOfMonth.ToString("dd/MM/yyyy");
                // đến
                if (!string.IsNullOrEmpty(Request.QueryString["to"]))
                {
                    try
                    {
                        DateTime tdate = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null);
                        ViewBag.To = tdate.ToString("dd/MM/yyyy");
                        todate = tdate.ToString("yyyy-MM-dd");
                    }
                    catch { }
                }
                int pageSize = 20;
                List<get_list_of_CustomerReserve_Result> l = new List<get_list_of_CustomerReserve_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.get_list_of_CustomerReserve(search, lop, fromdate, todate, (page - 1) * pageSize, pageSize, aclog.ID, aclog.RoleID).ToList();
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
            ///////////////
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "CustomerReserve", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        // danh sách cảnh báo học viên
        public ActionResult CustomerWarning(int page = 1)
        {
            try
            {

                var df = DateTime.ParseExact("01/01/1001", "dd/MM/yyyy", null).Date;
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                //page size
                int pageSize = 20;
                //lấy học viên bị cảnh báo          

                IEnumerable<tbl_CoursesAttendanceStudent> l;
                //if (aclog.RoleID == 4)
                //    l = CoursesAttendanceStudentTable.getcuswarningPriorityTeacher(aclog.ID);
                //else
                //    l = CoursesAttendanceStudentTable.getcuswarning().OrderByDescending(n => n.ModifiedDate).ToList();
                SqlParameter[] parameters =
                {
                    new SqlParameter("@centerid", SqlDbType.Int) { Value = aclog.SchoolID.Value },
                    new SqlParameter("@role", SqlDbType.Int) { Value = aclog.RoleID },
                    new SqlParameter("@uid", SqlDbType.Int) { Value = aclog.ID },
                    new SqlParameter("@Type", SqlDbType.Int) { Value = 1 },// lấy dữ liệu
                };
                var rs = SqlHelper.ExecuteDataTable("getallwarning", parameters);
                l = (from rw in rs.AsEnumerable()
                     select new tbl_CoursesAttendanceStudent()
                     {
                         ID = Convert.ToInt32(rw["ID"]),
                         SchoolID = Convert.ToInt32(rw["SchoolID"]),
                         SchoolName = rw["SchoolName"].ToString(),
                         CourseID = Convert.ToInt32(rw["CourseID"]),
                         StudentName = rw["StudentName"].ToString(),
                         CourseName = rw["SchoolName"].ToString(),
                         CreatedDate = Convert.ToDateTime(rw["CreatedDate"]),
                         TeacherName = rw["TeacherName"] == DBNull.Value ? "" : rw["TeacherName"].ToString(),
                         TeacherLeadName = rw["TeacherLeadName"] == DBNull.Value ? "" : rw["TeacherLeadName"].ToString(),
                         TeacherLeadID = rw["TeacherLeadID"] == DBNull.Value ? 0 : Convert.ToInt32(rw["TeacherLeadID"]),
                         AttendanceName = rw["AttendanceName"].ToString(),
                         LearningName = rw["LearningName"].ToString(),
                         Note = rw["Note"].ToString(),
                         CmtNew = rw["CmtNew"].ToString()

                     }).ToList();

                //key search
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                {
                    ViewBag.Search = Request.QueryString["search"];
                    string search = Request.QueryString["search"].ToString().Trim();

                    l = l.Where(n => AssetCRM.RemoveUnicode(n.SchoolName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        || AssetCRM.RemoveUnicode(n.StudentName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        || AssetCRM.RemoveUnicode(n.CourseName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        || AssetCRM.RemoveUnicode(n.TeacherName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        || AssetCRM.RemoveUnicode(n.TeacherLeadName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())).ToList();
                }
                //trung tâm
                if (Request.QueryString["center"].ToInt(0) != 0)
                {
                    int center = ViewBag.CenterID = Request.QueryString["center"].ToInt(0);
                    l = l.Where(n => n.SchoolID == center).ToList();
                }
                // thời gian bắt đầu từ
                if (!string.IsNullOrEmpty(Request.QueryString["from"]))
                {
                    try
                    {
                        DateTime fromdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                        l = l.Where(n => n.CreatedDate.Value >= fromdate).ToList();
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
                        l = l.Where(n => n.CreatedDate.Value <= todate).ToList();
                        ViewBag.To = todate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                //sort
                l = l.OrderByDescending(n => n.CreatedDate).ToList();
                int sort = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
                {
                    sort = Request.QueryString["sort"].ToInt(0);
                    if (sort == 1)
                        l = l.OrderBy(n => n.StudentName.Trim().Substring(n.StudentName.Trim().LastIndexOf(' ') + 1).ToString()).ToList();
                    if (sort == 2)
                        l = l.OrderByDescending(n => n.StudentName.Substring(n.StudentName.Trim().LastIndexOf(' ') + 1).ToString()).ToList();
                    if (sort == 3)
                        l = l.OrderBy(n => n.CreatedDate).ToList();
                    if (sort == 4)
                        l = l.OrderByDescending(n => n.CreatedDate).ToList();
                    if (sort == 5)
                        l = l.OrderBy(n => n.TeacherName.Trim().Substring(n.TeacherName.Trim().LastIndexOf(' ') + 1).ToString()).ToList();
                    if (sort == 4)
                        l = l.OrderByDescending(n => n.TeacherName).ToList();
                }
                ViewBag.Sort = sort;
                ViewBag.Total = l.Count();
                return View(l.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "CustomerWarning", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        // danh sách cảnh báo học viên
        public ActionResult WarningDetail(int id)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var student = AccountTable.getbyID(id);
                //var w = CoursesAttendanceStudentTable.getbyid(id);
                if (student == null)
                {
                    SetAlert("Không tìm thấy học viên", "e");
                    return RedirectToAction("CustomerWarning", "Customer");
                }
                bool check = false;
                SqlParameter[] parameters =
                    {
                      new SqlParameter("@aclogID", SqlDbType.Int) { Value = aclog.ID },
                      new SqlParameter("@roleID", SqlDbType.Int) { Value = aclog.RoleID },
                      new SqlParameter("@studentID", SqlDbType.Int) { Value = id },
                };
                var rs = SqlHelper.ExecuteDataTable("check_permission", parameters);
                check = Convert.ToBoolean((from rw in rs.AsEnumerable() select rw["KT"]).FirstOrDefault().ToString().ToInt(0));
                if (!check)
                {
                    SetAlert("Xin lỗi bạn không có quyền xem thông tin học viên này", "e");
                    if (HttpContext.Request.UrlReferrer != null)
                    {
                        return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
                    }
                    else
                    {
                        return RedirectToAction("TestResult", "Customer", new { area = "Admin" });
                    }
                }
                return View(student);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "WarningDetail", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult RepWarning(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var ac = AccountTable.getbyID(f.Get("hdfStudentID").ToInt(0));
                if (ac == null)
                {
                    return RedirectToAction("CustomerWarning", "Customer");
                }
                string content = f.Get("hdfcontent").ToString();
                var rs = CoursesAttendanceStudentWarningTable.insert(ac.ID, 0, content, aclog.UserName, 1);
                if (rs != null)
                {
                    ////cập nhật cmt mới cho màn hình chi tiết học viên
                    //if (w.ID != 0)
                    //{
                    //    CoursesAttendanceStudentTable.updatecmtnew(w.ID, content, aclog.UserName);
                    //}

                    List<tbl_Account> l = new List<tbl_Account>();
                    var student = AccountTable.getbyID(ac.ID);
                    var sup = AccountTable.getbyID(student.SupportUID.Value);
                    if (sup != null)
                        l.Add(sup);
                    //ds giáo viên dạy nó
                    var lcourse = CoursesAttendanceStudentTable.getbyuid(student.ID).Where(n => n.Warning == true).ToList().Select(n => n.CourseID).Distinct();
                    foreach (var item in lcourse)
                    {
                        if (item != null)
                        {
                            List<tbl_TeacherCourse> lcc = TeacherCourseTable.getbycourseid(item.Value);
                            foreach (var item2 in lcc)
                            {
                                if (item2 != null)
                                {
                                    tbl_Account teacher = AccountTable.getbyID(item2.TeacherUID.Value);
                                    if (teacher != null)
                                    {
                                        if (l.Where(n => n.UserName == teacher.UserName).FirstOrDefault() == null)
                                        {
                                            l.Add(teacher);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    foreach (var acpush in l)
                    {
                        if (acpush.ID == aclog.ID)
                            continue;
                        //gửi thông báo cho ng có liên quan
                        var nt = NotificationTable.insertsingle("", "Cảnh báo học viên có bình luận mới", "", acpush.ID, aclog.UserName, 15, ac.ID);

                    }
                    SetAlert("Thành công", "s");
                }
                return RedirectToAction("WarningDetail", "Customer", new { id = ac.ID });
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "RepWarning", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        // danh sách học viên có tư vấn viên nghỉ
        public ActionResult SupportOffList(int page = 1)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });


                string search = "";
                ViewBag.Search = Request.QueryString["search"];
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                    search = Request.QueryString["search"].ToString().Trim();

                int pageSize = 20;
                List<get_list_of_student_has_tvv_leave_Result> l = new List<get_list_of_student_has_tvv_leave_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.get_list_of_student_has_tvv_leave(search, pageSize, page - 1).ToList();
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
                AssetCRM.Writelog("CustomerController", "SupportOffList", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
            }

        }

        // danh sách học viên chuyển lớp
        public ActionResult ChangeCourse(int page = 1)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });


                string search = "";
                ViewBag.Search = Request.QueryString["search"];
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                    search = Request.QueryString["search"].ToString().Trim();
                int center = 0;
                if (Request.QueryString["center"].ToInt(0) != 0)
                {
                    center = ViewBag.CityID = Request.QueryString["center"].ToInt(0);
                }
                //sắp xếp
                int sort = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
                {
                    sort = Request.QueryString["sort"].ToInt(0);
                }
                ViewBag.Sort = sort;
                // thời gian bắt đầu từ
                // lấy ngày đầu và ngày cuối tháng
                DateTime date = DateTime.Now;
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                string fromdate = firstDayOfMonth.ToString("dd/MM/yyyy");
                ViewBag.From = firstDayOfMonth.ToString("dd/MM/yyyy");
                if (!string.IsNullOrEmpty(Request.QueryString["from"]))
                {
                    try
                    {
                        DateTime fdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                        ViewBag.From = fdate.ToString("dd/MM/yyyy");
                        fromdate = fdate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                string todate = lastDayOfMonth.ToString("dd/MM/yyyy");
                ViewBag.To = lastDayOfMonth.ToString("dd/MM/yyyy");
                // đến
                if (!string.IsNullOrEmpty(Request.QueryString["to"]))
                {
                    try
                    {
                        DateTime tdate = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null);
                        ViewBag.To = tdate.ToString("dd/MM/yyyy");
                        todate = tdate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                int pageSize = 20;
                List<get_list_of_changecourse_Result> l = new List<get_list_of_changecourse_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.get_list_of_changecourse(search, center, fromdate, todate, sort, pageSize, page - 1).ToList();
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
                AssetCRM.Writelog("CustomerController", "ChangeCourse", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }


        }

        // học viên sử dụng dịch vụ
        public ActionResult ServiceCustomerExam(int page = 1)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                //page size
                int pageSize = 20;
                //lấy hoc viên
                IEnumerable<tbl_ServicesStudent> l = ServicesStudentTable.getall().Where(n => n.ServiceExamsID != 0).OrderByDescending(n => n.ID).ToList();
                //key search
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                {
                    ViewBag.Search = Request.QueryString["search"];
                    string search = Request.QueryString["search"].ToString().Trim();

                    l = from ex in l
                        where AssetCRM.RemoveUnicode(ex.StudentName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        || AssetCRM.RemoveUnicode(ex.ServiceName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        || AssetCRM.RemoveUnicode(ex.SupplierServices).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        select ex;
                }
                //đợt thi
                string exams = "";
                if (Request.QueryString["exam"].ToInt(0) != 0)
                {
                    var exam = ServiceExamsTable.getbyID(Request.QueryString["exam"].ToInt(0));
                    l = from ex in l
                        where ex.ServiceExamsID == exam.ID
                        select ex;

                    exams = "<option value=\"" + exam.ID + "\">" + exam.ExamService + "</option>";
                }
                ViewBag.Exams = exams;
                //Loại thì thử hay thật
                if (Request.QueryString["type"].ToInt(0) != 0)
                {
                    int type = ViewBag.TypeID = Request.QueryString["type"].ToInt(0);
                    IEnumerable<tbl_ServiceExams> sve = ServiceExamsTable.getall();
                    if (type == 1)
                        sve = sve.Where(n => n.ExamReal == true).ToList();
                    if (type == 2)
                        sve = sve.Where(n => n.ExamReal == false).ToList();

                    l = from svexam in l
                        join ex in sve on svexam.ServiceExamsID equals ex.ID
                        select svexam;
                }
                //nhà cung cấp
                if (Request.QueryString["supplier"].ToInt(0) != 0)
                {
                    int supplier = ViewBag.SupplierID = Request.QueryString["supplier"].ToInt(0);
                    l = from ex in l
                        where ex.SupplierServicesID == supplier
                        select ex;
                }
                // thời gian thi từ
                if (!string.IsNullOrEmpty(Request.QueryString["fromexam"]))
                {
                    try
                    {
                        l = l.Where(n => n.ExamDate != null).ToList();
                        DateTime fromdate = DateTime.ParseExact(Request.QueryString["fromexam"].ToString(), "dd/MM/yyyy", null);
                        l = from ex in l
                            where ex.ExamDate.Value.Date >= fromdate.Date
                            select ex;
                        ViewBag.FromExam = fromdate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                // đến
                if (!string.IsNullOrEmpty(Request.QueryString["toexam"]))
                {
                    try
                    {
                        l = l.Where(n => n.ExamDate != null).ToList();
                        DateTime todate = DateTime.ParseExact(Request.QueryString["toexam"].ToString(), "dd/MM/yyyy", null);
                        l = from ex in l
                            where ex.ExamDate.Value.Date <= todate.Date
                            select ex;
                        ViewBag.ToExam = todate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                //sort
                int sort = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
                {
                    sort = Request.QueryString["sort"].ToInt(0);
                    if (sort == 1)
                        l = l.OrderBy(n => n.StudentName.Trim().Substring(n.StudentName.Trim().LastIndexOf(' ') + 1).ToString()).ToList();
                    if (sort == 2)
                        l = l.OrderByDescending(n => n.StudentName.Substring(n.StudentName.Trim().LastIndexOf(' ') + 1).ToString()).ToList();
                    if (sort == 3)
                        l = l.OrderBy(n => n.ExamDate).ToList();
                    if (sort == 4)
                        l = l.OrderByDescending(n => n.ExamDate).ToList();
                    if (sort == 5)
                        l = l.OrderBy(n => n.PriceService).ToList();
                    if (sort == 6)
                        l = l.OrderByDescending(n => n.PriceService).ToList();
                }
                ViewBag.Sort = sort;
                ViewBag.Total = l.Count();
                return View(l.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "ServiceCustomerExam", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        public class searchselect
        {
            public int id { get; set; }
            public string text { get; set; }
        }
        /// <summary>
        /// Search đợt thi ajax
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SearchServiceExam(string q)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                IEnumerable<tbl_ServiceExams> l = ServiceExamsTable.getall().OrderByDescending(n => n.ID).ToList();

                //search tên dự án 
                l = from ex in l
                    where AssetCRM.RemoveUnicode(ex.ExamService).ToUpper().Contains(AssetCRM.RemoveUnicode(q).ToUpper())
                    select ex;
                if (l.Count() > 20)
                    l = l.Skip(0).Take(20).ToList();
                List<searchselect> rs = new List<searchselect>();
                searchselect firt = new searchselect();
                firt.id = 0;
                firt.text = "All";
                rs.Add(firt);
                l = l.OrderBy(n => n.ExamService).ToList();
                foreach (var item in l)
                {
                    searchselect t = new searchselect();
                    t.id = item.ID;
                    t.text = item.ExamService;
                    rs.Add(t);
                }
                return Json(new { rs = false, l = rs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "SearchServiceExam", uid, ex.Message);
                //return RedirectToAction("Signin", "Login", new { area = "" });
                List<searchselect> rs = new List<searchselect>();
                return Json(new { rs = false, l = rs }, JsonRequestBehavior.AllowGet);
            }

        }
        // danh sách học viên muốn đăng ký
        public ActionResult CustomerAdvisory(int page = 1)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });


                string search = "";
                ViewBag.Search = Request.QueryString["search"];
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                    search = Request.QueryString["search"].ToString().Trim();
                int location = 0;
                if (Request.QueryString["location"].ToInt(0) != 0)
                {
                    location = ViewBag.CityID = Request.QueryString["location"].ToInt(0);
                }
                //Nguồn
                int source = 0;
                if (Request.QueryString["source"].ToInt(0) != 0)
                {
                    source = ViewBag.SourceID = Request.QueryString["source"].ToInt(0);
                }
                //công việc
                int jobs = 0;
                if (Request.QueryString["jobs"].ToInt(0) != 0)
                {
                    jobs = ViewBag.JobID = Request.QueryString["jobs"].ToInt(0);
                }
                //status
                int status = 0;
                if (Request.QueryString["status"].ToInt(0) != 0)
                {
                    status = ViewBag.StatusID = Request.QueryString["status"].ToInt(0);
                }
                //sắp xếp
                int sort = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
                {
                    sort = Request.QueryString["sort"].ToInt(0);
                }
                ViewBag.Sort = sort;
                // thời gian bắt đầu từ

                string fromdate = null;
                if (!string.IsNullOrEmpty(Request.QueryString["from"]))
                {
                    try
                    {
                        DateTime fdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                        ViewBag.From = fdate.ToString("dd/MM/yyyy");
                        fromdate = fdate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                string todate = null;
                // đến
                if (!string.IsNullOrEmpty(Request.QueryString["to"]))
                {
                    try
                    {
                        DateTime tdate = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null);
                        ViewBag.To = tdate.ToString("dd/MM/yyyy");
                        todate = tdate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                int pageSize = 20;
                List<get_list_of_customeradvisory_Result> l = new List<get_list_of_customeradvisory_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.get_list_of_customeradvisory(search, location, source, jobs, status, fromdate, todate, sort, pageSize, page - 1).ToList();
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
                AssetCRM.Writelog("CustomerController", "CustomerAdvisory", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]// lấy thông tin hẹn đăng ký về refund
        public ActionResult UpdateDoneAdvisory(int id)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var rs = AccountTable.getbyID(id);
                if (rs == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                else
                {
                    var ck = AccountTable.updateddvisory(rs.ID, false, aclog.UserName);
                    if (ck == null)
                        return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "UpdateDoneAdvisory", uid, ex.Message);
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]// lấy thông tin hẹn đăng ký về refund
        public ActionResult GetDistrict(int cityid)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var ds = DistrictTable.getbycityid(cityid);
                StringBuilder ap = new StringBuilder();
                foreach (var item in ds)
                {
                    ap.Append("<option value=\"" + item.ID + "\">" + item.DistrictName + "</option>");
                }
                return Json(new { rs = true, d = ap.ToString() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "GetDistrict", uid, ex.Message);
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]// lấy thông tin hẹn đăng ký về refund
        public ActionResult GetWardStreet(int districtid)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

            var wa = WardTable.getbydistrict(districtid);
            StringBuilder w = new StringBuilder();
            w.Append("<option value=\"0\">---</option>");
            foreach (var item in wa)
            {
                w.Append("<option value=\"" + item.ID + "\">" + item.WardName + "</option>");
            }

            var st = StreetTable.getbydistrict(districtid);
            StringBuilder s = new StringBuilder();
            s.Append("<option value=\"0\">---</option>");
            foreach (var item in st)
            {
                s.Append("<option value=\"" + item.ID + "\">" + item.StreetName + "</option>");
            }
            return Json(new { rs = true, w = w.ToString(), s = s.ToString() }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ReadyToTest(int page = 1)
        {
            try
            {
                var df = DateTime.ParseExact("01/01/1001", "dd/MM/yyyy", null).Date;
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 8 && aclog.RoleID != 9 && aclog.RoleID != 4 && aclog.RoleID != 6)// Super admin, quản lý, quản lý chuyên môn,kế toán,giao vien
                    return RedirectToAction("Signin", "Login", new { area = "" });

                if (aclog.SchoolID == null)
                    aclog.SchoolID = 0;

                //page size
                int pageSize = 20;

                string search = "";
                ViewBag.Search = Request.QueryString["search"];
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                    search = Request.QueryString["search"].ToString().Trim();

                SqlParameter[] parameters =
                {
                    new SqlParameter("@qsearch", SqlDbType.NVarChar,2000) { Value = search },
                    new SqlParameter("@count", SqlDbType.Int) { Value = 0 },
                    new SqlParameter("@Location", SqlDbType.Int) { Value = 0 },
                    new SqlParameter("@supportID", SqlDbType.Int) { Value = 0 },
                    new SqlParameter("@roleID", SqlDbType.Int) { Value = aclog.RoleID.Value },
                    new SqlParameter("@aclogID", SqlDbType.Int) { Value = aclog.ID },

                };
                var rs = SqlHelper.ExecuteDataTable("Get_Student_ReadyTo_Test", parameters);
                IEnumerable<tbl_Account> l = (from rw in rs.AsEnumerable()
                                              select new tbl_Account()
                                              {
                                                  ID = Convert.ToInt32(rw["ID"]),
                                                  LocationID = Convert.ToInt32(rw["LocationID"]),
                                                  SourceID = Convert.ToInt32(rw["SourceID"]),
                                                  JobID = Convert.ToInt32(rw["JobID"]),
                                                  Status = Convert.ToInt32(rw["Status"]),
                                                  DateExam = rw["DateExam"] == DBNull.Value ? df : Convert.ToDateTime(rw["DateExam"]),
                                                  FullName = rw["FullName"].ToString(),
                                                  SupportUID = Convert.ToInt32(rw["SupportUID"]),
                                                  SupportName = rw["SupportName"].ToString()
                                              }).ToList();
                //if (aclog.RoleID == 6)//tvv
                //    l = l.Where(n => n.SupportUID == aclog.ID).ToList();

                //if (aclog.RoleID == 2 || aclog.RoleID == 8)//manager||quan ly chuyen mon
                //    l = l.Where(n => n.LocationID == aclog.LocationID).ToList();
                ////Tỉnh thành
                if (Request.QueryString["location"].ToInt(0) != 0)
                {
                    int city = ViewBag.CityID = Request.QueryString["location"].ToInt(0);
                    l = l.Where(n => n.LocationID == city).ToList();
                }
                //Nguồn
                if (Request.QueryString["source"].ToInt(0) != 0)
                {
                    int source = ViewBag.SourceID = Request.QueryString["source"].ToInt(0);
                    l = l.Where(n => n.SourceID == source).ToList();
                }
                //công việc
                if (Request.QueryString["jobs"].ToInt(0) != 0)
                {
                    int jobs = ViewBag.JobID = Request.QueryString["jobs"].ToInt(0);
                    l = l.Where(n => n.JobID == jobs).ToList();
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
                        l = l.Where(n => n.DateExam != null && n.DateExam.Value.Date != df.Date).ToList();
                        DateTime fromdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);

                        l = from ex in l
                            where ex.DateExam.Value.Date >= fromdate.Date
                            select ex;
                        ViewBag.From = fromdate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        l = l.Where(n => n.DateExam != null && n.DateExam.Value.Date != df.Date).ToList();
                        DateTime fromdate = firstDayOfMonth;

                        l = from ex in l
                            where ex.DateExam.Value.Date >= fromdate.Date
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
                        l = l.Where(n => n.DateExam != null && n.DateExam.Value.Date != df.Date).ToList();
                        DateTime todate = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null);
                        l = from ex in l
                            where ex.DateExam.Value.Date <= todate.Date
                            select ex;
                        ViewBag.To = todate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        l = l.Where(n => n.DateExam != null && n.DateExam.Value.Date != df.Date).ToList();
                        DateTime todate = lastDayOfMonth;
                        l = from ex in l
                            where ex.DateExam.Value.Date <= todate.Date
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
                        l = l.OrderBy(n => n.FullName.Trim().Substring(n.FullName.Trim().LastIndexOf(' ') + 1).ToString()).ToList();
                    if (sort == 2)
                        l = l.OrderByDescending(n => n.FullName.Substring(n.FullName.Trim().LastIndexOf(' ') + 1).ToString()).ToList();
                    if (sort == 3)
                        l = l.OrderBy(n => n.SupportName.Substring(n.FullName.Trim().LastIndexOf(' ') + 1).ToString()).ToList();
                    if (sort == 4)
                        l = l.OrderByDescending(n => n.SupportName.Substring(n.FullName.Trim().LastIndexOf(' ') + 1).ToString()).ToList();
                    if (sort == 5)
                        l = l.OrderBy(n => n.DateExam).ToList();
                    if (sort == 6)
                        l = l.OrderByDescending(n => n.DateExam).ToList();
                }
                ViewBag.Sort = sort;
                ViewBag.Total = l.Count();
                return View(l.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "ReadyToTest", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost]
        public ActionResult UpdateTestResult(int uid, string date, string listen, string speaking, string reading, string writing, string overall, string note)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

                var account = AccountTable.getbyID(uid);
                if (account == null)
                {
                    return Json(new { rs = false, message = "Không tìm thấy học viên" }, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(listen) || string.IsNullOrEmpty(speaking) || string.IsNullOrEmpty(reading) || string.IsNullOrEmpty(writing) || string.IsNullOrEmpty(overall) || string.IsNullOrEmpty(note))
                {
                    return Json(new { rs = false, message = "Nhập đầy đủ thông tin" }, JsonRequestBehavior.AllowGet);
                }
                DateTime d = DateTime.ParseExact(date, "dd/MM/yyyy", null);
                double li = 0;
                double sp = 0;
                double re = 0;
                double wr = 0;
                double over = 0;
                try
                {
                    li = Convert.ToDouble(listen.TrimEnd('.'));
                    sp = Convert.ToDouble(speaking.TrimEnd('.'));
                    re = Convert.ToDouble(reading.TrimEnd('.'));
                    wr = Convert.ToDouble(writing.TrimEnd('.'));
                    over = Convert.ToDouble(overall.TrimEnd('.'));
                }
                catch
                {
                    return Json(new { rs = false, message = "Vui lòng nhập số" }, JsonRequestBehavior.AllowGet);
                }
                double target = (li + sp + re + wr) - (aclog.ScoreOut.ToFloat(0) * 4);
                int targetvalue = 3;
                if (target >= 0)
                    targetvalue = 2;
                var rs = CoursesExamTable.insert(0, "ĐIỂM THI THỰC TẾ", d, aclog.UserName);
                if (rs != null)
                {
                    var s = CoursesExamDetailTable.insert(rs.ID, 0, account.ID, li.ToString(), sp.ToString(), re.ToString(), wr.ToString(), note, over.ToString(), targetvalue, aclog.UserName);
                    return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                int aclogID = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "ReadyToTest", aclogID, ex.Message);
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            }

        }

        public class TestResults
        {
            public int ID { get; set; }
            public string FullName { get; set; }
            public string Phone { get; set; }
            public DateTime DateExam { get; set; }
            public double L { get; set; }
            public double S { get; set; }
            public double R { get; set; }
            public double W { get; set; }
            public double OverAll { get; set; }
            public double SubTotal { get; set; }
            public double CamKet { get; set; }
            public double Lech { get; set; }

            public int ColorID { get; set; }
            public string Color { get; set; }
        }
        [HttpGet]
        public ActionResult TestResult(int page = 1)
        {
            var df = DateTime.ParseExact("01/01/1001", "dd/MM/yyyy", null).Date;
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// Học viên thì đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            //page size
            int pageSize = 20;
            string search = "";
            string fromdate = null;
            string todate = null;
            ViewBag.Search = Request.QueryString["search"];
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                search = Request.QueryString["search"].ToString().Trim();
            // thời gian bắt đầu từ
            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                try
                {
                    DateTime fd = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                    fromdate = fd.ToString();
                    ViewBag.From = fd.ToString("dd/MM/yyyy");
                }
                catch { }
            }
            // đến
            if (!string.IsNullOrEmpty(Request.QueryString["to"]))
            {
                try
                {
                    DateTime td = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null);
                    todate = td.ToString();
                    ViewBag.To = td.ToString("dd/MM/yyyy");
                }
                catch { }
            }
            SqlParameter[] parameters =
            {
                new SqlParameter("@centerid", SqlDbType.Int) { Value = aclog.SchoolID.Value },
                new SqlParameter("@qsearch", SqlDbType.NVarChar,2000) { Value = search },
                new SqlParameter("@fromdate", SqlDbType.NVarChar,2000) { Value = fromdate },
                new SqlParameter("@todate", SqlDbType.NVarChar,2000) { Value = todate },
                new SqlParameter("@roleID", SqlDbType.Int) { Value = aclog.RoleID },
               new SqlParameter("@aclogID", SqlDbType.Int) { Value = aclog.ID },
            };
            ////NAM
            var rs = SqlHelper.ExecuteDataTable("Get_TestResult_Student", parameters);
            IEnumerable<TestResults> l = (from rw in rs.AsEnumerable()
                                          select new TestResults()
                                          {
                                              ID = Convert.ToInt32(rw["ID"]),
                                              FullName = rw["FullName"].ToString(),
                                              Phone = rw["Phone"].ToString(),
                                              DateExam = rw["DateExam"] == DBNull.Value ? df : Convert.ToDateTime(rw["DateExam"]),
                                              L = Convert.ToDouble(rw["L"]),
                                              S = Convert.ToDouble(rw["S"]),
                                              R = Convert.ToDouble(rw["R"]),
                                              W = Convert.ToDouble(rw["W"]),
                                              OverAll = Convert.ToDouble(rw["OverAll"]),
                                              SubTotal = Convert.ToDouble(rw["SubTotal"]),
                                              CamKet = Convert.ToDouble(rw["CamKet"]),
                                              Lech = Convert.ToDouble(rw["Lech"]),
                                              ColorID = Convert.ToInt32(rw["ColorID"]),
                                              Color = rw["Color"].ToString()
                                          }).ToList();

            //sort
            //l = l.OrderBy(n => n.FullName.Trim().Substring(n.FullName.Trim().LastIndexOf(' ') + 1).ToString()).ToList();

            if (Request.QueryString["status"].ToInt(0) != 0)
            {
                int status = Request.QueryString["status"].ToInt(0);
                l = l.Where(n => n.ColorID == status).ToList();
                ViewBag.Status = status;
            }
            l = l.OrderByDescending(n => n.DateExam).ToList();
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);

                if (sort == 1)
                    l = l.OrderBy(n => n.FullName).ToList();
                if (sort == 2)
                    l = l.OrderByDescending(n => n.FullName).ToList();
                if (sort == 3)
                    l = l.OrderBy(n => n.DateExam).ToList();
                if (sort == 4)
                    l = l.OrderByDescending(n => n.DateExam).ToList();
                if (sort == 5)
                    l = l.OrderBy(n => n.Lech).ToList();
                if (sort == 6)
                    l = l.OrderByDescending(n => n.Lech).ToList();
            }
            ViewBag.Sort = sort;
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }

        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult RepCusdetail(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var ac = AccountTable.getbyID(f.Get("hdfAcID").ToInt(0));
            string content = f.Get("hdfcontent").ToString();
            var rs = CoursesAttendanceStudentWarningTable.insert(ac.ID, 0, content, aclog.UserName, 2);
            if (rs != null)
            {
                //cập nhật cmt mới cho màn hình chi tiết học viên
                //if (w.ID != 0)
                //{
                //    CoursesAttendanceStudentTable.updatecmtnew(w.ID, content, aclog.UserName);
                //}
                List<tbl_Account> l = new List<tbl_Account>();
                var student = AccountTable.getbyID(ac.ID);
                var sup = AccountTable.getbyID(student.SupportUID.Value);
                if (sup != null)
                    l.Add(sup);
                //ds giáo viên dạy nó
                var lcourse = CoursesAttendanceStudentTable.getbyuid(student.ID).Where(n => n.Warning == true).ToList().Select(n => n.CourseID).Distinct();
                foreach (var item in lcourse)
                {
                    if (item != null)
                    {
                        List<tbl_TeacherCourse> lcc = TeacherCourseTable.getbycourseid(item.Value);
                        foreach (var item2 in lcc)
                        {
                            if (item2 != null)
                            {
                                tbl_Account teacher = AccountTable.getbyID(item2.TeacherUID.Value);
                                if (teacher != null)
                                {
                                    if (l.Where(n => n.UserName == teacher.UserName).FirstOrDefault() == null)
                                    {
                                        l.Add(teacher);
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (var acpush in l)
                {
                    if (acpush.ID == aclog.ID)
                        continue;
                    //gửi thông báo cho ng có liên quan
                    var nt = NotificationTable.insertsingle("", "Đánh giá điểm thi học viên có bình luận mới", "", acpush.ID, aclog.UserName, 13, ac.ID);

                }
                SetAlert("Thành công", "s");
            }
            return RedirectToAction("CustomerDetai", "Customer", new { id = ac.ID });
        }
        /// <summary>
        /// Xuất Excel danh sách học viên nơ học phí
        /// </summary>
        /// <param name="search"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExportExcelHVNoHocPhi(string search, int sort, int schoolID, string fHenthu, string tHenthu, string fNhaphoc, string tNhaphoc)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            if (aclog.RoleID != 1)
            {
                return Json(new { rs = false, message = "Truy cập bị từ chối!" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(search))
                search = "";
            //sort
            if (string.IsNullOrEmpty(sort.ToString()))
                sort = 0;
            if (string.IsNullOrEmpty(schoolID.ToString()))
                schoolID = 0;
            if (string.IsNullOrEmpty(fHenthu))
                fHenthu = null;
            if (string.IsNullOrEmpty(tHenthu))
                tHenthu = null;
            if (string.IsNullOrEmpty(fNhaphoc))
                fNhaphoc = null;
            if (string.IsNullOrEmpty(tNhaphoc))
                tNhaphoc = null;


            List<ASW_CustomerDebts_ExportExcel_Result> l = new List<ASW_CustomerDebts_ExportExcel_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_CustomerDebts_ExportExcel(search, schoolID, fHenthu, tHenthu, fNhaphoc, tNhaphoc, sort).ToList();
            }
            if (l.Count() == 0)
            {
                return Json(new { rs = false, message = "Không có dữ liệu!" }, JsonRequestBehavior.AllowGet);
            }
            StringBuilder strExport = new StringBuilder();
            strExport.Append("       <table border=\"1\">");
            strExport.Append("                     <tr>");
            strExport.Append("                        <th scope=\"col\">STT</th>");
            strExport.Append("                        <th scope=\"col\">Học viên</th>");
            strExport.Append("                        <th scope=\"col\">Số điện thoại</th>");
            strExport.Append("                        <th scope=\"col\">Khóa học</th>");
            strExport.Append("                        <th scope=\"col\">Trung tâm</th>");
            strExport.Append("                        <th scope=\"col\">Số tiền</th>");
            strExport.Append("                        <th scope=\"col\">Ngày hẹn thu</th>");
            strExport.Append("                        <th scope=\"col\">Ngày nhập học</th>");
            strExport.Append("                    </tr>");

            var index = 0;
            foreach (var i in l)
            {
                index++;
                string ngayhenthu = "";
                if (!string.IsNullOrEmpty(i.DatePriceLeft.ToString()))
                {
                    ngayhenthu = i.DatePriceLeft.Value.ToString("dd/MM/yyyy");
                }
                string ngaynhaphoc = "";
                if (!string.IsNullOrEmpty(i.CreatedDate.ToString()))
                {
                    ngaynhaphoc = i.CreatedDate.Value.ToString("dd/MM/yyyy");
                }
                strExport.Append("                    <tr>");
                strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.StudentName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.Phone + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.CourseName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.SchoolName + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + String.Format("{0:0,0}", i.PriceLeft) + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + ngayhenthu + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + ngaynhaphoc + "</td>");
                strExport.Append("                    </tr>");
            }
            strExport.Append("</table>");
            var filename = "DS_Hoc_Vien_No_Hoc_Phi_" + GetDateTime.Now.ToString("dd/MM/yyyy");
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ExportExcelCustomerReserve(string search, int sort, int schoolID, string fdate, string tdate, int classID, int studytimeID)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            if (aclog.RoleID != 1)
            {
                return Json(new { rs = false, message = "Truy cập bị từ chối!" }, JsonRequestBehavior.AllowGet);
            }
            if (fdate == "")
                fdate = null;
            if (tdate == "")
                tdate = null;
            List<ASW_CustomerReserve_ExportExcel_Result> l = new List<ASW_CustomerReserve_ExportExcel_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_CustomerReserve_ExportExcel(search, schoolID, classID, studytimeID, fdate, tdate, sort).ToList();
            }
            if (l.Count() == 0)
            {
                return Json(new { rs = false, noti = "Dữ liệu trống!" }, JsonRequestBehavior.AllowGet);
            }

            StringBuilder strExport = new StringBuilder();
            strExport.Append("       <table border=\"1\">");
            strExport.Append("                     <tr>");
            strExport.Append("                        <th scope=\"col\">STT</th>");
            strExport.Append("                        <th scope=\"col\">Học viên</th>");
            strExport.Append("                        <th scope=\"col\">Tỉnh/TP</th>");
            strExport.Append("                        <th scope=\"col\">Trung tâm</th>");
            strExport.Append("                        <th scope=\"col\">Lớp</th>");
            strExport.Append("                        <th scope=\"col\">Ca</th>");
            strExport.Append("                        <th scope=\"col\">Đóng thêm</th>");
            strExport.Append("                        <th scope=\"col\">Số điện thoại</th>");
            strExport.Append("                        <th scope=\"col\">Ghi chú</th>");
            strExport.Append("                        <th scope=\"col\">Ngày bảo lưu</th>");
            strExport.Append("                        <th scope=\"col\">Hạn bảo lưu</th>");
            strExport.Append("                    </tr>");

            var index = 0;
            foreach (var i in l)
            {
                index++;
                strExport.Append("                    <tr>");
                strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.StudentName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.CityName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.SchoolName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.ClassName + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.StudyName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + String.Format("{0:0,0}", i.Deposit) + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.StudentPhone + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.Note + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.CreatedDate.Value.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.DeadLineReserve.Value.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                    </tr>");
            }
            strExport.Append("</table>");
            var filename = "DS_Khach_Hang_Bao_Luu" + GetDateTime.Now.ToString("dd/MM/yyyy");
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Danh sách câu hỏi thường gặp
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult FAQ(int page = 1)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                //page size
                int pageSize = 20;

                //lấy hoc viên
                IEnumerable<tbl_FAQ> l = FAQTable.GetAll().Where(x => x.RoleID == aclog.RoleID || x.RoleID == 0).OrderByDescending(n => n.ID).ToList();
                if (aclog.RoleID == 1)
                    l = FAQTable.GetAll().OrderByDescending(n => n.ID).ToList();
                //key search
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                {
                    ViewBag.Search = Request.QueryString["search"];
                    string search = Request.QueryString["search"].ToString().Trim();

                    l = from ex in l
                        where AssetCRM.RemoveUnicode(ex.TitleQuestion).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        select ex;
                }
                ViewBag.Total = l.Count();
                return View(l.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "ServiceCustomer", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddUpdateFAQ(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int ID = f.Get("hdf-ID").ToInt(0);
            string title = f.Get("txt-title").ToString();
            string answer = f.Get("txt-answer").ToString();
            int role = f.Get("slRole").ToInt(0);
            if (ID == 0)
            {
                var rs = FAQTable.Insert(role, title, answer, aclog.UserName);
                if (rs != null)
                    SetAlert("Thành công", "s");
                else
                    SetAlert("Không thành công", "e");
            }
            else
            {
                var rs = FAQTable.Update(ID, role, title, answer, aclog.UserName);
                if (rs != null)
                    SetAlert("Thành công", "s");
                else
                    SetAlert("Không thành công", "e");
            }
            return RedirectToAction("FAQ", "Customer");
        }
        [HttpPost]
        public ActionResult LoadFAQDetail(int ID)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var sc = FAQTable.GetByID(ID);
                if (sc == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { rs = true, title = sc.TitleQuestion, answer = sc.AnswerContent, role = sc.RoleID }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "LoadFAQDetail", uid, ex.Message);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult ShowVsHidden(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var d = FAQTable.GetByID(id);
            if (d != null)
                FAQTable.UpdateIshide(d.ID, !d.isHide.Value, aclog.UserName);
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Xem lịch xử thanh toán cho học viên
        /// </summary>
        /// <returns></returns>
        public ActionResult HistoryPaidCustomer()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {

                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var t = AccountTable.getbyID(aclog.ID);
                if (t == null)
                {
                    SetAlert("Không tìm thấy học viên này :(", "e");
                    return RedirectToAction("ProfileUser", "Account");
                }
                return View(t);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("CustomerController", "HistoryPaidCustomer", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
            }

        }

        /// <summary>
        /// Danh sách khách cần tư vấn
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult AccountAdvisory(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = ViewBag.Search = Request.QueryString["search"].ToString();
            }
            int status = 2;
            if (!string.IsNullOrEmpty(Request.QueryString["status"]))
            {
                ViewBag.Status = status = Request.QueryString["status"].ToInt(0);
            }
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                ViewBag.Sort = sort = Request.QueryString["sort"].ToInt(0);
            }
            List<ASW_AccountCanTuVan_Result> l = new List<ASW_AccountCanTuVan_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_AccountCanTuVan(search, sort, status, null, null, (page - 1) * pageSize, pageSize).ToList();
            }
            ViewBag.StartIndex = (page - 1) * pageSize;

            int total = 0;
            if (l.Count > 0)
            {
                total = l[0].TotalRow.Value;
            }
            ViewBag.Total = l.Count();
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            return View(l);
        }

        [HttpPost]
        public ActionResult ShowVsHiddenAccountAdvisory(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var d = AccountAdvisoryTable.GetByID(id);
            if (d != null)
                AccountAdvisoryTable.UpdateStatus(d.ID, !d.StatusAdvisory.Value, aclog.UserName);
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateAccountAdvisorySeller(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int ID = f.Get("hdfID").ToInt(0);
            int seller = f.Get("ddl-seller").ToInt(0);
            var sel = AccountTable.getbyID(seller);
            if (sel == null)
            {
                SetAlert("Không tìm thấy seller", "e");
                return RedirectToAction("AccountAdvisory", "Customer");
            }
            var rs = AccountAdvisoryTable.UpdateSeller(ID, seller, sel.FullName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("AccountAdvisory", "Customer");
        }
        [HttpPost]
        public ActionResult UpdateAccountAdvisoryNote(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int ID = f.Get("hdfID-note").ToInt(0);
            string note = f.Get("txt-note").ToString();
            var rs = AccountAdvisoryTable.UpdateNote(ID, note);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("AccountAdvisory", "Customer");
        }

        [HttpPost]
        public ActionResult UpFileChungChi(FormCollection f, HttpPostedFileBase[] fileupload)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });
            if (aclog.RoleID != 1 && aclog.RoleID != 6 && aclog.RoleID != 7)
            {
                SetAlert("Bạn không có quyền", "e");
                return RedirectToAction("ServiceCustomer", "Customer");
            }
            var ser = ServicesStudentTable.getbyid(f.Get("hdfSerID").ToInt(0));
            if (ser == null)
            {
                SetAlert("Không tìm thấy dịch vụ", "e");
                return RedirectToAction("ServiceCustomer", "Customer");
            }

            if (ModelState.IsValid)
            {
                foreach (HttpPostedFileBase file in fileupload)
                {
                    //Checking file is available to save.  
                    if (file != null)
                    {
                        string name = Path.GetFileNameWithoutExtension(file.FileName);
                        string ext = Path.GetExtension(file.FileName);
                        if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                        {
                            SetAlert("File của bạn đã bị chặn, vui lòng không tải lên file .php, .asp, .aspx, .ps1", "e");
                            return RedirectToAction("ServiceCustomer", "Customer");
                        }
                        var fileName = name + " " + GetDateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss") + ext;
                        var path = Path.Combine(Server.MapPath("~/Upload/CertificateExams/"), fileName);
                        string link = "/Upload/CertificateExams/" + fileName;
                        file.SaveAs(path);
                        ServicesStudentTable.UpdateFileChungChi(ser.ID, link);
                    }
                }
            }
            SetAlert("Thành công", "s");
            return RedirectToAction("ServiceCustomer", "Customer");
        }

        [HttpPost]
        public ActionResult UpFileChungChiCourse(FormCollection f, HttpPostedFileBase[] fileupload)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });
            if (aclog.RoleID != 1 && aclog.RoleID != 6 && aclog.RoleID != 7)
            {
                SetAlert("Bạn không có quyền", "e");
                return RedirectToAction("CustomerList", "Customer");
            }
            var course = CoursesStudentTable.getbyid(f.Get("hdfCourseID").ToInt(0));
            if (course == null)
            {
                SetAlert("Không tìm thấy khóa học", "e");
                return RedirectToAction("CustomerDetai", "Customer", new { id = course.StudentUID.Value });
            }

            if (ModelState.IsValid)
            {
                foreach (HttpPostedFileBase file in fileupload)
                {
                    //Checking file is available to save.  
                    if (file != null)
                    {
                        string name = Path.GetFileNameWithoutExtension(file.FileName);
                        string ext = Path.GetExtension(file.FileName);
                        if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                        {
                            SetAlert("File của bạn đã bị chặn, vui lòng không tải lên file .php, .asp, .aspx, .ps1", "e");
                            return RedirectToAction("CustomerDetai", "Customer", new { id = course.StudentUID.Value });
                        }
                        var fileName = name + " " + GetDateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss") + ext;
                        var path = Path.Combine(Server.MapPath("~/Upload/CertificateCourseStudent/"), fileName);
                        string link = "/Upload/CertificateCourseStudent/" + fileName;
                        file.SaveAs(path);
                        CoursesStudentTable.UpdateChungChi(course.ID, link);
                    }
                }
            }
            SetAlert("Thành công", "s");
            return RedirectToAction("CustomerDetai", "Customer", new { id = course.StudentUID.Value });
        }

        public ActionResult ServiceByStudent(int page = 1)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

                //page size
                int pageSize = 20;
                //lấy hoc viên
                IEnumerable<tbl_ServicesStudent> l = ServicesStudentTable.getall().Where(x => x.StudentUID == aclog.ID).OrderByDescending(n => n.ID).ToList();
                //key search
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                {
                    ViewBag.Search = Request.QueryString["search"];
                    string search = Request.QueryString["search"].ToString().Trim();

                    l = from ex in l
                        where AssetCRM.RemoveUnicode(ex.StudentName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        || AssetCRM.RemoveUnicode(ex.ServiceName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        || AssetCRM.RemoveUnicode(ex.SupplierServices).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        select ex;
                }
                ViewBag.Total = l.Count();
                return View(l.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "ServiceByStudent", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }
        public ActionResult CustomerDebtsDataTable()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                return View();
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("CustomerController", "CustomerDebtsDataTable", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
            }

        }

        [HttpGet]
        public ActionResult LoadCustomerDebtsDataTable()
        {
            OjectDataTable dataTable = new OjectDataTable();
            if (Session["UID"] == null)
                return Json(dataTable, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
            {
                return Json(dataTable, JsonRequestBehavior.AllowGet);
            }
            //page size 
            dataTable.draw = int.Parse(Request.QueryString["draw"]);
            //bắt đầu từ vị trí nào
            int Start = Request.QueryString["start"] == null ? 0 : Request.QueryString["start"].ToInt(0);
            //độ dài mỗi trang
            int Length = Request.QueryString["length"] == null ? 0 : Request.QueryString["length"].ToInt(0);
            //index orderby
            int OrderBy = Request.QueryString["order"] == null ? 0 : Request.QueryString["order"].ToInt(0);
            //order by theo asc hay desc
            string Dir = Request.QueryString["dir"] == null ? "" : Request.QueryString["dir"].ToString().Trim();
            if (Dir.ToUpper().Contains("DESC"))
            {
                OrderBy += 6;// 6 là số colunm màn hình
            }

            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                search = Request.QueryString["search"].ToString().Trim();
            //center
            int center = 0;
            if (Request.QueryString["center"].ToInt(0) != 0)
            {
                center = Request.QueryString["center"].ToInt(0);
            }
            //sắp xếp
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);
            }
            // thời gian bắt đầu từ
            string fromdate = null;
            string todate = null;
            string fromdateleft = null;
            string todateleft = null;
            if (!string.IsNullOrEmpty(Request.QueryString["fHenthu"]))
                try
                {
                    DateTime fdateleft = DateTime.ParseExact(Request.QueryString["fHenthu"].ToString(), "dd/MM/yyyy", null);
                    fromdateleft = fdateleft.ToString("dd/MM/yyyy");
                }
                catch { }
            // đến
            if (!string.IsNullOrEmpty(Request.QueryString["tHenthu"]))
                try
                {
                    DateTime tdateleft = DateTime.ParseExact(Request.QueryString["tHenthu"].ToString(), "dd/MM/yyyy", null);
                    todateleft = tdateleft.ToString("dd/MM/yyyy");
                }
                catch { }

            // thời gian bắt đầu từ
            if (!string.IsNullOrEmpty(Request.QueryString["fNhaphoc"]))
            {
                try
                {
                    DateTime fdate = DateTime.ParseExact(Request.QueryString["fNhaphoc"].ToString(), "dd/MM/yyyy", null);
                    fromdate = fdate.ToString("dd/MM/yyyy");
                }
                catch { }
            }
            // đến
            if (!string.IsNullOrEmpty(Request.QueryString["tNhaphoc"]))
            {
                try
                {
                    DateTime tdate = DateTime.ParseExact(Request.QueryString["tNhaphoc"].ToString(), "dd/MM/yyyy", null);
                    todate = tdate.ToString("dd/MM/yyyy");
                }
                catch { }
            }

            List<get_list_of_CustomerDebts_DataTable_Result> l = new List<get_list_of_CustomerDebts_DataTable_Result>();
            using (var db = new ZimEntities())
            {
                l = db.get_list_of_CustomerDebts_DataTable(search, center, fromdateleft, todateleft, fromdate, todate, OrderBy, Length, Start, aclog.ID, aclog.RoleID.Value).ToList();
            }
            int total = 0;
            if (l.Count > 0)
            {
                total = l[0].TotalRow.Value;
            }
            //dữ liệu
            dataTable.data = l.ToArray();
            //số tượng item
            dataTable.recordsTotal = total;
            //phân trang
            dataTable.recordsFiltered = total;
            return Json(dataTable, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportCustomerDebtsDataTable()
        {
            if (Session["UID"] == null)
            {
                SetAlert("Hết phiên làm việc!", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
            {
                SetAlert("Bạn không có quyền thao tác!", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }

            //index orderby
            int OrderBy = Request.QueryString["order"] == null ? 0 : Request.QueryString["order"].ToInt(0);
            //order by theo asc hay desc
            string Dir = Request.QueryString["dir"] == null ? "" : Request.QueryString["dir"].ToString().Trim();
            if (Dir.ToUpper().Contains("DESC"))
            {
                OrderBy += 6;// 6 là số colunm màn hình
            }
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                search = Request.QueryString["search"].ToString().Trim();
            //center
            int center = 0;
            if (Request.QueryString["center"].ToInt(0) != 0)
            {
                center = Request.QueryString["center"].ToInt(0);
            }
            //sắp xếp
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);
            }
            // thời gian bắt đầu từ
            string fromdate = null;
            string todate = null;
            string fromdateleft = null;
            string todateleft = null;
            if (!string.IsNullOrEmpty(Request.QueryString["fHenthu"]))
                try
                {
                    DateTime fdateleft = DateTime.ParseExact(Request.QueryString["fHenthu"].ToString(), "dd/MM/yyyy", null);
                    fromdateleft = fdateleft.ToString("dd/MM/yyyy");
                }
                catch { }
            // đến
            if (!string.IsNullOrEmpty(Request.QueryString["tHenthu"]))
                try
                {
                    DateTime tdateleft = DateTime.ParseExact(Request.QueryString["tHenthu"].ToString(), "dd/MM/yyyy", null);
                    todateleft = tdateleft.ToString("dd/MM/yyyy");
                }
                catch { }

            // thời gian bắt đầu từ
            if (!string.IsNullOrEmpty(Request.QueryString["fNhaphoc"]))
            {
                try
                {
                    DateTime fdate = DateTime.ParseExact(Request.QueryString["fNhaphoc"].ToString(), "dd/MM/yyyy", null);
                    fromdate = fdate.ToString("dd/MM/yyyy");
                }
                catch { }
            }
            // đến
            if (!string.IsNullOrEmpty(Request.QueryString["tNhaphoc"]))
            {
                try
                {
                    DateTime tdate = DateTime.ParseExact(Request.QueryString["tNhaphoc"].ToString(), "dd/MM/yyyy", null);
                    todate = tdate.ToString("dd/MM/yyyy");
                }
                catch { }
            }

            List<get_list_of_CustomerDebts_DataTable_Result> l = new List<get_list_of_CustomerDebts_DataTable_Result>();
            using (var db = new ZimEntities())
            {
                l = db.get_list_of_CustomerDebts_DataTable(search, center, fromdateleft, todateleft, fromdate, todate, OrderBy, int.MaxValue, 0, aclog.ID, aclog.RoleID.Value).ToList();
            }

            var dataTable = new System.Data.DataTable("CustomerDebts");
            dataTable.Columns.Add("Học viên", typeof(string));
            dataTable.Columns.Add("Số điện thoại", typeof(string));
            dataTable.Columns.Add("Khóa học", typeof(string));
            dataTable.Columns.Add("Số tiền", typeof(string));
            dataTable.Columns.Add("Ngày hẹn thu", typeof(string));
            dataTable.Columns.Add("Ngày nhập học", typeof(string));
            for (int i = 0; i < l.Count; i++)
            {
                var item = l[i];
                var d = dataTable.Rows.Add(
                                    item.StudentName,
                                    item.Phone,
                                    item.CourseName,
                                    String.Format("{0,0:0}", item.PriceLeft),
                                     item.DatePriceLeft,
                                     item.CreatedDate
                );
            }
            CultureInfo cult = new CultureInfo("vi-VN");
            var grid = new GridView();
            grid.DataSource = dataTable;

            grid.DataBind();
            Response.ClearContent();
            Response.Buffer = true;

            Response.AddHeader("content-disposition", "attachment; filename=DanhSachHocVienNoHocPhi.xls");
            Response.ContentType = "application/ms-excel";
            Response.ContentEncoding = System.Text.Encoding.Unicode;
            Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            grid.HeaderStyle.Font.Bold = true;
            grid.ShowFooter = true;
            grid.BorderStyle = BorderStyle.Solid;
            grid.RenderControl(htw);
            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();
            return RedirectToAction("CustomerDebtsDataTable", "Customer", new { area = "Admin" });
        }

        [HttpGet]
        public ActionResult CustomerAboutToExpire(int page = 1)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3 && aclog.RoleID != 6)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                //page size
                int pageSize = 20;
                string search = "";
                ViewBag.Search = Request.QueryString["search"];
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                    search = Request.QueryString["search"].ToString().Trim();

                int searchMail = -1;
                if (!string.IsNullOrEmpty(Request.QueryString["searchMail"]))
                    searchMail = Request.QueryString["searchMail"].ToInt(-1);
                ViewBag.SearchMail = searchMail;

                var l = new List<Admin_Student_3LessonsLeftList_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.Admin_Student_3LessonsLeftList(search, searchMail, (page - 1) * pageSize, pageSize, aclog.ID, aclog.RoleID).ToList();
                }
                int total = 0;
                if (l.Count > 0)
                {
                    total = ViewBag.Total = l[0].TotalRow;
                }
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

                AssetCRM.Writelog("CustomerController", "CustomerList", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult ImportExcel(HttpPostedFileBase file)
        {
            string link = "";
            if (file == null)
                return RedirectToAction("CustomerList", "Customer", new { area = "Admin" });
            string name = Path.GetFileNameWithoutExtension(file.FileName);
            string ext = Path.GetExtension(file.FileName);
            if (ext != ".xlsx" && ext != ".csv" && ext != ".xls")
                return RedirectToAction("CustomerList", "Customer", new { area = "Admin" });
            var fileName = Guid.NewGuid() + ext;
            var path = Path.Combine(Server.MapPath("~/Upload/classdocument/"), fileName);
            string strPathAndQuery = Request.Url.PathAndQuery;
            string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
            link = strUrl + "Upload/classdocument/" + fileName;
            file.SaveAs(path);
            var _xl = new Application();
            var wb = _xl.Workbooks.Open(link);
            var sheets = wb.Sheets;
            if (sheets != null && sheets.Count != 0)
            {
                foreach (var item in sheets)
                {
                    var sheet = (Worksheet)item;
                    System.Data.DataTable dt = null;
                    if (sheet != null)
                    {
                        dt = new System.Data.DataTable();
                        var ColumnCount = ((Range)sheet.UsedRange.Rows[1, Type.Missing]).Columns.Count;
                        var rowCount = ((Range)sheet.UsedRange.Columns[2, Type.Missing]).Rows.Count;

                        for (int j = 0; j < ColumnCount; j++)
                        {
                            var cell = (Range)sheet.Cells[1, j + 1];
                            var column = new DataColumn(true ? cell.Value : string.Empty);
                            dt.Columns.Add(column);
                        }

                        for (int i = 0; i < rowCount - 1; i++)
                        {
                            var r = dt.NewRow();
                            for (int j = 0; j < ColumnCount; j++)
                            {
                                var cell = (Range)sheet.Cells[i + 1 + (true ? 1 : 0), j + 1];
                                r[j] = cell.Value;
                            }
                            dt.Rows.Add(r);
                        }
                    }
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            var p = new ExcelObject
                            {
                                PhoneNumber = dr[0] == null ? "" : dr[0].ToString(),
                                FullName = dr[1] == null ? "" : dr[1].ToString(),
                                Email = dr[2] == null ? "" : dr[2].ToString(),
                                BirthDay = dr[3] == null ? "" : dr[3].ToString(),
                                Address = dr[4] == null ? "" : dr[4].ToString()
                            };
                            AccountTable.ImportExcel(p);
                        }
                    }
                }
            }
            _xl.Quit();
            return RedirectToAction("CustomerList", "Customer", new { area = "Admin" });
        }

        [HttpPost]
        public ActionResult GuiMail(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                int studentID = f.Get("studentID").ToInt(0);
                int courseID = f.Get("courseID").ToInt(0);
                string tieude = f.Get("tieude").ToString();
                string noidung = f.Get("noidung").ToString();
                var user = AccountTable.getbyID(studentID);
                AssetCRM.SendMail(user.Email, tieude, noidung);
                // sau khi gui mail -> check SendMail = true
                using (var db = new ZimEntities())
                {
                    var check = db.tbl_CoursesStudent.Where(x => x.CourseID == courseID && x.StudentUID == studentID).FirstOrDefault();
                    check.SendMail = true;
                    db.SaveChanges();
                }
                SetAlert("Thành công", "s");
            }
            catch (Exception e)
            {
                SetAlert("Lỗi", "e");
            }
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }
        public ActionResult RevenueDetail(int page = 1)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                //page size
                int pageSize = 20;

                string search = "";
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                    search = Request.QueryString["search"].ToString().Trim();
                ViewBag.Search = search;

                int counselors = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["slTvv"]))
                {
                    counselors = Request.QueryString["slTvv"].ToInt(0);
                }
                ViewBag.Counselors = counselors;

                string fromdate = DateTime.Now.FirstDayOfMonth().ToString("yyyy-MM-dd 00:00:00.000");
                ViewBag.From = DateTime.Now.FirstDayOfMonth().ToString("dd/MM/yyyy");
                // thời gian bắt đầu từ
                if (!string.IsNullOrEmpty(Request.QueryString["from"]))
                {
                    try
                    {
                        DateTime fd = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                        fromdate = fd.ToString("yyyy-MM-dd 00:00:00.000");
                        ViewBag.From = fd.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                // đến
                string todate = DateTime.Now.LastDayOfMonth().ToString("yyyy-MM-dd 00:00:00.000");
                ViewBag.To = DateTime.Now.LastDayOfMonth().ToString("dd/MM/yyyy");
                if (!string.IsNullOrEmpty(Request.QueryString["to"]))
                {
                    try
                    {

                        DateTime td = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null);
                        todate = td.ToString("yyyy-MM-dd 23:59:59.998");
                        ViewBag.To = td.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                var l = new List<RevenueDetail_GetList_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.RevenueDetail_GetList(search, counselors, fromdate, todate, (page - 1) * pageSize, pageSize).ToList();
                }
                int total = 0;
                if (l.Count > 0)
                    total = l[0].TotalRow.Value;
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
                AssetCRM.Writelog("CustomerController", "RevenueDetail", Session["UID"].ToString().ToInt(0), ex.Message + "||" + ex.InnerException);
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }
        [HttpPost]
        public ActionResult CheckDisCountCode(int id, string code)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var appoinment = StudentAppointmentTable.getbyid(id);
                if (appoinment == null)
                    return Json(new { rs = false, message = "Không tìm thấy dữ liệu" }, JsonRequestBehavior.AllowGet);

                if (appoinment.DiscountID != 0)
                    return Json(new { rs = false, message = "Học viên này đã sử dụng mã khuyến mãi" }, JsonRequestBehavior.AllowGet);

                var discount = DiscountTable.getbycode(code);
                return Json(new { rs = true, message = "Thành công", data = discount }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("CustomerController", "CheckDisCountCode", 0, ex.Message + "||" + ex.InnerException);
                return Json(new { rs = false, message = "Không tìm thấy dữ liệu" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult LoadProgramBylanguage(int lang)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var programs = ClassTable.GetByLanguage(lang);
                var html = new StringBuilder();
                foreach (var i in programs)
                {
                    html.Append(" <option value=\"" + i.ID + "\">" + i.ClassName + " - " + String.Format("{0:0,0}", i.PriceClass) + "</option>");
                }
                return Json(new { rs = true, message = "Thành công", data = html.ToString() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("CustomerController", "LoadProgramBylanguage", 0, ex.Message + "||" + ex.InnerException);
                return Json(new { rs = false, message = "Không tìm thấy dữ liệu" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteDesrire(int id)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var rs = DesiredStudyTimeTable.Delete(id);
                if (rs != null)
                    return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
                else return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("CustomerController", "DeleteDesrire", 0, ex.Message + "||" + ex.InnerException);
                return Json(new { rs = false, message = "Không tìm thấy dữ liệu" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Lay danh sach ngay ma hv muon hoc
        /// </summary>
        [HttpPost]
        public ActionResult GetListWantToDay(int studentappointid)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var appointment = StudentAppointmentTable.getbyid(studentappointid);
            var day = appointment.ListWantToDay?.Split(',')?.FirstOrDefault();
            var desireDates = DesiredStudyTimeTable.getbyStudentAppointmentID(studentappointid);
            var cahoc = StudyTimeTable.getall().Where(x => x.isHide == false).OrderBy(x => x.sTime).ToList();
            StringBuilder html = new StringBuilder();
            for (var i = 0; i < desireDates.Count; i++)
            {
                html.Append("<div class=\"row mg-b-5\" id=\"div-id-" + desireDates[i].ID + "\">");
                html.Append("   <div class=\"col-5\">");
                html.Append("   <select name=\"slDay\" class=\"form-control select2\">");
                for (var d = 0; d < 7; d++)
                {
                    if (d == desireDates[i].DayOfWeekID)
                    {
                        html.Append("<option value=\"" + d + "\" selected >" + Enum.GetName(typeof(DayOfWeek), d) + "</option>");
                    }
                    else
                    {
                        html.Append("<option value=\"" + d + "\">" + Enum.GetName(typeof(DayOfWeek), d) + "</option>");
                    }
                }
                html.Append("   </select>");
                html.Append("   </div>");
                html.Append("   <div class=\"col-5\">");
                html.Append("   <select name=\"slStudytime\" class=\"form-control select2\">");
                foreach (var s in cahoc)
                {
                    if (s.ID == desireDates[i].StudyTimeID)
                    {
                        html.Append("<option value=\"" + s.ID + "\" selected >" + s.sTime + " - " + s.eTime + "</option>");
                    }
                    else
                    {
                        html.Append("<option value=\"" + s.ID + "\">" + s.sTime + " - " + s.eTime + "</option>");
                    }
                }
                html.Append("   </select>");
                html.Append("   </div>");
                html.Append("   <div>");
                html.Append("   <button type=\"button\" class=\"btn\" style=\"color:red;\" onclick=\"DeleteInDB(" + desireDates[i].ID + ")\" ><i class=\"far fa-times-circle\"></i>");
                html.Append("   </button>");
                html.Append("   </div>");
                html.Append("</div>");
            }
            return Json(new { rs = true, data = html.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateAppoinment(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var id = f.Get("hdId").ToInt(0);
            var studentappoinment = StudentAppointmentTable.getbyid(id);
            if (studentappoinment == null)
            {
                SetAlert("Không tìm thấy dữ liệu", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }

            var lang = f.Get("slLanguage-edit").ToInt(0);
            var program = f.Get("slProgram-edit").ToInt(0);
            var discount = "";
            if (!string.IsNullOrEmpty(f.Get("txt-discount-code")))
            {
                discount = f.Get("txt-discount-code").ToString();
            }
            var note = "";
            if (!string.IsNullOrEmpty(f.Get("txt-note")))
            {
                note = f.Get("txt-note").ToString();
            }
            var teacher = 0;
            if (!string.IsNullOrEmpty(f.Get("slTeacher")))
            {
                teacher = f.Get("slTeacher").ToString().ToInt(0);
            }
            
            DateTime? createdDate = null;
            if (!string.IsNullOrEmpty(f.Get("txt-from")))
            {
                createdDate = DateTime.ParseExact(f.Get("txt-from").ToString(), "dd/MM/yyyy", null);
            }
            var commitment = f.Get("slCommitment").ToInt(0);
            var paymentMethod = f.Get("slPaymentMethod").ToInt(0);
            var totalPrice = f.Get("txt-total").Replace(",", "").ToInt(0);
            var rs = StudentAppointmentTable.edit(id, program, discount, commitment, paymentMethod, totalPrice, note, aclog.UserName, teacher, createdDate);
            if (rs != null)
            {
                string dayofweekIDs = "";
                if (!string.IsNullOrEmpty(f.Get("want-to-day")))
                {
                    dayofweekIDs = f.Get("want-to-day").ToString();
                }
                string studyingTimeIDs = "";
                if (!string.IsNullOrEmpty(f.Get("ddl-study-combo")))
                {
                    studyingTimeIDs = f.Get("ddl-study-combo").ToString();
                }
                List<string> dayofweekIDList = dayofweekIDs.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToList();
                List<string> studyingTimeIDList = studyingTimeIDs.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToList();
                for (int i = 0; i < dayofweekIDList?.Count(); i++)
                {
                    var insertDesiredStudyTime = new tbl_DesiredStudyTime
                    {
                        StudentAppointmentID = studentappoinment.ID,
                        DayOfWeekID = int.Parse(dayofweekIDList[i]),
                        StudyTimeID = int.Parse(studyingTimeIDList[i])
                    };
                    DesiredStudyTimeTable.insert(insertDesiredStudyTime, aclog.FullName);
                }

                var history = CampaignSaleHistoryTable.getbyAppointmentID(studentappoinment.ID);
                if (history != null)
                {
                    //update lại tiền doanh thu
                    UpdateCampaignSale(history.TargetID ?? 0, totalPrice - studentappoinment.CustomPrice ?? 0);
                }

                SetAlert("Thành công", "s");
            }
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }
        [HttpPost]
        public ActionResult LoadProgram(int id)
        {
            var programs = ClassTable.GetByGradeID(id);
            StringBuilder html = new StringBuilder();
            foreach (var i in programs)
            {
                html.Append("<input class=\"checkbox-tools\" type=\"radio\" name=\"slProgram\" id=\"program-" + i.ID + "\" value=\"" + i.ID + "\" >");
                html.Append("<label for=\"program-" + i.ID + "\">" + i.ClassName + "</label>");
            }
            return Json(new { data = html.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadGrade(int id)
        {
            var grades = GradeTable.GetByLanguage(id);
            StringBuilder html = new StringBuilder();
            foreach (var i in grades)
            {
                html.Append("<input class=\"checkbox-tools\" type=\"radio\" name=\"slGrade\" id=\"grades-" + i.ID + "\" value=\"" + i.ID + "\" >");
                html.Append("<label for=\"grades-" + i.ID + "\" onclick=\"LoadProgram(" + i.ID + ")\" >" + i.GradeName + "</label>");
            }
            return Json(new { data = html.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadCourse(int id)
        {
            var l = new List<Admin_Course_GetByAppoitment_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_Course_GetByAppoitment(id).ToList();
            }
            StringBuilder html = new StringBuilder();
            html.Append("<option value=\"0\">---</option>");
            foreach (var i in l)
            {
                html.Append("<option value=\"" + i.ID + "\">" + i.CourseName + "[" + i.FisnishSchedule + "/" + i.TotalSchedule + "]" + " - " + i.ClassName + "</option>");
            }
            return Json(new { data = html.ToString(), JsonRequestBehavior.AllowGet });
        }

        [HttpPost]
        public ActionResult LoadCourseInfo(int id)
        {
            var l = new List<Admin_CourseSchedule_GetList_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_CourseSchedule_GetList(id).ToList();
            }
            StringBuilder html = new StringBuilder();
            foreach (var i in l)
            {
                html.Append("<div class=\"col-sm-4 form-group\">");
                html.Append("Giáo viên: " + i.TeacherName);
                html.Append("</div>");

                html.Append("<div class=\"col-sm-4 form-group\">");
                html.Append("Ngày học: " + i.Date.Value.ToString("dd/MM/yyyy"));
                html.Append("</div>");

                html.Append("<div class=\"col-sm-4 form-group\">");
                html.Append("Giờ học: " + i.StudyTimeName);
                html.Append("</div>");
            }

            return Json(new { data = html.ToString(), JsonRequestBehavior.AllowGet });
        }
        [HttpPost]
        public ActionResult LoadEmailnhanlop(int appointmentid, int courseid)
        {
            try
            {
                var appointment = StudentAppointmentTable.getbyid(appointmentid);
                var course = CoursesTable.getbyid(courseid);

                var student = AccountTable.getbyID(appointment.StudentUID.Value);
                using (var db = new ZimEntities())
                {
                    int id = 1;
                    switch (student.Language)
                    {
                        case 7:
                            id = 15;
                            break;
                        case 6:
                            id = 8;
                            break;
                        default:
                            id = 1;
                            break;
                    }
                    var template = db.tbl_EmailTemplate.Where(x => x.ID == id).FirstOrDefault();
                    string ct = "";
                    string tt = "";
                    var chuongtrinhhoc = ClassTable.getbyID(course.ClassID.Value);
                    var giaotrinhs = CurriculumnInCourseTable.getByCourseId(course.ID).ToList();
                    var first = CoursesScheduleTable.GetFirstLesson(course.ID);
                    var time = StudyTimeTable.getbyid(first.StudyTimeID.Value);
                    var teacher = AccountTable.getbyID(first.TeacherID.Value);
                    //tiêu đề mail
                    tt = template.EmailTitle;
                    //nội dung mail
                    ct = template.EmailContent;
                    ct = ct.Replace("[ten-khoa-hoc]", course.CourseName);
                    ct = ct.Replace("[ten-giang-vien]", teacher.FullName);
                    ct = ct.Replace("[id-pass-zoom]", teacher.ZoomID + " - " + teacher.ZoomPassword);
                    ct = ct.Replace("[ngay-khai-giang]", first.Date.Value.ToString("dd/MM/yyyy"));
                    ct = ct.Replace("[chuong-trinh-hoc]", chuongtrinhhoc.ClassName);
                    ct = ct.Replace("[link-giao-trinh]", string.Join("\n", giaotrinhs.Select(x => x.LinkCurriculumn)));
                    return Json(new { rs = true, tt, ct });
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Course", "LoadEmailnhanlop", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false });
            }
        }       
        [HttpPost]
        public ActionResult CancelReserve(int id)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3 && aclog.RoleID != 6)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                StudentAppointmentTable.CancelReserve(id, aclog.UserName);
                return Json(new { rs = true, JsonRequestBehavior.AllowGet });
            }
            catch
            {
                return Json(new { rs = false, JsonRequestBehavior.AllowGet });
            }
        }
        /// <summary>
        /// cập nhật doanh thu cho chiến dịch
        /// </summary>
        public static void UpdateCampaignSale(int targetId, double revenue)
        {
            try
            {
                // danh sách các ID mục tiêu cần update
                List<int> updateList = new List<int>();
                List<int> updateStatusList = new List<int>();

                using (var db = new ZimEntities())
                {
                    // lấy mục tiêu có managerID = ID tvv
                    var data = db.tbl_Target.Where(x => x.ID == targetId).FirstOrDefault();
                    if (data != null)
                    {
                        updateList.Add(data.ID);
                        do
                        {
                            var tempId = data.MainTargetID;
                            data = db.tbl_Target.Where(x => x.ID == tempId && x.CampaignID == data.CampaignID
                            && x.IsDone == false && x.IsHide == false).FirstOrDefault();
                            if (data != null)
                            {
                                updateList.Add(data.ID);
                            }
                        }
                        while (data != null);
                    }
                    updateList = updateList.Distinct().ToList();
                    // cập nhật doanh thu cho các mục tiêu
                    if (updateList.Count > 0)
                        db.Update_TargetRevenue(string.Join(",", updateList).ToString(), revenue);
                    updateStatusList.AddRange(updateList);
                }
                foreach (var u in updateStatusList)
                {
                    UpdateTargetStatus(u, revenue);
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("UpdateCampaignSale", "CampaignSale", 0, e.Message + " || " + e.InnerException + "params(" + targetId + ", " + revenue + ")");
            }
        }

        /// <summary>
        /// cập nhật trạng thái cho các mục tiêu
        /// 0 - chưa thực hiện, 1 - đúng tiến độ, 2 - xuất sắc, 3 - chậm tiến độ, 4 - rủi ro
        /// </summary>
        public static void UpdateTargetStatus(int targetId, double revenue)
        {
            try
            {
                var target = TargetTable.getbyid(targetId);
                var campaign = CampaignSaleTable.getbyid(target.CampaignID.Value);
                int currentDay = DateTime.Now.Day; // ngày hiện tại
                int totalDays = (campaign.eTime - campaign.sTime).Value.Days + 1;// tổng ngày trong chiến dịch
                double percentPerDay = 100 / (double)totalDays; // % hoàn thành/ 1 ngày
                double expectedPercent = percentPerDay * currentDay; // % phải hoàn thành dc
                double totalRevenue = target.CurrentValue ?? 0 + revenue; // tổng doanh thu
                double finishedPercent = (totalRevenue / target.TargetValue.Value) * 100; // % đã hoàn thành 
                                                                                          // hoàn thành mục tiêu trước hạn hoặc vượt mục tiêu
                if (finishedPercent > 100 || (finishedPercent == 100 && currentDay < totalDays))
                {
                    TargetTable.updateStatus(targetId, 2);
                }
                // hoàn thành mục tiêu trong khoảng +- 1 ngày
                else if (finishedPercent >= (expectedPercent - percentPerDay))
                {
                    TargetTable.updateStatus(targetId, 1);
                }
                // hoàn thành mục tiêu chậm 3 ngày
                else if (finishedPercent <= (expectedPercent - (percentPerDay * 3)) && finishedPercent > (expectedPercent - (percentPerDay * 7)))
                {
                    TargetTable.updateStatus(targetId, 3);
                }
                // hoàn thành mục tiêu chậm 7 ngày
                else if (finishedPercent <= (expectedPercent - (percentPerDay * 7)))
                {
                    TargetTable.updateStatus(targetId, 4);
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("UpdateTargetStatus", "CampaignSale", 0, e.Message + " || " + e.InnerException);
            }
        }

        [HttpPost]
        public ActionResult LoadAttention(int student, int course)
        {
            var schedules = new List<GetAttendanceByCourseAndStudent_Result>();
            using (var db = new ZimEntities())
            {
                schedules = db.GetAttendanceByCourseAndStudent(student, course).Where(x => x.AttendanceID != 0 && x.AttendanceID != null).ToList();
            }
            return Json(new { data = schedules });
        }

        [HttpPost]
        public ActionResult UpdateAcademic(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                int id = 0;
                if (!string.IsNullOrEmpty(f.Get("hdid")))
                {
                    id = Convert.ToInt32(f.Get("hdid"));
                }

                int academic = 0;
                if (!string.IsNullOrEmpty(f.Get("slAcademic")))
                {
                    academic = Convert.ToInt32(f.Get("slAcademic"));
                }

                AccountTable.UpdateAcademic(id, academic, aclog.UserName);

                SetAlert("Thành công", "s");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("CustomerController", "UpdateAcademic", 0, e.Message + "||" + e.InnerException);
                SetAlert("Đã xảy ra lỗi, vui lòng liên hệ Mona Software", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
        }

        [HttpGet]
        public ActionResult CourseStudentAPI()
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { redirect = "/Login/Signin" }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return Json(new { redirect = "/Login/Signin" }, JsonRequestBehavior.AllowGet);

                OjectDataTable dataTable = new OjectDataTable();
                dataTable.draw = int.Parse(Request.QueryString["draw"]);
                //bắt đầu từ vị trí nào
                int Start = Request.QueryString["start"] == null ? 0 : Request.QueryString["start"].ToInt(0);
                //độ dài mỗi trang
                int Length = Request.QueryString["length"] == null ? 0 : Request.QueryString["length"].ToInt(0);

                string search = "";
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                {
                    search = ViewBag.Search = Request.QueryString["search"].ToString().Trim();
                }
                
                int language = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["cblang"]))
                {
                    language = ViewBag.Language = Request.QueryString["cblang"].ToString().ToInt(0);
                }
                
                int grade = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["hdGrade"]))
                {
                    grade = ViewBag.Grade = Request.QueryString["hdGrade"].ToString().ToInt(0);
                }
                
                int program = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["hdProgram"]))
                {
                    program = ViewBag.Program = Request.QueryString["hdProgram"].ToString().ToInt(0);
                }
                
                int tvv = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["slTVV"]))
                {
                    tvv = Request.QueryString["slTVV"].ToString().ToInt(0);
                }
                if (aclog.RoleID == 6)
                {
                    tvv = aclog.ID;
                }

                ViewBag.TVV = tvv;


                var data = new List<getCustomerInCourse_Result>();
                int total = 0;

                using (ZimEntities db = new ZimEntities())
                {
                    data = db.getCustomerInCourse(search, program, Start, Length, tvv).ToList();

                    if (data.Count > 0)
                        total = data[0].TotalRow ?? 0;
                }
                //dữ liệu
                dataTable.data = data.ToArray();
                //số tượng item
                dataTable.recordsTotal = total;
                //phân trang
                dataTable.recordsFiltered = total;
                return Json(dataTable, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CourseDetailController", "LoadAttendanceByDates", uid, ex.Message + ex.InnerException);
                return Json(new { redirect = "/Login/Signin" }, JsonRequestBehavior.AllowGet);
            }
        }

        //[HttpGet]
        //public ActionResult CourseStudentAPI(int page = 1)
        //{
        //    try
        //    {
        //        if (Session["UID"] == null)
        //            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        //        var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
        //        if (aclog.RoleID == 5)
        //            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

        //        //page size
        //        int pageSize = 20;
        //        string search = "";
        //        //key search
        //        if (!string.IsNullOrEmpty(Request.QueryString["search"]))
        //        {
        //            ViewBag.Search = Request.QueryString["search"];
        //            search = Request.QueryString["search"].ToString().Trim();
        //        }

        //        int language = 0;
        //        if (!string.IsNullOrEmpty(Request.QueryString["cblang"]))
        //        {
        //            language = ViewBag.Language = Request.QueryString["cblang"].ToInt(0);
        //        }

        //        int program = 0;
        //        if (!string.IsNullOrEmpty(Request.QueryString["hdProgram"]))
        //        {
        //            program = ViewBag.Program = Request.QueryString["hdProgram"].ToInt(0);
        //        }

        //        int grade = 0;
        //        if (!string.IsNullOrEmpty(Request.QueryString["hdGrade"]))
        //        {
        //            grade = ViewBag.Grade = Request.QueryString["hdGrade"].ToInt(0);
        //        }

        //        int tvv = 0;
        //        if (!string.IsNullOrEmpty(Request.QueryString["slTVV"]))
        //        {
        //            tvv = ViewBag.TVV = Request.QueryString["slTVV"].ToInt(0);
        //        }

        //        List<getCustomerInCourse_Result> l = new List<getCustomerInCourse_Result>();
        //        using (var db = new ZimEntities())
        //        {
        //            l = db.getCustomerInCourse(search, program, (page - 1) * pageSize, pageSize, tvv).ToList();
        //        }

        //        return Json(new { rs = true, data = l }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        int uid = 0;
        //        if (Session["UID"] != null)
        //            uid = Session["UID"].ToString().ToInt(0);
        //        AssetCRM.Writelog("CustomerController", "CourseStudentAPI", uid, ex.Message + ex.InnerException);
        //        return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        [HttpPut]
        public ActionResult UpdateCourseStudent(int id)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

                CoursesStudentTable.UpdateHideExpired(id, aclog.UserName);
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("CustomerController", "UpdateCourseStudent", uid, ex.Message + ex.InnerException);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateTeacherByCourse(CourseModel model)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 6 && aclog.RoleID != 7 && aclog.RoleID != 8)// admin, manager, quản lý
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            bool changeAcdemic = false;
            tbl_Courses c = CoursesTable.getbyid(model.id);
            if (c == null)
            {
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

            string coursename = c.CourseName;
            if (!string.IsNullOrEmpty(model.courseName))
            {
                coursename = model.courseName;
            }

            int status = model.status;

            string statusName = "";
            switch (status)
            {
                case 1:
                    statusName = "Sắp mở";
                    break;
                case 2:
                    statusName = "Đang mở";
                    break;
                case 3:
                    statusName = "Đã đóng";
                    break;
            }

            CoursesTable.UpdateName(c.ID, coursename, aclog.UserName, status, statusName);

            tbl_Account acdemic = new tbl_Account();
            acdemic = AccountTable.getbyID(model.academic);

            if (aclog.RoleID == 7)
                acdemic = AccountTable.getbyID(c.AcademicUID.Value);

            if (c.AcademicUID != null)
            {
                if (acdemic != null)
                {
                    if (acdemic.ID != c.AcademicUID)
                        changeAcdemic = true;
                }

            }
            else
                changeAcdemic = true;

            if (!string.IsNullOrEmpty(model.salary) && !string.IsNullOrEmpty(model.numberOfSalary))
            {
                CourseSalaryTable.DeleteByCourseId(c.ID, aclog.UserName);

                List<double> salarys = model.salary.Split(',').Select(Double.Parse).ToList();
                List<int> lessons = model.numberOfSalary.Split(',').Select(Int32.Parse).ToList();

                for (var i = 0; i < lessons.Count; i++)
                {
                    CourseSalaryTable.insert(c.ID, lessons[i], salarys[i], aclog.UserName);
                }

                Thread tschedule = new Thread(() =>
                {
                    CoursesScheduleTable.UpdateSalaryForSchedule(c.ID);
                });
                tschedule.Start();
            }

            if (changeAcdemic)
            {
                CoursesTable.updateteacherlead(c.ID, model.lesson, acdemic, aclog.UserName);
                //gửi thông báo cho admin và giáo viên đó
                List<tbl_Account> ad = AccountTable.getall().Where(n => (n.RoleID == 1) && n.Status == 2).ToList();
                ad.Remove(aclog);
                //Gửi trước cho admin và manager
                foreach (var itemac in ad)
                {
                    NotificationTable.insertsingle("", "Chỉ định quản lý học vụ lớp", "Học vụ [ " + acdemic.FullName + " ] được chỉ định quản lý lớp [ " + c.CourseName + " ].", itemac.ID, aclog.UserName, 16, 0);
                }
                NotificationTable.insertsingle("", "Chỉ định quản lý học vụ lớp", "Học vụ [ " + acdemic.FullName + " ] được chỉ định quản lý lớp [ " + c.CourseName + " ].", acdemic.ID, aclog.UserName, 16, 0);
            }
            if (model.teacher != 0)
            {
                var schedules = CoursesScheduleTable.getbycourseid(c.ID).Where(x => x.FisnishStatus == 0 || x.FisnishStatus == 2).ToList();
                for (var i = 0; i < schedules.Count(); i++)
                {
                    CoursesScheduleTable.updateTeacher(schedules[i].ID, model.teacher, aclog.UserName);
                }
                TeacherCourseTable.insert(model.teacher, c, aclog.UserName);
            }
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public async Task<ActionResult> UpdateLesson(int id, int lesson, int registerlesson)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 6 && aclog.RoleID != 7 && aclog.RoleID != 8)// admin, manager, quản lý
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            var coursesStudent = CoursesStudentTable.getbyid(id);
            await CoursesTable.UpdateLesson(coursesStudent.CourseID ?? 0, lesson);
            await StudentAppointmentTable.UpdateLesson(coursesStudent.AppointmentID ?? 0, registerlesson);
            
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }

        public class CourseModel
        {
            public int id { set; get; } = 0;
            public string courseName { set; get; }
            public int status { set; get; } = 0;
            public int academic { set; get; } = 0;
            public int lesson { set; get; } = 0;
            public int teacher { set; get; } = 0;
            public string salary { set; get; }
            public string numberOfSalary { set; get; }
        }
        
        public class ExcelObject
        {
            public string PhoneNumber { set; get; }
            public string FullName { set; get; }
            public string Email { set; get; }
            public string BirthDay { set; get; }
            public string Address { set; get; }
        }
        public class CheckScheduleCourse
        {
            public int CourseID { get; set; }
            public int StudyTimeID { get; set; }
            public DateTime Date { get; set; }
        }
    }

    public class OjectDataTable
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public string totalcolunm { get; set; }
        public object[] data { get; set; }
        public OjectDataTable()
        {
            recordsTotal = 0;
            recordsFiltered = 0;
            totalcolunm = "0";
            data = new List<object>().ToArray();
        }
    }
    public class StudentInCourse
    {
        public int ID { get; set; }
        public int StudentUID { get; set; }
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public double PriceLeft { get; set; }
        public double CourseReserve { get; set; }
        public int SchoolID { get; set; }
        public string SchoolName { get; set; }
        public string Note { get; set; }
        public string Commitment { get; set; }
        public double PriceCourse { get; set; }
        public double TotalPaid { get; set; }
        public double PriceDiscout { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Warning { get; set; }
        public string Result { get; set; }
        public DateTime DatePriceLeft { get; set; }
    }
}