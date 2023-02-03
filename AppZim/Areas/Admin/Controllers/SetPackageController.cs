using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppZim.Models;
using PagedList;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using AppZim.Areas.Admin.Controllers;
using System.Media;
using WebUI.Business;
using System.Net.Http;
using RestSharp;
using Newtonsoft.Json;

namespace AppZim.Areas.Admin.Controllers
{
    public class SetPackageController : BaseController
    {
        // GET: Admin/SetPackage
        [HttpGet]
        public ActionResult ListSetPackage(int page = 1)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });
                IEnumerable<tbl_SetPackage> l = SetPackageTable.getall().OrderByDescending(n => n.ID);
                int pageSize = 10;

                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                {
                    ViewBag.Search = Request.QueryString["search"];
                    string search = Request.QueryString["search"].ToString();
                    l = from ex in l
                        where AssetCRM.RemoveUnicode(ex.PackageName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        select ex;
                }
                int setType = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["setType"]))
                {
                    setType = Request.QueryString["setType"].ToInt(0);
                    if (setType == 1)
                        l = l.Where(n => n.Price == 0).ToList();
                    if (setType == 2)
                        l = l.Where(n => n.Price > 0).ToList();
                }
                ViewBag.SetType = setType;
                int sort = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
                {
                    sort = Request.QueryString["sort"].ToInt(0);
                    if (sort == 1)
                        l = l.OrderBy(n => n.PackageName).ToList();
                    if (sort == 2)
                        l = l.OrderByDescending(n => n.PackageName).ToList();
                    if (sort == 3)
                        l = l.OrderBy(n => n.CreatedDate).ToList();
                    if (sort == 4)
                        l = l.OrderByDescending(n => n.CreatedDate).ToList();
                }
                ViewBag.Sort = sort;
                ViewBag.TotalCount = l.Count();
                return View(l.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "ListSetPackage", uid, ex.Message);
                SetAlert("Fail", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }


        }

        [HttpGet]
        public ActionResult CreateSet()
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });
                return View();
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "CreateSet", uid, ex.Message);
                SetAlert("Fail", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

      

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult AddSet(FormCollection f, HttpPostedFileBase[] setThumbnail)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });
                string packageName = "";
                if (string.IsNullOrEmpty(f.Get("txt-package-name")))
                    SetAlert("Enter Set's name", "e");
                else
                    packageName = f.Get("txt-package-name").ToString();
                int packageType = f.Get("setType").ToInt(1);
                int price = Convert.ToInt32(f.Get("txt-price").RemoveNonNumeric());
                if (packageType == 1)
                    price = 0;
                string linkIMG = "";
                string linkIMGThumbnails = "";

                string packageDescription = f.Get("hdfcontent").ToString();
                bool Listen = f["rd-lis"] != null ? true : false;
                bool Reading = f["rd-read"] != null ? true : false;
                bool Writing = f["rd-write"] != null ? true : false;
                bool Speaking = f["rd-speak"] != null ? true : false;

                //if (f.Get("hdf-type").ToInt(0) == 1)
                //    Listen = true;
                //if (f.Get("hdf-type").ToInt(0) == 2)
                //    Reading = true;

                if (setThumbnail != null)
                {
                    if (setThumbnail.Count() > 0)
                    {
                        for (int i = 0; i < setThumbnail.Count(); i++)
                        {
                            if (setThumbnail[i] != null)
                            {
                                string name = Path.GetFileNameWithoutExtension(setThumbnail[i].FileName);
                                string ext = Path.GetExtension(setThumbnail[i].FileName);
                                var fileName = name + GetDateTime.Now.ToString("ddmmyy_hhmmss") + ext;
                                var path = Path.Combine(Server.MapPath("~/Upload/SetIMG/"), fileName);
                                linkIMG = "/Upload/SetIMG/" + fileName;
                                setThumbnail[i].SaveAs(path);
                                AssetCRM.GenerateThumbNail("~" + linkIMG, "~/Upload/thumbnails/SetIMG/" + fileName, 150, false);
                                linkIMGThumbnails = "/Upload/thumbnails/SetIMG/" + fileName;
                                ModelState.Clear();
                            }
                            else
                            {
                                SetAlert("Vui lòng chọn hình", "e");
                                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                            }
                        }
                    }
                    else
                    {
                        SetAlert("Vui lòng chọn hình", "e");
                        return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                    }
                }
                else
                {
                    SetAlert("Vui lòng chọn hình", "e");
                    return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                }
                var set = SetPackageTable.insert(packageName, price, linkIMG, linkIMGThumbnails, packageDescription, Listen, Speaking, Reading, Writing, aclog.UserName);
                if (set == null)
                    SetAlert("Có lỗi đã xảy ra", "e");
                else
                    SetAlert("Thành công", "s");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "AddSet", uid, ex.Message);
                SetAlert("Fail", "w");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }

      
        public ActionResult SetDetail(int id)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var pk = SetPackageTable.getbyid(id);
                if (pk == null)
                {
                    SetAlert("Set's not found", "e");
                    return RedirectToAction("ListSetPackage", "SetPackage");
                }
                return View(pk);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "SetDetail", uid, ex.Message);
                SetAlert("Fail", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        
        [HttpPost]
        public ActionResult UploadFilePost()
        {
            try
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
                                string strPathAndQuery = Request.Url.PathAndQuery;
                                string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                                link = strUrl + "Upload/exercise/" + fileName;
                                file.SaveAs(path);
                            }
                        }
                    }
                }
                return Json(new { rs = true, l = link }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "SetDetail", uid, ex.Message);
                SetAlert("Fail", "e");
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// Lấy thông tin của gói
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult LoadInfoSet(int id)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                //if (aclog.RoleID == 5)
                //    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

                var rs = SetPackageTable.getbyid(id);
                if (rs != null)
                {
                    var lc = SetPackageStudentTable.getbysetid(id).Count;
                    return Json(new { rs = true, d = rs, p = string.Format("{0:N0}", rs.Price), count = lc + " phút" }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "LoadInfoSet", uid, ex.Message);
                SetAlert("Fail", "w");
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Cập nhật trạng thái show của gói cho học viên
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult UpdateStatus(int id)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

                var rs = SetPackageTable.UpdateShow(id, aclog.UserName);
                if (rs != null)
                    return Json(new { rs = true, d = rs }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "UpdateStatus", uid, ex.Message);
                SetAlert("Fail", "w");
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateSet(FormCollection f, HttpPostedFileBase[] setThumbnail)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });
                string packageName = "";
                int ID = f.Get("hdfSetID").ToInt(0);

                if (string.IsNullOrEmpty(f.Get("txt-package-name")))
                    SetAlert("Enter Set's name", "e");
                else
                    packageName = f.Get("txt-package-name").ToString();
                int packageType = f.Get("setType").ToInt(1);
                int price = Convert.ToInt32(f.Get("txt-price").RemoveNonNumeric());
                if (packageType == 1)
                    price = 0;
                if (packageType == 1)
                    price = 0;
                string linkIMG = "";
                string linkIMGThumbnails = "";
                //int status = f["setStatus"] != null ? 1 : 2;
                string packageDescription = f.Get("hdfDescription").ToString();
                if (setThumbnail != null)
                {
                    if (setThumbnail.Count() > 0)
                    {
                        for (int i = 0; i < setThumbnail.Count(); i++)
                        {
                            if (setThumbnail[i] != null)
                            {
                                string name = Path.GetFileNameWithoutExtension(setThumbnail[i].FileName);
                                string ext = Path.GetExtension(setThumbnail[i].FileName);
                                var fileName = name + GetDateTime.Now.ToString("ddmmyy_hhmmss") + ext;
                                var path = Path.Combine(Server.MapPath("~/Upload/SetIMG/"), fileName);
                                linkIMG = "/Upload/SetIMG/" + fileName;
                                setThumbnail[i].SaveAs(path);
                                AssetCRM.GenerateThumbNail("~" + linkIMG, "~/Upload/thumbnails/SetIMG/" + fileName, 150, false);
                                linkIMGThumbnails = "/Upload/thumbnails/SetIMG/" + fileName;
                                ModelState.Clear();
                            }
                        }
                    }
                }
                var pk = SetPackageTable.Update(ID, packageName, price, linkIMG, linkIMGThumbnails, packageDescription, aclog.UserName);
                if (pk == null)
                    SetAlert("Có lỗi đã xảy ra", "e");
                else
                    SetAlert("Thành công", "s");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "UpdateSet", uid, ex.Message);
                SetAlert("Fail", "w");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }

        }
        public ActionResult Question(int id = 0, int type = 0)
        {
            try
            {
                if (type == 1)
                {
                    ViewBag.QuestionTitle = "Listening";
                    ViewBag.QuestionType = 1;
                }
                else if (type == 2)
                {
                    ViewBag.QuestionTitle = "Reading";
                    ViewBag.QuestionType = 2;
                }
                else if (type == 3)
                {
                    ViewBag.QuestionTitle = "Writing";
                    ViewBag.QuestionType = 3;
                }
                else if (type == 4)
                {
                    ViewBag.QuestionTitle = "Speaking";
                    ViewBag.QuestionType = 4;
                }
                else
                {
                    SetAlert("Page not found!", "e");
                    return RedirectToAction("ListSetPackage", "SetPackage");
                }
                var set = SetPackageTable.getbyid(id);
                if (set == null)
                {
                    SetAlert("Set not found!", "e");
                    return RedirectToAction("ListSetPackage", "SetPackage");
                }
                ViewBag.SetID = id;
                ViewBag.TypeSkill = type;
                var dt = SetPackageDetailTable.getbysetid(id).Where(n => n.TypeDetail == type).FirstOrDefault();
                var groups = ExerciseGroupTable.getbysetid(id).Where(n => n.SetPackageDetailID == dt.ID).ToList();
                return View(groups);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "Question", uid, ex.Message);
                SetAlert("Fail", "w");
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult AddGroupSet(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                int id = f.Get("hdf-group-id").ToInt(0);
                int typeskill = f.Get("hdf-group-type").ToInt(0);

                if (id == 0)//tạo mới
                {
                    tbl_SetPackageDetail detail = SetPackageDetailTable.getbyid(f.Get("hdf-set-detail-id").ToInt(0));
                    if (detail == null)
                    {
                        SetAlert("Not found", "e");
                        return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                    }
                    string group = f.Get("txt-group-name").ToString();
                    if (string.IsNullOrEmpty(group))
                    {
                        SetAlert("Vui lòng nhập nhóm câu hỏi", "e");
                        return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                    }
                    int typemultichoice = f.Get("ddl-type").ToInt(0);
                    if (typemultichoice == 0)
                    {
                        SetAlert("Vui lòng chọn dạng bài làm", "e");
                        return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                    }
                    string groupquestion = f.Get("hdf-content").ToString();
                    string introduction = f.Get("hdf-introduction").ToString();
                    int egid = f.Get("ddl-group-id").ToInt(0);
                    var g = ExerciseGroupTable.insertsetpackage(detail, 2, group, typemultichoice, groupquestion, introduction, egid, aclog.UserName);
                    if (g == null)
                        SetAlert("Có lỗi đã xảy ra", "e");
                    else
                    {
                        //lư lại thứ tự câu hỏi với dạng drag hoặc write
                        if (typemultichoice == 2 || typemultichoice == 3)
                        {
                            tbl_Exercise ex = new tbl_Exercise();
                            string[] question = f.Get("hdf-group-question").ToString().Split('⅃');
                            string[] anwer = f.Get("hdf-anwer").ToString().Split('⅃');
                            for (int i = 0; i < question.Length - 1; i++)
                            {
                                if (ex == null)
                                    ex = new tbl_Exercise();
                                ex = ExerciseTable.insert(g.ID, question[i].Trim().ToInt().ToString(), "", "", 2, 0, aclog.UserName, 0, "");
                                for (int j = 0; j < anwer.Length - 1; j++)
                                {
                                    string[] anwardata = anwer[j].Split('⅂');
                                    if (anwardata[0].Trim().Replace("quest-id-", "").ToInt(0) == question[i].ToInt(0))
                                    {
                                        bool anwertrue = anwardata[1].ToBool(false);
                                        if (typemultichoice == 3)
                                        {
                                            anwertrue = true;//luôn đúng khi dạng bài là viết vào ô trống :D
                                        }
                                        string anwerquestion = anwardata[2];
                                        ExerciceMultiChoiseTable.insert(ex.ID, anwerquestion, anwertrue, aclog.UserName);
                                    }
                                }
                            }
                        }
                        SetAlert("Thành công", "s");
                    }
                }
                else//cập nhật
                {
                    var g = ExerciseGroupTable.getbyid(id);
                    if (g == null)
                    {
                        SetAlert("Không tìm thấy nhóm câu hỏi", "e");
                        return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                    }
                    string group = f.Get("txt-group-name").ToString();
                    string groupquestion = f.Get("hdf-content").ToString();
                    string introduction = f.Get("hdf-introduction").ToString();
                    int egid = f.Get("ddl-group-id").ToInt(0);
                    g = ExerciseGroupTable.updatesetpackage(g.ID, group, groupquestion, introduction, egid, aclog.UserName);
                    if (g == null)
                        SetAlert("Có lỗi đã xảy ra", "e");
                    else
                    {
                        //lưu lại thứ tự câu hỏi 
                        //lư lại thứ tự câu hỏi với dạng drag và dạng write
                        if (g.TypeMultiChoice == 2 || g.TypeMultiChoice == 3)
                        {
                            tbl_Exercise ex = new tbl_Exercise();
                            string[] question = f.Get("hdf-group-question").ToString().Split('⅃');
                            string[] anwer = f.Get("hdf-anwer").ToString().Split('⅃');
                            for (int i = 0; i < question.Length - 1; i++)
                            {
                                //int answerid = anwer
                                ex = ExerciseTable.getbytitleidvsgroupid(g.ID, question[i].Trim());
                                if (ex == null)
                                {
                                    ex = new tbl_Exercise();
                                    ex = ExerciseTable.insert(g.ID, question[i].Trim().ToInt().ToString(), "", "", 2, 0, aclog.UserName, 0, "");
                                }
                                for (int j = 0; j < anwer.Length - 1; j++)
                                {
                                    string[] anwardata = anwer[j].Split('⅂');

                                    bool anwertrue = anwardata[1].ToBool(false);
                                    if (g.TypeMultiChoice == 3)
                                    {
                                        anwertrue = true;//Luôn đúng khi là dạng điền vào ô trống
                                    }
                                    string anwerquestion = anwardata[2];

                                    if (anwardata[3].ToInt(0) == 0)
                                    {
                                        if (anwardata[0].Trim().Replace("quest-id-", "").ToInt(0) == question[i].ToInt(0))
                                        {
                                            ExerciceMultiChoiseTable.insert(ex.ID, anwerquestion, anwertrue, aclog.UserName);
                                        }
                                    }
                                    else
                                    {
                                        ExerciceMultiChoiseTable.update(anwardata[3].ToInt(0), anwerquestion, anwertrue, aclog.UserName);
                                    }
                                }
                            }
                            //xóa những câu hỏi đã bị xóa ra khỏi group
                            var gex = ExerciseTable.getbygroupid(g.ID);
                            foreach (var item in gex)
                            {
                                bool delete = true;
                                for (int i = 0; i < question.Length - 1; i++)
                                {
                                    if (item.ExerciseTitle.ToInt(0) == question[i].ToInt(-1))
                                    {
                                        delete = false;
                                        break;
                                    }
                                }
                                if (delete)//xóa KHOA OI DAY NE
                                {
                                    ExerciseTable.update(item.ID, item.ExerciseGroupID.Value, item.ExerciseTitle, delete, aclog.UserName, item.ParentQuestionID.Value,item.DescribeAnswer);
                                }
                            }
                        }
                        SetAlert("Thành công", "s");
                    }
                }
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "AddSet", uid, ex.Message);
                SetAlert("Fail", "w");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }
        /// <summary>
        /// Lấy thông tin group bài để sửa
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetInfoGroup(int id)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var rs = ExerciseGroupTable.getbyid(id);
                if (rs == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                if (rs.ExerciseGroupID == null)
                {
                    rs.ExerciseGroupID = 0;
                }
                //load câu trả lời đúng
                int index = 0;
                List<tbl_ExerciseGroup> lexg = ExerciseGroupTable.getbysetdetail(rs.SetPackageDetailID.Value);
                foreach (var item in lexg)
                {
                    if (item.ID == rs.ID)
                    {
                        index = lexg.IndexOf(item) + 1;
                        break;
                    }
                }
                StringBuilder trueanswer = new StringBuilder();
                StringBuilder noise = new StringBuilder();
                if (rs.TypeMultiChoice == 2)
                {
                    List<tbl_Exercise> lex = ExerciseTable.getbygroupid(rs.ID).Where(n => n.isHide != true).ToList();
                    foreach (var item in lex)
                    {
                        var answer = ExerciceMultiChoiseTable.getbyanwertrue(item.ID);
                        if (answer != null)
                        {
                            trueanswer.Append("<div class=\"col-12 col-md-6 form-group correct-answer\">");
                            trueanswer.Append(" <input class=\"form-control class-answer\" data-answer-id=\"" + answer.ID + "\" data-question=\"quest-id-" + item.ExerciseTitle.Trim().ToInt(0) + "\" data-id=\"answer-quest-id-" + item.ExerciseTitle.Trim().ToInt(0) + "\" quest-group-id=\"quest-group-" + item.ExerciseTitle.Trim().ToInt(0) + "\" autocomplete=\"off\" placeholder=\"Answer\" value=\"" + answer.AnswerTitle + "\" answer-true=\"true\">");
                            trueanswer.Append(" <a href = \"javascript:;\" class=\"add-noise btn btn-warning action-icon add-noise-update\" data-id=\"" + item.ExerciseTitle.Trim().ToInt(0) + "\"><i data-feather=\"plus\"></i> Noise</a>");
                            trueanswer.Append("</div>");
                        }
                    }
                    //load câu trả lời sai
                    foreach (var item in lex)
                    {
                        var answer = ExerciceMultiChoiseTable.getbyexeid(item.ID).Where(n => n.isTrue != true).ToList();
                        foreach (var an in answer)
                        {
                            noise.Append("<div class=\"col-12 col-md-6 form-group correct-answer noise-word\">");
                            noise.Append("  <input class=\"form-control class-answer\" data-answer-id=\"" + an.ID + "\" data-question=\"quest-id-" + item.ExerciseTitle.Trim().ToInt(0) + "\" data-id=\"answer-quest-id-" + item.ExerciseTitle.Trim().ToInt(0) + "\" quest-group-id=\"1\" autocomplete=\"off\" placeholder=\"Answer\" value=\"" + an.AnswerTitle + "\" answer-true=\"false\">");
                            noise.Append("  <a href=\"javascript:;\" class=\"delete-noise btn btn-transparent tx-danger action-icon\" data-id=\"" + an.ID + "\">");
                            noise.Append("      <i data-feather=\"minus-circle\"></i>");
                            noise.Append("  </a>");
                            noise.Append("</div>");
                        }
                    }
                }
                if (rs.TypeMultiChoice == 3)
                {
                    List<tbl_Exercise> lex = ExerciseTable.getbygroupid(rs.ID).Where(n => n.isHide != true).ToList();
                    foreach (var item in lex)
                    {
                        //Load câu trả lời đúng chơi chơi (câu hiển thị ở phía trên có thêm nút add)
                        var answer = ExerciceMultiChoiseTable.getbyexeid(item.ID).Where(n => n.isTrue == true).ToList();
                        if (answer.Count > 0)
                        {
                            for (int i = 0; i < answer.Count; i++)
                            {
                                if (answer[i] != null)
                                {
                                    if (i == 0)//lấy thằng đầu tiên để lên => mấy thằng còn lại ở dưới - ứng với mỗi câu hỏi
                                    {
                                        trueanswer.Append("<div class=\"col-12 col-md-6 form-group correct-answer\">");
                                        trueanswer.Append(" <input class=\"form-control class-answer\" data-answer-id=\"" + answer[i].ID + "\" data-question=\"quest-id-" + item.ExerciseTitle.Trim().ToInt(0) + "\" data-id=\"answer-quest-id-" + item.ExerciseTitle.Trim().ToInt(0) + "\" quest-group-id=\"quest-group-" + item.ExerciseTitle.Trim().ToInt(0) + "\" autocomplete=\"off\" placeholder=\"Answer\" value=\"" + answer[i].AnswerTitle + "\" answer-true=\"true\">");
                                        trueanswer.Append(" <a href = \"javascript:;\" class=\"add-noise btn btn-warning action-icon add-noise-update\" data-id=\"" + item.ExerciseTitle.Trim().ToInt(0) + "\"><i data-feather=\"plus\"></i> Thêm</a>");
                                        trueanswer.Append("</div>");
                                    }
                                    else
                                    {
                                        noise.Append("<div class=\"col-12 col-md-6 form-group correct-answer noise-word\">");
                                        noise.Append("  <input class=\"form-control class-answer\" data-answer-id=\"" + answer[i].ID + "\" data-question=\"quest-id-" + item.ExerciseTitle.Trim().ToInt(0) + "\" data-id=\"answer-quest-id-" + item.ExerciseTitle.Trim().ToInt(0) + "\" quest-group-id=\"1\" autocomplete=\"off\" placeholder=\"Nhập đáp án...\" value=\"" + answer[i].AnswerTitle + "\" answer-true=\"false\">");
                                        noise.Append("  <a href=\"javascript:;\" class=\"delete-noise btn btn-transparent tx-danger action-icon\" data-id=\"" + answer[i].ID + "\">");
                                        noise.Append("      <i data-feather=\"minus-circle\"></i>");
                                        noise.Append("  </a>");
                                        noise.Append("</div>");
                                    }
                                }
                            }
                        }
                    }

                }
                return Json(new { rs = true, d = rs, answer = trueanswer.ToString(), noise = noise.ToString(), index = index }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "GetInfoGroup", uid, ex.Message);
                SetAlert("Fail", "w");
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult GetInfoExerciseChoice(int id, int multi)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var rs = ExerciseTable.getbyid(id);
                if (rs == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var answer = ExerciceMultiChoiseTable.getbyexeid(rs.ID);
                if (multi == 0)
                {
                    if (answer.Count == 4)
                    {
                        return Json(new { rs = true, d = rs, A = answer[0], B = answer[1], C = answer[2], D = answer[3] }, JsonRequestBehavior.AllowGet);
                    }
                    else if (answer.Count == 3)
                    {
                        return Json(new { rs = true, d = rs, A = answer[0], B = answer[1], C = answer[2] }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { rs = true, d = rs, A = answer[0], B = answer[1] }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (multi == 1)//multi choice
                {
                    StringBuilder sbb = new StringBuilder();
                    StringBuilder html = new StringBuilder();
                    var ExerciseTitle = "";
                    var l = new tbl_Exercise();
                    if (rs.ParentQuestionID != null && rs.ParentQuestionID != 0)
                    {

                        l = ExerciseTable.getbyid(rs.ParentQuestionID.Value);

                        if (l != null)
                        {
                            if (!string.IsNullOrEmpty(l.ExerciseTitle) && l.ExerciseTitle.Length > 50)
                            {
                                ExerciseTitle = l.ExerciseTitle.Substring(0, 50) + "...";
                            }
                            else
                            {
                                ExerciseTitle = l.ExerciseTitle;
                            }

                            sbb.Append("<option value=\"" + l.ID + "\">" + ExerciseTitle + "</option>");
                        }

                    }
                    else
                    {
                        l.ID = 0;
                    }
                    //
                    var childquestions = ExerciseTable.getbygroupid(rs.ExerciseGroupID.Value).ToList();
                    sbb.Append("<option value=\"0\">None</option>");
                    foreach (var item in childquestions)
                    {
                        if (!string.IsNullOrEmpty(item.ExerciseTitle) && item.ExerciseTitle.Length > 50)
                        {
                            ExerciseTitle = item.ExerciseTitle.Substring(0, 50) + "...";
                        }
                        else
                        {
                            ExerciseTitle = item.ExerciseTitle;
                        }
                        if (item.ID != l.ID && rs.ID != item.ParentQuestionID && item.ID != rs.ID)
                        {
                            sbb.Append("<option value=\"" + item.ID + "\">" + ExerciseTitle + "</option>");
                        }
                    }

                    //
                    foreach (var item in answer)
                    {
                        string ck = "";
                        if (item.isTrue.Value)
                            ck = "checked";

                        html.Append("<div class=\"form-group col-md-6 d-flex align-items-center answer-multi-choice\" data-id=\"" + item.ID + "\">");
                        html.Append("  <div class=\"check-flag\">");
                        html.Append("      <label class=\"custom-checkbox\">");
                        html.Append("          <input class=\"check-input answer-multi-choice-checkbox\" " + ck + " type=\"checkbox\">");
                        html.Append("          <span class=\"ip-avata\"></span>");
                        html.Append("      </label>");
                        html.Append("  </div>");
                        html.Append("  <input type=\"text\" placeholder=\"Answer...\" value=\"" + item.AnswerTitle + "\" class=\"form-control answer-multi-choice-input\">");
                        html.Append("  <a href=\"javascript:;\" class=\"delete-answer-multi-choice pd-l-5\" data-id=\"" + item.ID + "\"><i data-feather=\"x-circle\"></i></a>");
                        html.Append("</div>");
                    }
                    return Json(new { rs = true, d = rs, sbb = sbb.ToString(), answer = html.ToString() }, JsonRequestBehavior.AllowGet);
                }
                else //map
                {
                    string ans = "";
                    var anone = ExerciceMultiChoiseTable.getbyexeid(rs.ID);
                    if (anone.Count > 0)
                        ans = anone.FirstOrDefault().AnswerTitle;
                    return Json(new { rs = true, d = rs, answer = ans }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "GetInfoExerciseChoice", uid, ex.Message);
                SetAlert("Fail", "w");
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// lấy câu trả lời của câu hỏi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LoadAnswerDrag(int id)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

                tbl_Exercise ex = ExerciseTable.getbyid(id);
                if (ex == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                tbl_ExerciseGroup g = ExerciseGroupTable.getbyid(ex.ExerciseGroupID.Value);

                List<tbl_ExerciceMultiChoise> law = ExerciceMultiChoiseTable.getbyexeid(ex.ID);
                StringBuilder drag = new StringBuilder();
                StringBuilder write = new StringBuilder();
                string a = "";
                string b = "";
                string c = "";
                string d = "";
                string AnswerTrue = "";

                if (g.TypeMultiChoice == 1)
                {
                    a = law[0].AnswerTitle;
                    b = law[1].AnswerTitle;
                    c = law[2].AnswerTitle;
                    d = law[3].AnswerTitle;
                }
                if (g.TypeMultiChoice == 2)
                {
                    foreach (var item in law)
                    {
                        if (item.isTrue.Value)
                            AnswerTrue = item.AnswerTitle;
                        else
                        {
                            drag.Append("<div class=\"row pd-b-5\">");
                            drag.Append("   <div class=\"col-md-10\"><input type=\"text\" class=\"form-control edit-answer-drag-noise\" data-id=\"" + item.ID + "\" value=\"" + item.AnswerTitle + "\"></div>");
                            drag.Append("   <div class=\"col-md-2 pd-5\">");
                            drag.Append("       <a href=\"javascript:;\" class=\"delete-noice\" data-id=\"" + item.ID + "\"><i data-feather=\"x-circle\"></i></a>");
                            drag.Append("   </div>");
                            drag.Append("</div>");
                        }
                    }
                }
                if (g.TypeMultiChoice == 3)
                {
                    foreach (var item in law)
                    {
                        write.Append("<div class=\"row pd-b-5\">");
                        write.Append("   <div class=\"col-md-10\"><input type=\"text\" class=\"form-control edit-answer-write\" data-id=\"" + item.ID + "\" value=\"" + item.AnswerTitle + "\"></div>");
                        write.Append("   <div class=\"col-md-2 pd-5\">");
                        write.Append("       <a href=\"javascript:;\" class=\"delete-noice\" data-id=\"" + item.ID + "\"><i data-feather=\"x-circle\"></i></a>");
                        write.Append("   </div>");
                        write.Append("</div>");
                    }
                }
                return Json(new
                {
                    rs = true,
                    A = a,
                    B = b,
                    C = c,
                    D = d,
                    Drag = drag.ToString(),
                    AnswerTrue = AnswerTrue,
                    Write = write.ToString()
                },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "LoadAnswerDrag", uid, ex.Message);
                SetAlert("Fail", "w");
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// Xóa nhóm câu hỏi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteGroup(int id)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var rs = ExerciseGroupTable.deletegroup(id, aclog.UserName);
                if (rs != null)
                    return Json(new { rs = true, }, JsonRequestBehavior.AllowGet);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "DeleteAnswerDrag", uid, ex.Message);
                SetAlert("Fail", "w");
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// Xóa câu trả lời
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteAnswerDrag(int id)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var rs = ExerciceMultiChoiseTable.deleteanswerdrag(id, aclog.UserName);
                if (rs != null)
                    return Json(new { rs = true, }, JsonRequestBehavior.AllowGet);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "DeleteAnswerDrag", uid, ex.Message);
                SetAlert("Fail", "w");
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// Xóa câu hỏi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteExerciseChoice(int id)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var rs = ExerciseTable.getbyid(id);
                if (rs == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                ExerciseTable.update(rs.ID, rs.ExerciseGroupID.Value, rs.ExerciseTitle, !rs.isHide.Value, aclog.UserName, rs.ParentQuestionID.Value,rs.DescribeAnswer);
                return Json(new { rs = true, }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "DeleteExerciseChoice", uid, ex.Message);
                SetAlert("Fail", "w");
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAnswerDrag(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                string answertrue = f.Get("txt-edit-answer-drag-true").ToString();
                tbl_Exercise ex = ExerciseTable.getbyid(f.Get("hdf-id").ToInt(0));
                if (ex == null)
                {
                    SetAlert("Không tìm thấy câu hỏi", "w");
                    return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                }
                tbl_ExerciceMultiChoise tr = ExerciceMultiChoiseTable.getbyanwertrue(ex.ID);
                ExerciceMultiChoiseTable.update(tr.ID, answertrue, true, aclog.UserName);
                string[] data = f.Get("hdf-data").ToString().Split('⅃');
                for (int i = 0; i < data.Length - 1; i++)
                {
                    string[] anwardata = data[i].Split('⅂');
                    string awnser = anwardata[0].ToString();
                    int idaw = anwardata[1].ToInt(0);

                    if (idaw == 0)
                        ExerciceMultiChoiseTable.insert(ex.ID, awnser, false, aclog.UserName);
                    else
                        ExerciceMultiChoiseTable.update(idaw, awnser, false, aclog.UserName);
                }
                SetAlert("Thành công", "s");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "UpdateAnswerDrag", uid, ex.Message);
                SetAlert("Fail", "w");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }

        public ActionResult Store(int page = 1)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                IEnumerable<tbl_SetPackage> l = SetPackageTable.getall().Where(n => n.Show == true && n.isHide != true).OrderByDescending(n => n.ID).ToList();
                int pageSize = 10;
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                {
                    ViewBag.Search = Request.QueryString["search"];
                    string search = Request.QueryString["search"].ToString();
                    l = from ex in l
                        where AssetCRM.RemoveUnicode(ex.PackageName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                        select ex;
                }
                int setType = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["type"]))
                {
                    setType = Request.QueryString["type"].ToInt(0);
                    if (setType == 1)
                        l = l.Where(n => n.Price == 0).ToList();
                    if (setType == 2)
                        l = l.Where(n => n.Price > 0).ToList();
                }
                ViewBag.SetType = setType;
                int sort = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
                {
                    sort = Request.QueryString["sort"].ToInt(0);
                    if (sort == 1)
                        l = l.OrderBy(n => n.PackageName).ToList();
                    if (sort == 2)
                        l = l.OrderByDescending(n => n.PackageName).ToList();
                    if (sort == 3)
                        l = l.OrderBy(n => n.CreatedDate).ToList();
                    if (sort == 4)
                        l = l.OrderByDescending(n => n.CreatedDate).ToList();
                }
                ViewBag.Sort = sort;
                ViewBag.TotalCount = l.Count();
                return View(l.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "Store", uid, ex.Message);
                SetAlert("Fail", "w");
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CheckOut(int ID = 0)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var Set = SetPackageTable.getbyid(ID);
                if (Set == null)
                {
                    SetAlert("Set not found", "e");
                    return RedirectToAction("Store", "SetPackage", new { area = "Admin" });
                }
                var setpackageStudent = SetPackageStudentTable.get_by_setID_and_studentID(Set.ID, aclog.ID);
                ViewBag.PaymentType = 1;
                if (setpackageStudent != null)
                {
                    ViewBag.PaymentType = setpackageStudent.PaymentType;
                }
                return View(Set);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "CheckOut", uid, ex.Message);
                SetAlert("Fail", "w");
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult AddExerciseChoice(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var nhombt = ExerciseGroupTable.getbyid(f.Get("hdf-group-id").ToInt(0));
                if (nhombt == null)
                {
                    SetAlert("Không tìm thấy nhóm bài tập", "e");
                    return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                }
                int parentQuestionID = f.Get("slt-parent-question").ToInt(0);
                int exid = f.Get("hdf-id").ToInt(0);
                string question = f.Get("hdf-question").ToString();
                string giaithich = f.Get("txt-giaithich-answer").ToString();
                var answertrue = f.Get("hdf-answer-true") == null ? f.Get("hdf-answer").ToString() : f.Get("hdf-answer-true").ToString();
                if (exid == 0)// tạo câu hỏi
                {
                    var q = ExerciseTable.insert(nhombt.ID, question, "", "", nhombt.TypeGroup.Value, 0, aclog.UserName, parentQuestionID, giaithich);
                    if (nhombt.TypeMultiChoice == 1)//chọn 1
                    {
                        if (nhombt.TypeGroup == 2)
                        {
                            string a = f.Get("txt-answer-a").ToString();
                            if (answertrue == "A")
                                ExerciceMultiChoiseTable.insert(q.ID, a, true, aclog.UserName);
                            else
                                ExerciceMultiChoiseTable.insert(q.ID, a, false, aclog.UserName);

                            string b = f.Get("txt-answer-b").ToString();
                            if (answertrue == "B")
                                ExerciceMultiChoiseTable.insert(q.ID, b, true, aclog.UserName);
                            else
                                ExerciceMultiChoiseTable.insert(q.ID, b, false, aclog.UserName);
                            string c = f.Get("txt-answer-c").ToString();
                            if (!string.IsNullOrEmpty(c))
                            {
                                if (answertrue == "C")
                                    ExerciceMultiChoiseTable.insert(q.ID, c, true, aclog.UserName);
                                else
                                    ExerciceMultiChoiseTable.insert(q.ID, c, false, aclog.UserName);
                            }
                            string d = f.Get("txt-answer-d").ToString();
                            if (!string.IsNullOrEmpty(d))
                            {
                                if (answertrue == "D")
                                    ExerciceMultiChoiseTable.insert(q.ID, d, true, aclog.UserName);
                                else
                                    ExerciceMultiChoiseTable.insert(q.ID, d, false, aclog.UserName);
                            }

                        }
                    }
                    else if (nhombt.TypeMultiChoice == 4)// chọn nhiều
                    {
                        string[] ans = answertrue.Split('⌉');
                        for (int i = 0; i < ans.Length - 1; i++)
                        {
                            string[] an = ans[i].Split('⌈');
                            if (an[0].ToInt(0) == 1)
                                ExerciceMultiChoiseTable.insert(q.ID, an[1].ToString(), true, aclog.UserName);
                            else
                                ExerciceMultiChoiseTable.insert(q.ID, an[1].ToString(), false, aclog.UserName);
                        }
                    }
                    else if (nhombt.TypeMultiChoice == 5)// map
                    {
                        ExerciceMultiChoiseTable.insert(q.ID, f.Get("txt-answer-map").ToString().Trim(), true, aclog.UserName);
                    }
                }
                else//cập nhật
                {
                    var ex = ExerciseTable.getbyid(exid);
                    if (ex == null)
                    {
                        SetAlert("Không tìm thấy câu hỏi", "e");
                        return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                    }
                    if (ex.ID == parentQuestionID)
                    {
                        parentQuestionID = 0;
                    }
                    var q = ExerciseTable.update(ex.ID, ex.ExerciseGroupID.Value, question, ex.isHide.Value, aclog.UserName, parentQuestionID, giaithich);
                    if (nhombt.TypeGroup == 2)
                    {
                        if (nhombt.TypeMultiChoice == 1)
                        {
                            string a = f.Get("txt-answer-a").ToString();
                            var lan = ExerciceMultiChoiseTable.getbyexeid(ex.ID);
                            if (!string.IsNullOrEmpty(a))
                            {
                                if (answertrue == "A")
                                    ExerciceMultiChoiseTable.update(lan[0].ID, a, true, aclog.UserName);
                                else
                                    ExerciceMultiChoiseTable.update(lan[0].ID, a, false, aclog.UserName);
                            }
                            string b = f.Get("txt-answer-b").ToString();
                            if (!string.IsNullOrEmpty(b))
                            {
                                if (answertrue == "B")
                                    ExerciceMultiChoiseTable.update(lan[1].ID, b, true, aclog.UserName);
                                else
                                    ExerciceMultiChoiseTable.update(lan[1].ID, b, false, aclog.UserName);
                            }
                            string c = f.Get("txt-answer-c").ToString();
                            if (!string.IsNullOrEmpty(c))
                            {
                                if (answertrue == "C")
                                    ExerciceMultiChoiseTable.update(lan[2].ID, c, true, aclog.UserName);
                                else
                                    ExerciceMultiChoiseTable.update(lan[2].ID, c, false, aclog.UserName);
                            }
                            string d = f.Get("txt-answer-d").ToString();
                            if (!string.IsNullOrEmpty(d))
                            {
                                if (answertrue == "D")
                                    ExerciceMultiChoiseTable.update(lan[3].ID, d, true, aclog.UserName);
                                else
                                    ExerciceMultiChoiseTable.update(lan[3].ID, d, false, aclog.UserName);
                            }
                        }
                        else if (nhombt.TypeMultiChoice == 4)// chọn nhiều
                        {
                            string[] ans = answertrue.Split('⌉');
                            for (int i = 0; i < ans.Length - 1; i++)
                            {
                                string[] an = ans[i].Split('⌈');
                                if (an[0].ToInt(0) == 1)
                                    ExerciceMultiChoiseTable.update(an[2].ToInt(0), an[1].ToString(), true, aclog.UserName);
                                else
                                    ExerciceMultiChoiseTable.update(an[2].ToInt(0), an[1].ToString(), false, aclog.UserName);
                            }
                        }
                        else if (nhombt.TypeMultiChoice == 5)// map
                        {
                            var anone = ExerciceMultiChoiseTable.getbyexeid(q.ID);
                            if (anone.Count > 0)
                                ExerciceMultiChoiseTable.update(anone.FirstOrDefault().ID, f.Get("txt-answer-map").ToString().Trim(), true, aclog.UserName);
                            else
                                ExerciceMultiChoiseTable.insert(q.ID, f.Get("txt-answer-map").ToString().Trim(), true, aclog.UserName);
                        }
                    }
                }
                SetAlert("Thành công", "s");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "AddExerciseChoice", uid, ex.Message);
                SetAlert("Fail", "w");
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult CheckoutSetPackage(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                int ID = f.Get("hdfSetID").ToInt(0);
                string code = f.Get("txt-discount");
                int paymenttype = f.Get("ddl-payment-type").ToInt(1);
                var pack = SetPackageTable.getbyid(ID);
                if (pack == null)
                {
                    SetAlert("Không tìm thấy gói bài!", "e");
                    return RedirectToAction("Store", "SetPackage", new { area = "Admin" });
                }
                //giáo viên nào sẽ là người chấm bài cho người này bây giờ?
                List<int> lexaminerID = SetPackageExaminersTable.getlistexaminer().OrderBy(n => n.TeacherID).Select(n => n.TeacherID.Value).ToList();
                if (lexaminerID.Count <= 0)
                {
                    return Json(new { rs = false, type = 2, message = "Chưa có giáo viên chấm gói bài này!" }, JsonRequestBehavior.AllowGet);
                }
                int startID = 0;
                tbl_SetPackageStudent firstset = SetPackageStudentTable.getall().OrderByDescending(n => n.ExaminerID).FirstOrDefault();
                if (firstset != null)
                {
                    startID = firstset.ExaminerID.Value;
                }
                List<int> lteacherid = new List<int>();
                lteacherid = lexaminerID.Where(n => n > startID).ToList();
                if (lexaminerID.Count > 0)
                {
                    lteacherid.AddRange(lexaminerID);
                }
                int UIDFix = lteacherid[0];
                var setpackagestudent = SetPackageStudentTable.get_by_setID_and_studentID(ID, aclog.ID);
                if (setpackagestudent == null)
                {
                    setpackagestudent = SetPackageStudentTable.insert(aclog.ID, aclog.FullName, ID, pack.Price.Value, paymenttype, 0, 0, UIDFix, aclog.UserName);
                }
                if (setpackagestudent.Paid.Value)
                {
                    SetAlert("Bạn đã mua gói này!", "e");
                    return RedirectToAction("Store", "SetPackage", new { area = "Admin" });
                }
                if (setpackagestudent == null)
                {
                    SetAlert("Thất bại!", "e");
                    return RedirectToAction("Store", "SetPackage", new { area = "Admin" });
                }
                if (paymenttype == 2)
                {
                    setpackagestudent = SetPackageStudentTable.updatePaid(setpackagestudent.ID, true, aclog.UserName);
                    SetAlert("Thành công!", "s");
                    return RedirectToAction("MySet", "StudentSet", new { area = "" });

                    //var client = new RestClient("https://http://app.asworg.com/wp-json/create/createorder/");
                    //client.Timeout = -1;
                    //var request = new RestRequest(Method.POST);
                    //request.AddHeader("key", "YXBpa2V5Y2hvYXB6aW0");
                    //request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                    //request.AddHeader("Content-Type", "multipart/form-data");
                    //request.AlwaysMultipartFormData = true;
                    //request.AddParameter("order_id", setpackagestudent.ID.ToString());
                    //request.AddParameter("title", setpackagestudent.StudentName + " - " + pack.PackageName);
                    //request.AddParameter("price", setpackagestudent.SetPrice.Value.ToString());
                    //request.AddParameter("name", setpackagestudent.StudentName);
                    //request.AddParameter("email", aclog.Email);
                    //request.AddParameter("phone", aclog.Phone);
                    //IRestResponse response = client.Execute(request);
                    //ZIMPaymentAPI result = JsonConvert.DeserializeObject<ZIMPaymentAPI>(response.Content);
                    //if (result.code == "Thành công")
                    //{
                    //    SetAlert("Thành công!", "s");
                    //    return Redirect(result.mess);
                    //}
                    //else
                    //{
                    //    SetAlert("Thất bại!", "e");
                    //    return RedirectToAction("Store", "SetPackage", new { area = "Admin" });
                    //}
                }
                else
                {
                    setpackagestudent = SetPackageStudentTable.udpatePaymentType(setpackagestudent.ID, paymenttype, aclog.UserName);
                    SetAlert("Thành công!", "s");
                    return RedirectToAction("MySet", "StudentSet", new { area = "" });
                }


            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "CheckoutSetPackage", uid, ex.Message);
                SetAlert("Fail", "w");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }

        }
        public class ZIMPaymentAPI
        {
            public string code { get; set; }
            public string mess { get; set; }
        }
        [HttpPost]
        public ActionResult GetSetFree(int ID)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false, type = 2, message = "Your session was out!" }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var setpackagestudent = SetPackageStudentTable.get_by_setID_and_studentID(ID, aclog.ID);
                if (setpackagestudent != null)
                    return Json(new { rs = false, type = 2, message = "You not have this set!" }, JsonRequestBehavior.AllowGet);
                var setpackage = SetPackageTable.getbyid(ID);
                if (setpackage == null || setpackage.Price > 0)
                    return Json(new { rs = false, type = 2, message = "Set not found" }, JsonRequestBehavior.AllowGet);
                //giáo viên nào sẽ là người chấm bài cho người này bây giờ?
                List<int> lexaminerID = SetPackageExaminersTable.getlistexaminer().OrderBy(n => n.TeacherID).Select(n => n.TeacherID.Value).ToList();
                if (lexaminerID.Count <= 0)
                {
                    return Json(new { rs = false, type = 2, message = "The examiner was not found!" }, JsonRequestBehavior.AllowGet);
                }
                int startID = 0;
                tbl_SetPackageStudent firstset = SetPackageStudentTable.getall().OrderByDescending(n => n.ID).FirstOrDefault();
                if (firstset != null)
                {
                    startID = firstset.ExaminerID.Value;
                }
                List<int> lteacherid = new List<int>();
                lteacherid = lexaminerID.Where(n => n > startID).ToList();
                if (lteacherid.Count <= 0)
                {
                    lteacherid.AddRange(lexaminerID);
                }
                int UIDFix = lteacherid[0];
                setpackagestudent = SetPackageStudentTable.insert(aclog.ID, aclog.FullName, ID, setpackage.Price.Value, 0, 0, 0, UIDFix, aclog.UserName);
                setpackagestudent = SetPackageStudentTable.updatePaid(setpackagestudent.ID, true, aclog.UserName);// Cập nhật đã thanh toán rồi=> xác nhận sở hữu => cho làm bài
                string link = Url.Action("MySet", "StudentSet", new { area = "" });
                if (setpackagestudent == null)
                    return Json(new { rs = false, type = 2, message = "Some thing error!" }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { rs = true, link = link }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "GetSetFree", uid, ex.Message);
                SetAlert("Fail", "w");
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult Customer(int page = 1)
        {
            try
            {
                var df = DateTime.ParseExact("01/01/1001", "dd/MM/yyyy", null).Date;
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                //page size
                int pageSize = 20;

                string search = "";
                ViewBag.Search = Request.QueryString["search"];
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                    search = Request.QueryString["search"].ToString().Trim();
                SqlParameter[] parameters =
                {
                    new SqlParameter("@qsearch", SqlDbType.NVarChar,2000) { Value = search },
                    new SqlParameter("@roleID", SqlDbType.Int) { Value = aclog.RoleID.Value },
                    new SqlParameter("@aclogID", SqlDbType.Int) { Value = aclog.ID },

                };
                var rs = SqlHelper.ExecuteDataTable("Get_Student_Set_Package", parameters);
                IEnumerable<tbl_SetPackageStudent> l = (from rw in rs.AsEnumerable()
                                                        select new tbl_SetPackageStudent()
                                                        {
                                                            ID = Convert.ToInt32(rw["ID"]),
                                                            StudentID = Convert.ToInt32(rw["StudentID"]),
                                                            SetPackageID = Convert.ToInt32(rw["SetPackageID"]),
                                                            SetPrice = Convert.ToInt32(rw["SetPrice"]),
                                                            PaymentType = Convert.ToInt32(rw["PaymentType"]),
                                                            DiscountID = Convert.ToInt32(rw["DiscountID"]),
                                                            DiscountPrice = Convert.ToInt32(rw["DiscountPrice"]),
                                                            Paid = Convert.ToBoolean(rw["Paid"]),
                                                            CreatedDate = rw["CreatedDate"] == DBNull.Value ? df : Convert.ToDateTime(rw["CreatedDate"]),
                                                            CreatedBy = rw["CreatedBy"].ToString(),
                                                            ModifiedBy = rw["ModifiedBy"] == DBNull.Value ? "" : Convert.ToString(rw["ModifiedBy"]),
                                                            ModifiedDate = rw["ModifiedDate"] == DBNull.Value ? df : Convert.ToDateTime(rw["ModifiedDate"])
                                                        }).ToList();

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
                        l = l.OrderBy(n => n.SetPrice).ToList();
                    if (sort == 6)
                        l = l.OrderByDescending(n => n.SetPrice).ToList();
                }
                ViewBag.Sort = sort;
                ViewBag.Total = l.Count();
                return View(l.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "Customer", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetSetDetail(int id)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog == null || aclog.ID == 5)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var setpackagestudent = SetPackageStudentTable.getbyid(id);
                if (setpackagestudent == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                else
                {
                    var setdetail = SetPackageTable.getbyid(setpackagestudent.SetPackageID.Value);
                    SetDetail data = new SetDetail()
                    {
                        ID = setpackagestudent.ID,
                        SetName = setdetail.PackageName,
                        StudentName = setpackagestudent.StudentName,
                        SetPrice = string.Format("{0:N0}", setpackagestudent.SetPrice.Value),
                        DiscountID = setpackagestudent.DiscountID.Value,
                        DiscountPrice = string.Format("{0:N0}", setpackagestudent.DiscountPrice.Value),
                        PriceLeft = string.Format("{0:N0}", setpackagestudent.SetPrice.Value - setpackagestudent.DiscountPrice.Value)
                    };
                    return Json(new { rs = true, data = data }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "GetSetDetail", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }


        }
        public ActionResult PaidSet(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog == null || aclog.ID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                int SetPackageStudentID = f.Get("hdfSetID").ToInt(0);
                var set = SetPackageStudentTable.getbyid(SetPackageStudentID);
                if (set == null)
                {
                    SetAlert("Set not found", "e");
                    return RedirectToAction("Customer", "SetPackage", new { area = "Admin" });
                }
                if (set.Paid.Value)
                {
                    SetAlert("Paid", "e");
                    return RedirectToAction("Customer", "SetPackage", new { area = "Admin" });
                }
                tbl_Account ac = AccountTable.getbyID(set.StudentID.Value);
                int setpriceLeft = set.SetPrice.Value - set.DiscountPrice.Value;
                int price = f.Get("txt-paid-money").Replace(",", "").ToInt(0);
                if (price != setpriceLeft)
                {
                    SetAlert("Please enter the correct set price!", "e");
                    return RedirectToAction("Customer", "SetPackage", new { area = "Admin" });
                }
                int schoolID = f.Get("ddl-school-received").ToInt(0);
                var schoolChoose = SchoolTable.getbyID(schoolID);
                string note = f.Get("txt-note").ToString();
                string paymentName = "";
                if (set.PaymentType == 1)
                    paymentName = "Zim Center";
                if (set.PaymentType == 2)
                    paymentName = "Gate";
                set = SetPackageStudentTable.updatePaid(set.ID, true, aclog.UserName);
                if (set == null)
                {
                    SetAlert("Cập nhật lỗi!", "e");
                    return RedirectToAction("Customer", "SetPackage", new { area = "Admin" });
                }
                else
                {
                    string name = "";
                    name = "7-" + set.ID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";//7 Thanh toán set
                    string nameencode = AppZim.ZIM.Encryptor.EncryptURL(name);
                    string strPathAndQuery = Request.Url.PathAndQuery;
                    string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                    string datalink = "" + strUrl + "invoice/?code=" + nameencode;
                    var barcode = AppZim.ZIM.AssetCRM.CreateQRCode(datalink, name);
                    tbl_Invoices iv = CashierController.InBill(0,6, set.ID, ac, price, note, "", 0, "", schoolChoose, aclog, set.PaymentType.Value, paymentName, name, nameencode, datalink);
                    if (iv != null)
                        SetlinkInvoice(datalink);
                    else
                    {
                        SetAlert("Không thành công", "e");
                        return RedirectToAction("Customer", "SetPackage", new { area = "Admin" });
                    }
                }
                SetAlert("Thành công", "s");
                return RedirectToAction("Customer", "SetPackage", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "PaidSet", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAnswerWrite(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });
                tbl_Exercise ex = ExerciseTable.getbyid(f.Get("hdf-id").ToInt(0));
                if (ex == null)
                {
                    SetAlert("Không tìm thấy câu hỏi", "w");
                    return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                }
                string[] data = f.Get("hdf-data").ToString().Split('⅃');
                for (int i = 0; i < data.Length - 1; i++)
                {
                    string[] anwardata = data[i].Split('⅂');
                    string awnser = anwardata[0].ToString();
                    int idaw = anwardata[1].ToInt(0);

                    if (idaw == 0)
                        ExerciceMultiChoiseTable.insert(ex.ID, awnser, true, aclog.UserName);
                    else
                        ExerciceMultiChoiseTable.update(idaw, awnser, true, aclog.UserName);
                }
                SetAlert("Thành công", "s");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "UpdateAnswerWrite", uid, ex.Message);
                SetAlert("Fail", "w");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateSetDetailSkill(FormCollection f, HttpPostedFileBase fileupload)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var set = SetPackageTable.getbyid(f.Get("hdf-set-id").ToInt(0));
                if (set == null)
                {
                    SetAlert("Không tìm thấy", "e");
                    return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                }
                int tskill = f.Get("hdf-set-type").ToInt(0);
                string des = f.Get("txt-des") == null ? "" : f.Get("txt-des").ToString();
                tbl_SetPackageDetail sd = SetPackageDetailTable.getbysetid(set.ID).Where(n => n.TypeDetail == tskill).FirstOrDefault();
                int minute = f.Get("txt-time-set").ToInt(0);
                if (sd != null)
                {
                    string link = "";
                    if (fileupload != null)
                    {
                        string name = Path.GetFileNameWithoutExtension(fileupload.FileName);
                        string ext = Path.GetExtension(fileupload.FileName);
                        var fileName = Guid.NewGuid() + ext;
                        var path = Path.Combine(Server.MapPath("~/Upload/exercise/"), fileName);
                        link = "/Upload/exercise/" + fileName;
                        fileupload.SaveAs(path);
                        minute = ZIM.AssetCRM.GetVideoDuration(HttpContext.Server.MapPath("~" + link)) + 2;
                    }
                    //if (sd.TypeDetail == 3)
                    //{
                    //    minute = 60;
                    //}
                    SetPackageDetailTable.update(sd.ID, link, minute, des, aclog.UserName);
                    SetAlert("Thành công", "s");
                }
                else
                    SetAlert("Thất bại", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageController", "UpdateSetDetailSkill", uid, ex.Message + "---" + ex.StackTrace + "---" + ex.Source);
                SetAlert("Thất bại", "w");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }

        }
        public ActionResult ListExaminer()
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Bạn đã hết phiên làm việc!" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Bạn đã hết phiên làm việc!" }, JsonRequestBehavior.AllowGet);
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            List<tbl_SetPackageExaminers> l = SetPackageExaminersTable.getlistexaminer().ToList();
            return View(l);
        }
        [HttpPost]
        public ActionResult AddExaminer(int tId, string check)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Bạn đã hết phiên làm việc!" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Bạn đã hết phiên làm việc!" }, JsonRequestBehavior.AllowGet);
            if (aclog.RoleID == 5)// học viên bị đá ra
                return Json(new { rs = false, message = "Thất bại!" }, JsonRequestBehavior.AllowGet);
            tbl_Account teacher = AccountTable.getbyID(tId);
            if (teacher == null)
            {
                return Json(new { rs = false, message = "Không tìm thấy giáo viên!" }, JsonRequestBehavior.AllowGet);
            }
            tbl_SetPackageExaminers se = SetPackageExaminersTable.getbyexaminer(tId);
            int id = 0;
            if (se != null)
            {
                id = se.ID;
            }
            se = SetPackageExaminersTable.insert(id, teacher.ID, teacher.FullName, teacher.Phone, Convert.ToBoolean(check), aclog.UserName);
            if (se != null)
            {
                StringBuilder html = new StringBuilder();
                html.Append("<tr id=\"examiner-" + se.ID + "\">");
                html.Append("   <td>" + se.TeacherName + "</td>");
                html.Append("    <td>" + se.TeacherPhone + "</td>");
                if (se.IsSelect.Value)
                {
                    html.Append("    <td><a href=\"javascript:;\" class=\"btn btn-icon delete-teacher\" data-teacherID=\"" + se.TeacherID + "\" style=\"color: red\"><i data-feather=\"delete\" style=\"height: 20px; width: 20px\" class=\"left\"></i></a></td>");
                }
                html.Append("</tr>");
                return Json(new { rs = true, message = "Thành công!", html = html.ToString(), dataid = se.ID }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { rs = false, message = "Đã xảy ra lỗi!" }, JsonRequestBehavior.AllowGet);
            }

        }
        //[HttpPost]
        //public ActionResult AddTopic(int tpid, int setdetailid)
        //{

        //    if (Session["UID"] == null)
        //        return Json(new { rs = false, message = "You are out of session!" }, JsonRequestBehavior.AllowGet);
        //    var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
        //    if (aclog == null)
        //        return Json(new { rs = false, message = "You are out of session!" }, JsonRequestBehavior.AllowGet);
        //    tbl_SetPackageDetail sd = SetPackageDetailTable.getbyid(setdetailid);
        //    if (sd == null)
        //    {
        //        return Json(new { rs = false, message = "Set not found!" }, JsonRequestBehavior.AllowGet);
        //    }
        //    tbl_Topic tp = TopicTable.getbyid(tpid);
        //    if (tp == null)
        //    {
        //        return Json(new { rs = false, message = "Topic not found!" }, JsonRequestBehavior.AllowGet);
        //    }
        //    sd = SetPackageDetailTable.updateTopic(sd.ID, tp.ID);
        //    if (sd == null)
        //    {
        //        return Json(new { rs = false, message = "Some thing error!" }, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        return Json(new { rs = true, message = "Success!", tp = tp }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult UpdateAnswerMap(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            int GroupID = f.Get("hdfMapID").ToInt(0);
            tbl_ExerciseGroup exgroup = ExerciseGroupTable.getbyid(GroupID);
            if (exgroup == null)
            {
                SetAlert("Thất bại", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            int noiseID = f.Get("hdfMapNoiseID").ToInt(0);
            string noiseTitle = "";
            if (string.IsNullOrEmpty(f.Get("txt-edit-answer-noise-map")))
            {
                SetAlert("Please enter noise answer title", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            else
            {
                noiseTitle = f.Get("txt-edit-answer-noise-map").ToString().Trim();
            }
            if (noiseID == 0)
            {
                //tạo mới
                tbl_ExerciceMultiChoise noiseword = ExerciceMultiChoiseTable.insertNoiseWord(GroupID, noiseTitle, aclog.UserName);
            }
            else
            {
                //update
                tbl_ExerciceMultiChoise noiseword = ExerciceMultiChoiseTable.getnoisewordbyID(noiseID);
                if (noiseword == null)
                {
                    SetAlert("Thất bại", "e");
                    return Redirect(Request.UrlReferrer.AbsoluteUri);
                }
                noiseword = ExerciceMultiChoiseTable.updateNoiseWord(noiseID, noiseTitle, aclog.UserName);
            }
            SetAlert("Thành công", "s");
            return Redirect(Request.UrlReferrer.AbsoluteUri);

        }
        public ActionResult DeleteNoiseWord(int id)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Phiên làm việc đã hết hạn!" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Phiên làm việc đã hết hạn!" }, JsonRequestBehavior.AllowGet);
            if (aclog.RoleID == 5)// học viên bị đá ra
                return Json(new { rs = false, message = "Thất bại!" }, JsonRequestBehavior.AllowGet);
            tbl_ExerciceMultiChoise noiseword = ExerciceMultiChoiseTable.getnoisewordbyID(id);
            if (noiseword == null)
            {
                return Json(new { rs = false, message = "Thất bại!" }, JsonRequestBehavior.AllowGet);
            }
            noiseword = ExerciceMultiChoiseTable.deleteNoiseWord(noiseword.ID, aclog.UserName);
            return Json(new { rs = true, message = "Thành công!" }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult GetQuestionCount(int ID)
        {
            return Json(new { rs = true, questionID = 1 }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult LoadCbbQuestion(int id)
        {
            StringBuilder aQ_form = new StringBuilder();
            var childquestions = ExerciseTable.getbygroupid(id).ToList();
            var ExerciseTitle = "";
            aQ_form.Append("<option value=\"0\">None</option>");
            foreach (var item in childquestions)
            {
                if (!string.IsNullOrEmpty(item.ExerciseTitle) && item.ExerciseTitle.Length > 50)
                {
                    ExerciseTitle = item.ExerciseTitle.Substring(0, 50) + "...";
                }
                else
                {
                    ExerciseTitle = item.ExerciseTitle;
                }

                aQ_form.Append("<option value=\"" + item.ID + "\">" + ExerciseTitle + "</option>");
            }

            return Json(new { rs = true, aQ_form = aQ_form.ToString() }, JsonRequestBehavior.AllowGet);

        }


    }

    public class SetDetail
    {
        public int ID { get; set; }
        public string StudentName { get; set; }
        public int StudentID { get; set; }
        public string SetName { get; set; }
        public string SetPrice { get; set; }
        public int DiscountID { get; set; }
        public string DiscountPrice { get; set; }
        public string PriceLeft { get; set; }
    }
}