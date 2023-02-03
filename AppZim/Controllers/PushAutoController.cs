using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using MB.Extensions;

namespace AppZim.Controllers
{
    public class PushAutoController : Controller
    {
        public static string datalink = "";
        public static string projectName = "ICC ACADEMY";
        // GET: PushAuto
        public ActionResult Push()
        {
            try
            {
                //đổi trạng thái khóa học
                //Thread tcheckstatuscourse = new Thread(checkstatuscourse);
                //tcheckstatuscourse.Start();

                //đổi trạng thái mã giảm giá hết hạn
                Thread tdiscountdeadline = new Thread(discountdeadline);
                tdiscountdeadline.Start();

                //chiến dịch sale done
                Thread tcampsaledone = new Thread(campsaledone);
                tcampsaledone.Start();

                //Tạo bảng lương tháng hiện tại cho khóa học
                //Thread tcreatesalary = new Thread(createsalaryforteacher);
                //tcreatesalary.Start();

                //gửi mail cho học viên, giáo viên, tvv của buổi test sắp diễn ra
                Thread tsendmailnotify30ptotest = new Thread(sendmailnotify30ptotest);
                tsendmailnotify30ptotest.Start();

                //thông báo các buổi học sắp diễn ra
                Thread tpushNotifyUpcomingLesson = new Thread(pushNotifyUpcomingLesson);
                tpushNotifyUpcomingLesson.Start();

                //thông báo khóa học còn 3 buổi
                //Thread tThongBaoKhiCon3BuoiHoc = new Thread(ThongBaoKhiCon3BuoiHoc);
                //tThongBaoKhiCon3BuoiHoc.Start();

                //tạo chu kỳ + mục tiêu vào đầu mỗi tháng
                //Thread tCreatePeriod = new Thread(CreatePeriod);
                //tCreatePeriod.Start();

                //tính lương khi hết chu kỳ
                Thread tFinishPeriod = new Thread(FinishPeriod);
                tFinishPeriod.Start();

                //gửi thông báo nếu sau 24h buổi học chưa điểm danh hoặc upload video
                Thread tNotifyForNotAttendance = new Thread(NotifyForNotAttendance);
                tNotifyForNotAttendance.Start();
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("PushAutoController", "Push", 0, ex.Message);
            }
            return View();
        }

        //gửi mail nhắc còn 30p nữa tới buổi test
        protected static void sendmailnotify30ptotest()
        {
            try
            {
                var l = new List<Admin_SendMailAppointmemtTest_GetList_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.Admin_SendMailAppointmemtTest_GetList().ToList();
                }
                foreach (var item in l)
                {
                    // Gửi mail cho học viên => Gửi thông báo cho học viên
                    //if (!string.IsNullOrEmpty(item.StudentEmail))
                    //    AssetCRM.SendMail(item.StudentEmail, "Có lịch hẹn Test sắp diễn ra", "Lịch Test của bạn "
                    //        + " vào ngày " + item.DateTest + " - " + item.TimeTest);
                    // gui thong bao qua webapp
                    var nt = NotificationTable.insertsingle("", "Lịch Test của bạn ", "Bắt đầu sau 30p nữa!", item.StudentUID.Value, "admin", 0, 0);

                    if (!string.IsNullOrEmpty(item.TeacherEmail))
                        AssetCRM.SendMail(item.TeacherEmail, "Bạn ơi, học viên của bạn sắp vào test. Chúc bạn có một buổi test thành công nhé!", "Lịch Test của học viên "
                            + item.StudentName + " vào ngày " + item.DateTest + " - " + item.TimeTest);

                    if (!string.IsNullOrEmpty(item.AdvisoryEmail))
                        AssetCRM.SendMail(item.AdvisoryEmail, "Có lịch hẹn Test sắp diễn ra", "Lịch Test của học viên "
                            + item.StudentName + " vào ngày " + item.DateTest + " - " + item.TimeTest);
                    StudentAppointmentTestTable.updateIsSendNotify(item.ID);
                }

            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("PushAutoController", "sendmailnotify30ptotest", 0, ex.Message);
            }
        }

        //xử lý trạng thái của khóa học
        protected static void checkstatuscourse()
        {
            try
            {
                var ck = HistoryPushTable.getbypushtypevsdate(2, GetDateTime.Now.Date);
                if (ck == null)
                {
                    var l = CoursesTable.getall().Where(n => n.Status != 3).ToList();
                    var sm = l.Where(n => n.Status == 1).ToList();
                    l = l.Where(n => n.Status == 2).ToList();

                    foreach (var item in sm)
                    {
                        if (item.OpeningDate <= GetDateTime.Now.Date)
                            CoursesTable.updatestatus(item.ID, 2, "Đang diễn ra", "auto");
                    }

                    foreach (var item in l)
                    {
                        var endcourse = CoursesScheduleTable.getbycourseid(item.ID).OrderByDescending(n => n.Date).FirstOrDefault();
                        if (endcourse != null)
                        {
                            if (endcourse.Date < GetDateTime.Now.Date)
                                CoursesTable.updatestatus(item.ID, 3, "Đã xong", "auto");
                        }
                    }
                    // đã push ngày hôm nay thì lưu lại rồi ko push nữa
                    HistoryPushTable.insert(2, "Trạng thái khóa học", "auto");
                }
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("PushAutoController", "checkstatuscourse", 0, ex.Message);
            }
        }


        //xử lý trạng thái mã giảm giá hết hạn
        protected static void discountdeadline()
        {
            try
            {
                var l = DiscountTable.getall().Where(n => n.Status == 1 && n.DeadLine < GetDateTime.Now.Date).ToList();
                if (l.Count > 0)
                {
                    foreach (var item in l)
                    {
                        // cập nhật hết hạn
                        DiscountTable.updatestatus(item.ID, 3, "auto");
                    }
                }
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("PushAutoController", "discountdeadline", 0, ex.Message);
            }
        }

        // thông báo khi còn 3 buổi nữa là hết lộ trình học + thông báo qua mail
        // chưa ổn nên đổi sang gửi mail thủ công cho từng học viên vì mỗi học viên có KM khác nhau
        //protected static void ThongBaoKhiCon3BuoiHoc()
        //{
        //    try
        //    {
        //        using (var db = new ZimEntities())
        //        {
        //            // danh sách khóa học đang diễn ra
        //            var lCourse = db.tbl_Courses.Where(x => x.Status == 2 && x.Mail3Lesson == false).Select(x => new { x.ID, x.CourseName }).ToList();
        //            foreach (var c in lCourse)
        //            {
        //                // danh sach cac buoi hoc chua hoan thanh
        //                var lSchedule = db.tbl_CoursesSchedule.Where(x => x.CourseID == c.ID && x.FisnishStatus == 0).Select(x => new { x.ID, x.TeacherID }).ToList();
        //                // if count <= 3 thi gửi thông báo + mail
        //                if (lSchedule.Count <= 3)
        //                {
        //                    // lấy danh sách học viên trong khóa
        //                    var lAcc = db.tbl_CoursesStudent.Where(x => x.CourseID == c.ID && x.isHide == false).Select(x => new { x.StudentUID }).ToList();
        //                    foreach (var item in lAcc)
        //                    {
        //                        var stu = DeviceMobileTable.getbyUID(item.StudentUID.Value)?.IncludePlayerID;
        //                        PushNotifyMobile.SendNotifyMobile("Chặng đường đã gần kết thúc rồi, cố lên bạn nha", new[] { stu });
        //                        // gui thong bao qua webapp
        //                        var nt = NotificationTable.insertsingle("", "Lớp học sắp kết thúc", "Chặng đường đã gần kết thúc rồi, cố lên bạn nha", item.StudentUID.Value, "admin", 0, 0);
        //                        // gui mail
        //                        AssetCRM.SendMail(AccountTable.getbyID(item.StudentUID.Value).Email, "Lớp học sắp kết thúc", "Chặng đường đã gần kết thúc rồi, cố lên bạn nha");
        //                    }
        //                    CoursesTable.update3mail(c.ID);
        //                    var te = DeviceMobileTable.getbyUID(lSchedule[0].TeacherID.Value)?.IncludePlayerID;
        //                    PushNotifyMobile.SendNotifyMobile("Chặng đường đã gần kết thúc rồi, hãy dành những lời động viên cho học viên của bạn nha", new[] { te });
        //                    NotificationTable.insertsingle("", "Khóa học sắp kết thúc", "Chặng đường người đã gần kết thúc rồi, hãy dành những lời động viên cho học viên của bạn nha", lSchedule[0].TeacherID.Value, "admin", 0, 0);
        //                    AssetCRM.SendMail(AccountTable.getbyID(lSchedule[0].TeacherID.Value).Email, "Khóa học sắp kết thúc", "Chặng đường người đã gần kết thúc rồi, hãy dành những lời động viên cho học viên của bạn nha");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AssetCRM.Writelog("PushAutoController", "ThongBaoKhiCon3BuoiHoc", 0, ex.Message);
        //    }
        //}

        //xử lý trạng thái chiến dịch sale đã hết
        protected static void campsaledone()
        {
            try
            {
                var ck = HistoryPushTable.getbypushtypevsdate(3, GetDateTime.Now.Date);//Push chiến dịch sale
                if (ck == null)
                {
                    var l = CampaignSaleTable.getall().Where(n => n.IsHide == false && n.eTime < GetDateTime.Now.Date).ToList();
                    foreach (var item in l)
                    {
                        CampaignSaleTable.updatedone(item.ID, "auto");
                    }
                    HistoryPushTable.insert(3, "Cập nhật chiến dịch sale đã xong", "auto");
                }
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("PushAutoController", "campsaledone", 0, ex.Message);
            }
        }

        //tạo mới bảng lương tính theo khóa học cho tháng mới
        //protected static void createsalaryforteacher()
        //{
        //    try
        //    {
        //        using (var db = new ZimEntities())
        //        {
        //            var courseList = db.Admin_TeacherSalary_GetListNotExists().ToList();
        //            foreach (var i in courseList)
        //            {
        //                var teacher = TeacherCourseTable.GetByCourseID(i.ID);
        //                TeacherSalaryTable.insert(new tbl_TeacherSalary() { CourseID = i.ID,
        //                    TeacherID = teacher.TeacherUID, Bonus = 0, NumberOfFirstSalary = 0,
        //                    NumberOfSecondSalary = 0, Status = 0, CreatedDate = DateTime.Now,
        //                    ApprovedBy = "", ApprovedDate = null });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AssetCRM.Writelog("PushAutoController", "createsalaryforteacher", 0, ex.Message);
        //    }
        //}

        /// <summary>
        /// thông báo buổi học sắp bắt đầu - 60p
        /// </summary>
        protected static void pushNotifyUpcomingLesson()
        {
            try
            {
                var teacherList = new List<Admin_CourseSchedule_UpcomingTeacher_Result>();
                var studentList = new List<Admin_CourseSchedule_UpcomingStudent_Result>();

                using (var db = new ZimEntities())
                {
                    teacherList = db.Admin_CourseSchedule_UpcomingTeacher().ToList();
                    studentList = db.Admin_CourseSchedule_UpcomingStudent().ToList();
                }

                if (studentList.Count > 0)
                {
                    var stu = studentList.Select(x => x.IncludePlayerID).ToList().ToArray();
                    PushNotifyMobile.SendNotifyMobile("Còn 1 tiếng nữa sẽ đến giờ học của bạn, bạn chuẩn bị vào lớp nha.", stu);
                    foreach (var item in studentList)
                    {
                        try
                        {
                            string studentContent = "Còn 1 tiếng nữa sẽ đến giờ học của bạn, bạn chuẩn bị vào lớp nha.";
                            AssetCRM.SendMail(item.Email, "Lớp học sắp bắt đầu", studentContent);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                if (teacherList.Count > 0)
                {
                    var tea = teacherList.Select(x => x.IncludePlayerID).ToList().ToArray();
                    PushNotifyMobile.SendNotifyMobile("Còn 1 tiếng nữa sẽ đến giờ dạy của bạn, bạn chuẩn bị vào lớp nha.", tea);
                    foreach (var item in teacherList)
                    {
                        try
                        {
                            string teacherContent = "Còn 1 tiếng nữa sẽ đến giờ dạy của bạn, bạn chuẩn bị vào lớp nha.";
                            AssetCRM.SendMail(item.Email, "Lớp học sắp bắt đầu", teacherContent);
                            CoursesScheduleTable.UpdateSentMail(item.ID);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("PushAutoController", "pushNotifyUpcomingLesson", 0, ex.Message + "||" + ex.InnerException);
            }
        }
        /// <summary>
        /// thông báo khi add GV vào buổi test mà sau 2 ngày TVV chưa note
        /// </summary>
        protected static void pushNotifyAcademic()
        {
            try
            {
                var now = DateTime.Now.Date;
                var list = new List<Admin_NotifyAcademic_Result>();
                using (var _db = new ZimEntities())
                {
                    list = _db.Admin_NotifyAcademic().ToList();
                }
                foreach (var i in list)
                {
                    AssetCRM.SendMail(i.Email, "Vui lòng cập nhật thông tin", "Buổi hẹn test với học viên " + i.FullName + " chưa được điền thông tin kết quả test");
                    StudentAppointmentTestTable.updateNoticeAcademic(i.ID);
                }
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("PushAutoController", "pushNotifyAcademic", 0, ex.Message + "||" + ex.InnerException);
            }
        }
        /// <summary>
        /// thông báo khi add GV vào buổi test mà sau 3 ngày học vụ chưa điền lý do
        /// </summary>
        protected static void pushNotifyReason()
        {
            try
            {
                var now = DateTime.Now.Date;
                var list = new List<Admin_NotifyAcademic_Result>();
                using (var _db = new ZimEntities())
                {
                    list = _db.Admin_NotifyAcademic().ToList();
                }
                foreach (var i in list)
                {
                    AssetCRM.SendMail(i.Email, "Vui lòng cập nhật thông tin", "Buổi hẹn test với học viên " + i.FullName + " chưa được điền thông tin kết quả test");
                    StudentAppointmentTestTable.updateNoticeResult(i.ID);
                }
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("PushAutoController", "pushNotifyReason", 0, ex.Message + "||" + ex.InnerException);
            }
        }
        ///// <summary>
        ///// tạo chu kỳ + mục tiêu tự động vào đầu mỗi tháng
        ///// </summary>
        //protected static void CreatePeriod()
        //{
        //    try
        //    {
        //        var now = DateTime.Now.Date;
        //        var firstDay = DateTime.Now.FirstDayOfMonth().Date;
        //        var lastDay = firstDay.LastDayOfMonth();
        //        if (now == firstDay)
        //        {
        //            //kiểm tra xem tạo chưa 
        //            var check = CampaignSaleTable.CheckExists(firstDay, lastDay, 3);
        //            if (check == null)
        //            {
        //                // tạo chu kỳ tháng
        //                var period = CampaignSaleTable.insert("T" + now.ToString("M/yyyy"), firstDay, firstDay.LastDayOfMonth(), "", "Auto", 3, 1);
        //                if (period != null)
        //                {
        //                    // tạo mục tiêu công ty
        //                    var companyTarget = TargetTable.insert(period.ID, 0, 1, 0, "Auto", 0, 1, "");
        //                    // tạo mục tiêu phòng ban
        //                    var departments = AccountTable.getbyrole(2);
        //                    foreach (var d in departments)
        //                    {
        //                        var mainTemp = TargetTable.insert(period.ID, companyTarget.ID, 2, 0, "Auto", 0, d.ID, "");
        //                        if (mainTemp != null)
        //                            TargetTable.insert(period.ID, mainTemp.ID, 4, 0, "Auto", 0, d.ID, "");
        //                    }
        //                    // tạo mục tiêu nhóm
        //                    var teams = AccountTable.getbyrole(3);
        //                    foreach (var t in teams)
        //                    {
        //                        var managerid = 0;
        //                        if (t.LeaderID != null)
        //                        {
        //                            managerid = t.LeaderID.Value;
        //                        }
        //                        var mainTarget = TargetTable.GetMainTarget(period.ID, 2, managerid);
        //                        if (mainTarget != null)
        //                        {
        //                            var mainTemp2 = TargetTable.insert(period.ID, mainTarget.ID, 3, 0, "Auto", 0, t.ID, "");
        //                            if (mainTemp2 != null)
        //                                TargetTable.insert(period.ID, mainTemp2.ID, 4, 0, "Auto", 0, t.ID, "");
        //                        }
        //                    }
        //                    // tạo mục tiêu cá nhân
        //                    var members = AccountTable.getbyrole(6);
        //                    foreach (var m in members)
        //                    {
        //                        var managerid = 0;
        //                        if (m.LeaderID != null)
        //                        {
        //                            managerid = m.LeaderID.Value;
        //                        }
        //                        var mainTarget = TargetTable.GetMainTarget(period.ID, 3, managerid);
        //                        if (mainTarget != null)
        //                            TargetTable.insert(period.ID, mainTarget.ID, 4, 0, "Auto", 0, m.ID, "");
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AssetCRM.Writelog("PushAutoController", "CreatePeriod", 0, ex.Message + "||" + ex.InnerException);
        //    }
        //}
        /// <summary>
        /// tính lương, thưởng cho tvv
        /// </summary>
        protected static void FinishPeriod()
        {
            try
            {
                var now = DateTime.Now.Date;
                var firstDay = DateTime.Now.FirstDayOfMonth().Date;
                if (now == firstDay)
                {
                    // các chiến dịch quá hạn
                    var campaigns = CampaignSaleTable.GetExpired(firstDay);
                    foreach (var i in campaigns)
                    {
                        using (var db = new ZimEntities())
                        {
                            // chỉ lấy ds nhân viên có target trong mấy chiến dịch này
                            var accounts = db.Admin_Account_GetHasTargetInCampaign(i.ID).ToList();
                            foreach (var a in accounts)
                            {
                                double revenue = 0;//doanh thu
                                double revenuePercent = 0;// % doanh thu
                                int careQuantity = 0;
                                int carePrice = 0;
                                int carePlusQuantity = 0;
                                int carePlusPrice = 0;
                                if (a.RoleID == 2)
                                {
                                    // lấy các mục tiêu phòng ban của nhân viên này
                                    var targets = TargetTable.getByManagerId(a.ID, i.ID).ToList();
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
                                        TargetTable.updatedone(t.ID, "Auto");
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
                                    var targets = TargetTable.getByManagerId(a.ID, i.ID).ToList();
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
                                        TargetTable.updatedone(t.ID, "Auto");
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
                                    var targets = TargetTable.getByManagerId(a.ID, i.ID).ToList();
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

                                        TargetTable.updatedone(t.ID, "Auto");
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

                                TempPayrollTable.insert(a.ID, i.ID, a.FullName, a.Salary ?? 0, revenue, revenuePercent
                                    , 0, 0, careQuantity, carePrice, carePlusQuantity, carePlusPrice, "", "Auto");
                            }
                        }
                        CampaignSaleTable.updatedone(i.ID, "Auto");
                    }
                }
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("PushAutoController", "FinishPeriod", 0, ex.Message + "||" + ex.InnerException);
            }
        }
        public static void NotifyForNotAttendance()
        {
            try
            {
                if (DateTime.Now.Minute < 5)
                {
                    using (var db = new ZimEntities())
                    {
                        var data = db.CoursesSchedule_GetNotAttendance().ToList();
                        foreach (var i in data)
                        {
                            try
                            {
                                CoursesScheduleTable.UpdateNotify(i.ID);
                                if (!string.IsNullOrEmpty(i.Email))
                                {
                                    AssetCRM.SendMail(i.Email, projectName + " - Có buổi học bạn chưa điểm danh hoặc upload video", "Chào bạn, <br> Khóa học " + i.CourseName + " ngày " + i.Date.Value.ToString("dd/MM/yyyy") + " " + i.sTime + " - " + i.eTime + " chưa được điểm danh hoặc chưa được upload video, vui lòng cập nhật thông tin buổi học");
                                }
                            }
                            catch { continue; }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("PushAutoController", "NotifyForNotAttendance", 0, ex.Message + "||" + ex.InnerException);
            }
        }
    }
}