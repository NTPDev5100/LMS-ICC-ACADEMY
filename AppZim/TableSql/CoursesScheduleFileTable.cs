using AppZim.Models;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AppZim.TableSql
{
    public class CoursesScheduleFileTable
    {
        public static tbl_CoursesScheduleFile insert(tbl_CoursesScheduleFile coursesScheduleFile, string user)
        {
            using (var db = new ZimEntities())
            {
                var data = new tbl_CoursesScheduleFile
                {
                    CoursesScheduleID = coursesScheduleFile.CoursesScheduleID,
                    TeacherAttendanceID = coursesScheduleFile.TeacherAttendanceID,
                    FileName =coursesScheduleFile.FileName,
                    IsHide = false,
                    CreatedDate = DateTime.Now,
                    CreatedBy = user,
                    ModifiedDate = DateTime.Now,
                    ModifiedBy = user
                };
                db.tbl_CoursesScheduleFile.Add(data);
                db.SaveChanges();
                return data;
            }
        }
        public static tbl_CoursesScheduleFile update(tbl_CoursesScheduleFile coursesScheduleFile, string user)
        {
            using (var db = new ZimEntities())
            {
                var data = db.tbl_CoursesScheduleFile.SingleOrDefault(x => x.ID == coursesScheduleFile.ID);
                if (data == null) return null;
                data.TeacherAttendanceID = coursesScheduleFile.TeacherAttendanceID == null ?
                     data.TeacherAttendanceID : coursesScheduleFile.TeacherAttendanceID;
                data.FileName = string.IsNullOrEmpty(coursesScheduleFile.FileName) ? 
                    data.FileName : coursesScheduleFile.FileName;
                data.IsHide = coursesScheduleFile.IsHide == null ?
                    data.IsHide : coursesScheduleFile.IsHide;
                data.ModifiedDate = DateTime.Now;
                data.ModifiedBy = user;
                db.SaveChanges();
                return data;
            }
        }
        public static tbl_CoursesScheduleFile getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                return db.tbl_CoursesScheduleFile
                    .SingleOrDefault(x => x.ID == id);
            }
        }
        public static List<tbl_CoursesScheduleFile> getbyScheduleID(int id)
        {
            using (var db = new ZimEntities())
            {
                var data = db.tbl_CoursesScheduleFile.Where(x => x.CoursesScheduleID == id && x.IsHide == false).ToList();
                return data;
            }
        }
    }
}