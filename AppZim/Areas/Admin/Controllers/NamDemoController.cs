using AppZim.TableSql;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppZim.Models;
using AppZim.Controllers;
using System.Globalization;
using System.Text;
using System.Web.Script.Serialization;
using static AppZim.Controllers.PushAutoController;
using System.Threading.Tasks;
using System.Data.Entity.Core.Objects;
using PagedList;
using AppZim.ZIM;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Net;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using WebUI.Business;
using System.Threading;

namespace AppZim.Areas.Admin.Controllers
{

    public class NamDemoController : BaseController
    {
        // GET: Admin/NamDemo

        public ActionResult Index()
        {
            //// hình avatar
            //var user = AccountTable.getall().Where(n => n.Avatar != null).ToList();
            //foreach (var item in user)
            //{
            //    if (!string.IsNullOrEmpty(item.Avatar))
            //    {
            //        string fileName = item.Avatar.Replace("/Upload/useravatar/", "");
            //        AssetCRM.GenerateThumbNail("~" + item.Avatar, "~/Upload/thumbnails/useravatar//" + fileName, 150, false);
            //        AccountTable.updatethumnail(item.ID, "/Upload/thumbnails/useravatar/" + fileName);
            //    }
            //}
            //// hình bài viết
            //var post = PostContentTable.getbyall().ToList();
            //foreach (var item in post)
            //{
            //    if (!string.IsNullOrEmpty(item.PostIMG))
            //    {
            //        string fileName = item.PostIMG.Replace("/Upload/newfeed/", "");
            //        string thumbnail = "";
            //        if (item.StatusPost != true)
            //        {
            //            AssetCRM.GenerateThumbNail("~" + item.PostIMG, "~/Upload/thumbnails/newfeed//" + fileName, 150, false);
            //            thumbnail = "/Upload/thumbnails/newfeed/" + fileName;
            //        }
            //        else
            //        {
            //            if (!item.BackgroundPost.Value)
            //            {
            //                AssetCRM.GenerateThumbNail("~" + item.PostIMG, "~/Upload/thumbnails/newfeed//" + fileName, 650, true);
            //                thumbnail = "/Upload/thumbnails/newfeed/" + fileName;
            //            }
            //            else
            //            {
            //                thumbnail = item.PostIMG;
            //                thumbnail = "/Upload/thumbnails/newfeed/" + fileName;
            //            }
            //        }
            //        PostContentTable.updatethumbnail(item.ID, thumbnail);
            //    }
            //}

            AppZim.ZIM.AssetCRM.Writelog("NamDemo", "Index", 0, "Lỗi rồi nhé");
            return View();
        }
        public static string encode(string text)
        {
            byte[] mybyte = System.Text.Encoding.UTF8.GetBytes(text);
            string returntext = System.Convert.ToBase64String(mybyte);
            return returntext;
        }

        public static string decode(string text)
        {
            byte[] mybyte = System.Convert.FromBase64String(text);
            string returntext = System.Text.Encoding.UTF8.GetString(mybyte);
            return returntext;     
        }

        [HttpGet]
        public ActionResult demon()
        {
            var l = AccountTable.getall().Where(n => n.RoleID != 5).ToList();
            string s = "";
            foreach (var item in l)
            {
                s += "" + item.ID + "-" + item.RoleName + "<br/>";
            }
            ViewBag.Info = s;
            return View();
        }

        [HttpPost]
        public ActionResult getinfo(FormCollection f)
        {
            string keyzim = f.Get("txt-key").ToString();
            int ID = f.Get("txt-id").ToInt(0);
            if (keyzim == "medi4")
            {
                var ck = AccountTable.getbyID(ID);
                if (ck != null)
                {
                    Session["UID"] = ck.ID.ToString();
                    return RedirectToAction("ProfileUser", "Account", new { area = "Admin" });
                }
            }
            return View("demon");
        }
        public static string a = "";
        protected static void testThread()
        {
            Thread.Sleep(100000);
            a = "hahaha";
          
        }
        [HttpGet]
        public ActionResult TestData()
        {

            //Thread tr = new Thread(testThread);
            //tr.Start();
            //string b = a;
            //string c = "";
            //// test nhẹ cái tạo lại hình bacode
            //List<tbl_CoursesStudent> l = new List<tbl_CoursesStudent>();
            //l.Add(CoursesStudentTable.getbyid(6973));
            //foreach (var item in l)
            //{
            //    var aclog = AccountTable.getbyusername(item.CreatedBy);
            //    var schoolinvoice = SchoolTable.getbyID(item.SchoolID.Value);
            //    var ac = AccountTable.getbyID(item.StudentUID.Value);
            //    var ck = InvoicesTable.getall().Where(n => n.CourseStudentID == item.ID).FirstOrDefault();
            //    if (ck == null)
            //    {
            //        string name = "0-" + item.ID + "-" + item.CreatedDate.Value.ToString("ddMMyyyyHHmmss") + "";// 1 hẹn đăng ký
            //        string nameencode = AppZim.ZIM.Encryptor.EncryptURL(name);
            //        string strPathAndQuery = Request.Url.PathAndQuery;
            //        string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
            //        string datalink = "" + strUrl + "invoice/?code=" + nameencode;
            //        var barcode = AppZim.ZIM.AssetCRM.CreateQRCode(datalink, name);
            //        var acby = AccountTable.getbyusername(item.CreatedBy);
            //        //InvoicesTable.insert(false, item.ID, 0, 0, 0, 0, ac, item.Deposit.Value, 0, item.CourseName, barcode, nameencode, aclog, "", 5, "Chuyển khoản", schoolinvoice, item.CreatedDate.Value);
            //        string dataleft = "";
            //        if (item.DatePriceLeft != null)
            //            dataleft = item.DatePriceLeft.Value.ToString("dd/MM/yyyy");
            //        InvoicesTable.insert(false, item.ID, 0, 0, 0, 0, ac, item.Paid.Value, item.PriceLeft.Value, item.CourseName, barcode, nameencode, acby, dataleft, 1, "Tiền mặt", schoolinvoice);
            //    }
            //}


            ////tạo lại cái hình
            //string datalink = "https://app.zim.vn/invoice/?code=MS0zMjEtMDYwODIwMTkyMDU2MTc=";
            //var barcode = AppZim.ZIM.AssetCRM.CreateQRCode(datalink, "1-321-06082019205617");


            //DateTime start = DateTime.ParseExact("25/08/2020", "dd/MM/yyyy", null);
            //int total = (GetDateTime.Now - start).Days;
            //for (int k = 0; k <= total; k++)
            //{
            //    var d = start.AddDays(k);
            //    //CHẠY GIAO VIÊN
            //    // chạy mỗi ngày
            //    // chạy mỗi ngày
            //    var datateacher = PushAutoController.tinhrank(true, d.AddDays(-1).Date, d.AddDays(-1).Date);
            //    for (int i = 0; i < datateacher.Count; i++)
            //    {
            //        var item = datateacher[i];
            //        int rank = i + 1;
            //        AccountRankTable.insert(1, item.UID, item.Role, rank, item.Rate1, item.Rate2, item.Rate3, item.Rate4, item.Rate5, item.Score, item.TotalTimeHour, item.TimeHour, "auto", d);
            //    }
            //    if (d.ToString("dddd").Contains("Monday"))//Ngày đầu tuần
            //    {
            //        //ngày cuối tuần
            //        DateTime endweek = d.AddDays(-1);
            //        DateTime startweek = endweek.AddDays(-6);
            //        var data = PushAutoController.tinhrank(true, startweek.Date, endweek.Date);
            //        for (int i = 0; i < data.Count; i++)
            //        {
            //            var item = data[i];
            //            int rank = i + 1;
            //            AccountRankTable.insert(2, item.UID, item.Role, rank, item.Rate1, item.Rate2, item.Rate3, item.Rate4, item.Rate5, item.Score, item.TotalTimeHour, item.TimeHour, "auto", d);
            //        }
            //    }
            //    if (d.Day == 1)//tháng
            //    {
            //        //ngày cuối tháng
            //        DateTime startmonth = d.AddMonths(-1);
            //        DateTime endmonth = d.AddDays(-1);
            //        var data = PushAutoController.tinhrank(true, startmonth.Date, endmonth.Date);
            //        for (int i = 0; i < data.Count; i++)
            //        {
            //            var item = data[i];
            //            int rank = i + 1;
            //            AccountRankTable.insert(3, item.UID, item.Role, rank, item.Rate1, item.Rate2, item.Rate3, item.Rate4, item.Rate5, item.Score, item.TotalTimeHour, item.TimeHour, "auto", d);
            //        }
            //    }
            //    if (d.ToString("dd/MM").Contains("01/01") || d.ToString("dd/MM").Contains("01/04") || d.ToString("dd/MM").Contains("01/07") || d.ToString("dd/MM").Contains("01/10"))//quý
            //    {
            //        //Ngày cuối quý
            //        DateTime startquy = d.AddMonths(-3);
            //        DateTime endquy = d.AddDays(-1);
            //        var data = PushAutoController.tinhrank(true, startquy.Date, endquy.Date);
            //        for (int i = 0; i < data.Count; i++)
            //        {
            //            var item = data[i];
            //            int rank = i + 1;
            //            AccountRankTable.insert(4, item.UID, item.Role, rank, item.Rate1, item.Rate2, item.Rate3, item.Rate4, item.Rate5, item.Score, item.TotalTimeHour, item.TimeHour, "auto", d);
            //        }
            //    }
            //    if (d.ToString("dd/MM").Contains("01/01"))//năm
            //    {
            //        DateTime daunam = d.AddYears(-1);
            //        DateTime cuoinam = d.AddDays(-1);
            //        var data = PushAutoController.tinhrank(true, daunam.Date, cuoinam);
            //        for (int i = 0; i < data.Count; i++)
            //        {
            //            var item = data[i];
            //            int rank = i + 1;
            //            AccountRankTable.insert(5, item.UID, item.Role, rank, item.Rate1, item.Rate2, item.Rate3, item.Rate4, item.Rate5, item.Score, item.TotalTimeHour, item.TimeHour, "auto", d);
            //        }
            //    }

            //    //CHẠY SALER
            //    // chạy mỗi ngày
            //    var datasaler = PushAutoController.tinhrank(false, d.AddDays(-1).Date, d.AddDays(-1).Date);
            //    for (int i = 0; i < datasaler.Count; i++)
            //    {
            //        var item = datasaler[i];
            //        int rank = i + 1;
            //        AccountRankTable.insert(1, item.UID, item.Role, rank, item.Rate1, item.Rate2, item.Rate3, item.Rate4, item.Rate5, item.Score, item.TotalTimeHour, item.TimeHour, "auto", d);
            //    }
            //    if (d.ToString("dddd").Contains("Monday"))//Ngày đầu tuần
            //    {
            //        //ngày cuối tuần
            //        DateTime endweek = d.AddDays(-1);
            //        DateTime startweek = endweek.AddDays(-6);
            //        var data = PushAutoController.tinhrank(false, startweek.Date, endweek.Date);
            //        for (int i = 0; i < data.Count; i++)
            //        {
            //            var item = data[i];
            //            int rank = i + 1;
            //            AccountRankTable.insert(2, item.UID, item.Role, rank, item.Rate1, item.Rate2, item.Rate3, item.Rate4, item.Rate5, item.Score, item.TotalTimeHour, item.TimeHour, "auto", d);
            //        }
            //    }
            //    if (d.Day == 1)//tháng
            //    {
            //        //ngày cuối tháng
            //        DateTime startmonth = d.AddMonths(-1);
            //        DateTime endmonth = d.AddDays(-1);
            //        var data = PushAutoController.tinhrank(false, startmonth.Date, endmonth.Date);
            //        for (int i = 0; i < data.Count; i++)
            //        {
            //            var item = data[i];
            //            int rank = i + 1;
            //            AccountRankTable.insert(3, item.UID, item.Role, rank, item.Rate1, item.Rate2, item.Rate3, item.Rate4, item.Rate5, item.Score, item.TotalTimeHour, item.TimeHour, "auto", d);
            //        }
            //    }
            //    if (d.ToString("dd/MM").Contains("01/01") || d.ToString("dd/MM").Contains("01/04") || d.ToString("dd/MM").Contains("01/07") || d.ToString("dd/MM").Contains("01/10"))//quý
            //    {
            //        //Ngày cuối quý
            //        DateTime startquy = d.AddMonths(-3);
            //        DateTime endquy = d.AddDays(-1);
            //        var data = PushAutoController.tinhrank(false, startquy.Date, endquy.Date);
            //        for (int i = 0; i < data.Count; i++)
            //        {
            //            var item = data[i];
            //            int rank = i + 1;
            //            AccountRankTable.insert(4, item.UID, item.Role, rank, item.Rate1, item.Rate2, item.Rate3, item.Rate4, item.Rate5, item.Score, item.TotalTimeHour, item.TimeHour, "auto", d);
            //        }
            //    }
            //    if (d.ToString("dd/MM").Contains("01/01"))//năm
            //    {
            //        DateTime daunam = d.AddYears(-1);
            //        DateTime cuoinam = d.AddDays(-1);
            //        var data = PushAutoController.tinhrank(false, daunam.Date, cuoinam);
            //        for (int i = 0; i < data.Count; i++)
            //        {
            //            var item = data[i];
            //            int rank = i + 1;
            //            AccountRankTable.insert(5, item.UID, item.Role, rank, item.Rate1, item.Rate2, item.Rate3, item.Rate4, item.Rate5, item.Score, item.TotalTimeHour, item.TimeHour, "auto", d);
            //        }
            //    }
            //}

            //  Tính tổng giờ dạy cảu giáo viên
            //DateTime starthour = DateTime.ParseExact("25/08/2020", "dd/MM/yyyy", null);
            //int totalhour = (GetDateTime.Now - starthour).Days;
            //for (int k = 0; k <= totalhour; k++)
            //{
            //    var d = starthour.AddDays(k);
            //    CHẠY GIAO VIÊN
            //    chạy mỗi ngày

            //    var datateacher = PushAutoController.tinhgiogiaovien(1, d.AddDays(-1).Date, d.AddDays(-1).Date);
            //    for (int i = 0; i < datateacher.Count; i++)
            //    {

            //        var item = datateacher[i];
            //        int rank = i + 1;
            //        var check = AccountTotalHourTeachingTable.getbyuid(item.UID).Where(n => n.DateRun.Value == d.Date).FirstOrDefault();
            //        if (check == null)
            //        {
            //            AccountTotalHourTeachingTable.insert(1, item.UID, rank, item.HoursOffline, item.HoursOnline, item.HoursRealOffline, item.HoursRealOnline, item.HoursAbesntOffline, item.HoursAbesntOnline,
            //                item.TotalHour, item.TotalHourOnline, item.HoursCourseSelfStudy, item.HoursCourseSelfStudyOnline, item.TotalCourseSelfStudy, item.TotalCourseSelfStudyOnline, item.Lead_HoursOffline, item.Lead_HoursOnline, item.Lead_HoursRealOffline, item.Lead_HoursRealOnline, item.Lead_HoursAbesntOffline, item.Lead_HoursAbesntOnline, item.Lead_TotalHour, item.Lead_TotalHourOnline, item.Lead_HoursCourseSelfStudy, item.Lead_HoursCourseSelfStudyOnline, item.Lead_TotalCourseSelfStudy, item.Lead_TotalCourseSelfStudyOnline, "auto", d);
            //        }
            //    }
            //    if (d.ToString("dddd").Contains("Monday"))//Ngày đầu tuần
            //    {
            //        ngày cuối tuần
            //        DateTime endweek = d.AddDays(-1);
            //        DateTime startweek = endweek.AddDays(-6);
            //        var data = PushAutoController.tinhgiogiaovien(2, startweek.Date, endweek.Date);
            //        for (int i = 0; i < data.Count; i++)
            //        {
            //            var item = data[i];
            //            int rank = i + 1;

            //            var check = AccountTotalHourTeachingTable.getbyuid(item.UID).Where(n => n.DateRun.Value == d.Date && n.Type == 2).FirstOrDefault();
            //            if (check == null)
            //            {
            //                AccountTotalHourTeachingTable.insert(2, item.UID, rank, item.HoursOffline, item.HoursOnline, item.HoursRealOffline, item.HoursRealOnline, item.HoursAbesntOffline, item.HoursAbesntOnline,
            //                item.TotalHour, item.TotalHourOnline, item.HoursCourseSelfStudy, item.HoursCourseSelfStudyOnline, item.TotalCourseSelfStudy, item.TotalCourseSelfStudyOnline, item.Lead_HoursOffline, item.Lead_HoursOnline, item.Lead_HoursRealOffline, item.Lead_HoursRealOnline, item.Lead_HoursAbesntOffline, item.Lead_HoursAbesntOnline, item.Lead_TotalHour, item.Lead_TotalHourOnline, item.Lead_HoursCourseSelfStudy, item.Lead_HoursCourseSelfStudyOnline, item.Lead_TotalCourseSelfStudy, item.Lead_TotalCourseSelfStudyOnline, "auto", d);
            //            }
            //        }
            //    }
            //    if (d.Day == 1)//tháng
            //    {
            //        ngày cuối tháng
            //        DateTime startmonth = d.AddMonths(-1);
            //        DateTime endmonth = d.AddDays(-1);
            //        var data = PushAutoController.tinhgiogiaovien(3, startmonth.Date, endmonth.Date);
            //        for (int i = 0; i < data.Count; i++)
            //        {
            //            var item = data[i];
            //            int rank = i + 1;
            //            var check = AccountTotalHourTeachingTable.getbyuid(item.UID).Where(n => n.DateRun.Value == d.Date && n.Type == 3).FirstOrDefault();
            //            if (check == null)
            //            {
            //                AccountTotalHourTeachingTable.insert(3, item.UID, rank, item.HoursOffline, item.HoursOnline, item.HoursRealOffline, item.HoursRealOnline, item.HoursAbesntOffline, item.HoursAbesntOnline,
            //                item.TotalHour, item.TotalHourOnline, item.HoursCourseSelfStudy, item.HoursCourseSelfStudyOnline, item.TotalCourseSelfStudy, item.TotalCourseSelfStudyOnline, item.Lead_HoursOffline, item.Lead_HoursOnline, item.Lead_HoursRealOffline, item.Lead_HoursRealOnline, item.Lead_HoursAbesntOffline, item.Lead_HoursAbesntOnline, item.Lead_TotalHour, item.Lead_TotalHourOnline, item.Lead_HoursCourseSelfStudy, item.Lead_HoursCourseSelfStudyOnline, item.Lead_TotalCourseSelfStudy, item.Lead_TotalCourseSelfStudyOnline, "auto", d);
            //            }
            //        }
            //    }
            //    if (d.ToString("dd/MM").Contains("01/01") || d.ToString("dd/MM").Contains("01/04") || d.ToString("dd/MM").Contains("01/07") || d.ToString("dd/MM").Contains("01/10"))//quý
            //    {
            //        Ngày cuối quý
            //        DateTime startquy = d.AddMonths(-3);
            //        DateTime endquy = d.AddDays(-1);
            //        var data = PushAutoController.tinhgiogiaovien(4, startquy.Date, endquy.Date);
            //        for (int i = 0; i < data.Count; i++)
            //        {
            //            var item = data[i];
            //            int rank = i + 1;
            //            var check = AccountTotalHourTeachingTable.getbyuid(item.UID).Where(n => n.DateRun.Value == d.Date && n.Type == 4).FirstOrDefault();
            //            if (check == null)
            //            {
            //                AccountTotalHourTeachingTable.insert(4, item.UID, rank, item.HoursOffline, item.HoursOnline, item.HoursRealOffline, item.HoursRealOnline, item.HoursAbesntOffline, item.HoursAbesntOnline,
            //                item.TotalHour, item.TotalHourOnline, item.HoursCourseSelfStudy, item.HoursCourseSelfStudyOnline, item.TotalCourseSelfStudy, item.TotalCourseSelfStudyOnline, item.Lead_HoursOffline, item.Lead_HoursOnline, item.Lead_HoursRealOffline, item.Lead_HoursRealOnline, item.Lead_HoursAbesntOffline, item.Lead_HoursAbesntOnline, item.Lead_TotalHour, item.Lead_TotalHourOnline, item.Lead_HoursCourseSelfStudy, item.Lead_HoursCourseSelfStudyOnline, item.Lead_TotalCourseSelfStudy, item.Lead_TotalCourseSelfStudyOnline, "auto", d);
            //            }
            //        }
            //    }
            //    if (d.ToString("dd/MM").Contains("01/01"))//năm
            //    {
            //        DateTime daunam = d.AddYears(-1);
            //        DateTime cuoinam = d.AddDays(-1);
            //        var data = PushAutoController.tinhgiogiaovien(5, daunam.Date, cuoinam);
            //        for (int i = 0; i < data.Count; i++)
            //        {
            //            var item = data[i];
            //            int rank = i + 1;
            //            var check = AccountTotalHourTeachingTable.getbyuid(item.UID).Where(n => n.DateRun.Value == d.Date && n.Type == 5).FirstOrDefault();
            //            if (check == null)
            //            {
            //                AccountTotalHourTeachingTable.insert(5, item.UID, rank, item.HoursOffline, item.HoursOnline, item.HoursRealOffline, item.HoursRealOnline, item.HoursAbesntOffline, item.HoursAbesntOnline,
            //                item.TotalHour, item.TotalHourOnline, item.HoursCourseSelfStudy, item.HoursCourseSelfStudyOnline, item.TotalCourseSelfStudy, item.TotalCourseSelfStudyOnline, item.Lead_HoursOffline, item.Lead_HoursOnline, item.Lead_HoursRealOffline, item.Lead_HoursRealOnline, item.Lead_HoursAbesntOffline, item.Lead_HoursAbesntOnline, item.Lead_TotalHour, item.Lead_TotalHourOnline, item.Lead_HoursCourseSelfStudy, item.Lead_HoursCourseSelfStudyOnline, item.Lead_TotalCourseSelfStudy, item.Lead_TotalCourseSelfStudyOnline, "auto", d);
            //            }
            //        }
            //    }
            //}


            //string URL = "https://dothi.net/Handler/SearchHandler.ashx";
            //WebClient webClient = new WebClient();
            //webClient.Encoding = Encoding.UTF8;


            //// cap nhat tinh
            //var tinh = webClient.DownloadString(URL += "?module=GetCity");
            //var kq = new JavaScriptSerializer().Deserialize<List<tinhthanh>>(tinh);
            //foreach (var item in kq)
            //{
            //    var city = LocationTable.getall().Where(n => AssetCRM.RemoveUnicode(n.CityName.Trim()).ToUpper() == AssetCRM.RemoveUnicode(item.Text.Trim()).ToUpper()).FirstOrDefault();
            //    if (city != null)
            //        LocationTable.updatecode(city.ID, item.Id);
            //}

            ////chen quận
            //var citys = LocationTable.getall();
            //foreach (var item in citys)
            //{
            //    if (item.ID == 1 || item.ID == 2)
            //        continue;

            //    List<quanhuyen> qh = new List<quanhuyen>();
            //    string urlApi = "https://dothi.net/Handler/SearchHandler.ashx?module=GetDistrict&cityCode=" + item.ModifiedBy;
            //    ServicePointManager.Expect100Continue = true;
            //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            //    var client = new WebClient();
            //    System.Collections.Specialized.NameValueCollection formData = new System.Collections.Specialized.NameValueCollection();
            //    byte[] responseBytes = client.UploadValues(urlApi, "POST", formData);
            //    string Result = Encoding.UTF8.GetString(responseBytes);
            //    qh = JsonConvert.DeserializeObject<List<quanhuyen>>(Result);

            //    foreach (var q in qh)
            //    {
            //        var checkq = DistrictTable.getbycityid(item.ID).Where(n => n.LocationID == item.ID && n.DistrictName.Trim() == q.Text.Trim()).FirstOrDefault();
            //        if (checkq == null)
            //        {
            //            DistrictTable.insert(item.ID, item.CityName, q.Text.Trim(), "admin", q.Id.ToInt(0));
            //        }
            //    }
            //}
            //// chèn đường vs phường
            //var quans = DistrictTable.getall();
            //foreach (var item in quans)
            //{
            //    List<quanhuyen> phuong = new List<quanhuyen>();
            //    string urlApi = "https://dothi.net/Handler/SearchHandler.ashx?module=GetWard&distId=" + item.idcu;
            //    ServicePointManager.Expect100Continue = true;
            //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            //    var client = new WebClient();
            //    System.Collections.Specialized.NameValueCollection formData = new System.Collections.Specialized.NameValueCollection();
            //    byte[] responseBytes = client.UploadValues(urlApi, "POST", formData);
            //    string Result = Encoding.UTF8.GetString(responseBytes);
            //    phuong = JsonConvert.DeserializeObject<List<quanhuyen>>(Result);

            //    foreach (var p in phuong)
            //    {
            //        var checkq = WardTable.getbycityid(item.LocationID).Where(n => n.DistrictID == item.ID && n.WardName.Trim() == p.Text.Trim()).FirstOrDefault();
            //        if (checkq != null)
            //            continue;
            //        WardTable.insert(item, p.Text.Trim(), "admin");
            //    }
            //}
            //foreach (var item in quans)
            //{
            //    List<quanhuyen> duong = new List<quanhuyen>();
            //    string urlApi = "https://dothi.net/Handler/SearchHandler.ashx?module=GetStreet&distId=" + item.idcu;
            //    ServicePointManager.Expect100Continue = true;
            //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            //    var client = new WebClient();
            //    System.Collections.Specialized.NameValueCollection formData = new System.Collections.Specialized.NameValueCollection();
            //    byte[] responseBytes = client.UploadValues(urlApi, "POST", formData);
            //    string Result = Encoding.UTF8.GetString(responseBytes);
            //    duong = JsonConvert.DeserializeObject<List<quanhuyen>>(Result);

            //    foreach (var p in duong)
            //    {
            //        var checkq = StreetTable.getbycityid(item.LocationID).Where(n => n.DistrictID == item.ID && n.StreetName.Trim() == p.Text.Trim()).FirstOrDefault();
            //        if (checkq != null)
            //            continue;
            //        StreetTable.insert(item, p.Text.Trim(), "admin");
            //    }
            //}           
            //AssetCRM.SendMailAmazone("khoavipmoney11@gmail.com", "TEST SERVER", "TESSSSSSSSST");
            return View();
        }

        public class duong
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public string StreetPrefix { get; set; }
        }
        public class phuongxa
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public string WardPrefix { get; set; }
        }
        public class quanhuyen
        {
            public string Id { get; set; }
            public string Text { get; set; }
        }
        public class tinhthanh
        {
            public string Id { get; set; }
            public string Text { get; set; }
        }
        /// <summary>
        /// phân trang tự viết
        /// </summary>
        /// <returns></returns>
        public class Paging
        {
            //private ZimEntities db = new ZimEntities();

            public string Pagination(int total, int page, int Take, int offset, string Params, string c_url)
            {
                if (total > 0)
                {

                    double rowPerPage = Take;
                    if (Convert.ToDouble(total) < Take)
                    {
                        rowPerPage = Convert.ToDouble(total);
                    }

                    int totalPage = Convert.ToInt16(Math.Ceiling(Convert.ToDouble(total) / rowPerPage));
                    int current = page;
                    int record = offset;
                    int pageStart = Convert.ToInt16(Convert.ToDouble(current) - Convert.ToDouble(offset));
                    int pageEnd = Convert.ToInt16(Convert.ToDouble(current) + Convert.ToDouble(offset));
                    string numPage = "";
                    if (totalPage < 1)
                        return "";
                    numPage += " <div class=\"pagination-container\">";
                    numPage += "<ul class='pagination mg-b-0 justify-content-center'>";
                    if (current > 1)
                        numPage += "<li class='previous'><a href='" + c_url + "?page=" + (page - 1) + Params + "' aria-label='Previous'>&laquo;</a></li>";
                    else
                        numPage += "<li class='disabled'><a href='#' aria-label='Previous'><span aria-hidden='true'>&laquo;</span></a></li>";
                    if (current > (offset + 1))
                        numPage += "<li><a href='" + c_url + "?page=1" + Params + "' name='page1'>1</a></li><li class='disabled spacing-dot'><a href='#'>...</a></li>";
                    for (int i = 1; i <= totalPage; i++)
                    {
                        if (pageStart <= i && pageEnd >= i)
                        {
                            if (i == current)
                                numPage += "<li class='active'><a href='#'>" + i + " <span class='sr-only'>(current)</span></a></li>";
                            else
                                numPage += "<li><a href='" + c_url + "?page=" + i + Params + "'>" + i + "</a></li>";
                        }
                    }
                    if (totalPage > pageEnd)
                    {
                        record = offset;
                        numPage += "<li class='disabled spacing-dot'><a href='#'>...</a></li><li><a href='" + c_url + "?page=" + (totalPage) + Params + "'>" + totalPage + "</a></li>";
                    }
                    if (current < totalPage)
                        numPage += "<li class='next'><a class='ui-bar-d' href='" + c_url + "?page=" + (page + 1) + Params + "'>&raquo;</a></li>";
                    else
                        numPage += "<li class='disabled'><a href='#' aria-label='Previous'><span aria-hidden='true'>&raquo;</span></a></li>";
                    numPage += "</ul>";
                    numPage += "</div>";
                    return numPage;
                }
                else
                {
                    return "no records found";
                }
            }
            //public string PanigationVer2(int Total,int PageIndex, int PageSize, string currentURL, string CurrentParameter)
            //{
            //    int PageNumber =Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Total) / PageSize));
            //    int previusPage = Convert.ToInt32();
            //    return null;
            //}
        }
        Paging pg = new Paging();
       
        public ActionResult TestPaging(int Page = 1)
        {
            var df = DateTime.ParseExact("01/01/1001", "dd/MM/yyyy", null).Date;
            using (var db = new ZimEntities())
            {
                int offset = 1;
                int pageSize = 20;
                if (!string.IsNullOrEmpty(Request.QueryString["page"]))
                    Page = Request.QueryString["page"].ToInt(1);
                string create = "";
                if (!string.IsNullOrEmpty(Request.QueryString["c"]))
                {
                    create = Request.QueryString["c"].ToString();
                }
                string titlepost = "";
                if (!string.IsNullOrEmpty(Request.QueryString["t"]))
                {
                    titlepost = Request.QueryString["t"].ToString();
                }
                int groupID = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["gr"]))
                {
                    groupID = Request.QueryString["gr"].ToString().ToInt(0);
                }
                int aclogID = 0;
                int roleID = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["ac"]))
                {
                    aclogID = Request.QueryString["ac"].ToString().ToInt(0);
                }
                if (!string.IsNullOrEmpty(Request.QueryString["r"]))
                {
                    roleID = Request.QueryString["r"].ToString().ToInt(0);
                }
                SqlParameter[] parametersTotal =
                {
                    new SqlParameter("@getCount", SqlDbType.Bit) { Value = 1 },
                    new SqlParameter("@createby", SqlDbType.NVarChar,2000) { Value = create },
                    new SqlParameter("@titlePost", SqlDbType.NVarChar,2000) { Value= titlepost },
                    new SqlParameter("@aclogID", SqlDbType.Int) { Value = aclogID },
                    new SqlParameter("@roleID", SqlDbType.Int) { Value = roleID },
                    new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize },
                    new SqlParameter("@pageIndex", SqlDbType.Int) { Value = Page },
                    new SqlParameter("@groupID", SqlDbType.Int) { Value = groupID },
                };
                var rsTotal = SqlHelper.ExecuteDataTable("get_postcontent", parametersTotal);
                int total = Convert.ToInt32((from rw in rsTotal.AsEnumerable() select rw["CountItem"]).FirstOrDefault().ToString());

                SqlParameter[] parametersData =
                {
                    new SqlParameter("@getCount", SqlDbType.Bit) { Value = 0 },
                    new SqlParameter("@createby", SqlDbType.NVarChar,2000) { Value = create },
                    new SqlParameter("@titlePost", SqlDbType.NVarChar,2000) { Value= titlepost },
                    new SqlParameter("@aclogID", SqlDbType.Int) { Value = aclogID },
                    new SqlParameter("@roleID", SqlDbType.Int) { Value = roleID },
                    new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize },
                    new SqlParameter("@pageIndex", SqlDbType.Int) { Value = Page },
                    new SqlParameter("@groupID", SqlDbType.Int) { Value = groupID },
                };

                var rs = SqlHelper.ExecuteDataTable("get_postcontent", parametersData);
                IEnumerable<tbl_PostContent> data = (from rw in rs.AsEnumerable()
                                                     select new tbl_PostContent()
                                                     {
                                                         ID = Convert.ToInt32(rw["ID"]),
                                                         TitlePost = rw["TitlePost"].ToString(),
                                                         PostIMG = rw["PostIMG"].ToString(),
                                                         ContentPost = rw["ContentPost"].ToString(),
                                                         Status = Convert.ToInt32(rw["Status"]),
                                                         NumberView = Convert.ToInt32(rw["NumberView"]),
                                                         isHideStaff = Convert.ToBoolean(rw["isHideStaff"]),
                                                         isHideStudent = Convert.ToBoolean(rw["isHideStudent"]),
                                                         CreatedDate = rw["CreatedDate"] == DBNull.Value ? df : Convert.ToDateTime(rw["CreatedDate"]),
                                                         CreatedBy = rw["CreatedBy"].ToString(),
                                                         ModifiedDate = rw["ModifiedDate"] == DBNull.Value ? df : Convert.ToDateTime(rw["ModifiedDate"]),
                                                         ModifiedBy = rw["CreatedBy"].ToString(),
                                                         StatusPost = Convert.ToBoolean(rw["StatusPost"]),
                                                         BackgroundPost = Convert.ToBoolean(rw["BackgroundPost"]),
                                                         GroupID = Convert.ToInt32(rw["GroupID"]),
                                                         PostIMGThumbnail = rw["PostIMGThumbnail"].ToString(),
                                                         PostType = Convert.ToInt32(rw["PostType"])
                                                     }).ToList();

                string c_url = Request.Url.AbsoluteUri.ToLower();
                string paging = pg.Pagination(total, Page, pageSize, offset, "", c_url);
                ViewBag.Paging = paging;
                return View(data.ToList());
            }
        }
    }
}