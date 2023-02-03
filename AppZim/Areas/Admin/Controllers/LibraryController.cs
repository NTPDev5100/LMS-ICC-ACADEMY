using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AppZim.Areas.Admin.Controllers
{
    public class LibraryController : BaseController
    {
        public ActionResult DocumentList()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            DriveInfo dDrive = new DriveInfo("C");
            if (dDrive.IsReady)
            {
                var freespace = dDrive.AvailableFreeSpace / 1024 / 1024 / 1024;
                var totalspace = dDrive.TotalSize / 1024 / 1024 / 1024;

                ViewBag.UsedSpace = totalspace - freespace;
                ViewBag.TotalSpace = totalspace;

                double percent = Math.Round((double)(totalspace - freespace) / totalspace * 100, 0);
                ViewBag.Percent = percent;
            }
            return View();
        }

        [HttpPost]
        public ActionResult AddCateZim(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 7)
            {
                SetAlert("Bạn không có quyền", "e");
                return RedirectToAction("DocumentList", "Library");
            }

            var cate = f.Get("txt-cate").ToString();
            var rs = DocumentZimTable.insertcatezim(cate, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("DocumentList", "Library");
        }

        [HttpPost]
        public ActionResult EditCateZim(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 7)
            {
                SetAlert("Bạn không có quyền", "e");
                return RedirectToAction("DocumentList", "Library");
            }

            var c = DocumentZimTable.getcatezimbyid(f.Get("hdfID").ToInt(0));
            if (c == null)
            {
                SetAlert("Không tìm thấy mục tài liệu", "e");
                return RedirectToAction("DocumentList", "Library");
            }
            var cate = f.Get("txt-edit-cate").ToString();
            var rs = DocumentZimTable.updatecatezim(c.ID, cate, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("DocumentList", "Library");
        }

        [HttpPost]
        public ActionResult UpFileDocument(FormCollection f, HttpPostedFileBase fileimg, HttpPostedFileBase[] fileupload)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 7)
            {
                SetAlert("Bạn không có quyền", "e");
                return RedirectToAction("DocumentList", "Library");
            }
            var cate = DocumentZimTable.getcatezimbyid(f.Get("hdfCateID").ToInt(0));
            if (cate == null)
            {
                SetAlert("Không tìm thấy mục tài liệu", "e");
                return RedirectToAction("DocumentList", "Library");
            }

            if (ModelState.IsValid)
            {   //iterating through multiple file collection   
                string linkimg = "";
                if (fileimg != null)
                {
                    string name = Path.GetFileNameWithoutExtension(fileimg.FileName);
                    string ext = Path.GetExtension(fileimg.FileName);
                    if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                    {
                        SetAlert("File của bạn đã bị chặn, vui lòng không tải lên file .php, .asp, .aspx, .ps1", "e");
                        return RedirectToAction("DocumentList", "Library");
                    }
                    var fileName = Guid.NewGuid() + ext;
                    var path = Path.Combine(Server.MapPath("~/Upload/Library/"), fileName);
                    linkimg = "/Upload/Library/" + fileName;
                    fileimg.SaveAs(path);
                }
                foreach (HttpPostedFileBase file in fileupload)
                {
                    //Checking file is available to save.  
                    if (file != null)
                    {
                        string name = Path.GetFileNameWithoutExtension(file.FileName);
                        string ext = Path.GetExtension(file.FileName);
                        if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                        {
                            SetAlert("File của bạn đã bị chặn, vui lòng không tải lên file .php, .asp, .aspx, .ps1", "e");
                            return RedirectToAction("DocumentList", "Library");
                        }
                        var fileName = name + " " + GetDateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss") + ext;
                        var path = Path.Combine(Server.MapPath("~/Upload/Library/"), fileName);
                        string link = "/Upload/Library/" + fileName;
                        file.SaveAs(path);
                        DocumentZimTable.insertdoczim(cate.ID, file.FileName, linkimg, link, aclog.UserName);
                    }
                }
            }
            SetAlert("Thành công", "s");
            return RedirectToAction("DocumentList", "Library");
        }

        [HttpPost]// tải lớp của khối
        public ActionResult LoadDoc(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var l = DocumentZimTable.getdocbycateid(id).Where(n => n.isHide != true).ToList();
            StringBuilder p = new StringBuilder();
            int index = 1;
            foreach (var item in l)
            {
                string filePath = Server.MapPath(item.DocumentLink);
                if (!System.IO.File.Exists(filePath))
                    continue;
                FileInfo filesize = new FileInfo(filePath);
                double fz = Math.Round((double)filesize.Length / 1024 / 1024, 2);

                p.Append("<div class=\"col-6 col-sm-4 col-md-3\">");
                p.Append("  <div class=\"card card-file\">");
                p.Append("      <div class=\"dropdown-file\">");
                p.Append("          <a href = \"javascript:;\" class=\"dropdown-link\" data-toggle=\"dropdown\"><i data-feather=\"more-vertical\"></i></a>");
                p.Append("          <div class=\"dropdown-menu dropdown-menu-right\">");
                p.Append("              <a href = \"" + item.DocumentLink + "\" class=\"dropdown-item download\" download><i data-feather=\"download\"></i>Download</a>");
                if (aclog.RoleID == 1 || aclog.RoleID == 2 || aclog.RoleID == 7)
                    p.Append("          <a href = \"javascript:;\" class=\"dropdown-item delete\" data-id=" + item.ID + "><i data-feather=\"trash\"></i>Delete</a>");
                p.Append("          </div>");
                p.Append("      </div>");

                string[] arrname = item.DocumentLink.Split('.');
                string type = arrname[arrname.Length - 1].ToLower();
                if (type.Contains("doc") || type.Contains("docx"))//file worf
                {
                    p.Append("      <div class=\"card-file-thumb tx-primary\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-word\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Word Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }
                else if (type.Contains("xlsx") || type.Contains("xlsm") || type.Contains("xls"))//file excel
                {
                    p.Append("      <div class=\"card-file-thumb tx-success\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-excel\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Excel Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }
                else if (type.Contains("pdf"))//file pdf
                {
                    p.Append("      <div class=\"card-file-thumb tx-danger\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-pdf\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Pdf Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }
                else if (type.Contains("ppt"))//file powerpoint
                {
                    p.Append("      <div class=\"card-file-thumb tx-orange\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-powerpoint\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Powerpoint Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }
                else if (type.Contains("jpg") || type.Contains("png") || type.Contains("gif") || type.Contains("tiff") || type.Contains("bmp"))//file img
                {
                    p.Append("      <div class=\"card-file-thumb tx-indigo\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-image\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Photo Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }
                else if (type.Contains("mp3") || type.Contains("wma") || type.Contains("wav") || type.Contains("flac") || type.Contains("ogg") || type.Contains("aiff") || type.Contains("alac"))//file audio
                {
                    p.Append("      <div class=\"card-file-thumb tx-info\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-audio\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Audio Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }
                else if (type.Contains("mp4") || type.Contains("3gp") || type.Contains("asf") || type.Contains("avi") || type.Contains("mov") || type.Contains("rm") || type.Contains("vob") || type.Contains("wmv"))//file video
                {
                    p.Append("      <div class=\"card-file-thumb tx-pink\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-video\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Video Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }
                else
                {
                    p.Append("      <div class=\"card-file-thumb tx-gray-600\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-alt\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Orther Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }

                p.Append("  </div>");
                p.Append("</div>");
                if (index % 4 == 0)
                    p.Append("<div class=\"d-none d-xl-block wd-100p ht-10\"></div>");
                index += 1;
            }

            return Json(new { rs = p.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]// tải lớp của khối
        public ActionResult SearchDocument(string q)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            IEnumerable<tbl_DocumentZim> l = DocumentZimTable.getalldoc();
            IEnumerable<tbl_DocumentCategoryZim> cate = DocumentZimTable.getcatezimall();

            l = from t in l
                join c in cate on t.CategoryZimID equals c.ID
                where c.isHide != true
                select t;

            l = from t in l
                where AssetCRM.RemoveUnicode(t.DocumentName).ToUpper().Contains(AssetCRM.RemoveUnicode(q).ToUpper())
                select t;

            StringBuilder p = new StringBuilder();
            int index = 1;
            foreach (var item in l)
            {
                string filePath = Server.MapPath(item.DocumentLink);
                if (!System.IO.File.Exists(filePath))
                    continue;
                FileInfo filesize = new FileInfo(filePath);
                double fz = Math.Round((double)filesize.Length / 1024 / 1024, 2);

                p.Append("<div class=\"col-6 col-sm-4 col-md-3\">");
                p.Append("  <div class=\"card card-file\">");
                p.Append("      <div class=\"dropdown-file\">");
                p.Append("          <a href = \"javascript:;\" class=\"dropdown-link\" data-toggle=\"dropdown\"><i data-feather=\"more-vertical\"></i></a>");
                p.Append("          <div class=\"dropdown-menu dropdown-menu-right\">");
                p.Append("              <a href = \"" + item.DocumentLink + "\" class=\"dropdown-item download\" download><i data-feather=\"download\"></i>Download</a>");
                if (aclog.RoleID == 1 || aclog.RoleID == 2 || aclog.RoleID == 7)
                    p.Append("          <a href = \"javascript:;\" class=\"dropdown-item delete\" data-id=" + item.ID + "><i data-feather=\"trash\"></i>Delete</a>");
                p.Append("          </div>");
                p.Append("      </div>");

                string[] arrname = item.DocumentLink.Split('.');
                string type = arrname[arrname.Length - 1].ToLower();
                if (type.Contains("doc") || type.Contains("docx"))//file worf
                {
                    p.Append("      <div class=\"card-file-thumb tx-primary\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-word\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Word Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }
                else if (type.Contains("xlsx") || type.Contains("xlsm") || type.Contains("xls"))//file excel
                {
                    p.Append("      <div class=\"card-file-thumb tx-success\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-excel\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Excel Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }
                else if (type.Contains("pdf"))//file pdf
                {
                    p.Append("      <div class=\"card-file-thumb tx-danger\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-pdf\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Pdf Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }
                else if (type.Contains("ppt"))//file powerpoint
                {
                    p.Append("      <div class=\"card-file-thumb tx-orange\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-powerpoint\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Powerpoint Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }
                else if (type.Contains("jpg") || type.Contains("png") || type.Contains("gif") || type.Contains("tiff") || type.Contains("bmp"))//file img
                {
                    p.Append("      <div class=\"card-file-thumb tx-indigo\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-image\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Photo Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }
                else if (type.Contains("mp3") || type.Contains("wma") || type.Contains("wav") || type.Contains("flac") || type.Contains("ogg") || type.Contains("aiff") || type.Contains("alac"))//file audio
                {
                    p.Append("      <div class=\"card-file-thumb tx-info\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-audio\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Audio Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }
                else if (type.Contains("mp4") || type.Contains("3gp") || type.Contains("asf") || type.Contains("avi") || type.Contains("mov") || type.Contains("rm") || type.Contains("vob") || type.Contains("wmv"))//file video
                {
                    p.Append("      <div class=\"card-file-thumb tx-pink\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-video\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Video Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }
                else
                {
                    p.Append("      <div class=\"card-file-thumb tx-gray-600\" data-id=" + item.CategoryZimID + ">");
                    p.Append("          <i class=\"far fa-file-alt\"></i>");
                    p.Append("      </div>");
                    p.Append("      <div class=\"card-body\">");
                    p.Append("          <h6><a href = \"" + item.DocumentLink + "\" class=\"link-02\">" + item.DocumentName + "</a></h6>");
                    p.Append("          <p>Orther Document</p>");
                    p.Append("          <span>" + fz + " Mb</span>");
                    p.Append("      </div>");
                }

                p.Append("  </div>");
                p.Append("</div>");
                if (index % 4 == 0)
                    p.Append("<div class=\"d-none d-xl-block wd-100p ht-10\"></div>");
                index += 1;
            }
            return Json(new { rs = p.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]// tải lớp của khối
        public ActionResult DeleteDoc(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 1 || aclog.RoleID == 2 || aclog.RoleID == 7)
            {
                var rs = DocumentZimTable.deletedoczim(id, aclog.UserName);
                if (rs != null)
                {
                    var doc = DocumentZimTable.GetDocByID(id);
                    int count = DocumentZimTable.getdocbycateid(doc.CategoryZimID.Value).Count();
                    return Json(new { rs = true, countDoc = count, cateID = doc.CategoryZimID }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        }
    }
}