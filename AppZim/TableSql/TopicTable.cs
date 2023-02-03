using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppZim.Models;

namespace AppZim.TableSql
{
    public class TopicTable
    {
        /// <summary>
        /// Thêm topic
        /// </summary>
        /// <param name="topicname">Tên topic</param>
        /// <param name="topicdescription">Mô tả</param>
        /// <param name="topictype">Loại topic: 1 writing, 2 speaking</param>
        /// <param name="createdby"></param>
        /// <returns></returns>

        public static tbl_Topic insert(string topicname, string topicdescription, string createdby)
        {
            using (var db= new ZimEntities())
            {
                tbl_Topic t = new tbl_Topic();
                t.TopicName = topicname;
                t.TopicDescription = topicdescription;
                t.CreateDate = t.ModifiedDate = GetDateTime.Now;
                t.CreatedBy = t.ModifiedBy = createdby;
                t.IsHide = false;
                t.Status = 1;    //1 active, 2 off           
                db.tbl_Topic.Add(t);
                db.SaveChanges();
                return t;
            }
        }   
        public static List<tbl_Topic> getall()
        {

            using (var db = new ZimEntities())
            {
                var l = db.tbl_Topic.Where(n=>n.IsHide!=true).ToList();
                return l;
            }
        }
        public static tbl_Topic getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Topic.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        public static tbl_Topic update(int id,string tn,string td, string createdby)
        {
            using (var db = new ZimEntities()) 
            {
                var tp = db.tbl_Topic.Where(n => n.ID == id).FirstOrDefault();
                if (tp != null)
                {
                    tp.TopicName = tn;                 
                    tp.TopicDescription = td;
                    tp.ModifiedBy = createdby;
                    tp.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                   
                }
                return tp;
            }
        }
        public static tbl_Topic active(int id,int active,string createdby)
        {
            using (var db = new ZimEntities())
            {
                var tp = db.tbl_Topic.Where(n => n.ID == id).FirstOrDefault();
                if (tp != null)
                {
                    tp.Status = active;
                    tp.ModifiedBy = createdby;
                    tp.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();

                }
                return tp;
            }
        }
        public static tbl_Topic delete(int id,string createdby)
        {
            using (var db = new ZimEntities())
            {
                var tp = db.tbl_Topic.Where(n => n.ID == id).FirstOrDefault();
                if (tp != null)
                {
                    tp.IsHide = true;
                    tp.ModifiedBy = createdby;
                    tp.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();

                }
                return tp;
            }
        }
    }
}