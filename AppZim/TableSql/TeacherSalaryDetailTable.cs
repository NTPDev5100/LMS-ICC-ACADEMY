using AppZim.Models;
using AppZim.ZIM;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AppZim.TableSql
{
    public class TeacherSalaryDetailTable
    {
        public static tbl_TeacherSalaryDetail insert(tbl_TeacherSalaryDetail teacherSalaryDetail, string user)
        {
            using (var db = new ZimEntities())
            {
                var data = new tbl_TeacherSalaryDetail
                {
                    CourseID = teacherSalaryDetail.CourseID,
                    CoursesScheduleID = teacherSalaryDetail.CoursesScheduleID,
                    TeacherID = teacherSalaryDetail.TeacherID,
                    Salary = teacherSalaryDetail.Salary,
                    Date = teacherSalaryDetail.Date,
                    Type = teacherSalaryDetail.Type,//1 lương thường, 3 lương học viên off
                    isClosing = false,//true-Đã chốt lương false-Chưa chốt lương
                    IsReject = false,
                    isHide = false,
                    CreatedDate = DateTime.Now,
                    CreatedBy = user,
                    ModifiedDate = DateTime.Now,
                    ModifiedBy = user,
                };
                db.tbl_TeacherSalaryDetail.Add(data);
                db.SaveChanges();
                return data;
            }
        }

        public static tbl_TeacherSalaryDetail insertCheckSalary(string user,int courseID,int coursesScheduleID,int teacherID,float salary,DateTime date,int type,string reasonNote)
        {
            using (var db = new ZimEntities())
            {
                var data = new tbl_TeacherSalaryDetail
                {
                    CourseID = courseID,
                    CoursesScheduleID = coursesScheduleID,
                    TeacherID = teacherID,
                    Salary = salary,
                    Date = date,
                    Type = type,//1 lương thường, 3 lương học viên off
                    isClosing = false,//true-Đã chốt lương false-Chưa chốt lương
                    isHide = false,
                    IsReject = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = user,
                    ModifiedDate = DateTime.Now,
                    ModifiedBy = user,
                };
                db.tbl_TeacherSalaryDetail.Add(data);
                db.SaveChanges();
                return data;
            }
        }



        public static tbl_TeacherSalaryDetail getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var data = db.tbl_TeacherSalaryDetail
                    .SingleOrDefault(x => x.ID == id);
                return data;
            }
        }
        
        public async static Task<tbl_TeacherSalaryDetail> getbyidAsync(int id)
        {
            using (var db = new ZimEntities())
            {
                var data = await db.tbl_TeacherSalaryDetail
                    .SingleOrDefaultAsync(x => x.ID == id);
                return data;
            }
        }
        public static List<tbl_TeacherSalaryDetail> getall()
        {
            using (var db = new ZimEntities())
            {
                var data = db.tbl_TeacherSalaryDetail
                    .Where(x => x.isHide == false).ToList();
                return data;
            }
        }
        /// <summary>
        /// Hàm này dùng để tính lương tháng trước
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static void updatePayroll(string user)
        {
            try
            {
                var teachers = AccountTable.getbyTeacherCourse();
                DateTime monthBefore = DateTime.Now.AddMonths(-1);

                if (teachers.Count == 0)
                {
                    // Không làm gì cả
                }
                else
                {
                    using (var db = new ZimEntities())
                    {
                        foreach (var i in teachers)
                        {
                            var teacherSalaryDetails = db.tbl_TeacherSalaryDetail
                            .Where(x => x.Date.Value.Month == monthBefore.Month
                            && x.Date.Value.Year == monthBefore.Year
                            && x.isClosing == false && x.TeacherID == i.ID && x.IsReject == false)
                            .ToList();//lấy danh sách buổi dạy cần tính lương của giáo viên

                            if (teacherSalaryDetails.Count == 0) continue;

                            double totalSalary = 0;
                            foreach (var item in teacherSalaryDetails)
                            {
                                totalSalary += item.Salary ?? 0;
                                item.isClosing = true;
                                item.ModifiedBy = user;
                                item.ModifiedDate = DateTime.Now;
                            }

                            var teacherSalary = db.tbl_TeacherSalary.Where(x => x.isHide == false && x.TeacherID == i.ID &&
                            x.Year == monthBefore.Year && x.Month == monthBefore.Month).FirstOrDefault();

                            if (teacherSalary == null)
                            {
                                var data = new tbl_TeacherSalary();
                                data.TeacherID = i.ID;
                                data.TotalSalary += totalSalary;
                                data.Year = monthBefore.Year;
                                data.Month = monthBefore.Month;
                                data.Bonus = 0;
                                data.isDonePaid = false;
                                data.isHide = false;
                                data.CreatedBy = user;
                                data.CreatedDate = DateTime.Now;
                                db.tbl_TeacherSalary.Add(data);
                            }
                            else
                            {
                                teacherSalary.TotalSalary += totalSalary;
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("TeacherSalaryDetailTable", "updatePayroll", 0, e.Message + "||" + e.InnerException);
            }
        }

        public static void PayrollForTeacher(int teacherId, string user)
        {
            try
            {
                DateTime monthBefore = DateTime.Now.AddMonths(-1).Date;
                DateTime now = DateTime.Now;
                using (var db = new ZimEntities())
                {
                    var teacherSalaryDetails = db.tbl_TeacherSalaryDetail
                    .Where(x => x.Date.Value.Month == monthBefore.Month
                    && x.Date.Value.Year == monthBefore.Year
                    && x.isClosing == false && x.TeacherID == teacherId && x.IsReject == false)
                    .ToList();//lấy danh sách buổi dạy cần tính lương của giáo viên

                    double totalSalary = 0;
                    foreach (var item in teacherSalaryDetails)
                    {
                        totalSalary += item.Salary ?? 0;
                        item.isClosing = true;
                        item.ModifiedBy = user;
                        item.ModifiedDate = now;
                    }

                    //Lấy danh sách các buổi học thử mà giáo viên này dạy, trong tháng, chưa tính lương
                    var trialStudies = db.tbl_StudentAppointmentTest.Where(x => x.TeacherID == teacherId &&
                    x.Payrolled == false && x.DateTest.Value.Month == monthBefore.Month &&
                    x.DateTest.Value.Year == monthBefore.Year).ToList();

                    double trialSalary = 0;
                    int totalTial = trialStudies.Count;
                    for (int i = 0; i < totalTial; i++)
                    {
                        trialSalary += trialStudies[i].SalaryTeacher ?? 0;
                        trialStudies[i].Payrolled = true;
                        trialStudies[i].ModifiedBy = user;
                        trialStudies[i].ModifiedDate = now;
                    }

                    var teacherSalary = db.tbl_TeacherSalary.Where(x => x.isHide == false && x.TeacherID == teacherId &&
                    x.Year == monthBefore.Year && x.Month == monthBefore.Month).FirstOrDefault();

                    if (teacherSalary == null)
                    {
                        var data = new tbl_TeacherSalary();
                        data.TeacherID = teacherId;
                        data.TotalSalary = totalSalary + trialSalary;
                        data.TrialStudySalary = trialSalary;
                        data.Year = monthBefore.Year;
                        data.Month = monthBefore.Month;
                        data.Bonus = 0;
                        data.isDonePaid = false;
                        data.isHide = false;
                        data.CreatedBy = user;
                        data.CreatedDate = DateTime.Now;
                        db.tbl_TeacherSalary.Add(data);
                    }
                    else
                    {
                        teacherSalary.TotalSalary += totalSalary + trialSalary;
                        teacherSalary.TrialStudySalary += trialSalary;
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("TeacherSalaryDetailTable", "updatePayrollForTeacher", 0, e.Message + "||" + e.InnerException);
            }
        }

        /// <summary>
        /// Chốt lương 1 buổi
        /// </summary>
        /// <param name="teacherId"></param>
        /// <param name="user"></param>
        public static void PayrollForOneLesson(int id, string user)
        {
            try
            {
                using (var db = new ZimEntities())
                {
                    var teacherSalaryDetails = db.tbl_TeacherSalaryDetail
                    .Where(x => x.ID == id).FirstOrDefault();

                    if (teacherSalaryDetails != null)
                    {
                        var teacherSalary = db.tbl_TeacherSalary.Where(x => x.isHide == false && x.TeacherID == teacherSalaryDetails.TeacherID &&
                        x.Year == teacherSalaryDetails.Date.Value.Year && x.Month == teacherSalaryDetails.Date.Value.Month).FirstOrDefault();

                        if (teacherSalary == null)
                        {
                            var data = new tbl_TeacherSalary();
                            data.TeacherID = teacherSalaryDetails.TeacherID;
                            data.TotalSalary = teacherSalaryDetails.Salary;
                            data.TrialStudySalary = 0;
                            data.Year = teacherSalaryDetails.Date.Value.Year;
                            data.Month = teacherSalaryDetails.Date.Value.Month;
                            data.Bonus = 0;
                            data.isDonePaid = false;
                            data.isHide = false;
                            data.CreatedBy = user;
                            data.CreatedDate = DateTime.Now;
                            db.tbl_TeacherSalary.Add(data);
                        }
                        else
                        {
                            teacherSalary.TotalSalary += teacherSalaryDetails.Salary;
                        }

                        teacherSalaryDetails.isClosing = true;

                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("TeacherSalaryDetailTable", "PayrollForOneLesson", 0, e.Message + "||" + e.InnerException);
            }
        }

        public async static Task<tbl_TeacherSalaryDetail> UpdateReason(int id, string reason, string user, int status, double salary)
        {
            using (var db = new ZimEntities())
            {
                var data = db.tbl_TeacherSalaryDetail.Where(x => x.ID == id && x.isClosing == false).FirstOrDefault();
                if (data != null)
                {
                    switch (status)
                    {
                        case 1:
                            data.IsReject = true;
                            break;
                        case 2:
                            data.IsReject = false;
                            data.isClosing = true;
                            Thread tlesson = new Thread(() => { PayrollForOneLesson(data.ID, user); });
                            tlesson.Start();
                            break;
                        default:
                            break;
                    }
                    data.Salary = salary;
                    data.ReasonNote = reason;
                    data.ModifiedBy = user;
                    data.ModifiedDate = DateTime.Now;
                    await db.SaveChangesAsync();
                }

                return data;
            }
        }
    }
}