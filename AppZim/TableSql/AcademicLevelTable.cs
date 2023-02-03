using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class AcademicLevelTable
    {
        public static tbl_AcademicLevel insert(string levelname, int position, int language, string languageName, 
            string createby, string content)
        {
            using (var db = new ZimEntities())
            {
                tbl_AcademicLevel ck = new tbl_AcademicLevel();
                ck.LevelName = levelname;
                ck.Position = position;
                ck.Language = language;
                ck.LanguageName = languageName;
                ck.LevelContent = content;
                ck.IsHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_AcademicLevel.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_AcademicLevel update(int ID, string levelname, int position, int language, string languageName,
            string createby, string content)
        {
            using (var db = new ZimEntities())
            {
                tbl_AcademicLevel ck = db.tbl_AcademicLevel.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.LevelName = levelname;
                    ck.Position = position;
                    ck.Language = language;
                    ck.LevelContent = content;
                    ck.IsHide = false;
                    ck.LanguageName = languageName;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static tbl_AcademicLevel getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AcademicLevel.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_AcademicLevel> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AcademicLevel.Where(x => x.IsHide == false).ToList();
                return l;
            }
        }
        
        public static List<tbl_AcademicLevel> getByLanguage( int language)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AcademicLevel.Where(x => x.Language == language && x.IsHide == false).ToList();
                return l;
            }
        }
    }
}