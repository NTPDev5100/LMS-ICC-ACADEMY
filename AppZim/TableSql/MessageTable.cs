using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class MessageTable
    {
        //thêm dữ liệu
        public static tbl_Message insert(int FromID, int ToID, string Content, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Message ck = new tbl_Message();
                ck.FromUID = FromID;// người gửi
                ck.ToUID = ToID;// người nhận
                ck.MessageContent = Content;// nội dung
                ck.MessageNew = true;// tin nhắn mới
                ck.QuickView = false;// xem nhanh
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_Message.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        //lấy tin nhắn nhận
        public static List<tbl_Message> getbyfIDvstID(int FID, int TID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Message.Where(n => (n.ToUID == FID || n.ToUID == TID) && (n.FromUID == FID || n.FromUID == TID)).ToList();
                return l;
            }
        }

        //lấy tin nhắn được nhận, do người khác gửi đên
        public static List<tbl_Message> getbyreceiveID(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Message.Where(n => n.ToUID == id).ToList();
                return l;
            }
        }

        public static void updateview(int fid, int tid, string create)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Message.Where(n => n.FromUID == fid && n.ToUID == tid && n.MessageNew == true).ToList();
                foreach(var item in l)
                {
                    item.MessageNew = false;
                    item.ModifiedBy = create;
                    item.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
            }
        }
    }
}