using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Energie.Car
{
    public class CarShared
    {
        //private DataRow drKlantConfig;
        private string strSql;
        private SqlConnection conn;
        private SqlCommand cmd;

        public CarShared()
        {

        }

        public int Save_Bericht(int type, string bericht, string onderwerp, Boolean inkomend, string berichtIDSender, Boolean verwerkt, Boolean fout)
        {
            conn = new SqlConnection(KC.ConnString);
            conn.Open();
            int Bericht_ID = -1;


            strSql = "INSERT INTO Car.dbo.Berichten \n";
            strSql += "(Inkomend \n";
            strSql += ",Datum \n";
            strSql += ",Onderwerp \n";
            strSql += ",Bericht \n";
            strSql += ",Type \n";
            strSql += ",BerichtID_Sender \n";
            strSql += ",Verwerkt \n";
            strSql += ",Fout) \n";
            strSql += "VALUES \n";
            strSql += "(@Inkomend \n";
            strSql += ",@Datum \n";
            strSql += ",@Onderwerp \n";
            strSql += ",@Bericht \n";
            strSql += ",@Type \n";
            strSql += ",@BerichtID_Sender \n";
            strSql += ",@Verwerkt \n";
            strSql += ",@Fout); SELECT @Bericht_ID = SCOPE_IDENTITY();";


            cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@Inkomend", inkomend);
            cmd.Parameters.AddWithValue("@Datum", DateTime.Now);
            cmd.Parameters.AddWithValue("@Onderwerp", onderwerp);
            cmd.Parameters.AddWithValue("@Bericht", bericht);
            cmd.Parameters.AddWithValue("@Type", type);
            cmd.Parameters.AddWithValue("@BerichtID_Sender", berichtIDSender);
            cmd.Parameters.AddWithValue("@Verwerkt", verwerkt);
            cmd.Parameters.AddWithValue("@Fout", fout);
            

            cmd.Parameters.Add(new SqlParameter("@Bericht_ID", SqlDbType.Int));
            cmd.Parameters["@Bericht_ID"].Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                Bericht_ID = (int)cmd.Parameters["@Bericht_ID"].Value;
                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Er is iets fout gegaan met het bewaren van het Switchbericht Electra (inbox), we adviseren U contact op te nemen met IT");
                //MessageBox.Show(ex.ToString());
                SchrijfLog("Er is iets fout gegaan met het bewaren van het Switchbericht Electra inbox : + " + ex.Message, 10, -1, KC.App_ID);
            }
            conn.Close();
            return Bericht_ID;

        }

        public void SchrijfLog(string omschrijving, int foutLevel, int bericht_ID, AppID App_ID)
        {
            try
            {
                SqlConnection conn = new SqlConnection(KC.ConnString);
                conn.Open();
                SqlCommand cmdLog = new SqlCommand();
                cmdLog.Connection = conn;
                strSql = "INSERT INTO Car.dbo.Log \n";
                strSql += "(TimeStamp \n";
                strSql += ", App_ID \n";
                strSql += ", Omschrijving \n";
                strSql += ", FoutLevel \n";
                strSql += ", Bericht_ID) \n";
                strSql += "VALUES \n";
                strSql += "(@TimeStamp \n";
                strSql += ", @App_ID \n";
                strSql += ", @Omschrijving \n";
                strSql += ", @FoutLevel \n";
                strSql += ", @Bericht_ID)";
                cmdLog.CommandText = strSql;
                cmdLog.Parameters.AddWithValue("@TimeStamp", DateTime.Now);
                cmdLog.Parameters.AddWithValue("@App_ID", App_ID);
                string strDescription = omschrijving;
                if (strDescription.Length > 500) { strDescription = strDescription.Substring(0, 500); }
                
                cmdLog.Parameters.AddWithValue("@Omschrijving", strDescription);
                cmdLog.Parameters.AddWithValue("@FoutLevel", foutLevel);
                cmdLog.Parameters.AddWithValue("@Bericht_ID", bericht_ID);
                cmdLog.ExecuteNonQuery();
                conn.Close();
            }
            catch //(Exception ex)
            {
                //EventLog eventlog = new EventLog("Application");
                //eventlog.Source = "Energie App";
                //eventlog.WriteEntry("WriteLog : " + ex.Message, EventLogEntryType.Error, 0);
            }
        }

        public string SchrijfGain(GainItem gainItem)
        {
            string result = "";

            try
            {
                SqlConnection conn = new SqlConnection(KC.ConnString);
                conn.Open();
                strSql = "INSERT INTO Car.dbo.Gain \n";
                strSql += "(Bericht_ID \n";
                strSql += ",Datum \n";
                strSql += ",Ontvanger \n";
                strSql += ",EAN18_Code \n";
                strSql += ",Product \n";
                strSql += ",NB \n";
                strSql += ",PV \n";
                strSql += ",LV \n";
                strSql += ",Dossier \n";
                strSql += ",Reden \n";
                strSql += ",Referentie \n";
                if (gainItem.OudeLV != -1) { strSql += ",OudeLV \n"; }
                strSql += ")VALUES \n";
                strSql += "(@Bericht_ID \n";
                strSql += ",@Datum \n";
                strSql += ",@Ontvanger \n";
                strSql += ",@EAN18_Code \n";
                strSql += ",@Product \n";
                strSql += ",@NB \n";
                strSql += ",@PV \n";
                strSql += ",@LV \n";
                strSql += ",@Dossier \n";
                strSql += ",@Reden \n";
                strSql += ",@Referentie";
                if (gainItem.OudeLV != -1) { strSql += ",@OudeLV"; }
                strSql += ")";

                SqlCommand cmd = new SqlCommand(strSql, conn);
                cmd.Parameters.AddWithValue("@Bericht_ID", gainItem.BerichtID);
                cmd.Parameters.AddWithValue("@Datum", gainItem.Datum);
                cmd.Parameters.AddWithValue("@Ontvanger", gainItem.Ontvanger);
                cmd.Parameters.AddWithValue("@EAN18_Code", gainItem.EAN18_Code);
                cmd.Parameters.AddWithValue("@Product", gainItem.Product);
                cmd.Parameters.AddWithValue("@NB", gainItem    .NetbeheerderEAN);
                cmd.Parameters.AddWithValue("@PV", gainItem.PV);
                cmd.Parameters.AddWithValue("@LV", gainItem.LV);
                cmd.Parameters.AddWithValue("@Dossier", gainItem.Dossier);
                cmd.Parameters.AddWithValue("@Reden", gainItem.Reden);
                cmd.Parameters.AddWithValue("@Referentie", gainItem.Referentie);
                if (gainItem.OudeLV != -1) { cmd.Parameters.AddWithValue("@OudeLV", gainItem.OudeLV); }
                cmd.ExecuteNonQuery();
                result = "OK";
            }
            catch(Exception ex)
            {
                result = "ERROR";
                SchrijfLog("Fout bij wegschrijven gain : " + ex.Message, 10, gainItem.BerichtID, gainItem.AppID);
            }

            if (conn.State == ConnectionState.Open) { conn.Close(); }
            return result;
        }


        public string SchrijfLoss(LossItem lossItem)
        {
            string result = "";

            try
            {
                SqlConnection conn = new SqlConnection(KC.ConnString);
                conn.Open();
                strSql = "INSERT INTO Car.dbo.Loss \n";
                strSql += "(Bericht_ID \n";
                strSql += ",Datum \n";
                strSql += ",Ontvanger \n";
                strSql += ",EAN18_Code \n";
                strSql += ",Product \n";
                strSql += ",NB \n";
                strSql += ",PV \n";
                strSql += ",LV \n";
                if (lossItem.OudeLV != -1) { strSql += ",OudeLV"; }
                strSql += ",Dossier \n";
                strSql += ",Reden \n";
                strSql += ",Referentie \n";
                if (lossItem.OudePV != -1) { strSql += ",OudePV"; }
                strSql += ") \n";
                strSql += "VALUES \n";
                strSql += "(@Bericht_ID \n";
                strSql += ",@Datum \n";
                strSql += ",@Ontvanger \n";
                strSql += ",@EAN18_Code \n";
                strSql += ",@Product \n";
                strSql += ",@NB \n";
                strSql += ",@PV \n";
                strSql += ",@LV \n";
                if (lossItem.OudeLV != -1) { strSql += ",@OudeLV"; }
                strSql += ",@Dossier \n";
                strSql += ",@Reden \n";
                strSql += ",@Referentie \n";
                if (lossItem.OudePV != -1) { strSql += ",@OudePV"; }
                strSql += ")";

                SqlCommand cmd = new SqlCommand(strSql, conn);
                cmd.Parameters.AddWithValue("@Bericht_ID", lossItem.BerichtID);
                cmd.Parameters.AddWithValue("@Datum", lossItem.Datum);
                cmd.Parameters.AddWithValue("@Ontvanger", lossItem.Ontvanger);
                cmd.Parameters.AddWithValue("@EAN18_Code", lossItem.EAN18_Code);
                cmd.Parameters.AddWithValue("@Product", lossItem.Product);
                cmd.Parameters.AddWithValue("@NB", lossItem.NetbeheerderEAN);
                cmd.Parameters.AddWithValue("@PV", lossItem.PV);
                cmd.Parameters.AddWithValue("@LV", lossItem.LV);
                cmd.Parameters.AddWithValue("@Dossier", lossItem.Dossier);
                cmd.Parameters.AddWithValue("@Reden", lossItem.Reden);
                cmd.Parameters.AddWithValue("@Referentie", lossItem.Referentie);
                if (lossItem.OudeLV != -1) { cmd.Parameters.AddWithValue("@OudeLV", lossItem.OudeLV); }
                if (lossItem.OudePV != -1) { cmd.Parameters.AddWithValue("@OudePV", lossItem.OudePV); }
                cmd.ExecuteNonQuery();
                result = "OK";
            }
            catch (Exception ex)
            {
                result = "ERROR";
                SchrijfLog("Fout bij wegschrijven loss : " + ex.Message, 10, lossItem.BerichtID, lossItem.AppID);
            }

            if (conn.State == ConnectionState.Open) { conn.Close(); }
            return result;
        }

        public void Save_Switch(int aansluitingID, int BerichtID, int BerichtIDGainLV, int BerichtIDGainPV)
        {
            try
            {
                SqlConnection conn = new SqlConnection(KC.ConnString);
                conn.Open();

                String strSql =
                        "update [EnergieDB].[dbo].[Switchberichten] set BerichtIDSwitch = @outboxID, \n";
                if (BerichtIDGainLV != -1) { strSql += "BerichtIDGainLV =@BerichtIDGainLV, \n"; }
                if (BerichtIDGainPV != -1) { strSql += "BerichtIDGainPV =@BerichtIDGainPV  \n"; }
                strSql += "where aansluiting_ID = @aansluitingID and BerichtIDSwitch = 0";

                SqlCommand cmd = new SqlCommand(strSql, conn);
                cmd.Parameters.Add(new SqlParameter("@aansluitingID", SqlDbType.VarChar, 14));
                cmd.Parameters["@aansluitingID"].Value = aansluitingID;
                cmd.Parameters.Add(new SqlParameter("@outboxID", SqlDbType.Int));
                cmd.Parameters["@outboxID"].Value = BerichtID;
                if (BerichtIDGainLV != -1)
                {
                    cmd.Parameters.Add(new SqlParameter("@BerichtIDGainLV", SqlDbType.Int));
                    cmd.Parameters["@BerichtIDGainLV"].Value = BerichtIDGainLV;
                }
                if (BerichtIDGainPV != -1)
                {
                    cmd.Parameters.Add(new SqlParameter("@BerichtIDGainPV", SqlDbType.Int));
                    cmd.Parameters["@BerichtIDGainPV"].Value = BerichtIDGainPV;
                }
                cmd.ExecuteNonQuery();
                
            }
            catch //(Exception ex)
            {

            }

            if (conn.State == ConnectionState.Open) { conn.Close(); }
        }

    }

    public class GainItem
    {
        public int BerichtID;
        public DateTime Datum;
        public Int64 Ontvanger;
        public Int64 EAN18_Code;
        public string Product;
        public Int64 NetbeheerderEAN;
        public Int64 PV;
        public Int64 LV;
        public string Dossier;
        public string Reden;
        public string Referentie;
        public AppID AppID;
        public Int64 OudeLV = -1;
    }
    public class LossItem
    {
        public int BerichtID;
        public DateTime Datum;
        public Int64 Ontvanger;
        public Int64 EAN18_Code;
        public string Product;
        public Int64 NetbeheerderEAN;
        public Int64 PV;
        public Int64 LV;
        public string Dossier;
        public string Reden;
        public string Referentie;
        public AppID AppID;
        public Int64 OudeLV = -1;
        public Int64 OudePV = -1;
    }
}
