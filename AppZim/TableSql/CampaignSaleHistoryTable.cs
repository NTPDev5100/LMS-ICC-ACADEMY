using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CampaignSaleHistoryTable
    {
        public static tbl_CampaignSaleHistory insert(int targetId, int studentId, int appointmentId, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CampaignSaleHistory ck = new tbl_CampaignSaleHistory();
                ck.TargetID = targetId;
                ck.StudentID = studentId;
                ck.AppointmentID = appointmentId;
                ck.CreatedBy = createby;
                ck.CreatedDate = GetDateTime.Now;
                db.tbl_CampaignSaleHistory.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_CampaignSaleHistory> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CampaignSaleHistory.ToList();
                return l;
            }
        }

        public static tbl_CampaignSaleHistory getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CampaignSaleHistory.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        
        public static tbl_CampaignSaleHistory getbyAppointmentID(int appointmentID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CampaignSaleHistory.Where(n => n.AppointmentID == appointmentID).FirstOrDefault();
                return l;
            }
        }
    }
}