using AppZim.api;
using AppZim.Areas.Admin.Controllers;
using AppZim.Models;
using AppZim.TableSql;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using MB.Extensions;
using Spire.Pdf.HtmlConverter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AppZim.Areas.api.ticket
{
    [Route("api/ticket/{action}", Name = "apiticket")]
    public class ticketController : ApiController
    {
        public string getToken()
        {
            try
            {
                IEnumerable<string> headerValues = Request.Headers.GetValues("token");
                return headerValues.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [HttpGet]
        public HttpResponseMessage getList(int type, int page)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                int pageSize = 10;
                List<API_ticket_getlist_Result> l = new List<API_ticket_getlist_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.API_ticket_getlist(aclog.ID, type, (page - 1) * pageSize, pageSize).ToList();
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { data = l, total = l.Count, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getCategories()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var listCategories = FeedBackCategoryTable.getall().Where(n => n.Role == aclog.RoleID || n.Role == 0).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { data = listCategories, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage create()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                int typeID = HttpContext.Current.Request.Form.Get("typeID").ToInt(0);
                string title = HttpContext.Current.Request.Form.Get("title");
                string contentFeedBack = HttpContext.Current.Request.Form.Get("contentFeedBack");

                var cate = FeedBackCategoryTable.getbyID(typeID);
                if (cate == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }

                tbl_Account sp = new tbl_Account();
                if (aclog.SupportUID != null)
                    sp = AccountTable.getbyID(aclog.SupportUID.Value);
                if (sp == null)
                {
                    sp = new tbl_Account();
                    sp.ID = 0;
                    sp.FullName = "";
                }

                var rs = FeedBackTable.insert(aclog.ID, aclog.FullName, sp.ID, sp.FullName, cate.ID, cate.CategoryFeedback, title, contentFeedBack, aclog.UserName);
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage replyTicket()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                int id = HttpContext.Current.Request.Form.Get("id").ToInt(0);
                string content = HttpContext.Current.Request.Form.Get("content");

                var fe = FeedBackTable.getbyid(id);
                if (fe == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                var rs = FeedBackDetailTable.insert(fe.ID, content, aclog.UserName);
                if (rs != null)
                {
                    string username = "";
                    if (aclog.UserName != fe.CreatedBy)
                        username = fe.CreatedBy;
                    else
                    {
                        if (fe.SupportID != 0)
                            username = AccountTable.getbyID(fe.SupportID.Value).UserName;
                    }
                    if (!string.IsNullOrEmpty(username))
                    {
                        var acpush = AccountTable.getbyusername(username);
                        //gửi thông báo cho thằng sp
                        var nt = NotificationTable.insertsingle("", "Feedback có nội dung mới", fe.Title, acpush.ID, aclog.UserName, 8, fe.ID);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage updateRate()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                int id = HttpContext.Current.Request.Form.Get("id").ToInt(0);
                int rate = HttpContext.Current.Request.Form.Get("rate").ToInt(0);

                var fe = FeedBackTable.getbyid(id);
                if (fe == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

                if (aclog.UserName != fe.CreatedBy)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

                if (fe.Rate == null)
                {
                    var rs = FeedBackTable.updaterate(fe.ID, rate, aclog.UserName);
                    if (rs != null)
                    {
                        if (fe.SupportID != 0)
                        {
                            //gửi thông báo cho thằng sp
                            var nt = NotificationTable.insertsingle("", "Feedback bạn xử lý được chấm điểm", fe.Title, fe.SupportID.Value, aclog.UserName, 8, fe.ID);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                    }
                    else
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpGet]
        public HttpResponseMessage UpdateLesson(int curriculumID)
        {
            try
            {
                var list = CurriculumDetailTable.getbycurriculumid(curriculumID);
                int index = 1;
                foreach (var i in list)
                {
                    CurriculumDetailTable.updateLesson(i.ID, index);
                    index += 1;
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpGet]
        public HttpResponseMessage UpdateExercise(int ClassID)
        {
            try
            {
                var listCurriculumn = CurriculumTable.getbyclass(ClassID);
                foreach (var i in listCurriculumn)
                {
                    var exList = ExerciseGroupTable.getbycurriculumid(i.ID);
                    Parallel.ForEach(exList, e =>
                    {
                        var detail = CurriculumDetailTable.getbycurriculumid(i.ID).FirstOrDefault();
                        ExerciseGroupTable.updateExercise(e.ID, detail.ID);
                    });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage PrintPDF()
        {
            try
            {
                var cc = ContractCustomerTable.getbyid(1);
                var fileName = Guid.NewGuid().ToString() + ".pdf";

                Spire.Pdf.HtmlConverter.Qt.HtmlConverter.Convert(cc.ContractContent,
                        fileName,
                        //enable javascript
                        true,
                        //load timeout
                        10 * 1000,
                        //page size
                        new SizeF(612, 792),
                        //page margins
                        new Spire.Pdf.Graphics.PdfMargins(0),
                        //load from content type
                        LoadHtmlType.SourceCode
                        );
                var doc = System.Diagnostics.Process.Start(fileName).StandardOutput;

                var httpContext = HttpContext.Current;
                var path = Path.Combine(httpContext.Server.MapPath("~/Upload/useravatar/"), fileName);
                string strPathAndQuery = httpContext.Request.Url.PathAndQuery;
                string strUrl = httpContext.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                var link = strUrl + "Upload/useravatar/" + fileName;
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success, link });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage SendMail()
        {
            var cc = ContractCustomerTable.getbyid(1);
            string title = BaseController.ProjectName + " gửi bạn hợp đồng cam kết chất lượng";
            string content = cc.ContractContent;
            Document pdf = new Document(PageSize.A4, 60f, 0f, 30f, 30f);
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    PdfWriter writer = PdfWriter.GetInstance(pdf, stream);
                    pdf.Open();

                    MemoryStream msHtml = new MemoryStream(Encoding.UTF8.GetBytes(content));
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdf, msHtml, null, Encoding.UTF8, new UnicodeFontFactory());

                    //XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdf, sr);
                    pdf.Close();

                    byte[] bytes = stream.ToArray();
                    stream.Close();

                    SmtpClient client = new SmtpClient();
                    client.Port = 587;//outgoing port for the mail.
                    client.Host = "smtp.gmail.com";
                    client.EnableSsl = true;
                    client.Timeout = 100000;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("nguyenphihung21098@gmail.com", "jiyeon761993");

                    var send_mail = new MailMessage();
                    send_mail.IsBodyHtml = true;
                    //address from where mail will be sent.
                    send_mail.From = new MailAddress("nguyenphihung21098@gmail.com");
                    //address to which mail will be sent.           
                    send_mail.To.Add(new MailAddress("nguyenphihung21098@gmail.com"));
                    //subject of the mail.
                    send_mail.Subject = title;
                    send_mail.Body = "";
                    send_mail.Attachments.Add(new Attachment(new MemoryStream(bytes), "hop_dong.pdf"));
                    client.Send(send_mail);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
        }
        [HttpPost]
        public HttpResponseMessage LoadGiaoVien(int curriculumnid, string studyid, string date, string day)
        {
            try
            {
                var curriculumn = CurriculumTable.getbyID(curriculumnid);
                Task<List<Admin_GetTeacherByClass_Result>> t1 = new Task<List<Admin_GetTeacherByClass_Result>>(() => GetTeacherByClass(curriculumn.ClassID.Value));
                Task<List<DateTime>> t2 = new Task<List<DateTime>>(() => GetValidDate(date, day));
                t1.Start();
                t2.Start();
                Task.WaitAll(t1, t2);
                int studyTimeID = Convert.ToInt32(studyid);
                var teacherList = t1.Result;
                var dates = t2.Result;
                Parallel.ForEach(teacherList, i =>
                {
                    int dem = 0;
                    foreach (var d in dates)
                    {
                        var _db = new ZimEntities();
                        var study = _db.tbl_StudyTime.Where(x => x.ID == studyTimeID).FirstOrDefault();
                        if (study == null)
                            continue;

                        string[] stime = study.sTime.Split(':');
                        TimeSpan sth = new TimeSpan(stime[0].ToInt(), stime[1].ToInt(0), 0);
                        DateTime st = GetDateTime.Now.Date.Add(sth);

                        string[] etime = study.eTime.Split(':');
                        TimeSpan eth = new TimeSpan(etime[0].ToInt(), etime[1].ToInt(0), 0);
                        DateTime et = GetDateTime.Now.Date.Add(eth);

                        var l = _db.tbl_CoursesSchedule.Where(n => n.TeacherID == i.UID && n.Date == d && n.IsHide != true).ToList();
                        foreach (var item in l)
                        {
                            var stu = _db.tbl_StudyTime.Where(x => x.ID == item.StudyTimeID.Value).FirstOrDefault();
                            if (stu == null)
                                continue;

                            string[] istime = stu.sTime.Split(':');
                            TimeSpan isth = new TimeSpan(istime[0].ToInt(), istime[1].ToInt(0), 0);
                            DateTime ist = GetDateTime.Now.Date.Add(isth);
                            string[] ietime = stu.eTime.Split(':');
                            TimeSpan ieth = new TimeSpan(ietime[0].ToInt(), ietime[1].ToInt(0), 0);
                            DateTime iet = GetDateTime.Now.Date.Add(ieth);

                            if (st == ist || et == iet)// thời gian bắt đầu, kết thúc cùng nhau => ko được
                            {
                                continue;
                            }

                            if ((st <= ist && ist < et) || (st < iet && iet <= et))// thời gian bắt đầu, kết thúc nằm trong ca khác => ko được
                            {
                                continue;
                            }

                            if ((st < ist && iet < et) || (st > ist && iet > et))// ca bị bao hoặc bao 1 ca khác => ko được
                            {
                                continue;
                            }
                        }

                        //check ca, ngày nghỉ
                        var off = _db.tbl_DayOffTeacher.Where(n => n.UID == i.UID && n.DayOff == d && n.isHide != true && n.StudyTimeID == studyTimeID).FirstOrDefault();
                        if (off != null)
                        {
                            continue;
                        }
                        dem += 1;
                    }
                    if (dem < curriculumn.TotalLesson)
                        teacherList.Remove(i);
                });
                StringBuilder html = new StringBuilder();
                html.Append("<option value=\"0\">---</option>");
                foreach (var item in teacherList)
                {
                    html.Append("<option value=\"" + item.UID + "\">" + item.FullName + "</option>");
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
        }
        public List<Admin_GetTeacherByClass_Result> GetTeacherByClass(int classid)
        {
            var teacherList = new List<Admin_GetTeacherByClass_Result>();
            using (var db = new ZimEntities())
            {
                teacherList = db.Admin_GetTeacherByClass(classid).ToList();
            }
            return teacherList;
        }
        public List<DateTime> GetValidDate(string date, string days)
        {
            var now = DateTime.Now;
            int[] day = days.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            ConcurrentBag<DateTime> dates = new ConcurrentBag<DateTime>();
            Parallel.For(0, 360,
                     index =>
                     {
                         if (Array.IndexOf(day, Convert.ToInt32(now.AddDays(index).DayOfWeek)) != -1)
                             dates.Add(now.AddDays(index));
                     });
            return dates.OrderBy(x => x).ToList();
        }
        [HttpGet]
        public HttpResponseMessage Statistical( DateTime starttime, DateTime endtime)
        {
            try
            {
                var _db = new ZimEntities();
                var courseList = _db.tbl_Courses.Where(x => (x.OpeningDate == starttime || x.EndingDate == endtime)
                || (starttime <= x.OpeningDate && x.OpeningDate < endtime) || (starttime < x.EndingDate && x.EndingDate <= endtime)
                || (starttime < x.OpeningDate && x.EndingDate < endtime) || (starttime > x.OpeningDate && x.EndingDate > endtime)).ToList();

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
        }
    }
    public class UnicodeFontFactory : FontFactoryImp
    {

        private static readonly string KaiuPath = Path.Combine(HttpContext.Current.Server.MapPath("~/fonts/"),
            "NotoSerif-Regular.TTF");//標楷體


        public override iTextSharp.text.Font GetFont(string fontname, string encoding, bool embedded, float size, int style, BaseColor color,
            bool cached)
        {
            BaseFont baseFont = BaseFont.CreateFont(KaiuPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            return new iTextSharp.text.Font(baseFont, size, style, color);
        }
    }
    public class StatisticalModel
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public int TeacherID { get; set; }
        public string TeacherName { get; set; }
    }
}