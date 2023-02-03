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

namespace AppZim.Areas.api.notification
{
    [Route("api/notification/{action}", Name = "apinotification")]
    public class notificationController : ApiController
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
        public HttpResponseMessage getTop5()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var top5 = NotificationTable.GetByUIDNew(aclog.ID).OrderByDescending(x => x.ID).Take(5).ToList();
                var total = NotificationTable.GetByUIDNew(aclog.ID).Count;
                if (top5.Count == 0)
                {
                    top5 = NotificationTable.GetByUID(aclog.ID).OrderByDescending(x => x.ID).Take(5).ToList();
                    total = top5.Count;
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = top5, total, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getList(int page)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                int pagesize = 10;
                List<API_notification_getlist_Result> l = new List<API_notification_getlist_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.API_notification_getlist(aclog.ID, (page - 1) * pagesize, pagesize).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage seen()
        {
            try
            {
                var aclog = AccountTable.getByToken(getToken());
                if (aclog == null)
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
                if (aclog.ExpiredToken < DateTime.Now)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

                int id = HttpContext.Current.Request.Form.Get("id").ToInt(0);

                if (id != 0)
                {
                    var rs = NotificationTable.updateView(id, aclog.UserName);
                    if (rs != null)
                        return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                    else
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                else
                {
                    var l = NotificationTable.GetByUID(aclog.ID).Where(n => n.Status == 1).ToList();
                    foreach (var item in l)
                    {
                        NotificationTable.updateView(item.ID, aclog.UserName);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getDebt(int studentID)
        {
            try
            {
                var aclog = AccountTable.getByToken(getToken());
                if (aclog == null)
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
                if (aclog.ExpiredToken < DateTime.Now)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
                if (aclog.RoleID != 14)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

                var l = new List<API_Parents_GetDebts_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.API_Parents_GetDebts(studentID).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
    }
}
