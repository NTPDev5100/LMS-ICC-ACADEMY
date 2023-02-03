using AppZim.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AppZim.Areas.Admin.ControllersApi
{
    [Route("api/ClassApi/{action}", Name = "ClassApi")]
    public class ClassApiController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage getgroubexe(DataSourceLoadOptions loadOptions)// nhóm bài
        {
            //var l = new ZimEntities();
            //var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_ExerciseGroup.OrderByDescending(n => n.ID).ToList(), loadOptions));
            //return s;

            var l = new ZimEntities();
            var queryParams = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);

            if (queryParams.ContainsKey("curriculumid"))
            {
                int id = queryParams["curriculumid"].ToInt(0);
                var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_ExerciseGroup.Where(n => n.CurriculumID == id).OrderByDescending(n => n.ID).ToList(), loadOptions));
                return s;
            }
            else
            {
                var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_ExerciseGroup.OrderByDescending(n => n.ID).ToList(), loadOptions));
                return s;
            }
        }
    }
}
