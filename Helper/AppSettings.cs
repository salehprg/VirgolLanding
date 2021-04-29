﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using virgollanding.Models;

namespace virgollanding.Helper
{
    public class AppSettings
    {
        //EmailServer Configuration
        public static string smtpHost { get; set; }
        public static string smtpPort { get; set; }
        public static string smtpUsername { get; set; }
        public static string smtpPassword { get; set; }

        //SMS API (Faraz sms) Configuration
        public static string FarazAPI_Username { get; set; }
        public static string FarazAPI_URL { get; set; }
        public static string FarazAPI_SendNumber { get; set; }
        public static string FarazAPI_Password { get; set; }
        public static string FarazAPI_ApiKey { get; set; }

        //--------------------
        public static string JWTSecret { get; set; }
        public static string GetValueFromDatabase (AppDbContext appDbContext , string key)
        {
            try
            {
                SiteSettings settings = appDbContext.SiteSettings.Where(x => x.key == key).FirstOrDefault();
                if(settings != null)
                {
                    return settings.value;
                }

                throw new Exception("Key Not Found !");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}\n" , JWTSecret);
        }


    }
}
