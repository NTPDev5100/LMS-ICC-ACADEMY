using AppZim.Areas.Admin.Controllers;
using AppZim.Models;
using AppZim.TableSql;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AppZim.Areas.Admin.ControllersApi
{
    [Route("api/AccountApi/{action}", Name = "AccountApi")]
    public class AccountApiController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage getstaff(DataSourceLoadOptions loadOptions)// saler
        {
            var l = new ZimEntities();
            var queryParams = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
            if (queryParams.ContainsKey("uid"))
            {
                int id = queryParams["uid"].ToInt(0);
                var aclog = AccountTable.getbyID(id);
                if (aclog.RoleID == 1)//saler
                {
                    var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Account.Where(n => n.RoleID == 1 || n.RoleID == 2 || n.RoleID == 3 || n.RoleID == 7 || n.RoleID == 8).OrderByDescending(n => n.ID).ToList(), loadOptions));
                    return s;
                }
                else
                {
                    var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Account.Where(n => n.RoleID == 2 || n.RoleID == 3 || n.RoleID == 7 || n.RoleID == 8).OrderByDescending(n => n.ID).ToList(), loadOptions));
                    return s;
                }
            }
            return null;
        }
        [HttpGet]
        public HttpResponseMessage gettearcher(DataSourceLoadOptions loadOptions)// giáo viên
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Account.Where(n => n.RoleID == 4).OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        #region học viên
        [HttpGet]
        public HttpResponseMessage getstudent(DataSourceLoadOptions loadOptions)// học viên
        {
            var l = new ZimEntities();
            var queryParams = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
            if (queryParams.ContainsKey("uid"))
            {
                int id = queryParams["uid"].ToInt(0);
                var aclog = AccountTable.getbyID(id);
                if (aclog.RoleID == 6)//saler
                {
                    var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Account.Where(n => n.RoleID == 5 && n.SupportUID == id).OrderByDescending(n => n.ID).ToList(), loadOptions));
                    return s;
                }
                else
                {
                    var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Account.Where(n => n.RoleID == 5).OrderByDescending(n => n.ID).ToList(), loadOptions));
                    return s;
                }
            }
            else
            {
                var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Account.Where(n => n.RoleID == 5).OrderByDescending(n => n.ID).ToList(), loadOptions));
                return s;
            }
            //var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Account.Where(n => n.RoleID == 5).OrderByDescending(n => n.ID).ToList(), loadOptions));
            //return s;
        }

        [HttpGet]
        public HttpResponseMessage getstudentdebt(DataSourceLoadOptions loadOptions)// học viên nợ học phí
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_CoursesStudent.Where(n => n.PriceLeft > 0).OrderByDescending(n => n.CreatedDate).ToList(), loadOptions));
            return s;
        }

        [HttpGet]
        public HttpResponseMessage getstudentappointment(DataSourceLoadOptions loadOptions)// học viên hẹn đăng kí
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_StudentAppointment.Where(n => n.isHide != true && n.CourseStudentID == 0).OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        [HttpGet]
        public HttpResponseMessage getstudentreserve(DataSourceLoadOptions loadOptions)// học viên bảo lưu
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_StudentAppointment.Where(n => n.isHide != true && n.CourseStudentID != 0).OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        [HttpGet]
        public HttpResponseMessage getstudentappointmenttest(DataSourceLoadOptions loadOptions)// học viên hẹn test
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_StudentAppointmentTest.OrderBy(n => n.CreatedDate).ToList(), loadOptions));
            return s;
        }

        [HttpGet]
        public HttpResponseMessage getstudentincourse(DataSourceLoadOptions loadOptions)// học viên hẹn đăng kí
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_CoursesStudent.Where(n => n.isHide != true).OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }
        #endregion

        [HttpGet]
        public HttpResponseMessage getsaler(DataSourceLoadOptions loadOptions)// saler
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Account.Where(n => n.RoleID == 6).OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        [HttpGet]
        public HttpResponseMessage getservicestudent(DataSourceLoadOptions loadOptions)// danh sách khách sử dụng dịch vụ
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_ServicesStudent.OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        // 
        [HttpGet]
        public HttpResponseMessage getcustomerwarning(DataSourceLoadOptions loadOptions)// lấy học viên bị cảnh baos
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_CoursesAttendanceStudent.Where(n => n.Warning == true).OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        [HttpGet]
        public HttpResponseMessage getcustomersupportoff(DataSourceLoadOptions loadOptions)// // lấy học viên có tư vấn viên nghỉ
        {
            var l = new ZimEntities();
            HttpResponseMessage s = new HttpResponseMessage();
            List<tbl_Account> data = new List<tbl_Account>();
            IEnumerable<tbl_Account> sps = l.tbl_Account.Where(n => n.RoleID == 6 && n.Status != 2).ToList();
            IEnumerable<tbl_Account> cus = l.tbl_Account.Where(n => n.RoleID == 5).ToList();
            var exs = from cu in cus
                      join sp in sps on cu.SupportUID equals sp.ID
                      select cu;
            data.AddRange(exs);
            s = Request.CreateResponse(DataSourceLoader.Load(data.ToList(), loadOptions));
            return s;
        }
        // lấy học viên chuyển lớp
        [HttpGet]
        public HttpResponseMessage getcustomerchangecourse(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            HttpResponseMessage s = new HttpResponseMessage();
            List<tbl_CoursesStudent> data = new List<tbl_CoursesStudent>();
            IEnumerable<tbl_CoursesStudent> hocvientrongkhoa = l.tbl_CoursesStudent.ToList();
            IEnumerable<tbl_CoursesStudentHistoryChange> hocvienchuyenkhoa = l.tbl_CoursesStudentHistoryChange.ToList();
            var css = from hocvien in hocvientrongkhoa
                      join chuyen in hocvienchuyenkhoa on hocvien.ID equals chuyen.CoursesStudentID
                      select hocvien;
            data.AddRange(css);
            s = Request.CreateResponse(DataSourceLoader.Load(data.ToList(), loadOptions));
            return s;
        }


        /// <summary>
        /// API Web vệ tinh
        /// </summary>
        public class StatusApi
        {
            public int Code { get; set; }
            public string Message { get; set; }
        }
        #region API ielts-share
        [HttpGet]
        public StatusApi insertdata(string fullname, string phone, string email, string address, string birthday, string identitycard, int cityid, string sourceweb)// insert tài khoản học viên
        {
            tbl_District district = new tbl_District();
            district.ID = 0;
            district.DistrictName = "";
            district.LocationID = 0;
            district.LocationName = "";

            tbl_Ward w = new tbl_Ward();
            w.ID = 0;
            w.WardName = "";
            tbl_Street s = new tbl_Street();
            s.ID = 0;
            s.StreetName = "";

            string bd = "";
            try
            {
                DateTime d = DateTime.ParseExact(birthday, "dd/MM/yyyy", null);
                bd = d.ToString("dd/MM/yyyy");
            }
            catch { }
            var city = LocationTable.getbyID(cityid);
            if (city != null)
            {
                district.LocationID = city.ID;
                district.LocationName = city.CityName;
            }

            //var rs = AccountTable.insertcustomer(district, fullname, email, phone, address, 1, "", 0, 100, "ielts-share.com", bd, 0, "", "", 0, "", "", "", "", "", identitycard, 0, "", "", "ielts-share.com", "api");
            var rs = AccountTable.insertcustomer(district, fullname, email, phone, address, 1, "", 0, 1007, sourceweb, bd, 0,
                "", "", 0, "", "", "", "", "", identitycard, 0, "", "", "", "api", false, w, s, "", 0, "", 0);
            StatusApi t = new StatusApi();
            if (rs != null)
            {
                t.Code = 200;
                t.Message = "success";
            }
            else
            {
                t.Code = 201;
                t.Message = "fail";
            }
            return t;
        }
        /// <summary>
        /// Danh sách thành phố
        /// </summary>
        public class CityApi
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }
        [HttpGet]
        public List<CityApi> getcity()
        {
            using (var db = new ZimEntities())
            {
                List<CityApi> l = new List<CityApi>();
                var citys = db.tbl_City.OrderBy(n => n.CityName).ToList();
                foreach (var item in citys)
                {
                    CityApi t = new CityApi();
                    t.ID = item.ID;
                    t.Name = item.CityName;
                    l.Add(t);
                }
                return l;
            }
        }
        #endregion

        #region API zim.vn
        /// <summary>
        /// lấy địa điểm có tổ chức thi
        /// </summary>
        /// <returns></returns>
        public List<CityApi> getloactioncenter()
        {
            List<CityApi> l = new List<CityApi>();
            IEnumerable<tbl_School> center = SchoolTable.getall().Where(n => n.isHide != true).ToList();
            IEnumerable<tbl_ServiceExams> exams = ServiceExamsTable.getall().Where(n => n.ExamDate > GetDateTime.Now.Date && n.SchoolID != null).OrderBy(n => n.ExamDate).ToList();

            center = from c in center
                     join e in exams on c.ID equals e.SchoolID
                     select c;

            var centers = center.Select(n => n.CityID).Distinct();
            foreach (var item in centers)
            {
                var city = LocationTable.getbyID(item.Value);
                if (city != null)
                {
                    CityApi t = new CityApi();
                    t.ID = city.ID;
                    t.Name = city.CityName;
                    l.Add(t);
                }
            }
            return l;
        }
        /// <summary>
        /// Danh sách trung tâm
        /// </summary>
        public class SchoolApi
        {
            public int ID { get; set; }
            public int CityID { get; set; }
            public string Center { get; set; }
        }
        [HttpGet]
        public List<SchoolApi> getcenter()
        {
            List<SchoolApi> l = new List<SchoolApi>();
            var center = SchoolTable.getall().Where(n => n.isHide == false).OrderBy(n => n.SchoolName).ToList();
            var exams = ServiceExamsTable.getall().Where(n => n.ExamDate > GetDateTime.Now.Date && n.SchoolID != null).OrderBy(n => n.ExamDate).ToList();
            foreach (var item in center)
            {
                var ck = exams.Where(n => n.SchoolID == item.ID).ToList();
                if (ck.Count == 0)
                    continue;

                SchoolApi t = new SchoolApi();
                t.ID = item.ID;
                t.CityID = item.CityID.Value;
                t.Center = item.SchoolName;
                l.Add(t);
            }
            return l;
        }
        /// <summary>
        /// Danh sách khóa thi
        /// </summary>
        public class ExamsApi
        {
            public int CenterID { get; set; }
            public string CenterName { get; set; }
            public int ExamID { get; set; }
            public string ExamName { get; set; }
            public string ExamDate { get; set; }
            public int Price { get; set; }
            public int Slot { get; set; }
            public bool ExamReal { get; set; }
        }
        [HttpGet]
        public List<ExamsApi> getexam()
        {
            List<ExamsApi> l = new List<ExamsApi>();
            var exams = ServiceExamsTable.getall().Where(n => n.ExamDate > GetDateTime.Now.Date && n.SchoolID != null).OrderBy(n => n.ExamDate).ThenBy(n => n.Hour).ToList();
            foreach (var item in exams)
            {
                ExamsApi t = new ExamsApi();
                t.CenterID = item.SchoolID.Value;
                t.CenterName = SchoolTable.getbyID(item.SchoolID.Value).SchoolName;
                t.ExamID = item.ID;
                t.ExamName = item.ExamService.Trim() + " " + item.ExamDate.Value.ToString("dd/MM/yyyy") + " " + item.Hour;
                t.ExamDate = item.ExamDate.Value.ToString("dd/MM/yyyy");
                var d = ServicesStudentTable.getbyserviceexamsid(item.ID);
                if (item.Slot == null)
                    continue;
                t.Price = item.ExamPrice.Value;
                t.Slot = item.Slot.Value - d.Count;
                t.ExamReal = false;
                if (item.ExamReal != null)
                    t.ExamReal = item.ExamReal.Value;
                if (t.Slot > 0)
                    l.Add(t);
            }
            return l;
        }
        public class StatusRegister
        {
            public int Code { get; set; }
            public string SBD { get; set; }
            public string Message { get; set; }
        }
        [HttpPost]
        public StatusRegister registerexams(string key, string fullname, string phone, string email, string address, int cityid, string birthday, int sourceweb, string identitycard, string typeacademic, int examid)
        {
            StatusRegister t = new StatusRegister();
            t.SBD = "";
            if (key != "SHkgxqFpIEh5IMOgLCDEkcOieSBsw6AgQVBJIG5ow6k=")
            {
                t.Code = 201;
                t.Message = "fail";
            }
            tbl_District district = new tbl_District();
            district.ID = 0;
            district.DistrictName = "";
            district.LocationID = 0;
            district.LocationName = "";

            string bd = "";
            try
            {
                DateTime d = DateTime.ParseExact(birthday, "dd/MM/yyyy", null);
                bd = d.ToString("dd/MM/yyyy");
            }
            catch { }
            var city = LocationTable.getbyID(cityid);
            if (city != null)
            {
                district.LocationID = city.ID;
                district.LocationName = city.CityName;
            }

            tbl_SourceOfCustomer sourse = new tbl_SourceOfCustomer();
            sourse = SourceOfCustomerTable.getbyID(sourceweb);
            if (sourse == null)
            {
                sourse = new tbl_SourceOfCustomer();
                sourse.ID = 0;
                sourse.SourceOfCustomer = "";
            }
            tbl_Ward w = new tbl_Ward();
            w.ID = 0;
            w.WardName = "";
            tbl_Street s = new tbl_Street();
            s.ID = 0;
            s.StreetName = "";
            var rs = AccountTable.insertcustomer(district, fullname, email, phone, address, 1, "", 0, sourse.ID, sourse.SourceOfCustomer, bd, 0,
                "", "", 0, "", "", "", "", typeacademic, identitycard, 0, "", "", "", "api", false, w, s, "", 0, "", 0);

            if (rs != null)
            {
                var sve = ServiceExamsTable.getbyID(examid);
                if (sve != null)
                {

                    //check slot
                    int slot = sve.Slot.Value;
                    List<tbl_ServicesStudent> lss = ServicesStudentTable.getbyserviceexamsid(sve.ID);
                    int dscount = lss.Count();
                    if (dscount >= slot)
                    {
                        t.Code = 201;
                        t.Message = "fail";
                    }
                    else
                    {
                        int nextstudent = dscount + 1;
                        string mssv = "";
                        if (nextstudent < 10)
                        {
                            mssv = "0" + nextstudent;
                        }
                        else
                        {
                            mssv = nextstudent.ToString();
                        }
                        string SBD = "Z-" + sve.SchoolID.ToString() + mssv;
                        int paymentid = 2;//thanh toán online
                        string paymentname = CashierController.Paymentmethod(paymentid);
                        tbl_SupplierServices supp = SupplierServicesTable.getbyID(sve.SupplierID.Value);
                        tbl_Services sv = new tbl_Services();
                        sv.ID = 0;
                        sv.ServiceName = "";
                        var svstudent = ServicesStudentTable.insert(rs.ID, rs.FullName, sv, sve.ExamPrice.Value, "", paymentid, paymentname, supp, sve.ExamDate.Value.ToString("dd/MM/yyyy"), rs.UserName, sve.ID, 0);
                        if (svstudent != null)// tạo hóa đơn
                        {
                            //Hóa đơn
                            if (sve.ExamPrice > 0)
                            {
                                tbl_School choolinvoice = new tbl_School();
                                choolinvoice.ID = 0;
                                choolinvoice.SchoolName = "";

                                string name = "2-" + rs.ID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";//2 mua dịch vụ
                                string nameencode = AppZim.ZIM.Encryptor.EncryptURL(name);

                                string datalink = "https://app.zim.vn/invoice/?code=" + nameencode;
                                var barcode = AppZim.ZIM.AssetCRM.CreateQRCode(datalink, name);
                                var iv = InvoicesTable.insert(0, false, 0, 0, rs.ID, 0, 0, rs, sve.ExamPrice.Value, 0, sve.ExamService, "", barcode, nameencode, rs, "", paymentid, paymentname, choolinvoice, 0);

                                //đẩy thông báo cho admin
                                var ladmin = AccountTable.getbyrole(1).Where(n => n.Status == 2 && n.ID == 1).ToList();
                                foreach (var item in ladmin)
                                {
                                    string title = "[" + choolinvoice.SchoolName + "] - Phiếu thu " + String.Format("{0:0,0 VNĐ}", iv.Price) + "";

                                    string content = "Phát sinh phiếu thu có ID = \"" + iv.ID + "\"XEM TẠI ĐÂY";
                                    var nt = NotificationTable.insertsingle("", title, content, item.ID, rs.UserName, 9, 0);
                                    // gửi mail
                                    if (!string.IsNullOrEmpty(item.Email) && item.ID == 1)
                                        AppZim.ZIM.AssetCRM.SendMail(item.Email, nt.NotificationTitle, nt.NotificationContent);
                                }
                                //==============>>>>                            
                                t.Code = 200;
                                t.SBD = SBD;
                                t.Message = datalink;
                            }
                        }
                        else
                        {
                            t.Code = 201;
                            t.Message = "fail";
                        }
                    }
                }
                else
                {
                    t.Code = 201;
                    t.Message = "fail";
                }
            }
            else
            {
                t.Code = 201;
                t.Message = "fail";
            }
            return t;
        }
        [HttpPost]
        public StatusApi insertadvisory(string key, string fullname, string phone, string email, string address, int cityid, string birthday, int sourceweb, string identitycard)// insert tài khoản học viên
        {
            StatusApi t = new StatusApi();
            if (key != "SHkgxqFpIEh5IMOgLCDEkcOieSBsw6AgQVBJIG5ow6k=")
            {
                t.Code = 201;
                t.Message = "fail";
            }
            tbl_District district = new tbl_District();
            district.ID = 0;
            district.DistrictName = "";
            district.LocationID = 0;
            district.LocationName = "";

            string bd = "";
            try
            {
                DateTime d = DateTime.ParseExact(birthday, "dd/MM/yyyy", null);
                bd = d.ToString("dd/MM/yyyy");
            }
            catch { }
            var city = LocationTable.getbyID(cityid);
            if (city != null)
            {
                district.LocationID = city.ID;
                district.LocationName = city.CityName;
            }

            tbl_SourceOfCustomer sourse = new tbl_SourceOfCustomer();
            sourse = SourceOfCustomerTable.getbyID(sourceweb);
            if (sourse == null)
            {
                sourse = new tbl_SourceOfCustomer();
                sourse.ID = 0;
                sourse.SourceOfCustomer = "";
            }
            tbl_Ward w = new tbl_Ward();
            w.ID = 0;
            w.WardName = "";
            tbl_Street s = new tbl_Street();
            s.ID = 0;
            s.StreetName = "";
            var rs = AccountTable.insertcustomer(district, fullname, email, phone, address, 1, "", 0, sourse.ID, sourse.SourceOfCustomer, bd,
                0, "", "", 0, "", "", "", "", "", identitycard, 0, "", "", "", "api", true, w, s, "", 0, "", 0);
            if (rs != null)
            {
                //Luue lại lịch sử cần tư vấn
                AccountHistoryAdvisoryTable.insert(rs.ID, "Đăng ký tư vấn từ: " + sourceweb + "", rs.UserName);
                t.Code = 200;
                t.Message = "success";
            }
            else
            {
                t.Code = 201;
                t.Message = "fail";
            }
            return t;
        }

        [HttpPost]
        public StatusApi InsertAdvisory(string key, string fullname, string phone, string email, string noidung)// insert học viên cần tư vấn
        {
            StatusApi t = new StatusApi();
            if (key != "SHkgxqFpIEh5IMOgLCDEkcOieSBsw6AgQVBJIG5ow6k=")
            {
                t.Code = 201;
                t.Message = "fail";
            }
            var rs = AccountAdvisoryTable.InsertCanTuVan(fullname, phone, email, noidung);
            if (rs != null)
            {
                t.Code = 200;
                t.Message = "success";
            }
            else
            {
                t.Code = 201;
                t.Message = "fail";
            }
            return t;
        }
        #endregion
    }
}
