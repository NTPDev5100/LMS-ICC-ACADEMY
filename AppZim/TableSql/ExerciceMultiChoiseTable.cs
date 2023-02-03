using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppZim.ZIM;

namespace AppZim.TableSql
{
    public class ExerciceMultiChoiseTable
    {
        //thêm dữ liệu
        public static tbl_ExerciceMultiChoise insert(int exid, string answer, bool istrue, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciceMultiChoise ck = new tbl_ExerciceMultiChoise();
                ck.ExerciseID = exid;
                ck.AnswerTitle = answer;
                ck.isTrue = istrue;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ExerciceMultiChoise.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_ExerciceMultiChoise getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciceMultiChoise.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_ExerciceMultiChoise> getbyexeid(int exid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciceMultiChoise.Where(n => n.ExerciseID == exid && n.isHide != true).ToList();
                return l;
            }
        }
        public static tbl_ExerciceMultiChoise getbytitle(string title, int exid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciceMultiChoise.Where(n => n.AnswerTitle.Replace(" ", "").Replace("\n", "").ToUpper() == title.Replace(" ", "").Replace("\n", "").ToUpper() && n.ExerciseID == exid && n.isHide != true).FirstOrDefault();
                return l;
            }
        }

        public static tbl_ExerciceMultiChoise getbyanwertrue(int exid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciceMultiChoise.Where(n => n.ExerciseID == exid && n.isTrue == true).FirstOrDefault();
                return l;
            }
        }
        ///SET PACKAGE=======================
        public static tbl_ExerciceMultiChoise update(int id, string answer, bool istrue, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciceMultiChoise ck = db.tbl_ExerciceMultiChoise.Where(n => n.ID == id).FirstOrDefault();
                if (!string.IsNullOrEmpty(answer))
                    ck.AnswerTitle = answer;
                ck.isTrue = istrue;
                ck.isHide = false;
                ck.ModifiedBy = createby;
                ck.ModifiedDate = GetDateTime.Now;
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_ExerciceMultiChoise deleteanswerdrag(int id, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciceMultiChoise ck = db.tbl_ExerciceMultiChoise.Where(n => n.ID == id).FirstOrDefault();
                if (ck == null)
                    return ck;
                ck.isHide = true;
                ck.ModifiedBy = createby;
                ck.ModifiedDate = GetDateTime.Now;
                db.SaveChanges();
                return ck;
            }
        }
        /// <summary>
        /// Them noise word cho kieu lam bai MAP
        /// </summary>
        /// <param name="exerciseGroupID"></param>
        /// <param name="Title"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_ExerciceMultiChoise insertNoiseWord(int exerciseGroupID, string Title, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciceMultiChoise ck = new tbl_ExerciceMultiChoise();
                ck.ExerciseID = 0;
                ck.ExerciseGroupID = exerciseGroupID;
                ck.isHide = false;
                ck.IsNoise = true;
                ck.isTrue = false;
                ck.AnswerTitle = Title;
                ck.CreatedBy = createby;
                ck.CreatedDate = GetDateTime.Now;
                ck.ModifiedBy = createby;
                ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ExerciceMultiChoise.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_ExerciceMultiChoise updateNoiseWord(int ID, string Title, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciceMultiChoise ck = db.tbl_ExerciceMultiChoise.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.AnswerTitle = Title;
                    ck.CreatedBy = createby;
                    ck.CreatedDate = GetDateTime.Now;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }                
                return ck;
            }
        }
        public static tbl_ExerciceMultiChoise deleteNoiseWord(int ID, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciceMultiChoise ck = db.tbl_ExerciceMultiChoise.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.isHide = true;
                    ck.CreatedBy = createby;
                    ck.CreatedDate = GetDateTime.Now;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static tbl_ExerciceMultiChoise getnoisewordbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciceMultiChoise ck = db.tbl_ExerciceMultiChoise.Where(n => n.ID == ID).FirstOrDefault();               
                return ck;
            }
        }
        public static List<tbl_ExerciceMultiChoise> getnoisewordbyExGroupID(int exgroupID)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_ExerciceMultiChoise.Where(n => n.ExerciseGroupID == exgroupID && n.isHide!=true).ToList();
                return ck;
            }
        }        
        public static tbl_ExerciceMultiChoise GetTrueChildChoiceByParentID(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciceMultiChoise.Where(n => n.ExerciseID == id && n.isHide != true && n.isTrue == true).FirstOrDefault();
                return l;
            }
        }
    }
}