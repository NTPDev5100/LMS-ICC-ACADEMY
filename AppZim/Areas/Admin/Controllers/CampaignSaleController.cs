using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using Hangfire;
using MB.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AppZim.Areas.Admin.Controllers
{
    public class CampaignSaleController : BaseController
    {
        #region chiến dịch(chu kỳ)
        public ActionResult AddCampaignSale(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var name = f.Get("txt-period").ToString();
                DateTime stime = DateTime.Now;
                DateTime etime = DateTime.Now;
                var manager = f.Get("slManager").ToString().ToInt(0);
                var type = f.Get("slPeriodType").ToString().ToInt(0);
                var time = "";
                if (!string.IsNullOrEmpty(f.Get("slTime")))
                {
                    time = f.Get("slTime").ToString();
                }
                var year = f.Get("slYear").ToString();
                if (type == 1)
                {
                    stime = DateTime.ParseExact(year + "-01-01", "yyyy-MM-dd", null);
                    etime = DateTime.ParseExact(year + "-12-31", "yyyy-MM-dd", null);
                }
                else if (type == 2)
                {
                    if (time == "13")
                    {
                        stime = DateTime.ParseExact(year + "-01-01", "yyyy-MM-dd", null);
                        etime = stime.AddMonths(3).LastDayOfMonth();
                    }
                    else if (time == "14")
                    {
                        stime = DateTime.ParseExact(year + "-04-01", "yyyy-MM-dd", null);
                        etime = stime.AddMonths(3).LastDayOfMonth();
                    }
                    else if (time == "15")
                    {
                        stime = DateTime.ParseExact(year + "-07-01", "yyyy-MM-dd", null);
                        etime = stime.AddMonths(3).LastDayOfMonth();
                    }
                    else
                    {
                        stime = DateTime.ParseExact(year + "-10-01", "yyyy-MM-dd", null);
                        etime = stime.AddMonths(3).LastDayOfMonth();
                    }
                }
                else
                {
                    stime = DateTime.ParseExact(year + "-" + time + "-01", "yyyy-MM-dd", null);
                    etime = stime.LastDayOfMonth();
                }
                string description = "";
                if (!string.IsNullOrEmpty(f.Get("txt-description")))
                {
                    description = f.Get("txt-description").ToString();
                }
                CampaignSaleTable.insert(name, stime, etime, description, aclog.UserName, type, manager);
                SetAlert("Thành công", "s");
                return RedirectToAction("DashBoard", "Home");
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("", "", 0, e.Message + " || " + e.InnerException);
                SetAlert("Đã xảy ra lỗi", "e");
                return RedirectToAction("DashBoard", "Home");
            }
        }
        public ActionResult PeriodDetail(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var campaign = CampaignSaleTable.getbyid(id);
            if (campaign == null)
            {
                SetAlert("Chu kỳ không tồn tại", "e");
                return RedirectToAction("DashBoard", "Home");
            }
            bool addList = false;
            string tvvName = "";
            if (!string.IsNullOrEmpty(Request.QueryString["txt-search"]))
            {
                tvvName = Request.QueryString["txt-search"].ToString();
                addList = true;
            }
            ViewBag.Search = tvvName;
            var data = new List<Admin_Target_LoadByCampaignId_Result>();
            using (var db = new ZimEntities())
            {
                data = db.Admin_Target_LoadByCampaignId(tvvName, id).ToList();
                if (addList == true)
                {
                    var temp = data;
                    do
                    {
                        var tempInFor = new List<Admin_Target_LoadByCampaignId_Result>();
                        tempInFor = db.Admin_Target_GetParentTarget(string.Join(",", temp.Select(x => x.MainTargetID ?? 0).ToList())).Select(x => new Admin_Target_LoadByCampaignId_Result() { ID = x.ID, CampaignID = x.CampaignID, MainTargetID = x.MainTargetID, TargetType = x.TargetType, TargetValue = x.TargetValue, CurrentValue = x.CurrentValue, Note = x.Note, IsDone = x.IsDone, CreatedDate = x.CreatedDate, CreatedBy = x.CreatedBy, FullName = x.FullName, Status = x.Status, FinishedPercent = x.FinishedPercent, ManagerID = x.ManagerID, RoleName = x.RoleName, RoleID = x.RoleID, Avatar = x.Avatar }).ToList();
                        if (tempInFor.Count > 0)
                            data.AddRange(tempInFor);
                        temp = tempInFor;
                    }
                    while (temp.Count > 0);
                    data = data.GroupBy(i => i.ID).Select(i => i.FirstOrDefault()).ToList();
                }
            }

            ViewBag.Campaign = campaign;
            return View(data);
        }

        public ActionResult RevenueRank(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var campaign = CampaignSaleTable.getbyid(id);
            if (campaign == null)
            {
                SetAlert("Chu kỳ không tồn tại", "e");
                return RedirectToAction("DashBoard", "Home");
            }
            int teamId = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slTeam"]))
            {
                teamId = Request.QueryString["slTeam"].ToString().ToInt(0);
            }
            ViewBag.Team = teamId;
            int rankType = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slRankType"]))
            {
                rankType = Request.QueryString["slRankType"].ToString().ToInt(0);
            }
            ViewBag.RankType = rankType;
            int targetType = 4;
            if (!string.IsNullOrEmpty(Request.QueryString["slTargetType"]))
            {
                targetType = Request.QueryString["slTargetType"].ToString().ToInt(0);
            }
            ViewBag.targetType = targetType;
            var data = new List<Admin_RevenueRank_GetList_Result>();
            using (var db = new ZimEntities())
            {
                data = db.Admin_RevenueRank_GetList(id, teamId, rankType, targetType).ToList();
            }
            ViewBag.Campaign = campaign;
            return View(data);
        }


        [HttpPost]
        public ActionResult LoadEmailWarrningRevenues(string campaignName, string fullName)
        {
            try
            {
                using (var db = new ZimEntities())
                {
                    var template = db.tbl_EmailTemplate.Where(x => x.ID == 39).FirstOrDefault();
                    string ct = "";
                    string tt = "";
                    if (template == null)
                    {
                        return Json(new { rs = false });
                    }
                    //tiêu đề mail
                    tt = template.EmailTitle;
                    //nội dung mail
                    ct = template.EmailContent;
                    ct = ct.Replace("[ten-chien-dich]", campaignName);
                    ct = ct.Replace("[ho-ten]", fullName);
                    return Json(new { rs = true, tt, ct });
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("CampaignSale", "LoadEmailWarrningRevenues", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false });
            }
        }



        [HttpPost]
        [ValidateInput(false)]
        public ActionResult GuiMail(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                string tieude = f.Get("txt-title").ToString();
                string noidung = f.Get("txt-content").ToString();
                string emails = f.Get("txt-emails").ToString();
                AssetCRM.SendMailMultipleBcc(tieude, noidung, emails);
                SetAlert("Thành công", "s");
            }
            catch (Exception e)
            {
                SetAlert("Lỗi", "e");
            }
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        [HttpPost]
        public ActionResult GetMangers(int type)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var leaderList = new List<tbl_Account>();
            int role = 6;
            switch (type)
            {
                case 1:
                    role = 1;
                    break;
                case 2:
                    role = 2;
                    break;
                case 3:
                    role = 3;
                    leaderList = AccountTable.getbyrole(2);
                    break;
                default:
                    role = 6;
                    leaderList = AccountTable.getbyrole(3);
                    break;
            }
            var list = AccountTable.getbyrole(role);
            StringBuilder html = new StringBuilder();
            html.Append("<option value=0>---</option>");
            if (leaderList.Count > 0)
            {
                foreach (var g in leaderList)
                {
                    html.Append("<optgroup label=\"" + g.FullName + "\">");
                    html.Append("<option value=" + g.ID + ">" + g.FullName + "</option>");
                    foreach (var i in list)
                    {
                        if (i.LeaderID == g.ID)
                            html.Append("<option value=" + i.ID + ">" + i.FullName + "</option>");
                    }
                    html.Append("</optgroup>");
                }
            }
            else
            {
                foreach (var i in list)
                {
                    html.Append("<option value=" + i.ID + ">" + i.FullName + "</option>");
                }
            }
            return Json(new { rs = true, data = html.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeletePeriod(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var rs = CampaignSaleTable.Delete(id, true, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Đã xảy ra lỗi", "e");
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditPeriod(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var periodId = f.Get("hdPeriodId").ToString().ToInt(0);
            var period = CampaignSaleTable.getbyid(periodId);
            if (period == null)
            {
                SetAlert("Đã xảy ra lỗi", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            var periodName = period.CampaignName;
            if (!string.IsNullOrEmpty(f.Get("txt-period-edit")))
            {
                periodName = f.Get("txt-period-edit").ToString();
            }
            int managerId = period.ManagerID.Value;
            if (!string.IsNullOrEmpty(f.Get("slManager-edit")))
            {
                managerId = f.Get("slManager-edit").ToInt(0);
            }
            string note = period.Note;
            if (!string.IsNullOrEmpty(f.Get("txt-description-edit")))
            {
                note = f.Get("txt-description-edit").ToString();
            }
            var rs = CampaignSaleTable.update(periodId, periodName, note, aclog.UserName, managerId);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Đã xảy ra lỗi", "e");
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }
        #endregion
        #region thưởng khi đạt mục tiêu
        public ActionResult TargetBonus(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            int campaignid = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slCampaign"]))
            {
                campaignid = ViewBag.Campaign = Request.QueryString["slCampaign"].ToInt(0);
            }
            int type = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slTargetType"]))
            {
                type = ViewBag.TargetType = Request.QueryString["slTargetType"].ToInt(0);
            }
            var l = new List<Admin_TargetBonus_GetList_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_TargetBonus_GetList(campaignid, type, (page - 1) * pageSize, pageSize).ToList();
            }
            int total = 0;
            if (l.Count > 0)
                total = l[0].TotalRow.Value;
            ViewBag.Total = total;
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            return View(l);
        }
        public ActionResult AddTargetBonus(FormCollection f, HttpPostedFileBase file)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2)
                return RedirectToAction("Signin", "Login", new { area = "" });

            SetAlert("Thành công", "s");
            int type = 0;
            if (!string.IsNullOrEmpty(f.Get("slType")))
            {
                type = f.Get("slType").ToInt(0);
            }
            double percent = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-percent")))
            {
                percent = Convert.ToDouble(f.Get("txt-percent"));
            }
            double target = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-target")))
            {
                target = Convert.ToDouble(f.Get("txt-target").ToString().Replace(",", ""));
            }
            TargetBonusTable.insert(0, type, percent, aclog.UserName, "", "", target);
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }
        public ActionResult EditTargetBonus(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2)
                return RedirectToAction("Signin", "Login", new { area = "" });

            SetAlert("Thành công", "s");
            int id = f.Get("hdfID").ToInt(0);
            double percent = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-percent-edit")))
            {
                percent = Convert.ToDouble(f.Get("txt-percent-edit"));
            }
            double target = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-target-edit")))
            {
                target = Convert.ToDouble(f.Get("txt-target-edit").ToString().Replace(",", ""));
            }
            TargetBonusTable.update(id, percent, aclog.UserName, "", "", target);
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

        [HttpPost]
        public ActionResult DeleteTargetBonus(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var rs = TargetBonusTable.Delete(id, true, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Đã xảy ra lỗi", "e");
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region mục tiêu
        public ActionResult AddTarget(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3 && aclog.RoleID != 6)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int campaignId = f.Get("slPeriod").ToInt(0);
            int targetType = f.Get("hdtype").ToInt(0);
            int managerId = 0;
            if (!string.IsNullOrEmpty(f.Get("slManager")))
            {
                managerId = f.Get("slManager").ToInt(0);
            }
            if (aclog.RoleID == 6)
            {
                managerId = aclog.ID;
            }
            string note = "";
            if (!string.IsNullOrEmpty(f.Get("txt-description")))
            {
                note = f.Get("txt-description").ToString();
            }
            double targetValue = Convert.ToDouble(f.Get("txt-targetvalue").Replace(",", ""));
            var manager = AccountTable.getbyID(managerId);
            if (manager == null)
                manager = new tbl_Account() { LeaderID = 0 };
            tbl_Target mainTartget;
            switch (targetType)
            {
                case 1:
                    mainTartget = new tbl_Target() { ID = 0 };
                    break;
                case 2:
                    mainTartget = TargetTable.GetMainTarget(campaignId, targetType - 1, manager.LeaderID ?? 0);
                    break;
                case 3:
                    if (manager.RoleID == 2)//trưởng phòng
                    {
                        mainTartget = TargetTable.getByMainTargetForRole23(campaignId, 2, manager.ID);
                    }
                    else
                    {
                        mainTartget = TargetTable.GetMainTarget(campaignId, targetType - 1, manager.LeaderID ?? 0);
                    }
                    break;
                default://mục tiêu cá nhân
                    if (manager.RoleID == 3)
                    {
                        mainTartget = TargetTable.getByMainTargetForRole23(campaignId, 3, manager.ID);
                    }
                    else
                    {
                        mainTartget = TargetTable.GetMainTarget(campaignId, targetType - 1, manager.LeaderID ?? 0);
                    }
                    break;
            }
            if (mainTartget == null)
            {
                switch (targetType)
                {
                    case 2:
                        SetAlert("Vui lòng tạo mục tiêu công ty trước", "e");
                        break;
                    case 3:
                        SetAlert("Vui lòng tạo mục tiêu phòng ban của trưởng nhóm này trước", "e");
                        break;
                    default:
                        SetAlert("Vui lòng tạo mục tiêu nhóm của tư vấn viên này trước này trước", "e");
                        break;
                }
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            var rs = TargetTable.insert(campaignId, mainTartget.ID, targetType, targetValue, aclog.UserName, 0, managerId, note);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Trong cùng một thời điểm chỉ được phép tồn tại một mục tiêu công ty", "e");
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

        public ActionResult EditTarget(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3 && aclog.RoleID != 6)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int id = f.Get("hdTarget").ToInt(0);
            var target = TargetTable.getbyid(id);
            if (target == null)
            {
                SetAlert("Mục tiêu không tồn tại", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            if (target.ManagerID != aclog.ID && aclog.RoleID == 6)
            {
                SetAlert("Bạn không có quyền chỉnh sửa mục tiêu này", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            string note = "";
            if (!string.IsNullOrEmpty(f.Get("txt-description-edit")))
            {
                note = f.Get("txt-description-edit").ToString();
            }
            double targetValue = Convert.ToDouble(f.Get("txt-targetvalue-edit").Replace(",", ""));
            var rs = TargetTable.update(id, targetValue, note, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Trong cùng một thời điểm chỉ được phép tồn tại một mục tiêu công ty", "e");
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }
        public ActionResult DeleteTarget(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3 && aclog.RoleID != 6)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var target = TargetTable.getbyid(id);
            if (target == null)
            {
                SetAlert("Mục tiêu không tồn tại", "e");
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            if (target.ManagerID != aclog.ID && aclog.RoleID == 6)
            {
                SetAlert("Bạn không có quyền xoá mục tiêu này", "e");
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            var rs = TargetTable.Delete(id, true, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else SetAlert("Thất bại", "e");
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult EditRevenue(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3)
                {
                    SetAlert("Bạn không có quyền thực hiện thao tác này", "e");
                    return RedirectToAction("Signin", "Login", new { area = "" });
                }

                var target = TargetTable.getbyid(f.Get("hdfid").ToInt());
                if (target == null)
                {
                    SetAlert("Mục tiêu không tồn tại", "e");
                    return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
                }
                double revenue = 0;
                if (!string.IsNullOrEmpty(f.Get("txt-revenue")))
                {
                    revenue = Convert.ToDouble(f.Get("txt-revenue"));
                }
                UpdateCampaignSale(target.ID, revenue - target.CurrentValue ?? 0);
                SetAlert("Thành công", "s");
                return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("CampaignSale", "EditRevenue", 0, e.Message + "||" + e.InnerException);
                SetAlert("Đã có lỗi xảy ra", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
            }
        }

        /// <summary>
        /// cập nhật doanh thu cho chiến dịch
        /// </summary>
        public static void UpdateCampaignSale(int targetId, double revenue)
        {
            try
            {
                // danh sách các ID mục tiêu cần update
                List<int> updateList = new List<int>();
                List<int> updateStatusList = new List<int>();

                using (var db = new ZimEntities())
                {
                    // lấy mục tiêu có managerID = ID tvv
                    var data = db.tbl_Target.Where(x => x.ID == targetId).FirstOrDefault();
                    if (data != null)
                    {
                        updateList.Add(data.ID);
                        do
                        {
                            var tempId = data.MainTargetID;
                            data = db.tbl_Target.Where(x => x.ID == tempId && x.CampaignID == data.CampaignID
                            && x.IsDone == false && x.IsHide == false).FirstOrDefault();
                            if (data != null)
                            {
                                updateList.Add(data.ID);
                            }
                        }
                        while (data != null);
                    }
                    updateList = updateList.Distinct().ToList();
                    // cập nhật doanh thu cho các mục tiêu
                    if (updateList.Count > 0)
                        db.Update_TargetRevenue(string.Join(",", updateList).ToString(), revenue);
                    updateStatusList.AddRange(updateList);
                }
                foreach (var u in updateStatusList)
                {
                    UpdateTargetStatus(u, revenue);
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("UpdateCampaignSale", "CampaignSale", 0, e.Message + " || " + e.InnerException + "params(" + targetId + ", " + revenue + ")");
            }
        }

        /// <summary>
        /// cập nhật trạng thái cho các mục tiêu
        /// 0 - chưa thực hiện, 1 - đúng tiến độ, 2 - xuất sắc, 3 - chậm tiến độ, 4 - rủi ro
        /// </summary>
        public static void UpdateTargetStatus(int targetId, double revenue)
        {
            try
            {
                var target = TargetTable.getbyid(targetId);
                var campaign = CampaignSaleTable.getbyid(target.CampaignID.Value);
                int currentDay = DateTime.Now.Day; // ngày hiện tại
                int totalDays = (campaign.eTime - campaign.sTime).Value.Days + 1;// tổng ngày trong chiến dịch
                double percentPerDay = 100 / (double)totalDays; // % hoàn thành/ 1 ngày
                double expectedPercent = percentPerDay * currentDay; // % phải hoàn thành dc
                double totalRevenue = target.CurrentValue ?? 0 + revenue; // tổng doanh thu
                double finishedPercent = (totalRevenue / target.TargetValue.Value) * 100; // % đã hoàn thành 
                                                                                          // hoàn thành mục tiêu trước hạn hoặc vượt mục tiêu
                if (finishedPercent > 100 || (finishedPercent == 100 && currentDay < totalDays))
                {
                    TargetTable.updateStatus(targetId, 2);
                }
                // hoàn thành mục tiêu trong khoảng +- 1 ngày
                else if (finishedPercent >= (expectedPercent - percentPerDay))
                {
                    TargetTable.updateStatus(targetId, 1);
                }
                // hoàn thành mục tiêu chậm 3 ngày
                else if (finishedPercent <= (expectedPercent - (percentPerDay * 3)) && finishedPercent > (expectedPercent - (percentPerDay * 7)))
                {
                    TargetTable.updateStatus(targetId, 3);
                }
                // hoàn thành mục tiêu chậm 7 ngày
                else if (finishedPercent <= (expectedPercent - (percentPerDay * 7)))
                {
                    TargetTable.updateStatus(targetId, 4);
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("UpdateTargetStatus", "CampaignSale", 0, e.Message + " || " + e.InnerException);
            }
        }
        #endregion
        #region chi phí các nền tảng của chiến dịch
        public ActionResult CostOfCampaign(int id, int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["txt-search"]))
            {
                search = ViewBag.Search = Request.QueryString["txt-search"].ToString();
            }
            ViewBag.Campaign = id;
            var l = new List<Admin_PlatformCampaign_GetList_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_PlatformCampaign_GetList(id, search, (page - 1) * pageSize, pageSize).ToList();
            }
            int total = 0;
            if (l.Count > 0)
                total = l[0].TotalRow.Value;
            ViewBag.Total = total;
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            return View(l);
        }
        public ActionResult AddCost(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            SetAlert("Thành công", "s");
            int campaignid = 0;
            if (!string.IsNullOrEmpty(f.Get("slPeriod")))
            {
                campaignid = f.Get("slPeriod").ToInt(0);
            }
            string name = "";
            if (!string.IsNullOrEmpty(f.Get("txt-name")))
            {
                name = f.Get("txt-name").ToString();
            }

            double expected = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-expected")))
            {
                expected = Convert.ToDouble(f.Get("txt-expected").ToString().Replace(",", ""));
            }

            double actual = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-actual")))
            {
                actual = Convert.ToDouble(f.Get("txt-actual").ToString().Replace(",", ""));
            }

            PlatformInCampaignTable.insert(campaignid, name, expected, aclog.UserName, actual);
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }
        public ActionResult EditCost(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            SetAlert("Thành công", "s");
            int id = f.Get("hdfID").ToInt(0);
            string name = "";
            if (!string.IsNullOrEmpty(f.Get("txt-name-edit")))
            {
                name = f.Get("txt-name-edit").ToString();
            }

            double expected = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-expected-edit")))
            {
                expected = Convert.ToDouble(f.Get("txt-expected-edit").ToString().Replace(",", ""));
            }

            double actual = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-actual-edit")))
            {
                actual = Convert.ToDouble(f.Get("txt-actual-edit").ToString().Replace(",", ""));
            }
            PlatformInCampaignTable.update(id, name, expected, aclog.UserName, actual);
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

        [HttpPost]
        public ActionResult DeleteCost(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var rs = PlatformInCampaignTable.Delete(id, true, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Đã xảy ra lỗi", "e");
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region lương tạm
        public ActionResult TempSalary()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var camps = CampaignSaleTable.getall().Where(x => x.CampaignType == 3).FirstOrDefault();
            if (camps == null)
            {
                SetAlert("Không có chiến dịch", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = Request.QueryString["search"].ToString();
            }
            ViewBag.Search = search;

            int campaign = camps.ID;
            if (!string.IsNullOrEmpty(Request.QueryString["slCampaign"]))
            {
                campaign = Request.QueryString["slCampaign"].ToString().ToInt(0);
            }
            ViewBag.Campaign = campaign;
            var l = new ConcurrentBag<Admin_SalaryReview_GetList_Result>();
            using (var _db = new ZimEntities())
            {
                // chỉ lấy ds nhân viên có target trong chiến dịch này
                var accounts = _db.Admin_Account_GetHasTargetInCampaign(campaign).Where(x => x.FullName.Contains(search) || search == "").ToList();
                if (aclog.RoleID == 6)
                    accounts = accounts.Where(x => x.ID == aclog.ID).ToList();
                Parallel.ForEach(accounts, a =>
                {
                    var db = new ZimEntities();
                    double revenue = 0;//doanh thu
                    double revenuePercent = 0;// % doanh thu
                    int careQuantity = 0;
                    int carePrice = 0;
                    int carePlusQuantity = 0;
                    int carePlusPrice = 0;
                    if (a.RoleID == 2)
                    {
                        // lấy các mục tiêu phòng ban của nhân viên này
                        var targets = TargetTable.getByManagerId(a.ID, campaign).ToList();
                        foreach (var t in targets)
                        {
                            if (t.TargetType == 2)
                            {
                                revenue += t.CurrentValue.Value;
                                var commitments = db.Admin_CommitmentSold(t.ID).ToList();
                                if (commitments.Count > 0)
                                {
                                    var carePlus = commitments.Where(x => x.CommitmentID == 2).FirstOrDefault();
                                    if (carePlus != null)
                                    {
                                        carePlusQuantity = carePlus.Quantity ?? 0;
                                        carePlusPrice = carePlus.Price ?? 0;
                                    }
                                    var care = commitments.Where(x => x.CommitmentID == 1).FirstOrDefault();
                                    if (care != null)
                                    {
                                        careQuantity = care.Quantity ?? 0;
                                        carePrice = care.Price ?? 0;
                                    }
                                }
                            }
                        }
                        // lấy % thưởng doanh thu
                        var rp = db.tbl_SalarySetting.Where(x => x.ID == a.Position).FirstOrDefault();
                        if (rp != null)
                        {
                            revenuePercent = rp.PercentSalary ?? 0;
                        }
                        // lấy % thưởng thêm nếu có
                        var bonusPercent = db.tbl_TargetBonus.Where(x => x.IsHide == false && x.TargetRevenue < revenue && x.TargetType == 3).OrderByDescending(x => x.TargetRevenue).FirstOrDefault();
                        if (bonusPercent != null)
                        {
                            revenuePercent += bonusPercent.TargetFinishPercent ?? 0;
                        }
                    }
                    else if (a.RoleID == 3)
                    {
                        // lấy các mục tiêu nhóm của nhân viên này
                        var targets = TargetTable.getByManagerId(a.ID, campaign).ToList();
                        foreach (var t in targets)
                        {
                            if (t.TargetType == 3)
                            {
                                revenue += t.CurrentValue.Value;
                                var commitments = db.Admin_CommitmentSold(t.ID).ToList();

                                if (commitments.Count > 0)
                                {
                                    var carePlus = commitments.Where(x => x.CommitmentID == 2).FirstOrDefault();
                                    if (carePlus != null)
                                    {
                                        carePlusQuantity = carePlus.Quantity ?? 0;
                                        carePlusPrice = carePlus.Price ?? 0;
                                    }
                                    var care = commitments.Where(x => x.CommitmentID == 1).FirstOrDefault();
                                    if (care != null)
                                    {
                                        careQuantity = care.Quantity ?? 0;
                                        carePrice = care.Price ?? 0;
                                    }
                                }
                            }
                        }
                        // lấy % thưởng doanh thu
                        var rp = db.tbl_SalarySetting.Where(x => x.ID == a.Position).FirstOrDefault();
                        if (rp != null)
                        {
                            revenuePercent = rp.PercentSalary ?? 0;
                        }
                        // lấy % thưởng thêm nếu có
                        var bonusPercent = db.tbl_TargetBonus.Where(x => x.IsHide == false && x.TargetRevenue < revenue && x.TargetType == 3).OrderByDescending(x => x.TargetRevenue).FirstOrDefault();
                        if (bonusPercent != null)
                        {
                            revenuePercent += bonusPercent.TargetFinishPercent ?? 0;
                        }
                    }
                    else
                    {
                        // lấy các mục tiêu của nhân viên này
                        var targets = TargetTable.getByManagerId(a.ID, campaign).ToList();
                        foreach (var t in targets)
                        {
                            revenue += t.CurrentValue.Value;
                            var commitments = db.Admin_CommitmentSold(t.ID).ToList();
                            if (commitments.Count > 0)
                            {
                                var carePlus = commitments.Where(x => x.CommitmentID == 2).FirstOrDefault();
                                if (carePlus != null)
                                {
                                    carePlusQuantity = carePlus.Quantity ?? 0;
                                    carePlusPrice = carePlus.Price ?? 0;
                                }
                                var care = commitments.Where(x => x.CommitmentID == 1).FirstOrDefault();
                                if (care != null)
                                {
                                    careQuantity = care.Quantity ?? 0;
                                    carePrice = care.Price ?? 0;
                                }
                            }
                        }
                        // lấy % thưởng doanh thu
                        var rp = db.tbl_SalarySetting.Where(x => x.ID == a.Position).FirstOrDefault();
                        if (rp != null)
                        {
                            revenuePercent = rp.PercentSalary ?? 0;
                        }
                        // lấy % thưởng thêm nếu có
                        var bonusPercent = db.tbl_TargetBonus.Where(x => x.IsHide == false && x.TargetRevenue < revenue && x.TargetType == 4).OrderByDescending(x => x.TargetRevenue).FirstOrDefault();
                        if (bonusPercent != null)
                        {
                            revenuePercent += bonusPercent.TargetFinishPercent ?? 0;
                        }
                    }

                    l.Add(new Admin_SalaryReview_GetList_Result
                    {
                        TotalRows = 0,
                        ID = 0,
                        UID = a.ID,
                        CampaignID = campaign,
                        FullName = a.FullName,
                        BasicSalary = a.Salary ?? 0,
                        Revenue = revenue,
                        RevenuePercent = revenuePercent,
                        BonusSalary = 0,
                        DeductedSalary = 0,
                        CareQuantity = careQuantity,
                        CarePrice = carePrice,
                        CarePlusQuantity = carePlusQuantity,
                        CarePlusPrice = carePlusPrice,
                        Note = "",
                        IsDone = false,
                        BankInformation = ""
                    });
                });
            }
            return View(l.OrderBy(x => x.FullName).ToList());
        }
        public ActionResult TempSalaryDetail(int id, int campaignId, int page = 1)
        {
            int pageSize = 20;
            var l = new List<Admin_RevenueDetail_GetByTarget_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_RevenueDetail_GetByTarget(id, campaignId, (page - 1) * pageSize, pageSize).ToList();
            }
            int total = 0;
            if (l.Count > 0)
                total = l[0].TotalRow.Value;
            ViewBag.Total = total;
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            return View(l);
        }
        #endregion


        /// <summary>
        /// tạo chu kỳ + mục tiêu tự động vào đầu mỗi tháng
        /// </summary>
        public static void CreatePeriod()
        {
            try
            {
                var now = DateTime.Now.Date;
                var firstDay = DateTime.Now.FirstDayOfMonth().Date;
                var lastDay = firstDay.LastDayOfMonth();
                if (now == firstDay)
                {
                    //kiểm tra xem tạo chưa 
                    var check = CampaignSaleTable.CheckExists(firstDay, lastDay, 3);
                    if (check == null)
                    {
                        // tạo chu kỳ tháng
                        var period = CampaignSaleTable.insert("T" + now.ToString("M/yyyy"), firstDay, firstDay.LastDayOfMonth(), "", "Auto", 3, 1);
                        if (period != null)
                        {
                            // tạo mục tiêu công ty
                            var companyTarget = TargetTable.insert(period.ID, 0, 1, 0, "Auto", 0, 1, "");
                            // tạo mục tiêu phòng ban
                            var departments = AccountTable.getbyrole(2);
                            foreach (var d in departments)
                            {
                                var mainTemp = TargetTable.insert(period.ID, companyTarget.ID, 2, 0, "Auto", 0, d.ID, "");
                                if (mainTemp != null)
                                    TargetTable.insert(period.ID, mainTemp.ID, 4, 0, "Auto", 0, d.ID, "");
                            }
                            // tạo mục tiêu nhóm
                            var teams = AccountTable.getbyrole(3);
                            foreach (var t in teams)
                            {
                                var managerid = 0;
                                if (t.LeaderID != null)
                                {
                                    managerid = t.LeaderID.Value;
                                }
                                var mainTarget = TargetTable.GetMainTarget(period.ID, 2, managerid);
                                if (mainTarget != null)
                                {
                                    var mainTemp2 = TargetTable.insert(period.ID, mainTarget.ID, 3, 0, "Auto", 0, t.ID, "");
                                    if (mainTemp2 != null)
                                        TargetTable.insert(period.ID, mainTemp2.ID, 4, 0, "Auto", 0, t.ID, "");
                                }
                            }
                            // tạo mục tiêu cá nhân
                            var members = AccountTable.getbyrole(6);
                            foreach (var m in members)
                            {
                                var managerid = 0;
                                if (m.LeaderID != null)
                                {
                                    managerid = m.LeaderID.Value;
                                }
                                var mainTarget = TargetTable.GetMainTarget(period.ID, 3, managerid);
                                if (mainTarget != null)
                                    TargetTable.insert(period.ID, mainTarget.ID, 4, 0, "Auto", 0, m.ID, "");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("PushAutoController", "CreatePeriod", 0, ex.Message + "||" + ex.InnerException);
            }
        }

        /// <summary>
        /// Kiểm tra tổng doanh thu trong chiến dịch tháng đó để đối chiếu điều kiện xuống rank
        /// </summary>
        public static void CheckRevenueOfCampaign()
        {
            try
            {
                using (var db = new ZimEntities())
                {
                    //Lấy chiến dịch hôm nay hết hạn
                    var now = DateTime.Now.Date;
                    var campaignIds = db.tbl_CampaignSale.Where(x => x.IsHide == false && x.eTime == now).Select(x => x.ID).ToList();
                    if (campaignIds == null || !campaignIds.Any())
                        return;
                    //Lấy doanh thu các tư vấn viên trong chiến dịch             
                    var target = db.Admin_GetRevenueOfSellerByCampaignId(string.Join(",", campaignIds)).ToList();
                    if (target == null || !target.Any())
                        return;
                    foreach (var item in target)
                    {
                        //check tvv này đã xuống rank chưa
                        var checkDownrank = db.tbl_ApprovalToLevelDown.Where(x => x.campaignId == item.CampaignID && x.UID == item.ManagerID).FirstOrDefault();
                        if (checkDownrank != null)
                            continue;
                        int newPosition = item.Position ?? 0;
                        //Lấy danh sách các level trở xuống
                        var levels = db.tbl_SalarySetting.Where(x => x.isHide == false && x.Million <= item.Million && x.MinimumMonthlyRevenue != null && x.MinimumMonthlyRevenue != 0).OrderByDescending(x => x.MinimumMonthlyRevenue).ToList();
                        for (int i = 0; i < levels.Count; i++)
                        {
                            //Nếu mà doanh thu nhỏ hơn mức tối thiểu
                            if (levels[i].MinimumMonthlyRevenue > item.CurrentValue)
                            {
                                newPosition = levels[i+1].ID;
                                //Nếu có thay đổi rank thì thoát khỏi vòng lặp
                                if (newPosition != item.Position)
                                {
                                    //Nếu đủ điều kiện xuống cấp
                                    ApprovalToLevelDownTable.insert(item.ManagerID.Value, item.CurrentValue.Value, item.Position.Value, newPosition, item.CampaignID.Value);                                   
                                    break;
                                }
                            }                              
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("CampaignSale", "CheckRevenueOfCampaign", 0, e.Message + " || " + e.InnerException);
            }
        }
    }
}