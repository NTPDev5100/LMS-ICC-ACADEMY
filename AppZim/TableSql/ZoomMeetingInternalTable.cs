using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ZoomMeetingInternalTable
    {
        public static tbl_ZoomMeetingInternal Insert(string topic, int roleId, string arrayUID, bool isSendMail,DateTime sDate,string sTime, string roomId, string roomPass, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ZoomMeetingInternal ck = new tbl_ZoomMeetingInternal();
                ck.Topic = topic;
                ck.RangeType = roleId;
                switch (roleId)
                {
                    case 0:
                        ck.RangeTypeName = "Tất cả";
                        break;
                    case 1:
                        ck.RangeTypeName = "Supper Admin";
                        break;
                    case 4:
                        ck.RangeTypeName = "Giáo viên";
                        break;
                    case 7:
                        ck.RangeTypeName = "Học vụ";
                        break;
                    case 9:
                        ck.RangeTypeName = "Kế toán";
                        break;
                    default:
                        ck.RangeTypeName = "Không biết";
                        break;
                }
                ck.ArrayUID = arrayUID;
                ck.isSendMail = isSendMail;
                ck.StartDate = sDate;
                ck.StartTime = sTime;
                ck.RoomId = roomId;
                ck.RoomPass = roomPass;
                ck.CreatedBy = createby;
                ck.isStart = false;
                ck.CreatedDate = DateTime.Now;
                db.tbl_ZoomMeetingInternal.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static bool UpdateisStart(int ID,bool isStart, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_ZoomMeetingInternal ck = db.tbl_ZoomMeetingInternal.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.isStart = isStart;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
        }
        public static List<tbl_ZoomMeetingInternal> GetAll()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ZoomMeetingInternal.ToList();
                return l;
            }
        }
        public static tbl_ZoomMeetingInternal getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ZoomMeetingInternal.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }
    }
}