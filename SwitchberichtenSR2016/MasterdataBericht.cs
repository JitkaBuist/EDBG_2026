//using System;
//using System.Collections.Generic;
//using System.Text;
//using Energie.DataTableHelper;
//using System.Data.SqlClient;
//using System.Data;
////mail libraries can be disabled when testing is done
//using System.Net.Mail;
//using System.IO;
//using System.Diagnostics;
//using System.Net;
//using System.Xml.Serialization;
//using System.Windows.Forms;
//using nl.Energie.EDSN;
//using System.Xml;
//using System.Net.Sockets;
//using System.Threading;

//namespace Energie.SwitchBericht
//{
//    public class MasterdataBericht
//    {
//        private string SQLstatement;
//        private static String ConnString = "";
//        private string urlWebService = Energie.DataAccess.Configurations.GetApplicationSetting("PORTAAL");//"https://emp.edsn.nl/b2b";
//        //private string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase );
//        private string path = "";//@"c:\test\";
//        private string certPV = "";
//        private string certPVPassword = "";
//        private string certLV = "";
//        private string certLVPassword = "";
//        private string HoofdPV = "";
//        private string HoofdLV = "";
//        private string Klant_Config = "";

//        private Int32 port = 13000;
//        private IPAddress localAddr = IPAddress.Parse("82.139.104.75");
//        private int intService = -1;
//        private TcpListener listener;
//        private Boolean blnContinue = true;
//        private System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
//        private string strFTPServer = Energie.DataAccess.Configurations.GetApplicationSetting("FTPSERVER");
//        private string strFTPUser = "";
//        private string strFTPPassword  = "";
//        private string strTest = Energie.DataAccess.Configurations.GetApplicationSetting("TEST");

//        public MasterdataBericht(string klant_Config)
//        {
//            Klant_Config = klant_Config;

//            if (Klant_Config != "")
//            {
//                HoofdPV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV_" + Klant_Config).Trim();
//                HoofdLV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV_" + Klant_Config).Trim();
//                ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB_" + Klant_Config);
//                certPV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPV_" + Klant_Config);
//                certPVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPVPASSWORD_" + Klant_Config);
//                certLV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLV_" + Klant_Config);
//                certLVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLVPASSWORD_" + Klant_Config);
//                path = Energie.DataAccess.Configurations.GetApplicationSetting("XMLPath_" + Klant_Config);
//                strFTPUser = Energie.DataAccess.Configurations.GetApplicationSetting("FTPUSER_" + Klant_Config).Trim();
//                strFTPPassword = Energie.DataAccess.Configurations.GetApplicationSetting("FTPPASSWORD_" + Klant_Config).Trim();
//            }
//            else
//            {
//                ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB");
//                path = Energie.DataAccess.Configurations.GetApplicationSetting("XMLPATH");//@"c:\test\";
//                certPV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPV");
//                certPVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPVPASSWORD");
//                certLV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLV");
//                certLVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLVPASSWORD");
//                HoofdPV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV").Trim();
//                HoofdLV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV").Trim();

//            }

//            //KC.KlantConfig = klantConfig;
//            //KC.ConnStringPortaal = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");

//            //carShared = new CarShared();
//        }

//        public Boolean RequestMasterdata(string ean18, string netbeheerder, Boolean blnPV, string strFileName, Boolean blnBatch, Boolean blnToFile, string strRequestFile)
//        {
//            Boolean success = true;
//            string OldFileName = strFileName;
//            //if (strFileName != "")
//            //{
//            //    string text = File.ReadAllText(strFileName);
//            //    text = text.Replace("<EDSNDocument>", @"<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "utf-8" + '"' + "?>");
//            //    text = text.Replace("</EDSNDocument>", "");
//            //    text = text.Replace("xmlns=" + '"' + "urn:nedu:edsn:data:masterdataresponse:1:standard" + '"', "xmlns:xsi=" + '"' + "http://www.w3.org/2001/XMLSchema-instance" + '"' + " xmlns:xsd=" + '"' + "http://www.w3.org/2001/XMLSchema" + '"');
//            //    text = text.Replace("<EDSNBusinessDocumentHeader>", "<EDSNBusinessDocumentHeader xmlns=" + '"' + "urn:nedu:edsn:data:masterdataresponse:1:standard" + '"' + ">");
//            //    text = text.Replace("<Portaal_Content>", "<Portaal_Content xmlns=" + '"' + "urn:nedu:edsn:data:masterdataresponse:1:standard" + '"' + ">");
//            //    strFileName = strFileName.Trim() + ".tmp";
//            //    File.WriteAllText(strFileName, text);
//            //}
//            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope enveloppe = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = "8712423010208";
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            if (blnPV != true) { source.SenderID = HoofdLV; } else { source.SenderID = HoofdPV; }
//            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
//            //source.ContactTypeIdentifier = "DDQ_O";
//            header.Source = source;


//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content();
//            enveloppe.Portaal_Content = portaal_content;

//            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint meteringpoint = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint();

//            meteringpoint.EANID = ean18;
//            portaal_content.Portaal_MeteringPoint = meteringpoint;

//            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company();
//            meteringpointGridOperator.ID = netbeheerder;
//            meteringpoint.GridOperator_Company = meteringpointGridOperator;


//            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation meteringpointPortaalMutation = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation();
//            meteringpoint.Portaal_Mutation = meteringpointPortaalMutation;

//            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation_Portaal_UserInformation mutationUserInfo = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation_Portaal_UserInformation();
//            if (blnPV != true) { mutationUserInfo.Organisation = HoofdLV; } else { mutationUserInfo.Organisation = HoofdPV; }
//            //mutationUserInfo.Organisation = "8712423014381";
//            meteringpointPortaalMutation.Portaal_UserInformation = mutationUserInfo;

//            nl.Energie.EDSN.MasterData.MasterData masterData = new nl.Energie.EDSN.MasterData.MasterData();

//            //String certPath = @"c:\test\EDSN2013010300005.p12";
//            //masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath, "T81SKoL#6D"));

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            if (strFileName == "")
//            {
//                if (blnPV == true)
//                {
//                    //certPath = certpath + @"EDSN2013053100007.p12";
//                    masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
//                }
//                else
//                {
//                    masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
//                }



//                ServicePointManager.Expect100Continue = true;
//                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
//            }

//            masterData.Url = KC.CarUrl + @"synchroon/ResponderMasterDataRespondingActivity";

//            masterData.Timeout = 120000;

//            nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope retour = new nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope();


//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope));
//            TextWriter WriteFileStream;
//            if (strFileName == "")
//            {
//                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                { BestandsAanvulling = " LV " + BestandsAanvulling; }

//                WriteFileStream = new StreamWriter(path + @"MasterDataRequest" + BestandsAanvulling + ".xml");
//                serializer.Serialize(WriteFileStream, enveloppe);
//                WriteFileStream.Close();

//                string ftpResponse = "";

//                    if (FTPClass.FtpSendFile(strFTPServer + @"MasterDataRequest" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MasterDataRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                    {
//                        if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse); }
//                        success = false;
//                    }

//            }
//            try
//            {
//                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }
//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope));
//                XmlDocument _Doc = new XmlDocument();



//                if (strFileName == "")
//                {
//                    if (blnToFile == true && Klant_Config != "")
//                    {
//                        string fileName = path + @"Masterdata.xml";

//                        // string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                        XmlSerializer serializer1 = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope));
//                        if (File.Exists(fileName)) { File.Delete(fileName); }
//                        TextWriter WriteFileStream1 = new StreamWriter(fileName);
//                        serializer1.Serialize(WriteFileStream1, enveloppe);
//                        WriteFileStream1.Close();

//                        IPAddress ipAddr = localAddr;
//                        IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

//                        // Create a TCP socket.
//                        Socket client = new Socket(AddressFamily.InterNetwork,
//                                SocketType.Stream, ProtocolType.Tcp);

//                        // Connect the socket to the remote endpoint.
//                        client.Connect(ipEndPoint);

//                        if (File.Exists(path + "RemoteResult.xml") == true) { File.Delete(path + "RemoteResult.xml"); }

//                        client.SendFile(fileName);


//                        // Release the socket.
//                        client.Shutdown(SocketShutdown.Both);
//                        client.Close();

//                        SqlConnection conn = new SqlConnection(ConnString);
//                        conn.Open();

//                        int intCounter = 0;
//                        int intRecords = 0;
//                        //WriteLog("Lus", 10, 0);
//                        while (intRecords == 0 && intCounter < 200)
//                        {
//                            string strSQL = "select count(*) from Messages.dbo.XMLMessage";
//                            SqlCommand cmd = new SqlCommand(strSQL, conn);
//                            intRecords = (int)cmd.ExecuteScalar();
//                            Application.DoEvents();
//                            Thread.Sleep(100);
//                            intCounter++;
//                        }

//                        //WriteLog("EindeLus " + intRecords.ToString(), 10, 0);

//                        if (intRecords > 0)
//                        {
//                            string strSQL = "select * from Messages.dbo.XMLMessage";
//                            SqlCommand cmd = new SqlCommand(strSQL, conn);
//                            string strXML = cmd.ExecuteScalar().ToString();
//                            //WriteLog("Eindelees " + strXML.Length.ToString(), 10, 0);


//                            serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope));
//                            _Doc = new XmlDocument();
//                            File.WriteAllText(path + "RemoteResult.xml", strXML);

//                            _Doc.LoadXml(strXML);
//                            //_Doc.Load(path + "RemoteResult.xml");
//                            //nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope result = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope();
//                            //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
//                            retour = (nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
//                        }
//                        conn.Close();
//                    }
//                    else
//                    {
//                        retour = masterData.CallMasterData(enveloppe);
//                    }

//                    //Save to file kan weg
//                    string responsefile = "MasterDataResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml";
//                    WriteFileStream = new StreamWriter(path + responsefile);
//                    //WriteFileStream = new StreamWriter(path + @"MasterDataResponse" + BestandsAanvulling + ".xml");
//                    serializer.Serialize(WriteFileStream, retour);
//                    WriteFileStream.Close();

//                    string ftpResponse = "";

//                        if (FTPClass.FtpSendFile(strFTPServer + responsefile, strFTPUser, strFTPPassword, path + responsefile, out ftpResponse) == false)
//                        {
//                            if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse); }
//                            success = false;
//                        }


//                    //Save to String
//                    StringWriter swXML = new StringWriter();
//                    serializer.Serialize(swXML, retour);

//                    ftpResponse = "";
//                    if (Klant_Config == "ROBIN" && strTest != "JA")
//                    {
//                        if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "MasterDataResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + responsefile, out ftpResponse) == false)
//                        {
//                            if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse); }
//                            success = false;
//                        }
//                    }

//                    //BestandsAanvulling = " LV 030514 143925";niet meer nodig 8nov
//                    //_Doc.Load(path + "MasterDataResult" + BestandsAanvulling + ".xml");
//                }
//                else
//                {

//                    retour = new nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope();

//                    _Doc.Load(strFileName);
//                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope));
//                    retour = (nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
//                }

//                //Nog check op rejection

//                //if (retour.Portaal_Content.Item.GetType() == typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection))
//                //{
//                //    nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection portaal_Rejection = new nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection();
//                //    portaal_Rejection = (nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_Rejection)retour.Portaal_Content.Item;

//                //    if (blnBatch == false) { MessageBox.Show("Fout : " + portaal_Rejection.Rejection[0].RejectionText); }
//                //    success = false;
//                //}
//                //else
//                //{
//                //    SwitchBericht switchBericht = new SwitchBericht();
//                //    int inboxID = switchBericht.Save_Inbox(26, _Doc.InnerXml.ToString(), "Masterdata : " + header.DocumentID);
//                //    int edineID = switchBericht.Save_Edine(inboxID, receiver.ReceiverID, source.SenderID, DateTime.Now, header.DocumentID, "UTILMD", "E07");
//                //    nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint portaal_MeteringPoint = new nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint();
//                //    portaal_MeteringPoint = (nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)retour.Portaal_Content.Item;

//                //    Masterdata md = new Masterdata(edineID, portaal_MeteringPoint, header.DocumentID, source.SenderID, receiver.ReceiverID, source.SenderID);

//                //    if (blnBatch == false)
//                //    {
//                //        MessageBox.Show("EAN : " + md.LOC_GC + "\r\n" + "Naam : " + md.NameIT + "\r\n" + "Adres : " + md.Address + " " + md.HouseNr +
//                //            "\r\nPlaats : " + md.PostalCode + " " + md.City + "\r\nSJV : " + md.EstimatedAnnualVolumeHigh + "\r\nSJV Laag : " + md.EstimatedAnnualVolumeLow +
//                //            "\r\nProfiel : " + md.Profile + "\r\nCapaciteitscode : " + md.CapaciteitsTariefCode + "\r\nLeverancier : " + md.NAD_DP +
//                //            "\r\nPV : " + md.BRP);
//                //    }
//                //    ProcessMessage.processMessage(inboxID);
//                //}

//                if (strFileName != "")
//                {
//                    File.Delete(strFileName);
//                    strFileName = OldFileName;
//                }
//                //EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;
//                //if (portaalResponse.Item.GetType() == typeof(EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection))
//                //{
//                //    EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
//                //    EDSN_MoveIn.MoveInResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
//                //    MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
//                //}
//                //else
//                //{
//                //    EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint reponseItem = (EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;

//                //    MessageBox.Show("Accepted");
//                //}
//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                if (blnBatch == false)
//                {
//                    //MessageBox.Show(S.ErrorCode.ToString());
//                    //MessageBox.Show(S.ErrorDetails);
//                    //MessageBox.Show(S.ErrorText);
//                    //MessageBox.Show(ex.Detail.InnerXml.ToString());
//                }
//                success = false;
//            }
//            catch (WebException exception)
//            {
//                if (blnBatch == false)
//                {
//                    //MessageBox.Show(exception.Message);
//                }
//                success = false;
//            }
//            catch (Exception exception)
//            {
//                if (blnBatch == false)
//                {
//                    //MessageBox.Show(exception.Message);
//                }
//                success = false;
//            }
//            return success;
//        }

//        public Boolean RequestMasterdataFromXML(string strRequestFile)
//        {
//            Boolean success = true;

//            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope enveloppe = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope();



//            nl.Energie.EDSN.MasterData.MasterData masterData = new nl.Energie.EDSN.MasterData.MasterData();

//            //String certPath = @"c:\test\EDSN2013010300005.p12";
//            //masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath, "T81SKoL#6D"));

//            //String certPath = certpath + @"EDSN2013053100006.p12";


//                    masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));



//                ServicePointManager.Expect100Continue = true;
//                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;


//            masterData.Url = KC.CarUrl + @"synchroon/ResponderMasterDataRespondingActivity";

//            masterData.Timeout = 120000;

//            nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope retour = new nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope();



//            try
//            {
//                XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope));
//                XmlDocument _Doc = new XmlDocument();
//                _Doc.Load(strRequestFile);
//                enveloppe = (nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

//                retour = masterData.CallMasterData(enveloppe);
//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope));
//                StreamWriter WriteFileStream = new StreamWriter(path + @"result.xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();

//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {


//                success = false;
//            }
//            catch (WebException exception)
//            {

//                success = false;
//            }
//            catch (Exception exception)
//            {

//                success = false;
//            }
//            return success;
//        }

//        public Boolean RequestMasterdataUpdate(Boolean blnPV, Boolean blnBatch)
//        {
//            Boolean blnData = false;
//            //Masterdata
//            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope enveloppe = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = "8712423010208";
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            if (blnPV != true) { source.SenderID = HoofdLV; } else { source.SenderID = HoofdPV; }
//            //source.SenderID = "8712423014381";
//            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
//            header.Source = source;


//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_Portaal_Content();
//            enveloppe.Portaal_Content = portaal_content;

//            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdate masterDataUpdate = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdate();

//            //String certPath = @"c:\test\EDSN2013010300005.p12";
//            //masterDataUpdate.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath, "T81SKoL#6D"));


//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            if (blnPV == true)
//            {
//                //certPath = certpath + @"EDSN2013053100007.p12";
//                masterDataUpdate.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
//            }
//            else
//            {
//                //masterDataUpdate.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
//                //masterDataUpdate.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV));
//            }


//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            masterDataUpdate.Url = KC.CarUrl + @"batch/ResponderMasterDataUpdateRespondingActivity";

//            masterDataUpdate.Timeout = 120000;

//            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope retour = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope();


//            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + @"MasterDataUpdate" + BestandsAanvulling + ".xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();

//            string ftpResponse = "";

//                if (FTPClass.FtpSendFile(strFTPServer + @"MasterDataUpdate" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MasterDataUpdate" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                {
//                    MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse);
//                }


//            try
//            {
//                BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }

//                //Tijdelijk

//                retour = masterDataUpdate.CallMasterDataUpdate(enveloppe);
//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
//                //Save to file kan weg
//                WriteFileStream = new StreamWriter(path + @"MasterDataUpdateResult" + BestandsAanvulling + ".xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();

//                ftpResponse = "";
//                if (blnBatch != true && retour.Portaal_Content.Length > 0)
//                {
//                    if (FTPClass.FtpSendFile(strFTPServer + @"MasterDataUpdateResult" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MasterDataUpdateResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                    {
//                        MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse);
//                    }
//                }

//                //Tijdelijk

//                //retour = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope();

//                XmlDocument _Doc = new XmlDocument();
//                _Doc.Load(@"C:\tmp\opnieuw\MasterDataUpdateResult LV Test1.xml");
//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
//                retour = (nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));


//                //Save to String
//                //StringWriter swXML = new StringWriter();
//                //serializer.Serialize(swXML, retour);



//                //XmlDocument _Doc = new XmlDocument();
//                //_Doc.Load(path + @"MasterDataUpdateResult PV 121816 927400647.xml");
//                ////_Doc.Load(@"\\venus\office\AppEnergie\XMLOutput\MasterDataUpdateResult PV 092613 95149.xml");
//                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
//                //retour = (nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

//                SwitchBericht switchBericht = new SwitchBericht(Klant_Config);


//                nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP portaal_MeteringPoint = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP();
//                for (int i = 0; i < retour.Portaal_Content.Length; i++)
//                {
//                    blnData = true;
//                    int inboxID = switchBericht.Save_Inbox(26, @"MasterDataUpdateResult" + BestandsAanvulling + ".xml", "Masterdata : " + header.DocumentID);
//                    //int inboxID = switchBericht.Save_Inbox(26, swXML.ToString(), "Masterdata : " + header.DocumentID);
//                    int edineID = switchBericht.Save_Edine(inboxID, receiver.ReceiverID, source.SenderID, DateTime.Now, header.DocumentID, "UTILMD", "E07");
//                    portaal_MeteringPoint = (nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP)retour.Portaal_Content[i];

//                    Masterdata md = new Masterdata(edineID, portaal_MeteringPoint, header.DocumentID, source.SenderID, receiver.ReceiverID, source.SenderID, blnBatch, Klant_Config);
//                    ProcessMessage.processMessage(inboxID, ConnString);
//                }

//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                if (blnBatch)
//                {
//                    WriteLog("Fout MasterdataUpdate: " + S.ErrorDetails + " " + S.ErrorText, 10, 0);
//                }
//                else
//                {
//                    MessageBox.Show(S.ErrorCode.ToString());
//                    MessageBox.Show(S.ErrorDetails);
//                    MessageBox.Show(S.ErrorText);
//                    MessageBox.Show(ex.Detail.InnerXml.ToString());
//                }
//            }
//            catch (WebException exception)
//            {
//                if (blnBatch)
//                {
//                    WriteLog("Fout MasterdataUpdate: " + exception.Message, 10, 0);
//                }
//                else
//                {
//                    MessageBox.Show(exception.Message);
//                }
//            }
//            catch (Exception exception)
//            {
//                if (blnBatch)
//                {
//                    WriteLog("Fout MasterdataUpdate: " + exception.Message, 10, 0);
//                }
//                else
//                {
//                    MessageBox.Show(exception.Message);
//                }
//            }
//            return blnData;
//        }

//        public void RequestMasterdataBatch(string ean18, string netbeheerder, Boolean blnPV)
//        {
//            nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope enveloppe = new nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = "8712423010208";
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            if (blnPV != true) { source.SenderID = HoofdLV; } else { source.SenderID = HoofdPV; }
//            //source.SenderID = "8712423014381";
//            //source.ContactTypeIdentifier = "DDQ_O";
//            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
//            header.Source = source;



//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            //EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_Portaal_Content();
//            //enveloppe.Portaal_Content = portaal_content;

//            nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint[] meteringpoint = new nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint[1];
//            meteringpoint[0] = new nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint();
//            meteringpoint[0].EANID = ean18;
//            //portaal_content.Portaal_MeteringPoint = meteringpoint;

//            enveloppe.Portaal_Content = meteringpoint;

//            nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company();
//            meteringpointGridOperator.ID = netbeheerder;
//            meteringpoint[0].GridOperator_Company = meteringpointGridOperator;


//            nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation meteringpointPortaalMutation = new nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation();
//            meteringpoint[0].Portaal_Mutation = meteringpointPortaalMutation;

//            nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation_Portaal_UserInformation mutationUserInfo = new nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation_Portaal_UserInformation();
//            mutationUserInfo.Organisation = HoofdLV;
//            meteringpointPortaalMutation.Portaal_UserInformation = mutationUserInfo;

//            nl.Energie.EDSN.MasterDataBatch.MasterDataBatch masterData = new nl.Energie.EDSN.MasterDataBatch.MasterDataBatch();

//            //String certPath = @"c:\test\EDSN2013010300005.p12";
//            //masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath, "T81SKoL#6D"));

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            if (blnPV == true)
//            {
//                //certPath = certpath + @"EDSN2013053100007.p12";
//                masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
//            }
//            else
//            {
//                masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
//            }


//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            masterData.Url = KC.CarUrl + @"batch/ResponderMasterDataBatchRespondingActivity";

//            masterData.Timeout = 120000;

//            nl.Energie.EDSN.MasterDataBatch.MasterDataBatchResponseEnvelope retour = new nl.Energie.EDSN.MasterDataBatch.MasterDataBatchResponseEnvelope();


//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataBatch.MasterDataBatchRequestEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + @"MasterDataBatch.xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();

//            try
//            {
//                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; } 
//                retour = masterData.CallMasterDataBatch(enveloppe);
//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataBatch.MasterDataBatchResponseEnvelope));
//                //Save to file kan weg
//                WriteFileStream = new StreamWriter(path + @"MasterDataBatchResult" + BestandsAanvulling + ".xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();

//                //string ftpResponse = "";
//                //if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "MasterDataBatchResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + @"MasterDataBatchResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                //{
//                //    MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse);
//                //}


//                //Save to String
//                //StringWriter swXML = new StringWriter();
//                //serializer.Serialize(swXML, retour);

//                //Nog check op rejection

//                //SwitchBericht switchBericht = new SwitchBericht();
//                //int inboxID = switchBericht.Save_Inbox(26, swXML.ToString(), "Masterdata : " + header.DocumentID);
//                //int edineID = switchBericht.Save_Edine(inboxID, receiver.ReceiverID, source.SenderID, DateTime.Now, header.DocumentID, "UTILMD", "E07");
//                //EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint portaal_MeteringPoint = new Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint();
//                //portaal_MeteringPoint = (nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)retour.Portaal_Content.Item;

//                //Masterdata md = new Masterdata(edineID, portaal_MeteringPoint, header.DocumentID, source.SenderID, receiver.ReceiverID, source.SenderID);

//                //EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;
//                //if (portaalResponse.Item.GetType() == typeof(EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection))
//                //{
//                //    EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
//                //    EDSN_MoveIn.MoveInResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
//                //    MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
//                //}
//                //else
//                //{
//                //    EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint reponseItem = (EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;

//                //    MessageBox.Show("Accepted");
//                //}
//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                MessageBox.Show(S.ErrorCode.ToString());
//                MessageBox.Show(S.ErrorDetails);
//                MessageBox.Show(S.ErrorText);
//                MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }
//            catch (WebException exception)
//            {
//                MessageBox.Show(exception.Message);
//            }
//            catch (Exception exception)
//            {
//                MessageBox.Show(exception.Message);
//            }
//        }

//        public void GetMeteringPointResponseFromFile(string strFileName)
//        {
//            string text = File.ReadAllText(strFileName);
//            text = text.Replace("<EDSNDocument>", @"<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "utf-8" + '"' + "?>");
//            text = text.Replace("</EDSNDocument>", "");
//            text = text.Replace("xmlns=" + '"' + "urn:nedu:edsn:data:getmeteringpointresponse:1:standard" + '"', "xmlns:xsi=" + '"' + "http://www.w3.org/2001/XMLSchema-instance" + '"' + " xmlns:xsd=" + '"' + "http://www.w3.org/2001/XMLSchema" + '"');
//            text = text.Replace("<EDSNBusinessDocumentHeader>", "<EDSNBusinessDocumentHeader xmlns=" + '"' + "urn:nedu:edsn:data:getmeteringpointresponse:1:standard" + '"' + ">");
//            text = text.Replace("<Portaal_Content>", "<Portaal_Content xmlns=" + '"' + "urn:nedu:edsn:data:getmeteringpointresponse:1:standard" + '"' + ">");
//            strFileName = strFileName.Trim() + ".tmp";
//            File.WriteAllText(strFileName, text);


//            nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope enveloppe = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = "8712423010208";
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            source.SenderID = HoofdLV;

//            try
//            {
//                nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope retour = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope();
//                XmlDocument _Doc = new XmlDocument();
//                _Doc.Load(strFileName);
//                XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope));
//                retour = (nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));


//                //Nog check op rejection

//                SwitchBericht switchBericht = new SwitchBericht(Klant_Config);
//                int inboxID = switchBericht.Save_Inbox(26, _Doc.InnerXml.ToString(), "Masterdata : " + header.DocumentID);
//                int edineID = switchBericht.Save_Edine(inboxID, receiver.ReceiverID, source.SenderID, DateTime.Now, header.DocumentID, "UTILMD", "E07");
//                nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope_PC_PMP portaal_MeteringPoint = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope_PC_PMP();
//                portaal_MeteringPoint = (nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope_PC_PMP)retour.Portaal_Content.Item;

//                Masterdata md = new Masterdata(edineID, portaal_MeteringPoint, header.DocumentID, source.SenderID, receiver.ReceiverID, source.SenderID, true, Klant_Config);

//                ProcessMessage.processMessage(inboxID, ConnString);
//                //EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;
//                //if (portaalResponse.Item.GetType() == typeof(EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection))
//                //{
//                //    EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
//                //    EDSN_MoveIn.MoveInResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
//                //    MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
//                //}
//                //else
//                //{
//                //    EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint reponseItem = (EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;

//                //    MessageBox.Show("Accepted");
//                //}
//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                MessageBox.Show(S.ErrorCode.ToString());
//                MessageBox.Show(S.ErrorDetails);
//                MessageBox.Show(S.ErrorText);
//                MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }
//            catch (WebException exception)
//            {
//                MessageBox.Show(exception.Message);
//            }
//            catch (Exception exception)
//            {
//                MessageBox.Show(exception.Message);
//            }
//            File.Delete(strFileName);
//        }

//        public nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope GetMeteringPoint(Boolean blnPV, string ean, Boolean blnToFile, string strRequestFile)
//        {
//            if (Klant_Config != "")
//            {
//                if (blnToFile == true)
//                {
//                    timer1.Tick += new EventHandler(timer1_Tick);
//                    timer1.Interval = 100;
//                    timer1.Enabled = true;
//                }
//            }

//            nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope retour = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope();

//            nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope enveloppe = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = "8712423010208";
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            if (blnPV != true) { source.SenderID = HoofdLV; } else { source.SenderID = HoofdPV; }
//            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
//            header.Source = source;

//            nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_Portaal_Content portaal_Content = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_Portaal_Content();

//            enveloppe.Portaal_Content = portaal_Content;

//                nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_Portaal_Content_Portaal_MeteringPoint meteringPoint = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope_Portaal_Content_Portaal_MeteringPoint();
//                meteringPoint.EANID = ean;

//                portaal_Content.Portaal_MeteringPoint = meteringPoint;


//            nl.Energie.EDSN.MeteringPointInformation.MeteringPointInformation masterData = new nl.Energie.EDSN.MeteringPointInformation.MeteringPointInformation();

//            if (blnPV == true)
//            {
//                //certPath = certpath + @"EDSN2013053100007.p12";
//                masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
//            }
//            else
//            {
//                masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
//            }

//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            masterData.Url = KC.CarUrl + @"synchroon/ResponderMeteringPointInformationRespondingActivity";

//            masterData.Timeout = 120000;


//            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope));
//            TextWriter WriteFileStream;
//            //if (Klant_Config != "" && blnToFile == true)
//            //{
//            //    WriteFileStream = new StreamWriter(path + @"GetMeteringInformationResponse" + BestandsAanvulling + ".xml");
//            //    serializer.Serialize(WriteFileStream, enveloppe);
//            //    WriteFileStream.Close();
//            //}

//            string ftpResponse = "";
//            //if (Klant_Config != "")
//            //{
//            //    if (FTPClass.FtpSendFile("ftp://62.148.191.136/" + @"GetMeteringInformationResponse" + BestandsAanvulling + ".xml", "robin", "Wu!69Z#", path + @"GetMeteringInformationResponse" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//            //    {
//            //        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//            //    }
//            //}


//            if (Klant_Config != "")
//            {
//                try
//                {
//                    BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                    if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }

//                    //Tijdelijk

//                    //if (strRequestFile != "")
//                    //{
//                    //    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope));
//                    //    XmlDocument _Doc1 = new XmlDocument();
//                    //    _Doc1.Load(strRequestFile);
//                    //    //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
//                    //    enveloppe = (nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc1.DocumentElement));
//                    //}

//                    if (blnToFile == true)
//                    {
//                        string fileName = path + @"MeteringPointInformation.xml";

//                        // string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                        XmlSerializer serializer1 = new XmlSerializer(typeof(nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope));
//                        if (File.Exists(fileName)) { File.Delete(fileName); }
//                        TextWriter WriteFileStream1 = new StreamWriter(fileName);
//                        serializer1.Serialize(WriteFileStream1, enveloppe);
//                        WriteFileStream1.Close();

//                        IPAddress ipAddr = localAddr;
//                        IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

//                        // Create a TCP socket.
//                        Socket client = new Socket(AddressFamily.InterNetwork,
//                                SocketType.Stream, ProtocolType.Tcp);

//                        // Connect the socket to the remote endpoint.
//                        client.Connect(ipEndPoint);

//                        if (File.Exists(path + "RemoteResult.xml") == true) { File.Delete(path + "RemoteResult.xml"); }

//                        client.SendFile(fileName);


//                        // Release the socket.
//                        client.Shutdown(SocketShutdown.Both);
//                        client.Close();

//                        SqlConnection conn = new SqlConnection(ConnString);
//                        conn.Open();

//                        int intCounter = 0;
//                        int intRecords = 0;
//                        //WriteLog("Lus", 10, 0);
//                        while (intRecords == 0 && intCounter < 200)
//                        {
//                            string strSQL = "select count(*) from Messages.dbo.XMLMessage";
//                            SqlCommand cmd = new SqlCommand(strSQL, conn);
//                            intRecords = (int)cmd.ExecuteScalar();
//                            Application.DoEvents();
//                            Thread.Sleep(100);
//                            intCounter++;
//                        }

//                        //WriteLog("EindeLus " + intRecords.ToString(), 10, 0);

//                        if (intRecords > 0)
//                        {
//                            string strSQL = "select * from Messages.dbo.XMLMessage";
//                            SqlCommand cmd = new SqlCommand(strSQL, conn);
//                            string strXML = cmd.ExecuteScalar().ToString();
//                            //WriteLog("Eindelees " + strXML.Length.ToString(), 10, 0);


//                            serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope));
//                            XmlDocument _Doc = new XmlDocument();
//                            File.WriteAllText(path + "RemoteResult.xml", strXML);

//                            _Doc.LoadXml(strXML);
//                            //_Doc.Load(path + "RemoteResult.xml");
//                            //nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope result = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope();
//                            //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
//                            retour = (nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
//                        }
//                        conn.Close();

//                    }
//                    else
//                    {
//                        retour = masterData.GetMeteringPoint(enveloppe);
//                    }
//                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope));
//                    //Save to file kan weg
//                    WriteFileStream = new StreamWriter(path + @"GetMeteringInformationResult" + BestandsAanvulling + ".xml");
//                    serializer.Serialize(WriteFileStream, retour);
//                    WriteFileStream.Close();

//                    ftpResponse = "";
//                    if (FTPClass.FtpSendFile(strFTPServer + @"GetMeteringInformationResult" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"GetMeteringInformationResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                    {
//                        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//                    }


//                    //Save to String
//                    StringWriter swXML = new StringWriter();
//                    serializer.Serialize(swXML, retour);
//                    //Tijdelijk

//                    //BestandsAanvulling = " PV 030514 143926";
//                    XmlDocument _Doc1 = new XmlDocument();
//                    _Doc1.Load(path + @"GetMeteringInformationResult" + BestandsAanvulling + ".xml");

//                    //nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope_Portaal_Content portaal_Response = retour.Portaal_Content;

//                    //nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope_Portaal_Content_Portaal_MeteringPoint portaal_MeteringPointResponse = (nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaal_Response.Item;

//                    //MessageBox.Show("Ean : " + portaal_MeteringPointResponse.EANID.ToString() + "\r\nAdres : " + portaal_MeteringPointResponse.EDSN_AddressExtended.StreetName + " " + portaal_MeteringPointResponse.EDSN_AddressExtended.BuildingNr + 
//                    //    "\r\nPlaats : " + portaal_MeteringPointResponse.EDSN_AddressExtended.ZIPCode.ToString() + " " + portaal_MeteringPointResponse.EDSN_AddressExtended.CityName +
//                    //    "\r\nNetgebied : " + portaal_MeteringPointResponse.GridArea + "\r\nNetbeheerder : " + portaal_MeteringPointResponse.GridOperator_Company.ID.ToString());

//                }
//                catch (System.Web.Services.Protocols.SoapException ex)
//                {

//                    //XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                    //TextReader tr = new StringReader(ex.Detail.InnerXml);
//                    //SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                    //MessageBox.Show(S.ErrorCode.ToString());
//                    //MessageBox.Show(S.ErrorDetails);
//                    //MessageBox.Show(S.ErrorText);
//                    //MessageBox.Show(ex.Detail.InnerXml.ToString());
//                }
//                catch (WebException exception)
//                {
//                    //MessageBox.Show(exception.Message);
//                }
//                catch (Exception exception)
//                {
//                    //MessageBox.Show(exception.Message);
//                }
//            }
//            else
//            {
//                if (blnToFile != true)
//                {
//                    try
//                    {
//                        //BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                        //if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }

//                        //Tijdelijk

//                        if (strRequestFile != "")
//                        {
//                            serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope));
//                            XmlDocument _Doc = new XmlDocument();
//                            _Doc.Load(strRequestFile);
//                            //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
//                            enveloppe = (nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
//                        }
//                        retour = masterData.GetMeteringPoint(enveloppe);
//                        //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope));
//                        //Save to file kan weg
//                        //WriteFileStream = new StreamWriter(path + @"GetMeteringInformationResult" + BestandsAanvulling + ".xml");
//                        //serializer.Serialize(WriteFileStream, retour);
//                        //WriteFileStream.Close();

//                        //ftpResponse = "";
//                        //if (FTPClass.FtpSendFile("ftp://62.148.191.136/" + @"GetMeteringInformationResult" + BestandsAanvulling + ".xml", "robin", "Wu!69Z#", path + @"GetMeteringInformationResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                        //{
//                        //    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//                        //}


//                        //Save to String
//                        //StringWriter swXML = new StringWriter();
//                        //serializer.Serialize(swXML, retour);
//                        //Tijdelijk

//                        //BestandsAanvulling = " PV 030514 143926";
//                        //XmlDocument _Doc = new XmlDocument();
//                        //_Doc.Load(path + @"GetMeteringInformationResult" + BestandsAanvulling + ".xml");

//                        //nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope_Portaal_Content portaal_Response = retour.Portaal_Content;

//                        //nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope_Portaal_Content_Portaal_MeteringPoint portaal_MeteringPointResponse = (nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaal_Response.Item;

//                        //MessageBox.Show("Ean : " + portaal_MeteringPointResponse.EANID.ToString() + "\r\nAdres : " + portaal_MeteringPointResponse.EDSN_AddressExtended.StreetName + " " + portaal_MeteringPointResponse.EDSN_AddressExtended.BuildingNr + 
//                        //    "\r\nPlaats : " + portaal_MeteringPointResponse.EDSN_AddressExtended.ZIPCode.ToString() + " " + portaal_MeteringPointResponse.EDSN_AddressExtended.CityName +
//                        //    "\r\nNetgebied : " + portaal_MeteringPointResponse.GridArea + "\r\nNetbeheerder : " + portaal_MeteringPointResponse.GridOperator_Company.ID.ToString());

//                    }
//                    catch (System.Web.Services.Protocols.SoapException ex)
//                    {

//                        //XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                        //TextReader tr = new StringReader(ex.Detail.InnerXml);
//                        //SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                        //MessageBox.Show(S.ErrorCode.ToString());
//                        //MessageBox.Show(S.ErrorDetails);
//                        //MessageBox.Show(S.ErrorText);
//                        //MessageBox.Show(ex.Detail.InnerXml.ToString());
//                    }
//                    catch (WebException exception)
//                    {
//                        //MessageBox.Show(exception.Message);
//                    }
//                    catch (Exception exception)
//                    {
//                        //MessageBox.Show(exception.Message);
//                    }
//                }
//                else
//                {
//                    // string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope));
//                    if (File.Exists(path + @"GetMeteringInformationResponse.xml")) { File.Delete(path + @"GetMeteringInformationResponse.xml"); }
//                    WriteFileStream = new StreamWriter(path + @"GetMeteringInformationResponse.xml");
//                    serializer.Serialize(WriteFileStream, enveloppe);
//                    WriteFileStream.Close();
//                }
//            }
//            return retour;
//        }

//        public void GetMeteringPointFromRequestXML (string strRequestFile)
//        {


//            nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope retour = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope();

//            nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope enveloppe = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


//            nl.Energie.EDSN.MeteringPointInformation.MeteringPointInformation masterData = new nl.Energie.EDSN.MeteringPointInformation.MeteringPointInformation();


//            masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));

//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            masterData.Url = KC.CarUrl + @"synchroon/ResponderMeteringPointInformationRespondingActivity";

//            masterData.Timeout = 120000;






//            try
//            {
//                XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope));
//                XmlDocument _Doc = new XmlDocument();
//                _Doc.Load(strRequestFile);
//                //nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope result = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope();
//                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
//                enveloppe = (nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

//                retour = masterData.GetMeteringPoint(enveloppe);

//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope));
//                //Save to file kan weg
//                StreamWriter WriteFileStream = new StreamWriter(path + @"result.xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();



//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                //XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                //TextReader tr = new StringReader(ex.Detail.InnerXml);
//                //SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                //MessageBox.Show(S.ErrorCode.ToString());
//                //MessageBox.Show(S.ErrorDetails);
//                //MessageBox.Show(S.ErrorText);
//                //MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }
//            catch (WebException exception)
//            {
//                //MessageBox.Show(exception.Message);
//            }
//            catch (Exception exception)
//            {
//                //MessageBox.Show(exception.Message);
//            }
//            //}
//            //else
//            //{
//            //    // string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//            //    XmlSerializer serializer1 = new XmlSerializer(typeof(nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointRequestEnvelope));
//            //    if (File.Exists(path + @"GetMeteringInformationResponse.xml")) { File.Delete(path + @"GetMeteringInformationResponse.xml"); }
//            //    TextWriter WriteFileStream1 = new StreamWriter(path + @"GetMeteringInformationResponse.xml");
//            //    serializer1.Serialize(WriteFileStream1, enveloppe);
//            //    WriteFileStream1.Close();
//            //}

//        }

//        //public nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsResponseEnvelope SearchMeteringPoint(Boolean blnPV, string ean, 
//        //    string postcode, string huisnummer, string huisnummerToevoeging, string straat, string plaats, string tntPerceelnr, string bagID,
//        //    string bagGebouw, int intProduct)
//        //{
//        //    nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsResponseEnvelope retour = new nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsResponseEnvelope();

//        //    nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope enveloppe = new nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope();

//        //    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//        //    nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader();
//        //    header.ContentHash = "";
//        //    header.CreationTimestamp = DateTime.Now;
//        //    header.DocumentID = GetMessageID.getMessageID(ConnString);
//        //    header.ExpiresAt = DateTime.Now.AddMinutes(200);
//        //    header.ExpiresAtSpecified = true;
//        //    header.MessageID = System.Guid.NewGuid().ToString();
//        //    enveloppe.EDSNBusinessDocumentHeader = header;

//        //    nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//        //    header.Destination = destination;

//        //    nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//        //    receiver.Authority = "";
//        //    receiver.ContactTypeIdentifier = "EDSN";
//        //    receiver.ReceiverID = "8712423010208";
//        //    destination.Receiver = receiver;

//        //    nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//        //    if (blnPV != true) { source.SenderID = HoofdLV; } else { source.SenderID = HoofdPV; }
//        //    if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
//        //    header.Source = source;

//        //    nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_PC portaal_Content = new nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_PC();

//        //    enveloppe.Portaal_Content = portaal_Content;

//        //    nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_PC_PMP meteringPoint = new nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_PC_PMP();
//        //    if (ean.Trim() != "") { meteringPoint.EANID = ean; }
//        //    else
//        //    {
//        //        if (postcode.Trim() != "" || huisnummer.Trim() != "" || huisnummerToevoeging != "" || straat != "" || plaats != "" ||
//        //            tntPerceelnr != "" || bagID != "")
//        //        {
//        //            nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_Address adress = new nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_Address();
//        //            if (postcode.Trim() != "") { adress.ZIPCode = postcode; }
//        //            if (huisnummer.Trim() != "") { adress.BuildingNr = huisnummer; }
//        //            if (straat != "") { adress.StreetName = straat; }
//        //            if (huisnummerToevoeging != "") { adress.ExBuildingNr = huisnummerToevoeging; }
//        //            if (plaats != "") { adress.CityName = plaats; }
//        //            if (tntPerceelnr != "") { adress.TNTID = tntPerceelnr; }
//        //            if (bagID.Trim() != "" && bagGebouw.Trim() != "")
//        //            {
//        //                nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_BAGType bag = new nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope_BAGType();
//        //                bag.BAGID = bagID;
//        //                bag.BAGBuildingID = bagGebouw;
//        //                adress.BAG = bag;
//        //            }
//        //            meteringPoint.EDSN_AddressExtended = adress;

//        //        }

//        //    }

//        //    portaal_Content.Portaal_MeteringPoint = meteringPoint;

//        //    nl.Energie.EDSN.MeteringPointInformation.MeteringPointInformation masterData = new nl.Energie.EDSN.MeteringPointInformation.MeteringPointInformation();

//        //    if (blnPV == true)
//        //    {
//        //        //certPath = certpath + @"EDSN2013053100007.p12";
//        //        masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
//        //    }
//        //    else
//        //    {
//        //        masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
//        //    }

//        //    ServicePointManager.Expect100Continue = true;
//        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//        //    masterData.Url = KC.CarUrl + @"synchroon/ResponderMeteringPointInformationRespondingActivity";

//        //    masterData.Timeout = 120000;


//        //    string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//        //    XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsRequestEnvelope));
//        //    TextWriter WriteFileStream = new StreamWriter(path + @"SearchMeteringInformationResponse" + BestandsAanvulling + ".xml");
//        //    serializer.Serialize(WriteFileStream, enveloppe);
//        //    WriteFileStream.Close();

//        //    string ftpResponse = "";
//        //    if (FTPClass.FtpSendFile(strFTPServer + @"SearchMeteringInformationResponse" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"SearchMeteringInformationResponse" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//        //    {
//        //        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//        //    }

//        //    try
//        //    {
//        //        BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//        //        if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }

//        //        //Tijdelijk

//        //        retour = masterData.SearchMeteringPoints(enveloppe);
//        //        serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsResponseEnvelope));
//        //        //Save to file kan weg
//        //        WriteFileStream = new StreamWriter(path + @"SearchMeteringInformationResult" + BestandsAanvulling + ".xml");
//        //        serializer.Serialize(WriteFileStream, retour);
//        //        WriteFileStream.Close();

//        //        ftpResponse = "";
//        //        if (FTPClass.FtpSendFile(strFTPServer + @"SearchMeteringInformationResult" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"SearchMeteringInformationResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//        //        {
//        //            //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//        //        }


//        //        //Save to String
//        //        //StringWriter swXML = new StringWriter();
//        //        //serializer.Serialize(swXML, retour);
//        //        ////Tijdelijk

//        //        ////BestandsAanvulling = " PV 030514 143926";
//        //        //XmlDocument _Doc = new XmlDocument();
//        //        //_Doc.Load(path + @"SearchMeteringInformationResult" + BestandsAanvulling + ".xml");

//        //        //nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsResponseEnvelope_Portaal_Content portaal_Response = retour.Portaal_Content;

//        //        //nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsResponseEnvelope_Portaal_Content_Result_Portaal_MeteringPoint portaal_MeteringPointResponse = (nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsResponseEnvelope_Portaal_Content_Result_Portaal_MeteringPoint)portaal_Response.Item;

//        //        //MessageBox.Show("Ean : " + portaal_MeteringPointResponse.EANID.ToString());
//        //        //+ "\r\nAdres : " + portaal_MeteringPointResponse.EDSN_AddressExtended.StreetName + " " + portaal_MeteringPointResponse.EDSN_AddressExtended.BuildingNr +
//        //        //    "\r\nPlaats : " + portaal_MeteringPointResponse.EDSN_AddressExtended.ZIPCode.ToString() + " " + portaal_MeteringPointResponse.EDSN_AddressExtended.CityName +
//        //        //    "\r\nNetgebied : " + portaal_MeteringPointResponse.GridArea + "\r\nNetbeheerder : " + portaal_MeteringPointResponse.GridOperator_Company.ID.ToString());

//        //    }
//        //    catch (System.Web.Services.Protocols.SoapException ex)
//        //    {

//        //        XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//        //        TextReader tr = new StringReader(ex.Detail.InnerXml);
//        //        SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//        //        MessageBox.Show(S.ErrorCode.ToString());
//        //        MessageBox.Show(S.ErrorDetails);
//        //        MessageBox.Show(S.ErrorText);
//        //        MessageBox.Show(ex.Detail.InnerXml.ToString());
//        //    }
//        //    catch (WebException exception)
//        //    {
//        //        MessageBox.Show(exception.Message);
//        //    }
//        //    catch (Exception exception)
//        //    {
//        //        MessageBox.Show(exception.Message);
//        //    }
//        //    return retour;
//        //}


//        public nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope SearchMeteringPointMP(Boolean blnPV, string ean,
//            string postcode, string huisnummer, string huisnummerToevoeging, string straat, string plaats, string tntPerceelnr, string bagID,
//            string bagGebouw, int intProduct, out String strError)
//        {
//            strError = "0";
//            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope retour = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope();

//            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope enveloppe = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            strError = "1";
//            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;
//            strError = "2";
//            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;
//            strError = "3";
//            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = "8712423010208";
//            destination.Receiver = receiver;
//            strError = "4";
//            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            if (blnPV != true) { source.SenderID = HoofdLV; } else { source.SenderID = HoofdPV; }
//            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
//            header.Source = source;
//            strError = "5";
//            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_PC portaal_Content = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_PC();

//            enveloppe.Portaal_Content = portaal_Content;

//            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_PC_PMP meteringPoint = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_PC_PMP();
//            strError = "6";
//            if (ean.Trim() != "") { meteringPoint.EANID = ean; }
//            else
//            {
//                strError = "7";
//                if (postcode.Trim() != "" || huisnummer.Trim() != "" || huisnummerToevoeging != "" || straat != "" || plaats != "" ||
//                    tntPerceelnr != "" || bagID != "")
//                {
//                    nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_MPAddressRequestType adress = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_MPAddressRequestType();
//                    if (postcode.Trim() != "") { adress.ZIPCode = postcode; }
//                    if (huisnummer.Trim() != "") { adress.BuildingNr = huisnummer; }
//                    if (straat != "") { adress.StreetName = straat; }
//                    if (huisnummerToevoeging != "") { adress.ExBuildingNr = huisnummerToevoeging; }
//                    if (plaats != "") { adress.CityName = plaats; }
//                    if (bagID.Trim() != "" && bagGebouw.Trim() != "")
//                    {
//                        nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_BAGType bag = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_BAGType();
//                        bag.BAGID = bagID;
//                        bag.BAGBuildingID = bagGebouw;
//                        adress.BAG = bag;
//                    }
//                    meteringPoint.EDSN_AddressSearch = adress;
//                    strError = "8.1";
//                }

//            }
//            strError = "9";
//            portaal_Content.Portaal_MeteringPoint = meteringPoint;
//            strError = "10";
//            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsMP masterData = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsMP();

//            if (blnPV == true)
//            {
//                //certPath = certpath + @"EDSN2013053100007.p12";
//                masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
//            }
//            else
//            {
//                masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
//            }
//            strError = "11";
//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            masterData.Url = KC.CarUrl + @"synchroon/ResponderMeteringPointInformationRespondingActivity";
//            strError = "12";
//            masterData.Timeout = 120000;

//            strError = "12";
//            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + @"SearchMeteringInformationResponse" + BestandsAanvulling + ".xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();
//            strError = "13";
//            string ftpResponse = "";
//            if (FTPClass.FtpSendFile(strFTPServer + @"SearchMeteringInformationResponse" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"SearchMeteringInformationResponse" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//            {
//                //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//            }
//            strError = "14";
//            try
//            {
//                BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }

//                //Tijdelijk

//                retour = masterData.SearchMeteringPoints(enveloppe);
//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope));
//                //Save to file kan weg
//                WriteFileStream = new StreamWriter(path + @"SearchMeteringInformationResult" + BestandsAanvulling + ".xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();

//                ftpResponse = "";
//                if (FTPClass.FtpSendFile(strFTPServer + @"SearchMeteringInformationResult" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"SearchMeteringInformationResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                {
//                    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//                }


//                //Save to String
//                //StringWriter swXML = new StringWriter();
//                //serializer.Serialize(swXML, retour);
//                ////Tijdelijk

//                ////BestandsAanvulling = " PV 030514 143926";
//                //XmlDocument _Doc = new XmlDocument();
//                //_Doc.Load(path + @"SearchMeteringInformationResult" + BestandsAanvulling + ".xml");

//                //nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsResponseEnvelope_Portaal_Content portaal_Response = retour.Portaal_Content;

//                //nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsResponseEnvelope_Portaal_Content_Result_Portaal_MeteringPoint portaal_MeteringPointResponse = (nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsResponseEnvelope_Portaal_Content_Result_Portaal_MeteringPoint)portaal_Response.Item;

//                //MessageBox.Show("Ean : " + portaal_MeteringPointResponse.EANID.ToString());
//                //+ "\r\nAdres : " + portaal_MeteringPointResponse.EDSN_AddressExtended.StreetName + " " + portaal_MeteringPointResponse.EDSN_AddressExtended.BuildingNr +
//                //    "\r\nPlaats : " + portaal_MeteringPointResponse.EDSN_AddressExtended.ZIPCode.ToString() + " " + portaal_MeteringPointResponse.EDSN_AddressExtended.CityName +
//                //    "\r\nNetgebied : " + portaal_MeteringPointResponse.GridArea + "\r\nNetbeheerder : " + portaal_MeteringPointResponse.GridOperator_Company.ID.ToString());

//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                strError += ex.Detail.InnerXml;
//                //MessageBox.Show(S.ErrorCode.ToString());
//                //MessageBox.Show(S.ErrorDetails);
//                //MessageBox.Show(S.ErrorText);
//                //MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }
//            catch (WebException exception)
//            {
//                //MessageBox.Show(exception.Message);
//                strError += exception.Message;
//            }
//            catch (Exception exception)
//            {
//                //MessageBox.Show(exception.Message);
//                strError += exception.Message;
//            }
//            return retour;
//        }


//        public string ToevoegenCK(string ean, string geboortedatum, string iban)
//        {
//            string strResult = "FAILED";

//            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope enveloppe = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = "8712423010208";
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            source.SenderID = HoofdLV; 
//            source.ContactTypeIdentifier = "DDQ_O"; 
//            header.Source = source;

//            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC portaal_Content = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC();

//            enveloppe.Portaal_Content = portaal_Content;

//            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP meteringPoint = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP();

//            meteringPoint.EANID = ean;

//            portaal_Content.Portaal_MeteringPoint = meteringPoint;

//            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC mpcc = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC();

//            meteringPoint.MPCommercialCharacteristics = mpcc;

//            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company bsp = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company();
//            bsp.ID = HoofdLV;
//            mpcc.BalanceSupplier_Company = bsp;

//            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC_GridContractParty gcp = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC_GridContractParty();
//            gcp.BirthDayKey = geboortedatum;
//            gcp.IBANKey = iban;
//            mpcc.GridContractParty = gcp;

//            nl.Energie.EDSN.CKIdentification.CKIdentification ckidentification = new nl.Energie.EDSN.CKIdentification.CKIdentification();

//            ckidentification.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));

//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            ckidentification.Url = KC.CarUrl + @"synchroon/ResponderCKIdentificationRespondingActivity";

//            ckidentification.Timeout = 120000;

//            nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope retour = new nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope();

//            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + @"CKIdentificationResponse" + BestandsAanvulling + ".xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();

//            string ftpResponse = "";
//            if (FTPClass.FtpSendFile(strFTPServer + @"CKIdentificationResponse" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"CKIdentificationResponse" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//            {
//                //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//            }

//            try
//            {
//                BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                BestandsAanvulling = " LV " + BestandsAanvulling;

//                //Tijdelijk

//                retour = ckidentification.CreateCKRequest(enveloppe);
//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope));
//                //Save to file kan weg
//                WriteFileStream = new StreamWriter(path + @"CKIdentificationResult" + BestandsAanvulling + ".xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();

//                ftpResponse = "";
//                if (FTPClass.FtpSendFile(strFTPServer + @"CKIdentificationResult" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"CKIdentificationResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                {
//                    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//                }

//                if (retour.Portaal_Content.GetType() != typeof(nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope_RejectionPortaalType))
//                {
//                    strResult = "OK";
//                }
//                else
//                {
//                    nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope_RejectionPortaalType rejection = new nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope_RejectionPortaalType();
//                    //rejection = (nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope_RejectionPortaalType)retour.Portaal_Content;
//                    //TODO rejection uitlezen
//                    strResult = "FAILED";
//                }

//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                MessageBox.Show(S.ErrorCode.ToString());
//                MessageBox.Show(S.ErrorDetails);
//                MessageBox.Show(S.ErrorText);
//                MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }
//            catch (WebException exception)
//            {
//                MessageBox.Show(exception.Message);
//            }
//            catch (Exception exception)
//            {
//                MessageBox.Show(exception.Message);
//            }
//            return strResult;
//        }


//        //public void TestP4()
//        //{
//        //    if (Klant_Config != "")
//        //    {
//        //        string HoofdLV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV_" + Klant_Config).Trim();
//        //        string path = Energie.DataAccess.Configurations.GetApplicationSetting("XMLPATH_" + Klant_Config);
//        //        string certLV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLV_" + Klant_Config);
//        //        string certLVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLVPASSWORD_" + Klant_Config);
//        //    }
//        //    else
//        //    {
//        //        string HoofdLV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV").Trim();
//        //        string path = Energie.DataAccess.Configurations.GetApplicationSetting("XMLPATH");
//        //        string certLV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLV");
//        //        string certLVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLVPASSWORD");
//        //    }

//        //    nl.Energie.EDSN.P4.P4CollectedDataBatchRequestEnvelope enveloppe = new nl.Energie.EDSN.P4.P4CollectedDataBatchRequestEnvelope();

//        //    nl.Energie.EDSN.P4.P4CollectedDataBatchRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.P4.P4CollectedDataBatchRequestEnvelope_EDSNBusinessDocumentHeader();
//        //    //request.EDSNBusinessDocumentHeader = header;


//        //    //header.ContentHash = "";
//        //    //header.CreationTimestamp = DateTime.Now;
//        //    //header.CreationTimestamp = DateTime.Now;
//        //    //header.DocumentID = GetMessageID.getMessageID();
//        //    //header.ExpiresAt = DateTime.Now.AddMinutes(200);
//        //    //header.ExpiresAtSpecified = true;
//        //    header.MessageID = System.Guid.NewGuid().ToString();
//        //    enveloppe.EDSNBusinessDocumentHeader = header;

//        //    nl.Energie.EDSN.P4.P4CollectedDataBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.P4.P4CollectedDataBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//        //    header.Destination = destination;

//        //    nl.Energie.EDSN.P4.P4CollectedDataBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.P4.P4CollectedDataBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//        //    receiver.Authority = "";
//        //    //receiver.ContactTypeIdentifier = "EDSN";
//        //    receiver.ReceiverID = "8712423010208";
//        //    destination.Item = receiver;

//        //    nl.Energie.EDSN.P4.P4CollectedDataBatchRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.P4.P4CollectedDataBatchRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//        //    source.SenderID = HoofdLV;
//        //    //source.ContactTypeIdentifier = "DDQ_O";
//        //    //source.ContactTypeIdentifier = "DDQ_O";
//        //    header.Source = source;

//        //    nl.Energie.EDSN.P4.P4CollectedDataBatchRequestEnvelope_P4Content_P4MeteringPoint[] contentMeteringPoint = new nl.Energie.EDSN.P4.P4CollectedDataBatchRequestEnvelope_P4Content_P4MeteringPoint[1];
//        //    enveloppe.P4Content = contentMeteringPoint;

//        //    contentMeteringPoint[0] = new nl.Energie.EDSN.P4.P4CollectedDataBatchRequestEnvelope_P4Content_P4MeteringPoint();
//        //    contentMeteringPoint[0].QueryDate = DateTime.Now;
//        //    contentMeteringPoint[0].QueryDateSpecified = true;
//        //    contentMeteringPoint[0].QueryReason = nl.Energie.EDSN.P4.P4CollectedDataBatchRequestEnvelope_QueryReasonTypeCode.DAY;
//        //    contentMeteringPoint[0].EANID = "871687120061530229";

//        //    nl.Energie.EDSN.P4.P4 p4 = new nl.Energie.EDSN.P4.P4();

//        //    //p4.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(@"C:\EDBS\Certificaten\sign.p4-test.edsn.nl.cer"));
//        //    //p4.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
//        //    //string certFormat = p4.ClientCertificates[0].GetFormat();
//        //    //string certKeyAlf = p4.ClientCertificates[0].GetKeyAlgorithm();

//        //    //MessageBox.Show(certFormat + " " + certKeyAlf);

//        //    ServicePointManager.Expect100Continue = true;
//        //    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
//        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;


//        //    p4.Url = @"https://p4-test.edsn.nl/P4BatchVerzoekMeterstand/P4Port";
//        //    //p4.Url = @"https://pp4.edsn.nl/P4BatchVerzoekMeterstand/P4Port ";
//        //    p4.Timeout = 120000;



//        //    nl.Energie.EDSN.P4.P4CollectedDataBatchResponseEnvelope response = new nl.Energie.EDSN.P4.P4CollectedDataBatchResponseEnvelope();

//        //    XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.P4.P4CollectedDataBatchRequestEnvelope));
//        //    StringWriter swXML = new StringWriter();
//        //    serializer.Serialize(swXML, enveloppe);
//        //    //MessageBox.Show(swXML.ToString());

//        //    TextWriter WriteFileStream = new StreamWriter(path + @"P4Request.xml");
//        //    serializer.Serialize(WriteFileStream, enveloppe);
//        //    WriteFileStream.Close();

//        //    try
//        //    {

//        //        response = p4.P4CollectedDataBatchRequestEnvelope(enveloppe);
//        //    }
//        //    catch (System.Web.Services.Protocols.SoapException ex)
//        //    {

//        //        XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//        //        TextReader tr = new StringReader(ex.Detail.InnerXml);
//        //        SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//        //        MessageBox.Show(S.ErrorCode.ToString());
//        //        MessageBox.Show(S.ErrorDetails);
//        //        MessageBox.Show(S.ErrorText);
//        //        MessageBox.Show(ex.Detail.InnerXml.ToString());
//        //    }
//        //    catch (WebException exception)
//        //    {
//        //        MessageBox.Show(exception.Message);
//        //    }
//        //    catch (Exception exception)
//        //    {
//        //        MessageBox.Show(exception.Message);
//        //    }


//        //    nl.Energie.EDSN.P4.P4CollectedDataBatchResponseEnvelope_P4Content p4content = new nl.Energie.EDSN.P4.P4CollectedDataBatchResponseEnvelope_P4Content();
//        //    p4content = response.P4Content;
//        //}

//        private void timer1_Tick(object sender, EventArgs e)
//        {
//            timer1.Enabled = false;
//            listener = new TcpListener(System.Net.IPAddress.Any, port);
//            listener.Start();
//            DoBeginAcceptTcpClient(listener);
//        }

//        public ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

//        public void DoBeginAcceptTcpClient(TcpListener listener)
//        {
//            // Set the event to nonsignaled state.
//            tcpClientConnected.Reset();

//            // Start to listen for connections from a client.
//            Console.WriteLine("Waiting for a connection...");

//            // Accept the connection. 
//            // BeginAcceptSocket() creates the accepted socket.
//            listener.BeginAcceptTcpClient(
//                new AsyncCallback(DoAcceptTcpClientCallback),
//                listener);

//            // Wait until a connection is made and processed before 
//            // continuing.
//            //tcpClientConnected.WaitOne();
//        }

//        public void DoAcceptTcpClientCallback(IAsyncResult ar)
//        {
//            // Get the listener that handles the client request.
//            listener = (TcpListener)ar.AsyncState;




//            // End the operation and display the received data on 
//            // the console.
//            TcpClient client = listener.EndAcceptTcpClient(ar);
//            using (var stream = client.GetStream())
//            using (var output = File.Create(path + "RemoteResult.xml"))
//            {
//                Console.WriteLine("Client connected. Starting to receive the file");

//                // read the file in chunks of 1KB
//                var buffer = new byte[1024];
//                int bytesRead;
//                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
//                {
//                    output.Write(buffer, 0, bytesRead);
//                }
//            }

//            // Process the connection here. (Add the client to a
//            // server table, read data, etc.)
//            Console.WriteLine("Client connected completed");

//            // Signal the calling thread to continue.
//            tcpClientConnected.Set();
//            if (blnContinue) { DoBeginAcceptTcpClient(listener); }
//        }

//        public void WriteLog(string description, int LevelID, int inbox_ID)
//        {
//            try
//            {
//                SqlConnection conn = new SqlConnection(ConnString);
//                conn.Open();
//                SqlCommand cmdLog = new SqlCommand();
//                cmdLog.Connection = conn;
//                string str_SQL = "insert into Messages.dbo.ApplicationLogs (TimeStmp, Description, SourceID, LevelID, Inbox_ID) values(@TimeStmp, @Description, " +
//                    " @SourceID, @LevelID, @Inbox_ID)";
//                cmdLog.CommandText = str_SQL;
//                cmdLog.Parameters.AddWithValue("@TimeStmp", DateTime.Now);
//                string strDescription = description;
//                if (description.Length > 500) { strDescription = strDescription.Substring(0, 500); }
//                cmdLog.Parameters.AddWithValue("@Description", strDescription);
//                cmdLog.Parameters.AddWithValue("@SourceID", 4);
//                cmdLog.Parameters.AddWithValue("@LevelID", LevelID);
//                cmdLog.Parameters.AddWithValue("@Inbox_ID", inbox_ID);
//                cmdLog.ExecuteNonQuery();
//                conn.Close();
//            }
//            catch (Exception ex)
//            {
//                //EventLog eventlog = new EventLog("Application");
//                //eventlog.Source = "Energie App";
//                //eventlog.WriteEntry("WriteLog : " + ex.Message, EventLogEntryType.Error, 0);
//            }
//        }
//    }

//}
