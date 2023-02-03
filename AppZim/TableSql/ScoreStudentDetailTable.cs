using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ScoreStudentDetailTable
    {
        public static tbl_ScoreStudentDetail Insert(int ScoreStudentID, int StudentUID, double Score, string remark, string CreatedBy)
        {
            using (var db = new ZimEntities())
            {
                var check = db.tbl_ScoreStudentDetail.Where(x => x.ScoreStudentID == ScoreStudentID && x.StudentID == StudentUID && x.IsHide == false).FirstOrDefault();
                if (check == null)
                {
                    var scoreStudent = ScoreStudentTable.GetByID(ScoreStudentID);
                    if (scoreStudent != null)
                    {
                        tbl_ScoreStudentDetail c = new tbl_ScoreStudentDetail();
                        c.ScoreStudentID = ScoreStudentID;
                        c.ScoreFactor = ConfigureScoreTable.GetByID(scoreStudent.ConfigureScoreID.Value).ScoreFactor;
                        c.StudentID = StudentUID;
                        c.Score = Score;
                        c.Remark = remark;
                        c.CreatedBy = CreatedBy;
                        c.CreatedDate = DateTime.Now;
                        c.IsHide = false;
                        db.tbl_ScoreStudentDetail.Add(c);
                        db.SaveChanges();
                        return c;
                    }
                }
                return null;
            }
        }

        public static string Update(int ID, double Score, string CreatedBy)
        {
            using (var db = new ZimEntities())
            {
                var c = db.tbl_ScoreStudentDetail.Where(x => x.ID == ID).FirstOrDefault();
                if (c != null)
                {
                    c.Score = Score;
                    c.ModifiedBy = CreatedBy;
                    c.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                    return c.ID.ToString();
                }
                else return null;
            }
        }

        public static tbl_ScoreStudentDetail GetByID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var c = db.tbl_ScoreStudentDetail.Where(x => x.ID == ID).FirstOrDefault();
                if (c != null)
                    return c;
                else return null;
            }
        }
    }
}