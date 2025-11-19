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
    public class UTILTSE30
    {
        public int aansluitingID;
        private StringBuilder UtiltsE30Bericht = new StringBuilder();
        private static String ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB");
        static string c_EOF = "'\r\n";
        private string HoofdPV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV").Trim();
        private string HoofdLV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV").Trim();

        public void createUtiltsE30(int? meterstandHoog, int? meterstandLaag, string leverancierEan13Code, string netbeheerderEan13Code, long ean18Code, DateTime? meterstandOpnameNLDT, int outbox392_432MessageID, int test)
        {
            //testsettings when test = 1;
            
            //variables 
            DateTime datum = DateTime.Now;
            Offset O = new Offset();
            int offset = O.getOffset();
            int countLines = 0;
            string messageID = GetMessageID.getMessageID();
            string controlref = messageID;  //datum.ToString("yyMMddHHmm");
            DateTime meetDatum = (DateTime)meterstandOpnameNLDT; //altijd gevuld als we hier komen

            //emptying the message
            UtiltsE30Bericht.Remove(0, UtiltsE30Bericht.Length);

            //create meterstandenbericht
            UtiltsE30Bericht.Append("UNA:+.? " + c_EOF);
            UtiltsE30Bericht.Append("UNB+UNOC:3+" + HoofdPV + ":14+");
            if (test == 1)
            {
                UtiltsE30Bericht.Append("8716867222234");
            }
            else
            {
                UtiltsE30Bericht.Append("8712423010208");
            }
            UtiltsE30Bericht.Append(":14+" + datum.ToString("yyMMdd:HHmm") + "+" + controlref);
            if (test == 1)
            {
                UtiltsE30Bericht.Append("++++++1");
            }
            UtiltsE30Bericht.Append(c_EOF);
            UtiltsE30Bericht.Append("UNH+" + messageID + "+UTILTS:D:01C:UN:E4NL38" + c_EOF); 
            UtiltsE30Bericht.Append("BGM+E30::260+" + messageID + "+9+AB" + c_EOF);
            UtiltsE30Bericht.Append("DTM+137:" + DateTime.Now.ToString("yyyyMMddHHmm") + ":203" + c_EOF);
            UtiltsE30Bericht.Append("DTM+735:?+0" + offset + "00:406" + c_EOF);
            UtiltsE30Bericht.Append("MKS+23+E01::260" + c_EOF);
            UtiltsE30Bericht.Append("NAD+MS+" + leverancierEan13Code + "::9" + c_EOF);
            UtiltsE30Bericht.Append("NAD+MR+" + netbeheerderEan13Code + "::9" + c_EOF);
            countLines = countLines + 7;
            
            //Kan hier normaalgesproken over loepen (de volgende 4 regels) Afspraak doen we niet 1 bericht = 1 ean
            UtiltsE30Bericht.Append("IDE+24+" + ean18Code.ToString() + "A" + c_EOF);
            UtiltsE30Bericht.Append("LOC+172+" + ean18Code.ToString() + "::9" + c_EOF);
            UtiltsE30Bericht.Append("PIA+5+871687000030:::9"+ c_EOF); //code voor electriciteit active 
            UtiltsE30Bericht.Append("STS+7++E03::260" + c_EOF);
            countLines = countLines + 4;

            //Console.WriteLine("test .. ID = " + meterstandHoog.ToString());            
            //Console.WriteLine("test .. ID = " + meterstandLaag.ToString());

            // CCI+++E24:EBO:260' met CAV+1:EBO:260'
            // Cav 1  +  2  = consumption 3  + 4 Production 
            if (meterstandHoog.HasValue)
            {
                UtiltsE30Bericht.Append("QTY+220:" + meterstandHoog.ToString() + ":KWH" + c_EOF);
                //tijd weggelaten we wisten niet of dit moest yes of no
                UtiltsE30Bericht.Append("DTM+368:" + meetDatum.ToString("yyyyMMdd") + "0000:203" + c_EOF); 
                UtiltsE30Bericht.Append("CCI+++E24:EBO:260" + c_EOF);
                UtiltsE30Bericht.Append("CAV+1:EBO:260" + c_EOF);
                UtiltsE30Bericht.Append("CCI+++E22::260" + c_EOF);
                UtiltsE30Bericht.Append("CAV+E26::260" + c_EOF);
                countLines = countLines + 6;
            }

            if (meterstandLaag.HasValue)
            {
                UtiltsE30Bericht.Append("QTY+220:" + meterstandLaag.ToString() + ":KWH" + c_EOF);
                //tijd weggelaten we wisten niet of dit moest yes of no
                UtiltsE30Bericht.Append("DTM+368:" + meetDatum.ToString("yyyyMMdd") + "0000:203" + c_EOF);
                UtiltsE30Bericht.Append("CCI+++E24:EBO:260" + c_EOF);
                UtiltsE30Bericht.Append("CAV+2:EBO:260" + c_EOF);
                UtiltsE30Bericht.Append("CCI+++E22::260" + c_EOF);
                UtiltsE30Bericht.Append("CAV+E26::260" + c_EOF);
                countLines = countLines + 6;
            }

            countLines = countLines + 1;
            UtiltsE30Bericht.Append("UNT+" + countLines.ToString() + "+" + messageID.ToString() + c_EOF);
            UtiltsE30Bericht.Append("UNZ+1+" + controlref + c_EOF);

            //for testing sent it to emailadres 
            //this.Sent(ean18Code.ToString(), controlref);

            //now put it to inbox
            this.Save_UtiltsE30(messageID, ean18Code.ToString(), outbox392_432MessageID, test);

        }
        private void Sent(string Ean18_Nummer, string controlref)
        {
            /*
            string messageBackupTo = "Marcel.Drost@Energie.nl";
            string subject = "Meterstanden Bericht EAN " + Ean18_Nummer + " datum " + controlref;
            string body = "Backup sent naar adres van Energie van het UTILTSE30 Bericht";
            MailMessage message = new MailMessage();
            message.Body = body;
            message.To.Add(messageBackupTo);
            message.Subject = subject;
            MailAddress messageFrom = new MailAddress("Marcel.Drost@Energie.nl");
            message.From = messageFrom;
            MemoryStream stream = new MemoryStream(UTF32Encoding.Default.GetBytes(UtiltsE30Bericht.ToString()));
            stream.Position = 0;
            DateTime fileNameDate;
            fileNameDate = DateTime.Now;
            string fileDate;
            fileDate = fileNameDate.ToString("yyyyMMddHHmm");
            Attachment messageAttachment2 = new Attachment(stream, "UtiltsE30Bericht" + fileDate + ".txt");
            message.Attachments.Add(messageAttachment2);
            SmtpClient client = new SmtpClient("mars");
            client.SendAsync(message, null);
            */
        }

        private void Save_UtiltsE30(string MessageID, string Ean18_Nummer, int outbox392_432MessageID, int test)
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;
            int outboxE30ID;

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
                    ",21  " +  //21 = UTILTSE30
                    ",@Bericht " +
                    ",@omschrijving " +
                    ",null  " +
                    ",@EmailAdres " +
                    ",'TE_VERSTUREN' "; 
                    if (test == 1)
                    {
                    SQLstatement = SQLstatement + ",'" + HoofdPV + "@cps.testfac.tennet',6); SELECT @outboxID = SCOPE_IDENTITY();";
                    }
                    else
                    {
                    SQLstatement = SQLstatement + ",'" + HoofdPV + "@cps.tennet.org',6); SELECT @outboxID = SCOPE_IDENTITY();";
                    }
            //Console.WriteLine(SQLstatement);

            SqlCommand cmdSaveOutbox = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveOutbox.Parameters.Add(new SqlParameter("@id", SqlDbType.VarChar, 14));
            cmdSaveOutbox.Parameters["@id"].Value = MessageID;
            cmdSaveOutbox.Parameters.Add(new SqlParameter("@Bericht", SqlDbType.NText));
            cmdSaveOutbox.Parameters["@Bericht"].Value = UtiltsE30Bericht.ToString();
            cmdSaveOutbox.Parameters.Add(new SqlParameter("@omschrijving", SqlDbType.NText));
            cmdSaveOutbox.Parameters["@omschrijving"].Value = "mtrstnd_E_" + Ean18_Nummer;
            cmdSaveOutbox.Parameters.Add(new SqlParameter("@EmailAdres", SqlDbType.NText));
            if (test == 1)
            {
                cmdSaveOutbox.Parameters["@EmailAdres"].Value = "8716867222234@cps.testfac.tennet";//eMailAdres;
            }
            else
            {
                cmdSaveOutbox.Parameters["@EmailAdres"].Value = "8712423010208@cps.tennet.org";//eMailAdres;
            }
            cmdSaveOutbox.Parameters.Add(new SqlParameter("@outboxID", SqlDbType.Int));
            cmdSaveOutbox.Parameters["@outboxID"].Direction = ParameterDirection.Output;       
            try
            {
                cmdSaveOutbox.ExecuteNonQuery();         
                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Er is iets fout gegaan met het bewaren van het Eprogramma, we adviseren U contact op te nemen met IT");
                Console.WriteLine(ex.ToString());
            }
            outboxE30ID = (int)cmdSaveOutbox.Parameters["@outboxID"].Value;

            //updating switchbericht met Outbox ID waar UTILTSE30 (gaat ervan uit dat er al een switchbericht is!
            SQLstatement =
                    "update [EnergieDB].[dbo].[Switchberichten] set Bericht_E30_Outbox_ID = @outboxE30ID " +
                    "where aansluiting_ID = @aansluitingID and Bericht_392_432_Outbox_ID = @outbox392_432MessageID";

            SqlCommand cmdSaveE30 = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveE30.Parameters.Add(new SqlParameter("@aansluitingID", SqlDbType.VarChar, 14));
            cmdSaveE30.Parameters["@aansluitingID"].Value = aansluitingID;
            cmdSaveE30.Parameters.Add(new SqlParameter("@outboxE30ID", SqlDbType.VarChar, 14));
            cmdSaveE30.Parameters["@outboxE30ID"].Value = outboxE30ID;
            cmdSaveE30.Parameters.Add(new SqlParameter("@outbox392_432MessageID", SqlDbType.Int));
            cmdSaveE30.Parameters["@outbox392_432MessageID"].Value = outbox392_432MessageID;
            cmdSaveE30.ExecuteNonQuery();
            cnPubs.Close();
        }


    }
}
