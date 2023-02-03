using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class SetPackagePointConfigTable
    {
        public static double GetPoint(int correctAns)
        {
            using (var db = new ZimEntities()) {
                var a = db.tbl_SetPackagePointConfig.Where(n => n.Min <= correctAns && correctAns <= n.Max).FirstOrDefault();
                if (a != null)
                {
                    return a.Point.Value;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}