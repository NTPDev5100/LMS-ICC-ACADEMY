using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class StudentAppointmentTestTable
    {
        //thêm dữ liệu
        public static tbl_StudentAppointmentTest insert(tbl_Account student, tbl_School school, tbl_Account acccre, DateTime datetest, string timetest,
            string note, string createby, string nationtime)
        {
            using (var db = new ZimEntities())
            {
                tbl_StudentAppointmentTest ck = new tbl_StudentAppointmentTest();
                ck.StudentUID = student.ID;
                ck.StudentName = student.FullName;
                ck.Phone = student.Phone;
                ck.SchoolID = school.ID;
                ck.SchoolName = school.SchoolName;
                ck.UIDCreateTest = acccre.ID;
                ck.CreateTestName = acccre.FullName;
                ck.DateTest = datetest.Date;
                ck.TimeTest = timetest;
                ck.Note = note;
                ck.TestDone = false;
                ck.IsSendNotify = false;
                ck.IsHide = false;
                ck.TeacherID = 0;
                ck.AcademicID = acccre.ID;
                ck.Color = "white";
                ck.NationTime = nationtime;
                ck.Payrolled = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_StudentAppointmentTest.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        //thêm dữ liệu
        public static tbl_StudentAppointmentTest update(int id, tbl_Account student, tbl_School school, tbl_Account acccre, DateTime datetest, string timetest,
            string note, string createby, string numberoflesson, string timeoflesson, string doc, string nationtime)
        {
            using (var db = new ZimEntities())
            {
                tbl_StudentAppointmentTest ck = db.tbl_StudentAppointmentTest.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.StudentUID = student.ID;
                    ck.StudentName = student.FullName;
                    ck.NumberOfLesson = numberoflesson;
                    ck.TimeOfLesson = timeoflesson;
                    ck.LinkDocument = doc;
                    ck.Phone = student.Phone;
                    ck.SchoolID = school.ID;
                    ck.SchoolName = school.SchoolName;
                    ck.UIDCreateTest = acccre.ID;
                    ck.CreateTestName = acccre.FullName;
                    ck.DateTest = datetest.Date;
                    ck.TimeTest = timetest;
                    ck.Note = note;
                    ck.TestDone = false;
                    ck.NationTime = nationtime;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        //update gv + tvv. bùa
        public static tbl_StudentAppointmentTest updateTeacherAndTVV(int id, int teacherid, int tvvid, double salaryTeacher)
        {
            using (var db = new ZimEntities())
            {
                tbl_StudentAppointmentTest ck = db.tbl_StudentAppointmentTest.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.TeacherID = teacherid;
                    ck.SalaryTeacher = salaryTeacher;
                    ck.AcademicID = tvvid;
                    ck.NoticeAcademic = DateTime.Now.AddDays(2).Date;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        //lấy dữu liệu hẹn test của học viên
        public static List<tbl_StudentAppointmentTest> getbystudentid(int studentid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointmentTest.Where(n => n.StudentUID == studentid && n.IsHide == false).OrderByDescending(n => n.DateTest).ToList();
                return l;
            }
        }

        public static tbl_StudentAppointmentTest getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointmentTest.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static tbl_StudentAppointmentTest updatetestdone(int id, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointmentTest.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.Color = "bisque";
                    l.TestDone = true;
                    l.NoticeResult = DateTime.Now.AddDays(3).Date;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }

        public static List<tbl_StudentAppointmentTest> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointmentTest.Where(n => n.IsHide == false).ToList();
                return l;
            }
        }
        //lấy tất cả học viên hẹn test nhưng chưa test ra để push
        public static List<tbl_StudentAppointmentTest> getpush()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointmentTest.Where(n => n.TestDone != true && n.IsHide == false).ToList();
                return l;
            }
        }

        public static void updateIsSendNotify(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointmentTest.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.IsSendNotify = true;
                    db.SaveChanges();
                }
            }
        }
        public static void updateIsSendMail(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointmentTest.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.isSendMail = true;
                    db.SaveChanges();
                }
            }
        }
        public static void updateColor(int id, string color)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointmentTest.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.Color = color;
                    db.SaveChanges();
                }
            }
        }
        public static void updateIsHide(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointmentTest.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.IsHide = true;
                    db.SaveChanges();
                }
            }
        }
        public static void updateNoticeResult(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointmentTest.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.NoticeResult = null;
                    db.SaveChanges();
                }
            }
        }
        public static void updateNoticeAcademic(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointmentTest.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.NoticeAcademic = null;
                    db.SaveChanges();
                }
            }
        }
    }
}