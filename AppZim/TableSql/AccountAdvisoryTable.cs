using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class AccountAdvisoryTable
    {
        public static tbl_AccountAdvisory InsertCanTuVan(string FullName, string Phone, string Email, string NoiDung)
        {
            using (var db = new ZimEntities())
            {
                tbl_AccountAdvisory ck = new tbl_AccountAdvisory();
                ck.FullNameCustomer = FullName;
                ck.PhoneCustomer = Phone;
                ck.EmailCustomer = Email;
                ck.ContentAdvisory = NoiDung;
                ck.StatusAdvisory = false;
                ck.Note = "";
                ck.UIDSeller = 0;
                ck.FullNameSeller = "";
                ck.CreatedBy = "Cần tư vấn từ web";
                ck.CreatedDate = GetDateTime.Now;
                db.tbl_AccountAdvisory.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_AccountAdvisory UpdateStatus(int ID, bool Status, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_AccountAdvisory ck = db.tbl_AccountAdvisory.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.StatusAdvisory = Status;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static tbl_AccountAdvisory UpdateNote(int ID, string Note)
        {
            using (var db = new ZimEntities())
            {
                tbl_AccountAdvisory ck = db.tbl_AccountAdvisory.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.Note = Note;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static tbl_AccountAdvisory UpdateSeller(int ID, int UIDSeller,string FullNameSeller)
        {
            using (var db = new ZimEntities())
            {
                tbl_AccountAdvisory ck = db.tbl_AccountAdvisory.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.UIDSeller = UIDSeller;
                    ck.FullNameSeller = FullNameSeller;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static tbl_AccountAdvisory GetByID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AccountAdvisory.Where(x => x.ID == ID).FirstOrDefault();
                return l;
            }
        }
    }
}