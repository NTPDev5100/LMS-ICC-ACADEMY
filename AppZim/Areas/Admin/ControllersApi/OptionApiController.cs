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
using System.Web.Http;

namespace AppZim.Areas.Admin.ControllersApi
{
    [Route("api/OptionApi/{action}", Name = "OptionApi")]
    public class OptionApiController : ApiController
    {
        //lấy thành phố
        [HttpGet]
        public HttpResponseMessage getcity(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_City.OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        //lấy quận huyện
        [HttpGet]
        public HttpResponseMessage getdistrict(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_District.OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        //lấy nguồn nhân viên
        [HttpGet]
        public HttpResponseMessage getscourse(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_SourceOfCustomer.OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        //lấy khối học
        [HttpGet]
        public HttpResponseMessage getgrade(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Grade.OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        //lấy khối học
        [HttpGet]
        public HttpResponseMessage getclass(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Class.OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        //lấy ca học
        [HttpGet]
        public HttpResponseMessage getstudytime(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_StudyTime.OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        //lấy cơ sở
        [HttpGet]
        public HttpResponseMessage getschool(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_School.OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        //lấy phòng cơ sở
        [HttpGet]
        public HttpResponseMessage getroomschool(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var queryParams = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);

            if (queryParams.ContainsKey("schoolid"))
            {
                int id = queryParams["schoolid"].ToInt(0);
                var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_SchoolRoom.Where(n => n.SchoolID == id).OrderByDescending(n => n.ID).ToList(), loadOptions));
                return s;
            }
            else
            {
                var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_SchoolRoom.OrderByDescending(n => n.ID).ToList(), loadOptions));
                return s;
            }
        }

        //danh sách mã km
        [HttpGet]
        public HttpResponseMessage getdiscount(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Discount.OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        //lấy tất cả ngày nghỉ
        [HttpGet]
        public HttpResponseMessage getdayoff(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_DayOff.Where(n => n.isHide != true).OrderByDescending(n => n.DayOff).ToList(), loadOptions));
            return s;
        }

        //tất cả nghề nghiệp
        [HttpGet]
        public HttpResponseMessage getjobs(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Job.Where(n => n.isHide != true).OrderBy(n => n.JobName).ToList(), loadOptions));
            return s;
        }

        //danh sách dich vụ hệ thống
        [HttpGet]
        public HttpResponseMessage getservices(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Services.OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        //danh sách feedback hệ thông
        [HttpGet]
        public HttpResponseMessage getfeedbacks(DataSourceLoadOptions loadOptions)
        {
            //var l = new ZimEntities();
            //var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_FeedBack.OrderByDescending(n => n.isNew).ThenByDescending(n => n.ID).ToList(), loadOptions));
            //return s;

            var l = new ZimEntities();
            var queryParams = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
            if (queryParams.ContainsKey("uid"))
            {
                int supportid = queryParams["uid"].ToInt(0);
                var ac = AccountTable.getbyID(supportid);
                if (ac.RoleID == 6)// tư vấn viên
                {
                    var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_FeedBack.Where(n => n.SupportID == supportid).OrderByDescending(n => n.isNew).ThenByDescending(n => n.ID).ToList(), loadOptions));
                    return s;
                }
                else
                {
                    var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_FeedBack.OrderByDescending(n => n.isNew).ThenByDescending(n => n.ID).ToList(), loadOptions));
                    return s;
                }
            }
            else
            {
                var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_FeedBack.OrderByDescending(n => n.isNew).ThenByDescending(n => n.ID).ToList(), loadOptions));
                return s;
            }
        }

        //danh sách feedback cua toi
        [HttpGet]
        public HttpResponseMessage getfeedbackuid(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var queryParams = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
            if (queryParams.ContainsKey("uid"))
            {
                int id = queryParams["uid"].ToInt(0);
                var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_FeedBack.Where(n => n.UID == id).OrderByDescending(n => n.isNew).ThenByDescending(n => n.ID).ToList(), loadOptions));
                return s;
            }
            return null;
        }

        //danh sách loại feedback
        [HttpGet]
        public HttpResponseMessage getcategoryfeedbacks(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_FeedBackCategory.OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        //lấy danh sách phiếu thu
        [HttpGet]
        public HttpResponseMessage getinvoices(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Invoices.Where(n => n.Refund != true).OrderByDescending(n => n.CreatedDate).ToList(), loadOptions));
            return s;
        }

        //lấy danh sách phiếu chi
        [HttpGet]
        public HttpResponseMessage getvouchers(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Invoices.Where(n => n.Refund == true).OrderByDescending(n => n.CreatedDate).ToList(), loadOptions));
            return s;
        }

        //lấy danh sách nhà cung cấp
        [HttpGet]
        public HttpResponseMessage getisuppliers(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_SupplierServices.OrderByDescending(n => n.CreatedDate).ToList(), loadOptions));
            return s;
        }

        //lấy danh sách mục đích học tập
        [HttpGet]
        public HttpResponseMessage getacademicpurposes(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_AcademicPurposes.OrderByDescending(n => n.CreatedDate).ToList(), loadOptions));
            return s;
        }

        //lấy danh sách mục đích học tập
        [HttpGet]
        public HttpResponseMessage getrefundlist(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Refunds.OrderByDescending(n => n.CreatedDate).ToList(), loadOptions));
            return s;
        }

        //lấy danh sách cấu hình lương
        [HttpGet]
        public HttpResponseMessage getsettingsalary(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_SalarySetting.OrderByDescending(n => n.Million).ToList(), loadOptions));
            return s;
        }

        //lấy danh sách kì thi
        [HttpGet]
        public HttpResponseMessage getserviceexam(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_ServiceExams.OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }
    }
}
