using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energie.DataTableHelper;
using System.Data.SqlClient;
using System.Data;
using System.Net.Mail;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Xml.Serialization;
using nl.Energie.EDSN;
using System.Xml;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Web.Services;
using System.Web;
using System.Globalization;

namespace Energie.Car
{
    //E 871690939500323513 
    //G 871687140023924231 

    public class P4Client
    {
        private string strSql;
        //private static String ConnString = "";
        //private string urlWebService = Energie.DataAccess.Configurations.GetApplicationSetting("PORTAAL");//"https://emp.edsn.nl/b2b";
        private CarShared carShared;
        private SqlConnection conn;
        public P4Client(string klantConfig)
        {
            KC.KlantConfig = klantConfig;
            KC.ConnStringPortaal = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");
            KC.App_ID = AppID.Car;

            carShared = new CarShared();
        }

        public Boolean StartSubscription(string strFileName, String Eancode)
        {
            Boolean blnData = false;
            conn = new SqlConnection(KC.ConnString);
            conn.Open();

            String strSQL = "SELECT * FROM Car.dbo.P4Subscription  where Send=0 and Subscribe=1 ";
            if (Eancode != "") { strSql += " and EANCode=@EANCode"; }
            SqlCommand cmd = new SqlCommand(strSQL, conn);
            if (Eancode != "") { cmd.Parameters.AddWithValue("@EANCode", Eancode); }
            DataTable dtP4Subscription = new DataTable();
            SqlDataAdapter daP4Subscription = new SqlDataAdapter(cmd);
            daP4Subscription.Fill(dtP4Subscription);
            SqlCommandBuilder cbP4Subscription = new SqlCommandBuilder(daP4Subscription);
            cbP4Subscription.GetUpdateCommand();

            foreach (DataRow drP4Subscription in dtP4Subscription.Rows)
            {
                nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope enveloppe = new nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope();

                nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader();


                //header.ContentHash = "";

                header.CreationTimestamp = DateTime.Now;
                header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
                //header.ExpiresAt = DateTime.Now.AddMinutes(200);
                //header.ExpiresAtSpecified = true;
                header.MessageID = System.Guid.NewGuid().ToString();
                enveloppe.EDSNBusinessDocumentHeader = header;

                nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
                header.Destination = destination;

                nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
                //receiver.Authority = "";
                receiver.ContactTypeIdentifier = "EDSN";
                receiver.ReceiverID = "8712423010208";
                destination.Receiver = receiver;

                nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader_Source();
                source.SenderID = KC.HoofdLV.ToString();
                source.ContactTypeIdentifier = "DDQ_O";
                header.Source = source;

                nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_Subscription subscription = new nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_Subscription();
                enveloppe.Subscription = subscription;

                nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_Subscription_MarketEvaluationPoint marketEvaluationPoint = new nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_Subscription_MarketEvaluationPoint();
                marketEvaluationPoint.mRID = drP4Subscription["EANCode"].ToString();
                subscription.MarketEvaluationPoint = marketEvaluationPoint;

                nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_Subscription_MarketParticipant marketParticipant = new nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_Subscription_MarketParticipant();
                marketParticipant.mRID = KC.HoofdLV.ToString();
                marketParticipant.MarketRole = new nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_Subscription_MarketParticipant_MarketRole();
                marketParticipant.MarketRole.name = "DDQ";
                subscription.MarketParticipant = marketParticipant;

                nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_Subscription_ReferenceInformation referenceInformation = new nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope_Subscription_ReferenceInformation();
                referenceInformation.mRID = header.DocumentID;
                subscription.ReferenceInformation = referenceInformation;

                nl.Energie.EDSN.P4Subscription.Subscriptions subscriptions = new nl.Energie.EDSN.P4Subscription.Subscriptions();

                if (strFileName == "")
                {
                    subscriptions.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
                }
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                
                subscriptions.Url = KC.B2BGateway + @"synchroon/Subscriptions";

                subscriptions.Timeout = 120000;

                nl.Energie.EDSN.P4Subscription.StartStopSubscriptionResponseEnvelope response = new nl.Energie.EDSN.P4Subscription.StartStopSubscriptionResponseEnvelope();



                XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope));
                TextWriter WriteFileStream;

                if (strFileName == "")
                {
                    string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                    { BestandsAanvulling = " LV " + BestandsAanvulling; }

                    WriteFileStream = new StreamWriter(KC.XMLPath + @"StartSubscriptionRequest" + BestandsAanvulling + ".xml"); 
                    serializer.Serialize(WriteFileStream, enveloppe);
                    WriteFileStream.Close();

                    if (KC.FTPServer != "")
                    {
                        string ftpResponse = "";
                        if (FTPClass.FtpSendFile(KC.FTPServer + @"StartSubscriptionRequest" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"StartSubscriptionRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                        {
                            //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                        }
                    }

                }
                try
                {
                    string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                    BestandsAanvulling = " LV " + BestandsAanvulling;
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.P4Subscription.StartStopSubscriptionResponseEnvelope));
                    XmlDocument _Doc = new XmlDocument();

                    if (strFileName == "")
                    {
                        response = subscriptions.StartSubscriptionRequest(enveloppe);
                        WriteFileStream = new StreamWriter(KC.XMLPath + @"StartStopSubscriptionResponse" + BestandsAanvulling + ".xml");
                        serializer.Serialize(WriteFileStream, response);
                        WriteFileStream.Close();


                    }
                    else
                    {
                        response = new nl.Energie.EDSN.P4Subscription.StartStopSubscriptionResponseEnvelope();

                        _Doc = new XmlDocument();
                        _Doc.Load(strFileName);
                        serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope));
                        response = (nl.Energie.EDSN.P4Subscription.StartStopSubscriptionResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
                    }

                    CarShared car = new CarShared();

                    drP4Subscription["send"] = true;
                    int intBericht_ID = car.Save_Bericht(5, @"MeterReadingExchangeResponse" + BestandsAanvulling + ".xml", "Verbruik : " + response.EDSNBusinessDocumentHeader.MessageID.ToString(), true, response.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false); ;
                    SaveStartStopSubcription(response, enveloppe.EDSNBusinessDocumentHeader.DocumentID.ToString(), (int)drP4Subscription["Id"], intBericht_ID);
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                    {

                    XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                    TextReader tr = new StringReader(ex.Detail.InnerXml);
                    SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);


                }
                catch //(WebException exception)
                {

                }
                //catch (Exception exception)
                //{

                //}

                daP4Subscription.Update(dtP4Subscription);

            }
            conn.Close();

            return blnData;
        }

        private void SaveStartStopSubcription(nl.Energie.EDSN.P4Subscription.StartStopSubscriptionResponseEnvelope response, String reference, int Id, int BerichtId)
        {
            strSql = "UPDATE Car.dbo.P4Subscription SET Send = @Send, Reference = @Reference, Reaction = @Reaction, BerichtId=@BerichtId WHERE id=@Id";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@Id", Id);
            cmd.Parameters.AddWithValue("@Send", true);
            cmd.Parameters.AddWithValue("@Reference", reference);
            cmd.Parameters.AddWithValue("@Reaction", response.Subscription.SubscriptionStatus.Reason);
            cmd.Parameters.AddWithValue("@BerichtId", BerichtId);
            cmd.ExecuteNonQuery();
        }

        public Boolean StopSubscription(string strFileName, String Eancode)
        {
            Boolean blnData = false;
            conn = new SqlConnection(KC.ConnString);
            conn.Open();

            String strSQL = "SELECT * FROM Car.dbo.P4_Subscription  where Send=0 and Subscribe=0 ";
            if (Eancode != "") { strSql += " and EANCode=@EANCode"; }
            SqlCommand cmd = new SqlCommand(strSQL, conn);
            if (Eancode != "") { cmd.Parameters.AddWithValue("@EANCode", Eancode); }
            DataTable dtP4Subscription = new DataTable();
            SqlDataAdapter daP4Subscription = new SqlDataAdapter(cmd);
            daP4Subscription.Fill(dtP4Subscription);
            SqlCommandBuilder cbP4Subscription = new SqlCommandBuilder(daP4Subscription);
            cbP4Subscription.GetUpdateCommand();

            foreach (DataRow drP4Subscription in dtP4Subscription.Rows)
            {
                nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope enveloppe = new nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope();

                nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader();


                //header.ContentHash = "";

                header.CreationTimestamp = DateTime.Now;
                header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
                header.ExpiresAt = DateTime.Now.AddMinutes(200);
                header.ExpiresAtSpecified = true;
                header.MessageID = System.Guid.NewGuid().ToString();
                enveloppe.EDSNBusinessDocumentHeader = header;

                nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
                header.Destination = destination;

                nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
                //receiver.Authority = "";
                receiver.ContactTypeIdentifier = "EDSN";
                receiver.ReceiverID = "8712423010208";
                destination.Receiver = receiver;

                nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_EDSNBusinessDocumentHeader_Source();
                source.SenderID = KC.HoofdLV.ToString();
                header.Source = source;

                nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_Subscription subscription = new nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_Subscription();
                enveloppe.Subscription = subscription;

                nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_Subscription_MarketEvaluationPoint marketEvaluationPoint = new nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_Subscription_MarketEvaluationPoint();
                marketEvaluationPoint.mRID = drP4Subscription["EAN18_Code"].ToString();
                subscription.MarketEvaluationPoint = marketEvaluationPoint;

                nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_Subscription_MarketParticipant marketParticipant = new nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_Subscription_MarketParticipant();
                marketParticipant.mRID = KC.HoofdLV.ToString();
                marketParticipant.MarketRole = new nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_Subscription_MarketParticipant_MarketRole();
                marketParticipant.MarketRole.name = "DDQ";

                nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_Subscription_ReferenceInformation referenceInformation = new nl.Energie.EDSN.P4Subscription.StopSubscriptionRequestEnvelope_Subscription_ReferenceInformation();
                referenceInformation.mRID = drP4Subscription["Reference"].ToString();
                subscription.ReferenceInformation = referenceInformation;

                nl.Energie.EDSN.P4Subscription.Subscriptions subscriptions = new nl.Energie.EDSN.P4Subscription.Subscriptions();

                if (strFileName == "")
                {
                    subscriptions.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
                }
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                subscriptions.Url = KC.B2BGateway + @"synchroon/Subscriptions";

                subscriptions.Timeout = 120000;

                nl.Energie.EDSN.P4Subscription.StartStopSubscriptionResponseEnvelope response = new nl.Energie.EDSN.P4Subscription.StartStopSubscriptionResponseEnvelope();



                XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.P4Subscription.StartSubscriptionRequestEnvelope));
                TextWriter WriteFileStream;

                if (strFileName == "")
                {
                    string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                    { BestandsAanvulling = " LV " + BestandsAanvulling; }

                    WriteFileStream = new StreamWriter(KC.XMLPath + @"StartSubscriptionRequest" + BestandsAanvulling + ".xml"); ;
                    serializer.Serialize(WriteFileStream, enveloppe);
                    WriteFileStream.Close();

                    if (KC.FTPServer != "")
                    {
                        string ftpResponse = "";
                        if (FTPClass.FtpSendFile(KC.FTPServer + @"StartSubscriptionRequest" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"StartSubscriptionRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                        {
                            //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                        }
                    }

                }
                try
                {
                    string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                    BestandsAanvulling = " LV " + BestandsAanvulling;
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.P4Subscription.StartStopSubscriptionResponseEnvelope));
                    XmlDocument _Doc = new XmlDocument();

                    if (strFileName == "")
                    {
                        response = subscriptions.StopSubscriptionRequest(enveloppe);
                        WriteFileStream = new StreamWriter(KC.XMLPath + @"StartStopSubscriptionResponse" + BestandsAanvulling + ".xml");
                        serializer.Serialize(WriteFileStream, response);
                        WriteFileStream.Close();


                    }
                    else
                    {
                        response = new nl.Energie.EDSN.P4Subscription.StartStopSubscriptionResponseEnvelope();

                        _Doc = new XmlDocument();
                        _Doc.Load(strFileName);
                        serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope));
                        response = (nl.Energie.EDSN.P4Subscription.StartStopSubscriptionResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
                    }

                    CarShared car = new CarShared();

                    int intBericht_ID = car.Save_Bericht(5, @"MeterReadingExchangeResponse" + BestandsAanvulling + ".xml", "Verbruik : " + enveloppe.EDSNBusinessDocumentHeader.MessageID.ToString(), true, response.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false); ;
                    SaveStartStopSubcription(response, response.EDSNBusinessDocumentHeader.DocumentID.ToString(), (int)drP4Subscription["Id"], intBericht_ID);
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                {

                    XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                    TextReader tr = new StringReader(ex.Detail.InnerXml);
                    SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);


                }
                catch //(WebException exception)
                {

                }
                //catch (Exception exception)
                //{

                //}


            }
            conn.Close();

            return blnData;
        }

        public Boolean GetHistoricalSmartMeterReading(string strFileName, String Eancode, DateTime DateFrom, DateTime DateTo)
        {
            Boolean blnData = false;
            conn = new SqlConnection(KC.ConnString);
            conn.Open();

            //String strSQL = "SELECT * FROM Car.dbo.P4_Subscription  where Send=0 and Subscribe=0 ";
            //if (Eancode != "") { strSql += " and EANCode=@EANCode"; }
            //SqlCommand cmd = new SqlCommand(strSQL, conn);
            //if (Eancode != "") { cmd.Parameters.AddWithValue("@EANCode", Eancode); }
            //DataTable dtP4Historical = new DataTable();
            //SqlDataAdapter daP4Historical = new SqlDataAdapter(cmd);
            //daP4Historical.Fill(dtP4Historical);
            //SqlCommandBuilder cbP4Subscription = new SqlCommandBuilder(daP4Historical);
            //cbP4Subscription.GetUpdateCommand();


            nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope enveloppe = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope();

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_EDSNBusinessDocumentHeader();


            //header.ContentHash = "";

            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            //receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";// "8716859000017";// "8716871000002";
            destination.Receiver = receiver;

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            source.SenderID = KC.HoofdLV.ToString();
            source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_Portaal_Content portaal_Content = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_Content;

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_Portaal_Content_Portaal_MeteringPoint portaal_MeteringPoint = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_Portaal_Content_Portaal_MeteringPoint();
            portaal_MeteringPoint.EANID = Eancode.Trim();
            portaal_Content.Portaal_MeteringPoint = portaal_MeteringPoint;

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_BalanceSupplier_Company balanceSupplier_Company = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_BalanceSupplier_Company();
            balanceSupplier_Company.ID = KC.HoofdLV.ToString();
            portaal_MeteringPoint.BalanceSupplier_Company = balanceSupplier_Company;

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_Portaal_Content_Query referenceInformation = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope_Portaal_Content_Query();
            referenceInformation.ExternalReference = header.DocumentID;
            referenceInformation.DateFrom = DateFrom;
            referenceInformation.DateTo = DateTo;
            portaal_Content.Query = referenceInformation;


            nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeries meterReadingSeries = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeries();

            if (strFileName == "")
            {
                meterReadingSeries.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
            }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            meterReadingSeries.Url = KC.B2BGateway + @"synchroon/GetHistoricalSmartMeterReadingSeries";

            meterReadingSeries.Timeout = 120000;

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesResponseEnvelope response = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesResponseEnvelope();



            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesRequestEnvelope));
            TextWriter WriteFileStream;

            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                { BestandsAanvulling = " LV " + BestandsAanvulling; }

                WriteFileStream = new StreamWriter(KC.XMLPath + @"GetHistoricalSmartMeterReadingRequest" + BestandsAanvulling + ".xml"); ;
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();

                if (KC.FTPServer != "")
                {
                    string ftpResponse = "";
                    if (FTPClass.FtpSendFile(KC.FTPServer + @"GetHistoricalSmartMeterReadingRequest" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"GetHistoricalSmartMeterReadingRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    {
                        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                    }
                }

            }
            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                BestandsAanvulling = " LV " + BestandsAanvulling;
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();

                if (strFileName == "")
                {
                    response = meterReadingSeries.GetHistoricalSmartMeterReadingSeriesRequestEnvelope(enveloppe);
                    WriteFileStream = new StreamWriter(KC.XMLPath + @"GetHistoricalSmartMeterReadingResponse" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, response);
                    WriteFileStream.Close();


                }
                else
                {
                    response = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesResponseEnvelope();

                    _Doc = new XmlDocument();
                    _Doc.Load(strFileName);
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope));
                    response = (nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
                }

                CarShared car = new CarShared();

                int intBericht_ID = car.Save_Bericht(5, @"GetHistoricalSmartMeterReadingResponse" + BestandsAanvulling + ".xml", "Verbruik : " + enveloppe.EDSNBusinessDocumentHeader.DocumentID.ToString(), true, response.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false); ;
                WriteHistorical(response, header.MessageID);
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);


            }
            catch //(WebException exception)
            {

            }
            //catch (Exception exception)
            //{

            //}



            conn.Close();

            return blnData;
        }

        public void WriteHistorical(nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesResponseEnvelope response, string MessageId)
        {
            String strSql = "INSERT INTO Car.dbo.P4Historical \n";
            strSql += "(Eancode \n";
            strSql += ", Reference) \n";
            strSql += "VALUES \n";
            strSql += "(@Eancode \n";
            strSql += ", @Reference); SELECT @Id = SCOPE_IDENTITY(); ";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@Eancode", response.Portaal_Content.Portaal_MeteringPoint.EANID);
            cmd.Parameters.AddWithValue("@Reference", response.Portaal_Content.Query.ExternalReference);
            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int));
            cmd.Parameters["@Id"].Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();
            int P4HistoricalId = (int)cmd.Parameters["@Id"].Value;

            if (response.Portaal_Content.Portaal_MeteringPoint.Portaal_EnergyMeter != null)
            {
                foreach (nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesResponseEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_EnergyMeter meter in response.Portaal_Content.Portaal_MeteringPoint.Portaal_EnergyMeter)
                {
                    strSql = "INSERT INTO Car.dbo.P4HistoricalMeter \n";
                    strSql += "(P4HistoricalId \n";
                    strSql += ", Meternr) \n";
                    strSql += "VALUES \n";
                    strSql += "(@P4HistoricalId \n";
                    strSql += ", @Meternr); SELECT @Id = SCOPE_IDENTITY(); ";
                    cmd = new SqlCommand(strSql, conn);
                    cmd.Parameters.AddWithValue("@P4HistoricalId", P4HistoricalId);
                    cmd.Parameters.AddWithValue("@Meternr", meter.ID);
                    cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int));
                    cmd.Parameters["@Id"].Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    int P4HistoricalMeterId = (int)cmd.Parameters["@Id"].Value;

                    foreach (nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesResponseEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_EnergyMeter_Register register in meter.Register)
                    {
                        strSql = "INSERT INTO Car.dbo.P4HistoricalRegister \n";
                        strSql += "(Register \n";
                        strSql += ", Unit \n";
                        strSql += ", P4HistoricalMeterId) \n";
                        strSql += "VALUES \n";
                        strSql += "(@Register \n";
                        strSql += ", @Unit \n";
                        strSql += ", @P4HistoricalMeterId); SELECT @Id = SCOPE_IDENTITY(); ";
                        cmd = new SqlCommand(strSql, conn);
                        cmd.Parameters.AddWithValue("@Register", register.ID);
                        cmd.Parameters.AddWithValue("@Unit", register.MeasureUnit);
                        cmd.Parameters.AddWithValue("@P4HistoricalMeterId", P4HistoricalMeterId);
                        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int));
                        cmd.Parameters["@Id"].Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        int P4HistoricalRegisterId = (int)cmd.Parameters["@Id"].Value;
                        foreach (nl.Energie.EDSN.P4GetHistoricalSmartMeterReading.GetHistoricalSmartMeterReadingSeriesResponseEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_EnergyMeter_Register_Reading reading in register.Reading)
                        {
                            strSql = "INSERT INTO Car.dbo.P4HistoricalRegisterReading \n";
                            strSql += "(RegisterId \n";
                            strSql += ", ReadingDate \n";
                            strSql += ", Reading) \n";
                            strSql += "VALUES \n";
                            strSql += "(@RegisterId \n";
                            strSql += ", @ReadingDate \n";
                            strSql += ", @Reading)";
                            cmd = new SqlCommand(strSql, conn);
                            cmd.Parameters.AddWithValue("@RegisterId", P4HistoricalRegisterId);
                            cmd.Parameters.AddWithValue("@ReadingDate", reading.ReadingDateTime);
                            decimal decReading = decimal.Parse(reading.Reading);
                            cmd.Parameters.AddWithValue("@Reading", decReading);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public void GetHistoricalSmartMeterReadingDiferential(string strFileName)
        {
            //Boolean blnData = false;
            conn = new SqlConnection(KC.ConnString);
            conn.Open();

            //String strSQL = "SELECT * FROM Car.dbo.P4_Subscription  where Send=0 and Subscribe=0 ";
            //if (Eancode != "") { strSql += " and EANCode=@EANCode"; }
            //SqlCommand cmd = new SqlCommand(strSQL, conn);
            //if (Eancode != "") { cmd.Parameters.AddWithValue("@EANCode", Eancode); }
            //DataTable dtP4Historical = new DataTable();
            //SqlDataAdapter daP4Historical = new SqlDataAdapter(cmd);
            //daP4Historical.Fill(dtP4Historical);
            //SqlCommandBuilder cbP4Subscription = new SqlCommandBuilder(daP4Historical);
            //cbP4Subscription.GetUpdateCommand();


            nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialRequestEnvelope enveloppe = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialRequestEnvelope();

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialRequestEnvelope_EDSNBusinessDocumentHeader();


            //header.ContentHash = "";

            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            //receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";// "8716859000017";// "8716871000002";
            destination.Receiver = receiver;

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            source.SenderID = KC.HoofdLV.ToString();
            source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialRequestEnvelope_Portaal_Content portaal_Content = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_Content;

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialRequestEnvelope_Portaal_Content_BalanceSupplier_Company balanceSupplier_Company = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialRequestEnvelope_Portaal_Content_BalanceSupplier_Company();
            balanceSupplier_Company.ID = KC.HoofdLV.ToString();
            portaal_Content.BalanceSupplier_Company = balanceSupplier_Company;

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferential meterReadingSeries = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferential();

            if (strFileName == "")
            {
                meterReadingSeries.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
            }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            meterReadingSeries.Url = KC.B2BGateway + @"synchroon/GetHistoricalSmartMeterReadingSeriesDifferential";

            meterReadingSeries.Timeout = 120000;

            nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialResponseEnvelope response = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialResponseEnvelope();



            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialRequestEnvelope));
            TextWriter WriteFileStream;

            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                { BestandsAanvulling = " LV " + BestandsAanvulling; }

                WriteFileStream = new StreamWriter(KC.XMLPath + @"GetHistoricalSmartMeterReadingDifferentialRequest" + BestandsAanvulling + ".xml"); ;
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();

                if (KC.FTPServer != "")
                {
                    string ftpResponse = "";
                    if (FTPClass.FtpSendFile(KC.FTPServer + @"GetHistoricalSmartMeterReadingDifferentialRequest" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"GetHistoricalSmartMeterReadingDifferentialRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    {
                        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                    }
                }

            }
            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                BestandsAanvulling = " LV " + BestandsAanvulling;
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();

                if (strFileName == "")
                {
                    response = meterReadingSeries.GetHistoricalSmartMeterReadingSeriesDifferentialRequest(enveloppe);
                    WriteFileStream = new StreamWriter(KC.XMLPath + @"GetHistoricalSmartMeterReadingDifferentialResponse" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, response);
                    WriteFileStream.Close();


                }
                else
                {
                    response = new nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialResponseEnvelope();

                    _Doc = new XmlDocument();
                    _Doc.Load(strFileName);
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope));
                    response = (nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
                }

                CarShared car = new CarShared();

                int intBericht_ID = car.Save_Bericht(5, @"GetHistoricalSmartMeterReadingDifferentialResponse" + BestandsAanvulling + ".xml", "Verbruik : " + enveloppe.EDSNBusinessDocumentHeader.MessageID.ToString(), true, response.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false); ;
                WriteHistoricalDiferential(response, header.MessageID);
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);


            }
            catch //(WebException exception)
            {

            }
            //catch (Exception exception)
            //{

            //}



            conn.Close();
        }

        public void WriteHistoricalDiferential(nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialResponseEnvelope response, string MessageId)
        {
            if (response.Portaal_Content.Portaal_MeteringPoint != null)
            {
                foreach (nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialResponseEnvelope_Portaal_Content_Portaal_MeteringPoint meteringPoint in response.Portaal_Content.Portaal_MeteringPoint)
                {
                    String strSql = "INSERT INTO Car.dbo.P4Historical \n";
                    strSql += "(Eancode \n";
                    strSql += ", Reference) \n";
                    strSql += "VALUES \n";
                    strSql += "(@Eancode \n";
                    strSql += ", @Reference); SELECT @Id = SCOPE_IDENTITY(); ";
                    SqlCommand cmd = new SqlCommand(strSql, conn);
                    cmd.Parameters.AddWithValue("@Eancode", meteringPoint.EANID);
                    cmd.Parameters.AddWithValue("@Referentie", MessageId);
                    cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int));
                    cmd.Parameters["@Id"].Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    int P4HistoricalId = (int)cmd.Parameters["@Id"].Value;

                    foreach (nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialResponseEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_EnergyMeter meter in meteringPoint.Portaal_EnergyMeter)
                    {
                        strSql = "INSERT INTO Car.dbo.P4HistoricalMeter \n";
                        strSql += "(P4HistoricalId \n";
                        strSql += ", Meternr) \n";
                        strSql += "VALUES \n";
                        strSql += "(@P4HistoricalId \n";
                        strSql += ", @Meternr); SELECT @Id = SCOPE_IDENTITY(); ";
                        cmd = new SqlCommand(strSql, conn);
                        cmd.Parameters.AddWithValue("@P4HistoricalId", P4HistoricalId);
                        cmd.Parameters.AddWithValue("@Meternr", meter.ID);
                        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int));
                        cmd.Parameters["@Id"].Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        int P4HistoricalMeterId = (int)cmd.Parameters["@Id"].Value;

                        foreach (nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialResponseEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_EnergyMeter_Register register in meter.Register)
                        {
                            strSql = "INSERT INTO Car.dbo.P4HistoricalRegister \n";
                            strSql += "(Register \n";
                            strSql += ", Unit \n";
                            strSql += ", P4HistoricalMeterId) \n";
                            strSql += "VALUES \n";
                            strSql += "(@Register \n";
                            strSql += ", @Unit \n";
                            strSql += ", @P4HistoricalMeterId); SELECT @Id = SCOPE_IDENTITY(); ";
                            cmd = new SqlCommand(strSql, conn);
                            cmd.Parameters.AddWithValue("@Register", register.ID);
                            cmd.Parameters.AddWithValue("@Unit", register.MeasureUnit);
                            cmd.Parameters.AddWithValue("@P4HistoricalMeterId", P4HistoricalMeterId);
                            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int));
                            cmd.Parameters["@Id"].Direction = ParameterDirection.Output;
                            cmd.ExecuteNonQuery();
                            int P4HistoricalRegisterId = (int)cmd.Parameters["@Id"].Value;
                            foreach (nl.Energie.EDSN.P4GetHistoricalSmartMeterReadingDifferential.GetHistoricalSmartMeterReadingSeriesDifferentialResponseEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_EnergyMeter_Register_Reading reading in register.Reading)
                            {
                                strSql = "INSERT INTO Car.dbo.P4HistoricalRegisterReading \n";
                                strSql += "(RegisterId \n";
                                strSql += ", ReadingDate \n";
                                strSql += ", Reading) \n";
                                strSql += "VALUES \n";
                                strSql += "(@RegisterId \n";
                                strSql += ", @ReadingDate \n";
                                strSql += ", @Reading)";
                                cmd = new SqlCommand(strSql, conn);
                                cmd.Parameters.AddWithValue("@RegisterId", P4HistoricalRegisterId);
                                cmd.Parameters.AddWithValue("@ReadingDate", reading.ReadingDate);
                                decimal decReading = decimal.Parse(reading.Reading);
                                cmd.Parameters.AddWithValue("@Reading", decReading);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }
    }
}
