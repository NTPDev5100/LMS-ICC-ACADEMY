using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace AppZim
{
    public  struct  GetDateTime
    {
        public static DateTime Now
        {       
            get { return DateTime.UtcNow.AddHours(7); }
        }
    }
    public struct Media
    {
        public static string Host
        {
            get {return ConfigurationManager.AppSettings["Host"].ToString(); }
        }
    }
}