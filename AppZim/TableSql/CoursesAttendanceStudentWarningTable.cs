using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CoursesAttendanceStudentWarningTable
    {
        public static tbl_CoursesAttendanceStudentWarning insert(int StudentUID, int CoursesAttendanceStudentID, string ContentFeedback, string createby,int typecomment)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesAttendanceStudentWarning ck = new tbl_CoursesAttendanceStudentWarning();
                ck.StudentUID = StudentUID;
                ck.CoursesAttendanceStudentID = CoursesAttendanceStudentID;
                ck.ContentFeedback = ContentFeedback;
                ck.isHide = false;
                ck.StatusCMT = 1;// 1 mới. 2 chấp nhận, 3 không chấp nhận
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                ck.TypeComment = typecomment;//1 dành cho cảnh báo các thứ, 2 dành cho bình luận trong customerdetaill
                db.tbl_CoursesAttendanceStudentWarning.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }       
        /// <summary>
        /// Lấy tất cả cmt của học viên bị cảnh báo
        /// </summary>
        /// <param name="CoursesAttendanceStudentID"></param>
        /// <returns></returns>
        public static List<tbl_CoursesAttendanceStudentWarning> getbycoursesattendanceStudentid(int CoursesAttendanceStudentID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesAttendanceStudentWarning.Where(n => n.CoursesAttendanceStudentID == CoursesAttendanceStudentID).ToList();
                return l;
            }
        }
        /// <summary>
        /// Lấy tất cả cmt thuộc về hv đó
        /// </summary>
        /// <param name="studentid"></param>
        /// <returns></returns>
        public static List<tbl_CoursesAttendanceStudentWarning> getbystudentid(int studentid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesAttendanceStudentWarning.Where(n => n.StudentUID == studentid).ToList();//=2 là commnet trong customerdetail => không tính
                return l;
            }
        }
        /// <summary>
        /// Chấp nhận giải pháp giáo viên đưa ra hay không
        /// </summary>
        /// <param name="id">id được chấp nhận</param>
        /// <param name="statusCMT">trạng thái 1 mới. 2 chấp nhận, 3 không chấp nhận</param>
        /// <param name="createby">người tạo</param>
        /// <returns></returns>
        public static tbl_CoursesAttendanceStudentWarning updateaccept(int id, int statusCMT, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesAttendanceStudentWarning ck = db.tbl_CoursesAttendanceStudentWarning.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.StatusCMT = statusCMT;// 1 mới. 2 chấp nhận, 3 không chấp nhận
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
       
    }
}