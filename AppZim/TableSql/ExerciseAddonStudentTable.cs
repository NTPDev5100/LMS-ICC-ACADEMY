using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ExerciseAddonStudentTable
    {
        //thêm dữ liệu
        public static tbl_ExerciseAddonStudent insert(tbl_Account student, tbl_Courses course,tbl_SetPackage setPackage, tbl_Exercise exercise, string link, string answer,
            int multichoise, int UIDFix, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseAddonStudent ck = new tbl_ExerciseAddonStudent();
                ck.UIDStudent = student.ID;
                ck.StudentName = student.FullName;
                ck.CourseID = course.ID;
                ck.CourseName = course.CourseName;
                ck.ExeGroupID = exercise.ExerciseGroupID.Value;
                ck.ExerciseID = exercise.ID;
                ck.ExerciseQuestion = exercise.ExerciseTitle;
                ck.SetPackageID = setPackage.ID;
                ck.SetPackageName = setPackage.PackageName;
                ck.Type = exercise.Type.Value;
                ck.FileAudio = link;
                ck.ParagraphAnswer = ck.ParagraphAnswerFix = answer;
                ck.ExerciseMultipleChoiseID = multichoise;
                ck.UIDFix = UIDFix;
                var fix = AccountTable.getbyID(UIDFix);
                if (fix != null)
                    ck.UIDFixName = fix.FullName;
                else
                    ck.UIDFixName = "";
                ck.NoteFix = "";
                ck.UndoFixNumber = 0;
                ck.Scores = 0;
                ck.StatusDone = 1;// 1 mới nộp, 2 đã nhận chấm, 3 đã chấm, 4 chấm lại, 5 xong
                if (fix != null)
                    ck.StatusDone = 2;//đã có ng nhận chấm chính là giáo viên tạo bài
                ck.ExercisePrice = 0;
                ck.PayStatus = 1;// chưa thanh toán, 2 đã thanh toán
                // người tạo bài
                var accex = AccountTable.getbyusername(exercise.CreatedBy);
                if (accex != null)
                {
                    ck.UIDCreatedByExercise = accex.ID;
                    ck.FullNameCreatedByExercise = accex.FullName;
                }
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ExerciseAddonStudent.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        /// <summary>
        /// lấy số lượng bài học viên đã nộp
        /// </summary>
        /// <returns></returns>
        public static int getallcount()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.ToList();
                return l.Count;
            }
        }
        /// <summary>
        /// Lấy danh sách số bài đã được đnáh giá
        /// </summary>
        /// <returns></returns>
        public static int getrateallcount()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.RateExercise != null).ToList();
                return l.Count;
            }
        }
        /// <summary>
        /// Lấy ra só lượng bài bị học viên yêu cầu chấm lại
        /// </summary>
        /// <returns></returns>
        public static int getundofixcount()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.UndoFixNumber > 0).ToList();
                return l.Count;
            }
        }
        public static tbl_ExerciseAddonStudent checkstudenex(int uid, int exid, int courseid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.UIDStudent == uid && n.ExerciseID == exid && n.CourseID == courseid).FirstOrDefault();
                return l;
            }
        }

        public static tbl_ExerciseAddonStudent getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        //Duy khoa
        public static int getcountbyrate(int rate)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.RateExercise == rate).ToList().Count();
                return l;
            }
        }
        public static List<tbl_ExerciseAddonStudent> getTop5Rate()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.RateExercise != null).OrderByDescending(n => n.RateDate).Take(5).ToList();
                return l;
            }
        }

        public static tbl_ExerciseAddonStudent getbyuidvsexeid(int uid, int exeid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.UIDStudent == uid && n.ExerciseID == exeid).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_ExerciseAddonStudent> getListbyuidvsexeid(int uid, int exeid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.UIDStudent == uid && n.ExerciseID == exeid).ToList();
                return l;
            }
        }


        /// <summary>
        /// Lấy theo trạng thái của câu hỏi
        /// </summary>
        /// <param name="status">1 mới nộp, 2 đã nhận chấm, 3 đã chấm, 4 chấm lại, 5 xong</param>
        /// <returns></returns>
        public static List<tbl_ExerciseAddonStudent> getbystatus(int status)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.StatusDone == status).ToList();
                return l;
            }
        }

        public static bool checkworkexercise(int studentid, int gid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.UIDStudent == studentid && n.ExeGroupID == gid).FirstOrDefault();
                if (l == null)
                    return false;
                else
                    return true;
            }
        }

        //cập nhật trạng thái
        public static tbl_ExerciseAddonStudent updatestatusdone(int id, int uidfix, int status, int rate, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.ID == id).FirstOrDefault();
                if (l == null)
                    return null;
                if (status != l.StatusDone)
                {
                    l.UIDFix = uidfix;
                    var fix = AccountTable.getbyID(uidfix);
                    if (fix == null)
                        return null;
                    l.UIDFixName = fix.FullName;
                    l.StatusDone = status;// 1 mới nộp, 2 đã nhận chấm, 3 đã chấm, 4 chấm lại, 5 xong
                    if (status == 5 && rate > 0)//chấp nhận bài chấm mới có rate
                    {
                        l.RateExercise = rate;
                        l.RateDate = GetDateTime.Now;
                    }
                    else
                    {
                        l.ModifiedBy = createby;
                        l.ModifiedDate = GetDateTime.Now;
                    }
                    db.SaveChanges();
                }
                return l;
            }
        }

        //cập nhật
        public static tbl_ExerciseAddonStudent updatefix(int id, string fix, string notefix, double score, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.ID == id).FirstOrDefault();
                if (l == null)
                    return null;
                l.ParagraphAnswerFix = fix;
                l.NoteFix = notefix;
                l.Scores = Math.Round(score, 2);
                l.StatusDone = 3; //đã chấm xong
                l.ModifiedBy = createby;
                l.ModifiedDate = GetDateTime.Now;
                db.SaveChanges();
                return l;
            }
        }

        //lấy danh sách bài tập của học viên đã làm trong khóa, tự luận
        public static List<tbl_ExerciseAddonStudent> getbyuidvscourseid(int uid, int courseid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.UIDStudent == uid && n.CourseID == courseid && n.Type == 1).ToList();
                return l;
            }
        }

        public static List<tbl_ExerciseAddonStudent> getbyuidfixdone(int uidfix)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.UIDFix == uidfix && n.StatusDone == 5).ToList();
                return l;
            }
        }

        //lấy tất cả bài tập của giáo viên đó
        public static List<tbl_ExerciseAddonStudent> getbyuidfix(int uidfix)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.UIDFix == uidfix && n.Type == 1).ToList();
                return l;
            }
        }

        /// <summary>
        /// cập nhật bài đó bị reject để ko tính rate
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reject"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_ExerciseAddonStudent rejectfix(int id, bool reject, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.RejectCheckFix = reject;
                    l.RejectCheckFixDate = GetDateTime.Now.Date;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }
        public  static List<tbl_ExerciseAddonStudent> getbyUIDStudent_ExeGroupID(int studentID, int groupID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.UIDStudent == studentID && n.ExeGroupID == groupID).ToList();
                return l;
            }
        }
        public static tbl_ExerciseAddonStudent getbyUIDStudent_SetPackageID(int studentID, int excerciseID,int setpackageID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.UIDStudent == studentID && n.ExerciseID == excerciseID && n.SetPackageID == setpackageID).FirstOrDefault();
                return l;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentID"></param>
        /// <param name="setpackageID"></param>
        /// <param name="type">1 trắc nghiệm, 2 viết,3 audio</param>
        /// <returns></returns>
        public static tbl_ExerciseAddonStudent getbyUIDStudent_SetPackageID_Type(int studentID,int setpackageID,int type)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.UIDStudent == studentID  && n.SetPackageID == setpackageID && n.Type == type).FirstOrDefault();
                return l;
            }
        }
        public static tbl_ExerciseAddonStudent updateCreateBy(int ID,int UIDCreator, string FullNameCreator)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseAddonStudent exad = db.tbl_ExerciseAddonStudent.Where(n => n.ID == ID).FirstOrDefault();
                if (exad != null)
                {
                    exad.FullNameCreatedByExercise= FullNameCreator;
                    exad.UIDCreatedByExercise= UIDCreator;                    
                    db.SaveChanges();
                }
                return exad;
            }
        }
        public static void updateUIDFix(int id, int uidFix)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseAddonStudent exad = db.tbl_ExerciseAddonStudent.Where(n => n.ID == id).FirstOrDefault();
                if (exad != null)
                {
                    exad.UIDFix = uidFix;
                    db.SaveChanges();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// lấy tất cả
        /// <param name="studentID"></param>
        /// <param name="setpackageID"></param>
        /// <param name="type">1 trắc nghiệm, 2 viết,3 audio</param>
        /// <returns></returns>
        public static List<tbl_ExerciseAddonStudent> GetDoneList(int studentID, int setpackageID, int type)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudent.Where(n => n.UIDStudent == studentID && n.SetPackageID == setpackageID && n.Type == type).ToList();
                return l;
            }
        }
    }
}