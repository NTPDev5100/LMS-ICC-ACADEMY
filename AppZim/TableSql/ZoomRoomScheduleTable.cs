using AppZim.TableSql;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppZim.Models;

namespace AppZim.TableSql
{
    public class ZoomRoomScheduleTable
    {
        public static tbl_ZoomRoomSchedule Insert(int CourseID, DateTime Date, int StudyTimeID, string RoomID, string RoomPass, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ZoomRoomSchedule ck = new tbl_ZoomRoomSchedule();
                ck.CourseID = CourseID;
                ck.Date = Date;
                ck.StudyTimeID = StudyTimeID;
                ck.RoomID = RoomID;
                ck.RoomPass = RoomPass;
                ck.isStart = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ZoomRoomSchedule.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        //public static tbl_ZoomTeacherAPI Update(int ID, string UserID, string ApiKey, string ApiSecrect, string Token, string ModifiedBy)
        //{
        //    using (var db = new ZimEntities())
        //    {
        //        tbl_ZoomTeacherAPI ck = db.tbl_ZoomTeacherAPI.Where(n => n.ID == ID).FirstOrDefault();
        //        if (ck != null)
        //        {
        //            ck.UserID = UserID;
        //            ck.APIKey = ApiKey;
        //            ck.APISecrect = ApiSecrect;
        //            ck.Token = Token;
        //            ck.ModifiedBy = ModifiedBy;
        //            ck.ModifiedDate = GetDateTime.Now;
        //            db.SaveChanges();
        //            return ck;
        //        }
        //        return null;
        //    }
        //}

        public static bool Update(int ID, bool isStart, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_ZoomRoomSchedule ck = db.tbl_ZoomRoomSchedule.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.isStart = isStart;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public static tbl_ZoomRoomSchedule getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ZoomRoomSchedule.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_ZoomRoomSchedule> GetAll()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ZoomRoomSchedule.ToList();
                return l;
            }
        }
    }
}