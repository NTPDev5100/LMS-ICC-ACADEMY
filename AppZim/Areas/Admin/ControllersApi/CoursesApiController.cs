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
using System.Threading.Tasks;
using static AppZim.ZIM.DataApi;
using System.Web;

namespace AppZim.Areas.Admin.ControllersApi
{
    [Route("api/CoursesApi/{action}", Name = "CoursesApi")]
    public class CoursesApiController : ApiController
    {
        //lấy khóa học
        [HttpGet]
        public HttpResponseMessage getcourse(DataSourceLoadOptions loadOptions)
        {
            //var l = new ZimEntities();
            //var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Courses.Where(n => n.CourseSelfStudy != true).OrderByDescending(n => n.ID).ToList(), loadOptions));
            //return s;

            var l = new ZimEntities();
            HttpResponseMessage s = new HttpResponseMessage();
            var queryParams = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
            if (queryParams.ContainsKey("uid"))
            {
                int uid = queryParams["uid"].ToInt(0);
                var aclog = AccountTable.getbyID(uid);
                if (aclog.RoleID == 8)//quản lý chuyển môn
                {
                    List<tbl_Courses> data = new List<tbl_Courses>();
                    IEnumerable<tbl_AssignManagerSpecializeToSchool> trungtamquanly = AssignManagerSpecializeToSchoolTable.getbyuid(aclog.ID).Where(n => n.isHide != true).ToList();
                    IEnumerable<tbl_Courses> tatcakhoa = CoursesTable.getall().Where(n => n.CourseSelfStudy != true).ToList();

                    var kh = from khoa in tatcakhoa
                             join trungtam in trungtamquanly on khoa.SchoolID equals trungtam.SchoolID
                             select khoa;
                    data.AddRange(kh);

                    s = Request.CreateResponse(DataSourceLoader.Load(data.ToList(), loadOptions));
                    //s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Courses.Where(n => n.CourseSelfStudy != true).OrderByDescending(n => n.ID).ToList(), loadOptions));
                    return s;
                }
                else
                {
                    s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Courses.Where(n => n.CourseSelfStudy != true).OrderByDescending(n => n.ID).ToList(), loadOptions));
                    return s;
                }
            }
            else
            {
                s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Courses.Where(n => n.CourseSelfStudy != true).OrderByDescending(n => n.ID).ToList(), loadOptions));
                return s;
            }
        }

        //lấy khóa học tụ học
        [HttpGet]
        public HttpResponseMessage getcourseselfstudy(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_Courses.Where(n => n.CourseSelfStudy == true).OrderByDescending(n => n.ID).ToList(), loadOptions));
            return s;
        }

        //lấy danh sách chiến dịch
        [HttpGet]
        public HttpResponseMessage getcampaignsale(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_CampaignSale.OrderByDescending(n => n.eTime).ToList(), loadOptions));
            return s;
        }

        //lấy danh sách chiến dịch
        [HttpGet]
        public HttpResponseMessage getstudentcampaignsale(DataSourceLoadOptions loadOptions)
        {
            var l = new ZimEntities();
            var queryParams = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);

            if (queryParams.ContainsKey("campid"))
            {
                int campid = queryParams["campid"].ToInt(0);
                int uid = queryParams["uid"].ToInt(0);

                var aclog = AccountTable.getbyID(uid);
                if (aclog.RoleID == 6)//saler
                {
                    var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_CoursesStudent.Where(n => n.CampaignSaleID == campid && n.SupportUID == uid).OrderByDescending(n => n.ID).ToList(), loadOptions));
                    return s;
                }
                else
                {
                    var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_CoursesStudent.Where(n => n.CampaignSaleID == campid).OrderByDescending(n => n.ID).ToList(), loadOptions));
                    return s;
                }
            }
            else
            {
                var s = Request.CreateResponse(DataSourceLoader.Load(l.tbl_CoursesStudent.OrderByDescending(n => n.ID).ToList(), loadOptions));
                return s;
            }
        }

        //Nhận đơn hàng từ website
        [HttpPost]
        public Task<DataAPI> InsertCourseCustomer()
        {
            DataAPI rs = new DataAPI();
            string token = HttpContext.Current.Request.Form["token"];
            string id = HttpContext.Current.Request.Form["id"]; //Khóa học
            string type = HttpContext.Current.Request.Form["type"]; //0: là khóa học,cho vào học luôn. 1: lớp học mong muốn,cho vào hẹn đăng ký
            string fullname = HttpContext.Current.Request.Form["fullname"];
            string phone = HttpContext.Current.Request.Form["phone"];
            string email = HttpContext.Current.Request.Form["email"];
            string price = HttpContext.Current.Request.Form["price"];

            if (token == "MoNaVOQG0zODlNb25hRGV2NTDDEV")
            {
                if (type.ToInt() == 0)
                {
                    var c = CoursesTable.getbyid(id.ToInt(0));
                    //var ac = AccountTable.InsertCus(fullname, email, phone);
                    if (c != null)
                    {
                        //CoursesStudentTable.insert(ac, c, "", price.ToInt(0), 0, 0, "", 0, "", "", 0, false, "", "", 0, 0, 0);
                        rs.Code = 1;
                        rs.Message = "Thành công";
                    }
                    else
                    {
                        rs.Code = 2;
                        rs.Message = "Không tìm thấy khóa học";
                    }
                }
                else if (type.ToInt() == 1)
                {
                    var lop = ClassTable.getbyID(id.ToInt(0));
                    //var ac = AccountTable.InsertCus(fullname, email, phone);
                    if (lop != null)
                    {
                        //StudentAppointmentTable.insert(ac, null, null, null, null, 0, "", 0, "", "", false, 0, "", "");
                        //đang bị lỗi thiếu mấy chỗ để null
                        rs.Code = 1;
                        rs.Message = "Thành công";
                    }
                    else
                    {
                        rs.Code = 2;
                        rs.Message = "Không tìm thấy lớp học";
                    }
                }
            }
            else
            {
                rs.Code = 2;
                rs.Message = "Lỗi Token";
            }
            return Task.FromResult(rs);
        }
    }
}
