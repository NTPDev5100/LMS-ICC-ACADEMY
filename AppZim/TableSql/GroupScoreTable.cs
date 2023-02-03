using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class GroupScoreTable
    {
        //thêm dữ liệu
        public static tbl_GroupScore insert(int CourseID, int ExerciseGroupID, int StudentID, double GroupScore,
            int CorrectQuestion, int TotalQuestion)
        {
            using (var db = new ZimEntities())
            {
                tbl_GroupScore ck = new tbl_GroupScore();
                ck.CourseID = CourseID;
                ck.ExerciseGroupID = ExerciseGroupID;
                ck.StudentID = StudentID;
                ck.GroupScore = GroupScore;
                ck.CorrectQuestion = CorrectQuestion;
                ck.TotalQuestion = TotalQuestion;
                ck.CreatedBy = StudentID.ToString();
                ck.CreatedDate = DateTime.Now;
                db.tbl_GroupScore.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_GroupScore getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_GroupScore.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_GroupScore> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_GroupScore.ToList();
                return l;
            }
        }
        public static tbl_GroupScore CheckExist(int courseid, int groupid, int studentid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_GroupScore.Where(x => x.ExerciseGroupID == groupid && x.CourseID == courseid && x.StudentID == studentid).FirstOrDefault();
                return l;
            }
        }
    }
}