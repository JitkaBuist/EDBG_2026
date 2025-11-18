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
    public class SwitchClient
    {
        //private string strSql;
        //private static String ConnString = "";
        //private string urlWebService = Energie.DataAccess.Configurations.GetApplicationSetting("PORTAAL");//"https://emp.edsn.nl/b2b";
        private CarShared carShared;


        private string DossierID = "";
        protected string strError_Message = "";

        private AppID AppID = AppID.Car;

        public SwitchClient(string klantConfig)
        {
            KC.KlantConfig = klantConfig;
            KC.ConnStringPortaal = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");

            carShared = new CarShared();
        }



        public Boolean GetGains(Boolean blnPV, string strFileName, Boolean blnBatch, out int nrRecords)
        {
            Boolean blnOK = false;
            nrRecords = 0;

            nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope enveloppe = new nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope();

            nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            if (blnPV != true) { source.SenderID = KC.HoofdLV.ToString(); } else { source.SenderID = KC.HoofdPV.ToString(); }
            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            header.Source = source;

            nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_Portaal_Content portaal_Content = new nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_Content;

            nl.Energie.EDSN.LossGainRejectUpdate.LossGainRejectUpdate gainRequest = new nl.Energie.EDSN.LossGainRejectUpdate.LossGainRejectUpdate();

            if (blnPV == true)
            {
                gainRequest.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertPV, KC.CertPVPassword));
            }
            else
            {
                gainRequest.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
            }

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            gainRequest.Url = KC.CarUrl + @"synchroon/ResponderLossGainRejectUpdateRespondingActivity";

            gainRequest.Timeout = 120000;

            nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope response = new nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope();

            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope));
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"LossGainRejectUpdate" + BestandsAanvulling + ".xml");
            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();

            string ftpResponse = "";

            if (FTPClass.FtpSendFile(KC.FTPServer + @"LossGainRejectUpdate" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"LossGainRejectUpdate" + BestandsAanvulling + ".xml", out ftpResponse) == false)
            {
                //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
            }

            try
            {
                if (strFileName == "")
                {
                    response = gainRequest.GainResult(enveloppe);
                }
                else
                {
                    XmlDocument _Doc = new XmlDocument();
                    _Doc.Load(strFileName);

                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope));
                    response = (nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
                }
                try
                {
                    BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                    if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope));
                    //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope));
                    //Save to file kan weg
                    WriteFileStream = new StreamWriter(KC.XMLPath + @"GainResult" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, response);
                    WriteFileStream.Close();

                    ftpResponse = "";
                    if (blnBatch != true && response.Portaal_Content.Length > 0)
                        if (FTPClass.FtpSendFile(KC.FTPServer + @"GainResult" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"GainResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                        {
                            //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                        }


                    blnOK = true;


                    //Save to String
                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, response);
                }
                catch { }

                //XmlDocument _Doc = new XmlDocument();
                //_Doc.Load(path + "GainResultReRun.xml");
                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope));
                //response = (nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));



                if (response.Portaal_Content.Length > 0)
                {
                    nrRecords = response.Portaal_Content.Length;
                    CarShared car = new CarShared();
                    int Bericht_ID = car.Save_Bericht(26, @"GainUpdateResult" + BestandsAanvulling + ".xml", "Gains : " + header.DocumentID, true, response.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);

                    nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint[] responseItems = (nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint[])response.Portaal_Content;

                    foreach (nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem in responseItems)
                    {
                        Car.Gain gain = new Gain(responseItem, Bericht_ID, source.SenderID);
                       //Verwerken bericht naar EnergieDB
                            
                    }

                    Boolean success = false;
                    string strError = "";
                    VerwerkCar verwerkCar = new VerwerkCar(Bericht_ID, out success, out strError, KC.ConnString);
                    if (!success)
                    {
                        strError_Message += " Error VerwerkCar: " + strError;
                    }
                }
                else
                {
                    //File.Delete(KC.XMLPath + @"GainResult" + BestandsAanvulling + ".xml");
                    //File.Delete(KC.XMLPath + @"LossGainRejectUpdate" + BestandsAanvulling + ".xml");
                }

                //EDSN_MoveIn.MoveInResponseEnvelope_Portaal_Content portaalResponse = response.Portaal_Content;
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
                blnOK = true;
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                if (KC.KlantConfig == "" && blnBatch)
                {
                    carShared.SchrijfLog("Fout Gains :" + S.ErrorDetails + " " + S.ErrorText, 10, -1, KC.App_ID);
                }
                else
                {
                    if (!blnBatch)
                    {
                        MessageBox.Show(S.ErrorCode.ToString());
                        MessageBox.Show(S.ErrorDetails);
                        MessageBox.Show(S.ErrorText);
                        MessageBox.Show(ex.Detail.InnerXml.ToString());
                    }
                }
            }
            catch (WebException exception)
            {
                if (KC.KlantConfig == "" && blnBatch)
                {
                    carShared.SchrijfLog("Fout Gains :" + exception.Message, 10, -1, KC.App_ID);
                }
                else
                {
                    if (!blnBatch)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }
            }
            catch (Exception exception)
            {
                if (KC.KlantConfig == "" && blnBatch)
                {
                    carShared.SchrijfLog("Fout Gains :" + exception.Message, 10, -1, KC.App_ID);
                }
                else
                {
                    if (!blnBatch)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }
            }

            return blnOK;
        }

        public Boolean GetLoss(Boolean blnPV, string strFileName, Boolean blnBatch, out int nrRecords)
        {
            nrRecords = 0;
            Boolean blnOK = false;

            nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope enveloppe = new nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope();

            nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            if (blnPV != true) { source.SenderID = KC.HoofdLV.ToString(); } else { source.SenderID = KC.HoofdPV.ToString(); }
            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            header.Source = source;

            nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_Portaal_Content portaal_Content = new nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_Content;

            nl.Energie.EDSN.LossGainRejectUpdate.LossGainRejectUpdate lossRequest = new nl.Energie.EDSN.LossGainRejectUpdate.LossGainRejectUpdate();

            if (blnPV == true)
            {
                lossRequest.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertPV, KC.CertPVPassword));
            }
            else
            {
                lossRequest.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
            }

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            lossRequest.Url = KC.CarUrl + @"synchroon/ResponderLossGainRejectUpdateRespondingActivity";


            lossRequest.Timeout = 120000;

            nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope response = new nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope();

            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope));
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"LossGainRejectUpdate" + BestandsAanvulling + ".xml");
            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();

            string ftpResponse = "";
            if (KC.KlantConfig != "")
            {
                if (FTPClass.FtpSendFile(KC.FTPServer + @"LossGainRejectUpdate" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"LossGainRejectUpdate" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                {
                    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                }
            }

            try
            {
                if (strFileName == "")
                {
                    response = lossRequest.LossResult(enveloppe);

                }
                else
                {
                    XmlDocument _Doc = new XmlDocument();
                    _Doc.Load(strFileName);

                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope));
                    response = (nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
                }
                try
                {
                    BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                    if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }
                    //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope));
                    //Save to file kan weg
                    WriteFileStream = new StreamWriter(KC.XMLPath + @"LossResult" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, response);
                    WriteFileStream.Close();

                    ftpResponse = "";
                    if (KC.FTPServer != "")
                    {
                        if (blnBatch != true && response.Portaal_Content.Length > 0)
                            if (FTPClass.FtpSendFile(KC.FTPServer + @"LossResult" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"LossResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                            {
                                //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                            }
                    }
                    ftpResponse = "";

                    
                    //Save to String
                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, response);
                }
                catch { }

                //XmlDocument _Doc = new XmlDocument();
                //_Doc.Load(path + "LossResult LV 121813 95616.xml");
                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope));
                //response = (nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));


                CarShared car = new CarShared();
                
           
                if (response.Portaal_Content.Length > 0)
                {
                    nrRecords = response.Portaal_Content.Length;
                    int Bericht_ID = car.Save_Bericht(7, @"LossUpdateResult" + BestandsAanvulling + ".xml", "Loss : " + header.DocumentID, true, response.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);

                    nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint[] responseItems = (nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint[])response.Portaal_Content;

                    

                    foreach (nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem in responseItems)
                    {
                        Car.Loss loss = new Loss(responseItem, Bericht_ID, source.SenderID);
                    }

                    Boolean success = false;
                    string strErrort = "";
                    VerwerkCar verwerkCar = new VerwerkCar(Bericht_ID, out success, out strErrort, KC.ConnString);
                    if (!success)
                    {
                        strError_Message += " Error VerwerkCar: " + strErrort;
                    }
                }
                else
                {
                    //File.Delete(KC.XMLPath + @"LossResult" + BestandsAanvulling + ".xml");
                    //File.Delete(KC.XMLPath + @"LossGainRejectUpdate" + BestandsAanvulling + ".xml");
                }

                blnOK = true;
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                if (blnBatch || KC.KlantConfig != "")
                {
                    carShared.SchrijfLog("Fout Losses " + S.ErrorDetails + " " + S.ErrorText, 10, -1, KC.App_ID);
                }
                else
                {
                    if (!blnBatch)
                    {
                        MessageBox.Show(S.ErrorCode.ToString());
                        MessageBox.Show(S.ErrorDetails);
                        MessageBox.Show(S.ErrorText);
                        MessageBox.Show(ex.Detail.InnerXml.ToString());
                    }
                }
            }
            catch (WebException exception)
            {

                if (blnBatch || KC.KlantConfig != "")
                {
                    carShared.SchrijfLog("Fout Losses " + exception.Message, 10, -1, KC.App_ID);
                }
                else
                {
                    if (!blnBatch)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }
            }
            catch (Exception exception)
            {

                if (blnBatch || KC.KlantConfig != "")
                {
                    carShared.SchrijfLog("Fout Losses " + exception.Message, 10, -1, KC.App_ID);
                }
                else
                {
                    if (!blnBatch)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }
            }

            return blnOK;
        }

        public Boolean SwitchWebService(Boolean blnBatch, out string pDossierID, out string strError, Boolean blnToFile, int aansluitingID)
        {
            //if (blnToFile == true)
            //{
            //    timer1.Tick += new EventHandler(timer1_Tick);
            //    timer1.Interval = 100;
            //    timer1.Enabled = true;
            //}

            Boolean blnOK = false;

            // afgeslankte vw switchberichten. 13-8-2025

            string SQLstatement = "Select           Aansluiting_ID, Ean18_code, Aansluiting_Kortenaam, Aansluiting_Type " +
                                                    ",Bericht_Code " +
                                                    ",Factuurmodel_ID " +
                                                    ",Contract_Start_DT " +
                                                    ",Verblijfsfunctie, Netbeheerder_EAN13_Code " +
                                                    ",Relatie_Straat, Relatie_Huisnummer, Relatie_Toevoeging, Relatie_Postcode, Relatie_Woonplaats " +
                                                    ", Aansluiting_Straat, Aansluiting_Huisnummer, Aansluiting_Toevoeging, Aansluiting_Postcode, Aansluiting_Woonplaats " +
                                                    ",ProductType, EnrollmentID " +
                                                    ",KvKNr, SlimmeMeterAllocatie, ISNULL(PV, 0) as PV  " +
                   "From vw_SwitchBerichten " +
                                                   "where Aansluiting_id = @aansluiting_ID and BerichtIDSwitch = 0 ";//and ProductType = 'E' "


            // afgeslankte vw switchberichten. 13-8-2025
            //                      string SQLstatement = "Select Aansluiting_ID, Ean18_code, Bericht_Code " +
            //                                              ", Aansluiting_Naam, Aansluiting_type, Factuurmodel_ID " +
            //                                              ", Contract_Start_DT,  " +
            //                                              ", Verblijfsfunctie, Netbeheerder_EAN13_Code " +
            //                                             
            //                                             ", Relatie_Straat, Relatie_Huisnummer, Relatie_Toevoeging, Relatie_Postcode, Relatie_Woonplaats " +
            //                                             ", Aansluiting_Straat, Aansluiting_Huisnummer, Aansluiting_Toevoeging, Aansluiting_Postcode, Aansluiting_Woonplaats " +
            //                                             ", ProductType, EnrollmentID  " +
            //                                              ", KvKNr, SlimmeMeterAllocatie,  ISNULL(PV, 0) as PV " +
            // //                                       "From vw_SwitchBerichten " +
            //                                       "where Aansluiting_id = @aansluiting_ID and BerichtIDSwitch = 0 ";//and ProductType = 'E' ";

            // nieuwe vw switchberichten, dus ook een nieuwe select zonder adressen ed., maar die is wel nodig, dus terug naar de oude.

            //           Invalid column name 'Netbeheerder_EAN13_Code'. in vw_switchberichten
            //Msg 207, Level 16, State 1, Line 2
            //Invalid column name 'WOZ_Omschrijving'.
            //Msg 207, Level 16, State 1, Line 2
            //Invalid column name 'Leverancier_EAN13_code'.


            //         string SQLstatement =              "Select Aansluiting_ID, Ean18_code, Bericht_Code " +
            //                                            ", Aansluiting_Naam, Aansluiting_type, Factuurmodel " +
            //                                            ", Contract_Start_DT, Beginstand_Hoog, Beginstand_Laag " +
            //                                            ", Verblijfsfunctie,  netbeheerder_EAN13_code " +                                            
            //                                           ", Meterstand_Opname_NL_DT  " +
            //                                            ", ProductType, EnrollmentID " +
            //                                            ", KvKNr, SlimmeMeterAllocatie, ISNULL(NewPV, 0) as NewPV, FactuurRelatie_ID " +
            //                                      "From vw_SwitchBerichten " +
            //                                      "where Aansluiting_id = @aansluiting_ID and BerichtIDSwitch = 0 ";//and ProductType = 'E' ";


            SqlDataAdapter da = new SqlDataAdapter(SQLstatement, KC.ConnString);
            da.SelectCommand.Parameters.Add(new SqlParameter("@aansluiting_ID", SqlDbType.Int));
            da.SelectCommand.Parameters["@aansluiting_ID"].Value = aansluitingID;
            DataTable dtGet = new DataTable();
            da.Fill(dtGet);
            DataRow dr;
            dr = dtGet.Rows[0];

            //strError = "Na Query";

            string strRequestFile = "";


            switch (dr["Bericht_Code"].ToString().Trim())
            {
                case "SWITCHLV":
                    blnOK = ChangeOfSupplier(dr, blnToFile, strRequestFile);  //else { blnOK = ChangeOfSupplierBatch(dtGet); }
                    break;
                case "MOVEIN":
                   blnOK = MoveIn(dr, blnToFile, strRequestFile);  //else { blnOK = MoveInBatch(dr); }
                    break;
                case "MOVEOUT":
                    blnOK = MoveOut(dr);
                    break;
                case "EOSUPPLY":
                    blnOK = EndOfSupply(dr, blnToFile, strRequestFile);  /*else { blnOK = EndOfSupplyBatch(dr); }*/
                    break;
                case "SWITCHPV":
                    blnOK = ChangeOfPV(dr, dr["PV"].ToString(), blnToFile, strRequestFile);
                    break;
                case "EOSNOT":
                    blnOK = EOSNotice(dr, blnToFile, strRequestFile);
                    break;
                case "NAMECH":
                    blnOK = NameChange(dr);
                    break;
                case "COAMET":
                    blnOK = ChangeOfAllocationMethod(dr, blnToFile, strRequestFile);
                    break;
            }
            pDossierID = DossierID;
            strError = strError_Message;


            return blnOK;

        }

        public Boolean ChangeOfSupplier(DataRow dr, Boolean blnToFile, string strRequestFile)
        {
            Boolean blnOK = false;
            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope enveloppe = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope();

            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader();



            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);// "EDM00010";
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            string sender = "";
            string identifier = "";
            if (dr["ProductType"].ToString() == "G")
            {
                sender = KC.HoofdLV.ToString();
                identifier = "DDQ_M";
            }
            else
            {
                sender = KC.HoofdLV.ToString();
                identifier = "DDQ_O";
            }


            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            source.SenderID = sender;
            source.ContactTypeIdentifier = identifier;
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC portaal_content = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP meteringpoint = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP();

            meteringpoint.EANID = dr["Ean18_code"].ToString();
            portaal_content.Portaal_MeteringPoint = meteringpoint;

            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_GridOperator_Company();
            meteringpointGridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();
            meteringpoint.GridOperator_Company = meteringpointGridOperator;

            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC meteringpointMPCommercial = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC();
            meteringpoint.MPCommercialCharacteristics = meteringpointMPCommercial;
            if (dr["KvKNr"].ToString() != "")
            {
                meteringpointMPCommercial.ChamberOfCommerceID = dr["KvKNr"].ToString();
            }

            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC_GridContractParty meteringpointGridContact = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC_GridContractParty();
            // Surname mag niet groter zijn dan 24 pos dus  Relatie_naam --> Kortenaam wordt max 24 pos.31-7-2025 13-8-2025
            //  Kortenaam ---> Aansluiting_Kortenaam 28-8-2025
            //portaal_MeteringPoint[1]/MPCommercialCharacteristics[1]/GridContractParty[1]/Surname[1] "xs:string ')" length must be at most 24 CHARACTERS          
            string naam = dr["Aansluiting_Kortenaam"].ToString();
            meteringpointGridContact.Surname = naam.Length > 24 ? naam.Substring(0, 24) : naam;
 //           meteringpointGridContact.Surname = dr["Factuur_KorteNaam"].ToString();
        // geboortedatum verdwijnt 13-8-2025
       //     if (!DBNull.Value.Equals(dr["Relatie_Geboortedatum"]))
       //     {
       //         meteringpointGridContact.BirthDateSpecified = true;
       //         meteringpointGridContact.BirthDate = (DateTime)dr["Relatie_Geboortedatum"];
       //     }
            //meteringpointGridContact.Initials = " ";
            meteringpointMPCommercial.GridContractParty = meteringpointGridContact;

            if (dr["ProductType"].ToString() == "G")
            {
                sender = KC.HoofdLV.ToString();
            }
            else
            {

                sender = KC.HoofdLV.ToString();
            }

            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company meteringpointBalanceSupplier = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company();
            meteringpointBalanceSupplier.ID = sender;
            meteringpointMPCommercial.BalanceSupplier_Company = meteringpointBalanceSupplier;

            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC_BalanceResponsibleParty_Company meteringpointBalanceResponsibleParty = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC_BalanceResponsibleParty_Company();
            if (dr["ProductType"].ToString() != "G") { meteringpointBalanceResponsibleParty.ID = KC.HoofdPV.ToString(); } else { meteringpointBalanceResponsibleParty.ID = "8717185189995"; };//"7614252022906"
            meteringpointMPCommercial.BalanceResponsibleParty_Company = meteringpointBalanceResponsibleParty;

            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_PM meteringpointPortaalMutation = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_PM();
            meteringpointPortaalMutation.MutationDate = (DateTime)dr["Contract_Start_DT"];
            meteringpoint.Portaal_Mutation = meteringpointPortaalMutation;


            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplier changeOfSupplier = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplier();

            //String certPath = certpath + @"EDSN2013053100006.p12";
            changeOfSupplier.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));


            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //moveIn.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderMoveInRespondingActivity";
            changeOfSupplier.Url = KC.CarUrl + @"synchroon/ResponderChangeOfSupplierRespondingActivity";

            changeOfSupplier.Timeout = 120000;

            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope retour = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope();


            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope));
            StringWriter swXML = new StringWriter();
            serializer.Serialize(swXML, enveloppe);
            //MessageBox.Show(swXML.ToString());
            //int intOutBoxID = Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "MoveIn", swXML.ToString());
            int intOutBoxID = carShared.Save_Bericht(1, swXML.ToString(), "ChangeOfPV : " + dr["Ean18_code"].ToString(), false, "", true, false);

            // change of supplier request ook wegschrijven
            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            BestandsAanvulling = " LV " + BestandsAanvulling;
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath  + @"ChangeOfSupplier" + BestandsAanvulling + ".xml");
            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();



            try
            {
               
                retour = changeOfSupplier.CallChangeOfSupplier(enveloppe);
                
                nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;

                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope));
                //Save to file kan weg
                WriteFileStream = new StreamWriter(KC.XMLPath + @"ChangeOfSupplierResponse" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, retour);
                WriteFileStream.Close();


                //string ftpResponse = "";
                //if (Klant_Config == "ROBIN" && strTest != "JA")
                //{
                //    if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "ChangeOfSupplierResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + @"ChangeOfSupplierResponse" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                //    {
                //        //MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse);
                //        WriteLog("Fout bij verzenden naar nexant ChangeOfSupplier - " + dr["Ean18_code"].ToString() + "-" + ftpResponse, 10, intOutBoxID);

                //    }
                //}

                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_Portaal_Content_Portaal_Rejection))
                {
                    nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
                    nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
                    //MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
                    //WriteLog("Fout bij  ChangeOfSupplier - " + dr["Ean18_code"].ToString() + "-" + rejectionPortaalType[0].RejectionText.ToString(), 10, intOutBoxID);
                    carShared.SchrijfLog("Fout bij  ChangeOfSupplier - " + dr["Ean18_code"].ToString() + " - " + rejectionPortaalType[0].RejectionText.ToString(), 10, intOutBoxID, AppID);
                    strError_Message = "Fout bij  ChangeOfSupplier - " + dr["Ean18_code"].ToString() + "-" + rejectionPortaalType[0].RejectionText.ToString();
                    
                }
                else
                {
                    blnOK = true;
                    nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem = (nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;


                    //414
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
                    swXML = new StringWriter();
                    serializer.Serialize(swXML, responseItem);

                    DossierID = responseItem.Portaal_Mutation.Dossier.ID;

                    int BerichtID = carShared.Save_Bericht(2, swXML.ToString(), "Move in : " + dr["Ean18_code"].ToString(), true, retour.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);

                    GainItem gainItem = new GainItem();
                    gainItem.AppID = AppID;
                    gainItem.BerichtID = BerichtID;
                    gainItem.Datum = responseItem.Portaal_Mutation.MutationDate;
                    gainItem.Dossier = responseItem.Portaal_Mutation.Dossier.ID;
                    gainItem.EAN18_Code = Int64.Parse(responseItem.EANID);
                    gainItem.LV = Int64.Parse(responseItem.MPCommercialCharacteristics.BalanceSupplier_Company.ID);
                    gainItem.PV = Int64.Parse(responseItem.MPCommercialCharacteristics.BalanceResponsibleParty_Company.ID);
                    gainItem.Reden = responseItem.Portaal_Mutation.MutationReason.ToString();
                    gainItem.Referentie = responseItem.Portaal_Mutation.ExternalReference;
                    gainItem.Ontvanger = Int64.Parse(retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID.ToString());
                    gainItem.Product = responseItem.ProductType.ToString();
                    gainItem.NetbeheerderEAN = Int64.Parse(responseItem.GridOperator_Company.ID.ToString());
                    gainItem.OudeLV = -1;
                    gainItem.Referentie = enveloppe.EDSNBusinessDocumentHeader.DocumentID;
                    if (responseItem.MPCommercialCharacteristics.OldBalanceSupplier_Company != null)
                    {
                        gainItem.OudeLV = Int64.Parse(responseItem.MPCommercialCharacteristics.OldBalanceSupplier_Company.ID.ToString());
                    }
                    carShared.SchrijfGain(gainItem);

                    ProcessMessage.processMessage(BerichtID, KC.ConnString);

                    carShared.Save_Switch((int)dr["Aansluiting_ID"], intOutBoxID, BerichtID, -1);
                    //MessageBox.Show("Accepted - Dossier " + responseItem.Portaal_Mutation.Dossier.ID);


                    blnOK = true;
                }
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                carShared.SchrijfLog("Fout bij ChangeOfLV : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID, KC.App_ID);
                //if (Klant_Config != "")
                //{
                //    strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                //        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
                //}
                //else
                //{
                //    //WriteEnrollmentLog(S.ErrorCode.ToString() + " - " + S.ErrorDetails + " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), dr["ProductType"].ToString(),
                //    //"Movein", "", EnrollmentID, dr["Ean18_code"].ToString());
                //}
                //MessageBox.Show(S.ErrorCode.ToString());
                //MessageBox.Show(S.ErrorDetails);
                //MessageBox.Show(S.ErrorText);
                //MessageBox.Show(ex.Detail.InnerXml.ToString());
            }
            catch (WebException exception)
            {
                carShared.SchrijfLog("Fout bij ChangeOfLV : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij ChangeOfLV : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
                //MessageBox.Show(exception.Message);
            }
            catch (Exception exception)
            {
                carShared.SchrijfLog("Fout bij ChangeOfLV : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij ChangeOfLV : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
                //MessageBox.Show(exception.Message);
                
            }

            return blnOK;
        }

        public Boolean MoveIn(DataRow dr, Boolean blnToFile, string strRequestFile)
        {

            Boolean blnOK = false;
            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope enveloppe = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope();

            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);// "EDM00010";
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            string sender = "";
            string identifier = "";
            if (dr["ProductType"].ToString() == "G")
            {
                sender = KC.HoofdLV.ToString();
                identifier = "DDQ_M";
            }
            else
            {
                sender = KC.HoofdLV.ToString();
                identifier = "DDQ_O";
            }
            source.SenderID = sender;
            source.ContactTypeIdentifier = identifier;
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC portaal_content = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP meteringpoint = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP();

            meteringpoint.EANID = dr["Ean18_code"].ToString();
            portaal_content.Portaal_MeteringPoint = meteringpoint;

            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_GridOperator_Company();
            meteringpointGridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();
            meteringpoint.GridOperator_Company = meteringpointGridOperator;

            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC meteringpointMPCommercial = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC();
            meteringpoint.MPCommercialCharacteristics = meteringpointMPCommercial;
            if (dr["KvKNr"].ToString() != "")
            {
                meteringpointMPCommercial.ChamberOfCommerceID = dr["KvKNr"].ToString();
            }

            //if (dr["Verblijfsfunctie"].ToString() == "00")
            //{
            //    meteringpointMPCommercial.Residential = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.N;
            //    meteringpointMPCommercial.DeterminationComplex = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.N;
            //}
            //if (dr["Verblijfsfunctie"].ToString() == "01")
            //{
            //    meteringpointMPCommercial.Residential = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.J;
            //    meteringpointMPCommercial.DeterminationComplex = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.N;
            //}
            //if (dr["Verblijfsfunctie"].ToString() == "10")
            //{
            //    meteringpointMPCommercial.Residential = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.N;
            //    meteringpointMPCommercial.DeterminationComplex = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.J;
            //}
            //if (dr["Verblijfsfunctie"].ToString() == "11")
            //{
            //    meteringpointMPCommercial.Residential = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.J;
            //    meteringpointMPCommercial.DeterminationComplex = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.J;
            //}
            //if (dr["Verblijfsfunctie"].ToString() == "22")
            //{
            //    meteringpointMPCommercial.Residential = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.NVT;
            //    meteringpointMPCommercial.DeterminationComplex = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.NVT;
            //}

            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_SimpleAddressType contact = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_SimpleAddressType();
           if (dr["Relatie_Postbus"].ToString().Trim() == "")
           {
               contact.StreetName = dr["Relatie_Straat"].ToString();
               contact.BuildingNr = dr["Relatie_Huisnummer"].ToString();
               if (dr["Relatie_Toevoeging"].ToString().Trim() != "")
                {
                    contact.ExBuildingNr = dr["Relatie_Toevoeging"].ToString();
                    contact.ExBuildingNr = contact.ExBuildingNr.Trim();

                }
                
                contact.ZIPCode = dr["Relatie_Postcode"].ToString();
                contact.ZIPCode = contact.ZIPCode.Trim();
                contact.CityName = dr["Relatie_Woonplaats"].ToString();
                contact.Country = "NL";
           }
            else
            {
                contact.StreetName = "Postbus";
                contact.BuildingNr = dr["Relatie_Postbus"].ToString();
                //contact.ExBuildingNr = " ";
                contact.ZIPCode = dr["Relatie_Postbus_Postcode"].ToString();
                contact.CityName = dr["Relatie_Postbus_Plaats"].ToString();
                contact.Country = "NL";
           }

           

            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC_GridContractParty meteringpointGridContact = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC_GridContractParty();
            //Factuur_Kortenaam  --> Aansluiting Kortenaam  30-8-2025
            if (dr["Aansluiting_Kortenaam"].ToString().Length > 24) { meteringpointGridContact.Surname = dr["Aansluiting_Kortenaam"].ToString().Substring(0, 24); }
           else { meteringpointGridContact.Surname = dr["Aansluiting_Kortenaam"].ToString(); }
           if (!DBNull.Value.Equals(dr["Relatie_Geboortedatum"]))
           {
               meteringpointGridContact.BirthDateSpecified = true;
                meteringpointGridContact.BirthDate = (DateTime)dr["Relatie_Geboortedatum"];
           }
           meteringpointGridContact.Contact = contact;
            //meteringpointGridContact.Initials = " ";
            meteringpointMPCommercial.GridContractParty = meteringpointGridContact;

            if (dr["ProductType"].ToString() == "G")
            {
                sender = KC.HoofdLV.ToString();
            }
            else
           {
                sender = KC.HoofdLV.ToString();
            }

            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company meteringpointBalanceSupplier = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company();
            meteringpointBalanceSupplier.ID = sender;
            meteringpointMPCommercial.BalanceSupplier_Company = meteringpointBalanceSupplier;

            string PV = "";
            if (dr["ProductType"].ToString() == "G")
            {
                PV = "8717185189995";
            }
            else
            {
                PV = KC.HoofdPV.ToString();
            }
            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC_BalanceResponsibleParty_Company meteringpointBalanceResponsibleParty = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC_BalanceResponsibleParty_Company();
            meteringpointBalanceResponsibleParty.ID = PV;// "8712423012615";
            meteringpointMPCommercial.BalanceResponsibleParty_Company = meteringpointBalanceResponsibleParty;



            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_PM meteringpointPortaalMutation = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_PM();
            meteringpointPortaalMutation.MutationDate = (DateTime)dr["Contract_Start_DT"];
            meteringpoint.Portaal_Mutation = meteringpointPortaalMutation;

            nl.Energie.EDSN.MoveIn.MoveIn moveIn = new nl.Energie.EDSN.MoveIn.MoveIn();

            //String certPath = certpath + @"EDSN2013053100006.p12";
            moveIn.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));



            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13|SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //moveIn.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderMoveInRespondingActivity";
            moveIn.Url = KC.CarUrl + @"synchroon/ResponderMoveInRespondingActivity";

            moveIn.Timeout = 120000;





            nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope retour = new nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope();




            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope));
            StringWriter swXML = new StringWriter();
            serializer.Serialize(swXML, enveloppe);
            //MessageBox.Show(swXML.ToString());
            int intOutBoxID = carShared.Save_Bericht(1, swXML.ToString(), "Move in : " +  dr["Ean18_code"].ToString(), false, "", true, false);

        
            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            BestandsAanvulling = " LV " + BestandsAanvulling;
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"MoveIn" + BestandsAanvulling + ".xml");
            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();


            try
            {
                
                retour = moveIn.CallMoveIn(enveloppe);
                nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;

                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope));
                WriteFileStream = new StreamWriter(KC.XMLPath + @"MoveInResponse" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, retour);
                WriteFileStream.Close();

// 

                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection))
                {
                    nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
                    nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
                    //MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
                    carShared.SchrijfLog("Fout MoveIn " + dr["Ean18_code"].ToString() + " - " + rejectionPortaalType[0].RejectionText.ToString(), 10, intOutBoxID, AppID);
                    strError_Message = "Fout MoveIn " + dr["Ean18_code"].ToString() + " - " + rejectionPortaalType[0].RejectionText.ToString();


                }
                else
                {
                    blnOK = true;
                    nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem = (nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;



                    //414
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
                    swXML = new StringWriter();
                    serializer.Serialize(swXML, responseItem);

                    DossierID = responseItem.Portaal_Mutation.Dossier.ID;

                    int BerichtID = carShared.Save_Bericht(2, swXML.ToString(), "Move in : " + dr["Ean18_code"].ToString(), true, retour.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);

                    GainItem gainItem = new GainItem();
                    gainItem.AppID = AppID;
                    gainItem.BerichtID = BerichtID;
                    gainItem.Datum = responseItem.Portaal_Mutation.MutationDate;
                    gainItem.Dossier = responseItem.Portaal_Mutation.Dossier.ID;
                    gainItem.EAN18_Code = Int64.Parse(responseItem.EANID);
                    gainItem.LV = Int64.Parse(responseItem.MPCommercialCharacteristics.BalanceSupplier_Company.ID);
                    gainItem.PV = Int64.Parse(responseItem.MPCommercialCharacteristics.BalanceResponsibleParty_Company.ID);
                    gainItem.Reden = responseItem.Portaal_Mutation.MutationReason.ToString();
                    gainItem.Referentie = responseItem.Portaal_Mutation.ExternalReference;
                    gainItem.Ontvanger = Int64.Parse(retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID.ToString());
                    gainItem.Product = responseItem.ProductType.ToString();
                    gainItem.NetbeheerderEAN = Int64.Parse(responseItem.GridOperator_Company.ID.ToString());
                    gainItem.OudeLV = -1;
                    gainItem.Referentie = enveloppe.EDSNBusinessDocumentHeader.DocumentID;
                    if (responseItem.MPCommercialCharacteristics.OldBalanceSupplier_Company != null)
                    {
                        gainItem.OudeLV = Int64.Parse(responseItem.MPCommercialCharacteristics.OldBalanceSupplier_Company.ID.ToString());
                    }
                    carShared.SchrijfGain(gainItem);

                    //ProcessMessage.processMessage(BerichtID, KC.ConnString);
                    Boolean blnSucces = false;
                    String strError = "";
                    VerwerkCar verwerkCar = new VerwerkCar(BerichtID, out blnSucces, out strError, KC.ConnString);

                    carShared.Save_Switch((int)dr["Aansluiting_ID"], intOutBoxID, BerichtID, -1);
                    //MessageBox.Show("Accepted - Dossier " + responseItem.Portaal_Mutation.Dossier.ID);

                    blnOK = true;
                }
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                carShared.SchrijfLog("Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
                

                //if (Klant_Config != "")
                //{
                //    strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                //        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
                //}
                //else
                //{
                //    //WriteEnrollmentLog(S.ErrorCode.ToString() + " - " + S.ErrorDetails + " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), dr["ProductType"].ToString(),
                //    //"Movein", "", EnrollmentID, dr["Ean18_code"].ToString());
                //}
                //MessageBox.Show(S.ErrorCode.ToString());
                //MessageBox.Show(S.ErrorDetails);
                //MessageBox.Show(S.ErrorText);
                //MessageBox.Show(ex.Detail.InnerXml.ToString());
            }
            catch (WebException exception)
            {
                carShared.SchrijfLog("Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
                //MessageBox.Show(exception.Message);
                
            }
            catch (Exception exception)
            {
                carShared.SchrijfLog("Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
                //MessageBox.Show(exception.Message);
               
            }

            return blnOK;
        }

        public Boolean MoveOut(DataRow dr)
        {
            Boolean blnOK = false;
            string eanForError = "";
            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope enveloppe = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            string sender = "";
            string identifier = "";
            if (dr["ProductType"].ToString() == "G")
            {
                sender = KC.HoofdLV.ToString();
                identifier = "DDQ_M";
            }
            else
            {
                sender = KC.HoofdLV.ToString();
                identifier = "DDQ_O";
            }

            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            source.SenderID = sender;
            source.ContactTypeIdentifier = identifier;
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint meteringpoint = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint();

            meteringpoint.EANID = dr["Ean18_code"].ToString();
            eanForError = dr["Ean18_code"].ToString();
            portaal_content.Portaal_MeteringPoint = meteringpoint;

            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company();
            meteringpointGridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();
            meteringpoint.GridOperator_Company = meteringpointGridOperator;

            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics meteringpointMPCommercial = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics();
            meteringpoint.MPCommercialCharacteristics = meteringpointMPCommercial;

            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company meteringpointBalanceSupplier = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company();
            meteringpointBalanceSupplier.ID = sender;
            meteringpointMPCommercial.BalanceSupplier_Company = meteringpointBalanceSupplier;

            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation meteringpointPortaalMutation = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation();
            meteringpointPortaalMutation.MutationDate = (DateTime)dr["Contract_Start_DT"];
            meteringpoint.Portaal_Mutation = meteringpointPortaalMutation;

            nl.Energie.EDSN.MoveOut.MoveOut MoveOut = new nl.Energie.EDSN.MoveOut.MoveOut();

            //String certPath = certpath + @"EDSN2013053100006.p12";
            MoveOut.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //MoveOut.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderMoveOutRespondingActivity";
            MoveOut.Url = KC.CarUrl + @"synchroon/ResponderMoveOutRespondingActivity";

            MoveOut.Timeout = 120000;

            nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope retour = new nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope();


            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope));

            //      TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"MoveOut.xml  ");
            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            BestandsAanvulling = " LV " + BestandsAanvulling;
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"MoveOut" + BestandsAanvulling + ".xml");

            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();

            StringWriter swXML = new StringWriter();
            serializer.Serialize(swXML, enveloppe);
            //MessageBox.Show(swXML.ToString());
            //int intOutBoxID = Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "MoveOut", swXML.ToString());
            int intOutBoxID = carShared.Save_Bericht(1, swXML.ToString(), "Move out : " + dr["Ean18_code"].ToString(), false, "", true, false);

            try
            {
               
                // 
                retour = MoveOut.CallMoveOut(enveloppe);
                nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;

                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope));

                WriteFileStream = new StreamWriter(KC.XMLPath + @"MoveOutResponse" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, retour);
                WriteFileStream.Close();

                //string ftpResponse = "";
                //if (Klant_Config == "ROBIN" && strTest != "JA")
                //{
                //    if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "MoveOutResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + @"MoveOutResponse" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                //    {
                //        //MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse);
                //        WriteLog("Fout bij verzenden naar nexant MoveOut - " + eanForError + "-" + ftpResponse, 10, intOutBoxID);
                //    }
                //}

                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_Portaal_Content_Portaal_Rejection))
                {
                    nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
                    nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
                    //MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
                    carShared.SchrijfLog("Fout MoveOut " + dr["Ean18_code"].ToString() + " - " + rejectionPortaalType[0].RejectionText.ToString(), 10, intOutBoxID, AppID);
                    strError_Message = "Fout MoveOut " + dr["Ean18_code"].ToString() + " - " + rejectionPortaalType[0].RejectionText.ToString();
                    
                }
                else
                {
                    blnOK = true;
                    nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem = (nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;

                    //406
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
                    swXML = new StringWriter();
                    serializer.Serialize(swXML, responseItem);

                    DossierID = responseItem.Portaal_Mutation.Dossier.ID;

                    int BerichtID = carShared.Save_Bericht(2, swXML.ToString(), "Move in : " + dr["Ean18_code"].ToString(), true, retour.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);

                    LossItem lossItem = new LossItem();
                    lossItem.AppID = AppID;
                    lossItem.BerichtID = BerichtID;
                    lossItem.Datum = responseItem.Portaal_Mutation.MutationDate;
                    lossItem.Dossier = responseItem.Portaal_Mutation.Dossier.ID;
                    lossItem.EAN18_Code = Int64.Parse(responseItem.EANID);
                    lossItem.LV = KC.HoofdLV;
                    lossItem.PV = KC.HoofdPV;
                    lossItem.Reden = responseItem.Portaal_Mutation.MutationReason.ToString();
                    lossItem.Referentie = responseItem.Portaal_Mutation.ExternalReference;
                    lossItem.Ontvanger = Int64.Parse(retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID.ToString());
                    lossItem.Product = responseItem.ProductType.ToString();
                    lossItem.NetbeheerderEAN = Int64.Parse(responseItem.GridOperator_Company.ID.ToString());
                    lossItem.Referentie = enveloppe.EDSNBusinessDocumentHeader.DocumentID;
                    if (responseItem.MPCommercialCharacteristics.OldBalanceSupplier_Company != null)
                    {
                        lossItem.OudeLV = Int64.Parse(responseItem.MPCommercialCharacteristics.OldBalanceSupplier_Company.ID.ToString());
                    }
                    carShared.SchrijfLoss(lossItem);

                    ProcessMessage.processMessage(BerichtID, KC.ConnString);

                    carShared.Save_Switch((int)dr["Aansluiting_ID"], intOutBoxID, BerichtID, -1);
                    //MessageBox.Show("Accepted - Dossier " + responseItem.Portaal_Mutation.Dossier.ID);



                    blnOK = true;
                }
            }

            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                carShared.SchrijfLog("Fout bij Moveout : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
                

                //if (Klant_Config != "")
                //{
                //    strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                //        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
                //}
                //else
                //{
                //    //WriteEnrollmentLog(S.ErrorCode.ToString() + " - " + S.ErrorDetails + " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), dr["ProductType"].ToString(),
                //    //"Movein", "", EnrollmentID, dr["Ean18_code"].ToString());
                //}
                //MessageBox.Show(S.ErrorCode.ToString());
                //MessageBox.Show(S.ErrorDetails);
                //MessageBox.Show(S.ErrorText);
                //MessageBox.Show(ex.Detail.InnerXml.ToString());
            }
            catch (WebException exception)
            {
                carShared.SchrijfLog("Fout bij Moveout : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij Moveout : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
                //MessageBox.Show(exception.Message);
              
            }
            catch (Exception exception)
            {
                carShared.SchrijfLog("Fout bij Moveout : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij Moveout : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
                //MessageBox.Show(exception.Message);
              
            }
            return blnOK;
        }

        private Boolean EndOfSupply(DataRow dr, Boolean blnToFile, string strRequestFile)
        {
            Boolean blnOK = false;
            string eanForError = "";

            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope enveloppe = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            string sender = "";
            string identifier = "";
            if (dr["ProductType"].ToString() == "G")
            {
                sender = KC.HoofdLV.ToString();
                identifier = "DDQ_M";
            }
            else
            {
                sender = KC.HoofdLV.ToString();
                identifier = "DDQ_O";
            }

            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            source.SenderID = sender;
            source.ContactTypeIdentifier = identifier;
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint meteringpoint = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint();
            portaal_content.Portaal_MeteringPoint = meteringpoint;
            meteringpoint.EANID = dr["Ean18_code"].ToString();
            eanForError = dr["Ean18_code"].ToString();

            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company();
            meteringpoint.GridOperator_Company = meteringpointGridOperator;
            meteringpointGridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();


            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics meteringpointMPCommercial = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics();
            meteringpoint.MPCommercialCharacteristics = meteringpointMPCommercial;


            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics mpcommercialcharacteristicts = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics();
            meteringpoint.MPCommercialCharacteristics = mpcommercialcharacteristicts;

            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company balansesupplier_company = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company();
            mpcommercialcharacteristicts.BalanceSupplier_Company = balansesupplier_company;
            balansesupplier_company.ID = sender;

            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation portaal_mutation = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation();
            meteringpoint.Portaal_Mutation = portaal_mutation;
            portaal_mutation.MutationDate = (DateTime)dr["Contract_Start_DT"];
            portaal_mutation.ExternalReference = " test";

            nl.Energie.EDSN.EndOfSupply.EndOfSupply EndOfSupply = new nl.Energie.EDSN.EndOfSupply.EndOfSupply();

            //String certPath = certpath + @"EDSN2013053100006.p12";
            EndOfSupply.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //EndOfSupply.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderEndOfSupplyRespondingActivity";
            EndOfSupply.Url = KC.CarUrl + @"synchroon/ResponderEndOfSupplyRespondingActivity";

            EndOfSupply.Timeout = 120000;

            nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope retour = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope();


            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            BestandsAanvulling = " LV " + BestandsAanvulling;
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope));
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"EndOfSupply" + BestandsAanvulling + ".xml");

            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();

            //string ftpResponse = "";
            //if (FTPClass.FtpSendFile(strFTPServer + @"EndOfSupply" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"EndOfSupply" + BestandsAanvulling + ".xml", out ftpResponse) == false)
            //{
            //    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
            //}


            StringWriter swXML = new StringWriter();
            serializer.Serialize(swXML, enveloppe);
            //MessageBox.Show(swXML.ToString());
            //int intOutBoxID = Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "EndOfSupply", swXML.ToString());
            int intOutBoxID = carShared.Save_Bericht(1, swXML.ToString(), "Move out : " + dr["Ean18_code"].ToString(), false, "", true, false);

            try
            {

                
                retour = EndOfSupply.CallEndOfSupply(enveloppe);
              

                nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;


                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope));
                WriteFileStream = new StreamWriter(KC.XMLPath + @"EndOfSupplyResponse" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, retour);
                WriteFileStream.Close();


                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_Portaal_Content_Portaal_Rejection))
                {
                    nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
                    nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
                    carShared.SchrijfLog("Fout EndOfSupply " + dr["Ean18_code"].ToString() + " - " + rejectionPortaalType[0].RejectionText.ToString(), 10, intOutBoxID, AppID);
                    strError_Message = "Fout EndOfSupply " + dr["Ean18_code"].ToString() + " - " + rejectionPortaalType[0].RejectionText.ToString();

                    //MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
                    //WriteLog("Fout bij EndOfSupply - " + eanForError + "-" + rejectionPortaalType[0].RejectionText.ToString(), 10, intOutBoxID);
                    //if (Klant_Config != "")
                    //{
                    //    strError_Message = "Fout bij EndOfSupply - " + eanForError + "-" + rejectionPortaalType[0].RejectionText.ToString();
                    //}
                    //else
                    //{
                    //    WriteEnrollmentLog(rejectionPortaalType[0].RejectionText.ToString(), dr["ProductType"].ToString(),
                    //        "EndOfSupply", "", EnrollmentID, dr["Ean18_code"].ToString());
                    //}
                }
                else
                {
                    blnOK = true;
                    nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem = (nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;

                    //406
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
                    swXML = new StringWriter();
                    serializer.Serialize(swXML, responseItem);

                    DossierID = responseItem.Portaal_Mutation.Dossier.ID;

                    int BerichtID = carShared.Save_Bericht(2, swXML.ToString(), "End of supply in : " + dr["Ean18_code"].ToString(), true, retour.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);

                    LossItem lossItem = new LossItem();
                    lossItem.AppID = AppID;
                    lossItem.BerichtID = BerichtID;
                    lossItem.Datum = responseItem.Portaal_Mutation.MutationDate;
                    lossItem.Dossier = responseItem.Portaal_Mutation.Dossier.ID;
                    lossItem.EAN18_Code = Int64.Parse(responseItem.EANID);
                    lossItem.LV = KC.HoofdLV;
                    lossItem.PV = KC.HoofdPV;
                    lossItem.Reden = responseItem.Portaal_Mutation.MutationReason.ToString();
                    lossItem.Referentie = responseItem.Portaal_Mutation.ExternalReference;
                    lossItem.Ontvanger = Int64.Parse(retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID.ToString());
                    lossItem.Product = responseItem.ProductType.ToString();
                    lossItem.NetbeheerderEAN = Int64.Parse(responseItem.GridOperator_Company.ID.ToString());
                    if (responseItem.MPCommercialCharacteristics.OldBalanceSupplier_Company != null)
                    {
                        lossItem.OudeLV = Int64.Parse(responseItem.MPCommercialCharacteristics.OldBalanceSupplier_Company.ID.ToString());
                    }
                    carShared.SchrijfLoss(lossItem);

                    ProcessMessage.processMessage(BerichtID, KC.ConnString);

                    carShared.Save_Switch((int)dr["Aansluiting_ID"], intOutBoxID, BerichtID, -1);

                    blnOK = true;
                }
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                carShared.SchrijfLog("Fout bij EOS : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID, KC.App_ID);
                

                strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
               

                //if (Klant_Config != "")
                //{
                //    strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                //        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
                //}
                //else
                //{
                //    //WriteEnrollmentLog(S.ErrorCode.ToString() + " - " + S.ErrorDetails + " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), dr["ProductType"].ToString(),
                //    //"Movein", "", EnrollmentID, dr["Ean18_code"].ToString());
                //}
                //MessageBox.Show(S.ErrorCode.ToString());
                //MessageBox.Show(S.ErrorDetails);
                //MessageBox.Show(S.ErrorText);
                //MessageBox.Show(ex.Detail.InnerXml.ToString());
            }
            catch (WebException exception)
            {
                carShared.SchrijfLog("Fout bij EOS : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij EOS : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
                //MessageBox.Show(exception.Message);
               
            }
            catch (Exception exception)
            {
                carShared.SchrijfLog("Fout bij EOS : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij EOS : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
                //MessageBox.Show(exception.Message);
               
            }

            return blnOK;
        }

        private Boolean ChangeOfPV(DataRow dr, string strPV, Boolean blnToFile, string strRequestFile)
        {
            Boolean blnOK = false;
            string eanForError = "";

            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope enveloppe = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            source.SenderID = KC.HoofdLV.ToString();// strPVSender;// strSender;
            source.ContactTypeIdentifier = "DDQ_O";// "DDQ_O";
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint meteringpoint = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint();
            portaal_content.Portaal_MeteringPoint = meteringpoint;
            meteringpoint.EANID = dr["Ean18_code"].ToString();
            eanForError = dr["Ean18_code"].ToString();

            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company();
            meteringpoint.GridOperator_Company = meteringpointGridOperator;
            meteringpointGridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();

            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics meteringpointMPCommercial = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics();
            meteringpoint.MPCommercialCharacteristics = meteringpointMPCommercial;

            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics mpcommercialcharacteristicts = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics();
            meteringpoint.MPCommercialCharacteristics = mpcommercialcharacteristicts;

            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company balansesupplier_company = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company();
            mpcommercialcharacteristicts.BalanceSupplier_Company = balansesupplier_company;
            balansesupplier_company.ID = KC.HoofdLV.ToString();//"8714252022919";

            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceResponsibleParty_Company balansresponsibleparty_company = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceResponsibleParty_Company();
            mpcommercialcharacteristicts.BalanceResponsibleParty_Company = balansresponsibleparty_company;
            balansresponsibleparty_company.ID = strPV; // strPVSender;// "8712423012615";

            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation portaal_mutation = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation();
            meteringpoint.Portaal_Mutation = portaal_mutation;
            portaal_mutation.MutationDate = (DateTime)dr["Contract_Start_DT"];


            nl.Energie.EDSN.ChangeOfPV.ChangeOfPV changeOfPV = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPV();

            //String certPath = certpath + @"EDSN2013053100007.p12";
            changeOfPV.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));


            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //changeOfPV.Url = @"https://portaal-fatn.nl.Energie.EDSN.nl/b2b/synchroon/ResponderChangeOfPVRespondingActivity";
            changeOfPV.Url = KC.CarUrl + @"synchroon/ResponderChangeOfPVRespondingActivity";

            changeOfPV.Timeout = 120000;

            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope retour = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope();


            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope));
            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            BestandsAanvulling = " LV " + BestandsAanvulling;
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"changeOfPV" + BestandsAanvulling + ".xml");
            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();

            StringWriter swXML = new StringWriter();
            serializer.Serialize(swXML, enveloppe);
            //MessageBox.Show(swXML.ToString());
            //int intOutBoxID = Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "ChangeOfPV", swXML.ToString());
            int intOutBoxID = carShared.Save_Bericht(1, swXML.ToString(), "Move out : " + dr["Ean18_code"].ToString(), false, "", true, false);

            try
            {

                retour = changeOfPV.CallChangeOfPV(enveloppe);


                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope));
                WriteFileStream = new StreamWriter(KC.XMLPath + @"ChangeOfPVResponseEnvelope" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, retour);
                WriteFileStream.Close();

                //string ftpResponse = "";
                //if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "ChangeOfPVResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + @"ChangeOfPVResponseEnvelope" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                //{
                //    MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse);
                //}



                nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;
                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_Rejection))
                {
                    nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
                    nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
                    //MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
                    carShared.SchrijfLog("Fout ChangeOfPV " + dr["Ean18_code"].ToString() + " - " + rejectionPortaalType[0].RejectionText.ToString(), 10, intOutBoxID, AppID);
                    strError_Message = "Fout ChangeOfPV " + dr["Ean18_code"].ToString() + " - " + rejectionPortaalType[0].RejectionText.ToString();

                }
                else
                {
                    blnOK = true;
                    nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem = (nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;

                    //414
                    new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
                    swXML = new StringWriter();
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
                    serializer.Serialize(swXML, responseItem);
                    //int inboxID_414 = Save_Inbox(12, swXML.ToString(), "MoveIn");
                    //int edineID = Save_Edine(inboxID_414, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "414");
                    //Save_414_Header(edineID, DateTime.Now, strReceiver, strSender);
                    //Save_414(edineID, header.DocumentID, dr["Ean18_code"].ToString(), (DateTime)dr["Contract_Start_DT"], "E01",
                    //        "", dr["Transportkosten_naam1"].ToString(), dr["Aansluiting_Naam"].ToString(),
                    //        dr["Transportkosten_Straat"].ToString(), dr["Transportkosten_Plaats"].ToString(), dr["Transportkosten_Postcode"].ToString(),
                    //        "NL", "E", "E01", "E01", responseItem.Portaal_Mutation.Dossier.ID, dr["EnrollmentID"].ToString());

                    //ProcessMessage.processMessage(inboxID_414);
                    //E09

                    DossierID = responseItem.Portaal_Mutation.Dossier.ID;

                    int BerichtID = carShared.Save_Bericht(2, swXML.ToString(), "ChangeOfPV" + dr["Ean18_code"].ToString(), true, retour.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);



                    //ProcessMessage.processMessage(BerichtID, KC.ConnString);

                    carShared.Save_Switch((int)dr["Aansluiting_ID"], intOutBoxID, BerichtID, -1);

                    //MessageBox.Show("Accepted - Dossier " + responseItem.Portaal_Mutation.Dossier.ID);
                    blnOK = true;
                }

            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {


                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                carShared.SchrijfLog("Fout bij ChangeOfPV : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();


                //if (Klant_Config != "")
                //{
                //    strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                //        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
                //}
                //else
                //{
                //    //WriteEnrollmentLog(S.ErrorCode.ToString() + " - " + S.ErrorDetails + " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), dr["ProductType"].ToString(),
                //    //"Movein", "", EnrollmentID, dr["Ean18_code"].ToString());
                //}
                //MessageBox.Show(S.ErrorCode.ToString());
                //MessageBox.Show(S.ErrorDetails);
                //MessageBox.Show(S.ErrorText);
                //MessageBox.Show(ex.Detail.InnerXml.ToString());
            }
            catch (WebException exception)
            {
                carShared.SchrijfLog("Fout bij ChangeOfPV : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij ChangeOfPV : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
                //MessageBox.Show(exception.Message);
            }

            catch (Exception exception)
            {
                carShared.SchrijfLog("Fout bij ChangeOfPV : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij ChangeOfPV : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
                //MessageBox.Show(exception.Message);

            }

            return blnOK;
        }

        private Boolean ChangeOfAllocationMethod(DataRow dr, Boolean blnToFile, string strRequestFile)
        {
            Boolean blnOK = false;

            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope enveloppe = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            string sender = "";
            string identifier = "";
            if (dr["ProductType"].ToString() == "G")
            {
                sender = KC.HoofdLV.ToString();
                identifier = "DDQ_M";
            }
            else
            {
                sender = KC.HoofdLV.ToString();
                identifier = "DDQ_O";
            }

            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            source.SenderID = sender;
            source.ContactTypeIdentifier = identifier;
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC pc = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC();
            enveloppe.Portaal_Content = pc;


            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP pc_pmp = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP();

            //pc_pmp = new nl.Energie.EDSN.ChangeOfAllocationMethod.NoticeEOSNotificationEnvelope_PC_PMP();
            enveloppe.Portaal_Content.Portaal_MeteringPoint = pc_pmp;

            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_MPPC pc_pmp_mppc = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_MPPC();
            if ((bool)dr["SlimmeMeterAllocatie"] == true)
            {
                pc_pmp_mppc.AllocationMethod = nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EnergyAllocationMethodPortaalCode.SMA;
            }
            else
            {
                pc_pmp_mppc.AllocationMethod = nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EnergyAllocationMethodPortaalCode.PRF;
            }
            pc_pmp.MPPhysicalCharacteristics = pc_pmp_mppc;

            pc_pmp.EANID = dr["Ean18_code"].ToString();
            pc_pmp.ValidFromDate = (DateTime)dr["Contract_Start_DT"];
            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_GridOperator_Company gridOperator = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_GridOperator_Company();
            pc_pmp.GridOperator_Company = gridOperator;
            gridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();
            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_MPCC mpcc = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_MPCC();
            pc_pmp.MPCommercialCharacteristics = mpcc;


            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company balanceSupplier = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company();
            mpcc.BalanceSupplier_Company = balanceSupplier;
            balanceSupplier.ID = sender;
            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_Portaal_Mutation pm = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_Portaal_Mutation();
            pc_pmp.Portaal_Mutation = pm;
            pm.ExternalReference = dr["Aansluiting_ID"].ToString();

            //pm.ExternalReference = (DateTime)dr["Contract_Start_DT"];
            //nl.Energie.EDSN.NoticeEos.NoticeEOSNotificationEnvelope_PC_PMP_MPCC_GridContractParty gridContact = new nl.Energie.EDSN.NoticeEos.NoticeEOSNotificationEnvelope_PC_PMP_MPCC_GridContractParty();
            //mpcc.GridContractParty = gridContact;
            //gridContact.Surname = dr["account_KorteNaam"].ToString();
            //gridContact.Contact = new nl.Energie.EDSN.NoticeEos.NoticeEOSNotificationEnvelope_BoundAddressType();
            //gridContact.Contact.StreetName = dr["Transportkosten_Straat"].ToString();
            //gridContact.Contact.BuildingNr = dr["Transportkosten_Huisnummer"].ToString();
            //gridContact.Contact.ZIPCode = dr["Transportkosten_Postcode"].ToString();
            //gridContact.Contact.CityName = dr["Transportkosten_Plaats"].ToString();
            //gridContact.Contact.Country = "NL";


            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethod ChangeOfAllocationMethod = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethod();


            //String certPath = certpath + @"EDSN2013053100006.p12";
            ChangeOfAllocationMethod.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //EndOfSupply.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderEndOfSupplyRespondingActivity";
            ChangeOfAllocationMethod.Url = KC.CarUrl + @"synchroon/ResponderChangeOfAllocationMethodRespondingActivity";

            ChangeOfAllocationMethod.Timeout = 120000;

            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope retour = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope();


            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope));
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"ChangeOfAllocation" + BestandsAanvulling + ".xml");
            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();

            //string ftpResponse = "";
            //if (FTPClass.FtpSendFile("ftp://62.148.191.136/" + @"EndOfSupply" + BestandsAanvulling + ".xml", "robin", "Wu!69Z#", path + @"EndOfSupply" + BestandsAanvulling + ".xml", out ftpResponse) == false)
            //{
            //    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
            //}


            StringWriter swXML = new StringWriter();
            serializer.Serialize(swXML, enveloppe);
            //MessageBox.Show(swXML.ToString());
            int intOutBoxID = 0; // Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "NoticeEOS", swXML.ToString());


            try
            {

                BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                { BestandsAanvulling = " LV " + BestandsAanvulling; }


                //XmlDocument _Doc = new XmlDocument();
                //_Doc.Load(path + @"EndOfSupplyResponse LV 120914 215836.xml");
                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope));
                //retour = (nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));



                retour = ChangeOfAllocationMethod.CallChangeOfAllocationMethod(enveloppe);

                nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_PC portaalResponse = retour.Portaal_Content;

                blnOK = true;


                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_PC_PR))
                {
                    nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_PC_PR rejection = (nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_PC_PR)portaalResponse.Item;
                    nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_RejectionPortaalType rejectionType = (nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_RejectionPortaalType)rejection.Rejection[0];
                    strError_Message = rejectionType.RejectionText;
                    blnOK = false;
                }
                else
                {
                    nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_PC_PMP portaalPMP = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_PC_PMP();
                    portaalPMP = (nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_PC_PMP)portaalResponse.Item;


                    SqlConnection cnPubs = new SqlConnection(KC.ConnString);
                    string SQLstatement;

                    cnPubs.Open();
                    SQLstatement =
                            "INSERT INTO [Car].[dbo].[SMAChange] ([EAN18_Code],[Datum],[SlimmeMeterAllocatie],[DossierID]) " +
                            "VALUES(@EAN18_Code, @Datum, @SlimmeMeterAllocatie,@DossierID)";


                    SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
                    cmdSaveInbox.Parameters.AddWithValue("@EAN18_Code", dr["Ean18_code"].ToString());
                    cmdSaveInbox.Parameters.AddWithValue("@Datum", (DateTime)dr["Contract_Start_DT"]);
                    cmdSaveInbox.Parameters.AddWithValue("@SlimmeMeterAllocatie", (bool)dr["SlimmeMeterAllocatie"]);
                    cmdSaveInbox.Parameters.AddWithValue("@DossierID", portaalPMP.Portaal_Mutation.Dossier.ID);
                    cmdSaveInbox.ExecuteNonQuery();
                    cnPubs.Close();

                }


                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope));
                WriteFileStream = new StreamWriter(KC.XMLPath + @"ChangeOfAllocation" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, retour);
                WriteFileStream.Close();


            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                carShared.SchrijfLog("Fout bij ChangeOfAllocation : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
                
                //if (Klant_Config != "")
                //{
                //    strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                //        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
                //}
                //else
                //{
                //    //WriteEnrollmentLog(S.ErrorCode.ToString() + " - " + S.ErrorDetails + " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), dr["ProductType"].ToString(),
                //    //"Movein", "", EnrollmentID, dr["Ean18_code"].ToString());
                //}
                //MessageBox.Show(S.ErrorCode.ToString());
                //MessageBox.Show(S.ErrorDetails);
                //MessageBox.Show(S.ErrorText);
                //MessageBox.Show(ex.Detail.InnerXml.ToString());
            }
            catch (WebException exception)
            {
                carShared.SchrijfLog("Fout bij ChangeOfAllocation : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij ChangeOfAllocation : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
                //MessageBox.Show(exception.Message);
               
            }
            catch (Exception exception)
            {
                carShared.SchrijfLog("Fout bij ChangeOfAllocation : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij ChangeOfAllocation : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
                //MessageBox.Show(exception.Message);
               
            }


            return blnOK;
        }

        private Boolean EOSNotice(DataRow dr, Boolean blnToFile, string strRequestFile)
        {
            Boolean blnOK = false;

            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope enveloppe = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208"; ;
            destination.Receiver = receiver;

            string sender = "";
            string identifier = "";
            if (dr["ProductType"].ToString() == "G")
            {
                sender = KC.HoofdLV.ToString();
                identifier = "DDQ_M";
            }
            else
            {
                sender = KC.HoofdLV.ToString();
                identifier = "DDQ_O";
            }

            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader_Source();
            source.SenderID = sender;
            source.ContactTypeIdentifier = identifier;
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP[] pc_pmp = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP[1];

            pc_pmp[0] = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP();
            enveloppe.Portaal_Content = pc_pmp;

            pc_pmp[0].EANID = dr["Ean18_code"].ToString();
            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_GridOperator_Company gridOperator = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_GridOperator_Company();
            pc_pmp[0].GridOperator_Company = gridOperator;
            gridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();
            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_MPCC mpcc = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_MPCC();
            pc_pmp[0].MPCommercialCharacteristics = mpcc;
            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_MPCC_BalanceSupplier_Company balanceSupplier = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_MPCC_BalanceSupplier_Company();
            mpcc.BalanceSupplier_Company = balanceSupplier;
            balanceSupplier.ID = sender;
            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_PM pm = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_PM();
            pc_pmp[0].Portaal_Mutation = pm;
            pm.MutationDate = (DateTime)dr["Contract_Start_DT"];
            //nl.Energie.EDSN.NoticeEos.NoticeEOSNotificationEnvelope_PC_PMP_MPCC_GridContractParty gridContact = new nl.Energie.EDSN.NoticeEos.NoticeEOSNotificationEnvelope_PC_PMP_MPCC_GridContractParty();
            //mpcc.GridContractParty = gridContact;
            //gridContact.Surname = dr["account_KorteNaam"].ToString();
            //gridContact.Contact = new nl.Energie.EDSN.NoticeEos.NoticeEOSNotificationEnvelope_BoundAddressType();
            //gridContact.Contact.StreetName = dr["Transportkosten_Straat"].ToString();
            //gridContact.Contact.BuildingNr = dr["Transportkosten_Huisnummer"].ToString();
            //gridContact.Contact.ZIPCode = dr["Transportkosten_Postcode"].ToString();
            //gridContact.Contact.CityName = dr["Transportkosten_Plaats"].ToString();
            //gridContact.Contact.Country = "NL";


            nl.Energie.EDSN.NoticeEOS.NoticeEOS NoticeEOS = new nl.Energie.EDSN.NoticeEOS.NoticeEOS();

            //String certPath = certpath + @"EDSN2013053100006.p12";
            NoticeEOS.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //EndOfSupply.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderEndOfSupplyRespondingActivity";
            NoticeEOS.Url = KC.CarUrl + @"synchroon/ResponderNoticeEOSRespondingActivity";

            NoticeEOS.Timeout = 120000;

            nl.Energie.EDSN.NoticeEOS.NoticeEOSAcknowledgementEnvelope retour = new nl.Energie.EDSN.NoticeEOS.NoticeEOSAcknowledgementEnvelope();


            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope));
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"NoticeEOS" + BestandsAanvulling + ".xml");
            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();

            //string ftpResponse = "";
            //if (FTPClass.FtpSendFile("ftp://62.148.191.136/" + @"EndOfSupply" + BestandsAanvulling + ".xml", "robin", "Wu!69Z#", path + @"EndOfSupply" + BestandsAanvulling + ".xml", out ftpResponse) == false)
            //{
            //    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
            //}


            StringWriter swXML = new StringWriter();
            serializer.Serialize(swXML, enveloppe);
            //MessageBox.Show(swXML.ToString());
            int intOutBoxID = 0; // Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "NoticeEOS", swXML.ToString());


            try
            {

                BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                { BestandsAanvulling = " LV " + BestandsAanvulling; }


                //XmlDocument _Doc = new XmlDocument();
                //_Doc.Load(path + @"EndOfSupplyResponse LV 120914 215836.xml");
                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope));
                //retour = (nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

                

                    retour = NoticeEOS.NoticeEOSNotification(enveloppe);
                
                nl.Energie.EDSN.NoticeEOS.NoticeEOSAcknowledgementEnvelope_PC portaalResponse = retour.Portaal_Content;

                blnOK = true;

                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.NoticeEOS.NoticeEOSAcknowledgementEnvelope));
                WriteFileStream = new StreamWriter(KC.XMLPath + @"NoticeEos" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, retour);
                WriteFileStream.Close();


            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                carShared.SchrijfLog("Fout bij EOSNotice : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
               
                //if (Klant_Config != "")
                //{
                //    strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                //        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
                //}
                //else
                //{
                //    //WriteEnrollmentLog(S.ErrorCode.ToString() + " - " + S.ErrorDetails + " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), dr["ProductType"].ToString(),
                //    //"Movein", "", EnrollmentID, dr["Ean18_code"].ToString());
                //}
                //MessageBox.Show(S.ErrorCode.ToString());
                //MessageBox.Show(S.ErrorDetails);
                //MessageBox.Show(S.ErrorText);
                //MessageBox.Show(ex.Detail.InnerXml.ToString());
            }
            catch (WebException exception)
            {
                carShared.SchrijfLog("Fout bij EOSNotice : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij EOSNotice : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
                //MessageBox.Show(exception.Message);
                
            }
            catch (Exception exception)
            {
                carShared.SchrijfLog("Fout bij EOSNotice : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij EOSNotice : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
                //MessageBox.Show(exception.Message);
               
            }


            return blnOK;
        }

        private Boolean NameChange(DataRow dr)
        {
            Boolean blnOK = false;

            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope enveloppe = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope();


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            source.SenderID = KC.HoofdLV.ToString();
            source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC portaal_content = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC_PMP meteringpoint = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC_PMP();
            portaal_content.Portaal_MeteringPoint = meteringpoint;
            meteringpoint.EANID = dr["Ean18_code"].ToString();


            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC_PMP_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC_PMP_GridOperator_Company();
            meteringpoint.GridOperator_Company = meteringpointGridOperator;
            meteringpointGridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();


            //nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC_PMP_MPCC meteringpointMPCommercial = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC_PMP_MPCC();
            //meteringpoint.MPCommercialCharacteristics = meteringpointMPCommercial;


            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC_PMP_MPCC mpcommercialcharacteristicts = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC_PMP_MPCC();
            meteringpoint.MPCommercialCharacteristics = mpcommercialcharacteristicts;

            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC_PMP_MPCC_GridContractParty gridcontractparty = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC_PMP_MPCC_GridContractParty();
            mpcommercialcharacteristicts.GridContractParty = gridcontractparty;


            //gridcontractparty.Initials = "C";
            //gridcontractparty.SurnamePrefix = "vd";
            gridcontractparty.Surname = dr["Relatie_Naam"].ToString();


            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company balansesupplier_company = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company();
            mpcommercialcharacteristicts.BalanceSupplier_Company = balansesupplier_company;
            balansesupplier_company.ID = KC.HoofdLV.ToString();

            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC_PMP_PM portaal_mutation = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_PC_PMP_PM();
            meteringpoint.Portaal_Mutation = portaal_mutation;
            //portaal_mutation.MutationReason = nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_MutationReasonPortaalCode.NAMECHG;
            //portaal_mutation.ExternalReference = " test";

            nl.Energie.EDSN.NameChange.NameChange NameChange = new nl.Energie.EDSN.NameChange.NameChange();

            //String certPath = certpath + @"EDSN2013053100006.p12";
            NameChange.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));


            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //NameChange.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderNameChangeRespondingActivity";
            NameChange.Url = KC.CarUrl + @"synchroon/ResponderNameChangeRespondingActivity";

            NameChange.Timeout = 120000;



            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope));
            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            BestandsAanvulling = " LV " + BestandsAanvulling;
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"NameChange" + BestandsAanvulling + ".xml");
            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();

            nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope retour = new nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope();
            //retour = NameChange.CallNameChange(enveloppe);
            int intOutBoxID = 0;

            try
            {
               
                retour = NameChange.CallNameChange(enveloppe);

                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope));
                WriteFileStream = new StreamWriter(KC.XMLPath + @"NameChangeResponseEnvelope" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, retour);
                WriteFileStream.Close();

                nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope_PC portaalResponse = retour.Portaal_Content;
                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope_PC_PR))
                {
                    nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope_PC_PR itemRejection = (nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope_PC_PR)portaalResponse.Item;
                    nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
                    carShared.SchrijfLog("Rejection in namechange : " + rejectionPortaalType[0].RejectionText.ToString(), 5, -1, AppID);
                    strError_Message = "Rejection in namechange : " + rejectionPortaalType[0].RejectionText.ToString();
                   
                    //if (Klant_Config != "")
                    //{
                    //    strError_Message = rejectionPortaalType[0].RejectionText.ToString();
                    //}
                    //else
                    //{
                    //    MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
                    //}
                }
                else
                {
                    blnOK = true;
                    nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope_PC_PR_PMP reponseItem = (nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope_PC_PR_PMP)portaalResponse.Item;

                    DossierID = reponseItem.Portaal_Mutation.Dossier.ID;

                    MessageBox.Show("Accepted");
                    blnOK = true;
                }
            }

            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                carShared.SchrijfLog("Fout bij NameChange : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij NameChange : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
               


            }
            catch (WebException exception)
            {
                carShared.SchrijfLog("Fout bij NameChange : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij NameChange : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
             
            }
            catch (Exception exception)
            {
                carShared.SchrijfLog("Fout bij NameChange : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID, KC.App_ID);
                strError_Message = "Fout bij NameChange : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
                
            }
            return blnOK;
        }

        public Boolean GetReject(Boolean blnPV, Boolean blnBatch)
        {
            Boolean blnOK = false;

            //Masterdata
            nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope enveloppe = new nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            if (blnPV != true) { source.SenderID = KC.HoofdLV.ToString(); } else { source.SenderID = KC.HoofdPV.ToString(); }
            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;

            nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_Portaal_Content portaal_Content = new nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_Content;

            nl.Energie.EDSN.LossGainRejectUpdate.LossGainRejectUpdate rejectRequest = new nl.Energie.EDSN.LossGainRejectUpdate.LossGainRejectUpdate();

            //String certPath = certpath + @"EDSN2013053100006.p12";
            if (blnPV == true)
            {
                //certPath = certpath + @"EDSN2013053100007.p12";
                rejectRequest.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertPV, KC.CertPVPassword));
            }
            else
            {
                rejectRequest.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
            }

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //rejectRequest.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderLossGainRejectUpdateRespondingActivity";
            rejectRequest.Url = KC.CarUrl + @"synchroon/ResponderLossGainRejectUpdateRespondingActivity";


            rejectRequest.Timeout = 120000;

            nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultResponseEnvelope retour = new nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultResponseEnvelope();

            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope));
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"LossGainRejectUpdate.xml");
            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();

            try
            {
                retour = rejectRequest.RejectionResult(enveloppe);

                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                BestandsAanvulling = " PV " + BestandsAanvulling;
                XmlSerializer serializer2 = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultResponseEnvelope));
                TextWriter WriteFileStream2 = new StreamWriter(KC.XMLPath + @"RejectionResponse" + BestandsAanvulling + ".xml");
                serializer2.Serialize(WriteFileStream2, retour);
                WriteFileStream2.Close();


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
                blnOK = true;
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                if (!blnBatch)
                {
                    MessageBox.Show(S.ErrorCode.ToString());
                    MessageBox.Show(S.ErrorDetails);
                    MessageBox.Show(S.ErrorText);
                    MessageBox.Show(ex.Detail.InnerXml.ToString());
                }
            }
            catch (WebException exception)
            {
                if (!blnBatch)
                {
                    MessageBox.Show(exception.Message);
                }
            }
            catch (Exception exception)
            {
                if (!blnBatch)
                {
                    MessageBox.Show(exception.Message);
                }
            }

            return blnOK;
        }
    }
}
