using AppZim.api;
using AppZim.Models;
using AppZim.TableSql;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace AppZim.Areas.api.feedbackschedule
{
    public class feedbackScheduleController : ApiController
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
        public HttpResponseMessage FeedbackSchedule(int course = 0, int page = 1)
        {
            try
            {
                var aclog = AccountTable.getByToken(getToken());
                if (aclog == null)
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
                if (aclog.ExpiredToken < DateTime.Now)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
                if (aclog.RoleID != 5)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

                var pageSize = 10;
                var l = new List<Admin_FeedbackSchedule_GetList_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.Admin_FeedbackSchedule_GetList(aclog.ID, course, (page - 1) * pageSize, pageSize).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message + " || " + ex.InnerException });
            }
        }
        [HttpGet]
        public HttpResponseMessage FeedbackedList(int course = 0, int page = 1)
        {
            try
            {
                var aclog = AccountTable.getByToken(getToken());
                if (aclog == null)
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
                if (aclog.ExpiredToken < DateTime.Now)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
                if (aclog.RoleID != 4 && aclog.RoleID != 5)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

                var pageSize = 10;
                if (aclog.RoleID == 5)
                {
                    var l = new List<Admin_FeedbackSchedule_GetListFeedbacked_Result>();
                    using (var db = new ZimEntities())
                    {
                        l = db.Admin_FeedbackSchedule_GetListFeedbacked(aclog.RoleID, aclog.ID, course, (page - 1) * pageSize, pageSize).ToList();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
                }
                else
                {
                    List<Teacher_GetListFeedback_Result> l = new List<Teacher_GetListFeedback_Result>();
                    using (var db = new ZimEntities())
                    {
                        l = db.Teacher_GetListFeedback(aclog.ID, (page - 1) * pageSize, pageSize, course).ToList();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message + " || " + ex.InnerException });
            }
        }
        [HttpPost]
        public HttpResponseMessage InsertFeedbackSchedule()
        {
            try
            {
                var aclog = AccountTable.getByToken(getToken());
                if (aclog == null)
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
                if (aclog.ExpiredToken < DateTime.Now)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
                if (aclog.RoleID != 5)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

                var courseScheduleID = HttpContext.Current.Request.Form.Get("courseScheduleID").ToInt(0);
                var courseID = HttpContext.Current.Request.Form.Get("courseID").ToInt(0);
                var feedbackContent = HttpContext.Current.Request.Form.Get("feedbackContent");
                var scheduleStatus = HttpContext.Current.Request.Form.Get("scheduleStatus").ToInt(0);
                var rating = HttpContext.Current.Request.Form.Get("rating").ToInt(0);

                var rs = FeedbackScheduleTable.Insert(aclog.ID, courseScheduleID, courseID, feedbackContent, scheduleStatus, rating, aclog.UserName);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = rs, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message + " || " + ex.InnerException });
            }
        }
        [HttpPost]
        public HttpResponseMessage UpdateFeedbackSchedule()
        {
            try
            {
                var aclog = AccountTable.getByToken(getToken());
                if (aclog == null)
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
                if (aclog.ExpiredToken < DateTime.Now)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
                if (aclog.RoleID != 5)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

                var feedbackid = HttpContext.Current.Request.Form.Get("feedbackID").ToInt(0);
                var feedbackContent = HttpContext.Current.Request.Form.Get("feedbackContent");
                var scheduleStatus = HttpContext.Current.Request.Form.Get("scheduleStatus").ToInt(0);
                var rating = HttpContext.Current.Request.Form.Get("rating").ToInt(0);

                var rs = FeedbackScheduleTable.Update(feedbackid, feedbackContent, scheduleStatus, rating, aclog.UserName);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = rs, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message + " || " + ex.InnerException });
            }
        }
    }
}