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
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AppZim.Areas.Admin.Controllers
{
    public class OptionController : BaseController
    {
        #region danh sách tỉnh thành đặt trung tâm
        public ActionResult Location(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            IEnumerable<tbl_City> l = LocationTable.getall();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from s in l
                    where AssetCRM.RemoveUnicode(s.CityName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select s;
            }
            l = l.OrderBy(n => n.CityName).ToList();
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddCity(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            string city = f.Get("txt-city").ToString();
            var rs = LocationTable.insert(city, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("Location", "Option");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditCity(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int ID = f.Get("hdfID").ToInt(0);
            string city = f.Get("txt-edit-city").ToString();
            var rs = LocationTable.update(ID, city, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("Location", "Option");
        }
        #endregion

        #region danh sách quận huyện
        public ActionResult District(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            IEnumerable<tbl_District> l = DistrictTable.getall().OrderByDescending(n => n.ID).ToList();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from s in l
                    where AssetCRM.RemoveUnicode(s.LocationName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(s.DistrictName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select s;
            }
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);
                if (sort == 1)
                    l = l.OrderBy(n => n.LocationName).ToList();
                if (sort == 2)
                    l = l.OrderByDescending(n => n.LocationName).ToList();
                if (sort == 3)
                    l = l.OrderBy(n => n.DistrictName).ToList();
                if (sort == 4)
                    l = l.OrderByDescending(n => n.DistrictName).ToList();
            }
            ViewBag.Sort = sort;
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddDistrict(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            int ctyid = f.Get("ddl-city").ToInt(0);
            var cty = LocationTable.getbyID(ctyid);
            string district = f.Get("txt-district").ToString();
            var rs = DistrictTable.insert(cty.ID, cty.CityName, district, aclog.UserName, 0);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("District", "Option");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditDistrict(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            int ID = f.Get("hdfID").ToInt(0);
            int ctyid = f.Get("ddl-edit-city").ToInt(0);
            var cty = LocationTable.getbyID(ctyid);
            string district = f.Get("txt-edit-district").ToString();
            var rs = DistrictTable.update(ID, cty.ID, cty.CityName, district, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("District", "Option");
        }
        #endregion

        #region danh sách trung tâm
        public ActionResult Center(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            IEnumerable<tbl_School> l = SchoolTable.getall();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from s in l
                    where AssetCRM.RemoveUnicode(s.SchoolName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(s.SchoolAddress).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(s.DistrictName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(s.SchoolPhone).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select s;
            }
            l = l.OrderBy(n => n.isHide).ThenBy(n => n.SchoolName).ToList();
            return View(l.ToPagedList(page, pageSize));
        }

        [HttpPost]
        public ActionResult AddCenter(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });


            var district = DistrictTable.getbyID(f.Get("ddl-district").ToInt(0));
            var school = f.Get("txt-center").ToString();
            var color = f.Get("txt-color").ToString();
            var phone = f.Get("txt-phone").ToString();
            var address = f.Get("txt-address").ToString();

            var rs = SchoolTable.insert(school, color, phone, address, district, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("Center", "Option");
        }

        [HttpPost]
        public ActionResult EditCenter(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            int ID = f.Get("hdfID").ToInt(0);
            bool ishide = false;
            if (!string.IsNullOrEmpty(f.Get("chk-hide")))
                ishide = true;
            var district = DistrictTable.getbyID(f.Get("ddl-edit-district").ToInt(0));
            var school = f.Get("txt-edit-center").ToString();
            var color = f.Get("txt-edit-color").ToString();
            var phone = f.Get("txt-edit-phone").ToString();
            var address = f.Get("txt-edit-address").ToString();

            var rs = SchoolTable.update(ID, school, color, phone, address, district.ID, district.DistrictName, ishide, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("Center", "Option");
        }
        #endregion

        #region danh sách phòng trung tâm
        //public ActionResult RoomCenter(int id, int page = 1)
        //{
        //    if (Session["UID"] == null)
        //        return RedirectToAction("Signin", "Login", new { area = "" });
        //    var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
        //    if (aclog.RoleID == 5)// học viên bị đá ra
        //        return RedirectToAction("Signin", "Login", new { area = "" });

        //    int pageSize = 20;
        //    var school = SchoolTable.getbyID(id);
        //    if (school == null)
        //    {
        //        SetAlert("Không tìm thấy cơ sở này", "e");
        //        return RedirectToAction("Center", "Option");
        //    }

        //    IEnumerable<tbl_SchoolRoom> l = SchoolRoomTable.getbyschoolid(school.ID);
        //    if (!string.IsNullOrEmpty(Request.QueryString["search"]))
        //    {
        //        string search = ViewBag.Search = Request.QueryString["search"].ToString();
        //        l = from r in l
        //            where AssetCRM.RemoveUnicode(r.RoomName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
        //            || AssetCRM.RemoveUnicode(r.RoomDescription).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
        //            select r;
        //    }
        //    ViewBag.SchoolID = school.ID;
        //    ViewBag.Total = l.Count();
        //    return View(l.ToPagedList(page, pageSize));
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult AddRoomCenter(FormCollection f)
        //{
        //    if (Session["UID"] == null)
        //        return RedirectToAction("Signin", "Login", new { area = "" });
        //    var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
        //    if (aclog.RoleID == 5)// học viên bị đá ra
        //        return RedirectToAction("Signin", "Login", new { area = "" });

        //    var school = SchoolTable.getbyID(f.Get("hdfSchoolID").ToInt(0));
        //    if (school == null)
        //    {
        //        SetAlert("Không tìm thấy cơ sở này", "e");
        //        return RedirectToAction("Center", "Option");
        //    }
        //    string room = f.Get("txt-room").ToString();
        //    string roomdes = f.Get("txt-room-des").ToString();

        //    var rs = SchoolRoomTable.insert(room, school.ID, school.DistrictName, roomdes, aclog.UserName);
        //    if (rs != null)
        //        SetAlert("Thành công", "s");
        //    else
        //        SetAlert("Không thành công", "e");
        //    return RedirectToAction("RoomCenter", "Option", new { id = school.ID });
        //}
        //[HttpPost]
        //public ActionResult EditRoomCenter(FormCollection f)
        //{
        //    if (Session["UID"] == null)
        //        return RedirectToAction("Signin", "Login", new { area = "" });
        //    var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
        //    if (aclog.RoleID == 5)// học viên bị đá ra
        //        return RedirectToAction("Signin", "Login", new { area = "" });

        //    var ID = f.Get("hdfID").ToInt(0);
        //    var school = SchoolTable.getbyID(f.Get("hdfSchoolID").ToInt(0));
        //    if (school == null)
        //    {
        //        SetAlert("Không tìm thấy cơ sở này", "e");
        //        return RedirectToAction("Center", "Option");
        //    }
        //    string room = f.Get("txt-edit-room").ToString();
        //    string roomdes = f.Get("txt-edit-room-des").ToString();

        //    var rs = SchoolRoomTable.update(ID, room, roomdes, school.ID, school.DistrictName, aclog.UserName);
        //    if (rs != null)
        //        SetAlert("Thành công", "s");
        //    else
        //        SetAlert("Không thành công", "e");
        //    return RedirectToAction("RoomCenter", "Option", new { id = school.ID });
        //}
        #endregion

        #region khối học
        public ActionResult Grade(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //page size
            int pageSize = 20;
            IEnumerable<tbl_Grade> l = GradeTable.getall().OrderBy(n => n.IsHidden).ThenByDescending(n => n.ID).ToList();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                string search = ViewBag.Search = Request.QueryString["search"].ToString();
                l = from r in l
                    where AssetCRM.RemoveUnicode(r.GradeName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(r.Description).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select r;
            }
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddGrade(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            string code = f.Get("txt-code").ToString();
            code = code.Replace(" ", "");
            string grade = f.Get("txt-grade").ToString();
            string des = f.Get("txt-grade-des").ToString();
            int language = f.Get("dll-language").ToInt(0);
            var rs = GradeTable.insert(code, grade, des, aclog.UserName, language);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Mã chuyên môn đã tồn tại", "e");
            return RedirectToAction("Grade", "Option");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditGrade(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            int ID = f.Get("hdfID").ToInt(0);
            string grade = f.Get("txt-edit-grade").ToString();
            string codegrade = f.Get("txt-edit-code").ToString();
            codegrade = codegrade.Replace(" ", "");
            string des = f.Get("txt-edit-grade-des").ToString();
            bool ishide = false;
            if (!string.IsNullOrEmpty(f.Get("chk-hide")))
                ishide = true;
            int language = f.Get("edit-dll-language").ToInt(0);
            var rs = GradeTable.update(ID, codegrade, grade, des, ishide, aclog.UserName, language);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("Grade", "Option");
        }
        public ActionResult DeleteGrade(int id)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_Grade.Where(x => x.ID == id).FirstOrDefault();
                ck.isDelete = true;
                db.SaveChanges();
            }
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }
        #endregion

        #region ca làm việc
        public ActionResult Shift(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //page size
            int pageSize = 20;
            IEnumerable<tbl_StudyTime> l = StudyTimeTable.getall();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                string search = ViewBag.Search = Request.QueryString["search"].ToString();
                l = from r in l
                    where AssetCRM.RemoveUnicode(r.StudyTimeName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(r.sTime).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(r.eTime).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(r.ModifiedBy).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(r.ModifiedDate.ToString()).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select r;
            }
            if (!string.IsNullOrEmpty(Request.QueryString["search-time"]))
            {
                int searchtime = ViewBag.SearchTime = Request.QueryString["search-time"].ToInt(1);
                l = l.Where(x => x.TimeOfDay == searchtime);
            }
            l = l.OrderBy(n => n.isHide).ThenByDescending(n => n.sTime).ToList();
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }

        public ActionResult CaHoc()
        {
            return View();
        }
        [HttpGet]
        public JsonResult LoadCaHoc(int page, int pageSize = 10)
        {
            IEnumerable<tbl_StudyTime> model = StudyTimeTable.getall().OrderBy(n => n.isHide).ThenByDescending(n => n.sTime).ToList();
            int totalRow = model.Count();

            model = model.OrderByDescending(x => x.CreatedDate)
              .Skip((page - 1) * pageSize)
              .Take(pageSize);


            return Json(new
            {
                data = model,
                total = totalRow,
                status = true
            }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddStudyTime(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            string studytime = f.Get("txt-lesson").ToString();
            int lesson = f.Get("ddl-lesson").ToInt(120);
            int timeofday = f.Get("time-of-day").ToInt(0);
            string[] stime = f.Get("ddl-stime").ToString().Split(':');
            TimeSpan sth = new TimeSpan(stime[0].ToInt(), stime[1].ToInt(0), 0);
            DateTime st = GetDateTime.Now.Date.Add(sth);

            DateTime et = st.AddMinutes(lesson);

            var rs = StudyTimeTable.insert(timeofday, studytime, lesson, st.ToString("HH:mm"), et.ToString("HH:mm"), aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("Shift", "Option");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditStudyTime(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int ID = f.Get("hdfID").ToInt(0);
            string studytime = f.Get("txt-edit-lesson").ToString();
            int lesson = f.Get("ddl-edit-lesson").ToInt(120);
            int timeofday = f.Get("edit-time-of-day").ToInt(0);
            string[] stime = f.Get("ddl-edit-stime").ToString().Split(':');
            TimeSpan sth = new TimeSpan(stime[0].ToInt(), stime[1].ToInt(0), 0);
            DateTime st = GetDateTime.Now.Date.Add(sth);

            DateTime et = st.AddMinutes(lesson);
            bool ishide = false;
            if (!string.IsNullOrEmpty(f.Get("chk-hide")))
                ishide = true;
            var rs = StudyTimeTable.update(ID, timeofday, studytime, lesson, st.ToString("HH:mm"), et.ToString("HH:mm"), ishide, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("Shift", "Option");
        }
        #endregion

        #region danh sách mã giảm giá
        public ActionResult Discount(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2)// không phải admin hoặc manager thì ko sửa được KM
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = ViewBag.Search = Request.QueryString["search"].ToString();
            }
            int status = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["status"]))
            {
                ViewBag.Status = status = Request.QueryString["status"].ToInt(0);
            }
            // thời gian bắt đầu từ
            string fromdate = null;
            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                try
                {
                    DateTime fd = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                    fromdate = fd.ToString();
                    ViewBag.From = Request.QueryString["from"].ToString();
                }
                catch { }
            }
            // đến
            string todate = null;
            if (!string.IsNullOrEmpty(Request.QueryString["to"]))
            {
                try
                {
                    DateTime td = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null);
                    todate = td.ToString();
                    ViewBag.To = Request.QueryString["to"].ToString();
                }
                catch { }
            }
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                ViewBag.Sort = sort = Request.QueryString["sort"].ToInt(0);
            }
            List<ASW_Discount_List_Result> l = new List<ASW_Discount_List_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_Discount_List(search, sort, status, fromdate, todate, (page - 1) * pageSize, pageSize).ToList();
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

        [HttpPost]// tìm kiếm KM theo id
        public ActionResult CheckCode(string code)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var d = DiscountTable.getbycode(code);
            if (d == null)
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetDiscountByID(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var d = DiscountTable.getbyid(id);
            if (d == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            else
            {
                bool type = false;
                if (d.DiscountType != null)
                    type = d.DiscountType.Value;
                return Json(new { rs = true, code = d.DiscountCode, p = String.Format("{0:0,0}", d.DiscountPrice), sDate = d.StartDate == null ? "" : d.StartDate.Value.ToString("dd/MM/yyyy"), dl = d.DeadLine.Value.ToString("dd/MM/yyyy"), note = d.Note, quantity = d.Quantity, percent = d.DiscountPercent, type = type }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AddCode(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2)// không phải admin hoặc manager thì ko sửa được KM
                return RedirectToAction("Signin", "Login", new { area = "" });

            string code = "";
            if (!string.IsNullOrEmpty(f.Get("txt-code")))
            {
                code = f.Get("txt-code").ToString().Replace(" ", "");
                var ckcode = DiscountTable.getbycode(code);
                if (ckcode != null)
                {
                    SetAlert("Mã khuyến mãi đã tồn tại", "e");
                    return RedirectToAction("Discount", "Option");
                }
            }
            else
            {
                bool ck = true;
                while (ck)
                {
                    code = AssetCRM.RandomStringWithText(10);
                    var ckcode = DiscountTable.getbycode(code);
                    if (ckcode == null)
                        ck = false;
                }
            }

            int price = f.Get("txt-price").ToString().Replace(",", "").ToInt(0);
            int percent = f.Get("txt-percent").ToString().Replace(",", "").ToInt(0);

            bool t = false;
            int type = f.Get("hdfpercent").ToString().ToInt(0);
            if (type == 0)
            {
                percent = 0;
                if (price == 0)
                {
                    SetAlert("Vui lòng nhập số tiền", "e");
                    return RedirectToAction("Discount", "Option");
                }
            }
            if (type == 1)
            {
                price = 0;
                t = true;
                if (percent <= 0 || percent > 100)
                {
                    SetAlert("Vui lòng nhập phần trăm giảm giá từ 0 đến 100%", "e");
                    return RedirectToAction("Discount", "Option");
                }
            }
            DateTime sDate = DateTime.Now;
            if (!string.IsNullOrEmpty(f.Get("txt-start")))
            {
                sDate = DateTime.ParseExact(f.Get("txt-start"), "dd/MM/yyyy", null);
            }

            DateTime dl = GetDateTime.Now.AddDays(1).Date;
            try
            {
                string day = f.Get("txt-dead-line").ToString().Replace(" ", "");
                dl = DateTime.ParseExact(day, "dd/MM/yyyy", null);
            }
            catch { }

            if (dl < GetDateTime.Now.Date)
                dl = GetDateTime.Now.AddDays(30).Date;

            string note = f.Get("txt-note").ToString();
            int quantity = f.Get("txt-quantity").ToInt(1);
            if (quantity == 0)
                quantity = 1;
            var rs = DiscountTable.insert(code, price, percent, note, sDate.Date, dl.Date, quantity, aclog.UserName, t);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("Discount", "Option");
        }
        [HttpPost]
        public ActionResult EditCode(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2)// không phải admin hoặc manager thì ko sửa được KM
                return RedirectToAction("Signin", "Login", new { area = "" });

            var d = DiscountTable.getbyid(f.Get("hdfID").ToInt(0));
            if (d == null)
            {
                SetAlert("Không tìm thấy KM", "e");
                return RedirectToAction("Discount", "Option");
            }
            string note = f.Get("txt-edit-note").ToString();
            int quantity = f.Get("txt-edit-quantity").ToInt(1);

            int slConlai = 0;
            int total = CoursesStudentTable.getall().Where(n => n.DiscountID == d.ID && n.ComboID == 0).ToList().Count;
            int totalcombo = ComboTable.GetAll().Where(n => n.DiscountID == d.ID).ToList().Count;
            int totalService = ServicesStudentTable.getall().Where(n => n.DiscountID == d.ID).ToList().Count;
            int slDaSuDung = total + totalcombo + totalService;
            if (quantity < slDaSuDung)
            {
                SetAlert("Số lần vừa nhập không thể hơn số lần đã sử dụng", "e");
                return RedirectToAction("Discount", "Option");
            }
            else
            {
                slConlai = quantity - slDaSuDung;
            }

            int price = f.Get("txt-edit-price").ToString().Replace(",", "").ToInt(0);
            int percent = f.Get("txt-edit-percent").ToString().Replace(",", "").ToInt(0);

            bool t = false;
            if (d.DiscountType != null)
                t = d.DiscountType.Value;
            if (!t)
            {
                percent = 0;
                if (price == 0)
                {
                    SetAlert("Vui lòng nhập số tiền", "e");
                    return RedirectToAction("Discount", "Option");
                }
            }
            else
            {
                price = 0;
                if (percent <= 0 || percent > 100)
                {
                    SetAlert("Vui lòng nhập phần trăm giảm giá từ 0 đến 100%", "e");
                    return RedirectToAction("Discount", "Option");
                }
            }

            DateTime dl = GetDateTime.Now.AddDays(1).Date;
            try
            {
                string day = f.Get("txt-edit-dead-line").ToString().Replace(" ", "");
                dl = DateTime.ParseExact(day, "dd/MM/yyyy", null);
            }
            catch { }

            if (dl < GetDateTime.Now.Date)
                dl = GetDateTime.Now.AddDays(1).Date;
            var rs = DiscountTable.update(d.ID, price, percent, dl.Date, note, quantity, slConlai, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("Discount", "Option");
        }
        #endregion

        //Giải thưởng
        public ActionResult Prize()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            return View();
        }

        #region Nguồn học viên
        public ActionResult SourceOfCustomer(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            IEnumerable<tbl_SourceOfCustomer> l = SourceOfCustomerTable.getall().OrderByDescending(n => n.ID).ToList();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from s in l
                    where AssetCRM.RemoveUnicode(s.SourceOfCustomer).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select s;
            }
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddSource(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            string source = f.Get("txt-source").ToString();
            var rs = SourceOfCustomerTable.insert(source, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("SourceOfCustomer", "Option");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditSource(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            int ID = f.Get("hdfID").ToInt(0);
            string source = f.Get("txt-edit-source").ToString();
            bool ishide = false;
            if (!string.IsNullOrEmpty(f.Get("chk-cb")))
                ishide = true;
            var rs = SourceOfCustomerTable.update(ID, source, ishide, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("SourceOfCustomer", "Option");
        }
        #endregion

        #region Ngày nghỉ trong năm
        public ActionResult DayOff(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            IEnumerable<tbl_DayOff> l = DayOffTable.getall().Where(n => n.isHide != true).OrderByDescending(n => n.DayOff).ToList();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from s in l
                    where AssetCRM.RemoveUnicode(s.DayOffName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(s.DayOff.Value.ToString("dd/MM/yyyy")).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select s;
            }
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddDayOff(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            DateTime doff = DateTime.ParseExact(f.Get("txt-dayoff").ToString().Trim(), "dd/MM/yyyy", null);
            DateTime toNgay = DateTime.ParseExact(f.Get("txt-to-dayoff").ToString().Trim(), "dd/MM/yyyy", null);
            string note = f.Get("txt-note").ToString();
            if (doff < GetDateTime.Now.Date)
            {
                SetAlert("Ngày nghỉ không được nhỏ hơn ngày hiện tại", "e");
                return RedirectToAction("DayOff", "Option");
            }
            if (toNgay < doff)
            {
                SetAlert("Ngày kết thúc không được nhỏ hơn ngày bắt đầu", "e");
                return RedirectToAction("DayOff", "Option");
            }
            if (doff != toNgay)
            {
                int totalDay = (toNgay.Date - doff.Date).Days;
                for (int i = 0; i <= totalDay; i++)
                {
                    var check = DayOffTable.getbydate(doff.Date.AddDays(i));
                    if (check == null)
                    {
                        DayOffTable.insert(doff.Date.AddDays(i), note, aclog.UserName);
                    }
                }
                SetAlert("Thành công", "s");
                return RedirectToAction("DayOff", "Option");
            }

            var rs = DayOffTable.insert(doff.Date, note, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("DayOff", "Option");
        }

        [HttpPost]
        public ActionResult RemoveDayOff(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var d = DayOffTable.getbyID(f.Get("hdfID").ToInt(0));
            if (d == null)
            {
                SetAlert("Không tìm thấy ngày nghỉ", "e");
                return RedirectToAction("DayOff", "Option");
            }
            if (d.DayOff.Value < GetDateTime.Now.Date)
            {
                SetAlert("Ngày nghỉ đã qua không thể xóa", "e");
                return RedirectToAction("DayOff", "Option");
            }
            var rs = DayOffTable.updatehide(d.ID, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("DayOff", "Option");
        }
        #endregion

        #region Danh sách công việc
        public ActionResult Jobs(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            IEnumerable<tbl_Job> l = JobTable.getall().OrderByDescending(n => n.ID).ToList();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();
                l = from s in l
                    where AssetCRM.RemoveUnicode(s.JobName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select s;
            }
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddJob(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            string job = f.Get("txt-job").ToString();
            var rs = JobTable.insert(job, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("Jobs", "Option");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditJob(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int ID = f.Get("hdfID").ToInt(0);
            string job = f.Get("txt-edit-job").ToString();
            var rs = JobTable.update(ID, job, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("Jobs", "Option");
        }
        #endregion

        #region loại hình giáo dục
        public ActionResult TypeEducation()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            return View();
        }
        public ActionResult AddTypeEducation(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            string job = f.Get("txt-job").ToString();
            var rs = JobTable.insert(job, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("TypeEducation", "Option");
        }
        #endregion

        #region danh sách dịch vụ
        public ActionResult Services(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = ViewBag.Search = Request.QueryString["search"].ToString();
            }
            int status = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["status"]))
            {
                ViewBag.Status = status = Request.QueryString["status"].ToInt(0);
            }
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                ViewBag.Sort = sort = Request.QueryString["sort"].ToInt(0);
            }
            List<ASW_Services_List_Result> l = new List<ASW_Services_List_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_Services_List(search, sort, status, (page - 1) * pageSize, pageSize).ToList();
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

        public ActionResult ServicesDetail(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = ViewBag.Search = Request.QueryString["search"].ToString();
            }
            int serviceID = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["serviceID"]))
            {
                ViewBag.ServiceID = serviceID = Request.QueryString["serviceID"].ToInt(0);
                ViewBag.ServiceName = ServicesTable.getbyid(serviceID).ServiceName;
            }
            int dotthi = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["dotthi"]))
            {
                ViewBag.DotThi = dotthi = Request.QueryString["dotthi"].ToInt(0);
            }
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                ViewBag.Sort = sort = Request.QueryString["sort"].ToInt(0);
            }
            List<ASW_ServicesDetail_List_Result> l = new List<ASW_ServicesDetail_List_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_ServicesDetail_List(search, sort, serviceID, dotthi, (page - 1) * pageSize, pageSize).ToList();
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

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddService(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var service = f.Get("txt-service").ToString();
            var mota = f.Get("txt-service-des").ToString();
            var ncc = f.Get("txt-service-ncc").ToString().ToInt();
            var phutrach = f.Get("txt-service-phutrach").ToInt(0);
            var n = SupplierServicesTable.getbyID(ncc);
            if (n == null)
            {
                SetAlert("Không tìm thấy nhà cung cấp", "e");
                return RedirectToAction("Services", "Option");
            }

            var rs = ServicesTable.insert(service, n.ID, n.StatusID.Value, phutrach, 1, mota, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("Services", "Option");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditService(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            int ID = f.Get("hdfID").ToInt(0);

            var s = ServicesTable.getbyid(ID);
            if (s == null)
            {
                SetAlert("Không tìm thấy dịch vụ", "e");
                return RedirectToAction("Services", "Option");
            }
            var service = f.Get("txt-edit-service").ToString();
            var mota = f.Get("txt-edit-service-des").ToString();
            var ncc = f.Get("txt-edit-service-ncc").ToString().ToInt();
            var phutrach = f.Get("txt-edit-service-phutrach").ToInt(0);
            var status = f.Get("txt-edit-service-status").ToString().ToInt();
            var n = SupplierServicesTable.getbyID(ncc);
            if (n == null)
            {
                SetAlert("Không tìm thấy nhà cung cấp", "e");
                return RedirectToAction("Services", "Option");
            }

            var rs = ServicesTable.update(ID, service, n.ID, n.StatusID.Value, phutrach, status, mota, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("Services", "Option");
        }
        #endregion

        #region danh sách feedback
        public ActionResult CategoryFeedback(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            IEnumerable<tbl_FeedBackCategory> l = FeedBackCategoryTable.getall().OrderByDescending(n => n.ID).ToList();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();
                l = from s in l
                    where AssetCRM.RemoveUnicode(s.CategoryFeedback).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select s;
            }
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddCategoryFeedback(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            string rolename = "Tất cả";
            var role = f.Get("ddl-role").ToInt(0);
            if (role != 0)
                rolename = AccountTable.viewrole(role, 1, 1).role;
            var cate = f.Get("txt-cate-feedback").ToString();
            var rs = FeedBackCategoryTable.insert(role, rolename, cate, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("CategoryFeedback", "Option");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditCategoryFeedback(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var cate = FeedBackCategoryTable.getbyID(f.Get("hdfID").ToInt(0));
            if (cate == null)
            {
                SetAlert("Không tìm thấy loại phản hồi", "e");
                return RedirectToAction("CategoryFeedback", "Option");
            }

            string rolename = "Tất cả";
            var role = f.Get("ddl-edit-role").ToInt(0);
            if (role != 0)
                rolename = AccountTable.viewrole(role, 1, 1).role;
            var catename = f.Get("txt-edit-cate").ToString();
            var rs = FeedBackCategoryTable.update(cate.ID, role, rolename, catename, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("CategoryFeedback", "Option");
        }
        #endregion

        #region danh sách nhà cung cấp
        public ActionResult SupplierList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = search = Request.QueryString["search"].ToString();
            }
            //trạng thái
            int status = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["status"]))
            {
                status = ViewBag.StatusID = Request.QueryString["status"].ToInt(0);
            }
            //sắp xếp
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                ViewBag.Sort = sort = Request.QueryString["sort"].ToInt(0);
            }
            List<ASW_SupplierServices_List_Result> l = new List<ASW_SupplierServices_List_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_SupplierServices_List(search, sort, status, (page - 1) * pageSize, pageSize).ToList();
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

        public ActionResult SupplierListDetail(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5 || aclog.RoleID == 4 || aclog.RoleID == 3 || aclog.RoleID == 6 || aclog.RoleID == 9)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var t = SupplierServicesTable.getbyID(id);
            if (t == null)
            {
                SetAlert("Không tìm thấy nhà cung cấp này :(", "e");
                return RedirectToAction("SupplierList", "Option");
            }
            return View(t);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddSupplier(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });


            var name = f.Get("txt-supplier").ToString();
            var mst = f.Get("txt-mst").ToString();
            var daidien = f.Get("txt-daidien").ToString();
            var phone = f.Get("txt-phone").ToString();
            var phutrach = f.Get("txt-phutrach").ToString();
            var address = f.Get("txt-address").ToString();
            var Description = f.Get("txt-Description").ToString();
            var rs = SupplierServicesTable.insert(name, 1, phone, mst, address, Description, phutrach, daidien, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("SupplierList", "Option");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditSupplier(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var sup = SupplierServicesTable.getbyID(f.Get("hdfID").ToInt(0));
            if (sup == null)
            {
                SetAlert("Không tìm thấy nhà cung cấp", "e");
                return RedirectToAction("SupplierList", "Option");
            }
            var name = f.Get("txt-edit-supplier").ToString();
            var mst = f.Get("txt-edit-mst").ToString();
            var daidien = f.Get("txt-edit-daidien").ToString();
            var statusID = f.Get("txt-edit-statusID").ToString().ToInt(1);
            var phone = f.Get("txt-edit-phone").ToString();
            var phutrach = f.Get("txt-edit-phutrach").ToString();
            var address = f.Get("txt-edit-address").ToString();
            var Description = f.Get("txt-edit-Description").ToString();
            var rs = SupplierServicesTable.update(sup.ID, name, statusID, phone, mst, address, Description, phutrach, daidien, sup.isHide.Value, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("SupplierList", "Option");
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditSupplierDetail(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var sup = SupplierServicesTable.getbyID(f.Get("hdfID").ToInt(0));
            if (sup == null)
            {
                SetAlert("Không tìm thấy nhà cung cấp", "e");
                return RedirectToAction("SupplierList", "Option");
            }
            var name = f.Get("txt-edit-supplier").ToString();
            var mst = f.Get("txt-edit-mst").ToString();
            var daidien = f.Get("txt-edit-daidien").ToString();
            var statusID = f.Get("txt-edit-statusID").ToString().ToInt(1);
            var phone = f.Get("txt-edit-phone").ToString();
            var phutrach = f.Get("txt-edit-phutrach").ToString();
            var address = f.Get("txt-edit-address").ToString();
            var Description = f.Get("txt-edit-Description").ToString();
            var rs = SupplierServicesTable.update(sup.ID, name, statusID, phone, mst, address, Description, phutrach, daidien, sup.isHide.Value, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("SupplierListDetail", "Option", new { id = sup.ID });
        }
        #endregion

        #region danh sách mục đich học tập
        public ActionResult AcademicPurposesList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            IEnumerable<tbl_AcademicPurposes> l = AcademicPurposesTable.getall().OrderByDescending(n => n.ID).ToList();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();
                l = from s in l
                    where AssetCRM.RemoveUnicode(s.AcademicPurposesName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select s;
            }
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddAcademicPurposes(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });


            var name = f.Get("txt-academicpurposes").ToString();
            var rs = AcademicPurposesTable.insert(name, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("AcademicPurposesList", "Option");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditAcademicPurposes(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aca = AcademicPurposesTable.getbyID(f.Get("hdfID").ToInt(0));
            if (aca == null)
            {
                SetAlert("Không tìm thấy dữ liệu", "e");
                return RedirectToAction("AcademicPurposesList", "Option");
            }
            var name = f.Get("txt-edit-academicpurposes").ToString();
            var rs = AcademicPurposesTable.update(aca.ID, name, aca.IsHidden.Value, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("AcademicPurposesList", "Option");
        }
        
        [HttpPost]
        public ActionResult AddStudyPath(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var name = f.Get("txt-name").ToString();
            var link = f.Get("txt-academicpurposes").ToString();
            var rs = StudyPathTable.insert(name, link, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }
        
        [HttpPost]
        public ActionResult EditStudyPath(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var id = f.Get("hdfID").ToInt();
            var name = f.Get("txt-name-edit").ToString();
            var link = f.Get("txt-edit-academicpurposes").ToString();
            var rs = StudyPathTable.update(id, name, link, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }
        public ActionResult StudyPath()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            List<tbl_StudyPath> l = StudyPathTable.getall();
            
            ViewBag.Total = l.Count();
            return View(l);
        }
        #endregion

        #region cấu hình lương saler
        public ActionResult SettingSalary(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            IEnumerable<tbl_SalarySetting> l = SalarySettingTable.getall().OrderBy(n => n.SalaryType).ThenByDescending(x => x.PercentSalary).ToList();

            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddSettingSalary(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2)
                return RedirectToAction("Signin", "Login", new { area = "" });

            //int million = f.Get("txt-total-price").ToString().Replace(",", "").ToInt(0);
            double percent = Convert.ToDouble(f.Get("txt-percent"));
            string positionname = f.Get("txt-positionname").ToString();
            if (percent < 0 || percent > 100)
            {
                SetAlert("Vui lòng nhập doanh số, phần trăm lớn hơn 0 và bé hơn 100", "e");
                return RedirectToAction("SettingSalary", "Option");
            }
            double revennue = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-revenue")))
            {
                revennue = Convert.ToDouble(f.Get("txt-revenue").ToString().Replace(",", ""));
            }
            double revennueDown = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-revenue-down")))
            {
                revennueDown = Convert.ToDouble(f.Get("txt-revenue-down").ToString().Replace(",", ""));
            }
            //int type = f.Get("slType").ToInt(0);
            var rs = SalarySettingTable.insert(positionname, revennue, revennueDown, percent, aclog.UserName, 1);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("SettingSalary", "Option");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditSettingSalary(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //int million = f.Get("txt-total-price-edit").ToString().Replace(",", "").ToInt(0);
            double percent = Convert.ToDouble(f.Get("txt-percent-edit"));
            string positionname = f.Get("txt-positionname-edit").ToString();
            if (percent < 0 || percent > 100)
            {
                SetAlert("Vui lòng nhập doanh số, phần trăm lớn hơn 0 và bé hơn 100", "e");
                return RedirectToAction("SettingSalary", "Option");
            }
            int id = f.Get("hdfID").ToInt(0);
            int type = f.Get("slType-edit").ToInt(0);
            double revennue = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-revenue-edit")))
            {
                revennue = Convert.ToDouble(f.Get("txt-revenue-edit").ToString().Replace(",", ""));
            }
            double revennueDown = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-revenue-down-edit")))
            {
                revennueDown = Convert.ToDouble(f.Get("txt-revenue-down-edit").ToString().Replace(",", ""));
            }
            var rs = SalarySettingTable.update(id, positionname, revennue, revennueDown, percent, aclog.UserName, 1);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("SettingSalary", "Option");
        }
        
        [HttpPost]
        public ActionResult DeleteSettingSalary(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            
            var rs = SalarySettingTable.Delete(id, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return Json(new { rs = true});
        }
        #endregion

        #region kì thi
        public ActionResult Exams(int page = 1)
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
            int schoolID = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["center"]))
            {
                ViewBag.SchoolID = schoolID = Request.QueryString["center"].ToInt(0);
            }
            int supplierID = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["supplier"]))
            {
                ViewBag.SupplierID = supplierID = Request.QueryString["supplier"].ToInt(0);
            }
            // lấy ngày đầu và ngày cuối tháng
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            string fdate = firstDayOfMonth.ToString("yyy-MM-dd");
            ViewBag.From = firstDayOfMonth.ToString("dd/MM/yyyy");
            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                DateTime fd = DateTime.ParseExact(Request.QueryString["from"], "dd/MM/yyyy", null);
                fdate = fd.ToString();
                ViewBag.From = Request.QueryString["from"];
            }
            string tdate = lastDayOfMonth.ToString("yyyy-MM-dd");
            ViewBag.To = lastDayOfMonth.ToString("dd/MM/yyyy");
            if (!string.IsNullOrEmpty(Request.QueryString["to"]))
            {
                DateTime td = DateTime.ParseExact(Request.QueryString["to"], "dd/MM/yyyy", null);
                tdate = td.ToString();
                ViewBag.To = Request.QueryString["to"];
            }
            List<ASW_ServiceExams_List_Result> l = new List<ASW_ServiceExams_List_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_ServiceExams_List(search, sort, schoolID, supplierID, tdate, fdate, (page - 1) * pageSize, pageSize).ToList();
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
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddExams(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var name = f.Get("txt-exam").ToString();
            var hour = f.Get("txt-time-exam").ToString();
            var d = f.Get("txt-exam-date").ToString().Replace(" ", "");
            DateTime date = new DateTime();
            try
            {
                date = DateTime.ParseExact(d, "dd/MM/yyyy", null);
            }
            catch
            {
                SetAlert("Vui lòng nhập đúng định dạng ngày", "e");
                return RedirectToAction("Exams", "Option");
            }
            tbl_School center = SchoolTable.getbyID(f.Get("ddl-center").ToInt(0));
            if (center == null)
            {
                SetAlert("Không tìm thấy trung tâm", "e");
                return RedirectToAction("Exams", "Option");
            }
            tbl_SupplierServices supp = SupplierServicesTable.getbyID(f.Get("ddl-supplier").ToInt(0));
            if (supp == null)
            {
                SetAlert("Không tìm thấy nhà cung cấp", "e");
                return RedirectToAction("Exams", "Option");
            }
            int slot = f.Get("txt-slot").ToInt(0);
            int price = f.Get("txt-price").ToString().Replace(",", "").ToInt(0);
            int giavon = f.Get("txt-price-von").ToString().Replace(",", "").ToInt(0);
            int serviceID = f.Get("ddl-service").ToInt(0);
            int discountID = f.Get("ddl-discount").ToInt(0);
            var real = false;
            if (!string.IsNullOrEmpty(f.Get("ck-real")))
                real = true;
            var rs = ServiceExamsTable.insert(center.CityID.Value, center.ID, supp.ID, serviceID, slot, price, giavon, real, name, date.Date, aclog.UserName, hour, discountID);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("Exams", "Option");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditExams(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var hour = f.Get("txt-time-exam-edit").ToString();
            var aca = ServiceExamsTable.getbyID(f.Get("hdfID").ToInt(0));
            if (aca == null)
            {
                SetAlert("Không tìm thấy dữ liệu", "e");
                return RedirectToAction("Exams", "Option");
            }

            var name = f.Get("txt-edit-exam").ToString();
            var d = f.Get("txt-edit-exam-date").ToString().Replace(" ", "");
            DateTime date = new DateTime();
            try
            {
                date = DateTime.ParseExact(d, "dd/MM/yyyy", null);
            }
            catch
            {
                SetAlert("Vui lòng nhập đúng định dạng ngày", "e");
                return RedirectToAction("Exams", "Option");
            }
            tbl_School center = SchoolTable.getbyID(f.Get("ddl-center-edit").ToInt(0));
            if (center == null)
            {
                SetAlert("Không tìm thấy trung tâm", "e");
                return RedirectToAction("Exams", "Option");
            }
            tbl_SupplierServices supp = SupplierServicesTable.getbyID(f.Get("ddl-supplier-edit").ToInt(0));
            if (supp == null)
            {
                SetAlert("Không tìm thấy nhà cung cấp", "e");
                return RedirectToAction("Exams", "Option");
            }
            int slot = f.Get("txt-slot-edit").ToInt(0);
            int price = f.Get("txt-price-edit").ToString().Replace(",", "").ToInt(0);
            var real = false;
            if (!string.IsNullOrEmpty(f.Get("ck-edit-real")))
                real = true;
            var rs = ServiceExamsTable.update(aca.ID, center.CityID.Value, center.ID, supp.ID, slot, price, real, name, date.Date, aclog.UserName, hour);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("Exams", "Option");
        }
        // lấy thông tin đợt thi
        [HttpPost]
        public ActionResult GetExam(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var ex = ServiceExamsTable.getbyID(id);

            if (ex == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            return Json(new { rs = true, ex = ex }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ReportExams(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var aca = ServiceExamsTable.getbyID(f.Get("hdfReportID").ToInt(0));
            if (aca == null)
            {
                SetAlert("Không tìm thấy dữ liệu", "e");
                return RedirectToAction("Exams", "Option");
            }
            var l = ServicesStudentTable.getbyserviceexamsid(aca.ID).OrderByDescending(n => n.ID).ToList();
            if (l.Count > 0)
            {
                var products = new System.Data.DataTable("Nam");
                products.Columns.Add("Full Name", typeof(string));
                products.Columns.Add("Phone", typeof(string));
                products.Columns.Add("Email", typeof(string));
                products.Columns.Add("LISTENING", typeof(double));
                products.Columns.Add("SPEAKING", typeof(double));
                products.Columns.Add("READING", typeof(double));
                products.Columns.Add("WRITING", typeof(double));
                products.Columns.Add("TASK 1", typeof(double));
                products.Columns.Add("TASK 2", typeof(double));
                products.Columns.Add("OVER ALL", typeof(double));

                for (int i = 0; i < l.Count; i++)
                {
                    var item = l[i];
                    var cus = AccountTable.getbyID(item.StudentUID.Value);
                    try
                    {
                        products.Rows.Add(cus.FullName, "'" + cus.Phone, cus.Email, item.Listening.Value, item.Speaking.Value, item.Reading.Value, item.Writing, item.TaskOne.Value, item.TaskTwo.Value, item.OverAll.Value); ;
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
                Response.AddHeader("content-disposition", "attachment; filename=Điểm thi [" + aca.ExamService + "].xls");
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
            return RedirectToAction("Exams", "Option");
        }
        #endregion

        public ActionResult IdiomsList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            IEnumerable<tbl_Idioms> l = IdiomsTable.getall().OrderByDescending(n => n.ID).ToList();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from s in l
                    where AssetCRM.RemoveUnicode(s.Idioms).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select s;
            }
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddIdioms(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });


            var content = f.Get("txt-content").ToString();
            if (string.IsNullOrEmpty(AssetCRM.RemoveHTMLTags(content)))
            {
                SetAlert("Vui lòng nhập nội dung", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            var id = f.Get("hdf-id").ToInt(0);
            if (id == 0)
            {
                IdiomsTable.insert(content, aclog.UserName);
                SetAlert("Thêm dữ liệu thành công", "s");
            }
            else
            {
                IdiomsTable.update(id, content, false, aclog.UserName);
                SetAlert("Cập nhật thành công", "s");
            }
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }
        /// <summary>
        /// lấy thông tin câu thành ngữ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetInfoIdioms(int id)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var rs = IdiomsTable.getbyid(id);
            if (rs == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            return Json(new { rs = true, d = rs }, JsonRequestBehavior.AllowGet);
        }
        //ẩn hiện câu thành ngữ
        [HttpPost]
        public ActionResult AcceptContract(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var rs = IdiomsTable.getbyid(id);
            if (rs != null)
                IdiomsTable.update(rs.ID, rs.Idioms, !rs.IsHidden.Value, aclog.UserName);
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteDiscount(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 1)
            {
                DiscountTable.Delete(id, true, aclog.UserName);
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Xuất Excel danh sách khuyến mãi
        /// </summary>
        /// <param name="search"></param>
        /// <param name="sort"></param>
        /// <param name="status"></param>
        /// <param name="fdate"></param>
        /// <param name="tdate"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExportExcelDiscount(string search, int sort, int status, string fdate, string tdate)
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
            if (!string.IsNullOrEmpty(fdate))
            {
                DateTime fd = DateTime.ParseExact(fdate, "dd/MM/yyyy", null);
                fdate = fd.ToString();
            }
            if (!string.IsNullOrEmpty(tdate))
            {
                DateTime td = DateTime.ParseExact(tdate, "dd/MM/yyyy", null);
                tdate = td.ToString();
            }

            List<ASW_Discount_ExportExcel_Result> l = new List<ASW_Discount_ExportExcel_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_Discount_ExportExcel(search, sort, status, fdate == "" ? null : fdate, tdate == "" ? null : tdate).ToList();
            }
            if (l.Count() == 0)
            {
                return Json(new { rs = false, noti = "Dữ liệu trống!" }, JsonRequestBehavior.AllowGet);
            }

            StringBuilder strExport = new StringBuilder();
            strExport.Append("       <table border=\"1\">");
            strExport.Append("                     <tr>");
            strExport.Append("                        <th scope=\"col\">STT</th>");
            strExport.Append("                        <th scope=\"col\">Code</th>");
            strExport.Append("                        <th scope=\"col\">Số tiền</th>");
            strExport.Append("                        <th scope=\"col\">Phần trăm</th>");
            strExport.Append("                        <th scope=\"col\">Trạng thái</th>");
            strExport.Append("                        <th scope=\"col\">Số lượng</th>");
            strExport.Append("                        <th scope=\"col\">Còn lại</th>");
            strExport.Append("                        <th scope=\"col\">Ghi chú</th>");
            strExport.Append("                        <th scope=\"col\">Bắt đầu</th>");
            strExport.Append("                        <th scope=\"col\">Hết hạn</th>");
            strExport.Append("                    </tr>");

            var index = 0;
            foreach (var i in l)
            {
                index++;
                strExport.Append("                    <tr>");
                strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.DiscountCode + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + String.Format("{0:0,0}", i.DiscountPrice) + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.DiscountPercent + "%</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.StatusName + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.Quantity + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.QuantityLeft + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.Note + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.StartDate.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.DeadLine.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                    </tr>");
            }
            strExport.Append("</table>");
            var filename = "Danh_Sach_Khuyen_Mai_" + GetDateTime.Now.ToString("dd/MM/yyyy");
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Xuất Excel nhà cung cấp
        /// </summary>
        /// <param name="search"></param>
        /// <param name="sort"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExportExcelSupplier(string search, int sort, int status)
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
            List<ASW_SupplierServices_ExportExcel_Result> l = new List<ASW_SupplierServices_ExportExcel_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_SupplierServices_ExportExcel(search, sort, status).ToList();
            }
            if (l.Count() == 0)
            {
                return Json(new { rs = false, noti = "Dữ liệu trống!" }, JsonRequestBehavior.AllowGet);
            }

            StringBuilder strExport = new StringBuilder();
            strExport.Append("       <table border=\"1\">");
            strExport.Append("                     <tr>");
            strExport.Append("                        <th scope=\"col\">STT</th>");
            strExport.Append("                        <th scope=\"col\">Nhà cung cấp</th>");
            strExport.Append("                        <th scope=\"col\">MST</th>");
            strExport.Append("                        <th scope=\"col\">Số điện thoại</th>");
            strExport.Append("                        <th scope=\"col\">Đại diện</th>");
            strExport.Append("                        <th scope=\"col\">Phụ trách</th>");
            strExport.Append("                        <th scope=\"col\">Trạng thái</th>");
            strExport.Append("                        <th scope=\"col\">Đại chỉ</th>");
            strExport.Append("                        <th scope=\"col\">Giới thiệu</th>");
            strExport.Append("                    </tr>");

            var index = 0;
            foreach (var i in l)
            {
                index++;
                strExport.Append("                    <tr>");
                strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.SupplierName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.MST + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.Phone + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.Represent + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.CuratorFullName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.StatusName + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.Address + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.Description + "</td>");
                strExport.Append("                    </tr>");
            }
            strExport.Append("</table>");
            var filename = "Danh_Sach_Nha_Cung_Cap_" + GetDateTime.Now.ToString("dd/MM/yyyy");
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ExportExcelService(string search, int sort, int status)
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
            List<ASW_Services_ExportExcel_Result> l = new List<ASW_Services_ExportExcel_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_Services_ExportExcel(search, sort, status).ToList();
            }
            if (l.Count() == 0)
            {
                return Json(new { rs = false, noti = "Dữ liệu trống!" }, JsonRequestBehavior.AllowGet);
            }

            StringBuilder strExport = new StringBuilder();
            strExport.Append("       <table border=\"1\">");
            strExport.Append("                     <tr>");
            strExport.Append("                        <th scope=\"col\">STT</th>");
            strExport.Append("                        <th scope=\"col\">Dịch vụ</th>");
            strExport.Append("                        <th scope=\"col\">Nhà cung cấp</th>");
            strExport.Append("                        <th scope=\"col\">Phụ trách</th>");
            strExport.Append("                        <th scope=\"col\">Trạng thái</th>");
            strExport.Append("                        <th scope=\"col\">Mô tả</th>");
            strExport.Append("                        <th scope=\"col\">Chỉnh sửa</th>");
            strExport.Append("                        <th scope=\"col\">Thời gian sửa</th>");
            strExport.Append("                    </tr>");

            var index = 0;
            foreach (var i in l)
            {
                index++;
                strExport.Append("                    <tr>");
                strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.ServiceName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.SupplierName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.CuratorFullName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.StatusName + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.DescribeService + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.ModifiedBy + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.ModifiedDate.Value.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                    </tr>");
            }
            strExport.Append("</table>");
            var filename = "Danh_Sach_Dich_Vu_" + GetDateTime.Now.ToString("dd/MM/yyyy");
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ExportExcelServiceDetail(string search, int sort, int dotthi, int serviceID, string serviceName)
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
            List<ASW_ServicesDetail_List_ExportExcel_Result> l = new List<ASW_ServicesDetail_List_ExportExcel_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_ServicesDetail_List_ExportExcel(search, sort, serviceID, dotthi).ToList();
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
            strExport.Append("                        <th scope=\"col\">Đợt thi</th>");
            strExport.Append("                        <th scope=\"col\">Ngày thi</th>");
            strExport.Append("                        <th scope=\"col\">Đã thanh toán</th>");
            strExport.Append("                        <th scope=\"col\">Chưa thanh toán</th>");
            strExport.Append("                        <th scope=\"col\">Khuyến mãi</th>");
            strExport.Append("                        <th scope=\"col\">Ngày đăng ký</th>");
            strExport.Append("                        <th scope=\"col\">Nhà cung cấp</th>");
            strExport.Append("                        <th scope=\"col\">Giá bán</th>");
            strExport.Append("                        <th scope=\"col\">Giá vốn</th>");
            strExport.Append("                        <th scope=\"col\">Lợi nhuận</th>");
            strExport.Append("                    </tr>");

            var index = 0;
            foreach (var i in l)
            {
                index++;
                strExport.Append("                    <tr>");
                strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.StudentName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.ExamService + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.NgayThi + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + String.Format("{0:0,0}", i.DaThanhToan) + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + String.Format("{0:0,0}", i.ChuaThanhToan) + "</td>");
                if (i.DiscountType == false)
                {
                    strExport.Append("                        <td style=\"text-align:left\">" + String.Format("{0:0,0}", i.KhuyenMai) + "</td>");
                }
                else
                {
                    strExport.Append("                        <td style=\"text-align:left\">" + i.KhuyenMai + "(%)</td>");
                }
                strExport.Append("                        <td style=\"text-align:center\">" + i.NgayDangKy + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.SupplierServices + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + String.Format("{0:0,0}", i.GiaBan) + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + String.Format("{0:0,0}", i.GiaVon) + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + String.Format("{0:0,0}", i.LoiNhuan) + "</td>");
                strExport.Append("                    </tr>");
            }
            strExport.Append("</table>");
            var filename = "DSHV_Mua_DV_" + serviceName + "_" + GetDateTime.Now.ToString("dd/MM/yyyy");
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Xóa đợt thi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteExam(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 1)
            {
                var check = ServiceExamsTable.getbyID(id);
                if (check.ExamDate.Value <= DateTime.Now)
                {
                    return Json(new { rs = false, message = "Không thể xóa đợt thi đã diễn ra" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var total = ServicesStudentTable.getbyserviceexamsid(id).Count();
                    if (total != 0)
                    {
                        return Json(new { rs = false, message = "Không thể xóa đợt thi đã có người giữ chỗ" }, JsonRequestBehavior.AllowGet);
                    }
                }
                ServiceExamsTable.Delete(id, true, aclog.UserName);
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Lấy danh sách DV bởi nhà cung cấp
        /// </summary>
        /// <param name="supplierID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetService(int supplierID)
        {
            List<tbl_Services> lser = ServicesTable.getall().Where(n => n.isHide != true && n.StatusID == 2).OrderByDescending(n => n.ID).ToList();
            if (supplierID != 0)
            {
                lser = lser.Where(n => n.SupplierID == supplierID).ToList();
            }
            return Json(new { rs = true, l = lser }, JsonRequestBehavior.AllowGet);
        }
        #region Gói cam kết
        // danh sách gói cam kết
        public ActionResult CommitmentPackage(int page = 1)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            IEnumerable<tbl_Class> l = ClassTable.GetByType(3);
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                string search = ViewBag.Search = Request.QueryString["search"].ToString();
                l = from r in l
                    where AssetCRM.RemoveUnicode(r.ClassName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(r.GradeName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select r;
            }
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }
        [HttpPost]
        public ActionResult AddCommitmentPackage(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            string classname = f.Get("txt-class-name").ToString();
            int price = f.Get("txt-price").ToString().Trim().Replace(",", "").ToInt(0);
            var rs = ClassTable.insert(classname, 0, "Gói cam kết", 3, price, price, aclog.UserName, 0, 0);

            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditCommitmentPackage(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

            bool ishide = false;
            if (!string.IsNullOrEmpty(f.Get("chk-cb")))
                ishide = true;
            int ID = f.Get("hdfID").ToInt(0);
            string classname = f.Get("txt-edit-class-name").ToString();
            int price = f.Get("txt-price-edit").ToString().Trim().Replace(",", "").ToInt(0);
            var rs = ClassTable.update(ID, classname, 0, "Gói cam kết", ishide, 3, price, price, aclog.UserName, 0, 0);

            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }

        [HttpPost]
        public ActionResult LoadHistoryChangePrice(int classid)
        {
            StringBuilder ap = new StringBuilder();
            var l = ClassHistoryTable.getbyclassid(classid).OrderByDescending(n => n.CreatedDate).ToList();
            foreach (var item in l)
            {
                var ac = AccountTable.getbyusername(item.CreatedBy);
                if (ac == null)
                    continue;
                ap.Append("<div class=\"timeline-item mg-b-20\">");
                ap.Append(" <div class=\"timeline-body\">");
                ap.Append("     <h6 class=\"mg-b-0\">" + ac.FullName + "</h6>");
                ap.Append("     <p style = \"margin-bottom:0px;\" ><a href=\"javascript:;\">" + AccountTable.getrolehtml(ac.RoleID.Value) + "</a> " + item.CreatedDate.Value.ToString("dd/MM/yyyy HH:mm") + "</p>");
                ap.Append("     " + item.HistoryContent + "");
                ap.Append(" </div>");
                ap.Append("</div>");
            }
            return Json(new { d = ap.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region danh sách trình độ
        public ActionResult AcademicLevel(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            IEnumerable<tbl_AcademicLevel> l = AcademicLevelTable.getall().OrderByDescending(n => n.Position).ToList();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from s in l
                    where AssetCRM.RemoveUnicode(s.LevelName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select s;
            }
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);
                l = l.Where(n => n.Language == sort || sort == 0).ToList();
            }
            ViewBag.Sort = sort;
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddAcademicLevel(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                string level = f.Get("txt-level").ToString();
                int position = f.Get("txt-position").ToInt(0);
                var language = Language.GetByID(f.Get("slLanguage").ToInt(0));
                string content = "";
                if(!string.IsNullOrEmpty(f.Get("txt-content")))
                {
                    content = f.Get("txt-content").ToString();
                }
                var rs = AcademicLevelTable.insert(level, position, language.ID, language.Name, aclog.UserName, content);
                if (rs != null)
                    SetAlert("Thành công", "s");
                else
                    SetAlert("Không thành công", "e");
                return RedirectToAction("AcademicLevel", "Option");
            }
            catch
            {
                SetAlert("Không thành công", "e");
                return RedirectToAction("AcademicLevel", "Option");
            }
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditAcademicLevel(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                int ID = f.Get("hdfID").ToInt(0);
                string level = f.Get("txt-edit-level").ToString();
                int position = f.Get("txt-edit-position").ToInt(0);
                var language = Language.GetByID(f.Get("edit-slLanguage").ToInt(0));
                string content = "";
                if (!string.IsNullOrEmpty(f.Get("txt-content-edit")))
                {
                    content = f.Get("txt-content-edit").ToString();
                }
                var rs = AcademicLevelTable.update(ID, level, position, language.ID, language.Name, aclog.UserName, content);
                if (rs != null)
                    SetAlert("Thành công", "s");
                else
                    SetAlert("Không thành công", "e");
                return RedirectToAction("AcademicLevel", "Option");
            }
            catch
            {
                SetAlert("Không thành công", "e");
                return RedirectToAction("AcademicLevel", "Option");
            }
        }
        [HttpPost]
        public ActionResult LoadLevel(int level)
        {
            try
            {
                var data = AcademicLevelTable.getByLanguage(level);
                StringBuilder html = new StringBuilder();
                foreach (var i in data)
                {
                    html.Append("<option value=\"" + i.ID + "\">" + i.LevelName + "</option>");
                }
                return Json(new { rs = true, data = html.ToString() });
            }
            catch
            {
                SetAlert("Không thành công", "e");
                return Json(new { rs = false });
            }
        }
        #endregion
        #region GMT
        public ActionResult GMT(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //page size
            int pageSize = 20;
            IEnumerable<tbl_Nation> l = NationTable.GetAll();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                string search = ViewBag.Search = Request.QueryString["search"].ToString();
                l = from r in l
                    where AssetCRM.RemoveUnicode(r.Nation).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(r.TimeZoneName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select r;
            }
            l = l.OrderBy(n => n.isHide).ThenBy(n => n.Nation).ToList();
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddGMT(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            string nation = f.Get("txt-nation").ToString();
            string timezone = f.Get("txt-timezone").ToString();
            int value = f.Get("txt-value").ToInt(0);
            var rs = NationTable.Insert(nation, timezone, value, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("GMT", "Option");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditGMT(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int ID = f.Get("hdfID").ToInt(0);
            string nation = f.Get("txt-edit-nation").ToString();
            string timezone = f.Get("txt-edit-timezone").ToString();
            int value = f.Get("txt-edit-value").ToInt(0);
            bool ishide = false;
            if (!string.IsNullOrEmpty(f.Get("chk-hide")))
                ishide = true;
            var rs = NationTable.Update(ID, nation, timezone, value, ishide, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("GMT", "Option");
        }
        #endregion
        #region Lịch sử điểm danh, video
        public ActionResult HistoryAttendance(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //page size
            int pageSize = 10;

            int course = 0;
            if (Request.QueryString["slCourse"].ToInt(0) != 0)
            {
                course = ViewBag.Course = Request.QueryString["slCourse"].ToInt(0);
            }

            // lấy ngày đầu và ngày cuối tháng
            DateTime date = DateTime.Now;
            var firstDayOfMonth = date.FirstDayOfMonth();
            var lastDayOfMonth = date.LastDayOfMonth();

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
            
            List<Admin_HistoryAttendance_GetList_Result> l = new List<Admin_HistoryAttendance_GetList_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_HistoryAttendance_GetList(course, fromdate, todate, (page - 1) * pageSize, pageSize).ToList();
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
        
        public ActionResult HistoryUploadVideo(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //page size
            int pageSize = 10;

            int course = 0;
            if (Request.QueryString["slCourse"].ToInt(0) != 0)
            {
                course = ViewBag.Course = Request.QueryString["slCourse"].ToInt(0);
            }

            // lấy ngày đầu và ngày cuối tháng
            DateTime date = DateTime.Now;
            var firstDayOfMonth = date.FirstDayOfMonth();
            var lastDayOfMonth = date.LastDayOfMonth();

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
            
            List<Admin_HistoryUploadVideo_GetList_Result> l = new List<Admin_HistoryUploadVideo_GetList_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_HistoryUploadVideo_GetList(course, fromdate, todate, (page - 1) * pageSize, pageSize).ToList();
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
        #endregion
    }
}