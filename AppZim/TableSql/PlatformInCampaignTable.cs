using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class PlatformInCampaignTable
    {
        public static tbl_PlatformInCampaign insert(int campaignId, string platformName, double expectedCost,
           string createby, double actualCosts)
        {
            using (var db = new ZimEntities())
            {
                tbl_PlatformInCampaign ck = new tbl_PlatformInCampaign();
                ck.CampaignID = campaignId;
                ck.PlatformName = platformName;
                ck.ExpectedCost = expectedCost;
                ck.IsHide = false;
                ck.ActualCosts = actualCosts;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_PlatformInCampaign.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_PlatformInCampaign update(int id, string platformName, double expectedCost,
            string createby, double actualCosts)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_PlatformInCampaign.Where(x => x.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.PlatformName = platformName;
                    ck.ExpectedCost = expectedCost;
                    ck.ActualCosts = actualCosts;
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
                return null;
            }
        }

        public static List<tbl_PlatformInCampaign> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_PlatformInCampaign.Where(x => x.IsHide == false).ToList();
                return l;
            }
        }

        public static tbl_PlatformInCampaign getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_PlatformInCampaign.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static tbl_PlatformInCampaign Delete(int ID, bool IsHide, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_PlatformInCampaign.Where(n => n.ID == ID).FirstOrDefault();
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