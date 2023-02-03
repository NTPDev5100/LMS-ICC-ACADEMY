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
    [Route("api/ExerciseApi/{action}", Name = "ExerciseApi")]
    public class ExerciseApiController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage getexercisestudentnew(DataSourceLoadOptions loadOptions)// bài mới
        {
            //var l = new ZimEntities();
            //var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_ExerciseAddonStudent.Where(n => n.StatusDone == 1 && (n.Type == 1 || n.Type == 3)).ToList(), loadOptions));
            //return s;

            HttpResponseMessage s = new HttpResponseMessage();
            var l = new ZimEntities();
            var queryParams = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
            if (queryParams.ContainsKey("aclog"))
            {
                int id = queryParams["aclog"].ToInt(0);
                //lấy bài mới 
                List<tbl_ExerciseAddonStudent> data = new List<tbl_ExerciseAddonStudent>();
                IEnumerable<tbl_ExerciseAddonStudent> exnew = l.tbl_ExerciseAddonStudent.Where(n => n.StatusDone == 1 && (n.Type == 1 || n.Type == 3)).ToList();
                IEnumerable<tbl_TeacherCourse> teachercourse = l.tbl_TeacherCourse.Where(n => n.TeacherUID == id && n.isHide != true).ToList();
                var exs = from ex in exnew
                          join teacher in teachercourse on ex.CourseID equals teacher.CourseID
                          select ex;
                data.AddRange(exs);
                s = Request.CreateResponse(DataSourceLoader.Load(data.ToList(), loadOptions));
                return s;
            }
            else
            {
                s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_ExerciseAddonStudent.Where(n => n.StatusDone == 1 && (n.Type == 1 || n.Type == 3)).ToList(), loadOptions));
                return s;
            }
        }

        [HttpGet]
        public HttpResponseMessage getexercisestudentprocess(DataSourceLoadOptions loadOptions)// đang chấm
        {
            HttpResponseMessage s = new HttpResponseMessage();
            var l = new ZimEntities();
            var queryParams = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
            if (queryParams.ContainsKey("aclog"))
            {
                int id = queryParams["aclog"].ToInt(0);
                s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_ExerciseAddonStudent.Where(n => n.StatusDone == 2 && n.UIDFix == id && (n.Type == 1 || n.Type == 3)).ToList(), loadOptions));
            }
            return s;
        }

        [HttpGet]
        public HttpResponseMessage getexercisestudentfeedback(DataSourceLoadOptions loadOptions)// chấm lại học viên không đồng ý
        {
            HttpResponseMessage s = new HttpResponseMessage();
            var l = new ZimEntities();
            var queryParams = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
            if (queryParams.ContainsKey("aclog"))
            {
                int id = queryParams["aclog"].ToInt(0);
                s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_ExerciseAddonStudent.Where(n => n.StatusDone == 4 && n.UIDFix == id && (n.Type == 1 || n.Type == 3)).ToList(), loadOptions));
            }
            return s;
        }

        [HttpGet]
        public HttpResponseMessage getexercisestudentdone(DataSourceLoadOptions loadOptions)// xong
        {
            HttpResponseMessage s = new HttpResponseMessage();
            var l = new ZimEntities();
            var queryParams = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
            if (queryParams.ContainsKey("aclog"))
            {
                int id = queryParams["aclog"].ToInt(0);
                s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_ExerciseAddonStudent.Where(n => (n.StatusDone == 3 || n.StatusDone == 5) && n.UIDFix == id && (n.Type == 1 || n.Type == 3)).OrderByDescending(n => n.ModifiedDate).ToList(), loadOptions));
            }
            return s;
        }

        [HttpGet]
        public HttpResponseMessage getallexercisestudentdone(DataSourceLoadOptions loadOptions)// lấy tất cả bài của giáo viên đã xong
        {
            HttpResponseMessage s = new HttpResponseMessage();
            var l = new ZimEntities();
            var queryParams = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
            if (queryParams.ContainsKey("aclog"))
            {
                int id = queryParams["aclog"].ToInt(0);
                s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_ExerciseAddonStudent.Where(n => (n.StatusDone == 3 || n.StatusDone == 5) && (n.Type == 1 || n.Type == 3)).OrderByDescending(n => n.ModifiedDate).ToList(), loadOptions));
            }
            return s;
        }
    }
}
