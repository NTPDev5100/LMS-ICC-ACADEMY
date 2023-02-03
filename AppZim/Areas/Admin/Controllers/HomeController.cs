using AppZim.Controllers;
using AppZim.Models;
using AppZim.TableSql;
using MB.Extensions;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebUI.Business;
using static AppZim.Areas.Admin.Controllers.CustomerController;
using static AppZim.Controllers.PushAutoController;

namespace AppZim.Areas.Admin.Controllers
{
    public class TVVData
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public double TotalInvoices { get; set; }
        public double StudentCount { get; set; }
        public double InvoicesCount { get; set; }
        public string Phone { get; set; }
    }
    public class CampaignRevenue
    {
        public int ID { get; set; }
        public DateTime SDate { get; set; }
        public DateTime EDate { get; set; }
        public string Date { get; set; }
        public double Total { get; set; }
    }
    public class DistrictStudent
    {
        public int DistrictID { get; set; }
        public string DistrictName { get; set; }
        public int StudentCount { get; set; }
    }
    //thống kê bài tập
    public class ExerciseDashboard
    {
        public int TuLuan { get; set; }
        public int TracNghiem { get; set; }
        public int NgheNoi { get; set; }
        public int DaNop { get; set; }
        public int DanhGia { get; set; }
        public int Chamlai { get; set; }

        public int HenTest { get; set; }
        public int DenTest { get; set; }
        public int DangKyHoc { get; set; }
        //New update 11032021
        public int BaoLuu { get; set; }
        public int HocNhieuLan { get; set; }
        public int NoHocPhi { get; set; }
        public int TongDangKyHoc { get; set; }
    }
    public class HomeController : BaseController
    {
        #region dashboard admin
        public static double Calculator(double a, double b)
        {
            if (a == 0 && b == 0)
                return 0;
            else
            {
                if (a == 0)
                    return -100;
                else if (b == 0)
                    return 100;
                else
                    return Math.Round(((a - b) / b) * 100, 2);
            }
        }
        public class Rate15
        {
            public string RateExercise { get; set; }
            public string RateTVV { get; set; }
            public List<tbl_ExerciseAddonStudent> Top5RateExercise { get; set; }
            public List<tbl_FeedBack> Top5RateTVV { get; set; }
        }
        public class ReportLanguage
        {
            public string Name { get; set; }
            public double Count { get; set; }
        }
        public class ReportDongTienLanDau
        {
            public string Name { get; set; }
            public double Count { get; set; }
        }
        public static async Task<Rate15> TinhTungRate15()
        {
            Rate15 l = new Rate15();
            await Task.Run(() =>
            {
                //var Rate1 = ExerciseAddonStudentTable.getcountbyrate(1);
                //var Rate2 = ExerciseAddonStudentTable.getcountbyrate(2);
                //var Rate3 = ExerciseAddonStudentTable.getcountbyrate(3);
                //var Rate4 = ExerciseAddonStudentTable.getcountbyrate(4);
                //var Rate5 = ExerciseAddonStudentTable.getcountbyrate(5);
                //var TotalRate = Rate1 + Rate2 + Rate3 + Rate4 + Rate5;

                //var Top5RateExercise = ExerciseAddonStudentTable.getTop5Rate();
                //string dataRateExercise = new JavaScriptSerializer().Serialize(new
                //{
                //    Rate1 = new List<double>() { Rate1, Math.Round((Convert.ToDouble(Rate1) / TotalRate) * 100, 1) }.ToArray(),
                //    Rate2 = new List<double>() { Rate2, Math.Round((Convert.ToDouble(Rate2) / TotalRate) * 100, 1) }.ToArray(),
                //    Rate3 = new List<double>() { Rate3, Math.Round((Convert.ToDouble(Rate3) / TotalRate) * 100, 1) }.ToArray(),
                //    Rate4 = new List<double>() { Rate4, Math.Round((Convert.ToDouble(Rate4) / TotalRate) * 100, 1) }.ToArray(),
                //    Rate5 = new List<double>() { Rate5, Math.Round((Convert.ToDouble(Rate5) / TotalRate) * 100, 1) }.ToArray(),
                //    TotalRate = Math.Round((Rate1 * 1 + Rate2 * 2 + Rate3 * 3 + Rate4 * 4 + Rate5 * 5) / Convert.ToDouble(TotalRate), 2)
                //});

                //l.RateExercise = dataRateExercise;
                //l.Top5RateExercise = Top5RateExercise;


                var RateTVV1 = FeedBackTable.getallbyrate(1);
                var RateTVV2 = FeedBackTable.getallbyrate(2);
                var RateTVV3 = FeedBackTable.getallbyrate(3);
                var RateTVV4 = FeedBackTable.getallbyrate(4);
                var RateTVV5 = FeedBackTable.getallbyrate(5);
                var TotalRateTVV = RateTVV1 + RateTVV2 + RateTVV3 + RateTVV4 + RateTVV5;

                var Top5RateTVV = FeedBackTable.getTop5Rate();
                string dataRateTVV = new JavaScriptSerializer().Serialize(new
                {
                    RateTVV1 = new List<double>() { RateTVV1, Math.Round((Convert.ToDouble(RateTVV1) / TotalRateTVV) * 100, 1) }.ToArray(),
                    RateTVV2 = new List<double>() { RateTVV2, Math.Round((Convert.ToDouble(RateTVV2) / TotalRateTVV) * 100, 1) }.ToArray(),
                    RateTVV3 = new List<double>() { RateTVV3, Math.Round((Convert.ToDouble(RateTVV3) / TotalRateTVV) * 100, 1) }.ToArray(),
                    RateTVV4 = new List<double>() { RateTVV4, Math.Round((Convert.ToDouble(RateTVV4) / TotalRateTVV) * 100, 1) }.ToArray(),
                    RateTVV5 = new List<double>() { RateTVV5, Math.Round((Convert.ToDouble(RateTVV5) / TotalRateTVV) * 100, 1) }.ToArray(),
                    TotalRateTVV = Math.Round((RateTVV1 * 1 + RateTVV2 * 2 + RateTVV3 * 3 + RateTVV4 * 4 + RateTVV5 * 5) / Convert.ToDouble(TotalRateTVV), 2)
                });

                l.RateTVV = dataRateTVV;
                l.Top5RateTVV = Top5RateTVV;
            });
            return l;
        }
        // GET: Admin/Home
        [HttpGet]
        public async Task<ActionResult> Dashboard(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3 && aclog.RoleID != 6 && aclog.RoleID != 9)
                return RedirectToAction("Signin", "Login", new { area = "" });

            using (var db = new ZimEntities())
            {
                //Ngày đầu tháng
                DateTime firstDayOfMonth = new DateTime(GetDateTime.Now.Year, GetDateTime.Now.Month, 1);
                DateTime firstDayOfMonthPre = firstDayOfMonth.AddMonths(-1);
                //ngày cuối tháng
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                DateTime lastDayOfMonthPre = lastDayOfMonth.AddMonths(-1);

                var ListNewCourse = db.tbl_Courses.Where(n => n.CourseSelfStudy != true && firstDayOfMonth <= EntityFunctions.TruncateTime(n.CreatedDate) && EntityFunctions.TruncateTime(n.CreatedDate) <= lastDayOfMonth).ToList().Count();
                var ListPrevCourse = db.tbl_Courses.Where(n => n.CourseSelfStudy != true && firstDayOfMonthPre <= EntityFunctions.TruncateTime(n.CreatedDate) && EntityFunctions.TruncateTime(n.CreatedDate) <= lastDayOfMonthPre).ToList().Count();
                var percentNewCourse = Calculator(Convert.ToDouble(ListNewCourse), Convert.ToDouble(ListPrevCourse));


                var ListNewCustomer = db.tbl_Account.Where(n => n.RoleID == 5 && firstDayOfMonth <= EntityFunctions.TruncateTime(n.CreatedDate) && EntityFunctions.TruncateTime(n.CreatedDate) <= lastDayOfMonth).ToList().Count();
                var ListPrevCustomer = db.tbl_Account.Where(n => n.RoleID == 5 && firstDayOfMonthPre <= EntityFunctions.TruncateTime(n.CreatedDate) && EntityFunctions.TruncateTime(n.CreatedDate) <= lastDayOfMonthPre).ToList().Count();
                var percentNewCustomer = Calculator(Convert.ToDouble(ListNewCustomer), Convert.ToDouble(ListPrevCustomer));

                //var ListNewServicesCustomer = db.tbl_ServicesStudent.Where(n => firstDayOfMonth <= EntityFunctions.TruncateTime(n.CreatedDate) && EntityFunctions.TruncateTime(n.CreatedDate) <= lastDayOfMonth).ToList().Count();
                //var ListPrevServicesCustomer = db.tbl_ServicesStudent.Where(n => firstDayOfMonthPre <= EntityFunctions.TruncateTime(n.CreatedDate) && EntityFunctions.TruncateTime(n.CreatedDate) <= lastDayOfMonthPre).ToList().Count();
                //var percentNewServicesCustomer = Calculator(Convert.ToDouble(ListNewServicesCustomer), Convert.ToDouble(ListPrevServicesCustomer));

                var ListNewAppointment = db.tbl_StudentAppointment.Where(n => n.isHide != true && n.CourseStudentID == 0 && firstDayOfMonth <= EntityFunctions.TruncateTime(n.CreatedDate) && EntityFunctions.TruncateTime(n.CreatedDate) <= lastDayOfMonth).ToList().Count();
                var ListPrevAppointment = db.tbl_StudentAppointment.Where(n => n.isHide != true && n.CourseStudentID == 0 && firstDayOfMonthPre <= EntityFunctions.TruncateTime(n.CreatedDate) && EntityFunctions.TruncateTime(n.CreatedDate) <= lastDayOfMonthPre).ToList().Count();
                var percentNewAppointment = Calculator(Convert.ToDouble(ListNewAppointment), Convert.ToDouble(ListPrevAppointment));

                ViewBag.DataTopPercent = new JavaScriptSerializer().Serialize(new
                {
                    NewCourse = new List<double>() { ListNewCourse, percentNewCourse }.ToArray(),
                    NewCustomer = new List<double>() { ListNewCustomer, percentNewCustomer }.ToArray(),
                    NewServicesCustomer = new List<double>() { 0, 0 }.ToArray(),
                    NewAppointment = new List<double>() { ListNewAppointment, percentNewAppointment }.ToArray(),
                });

                var Circle1 = db.tbl_CoursesStudent.Where(n => n.AppointmentID != 0).ToList().Count;
                var Circle2 = db.tbl_CoursesStudent.Where(n => n.PriceLeft == 0).ToList().Count;
                var Total = db.tbl_CoursesStudent.ToList().Count;
                ViewBag.Circle = new List<int>() { Total, Circle1, Circle2 };

                //số bài chưa chấp xong
                //ViewBag.ExerciseProcrss = db.tbl_ExerciseAddonStudent.Where(n => n.StatusDone == 1 || n.StatusDone == 2).ToList().Count();
            }
            //======================
            //var checkfix = ExerciseAddonStudentCheckFixTable.getall().OrderByDescending(n => n.ID).Skip(0).Take(20).ToList();
            //var fail = checkfix.Where(n => n.StatusAcceptReject == 3).ToList();
            //double pf = (double)fail.Count / checkfix.Count * 100.0f;
            //ViewBag.FailFixPercent = Math.Round(pf, 2);
            //ViewBag.FailFix = fail.Count;
            //lấy sao và 5 bài gần nhât
            var rate15 = TinhTungRate15();

            var data15 = await rate15;

            //ViewBag.RateExercise = data15.RateExercise;
            //ViewBag.Top5RateExercise = data15.Top5RateExercise;

            ViewBag.RateTVV = data15.RateTVV;
            ViewBag.Top5RateTVV = data15.Top5RateTVV;
            //=========================

            //List<TVVData> l;
            //var rs = SqlHelper.ExecuteDataTable("lay_doanhthu_hoadon_hocvien_cua_tvv");
            //l = (from rw in rs.AsEnumerable()
            //     select new TVVData()
            //     {
            //         ID = Convert.ToInt32(rw["ID"]),
            //         FullName = rw["FullName"].ToString(),
            //         TotalInvoices = rw["TotalInvoices"] == DBNull.Value ? 0 : Convert.ToDouble(rw["TotalInvoices"]),
            //         StudentCount = rw["studentCount"] == DBNull.Value ? 0 : Convert.ToDouble(rw["studentCount"]),
            //         InvoicesCount = rw["countRow"] == DBNull.Value ? 0 : Convert.ToDouble(rw["countRow"]),
            //         Phone = rw["Phone"] == DBNull.Value ? "" : rw["Phone"].ToString(),
            //     }).ToList();
            //ViewBag.ListTVV = l;
            //=========================Report quận huyện
            //SqlParameter[] parameters =
            //  {
            //        new SqlParameter("@schoolID", SqlDbType.Int) { Value = 0 },
            //  };
            //List<DistrictStudent> ll;
            //var rss = SqlHelper.ExecuteDataTable("DistrictStudentCount", parameters);
            //ll = (from rw in rss.AsEnumerable()
            //      select new DistrictStudent()
            //      {
            //          DistrictID = Convert.ToInt32(rw["dis"]),
            //          DistrictName = rw["DistrictName"] == DBNull.Value ? "Chưa có" : rw["DistrictName"].ToString(),
            //          StudentCount = rw["sl"] == DBNull.Value ? 0 : Convert.ToInt32(rw["sl"])
            //      }).ToList();
            //List<DistrictStudent> final = new List<DistrictStudent>();
            //int get5 = 8;
            double conlai = 0;
            //for (int i = 0; i < l.Count(); i++)
            //{
            //    DistrictStudent item = ll[i];
            //    if (item.DistrictID == 0)
            //    {
            //        conlai += item.StudentCount;
            //    }
            //    else
            //    {
            //        if (get5 != 0)
            //        {
            //            get5--;
            //            final.Add(item);
            //        }
            //        else
            //        {
            //            conlai += item.StudentCount;
            //        }
            //    }
            //}
            //final.Add(new DistrictStudent() { DistrictID = 0, DistrictName = "Còn lại", StudentCount = conlai });

            //string datadist = new JavaScriptSerializer().Serialize(new
            //{
            //    data = final.ToArray()
            //});
            //ViewBag.DataDistrict = datadist;
            //Report điểm thi
            //SqlParameter[] parameters_exam =
            //{
            //    new SqlParameter("@centerid", SqlDbType.Int) { Value = aclog.SchoolID.Value },
            //    new SqlParameter("@qsearch", SqlDbType.NVarChar,2000) { Value = "" },
            //    new SqlParameter("@fromdate", SqlDbType.NVarChar,2000) { Value = null },
            //    new SqlParameter("@todate", SqlDbType.NVarChar,2000) { Value = null },
            //    new SqlParameter("@roleID", SqlDbType.Int) { Value = aclog.RoleID },
            //    new SqlParameter("@aclogID", SqlDbType.Int) { Value = aclog.ID },
            //};
            //var rs_exam = SqlHelper.ExecuteDataTable("Get_TestResult_Student", parameters_exam);
            //List<TestResults> l_exam = (from rw in rs_exam.AsEnumerable()
            //                            select new TestResults()
            //                            {
            //                                ID = Convert.ToInt32(rw["ID"]),
            //                                //FullName = rw["FullName"].ToString(),
            //                                //DateExam = Convert.ToDateTime(rw["DateExam"]),
            //                                //L = Convert.ToDouble(rw["L"]),
            //                                //S = Convert.ToDouble(rw["S"]),
            //                                //R = Convert.ToDouble(rw["R"]),
            //                                //W = Convert.ToDouble(rw["W"]),
            //                                //OverAll = Convert.ToDouble(rw["OverAll"]),
            //                                //SubTotal = Convert.ToDouble(rw["SubTotal"]),
            //                                //CamKet = Convert.ToDouble(rw["CamKet"]),
            //                                //Lech = Convert.ToDouble(rw["Lech"]),

            //                                ColorID = Convert.ToInt32(rw["ColorID"]),
            //                                Color = rw["Color"].ToString()
            //                            }).ToList();

            //List<DistrictStudent> final_exam = new List<DistrictStudent>();
            //final_exam.Add(new DistrictStudent() { DistrictID = 0, DistrictName = "Vượt", StudentCount = l_exam.Where(n => n.ColorID == 1).ToList().Count() });
            //final_exam.Add(new DistrictStudent() { DistrictID = 0, DistrictName = "Đạt", StudentCount = l_exam.Where(n => n.ColorID == 2).ToList().Count() });
            //final_exam.Add(new DistrictStudent() { DistrictID = 0, DistrictName = "Không đạt", StudentCount = l_exam.Where(n => n.ColorID == 3).ToList().Count() });
            //final_exam.Add(new DistrictStudent() { DistrictID = 0, DistrictName = "D1", StudentCount = l_exam.Where(n => n.ColorID == 4).ToList().Count() });
            //final_exam.Add(new DistrictStudent() { DistrictID = 0, DistrictName = "D2", StudentCount = l_exam.Where(n => n.ColorID == 5).ToList().Count() });
            //string datadist_exam = new JavaScriptSerializer().Serialize(new
            //{
            //    data = final_exam.ToArray()
            //});

            //ViewBag.DataExamReport = datadist_exam;
            ////thống kê bài tập 

            //var rsex = SqlHelper.ExecuteDataTable("thong_ke_bai_tap_dashboard");
            //var exds = (from rw in rsex.AsEnumerable()
            //            select new ExerciseDashboard()
            //            {
            //                TuLuan = Convert.ToInt32(rw["TuLuan"]),
            //                TracNghiem = Convert.ToInt32(rw["TracNghiem"]),
            //                NgheNoi = Convert.ToInt32(rw["NgheNoi"]),
            //                DaNop = Convert.ToInt32(rw["DaNop"]),
            //                DanhGia = Convert.ToInt32(rw["DanhGia"]),
            //                Chamlai = Convert.ToInt32(rw["Chamlai"]),

            //                HenTest = Convert.ToInt32(rw["HenTest"]),
            //                DenTest = Convert.ToInt32(rw["DenTest"]),
            //                DangKyHoc = Convert.ToInt32(rw["DangKyHoc"]),
            //                BaoLuu = Convert.ToInt32(rw["BaoLuu"]),
            //                HocNhieuLan = Convert.ToInt32(rw["HocNhieuLan"]),
            //                NoHocPhi = Convert.ToInt32(rw["NoHocPhi"]),
            //                TongDangKyHoc = Convert.ToInt32(rw["TongDangKyHoc"])
            //            }).ToList();
            //ViewBag.EXDS = exds[0];

            //double tiledentest = Math.Round((double)exds[0].DenTest / exds[0].HenTest * 100, 2);
            //double titledangky = Math.Round((double)exds[0].DangKyHoc / exds[0].DenTest * 100, 2);

            //double tileHoanThanhHP = Math.Round((double)(exds[0].TongDangKyHoc- exds[0].NoHocPhi) / exds[0].TongDangKyHoc * 100, 2); //tỉ lệ hoàn thành học phí
            //double tileDang2Lan = Math.Round((double)exds[0].HocNhieuLan / exds[0].TongDangKyHoc * 100, 2); //tỉ lệ đăng ký học lại
            //ViewBag.TiLeTest = tiledentest;
            //ViewBag.TileDangKy = titledangky;
            //ViewBag.BaoLuu = exds[0].BaoLuu;
            //ViewBag.HocVienCoCongNo = exds[0].NoHocPhi;
            //ViewBag.HoanThanhHocPhi = exds[0].TongDangKyHoc - exds[0].NoHocPhi;
            //ViewBag.TiLeHoanThanhHocPhi = tileHoanThanhHP;
            //ViewBag.HocNhhieuLan = exds[0].HocNhieuLan;
            //ViewBag.TiLeHocNhieuLan = tileDang2Lan;
            //=========================
            // THONG KE
            var lacc = AccountTable.getall();
            using (var db = new ZimEntities())
            {
                int teamID = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["slTeam"]))
                {
                    teamID = Request.QueryString["slTeam"].ToInt(0);
                }
                ViewBag.TeamID = teamID;

                int memberID = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["slMember"]))
                {
                    memberID = Request.QueryString["slMember"].ToInt(0);
                }
                if (aclog.RoleID == 6)
                    memberID = aclog.ID;
                ViewBag.MemberID = memberID;
                // thời gian search
                DateTime currentStart = DateTime.Now.FirstDayOfMonth();
                if (!string.IsNullOrEmpty(Request.QueryString["from"]))
                {
                    currentStart = DateTime.ParseExact(Request.QueryString["from"].ToString(), "d/M/yyyy",null);
                }
                ViewBag.From = currentStart.ToString("dd/MM/yyyy");
                DateTime currentEnd = DateTime.Now.LastDayOfMonth();
                if (!string.IsNullOrEmpty(Request.QueryString["to"]))
                {
                    currentEnd = DateTime.ParseExact(Request.QueryString["to"].ToString(), "d/M/yyyy", null);
                }
                ViewBag.To = currentEnd.ToString("dd/MM/yyyy");
                // tháng trước thời gian search
                DateTime preStart = currentStart.AddMonths(-1).FirstDayOfMonth();
                DateTime preEnd = preStart.LastDayOfMonth();
                var statisticalValue = db.Admin_Statistical_GetValue(currentStart, currentEnd, preStart, preEnd, teamID, memberID).FirstOrDefault();
                ViewBag.Statistical = statisticalValue;
                // THONG KE HOC VIEN TRONG TUNG NGON NGU
                var obj = new List<Object>();
                var lRL = new List<ReportLanguage>(); // thang nay
                var _lRL = new List<ReportLanguage>(); // thang truoc
                // danh sach ngon ngu
                var lLanguage = db.tbl_Language.Where(x => x.isDelete != true).ToList();
                // danh sach hoc vien - thang hien tai
                var lHV = db.tbl_Account.Where(x => (currentStart <= x.CreatedDate && x.CreatedDate <= currentEnd)
                                                        && x.IsDelete != true && x.RoleID == 5).ToList();
                // danh sach hoc vien - thang truoc
                var _lHV = db.tbl_Account.Where(x => (preStart <= x.CreatedDate && x.CreatedDate <= preEnd)
                                                        && x.IsDelete != true && x.RoleID == 5).ToList();
                foreach (var item in lLanguage)
                {
                    // 
                    var i = new ReportLanguage();
                    i.Name = item.Name;
                    i.Count = lHV.Where(x => item.ID == x.Language).ToList().Count;
                    lRL.Add(i);
                    // 
                    i = new ReportLanguage();
                    i.Name = item.Name;
                    i.Count = _lHV.Where(x => item.ID == x.Language).ToList().Count;
                    _lRL.Add(i);
                }
                lRL = lRL.Take(4).ToList();
                _lRL = _lRL.Take(4).ToList();
                double iconlai = lHV.Count - lRL.Sum(x => x.Count);
                lRL.Add(new ReportLanguage { Name = "Khác", Count = iconlai });
                iconlai = _lHV.Count - _lRL.Sum(x => x.Count);
                _lRL.Add(new ReportLanguage { Name = "Khác", Count = iconlai });
                // th lRL < 5
                while (lRL.Count < 5)
                {
                    lRL.Add(new ReportLanguage { Name = "", Count = 0 });
                }
                while (_lRL.Count < 5)
                {
                    _lRL.Add(new ReportLanguage { Name = "", Count = 0 });
                }
                ViewBag.NgonNguPre = _lRL;
                ViewBag.NgonNgu = lRL;
                // TONG KET PHAN TRAM HOC VIEN DONG LAN DAU
                string[] nameCoe = { "", "Tháng", "50%", "Hoàn tất học phí", "Khác" };
                var lTien = new List<ReportDongTienLanDau>();
                var _lTien = new List<ReportDongTienLanDau>();
                for (int i = 1; i <= 4; i++)
                {
                    var er = new ReportDongTienLanDau();
                    er.Name = nameCoe[i];
                    er.Count = db.tbl_StudentAppointment.Where(x => (currentStart <= x.CreatedDate && x.CreatedDate <= currentEnd)
                                                                    && x.CoefficientID == i).ToList().Count;
                    lTien.Add(er);
                    // tinh thang truoc
                    int demCount = db.tbl_StudentAppointment.Where(x => (preStart <= x.CreatedDate && x.CreatedDate <= preEnd)
                                                                    && x.CoefficientID == i).ToList().Count;
                    _lTien.Add(new ReportDongTienLanDau { Name = er.Name, Count = demCount });
                }
                ViewBag.DongTienLanDau = lTien;
                ViewBag.DongTienLanDauPre = _lTien;
                // SO LUONG KHOA HOC
                ViewBag.SoKhoaHoc = db.tbl_Courses.Where(x => x.IsDelete != true && (currentStart <= x.CreatedDate && x.CreatedDate <= currentEnd)).ToList().Count;
                ViewBag.SoKhoaHocPre = db.tbl_Courses.Where(x => x.IsDelete != true && (preStart <= x.CreatedDate && x.CreatedDate <= preEnd)).ToList().Count;

                #region THONG KE GIAO VIEN THEO NGON NGU
                var lRLGV = new List<ReportLanguage>();
                // danh sach giao vien
                var lgv = db.tbl_Account.Where(x => (currentStart <= x.CreatedDate && x.CreatedDate <= currentEnd)
                                                        && x.IsDelete != true && x.RoleID == 4).ToList();
                foreach (var item in lLanguage)
                {
                    var i = new ReportLanguage();
                    i.Name = item.Name;
                    i.Count = lgv.Where(x => item.ID == x.Language).ToList().Count;
                    lRLGV.Add(i);
                }
                lRLGV = lRLGV.OrderByDescending(x => x.Count).Take(4).ToList();
                var iconlaigv = lgv.Count - lRLGV.Sum(x => x.Count);
                lRLGV.Add(new ReportLanguage { Name = "Khác", Count = iconlaigv });
                while (lRLGV.Count < 5)
                {
                    lRLGV.Add(new ReportLanguage { Name = "", Count = 0 });
                }
                ViewBag.NgonNguGV = lRLGV;
                #endregion

                ViewBag.TongSoBuoi = db.tbl_CoursesSchedule.Where(x => (currentStart <= x.Date && x.Date <= currentEnd)).ToList().Count;
                ViewBag.TongSoBuoiPre = db.tbl_CoursesSchedule.Where(x => (preStart <= x.Date && x.Date <= preEnd)).ToList().Count;

                ViewBag.SoBuoiDaHoc = db.tbl_CoursesSchedule.Where(x => (currentStart <= x.Date && x.Date <= currentEnd) && x.FisnishStatus == 1).ToList().Count;
                ViewBag.SoBuoiDaHocPre = db.tbl_CoursesSchedule.Where(x => (preStart <= x.Date && x.Date <= preEnd) && x.FisnishStatus == 1).ToList().Count;

                ViewBag.HVHoanThanh = db.Admin_Student_0LessonCourseList("", -1, -1, -1).Where(x => currentStart <= x.CreatedDate && x.CreatedDate <= currentEnd).Count();
                ViewBag.HVHoanThanhPre = db.Admin_Student_0LessonCourseList("", -1, -1, -1).Where(x => preStart <= x.CreatedDate && x.CreatedDate <= preEnd).Count();

                ViewBag.LuongGiaoVien = db.tbl_SalaryPaidHistory.Where(x => currentStart <= x.CreatedDate && x.CreatedDate <= currentEnd).ToList().Sum(x => x.PaidReal);
                ViewBag.LuongGiaoVienPre = db.tbl_SalaryPaidHistory.Where(x => preStart <= x.CreatedDate && x.CreatedDate <= preEnd).ToList().Sum(x => x.PaidReal);
                // so lop theo ngon ngu
                #region THONG KE COURSE THEO NGON NGU
                var dsLop = new List<ReportLanguage>();
                var _dsLop = new List<ReportLanguage>();
                var lCourse = db.tbl_Courses.Where(x => currentStart <= x.CreatedDate && x.CreatedDate <= currentEnd).ToList();
                var _lCourse = db.tbl_Courses.Where(x => preStart <= x.CreatedDate && x.CreatedDate <= preEnd).ToList();
                foreach (var item in lLanguage)
                {
                    var i = new ReportLanguage();
                    i.Name = item.Name;
                    i.Count = db.SoLopTheoNgonNgu(item.ID).Where(x => currentStart <= x.CreatedDate && x.CreatedDate <= currentEnd).ToList().Count();
                    dsLop.Add(i);
                    // thang truoc
                    var _i = new ReportLanguage();
                    _i.Name = item.Name;
                    _i.Count = db.SoLopTheoNgonNgu(item.ID).Where(x => preStart <= x.CreatedDate && x.CreatedDate <= preEnd).ToList().Count();
                    _dsLop.Add(_i);
                }
                dsLop = dsLop.Take(4).ToList();
                iconlai = lCourse.Count - dsLop.Sum(x => x.Count);
                dsLop.Add(new ReportLanguage { Name = "Khác", Count = iconlai });
                while (dsLop.Count < 5)
                {
                    dsLop.Add(new ReportLanguage { Name = "", Count = 0 });
                }
                ViewBag.CourseOfLanguage = dsLop;
                // --
                _dsLop = _dsLop.Take(4).ToList();
                iconlai = _lCourse.Count - _dsLop.Sum(x => x.Count);
                _dsLop.Add(new ReportLanguage { Name = "Khác", Count = iconlai });
                while (_dsLop.Count < 5)
                {
                    _dsLop.Add(new ReportLanguage { Name = "", Count = 0 });
                }
                ViewBag.CourseOfLanguagePre = _dsLop;
                #endregion

                #region THONG KE LY DO KHONG HOC
                // THONG KE LY DO KHONG HOC
                var khonghoc = new List<ReportLanguage>();
                var lreason = db.tbl_Reason.Where(x => x.isDelete != true && x.Type == 2).ToList();
                // data thang nay
                foreach (var item in lreason)
                {
                    var ck = new ReportLanguage();
                    ck.Name = item.Name;
                    ck.Count = db.tbl_StudentAppointmentTestDetail.Where(x => (currentStart <= x.CreatedDate && x.CreatedDate < currentEnd) && x.ReasonID == item.ID).ToList().Count;
                    khonghoc.Add(ck);
                }
                khonghoc = khonghoc.OrderByDescending(x => x.Count).Take(4).ToList();
                while (khonghoc.Count < 5)
                {
                    khonghoc.Add(new ReportLanguage { Name = "", Count = 0 });
                }
                ViewBag.StudyFail = khonghoc;
                // data thang truoc
                var khonghocpre = new List<ReportLanguage>();
                foreach (var item in khonghoc)
                {
                    int idreason = ReasonTable.GetIDbyName(item.Name);
                    var ck = new ReportLanguage();
                    ck.Name = item.Name;
                    ck.Count = db.tbl_StudentAppointmentTestDetail.Where(x => (preStart <= x.CreatedDate && x.CreatedDate < preEnd) && x.ReasonID == idreason).ToList().Count;
                    khonghocpre.Add(ck);
                }
                ViewBag.StudyFailPre = khonghocpre;
                #endregion
                #region THOGN KE LY DO DK HOC
                // THONG KE LY DO DK HOC
                var hoc = new List<ReportLanguage>();
                var lreasonHoc = db.tbl_Reason.Where(x => x.isDelete != true && x.Type == 1).ToList();
                // data thang nay
                foreach (var item in lreasonHoc)
                {
                    var ck = new ReportLanguage();
                    ck.Name = item.Name;
                    ck.Count = db.tbl_StudentAppointmentTestDetail.Where(x => (currentStart <= x.CreatedDate && x.CreatedDate < currentEnd) && x.ReasonID == item.ID).ToList().Count;
                    hoc.Add(ck);
                }
                hoc = hoc.OrderByDescending(x => x.Count).Take(4).ToList();
                while (hoc.Count < 5)
                {
                    hoc.Add(new ReportLanguage { Name = "", Count = 0 });
                }
                ViewBag.Study = hoc;
                // data thang truoc
                var hocpre = new List<ReportLanguage>();
                foreach (var item in hoc)
                {
                    int idreason = ReasonTable.GetIDbyName(item.Name);
                    var ck = new ReportLanguage();
                    ck.Name = item.Name;
                    ck.Count = db.tbl_StudentAppointmentTestDetail.Where(x => (preStart <= x.CreatedDate && x.CreatedDate < preEnd) && x.ReasonID == idreason).ToList().Count;
                    hocpre.Add(ck);
                }
                ViewBag.StudyPre = hocpre;
                #endregion
                // GIAO VIEN DAY THU
                ViewBag.GiaoVienDayThuPre = db.tbl_StudentAppointmentTestDetail.Where(x => preStart <= x.CreatedDate && x.CreatedDate <= preEnd).Select(x => x.TeacherTestID).Distinct().ToList().Count;
                ViewBag.GiaoVienDayThu = db.tbl_StudentAppointmentTestDetail.Where(x => currentStart <= x.CreatedDate && x.CreatedDate <= currentEnd).Select(x => x.TeacherTestID).Distinct().ToList().Count;
                // SO BUOI DAY THU
                ViewBag.SoBuoiDayThuPre = db.tbl_StudentAppointmentTestDetail.Where(x => preStart <= x.CreatedDate && x.CreatedDate <= preEnd).ToList().Count;
                ViewBag.SoBuoiDayThu = db.tbl_StudentAppointmentTestDetail.Where(x => currentStart <= x.CreatedDate && x.CreatedDate <= currentEnd).ToList().Count;
                #region THONG KE LUONG GIAO VIEN THEO NGON NGU
                var lSalary = new List<ReportLanguage>();
                double tongluong = db.tbl_SalaryPaidHistory.Where(x => currentStart <= x.CreatedDate && x.CreatedDate <= currentEnd).ToList().Sum(x => x.PaidReal).Value;
                foreach (var item in lLanguage)
                {
                    var ck = new ReportLanguage();
                    ck.Name = item.Name;
                    ck.Count = db.CalcSallaryByLanguage(item.ID).Where(x => currentStart <= x.CreatedDate && x.CreatedDate <= currentEnd).ToList().Sum(x => x.PaidReal).Value;
                    lSalary.Add(ck);
                }
                lSalary = lSalary.OrderByDescending(x => x.Count).Take(4).ToList();
                conlai = tongluong - lSalary.Sum(x => x.Count);
                lSalary.Add(new ReportLanguage { Name = "Khác", Count = conlai });
                while (lSalary.Count < 5)
                {
                    lSalary.Add(new ReportLanguage { Name = "", Count = 0 });
                }
                ViewBag.Salary = lSalary;
                // thang truoc
                var lSalaryPre = new List<ReportLanguage>();
                double tongluongPre = db.tbl_SalaryPaidHistory.Where(x => preStart <= x.CreatedDate && x.CreatedDate <= preEnd).ToList().Sum(x => x.PaidReal).Value;
                foreach (var item in lSalary)
                {
                    int idlanguage = Language.GetIdByName(item.Name);
                    if (idlanguage == 0)
                        break;
                    var ck = new ReportLanguage();
                    ck.Name = item.Name;
                    ck.Count = db.CalcSallaryByLanguage(idlanguage).Where(x => preStart <= x.CreatedDate && x.CreatedDate <= preEnd).ToList().Sum(x => x.PaidReal).Value;
                    lSalaryPre.Add(ck);
                }
                conlai = tongluongPre - lSalaryPre.Sum(x => x.Count);
                lSalaryPre.Add(new ReportLanguage { Name = "Khác", Count = conlai });
                while (lSalaryPre.Count < 5)
                {
                    lSalaryPre.Add(new ReportLanguage { Name = "", Count = 0 });
                }
                ViewBag.SalaryPre = lSalaryPre;
                #endregion
            }
            return View();
        }

        static int GetWeekNumberOfMonth(DateTime date)
        {
            date = date.Date;
            DateTime firstMonthDay = new DateTime(date.Year, date.Month, 1);
            DateTime firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);
            if (firstMonthMonday > date)
            {
                firstMonthDay = firstMonthDay.AddMonths(-1);
                firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);
            }
            return (date - firstMonthMonday).Days / 7 + 1;
        }

        [HttpPost] //Tinh data dashboard
        public ActionResult GetDataRevenue(int zimID)
        {
            if (zimID == 0)
            {
                //DataMonth    
                DateTime firstDayOfMonth = new DateTime(GetDateTime.Now.Year, GetDateTime.Now.Month, 1);
                //ngày cưới tháng
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                int week = GetWeekNumberOfMonth(lastDayOfMonth);

                //tính doanh thu trong tháng hiện tại         

                List<double> dataMonthChart = new List<double>();
                var l = InvoicesTable.getall().Where(n => n.Refund != true && n.CreatedDate.Value.Year == GetDateTime.Now.Year && n.CreatedDate.Value.Month == GetDateTime.Now.Month).ToList();

                var dataPreMonth = InvoicesTable.getall().Where(n => n.Refund != true && n.CreatedDate.Value.Year == GetDateTime.Now.AddMonths(-1).Year && n.CreatedDate.Value.Month == GetDateTime.Now.AddMonths(-1).Month).ToList();

                double currentMonth = l.Sum(i => Convert.ToDouble(i.Price.Value));
                double preMonth = dataPreMonth.Sum(i => Convert.ToDouble(i.Price.Value));

                double PercentMonth = 0;
                if (currentMonth == 0 && preMonth == 0)
                    PercentMonth = 0;
                else
                {
                    if (currentMonth == 0)
                        PercentMonth = -100;
                    else if (preMonth == 0)
                        PercentMonth = 100;
                    else
                        PercentMonth = Math.Round(((currentMonth - preMonth) / preMonth) * 100, 2);
                }

                int day = (lastDayOfMonth - firstDayOfMonth).Days;
                for (int o = 0; o <= day; o++)
                {
                    DateTime stime = firstDayOfMonth.AddDays(o).Date;
                    List<tbl_Invoices> lweek = new List<tbl_Invoices>();
                    lweek = l.Where(n => n.CreatedDate.Value.Date == stime.Date).ToList();

                    double ttprice = lweek.Sum(i => Convert.ToDouble(i.Price.Value));
                    dataMonthChart.Add(ttprice);
                }

                //DataYear
                List<double> dataYearChart = new List<double>();

                var dataYear = InvoicesTable.getall().Where(n => n.Refund != true && n.CreatedDate.Value.Year == GetDateTime.Now.Year).ToList();

                var dataPreYear = InvoicesTable.getall().Where(n => n.Refund != true && n.CreatedDate.Value.Year == GetDateTime.Now.AddYears(-1).Year).ToList();

                double CurrentYear = dataYear.Sum(i => Convert.ToDouble(i.Price.Value));
                double PreYear = dataPreYear.Sum(i => Convert.ToDouble(i.Price.Value));

                double PercentYear = 0;
                if (CurrentYear == 0 && PreYear == 0)
                    PercentYear = 0;
                else
                {
                    if (CurrentYear == 0)
                        PercentYear = -100;
                    else if (PreYear == 0)
                        PercentYear = 100;
                    else
                        PercentYear = Math.Round(((CurrentYear - PreYear) / PreYear) * 100, 2);
                }

                var totalStudentAppointment = StudentAppointmentTable.getall().Where(n => n.CreatedDate.Value.Year == GetDateTime.Now.Year && n.isHide != true && n.CourseStudentID == 0).ToList();
                var totalStudentStudying = CoursesStudentTable.getall().Where(n => n.isHide != true && n.CreatedDate.Value.Year == GetDateTime.Now.Year).ToList();
                List<int> dataStudentAppointment = new List<int>();
                List<int> dataStudentStudying = new List<int>();
                int StudentAppointmentAll = StudentAppointmentTable.getall().Where(n => n.CreatedDate.Value.Year == GetDateTime.Now.Year && n.isHide != true && n.CourseStudentID == 0).ToList().Count();
                int StudentStudyingAll = CoursesStudentTable.getall().Where(n => n.isHide != true && n.CreatedDate.Value.Year == GetDateTime.Now.Year).Select(n => n.StudentUID).Distinct().ToList().Count;
                for (int i = 1; i <= 12; i++)
                {
                    var lmonth = dataYear.Where(n => n.CreatedDate.Value.Month == i).ToList();
                    double monthrice = lmonth.Sum(p => Convert.ToDouble(p.Price.Value));
                    dataYearChart.Add(monthrice);

                    int countAppoinment = 0;
                    int countStudying = 0;
                    countAppoinment = totalStudentAppointment.Where(n => n.CreatedDate.Value.Month == i).ToList().Count();
                    countStudying = totalStudentStudying.Where(n => n.CreatedDate.Value.Month == i).Select(n => n.StudentUID).Distinct().ToList().Count;
                    dataStudentAppointment.Add(countAppoinment);
                    dataStudentStudying.Add(countStudying);
                }
                string dataStudent = new JavaScriptSerializer().Serialize(new
                {
                    Appointment = dataStudentAppointment.ToArray(),
                    AllApointment = string.Format("{0:N0}", Convert.ToDouble(StudentAppointmentAll)),
                    Studying = dataStudentStudying.ToArray(),
                    AllStudying = string.Format("{0:N0}", Convert.ToDouble(StudentStudyingAll))
                });
                string data = new JavaScriptSerializer().Serialize(new
                {
                    month = dataMonthChart.ToArray(),
                    year = dataYearChart.ToArray(),
                    percentMonth = PercentMonth,
                    percentYear = PercentYear
                });
                return Json(new { rs = true, dataChart = data, dataStudentChart = dataStudent }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var center = SchoolTable.getbyID(zimID);
                if (center != null)
                {
                    //DataMonth    
                    DateTime firstDayOfMonth = new DateTime(GetDateTime.Now.Year, GetDateTime.Now.Month, 1);
                    //ngày cưới tháng
                    DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                    int week = GetWeekNumberOfMonth(lastDayOfMonth);

                    //tính doanh chiến dịch trong tháng hiện tại              
                    List<double> dataMonthChart = new List<double>();
                    var l = InvoicesTable.getall().Where(n => n.Refund != true && n.CreatedDate.Value.Year == GetDateTime.Now.Year && n.CreatedDate.Value.Month == GetDateTime.Now.Month && n.SchoolID == zimID).ToList();
                    int day = (lastDayOfMonth - firstDayOfMonth).Days;
                    var dataPreMonth = InvoicesTable.getall().Where(n => n.Refund != true && n.CreatedDate.Value.Year == GetDateTime.Now.AddMonths(-1).Year && n.CreatedDate.Value.Month == GetDateTime.Now.AddMonths(-1).Month && n.SchoolID == zimID).ToList();

                    double currentMonth = l.Sum(i => Convert.ToDouble(i.Price.Value));
                    double preMonth = dataPreMonth.Sum(i => Convert.ToDouble(i.Price.Value));

                    double PercentMonth = 0;
                    if (currentMonth == 0 && preMonth == 0)
                        PercentMonth = 0;
                    else
                    {
                        if (currentMonth == 0)
                            PercentMonth = -100;
                        else if (preMonth == 0)
                            PercentMonth = 100;
                        else
                            PercentMonth = Math.Round(((currentMonth - preMonth) / preMonth) * 100, 2);
                    }
                    for (int o = 0; o <= day; o++)
                    {
                        DateTime stime = firstDayOfMonth.AddDays(o).Date;

                        List<tbl_Invoices> lweek = new List<tbl_Invoices>();
                        lweek = l.Where(n => n.CreatedDate.Value.Date == stime.Date).ToList();
                        double ttprice = lweek.Sum(p => Convert.ToDouble(p.Price.Value));
                        dataMonthChart.Add(ttprice);
                    }

                    //DataYear
                    var dataYear = InvoicesTable.getall().Where(n => n.Refund != true && n.CreatedDate.Value.Year == GetDateTime.Now.Year && n.SchoolID == zimID).ToList();
                    List<double> dataYearChart = new List<double>();

                    var dataPreYear = InvoicesTable.getall().Where(n => n.Refund != true && n.CreatedDate.Value.Year == GetDateTime.Now.AddYears(-1).Year && n.SchoolID == zimID).ToList();

                    double CurrentYear = dataYear.Sum(i => Convert.ToDouble(i.Price.Value));
                    double PreYear = dataPreYear.Sum(i => Convert.ToDouble(i.Price.Value));

                    double PercentYear = 0;
                    if (CurrentYear == 0 && PreYear == 0)
                        PercentYear = 0;
                    else
                    {
                        if (CurrentYear == 0)
                            PercentYear = -100;
                        else if (PreYear == 0)
                            PercentYear = 100;
                        else
                            PercentYear = Math.Round(((CurrentYear - PreYear) / PreYear) * 100, 2);
                    }
                    var totalStudentAppointment = StudentAppointmentTable.getall().Where(n => n.CreatedDate.Value.Year == GetDateTime.Now.Year && n.SchoolID == zimID && n.isHide != true && n.CourseStudentID == 0).ToList();
                    var totalStudentStudying = CoursesStudentTable.getall().Where(n => n.isHide != true && n.CreatedDate.Value.Year == GetDateTime.Now.Year && n.SchoolID == zimID).ToList();
                    int StudentAppointmentAll = StudentAppointmentTable.getall().Where(n => n.CreatedDate.Value.Year == GetDateTime.Now.Year && n.SchoolID == zimID && n.isHide != true && n.CourseStudentID == 0).ToList().Count();
                    int StudentStudyingAll = CoursesStudentTable.getall().Where(n => n.isHide != true && n.CreatedDate.Value.Year == GetDateTime.Now.Year && n.SchoolID == zimID).Select(n => n.StudentUID).Distinct().ToList().Count;
                    List<int> dataStudentAppointment = new List<int>();
                    List<int> dataStudentStudying = new List<int>();

                    for (int i = 1; i <= 12; i++)
                    {
                        var lmonth = dataYear.Where(n => n.CreatedDate.Value.Month == i).ToList();
                        int countAppoinment = 0;
                        int countStudying = 0;

                        countAppoinment = totalStudentAppointment.Where(n => n.CreatedDate.Value.Month == i).ToList().Count();
                        countStudying = totalStudentStudying.Where(n => n.CreatedDate.Value.Month == i).Select(n => n.StudentUID).Distinct().ToList().Count;
                        double monthrice = lmonth.Sum(p => Convert.ToDouble(p.Price.Value));

                        dataYearChart.Add(monthrice);
                        dataStudentAppointment.Add(countAppoinment);
                        dataStudentStudying.Add(countStudying);
                    }
                    string dataStudent = new JavaScriptSerializer().Serialize(new
                    {
                        Appointment = dataStudentAppointment.ToArray(),
                        AllApointment = string.Format("{0:N0}", Convert.ToDouble(StudentAppointmentAll)),
                        Studying = dataStudentStudying.ToArray(),
                        AllStudying = string.Format("{0:N0}", Convert.ToDouble(StudentStudyingAll))
                    });
                    string data = new JavaScriptSerializer().Serialize(new
                    {
                        month = dataMonthChart.ToArray(),
                        year = dataYearChart.ToArray(),
                        percentMonth = PercentMonth,
                        percentYear = PercentYear
                    });
                    return Json(new { rs = true, dataChart = data, dataStudentChart = dataStudent }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Lấy thông tin doanh thu của tư vấn viên theo chiến dịch
        /// </summary>
        /// <param name="tvvID"></param>
        /// <param name="sdate"></param>
        /// <param name="edate"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LoadCampaignRevenueTVV(int tvvID, string sdate, string edate)
        {
            var tvv = AccountTable.getbyID(tvvID);
            if (tvv == null)
                return Json(new { rs = false, message = "Khong tim thay tvv" }, JsonRequestBehavior.AllowGet);

            DateTime st = new DateTime();
            DateTime et = GetDateTime.Now;
            if (!string.IsNullOrEmpty(sdate))
            {
                st = DateTime.ParseExact(sdate, "dd/MM/yyyy", null);
            }
            if (!string.IsNullOrEmpty(edate))
            {
                et = DateTime.ParseExact(edate, "dd/MM/yyyy", null);
            }

            SqlParameter[] parameters =
              {
                    new SqlParameter("@startDate", SqlDbType.Date) { Value = st },
                    new SqlParameter("@endDate", SqlDbType.Date) { Value =et },
                    new SqlParameter("@TVVName", SqlDbType.NVarChar) { Value = tvv.UserName },
                };
            List<CampaignRevenue> l;
            var rs = SqlHelper.ExecuteDataTable("getCampaignRevenueTVV", parameters);
            l = (from rw in rs.AsEnumerable()
                 select new CampaignRevenue()
                 {
                     ID = Convert.ToInt32(rw["ID"]),
                     SDate = Convert.ToDateTime(rw["sTime"]),
                     EDate = Convert.ToDateTime(rw["eTime"]),
                     Total = rw["Total"] == DBNull.Value ? 0 : Convert.ToDouble(rw["Total"]),
                     Date = Convert.ToDateTime(rw["sTime"]).ToString("dd/MM") + " - " + Convert.ToDateTime(rw["eTime"]).ToString("dd/MM")
                 }).ToList();

            return Json(new { rs = true, data = l }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Lấy số lượng học sinh theo quận
        /// </summary>
        /// <param name="schoolID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LoadDistrictStudentCount(int schoolID)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            SqlParameter[] parameters =
              {
                    new SqlParameter("@schoolID", SqlDbType.Int) { Value = schoolID },
              };
            List<DistrictStudent> l;
            var rs = SqlHelper.ExecuteDataTable("DistrictStudentCount", parameters);
            l = (from rw in rs.AsEnumerable()
                 select new DistrictStudent()
                 {
                     DistrictID = Convert.ToInt32(rw["dis"]),
                     DistrictName = rw["DistrictName"] == DBNull.Value ? "Chưa có" : rw["DistrictName"].ToString(),
                     StudentCount = rw["sl"] == DBNull.Value ? 0 : Convert.ToInt32(rw["sl"])
                 }).ToList();
            List<DistrictStudent> final = new List<DistrictStudent>();
            int get5 = 8;
            int conlai = 0;
            for (int i = 0; i < l.Count; i++)
            {
                DistrictStudent item = l[i];
                if (item.DistrictID == 0)
                {
                    conlai += item.StudentCount;
                }
                else
                {
                    if (get5 != 0)
                    {
                        get5--;
                        final.Add(item);
                    }
                    else
                    {
                        conlai += item.StudentCount;
                    }
                }
            }
            final.Add(new DistrictStudent() { DistrictID = 0, DistrictName = "Còn lại", StudentCount = conlai });
            return Json(new { rs = true, data = final }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Lấy số lượng học sinh theo quận
        /// </summary>
        /// <param name="schoolID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LoadReportExam(int schoolID)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            //SqlParameter[] parameters =
            //{
            //        new SqlParameter("@qsearch", SqlDbType.NVarChar,2000) { Value = "" },
            //        new SqlParameter("@fromdate", SqlDbType.NVarChar,2000) { Value = null },
            //        new SqlParameter("@todate", SqlDbType.NVarChar,2000) { Value = null },
            //        new SqlParameter("@roleID", SqlDbType.Int) { Value = 0 },
            //        new SqlParameter("@aclogID", SqlDbType.Int) { Value = 0 },
            //};
            //var rs = SqlHelper.ExecuteDataTable("Get_Student_ReadyTo_Test", parameters);
            //List<TestResults> l = (from rw in rs.AsEnumerable()
            //                       select new TestResults()
            //                       {
            //                           ID = Convert.ToInt32(rw["ID"]),
            //                           //FullName = rw["FullName"].ToString(),
            //                           //DateExam = Convert.ToDateTime(rw["DateExam"]),
            //                           //L = Convert.ToDouble(rw["L"]),
            //                           //S = Convert.ToDouble(rw["S"]),
            //                           //R = Convert.ToDouble(rw["R"]),
            //                           //W = Convert.ToDouble(rw["W"]),
            //                           //OverAll = Convert.ToDouble(rw["OverAll"]),
            //                           //SubTotal = Convert.ToDouble(rw["SubTotal"]),
            //                           //CamKet = Convert.ToDouble(rw["CamKet"]),
            //                           //Lech = Convert.ToDouble(rw["Lech"]),

            //                           ColorID = Convert.ToInt32(rw["ColorID"]),
            //                           Color = rw["Color"].ToString()
            //                       }).ToList();

            List<DistrictStudent> final = new List<DistrictStudent>();
            //final.Add(new DistrictStudent() { DistrictID = 0, DistrictName = "Đạt", StudentCount = l.Where(n => n.ColorID == 2).ToList().Count() });
            //final.Add(new DistrictStudent() { DistrictID = 0, DistrictName = "Vượt", StudentCount = l.Where(n => n.ColorID == 1).ToList().Count() });
            //final.Add(new DistrictStudent() { DistrictID = 0, DistrictName = "Không đạt", StudentCount = l.Where(n => n.ColorID == 3).ToList().Count() });
            //final.Add(new DistrictStudent() { DistrictID = 0, DistrictName = "D1", StudentCount = l.Where(n => n.ColorID == 4).ToList().Count() });
            //final.Add(new DistrictStudent() { DistrictID = 0, DistrictName = "D2", StudentCount = l.Where(n => n.ColorID == 5).ToList().Count() });

            return Json(new { rs = true, data = final }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult DashboardSaler(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

            int pageSize = 20;
            IEnumerable<tbl_CampaignSale> l = CampaignSaleTable.getall().Where(n => n.eTime > aclog.CreatedDate).OrderByDescending(n => n.sTime).ToList();

            // thời gian bắt đầu từ
            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                try
                {
                    DateTime fromdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                    l = from ex in l
                        where ex.sTime.Value.Date >= fromdate.Date
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
                        where ex.eTime.Value.Date <= todate.Date
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
                    l = l.OrderBy(n => n.sTime).ToList();
                if (sort == 2)
                    l = l.OrderByDescending(n => n.sTime).ToList();
                if (sort == 3)
                    l = l.OrderBy(n => n.IsHide).ToList();
                if (sort == 4)
                    l = l.OrderByDescending(n => n.IsHide).ToList();
            }
            ViewBag.Sort = sort;
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }

        public ActionResult PartialMenu()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            return View();
        }
        [ChildActionOnly]
        public ActionResult PartialMenuTwo()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            return View();
        }

        public ActionResult PartialMobileMenu()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            return View();
        }

        public ActionResult ErrorNotFound()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LoadMember(int teamID)
        {
            var listMember = AccountTable.getbyrole(6).Where(x => x.LeaderID == teamID).ToList();
            StringBuilder html = new StringBuilder();
            html.Append("<option value=\"0\">---</option>");
            foreach (var i in listMember)
            {
                html.Append("<option value=" + i.ID + ">" + i.FullName + "</option>");
            }
            return Json(new { rs = true, data = html.ToString() }, JsonRequestBehavior.AllowGet);
        }
    }
}