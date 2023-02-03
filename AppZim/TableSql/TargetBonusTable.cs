using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class TargetBonusTable
    {
        public static tbl_TargetBonus insert(int campaignId, int targetType, double targetFinishPercent, 
            string createby, string imageLink, string descriptionBonus, double target)
        {
            using (var db = new ZimEntities())
            {
                tbl_TargetBonus ck = new tbl_TargetBonus();
                ck.CampaignID = campaignId;
                ck.TargetType = targetType;
                ck.TargetFinishPercent = targetFinishPercent;
                ck.IsHide = false;
                ck.ImageLink = imageLink;
                ck.TargetRevenue = target;
                ck.DescriptionBonus = descriptionBonus;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_TargetBonus.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_TargetBonus update(int id, double targetFinishPercent,
            string createby, string imageLink, string descriptionBonus, double target)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_TargetBonus.Where(x => x.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.TargetFinishPercent = targetFinishPercent;
                    ck.ImageLink = imageLink;
                    ck.TargetRevenue = target;
                    ck.DescriptionBonus = descriptionBonus;
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
                return null;
            }
        }

        public static List<tbl_TargetBonus> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_TargetBonus.Where(x => x.IsHide == false).ToList();
                return l;
            }
        }

        public static tbl_TargetBonus getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_TargetBonus.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static tbl_TargetBonus Delete(int ID, bool IsHide, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_TargetBonus.Where(n => n.ID == ID).FirstOrDefault();
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
    }
}