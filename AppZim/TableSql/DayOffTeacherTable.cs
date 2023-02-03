using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class DayOffTeacherTable
    {
        //thêm dữ liệu
        public static tbl_DayOffTeacher insert(int UID, DateTime dayoff, int studytime, string createby)
        {
            using (var db = new ZimEntities())
            {
                var check = db.tbl_DayOffTeacher.Where(n => n.UID == UID && n.DayOff == dayoff.Date && n.StudyTimeID == studytime).FirstOrDefault();
                if (check != null)
                {
                    if (check.isHide.Value)
                    {
                        check.isHide = false;
                        check.ModifiedBy = createby;
                        check.ModifiedDate = GetDateTime.Now;
                        db.SaveChanges();
                    }
                    return check;
                }
                tbl_DayOffTeacher ck = new tbl_DayOffTeacher();
                ck.UID = UID;
                ck.DayOff = dayoff.Date;
                ck.StudyTimeID = studytime; ;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_DayOffTeacher.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_DayOffTeacher update(int ID, bool hide, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_DayOffTeacher ck = db.tbl_DayOffTeacher.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.isHide = hide;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_DayOffTeacher getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_DayOffTeacher.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_DayOffTeacher> getbyuiddate(int uid, DateTime day)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_DayOffTeacher.Where(n => n.UID == uid && n.DayOff.Value == day.Date).ToList();
                return l;
            }
        }

        /// <summary>
        /// Check xem ngày đó, ca đó GV có rảnh không
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="day"></param>
        /// <param name="StudyTimeID"></param>
        /// <returns></returns>
        public static bool CheckGVCoRanhKhong(int uid, DateTime day, int StudyTimeID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_DayOffTeacher.Where(n => n.UID == uid && n.DayOff.Value == day.Date && n.StudyTimeID == StudyTimeID && n.isHide == false).FirstOrDefault();
                if (l == null)
                {
                    return true;  //nó rảnh nè
                }
                else
                {
                    return false; //nó bận rồi
                }
            }
        }

        public static List<tbl_DayOffTeacher> getbyuid(int uid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_DayOffTeacher.Where(n => n.UID == uid).ToList();
                return l;
            }
        }

        public static List<tbl_DayOffTeacher> getbyuidmonth(int uid, DateTime date)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_DayOffTeacher.Where(n => n.UID == uid && n.DayOff.Value.Month == date.Month && n.DayOff.Value.Year == date.Year).ToList();
                return l;
            }
        }
        public static List<tbl_DayOffTeacher> GetAllInWeek(int uid, DateTime monday, DateTime sunday)
        {
            using (var db = new ZimEntities())
            {
                monday = monday.Date;
                sunday = sunday.Date;
                var l = db.tbl_DayOffTeacher.Where(n => n.UID == uid && monday <= n.DayOff && n.DayOff <= sunday).ToList();
                return l;
            }
        }
    }
}