using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ScoreStudentTable
    {
        public static tbl_ScoreStudent Insert(int CourseID, int StudentUID, int ConfigureScoreID, string ScoreName, double Score, string remark, string CreatedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_ScoreStudent c = new tbl_ScoreStudent();
                c.ConfigureScoreID = ConfigureScoreID;
                c.NoteColumn = ScoreName;
                c.CreatedBy = CreatedBy;
                c.CreatedDate = DateTime.Now;
                c.IsHide = false;
                db.tbl_ScoreStudent.Add(c);
                db.SaveChanges();
                return c;
            }
        }

        public static void Delete(int ID, string CreatedBy)
        {
            using (var db = new ZimEntities())
            {
                var c = db.tbl_ScoreStudent.Where(x => x.ID == ID).FirstOrDefault();
                if (c != null)
                {
                    c.IsHide = true;
                    c.ModifiedBy = CreatedBy;
                    c.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                }
            }
        }

        public static tbl_ScoreStudent GetByID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var c = db.tbl_ScoreStudent.Where(x => x.ID == ID).FirstOrDefault();
                if (c != null)
                    return c;
                else return null;
            }
        }
    }
}