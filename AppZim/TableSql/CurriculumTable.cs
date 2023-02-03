using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CurriculumTable
    {
        //thêm dữ liệu
        public static tbl_Curriculum insert(int classid, string curriculumname, int style, int totallensson, 
            string createby, string linkcurrriculumn, tbl_Language language)
        {
            using (var db = new ZimEntities())
            {
                tbl_Curriculum ck = new tbl_Curriculum();
                ck.ClassID = classid;
                ck.CurriculumsName = curriculumname;
                ck.StyleLensson = style;
                ck.TotalLesson = totallensson;
                ck.LinkCurriculumn = linkcurrriculumn;
                ck.LanguageId = language.ID;
                ck.LanguageName = language.Name;
                ck.IsHide = false;
                ck.IsDelete = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_Curriculum.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_Curriculum update(int ID, int classid, string curriculumname, int style, int totallensson,
            bool ishide, string createby, string linkcurrriculumn, tbl_Language language)
        {
            using (var db = new ZimEntities())
            {
                tbl_Curriculum ck = db.tbl_Curriculum.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.ClassID = classid;
                    ck.CurriculumsName = curriculumname;
                    ck.TotalLesson = totallensson;
                    ck.StyleLensson = style;
                    ck.LinkCurriculumn = linkcurrriculumn;
                    ck.LanguageId = language.ID;
                    ck.LanguageName = language.Name;
                    ck.IsHide = ishide;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_Curriculum Delete(int ID, bool IsDelete, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_Curriculum ck = db.tbl_Curriculum.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.IsDelete = IsDelete;
                    ck.IsHide = IsDelete;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_Curriculum getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Curriculum.Where(n => n.ID == ID && n.IsDelete == false).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_Curriculum> getbyclass(int classid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Curriculum.Where(n => n.ClassID == classid).Where(n => n.IsHide != true && n.IsDelete == false).ToList();
                return l;
            }
        }
        public static List<tbl_Curriculum> GetAll()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Curriculum.Where(n => n.IsHide != true && n.IsDelete == false).ToList();
                return l;
            }
        }
        public static List<tbl_Curriculum> getbyclasshide(int classid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Curriculum.Where(n => n.ClassID == classid && n.IsDelete == false).ToList();
                return l;
            }
        }
        /// <summary>
        /// lấy giáo trình có số buổi học nhiều nhất của 1 class
        /// </summary>
        /// <param name="classid"></param>
        /// <returns></returns>
        public static tbl_Curriculum getmaxlesson(int classid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Curriculum.Where(n => n.ClassID == classid && n.IsDelete == false).OrderByDescending(x => x.TotalLesson).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_Curriculum> getByLanguage(int languageId)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Curriculum.Where(n => n.LanguageId == languageId && n.IsDelete == false).ToList();
                return l;
            }
        }
    }
}