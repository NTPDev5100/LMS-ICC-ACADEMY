using AppZim.Models;
using AppZim.ZIM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class GradeTable
    {
        //thêm dữ liệu
        public static tbl_Grade insert(string code, string Grade, string Des, string createby, int language)
        {
            using (var db = new ZimEntities())
            {
                var check = db.tbl_Grade.Where(x => x.GradeCode == code).FirstOrDefault();
                if (check != null)
                    return null;
                tbl_Grade ck = new tbl_Grade();
                ck.GradeCode = code;
                ck.GradeName = Grade;
                ck.Language = language;
                ck.Description = Des;
                ck.IsHidden = false;
                ck.isDelete = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_Grade.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_Grade update(int ID, string codegrade, string GradeName, string Des, bool hide, string createby, int language)
        {
            using (var db = new ZimEntities())
            {
                tbl_Grade ck = db.tbl_Grade.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.GradeCode = codegrade;
                    if (!string.IsNullOrEmpty(GradeName))
                        ck.GradeName = GradeName;
                    if (!string.IsNullOrEmpty(Des))
                        ck.Description = Des;
                    ck.Language = language;
                    ck.IsHidden = hide;
                    ck.isDelete = hide;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static List<tbl_Grade> getbyStudentAppointmentID(int id)
        {
            using (var db = new ZimEntities())
            {
                var studentAppointment = db.tbl_StudentAppointment.SingleOrDefault(x => x.ID == id);
                if (studentAppointment == null) studentAppointment = new tbl_StudentAppointment() { StudentUID = 0 };
                
                var acccount = db.tbl_Account.SingleOrDefault(x => x.ID == studentAppointment.StudentUID);
                if (acccount == null) acccount = new tbl_Account() { Language = 2};

                var result = db.tbl_Grade.Where(x => x.IsHidden == false && x.Language == acccount.Language).ToList();
                return result;

            }
        }
        public static tbl_Grade getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Grade.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_Grade> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Grade.Where(x=>x.isDelete != true).ToList();
                return l;
            }
        }
        
        public static List<tbl_Grade> GetByLanguage(int language)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Grade.Where(x=>x.isDelete != true && x.Language == language).ToList();
                return l;
            }
        }
    }
}