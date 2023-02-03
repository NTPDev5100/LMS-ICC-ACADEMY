using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using static AppZim.Areas.Admin.Controllers.CourseController;

namespace AppZim.Areas.Admin.Controllers
{
    public class RegisterCourseAndClass
    {
        public int ID { get; set; }
        public int Price { get; set; }
        public int TypeID { get; set; }
    }
    public class ExamsService
    {
        public int ID { get; set; }
        public int ExamPrice { get; set; }
        public string ExamService { get; set; }
        public bool ExamReal { get; set; }
        public string DiscountCode { get; set; }
        public bool DiscountType { get; set; }
        public int DiscountValue { get; set; }
    }
    public class CashierController : BaseController
    {
        [HttpPost]
        public ActionResult GetExams(int serviceID)
        {
            List<ExamsService> ex = new List<ExamsService>();
            List<tbl_ServiceExams> lexam = ServiceExamsTable.getall().Where(n => n.isHide != true).OrderByDescending(n => n.ID).ToList();
            if (serviceID != 0)
            {
                lexam = lexam.Where(n => n.ServiceID == serviceID).ToList();
            }
            if (lexam.Count() > 0)
            {
                foreach (var i in lexam)
                {
                    ExamsService e = new ExamsService();
                    e.ID = i.ID;
                    e.ExamPrice = i.ExamPrice.Value;
                    e.ExamService = i.ExamService;
                    e.ExamReal = i.ExamReal.Value;
                    var d = DiscountTable.getbyid(i.DiscountID.Value);
                    if (d != null)
                    {
                        if (d.Status == 1)
                        {
                            e.DiscountCode = d.DiscountCode;
                            e.DiscountType = d.DiscountType.Value;
                            if (d.DiscountType.Value == true) //true:phần trăm
                            {
                                e.DiscountValue = d.DiscountPercent.Value;
                            }
                            else
                            {
                                e.DiscountValue = d.DiscountPrice.Value;
                            }
                        }
                    }
                    ex.Add(e);
                }
            }
            return Json(new { rs = true, l = ex }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AssignCourceNew(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                #region user data
                tbl_District district = new tbl_District() { ID = 0, DistrictName = "", LocationID = 0, LocationName = "" };
                if (!string.IsNullOrEmpty(f.Get("ddl-district")))
                {
                    district = DistrictTable.getbyID(f.Get("ddl-district").ToInt(0));
                }

                if (district == null)
                {
                    district = new tbl_District();
                    district.ID = 0;
                    district.DistrictName = "";
                    district.LocationID = 0;
                    district.LocationName = "";
                }

                string fullname = "";
                if (!string.IsNullOrEmpty(f.Get("txt-full-name")))
                {
                    fullname = f.Get("txt-full-name").ToString();
                }

                string email = "";
                if (!string.IsNullOrEmpty(f.Get("txt-email")))
                {
                    email = f.Get("txt-email").ToString();
                }

                string phone = "";
                if (!string.IsNullOrEmpty(f.Get("txt-phone")))
                {
                    phone = f.Get("txt-phone").ToString();
                }

                if (string.IsNullOrEmpty(email))
                {
                    SetAlert("Vui lòng nhập email học viên", "e");
                    return RedirectToAction("RegisterCourseCustom", "Cashier");
                }
                var birthday = DateTime.Now.ToString("dd/MM/yyyy");
                if (!string.IsNullOrEmpty(f.Get("txt-birth-day")))
                {
                    birthday = f.Get("txt-birth-day").ToString().Replace(" ", "");
                }

                string identity = "";
                var identitycity = new tbl_City() { ID = 0, CityName = "" };
                string identitydate = "";

                var jobs = new tbl_Job() { ID = 0, JobName = "" };
                if (!string.IsNullOrEmpty(f.Get("ddl-job")))
                {
                    jobs = JobTable.getbyid(f.Get("ddl-job").ToInt(1));
                    if (jobs == null)
                    {
                        jobs = new tbl_Job() { ID = 0, JobName = "" };
                    }
                }

                string workplace = "";
                if (!string.IsNullOrEmpty(f.Get("txt-work-place")))
                {
                    workplace = f.Get("txt-work-place").ToString();
                }

                string notehome = "";
                if (!string.IsNullOrEmpty(f.Get("txt-note-home")))
                {
                    notehome = f.Get("txt-note-home").ToString();
                }

                var pur = new tbl_AcademicPurposes() { ID = 0, AcademicPurposesName = "" };
                if (!string.IsNullOrEmpty(f.Get("ddl-academic-purpose")))
                {
                    pur = AcademicPurposesTable.getbyID(f.Get("ddl-academic-purpose").ToInt(0));
                    if (pur == null)
                    {
                        pur = new tbl_AcademicPurposes();
                        pur.ID = 0;
                        pur.AcademicPurposesName = "";
                    }
                }

                string scorein = "";
                if (!string.IsNullOrEmpty(f.Get("in-point-hope")))
                {
                    scorein = f.Get("in-point-hope").ToString();
                }

                string scoreout = "";
                if (!string.IsNullOrEmpty(f.Get("out-point-hope")))
                {
                    scoreout = f.Get("out-point-hope").ToString();
                }

                string examdate = "";
                //try
                //{
                //    examdate = DateTime.ParseExact(f.Get("txt-exam").ToString().Replace(" ", ""), "dd/MM/yyyy", null).ToString("dd/MM/yyyy");
                //}
                //catch { }

                string ngaythuno = "";
                try
                {
                    ngaythuno = DateTime.ParseExact(f.Get("txt-thu-no").ToString().Replace(" ", ""), "dd/MM/yyyy", null).ToString("dd/MM/yyyy");
                }
                catch { ngaythuno = DateTime.Now.ToString("dd/MM/yyyy"); }

                tbl_Ward wa = new tbl_Ward() { ID = 0, WardName = "" };
                if (!string.IsNullOrEmpty(f.Get("ddl-ward")))
                {
                    wa = WardTable.getbyid(f.Get("ddl-ward").ToInt(0));
                    if (wa == null)
                    {
                        wa = new tbl_Ward();
                        wa.ID = 0;
                        wa.WardName = "";
                    }
                }

                tbl_Street st = new tbl_Street() { ID = 0, StreetName = "" };
                if (!string.IsNullOrEmpty(f.Get("ddl-street")))
                {
                    st = StreetTable.getbyid(f.Get("ddl-street").ToInt(0));
                    if (st == null)
                    {
                        st = new tbl_Street();
                        st.ID = 0;
                        st.StreetName = "";
                    }
                }

                string homenumber = "";
                int language = 2;
                if (!string.IsNullOrEmpty(f.Get("slLanguage")))
                {
                    language = f.Get("slLanguage").ToInt(2);
                }

                var ac = AccountTable.getbyusername(email);
                var accappometn = new tbl_Account();
                int spid = aclog.ID;
                using (var _db = new ZimEntities())
                {
                    if (aclog.RoleID == 1)
                    {
                        accappometn = _db.tbl_Account.Where(x => x.RoleID == 6 && x.Status == 2).OrderByDescending(x => Guid.NewGuid()).FirstOrDefault();
                    }
                    //else if (aclog.RoleID == 2)
                    //{
                    //    var leaderTemp = _db.tbl_Account.Where(x => x.RoleID == 3 && x.Status == 2 && x.LeaderID == aclog.ID).OrderByDescending(x => Guid.NewGuid()).FirstOrDefault();
                    //    accappometn = _db.tbl_Account.Where(x => x.RoleID == 6 && x.Status == 2 && x.LeaderID == leaderTemp.ID).OrderByDescending(x => Guid.NewGuid()).FirstOrDefault();
                    //}
                    //else if (aclog.RoleID == 3)
                    //{
                    //    accappometn = _db.tbl_Account.Where(x => x.RoleID == 6 && x.Status == 2 && x.LeaderID == aclog.ID).OrderByDescending(x => Guid.NewGuid()).FirstOrDefault();
                    //}

                    if (aclog.RoleID == 2 || aclog.RoleID == 3 || aclog.RoleID == 6 || accappometn == null)
                    {
                        accappometn = aclog;
                    }
                }
                spid = accappometn.ID;

                var sc = SourceOfCustomerTable.getall().OrderBy(n => n.ID).FirstOrDefault();
                if (aclog.RoleID == 6)//saler
                {
                    sc.ID = 0;
                    sc.SourceOfCustomer = "TVV";
                }

                if (ac != null)
                {
                    sc.ID = ac.SourceID == null ? 0 : ac.SourceID.Value;
                    sc.SourceOfCustomer = ac.SourceName;
                }
                else
                {
                    sc.ID = 0;
                    sc.SourceOfCustomer = "TVV";
                }

                if (ac == null)// tạo mới tài khoản
                {
                    ac = AccountTable.insertcustomer(district, fullname, email, phone, "", 2, "", spid, sc.ID, sc.SourceOfCustomer, birthday, jobs.ID, jobs.JobName,
                        workplace, pur.ID, pur.AcademicPurposesName, scorein, scoreout, examdate, "", identity, identitycity.ID, identitycity.CityName, identitydate, notehome, aclog.UserName, false, wa, st, homenumber, language, "", 0);
                }
                else// cập nhật
                {
                    if (ac.RoleID != 5)
                    {
                        SetAlert("Tài khoản này đã có trong hệ thống với chức vụ " + ac.RoleName, "e");
                        return RedirectToAction("ListAppointmentTest", "Cashier");
                    }
                    ac = AccountTable.updatecustomer(ac.ID, district, fullname, email, phone, ac.Address, 2, "", ac.SupportUID.Value, sc.ID, sc.SourceOfCustomer, birthday, jobs.ID, jobs.JobName,
                        workplace, pur.ID, pur.AcademicPurposesName, scorein, scoreout, examdate, "", identity, identitycity.ID, identitycity.CityName, identitydate, notehome, aclog.UserName, wa, st, homenumber, language, ac.LinkFB, ac.NationID.Value);
                }
                #endregion
                //Danh sách chi tiết invoice
                List<RegisterCourseAndClass> listRCC = new List<RegisterCourseAndClass>();
                string camket = "";
                if (!string.IsNullOrEmpty(f.Get("txt-cam-ket")))
                {
                    camket = f.Get("txt-cam-ket").ToString();
                }

                string ghichu = "";
                if (!string.IsNullOrEmpty(f.Get("txt-ghi-chu")))
                {
                    ghichu = f.Get("txt-ghi-chu").ToString();
                }

                var type = 1;
                if (!string.IsNullOrEmpty(f.Get("hdfRegister")))
                {
                    type = f.Get("hdfRegister").ToInt(1);
                }

                if (type == 1)// hẹn đăng ký
                {
                    tbl_School choolinvoice = null;
                    if (!string.IsNullOrEmpty(f.Get("ddl-payment-school")))
                    {
                        choolinvoice = SchoolTable.getbyID(f.Get("ddl-payment-school").ToInt(0));
                    }
                    if (choolinvoice == null)
                    {
                        SetAlert("Không tìm thấy trung tâm tạo phiếu", "e");
                        return RedirectToAction("RegisterCourseCustom", "Cashier");
                    }

                    var paymentmethod = 1;
                    if (!string.IsNullOrEmpty(f.Get("ddl-payment-register")))
                    {
                        paymentmethod = f.Get("ddl-payment-register").ToInt(1);
                    }
                    string payname = Paymentmethod(paymentmethod);

                    var paymentreceive = 0;
                    if (!string.IsNullOrEmpty(f.Get("ddl-payment-receive")))
                    {
                        paymentreceive = f.Get("ddl-payment-receive").ToInt(0);
                    }

                    //var lop = new tbl_Class() { ID = 0, ClassName = "", PriceClass = 0 };
                    //if (!string.IsNullOrEmpty(f.Get("ddl-class-course")))
                    //{
                    //    lop = ClassTable.getbyID(f.Get("ddl-class-course").ToInt(0));
                    //    if (lop == null)
                    //    {
                    //        SetAlert("Không tìm thấy lớp", "e");
                    //        return RedirectToAction("RegisterCourseCustom", "Cashier");
                    //    }
                    //}
                    int[] lops = null;
                    if (!string.IsNullOrEmpty(f.Get("ddl-class-course")))
                    {
                        lops = f.Get("ddl-class-course").ToString().Split(',').Select(x => Convert.ToInt32(x)).ToArray();
                    }

                    if (lops == null || lops.Count() == 0)
                    {
                        SetAlert("Vui lòng chọn chương trình học", "e");
                        return RedirectToAction("RegisterCourseCustom", "Cashier");
                    }

                    var commitment = new tbl_Class() { ID = 0, ClassName = "", PriceClass = 0 };
                    if (!string.IsNullOrEmpty(f.Get("commitmentpackage")))
                    {
                        commitment = ClassTable.getbyID(f.Get("commitmentpackage").ToInt(0));
                        if (commitment == null)
                        {
                            SetAlert("Không tìm thấy gói cam kết", "e");
                            return RedirectToAction("RegisterCourseCustom", "Cashier");
                        }
                    }

                    var chool = new tbl_School() { ID = 0, SchoolName = "" };
                    if (!string.IsNullOrEmpty(f.Get("ddl-school-combo")))
                    {
                        chool = SchoolTable.getbyID(f.Get("ddl-school-combo").ToInt(0));
                        if (chool == null)
                        {
                            chool = new tbl_School();
                            chool.ID = 0;
                            chool.SchoolName = "";
                        }
                    }

                    var ca = new tbl_StudyTime() { ID = 0, StudyTimeName = "" };
                    if (!string.IsNullOrEmpty(f.Get("ddl-study-combo")))
                    {
                        ca = StudyTimeTable.getbyid(f.Get("ddl-study-combo").ToInt(0));
                        if (ca == null)
                        {
                            ca = new tbl_StudyTime();
                            ca.ID = 0;
                            ca.StudyTimeName = "";
                        }
                    }

                    int rewardTeachers = 0;
                    if (!string.IsNullOrEmpty(f.Get("txt-rewardteachers")))
                    {
                        rewardTeachers = f.Get("txt-rewardteachers").ToString().Replace(",", "").ToInt(0);
                    }

                    //int tygia = 0;
                    //if (!string.IsNullOrEmpty(f.Get("MoneyExchangeRate")))
                    //{
                    //    tygia = f.Get("MoneyExchangeRate").ToString().Replace(",", "").ToInt(0);
                    //}

                    int coefficient = 0;
                    if (!string.IsNullOrEmpty(f.Get("coefficient")))
                    {
                        coefficient = f.Get("coefficient").ToInt();
                    }

                    string note = "";
                    if (!string.IsNullOrEmpty(f.Get("txt-ghi-chu")))
                    {
                        note = f.Get("txt-ghi-chu").ToString();
                    }

                    int lesson = 0;
                    if (!string.IsNullOrEmpty(f.Get("txt-lesson")))
                    {
                        lesson = f.Get("txt-lesson").ToInt(0);
                    }

                    int customPrice = 0;
                    if (!string.IsNullOrEmpty(f.Get("totalcourse")))
                    {
                        customPrice = f.Get("totalcourse").ToString().Replace(",", "").ToInt(0);
                    }
                    //if (customPrice <= 0)
                    //{
                    //    SetAlert("Tổng tiền không hợp lệ", "e");
                    //    return RedirectToAction("RegisterCourseCustom", "Cashier");
                    //}

                    string datedeposit = "";
                    try
                    {
                        datedeposit = ngaythuno;
                    }
                    catch { datedeposit = ""; }

                    var discount = new tbl_Discount() { ID = 0, DiscountCode = "", DiscountPrice = 0, Status = 1, QuantityLeft = 1 };
                    if (!string.IsNullOrEmpty(f.Get("txt-code")))
                    {
                        discount = DiscountTable.getbycode(f.Get("txt-code"));
                        if (discount == null)
                            discount = new tbl_Discount() { ID = 0, DiscountCode = "", DiscountPrice = 0, Status = 1, QuantityLeft = 1 };
                    }

                    if (discount.Status != 1 && discount.QuantityLeft <= 0)
                        discount = new tbl_Discount() { DiscountPrice = 0, ID = 0 };
                    tbl_StudentAppointment rs = null;
                    int inbill = 2;
                    if (lops.Count() == 1)
                    {
                        var lop = ClassTable.getbyID(lops[0]);
                        if (customPrice < (lop.PriceClass + commitment.PriceClass) || customPrice > (lop.PriceClassInitial + commitment.PriceClass))
                        {
                            SetAlert("Tổng tiền phải nằm trong khoảng Giá Min + gói cam kết => Giá Max + gói cam kết", "e");
                            return RedirectToAction("RegisterCourseCustom", "Cashier");
                        }
                        rs = StudentAppointmentTable.insert(ac, district, chool, lop, ca, rewardTeachers, customPrice,
                    note, paymentmethod, payname, aclog.UserName, false, 0, "", datedeposit, commitment,
                    discount.ID, discount.DiscountPrice, paymentreceive, coefficient, "", customPrice, lesson);
                    }
                    //đăng ký combo
                    else
                    {
                        double totalMin = 0;
                        double totalMax = 0;
                        try
                        {
                            foreach (var l in lops)
                            {
                                var tempClass = ClassTable.getbyID(l);
                                totalMin += Convert.ToDouble(tempClass.PriceClass);
                                totalMax += Convert.ToDouble(tempClass.PriceClassInitial);
                            }
                        }
                        catch { totalMin = 0; totalMax = 0; }

                        if (customPrice < (totalMin + commitment.PriceClass) || customPrice > (totalMax + commitment.PriceClass))
                        {
                            SetAlert("Tổng tiền phải nằm trong khoảng Giá Min + gói cam kết => Giá Max + gói cam kết", "e");
                            return RedirectToAction("RegisterCourseCustom", "Cashier");
                        }
                        var combo = ComboTable.insert(ac.ID, customPrice, discount.ID, discount.DiscountPrice ?? 0, customPrice, "", aclog.UserName);

                        rs = StudentAppointmentTable.insertwithCombo(ac, district, chool, new tbl_Class() { ID = 0, ClassName = "", PriceClass = 0 }, ca,
                           customPrice, note, paymentmethod, payname, aclog.UserName, false, 0, "", datedeposit, combo.ID, lesson, commitment, discount, rewardTeachers, customPrice, paymentreceive, coefficient);
                        foreach (var l in lops)
                        {
                            ComboDetailTable.insert(combo.ID, l, aclog.UserName);
                        }
                        inbill = 3;
                    }

                    if (rs != null)
                    {
                        DiscountTable.useupdate(discount.ID, aclog.UserName);

                        string ghichuphieuthu = "";
                        if (!string.IsNullOrEmpty(f.Get("txt-ghi-chu")))
                        {
                            ghichuphieuthu = f.Get("txt-ghi-chu").ToString();
                        }

                        string gcpt = "";
                        if (!string.IsNullOrEmpty(ghichuphieuthu))
                            gcpt += " [Ghi chú: " + ghichuphieuthu + "]";

                        try
                        {
                            //ngày - ca học
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

                            for (int i = 0; i < dayofweekIDList.Count(); i++)
                            {
                                var insertDesiredStudyTime = new tbl_DesiredStudyTime
                                {
                                    StudentAppointmentID = rs.ID,
                                    DayOfWeekID = int.Parse(dayofweekIDList[i]),
                                    StudyTimeID = int.Parse(studyingTimeIDList[i])
                                };
                                DesiredStudyTimeTable.insert(insertDesiredStudyTime, aclog.FullName);
                            }
                        }
                        catch (Exception e)
                        {
                            AssetCRM.Writelog("insert ngày học lỗi", "Cashier", 0, e.Message + "||" + e.InnerException);
                        }

                        //Hóa đơn 
                        if (customPrice > 0)
                        {
                            try
                            {
                                // cập nhật doanh thu cho chiến dịch
                                Thread tUpdateCampaignSale = new Thread(() => UpdateCampaignSale(ac, customPrice, rs.ID));
                                tUpdateCampaignSale.Start();


                                // đếm lại doanh thu cho tvv để gửi yêu cầu lên rank
                                Thread tuplevel = new Thread(() => UpLevel(ac));
                                tuplevel.Start();
                            }
                            catch (Exception e)
                            {
                                AssetCRM.Writelog("lỗi hàm AssignCourceNew chỗ cập nhật doanh thu rồi Hùng ơi", "CashierController", 0,
                                e.Message + "||" + e.InnerException);
                            }

                            try
                            {
                                string name = "1-" + rs.ID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";// 1 hẹn đăng ký
                                string nameencode = Encryptor.EncryptURL(name);
                                string strPathAndQuery = Request.Url.PathAndQuery;
                                string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                                string datalink = "" + strUrl + "Admin/Cashier/Invoice/?code=" + nameencode;
                                tbl_Invoices iv = InBill(rewardTeachers, inbill, rs.ID, ac, customPrice, gcpt, "", 0,
                                    ngaythuno, choolinvoice, aclog, paymentmethod, payname, name, nameencode, datalink);
                                if (iv != null)
                                {
                                    string tt = f.Get("hdtt").ToString();
                                    string ct = f.Get("hdct").ToString();
                                    ct = ct.Replace("[link-hoa-don]", datalink);
                                    SetlinkInvoice(datalink);
                                    if (!string.IsNullOrEmpty(ac.Email))
                                        AssetCRM.SendMail(ac.Email, tt, ct);
                                }
                                else
                                {
                                    SetAlert("Không tạo được phiếu thu", "e");
                                    return RedirectToAction("RegisterCourseCustom", "Cashier");
                                }
                            }
                            catch (Exception e)
                            {
                                AssetCRM.Writelog("lỗi hàm AssignCourceNew chỗ tạo phiếu thu rồi Hùng ơi", "CashierController", 0,
                                e.Message + "||" + e.InnerException);
                            }
                        }
                        try
                        {
                            var stu = DeviceMobileTable.getbyUID(ac.ID)?.IncludePlayerID;
                            PushNotifyMobile.SendNotifyMobile("Bạn vừa đăng ký thành công 1 lớp học!", new[] { stu });
                        }
                        catch { }
                        SetAlert("Thành công", "s");
                    }
                    else
                        SetAlert("Không thành công", "e");
                }
                #region mua dịch vụ
                //else if (type == 2)// mua dịch vụ
                //{
                //    var choolinvoice = SchoolTable.getbyID(f.Get("ddl-school-invoice-service").ToInt(0));
                //    if (choolinvoice == null)
                //    {
                //        SetAlert("Không tìm thấy trung tâm tạo phiếu", "e");
                //        return RedirectToAction("RegisterCourseCustom", "Cashier");
                //    }

                //    var paymentmethod = f.Get("ddl-payment-service").ToInt(1);
                //    int rewardTeachers = f.Get("txt-rewardteachers").ToString().Replace(",", "").ToInt(0);
                //    string payname = Paymentmethod(paymentmethod);

                //    var sv = ServicesTable.getbyid(f.Get("ddl-service").ToInt(0));
                //    if (sv == null)
                //    {
                //        SetAlert("Không tìm thấy dịch vụ", "e");
                //        return RedirectToAction("RegisterCourseCustom", "Cashier");
                //    }

                //    var exam = ServiceExamsTable.getbyID(f.Get("ddl-exams").ToInt(0));
                //    if (exam == null)
                //    {
                //        exam = new tbl_ServiceExams();
                //        exam.ID = 0;
                //        exam.ExamService = "";
                //    }

                //    //int money = f.Get("txt-price-service").ToString().Replace(",", "").ToInt(0);
                //    int money = f.Get("txt-price-thanhtoan-service").ToString().Replace(",", "").ToInt(0);
                //    string note = f.Get("txt-note-service").ToString();
                //    string codeService = f.Get("txt-code-service").ToString();
                //    var dis = DiscountTable.getbycode(codeService);
                //    int DiscountID = 0;
                //    if (!string.IsNullOrEmpty(codeService))
                //    {
                //        DiscountID = dis.ID;
                //        DiscountTable.useupdate(dis.ID, aclog.UserName);
                //    }

                //    string dateservice = f.Get("txt-date-service").ToString().Replace(" ", "");

                //    var sup = SupplierServicesTable.getbyID(f.Get("ddl-supp").ToString().ToInt(0));
                //    if (sup == null)
                //    {
                //        sup = new tbl_SupplierServices();
                //        sup.ID = 0;
                //        sup.SupplierName = "";
                //    }

                //    var rs = ServicesStudentTable.insert(ac.ID, ac.FullName, sv, money, note, paymentmethod, payname, sup, dateservice, aclog.UserName, exam.ID, DiscountID);
                //    if (rs != null)
                //    {
                //        string gcpt = "";
                //        if (!string.IsNullOrEmpty(note))
                //            gcpt += " [Ghi chú: " + note + "]";
                //        //Hóa đơn
                //        if (money > 0)
                //        {
                //            string name = "2-" + rs.ID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";//2 mua dịch vụ
                //            string nameencode = AppZim.ZIM.Encryptor.EncryptURL(name);
                //            string strPathAndQuery = Request.Url.PathAndQuery;
                //            string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                //            string datalink = "" + strUrl + "Admin/Cashier/Invoice/?code=" + nameencode;
                //            tbl_Invoices iv = InBill(rewardTeachers, 4, rs.ID, ac, money, sv.ServiceName + " " + exam.ExamService + gcpt, "", 0, "", choolinvoice, aclog, paymentmethod, payname, name, nameencode, datalink);
                //            if (iv != null)
                //                SetlinkInvoice(datalink);
                //            else
                //            {
                //                SetAlert("Không thành công", "e");
                //                return RedirectToAction("RegisterCourseCustom", "Cashier");
                //            }
                //        }
                //        SetAlert("Thành công", "s");
                //    }
                //    else
                //        SetAlert("Không thành công", "e");
                //}
                #endregion
                return RedirectToAction("RegisterCourseCustom", "Cashier");
            }
            catch (Exception e)
            {
                SetAlert("Đã xảy ra lỗi, vui lòng quay lại video thao tác và gửi admin", "e");
                AssetCRM.Writelog("lỗi hàm AssignCourceNew rồi Hùng ơi", "CashierController", 0,
                    e.Message + "||" + e.InnerException + "(" + f.Get("txt-email") + ","
                    + "," + f.Get("txt-full-name") + "," + f.Get("txt-phone") + "," + f.Get("txt-birth-day")
                    + "," + f.Get("ddl-job") + "," + f.Get("txt-work-place") + "," + f.Get("txt-note-home")
                    + "," + f.Get("ddl-academic-purpose") + "," + f.Get("in-point-hope") + "," + f.Get("out-point-hope")
                    + "," + f.Get("txt-thu-no") + "," + "," + f.Get("slLanguage") + "," + f.Get("want-to-day")
                    + "," + f.Get("txt-cam-ket") + "," + f.Get("txt-ghi-chu") + "," + f.Get("hdfRegister")
                    + "," + f.Get("ddl-payment-school") + "," + f.Get("ddl-payment-register") + "," + f.Get("ddl-payment-receive")
                    + "," + f.Get("ddl-class-course") + "," + f.Get("commitmentpackage") + "," + f.Get("ddl-school-combo")
                    + "," + f.Get("ddl-study-combo") + "," + f.Get("txt-payedmoney") + "," + f.Get("txt-rewardteachers")
                    + "," + f.Get("MoneyExchangeRate") + "," + f.Get("txt-ghi-chu") + "," + f.Get("totalcourse")
                    + "," + f.Get("txt-thu-no") + "," + f.Get("txt-code") + "," + f.Get("want-to-day")
                    + "," + f.Get("ddl-study-combo") + "," + ")");
                return RedirectToAction("RegisterCourseCustom", "Cashier");
            }
        }
        // đăng ký học
        public ActionResult RegisterCourse()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            if (TempData["StudentUID"] != null)
            {
                var StudentUID = Convert.ToInt32(TempData["StudentUID"]);
                using (var db = new ZimEntities())
                {
                    ViewBag.Phone = db.tbl_Account.Where(x => x.ID == StudentUID).FirstOrDefault().UserName;
                }
            }
            else
            {
                ViewBag.Phone = "";
            }
            return View();
        }
        // đăng ký học tùy chỉnh theo IEC
        public ActionResult RegisterCourseCustom(string studentEmail = "")
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            if (TempData["StudentUID"] != null)
            {
                var StudentUID = Convert.ToInt32(TempData["StudentUID"]);
                using (var db = new ZimEntities())
                {
                    ViewBag.Phone = db.tbl_Account.Where(x => x.ID == StudentUID).FirstOrDefault().UserName;
                }
            }
            else
            {
                ViewBag.Phone = "";
            }
            ViewBag.StudentEmail = studentEmail;
            return View();
        }
        /// <summary>
        ///lấy danh sách ca học
        /// </summary>
        /// <returns></returns>
        public JsonResult getStudyingTime()
        {
            var studyTime = StudyTimeTable.getall().OrderBy(x => x.sTime).ToList();
            return Json(studyTime);
        }

        // hẹn đăng ký
        public ActionResult SaveinfoStudent()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            return View();
        }

        // danh sách học viên hẹn test
        public ActionResult ListAppointmentTest(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
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
            int source = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["source"]))
            {
                source = ViewBag.SourceID = Request.QueryString["source"].ToInt(0);
            }
            int support = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["support"]))
            {
                support = ViewBag.SupportID = Request.QueryString["support"].ToInt(0);
            }
            int status = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["status"]))
            {
                status = ViewBag.StatusID = Request.QueryString["status"].ToInt(0);
            }
            int language = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slLanguage"]))
            {
                language = ViewBag.Language = Request.QueryString["slLanguage"].ToInt(0);
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
            string todate = null;
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
            if (fromdate == null && todate == null)
            {
                fromdate = ViewBag.From = DateTime.Now.FirstDayOfMonth().ToString("dd/MM/yyyy");
                todate = ViewBag.To = DateTime.Now.LastDayOfMonth().ToString("dd/MM/yyyy");
            }
            //status-register
            int statusRegister = 0;
            int pageSize = 20;
            List<get_list_of_AppointmentTest_Result> l = new List<get_list_of_AppointmentTest_Result>();
            using (var db = new ZimEntities())
            {
                l = db.get_list_of_AppointmentTest(search, source, center, support, status, fromdate, todate,
                    sort, pageSize, page - 1, statusRegister, aclog.RoleID, aclog.ID, language).ToList();
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

        // danh sách học viên hẹn test
        public ActionResult AppointmentDetailTest(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            string url = HttpContext.Request.UrlReferrer.AbsoluteUri;
            ViewBag.PreUrl = url;
            var l = StudentAppointmentTestTable.getbyid(id);
            if (l == null)
            {
                SetAlert("Không tìm thấy buổi test", "e");
                return RedirectToAction("ListAppointmentTest", "Cashier");
            }
            ViewBag.AcademicID = l.AcademicID;
            ViewBag.TeacherID = l.TeacherID;
            return View(l);
        }

        [HttpPost]
        public ActionResult LoadCourseBySchoolID(int SchoolID)
        {
            StringBuilder m = new StringBuilder();
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var d = CoursesTable.getall().Where(n => n.Status != 3 && n.CourseSelfStudy != true).ToList();
            if (SchoolID != 0)
                d = d.Where(n => n.SchoolID == SchoolID).ToList();
            if (d.Count == 0)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            else
            {
                m.Append("<option data-price=\"0\" value=\"0\">---Khóa học---</option>");
                foreach (var item in d)
                {
                    var CatchBitch = CoursesStudentTable.getListStudentInCourse(item.ID).Count;
                    if (CatchBitch < 18)
                        m.Append("<option data-price=\"" + String.Format("{0:0,0}", item.Price) + "\" value=\"" + item.ID + "\">" + item.CourseName + "</option>");
                }
                return Json(new { rs = true, l = m.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        //tinh số tiền nếu chọn combo khóa
        [HttpPost]// check mã khuyến mãi
        public ActionResult getpricecourse(string courseid)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            string[] arc = courseid.Split(',');
            int p = 0;
            for (int i = 0; i < arc.Length; i++)
            {
                var c = CoursesTable.getbyid(arc[i].ToInt(0));
                if (c == null)
                    continue;
                p += c.Price.Value;
            }
            return Json(new { p = String.Format("{0:0,0}", p) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost] //Tính tiền lớp
        public ActionResult GetTotalPrice(string classid, string courseid, int commitment)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            string[] arc = classid.Split(',');
            int p = 0;
            for (int i = 0; i < arc.Length; i++)
            {
                var c = ClassTable.getbyID(arc[i].ToInt(0));
                if (c == null)
                    continue;
                p += c.PriceClass.Value;
            }
            string[] arcourse = courseid.Split(',');
            for (int i = 0; i < arcourse.Length; i++)
            {
                var c = CoursesTable.getbyid(arcourse[i].ToInt(0));
                if (c == null)
                    continue;
                p += c.Price.Value;
            }
            var package = ClassTable.getbyID(commitment);
            p += package.PriceClass.Value;

            return Json(new { p = String.Format("{0:0,0}", p) }, JsonRequestBehavior.AllowGet);
        }

        public static string Paymentmethod(int p)
        {
            //if (p == 1)
            //    return "Tiền mặt";
            //if (p == 2)
            //    return "Thanh toán online";
            //if (p == 3)
            //    return "Khác";
            //if (p == 4)
            //    return "Quẹt thẻ";
            //if (p == 5)
            //    return "Chuyển khoản";
            //if (p == 6)
            //    return "Giáo viên nhận hộ";
            //return "Khác";
            if (p == 1)
                return "Tiền mặt";
            if (p == 5)
                return "Chuyển khoản";
            return "Khác";
        }

        #region lịch học của trung tâm
        public ActionResult ScheduleSchool()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5 || aclog.RoleID == 4)// khác lễ tân bị đá bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            ViewBag.m = GetDateTime.Now.ToString("dd/MM/yyyy");
            string shide = "hide";
            StringBuilder s = new StringBuilder();
            //if (aclog.RoleID == 8)
            //{
            //    shide = "";
            //    var l = AssignManagerSpecializeToSchoolTable.getbyuid(aclog.ID);
            //    foreach (var item in l)
            //    {
            //        var school = SchoolTable.getbyID(item.SchoolID.Value);
            //        if (school != null)
            //            s.Append("<option value=\"" + item.SchoolID.Value + "\">" + school.SchoolName + "</option>");
            //    }
            //}
            //else
            //{
            //    var school = SchoolTable.getbyID(aclog.SchoolID.Value);
            //    if (school != null)
            //        s.Append("<option value=\"" + school.ID + "\">" + school.SchoolName + "</option>");
            //}


            if (aclog.RoleID == 1 || aclog.RoleID == 7)
            {
                using (var db = new ZimEntities())
                {
                    var l = db.tbl_School.Where(x => x.isHide == false).Select(i => new { ID = i.ID, SchoolName = i.SchoolName }).ToList();
                    if (l.Count() != 0)
                    {
                        foreach (var item in l)
                        {
                            s.Append("<option value=\"" + item.ID + "\">" + item.SchoolName + "</option>");
                        }
                        shide = "";
                    }
                }
            }
            else
            {
                var school = SchoolTable.getbyID(aclog.SchoolID.Value);
                if (school != null)
                    s.Append("<option value=\"" + school.ID + "\">" + school.SchoolName + "</option>");
            }

            ViewBag.Hide = shide;
            ViewBag.School = s.ToString();
            return View();
        }
        [HttpPost]
        public ActionResult LoadScheduleSchool(string month, int type, int schoolid)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            DateTime date = DateTime.ParseExact(month, "dd/MM/yyyy", null);
            if (type == 1)// trừ
                date = date.AddMonths(-1);
            if (type == 2)// cộng
                date = date.AddMonths(1);

            //var calen = CoursesScheduleTable.getbyschoolid(aclog.SchoolID.Value);
            var calen = CoursesScheduleTable.getbyschoolid(schoolid);
            calen = calen.Where(n => n.Date.Value.Month == date.Month && n.Date.Value.Year == date.Year).ToList();

            List<CheckSchedule> l = new List<CheckSchedule>();
            for (int i = 0; i < calen.Count; i++)
            {
                var item = calen[i];
                var te = AccountTable.getbyID(item.TeacherID.Value);
                if (te == null)
                    continue;

                var c = CoursesTable.getbyid(item.CourseID.Value);
                if (c == null)
                    continue;
                var sty = StudyTimeTable.getbyid(item.StudyTimeID.Value);
                if (sty == null)
                    continue;

                string[] stu = sty.sTime.Split(':');
                string[] etu = sty.eTime.Split(':');
                TimeSpan s = new TimeSpan(int.Parse(stu[0]), int.Parse(stu[1]), 0);
                TimeSpan e = new TimeSpan(int.Parse(etu[0]), int.Parse(etu[1]), 0);

                //kiểm tra xem giáo viên up giáo trình buổi dạy ngày hôm đó chưa
                string colorcalenda = "#f42929";
                string url = "javascript:;";
                var ckcurri = CurriculumTeacherTable.getcurritecher(c.ID, item.Date.Value.Date, item.StudyTimeID.Value);
                if (ckcurri != null)
                {
                    colorcalenda = "#3cd629";
                    url = ckcurri.LinkDocument;
                }

                CheckSchedule t = new CheckSchedule();
                t.course = c.ID;
                t.title = c.CourseName + " GV: " + te.FullName;
                t.start = item.Date.Value.Add(s);
                t.end = item.Date.Value.Add(e);
                t.allDay = false;
                //t.url = "/Admin/CourseDetail/ScheduleCourse/" + c.ID + "";
                t.url = url;
                t.borderColor = colorcalenda;
                //t.room = room.RoomName;
                t.school = c.SchoolName;

                t.room = "";
                if (item.Date.Value == DateTime.Now.Date)
                {
                    if (aclog.RoleID == 4)
                    {
                        if (aclog.ID == item.TeacherID.Value) //chỉ cho GV có lịch đó được tạo phòng
                        {
                            if (string.IsNullOrEmpty(item.RoomID)) //chỉ hiện nút tạo phòng khi chưa tạo
                            {
                                t.school = "<a href=\"/Admin/ZoomMeeting/CreateRoom?CoursesScheduleID=" + item.ID + "\" class=\"btn btn-primary btn-sm\" style=\"font-size:10px; padding: 5px\"><i class=\"fas fa-video\" style=\"vertical-align:middle\"></i>  Tạo phòng mới</a>";
                            }
                            else if (!string.IsNullOrEmpty(item.RoomID) && !string.IsNullOrEmpty(item.RoomPass))
                            {
                                t.room = "<a href=\"javascript:;\" class=\"btn btn-success btn-sm\" onclick=\"CheckJoinRoom(" + item.ID + ")\" style=\"font-size:10px; padding: 5px\"><i class=\"far fa-play-circle\" style=\"vertical-align:middle\"></i>  Bắt đầu</a>";
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.RoomID) && !string.IsNullOrEmpty(item.RoomPass) && item.isRoomStart == true)
                        {
                            t.room = "<a href=\"javascript:;\" class=\"btn btn-success btn-sm\" onclick=\"CheckJoinRoom(" + item.ID + ")\" style=\"font-size:10px; padding: 5px\"><i class=\"far fa-play-circle\" style=\"vertical-align:middle\"></i>  Vào phòng học</a>";
                        }
                    }
                }

                int subid = 0;
                if (item.SubjectID != null)
                    subid = item.SubjectID.Value;
                else
                {
                    var cudt = CurriculumDetailTable.getbyID(item.CurriculumsDetailsID.Value);
                    if (cudt != null)
                        subid = cudt.SubjectID.Value;
                }
                var sub = SubjectTable.getbyID(subid);
                if (sub == null)
                {
                    sub = new tbl_Subject();
                    sub.SubjectName = "";
                }
                t.subject = sub.SubjectName;
                Task<bool> rs = datascheduleschool(l, c, t);
                if (rs.Result)
                    l.Add(t);
            }
            return Json(new { rs = true, l = l, m = date.ToString("dd/MM/yyyy") }, JsonRequestBehavior.AllowGet);
        }
        public async Task<bool> datascheduleschool(List<CheckSchedule> l, tbl_Courses c, CheckSchedule t)
        {
            bool ck = true;
            var lc = l.Where(n => n.course == c.ID).ToList();
            foreach (var jtem in lc)
            {
                if (jtem.course == t.course && jtem.start == t.start)
                {
                    ck = false;
                    break;
                }
            }
            return ck;
        }
        #endregion

        #region lịch test
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddAppointmentTest(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });


                //var district = DistrictTable.getbyID(f.Get("ddl-district").ToInt(1));
                tbl_District district = new tbl_District();
                if (f.Get("ddl-district") != null)
                    district = DistrictTable.getbyID(f.Get("ddl-district").ToInt(0));
                else
                {
                    district.ID = 0;
                    district.DistrictName = "";
                    district.LocationID = 0;
                    district.LocationName = "";
                }
                int sendmail = 0;
                if (!string.IsNullOrEmpty(f.Get("sendmail")))
                {
                    sendmail = f.Get("sendmail").ToInt(0);
                }
                string fullname = f.Get("txt-full-name").ToString();
                string email = f.Get("txt-phone").ToString();
                string phone = f.Get("txt-email").ToString();
                string address = f.Get("txt-address").ToString();
                var birthday = f.Get("txt-birth-day").ToString().Replace(" ", "");
                var accappometn = AccountTable.getbyID(f.Get("ddl-accsale").ToInt(0));
                var inPointHope = f.Get("in-point-hope").ToInt(0).ToString();
                var outPointHope = f.Get("out-point-hope").ToInt(0).ToString();
                var nation = 0;
                if (!string.IsNullOrEmpty(f.Get("slNation")))
                {
                    nation = f.Get("slNation").ToInt(0);
                }
                var teacher = 0;
                if (!string.IsNullOrEmpty(f.Get("slTeacher")))
                {
                    teacher = f.Get("slTeacher").ToInt(0);
                }
                var titleHentest = "";
                if (!string.IsNullOrEmpty(f.Get("hdtitle")))
                {
                    titleHentest = f.Get("hdtitle").ToString();
                }

                var contentHentest = "";
                if (!string.IsNullOrEmpty(f.Get("hdcontent")))
                {
                    contentHentest = f.Get("hdcontent").ToString();
                }
                var giaovien = AccountTable.getbyID(teacher);
                if (accappometn == null)
                {
                    using (var _db = new ZimEntities())
                    {
                        if (aclog.RoleID == 1)
                        {
                            accappometn = _db.tbl_Account.Where(x => x.RoleID == 6 && x.Status == 2).OrderByDescending(x => Guid.NewGuid()).FirstOrDefault();
                        }
                        else if (aclog.RoleID == 2)
                        {
                            var leaderTemp = _db.tbl_Account.Where(x => x.RoleID == 3 && x.Status == 2 && x.LeaderID == aclog.ID).OrderByDescending(x => Guid.NewGuid()).FirstOrDefault();
                            accappometn = _db.tbl_Account.Where(x => x.RoleID == 6 && x.Status == 2 && x.LeaderID == leaderTemp.ID).OrderByDescending(x => Guid.NewGuid()).FirstOrDefault();
                        }
                        if (aclog.RoleID == 6 || accappometn == null || aclog.RoleID == 3)
                        {
                            accappometn = aclog;
                        }
                    }
                }
                var jobs = new tbl_Job() { ID = 0, JobName = "" };
                if (!string.IsNullOrEmpty(f.Get("ddl-job")))
                    jobs = JobTable.getbyid(f.Get("ddl-job").ToInt(0));

                //string identity = f.Get("txt-identity-card").ToString();
                var identitycity = LocationTable.getbyID(f.Get("ddl-identity-city").ToInt(0));
                if (identitycity == null)
                {
                    identitycity = new tbl_City();
                    identitycity.ID = 0;
                    identitycity.CityName = "";
                }
                //string identitydate = f.Get("txt-identity-date").ToString();

                string workplace = f.Get("txt-work-place").ToString();
                //string notehome = f.Get("txt-note-home").ToString();

                var chool = new Models.tbl_School() { ID = 0, SchoolName = "" };
                if (!string.IsNullOrEmpty(f.Get("ddl-school")))
                    chool = SchoolTable.getbyID(f.Get("ddl-school").ToInt(0));

                DateTime datetest = new DateTime();
                try
                {
                    datetest = DateTime.ParseExact(f.Get("txt-date-test").ToString().Replace(" ", ""), "dd/MM/yyyy", null);
                }
                catch
                {
                    SetAlert("Định dạng ngày hẹn không đúng", "e");
                    return RedirectToAction("ListAppointmentTest", "Cashier");
                }
                string timetest = f.Get("txt-time-test").ToString();
                string note = f.Get("txt-note").ToString();

                int spid = accappometn.ID;
                var sc = SourceOfCustomerTable.getbyID(f.Get("ddl-soucrce").ToInt(0));
                if (sc == null)
                {
                    if (aclog.RoleID == 6)
                    {
                        sc = new tbl_SourceOfCustomer();
                        sc.ID = 0;
                        sc.SourceOfCustomer = "TVV";
                    }
                    else
                    {
                        sc = new tbl_SourceOfCustomer();
                        sc.ID = 0;
                        sc.SourceOfCustomer = "Khác";
                    }
                }
                tbl_Ward w = new tbl_Ward();
                if (f.Get("ddl-ward") != null)
                    w = WardTable.getbyid(f.Get("ddl-ward").ToInt(0));
                else
                {
                    w.ID = 0;
                    w.WardName = "";
                }
                tbl_Street s = new tbl_Street();
                if (f.Get("ddl-street") != null)
                    s = StreetTable.getbyid(f.Get("ddl-street").ToInt(0));
                else
                {
                    s.ID = 0;
                    s.StreetName = "";
                }
                tbl_AcademicPurposes p = new tbl_AcademicPurposes();
                if (!string.IsNullOrEmpty(f.Get("slPurpose")))
                    p = AcademicPurposesTable.getbyID(f.Get("slPurpose").ToInt(0));
                if (p == null)
                {
                    p = new tbl_AcademicPurposes() { ID = 0, AcademicPurposesName = "" };
                }
                //string homenumber = f.Get("txt-home-number").ToString();
                int language = f.Get("dll-language").ToInt(0);
                string linkfb = f.Get("txt-fb").ToString();
                string nationtime = "";
                if (!string.IsNullOrEmpty(f.Get("txt-nationtime")))
                {
                    nationtime = f.Get("txt-nationtime").ToString();
                }
                var ac = AccountTable.getbyusername(email);
                if (ac == null)
                {
                    ac = AccountTable.insertcustomer(district, fullname, email, phone, address, 2, "", spid, sc.ID, sc.SourceOfCustomer, birthday, jobs.ID, jobs.JobName,
                                        workplace, p.ID, p.AcademicPurposesName, inPointHope, outPointHope, "",
                                        "", "", identitycity.ID, identitycity.CityName, "", "", aclog.UserName,
                                        false, w, s, "", language, linkfb, nation);
                    int id = 5;
                    switch (ac.Language)
                    {
                        case 7:
                            id = 19;
                            break;
                        case 6:
                            id = 12;
                            break;
                        default:
                            id = 5;
                            break;
                    }
                    var emailTemplate = EmailTemplateTable.GetByID(id);
                    var title = emailTemplate.EmailTitle;
                    var content = emailTemplate.EmailContent;
                    content = content.Replace("[ten-hoc-vien]", fullname);
                    content = content.Replace("[tai-khoan]", ac.UserName);
                    content = content.Replace("[mat-khau]", "iccacademy");

                    AssetCRM.SendMail(ac.Email, title, content);
                }
                else
                {
                    if (ac.RoleID != 5)
                    {
                        SetAlert("Tài khoản này đã có trong hệ thống với chức vụ " + ac.RoleName, "e");
                        return RedirectToAction("ListAppointmentTest", "Cashier");
                    }
                    ac = AccountTable.updatecustomer(ac.ID, district, fullname, email, phone, address, 2, "", spid, sc.ID,
                        sc.SourceOfCustomer, birthday, jobs.ID, jobs.JobName, workplace, p.ID, p.AcademicPurposesName,
                        inPointHope, outPointHope, "", "", "", identitycity.ID, identitycity.CityName, "", "",
                        aclog.UserName, w, s, "", language, linkfb, nation);
                }
                var rs = StudentAppointmentTestTable.insert(ac, chool, accappometn, datetest, timetest, note, aclog.UserName, nationtime);
                try
                {
                    if (sendmail == 1)
                    {
                        Thread tnotify = new Thread(() => sendNotifyTest(rs, accappometn, giaovien, aclog, titleHentest, contentHentest));
                        tnotify.Start();
                    }
                }
                catch { }
                if (rs != null)
                    SetAlert("Thành công", "s");
                else
                    SetAlert("Không thành công", "e");
                return RedirectToAction("SaveinfoStudent", "Cashier");
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "AddAppointmentTest", 0, e.Message + e.InnerException);
                SetAlert("Không thành công", "e");
                return RedirectToAction("SaveinfoStudent", "Cashier");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveInfoTest(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 6 && aclog.RoleID != 7)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var test = StudentAppointmentTestTable.getbyid(f.Get("hdfID").ToInt(0));
                if (test == null)
                {
                    SetAlert("Không tìm thấy buổi test", "e");
                    return RedirectToAction("ListAppointmentTest", "Cashier");
                }

                var ngtuvan = AccountTable.getbyID(f.Get("ddl-user").ToInt(0));
                if (ngtuvan == null)
                {
                    ngtuvan = AccountTable.getbyID(test.AcademicID.Value);
                }
                
                var giaovien = AccountTable.getbyID(f.Get("ddl-teacher-test").ToInt(0));
                if (giaovien == null)
                {
                    giaovien = AccountTable.getbyID(test.TeacherID ?? 0);
                }

                double salaryTeacher = 0;
                if (aclog.RoleID == 1 || aclog.RoleID == 7)
                {
                    if (!string.IsNullOrEmpty(f.Get("salaryTeacher")))
                    {
                        salaryTeacher = Convert.ToDouble(f.Get("salaryTeacher").ToString().Replace(",", ""));
                    }
                }

                StudentAppointmentTestTable.updateTeacherAndTVV(test.ID, giaovien.ID, ngtuvan.ID, salaryTeacher);
                return RedirectToAction("ListAppointmentTest", "Cashier");
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "SaveInfoTest", 0, e.Message + "||" + e.InnerException);
                SetAlert("Vui lòng điền đầy đủ thông tin cần thiết", "e");
                return RedirectToAction("ListAppointmentTest", "Cashier");
            }
        }
        [HttpPost]
        public ActionResult SaveResultTest(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 4)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var test = StudentAppointmentTestTable.getbyid(f.Get("hdftID").ToInt(0));
                if (test == null)
                {
                    SetAlert("Không tìm thấy buổi test", "e");
                    return RedirectToAction("ListAppointmentTest", "Cashier");
                }

                string note = f.Get("txt-note").ToString();
                var rs = StudentAppointmentTestDetailTable.insert(test.ID, test.AcademicID.Value, test.TeacherID.Value, "0", "0", "0", "0", "0", 0, note, aclog.UserName, 0, 0);

                SetAlert("Thành công", "s");
                return RedirectToAction("ListAppointmentTest", "Cashier");
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "SaveResultTest", 0, e.Message + "||" + e.InnerException);
                SetAlert("Vui lòng điền đầy đủ thông tin cần thiết", "e");
                return RedirectToAction("ListAppointmentTest", "Cashier");
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveAcademicResult(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3 && aclog.RoleID != 6)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var test = StudentAppointmentTestTable.getbyid(f.Get("hdddID").ToInt(0));
                if (test == null)
                {
                    SetAlert("Không tìm thấy buổi test", "e");
                    return RedirectToAction("ListAppointmentTest", "Cashier");
                }
                var stu = AccountTable.getbyID(test.StudentUID.Value);
                var sch = SchoolTable.getbyID(test.SchoolID.Value);
                if (sch == null)
                {
                    sch = new tbl_School() { ID = 0, SchoolName = "" };
                }
                var tvv = AccountTable.getbyID(test.UIDCreateTest.Value);
                string input = f.Get("in-point-hope");
                string output = f.Get("out-point-hope");
                string numberoflesson = f.Get("txt-numberoflesson");
                string timeoflesson = f.Get("txt-timeoflesson");
                string linkdoc = f.Get("txt-linkdoc");
                string tt = f.Get("hdtt");
                string ct = f.Get("hdct");
                string title = f.Get("hdttdt");
                string content = f.Get("hdctdt");
                int sendmail = 0;
                if (!string.IsNullOrEmpty(f.Get("sendmail")))
                {
                    sendmail = f.Get("sendmail").ToInt(0);
                }
                var rs = StudentAppointmentTestTable.update(test.ID, stu, sch, tvv, test.DateTest.Value,
                    test.TimeTest, test.Note, aclog.UserName, numberoflesson, timeoflesson, linkdoc, test.NationTime);
                StudentAppointmentTestTable.updatetestdone(rs.ID, aclog.UserName);

                if (sendmail == 2 || sendmail == 3)
                {
                    Thread tnotify = new Thread(() => sendNotifyTestDone(rs, tt, ct));
                    tnotify.Start();
                }

                if (sendmail == 1 || sendmail == 3)
                {
                    Thread tnotifydaotao = new Thread(() => sendNotifydaotao(rs, title, content));
                    tnotifydaotao.Start();
                }

                using (var db = new ZimEntities())
                {
                    var acc = db.tbl_Account.Where(x => x.ID == test.StudentUID.Value).FirstOrDefault();
                    acc.ScoreIn = input;
                    acc.ScoreOut = output;
                    db.SaveChanges();
                }
                SetAlert("Thành công", "s");
                return RedirectToAction("ListAppointmentTest", "Cashier");
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "SaveAcademicResult", 0, e.Message + "||" + e.InnerException);
                SetAlert("Vui lòng điền đầy đủ thông tin cần thiết", "e");
                return RedirectToAction("ListAppointmentTest", "Cashier");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateReason(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 6 && aclog.RoleID != 7)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var test = StudentAppointmentTestTable.getbyid(f.Get("hdfIDr").ToInt(0));
                if (test == null)
                {
                    SetAlert("Không tìm thấy buổi test", "e");
                    return RedirectToAction("ListAppointmentTest", "Cashier");
                }

                // Ly do
                int ReasonType = f.Get("ReasonType").ToInt(0);
                int ReasonID = f.Get("ReasonID").ToInt(0);

                using (var db = new ZimEntities())
                {
                    var ck = db.tbl_StudentAppointmentTestDetail.Where(x => x.StudentAppointmentTestID == test.ID).FirstOrDefault();
                    if (ck != null)
                    {
                        ck.ReasonType = ReasonType;
                        ck.ReasonID = ReasonID;
                        db.SaveChanges();
                    }
                }

                return RedirectToAction("ListAppointmentTest", "Cashier");
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "SaveInfoTest", 0, e.Message + "||" + e.InnerException);
                SetAlert("Vui lòng điền đầy đủ thông tin cần thiết", "e");
                return RedirectToAction("ListAppointmentTest", "Cashier");
            }
        }

        [HttpGet]
        public ActionResult InvoiceList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
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
            // lấy ngày đầu và ngày cuối tháng
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            // thời gian bắt đầu từ
            string fromdate = firstDayOfMonth.ToString("yyyy-MM-dd 00:00:00.000");
            string todate = lastDayOfMonth.ToString("yyyy-MM-dd 23:59:59.998");
            ViewBag.From = firstDayOfMonth.ToString("dd/MM/yyyy");
            ViewBag.To = lastDayOfMonth.ToString("dd/MM/yyyy");
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

            int pageSize = 20;
            List<get_list_of_invoice_Result> l = new List<get_list_of_invoice_Result>();
            List<get_list_of_invoice_to_export_excel_Result> excelList = new List<get_list_of_invoice_to_export_excel_Result>();
            using (var db = new ZimEntities())
            {
                l = db.get_list_of_invoice(search, center, fromdate, todate, sort, pageSize, page - 1, aclog.ID, aclog.RoleID ?? 0).ToList();
            }

            ViewBag.Sort = sort;
            ViewBag.StartIndex = (page - 1) * pageSize;
            int total = 0; double totalPrice = 0;
            if (l.Count > 0)
            {
                total = l[0].TotalRow.Value;
                totalPrice = l[0].TotalPrice.Value;
            }
            ViewBag.TotalPrice = totalPrice;
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
        [HttpGet]
        public ActionResult ExportInvoiceList()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            string search = "";
            ViewBag.Search = Request.QueryString["search"];
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
            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                try
                {
                    DateTime fdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);

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

                    todate = tdate.ToString("dd/MM/yyyy 23:59:59");
                }
                catch { }
            }
            List<get_list_of_invoice_to_export_excel_Result> excelList = new List<get_list_of_invoice_to_export_excel_Result>();
            using (var db = new ZimEntities())
            {
                excelList = db.get_list_of_invoice_to_export_excel(search, center, fromdate, todate, sort).ToList();
            }

            var products = new System.Data.DataTable("Nam");
            products.Columns.Add("Họ và tên", typeof(string));
            products.Columns.Add("Số điện thoại", typeof(string));
            products.Columns.Add("Địa chỉ", typeof(string));
            products.Columns.Add("CMND", typeof(string));
            products.Columns.Add("Ngày cấp", typeof(string));
            products.Columns.Add("Nơi cấp", typeof(string));
            products.Columns.Add("Số tiền", typeof(string));
            products.Columns.Add("Phương thức thanh toán", typeof(string));
            products.Columns.Add("Ghi chú", typeof(string));
            products.Columns.Add("Trung tâm", typeof(string));
            products.Columns.Add("Người lập phiếu", typeof(string));
            products.Columns.Add("Ngày lập", typeof(string));
            foreach (var item in excelList)
            {
                try
                {
                    string identitydate = "";
                    if (item.IdentityCardDate != null)
                        identitydate = item.IdentityCardDate.Value.ToString("dd/MM/yyyy");
                    products.Rows.Add(item.StudentFullName, item.StudentPhone, item.StudentAddress, item.IdentityCard, identitydate, item.IdentityCardCity, String.Format("{0:0,0}", item.Price), item.PaymentmethodName, item.ReasonNote, item.SchoolName, item.CreatedInvoice, item.CreatedDate.Value.ToString("dd/MM/yyyy"));
                }
                catch
                {
                    continue;
                }
            }
            var grid = new GridView();
            grid.DataSource = products;
            grid.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=Excel invoices.xls");
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
            return RedirectToAction("InvoiceList", "Cashier", new { area = "Admin" });
        }

        #region phiếu thu - phiếu chi
        [HttpGet]
        public ActionResult Invoice(string code)
        {
            tbl_Invoices inv = InvoicesTable.getbycode(code);
            if (inv == null)
                return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });
            var checkAppointment = StudentAppointmentTable.getbyid(inv.AppointmentID.Value);
            if (checkAppointment != null)
            {
                ViewBag.CoefficientID = checkAppointment.CoefficientID;
                ViewBag.Appointment = checkAppointment;
            }
            return View(inv);
        }

        [HttpGet]
        public ActionResult InvoiceNew(string code)
        {
            string decode = AppZim.ZIM.Encryptor.DecryptURL(code);
            string[] arr = decode.Split('-');
            if (arr.Length != 3)
            {
                return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });
            }
            tbl_Invoices inv = InvoicesTable.getbycode(code);
            if (inv == null)
                return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });

            // 0 đăng ký học, 1 hẹn đăng ký, 2 mua dịch vụ, 3 Thanh toán tiền còn nợ, 4 refund, 5 đóng thêm tiền, 6 thanh toán combo

            ViewBag.NgayThanhToan = inv.CreatedDate.Value.ToString("dd/MM/yyyy HH:mm");
            ViewBag.Date = "Ngày " + inv.CreatedDate.Value.ToString("dd") + " Tháng " + inv.CreatedDate.Value.ToString("MM") + " Năm " + inv.CreatedDate.Value.ToString("yyyy") + "";
            ViewBag.Fullname = inv.StudentFullName;
            ViewBag.Phone = inv.StudentPhone;
            ViewBag.StudentUID = inv.StudentUID;
            if (!string.IsNullOrEmpty(inv.StudentAddress.Trim()))
                ViewBag.Address = inv.StudentAddress;
            else
                ViewBag.Address = "#";

            ViewBag.Money = String.Format("{0:0,0}", inv.Price);
            ViewBag.Moneytext = AppZim.ZIM.AssetCRM.MoneyToText(inv.Price.Value);

            ViewBag.Moneyleft = String.Format("{0:0,0}", inv.PriceLeft);
            ViewBag.Moneytextleft = AppZim.ZIM.AssetCRM.MoneyToText(inv.PriceLeft.Value);
            ViewBag.QrCode = inv.QRCode;
            ViewBag.Note = inv.ReasonNote;
            ViewBag.CamKet = inv.Camket;
            if (!string.IsNullOrEmpty(inv.PaymentmethodName))
                ViewBag.Payment = inv.PaymentmethodName;
            var CreatedInvoice = AccountTable.getbyusername(inv.CreatedBy);
            if (CreatedInvoice != null)
            {
                if (CreatedInvoice.RoleID != 5)
                    ViewBag.CreateInvoice = inv.CreatedInvoice;
            }
            ViewBag.NumberInvoice = inv.NumberInvoice;
            string schoolname = "";
            if (!string.IsNullOrEmpty(inv.SchoolName))
                schoolname = inv.SchoolName + ".";
            ViewBag.SchoolInvoice = schoolname;
            if (inv.DatePriceLeft != null)//ngày thu nợ
                ViewBag.DatePriceLeft = inv.DatePriceLeft.Value.ToString("dd/MM/yyyy");
            else
                ViewBag.DatePriceLeft = "#";
            if (inv.IdentityCard != null && inv.IdentityCardCity != null && inv.IdentityCardDate != null)
            {
                ViewBag.IdentityCard = inv.IdentityCard;
                ViewBag.IdentityCardDate = inv.IdentityCardDate.Value.ToString("dd/MM/yyyy");
                ViewBag.IdentityCardCity = inv.IdentityCardCity;
            }
            else
            {
                ViewBag.IdentityCard = "#";
                ViewBag.IdentityCardDate = "#";
                ViewBag.IdentityCardCity = "#";
            }
            bool show = true;
            if (inv.ServiceStudent > 0)
                show = false;
            ViewBag.ShowTT = show;
            return View(inv);
        }
        [HttpGet]
        public ActionResult Vouchers(string code)
        {
            string decode = AppZim.ZIM.Encryptor.DecryptURL(code);
            string[] arr = decode.Split('-');
            if (arr.Length != 3)
            {
                return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });
            }
            tbl_Invoices inv = InvoicesTable.getbycode(code);
            if (inv == null)
                return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });

            // 0 đăng ký học, 1 hẹn đăng ký, 2 mua dịch vụ, 3 Thanh toán tiền còn nợ

            ViewBag.Date = "Ngày " + inv.CreatedDate.Value.ToString("dd") + " Tháng " + inv.CreatedDate.Value.ToString("MM") + " Năm " + inv.CreatedDate.Value.ToString("yyyy") + "";
            ViewBag.Fullname = inv.StudentFullName;
            ViewBag.Phone = inv.StudentPhone;
            ViewBag.Address = inv.StudentAddress;

            ViewBag.Money = String.Format("{0:0,0}", inv.Price);
            ViewBag.Moneytext = AppZim.ZIM.AssetCRM.MoneyToText(inv.Price.Value);

            ViewBag.Moneyleft = String.Format("{0:0,0}", inv.PriceLeft);
            ViewBag.Moneytextleft = AppZim.ZIM.AssetCRM.MoneyToText(inv.PriceLeft.Value);
            ViewBag.QrCode = inv.QRCode;
            ViewBag.Note = inv.ReasonNote;
            ViewBag.CamKet = inv.Camket;
            if (!string.IsNullOrEmpty(inv.PaymentmethodName))
                ViewBag.Payment = inv.PaymentmethodName;

            ViewBag.CreateInvoice = inv.CreatedInvoice;
            ViewBag.NumberInvoice = inv.NumberInvoice;
            string schoolname = "";
            if (!string.IsNullOrEmpty(inv.SchoolName))
                schoolname = inv.SchoolName + ".";
            ViewBag.SchoolInvoice = schoolname;
            if (inv.DatePriceLeft != null)//ngày thu nợ
                ViewBag.DatePriceLeft = inv.DatePriceLeft.Value.ToString("dd/MM/yyyy");

            if (inv.IdentityCard != null && inv.IdentityCardCity != null && inv.IdentityCardDate != null)
            {
                ViewBag.IdentityCard = inv.IdentityCard;
                ViewBag.IdentityCardDate = inv.IdentityCardDate.Value.ToString("dd/MM/yyyy");
                ViewBag.IdentityCardCity = inv.IdentityCardCity;
            }
            bool show = true;
            if (inv.ServiceStudent > 0)
                show = false;
            ViewBag.ShowTT = show;
            return View();
        }
        #endregion

        [HttpPost]// lấy thông tin buổi test
        public ActionResult GetAppointmentTestByID(int ID)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var rs = StudentAppointmentTestTable.getbyid(ID);
            if (rs == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { rs = true, app = rs, date = rs.DateTest.Value.ToString("dd/MM/yyyy") }, JsonRequestBehavior.AllowGet);
        }

        //Cập nhật thông tin khách hàng và thông tin đằng ký
        [HttpPost]
        public ActionResult UpdateAppointmentTest(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var rs = AccountTable.getbyID(f.Get("hdfUID").ToInt(0));
            var accappometn = AccountTable.getbyID(f.Get("ddl-accsale").ToInt(0));
            var trungtam = SchoolTable.getbyID(f.Get("ddl-school").ToInt(0));
            if (trungtam == null)
            {
                trungtam = new Models.tbl_School();
                trungtam.ID = 0;
                trungtam.SchoolName = "";
            }
            DateTime datetest = new DateTime();
            try
            {
                datetest = DateTime.ParseExact(f.Get("txt-date-test").ToString().Replace(" ", ""), "dd/MM/yyyy", null);
            }
            catch
            {
                SetAlert("Định dạng ngày hẹn không đúng", "e");
                return RedirectToAction("ListAppointmentTest", "Cashier");
            }
            string timetest = f.Get("txt-time-test").ToString();
            string nationtime = f.Get("txt-nationtime").ToString();
            string notetest = f.Get("txt-note-test").ToString();

            var aptest = StudentAppointmentTestTable.getbyid(f.Get("hdfTestID").ToInt(0));
            if (aptest != null)
            {
                if (aptest.TestDone != true)
                    StudentAppointmentTestTable.update(aptest.ID, rs, trungtam, accappometn, datetest, timetest,
                        notetest, aclog.UserName, aptest.NumberOfLesson, aptest.TimeOfLesson, aptest.LinkDocument, nationtime);
            }
            var fulllink = f.Get("hdfulllink");
            SetAlert("Thành công", "s");
            if (!string.IsNullOrEmpty(fulllink))
                return Redirect(fulllink);
            return RedirectToAction("ListAppointmentTest", "Cashier");
        }
        #endregion


        [HttpGet]
        public ActionResult RefundList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
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
            //trạng thái
            int status = 0;
            if (Request.QueryString["status"].ToInt(0) != 0)
            {
                status = ViewBag.LopID = Request.QueryString["status"].ToInt(0);
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
            ViewBag.From = fromdate;
            ViewBag.To = todate;
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
            List<get_list_of_Refund_Result> l = new List<get_list_of_Refund_Result>();
            using (var db = new ZimEntities())
            {
                l = db.get_list_of_Refund(search, center, status, fromdate, todate, sort, pageSize, page - 1, aclog.ID).ToList();
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
        //admin duyệt hay ko duyệt yêu cầu
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult UpdateRefund(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var rc = RefundsTable.getbyid(f.Get("hdfID").ToInt(0));
            if (rc == null)
            {
                SetAlert("Không tìm thấy dữ liệu", "e");
                return RedirectToAction("RefundList", "Cashier");
            }
            if (rc.StatusID == 2)
            {
                SetAlert("Yêu cầu đã được xử lý", "e");
                return RedirectToAction("RefundList", "Cashier");
            }
            int price = f.Get("txt-refund").ToString().Replace(",", "").ToInt(0);
            if (price > rc.Price)
            {
                SetAlert("Số tiền vượt quá số tiền yêu cầu", "e");
                return RedirectToAction("RefundList", "Cashier");
            }
            string note = f.Get("txt-note-refund").ToString();
            int status = f.Get("ddl-status-refund").ToInt(3);// 3 ko duyệt
            var rs = RefundsTable.updatestatus(rc.ID, price, status, note, aclog.UserName);
            if (rs != null)
            {
                // gửi thông báo cho admin               
                var ladmin = AccountTable.getbyrole(1).Where(n => n.Status == 2 && n.ID == 1).ToList();
                string strPathAndQuery = Request.Url.PathAndQuery;
                string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                string datalink = "" + strUrl + "Admin/Cashier/RefundList";

                var item = AccountTable.getbyusername(rs.CreatedBy);
                //var item = AccountTable.getbyID(rs.StudentUID.Value);
                string title = "";
                string content = "";
                if (status == 2)
                {
                    title = "Yêu cầu hoàn tiền đã duyệt";
                    content = "Yêu cầu được duyệt với ID = \"" + rs.ID;
                    // kiểm tra:
                    /// nếu học viên này có ID chiến dịch && đã hết chiến dịch thì DoneSalary = true => để tính lương tvv
                    var check = StudentAppointmentTable.getbyid(rc.AppointmentID.Value);
                    if (check != null)
                    {
                        DateTime timenow = DateTime.Now.Date;
                        if (check.CampaignSaleID != null)
                        {
                            var camp = CampaignSaleTable.getbyid(check.CampaignSaleID.Value);
                            // so sanh: neu timenow > eTime => set DoneSalary = true
                            if (timenow > camp.eTime.Value.Date)
                            {
                                using (var db = new ZimEntities())
                                {
                                    var x = db.tbl_StudentAppointment.Where(n => n.ID == check.ID).FirstOrDefault();
                                    x.DoneSalary = true;
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }
                else
                {
                    title = "Yêu cầu hoàn tiền không được duyệt";
                    content = "Yêu cầu không được duyệt với ID = \"" + rs.ID;
                }
                var nt = NotificationTable.insertsingle("", title, content, item.ID, aclog.UserName, 10, 0);
                // gửi mail
                if (!string.IsNullOrEmpty(item.Email))
                    AppZim.ZIM.AssetCRM.SendMail(item.Email, nt.NotificationTitle, nt.NotificationContent);

                SetAlert("Thành công", "s");
            }
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("RefundList", "Cashier");
        }

        [HttpGet]
        public ActionResult PaymentVoucherList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
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
            //trạng thái
            int status = 0;
            if (Request.QueryString["status"].ToInt(0) != 0)
            {
                status = ViewBag.LopID = Request.QueryString["status"].ToInt(0);
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
            ViewBag.From = fromdate;
            ViewBag.To = todate;
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
            List<get_list_of_PaymentVoucher_Result> l = new List<get_list_of_PaymentVoucher_Result>();
            using (var db = new ZimEntities())
            {
                l = db.get_list_of_PaymentVoucher(search, center, fromdate, todate, sort, pageSize, page - 1, Request.QueryString["hdfexport"].ToInt(0), aclog.ID).ToList();
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
            if (Request.QueryString["hdfexport"].ToInt(0) == 1)
            {
                if (l.Count() > 0)
                {
                    var products = new System.Data.DataTable("Nam");
                    products.Columns.Add("Họ và tên", typeof(string));
                    products.Columns.Add("Số điện thoại", typeof(string));
                    products.Columns.Add("Địa chỉ", typeof(string));
                    products.Columns.Add("CMND", typeof(string));
                    products.Columns.Add("Ngày cấp", typeof(string));
                    products.Columns.Add("Nơi cấp", typeof(string));
                    products.Columns.Add("Số tiền", typeof(string));
                    products.Columns.Add("Phương thức thanh toán", typeof(string));
                    products.Columns.Add("Ghi chú", typeof(string));
                    products.Columns.Add("Trung tâm", typeof(string));
                    products.Columns.Add("Người lập phiếu", typeof(string));
                    products.Columns.Add("Ngày lập", typeof(string));
                    foreach (var item in l)
                    {
                        try
                        {
                            string identitydate = "";
                            if (item.IdentityCardDate != null)
                                identitydate = item.IdentityCardDate.Value.ToString("dd/MM/yyyy");
                            products.Rows.Add(item.StudentFullName, "'" + item.StudentPhone, item.StudentAddress, item.IdentityCard, identitydate, item.IdentityCardCity, String.Format("{0:0,0}", item.Price), item.PaymentmethodName, item.ReasonNote, item.SchoolName, item.CreatedInvoice, item.CreatedDate.Value.ToString("dd/MM/yyyy"));
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    var grid = new GridView();
                    grid.DataSource = products;
                    grid.DataBind();

                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename=Excel invoices.xls");
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
            }
            return View(l);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CreateVoucherss(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var refund = RefundsTable.getbyid(f.Get("hdfvcID").ToInt(0));
            if (refund == null)
            {
                SetAlert("Không tìm thấy dữ liệu", "e");
                return RedirectToAction("RefundList", "Cashier");
            }
            if (refund.RefundDone.Value)
            {
                SetAlert("Đã thanh toán, không thể tạo thêm phiếu chi", "e");
                return RedirectToAction("RefundList", "Cashier");
            }
            int pay = f.Get("ddl-payment").ToInt(0);
            string payname = Paymentmethod(pay);
            var school = SchoolTable.getbyID(f.Get("ddl-school-refund").ToInt(0));
            if (refund == null)
            {
                SetAlert("Không tìm thấy trung tâm", "e");
                return RedirectToAction("RefundList", "Cashier");
            }
            var ac = AccountTable.getbyID(refund.StudentUID.Value);
            if (ac == null)
            {
                SetAlert("Không tìm thông tin khách hàng", "e");
                return RedirectToAction("RefundList", "Cashier");
            }
            //Hóa đơn
            if (refund.Price > 0)
            {
                string name = "4-" + refund.ID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";//4 phiếu chi, refund
                string nameencode = AppZim.ZIM.Encryptor.EncryptURL(name);
                string strPathAndQuery = Request.Url.PathAndQuery;
                string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                string datalink = "" + strUrl + "vouchers/?code=" + nameencode;
                var barcode = AppZim.ZIM.AssetCRM.CreateQRCode(datalink, name);

                tbl_Invoices iv = InBill(0, 5, refund.ID, ac, refund.Price.Value, f.Get("txt-note-vouchers").ToString(), "", 0, "", school, aclog, pay, payname, name, nameencode, datalink);
                if (iv != null)
                {
                    //==============>>>>
                    RefundsTable.updatedone(refund.ID, aclog.UserName);

                    if (refund.CourseStudentID > 0)// hủy học
                    {
                        if (refund.KickOut != null)
                        {
                            if (refund.KickOut.Value)
                            {
                                var rs = CoursesStudentTable.removestudentoutcourse(refund.CourseStudentID.Value, "Hoàn tiền", aclog.UserName);
                                int count = CoursesStudentTable.getbycourseid(rs.CourseID.Value).Where(n => n.isHide != true).ToList().Count;
                                CoursesTable.updatecountstudent(rs.CourseID.Value, count, aclog.UserName);
                            }
                        }
                    }
                    if (refund.AppointmentID > 0)// hủy đăng ký
                        StudentAppointmentTable.updateclose(refund.AppointmentID.Value, "Hoàn tiền", aclog.UserName);
                    SetlinkInvoice(datalink);
                }
                else
                {
                    SetAlert("Không thành công", "e");
                    return RedirectToAction("RegisterCourseCustom", "Cashier");
                }

            }
            SetAlert("Thành công", "s");
            return RedirectToAction("RefundList", "Cashier");
        }

        [HttpPost]
        public ActionResult ReportInvoice(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var l = InvoicesTable.getall().Where(n => n.Refund != true).ToList();

            if (!string.IsNullOrEmpty(f.Get("txt-from-excel")))
            {
                DateTime from = DateTime.ParseExact(f.Get("txt-from-excel").ToString(), "dd/MM/yyyy", null);
                l = l.Where(n => n.CreatedDate.Value.Date >= from.Date).ToList();
            }
            if (!string.IsNullOrEmpty(f.Get("txt-to-excel")))
            {
                DateTime to = DateTime.ParseExact(f.Get("txt-to-excel").ToString(), "dd/MM/yyyy", null);
                l = l.Where(n => n.CreatedDate.Value.Date <= to.Date).ToList();
            }
            l = l.OrderByDescending(n => n.CreatedDate).ToList();
            if (l.Count > 0)
            {
                var products = new System.Data.DataTable("Nam");
                products.Columns.Add("Họ và tên", typeof(string));
                products.Columns.Add("Số điện thoại", typeof(string));
                products.Columns.Add("Địa chỉ", typeof(string));
                products.Columns.Add("CMND", typeof(string));
                products.Columns.Add("Ngày cấp", typeof(string));
                products.Columns.Add("Nơi cấp", typeof(string));
                products.Columns.Add("Số tiền", typeof(string));
                products.Columns.Add("Phương thức thanh toán", typeof(string));
                products.Columns.Add("Ghi chú", typeof(string));
                products.Columns.Add("Trung tâm", typeof(string));
                products.Columns.Add("Người lập phiếu", typeof(string));
                products.Columns.Add("Ngày lập", typeof(string));
                for (int i = 0; i < l.Count; i++)
                {
                    var item = l[i];
                    try
                    {
                        string identitydate = "";
                        if (item.IdentityCardDate != null)
                            identitydate = item.IdentityCardDate.Value.ToString("dd/MM/yyyy");
                        products.Rows.Add(item.StudentFullName, "'" + item.StudentPhone, item.StudentAddress, item.IdentityCard, identitydate, item.IdentityCardCity, String.Format("{0:0,0}", item.Price), item.PaymentmethodName, item.ReasonNote, item.SchoolName, item.CreatedInvoice, item.CreatedDate.Value.ToString("dd/MM/yyyy"));
                    }
                    catch
                    {
                        continue;
                    }
                }

                var grid = new GridView();
                grid.DataSource = products;
                grid.DataBind();

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=Excel invoices.xls");
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
            else
                SetAlert("Chưa có dữ liệu", "w");
            return RedirectToAction("InvoiceList", "Cashier");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pay(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int money = f.Get("txt-paid-payfor").ToString().Replace(",", "").ToInt(0);
            if (money <= 0)
            {
                SetAlert("Vui lòng nhập số tiền cần thanh toán", "e");
                return RedirectToAction("RegisterCourseCustom", "Cashier");
            }
            int ID = f.Get("hdfIDPay").ToInt(0);
            int Payfor = f.Get("hdfPayfor").ToInt(0);
            int school = f.Get("ddl-school-payfor").ToInt(0);
            var choolinvoice = SchoolTable.getbyID(school);
            if (choolinvoice == null)
            {
                SetAlert("Không tìm thấy trung tâm tạo phiếu", "e");
                return RedirectToAction("CustomerDebts", "Customer");
            }

            int payment = f.Get("ddl-payment-payfor").ToInt(0);
            string paymentname = CashierController.Paymentmethod(payment);
            string ngaythuno = "";
            try
            {
                ngaythuno = DateTime.ParseExact(f.Get("txt-date-left").ToString().Replace(" ", ""), "dd/MM/yyyy", null).ToString("dd/MM/yyyy");
            }
            catch { }
            string ghichu = f.Get("txt-ghi-chu-pay").ToString();
            //thanh toán khóa lẻ
            List<RegisterCourseAndClass> listRCC = new List<RegisterCourseAndClass>();
            if (Payfor == 1)
            {
                var cs = CoursesStudentTable.getbyid(ID);
                if (cs != null)
                {
                    if (money > 0)
                    {
                        //cập nhật dữ liệu khóa
                        var sc = CoursesStudentTable.updatePayCourse(cs.ID, money, ghichu, aclog.UserName);
                        var cus = AccountTable.getbyID(sc.StudentUID.Value);
                        if (sc != null)
                        {
                            if (sc.ComboID != null && sc.ComboID != 0)
                            {
                                //tinh tien con lai combo
                                ComboTable.UpdatePriceLeft(sc.ComboID.Value, money, aclog.UserName);
                                string name = "6-" + sc.ComboID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";//6 cọc thêm thuộc combo
                                string nameencode = AppZim.ZIM.Encryptor.EncryptURL(name);
                                string strPathAndQuery = Request.Url.PathAndQuery;
                                string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                                string datalink = "" + strUrl + "Admin/Cashier/Invoice/?code=" + nameencode;
                                var barcode = AppZim.ZIM.AssetCRM.CreateQRCode(datalink, name);
                                string gcpt = "";
                                if (!string.IsNullOrEmpty(ghichu))
                                    gcpt += " [Ghi chú: " + ghichu + "]";
                                //đăng ký combo                      
                                //iv = InvoicesTable.insert(false, 0, 0, 0, 0, sc.ComboID.Value, cus, money, ComboTable.GetCurrentPriceLeft(sc.ComboID.Value), gcpt, barcode, nameencode, aclog, ngaythuno, payment, paymentname, choolinvoice);  
                                var iv = InvoicesTable.insert(0, false, sc.ID, cs.AppointmentID.Value, 0, 0, sc.ComboID.Value, cus, money, ComboTable.GetCurrentPriceLeft(cs.ComboID.Value), gcpt, "", barcode, nameencode, aclog, ngaythuno, payment, paymentname, choolinvoice, 0);
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
                                if (iv != null)
                                {
                                    SetlinkInvoice(datalink);
                                }
                                else
                                {
                                    SetAlert("Thành công", "s");
                                    return RedirectToAction("RegisterCourseCustom", "Cashier");
                                }
                            }
                            else
                            {
                                string name = "3-" + cs.ID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";//3 thanh toán số tiền còn lại
                                string nameencode = AppZim.ZIM.Encryptor.EncryptURL(name);
                                string strPathAndQuery = Request.Url.PathAndQuery;
                                string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                                string datalink = "" + strUrl + "Admin/Cashier/Invoice/?code=" + nameencode;
                                var barcode = AppZim.ZIM.AssetCRM.CreateQRCode(datalink, name);
                                tbl_Invoices iv = InBill(0, 1, cs.ID, cus, money, cs.CourseName + ghichu, "", sc.PriceLeft.Value, ngaythuno, choolinvoice, aclog, payment, paymentname, name, nameencode, datalink);
                                tbl_InvoiceDetail ivdetail = InvoiceDetailTable.insert(iv.ID, iv.ComboID.Value, sc.ID, 1, sc.PriceCourse.Value, aclog.UserName);
                                if (iv != null)
                                {
                                    SetlinkInvoice(datalink);
                                }
                                else
                                {
                                    SetAlert("Thành công", "s");
                                    return RedirectToAction("RegisterCourseCustom", "Cashier");
                                }
                            }

                        }
                    }
                    SetAlert("Thành công", "s");
                }
            }
            //thanh toán hẹn đăng ký deposit
            else if (Payfor == 2)
            {
                var appoint = StudentAppointmentTable.getbyid(ID);
                if (appoint == null)
                {
                    SetAlert("Không tìm thấy dữ liệu", "e");
                    return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                }

                var cus = AccountTable.getbyID(appoint.StudentUID.Value);
                if (cus != null)
                {
                    var diadiem = LocationTable.getbyID(appoint.CityID.Value);
                    var lop = ClassTable.getbyID(appoint.ClassID.Value);
                    var trungtam = SchoolTable.getbyID(appoint.SchoolID.Value);
                    var cahoc = StudyTimeTable.getbyid(appoint.StudyID.Value);

                    string datedeposit = "";

                    if (appoint != null)
                    {
                        if (appoint.DateAppointment != null)
                            datedeposit = appoint.DateAppointment.Value.ToString("dd/MM/yyyy");
                    }
                    if (!string.IsNullOrEmpty(f.Get("txt-date-left").ToString()))
                    {
                        try
                        {
                            datedeposit = DateTime.ParseExact(f.Get("txt-date-left").ToString().Replace(" ", ""), "dd/MM/yyyy", null).ToString("dd/MM/yyyy");
                        }
                        catch { }
                    }


                    if (appoint != null)
                    {
                        appoint = StudentAppointmentTable.update(appoint.ID, cus, diadiem, trungtam, lop, cahoc, 0, ghichu,
                            appoint.PaymentMethodID.Value, appoint.PaymentMethodName, aclog.UserName, datedeposit, appoint.Lesson ?? 0);
                    }

                    //thêm tiền
                    if (money > 0)
                    {
                        if (appoint.CloseAppointment == 1)
                        {
                            SetAlert("Đăng ký đã hủy, không thể cọc thêm", "e");
                            return RedirectToAction("RegisterCourseCustom", "Cashier");
                        }
                        if (choolinvoice == null)
                        {
                            SetAlert("Vui lòng chọn trung tâm thu tiền", "e");
                            return RedirectToAction("RegisterCourseCustom", "Cashier");
                        }


                        //cập nhật thêm số tiền cọc hẹn
                        var rs = StudentAppointmentTable.update(appoint.ID, cus, diadiem, trungtam, lop, cahoc, money, ghichu,
                           payment, paymentname, aclog.UserName, datedeposit, appoint.Lesson ?? 0);

                        if (rs != null)
                        {
                            if (rs.ComboID != null && rs.ComboID != 0)
                            {
                                ComboTable.UpdatePriceLeft(rs.ComboID.Value, money, aclog.UserName);
                                string name = "6-" + rs.ComboID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";//6 cọc thêm thuộc combo
                                string nameencode = AppZim.ZIM.Encryptor.EncryptURL(name);
                                string strPathAndQuery = Request.Url.PathAndQuery;
                                string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                                string datalink = "" + strUrl + "Admin/Cashier/Invoice/?code=" + nameencode;
                                var barcode = AppZim.ZIM.AssetCRM.CreateQRCode(datalink, name);
                                string gcpt = "";
                                if (!string.IsNullOrEmpty(ghichu))
                                    gcpt += " [Ghi chú: " + ghichu + "]";
                                //var iv = InBill(3, rs.ComboID.Value, cus, money, gcpt, 0, datedeposit, choolinvoice, aclog, payment, paymentname, name, nameencode, datalink);
                                var iv = InvoicesTable.insert(0, false, 0, rs.ID, 0, 0, rs.ComboID.Value, cus, money, ComboTable.GetCurrentPriceLeft(rs.ComboID.Value), gcpt, "", barcode, nameencode, aclog, ngaythuno, payment, paymentname, choolinvoice, 0);
                                //lấy danh sách các cái khác thuộc combo
                                List<tbl_ComboDetail> lbdetail = ComboDetailTable.getbycomboid(rs.ComboID.Value);
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
                                if (iv != null)
                                    SetlinkInvoice(datalink);
                                else
                                {
                                    SetAlert("Thành công", "s");
                                    return RedirectToAction("RegisterCourseCustom", "Cashier");
                                }

                            }
                            else
                            {
                                string name = "5-" + appoint.ID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";//5 cọc thêm
                                string nameencode = AppZim.ZIM.Encryptor.EncryptURL(name);
                                string strPathAndQuery = Request.Url.PathAndQuery;
                                string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                                string datalink = "" + strUrl + "Admin/Cashier/Invoice/?code=" + nameencode;
                                var barcode = AppZim.ZIM.AssetCRM.CreateQRCode(datalink, name);

                                string gcpt = appoint.ClassName;
                                if (!string.IsNullOrEmpty(ghichu))
                                    gcpt += " [Ghi chú: " + ghichu + "]";
                                var iv = InBill(0, 2, appoint.ID, cus, money, gcpt, "", 0, datedeposit, choolinvoice, aclog, payment, paymentname, name, nameencode, datalink);
                                if (iv != null)
                                    SetlinkInvoice(datalink);
                                else
                                {
                                    SetAlert("Thành công", "s");
                                    return RedirectToAction("RegisterCourseCustom", "Cashier");
                                }
                            }

                        }
                    }
                    SetAlert("Thành công", "s");
                }

            }
            //thanh toán combo = 3
            else
            {
                var Combo = ComboTable.getbyid(ID);
                var ac = AccountTable.getbyID(Combo.UID.Value);
                if (Combo != null)
                {
                    int moneyleft = money;
                    var cbd = ComboDetailTable.getbycomboid(Combo.ID);
                    List<tbl_ComboDetail> temp = new List<tbl_ComboDetail>();
                    foreach (var item in cbd)
                    {
                        if (item.TypeID == 1 || item.TypeID == null)
                        {
                            var studentcourse = CoursesStudentTable.getbycourseid(item.CourseID.Value).Where(n => n.ComboID == item.ComboID).FirstOrDefault();
                            if (studentcourse != null)
                            {
                                if (studentcourse.PriceLeft > 0)
                                {
                                    tbl_ComboDetail c = new tbl_ComboDetail();
                                    c.ComboID = Combo.ID;
                                    c.CoursePrice = studentcourse.PriceLeft;
                                    c.TypeID = 1;
                                    c.CourseID = studentcourse.CourseID;
                                    temp.Add(c);
                                }
                            }
                        }
                        else
                        {
                            var appointment = StudentAppointmentTable.getbycomboclass(item.ComboID.Value, item.CourseID.Value);
                            if (appointment != null)
                            {
                                if (appointment.isHide == true)
                                {
                                    var studentcourse = CoursesStudentTable.getbystudentid(appointment.StudentUID.Value).Where(n => n.ComboID == Combo.ID && n.AppointmentID == appointment.ID).FirstOrDefault();
                                    if (studentcourse != null)
                                    {
                                        if (studentcourse.PriceLeft > 0)
                                        {
                                            tbl_ComboDetail c = new tbl_ComboDetail();
                                            c.ComboID = Combo.ID;
                                            c.CoursePrice = studentcourse.PriceLeft;
                                            c.TypeID = 3;
                                            c.CourseID = studentcourse.CourseID;
                                            temp.Add(c);
                                        }
                                    }

                                }
                                else
                                {
                                    //var cl = ClassTable.getbyID(appointment.ClassID.Value);
                                    var discount = ComboDetailTable.getbycomboidvscourseid(Combo.ID, item.CourseID.Value);
                                    int priceleft = appointment.ClassPrice.ToString().ToInt(0) - appointment.Deposit.Value - discount.DiscountPrice.Value;
                                    if (priceleft > 0)
                                    {
                                        tbl_ComboDetail c = new tbl_ComboDetail();
                                        c.ComboID = Combo.ID;
                                        c.CoursePrice = priceleft;
                                        c.TypeID = 2;
                                        c.CourseID = appointment.ClassID;
                                        temp.Add(c);
                                    }
                                }
                            }
                        }
                    }
                    temp = temp.OrderBy(n => n.CoursePrice).ToList();
                    bool chek = true;
                    foreach (var item in temp)
                    {
                        if (moneyleft > 0)
                        {
                            if (item.TypeID == 1 || item.TypeID == 3)
                            {
                                var cr = CoursesStudentTable.getbycourseid(item.CourseID.Value).Where(n => n.ComboID == item.ComboID).FirstOrDefault();
                                if (cr != null)
                                {
                                    if (cr.PriceLeft > moneyleft)
                                    {
                                        var ck = CoursesStudentTable.updatePayCourse(cr.ID, moneyleft, ghichu, aclog.UserName);
                                        if (ck == null)
                                            chek = false;
                                        moneyleft = 0;
                                    }
                                    else
                                    {
                                        var ck = CoursesStudentTable.updatePayCourse(cr.ID, cr.PriceLeft.Value, ghichu, aclog.UserName);
                                        if (ck == null)
                                            chek = false;
                                        moneyleft = moneyleft - cr.PriceLeft.Value;
                                    }
                                }

                            }
                            else
                            {
                                var ap = StudentAppointmentTable.getbycomboclass(item.ComboID.Value, item.CourseID.Value);
                                var cus = AccountTable.getbyID(ap.StudentUID.Value);
                                if (cus != null)
                                {
                                    var diadiem = LocationTable.getbyID(ap.CityID.Value);
                                    var lop = ClassTable.getbyID(ap.ClassID.Value);
                                    var trungtam = SchoolTable.getbyID(ap.SchoolID.Value);
                                    var cahoc = StudyTimeTable.getbyid(ap.StudyID.Value);
                                    string datedeposit = "";

                                    if (ap != null)
                                    {
                                        if (ap.DateAppointment != null)
                                            datedeposit = ap.DateAppointment.Value.ToString("dd/MM/yyyy");
                                    }
                                    if (!string.IsNullOrEmpty(f.Get("txt-date-left").ToString()))
                                    {
                                        try
                                        {
                                            datedeposit = DateTime.ParseExact(f.Get("txt-date-left").ToString().Replace(" ", ""), "dd/MM/yyyy", null).ToString("dd/MM/yyyy");
                                        }
                                        catch { }
                                    }
                                    if (ap != null)
                                    {

                                        //var cls = ClassTable.getbyID(ap.ClassID.Value);
                                        var discount = ComboDetailTable.getbycomboidvscourseid(item.ComboID.Value, ap.ClassID.Value);
                                        if (discount == null)
                                        {
                                            discount = new tbl_ComboDetail();
                                            discount.DiscountPrice = 0;
                                        }
                                        int priceleft = ap.ClassPrice.ToString().ToInt(0) - ap.Deposit.Value - discount.DiscountPrice.Value;

                                        if (moneyleft <= priceleft)
                                        {
                                            ap = StudentAppointmentTable.update(ap.ID, cus, diadiem, trungtam, lop, cahoc, moneyleft, ghichu,
                                        ap.PaymentMethodID.Value, ap.PaymentMethodName, aclog.UserName, datedeposit, ap.Lesson ?? 0);
                                            if (ap == null)
                                                chek = false;
                                            moneyleft = 0;
                                        }
                                        else
                                        {
                                            ap = StudentAppointmentTable.update(ap.ID, cus, diadiem, trungtam, lop, cahoc, priceleft, ghichu,
                                                ap.PaymentMethodID.Value, ap.PaymentMethodName, aclog.UserName, datedeposit, ap.Lesson ?? 0);
                                            if (ap == null)
                                                chek = false;
                                            moneyleft = moneyleft - priceleft;
                                        }
                                    }
                                }
                            }
                        }

                    }
                    if (chek)
                    {
                        ComboTable.UpdatePriceLeft(Combo.ID, money, aclog.UserName);
                        string name = "";
                        name = "6-" + ID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";//6 đăng ký combo học
                        string nameencode = AppZim.ZIM.Encryptor.EncryptURL(name);
                        string strPathAndQuery = Request.Url.PathAndQuery;
                        string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                        string datalink = "" + strUrl + "Admin/Cashier/Invoice/?code=" + nameencode;
                        var barcode = AppZim.ZIM.AssetCRM.CreateQRCode(datalink, name);
                        tbl_Invoices iv = InBill(0, 3, Combo.ID, ac, money, ghichu, "", ComboTable.GetCurrentPriceLeft(Combo.ID), ngaythuno, choolinvoice, aclog, payment, paymentname, name, nameencode, datalink);
                        //lấy danh sách các cái khác thuộc combo
                        List<tbl_ComboDetail> lbdetail = ComboDetailTable.getbycomboid(Combo.ID);
                        foreach (var item in lbdetail)
                        {
                            if (item.TypeID == 1)
                            {
                                tbl_CoursesStudent coursestudent = CoursesStudentTable.getbyCourseIDdandStudentID(item.CourseID.Value, ac.ID);
                                tbl_InvoiceDetail ivd = InvoiceDetailTable.insert(iv.ID, coursestudent.ComboID.Value, coursestudent.CourseID.Value, 1, coursestudent.PriceCourse.Value, aclog.UserName);
                            }
                            else
                            {
                                tbl_StudentAppointment stapp = StudentAppointmentTable.getbycomboclass(item.ComboID.Value, item.CourseID.Value);
                                if (stapp.isHide.Value)
                                {
                                    tbl_CoursesStudent coursestudent = CoursesStudentTable.getbystudentid(ac.ID).Where(n => n.AppointmentID == stapp.ID).FirstOrDefault();
                                    tbl_InvoiceDetail ivd = InvoiceDetailTable.insert(iv.ID, coursestudent.ComboID.Value, coursestudent.ID, 3, coursestudent.PriceCourse.Value, aclog.UserName);
                                }
                                else
                                {
                                    tbl_InvoiceDetail ivd = InvoiceDetailTable.insert(iv.ID, stapp.ComboID.Value, stapp.ClassID.Value, 2, stapp.ClassPrice.Value, aclog.UserName);
                                }
                            }
                        }
                        if (iv != null)
                            SetlinkInvoice(datalink);
                        else
                        {
                            SetAlert("Không thành công", "e");
                            return RedirectToAction("RegisterCourseCustom", "Cashier");
                        }
                    }
                    SetAlert("Thành công", "s");
                }
            }

            return RedirectToAction("RegisterCourseCustom", "Cashier");
        }
        public static tbl_Invoices InBill(int rewardTeachers, int payfor, int ID, tbl_Account student, int price, string note, string camket,
            int priceleft, string ngaythuno, tbl_School choolinvoice, tbl_Account createby, int payment, string paymentname,
            string name, string nameencode, string datalink)
        {
            try
            {
                var barcode = "";
                try
                {
                    barcode = AssetCRM.CreateQRCode(datalink, name);
                }
                catch (Exception e)
                {
                    barcode = "";
                    AssetCRM.Writelog("Không tạo được barcode", "InBill - Cashier", 0, e.Message + e.InnerException);
                }
                var iv = new tbl_Invoices();
                switch (payfor)
                {

                    case 1://In bill khóa
                        {

                            iv = InvoicesTable.insert(rewardTeachers, false, ID, 0, 0, 0, 0, student, price, priceleft, note, camket, barcode, nameencode, createby, ngaythuno, payment, paymentname, choolinvoice, 0);
                            break;
                        }
                    case 2://In bill lớp
                        {
                            iv = InvoicesTable.insert(rewardTeachers, false, 0, ID, 0, 0, 0, student, price, 0, note, camket, barcode, nameencode, createby, ngaythuno, payment, paymentname, choolinvoice, 0);
                            break;
                        }
                    case 3://In bill combo
                        {
                            var appointment = StudentAppointmentTable.getbyid(ID);
                            iv = InvoicesTable.insert(rewardTeachers, false, 0, ID, 0, 0, appointment?.ComboID ?? 0, student, price, priceleft, note, camket, barcode, nameencode, createby, ngaythuno, payment, paymentname, choolinvoice, 0);
                            break;
                        }
                    case 4://In bill mua dịch vụ
                        {
                            iv = InvoicesTable.insert(rewardTeachers, false, 0, 0, ID, 0, 0, student, price, priceleft, note, camket, barcode, nameencode, createby, ngaythuno, payment, paymentname, choolinvoice, 0);
                            break;
                        }
                    case 5://In bill hoàn tiền
                        {
                            iv = InvoicesTable.insert(rewardTeachers, true, 0, 0, 0, ID, 0, student, price, priceleft, note, camket, barcode, nameencode, createby, ngaythuno, payment, paymentname, choolinvoice, 0);
                            break;
                        }
                    case 6: //In bill thu tiền mua gói
                        {
                            iv = InvoicesTable.insert(rewardTeachers, true, 0, 0, 0, 0, 0, student, price, priceleft, note, camket, barcode, nameencode, createby, ngaythuno, payment, paymentname, choolinvoice, ID);
                            break;
                        }
                }
                try
                {
                    //đẩy thông báo cho admin
                    var ladmin = AccountTable.getbyrole(1).Where(n => n.Status == 2 || n.ID == 1).ToList();
                    foreach (var item in ladmin)
                    {
                        string title = "[" + choolinvoice.SchoolName + "] - Phiếu thu " + String.Format("{0:0,0 VNĐ}", iv.Price) + "";
                        string content = "Phát sinh phiếu thu có ID = \"" + iv.ID;
                        var nt = NotificationTable.insertsingle("", title, content, item.ID, createby.UserName, 9, 0);
                        // gửi mail
                        if (!string.IsNullOrEmpty(item.Email) && item.ID == 1)
                            AppZim.ZIM.AssetCRM.SendMail(item.Email, nt.NotificationTitle, nt.NotificationContent);
                    }
                }
                catch (Exception e)
                {
                    AssetCRM.Writelog("Không tạo được thông báo", "InBill - Cashier", 0, e.Message + e.InnerException);
                }

                //==============>>>>
                return iv;
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("InBill", "Cashier", 0, e.Message + "||" + e.InnerException);
                return null;
            }
        }

        [HttpPost]
        public ActionResult DeleteInvoice(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var kq = InvoicesTable.deleteinvoice(id, aclog.UserName);
            if (kq == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            //Hủy thông tin liên quan 
            //Hủy khóa
            if (kq.CourseStudentID != 0 && kq.ComboID == 0)
            {
                CoursesStudentTable.delete(kq.CourseStudentID.Value, "Hủy phiếu thu", aclog.UserName);
                int priceinvoice = kq.Price.Value;
                tbl_CoursesStudent ct = CoursesStudentTable.getbyid(kq.CourseStudentID.Value);
                if (ct != null)
                {
                    ct = CoursesStudentTable.updatepriceleft(ct.ID, priceinvoice, aclog.UserName);
                }
            }
            //Hủy hẹn
            if (kq.AppointmentID != 0 && kq.ComboID == 0)
            {
                int priceinvoice = kq.Price.Value;
                StudentAppointmentTable.delete(kq.CourseStudentID.Value, "Hủy phiếu thu", aclog.UserName);
                tbl_StudentAppointment appoint = StudentAppointmentTable.getbyid(kq.AppointmentID.Value);
                if (appoint.isHide.Value)
                {
                    tbl_CoursesStudent ct = CoursesStudentTable.getbystudentid(kq.StudentUID.Value).Where(n => n.AppointmentID == appoint.ID && n.ComboID == kq.ComboID).FirstOrDefault();
                    if (ct != null)
                    {
                        ct = CoursesStudentTable.updatepriceleft(ct.ID, priceinvoice, aclog.UserName);
                    }
                }
                else
                {
                    appoint = StudentAppointmentTable.udpateDeposit(appoint.ID, priceinvoice, aclog.UserName);
                }
            }
            //Hủy Hủy dịch vụ
            if (kq.ServiceStudent != 0)
            {
                ServicesStudentTable.delete(kq.CourseStudentID.Value, "Hủy phiếu thu", aclog.UserName);
            }
            //Hủy combo
            if (kq.ComboID != 0)
            {
                //cách 2 19.05.2020 by DK
                int totalcomborollbackprice = kq.Price.Value;
                int priceinvoice = kq.Price.Value;
                //nếu cái phiếu thu đó có cả appointmentID và combo => chuyển từ combo sang khóa => đó là tiền deposit
                if (kq.CourseStudentID != 0)
                {
                    tbl_CoursesStudent ct = CoursesStudentTable.getbyid(kq.CourseStudentID.Value);
                    if (ct != null)
                    {
                        ct = CoursesStudentTable.updatepriceleft(ct.ID, priceinvoice, aclog.UserName);
                    }

                }
                else if (kq.AppointmentID != 0)
                {
                    tbl_StudentAppointment appoint = StudentAppointmentTable.getbyid(kq.AppointmentID.Value);
                    if (appoint.isHide.Value)
                    {
                        tbl_CoursesStudent ct = CoursesStudentTable.getbystudentid(kq.StudentUID.Value).Where(n => n.AppointmentID == appoint.ID && n.ComboID == kq.ComboID).FirstOrDefault();
                        if (ct != null)
                        {
                            ct = CoursesStudentTable.updatepriceleft(ct.ID, priceinvoice, aclog.UserName);
                        }
                    }
                    else
                    {
                        appoint = StudentAppointmentTable.udpateDeposit(appoint.ID, priceinvoice, aclog.UserName);
                    }

                }
                else
                {
                    int priceinvoiceLeft = priceinvoice;
                    //lấy danh sách combodetail và order theo giá cao xuống thấp
                    List<tbl_ComboDetail> lcombodetail = ComboDetailTable.getbycomboid(kq.ComboID.Value).OrderByDescending(n => n.CoursePrice.Value).ToList();
                    foreach (var item in lcombodetail)//duyệt các khóa- lớp trong combo này
                    {
                        if (priceinvoice > 0)
                        {
                            if (item.TypeID == 1)//1 là id khóa, 2 là id lớp
                            {
                                tbl_CoursesStudent ct = CoursesStudentTable.getbyCourseIDdandStudentID(item.CourseID.Value, kq.StudentUID.Value);
                                //tiền khóa này đã đóng   
                                if (ct != null)
                                {
                                    //tiền khi đóng sẽ phân bổ từ khóa có tiền ít đến khóa có tiền nhiều 
                                    //=>khóa có tiền nhiều khả năng cao sẽ chưa đóng hoặc đóng ít
                                    //tính được số tiền còn lại khi phân bổ vào khóa này
                                    if (priceinvoice - (ct.TotalPaid.Value) <= 0)//chỉ phân bổ đủ cho khóa này, hoặc phân bổ hết cho khóa này => dừng duyệt
                                    {
                                        ct = CoursesStudentTable.updatepriceleft(ct.ID, priceinvoice, aclog.UserName);
                                        priceinvoice = 0;
                                        continue;
                                    }
                                    else
                                    {
                                        int previouspay = 0;
                                        List<tbl_Invoices> liv = InvoicesTable.getall().Where(n => n.CourseStudentID == ct.ID && n.isHide != true).ToList();
                                        if (liv.Count > 0)
                                        {
                                            previouspay = liv.Sum(n => n.Price.Value);
                                        }
                                        priceinvoice = priceinvoice - (ct.TotalPaid.Value - previouspay);
                                        ct = CoursesStudentTable.updatepriceleft(ct.ID, (ct.TotalPaid.Value - previouspay), aclog.UserName);
                                    }
                                    //hoàn lại tiền đã đóng cho 
                                }
                            }
                            else
                            {
                                tbl_StudentAppointment appoint = StudentAppointmentTable.getbycomboclass(item.ComboID.Value, item.CourseID.Value);
                                if (appoint.isHide.Value)
                                {
                                    var course = CoursesStudentTable.getbystudentid(appoint.StudentUID.Value).Where(n => n.ComboID == item.ComboID.Value && n.AppointmentID == appoint.ID).FirstOrDefault();
                                    if (course != null)
                                    {
                                        if (priceinvoice - (course.TotalPaid.Value) <= 0)//chỉ phân bổ đủ cho khóa này, hoặc phân bổ hết cho khóa này => dừng duyệt
                                        {
                                            course = CoursesStudentTable.updatepriceleft(course.ID, priceinvoice, aclog.UserName);
                                            priceinvoice = 0;
                                            continue;
                                        }
                                        else
                                        {
                                            //trừ tiền nó đóng riêng trong invoice
                                            int previouspay = 0;
                                            List<tbl_Invoices> liv = InvoicesTable.getall().Where(n => n.AppointmentID == appoint.ID && n.isHide != true).ToList();
                                            if (liv.Count > 0)
                                            {
                                                previouspay = liv.Sum(n => n.Price.Value);
                                            }
                                            priceinvoice = priceinvoice - (course.TotalPaid.Value - previouspay);
                                            course = CoursesStudentTable.updatepriceleft(course.ID, (course.TotalPaid.Value - previouspay), aclog.UserName);
                                        }
                                    }
                                }
                                else
                                {

                                    if (priceinvoice - (appoint.Deposit.Value) <= 0)
                                    {
                                        appoint = StudentAppointmentTable.udpateDeposit(appoint.ID, priceinvoice, aclog.UserName);
                                        priceinvoice = 0;
                                        continue;
                                    }
                                    else
                                    {

                                        int previouspay = 0;
                                        List<tbl_Invoices> liv = InvoicesTable.getall().Where(n => n.AppointmentID == appoint.ID && n.isHide != true).ToList();
                                        if (liv.Count > 0)
                                        {
                                            previouspay = liv.Sum(n => n.Price.Value);
                                        }
                                        priceinvoice = priceinvoice - (appoint.Deposit.Value - previouspay);
                                        appoint = StudentAppointmentTable.udpateDeposit(appoint.ID, appoint.Deposit.Value - previouspay, aclog.UserName);
                                    }

                                }
                            }
                        }

                    }
                }
                ComboTable.UpdatePriceLeft(kq.ComboID.Value, -totalcomborollbackprice, aclog.UserName);

            }
            //Hủy set
            if (kq.SetPackageStudentID != 0)
            {

            }
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Xóa phiếu chi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeletePhieuChi(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 1)
            {
                InvoicesTable.Delete(id, true);
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gửi phiếu thu qua mail cho học viên
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SendPhieuThuToStudent(int studentUID)
        {
            var p = AccountTable.getbyID(studentUID);
            if (p == null)
            {
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (string.IsNullOrEmpty(p.Email))
                {
                    return Json(new { rs = false, message = "Học viên chưa có mail!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    AppZim.ZIM.AssetCRM.SendMail(p.Email, "ASW - Phiếu thu", "");
                    return Json(new { rs = true, message = "Thành công" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpGet]
        public ActionResult InvoiceListDataTable()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            return View();
        }

        [HttpGet]
        public ActionResult LoadInvoiceListDataTable()
        {
            OjectDataTable dataTable = new OjectDataTable();
            if (Session["UID"] == null)
                return Json(dataTable, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
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
            List<get_list_of_invoice_Result> l = new List<get_list_of_invoice_Result>();
            //List<get_list_of_invoice_to_export_excel_Result> excelList = new List<get_list_of_invoice_to_export_excel_Result>();
            //using (var db = new ZimEntities())
            //{
            //    l = db.get_list_of_invoice(search, center, fromdate, todate, sort, pageSize, page - 1, aclog.ID).ToList();
            //}

            //List<get_list_of_CustomerDebts_DataTable_Result> l = new List<get_list_of_CustomerDebts_DataTable_Result>();
            //using (var db = new ZimEntities())
            //{
            //    l = db.get_list_of_CustomerDebts_DataTable(search, center, fromdateleft, todateleft, fromdate, todate, OrderBy, Length, Start, aclog.ID, aclog.RoleID.Value).ToList();
            //}
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
        [HttpPost, ValidateInput(false)]
        public ActionResult SendMailToStudent(FormCollection f)
        {
            try
            {
                var studentUID = f.Get("StudentUID").ToInt(0);
                var appointTestID = f.Get("AppointTestID").ToInt(0);
                var title = f.Get("Title").ToString();
                var content = f.Get("Content").ToString();
                AssetCRM.SendMail(AccountTable.getbyID(studentUID).Email, title, content);
                using (var db = new ZimEntities())
                {
                    var ck = db.tbl_StudentAppointmentTest.Where(x => x.ID == appointTestID).FirstOrDefault();
                    ck.isSendMail = true;
                    db.SaveChanges();
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

        public ActionResult ListDoneTest()
        {
            return View();
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult SendMailTestDone(FormCollection f)
        {
            try
            {
                var studentUID = f.Get("StudentUID-done").ToInt(0);
                var appointTestID = f.Get("AppointTestID-done").ToInt(0);
                var title = f.Get("txt-title-done").ToString();
                var content = f.Get("txt-introduction-done").ToString();
                AssetCRM.SendMail(AccountTable.getbyID(studentUID).Email, title, content);
                using (var db = new ZimEntities())
                {
                    var ck = db.tbl_StudentAppointmentTestDetail.Where(x => x.StudentAppointmentTestID == appointTestID).FirstOrDefault();
                    ck.IsSendMail = true;
                    db.SaveChanges();
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
        [HttpGet]
        public ActionResult ListAppointmentTestRefuse(int page = 1)
        {

            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });

            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                //page size
                int pageSize = 20;

                ////Master Khoa PUBG
                string search = "";
                ViewBag.Search = Request.QueryString["search"];
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                    search = Request.QueryString["search"].ToString().Trim();

                //Nguồn
                int source = -1;
                if (Request.QueryString["source"].ToInt(0) != 0)
                {
                    source = ViewBag.SourceID = Request.QueryString["source"].ToInt(0);
                }
                // lấy ngày đầu và ngày cuối tháng
                DateTime date = DateTime.Now;
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                string fromdate = firstDayOfMonth.ToString("yyyy-MM-dd");
                // thời gian bắt đầu từ
                if (!string.IsNullOrEmpty(Request.QueryString["from"]))
                {
                    try
                    {
                        DateTime fd = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                        fromdate = fd.ToString("yyyy-MM-dd");
                        ViewBag.From = fd.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                else
                {
                    ViewBag.From = firstDayOfMonth.ToString("dd/MM/yyyy");
                }
                // đến
                string todate = lastDayOfMonth.ToString("yyyy-MM-dd");
                if (!string.IsNullOrEmpty(Request.QueryString["to"]))
                {
                    try
                    {

                        DateTime td = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null);
                        todate = td.ToString("yyyy-MM-dd");
                        ViewBag.To = td.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                else
                {
                    ViewBag.To = lastDayOfMonth.ToString("dd/MM/yyyy");
                }
                var l = new List<GetListAppointmentTestRefuse_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.GetListAppointmentTestRefuse((page - 1) * pageSize, pageSize, search, source, fromdate, todate).ToList();
                }
                int total = 0;
                if (l.Count > 0)
                    total = l.Count;
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
            catch (Exception e)
            {
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }

        /// <summary>
        /// Lay danh sach ngay ma hv muon hoc
        /// </summary>
        [HttpPost]
        public ActionResult GetListWantToDay(int studentappointid)
        {
            var appointment = StudentAppointmentTable.getbyid(studentappointid);
            var day = appointment.ListWantToDay?.Split(',')?.FirstOrDefault();
            var desireDates = DesiredStudyTimeTable.getbyStudentAppointmentID(studentappointid);
            var cahoc = StudyTimeTable.getall().Where(x => x.isHide == false).OrderBy(x => x.sTime).ToList();
            StringBuilder html = new StringBuilder();
            for (var i = 0; i < desireDates.Count; i++)
            {
                html.Append("<div class=\"row mg-b-5\" id=\"numberTimeDesrire-"+ i +"\">");
                html.Append("   <div class=\"col-6\">");
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
                html.Append("   <div class=\"col-sm-1 form-group\" style=\"padding: 4px\">");
                html.Append("   <a href=\"javascript:;\" onclick=\"remove_div_timeDesrire("+i+")\" style=\"float:left;font-size:20px;color:red;\"><i class=\"far fa-times-circle\"></i>");
                html.Append("   </a>");
                html.Append("   </div>");
                html.Append("</div>");
            }
            if (desireDates.Count == 0)
            {
                for (var i = 0; i < 3; i++)
                {
                    html.Append("<div class=\"row mg-b-5\" id=\"numberTimeDesrire-" + i + "\">");
                    html.Append("   <div class=\"col-6\">");
                    html.Append("   <select name=\"slDay\" class=\"form-control select2\">");
                    for (var d = 0; d < 7; d++)
                    {
                        if (d == day?.ToInt(0))
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
                        if (s.ID == appointment.StudyID)
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
                    html.Append("   <div class=\"col-sm-1 form-group\" style=\"padding: 4px\">");
                    html.Append("   <a href=\"javascript:;\" onclick=\"remove_div_timeDesrire(" + i + ")\" style=\"float:left;font-size:20px;color:red;\"><i class=\"far fa-times-circle\"></i>");
                    html.Append("   </a>");
                    html.Append("   </div>");
                    html.Append("</div>");
                }
            }
            return Json(new { rs = true, data = html.ToString(), school = appointment.SchoolID, Lesson = appointment.Lesson, desireDates = desireDates.Count() }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// gửi thông báo lịch hẹn test cho học viên, giáo viên, tư vấn viên
        /// </summary>
        public void sendNotifyTest(tbl_StudentAppointmentTest test, tbl_Account ngtuvan, tbl_Account giaovien,
            tbl_Account aclog, string tt, string ct)
        {
            try
            {
                if (giaovien == null)
                {
                    giaovien = new tbl_Account() { ID = 0, FullName = "", ZoomID = "", ZoomPassword = "", Email = "" };
                }
                if (ngtuvan == null)
                {
                    ngtuvan = new tbl_Account() { ID = 0, FullName = "", Email = "" };
                }
                var title = "Thông tin buổi TEST";
                var content = "Bạn ơi, bạn vừa có thêm 1 lịch hẹn test. Vào để xem thông tin học viên ngay bạn nhé!";
                var nt = NotificationTable.insertsingle("", title, content, ngtuvan.ID, aclog.UserName, 0, 0);
                var gv = NotificationTable.insertsingle("", title, content, giaovien.ID, aclog.UserName, 0, 0);
                //lấy mẫu email
                var student = AccountTable.getbyID(test.StudentUID.Value);
                AssetCRM.SendMail(student.Email, tt, ct);
                using (var db = new ZimEntities())
                {
                    var ck = db.tbl_StudentAppointmentTest.Where(x => x.ID == test.ID).FirstOrDefault();
                    if (ck != null)
                    {
                        ck.isSendMail = true;
                        db.SaveChanges();
                    }
                }
                // gửi mail cho GV
                if (!string.IsNullOrEmpty(giaovien.Email))
                    AssetCRM.SendMail(giaovien.Email, gv.NotificationTitle, gv.NotificationContent);

                // thông báo qua app mobile
                var tvv = DeviceMobileTable.getbyUID(ngtuvan.ID)?.IncludePlayerID;
                PushNotifyMobile.SendNotifyMobile(content, new[] { tvv });
                var tea = DeviceMobileTable.getbyUID(giaovien.ID)?.IncludePlayerID;
                PushNotifyMobile.SendNotifyMobile("Bạn ơi, bạn vừa có thêm 1 lịch hẹn test. Vào để xem thông tin học viên ngay bạn nhé!", new[] { tea });
                var stu = DeviceMobileTable.getbyUID(test.StudentUID.Value)?.IncludePlayerID;
                PushNotifyMobile.SendNotifyMobile("Chúc mừng bạn đã hẹn lịch test thành công! Vào để xem thông tin buổi test ngay bạn nhé!", new[] { stu });
                // gửi mail cho TVV
                if (!string.IsNullOrEmpty(ngtuvan.Email))
                    AssetCRM.SendMail(ngtuvan.Email, nt.NotificationTitle, nt.NotificationContent);
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "sendNotifyTest", 0, e.Message + "||" + e.InnerException);
            }
        }

        public void sendNotifyTestDone(tbl_StudentAppointmentTest test, string tt, string ct)
        {
            try
            {
                var ac = AccountTable.getbyID(test.StudentUID.Value);
                if (!string.IsNullOrEmpty(ac.Email))
                {
                    AssetCRM.SendMail(AccountTable.getbyID(test.StudentUID.Value).Email, tt, ct);
                }
                StudentAppointmentTestTable.updateIsSendMail(test.ID);
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "sendNotifyTestDone", 0, e.Message + "||" + e.InnerException);
            }
        }

        public void sendNotifydaotao(tbl_StudentAppointmentTest test, string title, string content)
        {
            try
            {
                var ac = AccountTable.getbyID(test.StudentUID.Value);
                if (!string.IsNullOrEmpty(ac.Email))
                {
                    AssetCRM.SendMail(AccountTable.getbyID(test.StudentUID.Value).Email, title, content);
                }
                StudentAppointmentTestTable.updateIsSendMail(test.ID);
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "sendNotifydaotao", 0, e.Message + "||" + e.InnerException);
            }
        }

        [HttpPost]
        public ActionResult LoadClass(int language, int coefficient)
        {
            try
            {
                var classList = ClassTable.getall().Where(x => x.Language == language && x.isHide == false && (x.Coefficient == coefficient || coefficient == 0) && x.TypeClass != 3).ToList();
                if (classList.Count == 0)
                    return Json(new { rs = false });
                StringBuilder html = new StringBuilder();
                html.Append("<option>---</option>");
                foreach (var i in classList)
                {
                    html.Append("<option value=" + i.ID + " data-min=\"" + i.PriceClass + "\"  data-max=\"" + i.PriceClassInitial + "\" >" + i.ClassName + " : " + String.Format("{0:0,0}", i.PriceClass) + " - " + String.Format("{0:0,0}", i.PriceClassInitial) + " </option>");
                }
                return Json(new { rs = true, data = html.ToString() });
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "LoadClass", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false });
            }
        }
        [HttpPost]
        public ActionResult LoadInOutPut(int language)
        {
            try
            {
                //var account = AccountTable.getbyID(accountID);
                //if(account == null) return Json(new { rs = false });
                var academicLevels = AcademicLevelTable.getall().Where(x => x.Language == language).ToList();
                if (academicLevels.Count == 0)
                    return Json(new { rs = false });
                StringBuilder html = new StringBuilder();
                //StringBuilder input = new StringBuilder();
                //StringBuilder output = new StringBuilder();
                //html.Append("<option>---</option>");
                foreach (var i in academicLevels)
                {
                    //if (i.ID.ToString() == account.ScoreIn)
                    //{
                    //    input.Append("<option value=\"" + i.ID + "\" selected>" + i.LevelName + " - " + i.LanguageName + "</option>");
                    //}
                    //else
                    //{
                    //    input.Append("<option value=\"" + i.ID + "\">" + i.LevelName + " - " + i.LanguageName + "</option>");
                    //}

                    //if (i.ID.ToString() == account.ScoreOut)
                    //{
                    //    output.Append("<option value=\"" + i.ID + "\" selected>" + i.LevelName + " - " + i.LanguageName + "</option>");
                    //}
                    //else
                    //{
                    //    output.Append("<option value=\"" + i.ID + "\">" + i.LevelName + " - " + i.LanguageName + "</option>");
                    //}
                    html.Append("<option value=" + i.ID + ">" + i.LevelName + " - " + i.LanguageName + "</option>");
                }
                return Json(new { rs = true, data = html.ToString() }); ;
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "LoadClass", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false });
            }
        }
        [HttpPost]
        public ActionResult DeleteAppoitmentTest(int id)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3 && aclog.RoleID != 6)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                StudentAppointmentTestTable.updateIsHide(id);
                return Json(new { rs = true });
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "DeleteAppoitmentTest", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false });
            }
        }
        [HttpPost]
        public ActionResult LoadEmailTest(string fullname, int? language, int? nation, int? teacherid,
            string gioquocte, string giovn, string thoigianhoc)
        {
            try
            {
                var giaovien = AccountTable.getbyID(teacherid ?? 0);
                if (giaovien == null)
                {
                    giaovien = new tbl_Account() { ID = 0, FullName = "", ZoomID = "", ZoomPassword = "", Email = "" };
                }

                int m = 3;
                switch (language)
                {
                    case 7:
                        m = 17;
                        break;
                    case 6:
                        m = 10;
                        break;
                    default:
                        m = 3;
                        break;
                }
                var quocgia = new tbl_Nation() { Nation = "" };
                try
                {
                    quocgia = NationTable.GetByID(nation ?? 0);
                }
                catch { quocgia = new tbl_Nation() { Nation = "" }; }
                var email = EmailTemplateTable.GetByID(m);
                var tt = email.EmailTitle;
                var ct = email.EmailContent;
                ct = ct.Replace("[ten-hoc-vien]", fullname);
                ct = ct.Replace("[zoom-id]", giaovien.ZoomID);
                ct = ct.Replace("[ten-giang-vien]", giaovien.FullName);
                ct = ct.Replace("[zoom-password]", giaovien.ZoomPassword);
                ct = ct.Replace("[quoc-gia]", quocgia?.Nation);
                ct = ct.Replace("[gio-quoc-te]", gioquocte);
                ct = ct.Replace("[gio-vn]", giovn);
                ct = ct.Replace("[thoi-gian-hoc]", thoigianhoc + " - " + gioquocte);
                return Json(new { rs = true, tt, ct });
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "LoadEmailTest", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false });
            }
        }
        [HttpPost]
        public ActionResult LoadEmailDone(int id, string lesson, string time, string lotrinhhoc)
        {
            try
            {
                var test = StudentAppointmentTestTable.getbyid(id);
                var student = AccountTable.getbyID(test.StudentUID.Value);
                var detail = StudentAppointmentTestDetailTable.getbyAppointmentTestID(test.ID);
                var name = "";
                if (detail != null)
                {
                    try
                    {
                        var teacher = AccountTable.getbyID(detail.TeacherTestID.Value);
                        if (teacher != null)
                        {
                            name = teacher.FullName;
                        }
                    }
                    catch { }
                }

                var language = Language.GetByID(student.Language.Value);
                string lang = "";
                if (language != null)
                    lang = language.Name;
                //lấy mẫu email
                var email = EmailTemplateTable.GetByID(2);
                var tt = email.EmailTitle;
                var ct = email.EmailContent;
                ct = ct.Replace("[ten-hoc-vien]", test.StudentName);
                ct = ct.Replace("[ngon-ngu]", lang);
                ct = ct.Replace("[ten-giao-vien]", name);
                ct = ct.Replace("[nhan-xet]", detail == null ? "" : detail.Note);
                ct = ct.Replace("[link]", lotrinhhoc);
                ct = ct.Replace("[so-buoi-hoc]", lesson);
                ct = ct.Replace("[thoi-luong-hoc]", time);
                return Json(new { rs = true, tt, ct });
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "LoadEmailDone", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false });
            }
        }
        [HttpPost]
        public ActionResult LoadEmailDaoTao(int id, string input, string output)
        {
            try
            {
                var test = StudentAppointmentTestTable.getbyid(id);
                var ac = AccountTable.getbyID(test.StudentUID.Value);
                var idmail = 32;
                switch (ac.Language)
                {
                    case 7:
                        idmail = 34;
                        break;
                    case 6:
                        idmail = 33;
                        break;
                    default:
                        idmail = 32;
                        break;
                }
                var emailtemplate = EmailTemplateTable.GetByID(idmail);
                var contentMail = emailtemplate.EmailContent;
                contentMail = contentMail.Replace("[ngay]", DateTime.Now.Day.ToString());
                contentMail = contentMail.Replace("[thang]", DateTime.Now.Month.ToString());
                contentMail = contentMail.Replace("[nam]", DateTime.Now.Year.ToString());
                contentMail = contentMail.Replace("[ten-hoc-vien]", ac.FullName);
                contentMail = contentMail.Replace("[email]", ac.Email);
                contentMail = contentMail.Replace("[so-dien-thoai]", ac.Phone);
                contentMail = contentMail.Replace("[ngay-sinh]", ac.Birthday == null ? "" : ac.Birthday.Value.ToString("dd/MM/yyyy"));
                contentMail = contentMail.Replace("[noi-dung-dao-tao]", output);
                return Json(new { rs = true, title = emailtemplate.EmailTitle, content = contentMail });
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "LoadEmailDone", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false });
            }
        }
        [HttpPost]
        public ActionResult LoadEmailDangkyhoc(int language)
        {
            try
            {
                var lang = Language.GetByID(language);
                int id = 4;
                switch (lang.ID)
                {
                    case 7:
                        id = 18;
                        break;
                    case 6:
                        id = 11;
                        break;
                    default:
                        id = 4;
                        break;
                }
                //lấy mẫu email
                var email = EmailTemplateTable.GetByID(id);
                var tt = email.EmailTitle;
                var ct = email.EmailContent;
                return Json(new { rs = true, tt, ct });
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "LoadEmailDangkyhoc", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false });
            }
        }
        /// <summary>
        /// cập nhật doanh thu cho chiến dịch
        /// </summary>
        public static void UpdateCampaignSale(tbl_Account student, double revenue, int appoimentId)
        {
            try
            {
                // danh sách các ID mục tiêu cần update
                List<int> updateList = new List<int>();
                List<int> updateStatusList = new List<int>();
                //lấy các chiến dịch đang hoạt động ra
                var campains = CampaignSaleTable.getcampnow();
                foreach (var c in campains)
                {
                    updateList.Clear();
                    using (var db = new ZimEntities())
                    {
                        // lấy mục tiêu có managerID = ID tvv
                        var data = db.tbl_Target.Where(x => x.IsDone == false && x.IsHide == false
                        && x.CampaignID == c.ID && x.ManagerID == student.SupportUID.Value).OrderByDescending(x => x.TargetType).FirstOrDefault();
                        if (data != null)
                        {
                            updateList.Add(data.ID);
                            // lưu lịch sử
                            CampaignSaleHistoryTable.insert(data.ID, student.ID, appoimentId, "Auto");
                            do
                            {
                                var tempId = data.MainTargetID;
                                data = db.tbl_Target.Where(x => x.IsDone == false && x.IsHide == false
                                && x.CampaignID == c.ID && x.ID == tempId).FirstOrDefault();
                                if (data != null)
                                {
                                    updateList.Add(data.ID);
                                }
                            }
                            while (data != null);
                        }
                        else
                        {
                            var companyTarget = db.tbl_Target.Where(x => x.TargetType == 1 && x.IsDone == false
                            && x.IsHide == false && x.CampaignID == c.ID).FirstOrDefault();
                            if (companyTarget != null)
                            {
                                updateList.Add(companyTarget.ID);
                            }
                        }
                        updateList = updateList.Distinct().ToList();
                        // cập nhật doanh thu cho các mục tiêu
                        if (updateList.Count > 0)
                            db.Update_TargetRevenue(string.Join(",", updateList).ToString(), revenue);
                        updateStatusList.AddRange(updateList);
                    }
                }
                foreach (var u in updateStatusList)
                {
                    UpdateTargetStatus(u, revenue);
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("UpdateCampaignSale", "CampaignSale", 0, e.Message + " || " + e.InnerException + "params(" + appoimentId + "," + revenue + ")");
            }
        }

        /// <summary>
        /// Tính lại doanh thu để ra được data
        /// </summary>
        public static void UpLevel(tbl_Account student)
        {
            try
            {
                using (var db = new ZimEntities())
                {
                    
                    var seller = db.tbl_Account.Where(x => x.ID == student.SupportUID && x.Status == 2).FirstOrDefault();
                    if (seller == null)
                        return;
                    //Kiểm tra tháng này tvv có bị tuột rank thì sẽ ko được xét lên rank
                    List<Admin_GetLevelDownInMonth_Result> checkDownRank = db.Admin_GetLevelDownInMonth(DateTime.Now.Month,1,seller.ID).ToList();
                    if (checkDownRank.Any())
                        return;
                    var exists = db.tbl_ApprovalToLevelUp.Where(n => n.UID == seller.ID && n.Status == 0).FirstOrDefault();
                    if (exists != null)
                        return;

                    List<int> studentIds = db.tbl_Account.Where(x => x.SupportUID == student.SupportUID).Select(x => x.ID).ToList();
                    var revenue = db.Admin_GetRevenueBySellerId(seller.ID).FirstOrDefault() ?? 0;
                    var levels = db.tbl_SalarySetting.Where(x => x.isHide == false).OrderByDescending(x => x.Million).ToList();
                    int newPosition = seller.Position ?? 0;
                    revenue += seller.Revenue ?? 0;
                    for (int i = 0; i < levels.Count; i++)
                    {
                        if (levels[i].Million > revenue)
                            continue;

                        newPosition = levels[i].ID;
                    }

                    if (newPosition == seller.Position || newPosition == 0)
                        return;

                    tbl_ApprovalToLevelUp ck = new tbl_ApprovalToLevelUp();
                    ck.UID = seller.ID;
                    ck.Revenue = revenue;
                    ck.CurrentRank = newPosition;
                    ck.OldRank = seller.Position ?? 0;
                    ck.Status = 0;
                    ck.Note = "";
                    ck.CreatedBy = "Auto";
                    ck.CreatedDate = DateTime.Now;
                    ck.IsSendMail = false;
                    db.tbl_ApprovalToLevelUp.Add(ck);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("UpdateCampaignSale", "UpLevel", 0, e.Message + " || " + e.InnerException + "||" + student.ID);
            }
        }


        /// <summary>
        /// Tính lại doanh thu để ra được data
        /// </summary>
        /// Vẫn còn đang làm
        public static void DownLevel(tbl_Account student)
        {
            try
            {
                using (var db = new ZimEntities())
                {
                    var seller = db.tbl_Account.Where(x => x.ID == student.SupportUID && x.Status == 2).FirstOrDefault();
                    if (seller == null)
                        return;

                    var exists = db.tbl_ApprovalToLevelUp.Where(n => n.UID == seller.ID && n.Status == 0).FirstOrDefault();
                    if (exists != null)
                        return;

                    List<int> studentIds = db.tbl_Account.Where(x => x.SupportUID == student.SupportUID).Select(x => x.ID).ToList();
                    var revenue = db.Admin_GetRevenueBySellerId(seller.ID).FirstOrDefault() ?? 0;
                    var levels = db.tbl_SalarySetting.Where(x => x.isHide == false).OrderByDescending(x => x.Million).ToList();
                    int newPosition = seller.Position ?? 0;
                    revenue += seller.Revenue ?? 0;
                    for (int i = 0; i < levels.Count; i++)
                    {
                        if (levels[i].Million < revenue)
                            continue;

                        newPosition = levels[i-1].ID;
                    }

                    if (newPosition == seller.Position || newPosition == 0)
                        return;

                    tbl_ApprovalToLevelDown ck = new tbl_ApprovalToLevelDown();
                    ck.UID = seller.ID;
                    ck.Revenue = revenue;
                    ck.CurrentRank = newPosition;
                    ck.Status = 0;
                    ck.Note = "";
                    ck.CreatedBy = "Auto";
                    ck.CreatedDate = DateTime.Now;
                    ck.IsSendMail = false;
                    db.tbl_ApprovalToLevelDown.Add(ck);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("UpdateCampaignSale", "DownLevel", 0, e.Message + " || " + e.InnerException + "||" + student.ID);
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
        public ActionResult GetTestHistory(string phone)
        {
            try
            {
                var student = AccountTable.getbyusername(phone);
                int id = 0;
                if (student != null)
                    id = student.ID;
                var appoinmentTests = StudentAppointmentTestTable.getbystudentid(id);
                StringBuilder html = new StringBuilder();
                foreach (var i in appoinmentTests)
                {
                    html.Append("<div class=\"col-sm-3 form-group\">");
                    html.Append("<label class=\"wd-100p\">Ngày hẹn</label>");
                    html.Append("<div>" + i.DateTest == null ? "" : i.DateTest.Value.ToString("dd/MM/yyyy") + "</div>");
                    html.Append("</div>");
                    html.Append("<div class=\"col-sm-3 form-group\">");
                    html.Append("<label>Giờ hẹn</label>");
                    html.Append("<div>" + i.TimeTest + "</div>");
                    html.Append("</div>");
                    html.Append("<div class=\"col-sm-3 form-group\">");
                    html.Append("<label>Ghi chú</label>");
                    html.Append("<div>" + i.Note + "</div>");
                    html.Append("</div>");
                    html.Append("<div class=\"col-sm-3 form-group\">");
                    html.Append("<label>Người tạo</label>");
                    html.Append("<div>" + i.CreateTestName + "</div>");
                    html.Append("</div>");
                }
                return Json(new { data = html.ToString() }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
    public class StudentRefuse
    {
        public string FullName { get; set; }
        public string SourceName { get; set; }
        public string CreateTestName { get; set; }
        public string SchoolName { get; set; }
        public string TimeTest { get; set; }
        public string ReasonName { get; set; }
    }
}