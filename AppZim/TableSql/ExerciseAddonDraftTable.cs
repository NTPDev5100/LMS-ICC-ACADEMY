using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ExerciseAddonDraftTable
    {
        //thêm dữ liệu
        public static tbl_ExerciseAddonDraft insert(int ExerciseAddonStudentID, string ExerciseQuestion, string FileAudio, string ParagraphAnswerFix,
            string NoteFix, double Scores, string createby)
        {
            using (var db = new ZimEntities())
            {
                var check = db.tbl_ExerciseAddonDraft.Where(n => n.ExerciseAddonStudentID == ExerciseAddonStudentID).FirstOrDefault();
                if (check == null)//chưa có thì insert
                {
                    tbl_ExerciseAddonDraft ck = new tbl_ExerciseAddonDraft();
                    ck.ExerciseAddonStudentID = ExerciseAddonStudentID;
                    ck.DraftName = GetDateTime.Now.ToString("dd/MM/yyyy HH:mm") + "";
                    ck.ExerciseQuestion = ExerciseQuestion;
                    ck.FileAudio = FileAudio;
                    ck.ParagraphAnswerFix = ParagraphAnswerFix;
                    ck.NoteFix = NoteFix;

                    ck.Scores = Math.Round(Scores, 2);
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.tbl_ExerciseAddonDraft.Add(ck);
                    db.SaveChanges();
                    return ck;
                }
                else// có rôi thì cập nhật
                {
                    check.DraftName = GetDateTime.Now.ToString("dd/MM/yyyy HH:mm") + "";
                    check.ExerciseQuestion = ExerciseQuestion;
                    check.FileAudio = FileAudio;
                    check.ParagraphAnswerFix = ParagraphAnswerFix;
                    check.NoteFix = NoteFix;
                    check.Scores = Math.Round(Scores, 2);
                    check.ModifiedBy = createby;
                    check.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return check;
                }
            }
        }

        public static List<tbl_ExerciseAddonDraft> getbyexestudentdi(int ExerciseAddonStudentID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonDraft.Where(n => n.ExerciseAddonStudentID == ExerciseAddonStudentID).OrderByDescending(n => n.ID).ToList();
                return l;
            }
        }

        public static tbl_ExerciseAddonDraft getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonDraft.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
    }
}