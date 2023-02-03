using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class AccountRankTable
    {
        //thêm dữ liệu
        public static tbl_AccountRank insert(int type, int UID, int roleid, int rank, int rate1, int rate2, int rate3, int rate4, int rate5, double rate, double totaltimehour,
            double timehour, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_AccountRank ck = new tbl_AccountRank();
                ck.Type = type;//1 ngày, 2 tuần, 3 tháng, 4 quý, 5 năm

                if (type == 1)
                    ck.TypeName = "Ngày";
                if (type == 2)
                    ck.TypeName = "Tuần";
                if (type == 3)
                    ck.TypeName = "Tháng";
                if (type == 4)
                    ck.TypeName = "Quý";
                if (type == 5)
                    ck.TypeName = "Năm";

                ck.UID = UID;
                ck.RoleRank = roleid;
                ck.DateRun = GetDateTime.Now.Date;
                ck.Rank = rank;
                ck.Rate1 = rate1;
                ck.Rate2 = rate2;
                ck.Rate3 = rate3;
                ck.Rate4 = rate4;
                ck.Rate5 = rate5;
                ck.Rate = rate;
                if (totaltimehour > 0 && timehour > 0)
                {
                    ck.TotalTimeHour = Math.Round(totaltimehour, 2);
                    ck.TimeHour = Math.Round(timehour, 2);
                }
                else
                {
                    ck.TotalTimeHour = 0;
                    ck.TimeHour = 0;
                }

                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_AccountRank.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_AccountRank> getbyuid(int uid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AccountRank.Where(n => n.UID == uid).ToList();
                return l;
            }
        }

        public static List<tbl_AccountRank> getbyrole(int role)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AccountRank.Where(n => n.RoleRank == role).ToList();
                return l;
            }
        }

        public static tbl_AccountRank getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AccountRank.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
    }
}