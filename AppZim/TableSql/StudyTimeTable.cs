using AppZim.Models;
using AppZim.ZIM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AppZim.TableSql
{
    public class StudyTimeTable
    {
        public static tbl_StudyTime insert(int timeofday, string studyname, int lesson, string stime, string etime, string createby)
        {
            using (var db = new ZimEntities())
            {
                var check = db.tbl_StudyTime.Where(n => n.sTime == stime && n.eTime == etime).FirstOrDefault();
                if (check != null)
                    return check;
                tbl_StudyTime Cl = new tbl_StudyTime();
                Cl.TimeOfDay = timeofday;
                Cl.StudyTimeName = studyname;
                Cl.sTime = stime;
                Cl.eTime = etime;
                Cl.isHide = false;
                Cl.StyleLesson = lesson;
                Cl.CreatedBy = Cl.ModifiedBy = createby;
                Cl.CreatedDate = Cl.ModifiedDate = GetDateTime.Now;
                db.tbl_StudyTime.Add(Cl);
                db.SaveChanges();
                return Cl;
            }
        }

        public static tbl_StudyTime update(int ID, int timeofday, string studyname, int lesson, string stime, string etime, bool ishide, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_StudyTime Cl = db.tbl_StudyTime.Where(n => n.ID == ID).FirstOrDefault();
                Cl.TimeOfDay = timeofday;
                Cl.StudyTimeName = studyname;
                Cl.StyleLesson = lesson;
                Cl.sTime = stime;
                Cl.eTime = etime;
                Cl.isHide = ishide;
                Cl.ModifiedBy = createby;
                Cl.ModifiedDate = GetDateTime.Now;
                db.SaveChanges();
                return Cl;
            }
        }

        public static List<tbl_StudyTime> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudyTime.Where(x=>x.isHide != true).OrderBy(n => n.sTime).ToList();
                return l;
            }
        }
        public static tbl_StudyTime getbyid(int ID)
        {

            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudyTime.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }
        /// <summary>
        /// Chỉ lấy ID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static int getid(int ID)
        {
            using (var db = new ZimEntities())
            {
                int id = db.tbl_StudyTime.Where(n => n.ID == ID).Select(x => x.ID).Single();
                return id;
            }
        }
        public static List<tbl_StudyTime> getByTimeOfDay(int time)
        {
            using (var db = new ZimEntities())
            {
                var data = db.tbl_StudyTime.Where(n => n.TimeOfDay == time && n.isHide == false).ToList();
                return data;
            }
        }
    }
}