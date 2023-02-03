using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class SubjectTable
    {
        //thêm dữ liệu
        public static tbl_Subject insert(string code, string subject, int classid, string classname, int gradeid, string gradename, string createby, bool special)
        {
            using (var db = new ZimEntities())
            {
                tbl_Subject ck = new tbl_Subject();
                ck.Code = code;
                ck.SubjectName = subject;
                ck.GradeID = gradeid;
                ck.GradeName = gradename;
                ck.ClassID = classid;
                ck.ClassName = classname;
                ck.isHide = false;
                ck.SpecialSubject = special;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_Subject.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_Subject update(int ID,string code, string subject, int classid, string classname, int gradeid, string gradename, string createby, bool IsHide)
        {
            using (var db = new ZimEntities())
            {
                tbl_Subject ck = db.tbl_Subject.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.Code = code;
                    ck.SubjectName = subject;
                    ck.GradeID = gradeid;
                    ck.GradeName = gradename;
                    ck.ClassID = classid;
                    ck.ClassName = classname;
                    ck.isHide = IsHide;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_Subject getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Subject.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }

        /// <summary>
        /// Chỉ lấy những cái cần dùng
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static Subject getbyIDs(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Subject.Where(n => n.ID == ID).Select(i => new { ID = i.ID, SpecialSubject = i.SpecialSubject, ClassID = i.ClassID }).Single();
                Subject s = new Subject();
                s.ID = l.ID;
                s.SpecialSubject = l.SpecialSubject.Value;
                s.ClassID = l.ClassID.Value;
                return s;
            }
        }
        public class Subject
        {
            public int ID { get; set; }
            public bool SpecialSubject { get; set; }
            public int ClassID { get; set; }
        }


        public static List<tbl_Subject> getbyclassid(int classid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Subject.Where(n=>n.ClassID == classid).ToList();
                return l;
            }
        }
    }
}