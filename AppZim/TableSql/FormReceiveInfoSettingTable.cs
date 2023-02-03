using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class FormReceiveInfoSettingTable
    {
        //thêm dữ liệu
        public static tbl_FormReceiveInfoSetting insert(int sourceID, int saleID, int roleIDnoti, string title, string content, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_FormReceiveInfoSetting ck = new tbl_FormReceiveInfoSetting();
                ck.SourceCustomerID = sourceID;// nguồn học viên
                ck.SalerID = saleID;// sale nào
                ck.RoleNoti = roleIDnoti;// role nhận email
                ck.TitileForm = title;
                ck.ContentForm = content;// nội dung
                ck.Note = note;//ghi chú
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_FormReceiveInfoSetting.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }


        //thêm dữ liệu
        public static tbl_FormReceiveInfoSetting update(int ID, int sourceID, int saleID, int roleIDnoti, string title, string content, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_FormReceiveInfoSetting ck = db.tbl_FormReceiveInfoSetting.Where(n => n.ID == ID).FirstOrDefault();
                if (ck == null)
                    return null;
                ck.SourceCustomerID = sourceID;// nguồn học viên
                ck.SalerID = saleID;// sale nào
                ck.RoleNoti = roleIDnoti;// role nhận email
                ck.TitileForm = title;
                ck.ContentForm = content;// nội dung
                ck.Note = note;//ghi chú
                ck.isHide = false;
                ck.ModifiedBy = createby;
                ck.ModifiedDate = GetDateTime.Now;               
                db.SaveChanges();
                return ck;
            }
        }

        //get all
        public static List<tbl_FormReceiveInfoSetting> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FormReceiveInfoSetting.ToList();
                return l;
            }
        }

        //get by id
        public static tbl_FormReceiveInfoSetting getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FormReceiveInfoSetting.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
    }
}