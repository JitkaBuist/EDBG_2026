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
using System.Windows.Forms;
using nl.Energie.EDSN;
using System.Xml;
using System.Net.Sockets;
using System.Threading;
using Energie.Car;

namespace Energie.SwitchBericht
{
    public class GetMPInformation
    {
        //private string SQLstatement;
        private static String ConnString = "";
        //private string urlWebService = Energie.DataAccess.Configurations.GetApplicationSetting("PORTAAL");//"https://emp.edsn.nl/b2b";
        //private string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase );
        private string path = "";//@"c:\test\";
        private string certPV = "";
        private string certPVPassword = "";
        private string certLV = "";
        private string certLVPassword = "";
        private string HoofdPV = "";
        private string HoofdLV = "";
        private string Klant_Config = "";

        private Int32 port = 13000;
        private IPAddress localAddr = IPAddress.Parse("82.139.104.75");
        //private int intService = -1;
        //private TcpListener listener;
        //private Boolean blnContinue = true;
        private System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
        private string strFTPServer = Energie.DataAccess.Configurations.GetApplicationSetting("FTPSERVER");
        private string strFTPUser = "";
        private string strFTPPassword = "";
        private string strTest = Energie.DataAccess.Configurations.GetApplicationSetting("TEST");

        public GetMPInformation(string klant_Config)
        {
            Klant_Config = klant_Config;

            if (Klant_Config != "")
            {
                HoofdPV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV_" + Klant_Config).Trim();
                HoofdLV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV_" + Klant_Config).Trim();
                ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB_" + Klant_Config);
                certPV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPV_" + Klant_Config);
                certPVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPVPASSWORD_" + Klant_Config);
                certLV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLV_" + Klant_Config);
                certLVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLVPASSWORD_" + Klant_Config);
                path = Energie.DataAccess.Configurations.GetApplicationSetting("XMLPath_" + Klant_Config);
                strFTPUser = Energie.DataAccess.Configurations.GetApplicationSetting("FTPUSER_" + Klant_Config).Trim();
                strFTPPassword = Energie.DataAccess.Configurations.GetApplicationSetting("FTPPASSWORD_" + Klant_Config).Trim();
            }
            else
            {
                ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB");
                path = Energie.DataAccess.Configurations.GetApplicationSetting("XMLPATH");//@"c:\test\";
                certPV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPV");
                certPVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPVPASSWORD");
                certLV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLV");
                certLVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLVPASSWORD");
                HoofdPV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV").Trim();
                HoofdLV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV").Trim();

            }
            KC.KlantConfig = Klant_Config;
            KC.ConnStringPortaal = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");
        }
        public string RequestMPInformation(string eancode, string strKey, string strBirthDay, string strIBAN, string strFileName, Boolean blnBatch, Boolean blnToFile)
        {
            string strResult = "FAILED";
            string OldFileName = strFileName;

            nl.Energie.EDSN.GetMPInformation.GetMPInformationRequestEnvelope enveloppe = new nl.Energie.EDSN.GetMPInformation.GetMPInformationRequestEnvelope();
            nl.Energie.EDSN.GetMPInformation.EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.GetMPInformation.EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.GetMPInformation.Destination destination = new nl.Energie.EDSN.GetMPInformation.Destination();
            header.Destination = destination;

            nl.Energie.EDSN.GetMPInformation.Receiver receiver = new nl.Energie.EDSN.GetMPInformation.Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.GetMPInformation.Source source = new nl.Energie.EDSN.GetMPInformation.Source();
            source.SenderID = HoofdLV;
            source.ContactTypeIdentifier = "DDQ_O";
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;

            nl.Energie.EDSN.GetMPInformation.PC portaal_content = new nl.Energie.EDSN.GetMPInformation.PC();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.GetMPInformation.PMP meteringpoint = new nl.Energie.EDSN.GetMPInformation.PMP();

            meteringpoint.EANID = eancode;
            portaal_content.Portaal_MeteringPoint = meteringpoint;

            nl.Energie.EDSN.GetMPInformation.MPCC cc = new nl.Energie.EDSN.GetMPInformation.MPCC();
            meteringpoint.MPCommercialCharacteristics = cc;

            cc.GridContractParty.BirthDayKey = strBirthDay;
            cc.GridContractParty.IBANKey = strIBAN;

            String strConsentID = "";
            if (strBirthDay != "")
            {
                strConsentID = strBirthDay;
            }
            else
            {
                strConsentID = strIBAN;
            }

            nl.Energie.EDSN.GetMPInformation.PM pm = new nl.Energie.EDSN.GetMPInformation.PM();
            meteringpoint.Portaal_Mutation = pm;
            pm.ConsentID = strConsentID;


            nl.Energie.EDSN.GetMPInformation.Portaal_UserInformation pui = new nl.Energie.EDSN.GetMPInformation.Portaal_UserInformation();
            pm.Portaal_UserInformation = pui;
            pui.Organisation = HoofdLV;

            nl.Energie.EDSN.GetMPInformation.GetMPInformation mpInformation = new nl.Energie.EDSN.GetMPInformation.GetMPInformation();
            nl.Energie.EDSN.GetMPInformation.GetMPInformationResponseEnvelope retour = new nl.Energie.EDSN.GetMPInformation.GetMPInformationResponseEnvelope();

            mpInformation.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            mpInformation.Url = KC.CarUrl + @"/synchroon/ResponderGetMPInformationRespondingActivity";

            mpInformation.Timeout = 120000;


            //Boolean success = true;
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope));
            TextWriter WriteFileStream;
            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                { BestandsAanvulling = " LV " + BestandsAanvulling; }

                WriteFileStream = new StreamWriter(path + @"mpInformationRequest" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();

                string ftpResponse = "";

                if (FTPClass.FtpSendFile(strFTPServer + @"mpInformationRequest" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"mpInformationRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                {
                    if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse); }
                    //success = false;
                }

            }
            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                BestandsAanvulling = " LV " + BestandsAanvulling; 
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();



                if (strFileName == "")
                {
                    if (blnToFile == true && Klant_Config != "")
                    {
                        string fileName = path + @"mpInformation.xml";

                        // string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                        XmlSerializer serializer1 = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope));
                        if (File.Exists(fileName)) { File.Delete(fileName); }
                        TextWriter WriteFileStream1 = new StreamWriter(fileName);
                        serializer1.Serialize(WriteFileStream1, enveloppe);
                        WriteFileStream1.Close();

                        IPAddress ipAddr = localAddr;
                        IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

                        // Create a TCP socket.
                        Socket client = new Socket(AddressFamily.InterNetwork,
                                SocketType.Stream, ProtocolType.Tcp);

                        // Connect the socket to the remote endpoint.
                        client.Connect(ipEndPoint);

                        if (File.Exists(path + "RemoteResult.xml") == true) { File.Delete(path + "RemoteResult.xml"); }

                        client.SendFile(fileName);


                        // Release the socket.
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();

                        SqlConnection conn = new SqlConnection(ConnString);
                        conn.Open();

                        int intCounter = 0;
                        int intRecords = 0;
                        //WriteLog("Lus", 10, 0);
                        while (intRecords == 0 && intCounter < 200)
                        {
                            string strSQL = "select count(*) from Messages.dbo.XMLMessage";
                            SqlCommand cmd = new SqlCommand(strSQL, conn);
                            intRecords = (int)cmd.ExecuteScalar();
                            Application.DoEvents();
                            Thread.Sleep(100);
                            intCounter++;
                        }

                        //WriteLog("EindeLus " + intRecords.ToString(), 10, 0);

                        if (intRecords > 0)
                        {
                            string strSQL = "select * from Messages.dbo.XMLMessage";
                            SqlCommand cmd = new SqlCommand(strSQL, conn);
                            string strXML = cmd.ExecuteScalar().ToString();
                            //WriteLog("Eindelees " + strXML.Length.ToString(), 10, 0);


                            serializer = new XmlSerializer(typeof(nl.Energie.EDSN.GetMPInformation.GetMPInformationResponseEnvelope));
                            _Doc = new XmlDocument();
                            File.WriteAllText(path + "RemoteResult.xml", strXML);

                            _Doc.LoadXml(strXML);
                            //_Doc.Load(path + "RemoteResult.xml");
                            //nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope result = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope();
                            //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
                            retour = (nl.Energie.EDSN.GetMPInformation.GetMPInformationResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
                        }
                        conn.Close();
                    }
                    else
                    {
                        retour = mpInformation.GetMPInformationRequest(enveloppe);
                    }

                    //Save to file kan weg
                    string responsefile = "mpInformationResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml";
                    WriteFileStream = new StreamWriter(path + responsefile);
                    //WriteFileStream = new StreamWriter(path + @"MasterDataResponse" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    string ftpResponse = "";

                    if (FTPClass.FtpSendFile(strFTPServer + responsefile, strFTPUser, strFTPPassword, path + responsefile, out ftpResponse) == false)
                    {
                        if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse); }
                        //success = false;
                    }


                    //Save to String
                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);

                    ftpResponse = "";
                    if (Klant_Config == "ROBIN" && strTest != "JA")
                    {
                        if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "mpInformationResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + responsefile, out ftpResponse) == false)
                        {
                            if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse); }
                            //success = false;
                        }
                    }

                    //BestandsAanvulling = " LV 030514 143925";niet meer nodig 8nov
                    //_Doc.Load(path + "MasterDataResult" + BestandsAanvulling + ".xml");
                }
                else
                {

                    retour = new nl.Energie.EDSN.GetMPInformation.GetMPInformationResponseEnvelope();

                    _Doc.Load(strFileName);
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope));
                    retour = (nl.Energie.EDSN.GetMPInformation.GetMPInformationResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
                }

                //Nog check op rejection

                //if (retour.Portaal_Content.Item.GetType() == typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection))
                //{
                //    nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection portaal_Rejection = new nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection();
                //    portaal_Rejection = (nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection)retour.Portaal_Content.Item;

                //    if (blnBatch == false) { MessageBox.Show("Fout : " + portaal_Rejection.Rejection[0].RejectionText); }
                //    success = false;
                //}
                //else
                //{
                //    SwitchBericht switchBericht = new SwitchBericht();
                //    int inboxID = switchBericht.Save_Inbox(26, _Doc.InnerXml.ToString(), "Masterdata : " + header.DocumentID);
                //    int edineID = switchBericht.Save_Edine(inboxID, receiver.ReceiverID, source.SenderID, DateTime.Now, header.DocumentID, "UTILMD", "E07");
                //    nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint portaal_MeteringPoint = new nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint();
                //    portaal_MeteringPoint = (nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)retour.Portaal_Content.Item;

                //    Masterdata md = new Masterdata(edineID, portaal_MeteringPoint, header.DocumentID, source.SenderID, receiver.ReceiverID, source.SenderID);

                //    if (blnBatch == false)
                //    {
                //        MessageBox.Show("EAN : " + md.LOC_GC + "\r\n" + "Naam : " + md.NameIT + "\r\n" + "Adres : " + md.Address + " " + md.HouseNr +
                //            "\r\nPlaats : " + md.PostalCode + " " + md.City + "\r\nSJV : " + md.EstimatedAnnualVolumeHigh + "\r\nSJV Laag : " + md.EstimatedAnnualVolumeLow +
                //            "\r\nProfiel : " + md.Profile + "\r\nCapaciteitscode : " + md.CapaciteitsTariefCode + "\r\nLeverancier : " + md.NAD_DP +
                //            "\r\nPV : " + md.BRP);
                //    }
                //    ProcessMessage.processMessage(inboxID);
                //}

                if (strFileName != "")
                {
                    File.Delete(strFileName);
                    strFileName = OldFileName;
                }
                //EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;
                //if (portaalResponse.Item.GetType() == typeof(EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection))
                //{
                //    EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
                //    EDSN_MoveIn.MoveInResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
                //    MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
                //}
                //else
                //{
                //    EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint reponseItem = (EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;

                //    MessageBox.Show("Accepted");
                //}
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                if (blnBatch == false)
                {
                    //MessageBox.Show(S.ErrorCode.ToString());
                    //MessageBox.Show(S.ErrorDetails);
                    //MessageBox.Show(S.ErrorText);
                    //MessageBox.Show(ex.Detail.InnerXml.ToString());
                }
                //success = false;
            }
            catch //(WebException exception)
            {
                if (blnBatch == false)
                {
                    //MessageBox.Show(exception.Message);
                }
                //success = false;
            }
            //catch (Exception exception)
            //{
            //    if (blnBatch == false)
            //    {
            //        //MessageBox.Show(exception.Message);
            //    }
            //    success = false;
            //}


            return strResult;
        }

        public string RequestGetMeteringPointMP(string eancode, Boolean blnPV, string strFileName, Boolean blnBatch, Boolean blnToFile)
        {
            string strResult = "FAILED";
            string OldFileName = strFileName;

            nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope enveloppe = new nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope();
            nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            source.SenderID = HoofdLV;
            source.ContactTypeIdentifier = "DDQ_O";
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;

            nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope_PC portaal_content = new nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope_PC();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope_PC_PMP meteringpoint = new nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope_PC_PMP();

            meteringpoint.EANID = eancode;
            portaal_content.Portaal_MeteringPoint = meteringpoint;

            

            nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointMP getMeteringPointMP = new nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointMP();
            nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope retour = new nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope();

            getMeteringPointMP.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            getMeteringPointMP.Url = KC.CarUrl + @"/synchroon/ResponderGetMeteringPointMPRespondingActivity";

            getMeteringPointMP.Timeout = 120000;


            //Boolean success = true;
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope));
            TextWriter WriteFileStream;
            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                { BestandsAanvulling = " LV " + BestandsAanvulling; }

                WriteFileStream = new StreamWriter(path + @"mpInformationRequest" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();

                string ftpResponse = "";

                if (FTPClass.FtpSendFile(strFTPServer + @"GetMeteringPointMPRequest" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"GetMeteringPointMPRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                {
                    if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse); }
                    //success = false;
                }

            }
            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                BestandsAanvulling = " LV " + BestandsAanvulling;
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();



                if (strFileName == "")
                {
                    if (blnToFile == true && Klant_Config != "")
                    {
                        string fileName = path + @"GetMeteringPointMP.xml";

                        // string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                        XmlSerializer serializer1 = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope));
                        if (File.Exists(fileName)) { File.Delete(fileName); }
                        TextWriter WriteFileStream1 = new StreamWriter(fileName);
                        serializer1.Serialize(WriteFileStream1, enveloppe);
                        WriteFileStream1.Close();

                        IPAddress ipAddr = localAddr;
                        IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

                        // Create a TCP socket.
                        Socket client = new Socket(AddressFamily.InterNetwork,
                                SocketType.Stream, ProtocolType.Tcp);

                        // Connect the socket to the remote endpoint.
                        client.Connect(ipEndPoint);

                        if (File.Exists(path + "RemoteResult.xml") == true) { File.Delete(path + "RemoteResult.xml"); }

                        client.SendFile(fileName);


                        // Release the socket.
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();

                        SqlConnection conn = new SqlConnection(ConnString);
                        conn.Open();

                        int intCounter = 0;
                        int intRecords = 0;
                        //WriteLog("Lus", 10, 0);
                        while (intRecords == 0 && intCounter < 200)
                        {
                            string strSQL = "select count(*) from Messages.dbo.XMLMessage";
                            SqlCommand cmd = new SqlCommand(strSQL, conn);
                            intRecords = (int)cmd.ExecuteScalar();
                            Application.DoEvents();
                            Thread.Sleep(100);
                            intCounter++;
                        }

                        //WriteLog("EindeLus " + intRecords.ToString(), 10, 0);

                        if (intRecords > 0)
                        {
                            string strSQL = "select * from Messages.dbo.XMLMessage";
                            SqlCommand cmd = new SqlCommand(strSQL, conn);
                            string strXML = cmd.ExecuteScalar().ToString();
                            //WriteLog("Eindelees " + strXML.Length.ToString(), 10, 0);


                            serializer = new XmlSerializer(typeof(nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope));
                            _Doc = new XmlDocument();
                            File.WriteAllText(path + "RemoteResult.xml", strXML);

                            _Doc.LoadXml(strXML);
                            //_Doc.Load(path + "RemoteResult.xml");
                            //nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope result = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope();
                            //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
                            retour = (nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
                        }
                        conn.Close();
                    }
                    else
                    {
                        retour = getMeteringPointMP.GetMeteringPoint(enveloppe);
                    }

                    //Save to file kan weg
                    string responsefile = "GetMeteringPointMPResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml";
                    WriteFileStream = new StreamWriter(path + responsefile);
                    //WriteFileStream = new StreamWriter(path + @"MasterDataResponse" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    string ftpResponse = "";

                    if (FTPClass.FtpSendFile(strFTPServer + responsefile, strFTPUser, strFTPPassword, path + responsefile, out ftpResponse) == false)
                    {
                        if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse); }
                        //success = false;
                    }


                    //Save to String
                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);

                    ftpResponse = "";
                    if (Klant_Config == "ROBIN" && strTest != "JA")
                    {
                        if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "GetMeteringPointMPResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + responsefile, out ftpResponse) == false)
                        {
                            if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse); }
                            //success = false;
                        }
                    }

                    //BestandsAanvulling = " LV 030514 143925";niet meer nodig 8nov
                    //_Doc.Load(path + "MasterDataResult" + BestandsAanvulling + ".xml");
                }
                else
                {

                    retour = new nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope();

                    _Doc.Load(strFileName);
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope));
                    retour = (nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
                }

                //Nog check op rejection

                //if (retour.Portaal_Content.Item.GetType() == typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection))
                //{
                //    nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection portaal_Rejection = new nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection();
                //    portaal_Rejection = (nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection)retour.Portaal_Content.Item;

                //    if (blnBatch == false) { MessageBox.Show("Fout : " + portaal_Rejection.Rejection[0].RejectionText); }
                //    success = false;
                //}
                //else
                //{
                //    SwitchBericht switchBericht = new SwitchBericht();
                //    int inboxID = switchBericht.Save_Inbox(26, _Doc.InnerXml.ToString(), "Masterdata : " + header.DocumentID);
                //    int edineID = switchBericht.Save_Edine(inboxID, receiver.ReceiverID, source.SenderID, DateTime.Now, header.DocumentID, "UTILMD", "E07");
                //    nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint portaal_MeteringPoint = new nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint();
                //    portaal_MeteringPoint = (nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)retour.Portaal_Content.Item;

                //    Masterdata md = new Masterdata(edineID, portaal_MeteringPoint, header.DocumentID, source.SenderID, receiver.ReceiverID, source.SenderID);

                //    if (blnBatch == false)
                //    {
                //        MessageBox.Show("EAN : " + md.LOC_GC + "\r\n" + "Naam : " + md.NameIT + "\r\n" + "Adres : " + md.Address + " " + md.HouseNr +
                //            "\r\nPlaats : " + md.PostalCode + " " + md.City + "\r\nSJV : " + md.EstimatedAnnualVolumeHigh + "\r\nSJV Laag : " + md.EstimatedAnnualVolumeLow +
                //            "\r\nProfiel : " + md.Profile + "\r\nCapaciteitscode : " + md.CapaciteitsTariefCode + "\r\nLeverancier : " + md.NAD_DP +
                //            "\r\nPV : " + md.BRP);
                //    }
                //    ProcessMessage.processMessage(inboxID);
                //}

                if (strFileName != "")
                {
                    File.Delete(strFileName);
                    strFileName = OldFileName;
                }
                //EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;
                //if (portaalResponse.Item.GetType() == typeof(EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection))
                //{
                //    EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
                //    EDSN_MoveIn.MoveInResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
                //    MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
                //}
                //else
                //{
                //    EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint reponseItem = (EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;

                //    MessageBox.Show("Accepted");
                //}
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                if (blnBatch == false)
                {
                    //MessageBox.Show(S.ErrorCode.ToString());
                    //MessageBox.Show(S.ErrorDetails);
                    //MessageBox.Show(S.ErrorText);
                    //MessageBox.Show(ex.Detail.InnerXml.ToString());
                }
            }
            catch //(WebException exception)
            {
                if (blnBatch == false)
                {
                    //MessageBox.Show(exception.Message);
                }
            }
            //catch (Exception exception)
            //{
            //    if (blnBatch == false)
            //    {
            //        //MessageBox.Show(exception.Message);
            //    }
            //    success = false;
            //}


            return strResult;
        }

        public string RequestGetSCMPInformation(string eancode, Boolean blnPV, string strFileName, Boolean blnBatch, Boolean blnToFile)
        {
            string strResult = "FAILED";
            string OldFileName = strFileName;

            nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope enveloppe = new nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope();
            nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            source.SenderID = HoofdLV;
            source.ContactTypeIdentifier = "DDQ_O";
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;

            nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope_PC portaal_content = new nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope_PC();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope_PC_PMP meteringpoint = new nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope_PC_PMP();

            meteringpoint.EANID = eancode;
            portaal_content.Portaal_MeteringPoint = meteringpoint;



            nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformation getMeteringPointMP = new nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformation();
            nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationResponseEnvelope retour = new nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationResponseEnvelope();

            getMeteringPointMP.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            getMeteringPointMP.Url = KC.CarUrl + @"/synchroon/ResponderGetSCMPInformationRespondingActivity";

            getMeteringPointMP.Timeout = 120000;


            //Boolean success = true;
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope));
            TextWriter WriteFileStream;
            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                { BestandsAanvulling = " LV " + BestandsAanvulling; }

                WriteFileStream = new StreamWriter(path + @"GetSCMPInformationRequest" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();

                string ftpResponse = "";

                if (FTPClass.FtpSendFile(strFTPServer + @"GetSCMPInformationRequest" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"GetMeteringPointMPRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                {
                    if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse); }
                    //success = false;
                }

            }
            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                BestandsAanvulling = " LV " + BestandsAanvulling;
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();



                if (strFileName == "")
                {
                    if (blnToFile == true && Klant_Config != "")
                    {
                        string fileName = path + @"GetSCMPInformation.xml";

                        // string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                        XmlSerializer serializer1 = new XmlSerializer(typeof(nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope));
                        if (File.Exists(fileName)) { File.Delete(fileName); }
                        TextWriter WriteFileStream1 = new StreamWriter(fileName);
                        serializer1.Serialize(WriteFileStream1, enveloppe);
                        WriteFileStream1.Close();

                        IPAddress ipAddr = localAddr;
                        IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

                        // Create a TCP socket.
                        Socket client = new Socket(AddressFamily.InterNetwork,
                                SocketType.Stream, ProtocolType.Tcp);

                        // Connect the socket to the remote endpoint.
                        client.Connect(ipEndPoint);

                        if (File.Exists(path + "RemoteResult.xml") == true) { File.Delete(path + "RemoteResult.xml"); }

                        client.SendFile(fileName);


                        // Release the socket.
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();

                        SqlConnection conn = new SqlConnection(ConnString);
                        conn.Open();

                        int intCounter = 0;
                        int intRecords = 0;
                        //WriteLog("Lus", 10, 0);
                        while (intRecords == 0 && intCounter < 200)
                        {
                            string strSQL = "select count(*) from Messages.dbo.XMLMessage";
                            SqlCommand cmd = new SqlCommand(strSQL, conn);
                            intRecords = (int)cmd.ExecuteScalar();
                            Application.DoEvents();
                            Thread.Sleep(100);
                            intCounter++;
                        }

                        //WriteLog("EindeLus " + intRecords.ToString(), 10, 0);

                        if (intRecords > 0)
                        {
                            string strSQL = "select * from Messages.dbo.XMLMessage";
                            SqlCommand cmd = new SqlCommand(strSQL, conn);
                            string strXML = cmd.ExecuteScalar().ToString();
                            //WriteLog("Eindelees " + strXML.Length.ToString(), 10, 0);


                            serializer = new XmlSerializer(typeof(nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationResponseEnvelope));
                            _Doc = new XmlDocument();
                            File.WriteAllText(path + "RemoteResult.xml", strXML);

                            _Doc.LoadXml(strXML);
                            //_Doc.Load(path + "RemoteResult.xml");
                            //nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope result = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope();
                            //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
                            retour = (nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
                        }
                        conn.Close();
                    }
                    else
                    {
                        retour = getMeteringPointMP.CallGetSCMPInformation(enveloppe);
                    }

                    //Save to file kan weg
                    string responsefile = "GetSCMPInformationResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml";
                    WriteFileStream = new StreamWriter(path + responsefile);
                    //WriteFileStream = new StreamWriter(path + @"MasterDataResponse" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    string ftpResponse = "";

                    if (FTPClass.FtpSendFile(strFTPServer + responsefile, strFTPUser, strFTPPassword, path + responsefile, out ftpResponse) == false)
                    {
                        if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse); }
                        //success = false;
                    }


                    //Save to String
                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);

                    ftpResponse = "";
                    if (Klant_Config == "ROBIN" && strTest != "JA")
                    {
                        if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "GetSCMPInformationResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + responsefile, out ftpResponse) == false)
                        {
                            if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse); }
                            //success = false;
                        }
                    }

                    //BestandsAanvulling = " LV 030514 143925";niet meer nodig 8nov
                    //_Doc.Load(path + "MasterDataResult" + BestandsAanvulling + ".xml");
                }
                else
                {

                    retour = new nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationResponseEnvelope();

                    _Doc.Load(strFileName);
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationResponseEnvelope));
                    retour = (nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
                }

                //Nog check op rejection

                //if (retour.Portaal_Content.Item.GetType() == typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection))
                //{
                //    nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection portaal_Rejection = new nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection();
                //    portaal_Rejection = (nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection)retour.Portaal_Content.Item;

                //    if (blnBatch == false) { MessageBox.Show("Fout : " + portaal_Rejection.Rejection[0].RejectionText); }
                //    success = false;
                //}
                //else
                //{
                //    SwitchBericht switchBericht = new SwitchBericht();
                //    int inboxID = switchBericht.Save_Inbox(26, _Doc.InnerXml.ToString(), "Masterdata : " + header.DocumentID);
                //    int edineID = switchBericht.Save_Edine(inboxID, receiver.ReceiverID, source.SenderID, DateTime.Now, header.DocumentID, "UTILMD", "E07");
                //    nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint portaal_MeteringPoint = new nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint();
                //    portaal_MeteringPoint = (nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)retour.Portaal_Content.Item;

                //    Masterdata md = new Masterdata(edineID, portaal_MeteringPoint, header.DocumentID, source.SenderID, receiver.ReceiverID, source.SenderID);

                //    if (blnBatch == false)
                //    {
                //        MessageBox.Show("EAN : " + md.LOC_GC + "\r\n" + "Naam : " + md.NameIT + "\r\n" + "Adres : " + md.Address + " " + md.HouseNr +
                //            "\r\nPlaats : " + md.PostalCode + " " + md.City + "\r\nSJV : " + md.EstimatedAnnualVolumeHigh + "\r\nSJV Laag : " + md.EstimatedAnnualVolumeLow +
                //            "\r\nProfiel : " + md.Profile + "\r\nCapaciteitscode : " + md.CapaciteitsTariefCode + "\r\nLeverancier : " + md.NAD_DP +
                //            "\r\nPV : " + md.BRP);
                //    }
                //    ProcessMessage.processMessage(inboxID);
                //}

                if (strFileName != "")
                {
                    File.Delete(strFileName);
                    strFileName = OldFileName;
                }
                //EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;
                //if (portaalResponse.Item.GetType() == typeof(EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection))
                //{
                //    EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
                //    EDSN_MoveIn.MoveInResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
                //    MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
                //}
                //else
                //{
                //    EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint reponseItem = (EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;

                //    MessageBox.Show("Accepted");
                //}
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                if (blnBatch == false)
                {
                    //MessageBox.Show(S.ErrorCode.ToString());
                    //MessageBox.Show(S.ErrorDetails);
                    //MessageBox.Show(S.ErrorText);
                    //MessageBox.Show(ex.Detail.InnerXml.ToString());
                }
                //success = false;
            }
            catch //(WebException exception)
            {
                if (blnBatch == false)
                {
                    //MessageBox.Show(exception.Message);
                }
                //success = false;
            }
            //catch (Exception exception)
            //{
            //    if (blnBatch == false)
            //    {
            //        //MessageBox.Show(exception.Message);
            //    }
            //    success = false;
            //}


            return strResult;
        }
    }
        
    

}
