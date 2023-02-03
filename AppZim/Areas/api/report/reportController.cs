using AppZim.api;
using AppZim.Models;
using AppZim.TableSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AppZim.Areas.api.report
{
    [Route("api/report/{action}", Name = "apireport")]
    public class reportController : ApiController
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
        public HttpResponseMessage getCustomerWarning(int centerID, string search, string fdate, string tdate, int page)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                //page size
                int pageSize = 10;
                List<API_getCustomerWarning_Result> l = new List<API_getCustomerWarning_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.API_getCustomerWarning(centerID, aclog.RoleID.Value, aclog.ID, search, fdate, tdate, (page - 1) * pageSize, pageSize).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getTestResult(int centerID, string search, string fdate, string tdate, int page)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                int pageSize = 10;
                List<API_Get_TestResult_Student_Result> l = new List<API_Get_TestResult_Student_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.API_Get_TestResult_Student(search, "", fdate, tdate, aclog.RoleID.Value, aclog.ID, (page - 1) * pageSize, pageSize).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
    }
}
