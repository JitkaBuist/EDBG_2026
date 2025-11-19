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
    public class CKIdentification
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

        //private Int32 port = 13000;
        private IPAddress localAddr = IPAddress.Parse("82.139.104.75");
        //private int intService = -1;
        //private TcpListener listener;
        //private Boolean blnContinue = true;
        private System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
        private string strFTPServer = Energie.DataAccess.Configurations.GetApplicationSetting("FTPSERVER");
        private string strFTPUser = "";
        private string strFTPPassword = "";
        private string strTest = Energie.DataAccess.Configurations.GetApplicationSetting("TEST");

        public CKIdentification(string klant_Config)
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

            KC.KlantConfig = klant_Config;
            KC.ConnStringPortaal = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");
        }

        public Boolean AddIdentification(String eancode, string strBirthDay, string strIBAN, Boolean blnBatch, string strFileName)
        {
            //(string ean18, string netbeheerder, Boolean blnPV, string strFileName, Boolean blnBatch, Boolean blnToFile, string strRequestFile)

            Boolean success = true;
            string OldFileName = strFileName;

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope enveloppe = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope();

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            source.SenderID = HoofdLV;
            source.ContactTypeIdentifier = "DDQ_O";
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC portaal_content = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP meteringpoint = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP();

            meteringpoint.EANID = eancode;
            portaal_content.Portaal_MeteringPoint = meteringpoint;

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC cc = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC();
            meteringpoint.MPCommercialCharacteristics = cc;

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company balanceSupplier = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company();
            balanceSupplier.ID = HoofdLV;
            cc.BalanceSupplier_Company = balanceSupplier;

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC_GridContractParty contractParty = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC_GridContractParty();
            contractParty.BirthDayKey = strBirthDay;
            contractParty.IBANKey = strIBAN;
            cc.GridContractParty = contractParty;

            nl.Energie.EDSN.CKIdentification.CKIdentification ckIdentification = new nl.Energie.EDSN.CKIdentification.CKIdentification();

            ckIdentification.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            ckIdentification.Url = KC.CarUrl + @"/synchroon/ResponderCKIdentificationRespondingActivity";

            ckIdentification.Timeout = 120000;

            nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope retour = new nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope();

            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope));
            TextWriter WriteFileStream;

            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                { BestandsAanvulling = " LV " + BestandsAanvulling; }

                WriteFileStream = new StreamWriter(path + @"CKIdentificationRequest" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();

                string ftpResponse = "";

                if (FTPClass.FtpSendFile(strFTPServer + @"CKIdentificationRequest" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"CKIdentificationRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                {
                    if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse); }
                    success = false;
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
                    retour = ckIdentification.CreateCKRequest(enveloppe);
                    

                    //Save to file kan weg
                    string responsefile = "CKIdentificationResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml";
                    WriteFileStream = new StreamWriter(path + responsefile);
                    //WriteFileStream = new StreamWriter(path + @"MasterDataResponse" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    string ftpResponse = "";

                    if (FTPClass.FtpSendFile(strFTPServer + responsefile, strFTPUser, strFTPPassword, path + responsefile, out ftpResponse) == false)
                    {
                        if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse); }
                        success = false;
                    }


                    //Save to String
                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);

                    ftpResponse = "";
                    if (Klant_Config == "ROBIN" && strTest != "JA")
                    {
                        if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "CKIdentificationEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + responsefile, out ftpResponse) == false)
                        {
                            if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse); }
                            success = false;
                        }
                    }

                    //BestandsAanvulling = " LV 030514 143925";niet meer nodig 8nov
                    //_Doc.Load(path + "MasterDataResult" + BestandsAanvulling + ".xml");
                }
                else
                {

                    retour = new nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope();

                    _Doc.Load(strFileName);
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope));
                    retour = (nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
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
                success = false;
            }
            //catch (WebException exception)
            //{
            //    if (blnBatch == false)
            //    {
            //        //MessageBox.Show(exception.Message);
            //    }
            //    success = false;
            //}
            catch 
            {
                if (blnBatch == false)
                {
                    //MessageBox.Show(exception.Message);
                }
                success = false;
            }

            return success;
        }
    }
}
