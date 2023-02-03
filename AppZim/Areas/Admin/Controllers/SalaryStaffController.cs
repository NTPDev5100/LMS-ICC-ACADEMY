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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AppZim.Areas.Admin.Controllers
{
    public class SalaryStaffController : BaseController
    {
        public class SalaryReviewClass
        {
            public int CampaignID { get; set; }
            public string CampaignName { get; set; }
            public int UID { get; set; }
            public string FullName { get; set; }
            public double SalaryCampaign { get; set; }
            public DateTime sDate { get; set; }
            public DateTime eDate { get; set; }
            public double TotalSalary { get; set; }
        }
        public ActionResult SalaryReview(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var camps = CampaignSaleTable.getall().Where(x => x.CampaignType == 3).FirstOrDefault();
            if (camps == null)
            {
                SetAlert("Không có chiến dịch", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }

            List<Admin_SalaryReview_GetList_Result> l = new List<Admin_SalaryReview_GetList_Result>();

            int pageSize = 20;
            string search = "";
            ViewBag.Search = Request.QueryString["search"];
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = Request.QueryString["search"].ToString();
            }
            int cam = camps.ID;
            if (!string.IsNullOrEmpty(Request.QueryString["slCampaign"]))
            {
                cam = Request.QueryString["slCampaign"].ToString().ToInt(0);
            }
            ViewBag.Campaign = cam;
            using (var db = new ZimEntities())
            {
                l = db.Admin_SalaryReview_GetList(cam, search, (page - 1) * pageSize, pageSize).ToList();
            }
            ViewBag.StartIndex = (page - 1) * pageSize;
            int total = 0;
            if (l.Count > 0)
            {
                total = l[0].TotalRows.Value;
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


        public ActionResult SalaryReviewDetail(int id, int campaignId, int page = 1)
        {
            int pageSize = 20;
            var l = new List<Admin_RevenueDetail_GetByTarget_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_RevenueDetail_GetByTarget(id, campaignId, (page - 1) * pageSize, pageSize).ToList();
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
        public ActionResult SalaryReviewTeacher(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

            var teacherID = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slTeacher"]))
            {
                teacherID = Request.QueryString["slTeacher"].ToInt(0);
            }
            if (aclog.RoleID == 4)
            {
                teacherID = aclog.ID;
            }
            ViewBag.Teacher = teacherID;

            int pageSize = 20;
            int course = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slCourse"]))
            {
                course = ViewBag.Course = Request.QueryString["slCourse"].ToInt(0);
            }

            //string from = DateTime.Now.FirstDayOfMonth().ToString("yyyy-MM-dd");
            //if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            //{
            //    from = DateTime.ParseExact(Request.QueryString["from"], "dd/MM/yyyy", null).ToString("yyyy-MM-dd");
            //}
            //ViewBag.From = DateTime.ParseExact(from, "yyyy-MM-dd", null).ToString("dd/MM/yyyy");

            //string to = DateTime.Now.LastDayOfMonth().ToString("yyyy-MM-dd");
            //if (!string.IsNullOrEmpty(Request.QueryString["to"]))
            //{
            //    to = DateTime.ParseExact(Request.QueryString["to"], "dd/MM/yyyy", null).ToString("yyyy-MM-dd");
            //}
            //ViewBag.To = DateTime.ParseExact(to, "yyyy-MM-dd", null).ToString("dd/MM/yyyy");

            int month = DateTime.Now.Month;
            if (!string.IsNullOrEmpty(Request.QueryString["slMonth"]))
            {
                month = Request.QueryString["slMonth"].ToInt(0);
            }
            ViewBag.Month = month;

            int year = DateTime.Now.Year;
            if (!string.IsNullOrEmpty(Request.QueryString["slYear"]))
            {
                year = Request.QueryString["slYear"].ToInt(0);
            }
            ViewBag.Year = year;

            int export = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["hdExport"]))
            {
                export = Request.QueryString["hdExport"].ToInt(0);
            }
            if (export == 1)
            {
                page = 1;
                pageSize = int.MaxValue;
            }
            List<Admin_TeacherSalary_GetList_Result> l = new List<Admin_TeacherSalary_GetList_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_TeacherSalary_GetList(course, teacherID, month, year, (page - 1) * pageSize, pageSize).ToList();
            }
            ViewBag.StartIndex = (page - 1) * pageSize;
            if (export == 1)
            {
                var excel = new System.Data.DataTable("e");
                excel.Columns.Add("Tên giáo viên", typeof(string));
                excel.Columns.Add("Năm", typeof(string));
                excel.Columns.Add("Tháng", typeof(string));
                excel.Columns.Add("thưởng thêm", typeof(string));
                excel.Columns.Add("Trạng thái duyệt lương", typeof(string));
                excel.Columns.Add("Tổng lương", typeof(string));
                foreach (var i in l)
                {
                    try
                    {
                        excel.Rows.Add(i.TeacherName, i.Year,
                            i.Month, String.Format("{0:0,0}", i.Bonus), i.isDonePaid == true ? "Đã duyệt lương" : "Chưa duyệt lương"
                            , String.Format("{0:0,0}", i.TotalSalary));
                    }
                    catch
                    {
                        continue;
                    }
                }
                var grid = new GridView();
                grid.DataSource = excel;
                grid.DataBind();
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=luong_giao_vien.xls");
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
            }
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
        public ActionResult updatTeacherSalary(int id, int bonus, bool isDonePaid)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var teacherSalary = TeacherSalaryTable.getbyid(id);
            var updateTeacherSalary = TeacherSalaryTable.update(new tbl_TeacherSalary
            {
                ID = id,
                Bonus = bonus + teacherSalary.Bonus,
                isDonePaid = isDonePaid,
            }, aclog.UserName);

            if (updateTeacherSalary == null)
                return Json(new { rs = true, message = "Thất bại" }, JsonRequestBehavior.AllowGet);

            var stu = DeviceMobileTable.getbyUID(teacherSalary.TeacherID ?? 0)?.IncludePlayerID;
            PushNotifyMobile.SendNotifyMobile("Lương về lương về, hãy vào xem và gửi thông báo cho admin nếu có vấn đề nha bạn", new[] { stu });
            // gui thong bao qua webapp
            var nt = NotificationTable.insertsingle("", "Lương về lương về", "Hãy vào xem và gửi thông báo cho admin nếu có vấn đề nha bạn", teacherSalary.TeacherID ?? 0, "admin", 0, 0);
            return Json(new { rs = true, message = "Thành công" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SalatyReviewTeacherDetail(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            var teacherID = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slTeacher"]))
            {
                teacherID = Request.QueryString["slTeacher"].ToInt(0);
            }
            ViewBag.Teacher = teacherID;

            var courseId = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slCourse"]))
            {
                courseId = Request.QueryString["slCourse"].ToInt(0);
            }
            ViewBag.Course = courseId;


            //bool? isClosing = null;
            //if (!string.IsNullOrEmpty(Request.QueryString["slisClosing"]))
            //{
            //    isClosing = Request.QueryString["slisClosing"].ToBool();
            //}

            string from = DateTime.Now.FirstDayOfMonth().ToString("yyyy-MM-dd");
            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                from = DateTime.ParseExact(Request.QueryString["from"], "dd/MM/yyyy", null).ToString("yyyy-MM-dd");
            }
            ViewBag.From = DateTime.ParseExact(from, "yyyy-MM-dd", null).ToString("dd/MM/yyyy");

            string to = DateTime.Now.LastDayOfMonth().ToString("yyyy-MM-dd");
            if (!string.IsNullOrEmpty(Request.QueryString["to"]))
            {
                to = DateTime.ParseExact(Request.QueryString["to"], "dd/MM/yyyy", null).ToString("yyyy-MM-dd");
            }
            ViewBag.To = DateTime.ParseExact(to, "yyyy-MM-dd", null).ToString("dd/MM/yyyy");

            List<Admin_TeacherSalaryDetail_GetList_Result> l = new List<Admin_TeacherSalaryDetail_GetList_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_TeacherSalaryDetail_GetList(from, to, teacherID, courseId, (page - 1) * pageSize, pageSize).ToList();
            }

            ViewBag.ClosingFrom = DateTime.Now.AddMonths(-1).FirstDayOfMonth().ToString("dd-MM-yyyy");
            ViewBag.ClosingTo = DateTime.Now.AddMonths(-1).LastDayOfMonth().ToString("dd-MM-yyyy");

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
        public ActionResult UpdatePayroll()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            Thread tsalary = new Thread(() => { TeacherSalaryDetailTable.updatePayroll(aclog.UserName); });
            tsalary.Start();

            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SalaryReviewTeacherOnline(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 9)// khác admin, manager,kế toán
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = ViewBag.Search = Request.QueryString["search"].ToString();
            }
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                ViewBag.Sort = sort = Request.QueryString["sort"].ToInt(0);
            }
            int year = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["year"]))
            {
                ViewBag.Year = year = Request.QueryString["year"].ToInt(0);
            }
            List<ASW_SalaryReviewTeacher_Online_Result> l = new List<ASW_SalaryReviewTeacher_Online_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_SalaryReviewTeacher_Online(search, sort, year, (page - 1) * pageSize, pageSize).ToList();
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
        //[HttpPost]
        //public ActionResult AcceptSalarySaler(int uid, int campid, string add, string note)
        //{
        //    var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
        //    if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 9)// khác admin, manager,kế toán
        //    {
        //        SetAlert("Bạn không có quyền truy cập", "e");
        //        return RedirectToAction("Signin", "Login", new { area = "" });
        //    }
        //    var saler = AccountTable.getbyID(uid);
        //    if (saler == null)
        //        return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        //    var camp = CampaignSaleTable.getbyid(campid);
        //    if (camp == null)
        //        return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        //    int addition = add.Replace(",", "").ToInt(0);

        //    int totalcamp = 0; // tổng doanh thu
        //    // TriS
        //    IEnumerable<tbl_CoursesStudent> l = CoursesStudentTable.getall().Where(n => n.CampaignSaleID == campid && n.SupportUID == uid).OrderByDescending(n => n.ID).ToList();
        //    var l2 = StudentAppointmentTable.getByCampaignSaleID(campid, saler.UserName);
        //    List<ChienDich> fl = new List<ChienDich>();
        //    // tbl_CoursesStudent
        //    foreach (var i in l)
        //    {
        //        ChienDich x = new ChienDich();
        //        x.TenHocVien = i.StudentName;
        //        x.TenKhoaHoc = i.CourseName;
        //        x.Gia = (double)i.PriceCourse + (double)i.CommitmentPrice;
        //        x.DuocGiam = (double)i.PriceDiscout;
        //        x.DaThanhToan = (double)i.Deposit;
        //        x.ConThieu = (x.Gia - x.DuocGiam - x.DaThanhToan);
        //        x.SupportID = i.SupportUID.Value;
        //        x.NgayTao = i.CreatedDate.Value;
        //        fl.Add(x);
        //    }
        //    // StudentAppointment
        //    foreach (var i in l2)
        //    {
        //        ChienDich x = new ChienDich();
        //        x.TenHocVien = i.StudentName;
        //        x.TenKhoaHoc = i.ClassName;
        //        x.Gia = (double)i.ClassPrice;
        //        x.DuocGiam = (double)i.DiscountPrice;
        //        x.DaThanhToan = (double)i.Deposit;
        //        x.ConThieu = (x.Gia - x.DuocGiam - x.DaThanhToan);
        //        x.SupportID = AccountTable.getbyusername(i.CreatedBy).ID;
        //        x.NgayTao = i.CreatedDate.Value;
        //        fl.Add(x);
        //    }

        //    //int totalRefund = 0; //tổng tiền hoàn trả
        //    totalcamp = (int)fl.Sum(x => x.DaThanhToan); //tổng doanh thu
        //    // end TriS
        //    var setting = SalarySettingTable.getall().Where(n => n.Million >= totalcamp).OrderBy(n => n.Million).FirstOrDefault();
        //    if (setting == null)
        //    {
        //        setting = SalarySettingTable.getall().OrderByDescending(n => n.Million).FirstOrDefault();
        //    }

        //    var d = SalaryPaidHistoryTable.CalcSaler(saler, camp.sTime.Value.ToString("MM/yyyy"), camp.ID, 2000000, addition, note, aclog.UserName, totalcamp);
        //    if (d != null)// true
        //    {
        //        //Đánh dấu những StudentAppointment đã được tính lương
        //        using (var db = new ZimEntities())
        //        {
        //            foreach (var item in l2)
        //            {
        //                var check = db.tbl_StudentAppointment.Where(x => x.ID == item.ID).FirstOrDefault();
        //                check.DoneSalary = true;
        //                db.SaveChanges();
        //            }
        //        }

        //        return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        //    }
        //    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        //}

        // GET: Admin/SalaryStaff
        public ActionResult SalaryStaff(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 9)// khác admin, manager,kế toán
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = ViewBag.Search = Request.QueryString["search"].ToString();
            }
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                ViewBag.Sort = sort = Request.QueryString["sort"].ToInt(0);
            }
            List<ASW_History_Seller_Result> l = new List<ASW_History_Seller_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_History_Seller(search, sort, (page - 1) * pageSize, pageSize).ToList();
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

        //Danh sách thông tin nhân viên đã có cấp bậc
        public ActionResult StaffOfPositionList(int page = 1, int? positionId = null)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });


            //key search
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = Request.QueryString["search"].ToString();
            }
            
            ViewBag.Search = Request.QueryString["search"];
            //sort
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slSort"]))
            {
                ViewBag.Sort = sort = Request.QueryString["slSort"].ToInt(0);
            }
            ViewBag.Sort = Request.QueryString["slSort"].ToInt(0);

            //sort type
            int sorttype = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slSortType"]))
            {
                ViewBag.SortType = sorttype = Request.QueryString["slSortType"].ToInt(0);
            }
            ViewBag.SortType = Request.QueryString["slSortType"].ToInt(0);

            int pageSize = 20;
            List<Admin_StaffOfPosition_GetList_Result> l = new List<Admin_StaffOfPosition_GetList_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_StaffOfPosition_GetList(search, positionId, sort, sorttype, pageSize, page).ToList();
            }

            ViewBag.StartIndex = (page - 1) * pageSize;
            int total = 0;
            if (l.Any())
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
            ViewBag.PositionId = positionId;
            return View(l);
        }

        [HttpPost]
        public ActionResult UpdateStaffPosition(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                //Nhân viên chưa có cấp bậc
                var staff = AccountTable.getbyID(f.Get("ddl-full-name").ToInt(0));
                int staffId = 0;
                if (staff != null)
                {
                    staffId = staff.ID;
                }
                int positionId = f.Get("dll-position").ToInt(0);


                AccountTable.UpdatePosition(staffId, positionId, aclog.FullName);
                SetAlert("Thành công", "s");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            catch(Exception e)
            {
                AssetCRM.Writelog("SalaryStaffController", "UpdateStaffPosition", 0, e.Message + "||" + e.InnerException);
                SetAlert("Đã xảy ra lỗi", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }           
        }

        public ActionResult SalaryStaffDetail()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            return View();
        }
        public ActionResult StaffSalary(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = ViewBag.Search = Request.QueryString["search"].ToString();
            }
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                ViewBag.Sort = sort = Request.QueryString["sort"].ToInt(0);
            }
            int type = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["type"]))
            {
                ViewBag.Type = type = Request.QueryString["type"].ToInt(0);
            }
            List<ASW_Staff_Salary_Result> l = new List<ASW_Staff_Salary_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_Staff_Salary(search, sort, type, (page - 1) * pageSize, pageSize).ToList();
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

        /// <summary>
        /// Xoá lương nhân viên
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteSalaryStaff(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            if (aclog.RoleID == 1)
            {
                StaffSalaryTable.Delete(id,aclog.UserName);
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SettingRoseSeller(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = ViewBag.Search = Request.QueryString["search"].ToString();
            }
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                ViewBag.Sort = sort = Request.QueryString["sort"].ToInt(0);
            }
            List<ASW_SettingRoseSeller_Result> l = new List<ASW_SettingRoseSeller_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_SettingRoseSeller(search, sort, (page - 1) * pageSize, pageSize).ToList();
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
        public ActionResult AddStaffSalary(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int id = f.Get("hdfID").ToInt(0);
            int pID = f.Get("txt-position").ToInt(0);
            int salary = f.Get("txt-salary").ToString().Replace(",", "").ToInt(0);
            if (salary == 0)
            {
                SetAlert("Vui lòng nhập tiền lương lớn hơn 0", "e");
                return RedirectToAction("StaffSalary", "SalaryStaff");
            }
            tbl_StaffSalary rs = null;
            if (id == 0)
            {
                var check = StaffSalaryTable.GetByPostionID(pID);
                if (check != null)
                {
                    SetAlert("Thành công", "s");
                    return RedirectToAction("StaffSalary", "SalaryStaff");
                }
                rs = StaffSalaryTable.Insert(pID, salary, 0, 1, aclog.UserName);
            }
            else
            {
                rs = StaffSalaryTable.Update(id, salary, 0, aclog.UserName);
            }
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("StaffSalary", "SalaryStaff");
        }

        [HttpPost]
        public ActionResult AddSettingSalaryTeacher(int id, int teacherID, string luong, int loai, int phantram)
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
            int salary = Convert.ToInt32(luong.Replace(",", "").ToInt(0));
            if (loai == 2 && salary == 0)
            {
                return Json(new { rs = false, message = "Vui lòng nhập tiền lương/giờ lớn hơn 0!" }, JsonRequestBehavior.AllowGet);
            }
            if (loai == 1 && phantram == 0)
            {
                return Json(new { rs = false, message = "Vui lòng nhập phần trăm lớn hơn 0!" }, JsonRequestBehavior.AllowGet);
            }
            tbl_StaffSalary rs = null;
            if (id == 0)
            {
                var check = StaffSalaryTable.GetByUID(teacherID);
                if (check != null)
                {
                    if (check.TypeSalary == loai)
                    {
                        return Json(new { rs = true, message = "Thành công!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                rs = StaffSalaryTable.Insert(teacherID, salary, phantram, loai, aclog.UserName);
            }
            else
            {
                rs = StaffSalaryTable.Update(id, salary, phantram, aclog.UserName);
            }

            var teacher = AccountTable.getbyID(rs.UID.Value);
            string ngaySua = "";
            if (!string.IsNullOrEmpty(rs.ModifiedDate.ToString()))
            {
                ngaySua = rs.ModifiedDate.Value.ToString();
            }
            string type = "success";
            if (rs.TypeSalary == 2)
                type = "secondary";
            StringBuilder strExport = new StringBuilder();
            strExport.Append("                    <tr class=\"remove-row-" + rs.ID + "\">");
            strExport.Append("                        <td>" + teacher.FullName + "</td>");
            strExport.Append("                        <td>" + teacher.Phone + "</td>");
            strExport.Append("                        <td>" + String.Format("{0:0,0}", rs.Salary) + "</td>");
            strExport.Append("                        <td>" + rs.Persent + " (%)</td>");
            strExport.Append("                        <td><span class=\"badge badge-" + type + " pd-l-10 pd-r-10 pd-t-5 pd-b-5\">" + rs.TypeNameSalary + "</span></td>");
            strExport.Append("                        <td>" + rs.CreatedDate + "</td>");
            strExport.Append("                        <td>" + ngaySua + "</td>");
            strExport.Append("                        <td>");
            strExport.Append("            <div class=\"dropdown dropdown-icon\">");
            strExport.Append("                  <a href=\"javascript:;\" class=\"material-icons\" id=\"drop-down-1\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">more_vert</a>");
            strExport.Append("                <div class=\"dropdown-menu\" aria-labelledby=\"drop-down-1\"> ");
            strExport.Append("                        <a class=\"dropdown-item btn-icon edit-salary\" href=\"javascript:;\" data-id=" + rs.ID + " data-uid=" + rs.UID + " data-salary=" + String.Format("{0:0,0}", rs.Salary) + " data-persent=" + rs.Persent + " data-type=" + rs.TypeSalary + "><i data-feather=\"edit\"></i> Chỉnh sửa</a>");
            strExport.Append("              </div ");
            strExport.Append("             </div>");
            strExport.Append("                          </td>");
            strExport.Append("                    </tr>");
            return Json(new { rs = true, data = strExport.ToString(), message = "Thành công!" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddSettingSalaryRoseSeller(int id, int teacherID, int phantram)
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
            tbl_StaffSalaryRoseSeller rs = null;
            if (id == 0)
            {
                var check = StaffSalaryRoseSellerTable.GetByUID(teacherID);
                if (check == null)
                {
                    rs = StaffSalaryRoseSellerTable.Insert(teacherID, phantram, aclog.UserName);
                }
            }
            else
            {
                //kiểm tra xem nó hết duyệt lương mới cho cập nhật phần trăm hoa hồng
                var camps = CampaignSaleTable.getall().Where(n => n.eTime < GetDateTime.Now.Date).OrderByDescending(x => x.ID).ToList();
                List<SalaryReviewClass> salary = new List<SalaryReviewClass>();
                foreach (var cap in camps)
                {
                    var checkpaidsalary = SalaryPaidHistoryTable.getbyuidvscampid(teacherID, cap.ID);
                    if (checkpaidsalary != null)
                    {
                        continue;
                    }
                    if (CoursesStudentTable.getbycampid(cap.ID).Where(n => n.SupportUID == teacherID).Count() > 0)
                    {
                        return Json(new { rs = false, message = "Vui lòng duyệt hết lương seller trước!" }, JsonRequestBehavior.AllowGet);
                    }
                }

                rs = StaffSalaryRoseSellerTable.Update(id, phantram, aclog.UserName);
            }

            var teacher = AccountTable.getbyID(rs.UID.Value);
            string ngaySua = "";
            if (!string.IsNullOrEmpty(rs.ModifiedDate.ToString()))
            {
                ngaySua = rs.ModifiedDate.Value.ToString();
            }
            StringBuilder strExport = new StringBuilder();
            strExport.Append("                    <tr class=\"remove-row-" + rs.ID + "\">");
            strExport.Append("                        <td>" + teacher.FullName + "</td>");
            strExport.Append("                        <td>" + teacher.Phone + "</td>");
            strExport.Append("                        <td>" + rs.SalaryPresent + " (%)</td>");
            strExport.Append("                        <td>" + rs.CreatedDate + "</td>");
            strExport.Append("                        <td>" + ngaySua + "</td>");
            strExport.Append("                        <td>");
            strExport.Append("            <div class=\"dropdown dropdown-icon\">");
            strExport.Append("                  <a href=\"javascript:;\" class=\"material-icons\" id=\"drop-down-1\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">more_vert</a>");
            strExport.Append("                <div class=\"dropdown-menu\" aria-labelledby=\"drop-down-1\"> ");
            strExport.Append("                        <a class=\"dropdown-item btn-icon edit-salary\" href=\"javascript:;\" data-id=" + rs.ID + " data-uid=" + rs.UID + " data-persent=" + rs.SalaryPresent + "><i data-feather=\"edit\"></i> Chỉnh sửa</a>");
            strExport.Append("              </div ");
            strExport.Append("             </div>");
            strExport.Append("                          </td>");
            strExport.Append("                    </tr>");
            return Json(new { rs = true, data = strExport.ToString(), message = "Thành công!" }, JsonRequestBehavior.AllowGet);
        }


        //[HttpPost]
        //public ActionResult ExportExcelSalaryTeacherOffline(string search, int sort, int year)
        //{
        //    if (Session["UID"] == null)
        //        return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
        //    var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
        //    if (aclog == null)
        //        return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
        //    var tearcherID = 0;
        //    if (aclog.RoleID == 4)
        //    {
        //        tearcherID = aclog.ID;
        //    }
        //    List<ReportSalaryTearcher_Result> l = new List<ReportSalaryTearcher_Result>();
        //    using (var db = new ZimEntities())
        //    {
        //        l = db.ReportSalaryTearcher(search, tearcherID, sort, year).ToList();
        //    }
        //    if (l.Count() == 0)
        //    {
        //        return Json(new { rs = false, noti = "Dữ liệu trống!" }, JsonRequestBehavior.AllowGet);
        //    }

        //    StringBuilder strExport = new StringBuilder();
        //    strExport.Append("       <table border=\"1\">");
        //    strExport.Append("                     <tr>");
        //    strExport.Append("                        <th scope=\"col\">STT</th>");
        //    strExport.Append("                        <th scope=\"col\">Họ và tên</th>");
        //    strExport.Append("                        <th scope=\"col\">Số điện thoại</th>");
        //    strExport.Append("                        <th scope=\"col\">Khóa</th>");
        //    strExport.Append("                        <th scope=\"col\">Ngày bắt đầu</th>");
        //    strExport.Append("                        <th scope=\"col\">Lương/buổi</th>");
        //    strExport.Append("                        <th scope=\"col\">Số buổi đã dạy</th>");
        //    strExport.Append("                        <th scope=\"col\">Đánh giá</th>");
        //    strExport.Append("                        <th scope=\"col\">Số buổi chưa dạy</th>");
        //    strExport.Append("                        <th scope=\"col\">Tổng lương</th>");
        //    strExport.Append("                        <th scope=\"col\">Ghi chú</th>");
        //    strExport.Append("                    </tr>");

        //    var index = 0;
        //    foreach (var i in l)
        //    {
        //        index++;
        //        strExport.Append("                    <tr>");
        //        strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
        //        strExport.Append("                        <td style=\"text-align:left\">" + i.FullName + "</td>");
        //        strExport.Append("                        <td style=\"text-align:left\">" + i.Phone + "</td>");
        //        strExport.Append("                        <td style=\"text-align:center\">"+i.CourseName+"</td>");
        //        strExport.Append("                        <td style=\"text-align:left\">" + i.OpeningDate.Value.ToString("dd/MM/yyyy") + "</td>");
        //        strExport.Append("                        <td style=\"text-align:left\">" + i.SalaryInTimes + "</td>");
        //        strExport.Append("                        <td style=\"text-align:center\">" + i.SoBuoiDay + "</td>");
        //        strExport.Append("                        <td style=\"text-align:center\">" + (i.RateTotal == 0 ? "0" : Math.Round((double)i.RateSum.Value / i.RateTotal.Value, 1).ToString()) + "</td>");
        //        strExport.Append("                        <td style=\"text-align:center\">" + i.SoBuoiChuaDay + "</td>");
        //        strExport.Append("                        <td style=\"text-align:center\">" + String.Format("{0:0,0}", i.TongLuong) + "</td>");
        //        strExport.Append("                        <td style=\"text-align:center\"></td>");
        //        strExport.Append("                    </tr>");
        //    }
        //    strExport.Append("</table>");
        //    string mth = "Năm_";
        //    if (sort != 0)
        //    {
        //        mth = "Thang_" + sort + "/";
        //    }
        //    var filename = "Luong_Giao_Vien_Day_Offline_" + mth + "" + GetDateTime.Now.Date.Year;
        //    return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public ActionResult ExportExcelSalaryTeacherOnline(string search, int sort, int year)
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
            List<ASW_SalaryReviewTeacher_Online_ExportExcel_Result> l = new List<ASW_SalaryReviewTeacher_Online_ExportExcel_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_SalaryReviewTeacher_Online_ExportExcel(search, sort, year).ToList();
            }
            if (l.Count() == 0)
            {
                return Json(new { rs = false, noti = "Dữ liệu trống!" }, JsonRequestBehavior.AllowGet);
            }

            StringBuilder strExport = new StringBuilder();
            strExport.Append("       <table border=\"1\">");
            strExport.Append("                     <tr>");
            strExport.Append("                        <th scope=\"col\">STT</th>");
            strExport.Append("                        <th scope=\"col\">Họ và tên</th>");
            strExport.Append("                        <th scope=\"col\">Số điện thoại</th>");
            strExport.Append("                        <th scope=\"col\">Khóa học</th>");
            strExport.Append("                        <th scope=\"col\">Loại lớp</th>");
            strExport.Append("                        <th scope=\"col\">Tháng</th>");
            strExport.Append("                        <th scope=\"col\">Năm</th>");
            strExport.Append("                        <th scope=\"col\">Phần trăm</th>");
            strExport.Append("                        <th scope=\"col\">Phương thức</th>");
            strExport.Append("                        <th scope=\"col\">Tổng lương</th>");
            strExport.Append("                        <th scope=\"col\">Ghi chú</th>");
            strExport.Append("                    </tr>");

            var index = 0;
            foreach (var i in l)
            {
                index++;
                strExport.Append("                    <tr>");
                strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.FullName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.Phone + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.CourseName + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">Online</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.Month + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.Year + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.PersentSalarySetting + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.PersentCourseName + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + String.Format("{0:0,0}", i.TotalSalaryTeacher) + "</td>");
                strExport.Append("                        <td style=\"text-align:center\"></td>");
                strExport.Append("                    </tr>");
            }
            strExport.Append("</table>");
            string mth = "Năm_";
            if (sort != 0)
            {
                mth = "Thang_" + sort + "/";
            }
            var filename = "Luong_Giao_Vien_Day_Online_" + mth + "" + GetDateTime.Now.Date.Year;
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ExportExcelHistorySalarySeller(string search, int sort)
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
            List<ASW_History_Seller_ExportExcel_Result> l = new List<ASW_History_Seller_ExportExcel_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_History_Seller_ExportExcel(search, sort).ToList();
            }
            if (l.Count() == 0)
            {
                return Json(new { rs = false, noti = "Dữ liệu trống!" }, JsonRequestBehavior.AllowGet);
            }

            StringBuilder strExport = new StringBuilder();
            strExport.Append("       <table border=\"1\">");
            strExport.Append("                     <tr>");
            strExport.Append("                        <th scope=\"col\">STT</th>");
            strExport.Append("                        <th scope=\"col\">Họ và tên</th>");
            strExport.Append("                        <th scope=\"col\">Chiến dịch</th>");
            strExport.Append("                        <th scope=\"col\">Tiền chiến dịch</th>");
            strExport.Append("                        <th scope=\"col\">Thưởng</th>");
            strExport.Append("                        <th scope=\"col\">Tổng lương</th>");
            strExport.Append("                        <th scope=\"col\">Ghi chú</th>");
            strExport.Append("                        <th scope=\"col\">Ngày duyệt</th>");
            strExport.Append("                        <th scope=\"col\">Người duyệt</th>");
            strExport.Append("                        <th scope=\"col\">Ghi chú thêm</th>");
            strExport.Append("                    </tr>");

            var index = 0;
            foreach (var i in l)
            {
                index++;
                strExport.Append("                    <tr>");
                strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.FullName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.CampaignName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + String.Format("{0:0,0}", i.TienChienDich) + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + String.Format("{0:0,0}", i.AdditionFee) + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + String.Format("{0:0,0}", i.PaidReal) + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.Note + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.CreatedDate.Value.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.NguoiDuyet + "</td>");
                strExport.Append("                        <td style=\"text-align:center\"></td>");
                strExport.Append("                    </tr>");
            }
            strExport.Append("</table>");
            var filename = "DS_Luong_Seller_Da_Duyet_" + GetDateTime.Now.ToString("dd/MM/yyyy");
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult EditTempPayroll(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int id = f.Get("hdid").ToInt(0);
            string note = "";
            if (!string.IsNullOrEmpty(f.Get("txt-note")))
            {
                note = f.Get("txt-note").ToString();
            }
            double bonus = 0;
            if (!string.IsNullOrEmpty(f.Get("bonus")))
            {
                bonus = Convert.ToDouble(f.Get("bonus").ToString().Replace(",", ""));
            }
            double deducted = 0;
            if (!string.IsNullOrEmpty(f.Get("deducted")))
            {
                deducted = Convert.ToDouble(f.Get("deducted").ToString().Replace(",", ""));
            }
            var rs = TempPayrollTable.update(id, bonus, deducted, note, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Thất bại", "e");
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }
        [HttpPost]
        public ActionResult AcceptSalary(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var rs = TempPayrollTable.UpdateDone(id, aclog.UserName);
            if (rs != null)
            {
                var payroll = TempPayrollTable.getbyid(id);
                var user = AccountTable.getbyID(payroll?.UID ?? 0);
                var campaign = CampaignSaleTable.getbyid(payroll?.CampaignID ?? 0);
                var email = EmailTemplateTable.GetByID(35);
                string tt = email.EmailTitle;
                string ct = email.EmailContent;
                double salary = (payroll?.BasicSalary ?? 0) + (payroll?.BonusSalary ?? 0) + ((payroll?.Revenue ?? 0) * ((payroll?.RevenuePercent ?? 0) / 100)) + ((((payroll?.CareQuantity ?? 0) * (payroll?.CarePrice ?? 0)) + ((payroll?.CarePlusPrice ?? 0) * (payroll?.CarePlusQuantity ?? 0))) * ((25 - (payroll?.RevenuePercent ?? 0)) / 100)) - (payroll?.DeductedSalary ?? 0);
                ct = ct.Replace("{ten-chien-dich}", campaign.CampaignName);
                ct = ct.Replace("{ten-nhan-vien}", user?.FullName);
                ct = ct.Replace("{chuc-vu}", user?.RoleName);
                ct = ct.Replace("{doanh-thu}", String.Format("{0:0,0}", payroll?.Revenue));
                ct = ct.Replace("{luong-co-ban}", String.Format("{0:0,0}", payroll?.BasicSalary));
                ct = ct.Replace("{thuong-goi-cam-ket}", String.Format("{0:0,0}", (((payroll?.CareQuantity ?? 0) * (payroll?.CarePrice ?? 0)) + ((payroll?.CarePlusPrice ?? 0) * (payroll?.CarePlusQuantity ?? 0))) * ((25 - (payroll?.RevenuePercent ?? 0)) / 100)));
                ct = ct.Replace("{phan-tram-doanh-thu}", payroll?.RevenuePercent + " %");
                ct = ct.Replace("{luong-tru}", String.Format("{0:0,0}", payroll?.DeductedSalary));
                ct = ct.Replace("{luong-thuong}", String.Format("{0:0,0}", payroll?.BonusSalary));
                ct = ct.Replace("{tong-luong}", String.Format("{0:0,0}", salary));
                ct = ct.Replace("{thong-tin-ngan-hang}", user?.BankInformation);
                ct = ct.Replace("{ghi-chu}", payroll?.Note);
                AssetCRM.SendMail(user?.Email, tt, ct);
            }
            else
            {
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> Courses(int teacherId)
        {
            try
            {
                var data = await CoursesTable.GetCourseByTeacherId(teacherId);
                return Json(new { rs = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("SalaryStaffController", "GET Courses", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Loadvideos(int courseScheduleId)
        {
            try
            {
                var data = CoursesScheduleFileTable.getbyScheduleID(courseScheduleId);
                return Json(new { rs = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("SalaryStaffController", "GET Loadvideos", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult PayrollForTeacher(int teacherId)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 4)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                TeacherSalaryDetailTable.PayrollForTeacher(teacherId, aclog.UserName);

                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("SalaryStaffController", "GET PayrollForTeacher", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult LoadTrialList(int teacherId, int month, int year)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                using (var db = new ZimEntities())
                {
                    var trialStudies = db.tbl_StudentAppointmentTest.Where(x => x.TeacherID == teacherId &&
                    x.Payrolled == false && x.DateTest.Value.Month == month &&
                    x.DateTest.Value.Year == year).ToList();

                    int total = trialStudies.Count;
                    List<object> data = new List<object>();
                    for (int i = 0; i < total; i++)
                    {
                        data.Add(new { DateTest = trialStudies[i].DateTest.Value.ToString("dd/MM/yyyy"), TimeTest = trialStudies[i].TimeTest, StudentName = trialStudies[i].StudentName });
                    }

                    return Json(new { rs = true, data }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("SalaryStaffController", "GET Loadvideos", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        // danh sách giáo viên viên
        public ActionResult TeacherListOnlyName(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            //key search
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = search = Request.QueryString["search"].ToString();
            }

            string fromdate = DateTime.Now.FirstDayOfMonth().ToString("yyyy-MM-dd 00:00:00.000");
            ViewBag.From = DateTime.Now.FirstDayOfMonth().ToString("dd/MM/yyyy");
            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                fromdate = DateTime.ParseExact(Request.QueryString["from"].ToString().Trim(), "dd/MM/yyyy", null).ToString("yyyy-MM-dd 00:00:00.000");
                ViewBag.From = Request.QueryString["from"].ToString();
            }

            string todate = DateTime.Now.LastDayOfMonth().ToString("yyyy-MM-dd 23:59:59.998");
            ViewBag.To = DateTime.Now.LastDayOfMonth().ToString("dd/MM/yyyy");
            if (!string.IsNullOrEmpty(Request.QueryString["to"]))
            {
                todate = DateTime.ParseExact(Request.QueryString["to"].ToString().Trim(), "dd/MM/yyyy", null).ToString("yyyy-MM-dd 23:59:59.998");
                ViewBag.To = Request.QueryString["to"].ToString();
            }

            List<GetTeacherHasSchedule_Result> l = new List<GetTeacherHasSchedule_Result>();
            using (var db = new ZimEntities())
            {
                l = db.GetTeacherHasSchedule(search, fromdate, todate, (page - 1) * pageSize, pageSize).ToList();
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

        public ActionResult CourseofTeacher(int id, string fromdate, string todate, int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            string coursename = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = coursename = Request.QueryString["search"].ToString();
            }

            int pageSize = 20;
            var teacher = AccountTable.getbyID(id);
            ViewBag.ID = id;
            ViewBag.FromDate = fromdate;
            ViewBag.ToDate = todate;
            ViewBag.Teacher = teacher;

            string from = DateTime.ParseExact(fromdate, "dd/MM/yyyy", null).ToString("yyyy-MM-dd");
            string to = DateTime.ParseExact(todate, "dd/MM/yyyy", null).ToString("yyyy-MM-dd");
            var data = new List<GetCourseByTeacherAndSchedule_Result>();
            using (var db = new ZimEntities())
            {
                data = db.GetCourseByTeacherAndSchedule(id, coursename, from, to, (page - 1) * pageSize, pageSize).ToList();
            }
            ViewBag.StartIndex = (page - 1) * pageSize;

            int total = 0;
            if (data.Count > 0)
            {
                total = data[0].TotalRow.Value;
            }
            ViewBag.Total = data.Count();
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            return View(data);
        }

        public ActionResult ApprovalToLevelUp(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)
                return RedirectToAction("Signin", "Login", new { area = "" });

            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = search = Request.QueryString["search"].ToString();
            }

            int pageSize = 20;

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
                    fromdate = fdate.ToString("yyyy-MM-dd 00:00:00.000");
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
                    todate = tdate.ToString("yyyy-MM-dd 23:59:59.998");
                }
                catch { }
            }
            var data = new List<Admin_ApprovalToLevelUp_GetList_Result>();
            using (var db = new ZimEntities())
            {
                data = db.Admin_ApprovalToLevelUp_GetList(search, fromdate, todate, (page - 1) * pageSize, pageSize).ToList();
            }
            ViewBag.StartIndex = (page - 1) * pageSize;

            int total = 0;
            if (data.Count > 0)
            {
                total = data[0].TotalRow.Value;
            }
            ViewBag.Total = data.Count();
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            return View(data);
        }


        public ActionResult ApprovalToLevelDown(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)
                return RedirectToAction("Signin", "Login", new { area = "" });

            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = search = Request.QueryString["search"].ToString();
            }

            int pageSize = 20;

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
                    fromdate = fdate.ToString("yyyy-MM-dd 00:00:00.000");
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
                    todate = tdate.ToString("yyyy-MM-dd 23:59:59.998");
                }
                catch { }
            }
            var data = new List<Admin_ApprovalToLevelDown_GetList_Result>();
            using (var db = new ZimEntities())
            {
                data = db.Admin_ApprovalToLevelDown_GetList(search, fromdate, todate, (page - 1) * pageSize, pageSize).ToList();
            }
            ViewBag.StartIndex = (page - 1) * pageSize;

            int total = 0;
            if (data.Count > 0)
            {
                total = data[0].TotalRow.Value;
            }
            ViewBag.Total = data.Count();
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            return View(data);
        }


        [HttpPost]
        public ActionResult UpdateApprovalToLevelDown(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int id = 0;
            if (!string.IsNullOrEmpty(f.Get("hdfID")))
            {
                id = f.Get("hdfID").ToInt(0);
            }

            int status = 1;
            if (!string.IsNullOrEmpty(f.Get("slStatus")))
            {
                status = f.Get("slStatus").ToInt(0);
            }

            string note = "";
            if (!string.IsNullOrEmpty(f.Get("txt-note")))
            {
                note = f.Get("txt-note").ToString();
            }
            int level = 0;
            if (!string.IsNullOrEmpty(f.Get("slLevel")))
            {
                level = f.Get("slLevel").ToInt(0);
            }

            var rs = ApprovalToLevelDownTable.update(id, status, note, null, aclog.UserName, level);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không tìm thấy yêu cầu", "e");

            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

        [HttpPost]
        public ActionResult UpdateApprovalToLevelUp(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int id = 0;
            if (!string.IsNullOrEmpty(f.Get("hdfID")))
            {
                id = f.Get("hdfID").ToInt(0);
            }

            int status = 1;
            if (!string.IsNullOrEmpty(f.Get("slStatus")))
            {
                status = f.Get("slStatus").ToInt(0);
            }

            string note = "";
            if (!string.IsNullOrEmpty(f.Get("txt-note")))
            {
                note = f.Get("txt-note").ToString();
            }
            int level = 0;
            if (!string.IsNullOrEmpty(f.Get("slLevel")))
            {
                level = f.Get("slLevel").ToInt(0);
            }

            var rs = ApprovalToLevelUpTable.update(id, status, note, null, aclog.UserName, level);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không tìm thấy yêu cầu", "e");

            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult GuiMailLenRank(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                string tieude = f.Get("txt-title").ToString();
                string noidung = f.Get("txt-content").ToString();
                string emails = f.Get("txt-emails").ToString();
                int id = f.Get("ID").ToInt(0); 
                AssetCRM.SendMailMultipleBcc(tieude, noidung, emails);
                using (var db = new ZimEntities())
                {
                    var check = db.tbl_ApprovalToLevelUp.Where(x => x.ID == id).FirstOrDefault();
                    check.IsSendMail = true;
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

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult GuiMailXuongRank(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                string tieude = f.Get("txt-title").ToString();
                string noidung = f.Get("txt-content").ToString();
                string emails = f.Get("txt-emails").ToString();
                int id = f.Get("ID").ToInt(0);
                AssetCRM.SendMailMultipleBcc(tieude, noidung, emails);
                using (var db = new ZimEntities())
                {
                    var check = db.tbl_ApprovalToLevelDown.Where(x => x.ID == id).FirstOrDefault();
                    check.IsSendMail = true;
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
    }
}