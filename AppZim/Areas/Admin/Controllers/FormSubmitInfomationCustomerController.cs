using AppZim.Models;
using AppZim.TableSql;
using MB.Extensions;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace AppZim.Areas.Admin.Controllers
{
    public class FormSubmitInfomationCustomerController : BaseController
    {
        // GET: Admin/FormSubmitInfomationCustomer
        public ActionResult ListForms(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            var l = FormReceiveInfoSettingTable.getall().OrderByDescending(n => n.ID).ToList();          
            return View(l.ToPagedList(page, pageSize));
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AddSettingForm(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int source = f.Get("ddl-source").ToInt(0);
            int sale = f.Get("ddl-saler").ToInt(0);
            int role = f.Get("ddl-role").ToInt(0);
            string emailcontent = f.Get("hdfanswer").ToString();
            string title = f.Get("txt-title").ToString();
            string note = f.Get("txt-note").ToString();

            int id = f.Get("hdfID").ToInt(0);
            var rs = new tbl_FormReceiveInfoSetting();
            if (id == 0)
                rs = FormReceiveInfoSettingTable.insert(source, sale, role, title, emailcontent, note, aclog.UserName);
            else
                rs = FormReceiveInfoSettingTable.update(id, source, sale, role, title, emailcontent, note, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không hành công", "e");
            return RedirectToAction("ListForms", "FormSubmitInfomationCustomer");
        }

        [HttpPost]
        public ActionResult GetInfoForm(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            var f = FormReceiveInfoSettingTable.getbyid(id);
            if (f == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { rs = true, l = f }, JsonRequestBehavior.AllowGet);
        }

        #region upload file audio qua ajax
        [HttpPost]
        public ActionResult UploadFileImage()
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
                            return Json(new { rs = false, l = "File của bạn không được hỗi trợ" }, JsonRequestBehavior.AllowGet);
                        }
                        var fileName = Guid.NewGuid() + ext;
                        var path = Path.Combine(Server.MapPath("~/Upload/imageform/"), fileName);

                        string strPathAndQuery = Request.Url.PathAndQuery;
                        string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");

                        link = "" + strUrl + "Upload/imageform/" + fileName;
                        file.SaveAs(path);
                    }
                }
            }
            return Json(new { rs = true, l = link }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        [HttpGet]
        public ActionResult SaveInfo(int id)
        {
            var ck = FormReceiveInfoSettingTable.getbyid(id);
            string title = "Thông tin đăng ký";
            string note = "";
            if (ck != null)
            {
                title = ck.TitileForm;
                note = ck.Note;
            }
            ViewBag.FormID = id;
            ViewBag.TitleForm = title;
            ViewBag.Note = note;
            return View();
        }
        [HttpPost]
        public ActionResult SubmitSaveInfo(FormCollection f)
        {
            int selerID = 0;
            var sc = SourceOfCustomerTable.getall().OrderBy(n => n.ID).FirstOrDefault();
            int formid = f.Get("hdfFormID").ToInt(0);
            var fo = FormReceiveInfoSettingTable.getbyid(formid);
            if (fo != null)
            {
                if (fo.SalerID != null)
                    selerID = fo.SalerID.Value;
                sc.ID = 0;
                sc.SourceOfCustomer = "TVV";
            }            

            var name = f.Get("txt-name").ToString();
            var md = AcademicPurposesTable.getbyID(f.Get("ddl-academicpurposes").ToInt(0));
            if (md == null)
            {
                md = new tbl_AcademicPurposes();
                md.ID = 0;
                md.AcademicPurposesName = "";
            }           
            var email = f.Get("txt-email").ToString();
            var phone = f.Get("txt-phone").ToString();
            var birthday = f.Get("txt-birthday").ToString();
            var note = f.Get("txt-note").ToString();
            string edutype = f.Get("hdfEduType").ToString();
            int language = f.Get("dll-language").ToInt(0);
            var district = DistrictTable.getbyID(f.Get("ddl-district").ToInt(1));
            var job = JobTable.getbyid(f.Get("ddl-job").ToInt(0));

            tbl_Ward w = new tbl_Ward();
            w.ID = 0;
            w.WardName = "";
            tbl_Street s = new tbl_Street();
            s.ID = 0;
            s.StreetName = "";
            string homenumber = "";

            var ck = AccountTable.getbyusername(phone);
            if (ck == null)
                ck = AccountTable.insertcustomer(district, name, email, phone, district.DistrictName + "-" + district.LocationName, 1, "", selerID, sc.ID, sc.SourceOfCustomer, birthday, job.ID, job.JobName,
                    "", md.ID, md.AcademicPurposesName, "", "", "", edutype, "", 0, "", "", "", "form", false, w, s, homenumber, language,"", 0);
            //add note
            if (!string.IsNullOrEmpty(note))// lưu lại ghi chú học viên
                AccountHistoryChangeTable.insert(ck.ID, ck.FullName, note, fo.CreatedBy);
            //
            if (fo != null)//gửi mail và thông báo
            {
                //gửi cho học viên
                AppZim.ZIM.AssetCRM.SendMail(ck.Email, fo.TitileForm, fo.ContentForm);
                // gửi cho thằng được nhận
            }
            TempData["SubmitSuccess"] = 1;
            return RedirectToAction("SaveInfo", "FormSubmitInfomationCustomer", new { id = formid });
        }
    }
}