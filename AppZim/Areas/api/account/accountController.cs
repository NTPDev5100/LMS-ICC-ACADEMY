using AppZim.api;
using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;

namespace AppZim.API.Account
{
    [Route("api/account/{action}", Name = "apiaccount")]
    public class AccountController : ApiController
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
        [HttpPost]
        public HttpResponseMessage login()
        {
            var username = HttpContext.Current.Request.Form.Get("username");
            var password = HttpContext.Current.Request.Form.Get("password");
            var aclog = AccountTable.login(username, password);
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.WrongUserNameOrPass });
            if (aclog.RoleID != 4 && aclog.RoleID != 5 && aclog.RoleID != 14)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });
            try
            {
                var data = new AccountDetail();
                data.id = aclog.ID;
                data.userName = aclog.UserName;
                if (aclog.LocationID != null)
                {
                    data.cityID = aclog.LocationID.Value;
                    data.cityName = aclog.LocationName;
                }
                if (aclog.DistrictID != null)
                {
                    data.districtID = aclog.DistrictID.Value;
                    data.districtName = aclog.DistrictName;
                }
                if (aclog.WardID != null)
                {
                    data.wardID = aclog.WardID.Value;
                    data.wardname = aclog.WardName;
                }
                if (aclog.StreetID != null)
                {
                    data.streetID = aclog.StreetID.Value;
                    data.streetName = aclog.StreetName;
                }
                data.homeNumber = aclog.HomeNumber;
                data.address = aclog.Address;
                data.fullName = aclog.FullName;
                data.phoneNumber = aclog.Phone;
                data.email = aclog.Email;
                if (aclog.Birthday != null)
                    data.birthDay = aclog.Birthday.Value.ToString("dd/MM/yyyy");
                if (!string.IsNullOrEmpty(aclog.IdentityCard))
                {
                    data.identityCard = aclog.IdentityCard;
                    data.identityCardCityID = aclog.IdentityCardCityID.Value;
                    data.identityCardCityName = aclog.IdentityCardCityName;
                    if (aclog.IdentityCardDate != null)
                        data.identityCardDate = aclog.IdentityCardDate.Value.ToString("dd/MM/yyyy");
                }

                data.avatar = aclog.Avatar;
                if (aclog.JobID != null)
                {
                    data.jobID = aclog.JobID.Value;
                    data.job = aclog.Job;
                }

                data.workPlace = aclog.WorkPlace;
                if (aclog.AcademicPurposesID != null)
                {
                    data.academicPurposesID = aclog.AcademicPurposesID.Value;
                    data.academicPurposes = aclog.AcademicPurposes;
                }
                data.cityList = LocationTable.getall();
                data.jobList = JobTable.getall();
                data.academicPurposesList = AcademicPurposesTable.getall();
                string token = Guid.NewGuid().ToString();
                data.token = token;
                data.roleID = aclog.RoleID.Value;
                AccountTable.UpdateToken(aclog.ID, token);

                return Request.CreateResponse(HttpStatusCode.OK, new { data = data, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage logout()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            AccountTable.Logout(aclog.ID);
            return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
        }
        [HttpPost]
        public HttpResponseMessage forgotPassword()
        {
            try
            {
                var email = HttpContext.Current.Request.Form.Get("email");
                var ac = AccountTable.getbyemail(email);
                if (ac == null)
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.NotFoundInformation });

                var code = AssetCRM.RandomStringWithText(50);
                AccountTable.setkeyforgotpass(ac.ID, code);
                string encodecode = Encryptor.EncryptURL(code);

                string strUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority);
                string link = "" + strUrl + "/set-password/?user=" + ac.UserName + "&code=" + encodecode;

                AssetCRM.SendMail(ac.Email, "Reset your password", "<a href=\"" + link + "\" target=\"_blank\">Reset your password</a>");
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.SentLink });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage changePassword()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            string oldPass = Encryptor.Encrypt(HttpContext.Current.Request.Form.Get("oldPass"));
            string newPass = HttpContext.Current.Request.Form.Get("newPass");
            if (oldPass == aclog.Password)
            {
                try
                {
                    var change = AccountTable.setpassnewforgotpass(aclog.ID, newPass);
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.WrongPassword });
            }
        }
        [HttpGet]
        public HttpResponseMessage getProfile()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var data = new AccountDetail();
                data.id = aclog.ID;
                data.userName = aclog.UserName;
                if (aclog.LocationID != null)
                    data.cityID = aclog.LocationID.Value;
                data.cityName = aclog.LocationName;
                if (aclog.DistrictID != null)
                    data.districtID = aclog.DistrictID.Value;
                data.districtName = aclog.DistrictName;
                if (aclog.WardID != null)
                    data.wardID = aclog.WardID.Value;
                data.wardname = aclog.WardName;
                if (aclog.StreetID != null)
                    data.streetID = aclog.StreetID.Value;
                data.streetName = aclog.StreetName;
                data.homeNumber = aclog.HomeNumber;
                data.address = aclog.Address;
                data.fullName = aclog.FullName;
                data.phoneNumber = aclog.Phone;
                data.email = aclog.Email;
                if (aclog.Birthday != null)
                    data.birthDay = aclog.Birthday.Value.ToString("dd/MM/yyyy");
                data.identityCard = aclog.IdentityCard;
                if (aclog.IdentityCardCityID != null)
                    data.identityCardCityID = aclog.IdentityCardCityID.Value;
                data.identityCardCityName = aclog.IdentityCardCityName;
                if (aclog.IdentityCardDate != null)
                    data.identityCardDate = aclog.IdentityCardDate.Value.ToString("dd/MM/yyyy");
                data.avatar = aclog.Avatar;
                if (aclog.JobID != null)
                    data.jobID = aclog.JobID.Value;
                data.job = aclog.Job;
                data.workPlace = aclog.WorkPlace;
                if (aclog.AcademicPurposesID != null)
                    data.academicPurposesID = aclog.AcademicPurposesID.Value;
                data.academicPurposes = aclog.AcademicPurposes;
                data.cityList = LocationTable.getall();
                data.jobList = JobTable.getall();
                data.academicPurposesList = AcademicPurposesTable.getall();
                data.token = aclog.Token;
                data.roleID = aclog.RoleID.Value;
                return Request.CreateResponse(HttpStatusCode.OK, new { data = data, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage updateProfile()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                int cityID = HttpContext.Current.Request.Form.Get("cityID").ToInt(0);
                var cityName = HttpContext.Current.Request.Form.Get("cityName");
                int districtID = HttpContext.Current.Request.Form.Get("districtID").ToInt(0);
                var districtName = HttpContext.Current.Request.Form.Get("districtName");
                int wardID = HttpContext.Current.Request.Form.Get("wardID").ToInt(0);
                var wardname = HttpContext.Current.Request.Form.Get("wardname");
                int streetID = HttpContext.Current.Request.Form.Get("streetID").ToInt(0);
                var streetName = HttpContext.Current.Request.Form.Get("streetName");
                var homeNumber = HttpContext.Current.Request.Form.Get("homeNumber");
                var address = HttpContext.Current.Request.Form.Get("address");
                var fullName = HttpContext.Current.Request.Form.Get("fullName");
                var phoneNumber = HttpContext.Current.Request.Form.Get("phoneNumber");
                var email = HttpContext.Current.Request.Form.Get("email");
                var birthDay = HttpContext.Current.Request.Form.Get("birthDay");
                var identityCard = HttpContext.Current.Request.Form.Get("identityCard");
                var identityCardCityID = HttpContext.Current.Request.Form.Get("identityCardCityID").ToInt(0);
                var identityCardCityName = HttpContext.Current.Request.Form.Get("identityCardCityName");
                var identityCardDate = HttpContext.Current.Request.Form.Get("identityCardDate");
                var avatar = HttpContext.Current.Request.Form.Get("avatar");
                var jobID = HttpContext.Current.Request.Form.Get("jobID").ToInt(0);
                var job = HttpContext.Current.Request.Form.Get("job");
                var workPlace = HttpContext.Current.Request.Form.Get("workPlace");
                var academicPurposesID = HttpContext.Current.Request.Form.Get("academicPurposesID").ToInt(0);
                var academicPurposes = HttpContext.Current.Request.Form.Get("academicPurposes");

                AccountTable.UpdateProfile(aclog.ID, cityID, cityName, districtID, districtName,
                    wardID, wardname, streetID, streetName, homeNumber, address, fullName,
                    phoneNumber, email, birthDay, identityCard, identityCardCityID, identityCardCityName,
                    identityCardDate, avatar, jobID, job, workPlace, academicPurposesID,
                    academicPurposes, aclog.UserName);
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage uploadImage()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                string link = "";
                var httpContext = HttpContext.Current;
                var file = httpContext.Request.Files.Get("file");
                if (file != null)
                {
                    WebImage img = new WebImage(file.InputStream);
                    if (img.Width > 1000)
                        img.Resize(1000, 1000);

                    string ext = Path.GetExtension(file.FileName).ToLower();
                    string fileName = Guid.NewGuid() + ext; // getting File Name
                    string fileExtension = Path.GetExtension(fileName).ToLower();
                    var result = AssetCRM.isValidFileCustom(ext); // Validate Header
                    if (result)
                    {
                        fileName = Guid.NewGuid() + ext;
                        var path = Path.Combine(httpContext.Server.MapPath("~/Upload/useravatar/"), fileName);
                        string strPathAndQuery = httpContext.Request.Url.PathAndQuery;
                        string strUrl = httpContext.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                        link = "Upload/useravatar/" + fileName;
                        img.Save(path);
                        return Request.CreateResponse(HttpStatusCode.OK, new { data = link, message = ApiMessage.Success });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.InvalidFile });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.InvalidFile });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getDistrict(int cityID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var districtList = DistrictTable.getbycityid(cityID);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = districtList, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getWard(int districtID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var wardList = WardTable.getbydistrict(districtID);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = wardList, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getStreet(int districtID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var streetList = StreetTable.getbydistrict(districtID);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = streetList, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getStudentOfParent()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 14)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var studentList = new List<Student>();
                var list = AccountTable.GetStudentByParentID(aclog.ID);
                foreach (var i in list)
                {
                    var student = new Student();
                    student.ID = i.ID;
                    student.UserName = i.UserName;
                    student.FullName = i.FullName;
                    student.Email = i.Email;
                    student.StatusName = i.StatusName;
                    student.CreatedDate = i.CreatedDate.Value.ToString("dd/MM/yyyy");
                    studentList.Add(student);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = studentList, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage SaveIncludePlayerID()
        {
            try
            {
                int accountID = HttpContext.Current.Request.Form.Get("AccountID").ToInt(0);
                string includePlayerID = HttpContext.Current.Request.Form.Get("IncludePlayerID");
                var user = AccountTable.getbyID(accountID);
                if (user != null)
                {
                    DeviceMobileTable.AddIncludePlayerID(accountID, includePlayerID);
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
