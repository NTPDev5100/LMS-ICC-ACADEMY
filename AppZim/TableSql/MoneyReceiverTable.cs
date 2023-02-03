using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class MoneyReceiverTable
    {

        public static tbl_MoneyReceiver Insert(int userReceiverId, string note, string bankInfomation,string createdBy)
        {
            using (var db = new ZimEntities())
            {
                if (userReceiverId != 0)
                {
                    var user = AccountTable.getbyID(userReceiverId);
                    if (user != null)
                    {
                        tbl_MoneyReceiver mr = new tbl_MoneyReceiver();
                        mr.UserReceiverId = user.ID;
                        mr.BankInfomation = bankInfomation;
                        mr.Note = note;
                        mr.CreatedBy = createdBy;
                        mr.CreatedDate = DateTime.Now;
                        mr.Payment = 0;
                        mr.IsHide = false;
                        db.tbl_MoneyReceiver.Add(mr);
                        db.SaveChanges();
                        return mr;
                    }                 
                }
                else
                {
                    tbl_MoneyReceiver mr = new tbl_MoneyReceiver();
                    mr.BankInfomation = bankInfomation;
                    mr.Note = note;
                    mr.CreatedBy = createdBy;
                    mr.CreatedDate = DateTime.Now;
                    mr.IsHide = false;
                    db.tbl_MoneyReceiver.Add(mr);
                    db.SaveChanges();
                    return mr;
                }
                return null;
            }    
        }

        public static tbl_MoneyReceiver Update(int id, int? userReceiverId ,string note,string bankInfomation, string modifiedBy, double? payment)
        {
            using (var db = new ZimEntities())
            {
                var entity = db.tbl_MoneyReceiver.SingleOrDefault(x => x.ID == id && x.IsHide == false);
                if (entity != null)
                {
                    entity.UserReceiverId = userReceiverId ?? entity.UserReceiverId;
                    entity.Note = note ?? entity.Note;
                    entity.Payment = payment ?? entity.Payment;
                    entity.BankInfomation = bankInfomation ?? entity.BankInfomation;
                    entity.ModifiedBy = modifiedBy;
                    entity.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                    return entity;
                }
                return null;
            }
        }
   
        //lấy theo ID
        public static tbl_MoneyReceiver getbyID(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_MoneyReceiver.Where(x => x.ID == id && x.IsHide == false).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_MoneyReceiver> GetAll()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_MoneyReceiver.Where(x => x.IsHide == false).ToList();
                return l;
            }
        }

        public static int? GetUserReceiverId(int id)
        {
            using (var db = new ZimEntities())
            {
                var userId = db.tbl_MoneyReceiver.Where(x => x.ID == id && x.IsHide == false).Select(x => x.UserReceiverId).FirstOrDefault();
                return userId;
            }
        }

        public static bool checkUserReceiverExists(int userReceiverId)
        {
            using (var db = new ZimEntities())
            {
                var rs = db.tbl_MoneyReceiver.Where(x => x.UserReceiverId == userReceiverId && x.IsHide == false);
                if (rs.Any())
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Xóa người nhận hộ
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="IsDelete"></param>
        /// <param name="ModifiedBy"></param>
        /// <returns></returns>
        public static tbl_MoneyReceiver Delete(int ID, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_MoneyReceiver.Where(n => n.ID == ID).FirstOrDefault();
                if (ck == null)
                    return ck;
                ck.IsHide = true;
                ck.ModifiedBy = ModifiedBy;
                ck.ModifiedDate = GetDateTime.Now;
                db.SaveChanges();
                return ck;
            }
        }


    }
}