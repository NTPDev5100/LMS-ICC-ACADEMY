using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppZim.Areas.Admin.Controllers
{
    public class ClassDetailController : BaseController
    {
        #region lớp chi tiết
        public ActionResult DetailClass(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            string search = "";
            if(!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = ViewBag.Search = Request.QueryString["search"].ToString();
            }
            //var m = ClassTable.getbyID(id);
            //if (m == null)
            //{
            //    SetAlert("Không tìm thấy lớp học!", "e");
            //    return RedirectToAction("ClassList", "Class");
            //}
            //// môn học của lớp đó
            //var subs = SubjectTable.getbyclassid(m.ID);
            //ViewBag.Sub = subs;
            // giáo trình của lớp
            int language = 0;
            if(!string.IsNullOrEmpty(Request.QueryString["search-language"]))
            {
                language = Request.QueryString["search-language"].ToInt(0);
            }
            ViewBag.Language = language;
            var crus = CurriculumTable.GetAll();
            crus = crus.Where(x => x.CurriculumsName.Contains(search) && (x.LanguageId == language || language == 0)).ToList();
            //crus = crus.OrderBy(x => x.IsHide).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return View(crus.ToPagedList(page, pageSize));
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddSubject(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var lop = ClassTable.getbyID(f.Get("hdfClassID").ToInt(0));
            if (lop == null)
            {
                SetAlert("Không tìm thấy lớp học!", "e");
                return RedirectToAction("ClassList", "Class");
            }
            var grade = GradeTable.getbyID(lop.GradeID.Value);
            string code = f.Get("txt-code").ToString().Replace(" ", "");
            string subject = f.Get("txt-subject").ToString();
            if (code.Length > 20)
            {
                SetAlert("Mã môn không được trên 20 ký tự", "e");
                return RedirectToAction("DetailClass", "ClassDetail", new { id = lop.ID });
            }

            //bool special = false;
            //if (f.Get("hdfspecial").ToInt(0) == 1)
            //    special = true;

            var rs = SubjectTable.insert(code, subject, lop.ID, lop.ClassName, grade.ID, grade.GradeName, aclog.UserName, false);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("DetailClass", "ClassDetail", new { id = lop.ID });
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditSubject(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int ID = f.Get("hdfID").ToInt(0);
            var mon = SubjectTable.getbyID(ID);
            if (mon == null)
            {
                SetAlert("Không tìm thấy môn học!", "e");
                return RedirectToAction("ClassList", "Class");
            }
            string code = f.Get("txt-edit-code").ToString().Replace(" ", "");
            string subject = f.Get("txt-edit-subject").ToString();
            if (code.Length > 20)
            {
                SetAlert("Mã môn không được trên 20 ký tự", "e");
                return RedirectToAction("DetailClass", "ClassDetail");
            }
            bool ishide = false;
            if (!string.IsNullOrEmpty(f.Get("chk-hide")))
                ishide = true;
            var rs = SubjectTable.update(ID, code, subject, mon.ClassID.Value, mon.ClassName, mon.GradeID.Value, mon.GradeName, aclog.UserName, ishide);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("DetailClass", "ClassDetail", new { id = mon.ClassID });
        }


        [HttpPost]
        public ActionResult AddCurriculum(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            //var lop = ClassTable.getbyID(f.Get("hdfClassID").ToInt(0));
            //if (lop == null)
            //{
            //    SetAlert("Không tìm thấy lớp học!", "e");
            //    return RedirectToAction("ClassList", "Class");
            //}

            string curri = f.Get("txt-curriculum").ToString();
            string link = f.Get("txt-link").ToString();
            //int style = f.Get("ddl-style-curriculum").ToString().ToInt(120);//số phút 1 buổi
            //int lesson = f.Get("txt-lesson").ToString().ToInt(24);// sô buổi
            int languageId = f.Get("slLanguage").ToString().ToInt();
            var language = Language.GetByID(languageId);
            if (language == null)
                language = Language.GetAll().FirstOrDefault();
            var rs = CurriculumTable.insert(0, curri, 0, 0, aclog.UserName, link, language);
            if (rs != null)
            {
                // tạo ra giáo trình chi tiết chuyển nó đến để tự cập nhật
                //for (int i = 1; i <= lesson; i++)
                //{
                //    CurriculumDetailTable.insert(rs.ID, rs.CurriculumsName, i, 0, "", aclog.UserName);
                //}
                SetAlert("Thành công", "s");
                return RedirectToAction("DetailClass", "ClassDetail");
            }
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("DetailClass", "ClassDetail");
        }

        [HttpPost]
        public ActionResult EditCurriculum(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int ID = f.Get("hdfID").ToInt(0);
            //var lop = ClassTable.getbyID(f.Get("hdfClassID").ToInt(0));
            //if (lop == null)
            //{
            //    SetAlert("Không tìm thấy lớp học!", "e");
            //    return RedirectToAction("ClassList", "Class");
            //}

            string curri = f.Get("txt-edit-curriculum").ToString();
            string link = f.Get("txt-edit-link").ToString();

            var cur = CurriculumTable.getbyID(f.Get("hdfID").ToInt(0));
            if (cur == null)
            {
                SetAlert("Không tìm thấy giáo trình!", "e");
                return RedirectToAction("DetailClass", "ClassDetail");
            }
            int languageId = f.Get("slLanguage-edit").ToString().ToInt();
            var language = Language.GetByID(languageId);
            if (language == null)
                language = Language.GetAll().FirstOrDefault();

            bool ishide = false;
            if (!string.IsNullOrEmpty(f.Get("chk-hide")))
                ishide = true;
            var rs = CurriculumTable.update(ID, 0, curri, cur.StyleLensson.Value, cur.TotalLesson.Value, ishide,
                aclog.UserName, link, language);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("DetailClass", "ClassDetail");
        }
        #endregion

        #region giáo trình detail
        public ActionResult CurriculumListDetail(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var m = CurriculumTable.getbyID(id);
            if (m == null)
            {
                SetAlert("Không tìm thấy giáo trình!", "e");
                return RedirectToAction("ClassList", "Class");
            }
            return View(m);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult UpdateCurriculumDetail(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int ID = f.Get("hdfID").ToInt(0);
            //kiểm tra xem có khóa nào dùng giáo trình này chưa, chưa thì mới cho cập nhật

            //============================================================================
            string[] ardata = f.Get("hdfData").ToString().Split(',');
            for (int i = 0; i < ardata.Length - 1; i++)
            {
                string[] ardetail = ardata[i].Split('-');
                var detail = CurriculumDetailTable.getbyID(ardetail[0].ToInt(0));// ID giáo trình chi tiết
                if (detail == null)
                    continue;
                var sub = SubjectTable.getbyID(ardetail[1].ToInt(0));// ID môn học
                if (detail == null)
                    continue;
                CurriculumDetailTable.update(detail.ID, detail.CurriculumID.Value, detail.CurriculumName, detail.LessonNumber.Value, sub.ID, sub.SubjectName, aclog.UserName);
            }
            SetAlert("Thành công", "s");
            return RedirectToAction("CurriculumListDetail", "ClassDetail", new { id = ID });
        }
        #endregion

        #region danh sách tài liệu theo giáo trình
        public ActionResult DocumentClass(int id, int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var giaotrinh = CurriculumTable.getbyID(id);
            if (giaotrinh == null)
            {
                SetAlert("Không tìm thấy giáo trình", "e");
                return RedirectToAction("ClassList", "Class");
            }
            ViewBag.ID = id;
            int pageSize = 20;
            IEnumerable<tbl_Document> l = DocumentTable.getdocbycurrid(id).Where(x => x.IsDelete == false).OrderBy(n => n.isHide).ThenByDescending(n => n.ID).ToList();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from s in l
                    where AssetCRM.RemoveUnicode(s.DocumentName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select s;
            }
            //Sắp xếp
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);
                if (sort != 0)
                {
                    if (sort == 1)
                        l = l.Where(n => n.isHide == true).ToList();
                    else if (sort == 2)
                        l = l.Where(n => n.isHide == false).ToList();
                }
            }
            ViewBag.ClassID = giaotrinh;
            ViewBag.Sort = sort;
            return View(l.ToPagedList(page, pageSize));
        }

        [HttpPost]
        public ActionResult LoadDocumentDetail(int id = 0)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false, message = "Phiên làm việc hết hạn!" }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1)
                {
                    return Json(new { rs = false, message = "Bạn không có quyền chỉnh sửa!" }, JsonRequestBehavior.AllowGet);
                }
                var d = DocumentTable.getdocid(id);
                if (d == null)
                {
                    return Json(new { rs = false, message = "Tài liệu không tồn tại!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { rs = true, l = d }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("ClassDetailController", "LoadDocumentDetail", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddCategoryDoc(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var giaotrinh = CurriculumTable.getbyID(f.Get("hdfCruID").ToInt(0));
            if (giaotrinh == null)
            {
                SetAlert("Không tìm thấy giáo trình", "e");
                return RedirectToAction("ClassList", "Class");
            }
            string catedoc = f.Get("txt-document").ToString();
            var rs = DocumentTable.insertcate(giaotrinh.ClassID.Value, giaotrinh.ID, catedoc, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("DocumentClass", "ClassDetail", new { id = giaotrinh.ID });
        }
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult UpFileDocument(FormCollection f, HttpPostedFileBase fileupload)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var giaotrinh = CurriculumTable.getbyID(f.Get("hdfCruID").ToInt(0));
            if (giaotrinh == null)
            {
                SetAlert("Không tìm thấy giáo trình", "e");
                return RedirectToAction("ClassList", "Class");
            }
            var cate = DocumentTable.getcatebyid(f.Get("ddl-cate").ToInt(0));
            if (cate == null)
            {
                cate = new tbl_DocumentCategory();
                cate.ID = 0;
            }
            int ID = f.Get("hdfDocumentID").ToInt();
            string doc = f.Get("hdfDocumentName").ToString();
            string nameDoc = f.Get("name").ToString(); //tên tài liệu

            //if (ModelState.IsValid)
            //{   //iterating through multiple file collection   
            //    foreach (HttpPostedFileBase file in fileupload)
            //    {
            //        //Checking file is available to save.  
            //        if (file != null)
            //        {
            //            string name = Path.GetFileNameWithoutExtension(file.FileName);
            //            string ext = Path.GetExtension(file.FileName);
            //            if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
            //            {
            //                SetAlert("File của bạn đã bị chặn, vui lòng không tải lên file .php, .asp, .aspx, .ps1", "e");
            //                return RedirectToAction("OrderDetail", "OrderList");
            //            }
            //            var fileName = name + " " + GetDateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss") + ext;
            //            var path = Path.Combine(Server.MapPath("~/Upload/classdocument/"), fileName);
            //            string link = "/Upload/classdocument/" + fileName;
            //            file.SaveAs(path);
            //            DocumentTable.insertdoc(cate.ID, giaotrinh.ID, file.FileName, link, 0, aclog.UserName);
            //        }
            //    }
            //}
            string filename = "";
            string link = "";
            if (fileupload != null)
            {
                string name = Path.GetFileNameWithoutExtension(fileupload.FileName);
                string ext = Path.GetExtension(fileupload.FileName);
                if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1") || ext.Contains(".exe"))
                {
                    SetAlert("File của bạn đã bị chặn, vui lòng không tải lên file .php, .asp, .aspx, .ps1", "e");
                    return RedirectToAction("OrderDetail", "OrderList");
                }
                //var fileName = name + " " + GetDateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss") + ext;
                var fileName = Guid.NewGuid() + ext;
                var path = Path.Combine(Server.MapPath("~/Upload/classdocument/"), fileName);
                link = "/Upload/classdocument/" + fileName;
                fileupload.SaveAs(path);
                filename = fileupload.FileName;
            }

            if (ID == 0)
            {
                DocumentTable.Insert(cate.ID, giaotrinh.ID, doc, link, 0, aclog.UserName, filename, nameDoc);
            }
            else
            {
                var d = DocumentTable.getdocid(ID);
                DocumentTable.Update(ID, doc, link != "" ? link : d.DocumentLink, filename != "" ? filename : d.FileName, nameDoc, aclog.UserName);
            }
            SetAlert("Thành công", "s");
            return RedirectToAction("DocumentClass", "ClassDetail", new { id = giaotrinh.ID });
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult HiddenDocument(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var doc = DocumentTable.getdocid(f.Get("hdfID").ToInt(0));
            if (doc == null)
            {
                SetAlert("Không tìm thấy giáo trình", "e");
                return RedirectToAction("ClassList", "Class");
            }
            var giaotrinh = CurriculumTable.getbyID(doc.CurriculumID.Value);
            if (giaotrinh == null)
            {
                SetAlert("Không tìm thấy giáo trình", "e");
                return RedirectToAction("ClassList", "Class");
            }
            var rs = DocumentTable.hiddendoc(doc.ID, !doc.isHide.Value, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("DocumentClass", "ClassDetail", new { id = giaotrinh.ID });
        }
        #endregion

        #region Nhóm bài tập theo giáo trình
        public ActionResult ExerciseClass(int id, int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var giaotrinh = CurriculumTable.getbyID(id);
            if (giaotrinh == null)
            {
                SetAlert("Không tìm thấy giáo trình", "e");
                return RedirectToAction("ClassList", "Class");
            }
            var lop = ClassTable.getbyID(giaotrinh.ClassID.Value);
            if (lop == null)
            {
                SetAlert("Không tìm thấy giáo trình", "e");
                return RedirectToAction("ClassList", "Class");
            }
            ViewBag.ClassID = CurriculumTable.getbyID(id);
            int pageSize = 20;
            IEnumerable<tbl_ExerciseGroup> l = ExerciseGroupTable.getbycurriculumid(id).Where(x => x.IsDelete == false).OrderByDescending(n => n.ID).ToList();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from s in l
                    where AssetCRM.RemoveUnicode(s.ExerciseGroupName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(s.TypeGroupName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select s;
            }
            //Sắp xếp
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);
                if (sort == 1)
                    l = l.OrderBy(n => n.ExerciseGroupName).ToList();
                if (sort == 2)
                    l = l.OrderByDescending(n => n.ExerciseGroupName).ToList();
                if (sort == 3)
                    l = l.OrderBy(n => n.CountExe).ToList();
                if (sort == 4)
                    l = l.OrderByDescending(n => n.CountExe).ToList();
            }
            ViewBag.CurrID = id;
            ViewBag.Sort = sort;
            return View(l.ToPagedList(page, pageSize));
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddGroupEx(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var giaotrinh = CurriculumTable.getbyID(f.Get("hdfCruID").ToInt(0));
            if (giaotrinh == null)
            {
                SetAlert("Không tìm thấy giáo trình", "e");
                return RedirectToAction("ClassList", "Class");
            }
            int type = f.Get("ddl-type").ToInt(1);
            string groupname = f.Get("txt-group").ToString();
            var rs = ExerciseGroupTable.insert(giaotrinh.ID, 0, type, groupname, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("ExerciseClass", "ClassDetail", new { id = giaotrinh.ID });
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditGroupEx(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var g = ExerciseGroupTable.getbyid(f.Get("hdfID").ToInt());
            if (g == null)
            {
                SetAlert("Không tìm thấy nhóm", "e");
                return RedirectToAction("ClassList", "Class");
            }
            var giaotrinh = CurriculumTable.getbyID(g.CurriculumID.Value);
            if (giaotrinh == null)
            {
                SetAlert("Không tìm thấy giáo trình", "e");
                return RedirectToAction("ClassList", "Class");
            }

            string groupname = f.Get("txt-edit-group").ToString();
            var rs = ExerciseGroupTable.update(g.ID, groupname, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("ExerciseClass", "ClassDetail", new { id = giaotrinh.ID });
        }
        #endregion


        #region Bài tập group
        public ActionResult ExerciseClassGroup(int id, int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            //nhóm bài tập
            ViewBag.Group = ExerciseGroupTable.getbyid(id);
            ViewBag.GiaoTrinh = CurriculumTable.getbyID(ExerciseGroupTable.getbyid(id).CurriculumID.Value);
            //page size
            int pageSize = 20;

            //lấy bài mới 
            //List<tbl_ExerciseAddonStudent> l = new List<tbl_ExerciseAddonStudent>();
            IEnumerable<tbl_Exercise> l = ExerciseTable.getbygroupid(id).OrderBy(n => n.ID).ToList();
            //key search
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from ex in l
                    where AssetCRM.RemoveUnicode(AssetCRM.RemoveHTMLTags(ex.ExerciseTitle)).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
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
                if (sort != 0)
                {
                    if (sort == 1)
                        l = l.OrderBy(n => n.ExerciseTitle).ToList();
                    if (sort == 2)
                        l = l.OrderByDescending(n => n.ExerciseTitle).ToList();
                    if (sort == 3)
                        l = l.Where(n => n.isHide == false).ToList();
                    if (sort == 4)
                        l = l.Where(n => n.isHide == true).ToList();
                }
            }
            ViewBag.Sort = sort;
            return View(l.ToPagedList(page, pageSize));
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AddExercise(FormCollection f, HttpPostedFileBase fileupload)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var nhombt = ExerciseGroupTable.getbyid(f.Get("hdfgID").ToInt(0));
            if (nhombt == null)
            {
                SetAlert("Không tìm thấy nhóm bài tập", "e");
                return RedirectToAction("ClassList", "Class");
            }
            string ques = f.Get("hdfQuestion").ToString();
            string DescribeAnswer = "";
            if (nhombt.TypeGroup == 2)//trắc nghiệm mới có phần giải thích đáp án
            {
                DescribeAnswer = f.Get("txt-DescribeAnswer").ToString();
            }

            string filename = "";
            string link = "";
            if (fileupload != null)
            {
                string name = Path.GetFileNameWithoutExtension(fileupload.FileName);
                string ext = Path.GetExtension(fileupload.FileName);
                if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1") || ext.Contains(".exe"))
                {
                    SetAlert("File của bạn đã bị chặn, vui lòng không tải lên file .php, .asp, .aspx, .ps1", "e");
                    return RedirectToAction("OrderDetail", "OrderList");
                }
                //var fileName = name + " " + GetDateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss") + ext;
                var fileName = Guid.NewGuid() + ext;
                var path = Path.Combine(Server.MapPath("~/Upload/exercise/"), fileName);
                link = "/Upload/exercise/" + fileName;
                fileupload.SaveAs(path);
                filename = fileupload.FileName;
            }
            var q = ExerciseTable.insert(nhombt.ID, ques, filename, link, nhombt.TypeGroup.Value, 0, aclog.UserName, 0, DescribeAnswer);
            if (nhombt.TypeGroup == 2)
            {
                var kq = f.Get("hdfAnswerTrue").ToString();

                string a = f.Get("txt-a").ToString();
                if (kq == "A")
                    ExerciceMultiChoiseTable.insert(q.ID, a, true, aclog.UserName);
                else
                    ExerciceMultiChoiseTable.insert(q.ID, a, false, aclog.UserName);

                string b = f.Get("txt-b").ToString();
                if (kq == "B")
                    ExerciceMultiChoiseTable.insert(q.ID, b, true, aclog.UserName);
                else
                    ExerciceMultiChoiseTable.insert(q.ID, b, false, aclog.UserName);

                string c = f.Get("txt-c").ToString();
                if (kq == "C")
                    ExerciceMultiChoiseTable.insert(q.ID, c, true, aclog.UserName);
                else
                    ExerciceMultiChoiseTable.insert(q.ID, c, false, aclog.UserName);

                string d = f.Get("txt-d").ToString();
                if (kq == "D")
                    ExerciceMultiChoiseTable.insert(q.ID, d, true, aclog.UserName);
                else
                    ExerciceMultiChoiseTable.insert(q.ID, d, false, aclog.UserName);
            }
            SetAlert("Thành công", "s");
            return RedirectToAction("ExerciseClassGroup", "ClassDetail", new { id = nhombt.ID });
        }

        #region upload file imgage notification
        [HttpPost]
        public ActionResult UploadFileExercise()
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
                        string ext = Path.GetExtension(file.FileName);
                        if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                        {
                            return Json(new { rs = false, l = "" }, JsonRequestBehavior.AllowGet);
                        }
                        //var fileName = Guid.NewGuid() + ext;
                        //var path = Path.Combine(Server.MapPath("~/Upload/exercise/"), fileName);

                        //string strPathAndQuery = Request.Url.PathAndQuery;
                        //string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");

                        //link = "" + strUrl + "Upload/exercise/" + fileName;
                        //file.SaveAs(path);

                        string fileContentType = file.ContentType; // getting ContentType
                        byte[] tempFileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(tempFileBytes, 0, Convert.ToInt32(file.ContentLength));
                        string fileName = Guid.NewGuid() + ext; // getting File Name
                        string fileExtension = Path.GetExtension(fileName).ToLower();
                        var result = AssetCRM.isValidFile(tempFileBytes, fileExtension, fileContentType); // Validate Header
                        if (result)
                        {
                            fileName = Guid.NewGuid() + ext;
                            var path = Path.Combine(Server.MapPath("~/Upload/exercise/"), fileName);
                            link = "/Upload/exercise/" + fileName;
                            file.SaveAs(path);
                        }
                    }
                }
            }
            return Json(new { rs = true, l = link }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        [HttpPost]
        public ActionResult QuestionOff(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int t = f.Get("hdfType").ToInt(0);
            var q = ExerciseTable.getbyid(f.Get("hdfID").ToInt(0));
            if (t == 1)
                ExerciseTable.update(q.ID, q.ExerciseGroupID.Value, q.ExerciseTitle, true, aclog.UserName, q.ParentQuestionID.Value,q.DescribeAnswer);
            else
                ExerciseTable.update(q.ID, q.ExerciseGroupID.Value, q.ExerciseTitle, false, aclog.UserName, q.ParentQuestionID.Value,q.DescribeAnswer);
            SetAlert("Thành công", "s");
            return RedirectToAction("ExerciseClassGroup", "ClassDetail", new { id = q.ExerciseGroupID });
        }
        #endregion

        [HttpPost]
        public ActionResult DeleteDocument(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)
            {
                DocumentTable.Delete(id, true, aclog.UserName);
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteExercise(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)
            {
                ExerciseTable.Delete(id, true, aclog.UserName);
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Xóa nhóm câu hỏi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteExerciseGroup(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)
            {
                ExerciseGroupTable.Delete(id, true, aclog.UserName);
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Xóa giáo trình
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteCurriculums(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)
            {
                var check = CoursesTable.GetByCurriculumsID(id);
                if (check.Count() > 0)
                {
                    return Json(new { rs = false, message = "Không thể xóa giáo trình có liên kết với khóa học" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    CurriculumTable.Delete(id, true, aclog.UserName);
                    return Json(new { rs = true, message = "Thành công" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        }
    }
}