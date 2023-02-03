using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ConfigureScoreTable
    {
        public static tbl_ConfigureScore Insert(int CurriculumID, int ScoreFactor, string ScoreColumn, bool isScore, string createdBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_ConfigureScore c = new tbl_ConfigureScore();
                c.CurriculumID = CurriculumID;
                c.ScoreFactor = ScoreFactor;
                c.ScoreColumn = ScoreColumn;
                c.CreatedBy = createdBy;
                c.CreatedDate = DateTime.Now;
                c.IsRemark = isScore;// 0 diem, 1 nhan xet
                c.IsHide = false;
                db.tbl_ConfigureScore.Add(c);
                db.SaveChanges();
                return c;
            }
        }

        public static string Update(int ID, int ScoreFactor, string ScoreColumn, bool IsHide, string createdBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_ConfigureScore c = db.tbl_ConfigureScore.Where(x => x.ID == ID).FirstOrDefault();
                if (c != null)
                {
                    c.ScoreFactor = ScoreFactor;
                    c.ScoreColumn = ScoreColumn;
                    c.ModifiedBy = createdBy;
                    c.ModifiedDate = DateTime.Now;
                    c.IsHide = IsHide;
                    db.SaveChanges();
                    return c.ID.ToString();
                }
                else return null;
            }
        }

        public static tbl_ConfigureScore GetByID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var c = db.tbl_ConfigureScore.Where(x => x.ID == ID).FirstOrDefault();
                return c;
            }
        }

        public static List<tbl_ConfigureScore> GetByCurriculumID(int CurriculumID)
        {
            using (var db = new ZimEntities())
            {
                var c = db.tbl_ConfigureScore.Where(x => x.CurriculumID == CurriculumID).OrderBy(x => x.ScoreFactor).ToList();
                return c;
            }
        }
    }
}