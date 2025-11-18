using System;
using System.Collections.Generic;
using System.Text;
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
using nl.Energie.VerwerkCar;

namespace Energie.Car
{
    

    public class MasterdataClient
    {
        //private string strSql;
        //private static String ConnString = "";
        //private string urlWebService = Energie.DataAccess.Configurations.GetApplicationSetting("PORTAAL");//"https://emp.edsn.nl/b2b";
        private CarShared carShared;

        public MasterdataClient(string klantConfig)
        {
            KC.KlantConfig = klantConfig;
            KC.ConnStringPortaal = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");

            carShared = new CarShared();
        }

        public String RequestMasterdataUpdate(Boolean blnPV, Boolean blnBatch, out int nrRecords) 
        {
            string strError = "Start";
            nrRecords = 0;

            //Boolean blnData = false;
            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope enveloppe = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope();

            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            if (blnPV != true) { source.SenderID = KC.HoofdLV.ToString(); } else { source.SenderID = KC.HoofdPV.ToString(); }
            //source.SenderID = "8712423014381";
            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            header.Source = source;

            strError = source.SenderID;

            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdate masterDataUpdate = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdate();

            strError = "Certificaat";

            if (blnPV == true)
            {
                masterDataUpdate.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertPV, KC.CertPVPassword));
            }
            else
            {
                masterDataUpdate.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
            }


            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            masterDataUpdate.Url = KC.CarUrl + @"batch/ResponderMasterDataUpdateRespondingActivity";

            masterDataUpdate.Timeout = 120000;

            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope response = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope();

            strError = KC.XMLPath;

            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateRequestEnvelope));
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"MasterDataUpdate" + BestandsAanvulling + ".xml");
            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();

            string ftpResponse = ""; 

            if (KC.FTPServer != "")
            {
                if (FTPClass.FtpSendFile(KC.FTPServer + @"MasterDataUpdate" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"MasterDataUpdate" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                {
                    if (!blnBatch) { MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse); }
                }
            }

            strError = "Na ftp";

            try
            {
                BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }

                //Tijdelijk

                strError = "Voor call " + KC.XMLPath;
                response = masterDataUpdate.CallMasterDataUpdate(enveloppe);
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
                //Save to file kan weg
                WriteFileStream = new StreamWriter(KC.XMLPath + @"MasterDataUpdateResult" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, response);
                WriteFileStream.Close();
                strError = "Na afroep";
                ftpResponse = "";
                if (blnBatch != true && response.Portaal_Content.Length > 0)
                {
                    if (FTPClass.FtpSendFile(KC.FTPServer + @"MasterDataUpdateResult" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"MasterDataUpdateResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    {
                        if (!blnBatch) { MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); }
                    }
                }

                //Tijdelijk

                //response = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope();

                //XmlDocument _Doc = new XmlDocument();
                //_Doc.Load(@"C:\tmp\opnieuw\MD.xml");
                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
                //response = (nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));


                //Save to String
                //StringWriter swXML = new StringWriter();
                //serializer.Serialize(swXML, response);



                //XmlDocument _Doc = new XmlDocument();
                //_Doc.Load(path + @"MasterDataUpdateResult PV 121816 927400647.xml");
                ////_Doc.Load(@"\\venus\office\AppEnergie\XMLOutput\MasterDataUpdateResult PV 092613 95149.xml");
                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
                //response = (nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

                strError = "Voor car " + KC.XMLPath; ;
                CarShared car = new CarShared();


                if (response.Portaal_Content.Length > 0)
                {
                    nrRecords = response.Portaal_Content.Length;
                    nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP portaal_MeteringPoint = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP();
                    int Bericht_ID = car.Save_Bericht(6, @"MasterDataUpdateResult" + BestandsAanvulling + ".xml", "Masterdata : " + header.DocumentID, true, response.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);
                    for (int i = 0; i < response.Portaal_Content.Length; i++)
                    {
                        //blnData = true;

                        //int inboxID = switchBericht.Save_Inbox(26, swXML.ToString(), "Masterdata : " + header.DocumentID);
                        //int edineID = switchBericht.Save_Edine(inboxID, receiver.ReceiverID, source.SenderID, DateTime.Now, header.DocumentID, "UTILMD", "E07");
                        portaal_MeteringPoint = (nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP)response.Portaal_Content[i];

                        Int64 SenderEAN = Int64.Parse(source.SenderID.ToString());

                        Masterdata md = new Masterdata(portaal_MeteringPoint, Bericht_ID, blnBatch, SenderEAN);
                        //ProcessMessage.processMessage(Bericht_ID, KC.ConnString);
                    }

                    Boolean success = false;
                    string strError_Message = "";
                    VerwerkCar verwerkCar = new VerwerkCar(Bericht_ID, out success, out strError_Message, KC.ConnString);
                    if (!success)
                    {
                        strError += " Error VerwerkCar: " + strError_Message;
                    }
                }
                else
                {
                    File.Delete(KC.XMLPath + @"MasterDataUpdate" + BestandsAanvulling + ".xml");
                }

            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                strError = KC.CarUrl + " " + ex.Detail.InnerXml;
                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                if (blnBatch)
                {
                    carShared.SchrijfLog("Fout MasterdataUpdate: " + "- " + KC.CarUrl + S.ErrorDetails + " " + S.ErrorText, 10, -1, KC.App_ID);
                    //WriteLog("Fout MasterdataUpdate: " + S.ErrorDetails + " " + S.ErrorText, 10, 0);
                }
                else
                {
                    MessageBox.Show(S.ErrorCode.ToString());
                    MessageBox.Show(S.ErrorDetails);
                    MessageBox.Show(S.ErrorText);
                    MessageBox.Show(ex.Detail.InnerXml.ToString());
                }
            }
            catch (WebException exception)
            {
                strError = KC.CarUrl + " " + exception.Message;
                if (blnBatch)
                {
                    carShared.SchrijfLog("Fout MasterdataUpdate: " + "- " + KC.CarUrl + exception.Message, 10, -1, KC.App_ID);
                    //WriteLog("Fout MasterdataUpdate: " + exception.Message, 10, 0);
                }
                else
                {
                    MessageBox.Show(exception.Message);
                }
            }
            catch (Exception exception)
            {
                strError = KC.CarUrl + " " + exception.Message;
                if (blnBatch)
                {
                    //WriteLog("Fout MasterdataUpdate: " + exception.Message, 10, 0);
                    carShared.SchrijfLog("Fout MasterdataUpdate: " + "- " + KC.CarUrl + exception.Message, 10, -1, KC.App_ID);
                }
                else
                {
                    MessageBox.Show(exception.Message);
                }
            }
            return strError;
        }

        public String RequestMasterdataGain(Boolean blnPV, Boolean blnBatch, out int nrRecords)
        {
            string strError = "Start";
            nrRecords = 0;

            //Boolean blnData = false;
            nl.Energie.EDSN.MasterdataGain.MasterDataGainRequestEnvelope enveloppe = new nl.Energie.EDSN.MasterdataGain.MasterDataGainRequestEnvelope();

            nl.Energie.EDSN.MasterdataGain.MasterDataGainRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MasterdataGain.MasterDataGainRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MasterdataGain.MasterDataGainRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MasterdataGain.MasterDataGainRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MasterdataGain.MasterDataGainRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MasterdataGain.MasterDataGainRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.MasterdataGain.MasterDataGainRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MasterdataGain.MasterDataGainRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            if (blnPV != true) { source.SenderID = KC.HoofdLV.ToString(); } else { source.SenderID = KC.HoofdPV.ToString(); }
            //source.SenderID = "8712423014381";
            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            header.Source = source;

            strError = source.SenderID;

            nl.Energie.EDSN.MasterdataGain.MasterDataGainRequestEnvelope_PC portaal_content = new nl.Energie.EDSN.MasterdataGain.MasterDataGainRequestEnvelope_PC();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.MasterdataGain.MasterDataGain masterDataGain = new nl.Energie.EDSN.MasterdataGain.MasterDataGain();

            strError = "Certificaat";

            if (blnPV == true)
            {
                masterDataGain.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertPV, KC.CertPVPassword));
            }
            else
            {
                masterDataGain.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
            }


            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            masterDataGain.Url = KC.CarUrl + @"batch/ResponderMasterDataGainRespondingActivity";

            masterDataGain.Timeout = 120000;

            nl.Energie.EDSN.MasterdataGain.MasterDataGainResponseEnvelope response = new nl.Energie.EDSN.MasterdataGain.MasterDataGainResponseEnvelope();

            strError = KC.XMLPath;

            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterdataGain.MasterDataGainRequestEnvelope));
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"MasterDataGain" + BestandsAanvulling + ".xml");
            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();

            string ftpResponse = "";

            if (KC.FTPServer != "")
            {
                if (FTPClass.FtpSendFile(KC.FTPServer + @"MasterDataGain" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"MasterDataGain" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                {
                    if (!blnBatch) { MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse); }
                }
            }

            strError = "Na ftp";

            try
            {
                BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }

                //Tijdelijk

                strError = "Voor call " + KC.XMLPath;
                response = masterDataGain.CallMasterDataGain(enveloppe);
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterdataGain.MasterDataGainResponseEnvelope));
                //Save to file kan weg
                WriteFileStream = new StreamWriter(KC.XMLPath + @"MasterDataGainResult" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, response);
                WriteFileStream.Close();
                strError = "Na afroep";
                ftpResponse = "";
                if (blnBatch != true && response.Portaal_Content.Length > 0)
                {
                    if (FTPClass.FtpSendFile(KC.FTPServer + @"MasterDataGainResult" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"MasterDataGainResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    {
                        if (!blnBatch) { MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); }
                    }
                }

                //Tijdelijk

                //response = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope();

                //XmlDocument _Doc = new XmlDocument();
                //_Doc.Load(@"C:\tmp\opnieuw\MasterDataUpdateResult LV Test1.xml");
                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
                //response = (nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));


                //Save to String
                //StringWriter swXML = new StringWriter();
                //serializer.Serialize(swXML, response);



                //XmlDocument _Doc = new XmlDocument();
                //_Doc.Load(path + @"MasterDataUpdateResult PV 121816 927400647.xml");
                ////_Doc.Load(@"\\venus\office\AppEnergie\XMLOutput\MasterDataUpdateResult PV 092613 95149.xml");
                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
                //response = (nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

                strError = "Voor car " + KC.XMLPath; ;
                CarShared car = new CarShared();


                if (response.Portaal_Content.Length > 0)
                {
                    nrRecords = response.Portaal_Content.Length;
                    nl.Energie.EDSN.MasterdataGain.MasterDataGainResponseEnvelope_PC_PMP portaal_MeteringPoint = new nl.Energie.EDSN.MasterdataGain.MasterDataGainResponseEnvelope_PC_PMP();
                    int Bericht_ID = car.Save_Bericht(6, @"MasterDataUpdateResult" + BestandsAanvulling + ".xml", "Masterdata : " + header.DocumentID, true, response.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);
                    for (int i = 0; i < response.Portaal_Content.Length; i++)
                    {
                        //blnData = true;

                        //int inboxID = switchBericht.Save_Inbox(26, swXML.ToString(), "Masterdata : " + header.DocumentID);
                        //int edineID = switchBericht.Save_Edine(inboxID, receiver.ReceiverID, source.SenderID, DateTime.Now, header.DocumentID, "UTILMD", "E07");
                        portaal_MeteringPoint = (nl.Energie.EDSN.MasterdataGain.MasterDataGainResponseEnvelope_PC_PMP)response.Portaal_Content[i];

                        Int64 SenderEAN = Int64.Parse(source.SenderID.ToString());

                        MasterdataGain md = new MasterdataGain(portaal_MeteringPoint, Bericht_ID, blnBatch, SenderEAN);
                        //ProcessMessage.processMessage(Bericht_ID, KC.ConnString);
                    }

                    Boolean success = false;
                    string strError_Message = "";
                    VerwerkCar verwerkCar = new VerwerkCar(Bericht_ID, out success, out strError_Message, KC.ConnString);
                    if (!success)
                    {
                        strError += " Error VerwerkCar: " + strError_Message;
                    }
                }
                else
                {
                    File.Delete(KC.XMLPath + @"MasterDataGain" + BestandsAanvulling + ".xml");
                }

            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                strError = KC.CarUrl + " " + ex.Detail.InnerXml;
                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                if (blnBatch)
                {
                    carShared.SchrijfLog("Fout MasterdataGain: " + "- " + KC.CarUrl + S.ErrorDetails + " " + S.ErrorText, 10, -1, KC.App_ID);
                    //WriteLog("Fout MasterdataUpdate: " + S.ErrorDetails + " " + S.ErrorText, 10, 0);
                }
                else
                {
                    MessageBox.Show(S.ErrorCode.ToString());
                    MessageBox.Show(S.ErrorDetails);
                    MessageBox.Show(S.ErrorText);
                    MessageBox.Show(ex.Detail.InnerXml.ToString());
                }
            }
            catch (WebException exception)
            {
                strError = KC.CarUrl + " " + exception.Message;
                if (blnBatch)
                {
                    carShared.SchrijfLog("Fout MasterdataGain: " + "- " + KC.CarUrl + exception.Message, 10, -1, KC.App_ID);
                    //WriteLog("Fout MasterdataUpdate: " + exception.Message, 10, 0);
                }
                else
                {
                    MessageBox.Show(exception.Message);
                }
            }
            catch (Exception exception)
            {
                strError = KC.CarUrl + " " + exception.Message;
                if (blnBatch)
                {
                    //WriteLog("Fout MasterdataUpdate: " + exception.Message, 10, 0);
                    carShared.SchrijfLog("Fout MasterdataGain: " + "- " + KC.CarUrl + exception.Message, 10, -1, KC.App_ID);
                }
                else
                {
                    MessageBox.Show(exception.Message);
                }
            }
            return strError;
        }

        public nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope SearchMeteringPointMP(Boolean blnPV, string ean,
            string postcode, string huisnummer, string huisnummerToevoeging, string straat, string plaats, string tntPerceelnr, string bagID,
            string bagGebouw, int intProduct, out String strError)
        {
            strError = "0";
            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope retour = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope();

            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope enveloppe = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            strError = "1";
            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;
            strError = "2";
            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;
            strError = "3";
            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;
            strError = "4";
            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            if (blnPV != true) { source.SenderID = KC.HoofdLV.ToString(); } else { source.SenderID = KC.HoofdPV.ToString(); }
            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            header.Source = source;
            strError = "5";
            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_PC portaal_Content = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_PC();

            enveloppe.Portaal_Content = portaal_Content;

            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_PC_PMP meteringPoint = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_PC_PMP();
            strError = "6";
            if (ean.Trim() != "") { meteringPoint.EANID = ean; }
            else
            {
                strError = "7";
                if (postcode.Trim() != "" || huisnummer.Trim() != "" || huisnummerToevoeging != "" || straat != "" || plaats != "" ||
                    tntPerceelnr != "" || bagID != "")
                {
                    nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_MPAddressRequestType adress = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_MPAddressRequestType();
                    if (postcode.Trim() != "") { adress.ZIPCode = postcode; }
                    if (huisnummer.Trim() != "") { adress.BuildingNr = huisnummer; }
                    if (straat != "") { adress.StreetName = straat; }
                    if (huisnummerToevoeging != "") { adress.ExBuildingNr = huisnummerToevoeging; }
                    if (plaats != "") { adress.CityName = plaats; }
                    if (bagID.Trim() != "" && bagGebouw.Trim() != "")
                    {
                        nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_BAGType bag = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope_BAGType();
                        bag.BAGID = bagID;
                        bag.BAGBuildingID = bagGebouw;
                        adress.BAG = bag;
                    }
                    meteringPoint.EDSN_AddressSearch = adress;
                    strError = "8.1";
                }

            }
            strError = "9";
            portaal_Content.Portaal_MeteringPoint = meteringPoint;
            strError = "10";
            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsMP masterData = new nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsMP();

            if (blnPV == true)
            {
                //certPath = certpath + @"EDSN2013053100007.p12";
                masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertPV, KC.CertPVPassword));
            }
            else
            {
                masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
            }
            strError = "11";
            if (!File.Exists(KC.CertLV)) { throw new Exception(strError); }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            masterData.Url = KC.CarUrl + @"synchroon/ResponderSearchMeteringPointsMPRespondingActivity";
            strError = "12";
            masterData.Timeout = 120000;

            strError = "12";
            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsRequestEnvelope));
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"SearchMeteringInformationRequest" + BestandsAanvulling + ".xml");
            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();
            strError = "13";

   //       FTP naar DENIT verwijderd jb 11-4-2019
   //         string ftpResponse = "";
   //         if (FTPClass.FtpSendFile(KC.FTPServer + @"SearchMeteringInformationResponse" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"SearchMeteringInformationResponse" + BestandsAanvulling + ".xml", out ftpResponse) == false)
   //         {
   //            //MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse); 
   //        }
            
            try
            {
                BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }

                //Tijdelijk

                retour = masterData.SearchMeteringPoints(enveloppe);
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope));
                //Save to file kan weg
                WriteFileStream = new StreamWriter(KC.XMLPath + @"SearchMeteringInformationResponse" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, retour);
                WriteFileStream.Close();
                strError = "";

                //ftpResponse = "";
                //if (FTPClass.FtpSendFile(KC.FTPServer + @"SearchMeteringInformationResult" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"SearchMeteringInformationResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                //{
                //    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                //}


                //Save to String
                //StringWriter swXML = new StringWriter();
                //serializer.Serialize(swXML, retour);
                ////Tijdelijk

                ////BestandsAanvulling = " PV 030514 143926";
                //XmlDocument _Doc = new XmlDocument();
                //_Doc.Load(path + @"SearchMeteringInformationResult" + BestandsAanvulling + ".xml");

                //nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsResponseEnvelope_Portaal_Content portaal_Response = retour.Portaal_Content;

                //nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsResponseEnvelope_Portaal_Content_Result_Portaal_MeteringPoint portaal_MeteringPointResponse = (nl.Energie.EDSN.MeteringPointInformation.SearchMeteringPointsResponseEnvelope_Portaal_Content_Result_Portaal_MeteringPoint)portaal_Response.Item;

                //MessageBox.Show("Ean : " + portaal_MeteringPointResponse.EANID.ToString());
                //+ "\r\nAdres : " + portaal_MeteringPointResponse.EDSN_AddressExtended.StreetName + " " + portaal_MeteringPointResponse.EDSN_AddressExtended.BuildingNr +
                //    "\r\nPlaats : " + portaal_MeteringPointResponse.EDSN_AddressExtended.ZIPCode.ToString() + " " + portaal_MeteringPointResponse.EDSN_AddressExtended.CityName +
                //    "\r\nNetgebied : " + portaal_MeteringPointResponse.GridArea + "\r\nNetbeheerder : " + portaal_MeteringPointResponse.GridOperator_Company.ID.ToString());

            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                strError += ex.Detail.InnerXml;
                //MessageBox.Show(S.ErrorCode.ToString());
                //MessageBox.Show(S.ErrorDetails);
                //MessageBox.Show(S.ErrorText);
                //MessageBox.Show(ex.Detail.InnerXml.ToString());
            }
            catch (WebException exception)
            {
                //MessageBox.Show(exception.Message);
                strError += exception.Message;
            }
            catch (Exception exception)
            {
                //MessageBox.Show(exception.Message);
                strError += exception.Message;
            }
            return retour;
        }

        public nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope RequestGetMeteringPointMP(string eancode, Boolean blnPV, string strFileName, Boolean blnBatch, Boolean blnToFile)
        {
            string strResult = "FAILED";
            string OldFileName = strFileName;

            nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope enveloppe = new nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope();
            nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
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
            source.SenderID = KC.HoofdLV.ToString();
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

            getMeteringPointMP.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            getMeteringPointMP.Url = KC.CarUrl + @"/synchroon/ResponderGetMeteringPointMPRespondingActivity";

            getMeteringPointMP.Timeout = 120000;


            //Boolean success = true;
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointRequestEnvelope));
            TextWriter WriteFileStream;
            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                { BestandsAanvulling = " LV " + BestandsAanvulling; }

                WriteFileStream = new StreamWriter(KC.XMLPath + @"GetMeteringPointMPRequest" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();

                //string ftpResponse = "";

                //if (FTPClass.FtpSendFile(strFTPServer + @"GetMeteringPointMPRequest" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"GetMeteringPointMPRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                //{
                //    if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse); }
                //    success = false;
                //}

            }
            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                BestandsAanvulling = " LV " + BestandsAanvulling;
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();



                if (strFileName == "")
                {
                    
                    retour = getMeteringPointMP.GetMeteringPoint(enveloppe);

                    //Save to file kan weg
                    string responsefile = "GetMeteringPointMPResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml";
                    WriteFileStream = new StreamWriter(KC.XMLPath + responsefile);
                    //WriteFileStream = new StreamWriter(path + @"MasterDataResponse" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    //string ftpResponse = "";

                    //if (FTPClass.FtpSendFile(strFTPServer + responsefile, strFTPUser, strFTPPassword, path + responsefile, out ftpResponse) == false)
                    //{
                    //    if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar Denit " + ftpResponse); }
                    //    success = false;
                    //}


                    //Save to String
                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);

                    //ftpResponse = "";
                    //if (Klant_Config == "ROBIN" && strTest != "JA")
                    //{
                    //    if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "GetMeteringPointMPResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + responsefile, out ftpResponse) == false)
                    //    {
                    //        if (blnBatch == false) { MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse); }
                    //        success = false;
                    //    }
                    //}

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

                carShared.SchrijfLog("Fout bij GetMeteringMP : + " + eancode + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, 0, KC.App_ID);

            }
            catch (WebException exception)
            {
                carShared.SchrijfLog("Fout bij GetMeteringMP : + " + eancode + " - " + exception.Message, 10, 0, KC.App_ID);
                strResult = "Fout bij GetMeteringMP : + " + eancode + " - " + exception.Message;
            }
            catch (Exception exception)
            {
                carShared.SchrijfLog("Fout bij GetMeteringMP : + " + eancode + " - " + exception.Message, 10, 0, KC.App_ID);
                strResult = "Fout bij GetMeteringMP : + " + eancode + " - " + exception.Message;
            }


            return retour;
        }

        public string RequestMPInformation(string eancode, string strKey, string strBirthDay, string strIBAN, string strFileName, Boolean blnBatch, Boolean blnToFile)
        {
            string strResult = "FAILED";
            string OldFileName = strFileName;

            nl.Energie.EDSN.GetMPInformation.GetMPInformationRequestEnvelope enveloppe = new nl.Energie.EDSN.GetMPInformation.GetMPInformationRequestEnvelope();
            nl.Energie.EDSN.GetMPInformation.EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.GetMPInformation.EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
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
            source.SenderID = KC.HoofdLV.ToString();
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
            pui.Organisation = KC.HoofdLV.ToString();

            nl.Energie.EDSN.GetMPInformation.GetMPInformation mpInformation = new nl.Energie.EDSN.GetMPInformation.GetMPInformation();
            nl.Energie.EDSN.GetMPInformation.GetMPInformationResponseEnvelope retour = new nl.Energie.EDSN.GetMPInformation.GetMPInformationResponseEnvelope();

            mpInformation.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            mpInformation.Url = KC.CarUrl + @"/synchroon/ResponderGetMPInformationRespondingActivity";

            mpInformation.Timeout = 120000;


            //Boolean success = true;
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope));
            //TextWriter WriteFileStream;
            //if (strFileName == "")
            //{
            //    string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            //    { BestandsAanvulling = " LV " + BestandsAanvulling; }

            //    WriteFileStream = new StreamWriter(KC.XMLPath + @"mpInformationRequest" + BestandsAanvulling + ".xml");
            //    serializer.Serialize(WriteFileStream, enveloppe);
            //    WriteFileStream.Close();

               
            //}
            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                BestandsAanvulling = " LV " + BestandsAanvulling;
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();



                if (strFileName == "")
                {
                    
                        retour = mpInformation.GetMPInformationRequest(enveloppe);

                    //Save to file kan weg
                    //string responsefile = "mpInformationResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml";
                    //WriteFileStream = new StreamWriter(KC.XMLPath + responsefile);
                    ////WriteFileStream = new StreamWriter(path + @"MasterDataResponse" + BestandsAanvulling + ".xml");
                    //serializer.Serialize(WriteFileStream, retour);
                    //WriteFileStream.Close();

                    //string ftpResponse = "";

                   


                    //Save to String
                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);

                    
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
                
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                carShared.SchrijfLog("Fout bij GetMeteringMP : + " + eancode + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, 0, KC.App_ID);

            }
            catch (WebException exception)
            {
                carShared.SchrijfLog("Fout bij GetMeteringMP : + " + eancode + " - " + exception.Message, 10, 0, KC.App_ID);
                strResult = "Fout bij GetMeteringMP : + " + eancode + " - " + exception.Message;
            }
            catch (Exception exception)
            {
                carShared.SchrijfLog("Fout bij GetMeteringMP : + " + eancode + " - " + exception.Message, 10, 0, KC.App_ID);
                strResult = "Fout bij GetMeteringMP : + " + eancode + " - " + exception.Message;
            }


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
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
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
            source.SenderID = KC.HoofdLV.ToString();
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

            getMeteringPointMP.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            getMeteringPointMP.Url = KC.CarUrl + @"/synchroon/ResponderGetSCMPInformationRespondingActivity";

            getMeteringPointMP.Timeout = 120000;


            //Boolean success = true;
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationRequestEnvelope));
            //TextWriter WriteFileStream;
            //if (strFileName == "")
            //{
            //    string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            //    { BestandsAanvulling = " LV " + BestandsAanvulling; }

            //    WriteFileStream = new StreamWriter(KC.XMLPath + @"GetSCMPInformationRequest" + BestandsAanvulling + ".xml");
            //    serializer.Serialize(WriteFileStream, enveloppe);
            //    WriteFileStream.Close();

                

            //}
            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                BestandsAanvulling = " LV " + BestandsAanvulling;
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.GetSCMPInformation.GetSCMPInformationResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();



                if (strFileName == "")
                {
                    
                    retour = getMeteringPointMP.CallGetSCMPInformation(enveloppe);
                    

                    //Save to file kan weg
                    //string responsefile = "GetSCMPInformationResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml";
                    //WriteFileStream = new StreamWriter(KC.XMLPath + responsefile);
                    ////WriteFileStream = new StreamWriter(path + @"MasterDataResponse" + BestandsAanvulling + ".xml");
                    //serializer.Serialize(WriteFileStream, retour);
                    //WriteFileStream.Close();

                    


                    //Save to String
                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);

                   
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

                

                if (strFileName != "")
                {
                    File.Delete(strFileName);
                    strFileName = OldFileName;
                }
                
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                carShared.SchrijfLog("Fout bij GetSCMPInformation : + " + eancode + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, 0, KC.App_ID);

            }
            catch (WebException exception)
            {
                carShared.SchrijfLog("Fout bij GetSCMPInformation : + " + eancode + " - " + exception.Message, 10, 0, KC.App_ID);
                strResult = "Fout bij GetSCMPInformation : + " + eancode + " - " + exception.Message;
            }
            catch (Exception exception)
            {
                carShared.SchrijfLog("Fout bij GetSCMPInformation : + " + eancode + " - " + exception.Message, 10, 0, KC.App_ID);
                strResult = "Fout bij GetSCMPInformation : + " + eancode + " - " + exception.Message;
            }


            return strResult;
        }

        public string ToevoegenCK(string ean, string geboortedatum, string iban)
        {
            string strResult = "FAILED";

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope enveloppe = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
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
            source.SenderID = KC.HoofdLV.ToString();
            source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC portaal_Content = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC();

            enveloppe.Portaal_Content = portaal_Content;

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP meteringPoint = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP();

            meteringPoint.EANID = ean;

            portaal_Content.Portaal_MeteringPoint = meteringPoint;

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC mpcc = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC();

            meteringPoint.MPCommercialCharacteristics = mpcc;

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company bsp = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company();
            bsp.ID = KC.HoofdLV.ToString();
            mpcc.BalanceSupplier_Company = bsp;

            nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC_GridContractParty gcp = new nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope_PC_PMP_MPCC_GridContractParty();
            gcp.BirthDayKey = geboortedatum;
            gcp.IBANKey = iban;
            mpcc.GridContractParty = gcp;

            nl.Energie.EDSN.CKIdentification.CKIdentification ckidentification = new nl.Energie.EDSN.CKIdentification.CKIdentification();

            ckidentification.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            ckidentification.Url = KC.CarUrl + @"synchroon/ResponderCKIdentificationRespondingActivity";

            ckidentification.Timeout = 120000;

            nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope retour = new nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope();

            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.CKIdentification.CreateCKRequestEnvelope));
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"CKIdentificationResponse" + BestandsAanvulling + ".xml");
            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();


            try
            {
                BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                BestandsAanvulling = " LV " + BestandsAanvulling;

                //Tijdelijk

                retour = ckidentification.CreateCKRequest(enveloppe);
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope));
                //Save to file kan weg
                WriteFileStream = new StreamWriter(KC.XMLPath + @"CKIdentificationResult" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, retour);
                WriteFileStream.Close();

                if (retour.Portaal_Content.GetType() != typeof(nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope_RejectionPortaalType))
                {
                    strResult = "OK";
                }
                else
                {
                    nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope_RejectionPortaalType rejection = new nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope_RejectionPortaalType();
                    //rejection = (nl.Energie.EDSN.CKIdentification.CreateCKResponseEnvelope_RejectionPortaalType)retour.Portaal_Content;
                    //TODO rejection uitlezen
                    strResult = "FAILED";
                }

            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                carShared.SchrijfLog("Fout bij CKIdentification : + " + ean + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, 0, KC.App_ID);

            }
            catch (WebException exception)
            {
                carShared.SchrijfLog("Fout bij CKIdentification : + " + ean + " - " + exception.Message, 10, 0, KC.App_ID);
                strResult = "Fout bij GetSCMPInformation : + " + ean + " - " + exception.Message;
            }
            catch (Exception exception)
            {
                carShared.SchrijfLog("Fout bij CKIdentification : + " + ean + " - " + exception.Message, 10, 0, KC.App_ID);
                strResult = "Fout bij GetSCMPInformation : + " + ean + " - " + exception.Message;
            }
            return strResult;
        }

        public Boolean RequestMasterdata(string ean18, string netbeheerder, Boolean blnPV, string strFileName, Boolean blnBatch)
        {
            Boolean success = true;
            string OldFileName = strFileName;
           
            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope enveloppe = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            if (blnPV != true) { source.SenderID = KC.HoofdLV.ToString(); } else { source.SenderID = KC.HoofdPV.ToString(); }
            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint meteringpoint = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint();

            meteringpoint.EANID = ean18;
            portaal_content.Portaal_MeteringPoint = meteringpoint;

            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company();
            meteringpointGridOperator.ID = netbeheerder;
            meteringpoint.GridOperator_Company = meteringpointGridOperator;


            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation meteringpointPortaalMutation = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation();
            meteringpoint.Portaal_Mutation = meteringpointPortaalMutation;

            nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation_Portaal_UserInformation mutationUserInfo = new nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation_Portaal_UserInformation();
            if (blnPV != true) { mutationUserInfo.Organisation = KC.HoofdLV.ToString(); } else { mutationUserInfo.Organisation = KC.HoofdPV.ToString(); }
            //mutationUserInfo.Organisation = "8712423014381";
            meteringpointPortaalMutation.Portaal_UserInformation = mutationUserInfo;

            nl.Energie.EDSN.MasterData.MasterData masterData = new nl.Energie.EDSN.MasterData.MasterData();

            //String certPath = @"c:\test\EDSN2013010300005.p12";
            //masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath, "T81SKoL#6D"));

            //String certPath = certpath + @"EDSN2013053100006.p12";
            if (strFileName == "")
            {
                if (blnPV == true)
                {
                    //certPath = certpath + @"EDSN2013053100007.p12";
                    masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertPV, KC.CertPVPassword));
                }
                else
                {
                    masterData.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
                }



                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            }

            masterData.Url = KC.CarUrl + @"synchroon/ResponderMasterDataRespondingActivity";

            masterData.Timeout = 120000;

            nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope retour = new nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope();


            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataRequestEnvelope));
            TextWriter WriteFileStream;
            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                { BestandsAanvulling = " LV " + BestandsAanvulling; }

                WriteFileStream = new StreamWriter(KC.XMLPath + @"MasterDataRequest" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();

                

            }
            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();



                if (strFileName == "")
                {
                    
                    retour = masterData.CallMasterData(enveloppe);

                    //Save to file kan weg
                    string responsefile = "MasterDataResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml";
                    WriteFileStream = new StreamWriter(KC.XMLPath + responsefile);
                    //WriteFileStream = new StreamWriter(path + @"MasterDataResponse" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                   


                    //Save to String
                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);

                   
                }
                else
                {

                    retour = new nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope();

                    _Doc.Load(strFileName);
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope));
                    retour = (nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
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
            catch //(WebException exception)
            {
                if (blnBatch == false)
                {
                    //MessageBox.Show(exception.Message);
                }
                success = false;
            }
            //catch (Exception exception)
            //{
            //    if (blnBatch == false)
            //    {
            //        //MessageBox.Show(exception.Message);
            //    }
            //    success = false;
            //}
            return success;
        }
    }
}
