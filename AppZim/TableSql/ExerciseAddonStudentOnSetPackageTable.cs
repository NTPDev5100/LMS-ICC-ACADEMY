using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ExerciseAddonStudentOnSetPackageTable
    {
        public static tbl_ExerciseAddonStudentOnSetPackage insert(int exerciseAddonStudentID, int part, int exerciseID, string exerciseQuestion, int setPackageID, string setPackageName, string creator)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseAddonStudentOnSetPackage t = new tbl_ExerciseAddonStudentOnSetPackage();
                t.ExerciseAddonStudentID = exerciseAddonStudentID;
                t.Part = part;
                t.ExerciseID = exerciseID;
                t.ExerciseQuestion = exerciseQuestion;
                t.SetPackageID = setPackageID;
                t.SetPackageName = setPackageName;
                t.CreatedBy = t.ModifiedBy = creator;
                t.CreatedDate = t.ModifiedDate = GetDateTime.Now;
                db.tbl_ExerciseAddonStudentOnSetPackage.Add(t);
                db.SaveChanges();
                return t;
            }
        }
        public static tbl_ExerciseAddonStudentOnSetPackage getbyid(int id)
        {
            using (var db=new ZimEntities())
            {
                return db.tbl_ExerciseAddonStudentOnSetPackage.Where(n => n.ID == id).FirstOrDefault();
            }
        }
        public static List<tbl_ExerciseAddonStudentOnSetPackage> getbyexaddid(int exaddonid)
        {
            using (var db = new ZimEntities())
            {
                return db.tbl_ExerciseAddonStudentOnSetPackage.Where(n => n.ExerciseAddonStudentID == exaddonid).ToList();
            }
        }
    }
}