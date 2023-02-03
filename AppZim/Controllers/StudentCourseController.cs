using AppZim.Areas.Admin.Controllers;
using AppZim.Models;
using AppZim.TableSql;
using MB.Extensions;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using static AppZim.Controllers.HomeController;
using AppZim.ZIM;
using System.Threading.Tasks;
using AppZim.api;
using System.Web.Http;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;

namespace AppZim.Controllers
{
    public class StudentCourseController : BaseController
    {
        // GET: StudentCourse
        public ActionResult CourseList()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var l = CoursesStudentTable.getbystudentid(aclog.ID);
            return View(l);
        }

        public ActionResult CourseDetail(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)//éo phải học viên thì bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
            var courseStudent = CoursesStudentTable.getbyCourseIDdandStudentID(c.ID, aclog.ID);
            if (courseStudent == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
            return View(c);
        }

        public ActionResult CourseDocument(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
            ViewBag.Course = c;

            return View(c);
        }

        public ActionResult CourseExrcise(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
            ViewBag.Course = c;

            return View(c);
        }

        public ActionResult CourseNotification(int id, int page = 0)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
            ViewBag.Course = c;
            int pageSize = 10;
            var l = NotificationCourseTable.getbycourseid(c.ID).OrderByDescending(n => n.CreatedDate).ToList();
            if (page == 0)
                page = 1;
            return View(l.ToPagedList(page, pageSize));
        }
        public ActionResult PartialNextLession(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var calen = CoursesScheduleTable.getbycourseid(id);
            List<Schedule> l = new List<Schedule>();
            foreach (var item in calen)
            {
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
                int subid = 0;
                if (item.SubjectID != null)
                    subid = item.SubjectID.Value;
                else
                {
                    var currdetail = CurriculumDetailTable.getbyID(item.CurriculumsDetailsID.Value);
                    if (currdetail != null)
                        subid = currdetail.SubjectID.Value;
                }
                var sub = SubjectTable.getbyID(subid);
                if (sub == null)
                {
                    sub = new tbl_Subject();
                    sub.SubjectName = "";
                }

                //var room = SchoolRoomTable.getbyID(item.SchoolRoomID.Value);
                Schedule t = new Schedule();
                t.couserID = c.ID;
                t.className = c.ClassName;
                t.teacher = te.FullName;
                //t.room = room.RoomName;
                t.start = item.Date.Value.Add(s);
                t.end = item.Date.Value.Add(e);
                t.allDay = false;
                t.subject = sub.SubjectName;
                t.branch = SchoolTable.getbyID(item.SchoolID.Value).SchoolName;
                t.url = url;
                t.borderColor = colorcalenda;
                bool ck = true;
                var lc = l.Where(n => n.couserID == c.ID).ToList();
                foreach (var jtem in lc)
                {
                    if (jtem.couserID == t.couserID && jtem.start == t.start)
                    {
                        ck = false;
                        break;
                    }
                }
                if (ck)
                    l.Add(t);

            }
            var top6 = l.Where(n => n.start >= GetDateTime.Now).OrderBy(n => n.start).Take(6).ToList();
            ViewBag.Top6 = top6;
            return View();
        }
        public ActionResult TeacherCourse(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            if (c.CourseSelfStudy == true)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            ViewBag.Course = c;
            return View(c);
        }
        public ActionResult VideoCourse(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var data = new List<Admin_VideoSchedule_GetList_Result>();
            using (var db = new ZimEntities())
            {
                data = db.Admin_VideoSchedule_GetList(id).ToList();
            }
            var c = CoursesTable.getbyid(id);
            ViewBag.Course = c;
            return View(data);
        }
        public ActionResult NoteCourse(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            if (c.CourseSelfStudy == true)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            ViewBag.Course = c;
            return View(c);
        }
        [HttpGet]
        public ActionResult Contract()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                DateTime dt = DateTime.Now;
                var tt = SchoolTable.getall().FirstOrDefault();
                var ct = "";
                var kq = ContractTable.getbyid(1);
                if (kq == null)
                    ViewBag.Content = "";
                else
                {
                    ct = kq.ContractContent;
                    ct = ct.Replace("{trung-tam}", tt.SchoolName);
                    ct = ct.Replace("{ngay}", dt.ToString("dd"));
                    ct = ct.Replace("{thang}", dt.ToString("MM"));
                    ct = ct.Replace("{nam}", dt.ToString("yyyy"));
                    //thông tin học viên
                    var student = AccountTable.getbyID(aclog.ID);
                    ct = ct.Replace("{so-dien-thoai}", student.Phone == null ? "N/A" : student.Phone);
                    ct = ct.Replace("{email}", student.Email == null ? "N/A" : student.Email);
                    ct = ct.Replace("{hoc-vien}", student.FullName == null ? "N/A" : student.FullName);
                  
                    ViewBag.Content = ct;
                }
                return View();
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ContractController", "Contract", Session["UID"].ToString().ToInt(0), ex.Message);
                SetAlert("Đã có lỗi xảy ra", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }
        
        [HttpGet]
        public async Task<ActionResult> SignContract(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var signature = await SignatureTable.GetByUserId(aclog.ID);
                ViewBag.Signature = signature?.Signature ?? "";
                var data = await ContractCustomerTable.GetByStudentId(aclog.ID);

                int pageSize = 10;
                ViewBag.StartIndex = (page - 1) * pageSize;
                int total = data.Count;
                ViewBag.Total = total;
                int PageCount = 0;
                if (total % pageSize == 0)
                    PageCount = total / pageSize;
                else
                    PageCount = total / pageSize + 1;
                ViewBag.Page = page;
                ViewBag.PageCount = PageCount;
                return View(data.Skip((page - 1) * pageSize).Take(pageSize).ToList());
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ContractController", "SignContract", Session["UID"].ToString().ToInt(0), ex.Message + ex.InnerException);
                SetAlert("Đã có lỗi xảy ra", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }
        
        [System.Web.Http.HttpPost]
        public async Task<ActionResult> StudentSignContract([FromBody] StudentSignContract model)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 5)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

                var data = ContractCustomerTable.getbyid(model.id);
                data.ContractContent = data.ContractContent.Replace("[signature]", "<img src=\"" + model.sign + "\" />");
                await ContractCustomerTable.updateContent(model.id, data.ContractContent, aclog.UserName);
                await SignatureTable.Insert(aclog.ID, model.sign, aclog.UserName);

                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ContractController", "StudentSignContract", 0, ex.Message + ex.InnerException);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}