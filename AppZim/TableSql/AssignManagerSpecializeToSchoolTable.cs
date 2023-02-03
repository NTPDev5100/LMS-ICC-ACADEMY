using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class AssignManagerSpecializeToSchoolTable
    {
        public static void insertschool(int UID, int SchoolID, bool ishide, string createby)
        {
            using (var db = new ZimEntities())
            {

                var cks = db.tbl_AssignManagerSpecializeToSchool.Where(n => n.UID == UID && n.SchoolID == SchoolID).FirstOrDefault();
                if (cks == null)// chưa có thì add vô
                {
                    tbl_AssignManagerSpecializeToSchool ck = new tbl_AssignManagerSpecializeToSchool();
                    ck.UID = UID;
                    ck.SchoolID = SchoolID;
                    ck.isHide = false;
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.tbl_AssignManagerSpecializeToSchool.Add(ck);
                    db.SaveChanges();
                }
                else// có rồi thì sửa
                {
                    cks.isHide = ishide;
                    cks.ModifiedBy = createby;
                    cks.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
            }
        }

        //kiểm tra xem có check cái lớp đó chưa
        public static tbl_AssignManagerSpecializeToSchool checkschool(int uid, int schoolid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AssignManagerSpecializeToSchool.Where(n => n.UID == uid && n.SchoolID == schoolid).FirstOrDefault();
                return l;
            }
        }

        //kiểm tra xem có check cái lớp đó chưa
        public static List<tbl_AssignManagerSpecializeToSchool> getbyuid(int uid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AssignManagerSpecializeToSchool.Where(n => n.UID == uid && n.isHide != true).ToList();
                return l;
            }
        }
    }
}