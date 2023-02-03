using AppZim.TableSql;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppZim.Models;
using System.Text;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AppZim.ZIM;
using static AppZim.ZIM.Zoom;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace AppZim.Areas.Admin.Controllers
{
    public class ZoomMeetingController : BaseController
    {
        // GET: Admin/ZoomMeeting
        public ActionResult TeacherConfigZoom(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// chỉ admin mới được xem
                return RedirectToAction("Signin", "Login", new { area = "" });
            int pageSize = 20;
            string search = "";
            ViewBag.Search = Request.QueryString["search"];
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                search = Request.QueryString["search"].ToString().Trim();

            List<VietCoCo_configTeacherZoom_Result> l = new List<VietCoCo_configTeacherZoom_Result>();
            using (var db = new ZimEntities())
            {
                l = db.VietCoCo_configTeacherZoom(search, 0, page - 1, pageSize, aclog.ID, aclog.RoleID).ToList();
            }

            ViewBag.StartIndex = (page - 1) * pageSize;
            int total = 0;
            if (l.Count() > 0)
            {
                total = l[0].TotalRow.Value;
            }
            ViewBag.Total = total;
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            return View(l);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddUpdateConfig(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int id = f.Get("hdfID").ToInt();
            int typePage = f.Get("txt-page").ToInt(0);
            int teacherID = f.Get("txt-uid").ToInt(0);
            string userID = f.Get("txt-userID").ToString();
            string key = f.Get("txt-key").ToString();
            string secrect = f.Get("txt-secrect").ToString();
            string token = "";
            if (!string.IsNullOrEmpty(f.Get("txt-token")))
            {
                token = f.Get("txt-token").ToString();
            }

            tbl_ZoomTeacherAPI rs = null;
            if (id == 0)
            {
                tbl_ZoomTeacherAPI ck = ZoomTeacherAPITable.GetbyTeacherID(teacherID);
                if (ck == null)
                {
                    rs = ZoomTeacherAPITable.Insert(teacherID, userID, key, secrect, token, aclog.UserName);
                }
                else
                {
                    rs = ck;
                }
            }
            else
            {
                rs = ZoomTeacherAPITable.Update(id, userID, key, secrect, token, aclog.UserName);
            }
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        public ActionResult CreateRoom(int CoursesScheduleID)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var returns = Redirect(Request.UrlReferrer.AbsoluteUri);
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var c = CoursesScheduleTable.getbyid(CoursesScheduleID);
                if (c == null)
                {
                    SetAlert("Không tìm thấy lịch học!", "e");
                    return returns;
                }
                var a = ZoomTeacherAPITable.GetbyTeacherID(aclog.ID);
                if (a == null)
                {
                    SetAlert("Vui lòng cấu hình API Zoom trước!", "e");
                    return returns;
                }

                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var now = DateTime.UtcNow;
                var apiSecret = a.APISecrect;
                byte[] symmetricKey = Encoding.ASCII.GetBytes(apiSecret);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = a.APIKey,
                    Expires = now.AddSeconds(300),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256),
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                string userId = a.UserID;
                var client = new RestClient($"https://api.zoom.us/v2/users/{userId}/meetings");
                var request = new RestRequest(Method.POST);
                request.RequestFormat = DataFormat.Json;
                //request.AddJsonBody(new { topic = CoursesTable.getbyid(c.CourseID.Value).CourseName, duration = "10", start_time = "2021-04-20T05:00:00", type = "2" });
                request.AddJsonBody(new { topic = CoursesTable.getbyid(c.CourseID.Value).CourseName, duration = "200", start_time = c.Date.Value.Date.ToString("yyyy-MM-dd") + "T00:00:00", type = "2" });

                request.AddHeader("authorization", String.Format("Bearer {0}", tokenString));
                IRestResponse restResponse = client.Execute(request);
                HttpStatusCode statusCode = restResponse.StatusCode;
                int numericStatusCode = (int)statusCode;
                var jObject = JObject.Parse(restResponse.Content);

                if (numericStatusCode == 404)
                {
                    if (jObject["code"].ToString() == "1001")
                    {
                        SetAlert("Tài khoản tạo phòng chưa đúng!", "e");
                        return returns;
                    }
                    else if (jObject["code"].ToString() == "124")
                    {
                        SetAlert("Lỗi xác thực, Vui lòng kiểm tra lại API Key và Secrect!", "e");
                        return returns;
                    }
                    else
                    {
                        SetAlert("Đã xảy ra lỗi, vui lòng liên hệ admin!", "e");
                        return returns;
                    }
                }
                else if (numericStatusCode == 201)
                {
                    CoursesScheduleTable.UpdateRoom(c.ID, jObject["id"].ToString(), jObject["encrypted_password"].ToString(), jObject["start_url"].ToString());
                }
                SetAlert("Tạo phòng thành công!", "s");
                return returns;
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ZoomMeetingController", "CreateRoom", Session["UID"].ToString().ToInt(0), ex.Message);
                return returns;
            }
        }

        /// <summary>
        /// Check Join Meeting
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckJoinRoom(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var c = CoursesScheduleTable.getbyid(id);
            if (c == null)
                return Json(new { rs = false, m = "Không tìm thấy lịch học" }, JsonRequestBehavior.AllowGet);

            if (string.IsNullOrEmpty(c.RoomID) || string.IsNullOrEmpty(c.RoomPass))
                return Json(new { rs = false, m = "Lỗi phòng học, vui lòng liên hệ admin!" }, JsonRequestBehavior.AllowGet);

            if (aclog.RoleID == 5)
            {
                if (c.isRoomStart == false)
                    return Json(new { rs = false, m = "Phòng học chưa bắt đầu!" }, JsonRequestBehavior.AllowGet);
                return Json(new { rs = true, url = "/Admin/ZoomMeeting/JoinRoom?id=" + c.ID + "" }, JsonRequestBehavior.AllowGet);
            }
            var schedule = CoursesScheduleTable.UpdateisStartRoom(c.ID, true);
            return Json(new { rs = true, url = schedule.ZoomStartURL }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult JoinRoom(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var c = CoursesScheduleTable.getbyid(id);
            var z = ZoomTeacherAPITable.GetbyTeacherID(c.TeacherID.Value);
            if (z == null)
            {
                SetAlert("Vui lòng cấu hình Zoom trước!", "e");
                return RedirectToAction("ScheduleCourse", "CourseDetail", new { id = c.ID });
            }
            Room r = new Room();
            r.ID = c.ID;
            r.RoomID = c.RoomID;
            r.RoomPass = c.RoomPass;
            if (aclog.RoleID.Value == 5)
            {
                r.Role = 0; //học viên
            }
            else if (aclog.RoleID.Value == 4 || aclog.RoleID.Value == 1)
            {
                r.Role = 1; //admin, gv =>host
            }
            else
            {
                r.Role = 5; //người ngoài
            }
            r.Key = z.APIKey;
            r.Secrect = z.APISecrect;
            r.FullName = aclog.FullName;
            ViewBag.Data = r; //trả data qua view => bên view auto click redirect


            //auto điểm danh
            if (aclog.RoleID == 5)
            {
                //Phần dùng để kiểm tra HV vô phòng sớm hay muộn
                DateTime sMeeting = DateTime.Now;  //thời gian nó bấm bắt đầu meeting
                DateTime eMeeting = DateTime.Now;

                var sty = StudyTimeTable.getbyid(c.StudyTimeID.Value);
                string[] stu = sty.sTime.Split(':');
                TimeSpan s = new TimeSpan(int.Parse(stu[0]), int.Parse(stu[1]), 0);
                DateTime sTimeStudy = sMeeting.Date.Add(s);//thời gian bắt đầu ca học

                string[] etu = sty.eTime.Split(':');
                TimeSpan e = new TimeSpan(int.Parse(etu[0]), int.Parse(etu[1]), 0);
                DateTime eTimeStudy = eMeeting.Date.Add(e);//thời gian kết thúc ca học
                //end

                DateTime d = DateTime.Now.Date;
                string note = "không";
                int attid = 1;
                int learningid = 8;
                bool warning = false;
                if (sMeeting.Subtract(sTimeStudy).Minutes < -15) // => nó vô trễ
                {
                    attid = 4;
                }
                string attname = OptionDiemDanh(attid);
                string learingname = OptionNhanXet(learningid);

                var tea = AccountTable.getbyID(z.TeacherID.Value);
                tbl_CoursesAttendanceStudent diemdanh = null;
                var ck = CoursesAttendanceStudentTable.checkadd(c.CourseID.Value, aclog.ID, d.Date, c.StudyTimeID.Value);
                var l = CoursesScheduleTable.getbycourseidbydatevsstudyid(c.CourseID.Value, d.Date, c.StudyTimeID.Value);
                if (l == null)
                {
                    SetAlert("Lỗi tự động điểm danh!", "e");
                }
                else
                {
                    if (ck == null) //kiểm tra HV điểm danh chưa
                    {
                        var cou = CoursesTable.getbyid(c.CourseID.Value);
                        var school = SchoolTable.getbyID(cou.SchoolID.Value);
                        tbl_Account lead = new tbl_Account();
                        lead.ID = 0;
                        lead.FullName = "";
                        if (cou.TeacherLeaderUID != null)
                        {
                            lead.ID = cou.TeacherLeaderUID.Value;
                            lead.FullName = cou.TeacherLeaderName;
                        }
                        diemdanh = CoursesAttendanceStudentTable.insert(school, cou, aclog, c.StudyTimeID.Value, d.Date, attid, attname, learningid, learingname, note, tea.UserName, warning, tea.FullName, lead);
                    }
                    else
                    {
                        diemdanh = ck;
                    }
                }

                //update thời gian bắt đầu meeting của HV để làm thống kê
                if (string.IsNullOrEmpty(diemdanh.TotalJoinMeeting.ToString())) //nó chưa bấm start meeting lần nào
                {
                    if (DateTime.Compare(sMeeting, sTimeStudy) == -1) //nếu sMeeting < sTimeStudy sẽ trả về -1 => update lại sMeeting = sTimeStudy
                    {
                        sMeeting = sTimeStudy;
                    }
                    //nếu sMeeting > eTimeStudy sẽ trả về -1 => update mới update sMeeting
                    if (DateTime.Compare(sMeeting, eTimeStudy) == -1) //nếu lúc HV bấm vẫn nhỏ hơn datetime kết thúc ca học thì update
                    {
                        CoursesAttendanceStudentTable.UpdateTimeStartMeeting(diemdanh.ID, sMeeting);
                    }
                }
            }
            //end auto điểm danh
            return View();
        }

        public ActionResult LoadMeeting(int CoursesScheduleID)
        {
            ViewBag.CoursesScheduleID = CoursesScheduleID;
            return View();
        }

        /// <summary>
        /// Hướng dẫn cấu hình API Zoom
        /// </summary>
        /// <returns></returns>
        public ActionResult RegistrationZoom()
        {
            return View();
        }
        /// <summary>
        /// Kết thúc meeting thì làm gì....
        /// </summary>
        /// <returns></returns>
        public ActionResult AfterMeeting(int CoursesScheduleID)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var cs = CoursesScheduleTable.getbyid(CoursesScheduleID);
            if (aclog.RoleID == 5)
            {
                DateTime eMeeting = DateTime.Now; //thời gian nó bấm kết thúc meeting
                var sty = StudyTimeTable.getbyid(cs.StudyTimeID.Value);
                string[] etu = sty.eTime.Split(':');
                TimeSpan e = new TimeSpan(int.Parse(etu[0]), int.Parse(etu[1]), 0); //thời gian kết thúc ca học
                DateTime eTimeStudy = eMeeting.Date.Add(e);

                if (DateTime.Compare(eMeeting, eTimeStudy) == 1) //nếu thời gian HV bấm out lớn thời gian kết thúc ca học => update lại
                {
                    eMeeting = eTimeStudy;
                }

                var ck = CoursesAttendanceStudentTable.checkadd(cs.CourseID.Value, aclog.ID, eMeeting.Date, cs.StudyTimeID.Value);
                if (ck != null)
                {
                    if (string.IsNullOrEmpty(ck.TotalJoinMeeting.ToString())) //nó chưa bấm out meeting lần nào
                    {
                        CoursesAttendanceStudentTable.UpdateTimeEndMeeting(ck.ID, eMeeting, 1);
                    }
                    else
                    {
                        CoursesAttendanceStudentTable.UpdateTimeEndMeeting(ck.ID, eMeeting, ck.TotalJoinMeeting.Value + 1);
                    }


                    if (DateTime.Compare(eMeeting, eTimeStudy) == -1) //nếu thời gian HV bấm out nhỏ thời gian kết thúc ca học => nó về sớm => khô máu nó
                    {
                        string attname = OptionDiemDanh(5);
                        CoursesAttendanceStudentTable.UpdateDiemDanh(ck.ID, 5, attname);
                    }
                }
            }
            else if (aclog.RoleID == 4)
            {
                CoursesScheduleTable.UpdateisStartRoom(cs.ID, false); //đóng room lại => redirect gv tới trang điểm danh
                return RedirectToAction("AttendanceCourse", "CourseDetail", new { id = cs.CourseID.Value });
            }
            else
            {
                return RedirectToAction("CourseList", "Course", new { area = "Admin" }); //về trang quản lý khóa học
            }
            return RedirectToAction("FeedbackSchedule", "CourseDetail", new { area = "Admin", slCourse = cs.CourseID }); //về trang đánh giá của học viên
        }

        public ActionResult ListRecording(int page = 1, int idTeacherConfig = 0)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });


            string search = "";
            ViewBag.Search = Request.QueryString["search"];
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                search = Request.QueryString["search"].ToString().Trim();

            DateTime date = DateTime.Now.Date;
            DateTime firstDayOfMonth = new DateTime(date.Year, date.Month, 1).Date; //ngày đầu của tháng
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1).Date; //ngày cuối của tháng

            string fromdate = firstDayOfMonth.ToString("yyyy-MM-dd");
            string todate = lastDayOfMonth.ToString("yyyy-MM-dd");

            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                DateTime f = DateTime.ParseExact(Request.QueryString["from"], "dd/MM/yyyy", null);
                fromdate = f.ToString("yyyy-MM-dd");
                firstDayOfMonth = f;
            }
            ViewBag.From = firstDayOfMonth.ToString("dd/MM/yyyy");
            if (!string.IsNullOrEmpty(Request.QueryString["to"]))
            {
                DateTime t = DateTime.ParseExact(Request.QueryString["to"], "dd/MM/yyyy", null);
                todate = t.ToString("yyyy-MM-dd");
                lastDayOfMonth = t;
            }
            ViewBag.To = lastDayOfMonth.ToString("dd/MM/yyyy");

            if (!string.IsNullOrEmpty(Request.QueryString["idTeacherConfig"]))
            {
                idTeacherConfig = Request.QueryString["idTeacherConfig"].ToInt(0);
            }
            var ck = ZoomTeacherAPITable.GetbyTeacherID(idTeacherConfig);
            if (ck == null)
            {
                SetAlert("Không thành công!", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }


            int page_size = int.MaxValue;
            string userId = ck.UserID;
            string tokenString = "";
            if (!string.IsNullOrEmpty(ck.Token))
            {
                tokenString = ck.Token;
            }
            else
            {
                tokenString = AccessToken(ck.APISecrect, ck.APIKey);
            }
            var client = new RestClient($"https://api.zoom.us/v2/users/{userId}/recordings?trash_type=meeting_recordings&to={todate}&from={fromdate}&mc=false&page_size={page_size}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", String.Format("Bearer {0}", tokenString));
            IRestResponse response = client.Execute(request);
            HttpStatusCode statusCode = response.StatusCode;
            int numericStatusCode = (int)statusCode;

            //chỗ này hứng data nè
            List<RecordingMeeting> rMeeting = new List<RecordingMeeting>();
            if (numericStatusCode == 200)
            {
                var jObject = JObject.Parse(response.Content);
                if (jObject != null)
                {
                    rMeeting = JsonConvert.DeserializeObject<List<RecordingMeeting>>(jObject["meetings"].ToString());
                    if (rMeeting.Count() > 0 && !string.IsNullOrEmpty(search))
                    {
                        rMeeting = rMeeting.Where(x => AssetCRM.RemoveUnicode(x.topic).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())).ToList();
                    }
                }
            }
            ViewBag.idTeacherConfig = idTeacherConfig;
            return View(rMeeting);
        }

        public ActionResult DetailRecording(string MeetingId, int idTeacherConfig = 0)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 5)// chỉ admin,gv mới được xem
                return RedirectToAction("Signin", "Login", new { area = "" });
            var ck = ZoomTeacherAPITable.GetbyTeacherID(idTeacherConfig);
            if (ck == null)
            {
                SetAlert("Không thành công!", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }

            //string userId = "demonhunterg@gmail.com";
            string userId = ck.UserID;
            //string tokenString = AccessToken("NrzjDgDWnK73QEzXUspdOrspz6ulDeFVnsGA", "DgIpOwp2QaiFerer-X9tYg");
            string tokenString = "";
            if (!string.IsNullOrEmpty(ck.Token))
            {
                tokenString = ck.Token;
            }
            else
            {
                tokenString = AccessToken(ck.APISecrect, ck.APIKey);
            }
            //string tokenString = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJhdWQiOm51bGwsImlzcyI6IkRnSXBPd3AyUWFpRmVyZXItWDl0WWciLCJleHAiOjE3NDcyNDUwMDAsImlhdCI6MTYyMDQxMDA1NH0.Y2P7FZfZFbYWLMC7tzhUTwM0gKBm8nxj3EvBvGpVaoQ";
            var client = new RestClient($"https://api.zoom.us/v2/meetings/{MeetingId}/recordings");
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", String.Format("Bearer {0}", tokenString));
            IRestResponse response = client.Execute(request);
            HttpStatusCode statusCode = response.StatusCode;
            int numericStatusCode = (int)statusCode;

            //chỗ này hứng data nè
            List<RecordingFiles> rFile = new List<RecordingFiles>();
            if (numericStatusCode == 200)
            {
                var jObject = JObject.Parse(response.Content);
                if (jObject != null)
                {
                    rFile = JsonConvert.DeserializeObject<List<RecordingFiles>>(jObject["recording_files"].ToString());
                    ViewBag.Topic = jObject["topic"].ToString();
                }
            }

            ViewBag.idTeacherConfig = idTeacherConfig;
            return View(rFile);
        }

        /// <summary>
        /// Xóa Meeting Recording
        /// </summary>
        /// <param name="MeetingId"></param>
        /// <param name="idTeacherConfig"></param>
        /// <returns></returns>
        public ActionResult DeleteMeetingRecording(string MeetingId, int idTeacherConfig = 0)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)// chỉ admin mới được xóa
                return RedirectToAction("Signin", "Login", new { area = "" });
            var ck = ZoomTeacherAPITable.GetbyTeacherID(idTeacherConfig);
            if (ck == null)
            {
                SetAlert("Không thành công!", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }

            string userId = ck.UserID;
            string tokenString = "";
            if (!string.IsNullOrEmpty(ck.Token))
            {
                tokenString = ck.Token;
            }
            else
            {
                tokenString = AccessToken(ck.APISecrect, ck.APIKey);
            }
            var client = new RestClient($"https://api.zoom.us/v2/meetings/{MeetingId}/recordings?action=trash");
            var request = new RestRequest(Method.DELETE);
            request.AddHeader("authorization", String.Format("Bearer {0}", tokenString));
            IRestResponse response = client.Execute(request);
            HttpStatusCode statusCode = response.StatusCode;
            int numericStatusCode = (int)statusCode;

            switch (numericStatusCode)
            {
                case 204:
                    SetAlert("Thành công!", "e");
                    break;
                case 400:
                    SetAlert("Người dùng không thuộc tài khoản này!", "e");
                    break;
                case 404:
                    SetAlert("Không tìm thấy Record!", "e");
                    break;
                default:
                    SetAlert("Bạn không có quyền phù hợp!", "e");
                    break;
            }
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        public ActionResult DeleteMeetingRecordingFile(string meeting_id, string recordingId, int idTeacherConfig = 0)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)// chỉ admin mới được xóa
                return RedirectToAction("Signin", "Login", new { area = "" });
            var ck = ZoomTeacherAPITable.GetbyTeacherID(idTeacherConfig);
            if (ck == null)
            {
                SetAlert("Không thành công!", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }

            string userId = ck.UserID;
            string tokenString = "";
            if (!string.IsNullOrEmpty(ck.Token))
            {
                tokenString = ck.Token;
            }
            else
            {
                tokenString = AccessToken(ck.APISecrect, ck.APIKey);
            }
            var client = new RestClient($"https://api.zoom.us/v2/meetings/{meeting_id}/recordings/{recordingId}?action=trash");
            var request = new RestRequest(Method.DELETE);
            request.AddHeader("authorization", String.Format("Bearer {0}", tokenString));
            IRestResponse response = client.Execute(request);
            //HttpStatusCode statusCode = response.StatusCode;
            //int numericStatusCode = (int)statusCode;

            //switch (numericStatusCode)
            //{
            //    case 204:
            //        SetAlert("Thành công!", "e");
            //        break;
            //    case 400:
            //        SetAlert("Người dùng không thuộc tài khoản này!", "e");
            //        break;
            //    case 404:
            //        SetAlert("Không tìm thấy Record File!", "e");
            //        break;
            //    default:
            //        SetAlert("Bạn không có quyền phù hợp!", "e");
            //        break;
            //}

            SetAlert("Thành công!", "s");
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }



        /// <summary>
        /// Danh sách tất cả meeting đang diễn ra của tất cả GV
        /// </summary>
        /// <returns></returns>
        public ActionResult ListMeeting(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)// chỉ admin mới được xem
                return RedirectToAction("Signin", "Login", new { area = "" });


            string search = "";
            ViewBag.Search = Request.QueryString["search"];
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                search = Request.QueryString["search"].ToString().Trim();


            List<Meeting> meeting = new List<Meeting>();
            //lấy danh sách giáo viên có cấu hình zoom
            List<tbl_ZoomTeacherAPI> list = ZoomTeacherAPITable.GetAll();
            if (list.Count() != 0)
            {
                foreach (var i in list)
                {
                    int page_size = int.MaxValue;
                    string userId = i.UserID;
                    string tokenString = "";
                    if (!string.IsNullOrEmpty(i.Token))
                    {
                        tokenString = i.Token;
                    }
                    else
                    {
                        tokenString = AccessToken(i.APISecrect, i.APIKey);
                    }

                    var client = new RestClient($"https://api.zoom.us/v2/users/{userId}/meetings?page_size={page_size}&type=live");
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("authorization", String.Format("Bearer {0}", tokenString));
                    IRestResponse response = client.Execute(request);
                    HttpStatusCode statusCode = response.StatusCode;
                    int numericStatusCode = (int)statusCode;

                    //chỗ này hứng data nè
                    if (numericStatusCode == 200)
                    {
                        var jObject = JObject.Parse(response.Content);
                        if (jObject != null)
                        {
                            List<Meeting> m = JsonConvert.DeserializeObject<List<Meeting>>(jObject["meetings"].ToString());
                            if (m.Count() != 0)
                            {
                                var tc = AccountTable.getbyID(i.TeacherID.Value).FullName;
                                foreach (var j in m)
                                {
                                    Meeting m2 = new Meeting();
                                    m2.id = j.id;
                                    string[] array = j.join_url.Split('='); //lấy password từ url
                                    m2.password = array[1];
                                    int cs = CoursesScheduleTable.GetByRoomID(j.id);
                                    m2.CoursesScheduleID = cs;
                                    m2.join_url = j.join_url;
                                    m2.created_at = j.created_at;
                                    m2.start_time = j.start_time;
                                    m2.topic = j.topic;
                                    m2.timezone = j.timezone;
                                    m2.teacherName = tc;
                                    meeting.Add(m2);
                                }
                            }
                        }
                    }
                }
            }

            if (meeting.Count() > 0)
            {
                if (!string.IsNullOrEmpty(search))
                {
                    meeting = meeting.Where(x => AssetCRM.RemoveUnicode(x.topic).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())).ToList();
                }
            }
            return View(meeting);
        }

        /// <summary>
        /// Danh sách meeting theo GV
        /// </summary>
        /// <param name="page"></param>
        /// <param name="teacherId"></param>
        /// <returns></returns>
        public ActionResult ListMeetingByTeacher(int page = 1, int teacherId = 0)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 4)// chỉ admin,gv mới được xem
                return RedirectToAction("Signin", "Login", new { area = "" });

            string search = "";
            ViewBag.Search = Request.QueryString["search"];
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                search = Request.QueryString["search"].ToString().Trim();
            string type = "";
            ViewBag.Type = Request.QueryString["type"];
            switch (Request.QueryString["type"].ToInt(0))
            {
                case 2:
                    type = "live";
                    break;
                case 3:
                    type = "upcoming";
                    break;
                default:
                    type = "scheduled";
                    break;
            }

            if (!string.IsNullOrEmpty(Request.QueryString["teacherId"]))
            {
                teacherId = Request.QueryString["teacherId"].ToInt(0);
            }
            ViewBag.teacherId = teacherId;


            //lấy giáo viên có cấu hình zoom
            var ck = ZoomTeacherAPITable.GetbyTeacherID(teacherId);
            if (ck == null)
            {
                SetAlert("Không thành công!", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }

            string userId = ck.UserID;
            string tokenString = "";
            if (!string.IsNullOrEmpty(ck.Token))
            {
                tokenString = ck.Token;
            }
            else
            {
                tokenString = AccessToken(ck.APISecrect, ck.APIKey);
            }

            int pageSize = 20;
            int page_number = page - 1;
            var client = new RestClient($"https://api.zoom.us/v2/users/{userId}/meetings?page_number={page_number}&page_size={pageSize}&type={type}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", String.Format("Bearer {0}", tokenString));
            IRestResponse response = client.Execute(request);
            HttpStatusCode statusCode = response.StatusCode;
            int numericStatusCode = (int)statusCode;
            int total = 0;
            int PageCount = 0;
            List<Meeting> meeting = new List<Meeting>();

            //chỗ này hứng data nè
            if (numericStatusCode == 200)
            {
                var jObject = JObject.Parse(response.Content);
                if (jObject != null)
                {
                    total = Convert.ToInt32(jObject["total_records"]);
                    PageCount = Convert.ToInt32(jObject["page_count"]);
                    meeting = JsonConvert.DeserializeObject<List<Meeting>>(jObject["meetings"].ToString());
                }
            }

            ViewBag.StartIndex = page_number;
            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;

            //var arrayNew = meeting.Select(i => new { idmeeting = i.id, url = i.join_url });
            //var array1 = meeting.Select(i => new KeyValuePair<string, string>(i.id, i.join_url));
            //var array2 = from i in meeting select new Tuple<string, string>(i.id, i.join_url);
            return View(meeting);
        }


        /// <summary>
        /// Danh sách Meeting dataTable
        /// </summary>
        /// <returns></returns>
        public ActionResult ManageListMeeting()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                // lấy ngày đầu và ngày cuối tháng
                DateTime date = DateTime.Now;
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                ViewBag.fromdate = firstDayOfMonth.ToString("dd/MM/yyyy");
                ViewBag.enddate = lastDayOfMonth.ToString("dd/MM/yyyy");

                return View();
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ZoomMeetingController", "ListMeeting", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }

        /// <summary>
        /// Load danh sách mê
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult LoadManageListMeeting()
        {
            OjectDataTable dataTable = new OjectDataTable();
            if (Session["UID"] == null)
                return Json(dataTable, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
            {
                return Json(dataTable, JsonRequestBehavior.AllowGet);
            }
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
                OrderBy += 6;// 6 là số colunm màn hình
            }

            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                search = Request.QueryString["search"].ToString().Trim();
            //center
            int isRoomStart = 2;
            if (!string.IsNullOrEmpty(Request.QueryString["isRoomStart"]))
            {
                isRoomStart = Request.QueryString["isRoomStart"].ToInt(2);
            }

            // lấy ngày đầu và ngày cuối tháng
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // thời gian bắt đầu từ
            string fromdate = firstDayOfMonth.ToString();
            string todate = lastDayOfMonth.ToString();

            // thời gian bắt đầu từ
            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                try
                {
                    DateTime fdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                    fromdate = fdate.ToString();
                }
                catch { }
            }
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

            List<VietCoCo_ListMeeting_Result> l = new List<VietCoCo_ListMeeting_Result>();
            using (var db = new ZimEntities())
            {
                l = db.VietCoCo_ListMeeting(search, isRoomStart, OrderBy, fromdate, todate, Start, Length, aclog.ID, aclog.RoleID.Value).ToList();
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
        [HttpGet]
        public ActionResult LoadManageListMeetingForStudent()
        {
            OjectDataTable dataTable = new OjectDataTable();
            if (Session["UID"] == null)
                return Json(dataTable, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            int courseID = int.Parse(Request.QueryString["courseID"]);
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
                OrderBy += 6;// 6 là số colunm màn hình
            }

            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                search = Request.QueryString["search"].ToString().Trim();
            //center
            int isRoomStart = 2;
            if (!string.IsNullOrEmpty(Request.QueryString["isRoomStart"]))
            {
                isRoomStart = Request.QueryString["isRoomStart"].ToInt(2);
            }
            // thời gian bắt đầu từ
            string fromdate = null;
            string todate = null;


            // thời gian bắt đầu từ
            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                try
                {
                    DateTime fdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                    fromdate = fdate.ToString("dd/MM/yyyy");
                }
                catch { }
            }
            // đến
            if (!string.IsNullOrEmpty(Request.QueryString["to"]))
            {
                try
                {
                    DateTime tdate = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null);
                    todate = tdate.ToString("dd/MM/yyyy");
                }
                catch { }
            }

            List<VietCoCo_ListMeeting_Result> l = new List<VietCoCo_ListMeeting_Result>();
            using (var db = new ZimEntities())
            {
                l = db.VietCoCo_ListMeeting(search, isRoomStart, OrderBy, fromdate, todate, Start, Length, aclog.ID, aclog.RoleID.Value).ToList();
            }
            l = l.Where(x => x.CourseID == courseID).ToList();
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
        /// <summary>
        /// Đóng phòng học
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult OffMeeting(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            try
            {
                var c = CoursesScheduleTable.getbyid(id);
                if (c == null)
                {
                    return Json(new { rs = false, m = "Không tìm thấy dữ liệu!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    //gọi api update status meeting
                    var ck = ZoomTeacherAPITable.GetbyTeacherID(c.TeacherID.Value);
                    if (ck == null)
                    {
                        return Json(new { rs = false, m = "Vui lòng cập nhật cấu hình cho GV trước!" }, JsonRequestBehavior.AllowGet);
                    }
                    string userId = ck.UserID;
                    string tokenString = "";
                    if (!string.IsNullOrEmpty(ck.Token))
                    {
                        tokenString = ck.Token;
                    }
                    else
                    {
                        tokenString = AccessToken(ck.APISecrect, ck.APIKey);
                    }
                    var client = new RestClient($"https://api.zoom.us/v2/meetings/{c.RoomID}/status");
                    var request = new RestRequest(Method.PUT);
                    request.AddHeader("authorization", String.Format("Bearer {0}", tokenString));
                    request.AddParameter("application/json", "{\"action\":\"end\"}", ParameterType.RequestBody);
                    IRestResponse response = client.Execute(request);
                    HttpStatusCode statusCode = response.StatusCode;
                    int numericStatusCode = (int)statusCode;

                    //Cập nhật trạng thái đóng phòng học
                    CoursesScheduleTable.UpdateisStartRoom(c.ID, false);
                }
                return Json(new { rs = true, m = "Đã đóng thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ZoomMeetingController", "OffMeeting", Session["UID"].ToString().ToInt(0), ex.Message);
                return Json(new { rs = false, m = "Rất tiếc đã xảy ra lỗi!" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult LoadDetailRecording(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            try
            {
                var c = CoursesScheduleTable.getbyid(id);
                if (c == null)
                {
                    return Json(new { rs = false, m = "Không tìm thấy dữ liệu!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    List<RecordingFiles> rFile = new List<RecordingFiles>();
                    //gọi api update status meeting
                    var ck = ZoomTeacherAPITable.GetbyTeacherID(c.TeacherID.Value);
                    if (ck == null)
                    {
                        return Json(new { rs = false, m = "Vui lòng cập nhật cấu hình cho GV trước!" }, JsonRequestBehavior.AllowGet);
                    }
                    string userId = ck.UserID;
                    string tokenString = "";
                    if (!string.IsNullOrEmpty(ck.Token))
                    {
                        tokenString = ck.Token;
                    }
                    else
                    {
                        tokenString = AccessToken(ck.APISecrect, ck.APIKey);
                    }
                    var client = new RestClient($"https://api.zoom.us/v2/meetings/{c.RoomID}/recordings");
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("authorization", String.Format("Bearer {0}", tokenString));
                    IRestResponse response = client.Execute(request);
                    HttpStatusCode statusCode = response.StatusCode;
                    int numericStatusCode = (int)statusCode;

                    //chỗ này hứng data nè
                    if (numericStatusCode == 200)
                    {
                        var jObject = JObject.Parse(response.Content);
                        if (jObject != null)
                        {
                            rFile = JsonConvert.DeserializeObject<List<RecordingFiles>>(jObject["recording_files"].ToString());
                        }
                    }
                    if (rFile.Count() == 0)
                    {
                        return Json(new { rs = false, m = "Dữ liệu trống!" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { rs = true, data = rFile }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ZoomMeetingController", "LoadDetailRecording", Session["UID"].ToString().ToInt(0), ex.Message);
                return Json(new { rs = false, m = "Rất tiếc đã xảy ra lỗi!" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Danh sách cuộc họp nội bộ
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult MeetingInternal(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)// chỉ admin mới được xem
                return RedirectToAction("Signin", "Login", new { area = "" });
            int pageSize = 20;
            string search = "";
            ViewBag.Search = Request.QueryString["search"];
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                search = Request.QueryString["search"].ToString().Trim();

            List<zoom_meeting_internal_Result> l = new List<zoom_meeting_internal_Result>();
            using (var db = new ZimEntities())
            {
                l = db.zoom_meeting_internal(search, 0, page - 1, pageSize).ToList();
            }

            ViewBag.StartIndex = (page - 1) * pageSize;
            int total = 0;
            if (l.Count() > 0)
            {
                total = l[0].TotalRow.Value;
            }
            ViewBag.Total = total;
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            return View(l);
        }

        [HttpPost]
        public ActionResult LoadStaffByRole(int roleId)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            StringBuilder html = new StringBuilder();
            if (roleId == 0) //trừ học viên ra
            {
                html.Append("<option value=\"0\" selected>Tất cả</option>");
            }
            else
            {
                using (var db = new ZimEntities())
                {
                    var ac = db.tbl_Account.Where(x => x.Status == 2 && x.RoleID == roleId && x.IsDelete == false && x.ID != aclog.ID).Select(i => new { ID = i.ID, FullName = i.FullName }).ToList();
                    if (ac.Count() == 0)
                    {
                        return Json(new { rs = false, m = "Dữ liệu nhân viên trống!" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        foreach (var item in ac)
                        {
                            html.Append("<option value=\"" + item.ID + "\" selected>" + item.FullName + "</option>");
                        }
                    }
                }
            }
            return Json(new { rs = true, data = html.ToString() }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Tạo phòng họp nội bộ
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateRoomInternal(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var returns = Redirect(Request.UrlReferrer.AbsoluteUri);
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var a = ZoomTeacherAPITable.GetbyTeacherID(aclog.ID);
                if (a == null)
                {
                    SetAlert("Vui lòng cấu hình API Zoom trước!", "e");
                    return returns;
                }

                DateTime sDate = new DateTime();
                try
                {
                    sDate = DateTime.ParseExact(f.Get("txt-StartDate").ToString().Replace(" ", ""), "dd/MM/yyyy", null);
                }
                catch
                {
                    SetAlert("Định dạng ngày họp không đúng", "e");
                    return returns;
                }
                string sTime = f.Get("txt-time").ToString();
                int type = f.Get("ddl-role").ToInt(0);
                string topic = f.Get("txt-topic");
                string arrayUID = f.Get("ddl-staff");
                bool isSendMail = false;
                if (!string.IsNullOrEmpty(f.Get("chk-box")))
                    isSendMail = true;
                string starttime = sDate.Date.ToString("yyyy-MM-dd") + "T" + sTime + ":00";

                string userId = a.UserID;
                string tokenString = "";
                if (!string.IsNullOrEmpty(a.Token))
                {
                    tokenString = a.Token;
                }
                else
                {
                    tokenString = AccessToken(a.APISecrect, a.APIKey);
                }
                var client = new RestClient($"https://api.zoom.us/v2/users/{userId}/meetings");
                var request = new RestRequest(Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddJsonBody(new { topic = topic, duration = "180", start_time = starttime, type = "2" });

                request.AddHeader("authorization", String.Format("Bearer {0}", tokenString));
                IRestResponse restResponse = client.Execute(request);
                HttpStatusCode statusCode = restResponse.StatusCode;
                int numericStatusCode = (int)statusCode;
                var jObject = JObject.Parse(restResponse.Content);

                if (numericStatusCode == 404)
                {
                    if (jObject["code"].ToString() == "1001")
                    {
                        SetAlert("Tài khoản tạo phòng chưa đúng!", "e");
                        return returns;
                    }
                    else if (jObject["code"].ToString() == "124")
                    {
                        SetAlert("Lỗi xác thực, Vui lòng kiểm tra lại API Key và Secrect!", "e");
                        return returns;
                    }
                    else
                    {
                        SetAlert("Đã xảy ra lỗi, vui lòng liên hệ admin!", "e");
                        return returns;
                    }
                }
                else if (numericStatusCode == 201)
                {
                    var roomId = jObject["id"].ToString();
                    var roomPass = jObject["encrypted_password"].ToString();
                    if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(roomPass))
                    {
                        SetAlert("Lỗi tạo phòng, vui lòng liên hệ admin!", "e");
                        return returns;
                    }
                    var z = ZoomMeetingInternalTable.Insert(topic, type, arrayUID, isSendMail, sDate, sTime, roomId, roomPass, aclog.UserName);
                    try
                    {
                        Thread pushNoti = new Thread(() => PushNotiJoinMeeting(z));
                        pushNoti.Start();
                    }
                    catch (Exception ex)
                    {
                        AssetCRM.Writelog("ZoomMeetingController", "CreateRoomInternal", Session["UID"].ToString().ToInt(0), ex.Message);
                        SetAlert("Lỗi gửi thông báo, vui lòng liên hệ nhà cung cấp!", "e");
                    }
                    //try
                    //{
                    //    if (isSendMail == true)
                    //    {
                    //        Thread pushMail = new Thread(() => PushMailJoinMeeting(z));
                    //        pushMail.Start();
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    AssetCRM.Writelog("ZoomMeetingController", "CreateRoomInternal", Session["UID"].ToString().ToInt(0), ex.Message);
                    //    SetAlert("Lỗi gửi mail, vui lòng liên hệ nhà cung cấp!", "e");
                    //}
                }
                SetAlert("Tạo phòng thành công!", "s");
                return returns;
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ZoomMeetingController", "CreateRoomInternal", Session["UID"].ToString().ToInt(0), ex.Message);
                SetAlert("Đã xảy ra lỗi, vui lòng liên hệ nhà cung cấp!", "e");
                return returns;
            }
        }

        public void PushNotiJoinMeeting(tbl_ZoomMeetingInternal d)
        {
            try
            {
                string link = "<a href=\"/admin/zoommeeting/JoinMeetingInternal?id=" + d.ID + "\">CLICK THAM GIA</a>";
                string content = "Bạn nhận được lời mời tham dự cuộc họp vào ngày " + d.StartDate.Value.ToString("dd/MM/yyyy") + " lúc " + d.StartTime + " " + link;
                string[] arrayUID = d.ArrayUID.Split(',');
                if (arrayUID.Count() != 0)
                {
                    Parallel.For(0, arrayUID.Length, index =>
                    {
                        NotificationTable.insertsingle("", d.Topic, content, arrayUID[index].ToInt(0), d.CreatedBy, 0, 0);
                    });
                }
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ZoomMeetingController", "PushNotiJoinMeeting", 0, ex.Message);
            }
        }

        public void PushMailJoinMeeting(tbl_ZoomMeetingInternal d)
        {
            try
            {
                string link = "<a href=\"https://us05web.zoom.us/j/" + d.RoomId + "?pwd=" + d.RoomPass + "\">CLICK THAM GIA</a>";
                string content = "Bạn nhận được lời mời tham dự cuộc họp vào ngày " + d.StartDate.Value.ToString("dd/MM/yyyy") + " lúc " + d.StartTime + " " + link;
                string[] arrayUID = d.ArrayUID.Split(',');
                if (arrayUID.Count() != 0)
                {
                    Parallel.For(0, arrayUID.Length, index =>
                    {
                        // gửi mail
                        var ac = AccountTable.getbyID(arrayUID[index].ToInt(0));
                        if (ac != null)
                        {
                            if (!string.IsNullOrEmpty(ac.Email))
                                AppZim.ZIM.AssetCRM.SendMail(ac.Email, d.Topic, content);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ZoomMeetingController", "PushMailJoinMeeting", 0, ex.Message);
            }
        }

        /// <summary>
        /// Join meeting nội bộ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult JoinMeetingInternal(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var returns = Redirect(Request.UrlReferrer.AbsoluteUri);
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var c = ZoomMeetingInternalTable.getbyID(id);
                if (c == null)
                {
                    SetAlert("Không tìm thấy cuộc họp!", "e");
                    return returns;
                }
                else if (c.StartDate < DateTime.Now)
                {
                    SetAlert("Lịch họp đã hết hạn!", "e");
                    return returns;
                }

                var z = ZoomTeacherAPITable.GetbyTeacherID(AccountTable.getbyusername(c.CreatedBy).ID);
                if (aclog.UserName != c.CreatedBy) //nếu không phải người tạo phòng
                {
                    if (c.isStart == false)
                    {
                        SetAlert("Phòng họp chưa bắt đầu!", "e");
                        return returns;
                    }
                }
                else
                {
                    if (z == null)
                    {
                        SetAlert("Vui lòng cấu hình Zoom trước!", "e");
                        return returns;
                    }
                    else
                    {
                        ZoomMeetingInternalTable.UpdateisStart(c.ID, true, aclog.UserName); //mở phòng nếu nó người tạo
                    }
                }

                Room r = new Room();
                r.ID = c.ID;
                r.RoomID = c.RoomId;
                r.RoomPass = c.RoomPass;
                if (aclog.RoleID.Value == 5)
                {
                    r.Role = 0; //học viên
                }
                else if (aclog.UserName == c.CreatedBy)
                {
                    r.Role = 1; //admin, gv =>host
                }
                else
                {
                    r.Role = 5; //người ngoài
                }
                r.Key = z.APIKey;
                r.Secrect = z.APISecrect;
                r.FullName = aclog.FullName;
                ViewBag.Data = r; //trả data qua view => bên view auto click redirect
                return View();
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ZoomMeetingController", "JoinMeetingInternal", Session["UID"].ToString().ToInt(0), ex.Message);
                SetAlert("Đã xảy ra lỗi, vui lòng liên hệ nhà cung cấp!", "e");
                return returns;
            }
        }
        public ActionResult LoadMeetingInternal(int IdZoomMeetingInternal)
        {
            ViewBag.Id = IdZoomMeetingInternal;
            return View();
        }
        public ActionResult AfterMeetingInternal(int IdZoomMeetingInternal)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog != null)
                {
                    var check = ZoomMeetingInternalTable.getbyID(IdZoomMeetingInternal);
                    if (check != null)
                    {
                        if (check.CreatedBy == aclog.UserName)
                        {
                            //đóng phòng họp nếu đúng là người tạo
                            ZoomMeetingInternalTable.UpdateisStart(check.ID, false, aclog.UserName);
                        }
                    }
                }
                return RedirectToAction("ProfileUser", "Account", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ZoomMeetingController", "AfterMeetingInternal", Session["UID"].ToString().ToInt(0), ex.Message);
                //SetAlert("Đã xảy ra lỗi, vui lòng liên hệ nhà cung cấp!", "e");
                return RedirectToAction("ProfileUser", "Account", new { area = "Admin" });
            }
        }

        [HttpPost]
        public ActionResult LoadUserJoinMeeting(int id, int PageIndex, string searchName, int PageSize)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            try
            {
                StringBuilder html = new StringBuilder();
                using (var db = new ZimEntities())
                {
                    var ac = db.tbl_ZoomMeetingInternal.Where(x => x.ID == id).Select(i => new { ID = i.ID, ArrayUID = i.ArrayUID }).Single();
                    if (ac == null)
                    {
                        return Json(new { rs = false, m = "Dữ liệu nhân viên trống!" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        string[] array = ac.ArrayUID.Split(',');
                        if (array.Count() != 0)
                        {
                            List<int> listUser = new List<int>();
                            for (var i = 0; i < array.Count(); i++)
                            {
                                var user = AccountTable.getbyID(Convert.ToInt32(array[i]));
                                if (user != null)
                                {
                                    listUser.Add(user.ID);
                                }
                            }

                            if (listUser.Count() != 0)
                            {
                                listUser = listUser.Skip(PageIndex * PageSize).Take(PageSize).ToList();
                                foreach (var i in listUser)
                                {
                                    var item = db.tbl_Account.Where(x => x.ID == i && x.Status == 2 && x.IsDelete == false).Select(j => new { AvatarThumbnail = j.AvatarThumbnail, FullName = j.FullName, Phone = j.Phone }).Single();
                                    if (item != null)
                                    {
                                        html.Append("<div class=\"media blog status-post d-flex align-items-center justify-content-between\">");
                                        html.Append("    <div class=\"author\">");
                                        if (!string.IsNullOrEmpty(item.AvatarThumbnail))
                                            html.Append("        <a href=\"#\" class=\"avatar\"><img src=\"" + item.AvatarThumbnail + "\" onerror=\"this.onerror = null; this.src = '/app-assets/zimv2/images/invis-user.png'; \" alt=\"avatar\"></a>");
                                        else
                                            html.Append("        <a href=\"#\" class=\"avatar\"><img src=\"/app-assets/zimv2/images/invis-user.png\" alt=\"avatar\"></a>");
                                        html.Append("        <div class=\"author-info\">");
                                        html.Append("            <a href=\"#\" class=\"username\"><span class=\"hasVerifiedBadge\">" + item.FullName + "</span></a>");
                                        html.Append("            <div class=\"date-comment-view\">");
                                        html.Append("                <span class=\"date\">");
                                        html.Append("                    <span class=\"DateTime\" title=\"11:20 ngày 2/10/19\">");
                                        html.Append("                        " + item.Phone + "");
                                        html.Append("                    </span>");
                                        html.Append("                </span>");
                                        html.Append("            </div>");
                                        html.Append("        </div>");
                                        html.Append("    </div>");
                                        html.Append("</div>");
                                    }
                                }
                            }
                        }
                    }
                }
                return Json(new { rs = true, data = html.ToString(), PageIndex = PageIndex += 1, hdfId = id }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ZoomMeetingController", "LoadUserJoinMeeting", Session["UID"].ToString().ToInt(0), ex.Message);
                SetAlert("Đã xảy ra lỗi, vui lòng liên hệ nhà cung cấp!", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
        }
    }
}