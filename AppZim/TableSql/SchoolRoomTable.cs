using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class SchoolRoomTable
    {
        //thêm dữ liệu
        public static tbl_SchoolRoom insert(string roomname, int schoolid, string schoolname, string roomdes, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_SchoolRoom ck = new tbl_SchoolRoom();
                ck.RoomName = roomname;
                ck.SchoolID = schoolid;
                ck.SchoolName = schoolname;
                ck.RoomDescription = roomdes;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_SchoolRoom.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_SchoolRoom update(int ID, string roomname, string roomdes, int schoolid, string schoolname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_SchoolRoom ck = db.tbl_SchoolRoom.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.RoomName = roomname;
                    ck.SchoolID = schoolid;
                    ck.SchoolName = schoolname;
                    ck.RoomDescription = roomdes;
                    ck.isHide = false;
                    ck.isHide = false;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_SchoolRoom getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SchoolRoom.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_SchoolRoom> getbyschoolid(int schoolid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SchoolRoom.Where(n=>n.SchoolID == schoolid).ToList();
                return l;
            }
        }
    }
}