using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppZim.TableSql;
using AppZim.Models;

namespace AppZim.TableSql
{
    public class SetPackageHistoryDetailTable
    {
        public static tbl_SetPackageHistoryDetail insert(int sethistoryID, int exid, string question, string createdby)
        {
            using(var db=new ZimEntities())
            {
                tbl_SetPackageHistoryDetail a = new tbl_SetPackageHistoryDetail();
                a.SetPackageHistoryID = sethistoryID;
                a.GroupExerciseID = exid;
                a.Question = question;
                a.Result="";
                a.CreatedDate = GetDateTime.Now;
                a.IsHide = false;
                a.CreatedBy = createdby;
                db.tbl_SetPackageHistoryDetail.Add(a);
                db.SaveChanges();
                return a;
            }
        }
        public static tbl_SetPackageHistoryDetail getbyHistoryIDandExerciseGroupID(int sethistoryID, int exercisegroupID)
        {
            using (var db=new ZimEntities())
            {
                var a = db.tbl_SetPackageHistoryDetail.Where(n => n.SetPackageHistoryID == sethistoryID && n.GroupExerciseID == exercisegroupID).FirstOrDefault();
                return a;
            }
        }
        public static tbl_SetPackageHistoryDetail updateGroupResult(int ID, string groupResult, string createdby)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_SetPackageHistoryDetail.Where(n => n.ID == ID).FirstOrDefault();
                if (a != null)
                {
                    a.Result = groupResult;
                    a.ModifiedDate = GetDateTime.Now;
                    a.ModifiedBy = createdby;
                    db.SaveChanges();
                }
                return a;
            }
        }
    }
}