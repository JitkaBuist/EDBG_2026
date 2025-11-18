using System;
using System.Collections.Generic;
using System.Text;
using Energie.DataTableHelper;
using System.Data.SqlClient;
using System.Data;
//mail libraries can be disabled when testing is done
using System.Net.Mail;
using System.IO;

namespace Energie.SwitchBericht
{
    public class UtilmdE10

    {
        public int aansluitingID;
        public string netbeheerderEan13Code;

        private static String ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB");
        static string c_EOF = "'\r\n";
        private StringBuilder utilmdE10 = new StringBuilder();
        private string HoofdPV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV").Trim();
        private string HoofdLV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV").Trim();

        public void createUtilmdE10 ()
        {
            //Console.WriteLine("test .. ID = " + aansluitingID.ToString());

            // Setting Objects 
            DateTime datum = DateTime.Now;
            Offset O = new Offset();
            int offset = O.getOffset();
            int countLines = 0;
            string messageID = GetMessageID.getMessageID();
            string controlref = messageID;//datum.ToString("yyMMddHHmm");
            string SQLstatement;
            int utilmdE10MessageID;
            string berichtNaam;            
            long ean18Code;

            //emptying the message
            utilmdE10.Remove(0, utilmdE10.Length);

            //query the ean18_code
            SQLstatement = "select EAN18_Code from Aansluitingen where ID = @aansluiting_ID";
            SqlDataAdapter da = new SqlDataAdapter(SQLstatement, ConnString);
            da.SelectCommand.Parameters.Add(new SqlParameter("@aansluiting_ID", SqlDbType.Int));
            da.SelectCommand.Parameters["@aansluiting_ID"].Value = aansluitingID;
            DataTable dtGet = new DataTable();   
            da.Fill(dtGet);
            ean18Code = (long)dtGet.Rows[0].ItemArray[0];
            berichtNaam = "E10";

            //Create E10bericht
            utilmdE10.Append("UNA:+.? " + c_EOF);
            utilmdE10.Append("UNB+UNOC:3+" + HoofdPV + ":14+8712423010208:14+" + datum.ToString("yyMMdd:HHmm") + "+" + controlref + c_EOF); //moet ons oude ean zijn, (edsn zijn we zo geboekt)
            utilmdE10.Append("UNH+" + messageID + "+UTILMD:D:01C:UN:E4NL32" + c_EOF); //E4NL20 
            utilmdE10.Append("BGM+" + berichtNaam + "+UTILMD" + messageID + "+9+NA" + c_EOF); //E10 = always NA
            utilmdE10.Append("DTM+137:" + DateTime.Now.ToString("yyyyMMddHHmm") + ":203" + c_EOF);
            utilmdE10.Append("DTM+735:?+0" + offset + "00:406" + c_EOF);
            utilmdE10.Append("MKS+23+E02::260" + c_EOF);
            utilmdE10.Append("NAD+MS+" + HoofdLV + "::9" + c_EOF);   
            utilmdE10.Append("NAD+MR+" + netbeheerderEan13Code + "::9" + c_EOF);
            utilmdE10.Append("NAD+DDQ" + c_EOF);
            utilmdE10.Append("IDE+24+" + ean18Code.ToString() + "A" + c_EOF);
            utilmdE10.Append("LOC+172+" + ean18Code.ToString() + "::9" + c_EOF);          
            utilmdE10.Append("STS+7++E32::260" + c_EOF);
            countLines = 12;
            utilmdE10.Append("UNT+" + countLines.ToString() + "+" + messageID.ToString() + c_EOF);
            utilmdE10.Append("UNZ+1+" + controlref + c_EOF);

            //now put it to inbox
            utilmdE10MessageID = this.Save_UtilmdE10(messageID, ean18Code.ToString());

        }


        private int Save_UtilmdE10(string messageID , string ean18_Nummer )
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;
            int outboxE10_MessageID = 0;

            cnPubs.Open();
            SQLstatement =
                    "INSERT INTO [Messages].[dbo].[Outbox] " +
                    "([Message_id] " +
                    ",[EdineMessagetype_id] " +
                    ",[Bericht] " +
                    ",[Omschrijving] " +
                    ",[Verzonden] " +
                    ",[EdineMailadres] " +
                    ",[BerichtStatus] " +
                    ",[Afzender] "+
                    ",[Outboxtype_ID]) " +
                    "VALUES " +
                    "(@id " +
                    ",@Messagetype_id  " +  
                    ",@Bericht " +
                    ",@omschrijving " +
                    ",null  " +
                    ",@EmailAdres " +
                    ",'TE_VERSTUREN' ";

     
            SQLstatement = SQLstatement + ",'" + HoofdPV + "cps.tennet.org',1); SELECT @outboxID = SCOPE_IDENTITY();";

            SqlCommand cmdSaveOutbox = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveOutbox.Parameters.Add(new SqlParameter("@id", SqlDbType.VarChar, 14));
            cmdSaveOutbox.Parameters["@id"].Value = messageID;
            cmdSaveOutbox.Parameters.Add(new SqlParameter("@Bericht", SqlDbType.NText));
            cmdSaveOutbox.Parameters["@Bericht"].Value = utilmdE10.ToString();
            cmdSaveOutbox.Parameters.Add(new SqlParameter("@omschrijving", SqlDbType.NText));
            cmdSaveOutbox.Parameters["@omschrijving"].Value = "UTILMDE10_" + ean18_Nummer;
            cmdSaveOutbox.Parameters.Add(new SqlParameter("@EmailAdres", SqlDbType.NText));
            cmdSaveOutbox.Parameters["@EmailAdres"].Value = "8712423010208@cps.tennet.org";//eMailAdres;         

            cmdSaveOutbox.Parameters.Add(new SqlParameter("@Messagetype_id", SqlDbType.Int));
            cmdSaveOutbox.Parameters["@Messagetype_id"].Value = 29; //29 = UTILMD E10    

            cmdSaveOutbox.Parameters.Add(new SqlParameter("@outboxID", SqlDbType.Int));
            cmdSaveOutbox.Parameters["@outboxID"].Direction = ParameterDirection.Output;
            try
            {
                cmdSaveOutbox.ExecuteNonQuery();
                outboxE10_MessageID = (int)cmdSaveOutbox.Parameters["@outboxID"].Value;
                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Er is iets fout gegaan met het bewaren van utilmdE10, we adviseren U contact op te nemen met IT");
                Console.WriteLine(ex.ToString());
            }

            //updating switchbericht met Outbox ID waar 392 of 432 instaat
            return outboxE10_MessageID;

        }
    }
}
