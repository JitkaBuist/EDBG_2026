using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Xml.Serialization;
using System.Xml;
using nl.Energie.EDSN;
using Energie.DataTableHelper;
using System.Data.SqlClient;
using System.Windows.Forms;
using Energie.Car;

namespace Energie.SwitchBericht
{
    public class GBU
    {
        //private string SQLstatement;
        //private SqlConnection conn;
        private static String ConnString = "";
        public string strSender = "";
        public string strPVSender = "";
        private string path = "";
        private string certPV = "";
        private string certPVPassword = "";
        private string certLV = "";
        private string certLVPassword = "";
        //private string urlWebService = "";
        //static string c_EOF = "'\r\n";
        public string strReceiver = "8712423010208";
        
        public GBU(string Klant_Config)
        {
            ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB_" + Klant_Config);
            strSender = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV_" + Klant_Config).Trim();
            strPVSender = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV_" + Klant_Config).Trim();
            path = Energie.DataAccess.Configurations.GetApplicationSetting("XMLPATH_" + Klant_Config);//@"c:\test\";
            certPV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPV_" + Klant_Config);
            certPVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPVPASSWORD_" + Klant_Config);
            certLV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLV_" + Klant_Config);
            certLVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLVPASSWORD_" + Klant_Config);

            KC.KlantConfig = Klant_Config;
            KC.ConnStringPortaal = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");
            KC.CarUrl = Energie.DataAccess.Configurations.GetApplicationSetting("PORTAAL");//"https://emp.edsn.nl/b2b";
        }
        public string[] GetFileList()
        {

            string[] arrFileList;

            arrFileList = new string[10];
            nl.Energie.EDSN.FileList.FileListQueryEnvelope envelope = new nl.Energie.EDSN.FileList.FileListQueryEnvelope();

            nl.Energie.EDSN.FileList.FileListQueryEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.FileList.FileListQueryEnvelope_EDSNBusinessDocumentHeader();

            //header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            envelope.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.FileList.FileListQueryEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.FileList.FileListQueryEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.FileList.FileListQueryEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.FileList.FileListQueryEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = strReceiver;
            destination.Receiver = receiver;

            nl.Energie.EDSN.FileList.FileListQueryEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.FileList.FileListQueryEnvelope_EDSNBusinessDocumentHeader_Source();

            string sender = "";
            string identifier = "";
            sender = strSender;
            identifier = "DDQ_O";
            
            source.SenderID = sender;
            source.ContactTypeIdentifier = identifier;
            header.Source = source;

            nl.Energie.EDSN.FileList.FileListQueryEnvelope_FileListQueryContent content = new nl.Energie.EDSN.FileList.FileListQueryEnvelope_FileListQueryContent();
            envelope.FileListQueryContent = content;

            nl.Energie.EDSN.FileList.FileListQueryEnvelope_FileListQueryContent_FileList_EDSNFileDetails EDSNFileDetails = new nl.Energie.EDSN.FileList.FileListQueryEnvelope_FileListQueryContent_FileList_EDSNFileDetails();
            content.FileList_EDSNFileDetails = EDSNFileDetails;

            EDSNFileDetails.Group = "ADC";
            
            EDSNFileDetails.FileType = "ADC Verplichting";

            nl.Energie.EDSN.FileList.FileListQueryEnvelope_FileListQueryContent_FileList_EDSNFileDetails_StartDate_EDSNFileDetails startDate = new nl.Energie.EDSN.FileList.FileListQueryEnvelope_FileListQueryContent_FileList_EDSNFileDetails_StartDate_EDSNFileDetails();
            EDSNFileDetails.StartDate_EDSNFileDetails = startDate;
            startDate.DateTime = DateTime.Now.AddDays(-2);

            nl.Energie.EDSN.FileList.FileListQueryEnvelope_FileListQueryContent_FileList_EDSNFileDetails_EndDate_EDSNFileDetails endDate = new nl.Energie.EDSN.FileList.FileListQueryEnvelope_FileListQueryContent_FileList_EDSNFileDetails_EndDate_EDSNFileDetails();
            EDSNFileDetails.EndDate_EDSNFileDetails = endDate;
            endDate.DateTime = DateTime.Now;

            EDSNFileDetails.Direction = "I";

            nl.Energie.EDSN.FileList.FileListService fileList = new nl.Energie.EDSN.FileList.FileListService();
            
            fileList.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));


            
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //moveIn.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderMoveInRespondingActivity";
            fileList.Url = KC.CarUrl + @"bulk/generic/FileListCAR";

            fileList.Timeout = 120000;

            nl.Energie.EDSN.FileList.FileListResponseEnvelope retour = new nl.Energie.EDSN.FileList.FileListResponseEnvelope();

            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.FileList.FileListQueryEnvelope));
            StringWriter swXML = new StringWriter();
            serializer.Serialize(swXML, envelope);
            TextWriter WriteFileStream = new StreamWriter(path + @"FileList.xml");
            serializer.Serialize(WriteFileStream, envelope);
            WriteFileStream.Close();

            try
            {
                retour = fileList.FileListQuery(envelope);

                if (retour.FileListResponseContent != null)
                {
                    arrFileList[0] = retour.FileListResponseContent.FileListResponse_EDSNFileDetails.Length.ToString();
                }
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                //WriteLog("Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                //    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID);
                MessageBox.Show(S.ErrorCode.ToString());
                MessageBox.Show(S.ErrorDetails);
                MessageBox.Show(S.ErrorText);
                MessageBox.Show(ex.Detail.InnerXml.ToString());
            }
            catch (WebException exception)
            {
                //WriteLog("Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
                MessageBox.Show(exception.Message);
            }
            catch (Exception exception)
            {
                //WriteLog("Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
                MessageBox.Show(exception.Message);
            }
            return arrFileList;
        }

    }
}
