using AppZim.TableSql;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AppZim.ZIM;
using AppZim.Models;

namespace AppZim.Areas.Admin.Controllers
{
    public class TopicController : Controller
    {
        // GET: Admin/Topic
        public ActionResult ListTopic()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            List<tbl_Topic> lt = TopicTable.getall();
            return View(lt);
        }
        [HttpPost]
        public ActionResult AddTopic(int id, string tn, string td)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Đã hết phiên làm việc!" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Đã hết phiên làm việc!" }, JsonRequestBehavior.AllowGet);
            if (string.IsNullOrEmpty(tn))
            {
                return Json(new { rs = false, message = "Vui lòng nhập tên chủ đề!" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(td))
            {
                return Json(new { rs = false, message = "Vui lòng nhập mô tả!" }, JsonRequestBehavior.AllowGet);
            }
            var tp = TopicTable.getbyid(id);
            if (tp == null)
            {
                tp = TopicTable.insert(tn, td, aclog.UserName);
                if (tp == null)
                {
                    return Json(new { rs = false, message = "Đã xảy ra lỗi!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    StringBuilder html = new StringBuilder();
                    string description = AssetCRM.RemoveHTMLTags(tp.TopicDescription);
                    html.Append("<tr data-id=\"" + tp.ID + "\" id=\"tr-topic-" + tp.ID + "\" class=\"tr-row\">");
                    html.Append("    <td class=\"text-center\"><a href=\"javascript:;\" data-id=\"" + tp.ID + "\" class=\"collapse-toggle topic-details\"><i class=\"fas fa-plus-square\"></i></a></td>");
                    html.Append("    <td>" + tp.TopicName + "</td>");
                    html.Append("    <td>" + (description.Length > 100 ? description.Substring(0, 100) : description) + "</td>");
                    html.Append("<td>");
                    html.Append("    <div class=\"custom-control custom-switch\">");
                    html.Append("        <input type=\"checkbox\" checked class=\"custom-control-input chk-status\" id=\"chk-show-" + tp.ID + "\" data-id=\"" + tp.ID + "\">");
                    html.Append("        <label class=\"custom-control-label\" for=\"chk-show-@item.ID\">Hoạt động</label>");
                    html.Append("    </div>");
                    html.Append("</td>");
                    html.Append("    <td>");
                    html.Append("        <div class=\"dropdown dropdown-icon\">");
                    html.Append("            <a href=\"javascript:;\" class=\"material-icons\" id=\"drop-down-1\"");
                    html.Append("               data-toggle=\"dropdown\" aria-haspopup=\"true\"");
                    html.Append("               aria-expanded=\"false\">more_vert</a>");
                    html.Append("            <div class=\"dropdown-menu\" aria-labelledby=\"drop-down-1\">");
                    html.Append("                <a class=\"dropdown-item btn-icon edit-topic\" href=\"javascript:;\"   data-id=\"" + tp.ID + "\" data-toggle=\"modal\" data-target=\"#js-edit-group\"><i data-feather=\"edit\"></i>Chỉnh sửa</a>");
                    html.Append("                <a class=\"dropdown-item btn-icon delete-topic\" href=\"javascript:;\"  data-id=\"" + tp.ID + "\"><i data-feather=\"trash\"></i>Xóa</a>");
                    html.Append("            </div>");
                    html.Append("        </div>");
                    html.Append("    </td>");
                    html.Append("</tr>");
                    html.Append("<tr class=\"collapse-row\">");
                    html.Append("    <td colspan=\"6\">");
                    html.Append("        <div class=\"collapse-content pd-y-15-f\">");
                    html.Append("            <div>");
                    html.Append("                <h5 class=\"mg-b-15 mg-t-10\">Danh sách câu hỏi</h5>");
                    html.Append("            </div>");
                    html.Append("            <div class=\"table-responsive\">");
                    html.Append("                <table class=\"table table-bordered table-question\">");
                    html.Append("                    <thead class=\"thead-dark\">");
                    html.Append("                        <tr>");
                    html.Append("                            <th><span class=\"nowrap\">Part/Task</span></th>");
                    html.Append("                            <th class=\"quest-title\">Question</th>");
                    html.Append("                            <th class=\"wd-50\"></th>");
                    html.Append("                        </tr>");
                    html.Append("                    </thead>");
                    html.Append("                    <tbody id=\"tbody-question-" + tp.ID + "\">");
                    html.Append("                    </tbody>");
                    html.Append("                </table>");
                    html.Append("            </div>");
                    html.Append("            <div class=\"mg-t-15\">");
                    html.Append("                <a data-id=\"" + tp.ID + "\" href=\"javascript:;\" class=\"btn btn-icon btn-info add-question\"><i data-feather=\"plus\" class=\"left\"></i>Thêm câu hỏi</a>");
                    html.Append("            </div>");
                    html.Append("        </div>");
                    html.Append("    </td>");
                    html.Append("</tr>");
                    return Json(new { rs = true, message = "Thành công!", html = html.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                tp = TopicTable.update(id, tn, td, aclog.UserName);
                if (tp != null)
                {
                    StringBuilder html = new StringBuilder();

                    string description = AssetCRM.RemoveHTMLTags(tp.TopicDescription);
                    html.Append("    <td class=\"text-center\"><a href=\"javascript:;\" data-id=\"" + tp.ID + "\" class=\"collapse-toggle topic-details\"><i class=\"fas fa-plus-square\"></i></a></td>");

                    html.Append("    <td>" + tp.TopicName + "</td>");
                    html.Append("    <td>" + (description.Length > 100 ? description.Substring(0, 100) : description) + "</td>");
                    html.Append("<td>");
                    html.Append("    <div class=\"custom-control custom-switch\">");
                    if (tp.Status == 1)
                    {
                        html.Append("        <input type=\"checkbox\" checked class=\"custom-control-input chk-status\" id=\"chk-show-" + tp.ID + "\" data-id=\"" + tp.ID + "\">");
                    }
                    else
                    {
                        html.Append("        <input type=\"checkbox\" class=\"custom-control-input chk-status\" id=\"chk-show-" + tp.ID + "\" data-id=\"" + tp.ID + "\">");
                    }
                    html.Append("        <label class=\"custom-control-label\" for=\"chk-show-@item.ID\">Hoạt động</label>");
                    html.Append("    </div>");
                    html.Append("</td>");
                    html.Append("    <td>");
                    html.Append("        <div class=\"dropdown dropdown-icon\">");
                    html.Append("            <a href=\"javascript:;\" class=\"material-icons\" id=\"drop-down-1\"");
                    html.Append("               data-toggle=\"dropdown\" aria-haspopup=\"true\"");
                    html.Append("               aria-expanded=\"false\">more_vert</a>");
                    html.Append("            <div class=\"dropdown-menu\" aria-labelledby=\"drop-down-1\">");
                    html.Append("                <a class=\"dropdown-item btn-icon edit-topic\" href=\"javascript:;\" data-id=\"" + tp.ID + "\" data-toggle=\"modal\" data-target=\"#js-edit-group\"><i data-feather=\"edit\"></i>Chỉnh sửa</a>");
                    html.Append("                <a class=\"dropdown-item btn-icon delete-topic\" href=\"javascript:;\" data-id=\"" + tp.ID + "\"><i data-feather=\"trash\"></i>Xóa</a>");
                    html.Append("            </div>");
                    html.Append("        </div>");
                    html.Append("    </td>");
                    return Json(new { rs = true, message = "Thành công!", html = html.ToString() }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { rs = false, message = "Fails to update!" }, JsonRequestBehavior.AllowGet);
                }
            }

        }
        [HttpPost]
        public ActionResult LoadTopicDetail(int id)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Đã hết phiên làm việc!" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Đã hết phiên làm việc!" }, JsonRequestBehavior.AllowGet);
            tbl_Topic tp = TopicTable.getbyid(id);
            if (tp == null)
            {
                return Json(new { rs = false, message = "Không tìm thấy!" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { rs = true, tp = tp }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult AddQuestion(int tpid, int qid, string part, string question)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Đã hết phiên làm việc!" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Đã hết phiên làm việc!" }, JsonRequestBehavior.AllowGet);
            tbl_Topic tp = TopicTable.getbyid(tpid);
            if (tp == null)
            {
                return Json(new { rs = false, message = "Không tìm thấy chủ đề!" }, JsonRequestBehavior.AllowGet);
            }
            int type = 1;
            if (part.ToInt(0) == 1 || part.ToInt(0) == 2 || part.ToInt(0) == 3)
            {
                type = 3;
            }
            tbl_Exercise ex = ExerciseTable.getbyid(qid);
            if (ex == null)
            {
                ex = ExerciseTable.insertTopicQuestion(tp.ID, part.ToInt(1), question, "", "", type, aclog.UserName);
                if (ex == null)
                {
                    return Json(new { rs = false, message = "Tạo câu hỏi thất bại!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string partname = "";
                    if (ex.PartNumber == 1)
                    {
                        partname = "Part 1";
                    }
                    if (ex.PartNumber == 2)
                    {
                        partname = "Part 2";
                    }
                    if (ex.PartNumber == 3)
                    {
                        partname = "Part 3";
                    }
                    if (ex.PartNumber == 4)
                    {
                        partname = "Task One";
                    }
                    if (ex.PartNumber == 5)
                    {
                        partname = "Task Two";
                    }
                    int number = ExerciseTable.getListTopicQuestion(tp.ID).FindIndex(n => n.ID == ex.ID) + 1;
                    string QS = AssetCRM.RemoveHTMLTags(ex.ExerciseTitle);
                    StringBuilder html = new StringBuilder();
                    html.Append("<tr id=\"question-" + ex.ID + "\" data-id=\"" + ex.ID + "\">");
                    html.Append("    <td class=\"text-center wd-50\">" + number + "</td>");
                    html.Append("    <td>" + partname + "</td>");
                    html.Append("    <td>" + (QS.Length > 150 ? QS.Substring(0, 150) : QS) + "</td>");
                    html.Append("    <td class=\"wd-50\">");
                    html.Append("<div class=\"dropdown dropdown-icon\">");
                    html.Append("    <a href=\"javascript:;\" class=\"material-icons\" id=\"drop-down-1\"");
                    html.Append("       data-toggle=\"dropdown\" aria-haspopup=\"true\"");
                    html.Append("       aria-expanded=\"false\">more_vert</a>");
                    html.Append("    <div class=\"dropdown-menu\" aria-labelledby=\"drop-down-1\">");
                    html.Append("        <a class=\"dropdown-item btn-icon edit-question\" href=\"javascript:;\" data-topicID=\"" + ex.TopicID + "\"  data-id=\"" + ex.ID + "\"><i data-feather=\"edit\"></i>Sửa</a>");
                    html.Append("        <a class=\"dropdown-item btn-icon delete-question\" href=\"javascript:;\"  data-topicID=\"" + ex.TopicID + "\"  data-id=\"" + ex.ID + "\"><i data-feather=\"trash\"></i>Xóa</a>");
                    html.Append("    </div>");
                    html.Append("</div>");
                    html.Append("    </td>");
                    html.Append("</tr>");
                    return Json(new { rs = true, message = "Thành công!", html = html.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                ex = ExerciseTable.updateTopicQuestion(ex.ID, part.ToInt(1), question, aclog.UserName);
                if (ex == null)
                {
                    return Json(new { rs = false, message = "Tạo câu hỏi thất bại!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string partname = "";
                    if (ex.PartNumber == 1)
                    {
                        partname = "Part 1";
                    }
                    if (ex.PartNumber == 2)
                    {
                        partname = "Part 2";
                    }
                    if (ex.PartNumber == 3)
                    {
                        partname = "Part 3";
                    }
                    if (ex.PartNumber == 4)
                    {
                        partname = "Task One";
                    }
                    if (ex.PartNumber == 5)
                    {
                        partname = "Task Two";
                    }
                    string QS = AssetCRM.RemoveHTMLTags(ex.ExerciseTitle);
                    StringBuilder html = new StringBuilder();
                    int number = ExerciseTable.getListTopicQuestion(tp.ID).FindIndex(n => n.ID == ex.ID) + 1;
                    html.Append("    <td class=\"text-center wd-50\">" + number + "</td>");
                    html.Append("    <td>" + partname + "</td>");
                    html.Append("    <td>" + (QS.Length > 150 ? QS.Substring(0, 150) : QS) + "</td>");
                    html.Append("    <td class=\"wd-50\">");
                    html.Append("<div class=\"dropdown dropdown-icon\">");
                    html.Append("    <a href=\"javascript:;\" class=\"material-icons\" id=\"drop-down-1\"");
                    html.Append("       data-toggle=\"dropdown\" aria-haspopup=\"true\"");
                    html.Append("       aria-expanded=\"false\">more_vert</a>");
                    html.Append("    <div class=\"dropdown-menu\" aria-labelledby=\"drop-down-1\">");
                    html.Append("        <a class=\"dropdown-item btn-icon edit-question\" data-topicID=\"" + ex.TopicID + "\"  href=\"javascript:;\"  data-id=\"" + ex.ID + "\"><i data-feather=\"edit\"></i>Sửa</a>");
                    html.Append("        <a class=\"dropdown-item btn-icon delete-question\" data-topicID=\"" + ex.TopicID + "\"  href=\"javascript:;\" data-id=\"" + ex.ID + "\"><i data-feather=\"trash\"></i>Xóa</a>");
                    html.Append("    </div>");
                    html.Append("</div>");
                    html.Append("    </td>");
                    return Json(new { rs = true, message = "Thành công!", html = html.ToString() }, JsonRequestBehavior.AllowGet);
                }

            }
        }
        [HttpPost]
        public ActionResult ActiveTopic(int id)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Phiên làm việc đã hết hạn!" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Phiên làm việc đã hết hạn!" }, JsonRequestBehavior.AllowGet);
            tbl_Topic tp = TopicTable.getbyid(id);
            if (tp == null)
            {
                return Json(new { rs = false, message = "Không tìm thấy!" }, JsonRequestBehavior.AllowGet);
            }
            int currentStatus = tp.Status.Value;
            if (currentStatus == 1)
            {
                currentStatus = 2;
            }
            else
            {
                currentStatus = 1;
            }
            tp = TopicTable.active(id, currentStatus, aclog.UserName);
            return Json(new { rs = true, message = "Thành công!" }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult LoadQuestionDetail(int id)
        {

            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Phiên làm việc đã hết hạn!" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Phiên làm việc đã hết hạn!" }, JsonRequestBehavior.AllowGet);
            tbl_Exercise ex = ExerciseTable.getbyid(id);
            if (ex == null)
            {
                return Json(new { rs = false, message = "Không tìm thấy!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { rs = true, message = "Thành công!", ex = ex }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult DeleteTopic(int id)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Phiên làm việc đã hết hạn!" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Phiên làm việc đã hết hạn!" }, JsonRequestBehavior.AllowGet);
            tbl_Topic tp = TopicTable.getbyid(id);
            if (tp == null)
            {
                return Json(new { rs = false, message = "Không tìm thấy!" }, JsonRequestBehavior.AllowGet);
            }
            tp = TopicTable.delete(id, aclog.UserName);
            return Json(new { rs = true, message = "Thành công!" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteQuestion(int id)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Phiên làm việc đã hết hạn!" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Phiên làm việc đã hết hạn!" }, JsonRequestBehavior.AllowGet);
            tbl_Exercise ex = ExerciseTable.getbyid(id);
            if (ex == null)
            {
                return Json(new { rs = false, message = "Không tìm thấy!" }, JsonRequestBehavior.AllowGet);
            }
            ex = ExerciseTable.update(ex.ID, 0, ex.ExerciseTitle, true, aclog.UserName, ex.ParentQuestionID.Value,ex.DescribeAnswer);
            return Json(new { rs = true, message = "Thành công!" }, JsonRequestBehavior.AllowGet);
        }
    }
}