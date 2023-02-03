using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static AppZim.Areas.Admin.Controllers.CourseController;

namespace AppZim.Areas.Admin.Controllers
{
    public class EmailController : BaseController
    {
        // GET: Admin/Email
        public ActionResult MailList()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 7)
                return RedirectToAction("Signin", "Login", new { area = "" });
            return View();
        }

        /// <summary>
        /// Thêm, cập nhật template
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AddTemplateMail(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });
                int Id = f.Get("hdfId").ToInt(0);
                int type = f.Get("ddl-type").ToInt(0);
                string title = f.Get("txt-title").ToString();
                string content = f.Get("hdfcontent").ToString();
                var rs = EmailTemplateTable.InsertUpdate(Id, type, title, content, aclog.UserName);
                if (rs != null)
                {
                    SetAlert("Thành công", "s");
                    //ActivitiesLog.WritelogEmailTemplate(rs, aclog);
                }
                else
                    SetAlert("Không thành công", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("EmailController", "AddTemplateMail", Session["UID"].ToString().ToInt(0), ex.Message);
                SetAlert("Đãy ra lỗi, vui lòng liên hệ quản trị viên", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
        }
        /// <summary>
        /// Giao diện gửi mail
        /// </summary>
        /// <returns></returns>
        public ActionResult SendMail()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 7)
                return RedirectToAction("Signin", "Login", new { area = "" });
            return View();
        }
        /// <summary>
        /// Load dữ liệu theo template
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="temId"></param>
        /// <param name="datekhaigiang"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult loadtemplate(int courseId, int temId, string datekhaigiang)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                string ct = "";
                string tt = "";
                using (var db = new ZimEntities())
                {
                    var course = db.tbl_Courses.Where(x => x.ID == courseId).FirstOrDefault();
                    var template = db.tbl_EmailTemplate.Where(x => x.ID == temId).FirstOrDefault();
                    if (course == null || template == null)
                    {
                        return Json(new { rs = false, m = "Không tìm thấy dữ liệu!" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        string ngaykhaigiang = "";
                        string listCahoc = "";
                        string giaovienday = "";
                        if (temId == 1 || temId == 2 || temId == 3)
                        {
                            var lichhoc = db.tbl_CoursesSchedule.Where(x => x.CourseID == course.ID).Select(i => new { i.CourseID, i.Date, i.StudyTimeID }).ToList();
                            ngaykhaigiang = lichhoc[0].Date.Value.ToString("dd/MM/yyyy");
                            //lịch học
                            //Gộp tiết học lại theo ca trên lịch học
                            List<CheckScheduleCourse> lh = new List<CheckScheduleCourse>();
                            foreach (var h in lichhoc)
                            {
                                CheckScheduleCourse t = new CheckScheduleCourse();
                                t.CourseID = h.CourseID.Value;
                                t.StudyTimeID = h.StudyTimeID.Value;
                                t.Date = h.Date.Value;
                                bool ck = true;
                                var lc = lh.Where(n => n.CourseID == course.ID).ToList();
                                Parallel.ForEach(lc, (i, state) =>
                                {
                                    if (i.CourseID == t.CourseID && i.Date == t.Date && i.StudyTimeID == t.StudyTimeID)
                                    {
                                        ck = false;
                                        state.Break(); //nếu ca đó có rồi thì bỏ qua
                                    }
                                });

                                if (ck)
                                    lh.Add(t);
                            }


                            //chuyển lịch học đã gộp ở trên thành dạng danh sách để show ra
                            StringBuilder cahoc = new StringBuilder();
                            cahoc.Append("<ul style=\"list-style-type:disc;color:red\">");
                            if (!string.IsNullOrEmpty(datekhaigiang)) //nếu nó có chọn ngày khai giảng
                            {
                                DateTime date = DateTime.ParseExact(datekhaigiang, "dd/MM/yyyy", null);
                                ngaykhaigiang = date.ToString("dd/MM/yyyy");
                                bool checkDate = false; //check xem trong các ngày học có ngày nào trùng ngày khai giảng nó đã chọn không
                                Parallel.ForEach(lh, (item, state) =>
                                {
                                    if (item.Date == date)
                                    {
                                        checkDate = true;
                                        state.Break();
                                    }
                                });
                                if (checkDate == true)
                                {
                                    for (var i = 0; i < lh.Count; i++)
                                    {
                                        tbl_StudyTime study = StudyTimeTable.getbyid(lh[i].StudyTimeID);
                                        if (date == lh[i].Date)
                                        {
                                            cahoc.Append("<li>[" + lh[i].Date.ToString("dd/MM/yyyy") + "]: [" + study.sTime + "_" + study.eTime + "] (Ngày khai giảng)</li>");
                                        }
                                        else
                                        {
                                            cahoc.Append("<li>[" + lh[i].Date.ToString("dd/MM/yyyy") + "]: [" + study.sTime + "_" + study.eTime + "]</li>");
                                        }
                                    }
                                }
                                else
                                {
                                    cahoc.Append("<li>[" + date.Date.ToString("dd/MM/yyyy") + "] (Ngày khai giảng)</li>");
                                    for (var i = 0; i < lh.Count; i++)
                                    {
                                        tbl_StudyTime study = StudyTimeTable.getbyid(lh[i].StudyTimeID);
                                        cahoc.Append("<li>[" + lh[i].Date.ToString("dd/MM/yyyy") + "]: [" + study.sTime + "_" + study.eTime + "]</li>");
                                    }
                                }
                            }
                            else //nếu nó không chọn thì mặc định ngày đầu là ngày khai giảng
                            {
                                for (var i = 0; i < lh.Count; i++)
                                {
                                    tbl_StudyTime study = StudyTimeTable.getbyid(lh[i].StudyTimeID);
                                    if (i == 0)
                                    {
                                        cahoc.Append("<li>[" + lh[i].Date.ToString("dd/MM/yyyy") + "]: [" + study.sTime + "_" + study.eTime + "] (Ngày khai giảng)</li>");
                                    }
                                    else
                                    {
                                        cahoc.Append("<li>[" + lh[i].Date.ToString("dd/MM/yyyy") + "]: [" + study.sTime + "_" + study.eTime + "]</li>");
                                    }
                                }
                            }
                            cahoc.Append("</ul>");
                            listCahoc = cahoc.ToString();

                            //lấy tên những giáo viên khóa này ra
                            if (temId == 1 || temId == 2) //mẫu gửi cho học viên
                            {
                                var teacher = db.tbl_TeacherCourse.Where(x => x.CourseID == course.ID).Select(i => i.TeacherUID).ToList();
                                if (teacher.Count() > 0)
                                {
                                    for (var t = 0; t < teacher.Count; t++)
                                    {
                                        int uid = teacher[t].Value;
                                        var name = db.tbl_Account.Where(x => x.ID == uid).Select(x => x.FullName).FirstOrDefault();
                                        if (t == 0)
                                        {
                                            giaovienday += name;
                                        }
                                        else
                                        {
                                            giaovienday += ", " + name;
                                        }
                                    }
                                }
                            }
                        }


                        //tiêu đề mail
                        tt = template.EmailTitle;
                        tt = tt.Replace("[ten-khoa-hoc]", course.CourseName);
                        //tt = tt.Replace("[ten-giang-vien]", course.CourseName); //phần này lúc gửi sẽ thay tên từng gv vào
                        tt = tt.Replace("[ngay-khai-giang]", ngaykhaigiang);

                        //nội dung mail
                        ct = template.EmailContent;
                        ct = ct.Replace("[ten-khoa-hoc]", course.CourseName);
                        var school = SchoolTable.getbyID(course.SchoolID.Value);
                        ct = ct.Replace("[dia-diem-hoc]", school.SchoolName + " - " + school.SchoolAddress);
                        ct = ct.Replace("[thoi-gian-hoc]", listCahoc);
                        ct = ct.Replace("[ten-giang-vien]", giaovienday);
                        ct = ct.Replace("[ma-lop-hoc]", "#" + course.ID);
                        ct = ct.Replace("[ngay-khai-giang]", ngaykhaigiang);

                        return Json(new { rs = true, title = tt.ToString(), content = ct.ToString(), khaigiang = ngaykhaigiang }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("EmailController", "loadtemplate", Session["UID"].ToString().ToInt(0), ex.Message);
                return Json(new { rs = false, m = "Đãy ra lỗi, vui lòng liên hệ quản trị viên" }, JsonRequestBehavior.AllowGet);
            }
        }

        public class CheckScheduleCourse
        {
            public int CourseID { get; set; }
            public int StudyTimeID { get; set; }
            public DateTime Date { get; set; }
        }

        /// <summary>
        /// Hàm gửi mail
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        /// 
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult funtionSendMail(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 7)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });
                int courseId = f.Get("ddl-course").ToInt(0);
                int templateId = f.Get("ddl-type").ToInt(0); //nó là ID
                string title = f.Get("txt-title").ToString();
                DateTime ngaykhaigiang = DateTime.Now;
                if (!string.IsNullOrEmpty(f.Get("txt-ngaykhaigiang")))
                {
                    ngaykhaigiang = DateTime.ParseExact(f.Get("txt-ngaykhaigiang").ToString(), "dd/MM/yyyy", null);
                }
                string content = f.Get("hdfcontent").ToString();
                using (var db = new ZimEntities())
                {
                    var tem = db.tbl_EmailTemplate.Where(x => x.ID == templateId).FirstOrDefault();
                    if (tem.Type != 3) //gửi cho học viên
                    {
                        //lấy danh sách học viên học khóa này
                        var listHV = db.tbl_CoursesStudent.Where(x => x.CourseID == courseId).Select(i => new { i.StudentUID }).Distinct().ToList();
                        if (listHV.Count() > 0)
                        {
                            Parallel.ForEach(listHV, uid =>
                            {
                                var ac = AccountTable.getbyID(uid.StudentUID.Value);
                                //var mail = db.tbl_Account.Where(x => x.ID == uid.StudentUID.Value && x.Status == 2 && x.IsDelete == false).Select(i => i.Email).FirstOrDefault();
                                if (!string.IsNullOrEmpty(ac.Email))
                                {
                                    AppZim.ZIM.AssetCRM.SendMail(ac.Email, title, content);
                                }
                            });
                        }
                    }
                    else //gửi cho giảng viên
                    {
                        var listGV = db.tbl_TeacherCourse.Where(x => x.CourseID == courseId).Select(i => i.TeacherUID).ToList();
                        if (listGV.Count() > 0)
                        {
                            Parallel.ForEach(listGV, uid =>
                            {
                                var user = db.tbl_Account.Where(x => x.ID == uid && x.Status == 2 && x.IsDelete == false).Select(i => new { i.Email, i.FullName }).FirstOrDefault();
                                if (!string.IsNullOrEmpty(user.Email))
                                {
                                    title = title.Replace("[ten-giang-vien]", user.FullName);
                                    content = content.Replace("[ten-giang-vien]", user.FullName);
                                    AppZim.ZIM.AssetCRM.SendMail(user.Email, title, content);
                                }
                            });
                        }
                    }

                    var rs = HistoryPushMailTable.Insert(courseId, templateId, tem.TypeName, ngaykhaigiang, title, content, aclog.UserName);
                    if (rs != null)
                    {
                        SetAlert("Thành công", "s");
                    }
                    else
                        SetAlert("Không thành công", "e");
                }
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("EmailController", "funtionSendMail", Session["UID"].ToString().ToInt(0), ex.Message);
                SetAlert("Đãy ra lỗi, vui lòng liên hệ quản trị viên", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
        }

        public ActionResult HistoryPushMail()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 7)
                return RedirectToAction("Signin", "Login", new { area = "" });
            return View();
        }
        public ActionResult LoadHistoryPushMail()
        {
            List<get_list_history_push_mail_datatable_Result> l = new List<get_list_history_push_mail_datatable_Result>();
            try
            {
                if (Session["UID"] == null)
                    return Json(new { data = l.ToArray(), redirect = "/Login/Signin" }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return Json(new { data = l.ToArray(), redirect = "/Login/Signin" }, JsonRequestBehavior.AllowGet);
                OjectDataTable dataTable = new OjectDataTable();
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
                    OrderBy += 7;// 7 là số colunm màn hình
                }
                string search = "";
                //key search
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                {
                    ViewBag.Search = Request.QueryString["search"];
                    search = Request.QueryString["search"].ToString().Trim();

                }
                int classId = 0;
                if (Request.QueryString["class"].ToInt(0) != 0)
                {
                    classId = Request.QueryString["class"].ToInt(0);

                }
                // thời gian bắt đầu từ
                string fromdate = null;
                if (!string.IsNullOrEmpty(Request.QueryString["from"]))
                {
                    try
                    {
                        DateTime fdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                        fromdate = fdate.ToString();
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
                        todate = tdate.ToString();
                    }
                    catch { }
                }
                using (var db = new ZimEntities())
                {
                    l = db.get_list_history_push_mail_datatable(search, classId, fromdate, todate, OrderBy, Length, Start).ToList();
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
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("EmailController", "LoadHistoryPushMail", uid, ex.Message);
                return Json(new { data = l.ToArray(), redirect = "/Login/Signin" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult loadContent(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 7)
                return RedirectToAction("Signin", "Login", new { area = "" });
            using (var db = new ZimEntities())
            {
                var content = db.tbl_HistoryPushMail.Where(x => x.ID == id).Select(i => new { i.EmailContent, i.EmailTitle }).FirstOrDefault();
                return Json(new { rs = true, title = content.EmailTitle, data = content.EmailContent.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// load template cho mẫu mail id = 3
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult loadEmailTemplate(int id, int testid)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 7)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var test = StudentAppointmentTestTable.getbyid(testid);
            var teacher = AccountTable.getbyID(test.TeacherID.Value);
            var content = "";
            var title = "";
            var student = AccountTable.getbyID(test.StudentUID.Value);
            switch (student.Language)
            {
                case 7:
                    id = 17;
                    break;
                case 6:
                    id = 10;
                    break;
                default:
                    id = 3;
                    break;
            }
            var email = EmailTemplateTable.GetByID(id);
            title = email.EmailTitle;
            content = email.EmailContent;
            content = content.Replace("[ten-hoc-vien]", test.StudentName);
            content = content.Replace("[ten-giang-vien]", teacher == null ? "" : teacher.FullName);
            content = content.Replace("[thoi-gian-hoc]", test.DateTest.Value.ToString("dd/MM/yyyy") + " " + test.TimeTest);
            return Json(new { rs = true, title, content }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// load template cho mẫu mail id = 2
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult loadEmailTemplateDone(int id, int testid)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 6 && aclog.RoleID != 7)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var test = StudentAppointmentTestTable.getbyid(testid);
            var detail = StudentAppointmentTestDetailTable.getbyAppointmentTestID(testid);
            var student = AccountTable.getbyID(test.StudentUID.Value);
            var teacher = AccountTable.getbyID(detail.TeacherTestID.Value);
            var language = Language.GetByID(student.Language.Value);
            var lop = ClassTable.getbyID(student.ScoreOut.ToInt(0));
            var curriculumn = CurriculumTable.getmaxlesson(lop.ID);
            switch(student.Language)
            {
                case 7:
                    id = 16;
                    break;
                case 6:
                    id = 9;
                    break;
                default:
                    id = 2;
                    break;
            }
            //lấy mẫu email
            var email = EmailTemplateTable.GetByID(id);
            var tt = email.EmailTitle;
            var ct = email.EmailContent;
            ct = ct.Replace("[ten-hoc-vien]", test.StudentName);
            ct = ct.Replace("[ngon-ngu]", language.Name);
            ct = ct.Replace("[ten-giao-vien]", teacher.FullName);
            ct = ct.Replace("[nhan-xet]", detail.Note);
            ct = ct.Replace("[ten-khoa-hoc]", lop.ClassName);
            ct = ct.Replace("[so-buoi-hoc]", curriculumn.TotalLesson.Value.ToString());
            ct = ct.Replace("[thoi-luong-hoc]", curriculumn.StyleLensson.Value.ToString());
            return Json(new { rs = true, title = tt, content = ct }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// load template cho mẫu mail id = 6
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult loadEmailTemplateEnd(int id, int coursestudentid)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 7)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var courseStudent = CoursesStudentTable.getbyid(coursestudentid);
            var schedule = CoursesScheduleTable.getbycourseid(courseStudent.CourseID.Value);
            var student = AccountTable.getbyID(courseStudent.StudentUID.Value);
            var content = "";
            var title = "";
            switch (student.Language)
            {
                case 7:
                    id = 20;
                    break;
                case 6:
                    id = 13;
                    break;
                default:
                    id = 6;
                    break;
            }
            var email = EmailTemplateTable.GetByID(id);
            title = email.EmailTitle;
            content = email.EmailContent;
            content = content.Replace("[ten-khoa-hoc]", courseStudent.CourseName);
            content = content.Replace("[so-buoi-hoc]", schedule.Count.ToString());
            content = content.Replace("[so-buoi-con-lai]", schedule.Where(x => x.FisnishStatus == 0 && x.IsHide == false).ToList().Count.ToString());
            return Json(new { rs = true, title, content }, JsonRequestBehavior.AllowGet);
        }
    }
}