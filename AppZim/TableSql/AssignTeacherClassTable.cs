using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class AssignTeacherClassTable
    {
        //thêm dữ liệu
        public static void insertclass(int UID, int classid, string createby)
        {
            using (var db = new ZimEntities())
            {
                var check = db.tbl_AssignTeacherClass.Where(n => n.UID == UID && n.ClassID == classid).FirstOrDefault();
                if (check != null)
                {
                    check.UID = UID;
                    check.ClassID = classid;
                    check.isHide = !check.isHide;
                    check.ModifiedBy = createby;
                    check.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                else
                {
                    tbl_AssignTeacherClass ck = new tbl_AssignTeacherClass();
                    ck.UID = UID;
                    ck.ClassID = classid;
                    ck.SubjectID = 0;
                    ck.SubjectLevel = 1;// 1 trung bình, 2 khá, 3 giỏi
                    ck.isHide = false;
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.tbl_AssignTeacherClass.Add(ck);
                    db.SaveChanges();
                }
            }
        }

        public static tbl_AssignTeacherClass insertsubject(int UID, int classid, int subid, int level, string createby)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_AssignTeacherClass.Where(n => n.UID == UID && n.ClassID == classid && n.SubjectID == subid).FirstOrDefault();
                if (ck == null)
                {
                    ck = new tbl_AssignTeacherClass();
                    ck.UID = UID;
                    ck.ClassID = classid;
                    ck.SubjectID = subid;
                    ck.SubjectLevel = level;// 1 trung bình, 2 khá, 3 giỏi
                    ck.isHide = true;
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.tbl_AssignTeacherClass.Add(ck);
                    db.SaveChanges();
                    return ck;
                }
                ck.SubjectLevel = level;// 1 trung bình, 2 khá, 3 giỏi                
                ck.ModifiedBy = createby;
                ck.ModifiedDate = GetDateTime.Now;
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_AssignTeacherClass getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AssignTeacherClass.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }
        public static tbl_AssignTeacherClass getbyuidclasssub(int uid, int classid, int subid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AssignTeacherClass.Where(n => n.UID == uid && n.ClassID == classid && n.SubjectID == subid).FirstOrDefault();
                return l;
            }
        }

        //kiểm tra xem có check cái lớp đó chưa
        public static bool checkclass(int uid, int classid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AssignTeacherClass.Where(n => n.UID == uid && n.ClassID == classid && n.isHide != true).ToList();
                if (l.Count == 0)
                    return false;
                return true;
            }
        }
    }
}