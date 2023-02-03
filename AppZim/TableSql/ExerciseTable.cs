using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ExerciseTable
    {
        //thêm dữ liệu
        public static tbl_Exercise insert(int groupid, string exercise, string filename, string linkfile, int type, int price, string createby, int parentQuestionID,string DescribeAnswer)
        {
            using (var db = new ZimEntities())
            {
                tbl_Exercise ck = new tbl_Exercise();
                ck.ExerciseGroupID = groupid;
                ck.ExerciseTitle = exercise;
                ck.FileName = filename;
                ck.LinkFile = linkfile;
                ck.Type = type;// 1 tự luận, 2 trắc nghiệm, 3 nghe nói
                ck.ExerciseAddonPrice = price;
                ck.WordLimit = 0;
                if (type == 1)
                    ck.WordLimit = 3000;// 3 ngàn từ
                ck.isHide = false;
                ck.IsDelete = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                ck.TopicID = 0;
                ck.PartNumber = 0;
                ck.DescribeAnswer = DescribeAnswer;
                ck.ParentQuestionID = parentQuestionID;//=0 là câu hỏi cha, !=0 là câu hỏi con 
                db.tbl_Exercise.Add(ck);
                db.SaveChanges();
                //cập nhật gói
                ExerciseGroupTable.updatecount(groupid, createby);
                return ck;
            }
        }
        //thêm dữ liệu
        public static tbl_Exercise insert(int groupid, string exercise, string filename, string linkfile, int type, int price, string createby, int parentQuestionID)
        {
            using (var db = new ZimEntities())
            {
                tbl_Exercise ck = new tbl_Exercise();
                ck.ExerciseGroupID = groupid;
                ck.ExerciseTitle = exercise;
                ck.FileName = filename;
                ck.LinkFile = linkfile;
                ck.Type = type;// 1 tự luận, 2 trắc nghiệm, 3 nghe nói
                ck.ExerciseAddonPrice = price;
                ck.WordLimit = 0;
                if (type == 1)
                    ck.WordLimit = 3000;// 3 ngàn từ
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                ck.TopicID = 0;
                ck.PartNumber = 0;
                ck.ParentQuestionID = parentQuestionID;//=0 là câu hỏi cha, !=0 là câu hỏi con 
                db.tbl_Exercise.Add(ck);
                db.SaveChanges();
                //cập nhật gói
                ExerciseGroupTable.updatecount(groupid, createby);
                return ck;
            }
        }
        public static tbl_Exercise update(int id, int groupid, string exercise, bool hide, string createby, int parentQuestionID, string DescribeAnswer)
        {
            using (var db = new ZimEntities())
            {
                tbl_Exercise ck = db.tbl_Exercise.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.ExerciseGroupID = groupid;
                    ck.ExerciseTitle = exercise;
                    ck.isHide = hide;
                    ck.DescribeAnswer = DescribeAnswer;
                    ck.ModifiedBy = createby;
                    ck.ParentQuestionID = parentQuestionID;//=0 là câu hỏi cha, !=0 là câu hỏi con 
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static tbl_Exercise update(int id, int groupid, string exercise, bool hide, string createby, int parentQuestionID)
        {
            using (var db = new ZimEntities())
            {
                tbl_Exercise ck = db.tbl_Exercise.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.ExerciseGroupID = groupid;
                    ck.ExerciseTitle = exercise;
                    ck.isHide = hide;
                    ck.ModifiedBy = createby;
                    ck.ParentQuestionID = parentQuestionID;//= 0 là câu hỏi cha, != 0 là câu hỏi con 
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static tbl_Exercise Delete(int id, bool IsDelete, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_Exercise ck = db.tbl_Exercise.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.IsDelete = IsDelete;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_Exercise getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                tbl_Exercise l = db.tbl_Exercise.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_Exercise> getbygroupid(int gid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Exercise.Where(n => n.ExerciseGroupID == gid && n.IsDelete == false).ToList();
                return l;
            }
        }
        /// <summary>
        /// Lấy xem dạng bài
        /// </summary>
        /// <param name="type">1 tự luận, 2 trắc nghiệm, 3 nghe nói</param>
        /// <returns></returns>
        public static List<tbl_Exercise> getallnothide(int type)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Exercise.Where(n => n.Type == type && n.isHide != true).ToList();
                return l;
            }
        }
        /// <summary>
        /// SET PACKAGE
        /// </summary>
        /// <param name="ExerciseID"></param>
        /// <param name="AnswerTitle"></param>
        /// <returns></returns>
        //lấy câu hỏi của dạng kéo, cái title theo số
        public static tbl_Exercise getbytitleidvsgroupid(int ExerciseGroupID, string AnswerTitle)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Exercise.Where(n => n.ExerciseGroupID == ExerciseGroupID && n.ExerciseTitle == AnswerTitle && n.isHide != true).FirstOrDefault();
                return l;
            }
        }
        /// <summary>
        /// thêm câu hỏi cho topic
        /// </summary>
        /// <param name="topicID"></param>
        /// <param name="partNumber"></param>
        /// <param name="question"></param>
        /// <param name="filename"></param>
        /// <param name="linkfile"></param>
        /// <param name="type"></param>
        /// <param name="price"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_Exercise insertTopicQuestion(int topicID, int partNumber, string question, string filename, string linkfile, int type, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Exercise ck = new tbl_Exercise();
                ck.TopicID = topicID;
                ck.PartNumber = partNumber;
                ck.ExerciseTitle = question;
                ck.FileName = filename;
                ck.LinkFile = linkfile;
                ck.Type = type;// 1 tự luận, 2 trắc nghiệm, 3 nghe nói
                ck.WordLimit = 0;
                ck.WordLimit = 0;
                ck.isHide = false;
                ck.IsDelete = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;

                //default
                ck.ExerciseGroupID = 0;
                ck.ExerciseAddonPrice = 0;
                db.tbl_Exercise.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static List<tbl_Exercise> getListTopicQuestion(int topicID)
        {
            using (var db = new ZimEntities())
            {
                List<tbl_Exercise> l = db.tbl_Exercise.Where(n => n.TopicID == topicID && n.isHide != true && n.IsDelete == false).ToList();
                return l;
            }
        }
        public static tbl_Exercise updateTopicQuestion(int id, int partnumber, string question, string created)
        {
            using (var db = new ZimEntities())
            {
                tbl_Exercise l = db.tbl_Exercise.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.PartNumber = partnumber;
                    l.ExerciseTitle = question;
                    l.ModifiedBy = created;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }
        public static List<tbl_Exercise> getListHasParent(int id)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_Exercise.Where(n => n.ParentQuestionID == id && n.isHide != true && n.IsDelete == false).ToList();
                return ck;
            }
        }
    }
}