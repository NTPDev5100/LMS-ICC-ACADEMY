using AppZim.Areas.Admin.Controllers;
using AppZim.Models;
using AppZim.TableSql;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppZim.Controllers
{
    public class StudentExrciseController : BaseController
    {
        // GET: StudentExrcise
        public ActionResult ExrciseDetailEssay(int courseid, int eid)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var ck = CoursesStudentTable.getbystudentid(aclog.ID).Where(n => n.CourseID == courseid).FirstOrDefault();
            if (ck == null)
            {
                SetAlert("Bạn không đăng kí học khóa này", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var c = CoursesTable.getbyid(courseid);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var ex = ExerciseTable.getbyid(eid);
            if (ex == null)
            {
                SetAlert("Không tìm thấy câu hỏi", "e");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }
            var checkdate = CoursesExerciseDeadlineTable.getbycourseid(c.ID, ex.ID);
            if (checkdate == null)
            {
                SetAlert("Bài tập chưa được mở", "e");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }

            ViewBag.Course = c;
            ViewBag.Exercise = ex;
            ViewBag.FullName = aclog.FullName;
            ViewBag.Deadline = checkdate.eTime.Value.ToString("dd/MM/yyyy");
            return View();
        }

        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AnswerExrcise(FormCollection f, HttpPostedFileBase[] file)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int courseid = f.Get("hdfcourseid").ToInt(0);
            int hdfexeid = f.Get("hdfexeid").ToInt(0);
            string answer = f.Get("hdfanswer").ToString();

            string linkfile = "";
            if (!string.IsNullOrEmpty(f.Get("hdflink")))
                linkfile = f.Get("hdflink").ToString();

            var ck = CoursesStudentTable.getbystudentid(aclog.ID).Where(n => n.CourseID == courseid).FirstOrDefault();
            if (ck == null)
            {
                SetAlert("Bạn không đăng kí học khóa này", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var c = CoursesTable.getbyid(courseid);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var ex = ExerciseTable.getbyid(hdfexeid);
            if (ex == null)
            {
                SetAlert("Không tìm thấy câu hỏi", "e");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }
            var checkdate = CoursesExerciseDeadlineTable.getbycourseid(c.ID, ex.ID);
            if (checkdate == null)
            {
                SetAlert("Bài tập chưa được mở", "e");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }
            if (checkdate.eTime.Value < GetDateTime.Now.Date)
            {
                SetAlert("Đã hết hạn làm bài", "e");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }
            var checkex = ExerciseAddonStudentTable.getbyuidvsexeid(aclog.ID, ex.ID);
            if (checkex != null)
            {
                SetAlert("Thành công", "s");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }

            int idfix = 0;
            var creex = AccountTable.getbyusername(ex.CreatedBy);
            if (creex != null)
            {
                if (creex.RoleID == 4)// giáo viên
                    idfix = creex.ID;
            }

            if (string.IsNullOrEmpty(linkfile) && file == null && ex.Type == 3)
            {
                SetAlert("Vui lòng chọn file hoặc ghi âm", "e");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }
            var rs = ExerciseAddonStudentTable.insert(aclog, c, new tbl_SetPackage() { ID = 0, PackageName = "" }, ex, "", answer, 0, idfix, aclog.UserName);

            if (ex.Type == 3)// audio mới kiểm tra file 
            {
                foreach (var item in file)
                {
                    if (item != null)
                    {
                        string name = Path.GetFileNameWithoutExtension(item.FileName);
                        string ext = Path.GetExtension(item.FileName);
                        if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                        {
                            SetAlert("File của bạn đã bị chặn, vui lòng không tải lên file .php, .asp, .aspx, .ps1", "e");
                            return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
                        }
                        var fileName = Guid.NewGuid() + ext;
                        var path = Path.Combine(Server.MapPath("~/Upload/exercisestudent/"), fileName);
                        linkfile += "/Upload/exercisestudent/" + fileName + "|";
                        item.SaveAs(path);
                    }
                }

                if (!string.IsNullOrEmpty(linkfile))
                {
                    string[] arrlink = linkfile.Split('|');
                    for (int p = 0; p < arrlink.Length; p++)
                    {
                        //lưu file
                        if (!string.IsNullOrEmpty(arrlink[p]))
                            ExerciseAddonStudentFileAudioTable.insert(rs.ID, arrlink[p], aclog.UserName);
                    }
                }
            }

            if (rs != null)
            {
                if (idfix != 0)// gửi thông báo cho giáo viên
                {
                    var nt = NotificationTable.insertsingle("", "Thông báo sửa bài", "Có bài tập mới từ khóa " + rs.CourseName, rs.UIDFix.Value, aclog.UserName, 6, 0);

                }
                SetAlert("Nộp bài thành công", "s");
            }
            else
                SetAlert("Nộp bài không thành công", "e");
            return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
        }


        public ActionResult ExrciseDetailEssayFix(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var l = ExerciseAddonStudentTable.getbyid(id);
            if (l == null)
            {
                SetAlert("Không tìm thấy bài làm", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }
            if (l.UIDStudent != aclog.ID)
            {
                SetAlert("Bạn không xem được bài sửa", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }
            tbl_ExerciseAddonStudent addon = ExerciseAddonStudentTable.getbyid(id);
            ViewBag.StatusDone = addon.StatusDone;
            var lfix = ExerciseAddonStudentFixTable.getbyexestudent(l.ID);
            ViewBag.Fix = lfix;
            var lscore = ExerciseScoreDetailTable.getbyexeid(l.ID);
            ViewBag.Score = lscore;
            var ex = ExerciseTable.getbyid(l.ExerciseID.Value);
            ViewBag.Exercise = ex;
            tbl_Courses cs = CoursesTable.getbyid(l.CourseID.Value);
            ViewBag.Course = cs;
            return View(l);
        }

        public ActionResult ExrciseDetailEssayMulti(int courseid, int gid)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var ck = CoursesStudentTable.getbystudentid(aclog.ID).Where(n => n.CourseID == courseid).FirstOrDefault();
            if (ck == null)
            {
                SetAlert("Bạn không đăng kí học khóa này", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var c = CoursesTable.getbyid(courseid);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var gex = ExerciseGroupTable.getbyid(gid);
            if (gex == null)
            {
                SetAlert("Không tìm thấy bài tập", "e");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }
            string score = "N/A";
            int i = 0;
            var ckwork = ExerciseAddonStudentTable.checkworkexercise(aclog.ID, gid);
            if (ckwork)
            {
                var le = ExerciseTable.getbygroupid(gid);
                foreach (var item in le)
                {
                    var atrue = ExerciceMultiChoiseTable.getbyanwertrue(item.ID);
                    var astu = ExerciseAddonStudentTable.getbyuidvsexeid(aclog.ID, item.ID);
                    if (atrue.ID == astu.ExerciseMultipleChoiseID)
                        i += 1;
                }
                score = i + "/" + le.Count;
            }
            ViewBag.Score = score;
            ViewBag.Course = c;
            ViewBag.GroupExercise = gex;
            ViewBag.FullName = aclog.FullName;
            return View();
        }
        public ActionResult SubmitExrciseDetailEssayMulti(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int gid = f.Get("hdfgroupid").ToInt(0);
            int courseid = f.Get("hdfcourseid").ToInt(0);

            var ckstu = CoursesStudentTable.getbystudentid(aclog.ID).Where(n => n.CourseID == courseid).FirstOrDefault();
            if (ckstu == null)
            {
                SetAlert("Bạn không đăng kí học khóa này", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var c = CoursesTable.getbyid(courseid);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var gex = ExerciseGroupTable.getbyid(gid);
            if (gex == null)
            {
                SetAlert("Không tìm thấy bài tập", "e");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }
            string[] data = f.Get("hdfdata").ToString().Split('|');

            bool ck = false;
            for (int i = 0; i < data.Length - 1; i++)
            {
                string[] qa = data[i].Split('-');
                var q = ExerciseTable.getbyid(qa[0].ToInt(0));
                var a = ExerciceMultiChoiseTable.getbyid(qa[1].ToInt(0));
                if (q == null || a == null)
                {
                    ck = true;
                    break;
                }
            }
            if (ck)
            {
                SetAlert("Dữ liệu không hợp lệ. Vui lòng thử lại", "e");
                return RedirectToAction("ExrciseDetailEssayMulti", "StudentExrcise", new { courseid = c.ID, gid = gex.ID });
            }
            for (int i = 0; i < data.Length - 1; i++)
            {
                string[] qa = data[i].Split('-');
                var q = ExerciseTable.getbyid(qa[0].ToInt(0));
                var anwer = ExerciceMultiChoiseTable.getbyid(qa[1].ToInt(0));
                ExerciseAddonStudentTable.insert(aclog, c, new tbl_SetPackage() { ID = 0, PackageName = "" }, q, "", anwer.AnswerTitle, anwer.ID, 0, aclog.UserName);
            }

            SetAlert("Thành công", "s");
            return RedirectToAction("ExrciseDetailEssayMulti", "StudentExrcise", new { courseid = c.ID, gid = gex.ID });
        }

        [HttpPost]
        public ActionResult SumitAcceptRedo(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var exstu = ExerciseAddonStudentTable.getbyid(f.Get("hdfExID").ToInt(0));
            if (exstu == null)
            {
                SetAlert("Không tìm thấy bài tập", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }
            int redo = f.Get("hdfRedo").ToInt(0);
            int rate = f.Get("hdfRate").ToInt(5);

            int status = 5; // chấp nhận bài sửa, chấm xong
            if (redo == 1)
                status = 4;// yêu cầu chấm lại
            string comment = f.Get("hdfNote").ToString();
            var rs = ExerciseAddonStudentTable.updatestatusdone(exstu.ID, exstu.UIDFix.Value, status, rate, aclog.UserName);
            if (rs != null)
            {
                if (!string.IsNullOrEmpty(comment))// thêm phải hồi học viên
                    ExerciseAddonStudentCommentRedoTable.insert(rs.ID, comment, aclog.UserName);
                if (status == 4)// thông báo cho thằng sửa lại
                {
                    var nt = NotificationTable.insertsingle("", "Thông báo sửa lại bài", "Bạn được yêu cầu sửa lại bài, " + rs.CourseName, rs.UIDFix.Value, aclog.UserName, 12, rs.ID);
                    
                }
                SetAlert("Thành công", "s");
            }
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("ExrciseDetailEssayFix", "StudentExrcise", new { id = exstu.ID });
        }

        // GET: StudentExrcise
        public ActionResult ExrciseDetailAudio(int courseid, int eid)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var ck = CoursesStudentTable.getbystudentid(aclog.ID).Where(n => n.CourseID == courseid).FirstOrDefault();
            if (ck == null)
            {
                SetAlert("Bạn không đăng kí học khóa này", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var c = CoursesTable.getbyid(courseid);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var ex = ExerciseTable.getbyid(eid);
            if (ex == null)
            {
                SetAlert("Không tìm thấy câu hỏi", "e");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }
            var checkdate = CoursesExerciseDeadlineTable.getbycourseid(c.ID, ex.ID);
            if (checkdate == null)
            {
                SetAlert("Bài tập chưa được mở", "e");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }

            ViewBag.Course = c;
            ViewBag.Exercise = ex;
            ViewBag.FullName = aclog.FullName;
            ViewBag.Deadline = checkdate.eTime.Value.ToString("dd/MM/yyyy");
            return View();
        }


        #region upload file audio qua ajax
        [HttpPost]
        public ActionResult UploadFileAudio()
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
                        string name = Path.GetFileNameWithoutExtension(file.FileName);
                        string ext = ".webm";
                        if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                        {
                            SetAlert("File của bạn đã bị chặn, vui lòng không tải lên file .php, .asp, .aspx, .ps1", "e");
                            return RedirectToAction("OrderDetail", "OrderList");
                        }
                        var fileName = name + GetDateTime.Now.ToString("dd-MM-yyyy hhmmss") + Guid.NewGuid() + ext;
                        var path = Path.Combine(Server.MapPath("~/Upload/exercisestudent/"), fileName);
                        link = "/Upload/exercisestudent/" + fileName;
                        file.SaveAs(path);
                    }
                }
            }
            return Json(new { rs = true, l = link }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult Exercise(int id = 0)
        {

            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            if (id != 0)
            {
                var c = CoursesTable.getbyid(id);
                if (c == null)
                {
                    SetAlert("Không tìm thấy khóa học này", "e");
                    return RedirectToAction("CourseList", "StudentCourse");
                }
                ViewBag.Course = c;
                return View(c);

            }
            else
            {
                var lc = CoursesStudentTable.getbystudentid(aclog.ID);
                if (lc.Count > 0)
                {
                    var c = CoursesTable.getbyid(lc[0].CourseID.Value);
                    if (c == null)
                    {
                        SetAlert("Không tìm thấy khóa học này", "e");
                        return RedirectToAction("CourseList", "StudentCourse");
                    }
                    ViewBag.Course = c;
                    return View(c);
                }
                else
                {
                    SetAlert("Không có khóa học", "e");
                    return RedirectToAction("CourseList", "StudentCourse");
                }
            }
        }
    }
}