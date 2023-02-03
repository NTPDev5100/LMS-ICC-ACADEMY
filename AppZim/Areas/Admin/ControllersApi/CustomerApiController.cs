using AppZim.Areas.Admin.Controllers;
using AppZim.Models;
using AppZim.TableSql;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace AppZim.Areas.Admin.ControllersApi
{
    [Route("api/CustomerApi/{action}", Name = "CustomerApi")]
    public class CustomerApiController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Account.Where(n=>n.RoleID == 4).ToList(), loadOptions));
            return s;
        }

        public StatusApi Rating(DataSourceLoadOptions loadOptions, int CourseScheduleID, int courseID, int studentID, int rate, int status, string note, string createBy)
        {
            StatusApi t = new StatusApi();
            IEnumerable<string> head_key;
            if (Request.Headers.TryGetValues("key", out head_key))
            {
                string key = head_key.FirstOrDefault().ToString();
                if (key != "SHkgxqFpIEh5IMOgLCDEkcOieSBsw6AgQVBJIG5ow6k")
                {
                    t.Code = 201;
                    t.Message = "Fail";
                    return t;
                }
                else
                {
                    var check = FeedbackScheduleTable.GetByCourseScheduleID(CourseScheduleID);
                    if (check.Count > 0)
                    {
                        t.Code = 201;
                        t.Message = "Fail";
                        return t;
                    }
                    FeedbackScheduleTable.Insert(studentID, CourseScheduleID, courseID, note, status, rate, createBy);
                    t.Code = 200;
                    t.Message = "Done";
                    return t;
                }
            }
            else
            {
                t.Code = 201;
                t.Message = "Fail";
                return t;
            }
        }

        [HttpGet]
        public HttpResponseMessage GetListRating(DataSourceLoadOptions loadOptions,int courseID, string sDate, string eDate)
        {
            List<StatusApi> t = new List<StatusApi>();
            IEnumerable<string> head_key;
            if (Request.Headers.TryGetValues("key", out head_key))
            {
                string key = head_key.FirstOrDefault().ToString();
                if (key != "SHkgxqFpIEh5IMOgLCDEkcOieSBsw6AgQVBJIG5ow6k")
                {
                    t.Add(new StatusApi
                    {
                        Code = 201,
                        Message = "Fail"
                    });
                    var s = Request.CreateResponse(DataSourceLoader.Load(t, loadOptions));
                    return s;
                }
                else
                {
                    // viết code ở đây
                    //var l = from ex in SetPackageTable.getall()
                    //        where ex.Show == true && ex.isHide != true
                    //        select new
                    //        {
                    //            ID = ex.ID,
                    //            PackageName = ex.PackageName
                    //        };

                    //Vòng lặp từ 1 đến 5*
                    for (int i = 1; i <= 5; i++)
                    {
                        int rate = FeedbackScheduleTable.GetRating(courseID, i, sDate, eDate);
                        t.Add(new StatusApi { Code = i, Message = rate.ToString() }) ;
                    }
                    var s = Request.CreateResponse(DataSourceLoader.Load(t, loadOptions));
                    return s;
                }
            }
            else
            {
                t.Add(new StatusApi
                {
                    Code = 201,
                    Message = "Fail"
                });
                var s = Request.CreateResponse(DataSourceLoader.Load(t, loadOptions));
                return s;
            }
        }
    }
}
