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
    public class SetPackageCustomController : BaseController
    {
        // GET: Admin/SetPackageCustom
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ListSetPackages(int page = 1)
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
                AssetCRM.Writelog("SetPackageController", "ListSetPackages", uid, ex.Message);
                SetAlert("Fail", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }


        }

        /// <summary>
        /// Tạo gói theo custom trung tâm Nhật
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CreateSets()
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
                AssetCRM.Writelog("SetPackageCustomController", "CreateSets", uid, ex.Message);
                SetAlert("Fail", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }

        /// <summary>
        /// Custom cho trung tâm tiếng Nhật
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SetDetails(int id)
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
                    return RedirectToAction("ListSetPackages", "SetPackage");
                }
                return View(pk);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("SetPackageCustomController", "SetDetails", uid, ex.Message);
                SetAlert("Fail", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult AddSets(FormCollection f, HttpPostedFileBase[] setThumbnail)
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
                bool Listen = f["rd-lis"] != null ? true : false;  //nghe hiểu
                bool Reading = f["rd-read"] != null ? true : false; //đọc hiểu
                bool Vocabulary = f["rd-vocabulary"] != null ? true : false; //tự vựng - hán tự
                bool Grammar = f["rd-grammar"] != null ? true : false; //ngữ pháp

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
                                string name = System.IO.Path.GetFileNameWithoutExtension(setThumbnail[i].FileName);
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
                var set = SetPackageTable.Inserts(packageName, price, linkIMG, linkIMGThumbnails, packageDescription, Listen, Vocabulary, Reading, Grammar, aclog.UserName);
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
                AssetCRM.Writelog("SetPackageCustomController", "AddSets", uid, ex.Message);
                SetAlert("Fail", "w");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }

        public ActionResult Questions(int id = 0, int type = 0)
        {
            try
            {
                if (type == 1)
                {
                    ViewBag.QuestionTitle = "Nghe Hiểu";
                    ViewBag.QuestionType = 1;
                }
                else if (type == 2)
                {
                    ViewBag.QuestionTitle = "Đọc Hiểu";
                    ViewBag.QuestionType = 2;
                }
                else if (type == 3)
                {
                    ViewBag.QuestionTitle = "Ngữ pháp";
                    ViewBag.QuestionType = 3;
                }
                else if (type == 4)
                {
                    ViewBag.QuestionTitle = "Từ vựng - Hán từ";
                    ViewBag.QuestionType = 4;
                }
                else
                {
                    SetAlert("Không tìm thấy trang!", "e");
                    return RedirectToAction("ListSetPackages", "SetPackageCustom");
                }
                var set = SetPackageTable.getbyid(id);
                if (set == null)
                {
                    SetAlert("Không tìm thấy gói bài!", "e");
                    return RedirectToAction("ListSetPackages", "SetPackageCustom");
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
                AssetCRM.Writelog("SetPackageCustomController", "Questions", uid, ex.Message);
                SetAlert("Fail", "w");
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateSets(FormCollection f, HttpPostedFileBase[] setThumbnail)
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
                AssetCRM.Writelog("SetPackageCustomController", "UpdateSets", uid, ex.Message);
                SetAlert("Fail", "w");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }

        }
    }
}