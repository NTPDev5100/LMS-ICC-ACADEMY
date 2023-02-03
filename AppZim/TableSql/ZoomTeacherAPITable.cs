using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ZoomTeacherAPITable
    {
        public static tbl_ZoomTeacherAPI Insert(int TeacherID, string UserID, string ApiKey, string ApiSecrect, string Token, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ZoomTeacherAPI ck = new tbl_ZoomTeacherAPI();
                ck.TeacherID = TeacherID;
                ck.UserID = UserID;
                ck.APIKey = ApiKey;
                ck.APISecrect = ApiSecrect;
                ck.Token = Token;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ZoomTeacherAPI.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_ZoomTeacherAPI Update(int ID, string UserID, string ApiKey, string ApiSecrect, string Token, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_ZoomTeacherAPI ck = db.tbl_ZoomTeacherAPI.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.UserID = UserID;
                    ck.APIKey = ApiKey;
                    ck.APISecrect = ApiSecrect;
                    ck.Token = Token;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
                return null;
            }
        }

        public static tbl_ZoomTeacherAPI getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ZoomTeacherAPI.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }
        public static bool getbyTeacherID(int teacherID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ZoomTeacherAPI.Where(n => n.TeacherID == teacherID).FirstOrDefault();
                if (l != null)
                    return true;
                return false;
            }
        }

        public static tbl_ZoomTeacherAPI GetbyTeacherID(int teacherID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ZoomTeacherAPI.Where(n => n.TeacherID == teacherID).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_ZoomTeacherAPI> GetAll()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ZoomTeacherAPI.ToList();
                return l;
            }
        }
    }
}