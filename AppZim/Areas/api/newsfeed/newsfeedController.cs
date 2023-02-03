using AppZim.api;
using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using Microsoft.Ajax.Utilities;
using StackExchange.DataExplorer.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;

namespace AppZim.Areas.api.newsfeed
{
    public class newsfeedController : ApiController
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
        public HttpResponseMessage getList(int groupID, string search = "")
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                List<tbl_PostContent> l = new List<tbl_PostContent>();
                //Lấy danh sách bài viết
                if (search == null)
                    search = "";
                if (groupID == 0)//đang ở trang chủ
                {
                    if (aclog.RoleID == 5)//nếu là học viên
                    {
                        l = PostContentTable.getbyall().Where(n => (n.Status == 3 && n.isHideStudent != true)).ToList();
                    }
                    else
                    {
                        //nếu là role khác, lấy những bài mà nó có thể lấy nhưng ko thuộc group
                        l = PostContentTable.getbyall().Where(n => (n.Status == 3 && n.isHideStaff != true)).ToList();
                    }
                    //Lấy avaiableGroup 
                    List<tbl_Group> lgroup = GroupTable.getAvailableGroup(aclog.ID);
                    foreach (var item in lgroup)
                    {
                        //ds  bài của group này
                        List<tbl_PostContent> lp = PostContentTable.GetByGroupID(item.ID).Where(n => n.Status != 4).ToList();
                        l.AddRange(lp);
                    }
                }
                else
                {
                    //check available group
                    tbl_Group gr = GroupTable.getbyid(groupID);
                    if (gr == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.NotFoundGroup });
                    }
                    var check = GroupTable.getAvailableGroup(aclog.ID).Where(n => n.ID == gr.ID).FirstOrDefault();
                    if (check == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });
                    }
                    List<tbl_PostContent> lp = PostContentTable.GetByGroupID(gr.ID).Where(n => n.Status != 4).ToList();
                    l = lp;
                }
                if (search.IndexOf("@") == 0)
                {
                    string searchsb = search.Substring(1, search.Length - 1);
                    IEnumerable<tbl_Account> staff = AccountTable.getall().ToList();
                    l = (from s in l
                         join a in staff on s.CreatedBy equals a.UserName
                         where AssetCRM.RemoveUnicode(a.FullName).ToUpper().Contains(AssetCRM.RemoveUnicode(searchsb).ToUpper())
                         select s).ToList();
                }
                else
                {
                    l = (from s in l
                         where AssetCRM.RemoveUnicode(s.TitlePost).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                         select s).ToList();
                }
                l = l.DistinctBy(x => x.ID).OrderByDescending(n => n.ID).ToList();
                List<PostNewsFeed> post = new List<PostNewsFeed>();
                foreach (var i in l)
                {
                    var p = new PostNewsFeed();
                    var create = AccountTable.getbyusername(i.CreatedBy);
                    p.ID = i.ID;
                    p.Title = i.TitlePost;
                    p.IMG = i.PostIMG;
                    p.Content = i.ContentPost;
                    p.CreatedBy = i.CreatedBy;
                    if (create != null)
                    {
                        p.CreatedName = create.FullName;
                        p.Avatar = create.Avatar;
                    }
                    p.CreatedDate = i.CreatedDate.Value;
                    post.Add(p);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = post, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage create()
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
                var id = HttpContext.Current.Request.Form.Get("id").ToInt(0);
                string link = HttpContext.Current.Request.Form.Get("link");
                var fileType = HttpContext.Current.Request.Form.Get("fileType").ToInt(0);
                var postType = HttpContext.Current.Request.Form.Get("postType").ToInt(0);
                var groupID = HttpContext.Current.Request.Form.Get("groupID").ToInt(0);
                var title = HttpContext.Current.Request.Form.Get("title");
                var content = HttpContext.Current.Request.Form.Get("content");
                var background = HttpContext.Current.Request.Form.Get("background").ToInt(0);
                string linkimg = link;
                string linkimgthumb = link;
                int filetype = fileType;//1 imgage, 2 video
                int ptype = postType;
                int groupid = groupID;
                if (ptype == 0)
                {
                    groupid = 0;
                    if (id == 0)
                        PostContentTable.insert(title, linkimg, linkimgthumb, content, aclog.UserName, false, false, groupid, filetype);
                    else
                    {
                        if (aclog.RoleID != 1 && aclog.RoleID != 2)
                        {
                            var po = PostContentTable.getbyid(id);
                            if (aclog.UserName == po.CreatedBy && po.Status != 3)
                                PostContentTable.update(id, title, linkimg, linkimgthumb, content, aclog.UserName);
                        }
                        else
                            PostContentTable.update(id, title, linkimg, linkimgthumb, content, aclog.UserName);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                }
                else
                {
                    var bg = background;
                    bool bgt = false;
                    if (bg != 0)
                    {
                        bgt = true;
                        if (bg == 1)
                            linkimg = "/app-assets/zimv2/assets/img/bg-status1.jpg";
                        if (bg == 2)
                            linkimg = "/app-assets/zimv2/assets/img/bg-status2.jpg";
                        if (bg == 3)
                            linkimg = "/app-assets/zimv2/assets/img/bg-status3.jpg";
                        if (bg == 4)
                            linkimg = "/app-assets/zimv2/assets/img/bg-status4.jpg";
                        if (bg == 5)
                            linkimg = "/app-assets/zimv2/assets/img/bg-status5.jpg";
                        if (bg == 6)
                            linkimg = "/app-assets/zimv2/assets/img/bg-status6.jpg";
                        if (bg == 7)
                            linkimg = "/app-assets/zimv2/assets/img/bg-status7.jpg";
                        if (bg == 8)
                            linkimg = "/app-assets/zimv2/assets/img/bg-status8.jpg";
                        if (bg == 9)
                            linkimg = "/app-assets/zimv2/assets/img/bg-status9.jpg";
                        if (bg == 10)
                            linkimg = "/app-assets/zimv2/assets/img/bg-status10.jpg";
                        if (bg == 11)
                            linkimg = "/app-assets/zimv2/assets/img/bg-status11.jpg";
                        if (bg == 12)
                            linkimg = "/app-assets/zimv2/assets/img/bg-status12.jpg";
                        if (bg == 13)
                            linkimg = "/app-assets/zimv2/assets/img/bg-status13.jpg";
                        if (bg == 14)
                            linkimg = "/app-assets/zimv2/assets/img/bg-status14.jpg";
                        if (bg == 15)
                            linkimg = "/app-assets/zimv2/assets/img/bg-status15.jpg";
                        filetype = 1;
                        linkimgthumb = linkimg;
                    }
                    tbl_Group gr = GroupTable.getbyid(groupid);
                    if (gr != null)
                    {
                        var member = MemeberTable.getbymenberid(aclog.ID, gr.ID);

                        if (aclog.ID != 1)
                        {
                            if (member != null)
                            {
                                if (member.Status == 2)
                                {
                                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });
                                }
                            }
                            else
                            {
                                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 4 && aclog.UserName != gr.CreatedBy)
                                {
                                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });
                                }
                            }
                        }
                    }
                    if (id == 0)
                    {
                        var p = PostContentTable.insert(title, linkimg, linkimgthumb, content, aclog.UserName, true, bgt, groupid, filetype);
                        PostContentTable.updatestatus(p.ID, 3, aclog.UserName);
                    }
                    else
                    {
                        var p = PostContentTable.getbyid(id);
                        if (p != null)
                        {
                            if (p.CreatedBy == aclog.UserName)
                                PostContentTable.update(p.ID, title, linkimg, linkimgthumb, content, aclog.UserName, true, bgt);
                        }
                    }

                    if (groupid != 0)
                    {
                        SendEmailGroupTable.insert(4, groupid, aclog.ID, 0);
                        Thread sendmail = new Thread(Admin.Controllers.AccountController.SendNotiGroup);
                        sendmail.Start();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                }
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
                if (httpContext.Request.Files.Count > 0)
                {
                    var file = httpContext.Request.Files[0];
                    if (file != null)
                    {
                        string ext = Path.GetExtension(file.FileName).ToLower();
                        string fileName = Guid.NewGuid() + ext; // getting File Name
                        string fileExtension = Path.GetExtension(fileName).ToLower();
                        var result = AssetCRM.isValidImageAndVideo(ext); // Validate Header
                        if (result)
                        {
                            if (ext == ".mp3" || ext == ".mp4" || ext == ".flv" || ext == ".mpeg" || ext == ".mov")
                            {
                                fileName = Guid.NewGuid() + ".mp4";
                                var path = Path.Combine(httpContext.Server.MapPath("~/Upload/newfeed/"), fileName);
                                string strPathAndQuery = httpContext.Request.Url.PathAndQuery;
                                string strUrl = httpContext.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                                link = strUrl + "Upload/newfeed/" + fileName;
                                file.SaveAs(path);
                            }
                            else if (ext == ".jpg" || ext == ".png" || ext == ".jpeg" || ext == ".bmp")
                            {
                                WebImage img = new WebImage(file.InputStream);
                                if (img.Width > 1000)
                                    img.Resize(1000, 1000);

                                fileName = Guid.NewGuid() + ext;
                                var path = Path.Combine(httpContext.Server.MapPath("~/Upload/newfeed/"), fileName);
                                string strPathAndQuery = httpContext.Request.Url.PathAndQuery;
                                string strUrl = httpContext.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                                link = strUrl + "Upload/newfeed/" + fileName;
                                img.Save(path);
                            }
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
        [HttpPost]
        public HttpResponseMessage deletePost()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                int id = HttpContext.Current.Request.Form.Get("id").ToInt(0);

                if (aclog.RoleID == 1)
                {
                    PostContentTable.deletestatus(id, aclog.UserName);
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                }
                else
                {
                    var p = PostContentTable.getbyid(id);
                    if (p.CreatedBy == aclog.UserName)
                    {
                        PostContentTable.deletestatus(id, aclog.UserName);
                        return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                    }
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage loadComment(int postID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var listComment = PostCommentTable.getbypostid(postID);
                var list = new List<LoadComment>();
                foreach (var i in listComment)
                {
                    var comment = new LoadComment();
                    var user = AccountTable.getbyusername(i.CreatedBy);
                    comment.CommentID = i.ID;
                    comment.PostCommentID = i.PostCommentID.Value;
                    comment.PostContentID = i.PostContentID.Value;
                    comment.UID = i.UID.Value;
                    comment.CMT = i.CMT;
                    comment.CreatedDate = i.CreatedDate.Value;
                    if (user != null)
                    {
                        comment.CreatedBy = user.FullName;
                        comment.Avatar = user.Avatar;
                    }
                    list.Add(comment);
                }
                list = list.OrderByDescending(x => x.CreatedDate).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { data = list, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage comment()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                int postContentID = HttpContext.Current.Request.Form.Get("postContentID").ToInt(0);
                int postCommentID = HttpContext.Current.Request.Form.Get("postCommentID").ToInt(0);
                string comment = HttpContext.Current.Request.Form.Get("comment");

                tbl_PostContent po = new tbl_PostContent();
                if (postContentID != 0)// PostContentID = 0 là reply comment
                    po = PostContentTable.getbyid(postContentID);
                else
                {
                    var cmt = PostCommentTable.getbyid(postCommentID);
                    po = PostContentTable.getbyid(cmt.PostContentID.Value);
                }
                var p = PostCommentTable.insert(postContentID, postCommentID, aclog.ID, comment, aclog.UserName);

                return Request.CreateResponse(HttpStatusCode.OK, new { data = p, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getStatiscial(string fdate, string tdate)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                // thời gian bắt đầu từ
                string fromdate = null;
                if (!string.IsNullOrEmpty(fdate))
                {
                    try
                    {
                        DateTime fd = DateTime.ParseExact(fdate, "dd/MM/yyyy", null);
                        fromdate = fd.ToString("yyyy-MM-dd 00:00:00.000");
                    }
                    catch { }
                }
                string todate = null;
                // đến
                if (!string.IsNullOrEmpty(tdate))
                {
                    try
                    {
                        DateTime td = DateTime.ParseExact(tdate, "dd/MM/yyyy", null);
                        todate = td.ToString("yyyy-MM-dd 23:59:59.998");
                    }
                    catch { }
                }
                if (aclog.RoleID == 5)
                {
                    Statiscial sa = new Statiscial();
                    

                    var dashboard = new API_Student_Dashboard_Result();
                    using (var db = new ZimEntities())
                    {
                        dashboard = db.API_Student_Dashboard(aclog.ID, fromdate, todate).FirstOrDefault();
                    }
                    sa.value1 = dashboard.NumberOfLessson;
                    sa.value2 = dashboard.NumberOfCourse;
                    sa.value3 = dashboard.NumberOfFeedback;
                    sa.value4 = dashboard.NumberOfFeedbackDone;
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = sa, message = ApiMessage.Success });
                }
                else
                {
                    Statiscial sa = new Statiscial();
                    var dashboard = new API_Teacher_Dashboard_Result();
                    using (var db = new ZimEntities())
                    {
                        dashboard = db.API_Teacher_Dashboard(aclog.ID, fromdate, todate).FirstOrDefault();
                    }
                    sa.value1 = dashboard.NumberOfLessson;
                    sa.value2 = dashboard.NumberOfCourse;
                    sa.value3 = dashboard.NumberOfFeedback;
                    sa.value4 = dashboard.UnUploadVideo;
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = sa, message = ApiMessage.Success });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getTop5News()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                List<tbl_PostContent> l = new List<tbl_PostContent>();
                if (aclog.RoleID == 5)//nếu là học viên
                {
                    l = PostContentTable.getbyall().Where(n => (n.Status == 3 && n.isHideStudent != true)).ToList();
                }
                else
                {
                    //nếu là role khác, lấy những bài mà nó có thể lấy nhưng ko thuộc group
                    l = PostContentTable.getbyall().Where(n => (n.Status == 3 && n.isHideStaff != true)).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getSlogan()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var slogan = IdiomsTable.getone().Idioms;
                return Request.CreateResponse(HttpStatusCode.OK, new { data = slogan, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getGroupList()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var groupList = GroupTable.getAvailableGroup(aclog.ID);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = groupList, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getGroupInformation(int groupID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var group = GroupTable.getbyid(groupID);
                if (group == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundGroup });

                List<int> listmember = GroupTable.GetGroupMember(group.ID);
                var listPost = PostContentTable.GetByGroupID(group.ID);
                int countcomment = 0;
                foreach (var item in listPost)
                {
                    var cm = PostCommentTable.getbypostid(item.ID);
                    countcomment += cm.Count;
                }
                var list = new List<SortAccount>();
                foreach (var i in listmember)
                {
                    var acc = AccountTable.getbyID(i);
                    var account = new SortAccount();
                    account.ID = acc.ID;
                    account.FullName = acc.FullName;
                    account.Avatar = acc.Avatar;
                    account.RoleName = acc.RoleName;
                    list.Add(account);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { totalPost = listPost.Count, totalcomment = countcomment, totalmember = listmember.Count, list, data = group, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage removeStudentFromGroup(int studentID, int groupID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                tbl_Group G = GroupTable.getbyid(groupID);
                tbl_Account acmember = AccountTable.getbyID(studentID);
                if (acmember.ID == 1)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });
                }
                var member = MemeberTable.insert(studentID, groupID, 2, aclog.UserName);
                if (member != null)
                {
                    SendEmailGroupTable.insert(3, G.ID, aclog.ID, member.MemberID.Value);
                    Thread sendmail = new Thread(SendNotiGroup);
                    sendmail.Start();
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage addStudentToGroup(int studentID, int groupID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                tbl_Group G = GroupTable.getbyid(groupID);
                var member = MemeberTable.insert(studentID, groupID, 1, aclog.UserName);
                if (member != null)
                {
                    SendEmailGroupTable.insert(2, G.ID, aclog.ID, member.MemberID.Value);
                    Thread sendmail = new Thread(SendNotiGroup);
                    sendmail.Start();
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage searchStudent(string search, int groupid)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                if (search == null)
                    search = "";
                var list = new List<API_Teacher_SearchStudent_Result>();
                using (var db = new ZimEntities())
                {
                    list = db.API_Teacher_SearchStudent(search, groupid).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = list, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        /// <summary>
        /// Gửi noti và email cho hoạt động của group
        /// </summary>
        /// <param name="type">1 là khởi tạo thành công,2 được thêm vào nhóm, 3 được xóa khỏi nhóm,4 có bài viết mới</param>
        /// <param name="groupId">ID nhóm, để lấy tên,id nhóm push noti</param>
        /// <param name="aclogId">ID user => trừ người này raaa</param>
        /// <param name="userTarget">Nếu !=0 thì target chỉ định, khác 0, tất cả người có khả năng</param>
        public static void SendNotiGroup()
        {

            List<tbl_SendEmailGroup> l = SendEmailGroupTable.getallundone();
            foreach (var item1 in l)
            {
                try
                {
                    int type = item1.Type.Value;
                    int groupId = item1.GroupId.Value;
                    int aclogId = item1.AclogId.Value;
                    //xác định đây là group cho ai - Trung tâm hay Khóa\
                    tbl_Group group = GroupTable.getbyid(groupId);
                    //xác định người tạo group là ai
                    tbl_Account owner = AccountTable.getbyID(aclogId);
                    if (type == 1)
                    {
                        //Lấy danh sách người cần gửi thông báo
                        //nếu là group của trung tâm
                        //gửi cho quản lý 
                        List<tbl_Account> lquanly = AccountTable.getbyrole(2).Where(n => n.SchoolID.Value == group.CenterID.Value && n.Status == 2 && n.ID != owner.ID).ToList();
                        foreach (var item in lquanly)
                        {
                            try
                            {
                                var nt = NotificationTable.insertsingle("", "Thêm vào nhóm", "[ " + group.GroupName + " ] > .", item.ID, owner.UserName, 5, group.ID);
                                if (!string.IsNullOrEmpty(item.Email))
                                {
                                    AssetCRM.SendMailAmazone(item.Email, "| " + item.FullName + ", bạn có thông báo mới", "Bạn đã được thêm vào group [" + group.GroupName + "]" );
                                }
                            }
                            catch { }
                        }
                        if (group.CourseID.Value != 0)
                        {
                            List<tbl_CoursesStudent> listStudent = CoursesStudentTable.getListStudentInCourse(group.CourseID.Value);
                            foreach (var student in listStudent)
                            {
                                try
                                {
                                    tbl_Account item = AccountTable.getbyID(student.StudentUID.Value);
                                    if (item != null)
                                    {
                                        
                                        var nt = NotificationTable.insertsingle("", "Thêm vào nhóm", "[ " + group.GroupName + " ] > .", item.ID, owner.UserName, 5, group.ID);
                                        
                                        if (!string.IsNullOrEmpty(item.Email))
                                        {
                                            AssetCRM.SendMailAmazone(item.Email, "| " + item.FullName + ", bạn có thông báo mới", "Bạn đã được thêm vào group [" + group.GroupName + "]" );
                                        }
                                    }

                                }
                                catch { }
                            }
                            List<tbl_TeacherCourse> listTeacher = TeacherCourseTable.getbycourseid(group.CourseID.Value).Where(n => n.isHide != true).ToList();
                            foreach (var student in listTeacher)
                            {
                                try
                                {
                                    tbl_Account item = AccountTable.getbyID(student.TeacherUID.Value);
                                    if (item != null)
                                    {
                                        
                                        var nt = NotificationTable.insertsingle("", "Thêm vào nhóm", "[ " + group.GroupName + " ] > .", item.ID, owner.UserName, 5, group.ID);
                                        
                                        if (!string.IsNullOrEmpty(item.Email))
                                        {
                                            AssetCRM.SendMailAmazone(item.Email, "| " + item.FullName + ", bạn có thông báo mới", "Bạn đã được thêm vào group [" + group.GroupName + "]" );
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    else if (type == 2)
                    {
                        //Lấy danh sách người cần gửi thông báo
                        //nếu là group của trung tâm
                        try
                        {
                            tbl_Account sendto = AccountTable.getbyID(item1.TargetId.Value);
                            
                            var nt = NotificationTable.insertsingle("", "Thêm vào nhóm", "[ " + group.GroupName + " ] > .", sendto.ID, owner.UserName, 5, group.ID);
                            
                            if (!string.IsNullOrEmpty(sendto.Email))
                            {
                                AssetCRM.SendMailAmazone(sendto.Email, "| " + sendto.FullName + ", bạn có thông báo mới", "Bạn đã được thêm vào group [" + group.GroupName + "]" );
                            }
                        }
                        catch
                        {

                        }
                    }
                    else if (type == 3)
                    {
                        try
                        {
                            tbl_Account sendto = AccountTable.getbyID(item1.TargetId.Value);

                            var nt = NotificationTable.insertsingle("", "Bạn đã được yêu cầu khỏi group", "[ " + group.GroupName + " ]", sendto.ID, owner.UserName, 0, 0);
                            if (!string.IsNullOrEmpty(sendto.Email))
                            {
                                AssetCRM.SendMailAmazone(sendto.Email, "| " + sendto.FullName + ", bạn có thông báo mới", "Bạn đã được yêu cầu rời khỏi group." );
                            }
                        }
                        catch
                        {

                        }
                    }
                    else if (type == 4)
                    {
                        //Lấy người trong group, trừ admin
                        List<int> listMember = GroupTable.GetGroupMember(group.ID);
                        foreach (var item in listMember)
                        {
                            try
                            {
                                tbl_Account sendto = AccountTable.getbyID(item);
                                
                                var nt = NotificationTable.insertsingle("", "Có bài viết mới ở group mà bạn tham gia", "[ " + group.GroupName + " ] .", sendto.ID, owner.UserName, 5, group.ID);
                                
                                if (!string.IsNullOrEmpty(sendto.Email))
                                {
                                    AssetCRM.SendMailAmazone(sendto.Email, "| " + sendto.FullName + ", bạn có thông báo mới", "Có bài viết mới ở group mà bạn tham gia." );
                                }
                            }
                            catch { }
                        }

                    }
                    else if (type == 5)//giải tán
                    {
                        //Lấy người trong group, trừ admin
                        List<int> listMember = GroupTable.GetGroupMember(group.ID);
                        foreach (var item in listMember)
                        {
                            try
                            {
                                tbl_Account sendto = AccountTable.getbyID(item);
                                var nt = NotificationTable.insertsingle("", "Nhóm của bạn đã bị giải tán", "[ " + group.GroupName + " ] ", sendto.ID, owner.UserName, 0, 0);
                                if (!string.IsNullOrEmpty(sendto.Email))
                                {
                                    AssetCRM.SendMailAmazone(sendto.Email, "| " + sendto.FullName + ", bạn có thông báo mới", "Nhóm của bạn đã bị giải tán" );
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch
                {

                }
                SendEmailGroupTable.sendDone(item1.ID);
            }
        }
    }
}
