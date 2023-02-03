using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CoursesExamDetailTable
    {
        //thêm dữ liệu
        public static tbl_CoursesExamDetail insert(int coursesexamid, int courseid, int uid, string l, string s, string r, string w,
            string note,string overall, int target, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesExamDetail ck = new tbl_CoursesExamDetail();
                ck.CoursesExamID = coursesexamid;
                ck.CourseID = courseid;
                ck.UID = uid;
                ck.Listening = l;
                ck.Speaking = s;
                ck.Reading = r;
                ck.Writing = w;
                ck.Note = note;
                ck.Overall = overall;
                ck.Target = target;// 1 chưa thi, 2 đạt, 3 không đạt
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_CoursesExamDetail.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_CoursesExamDetail getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesExamDetail ck = db.tbl_CoursesExamDetail.Where(n => n.ID == id).FirstOrDefault();
                return ck;
            }
        }

        public static List<tbl_CoursesExamDetail> getbyexamid(int examid)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_CoursesExamDetail.Where(n => n.CoursesExamID == examid).ToList();
                return ck;
            }
        }
        public static List<tbl_CoursesExamDetail> getbyuid(int student)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_CoursesExamDetail.Where(n => n.UID == student && n.CourseID !=0).ToList();
                return ck;
            }
        }
        public static List<tbl_CoursesExamDetail> getDetailInCustomerDetail(int student)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_CoursesExamDetail.Where(n => n.UID == student && n.CourseID == 0).ToList();
                return ck;
            }
        }
    }
}