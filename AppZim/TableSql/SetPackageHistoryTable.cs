using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppZim.Models;

namespace AppZim.TableSql
{
    public class SetPackageHistoryTable
    {
        public static tbl_SetPackageHistory insert(int studentID, int setpackageID, int SetDetailID, string createdby, int times)
        {
            using (var db = new ZimEntities())
            {
                tbl_SetPackageHistory a = new tbl_SetPackageHistory();
                a.StudentID = studentID;
                a.SetpackageID = setpackageID;
                a.SetpackageDetailID = SetDetailID;
                a.IsHide = false;
                a.IsDone = false;
                a.Times = times;
                a.CreatedDate = a.ModifiedDate = GetDateTime.Now;
                a.CreatedBy = a.ModifiedBy = createdby;
                db.tbl_SetPackageHistory.Add(a);
                db.SaveChanges();
                return a;
            }
        }
        public static tbl_SetPackageHistory getbyID(int id)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_SetPackageHistory.Where(n => n.ID == id).FirstOrDefault();
                return a;
            }
        }
        public static tbl_SetPackageHistory getbyStudentIDandSetDetailID(int studentID, int SetDetailID)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_SetPackageHistory.Where(n => n.StudentID == studentID && n.SetpackageDetailID == SetDetailID).FirstOrDefault();
                return a;
            }
        }
        public static tbl_SetPackageHistory updatedone(int ID, int correct, double totalPoint, string createdby)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_SetPackageHistory.Where(n => n.ID == ID).FirstOrDefault();
                if (a != null)
                {
                    a.IsDone = true;
                    a.TotalCorrect = correct;
                    a.TotalPoint = totalPoint;
                    a.ModifiedBy = createdby;
                    a.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return a;
            }
        }
        public static List<tbl_SetPackageHistory> getbyListStudentIDandSetDetailID(int studentID, int SetDetailID)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_SetPackageHistory.Where(n => n.StudentID == studentID && n.SetpackageDetailID == SetDetailID).ToList();
                return a;
            }
        }
    }
}