using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Energie.Car
{
    public static class KC
    {
        private static string _klantconfig = "";
        private static string _connstringPortaal = "";
        public static string ConnString;
        public static Int64 HoofdPV;
        public static Int64 HoofdLV;
        public static string CertPV;
        public static string CertPVPassword;
        public static string CertLV;
        public static string CertLVPassword;
        public static string XMLPath;
        public static string ExcelPath;
        public static string FTPUser;
        public static string FTPPassword;
        public static string FTPServer;
        
        public static AppID App_ID;
        public static string CarUrl;
        public static Boolean blnPV;
        public static String CarServiceUrl;
        public static String B2BGateway;
        public static String GLDPMUrl;
        public static String ScheduleUrl;
        public static String Allocation2Url;
        public static Int64 TennetEan13;
        public static string KlantConfig
        {
            get { return _klantconfig; }
            set
            {
                _klantconfig = value;
                if (_klantconfig != "" && _connstringPortaal != "" )
                {
                    ReadKlantConfig();
                }
            }
        }

        public static string ConnStringPortaal
        {
            get { return _connstringPortaal; }
            set
            {
                _connstringPortaal = value;
                if (_klantconfig != "" && _connstringPortaal != "")
                {
                    ReadKlantConfig();
                }
            }
        }

        private static void ReadKlantConfig()
        {
            SqlConnection conn = new SqlConnection(_connstringPortaal);
            conn.Open();

            string strSql = "SELECT ConnString FROM KlantConfig.dbo.KlantConfig WHERE KlantConfig = @KlantConfig \n";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@KlantConfig", _klantconfig);
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                ConnString = rdr.GetString(0);
            }
            rdr.Close();

            conn.Close();
            conn = new SqlConnection(ConnString);
            conn.Open();

            strSql = "SELECT HoofdPV,HoofdLV,CertPV,CertPVPassword,CertLV,CertLVPassword,XMLPath,FTPUser \n";
            strSql += ", FTPPassword, FTPServer,  ExcelPath, CarUrl, blnPV, CarServiceUrl, B2BGateway, GLDPMUrl, ScheduleUrl, Allocation2Url,TennetEan13 FROM EnergieDB.dbo.KlantConfig WHERE KlantConfig = @KlantConfig \n";
            cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@KlantConfig", _klantconfig);
            rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                HoofdPV = rdr.GetInt64(0);
                HoofdLV = rdr.GetInt64(1);
                CertPV = rdr.GetString(2);
                CertPVPassword = rdr.GetString(3);
                CertLV = rdr.GetString(4);
                CertLVPassword = rdr.GetString(5);
                XMLPath = rdr.GetString(6);
                FTPUser = rdr.GetString(7);
                FTPPassword = rdr.GetString(8);
                FTPServer = rdr.GetString(9);
       //         KlantIdElk = rdr.GetInt32(10);
       //         KlantIdGas = rdr.GetInt32(11);
                ExcelPath = rdr.GetString(10);
                CarUrl = rdr.GetString(11);
                blnPV = rdr.GetBoolean(12);
                CarServiceUrl = rdr.GetString(13);
                B2BGateway = rdr.GetString(14);
                GLDPMUrl = rdr.GetString(15);
                ScheduleUrl = rdr.GetString(16);
                Allocation2Url = rdr.GetString(17);
                TennetEan13 = rdr.GetInt64(18);
            }
            rdr.Close();

            conn.Close();
        }
    }

    public enum AppID
    {
        EDBG = 1,
        VerwerkCar = 2,
        ProcessBerichten = 3,
        Car = 4
    }
}
