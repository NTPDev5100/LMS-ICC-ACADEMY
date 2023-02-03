using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebUI.Business;

namespace AppZim.Areas.Admin.Controllers
{
    public class TeacherExerciseController : BaseController
    {
        // GET: Admin/TeacherExercise
        public ActionResult ExerciseList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //page size
            int pageSize = 20;

            //lấy bài mới 
            //List<tbl_ExerciseAddonStudent> l = new List<tbl_ExerciseAddonStudent>();
            IEnumerable<tbl_ExerciseAddonStudent> exnew = ExerciseAddonStudentTable.getbystatus(1).Where(n => n.StatusDone == 1 && (n.Type == 1 || n.Type == 3)).ToList();
            IEnumerable<tbl_TeacherCourse> teachercourse = TeacherCourseTable.getbyteacherid(aclog.ID).Where(n => n.TeacherUID == aclog.ID && n.isHide != true).ToList();
            //lấy ra nhưng bài tập có thể nhận được
            var l = from ex in exnew
                    join teacher in teachercourse on ex.CourseID equals teacher.CourseID
                    select ex;
            //l.AddRange(exs);

            //key search
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from ex in l
                    where AssetCRM.RemoveUnicode(ex.StudentName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.StudentName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.CourseName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.FullNameCreatedByExercise).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select ex;
            }
            if (Request.QueryString["course"].ToInt(0) != 0)
            {
                ViewBag.CourseID = Request.QueryString["course"].ToInt(0);
                int cid = Request.QueryString["course"].ToInt(0);
                l = from ex in l
                    where ex.CourseID == cid
                    select ex;
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
            l = l.OrderByDescending(n => n.CreatedDate).ToList();
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);
                if (sort == 1)
                    l = l.OrderBy(n => n.StudentName).ToList();
                if (sort == 2)
                    l = l.OrderByDescending(n => n.StudentName).ToList();
                if (sort == 3)
                    l = l.OrderBy(n => n.CreatedDate).ToList();
                if (sort == 4)
                    l = l.OrderByDescending(n => n.CreatedDate).ToList();
                if (sort == 5)
                    l = l.OrderBy(n => n.FullNameCreatedByExercise).ToList();
                if (sort == 6)
                    l = l.OrderByDescending(n => n.FullNameCreatedByExercise).ToList();
            }
            ViewBag.Sort = sort;
            ViewBag.ToTal = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }

        public ActionResult ExerciseDetail(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var l = ExerciseAddonStudentTable.getbyid(id);
            if (l == null)
            {
                SetAlert("Không tìm thấy bài", "e");
                return RedirectToAction("ExerciseList", "TeacherExercise");
            }
            if (l.UIDFix != aclog.ID)
                return RedirectToAction("ExerciseList", "TeacherExercise");

            //if (l.StatusDone == 1)
            //    ExerciseAddonStudentTable.updatestatusdone(id, aclog.ID, 2, 0, aclog.UserName);

            ViewBag.ExerciseStudent = l;
            return View();
        }
        public ActionResult ExerciseAudioDetail(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var l = ExerciseAddonStudentTable.getbyid(id);
            if (l == null)
            {
                SetAlert("Không tìm thấy bài", "e");
                return RedirectToAction("ExerciseList", "TeacherExercise");
            }
            //if (l.StatusDone == 1)
            //    ExerciseAddonStudentTable.updatestatusdone(id, aclog.ID, 2, 0, aclog.UserName);

            var ex = ExerciseTable.getbyid(l.ExerciseID.Value);
            ViewBag.Exercise = ex;
            ViewBag.ExerciseStudent = l;

            return View(l);
        }
        [HttpPost]
        public ActionResult AcceptFix(int id)
        {
            if (Session["UID"] == null)
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 4)// khác giáo viên bị đá
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            var l = ExerciseAddonStudentTable.getbyid(id);
            if (l == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            if (l.StatusDone == 1)
            {
                ExerciseAddonStudentTable.updatestatusdone(l.ID, aclog.ID, 2, 0, aclog.UserName);
                return Json(new { rs = true, link = "/Admin/TeacherExercise/ExerciseDetail/" + l.ID + "" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            //if (l.Type == 1)
            //{
            //    //return RedirectToAction("ExerciseDetail", "TeacherExercise", new { id = l.ID });
            //    return Json(new { rs = true, link = "/Admin/TeacherExercise/ExerciseDetail/" + l.ID + "" }, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    //return RedirectToAction("ExerciseAudioDetail", "TeacherExercise", new { id = l.ID });
            //    return Json(new { rs = true, link = "/Admin/TeacherExercise/ExerciseAudioDetail/" + l.ID + "" }, JsonRequestBehavior.AllowGet);
            //}            
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
                        //string name = Path.GetFileNameWithoutExtension(file.FileName);
                        string ext = ".webm";
                        var fileName = Guid.NewGuid() + ext;
                        var path = Path.Combine(Server.MapPath("~/Upload/exerciseaudiofix/"), fileName);
                        link = "/Upload/exerciseaudiofix/" + fileName;
                        file.SaveAs(path);
                    }
                }
            }
            return Json(new { rs = true, l = link }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region chấm xong và lưu nháp
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult FixDoneExercise(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var ex = ExerciseAddonStudentTable.getbyid(f.Get("hdfid").ToInt(0));
            if (ex == null)
            {
                SetAlert("Không tìm thấy bài", "e");
                return RedirectToAction("ExerciseList", "TeacherExercise");
            }

            string[] data = f.Get("hdfdata").ToString().Split('ḷ');
            //ẩn cũ đi
            ExerciseAddonStudentFixTable.updatehide(ex.ID, aclog.UserName);
            for (int i = 0; i < data.Length - 1; i++)
            {
                var item = data[i];
                string[] dt = item.Split('ṭ');
                // thêm mới comment
                //ExerciseAddonStudentFixTable.insert(ex.ID, dt[0], dt[1], dt[2], aclog.UserName);
                //danh sách link ở version 2.0
                var cmt = ExerciseAddonStudentFixTable.insert(ex.ID, dt[0], dt[1], "", aclog.UserName);
                string[] arlink = dt[2].ToString().Split('|');
                for (int j = 0; j < arlink.Length - 1; j++)
                {
                    if (!string.IsNullOrEmpty(arlink[j]))
                        ExerciseAddonStudentFixLinkAudioTable.insert(ex.ID, cmt.ID, arlink[j], aclog.UserName);
                }
            }

            string[] datascore = f.Get("hdfscore").ToString().Split('ḷ');
            //ẩn điểm cũ nếu có
            ExerciseScoreDetailTable.updatehide(ex.ID, aclog.UserName);
            for (int i = 0; i < datascore.Length - 1; i++)
            {
                var item = datascore[i];
                string[] dt = item.Split('ṭ');
                //thêm điểm mới
                ExerciseScoreDetailTable.insert(ex.ID, dt[0], dt[1].Replace(",", ".").ToFloat(0), aclog.UserName);
            }

            string fix = f.Get("hdffix").ToString();
            string note = f.Get("hdfnote").ToString();
            double score = f.Get("txt-totalscore").ToFloat(0);

            var rs = ExerciseAddonStudentTable.updatefix(ex.ID, fix, note, score, aclog.UserName);
            if (rs != null)
            {
                var nt = NotificationTable.insertsingle("", "Thông báo sửa bài", "Bài tập của bạn đã được sửa " + rs.CourseName, rs.UIDStudent.Value, aclog.UserName, 7, rs.ID);

                SetAlert("Sửa bài thành công", "s");
            }
            else
                SetAlert("Sửa bài không thành công", "e");
            return RedirectToAction("ExerciseList", "TeacherExercise");
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult SaveDraft(int hdfid, string hdfdata, string hdffix, string hdfnote)
        {
            string username = "sessionempty";
            if (Session["UID"] != null)
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog != null)// học viên bị đá ra
                    username = aclog.UserName;
            }

            var exe = ExerciseAddonStudentTable.getbyid(hdfid);
            if (exe == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            var fixbase64 = System.Convert.FromBase64String(hdffix);
            string fix = System.Text.Encoding.UTF8.GetString(fixbase64);

            var notebase64 = System.Convert.FromBase64String(hdfnote);
            string note = System.Text.Encoding.UTF8.GetString(notebase64);

            var ex = ExerciseAddonDraftTable.insert(exe.ID, exe.ExerciseQuestion, exe.FileAudio, fix, note, 0, username);

            //xóa dữ liệu nháp cũ và file cũ đi
            var datadeletes = ExerciseAddonDraftFixTable.getbyexedraftid(ex.ID);
            foreach (var datadelete in datadeletes)
            {
                //xóa file
                string filePath = Server.MapPath(datadelete.LinkAudio);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
                ExerciseAddonDraftFixTable.deletedata(datadelete.ID);
            }
            var dataaudios = ExerciseAddonDraftFixLinkAudioTable.getbyexedraftid(ex.ID);
            foreach (var dataaudio in dataaudios)
            {
                //xóa file
                string filePath = Server.MapPath(dataaudio.LinkFile);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
                ExerciseAddonDraftFixLinkAudioTable.deletedata(dataaudio.ID);
            }
            //

            string[] data = hdfdata.Split('ḷ');
            for (int i = 0; i < data.Length - 1; i++)
            {
                var item = data[i];
                string[] dt = item.Split('ṭ');
                var cmt = ExerciseAddonDraftFixTable.insert(ex.ID, dt[0], dt[1], "", username);
                string[] arlink = dt[2].ToString().Split('|');
                for (int j = 0; j < arlink.Length - 1; j++)
                {
                    if (!string.IsNullOrEmpty(arlink[j]))
                        ExerciseAddonDraftFixLinkAudioTable.insert(ex.ID, cmt.ID, arlink[j], username);
                }
            }
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadDraft(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            StringBuilder m = new StringBuilder();
            var l = ExerciseAddonDraftTable.getbyexestudentdi(id);
            m.Append("<option value=\"0\">------</option>");
            foreach (var item in l)
            {
                m.Append("<option value=\"" + item.ID + "\">" + item.DraftName + "</option>");
            }
            return Json(new { rs = true, l = m.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadDraftFirst(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            StringBuilder m = new StringBuilder();
            var l = ExerciseAddonDraftTable.getbyexestudentdi(id);
            if (l.Count > 0)
            {
                l = l.OrderBy(n => n.ModifiedDate).ToList();
                return Json(new { rs = true, l = l.FirstOrDefault().ID }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            
        }
        public class DraftDetail
        {
            public tbl_ExerciseAddonDraft Draft { get; set; }
            public List<ListFixDetail> ListFix { get; set; }
        }
        public class ListFixDetail
        {
            public tbl_ExerciseAddonDraftFix DraftFix { get; set; }
            public string ListAudio { get; set; }
            public string FullName { get; set; }
            public string Role { get; set; }
            public string av { get; set; }
        }

        [HttpPost]
        public ActionResult LoadDraftDetail(int id)
        {
            DraftDetail l = new DraftDetail();
            int dem = 0;
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            var d = ExerciseAddonDraftTable.getbyid(id);
            if (d == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            l.Draft = d;
            var de = ExerciseAddonDraftFixTable.getbyexedraftid(d.ID);
            if (de.Count > 0)
                dem = de.OrderByDescending(n => n.NumberaCMT).FirstOrDefault().NumberaCMT.Value;

            List<ListFixDetail> ListFix = new List<ListFixDetail>();
            foreach (var item in de)
            {
                ListFixDetail f = new ListFixDetail();
                f.DraftFix = item;
                List<tbl_ExerciseAddonDraftFixLinkAudio> taudio = ExerciseAddonDraftFixLinkAudioTable.getbycmtid(item.ID);
                StringBuilder p = new StringBuilder();
                foreach (var atem in taudio)
                {
                    p.Append("<div class=\"audio-result mg-t-10\" data-index=\"-1\" data-audio = \"" + atem.LinkFile + "\"><audio controls = \"\" class=\"valign-center\" style=\"display: block; width: 100%;\">");
                    p.Append("  <source src = \"" + atem.LinkFile + "\" type=\"audio/webm\"></audio>");
                    p.Append("  <a download = \"audio.webm\" class=\"mg-t-10\" href=\"" + atem.LinkFile + "\" style=\"display: inline-block;\">");
                    p.Append("      <i class=\"fas fa-download\"></i> Download");
                    p.Append("  </a>");
                    p.Append("  <a class=\"mg-t-10 mg-l-15 delete-audio\" href=\"javascript:;\" style=\"display: inline-block; cursor: pointer;\">");
                    p.Append("      <i class=\"fas fa-trash-alt\"></i> Delete");
                    p.Append("  </a>");
                    p.Append("</div>");
                }
                f.ListAudio = p.ToString();

                var accre = AccountTable.getbyusername(item.CreatedBy);
                if (accre != null)
                {
                    f.FullName = accre.FullName;
                    f.Role = AccountTable.getrolehtml(accre.RoleID.Value);
                    f.av = "/app-assets/zimv2/images/invis-user.png";
                    if (!string.IsNullOrEmpty(accre.Avatar))
                        f.av = accre.Avatar;
                }
                else
                {
                    f.FullName = aclog.FullName;
                    f.Role = AccountTable.getrolehtml(aclog.RoleID.Value);
                    f.av = "/app-assets/zimv2/images/invis-user.png";
                    if (!string.IsNullOrEmpty(aclog.Avatar))
                        f.av = aclog.Avatar;
                }
                ListFix.Add(f);
            }
            l.ListFix = ListFix;
            return Json(new { rs = true, l = l, dem = dem }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult ExerciseProcess(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //page size
            int pageSize = 20;

            //lấy bài mới 
            //List<tbl_ExerciseAddonStudent> l = new List<tbl_ExerciseAddonStudent>();
            IEnumerable<tbl_ExerciseAddonStudent> l = ExerciseAddonStudentTable.getbystatus(2).Where(n => n.StatusDone == 2 && n.UIDFix == aclog.ID && (n.Type == 1 || n.Type == 3)).ToList();
            //key search
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from ex in l
                    where AssetCRM.RemoveUnicode(ex.StudentName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.StudentName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.CourseName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.FullNameCreatedByExercise).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select ex;
            }
            if (Request.QueryString["course"].ToInt(0) != 0)
            {
                ViewBag.CourseID = Request.QueryString["course"].ToInt(0);
                int cid = Request.QueryString["course"].ToInt(0);
                l = from ex in l
                    where ex.CourseID == cid
                    select ex;
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
                    l = l.OrderBy(n => n.StudentName).ToList();
                if (sort == 2)
                    l = l.OrderByDescending(n => n.StudentName).ToList();
                if (sort == 3)
                    l = l.OrderBy(n => n.CreatedDate).ToList();
                if (sort == 4)
                    l = l.OrderByDescending(n => n.CreatedDate).ToList();
                if (sort == 5)
                    l = l.OrderBy(n => n.FullNameCreatedByExercise).ToList();
                if (sort == 6)
                    l = l.OrderByDescending(n => n.FullNameCreatedByExercise).ToList();
            }
            ViewBag.Sort = sort;
            ViewBag.ToTal = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }

        public ActionResult ExerciseRedoFix(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //page size
            int pageSize = 20;

            //lấy bài mới 
            //List<tbl_ExerciseAddonStudent> l = new List<tbl_ExerciseAddonStudent>();
            IEnumerable<tbl_ExerciseAddonStudent> l = ExerciseAddonStudentTable.getbystatus(4).Where(n => n.StatusDone == 4 && n.UIDFix == aclog.ID && (n.Type == 1 || n.Type == 3)).ToList();
            //key search
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from ex in l
                    where AssetCRM.RemoveUnicode(ex.StudentName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.StudentName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.CourseName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.FullNameCreatedByExercise).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select ex;
            }
            if (Request.QueryString["course"].ToInt(0) != 0)
            {
                ViewBag.CourseID = Request.QueryString["course"].ToInt(0);
                int cid = Request.QueryString["course"].ToInt(0);
                l = from ex in l
                    where ex.CourseID == cid
                    select ex;
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
                    l = l.OrderBy(n => n.StudentName).ToList();
                if (sort == 2)
                    l = l.OrderByDescending(n => n.StudentName).ToList();
                if (sort == 3)
                    l = l.OrderBy(n => n.CreatedDate).ToList();
                if (sort == 4)
                    l = l.OrderByDescending(n => n.CreatedDate).ToList();
                if (sort == 5)
                    l = l.OrderBy(n => n.FullNameCreatedByExercise).ToList();
                if (sort == 6)
                    l = l.OrderByDescending(n => n.FullNameCreatedByExercise).ToList();
            }
            ViewBag.Sort = sort;
            return View(l.ToPagedList(page, pageSize));
        }

        public ActionResult ExerciseDone(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //page size
            int pageSize = 20;

            //lấy bài mới 
            List<tbl_ExerciseAddonStudent> lall = new List<tbl_ExerciseAddonStudent>();
            IEnumerable<tbl_ExerciseAddonStudent> l3 = ExerciseAddonStudentTable.getbystatus(3).Where(n => (n.StatusDone == 3 || n.StatusDone == 5) && n.UIDFix == aclog.ID && (n.Type == 1 || n.Type == 3)).OrderByDescending(n => n.ModifiedDate).ToList();
            IEnumerable<tbl_ExerciseAddonStudent> l5 = ExerciseAddonStudentTable.getbystatus(5).Where(n => (n.StatusDone == 3 || n.StatusDone == 5) && n.UIDFix == aclog.ID && (n.Type == 1 || n.Type == 3)).OrderByDescending(n => n.ModifiedDate).ToList();
            lall.AddRange(l3);
            lall.AddRange(l5);
            IEnumerable<tbl_ExerciseAddonStudent> l = lall.OrderByDescending(n => n.CreatedDate).ToList();
            //key search
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from ex in l
                    where AssetCRM.RemoveUnicode(ex.StudentName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.StudentName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.CourseName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.FullNameCreatedByExercise).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select ex;
            }
            if (Request.QueryString["course"].ToInt(0) != 0)
            {
                ViewBag.CourseID = Request.QueryString["course"].ToInt(0);
                int cid = Request.QueryString["course"].ToInt(0);
                l = from ex in l
                    where ex.CourseID == cid
                    select ex;
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
                    l = l.OrderBy(n => n.StudentName).ToList();
                if (sort == 2)
                    l = l.OrderByDescending(n => n.StudentName).ToList();
                if (sort == 3)
                    l = l.OrderBy(n => n.CreatedDate).ToList();
                if (sort == 4)
                    l = l.OrderByDescending(n => n.CreatedDate).ToList();
                if (sort == 5)
                    l = l.OrderBy(n => n.FullNameCreatedByExercise).ToList();
                if (sort == 6)
                    l = l.OrderByDescending(n => n.FullNameCreatedByExercise).ToList();
                if (sort == 7)
                    l = l.OrderBy(n => n.RateExercise).ToList();
                if (sort == 8)
                    l = l.OrderByDescending(n => n.RateExercise).ToList();
            }
            ViewBag.Sort = sort;
            ViewBag.ToTal = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }
        public ActionResult ViewExerciseDone(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var l = ExerciseAddonStudentTable.getbyid(id);
            if (l == null)
            {
                SetAlert("Không tìm thấy bài", "e");
                return RedirectToAction("ExerciseList", "TeacherExercise");
            }
            var lfix = ExerciseAddonStudentFixTable.getbyexestudent(l.ID);
            ViewBag.Fix = lfix;

            var lscore = ExerciseScoreDetailTable.getbyexeid(l.ID);
            ViewBag.Score = lscore;

            var ex = ExerciseTable.getbyid(l.ExerciseID.Value);
            ViewBag.Exercise = ex;

            ViewBag.Course = CoursesTable.getbyid(l.CourseID.Value);

            return View(l);
        }
        public ActionResult ViewExerciseAudioDone(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var l = ExerciseAddonStudentTable.getbyid(id);
            if (l == null)
            {
                SetAlert("Không tìm thấy bài", "e");
                return RedirectToAction("ExerciseList", "TeacherExercise");
            }
            var ex = ExerciseTable.getbyid(l.ExerciseID.Value);
            ViewBag.Exercise = ex;
            ViewBag.ExerciseStudent = l;
            return View(l);
        }

        public ActionResult TeacherExerciseFix(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //page size
            int pageSize = 20;

            //lấy bài mới 
            List<tbl_ExerciseAddonStudent> lall = new List<tbl_ExerciseAddonStudent>();
            if (aclog.RoleID == 4)
            {
                IEnumerable<tbl_ExerciseAddonStudent> l3 = ExerciseAddonStudentTable.getbystatus(3).Where(n => (n.StatusDone == 3 || n.StatusDone == 5) && n.UIDFix == aclog.ID && (n.Type == 1 || n.Type == 3)).OrderByDescending(n => n.ModifiedDate).ToList();
                IEnumerable<tbl_ExerciseAddonStudent> l5 = ExerciseAddonStudentTable.getbystatus(5).Where(n => (n.StatusDone == 3 || n.StatusDone == 5) && n.UIDFix == aclog.ID && (n.Type == 1 || n.Type == 3)).OrderByDescending(n => n.ModifiedDate).ToList();
                lall.AddRange(l3);
                lall.AddRange(l5);
            }
            else
            {
                IEnumerable<tbl_ExerciseAddonStudent> l3 = ExerciseAddonStudentTable.getbystatus(3).Where(n => (n.StatusDone == 3 || n.StatusDone == 5) && (n.Type == 1 || n.Type == 3)).OrderByDescending(n => n.ModifiedDate).ToList();
                IEnumerable<tbl_ExerciseAddonStudent> l5 = ExerciseAddonStudentTable.getbystatus(5).Where(n => (n.StatusDone == 3 || n.StatusDone == 5) && (n.Type == 1 || n.Type == 3)).OrderByDescending(n => n.ModifiedDate).ToList();
                lall.AddRange(l3);
                lall.AddRange(l5);
            }
            IEnumerable<tbl_ExerciseAddonStudent> l = lall.OrderByDescending(n => n.CreatedDate).ToList();
            //key search
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from ex in l
                    where AssetCRM.RemoveUnicode(ex.StudentName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.StudentName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.CourseName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.FullNameCreatedByExercise).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.UIDFixName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select ex;
            }
            if (Request.QueryString["course"].ToInt(0) != 0)
            {
                ViewBag.CourseID = Request.QueryString["course"].ToInt(0);
                int cid = Request.QueryString["course"].ToInt(0);
                l = from ex in l
                    where ex.CourseID == cid
                    select ex;
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
                    l = l.OrderBy(n => n.StudentName).ToList();
                if (sort == 2)
                    l = l.OrderByDescending(n => n.StudentName).ToList();
                if (sort == 3)
                    l = l.OrderBy(n => n.CreatedDate).ToList();
                if (sort == 4)
                    l = l.OrderByDescending(n => n.CreatedDate).ToList();
                if (sort == 5)
                    l = l.OrderBy(n => n.UIDFixName).ToList();
                if (sort == 6)
                    l = l.OrderByDescending(n => n.UIDFixName).ToList();
                if (sort == 7)
                    l = l.OrderBy(n => n.RateExercise).ToList();
                if (sort == 8)
                    l = l.OrderByDescending(n => n.RateExercise).ToList();
            }
            ViewBag.Sort = sort;
            ViewBag.ToTal = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }
        /// <summary>
        /// Danh sách bài random lấy ra check
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ExerciseCheckFix(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //page size
            int pageSize = 20;
            List<tbl_ExerciseAddonStudentCheckFix> l = new List<tbl_ExerciseAddonStudentCheckFix>();
            //key search
            string search = "";
            int status = 0;

            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                search = Request.QueryString["search"].ToString().Trim();
            ViewBag.Search = search;

            if (!string.IsNullOrEmpty(Request.QueryString["status"]))
                status = Request.QueryString["status"].ToInt(0);
            ViewBag.Status = status;
            if (Request.QueryString["search"].ToInt(0) == 0)
            {
                //tạo tham số và gọi stored
                SqlParameter[] parameters =
                    {
                    new SqlParameter("@qsearch", SqlDbType.NVarChar, 2000) { Value = search },
                    new SqlParameter("@status", SqlDbType.Int) { Value = status }
                };
                var rs = SqlHelper.ExecuteDataTable("lay_bai_cham_cua_giao_vien_de_kiem_tra", parameters);
                l = (from rw in rs.AsEnumerable()
                     select new tbl_ExerciseAddonStudentCheckFix()
                     {
                         ID = Convert.ToInt32(rw["ID"]),
                         ExerciseAddonStudentID = Convert.ToInt32(rw["ExerciseAddonStudentID"]),
                         StatusAcceptReject = Convert.ToInt32(rw["StatusAcceptReject"]),
                         indexName = rw["indexName"].ToString(),
                         UIDCheckFixName = rw["UIDCheckFixName"].ToString(),
                         UIDCheckFix = Convert.ToInt32(rw["UIDCheckFix"])
                     }).OrderBy(n => n.StatusAcceptReject).ThenByDescending(n => n.CreatedDate).ToList();
            }
            else
            {
                tbl_ExerciseAddonStudentCheckFix t = ExerciseAddonStudentCheckFixTable.getbyExerciseAddonStudentID(Request.QueryString["search"].ToInt(0));
                l.Add(t);
            }
            if (aclog.RoleID == 8)
                l = l.Where(n => n.UIDCheckFix == aclog.ID).ToList();
            ViewBag.ToTal = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }

        [HttpPost]
        public ActionResult CheckFix(int id, int status)
        {
            if (Session["UID"] == null)
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 7 && aclog.RoleID != 8)// khác giáo viên bị đá
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            var ck = ExerciseAddonStudentCheckFixTable.getbyExerciseAddonStudentID(id);
            if (aclog.RoleID == 8)
            {
                if (ck.UIDCheckFix != aclog.ID)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            var l = ExerciseAddonStudentCheckFixTable.updateaccept(ck.ExerciseAddonStudentID, status, aclog.UserName);
            if (l == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            bool reject = false;
            if (status == 3)//reject
                reject = true;
            ExerciseAddonStudentTable.rejectfix(l.ExerciseAddonStudentID, reject, aclog.UserName);
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }
    }
}