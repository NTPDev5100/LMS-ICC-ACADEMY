using AppZim.Controllers;
using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AppZim.Areas.Admin.Controllers
{

    public class AccountController : BaseController
    {

        // GET: Admin/Account
        [HttpGet]
        public ActionResult ProfileUser(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                List<tbl_PostContent> l = new List<tbl_PostContent>();
                //Lấy danh sách bài viết

                int groupID = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["group"]))
                {
                    groupID = Request.QueryString["group"].ToInt(0);
                }
                if (groupID == 0)//đang ở trang chủ
                {
                    if (aclog.RoleID == 1 || aclog.RoleID == 2)// lấy tất cả, trừ bài bị reject và bị ẩn đi
                    {
                        //lấy tất cả bài post hoặc bài status
                        l = PostContentTable.getbyall().Where(n => ((n.Status == 3 || n.StatusPost == true) && n.isHideStaff != true)).ToList();
                    }
                    else if (aclog.RoleID == 5)//nếu là học viên
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
                        SetAlert("Không tìm thấy nhóm", "w");
                        return RedirectToAction("Signin", "Login", new { area = "" });
                    }
                    var check = GroupTable.getAvailableGroup(aclog.ID).Where(n => n.ID == gr.ID).FirstOrDefault();
                    if (check == null)
                    {
                        SetAlert("Access is denied", "w");
                        return Redirect(Request.UrlReferrer.AbsoluteUri);
                    }
                    List<tbl_PostContent> lp = PostContentTable.GetByGroupID(gr.ID).Where(n => n.Status != 4).ToList();
                    l = lp;
                    ViewBag.Group = gr;
                }
                ViewBag.GroupID = groupID;
                int pageSize = 20;
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                {
                    ViewBag.Search = Request.QueryString["search"];
                    string search = Request.QueryString["search"].ToString();
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
                }
                l = l.OrderByDescending(n => n.ID).ToList();
                return View(l.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "ProfileUser", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }


        [HttpPost]
        public ActionResult LoadMemeber(string groupID, int PageIndex, string searchName, int PageSize)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

                int group = groupID.ToInt(0);
                if (group != 0)
                {
                    tbl_Group gr = GroupTable.getbyid(group);
                    var lr = gr.ListRole.Split(',').ToList();
                    //Nếu là tìm kiếm, thì
                    if (!string.IsNullOrEmpty(searchName))
                    {
                        //Tìm trong tl những thằng có tên, tên tài khoản, trừ cái thằng đang login ra
                        string search = searchName.Trim();
                        //Lấy tất cả tài khoản active
                        IEnumerable<tbl_Account> l = AccountTable.getall().Where(n => n.Status == 2 && (AssetCRM.RemoveUnicode(n.FullName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper()) || AssetCRM.RemoveUnicode(n.Phone).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())) && n.ID != aclog.ID).ToList();
                        l = l.OrderBy(n => n.FullName).ToList();
                        if (aclog.RoleID != 1)
                        {
                            l = l.Where(n => n.RoleID.Value != 1).ToList();
                        }
                        else if (aclog.RoleID != 1 && aclog.RoleID != 2)
                        {
                            l = l.Where(n => n.RoleID.Value != 1 && n.RoleID != 2).ToList();
                        }
                        //Lấy theo pageIndex, pageSize
                        l = l.Skip(PageIndex * PageSize).Take(PageSize).ToList();
                        //var dataList = l.ToPagedList(PageIndex, PageSize).ToList();
                        StringBuilder html = new StringBuilder();
                        foreach (var item in l)
                        {
                            html.Append("<div class=\"media blog status-post d-flex align-items-center justify-content-between\">");
                            html.Append("    <div class=\"author\">");
                            if (!string.IsNullOrEmpty(item.AvatarThumbnail))
                                html.Append("        <a href=\"#\" class=\"avatar\"><img src=\"" + item.AvatarThumbnail + "\" onerror=\"this.onerror = null; this.src = '/app-assets/zimv2/images/invis-user.png'; \" alt=\"avatar\"></a>");
                            else
                                html.Append("        <a href=\"#\" class=\"avatar\"><img src=\"/app-assets/zimv2/images/invis-user.png\" alt=\"avatar\"></a>");
                            html.Append("        <div class=\"author-info\">");
                            if (item.RoleID == 1)
                            {
                                html.Append("            <a href=\"#\" class=\"username\"><span class=\"hasVerifiedBadge\">" + item.FullName + "</span></a>");
                            }
                            else
                            {
                                html.Append("            <a href=\"#\" class=\"username\"><span class=\"\">" + item.FullName + "</span></a>");
                            }
                            html.Append("            <div class=\"date-comment-view\">");
                            html.Append("                <span class=\"date\">");
                            html.Append("                    <span class=\"DateTime\" title=\"11:20 ngày 2/10/19\">");
                            html.Append("                        " + item.RoleName + "");
                            html.Append("                    </span>");
                            html.Append("                </span>");
                            html.Append("            </div>");
                            html.Append("        </div>");
                            html.Append("    </div>");
                            if (aclog.RoleID == 1 || aclog.RoleID == 2 || aclog.RoleID == 7 || aclog.UserName == gr.CreatedBy)
                            {
                                html.Append("    <div class=\"action-blog\">");
                                var member = MemeberTable.getbymenberid(item.ID, gr.ID);
                                if (member != null)
                                {
                                    if (member.Status == 2)
                                        html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"add\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-success\" data-toggle=\"tooltip\" data-title=\"Thêm vào group\">Thêm</a>");
                                    else
                                    {
                                        if (aclog.ID == 1)
                                        {
                                            html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"remove\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-danger\" data-toggle=\"tooltip\" data-title=\"Xoá khỏi group\">Xóa</a>");
                                        }
                                        else
                                        {
                                            if (aclog.UserName == gr.CreatedBy)
                                            {
                                                if (aclog.RoleID == 1 && item.ID != 1)
                                                {
                                                    html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"remove\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-danger\" data-toggle=\"tooltip\" data-title=\"Xoá khỏi group\">Xóa</a>");
                                                }
                                                else if (aclog.RoleID == 2)
                                                {
                                                    if (item.RoleID != 1)
                                                    {

                                                        html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"remove\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-danger\" data-toggle=\"tooltip\" data-title=\"Xoá khỏi group\">Xóa</a>");

                                                    }
                                                }
                                                else if (aclog.RoleID == 7)
                                                {
                                                    if (item.RoleID != 1 && item.RoleID != 2)
                                                    {

                                                        html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"remove\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-danger\" data-toggle=\"tooltip\" data-title=\"Xoá khỏi group\">Xóa</a>");

                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (item.UserName != gr.CreatedBy)
                                                {
                                                    if (aclog.RoleID == 1)
                                                    {
                                                        if (item.RoleID != 1 && item.ID != 1)
                                                        {

                                                            html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"remove\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-danger\" data-toggle=\"tooltip\" data-title=\"Xoá khỏi group\">Xóa</a>");

                                                        }
                                                    }
                                                    else if (aclog.RoleID == 2)
                                                    {
                                                        if (item.RoleID != 1 && item.RoleID != 2)
                                                        {

                                                            html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"remove\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-danger\" data-toggle=\"tooltip\" data-title=\"Xoá khỏi group\">Xóa</a>");

                                                        }
                                                    }
                                                    else if (aclog.RoleID == 7)
                                                    {
                                                        if (item.RoleID != 1 && item.RoleID != 2 && item.RoleID != 7)
                                                        {

                                                            html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"remove\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-danger\" data-toggle=\"tooltip\" data-title=\"Xoá khỏi group\">Xóa</a>");

                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"add\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-success\" data-toggle=\"tooltip\" data-title=\"Thêm vào group\">Thêm</a>");
                                }
                                html.Append("    </div>");
                            }

                            html.Append("</div>");
                        }
                        return Json(new { rs = true, data = html.ToString(), PageIndex = PageIndex += 1 }, JsonRequestBehavior.AllowGet);
                    }
                    else//Nếu là xem thêm không có tìm kiếm(Load thêm danh sách)
                    {
                        List<int> l = GroupTable.GetGroupMember(group);
                        //Xóa mày ra,
                        l = l.Where(n => n != aclog.ID).ToList();
                        //Lấy danh sách theo tên
                        // = l.OrderBy(n => n.FullName).ToList();
                        //Lấy theo pageIndex, pageSize
                        l = l.Skip(PageIndex * PageSize).Take(PageSize).ToList();
                        StringBuilder html = new StringBuilder();
                        foreach (var s in l)
                        {
                            var item = AccountTable.getbyID(s);
                            html.Append("<div class=\"media blog status-post d-flex align-items-center justify-content-between\">");
                            html.Append("    <div class=\"author\">");
                            if (!string.IsNullOrEmpty(item.AvatarThumbnail))
                                html.Append("        <a href=\"#\" class=\"avatar\"><img src=\"" + item.AvatarThumbnail + "\" onerror=\"this.onerror = null; this.src = '/app-assets/zimv2/images/invis-user.png'; \" alt=\"avatar\"></a>");
                            else
                                html.Append("        <a href=\"#\" class=\"avatar\"><img src=\"/app-assets/zimv2/images/invis-user.png\"  alt=\"avatar\"></a>");
                            html.Append("        <div class=\"author-info\">");
                            if (item.RoleID == 1)
                            {
                                html.Append("            <a href=\"#\" class=\"username\"><span class=\"hasVerifiedBadge\">" + item.FullName + "</span></a>");
                            }
                            else
                            {
                                html.Append("            <a href=\"#\" class=\"username\"><span class=\"\">" + item.FullName + "</span></a>");
                            }
                            html.Append("            <div class=\"date-comment-view\">");
                            html.Append("                <span class=\"date\">");
                            html.Append("                    <span class=\"DateTime\" title=\"11:20 ngày 2/10/19\">");
                            html.Append("                        " + item.RoleName + "");
                            html.Append("                    </span>");
                            html.Append("                </span>");
                            html.Append("            </div>");
                            html.Append("        </div>");
                            html.Append("    </div>");
                            if (aclog.RoleID == 1 || aclog.RoleID == 2 || aclog.RoleID == 7 || aclog.UserName == gr.CreatedBy)
                            {
                                if (aclog.ID == 1)
                                {
                                    html.Append("    <div class=\"action-blog \">");
                                    html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"remove\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-danger\" data-toggle=\"tooltip\" data-title=\"Xoá khỏi group\">Xóa</a>");
                                    html.Append("    </div>");
                                }
                                else
                                {
                                    if (aclog.UserName == gr.CreatedBy)
                                    {
                                        if (aclog.RoleID == 1 && item.ID != 1)
                                        {
                                            html.Append("    <div class=\"action-blog \">");
                                            html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"remove\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-danger\" data-toggle=\"tooltip\" data-title=\"Xoá khỏi group\">Xóa</a>");
                                            html.Append("    </div>");
                                        }
                                        else if (aclog.RoleID == 2)
                                        {
                                            if (item.RoleID != 1)
                                            {
                                                html.Append("    <div class=\"action-blog \">");
                                                html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"remove\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-danger\" data-toggle=\"tooltip\" data-title=\"Xoá khỏi group\">Xóa</a>");
                                                html.Append("    </div>");
                                            }
                                        }
                                        else if (aclog.RoleID == 7)
                                        {
                                            if (item.RoleID != 1 && item.RoleID != 2)
                                            {
                                                html.Append("    <div class=\"action-blog \">");
                                                html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"remove\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-danger\" data-toggle=\"tooltip\" data-title=\"Xoá khỏi group\">Xóa</a>");
                                                html.Append("    </div>");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (item.UserName != gr.CreatedBy)
                                        {
                                            if (aclog.RoleID == 1)
                                            {
                                                if (item.RoleID != 1 && item.ID != 1)
                                                {
                                                    html.Append("    <div class=\"action-blog \">");
                                                    html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"remove\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-danger\" data-toggle=\"tooltip\" data-title=\"Xoá khỏi group\">Xóa</a>");
                                                    html.Append("    </div>");
                                                }
                                            }
                                            else if (aclog.RoleID == 2)
                                            {
                                                if (item.RoleID != 1 && item.RoleID != 2 && item.ID != 1)
                                                {
                                                    html.Append("    <div class=\"action-blog \">");
                                                    html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"remove\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-danger\" data-toggle=\"tooltip\" data-title=\"Xoá khỏi group\">Xóa</a>");
                                                    html.Append("    </div>");
                                                }
                                            }
                                            else if (aclog.RoleID == 7)
                                            {
                                                if (item.RoleID != 1 && item.RoleID != 2 && item.RoleID != 7)
                                                {
                                                    html.Append("    <div class=\"action-blog \">");
                                                    html.Append("        <a href=\"javascript:;\"  onclick=\"UpdateMember(this)\" data-doing=\"remove\" data-memid=\"" + item.ID + "\"  class=\"btn btn-sm btn-outline-danger\" data-toggle=\"tooltip\" data-title=\"Xoá khỏi group\">Xóa</a>");
                                                    html.Append("    </div>");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            html.Append("</div>");
                        }
                        return Json(new { rs = true, data = html.ToString(), PageIndex = PageIndex += 1 }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "LoadMemeber", Session["UID"].ToString().ToInt(0), ex.Message);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddMember(int memberID, int groupID)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

                tbl_Group G = GroupTable.getbyid(groupID);
                //if (aclog.UserName != G.CreatedBy)
                //    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var member = MemeberTable.insert(memberID, groupID, 1, aclog.UserName);
                if (member != null)
                {

                    SendEmailGroupTable.insert(2, G.ID, aclog.ID, member.MemberID.Value);
                    Thread sendmail = new Thread(SendNotiGroup);
                    sendmail.Start();
                    return Json(new { rs = true, data = member.ID }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "AddMember", Session["UID"].ToString().ToInt(0), ex.Message);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }


        }

        [HttpPost]
        public ActionResult RemoveMember(int memberID, int groupID)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

                tbl_Group G = GroupTable.getbyid(groupID);
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 7)
                    return Json(new { rs = false, message = "Access is denied" }, JsonRequestBehavior.AllowGet);
                tbl_Account acmember = AccountTable.getbyID(memberID);
                if (acmember.ID == 1)
                {
                    return Json(new { rs = false, message = "Access is denied" }, JsonRequestBehavior.AllowGet);
                }
                if (aclog.RoleID == 1 || aclog.RoleID == 2 || aclog.RoleID == 7 || aclog.UserName == G.CreatedBy)
                {
                    if (aclog.ID == 1)
                    {
                        var member = MemeberTable.insert(memberID, groupID, 2, aclog.UserName);
                        tbl_Account ac = AccountTable.getbyID(member.MemberID.Value);
                        SendEmailGroupTable.insert(3, G.ID, aclog.ID, member.MemberID.Value);
                        Thread sendmail = new Thread(SendNotiGroup);
                        sendmail.Start();
                        return Json(new { rs = true, data = member.ID }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        if (aclog.UserName == G.CreatedBy)
                        {
                            if (aclog.RoleID == 1)
                            {
                                var member = MemeberTable.insert(memberID, groupID, 2, aclog.UserName);
                                if (member != null)
                                {
                                    SendEmailGroupTable.insert(3, G.ID, aclog.ID, member.MemberID.Value);
                                    Thread sendmail = new Thread(SendNotiGroup);
                                    sendmail.Start();
                                    return Json(new { rs = true, data = member.ID }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json(new { rs = false, message = "Some thing error" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else if (aclog.RoleID == 2)
                            {
                                if (acmember.RoleID != 1)
                                {
                                    var member = MemeberTable.insert(memberID, groupID, 2, aclog.UserName);
                                    if (member != null)
                                    {
                                        SendEmailGroupTable.insert(3, G.ID, aclog.ID, member.ID);
                                        Thread sendmail = new Thread(SendNotiGroup);
                                        sendmail.Start();
                                        return Json(new { rs = true, data = member.ID }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        return Json(new { rs = false, message = "Some thing error" }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    return Json(new { rs = false, message = "Access is denied" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else if (aclog.RoleID == 7)
                            {
                                if (acmember.RoleID != 1 && acmember.RoleID != 2)
                                {
                                    var member = MemeberTable.insert(memberID, groupID, 2, aclog.UserName);
                                    if (member != null)
                                    {
                                        SendEmailGroupTable.insert(3, G.ID, aclog.ID, member.MemberID.Value);
                                        Thread sendmail = new Thread(SendNotiGroup);
                                        sendmail.Start();
                                        return Json(new { rs = true, data = member.ID }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        return Json(new { rs = false, message = "Some thing error" }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    return Json(new { rs = false, message = "Access is denied" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                return Json(new { rs = false, message = "Access is denied" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            if (acmember.UserName != G.CreatedBy)
                            {
                                if (aclog.RoleID == 1)
                                {
                                    if (acmember.RoleID != 1)
                                    {
                                        var member = MemeberTable.insert(memberID, groupID, 2, aclog.UserName);
                                        if (member != null)
                                        {
                                            SendEmailGroupTable.insert(3, G.ID, aclog.ID, member.MemberID.Value);
                                            Thread sendmail = new Thread(SendNotiGroup);
                                            sendmail.Start();
                                            return Json(new { rs = true, data = member.ID }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            return Json(new { rs = false, message = "Some thing error" }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    else
                                    {
                                        return Json(new { rs = false, message = "Access is denied" }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else if (aclog.RoleID == 2)
                                {
                                    if (acmember.RoleID != 1 && acmember.RoleID != 2)
                                    {
                                        var member = MemeberTable.insert(memberID, groupID, 2, aclog.UserName);
                                        if (member != null)
                                        {
                                            SendEmailGroupTable.insert(3, G.ID, aclog.ID, member.MemberID.Value);
                                            Thread sendmail = new Thread(SendNotiGroup);
                                            sendmail.Start();
                                            return Json(new { rs = true, data = member.ID }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            return Json(new { rs = false, message = "Some thing error" }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    else
                                    {
                                        return Json(new { rs = false, message = "Access is denied" }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else if (aclog.RoleID == 7)
                                {
                                    if (acmember.RoleID != 1 && acmember.RoleID != 2 && acmember.RoleID != 7)
                                    {
                                        var member = MemeberTable.insert(memberID, groupID, 2, aclog.UserName);
                                        if (member != null)
                                        {
                                            SendEmailGroupTable.insert(3, G.ID, aclog.ID, member.MemberID.Value);
                                            Thread sendmail = new Thread(SendNotiGroup);
                                            sendmail.Start();
                                            return Json(new { rs = true, data = member.ID }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            return Json(new { rs = false, message = "Some thing error" }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    else
                                    {
                                        return Json(new { rs = false, message = "Access is denied" }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    return Json(new { rs = false, message = "Access is denied" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                return Json(new { rs = false, message = "Access is denied" }, JsonRequestBehavior.AllowGet);
                            }
                        }

                    }

                }
                else
                {
                    return Json(new { rs = false, message = "Access is denied" }, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "RemoveMember", Session["UID"].ToString().ToInt(0), ex.Message);
                return Json(new { rs = false, message = "Some thing error" }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Notifications()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            return View();
        }

        public ActionResult Message()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            return View();
        }

        [HttpPost]// tìm kiếm theo số điện thoại
        public ActionResult SearchPhone(string phone)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if(string.IsNullOrEmpty(phone))
                    return Json(new { rs = false, rolename = "" }, JsonRequestBehavior.AllowGet);
                // tên phone thôi. chứ nó là UserName đó
                var ac = AccountTable.getbyusername(phone);
                if (ac == null)
                    return Json(new { rs = false, rolename = "" }, JsonRequestBehavior.AllowGet);
                else if (ac.RoleID != 5)
                {
                    return Json(new { rs = false, rolename = ac.RoleName }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    #region thông tin thanh toán
                    //IEnumerable<tbl_CoursesStudent> l = CoursesStudentTable.getall().Where(n => n.PriceLeft > 0 && n.StudentUID == ac.ID && n.isHide != true).OrderByDescending(n => n.CreatedDate).ToList();
                    //IEnumerable<tbl_StudentAppointment> l2 = StudentAppointmentTable.getbystudentid(ac.ID).Where(n => n.isHide != true && n.CloseAppointment != 1);

                    //IEnumerable<tbl_Class> l3 = ClassTable.getall();
                    //IEnumerable<tbl_StudentAppointment> query = from tbl_StudentAppointment in l2
                    //                                            where (tbl_StudentAppointment.ClassPrice - tbl_StudentAppointment.Deposit > 0)
                    //                                            select tbl_StudentAppointment;
                    //List<tbl_Combo> lcb = new List<tbl_Combo>();
                    //List<tbl_CoursesStudent> lc = new List<tbl_CoursesStudent>();
                    //StringBuilder html = new StringBuilder();
                    //foreach (var item in l)
                    //{
                    //    if (item.ComboID != null)
                    //    {
                    //        if (item.ComboID != 0)
                    //        {
                    //            tbl_Combo cb = ComboTable.getbyid(item.ComboID.Value);
                    //            var ck = lcb.Find(n => n.ID == cb.ID);
                    //            if (ck != null)
                    //                continue;
                    //            else
                    //                lcb.Add(cb);
                    //        }
                    //        else
                    //        {
                    //            html.Append("<tr class=\"tr-row\">");
                    //            html.Append("    <td colspan=\"2\">" + item.CourseName + "</td>");
                    //            html.Append("    <td class=\"no-wrap\">" + string.Format("{0:N0}", item.PriceLeft) + "</td>");
                    //            html.Append("    <td class=\"no-wrap\">");
                    //            html.Append("        <a class=\"pd-5 payform\" data-id=\"" + item.ID + "\" data-payfor=\"1\" data-name=\"" + item.StudentName + "\" data-course=\"" + item.CourseName + "\" data-price=\"" + item.PriceLeft + "\" href=\"#div-modal-pay\" data-toggle=\"modal\"><i class=\"far fa-credit-card\"></i></a>");
                    //            html.Append("    </td>");
                    //            html.Append("</tr>");
                    //        }
                    //    }
                    //    else
                    //    {
                    //        html.Append("<tr class=\"tr-row\">");
                    //        html.Append("    <td colspan=\"2\">" + item.CourseName + "</td>");
                    //        html.Append("    <td class=\"no-wrap\">" + string.Format("{0:N0}", item.PriceLeft) + "</td>");
                    //        html.Append("    <td class=\"no-wrap\">");
                    //        html.Append("        <a class=\"pd-5 payform\" href=\"#div-modal-pay\" data-toggle=\"modal\" data-payfor=\"1\" data-id=\"" + item.ID + "\" data-name=\"" + item.StudentName + "\" data-course=\"" + item.CourseName + "\" data-price=\"" + item.PriceLeft + "\"><i class=\"far fa-credit-card\"></i></a>");
                    //        html.Append("    </td>");
                    //        html.Append("</tr>");
                    //    }
                    //}
                    //List<tbl_StudentAppointment> lap = new List<tbl_StudentAppointment>();
                    //foreach (var item in query)
                    //{
                    //    if (item.ComboID != null)
                    //    {
                    //        if (item.ComboID != 0)
                    //        {
                    //            tbl_Combo cb = ComboTable.getbyid(item.ComboID.Value);
                    //            var ck = lcb.Find(n => n.ID == cb.ID);
                    //            if (ck != null)
                    //                continue;
                    //            else
                    //                lcb.Add(cb);
                    //        }
                    //        else
                    //        {
                    //            var cl = ClassTable.getbyID(item.ClassID.Value);
                    //            html.Append("<tr class=\"tr-row\">");
                    //            html.Append("    <td colspan=\"2\">" + item.ClassName + "</td>");
                    //            html.Append("    <td class=\"no-wrap\">" + string.Format("{0:N0}", (cl.PriceClass - item.Deposit)) + "</td>");
                    //            html.Append("    <td class=\"no-wrap\">");
                    //            html.Append("         <a class=\"pd-5 payform\" href=\"#div-modal-pay\" data-payfor=\"2\" data-toggle=\"modal\" data-id=\"" + item.ID + "\" data-name=\"" + item.StudentName + "\" data-course=\"" + item.ClassName + "\" data-price=\"" + (cl.PriceClass - item.Deposit) + "\"><i class=\"far fa-credit-card\"></i></a>");
                    //            html.Append("    </td>");
                    //            html.Append("</tr>");
                    //        }

                    //    }
                    //    else
                    //    {

                    //        var cl = ClassTable.getbyID(item.ClassID.Value);
                    //        html.Append("<tr class=\"tr-row\">");
                    //        html.Append("    <td colspan=\"2\">" + item.ClassName + "</td>");
                    //        html.Append("    <td class=\"no-wrap\">" + string.Format("{0:N0}", (cl.PriceClass - item.Deposit)) + "</td>");
                    //        html.Append("    <td class=\"no-wrap\">");
                    //        html.Append("         <a class=\"pd-5 payform\" href=\"#div-modal-pay\" data-payfor=\"2\" data-toggle=\"modal\" data-id=\"" + item.ID + "\" data-name=\"" + item.StudentName + "\" data-course=\"" + item.ClassName + "\" data-price=\"" + (cl.PriceClass - item.Deposit) + "\"><i class=\"far fa-credit-card\"></i></a>");
                    //        html.Append("    </td>");
                    //        html.Append("</tr>");
                    //    }
                    //}
                    //foreach (var item in lcb)
                    //{
                    //    //tính tiền còn lại của combo 

                    //    int dathu = InvoicesTable.getbystudentid(ac.ID).Where(n => n.isHide != true && n.ComboID == item.ID).Sum(n => Convert.ToInt32(n.Price.Value));
                    //    var cbd = ComboDetailTable.getbycomboid(item.ID);
                    //    var student = AccountTable.getbyID(item.UID.Value);
                    //    html.Append("<tr class=\"tr-row\" data-comboid=\"" + item.ID + "\">");
                    //    html.Append("    <td><a href=\"javascript:;\" class=\"collapse-toggle\"><i class=\"fas fa-plus-square\"></i></a></td>");
                    //    html.Append("    <td>Combo</td>");
                    //    html.Append("    <td class=\"no-wrap\">" + string.Format("{0:N0}", item.PriceLeft) + "</td>");
                    //    html.Append("    <td class=\"no-wrap\">");
                    //    html.Append("           <a class=\"pd-5 payform\" data-payfor=\"3\"  href=\"#div-modal-pay\" data-toggle=\"modal\" data-id=\"" + item.ID + "\" data-name=\"" + student.FullName + "\" data-course=\"Combo-" + item.CreatedDate.Value.ToString("dd/MM/yyyy") + "\" data-price=\"" + item.PriceLeft + "\"><i class=\"far fa-credit-card\"></i></a>");
                    //    html.Append("    </td>");
                    //    html.Append("</tr>");
                    //    html.Append("<tr class=\"collapse-row\">");
                    //    html.Append("    <td colspan=\"8\">");
                    //    html.Append("        <div class=\"collapse-content\">");
                    //    html.Append("            <table class=\"table table-bordered\">");
                    //    html.Append("                <thead class=\"thead-light\">");
                    //    html.Append("                    <tr>");
                    //    html.Append("                        <th>Khóa/Lớp</th>");
                    //    //html.Append("                        <th>Học phí</th>");
                    //    //html.Append("                        <th>Đã đóng</th>");
                    //    html.Append("                        <th>Còn lại</th>");
                    //    html.Append("                        <th>Thao tác</th>");
                    //    html.Append("                    </tr>");
                    //    html.Append("                </thead>");
                    //    html.Append("                <tbody>");
                    //    foreach (var item2 in cbd)
                    //    {
                    //        if (item2.TypeID == 1 || item2.TypeID == null)
                    //        {
                    //            var studentcourse = CoursesStudentTable.getbycourseid(item2.CourseID.Value).Where(n => n.ComboID == item.ID).FirstOrDefault();
                    //            if (studentcourse != null)
                    //            {

                    //                html.Append("                    <tr>");
                    //                html.Append("                        <td>" + studentcourse.CourseName + "</td>");
                    //                html.Append("                        <td>" + string.Format("{0:N0}", studentcourse.PriceLeft) + "</td>");
                    //                if (studentcourse.PriceLeft > 0)
                    //                {
                    //                    html.Append("                        <td><a class=\"pd-5 payform\" data-payfor=\"1\" href=\"#div-modal-pay\" data-toggle=\"modal\" data-id=\"" + studentcourse.ID + "\" data-name=\"" + studentcourse.StudentName + "\" data-course=\"" + studentcourse.CourseName + "\" data-price=\"" + studentcourse.PriceLeft + "\"><i class=\"far fa-credit-card\"></i></a></td>");
                    //                }
                    //                else
                    //                {
                    //                    html.Append("                        <td></td>");
                    //                }

                    //                html.Append("                    </tr>");
                    //            }
                    //        }
                    //        else
                    //        {
                    //            var studentAppointment = StudentAppointmentTable.getbycomboclass(item.ID, item2.CourseID.Value);
                    //            if (studentAppointment != null)
                    //            {
                    //                if (studentAppointment.isHide == true)
                    //                {
                    //                    var course = CoursesStudentTable.getall().Where(n => n.ComboID == item.ID && n.AppointmentID == studentAppointment.ID).FirstOrDefault();
                    //                    if (course != null)
                    //                    {
                    //                        html.Append("                    <tr>");
                    //                        html.Append("                        <td>" + course.CourseName + "</td>");
                    //                        html.Append("                        <td>" + string.Format("{0:N0}", course.PriceLeft) + "</td>");
                    //                        if (course.PriceLeft > 0)
                    //                            html.Append("                        <td><a class=\"pd-5 payform\" data-payfor=\"1\" href=\"#div-modal-pay\" data-toggle=\"modal\" data-id=\"" + course.ID + "\" data-name=\"" + course.StudentName + "\" data-course=\"" + course.CourseName + "\" data-price=\"" + course.PriceLeft + "\"><i class=\"far fa-credit-card\"></i></a></td>");
                    //                        else
                    //                            html.Append("                        <td></td>");
                    //                        html.Append("                    </tr>");
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    //var cls = ClassTable.getbyID(studentAppointment.ClassID.Value);
                    //                    var discount = ComboDetailTable.getbycomboidvscourseid(item.ID, item2.CourseID.Value);
                    //                    //int priceleft = cls.PriceClass.Value - studentAppointment.Deposit.Value - discount.DiscountPrice.Value;
                    //                    int priceleft = item2.CoursePrice.Value - studentAppointment.Deposit.Value - discount.DiscountPrice.Value;
                    //                    html.Append("                    <tr>");
                    //                    html.Append("                        <td>" + studentAppointment.ClassName + "</td>");
                    //                    //html.Append("                        <td>" + string.Format("{0:N0}", cls.PriceClass) + "</td>");
                    //                    //html.Append("                        <td>" + string.Format("{0:N0}", studentAppointment.Deposit+ discount.DiscountPrice) + "</td>");
                    //                    html.Append("                        <td>" + string.Format("{0:N0}", priceleft) + "</td>");
                    //                    if (priceleft > 0)
                    //                        html.Append("                        <td><a class=\"pd-5 payform\" data-payfor=\"2\" href=\"#div-modal-pay\" data-toggle=\"modal\" data-id=\"" + studentAppointment.ID + "\" data-name=\"" + studentAppointment.StudentName + "\" data-course=\"" + studentAppointment.ClassName + "\" data-price=\"" + priceleft + "\"><i class=\"far fa-credit-card\"></i></a></td>");
                    //                    else
                    //                        html.Append("                        <td></td>");
                    //                    html.Append("                    </tr>");
                    //                }
                    //            }
                    //        }
                    //    }
                    //    html.Append("                </tbody>");
                    //    html.Append("            </table>");
                    //    html.Append("        </div>");
                    //    html.Append("    </td>");
                    //    html.Append("</tr>");

                    //}
                    #endregion
                    var academicLevels = AcademicLevelTable.getall().Where(x => x.Language == ac.Language).ToList();
                    StringBuilder input = new StringBuilder();
                    StringBuilder output = new StringBuilder();
                    Parallel.ForEach(academicLevels, i =>
                    {
                        if (i.ID.ToString() == ac.ScoreIn)
                        {
                            input.Append("<option value=\"" + i.ID + "\" selected>" + i.LevelName + " - " + i.LanguageName + "</option>");
                        }
                        else
                        {
                            input.Append("<option value=\"" + i.ID + "\">" + i.LevelName + " - " + i.LanguageName + "</option>");
                        }

                        if (i.ID.ToString() == ac.ScoreOut)
                        {
                            output.Append("<option value=\"" + i.ID + "\" selected>" + i.LevelName + " - " + i.LanguageName + "</option>");
                        }
                        else
                        {
                            output.Append("<option value=\"" + i.ID + "\">" + i.LevelName + " - " + i.LanguageName + "</option>");
                        }
                    });
                    return Json(new { rs = true, acc = ac, /*aphtml = html.ToString(),*/ inputhtml = input.ToString(), outputhtml = output.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "SearchPhone", Session["UID"].ToString().ToInt(0), ex.Message);
                return Json(new { rs = false, rolename = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]// tìm kiếm acc theo id
        public ActionResult SearchID(int id)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var ac = AccountTable.getbyID(id);
                if (ac == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                else
                {
                    string bd = "";
                    string exam = "";
                    string identitydate = "";

                    if (ac.Birthday != null)
                        bd = ac.Birthday.Value.ToString("dd/MM/yyyy");
                    if (ac.DateExam != null)
                        exam = ac.DateExam.Value.ToString("dd/MM/yyyy");
                    if (ac.IdentityCardDate != null)
                        identitydate = ac.IdentityCardDate.Value.ToString("dd/MM/yyyy");

                    var sms = MessageTable.getbyfIDvstID(aclog.ID, ac.ID);
                    //cập nhật đã xem tin
                    var lsms = MessageTable.getbyreceiveID(aclog.ID).Where(n => n.MessageNew == true).ToList();
                    if (lsms.Count > 0)
                    {
                        var l = lsms.Select(n => n.FromUID).Distinct();
                        foreach (var item in l)
                        {
                            MessageTable.updateview(item.Value, aclog.ID, aclog.UserName);
                        }
                    }
                    return Json(new { rs = true, acc = ac, bd = bd, exam = exam, sms = sms, identitydate = identitydate }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "SearchID", Session["UID"].ToString().ToInt(0), ex.Message);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]// check mã khuyến mãi
        public ActionResult SearchCodeDiscout(string code, string countid)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var d = DiscountTable.getbycode(code);
                if (d == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                else
                {
                    if (d.Status == 1 && d.QuantityLeft > 0)
                    {
                        if (d.DeadLine != null)
                        {
                            if (d.DeadLine.Value < GetDateTime.Now.Date)
                                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                        }
                        int pc = 0;
                        string[] arc = countid.Split(',');
                        for (int i = 0; i < arc.Length; i++)
                        {
                            var c = CoursesTable.getbyid(arc[i].ToInt(0));
                            if (c == null)
                                continue;
                            pc += c.Price.Value;
                        }

                        var total = CoursesStudentTable.getbydiscountid(d.ID);
                        if (d.Status == 1 && GetDateTime.Now.Date <= d.DeadLine.Value.Date && d.Quantity > total.Count)
                        {
                            int p = 0;
                            bool dispercent = false;
                            if (d.DiscountType != null)
                                dispercent = d.DiscountType.Value;
                            if (!dispercent)
                                p = d.DiscountPrice.Value;
                            else
                                p = Math.Round((float)((d.DiscountPercent.Value / 100f) * pc), 0).ToString().ToInt(0);
                            int priceleft = pc - p;
                            return Json(new { rs = true, d = String.Format("{0:0,0}", p), pl = String.Format("{0:0,0}", priceleft) }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "SearchCodeDiscout", 0, ex.Message);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]// check mã khuyến mãi cho cả lớp
        public ActionResult SearchCodeDiscoutNew(string code, string countid, string classid, int commitment)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var d = DiscountTable.getbycode(code);
                if (d == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                else
                {
                    int pc = 0;
                    string[] arc = countid.Split(',');
                    for (int i = 0; i < arc.Length; i++)
                    {
                        var c = CoursesTable.getbyid(arc[i].ToInt(0));
                        if (c == null)
                            continue;
                        pc += c.Price.Value;
                    }
                    string[] arclass = classid.Split(',');
                    for (int i = 0; i < arclass.Length; i++)
                    {
                        var c = ClassTable.getbyID(arclass[i].ToInt(0));
                        if (c == null)
                            continue;
                        pc += c.PriceClass.Value;
                    }
                    var package = ClassTable.getbyID(commitment);
                    pc += package.PriceClass.Value;

                    var total = CoursesStudentTable.getbydiscountid(d.ID);
                    if (d.Status == 1 && GetDateTime.Now.Date <= d.DeadLine.Value.Date && d.Quantity > total.Count)
                    {
                        int p = 0;
                        bool dispercent = false;
                        if (d.DiscountType != null)
                            dispercent = d.DiscountType.Value;
                        if (!dispercent)
                            p = d.DiscountPrice.Value;
                        else
                            p = Math.Round((float)((d.DiscountPercent.Value / 100f) * pc), 0).ToString().ToInt(0);
                        int priceleft = pc - p;
                        return Json(new { rs = true, d = String.Format("{0:0,0}", p), pl = String.Format("{0:0,0}", priceleft), id = d.ID }, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "SearchCodeDiscoutNew", Session["UID"].ToString().ToInt(0), ex.Message);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult SearchCodeDiscoutNewCustom(string code)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var d = DiscountTable.getbycode(code);
                if (d == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                else
                {
                    if (d.Status == 1 && GetDateTime.Now.Date <= d.DeadLine.Value.Date && d.QuantityLeft > 0)
                    {

                        return Json(new { rs = true, d = String.Format("{0:0,0}", d.DiscountPrice), id = d.ID, p = d.DiscountPercent ?? 0 }, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "SearchCodeDiscoutNew", Session["UID"].ToString().ToInt(0), ex.Message);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]// check mã khuyến mãi cho cả lớp
        public ActionResult SearchCodeDiscoutServide(string code, int examID)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var d = DiscountTable.getbycode(code);
                if (d == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                else
                {
                    var total = StudentAppointmentTable.getbydiscountid(d.ID);
                    if (d.Status == 1 && GetDateTime.Now.Date <= d.DeadLine.Value.Date && d.Quantity > total.Count)
                    {
                        var ex = ServiceExamsTable.getbyID(examID);
                        int p = 0;
                        bool dispercent = false;
                        if (d.DiscountType != null)
                            dispercent = d.DiscountType.Value;
                        if (!dispercent)
                            p = d.DiscountPrice.Value;
                        else
                            p = Math.Round((float)((d.DiscountPercent.Value / 100f) * ex.ExamPrice), 0).ToString().ToInt(0);
                        int priceleft = ex.ExamPrice.Value - p;
                        return Json(new { rs = true, d = String.Format("{0:0,0}", p), pl = String.Format("{0:0,0}", priceleft) }, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "SearchCodeDiscoutNew", Session["UID"].ToString().ToInt(0), ex.Message);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]// chat
        public ActionResult viewnoti(int ID)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (ID != 0)
                {
                    var rs = NotificationTable.updateView(ID, aclog.UserName);
                    if (rs != null)
                        return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var l = NotificationTable.GetByUID(aclog.ID).Where(n => n.Status == 1).ToList();
                    foreach (var item in l)
                    {
                        NotificationTable.updateView(item.ID, aclog.UserName);
                    }
                    return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "viewnoti", Session["UID"].ToString().ToInt(0), ex.Message);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult UpdateProfile(FormCollection f, HttpPostedFileBase fileupload)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

                int gender = f.Get("ddl-gender").ToInt(0);
                string fullname = f.Get("txt-full-name").ToString();
                string email = f.Get("txt-email").ToString();
                string phone = f.Get("txt-phone").ToString();
                string pass = f.Get("txt-password").ToString();
                string address = f.Get("txt-address").ToString();

                DateTime bd = GetDateTime.Now.Date;
                try
                {
                    string s = f.Get("txt-birthday").ToString().Replace(" ", "");
                    bd = DateTime.ParseExact(s, "dd/MM/yyyy", null);
                }
                catch
                {
                    bd = GetDateTime.Now.Date;
                }

                string av = "";
                string avthumb = "";
                if (fileupload != null)
                {
                    int size = fileupload.ContentLength;
                    if (size > 1000000)
                    {
                        SetAlert("File size <= 1 Mb", "w");
                        return RedirectToAction("ProfileUser", "Account");
                    }
                    string name = Path.GetFileNameWithoutExtension(fileupload.FileName);
                    string ext = Path.GetExtension(fileupload.FileName);
                    if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                    {
                        SetAlert("Vui lòng chỉ Upload với định dạng hình ảnh!", "e");
                        return RedirectToAction("ProfileUser", "Account");
                    }

                    string fileContentType = fileupload.ContentType; // getting ContentType
                    byte[] tempFileBytes = new byte[fileupload.ContentLength];
                    var data = fileupload.InputStream.Read(tempFileBytes, 0, Convert.ToInt32(fileupload.ContentLength));
                    string fileName = fileupload.FileName; // getting File Name
                    string fileExtension = Path.GetExtension(fileName).ToLower();
                    var result = AssetCRM.isValidFile(tempFileBytes, fileExtension, fileContentType); // Validate Header
                    if (result)
                    {
                        fileName = Guid.NewGuid() + ext;
                        var path = Path.Combine(Server.MapPath("~/Upload/useravatar/"), fileName);
                        av = "/Upload/useravatar/" + fileName;
                        avthumb = "/Upload/thumbnails/useravatar/" + fileName;
                        fileupload.SaveAs(path);
                        AssetCRM.GenerateThumbNail("~/Upload/useravatar//" + fileName, "~/Upload/thumbnails/useravatar//" + fileName, 150, false);
                    }
                    else
                        SetAlert("Không thành công", "e");
                }

                var rs = AccountTable.updateprofile(aclog.ID, fullname, gender, pass, email, phone, address, bd, av, avthumb, aclog.UserName);
                if (rs != null)
                    SetAlert("Thành công", "s");
                else
                    SetAlert("Không thành công", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "UpdateProfile", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }


        //inserts device token trình duyệt
        public ActionResult InsertDevicetoken(string PushEndpoint, string PushP256DH, string PushAuth)
        {
            try
            {
                if (Session["UID"] != null)
                {
                    var aclog = AccountTable.getbyID(int.Parse(Session["UID"].ToString()));
                    var dv = DeviceBrowserTable.insert(aclog.ID, PushEndpoint, PushP256DH, PushAuth, aclog.UserName);
                }
                return Json(true);
            }
            catch (Exception ex)
            {
                int id = 0;

                if (Session["UID"] != null)
                {
                    id = Session["UID"].ToString().ToInt(0);
                }
                AssetCRM.Writelog("AccountController", "InsertDevicetoken", id, ex.Message);
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> RankUser()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5 || aclog.RoleID == 6 || aclog.RoleID == 4)
                {
                    SetAlert("Bạn không có quyền truy cập", "e");
                    return RedirectToAction("Signin", "Login", new { area = "" });
                }


                //lấy sao và 5 bài gần nhât
                var rate15 = HomeController.TinhTungRate15();
                var data15 = await rate15;

                ViewBag.RateExercise = data15.RateExercise;
                ViewBag.Top5RateExercise = data15.Top5RateExercise;

                ViewBag.RateTVV = data15.RateTVV;
                ViewBag.Top5RateTVV = data15.Top5RateTVV;
                return View();
            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "RankUser", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PostDetail(int id, int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var po = PostContentTable.getbyid(id);
                if (po == null)
                    return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });
                if (po.Status != 3)
                    return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });

                if (aclog.RoleID == 5 && po.isHideStudent.Value)
                    return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });
                if (aclog.RoleID == 6 && po.isHideStaff.Value)
                    return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });

                ViewBag.ID = id;
                //lưu lại số lượt truy cập
                if (page == 1)
                    PostContentTable.updatenumberview(id);
                IEnumerable<tbl_PostComment> l = PostCommentTable.getbypostid(id).Where(n => n.isHide != true).OrderByDescending(n => n.ModifiedDate).ToList();
                int pageSize = 20;
                return View(l.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "PostDetail", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        //Tạo group
        [HttpPost]
        public ActionResult CreateGroup(FormCollection f, HttpPostedFileBase fileCoverImage)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 7)
                {
                    SetAlert("Access is denied", "e");
                    return Redirect(Request.UrlReferrer.AbsoluteUri);
                }
                string groupName = f.Get("txt-gr-name").ToString();
                if (string.IsNullOrEmpty(groupName))
                {
                    SetAlert("Missing Group Name", "e");
                    return Redirect(Request.UrlReferrer.AbsoluteUri);
                }
                //string listRole = f.Get("ddl-grrole").ToString();
                //if (string.IsNullOrEmpty(listRole))
                //{
                //    SetAlert("Missing Role", "e");
                //    return RedirectToAction("ProfileUser", "Account");
                //}
                int center = 0;
                if (aclog.RoleID == 1)
                {

                    center = f.Get("ddl-center").ToInt(0);
                    if (center != 0)
                    {
                        tbl_School sc = SchoolTable.getbyID(center);
                        if (sc == null)
                        {
                            SetAlert("Center is not found", "e");
                            return Redirect(Request.UrlReferrer.AbsoluteUri);
                        }
                    }
                }
                else
                {
                    center = aclog.SchoolID.Value;
                }
                int course = 0;
                if (!string.IsNullOrEmpty(f.Get("ddl-course")))
                {
                    course = f.Get("ddl-course").ToInt(0);
                }
                string coverIMG = "";
                #region IMG
                //if (fileCoverImage != null)
                //{
                //    int size = fileCoverImage.ContentLength;
                //    if (size > 1000000)
                //    {
                //        SetAlert("File size <= 1 Mb", "w");
                //        return RedirectToAction("ProfileUser", "Account");
                //    }
                //    string name = Path.GetFileNameWithoutExtension(fileCoverImage.FileName);
                //    string ext = Path.GetExtension(fileCoverImage.FileName);
                //    if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                //    {
                //        SetAlert("Fail", "e");
                //        return RedirectToAction("ProfileUser", "Account");
                //    }

                //    string fileContentType = fileCoverImage.ContentType; // getting ContentType
                //    byte[] tempFileBytes = new byte[fileCoverImage.ContentLength];
                //    var data = fileCoverImage.InputStream.Read(tempFileBytes, 0, Convert.ToInt32(fileCoverImage.ContentLength));
                //    string fileName = fileCoverImage.FileName; // getting File Name
                //    string fileExtension = Path.GetExtension(fileName).ToLower();
                //    var result = AssetCRM.isValidFile(tempFileBytes, fileExtension, fileContentType); // Validate Header
                //    if (result)
                //    {
                //        fileName = Guid.NewGuid() + ext;
                //        var path = Path.Combine(Server.MapPath("~/Upload/groupcoverimage/"), fileName);
                //        coverIMG = "/Upload/groupcoverimage/" + fileName;
                //        fileCoverImage.SaveAs(path);
                //    }
                //    else
                //        SetAlert("Fail", "e");
                //}
                //else
                //{
                //    SetAlert("Missing Group Cover Image", "e");
                //    return RedirectToAction("ProfileUser", "Account");
                //}
                #endregion 
                var res = GroupTable.insert(groupName, coverIMG, center, course, aclog.UserName);
                if (res != null)
                {
                    SetAlert("Success", "s");
                    SendEmailGroupTable.insert(1, res.ID, aclog.ID, 0);
                    Thread sendmail = new Thread(SendNotiGroup);
                    sendmail.Start();
                }
                else
                    SetAlert("Fail", "e");
                if (res.CourseID != 0)
                {
                    return RedirectToAction("ScheduleCourse", "CourseDetail", new { area = "Admin", id = res.ID });
                }
                else
                {
                    return Redirect(Request.UrlReferrer.AbsoluteUri);
                }

            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "CreateGroup", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
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
                                var nt = NotificationTable.insertsingle("", "Thêm vào nhóm", "[ " + group.GroupName + " ] >. XEM GROUP", item.ID, owner.UserName, 5, group.ID);
                                if (!string.IsNullOrEmpty(item.Email))
                                {
                                    AssetCRM.SendMailAmazone(item.Email, "ZIM | " + item.FullName + ", bạn có thông báo mới", "Bạn đã được thêm vào group [" + group.GroupName + "]" + "<a href=\"" + Media.Host + "/Admin/Notification/Notification\">Xem</a>");
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
                                        var nt = NotificationTable.insertsingle("", "Thêm vào nhóm", "[ " + group.GroupName + " ] >. XEM GROUP", item.ID, owner.UserName, 5, group.ID);
                                        if (!string.IsNullOrEmpty(item.Email))
                                        {
                                            AssetCRM.SendMailAmazone(item.Email, "ZIM | " + item.FullName + ", bạn có thông báo mới", "Bạn đã được thêm vào group [" + group.GroupName + "]" + "<a href=\"" + Media.Host + "/Admin/Notification/Notification\">Xem</a>");
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
                                        var nt = NotificationTable.insertsingle("", "Thêm vào nhóm", "[ " + group.GroupName + " ] > . XEM GROUP", item.ID, owner.UserName, 5, group.ID);
                                        if (!string.IsNullOrEmpty(item.Email))
                                        {
                                            AssetCRM.SendMailAmazone(item.Email, "ZIM | " + item.FullName + ", bạn có thông báo mới", "Bạn đã được thêm vào group [" + group.GroupName + "]" + "<a href=\"" + Media.Host + "/Admin/Notification/Notification\">Xem</a>");
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
                            var nt = NotificationTable.insertsingle("", "Thêm vào nhóm", "[ " + group.GroupName + " ] > . XEM GROUP", sendto.ID, owner.UserName, 5, group.ID);
                            if (!string.IsNullOrEmpty(sendto.Email))
                            {
                                AssetCRM.SendMailAmazone(sendto.Email, "ZIM | " + sendto.FullName + ", bạn có thông báo mới", "Bạn đã được thêm vào group [" + group.GroupName + "]" + "<a href=\"" + Media.Host + "/Admin/Notification/Notification\">Xem</a>");
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
                                AssetCRM.SendMailAmazone(sendto.Email, "ZIM | " + sendto.FullName + ", bạn có thông báo mới", "Bạn đã được yêu cầu rời khỏi group." + "<a href=\"" + Media.Host + "/Admin/Notification/Notification\">Xem</a>");
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
                                    AssetCRM.SendMailAmazone(sendto.Email, "ZIM | " + sendto.FullName + ", bạn có thông báo mới", "Có bài viết mới ở group mà bạn tham gia." + "<a href=\"" + Media.Host + "/Admin/Notification/Notification\">Xem</a>");
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
                                    AssetCRM.SendMailAmazone(sendto.Email, "ZIM | " + sendto.FullName + ", bạn có thông báo mới", "Nhóm của bạn đã bị giải tán" + "<a href=\"" + Media.Host + "/Admin/Notification/Notification\">Xem</a>");
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
        [HttpPost]
        public ActionResult EditGroup(FormCollection f, HttpPostedFileBase EditfileCoverImage)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                string currentURL = Request.UrlReferrer.ToString();

                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                int GroupID = f.Get("hdfGroupID").ToInt(0);
                var GR = GroupTable.getbyid(GroupID);
                if (GR != null)
                {
                    string groupName = f.Get("txtedit-gr-name").ToString();
                    string coverIMG = "";
                    int course = 0;
                    if (!string.IsNullOrEmpty(f.Get("ddl-edit-course")))
                    {
                        course = f.Get("ddl-edit-course").ToInt(0);
                    }
                    GR = GroupTable.update(GroupID, groupName, coverIMG, GR.CenterID.Value, course, aclog.UserName);
                    if (GR != null)
                        SetAlert("Success", "s");
                    else
                        SetAlert("Fail", "e");
                }
                else
                {
                    SetAlert("Không tìm thấy group", "e");
                    return Redirect(Request.UrlReferrer.AbsoluteUri);
                }
                if (GR.CourseID != 0)
                {
                    return RedirectToAction("ScheduleCourse", "CourseDetail", new { area = "Admin", id = GR.CourseID });

                }
                else
                {
                    return RedirectToAction("ProfileUser", "Account");
                }
            }
            catch (Exception ex)
            {
                SetAlert("Fail", "w");
                AssetCRM.Writelog("AccountController", "EditGroup", Session["UID"].ToString().ToInt(0), ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult GetListCourse(int centerId)
        {
            if (Session["UID"] == null)
            {
                return Json(new { rs = false, message = "Session time out" });
            }
            try
            {
                tbl_Account aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog == null)
                {
                    return Json(new { rs = false, message = "Session time out" });
                }
                List<tbl_Courses> lcourse = CoursesTable.getall().Where(n => n.SchoolID.Value == centerId).OrderByDescending(n => n.ID).ToList();
                return Json(new { rs = true, data = lcourse });
            }
            catch
            {
                return Json(new { rs = false, message = "Some thing error" });
            }
        }

        [HttpGet]
        public ActionResult RemoveGroup(int groupId)
        {
            if (Session["UID"] == null)
            {
                SetAlert("Session time out", "s");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
            try
            {
                tbl_Account aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog == null)
                {
                    SetAlert("Session time out", "s");
                    return RedirectToAction("Signin", "Login", new { area = "" });

                }
                if (aclog.RoleID != 1)
                {
                    SetAlert("Access is denied", "s");
                    return RedirectToAction("Signin", "Login", new { area = "" });
                }
                tbl_Group g = GroupTable.getbyid(groupId);
                if (g == null)
                {
                    SetAlert("Group not found", "s");
                    return RedirectToAction("Signin", "Login", new { area = "" });
                }
                else
                {
                    var rs = GroupTable.remove(g.ID);
                    if (rs != null)
                    {
                        //Gửi mail
                        SendEmailGroupTable.insert(5, g.ID, aclog.ID, 0);
                        Thread sendmail = new Thread(SendNotiGroup);
                        sendmail.Start();

                        if (rs.CourseID.Value != 0)
                        {
                            return RedirectToAction("ScheduleCourse", "CourseDetail", new { area = "Admin", id = rs.CourseID.Value });
                        }
                        else
                        {
                            return RedirectToAction("ProfileUser", "Account", new { area = "Admin" });
                        }
                    }
                    else
                    {
                        SetAlert("Some thing error", "s");
                        return RedirectToAction("ProfileUser", "Account", new { area = "Admin" });
                    }

                }
            }
            catch
            {
                SetAlert("Some thing error", "s");
                return RedirectToAction("ProfileUser", "Account", new { area = "Admin" });
            }
        }
        [HttpPost]
        public ActionResult ChangeStatus(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                AccountTable.UpdateStatus(id, 3);
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                SetAlert("Some thing error", "s");
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetByRolesAndLanguage(string roles, string languages)
        {

            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            try
            {
                if(roles.Contains("0"))
                    roles = "2,3,4,5,6,7";

                var data = AccountTable.GetByRolesAndLanguage(roles, languages);
                return Json(new { rs = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("AccountControler", "GetByRolesAndLanguage", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetByLanguage(string language)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (language.Any())
            {
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                try
                {
                    var data = AccountTable.GetByLangue(language?.Split(',').Select(x => Convert.ToInt32(x)).ToArray());
                    return Json(new { rs = true, data }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    AssetCRM.Writelog("AccountControler", "GetByLanguage", 0, e.Message + "||" + e.InnerException);
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return null;
            }
        }
        
        [HttpGet]
        public async Task<ActionResult> LoadDataHeader()
        {
            if (Session["UID"] == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

            var warningList = 0;
            var lnoti = await NotificationTable.GetByUIDNewAsync(aclog.ID);
            var redList = await FeedBackTable.GetRedList();
            var orangeList = await FeedBackTable.GetOrangeList();
            var greenList = await StudentAppointmentTable.GetListStudentAppointment("", 0, 0, DateTime.Now.FirstDayOfMonth().ToString("yyyy-MM-dd 00:00:00.000"), DateTime.Now.LastDayOfMonth().ToString("yyyy-MM-dd 23:59:59.998"), 1, 0, aclog.ID, aclog.RoleID ?? 0, 0);
            if (greenList.Count > 0)
            {
                warningList = greenList[0].TotalRow.Value;
            }

            int rejects = 0;
            var rejectSalary = await TeacherSalaryTable.Get_ListTeacherSalaryIsReject(0, 0, null, null, 0, 1);
            if (rejectSalary.Count > 0)
            {
                rejects = rejectSalary[0].TotalRow ?? 0;
            }

            //var periods = await CampaignSaleTable.getallasync();
            var periods = CampaignSaleTable.GetCampaignsInYear();
            var manager = await AccountTable.GetManagerAsync();

            return Json(new { rs = true, warningList, redList, orangeList, rejects, lnoti = lnoti.Count(), notiList = lnoti.Take(5).ToList(), periods, manager }, JsonRequestBehavior.AllowGet);
        }
    }
}