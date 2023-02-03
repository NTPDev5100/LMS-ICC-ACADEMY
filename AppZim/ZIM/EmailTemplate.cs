using AppZim.Areas.Admin.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace AppZim.ZIM
{
    public class EmailTemplate
    {
        public static string DefaultTemplate_Base64 = "PCFET0NUWVBFIGh0bWwKICBQVUJMSUMgIi0vL1czQy8vRFREIFhIVE1MIDEuMCBUcmFuc2l0aW9uYWwvL0VOIiAiaHR0cDovL3d3dy53My5vcmcvVFIveGh0bWwxL0RURC94aHRtbDEtdHJhbnNpdGlvbmFsLmR0ZCI+CjxodG1sIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hodG1sIj4KCjxoZWFkPgogIDxtZXRhIGh0dHAtZXF1aXY9IkNvbnRlbnQtVHlwZSIgY29udGVudD0idGV4dC9odG1sOyBjaGFyc2V0PVVURi04IiAvPgogIDx0aXRsZT48L3RpdGxlPgogIDxtZXRhIG5hbWU9InZpZXdwb3J0IiBjb250ZW50PSJ3aWR0aD1kZXZpY2Utd2lkdGgsIGluaXRpYWwtc2NhbGU9MS4wIiAvPgo8L2hlYWQ+Cgo8Ym9keSBzdHlsZT0nbWFyZ2luOjA7cGFkZGluZzowOyAnPgogIDxsaW5rIHJlbD0ic3R5bGVzaGVldCIgaHJlZj0iaHR0cHM6Ly9zdGFja3BhdGguYm9vdHN0cmFwY2RuLmNvbS9mb250LWF3ZXNvbWUvNC43LjAvY3NzL2ZvbnQtYXdlc29tZS5taW4uY3NzIj4KICA8dGFibGUgYm9yZGVyPSIwIiBjZWxscGFkZGluZz0iMCIgY2VsbHNwYWNpbmc9IjAiIHdpZHRoPSIxMDAlIgogICAgc3R5bGU9J2ZvbnQtc2l6ZToxNHB4OyBmb250LWZhbWlseTotYXBwbGUtc3lzdGVtLCBCbGlua01hY1N5c3RlbUZvbnQsICJTZWdvZSBVSSIsIFJvYm90bywgSGVsdmV0aWNhLCBBcmlhbCwgc2Fucy1zZXJpZiwgIkFwcGxlIENvbG9yIEVtb2ppIiwgIlNlZ29lIFVJIEVtb2ppIiwgIlNlZ29lIFVJIFN5bWJvbCI7Jz4KICAgIDx0cj4KICAgICAgPHRkPgogICAgICAgIDx0YWJsZSBhbGlnbj0iY2VudGVyIiBib3JkZXI9IjAiIGNlbGxwYWRkaW5nPSIwIiBjZWxsc3BhY2luZz0iMCIKICAgICAgICAgIHN0eWxlPSJib3JkZXItY29sbGFwc2U6Y29sbGFwc2U7IG1heC13aWR0aDogODAwcHg7Ij4KICAgICAgICAgIDx0ciBjbGFzcz0iYm9keS1yb3ciPgogICAgICAgICAgICA8dGQgc3R5bGU9InBhZGRpbmc6MTVweDsgbGluZS1oZWlnaHQ6IDEuNTsgY29sb3I6ICMyMjI7Zm9udC1zaXplOjE2cHg7Ij4KICAgICAgICAgICAgICB7bWFpbC1jb250ZW50fQogICAgICAgICAgICA8L3RkPgogICAgICAgICAgPC90cj4KICAgICAgICA8L3RhYmxlPgogICAgICA8L3RkPgogICAgPC90cj4KICA8L3RhYmxlPgo8L2JvZHk+CjwvaHRtbD4=";

        /// <summary>
        /// Email xác nhận học viên đã đặt lịch học qua trang Học Viên
        /// </summary>
        /// <param name="StudentName">Tên học viên</param>
        /// <param name="TeacherName">Tên giáo viên</param>
        /// <param name="date">Ngày giờ học</param>
        /// <returns></returns>
        public static string ConfirmBooking(string StudentName, string TeacherName, string date)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(DefaultTemplate_Base64);
            string mailContent = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            StringBuilder html = new StringBuilder();
            html.Append("<p align=\"center\" style=\"font-size:24px;font-weight:800;\">Bạn đã đặt Buổi Học tại " + BaseController.ProjectName + "</p>");
            html.Append("<p style=\"font-weight:bold; margin: 8px 0;margin-bottom:15px;font-size:18px;\">Thân Chào " + StudentName + ",</p>");
            html.Append("<p style=\"margin: 8px 0\">Bạn vừa đặt thành công buổi học tại <strong>" + BaseController.ProjectName + "</strong> với giáo viên " + TeacherName + " vào lúc " + date + ".</p>");
            html.Append("<p style=\"margin: 8px 0\">Bạn vui lòng truy cập trang <strong><a href=\"http://ct-ieclanguage.monamedia.net/\" target=\"_blank\" style=\"color: blue\">" + BaseController.ProjectName + "</a></strong> và chuẩn bị vào lớp trước 5 phút nhé.</p>");
            html.Append("<p style=\"margin: 8px 0\">Chúc bạn có một buổi học thật thú vị và bổ ích!</p>");
            html.Append("<p style=\"margin: 8px 0\">Trân trọng,</p>");
            html.Append("<p style=\"margin: 8px 0\">" + BaseController.ProjectName + "</p>");
            html.Append("<hr>");

            return mailContent.Replace("{mail-content}", html.ToString());
        }


        /// <summary>
        /// Email nhắc vào học trước giờ học 30 phút
        /// </summary>
        /// <param name="StudentName">Tên học viên</param>
        /// <returns></returns>
        public static string ReminderStudent(string StudentName)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(DefaultTemplate_Base64);
            string mailContent = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            StringBuilder html = new StringBuilder();
            html.Append("<p align=\"center\" style=\"font-size:24px;font-weight:800;\">Lớp Học Sẽ Bắt Đầu Trong 30 phút</p>");
            html.Append("<p style=\"font-weight:bold; margin: 8px 0;margin-bottom:15px;font-size:18px;\">Thân chào " + StudentName + ",</p>");
            html.Append("<p style=\"margin: 8px 0\">Lớp học của bạn sẽ bắt đầu trong vòng <strong>30 phút</strong> nữa. Bạn vui lòng truy cập trang <strong><a href=\"" + BaseController.ProjectDomain + " target =\"_blank\" style=\"color: blue\">" + BaseController.ProjectName + "</a></strong> để xem lịch học và chuẩn bị vào lớp trước 5 phút nhé. Chúc bạn có một buổi học thật thú vị và bổ ích!</p>");
            html.Append("<p style=\"margin: 8px 0\">Trân trọng,</p>");
            html.Append("<p style=\"margin: 8px 0\">" + BaseController.ProjectName + " VN</p>");
            html.Append("<hr>");
            html.Append("<p style=\"margin: 8px 0\">This is an automated email.  Please do not reply.</p>");
            return mailContent.Replace("{mail-content}", html.ToString());
        }


        /// <summary>
        /// Email xác nhận Học viên đã Hủy buổi học
        /// </summary>
        /// <param name="StudentName">Tên học viên</param>
        /// <param name="TeacherName">Tên giáo viên</param>
        /// <param name="date">Ngày giờ học</param>
        /// <returns></returns>
        public static string ConfirmCancel(string StudentName, string TeacherName, string date)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(DefaultTemplate_Base64);
            string mailContent = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            StringBuilder html = new StringBuilder();
            html.Append("<p align=\"center\" style=\"font-size:24px;font-weight:800;\">Buổi Học CỦa Bạn Đã Được Hủy</p>");
            html.Append("<p style=\"font-weight:bold; margin: 8px 0;margin-bottom:15px;font-size:18px;\">Thân chào " + StudentName + ",</p>");
            html.Append("<p style=\"margin: 8px 0\">Buổi học của bạn vào " + date + " với giáo viên " + TeacherName + " đã được hủy.</p>");
            html.Append("<p style=\"margin: 8px 0\">Xin vui lòng đặt buổi học khác phù hợp hơn với lịch học của bạn.</p>");
            html.Append("<p style=\"margin: 8px 0\">Trân trọng,</p>");
            html.Append("<p style=\"margin: 8px 0\">" + BaseController.ProjectName + " VN</p>");
            html.Append("<hr>");
            html.Append("<p style=\"margin: 8px 0\">This is an automated email.  Please do not reply.</p>");

            return mailContent.Replace("{mail-content}", html.ToString());
        }

        /// <summary>
        /// 4.	Email xác nhận học viên đã đặt lịch học qua trang Học Viên
        /// </summary>
        /// <param name="StudentName">Tên học viên</param>
        /// <param name="TeacherName">Tên giáo viên</param>
        /// <param name="date">Ngày giờ học</param>
        /// <param name="JoinLesson">Link đến trang lớp học</param>
        /// <returns></returns>
        public static string HaveSchedule(string StudentName, string TeacherName, string date)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(DefaultTemplate_Base64);
            string mailContent = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            StringBuilder html = new StringBuilder();
            html.Append("<p style=\"font-weight:bold; margin: 8px 0;margin-bottom:15px;font-size:18px;\">Hi Teacher " + TeacherName + ",</p>");
            html.Append("<p style=\"margin: 8px 0\">You have new lesson booking with student " + StudentName + " for " + date + " .</p>");
            html.Append("<p style=\"margin: 8px 0\">Please join the lesson at few minutes before the lesson starts.</p>");
            html.Append("<p style=\"margin: 8px 0\">" + BaseController.ProjectName + " VN.</p>");
            html.Append("<hr>");
            html.Append("<p style=\"margin: 8px 0\">This is an automated email.  Please do not reply.</p>");

            return mailContent.Replace("{mail-content}", html.ToString());
        }

        public static string TeacherCancelOpenedSlot(string TeacherName, string date)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(DefaultTemplate_Base64);
            string mailContent = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            StringBuilder html = new StringBuilder();
            html.Append("<p style=\"margin: 8px 0\">Teacher " + TeacherName + " closed opened slot " + date + " (timezone +0).</p>");
            html.Append("<p style=\"margin: 8px 0\">" + BaseController.ProjectName + " VN.</p>");
            html.Append("<hr>");
            html.Append("<p style=\"margin: 8px 0\">This is an automated email.  Please do not reply.</p>");

            return mailContent.Replace("{mail-content}", html.ToString());
        }

        public static string TeacherOpenSlot(string TeacherName, string date)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(DefaultTemplate_Base64);
            string mailContent = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            StringBuilder html = new StringBuilder();
            html.Append("<p style=\"margin: 8px 0\">Teacher " + TeacherName + " opened available slot " + date + " (timezone +0).</p>");
            html.Append("<p style=\"margin: 8px 0\">" + BaseController.ProjectName + " VN.</p>");
            html.Append("<hr>");
            html.Append("<p style=\"margin: 8px 0\">This is an automated email.  Please do not reply.</p>");

            return mailContent.Replace("{mail-content}", html.ToString());
        }

        /// <summary>
        /// Email nhắc vào dạy trước giờ học 30 phút
        /// </summary>
        /// <param name="StudentName">Tên học viên</param>
        /// <param name="TeacherName">Tên giáo viên</param>
        /// <param name="JoinLesson">Link đến trang lớp học</param>
        /// <returns></returns>
        public static string ReminderTeacher(string TeacherName, string JoinLesson)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(DefaultTemplate_Base64);
            string mailContent = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            StringBuilder html = new StringBuilder();
            html.Append("<p align=\"center\" style=\"font-size:24px;font-weight:800;\">Upcoming Class in 30 Mins</p>");
            html.Append("<p style=\"font-weight:bold; margin: 8px 0;margin-bottom:15px;font-size:18px;\">Hi Teacher " + TeacherName + ",</p>");
            html.Append("<p style=\"margin: 8px 0\">You have a class with student in 30 minutes.</p>");
            html.Append("<p style=\"margin: 8px 0\">Please join the lesson a few minutes before the lesson starts.</p>");
            html.Append("<p style=\"margin: 8px 0\">Join lesson <a href=\"" + JoinLesson + "\" target=\"_blank\" style=\"color: blue\">here</a>.</p>");
            html.Append("<p style=\"margin: 8px 0\">" + BaseController.ProjectName + " VN.</p>");
            html.Append("<hr>");
            html.Append("<p style=\"margin: 8px 0\">This is an automated email.  Please do not reply.</p>");
            return mailContent.Replace("{mail-content}", html.ToString());
        }

        /// <summary>
        /// Email xác nhận Học viên đã Hủy buổi học
        /// </summary>
        /// <param name="StudentName"></param>
        /// <param name="TeacherName"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ConfirmStudentCancel(string StudentName, string TeacherName, string date)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(DefaultTemplate_Base64);
            string mailContent = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            StringBuilder html = new StringBuilder();
            html.Append("<p style=\"font-weight:bold; margin: 8px 0;margin-bottom:15px;font-size:18px;\">Hi Teacher " + TeacherName + ",</p>");
            html.Append("<p style=\"margin: 8px 0\">Your coming class with student " + StudentName + " for " + date + " has been canceled.</p>");
            html.Append("<p style=\"margin: 8px 0\">" + BaseController.ProjectName + " VN.</p>");
            html.Append("<hr>");
            return mailContent.Replace("{mail-content}", html.ToString());
        }
        /// <summary>
        /// Email thông báo lịch đã được mở
        /// </summary>
        /// <returns></returns>
        public static string NotifySchedule(string fdate, string tdate)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(DefaultTemplate_Base64);
            string mailContent = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            StringBuilder html = new StringBuilder();
            html.Append("<p style=\"font-weight:bold; margin: 8px 0;margin-bottom:15px;font-size:18px;\">Xin chào các học viên,</p>");
            html.Append("<p style=\"margin: 8px 0\">Hiện tại lịch dạy của các giáo viên trong tuần " + fdate + " - " + tdate + " đã được mở, xin vui lòng vào đặt lịch.</p>");
            html.Append("<p style=\"margin: 8px 0\">" + BaseController.ProjectName + " VN.</p>");
            html.Append("<hr>");
            return mailContent.Replace("{mail-content}", html.ToString());
        }

        /// <summary>
        /// Email hóa đơn
        /// </summary>
        /// <returns></returns>
        public static string Invoice()
        {
            var base64EncodedBytes = System.Convert.FromBase64String(DefaultTemplate_Base64);
            string mailContent = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            StringBuilder html = new StringBuilder();
            html.Append("<p style=\"margin: 8px 0\">" + BaseController.ProjectName + " VN.</p>");
            html.Append("<hr>");
            return mailContent.Replace("{mail-content}", html.ToString());
        }
        /// <summary>
        /// thông báo tổng
        /// </summary>
        /// <returns></returns>
        public static string Notify(string content)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(DefaultTemplate_Base64);
            string mailContent = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            StringBuilder html = new StringBuilder();
            html.Append("<p align=\"center\" style=\"font-size:24px;font-weight:800;\">Thông báo từ trung tâm " + BaseController.ProjectName + "</p>");
            html.Append("<p style=\"font-weight:bold; margin: 8px 0;margin-bottom:15px;font-size:18px;\">Xin chào Anh/Chị,</p>");
            html.Append("<p style=\"margin: 8px 0\">" + content + "</p>");
            html.Append("<p style=\"margin: 8px 0\">Trân trọng,</p>");
            html.Append("<p style=\"margin: 8px 0\">" + BaseController.ProjectName + ".</p>");
            html.Append("<hr>");
            html.Append("<p style=\"margin: 8px 0\">This is an automated email.  Please do not reply.</p>");
            return mailContent.Replace("{mail-content}", html.ToString());
        }
    }
}