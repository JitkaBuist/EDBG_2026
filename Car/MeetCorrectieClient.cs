using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using nl.Energie.EDSN;
using System.Xml;
using System.Windows.Forms;
using Energie.DataTableHelper;
using nl.Energie.VerwerkCar;
// using Energie.DataAccess.EnergieDBDataSetTableAdapters;

namespace Energie.Car
{
    
    public class MeetCorrectieClient
    {
        //private string strSql;
        private CarShared carShared;
        public MeetCorrectieClient(string klantConfig)
        {
            KC.KlantConfig = klantConfig;
            KC.ConnStringPortaal = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");

            carShared = new CarShared();
        }

        public string RequestMeasureCorrection(Boolean blnPV, Boolean blnBatch)
        {
            string strError = "Start";

            //Boolean blnData = false;
            nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionRequestEnvelope enveloppe = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionRequestEnvelope();

            nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            if (blnPV != true) { source.SenderID = KC.HoofdLV.ToString(); } else { source.SenderID = KC.HoofdPV.ToString(); }
            //source.SenderID = "8712423014381";
            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            header.Source = source;

            strError = source.SenderID;

            nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionRequestEnvelope_PC portaal_content = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionRequestEnvelope_PC();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.MeasureCorrection.MeasureCorrection measureCorrection = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrection();

            strError = "Certificaat";

            if (blnPV == true)
            {
                measureCorrection.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertPV, KC.CertPVPassword));
            }
            else
            {
                measureCorrection.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
            }


            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            measureCorrection.Url = KC.CarUrl + @"batch/ResponderMeasureCorrectionRespondingActivity";

            measureCorrection.Timeout = 120000;

            nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResponseEnvelope response = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResponseEnvelope();

            strError = KC.XMLPath;

            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionRequestEnvelope));
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"MeasureCorrection" + BestandsAanvulling + ".xml");
            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();



            try
            {
                BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }

                //Tijdelijk

                strError = "Voor call " + KC.XMLPath;
                response = measureCorrection.MeasureCorrectionRequest(enveloppe);
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResponseEnvelope));
                //Save to file kan weg
                WriteFileStream = new StreamWriter(KC.XMLPath + @"MeasureCorrectionResult" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, response);
                WriteFileStream.Close();
                strError = "Na afroep";


                //Tijdelijk

                //response = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResponseEnvelope();

                //XmlDocument _Doc = new XmlDocument();
                //_Doc.Load(@"C:\inetpub\wwwroot\XMLOutput\EDBG\MeasureCorrectionResult LV 120720 2051517227.xml");
                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResponseEnvelope));
                //response = (nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));


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


                if (response != null && response.Portaal_Content.Length > 0)
                {
                    nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResponseEnvelope_PC_PMP portaal_MeteringPoint = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResponseEnvelope_PC_PMP();
                    int Bericht_ID = car.Save_Bericht(6, @"MeasureCorrectionResult" + BestandsAanvulling + ".xml", "Masterdata : " + header.DocumentID, true, response.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);
                    for (int i = 0; i < response.Portaal_Content.Length; i++)
                    {
                        //blnData = true;

                        //int inboxID = switchBericht.Save_Inbox(26, swXML.ToString(), "Masterdata : " + header.DocumentID);
                        //int edineID = switchBericht.Save_Edine(inboxID, receiver.ReceiverID, source.SenderID, DateTime.Now, header.DocumentID, "UTILMD", "E07");
                        portaal_MeteringPoint = (nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResponseEnvelope_PC_PMP)response.Portaal_Content[i];

                        Int64 SenderEAN = Int64.Parse(source.SenderID.ToString());

                        MeetCorrectie md = new MeetCorrectie(portaal_MeteringPoint, Bericht_ID, blnBatch, SenderEAN);
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

        public string MeasureCorrectionAcknowledgement(Boolean blnPV, Boolean blnBatch, Boolean blnAccoord, string EANCode, string Referentie, string[] Afwijsreden, string[] Toelichting, string Dossier, DateTime BeginDatum)
        {
            string strError = "Start";
            //Boolean blnData = false;
            nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope enveloppe = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope();

            nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_EDSNBusinessDocumentHeader_Source();
            if (blnPV != true) { source.SenderID = KC.HoofdLV.ToString(); } else { source.SenderID = KC.HoofdPV.ToString(); }
            //source.SenderID = "8712423014381";
            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            header.Source = source;

            strError = source.SenderID;

            nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_PC portaal_content = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_PC();
            enveloppe.Portaal_Content = portaal_content;

            if (blnAccoord)
            {
                nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_PC_PMP pmp = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_PC_PMP();
                pmp.EANID = EANCode;
                pmp.Portaal_Mutation = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_PC_PMP_PM();
                pmp.Portaal_Mutation.Dossier = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_PC_PMP_PM_Dossier();
                pmp.Portaal_Mutation.Dossier.ID = Dossier;
                pmp.Portaal_Mutation.Dossier.ValidFromDate = BeginDatum;
                if (Referentie != "")
                {
                    pmp.Portaal_Mutation.ExternalReference = Referentie;
                }
                portaal_content.Item = pmp;
            }
            else
            {
                nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_PC_Portaal_Rejection pr = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_PC_Portaal_Rejection();
                nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_RejectionPortaalType[] rpt = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_RejectionPortaalType[Afwijsreden.Length];
                for (int i=0;i<Afwijsreden.Length;i++)
                {
                    rpt[i] = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_RejectionPortaalType();
                    rpt[i].RejectionCode = Afwijsreden[i];
                    rpt[i].RejectionText = Toelichting[i];
                }
                pr.Rejection = rpt;
                portaal_content.Item = pr;
                nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_PC_Portaal_Rejection_PMP pmp = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_PC_Portaal_Rejection_PMP();
                pmp.EANID = EANCode;
                pmp.Portaal_Mutation = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_PC_Portaal_Rejection_PMP_PM();
                pmp.Portaal_Mutation.Dossier = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope_PC_Portaal_Rejection_PMP_PM_Dossier();
                pmp.Portaal_Mutation.Dossier.ID = Dossier;
                pmp.Portaal_Mutation.Dossier.ValidFromDate = BeginDatum;
                if (Referentie != "")
                {
                    pmp.Portaal_Mutation.ExternalReference = Referentie;
                }
                pr.Portaal_MeteringPoint = pmp;
            }
            

            nl.Energie.EDSN.MeasureCorrection.MeasureCorrection measureCorrection = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrection();

            strError = "Certificaat";

            if (blnPV == true)
            {
                measureCorrection.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertPV, KC.CertPVPassword));
            }
            else
            {
                measureCorrection.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
            }


            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            measureCorrection.Url = KC.CarUrl + @"batch/ResponderMeasureCorrectionRespondingActivity";

            measureCorrection.Timeout = 120000;

            nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultAcknowledgementEnvelope response = new nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultAcknowledgementEnvelope();

            strError = KC.XMLPath;

            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultNotificationEnvelope));
            TextWriter WriteFileStream = new StreamWriter(KC.XMLPath + @"MeasureCorrectionAcknowledgement" + BestandsAanvulling + ".xml");
            serializer.Serialize(WriteFileStream, enveloppe);
            WriteFileStream.Close();



            try
            {
                BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }

                //Tijdelijk

                strError = "Voor call " + KC.XMLPath;
                response = measureCorrection.MeasureCorrectionResultNotification(enveloppe);
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultAcknowledgementEnvelope));
                //Save to file kan weg
                WriteFileStream = new StreamWriter(KC.XMLPath + @"MeasureCorrectionResult" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, response);
                WriteFileStream.Close();
                strError = "Na afroep";


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


                if (response.Portaal_Content.Portaal_Rejection.Length == 0 )
                {
                    strError = "Geaccepteerd";
                }
                else
                {
                    strError = "Afwijziging : ";
                    foreach (nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResultAcknowledgementEnvelope_RejectionPortaalType rpt in response.Portaal_Content.Portaal_Rejection)
                    {
                        strError = strError + rpt.RejectionCode + " " + rpt.RejectionText + " ";
                    }
                    //File.Delete(KC.XMLPath + @"MasterDataUpdate" + BestandsAanvulling + ".xml");
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
    }
}
