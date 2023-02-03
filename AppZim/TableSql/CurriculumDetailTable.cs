using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CurriculumDetailTable
    {
        //thêm dữ liệu
        public static tbl_CurriculumDetail insert(int curriculumid, string curriculumname, int stt, int subjectid, string subjectname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CurriculumDetail ck = new tbl_CurriculumDetail();
                ck.CurriculumID = curriculumid;
                ck.CurriculumName = curriculumname;
                ck.LessonNumber = stt;
                ck.SubjectID = subjectid;
                ck.SubjectName = subjectname;
                ck.IsHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_CurriculumDetail.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_CurriculumDetail update(int ID, int curriculumid, string curriculumname, int stt, int subjectid, string subjectname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CurriculumDetail ck = db.tbl_CurriculumDetail.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.CurriculumID = curriculumid;
                    ck.CurriculumName = curriculumname;
                    ck.LessonNumber = stt;
                    ck.SubjectID = subjectid;
                    ck.SubjectName = subjectname;
                    ck.IsHide = false;
                    ck.IsHide = false;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_CurriculumDetail getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CurriculumDetail.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_CurriculumDetail> getbycurriculumid(int curriculumid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CurriculumDetail.Where(n => n.CurriculumID == curriculumid).ToList();
                return l;
            }
        }
        public static void updateLesson(int iD, int index)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CurriculumDetail.Where(x => x.ID == iD).FirstOrDefault();
                if (l != null)
                {
                    l.LessonNumber = index;
                    db.SaveChanges();
                }
            }
        }
    }
}