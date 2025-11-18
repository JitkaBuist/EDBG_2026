using System;
using System.Collections.Generic;
using System.Text;
using Energie.DataTableHelper;
using System.Data.SqlClient;
using System.Data;
//mail libraries can be disabled when testing is done
using System.Net.Mail;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Forms;
using nl.Energie.EDSN;

namespace Energie.SwitchBericht
{
    public class P4Bericht
    {
        private string SQLstatement;
        private static String ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB");
        private string urlWebService = Energie.DataAccess.Configurations.GetApplicationSetting("PORTAAL");//"https://emp.edsn.nl/b2b";
        //private string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
        private string path = Energie.DataAccess.Configurations.GetApplicationSetting("XMLPATH");//@"c:\test\";
        private string certPV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPV");
        private string certPVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPVPASSWORD");
        private string certLV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLV");
        private string certLVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLVPASSWORD");
        private string HoofdPV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV").Trim();
        private string HoofdLV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV").Trim();

        public void P4CollectedDataBatch(string strFileName)
        {

            string text = File.ReadAllText(strFileName);
            //if (text.IndexOf("<EDSNDocument>") != -1)
            //{
            //    text = text.Replace("<EDSNDocument>", @"<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "utf-8" + '"' + "?>");
            //}
            //else
            //{
            //    text = @"<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "utf-8" + '"' + "?>" + text;
            //}
            text = text.Replace("<EDSNDocument>", "");
            text = text.Replace("</EDSNDocument>", "");
            //text = text.Replace("xmlns=" + '"' + "urn:nedu:edsn:data:p4collecteddatabatchresultresponseenvelope:1:draft" + '"', "xmlns:xsi=" + '"' + "http://www.w3.org/2001/XMLSchema-instance" + '"' + " xmlns:xsd=" + '"' + "http://www.w3.org/2001/XMLSchema" + '"');
            //text = text.Replace("<EDSNBusinessDocumentHeader>", "<EDSNBusinessDocumentHeader xmlns=" + '"' + "urn:nedu:edsn:data:p4collecteddatabatchresultresponseenvelope:1:'draft" + '"' + ">");
            //text = text.Replace("<Portaal_Content>", "<Portaal_Content xmlns=" + '"' + "urn:edsn:edsn:data:p4collecteddatabatchresultresponseenvelope:1:draft" + '"' + ">");
            //strFileName = strFileName.Trim() + ".tmp";
            //File.WriteAllText(strFileName, text);


            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.P4.P4CollectedDataBatchResultResponseEnvelope));
            //TextWriter WriteFileStream;
            nl.Energie.EDSN.P4.P4CollectedDataBatchResultResponseEnvelope retour = new nl.Energie.EDSN.P4.P4CollectedDataBatchResultResponseEnvelope();

            try
            {
                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.P4.P4CollectedDataBatchResultResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();


                _Doc.Load(strFileName);
                retour = (nl.Energie.EDSN.P4.P4CollectedDataBatchResultResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));



                int inboxID = 0;

                for (int i = 0; i < retour.P4Content.Length; i++)
                {

                    nl.Energie.EDSN.P4.P4CollectedDataBatchResultResponseEnvelope_P4Content_P4MeteringPoint P4Content = (nl.Energie.EDSN.P4.P4CollectedDataBatchResultResponseEnvelope_P4Content_P4MeteringPoint)retour.P4Content[i];
                    nl.Energie.EDSN.P4.P4CollectedDataBatchResultResponseEnvelope_EDSNBusinessDocumentHeader_Destination destination = (nl.Energie.EDSN.P4.P4CollectedDataBatchResultResponseEnvelope_EDSNBusinessDocumentHeader_Destination)retour.EDSNBusinessDocumentHeader.Destination;
                    nl.Energie.EDSN.P4.P4CollectedDataBatchResultResponseEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = (nl.Energie.EDSN.P4.P4CollectedDataBatchResultResponseEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver)destination.Item;
                    inboxID = Save_Inbox(27, _Doc.InnerXml.ToString(), "Verbruik : " + retour.EDSNBusinessDocumentHeader.MessageID.ToString());
                    int intedinID = Save_Edine(inboxID, retour.EDSNBusinessDocumentHeader.Source.SenderID, receiver.ReceiverID,
                        retour.EDSNBusinessDocumentHeader.CreationTimestamp, retour.EDSNBusinessDocumentHeader.MessageID, "UTILTS", "E11");



                }
                if (inboxID > 0) { ProcessMessage.processMessage(inboxID); }
                //Nog check op rejection


            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                MessageBox.Show(S.ErrorCode.ToString());
                MessageBox.Show(S.ErrorDetails);
                MessageBox.Show(S.ErrorText);
                MessageBox.Show(ex.Detail.InnerXml.ToString());
            }
            catch (WebException exception)
            {
                MessageBox.Show(exception.Message);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            File.Delete(strFileName);
        }

        public int Save_Inbox(int edineMessagetype_ID, string message, string subject)
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;
            int inboxID = -1;

            cnPubs.Open();
            SQLstatement =
                    "INSERT INTO [Messages].[dbo].[Inbox] " +
                    "([Ontvangen] " +
                    ",[EdineMessagetype_ID] " +
                    ",[Edine_Message] " +
                    ",[Processed] " +
                    ",[UID] " +
                    ",[errors] " +
                    ",[Subject] " +
                    ",[ToEnergieDB]) " +
                    "VALUES " +
                    "(GetDate() " +
                    ",@EdineMessagetype_ID " +
                    ",@Edine_Message " +
                    ",1 " +
                    ",convert(varchar, getdate(), 126) " +
                    ",0 " +
                    ",@Subject " +
                    ",1); SELECT @inboxID = SCOPE_IDENTITY();";


            SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveInbox.Parameters.Add(new SqlParameter("@EdineMessagetype_ID", SqlDbType.Int));
            cmdSaveInbox.Parameters["@EdineMessagetype_ID"].Value = edineMessagetype_ID;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Edine_Message", SqlDbType.NText));
            cmdSaveInbox.Parameters["@Edine_Message"].Value = message;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Subject", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@Subject"].Value = subject;

            cmdSaveInbox.Parameters.Add(new SqlParameter("@inboxID", SqlDbType.Int));
            cmdSaveInbox.Parameters["@inboxID"].Direction = ParameterDirection.Output;
            try
            {
                cmdSaveInbox.ExecuteNonQuery();
                inboxID = (int)cmdSaveInbox.Parameters["@inboxID"].Value;
                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Er is iets fout gegaan met het bewaren van het verbruik Electra (inbox), we adviseren U contact op te nemen met IT");
                MessageBox.Show(ex.ToString());
            }

            return inboxID;

        }
        public int Save_Edine(int inboxID, string unb_Sender_ID, string unb_Recipient_ID, DateTime unb_Date, string messageNumber, string unh_Message_ID_Type,
            string bgm_Document_Message_Name)
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;
            int edineID = -1;

            cnPubs.Open();
            SQLstatement =
                    "INSERT INTO [Messages].[dbo].[Edine] " +
                    "([Inbox_ID] " +
                    ",[Processed] " +
                    ",[UNB_Syntax_ID] " +
                    ",[UNB_Syntax_version] " +
                    ",[UNB_Sender_ID] " +
                    ",[UNB_Sender_Partner_ID] " +
                    ",[UNB_Sender_Address] " +
                    ",[UNB_Recipient_ID] " +
                    ",[UNB_Recipient_Partner] " +
                    ",[UNB_Recipient_Address] " +
                    ",[UNB_Date] " +
                    ",[UNB_Control_Reference] " +
                    ",[UNH_Message_Reference] " +
                    ",[UNH_Message_ID_Type] " +
                    ",[UNH_Message_ID_Type_Version] " +
                    ",[UNH_Message_ID_Type_Release] " +
                    ",[UNH_Message_ID_Agency] " +
                    ",[UNH_Message_ID_Assigned] " +
                    ",[BGM_Document_Message_Name] " +
                    ",[BGM_Document_Message_Number] " +
                    ",[BGM_Message_Function] " +
                    ",[BGM_Response_Type] " +
                    ",[UNT_Segment] " +
                    ",[UNT_Reference] " +
                    ",[UNZ_Count] " +
                    ",[UNZ_reference]) " +
                    "VALUES " +
                    "(@Inbox_ID " +
                    ",1 " +
                    ",'UNOC' " +
                    ",'3' " +
                    ",@UNB_Sender_ID " +
                    ",'14' " +
                    ",'' " +
                    ",@UNB_Recipient_ID " +
                    ",'14' " +
                    ",'' " +
                    ",@UNB_Date " +
                    ",@MessageNumber " +
                    ",@MessageNumber " +
                    ",@UNH_Message_ID_Type " +
                    ",'D' " +
                    ",'01C' " +
                    ",'UN' " +
                    ",'E4NL21' " +
                    ",@BGM_Document_Message_Name " +
                    ",@MessageNumber " +
                    ",'9' " +
                    ",'AB' " +
                    ",'1' " +
                    ",@MessageNumber " +
                    ",'1' " +
                    ",@MessageNumber); SELECT @edineID = SCOPE_IDENTITY();";


            SqlCommand cmdSaveEdine = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveEdine.Parameters.Add(new SqlParameter("@Inbox_ID", SqlDbType.Int));
            cmdSaveEdine.Parameters["@Inbox_ID"].Value = inboxID;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@UNB_Sender_ID", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@UNB_Sender_ID"].Value = unb_Sender_ID;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@UNB_Recipient_ID", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@UNB_Recipient_ID"].Value = unb_Recipient_ID;
            //cmdSaveEdine.Parameters.Add(new SqlParameter("@UNB_Recipient_ID", SqlDbType.VarChar));
            //cmdSaveEdine.Parameters["@UNB_Recipient_ID"].Value = unb_Recipient_ID;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@UNB_Date", SqlDbType.DateTime));
            cmdSaveEdine.Parameters["@UNB_Date"].Value = unb_Date;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@MessageNumber", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@MessageNumber"].Value = messageNumber;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@UNH_Message_ID_Type", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@UNH_Message_ID_Type"].Value = unh_Message_ID_Type;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@BGM_Document_Message_Name", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@BGM_Document_Message_Name"].Value = bgm_Document_Message_Name;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@edineID", SqlDbType.Int));
            cmdSaveEdine.Parameters["@edineID"].Direction = ParameterDirection.Output;
            try
            {
                cmdSaveEdine.ExecuteNonQuery();
                edineID = (int)cmdSaveEdine.Parameters["@edineID"].Value;
                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Er is iets fout gegaan met het bewaren van het Verbruiken Electra, we adviseren U contact op te nemen met IT");
                MessageBox.Show(ex.ToString());
            }

            return edineID;

        }

        public int Save_UTILTS_E11_Header(int Edine_Id, string sender, string receiver, DateTime messageProcessed, DateTime processingStart, DateTime processingEnd, string fase)
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;
            int inboxID = -1;

            cnPubs.Open();
            SQLstatement =
                    "INSERT INTO [Messages].[dbo].[UTILTS_E11_Header] " +
                    "([EdineId] " +
                    ",[MS] " +
                    ",[MR] " +
                    ",[MessageProcessed] ";
            if (processingStart != DateTime.MinValue) { SQLstatement = SQLstatement + ",[ProcessingStart] "; }
            if (processingEnd != DateTime.MinValue) { SQLstatement = SQLstatement + ",[ProcessingEnd] "; }
            SQLstatement = SQLstatement + ",[Fase]) " +
                    "VALUES " +
                    "(@EdineId " +
                    ",@MS " +
                    ",@MR " +
                    ",@MessageProcessed ";
            if (processingStart != DateTime.MinValue) { SQLstatement = SQLstatement + ",@ProcessingStart "; }
            if (processingEnd != DateTime.MinValue) { SQLstatement = SQLstatement + ",@ProcessingEnd "; }
            SQLstatement = SQLstatement + ",@Fase)";


            SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveInbox.Parameters.Add(new SqlParameter("@EdineId", SqlDbType.Int));
            cmdSaveInbox.Parameters["@EdineId"].Value = Edine_Id;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@MS", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@MS"].Value = sender;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@MR", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@MR"].Value = receiver;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@MessageProcessed", SqlDbType.DateTime));
            cmdSaveInbox.Parameters["@MessageProcessed"].Value = messageProcessed;
            if (processingStart != DateTime.MinValue)
            {
                cmdSaveInbox.Parameters.Add(new SqlParameter("@ProcessingStart", SqlDbType.DateTime));
                cmdSaveInbox.Parameters["@ProcessingStart"].Value = processingStart;
            }
            if (processingEnd != DateTime.MinValue)
            {
                cmdSaveInbox.Parameters.Add(new SqlParameter("@ProcessingEnd", SqlDbType.DateTime));
                cmdSaveInbox.Parameters["@ProcessingEnd"].Value = processingEnd;
            }
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Fase", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@Fase"].Value = fase;
            try
            {
                cmdSaveInbox.ExecuteNonQuery();
                //inboxID = (int)cmdSaveInbox.Parameters["@inboxID"].Value;
                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Er is iets fout gegaan met het bewaren van het verbruik Electra (inbox), we adviseren U contact op te nemen met IT");
                MessageBox.Show(ex.ToString());
            }

            return inboxID;

        }

        public int Save_UTILTS_E11(int Edine_Id, string transactionId, string transaction_Status, string loc_GC, string readingType, DateTime dtmUTCReading, string typeofMeteringPoint,
            string reading_TimeFrame, string originCode, decimal volume, string volumeType, string volumeStatus, string productID, string measureUnit)
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;
            int inboxID = -1;

            cnPubs.Open();
            SQLstatement =
                    "INSERT INTO [Messages].[dbo].[UTILTS_E11] " +
                    "([EdineId] " +
                    ",[TransactionId] " +
                    ",[Transaction_Status] " +
                    ",[Transaction_StatusReason] " +
                    ",[LOC_GC] " +
                    ",[ReadingType] " +
                    ",[dtmUTCReading] " +
                    ",[TypeofMeteringPoint] " +
                    ",[Reading_TimeFrame] " +
                    ",[OriginCode] " +
                    ",[Volume] " +
                    ",[VolumeType] ";
            if (volumeStatus != null) { SQLstatement = SQLstatement + ",[VolumeStatus] "; }
            SQLstatement = SQLstatement + ",[Seq] " +
                    ",[ProductID] " +
                    ",[MeasureUnit]) " +
                    "VALUES " +
                    "(@EdineId " +
                    ",@TransactionId " +
                    ",@Transaction_Status " +
                    ",'7' " +
                    ",@LOC_GC " +
                    ",@ReadingType " +
                    ",@dtmUTCReading " +
                    ",@TypeofMeteringPoint " +
                    ",@Reading_TimeFrame " +
                    ",@OriginCode " +
                    ",@Volume " +
                    ",@VolumeType ";
            if (volumeStatus != null) { SQLstatement = SQLstatement + ",@VolumeStatus "; }
            SQLstatement = SQLstatement + ",@Seq " +
                    ",@ProductID " +
                    ",@MeasureUnit)";


            SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveInbox.Parameters.Add(new SqlParameter("@EdineId", SqlDbType.Int));
            cmdSaveInbox.Parameters["@EdineId"].Value = Edine_Id;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@TransactionId", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@TransactionId"].Value = transactionId;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Transaction_Status", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@Transaction_Status"].Value = transaction_Status;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@LOC_GC", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@LOC_GC"].Value = loc_GC;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@ReadingType", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@ReadingType"].Value = readingType;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@dtmUTCReading", SqlDbType.DateTime));
            cmdSaveInbox.Parameters["@dtmUTCReading"].Value = dtmUTCReading;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@TypeofMeteringPoint", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@TypeofMeteringPoint"].Value = typeofMeteringPoint;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Reading_TimeFrame", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@Reading_TimeFrame"].Value = reading_TimeFrame;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@OriginCode", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@OriginCode"].Value = originCode;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Volume", SqlDbType.Decimal));
            cmdSaveInbox.Parameters["@Volume"].Value = volume;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@VolumeType", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@VolumeType"].Value = volumeType;
            if (volumeStatus != null)
            {
                cmdSaveInbox.Parameters.Add(new SqlParameter("@VolumeStatus", SqlDbType.VarChar));
                cmdSaveInbox.Parameters["@VolumeStatus"].Value = volumeStatus;
            }
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Seq", SqlDbType.TinyInt));
            cmdSaveInbox.Parameters["@Seq"].Value = 1;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@ProductID"].Value = productID;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@MeasureUnit", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@MeasureUnit"].Value = measureUnit;
            try
            {
                cmdSaveInbox.ExecuteNonQuery();
                //inboxID = (int)cmdSaveInbox.Parameters["@inboxID"].Value;
                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Er is iets fout gegaan met het bewaren van het verbruik Electra (inbox), we adviseren U contact op te nemen met IT");
                MessageBox.Show(ex.ToString());
            }

            return inboxID;

        }



    }
}
