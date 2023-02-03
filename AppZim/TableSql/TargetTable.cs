using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class TargetTable
    {
        public static tbl_Target insert(int campaignId, int mainTargetId, int targetType, double targetValue,
            string createby, double CurrentValue, int managerId, string note)
        {
            using (var db = new ZimEntities())
            {
                var check = db.tbl_Target.Where(x => x.IsDone == false && x.IsHide == false && x.CampaignID == campaignId && x.TargetType == 1 && x.MainTargetID == 0 && mainTargetId == 0).SingleOrDefault();
                if (check != null)
                    return null;
                tbl_Target ck = new tbl_Target();
                ck.CampaignID = campaignId;
                ck.MainTargetID = mainTargetId;
                ck.ManagerID = managerId;
                ck.TargetType = targetType;
                ck.TargetValue = targetValue;
                ck.CurrentValue = CurrentValue;
                ck.Note = note;
                ck.Status = 0;
                ck.IsHide = false;
                ck.IsDone = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_Target.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_Target update(int id, double targetValue, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_Target.Where(x => x.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.TargetValue = targetValue;
                    ck.Note = note;
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
                return null;
            }
        }
        
        public static void updateStatus(int id, int status)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_Target.Where(x => x.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.Status = status;
                    db.SaveChanges();
                }
            }
        }

        public static List<tbl_Target> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Target.Where(x => x.IsHide == false).ToList();
                return l;
            }
        }

        public static tbl_Target getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Target.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static tbl_Target updatedone(int ID, string createby)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_Target.Where(n => n.ID == ID).FirstOrDefault();
                if (a != null)
                {
                    a.IsDone = true;
                    a.ModifiedBy = createby;
                    a.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return a;
            }
        }

        public static tbl_Target Delete(int ID, bool IsHide, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_Target.Where(n => n.ID == ID).FirstOrDefault();
                if (a != null)
                {
                    a.IsHide = IsHide;
                    a.ModifiedBy = ModifiedBy;
                    a.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return a;
            }
        }
        
        public static tbl_Target GetMainTarget(int period, int type, int manager)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_Target.Where(n => n.CampaignID == period && n.IsDone == false && n.IsHide == false && n.TargetType == type && (n.ManagerID == manager || manager == 0)).OrderBy(x => x.TargetType).ThenBy(x => x.TargetValue).FirstOrDefault();
                return a;
            }
        }
        
        public static List<tbl_Target> GetByCampaignID(int period)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_Target.Where(n => n.CampaignID == period && n.IsDone == false && n.IsHide == false).ToList();
                return a;
            }
        }
        public static List<tbl_Target> getByPeriodAndType(int period, int type)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Target.Where(x => x.IsHide == false && x.TargetType == type && x.CampaignID == period).ToList();
                return l;
            }
        }
        
        public static tbl_Target getByMainTargetForRole23(int period, int type, int managerId)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Target.Where(x => x.IsHide == false && x.TargetType == type && x.CampaignID == period 
                && x.IsDone == false && x.ManagerID == managerId).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_Target> getByManagerId(int managerId, int period)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Target.Where(x => x.IsHide == false && x.ManagerID == managerId && x.CampaignID == period).ToList();
                return l;
            }
        }

        public static tbl_Target UpdateRevenue(int id, double revenue, string username)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_Target.Where(n => n.ID == id).FirstOrDefault();
                if (a != null)
                {
                    a.CurrentValue = revenue;
                    a.ModifiedBy = username;
                    a.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return a;
            }
        }
    }
}