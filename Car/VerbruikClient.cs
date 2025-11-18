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
    public class VerbruikClient
    {
        private string strSql;
        private static String ConnString = "";
        //private string urlWebService = Energie.DataAccess.Configurations.GetApplicationSetting("PORTAAL");//"https://emp.edsn.nl/b2b";
        private CarShared carShared;

        public VerbruikClient(string klantConfig)
        {
            KC.KlantConfig = klantConfig;
            KC.ConnStringPortaal = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");
            KC.App_ID = AppID.Car;

            carShared = new CarShared();
        }

        public Boolean RequestMeterReading(Boolean blnPV, string strFileName, Boolean blnBatch, out int nrRecords, out string strError)
        {
            Boolean blnData = false;
            strError = "";
            nrRecords = 0;

            string OldFileName = strFileName;

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope enveloppe = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope();

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            if (blnPV != true) { source.SenderID = KC.HoofdLV.ToString(); } else { source.SenderID = KC.HoofdPV.ToString(); }
            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            header.Source = source;


            nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.MeterReading.MeterReading meterReading = new nl.Energie.EDSN.MeterReading.MeterReading();

            if (strFileName == "")
            {
                if (blnPV == true)
                {
                    meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertPV, KC.CertPVPassword));
                }
                else
                {
                    meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
                }
            }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            meterReading.Url = KC.CarUrl + @"synchroon/ResponderMeterReadingRespondingActivity";

            meterReading.Timeout = 120000;

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope response = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope();



            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope));
            TextWriter WriteFileStream;

            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                { BestandsAanvulling = " LV " + BestandsAanvulling; }

                WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingExchangeRequest" + BestandsAanvulling + ".xml"); ;
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();

                if (KC.FTPServer != "")
                {
                    string ftpResponse = "";
                    if (FTPClass.FtpSendFile(KC.FTPServer + @"MeterReadingExchangeRequest" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"MeterReadingExchangeRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    {
                        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                    }
                }

            }

            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();




                if (strFileName == "")
                {
                    response = meterReading.MeterReadingExchangeRequest(enveloppe);
                    //7 novMeterReadingExchangeResponse 
                    WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingExchangeResponse" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, response);
                    WriteFileStream.Close();

                    // FTP doen we niet meer. 19-2-2021
                    //string ftpResponse = "";
                    //if (blnBatch != true && response.Portaal_Content.Length > 0)
                    //{
                    //   if (FTPClass.FtpSendFile(KC.FTPServer + @"MeterReadingExchangeResponse" + BestandsAanvulling + ".xml", KC.FTPUser, KC.FTPPassword, KC.XMLPath + @"MeterReadingExchangeResponse" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    //   {
                    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                    //    }
                    // }

                    // StringWriter swXML = new StringWriter();
                    // serializer.Serialize(swXML, response);
                    // _Doc.Load(KC.XMLPath + "MeterReadingExchangeResponse" + BestandsAanvulling + ".xml");

                }
                else
                {
                    response = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope();

                    _Doc = new XmlDocument();
                    _Doc.Load(strFileName);
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope));
                    response = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
                }

                CarShared car = new CarShared();
                if (response.Portaal_Content.Length > 0)
                {
                    nrRecords = response.Portaal_Content.Length;
                    int intBericht_ID = car.Save_Bericht(5, @"MeterReadingExchangeResponse" + BestandsAanvulling + ".xml", "Verbruik : " + response.EDSNBusinessDocumentHeader.MessageID.ToString(), true, response.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false); ;
                    for (int i = 0; i < response.Portaal_Content.Length; i++)
                    {
                        DateTime dtOntvangst = response.EDSNBusinessDocumentHeader.CreationTimestamp;

                        Int64 SenderEAN = Int64.Parse(source.SenderID.ToString());

                        //int intedinID = Save_Edine(inboxID, response.EDSNBusinessDocumentHeader.Source.SenderID, response.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID,
                        //    response.EDSNBusinessDocumentHeader.CreationTimestamp, response.EDSNBusinessDocumentHeader.MessageID, "UTILTS", "E11");

                        nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP portaal_meteringPoint = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP)response.Portaal_Content[i];
                        
                        Verbruik verbruik = new Verbruik(portaal_meteringPoint, intBericht_ID, dtOntvangst);

                        //if (intBericht_ID > 0) { ProcessMessage.processMessage(intBericht_ID, ConnString); }

                    }
                }
                else
                {
                   // De files niet wissen 19/2/2021
   //                 File.Delete(KC.XMLPath + @"MeterReadingExchangeRequest" + BestandsAanvulling + ".xml");
   //                 File.Delete(KC.XMLPath + @"MeterReadingExchangeResponse" + BestandsAanvulling + ".xml");
                }



            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                //if (blnBatch || Klant_Config != "")
                //{
                //    WriteLog("Fout Meterreading :" + S.ErrorDetails + " " + S.ErrorText, 10, -1);
                //}
                //else
                //{
                //    MessageBox.Show(S.ErrorCode.ToString());
                //    MessageBox.Show(S.ErrorDetails);
                //    MessageBox.Show(S.ErrorText);
                //    MessageBox.Show(ex.Detail.InnerXml.ToString());
                //}
                strError = "Error in requestMeterReading: " + S.ErrorCode.ToString() + " " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
                carShared.SchrijfLog("Fout requestMeterReading " + " " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, -1, KC.App_ID);
            }
            catch (WebException exception)
            {
                //if (blnBatch || Klant_Config != "")
                //{
                //    WriteLog("Fout Meterreading :" + exception.Message, 10, -1);
                //}
                //else
                //{
                //    MessageBox.Show(exception.Message);
                //}
                strError = "Error in RequestMeterReading: " + exception.Message;
                carShared.SchrijfLog("Error in RequestMeterReading: " + exception.Message, 10, -1, KC.App_ID);
            }
            catch (Exception exception)
            {
                //    //if (blnBatch || Klant_Config != "")
                //    //{
                //    //    WriteLog("Fout Meterreading :" + exception.Message, 10, -1);
                //    //}
                //    //else
                //    //{
                //    //    MessageBox.Show(exception.Message);
                //    //}
                strError = "Error in RequestMeterReading: " + exception.Message;
            }
            //    File.Delete(strFileName);
            return blnData;
        }

        public int VastGesteldeStand(string strFileName, VastGesteldeStand vastGesteldeStand, Boolean blnBatch, int intMeterstandId, out string strError)
        {
            strError = "";
            if (intMeterstandId != -1)
            {
                SqlConnection conn = new SqlConnection(KC.ConnString);
                conn.Open();
                strSql = "SELECT BerichtId \n";
                strSql += ",EanCode \n";
                strSql += ",Verstuurd \n";
                strSql += ",VerstuurdDT \n";
                strSql += ",BeginD \n";
                strSql += ",EindD \n";
                strSql += ",Netbeheerder \n";
                strSql += ",DossierID \n";
                strSql += ",RedenMutatie \n";
                strSql += ",Fout \n";
                strSql += ",Referentie \n";
                strSql += ",Compleet \n";
                strSql += "FROM Car.dbo.MeterStanden_Header \n";
                strSql += "where MeterstandId = @MeterstandId";

                SqlCommand cmd = new SqlCommand(strSql, conn);
                cmd.Parameters.AddWithValue("@MeterstandId", intMeterstandId);
                DataTable dtHeader = new DataTable();
                SqlDataAdapter daHeader = new SqlDataAdapter(cmd);
                daHeader.Fill(dtHeader);

                if (dtHeader.Rows.Count > 0)
                {
                    string[] NrOfDigits = new string[4];
                    string Meternummer = "";
                    string Product = "";
                    string Aantalregister = "";
                    DataRow drHeader = dtHeader.Rows[0];
                    strSql = "SELECT BerichtId \n";
                    strSql += ",Direction \n";
                    strSql += ",TarifType \n";
                    strSql += ",Herkomst \n";
                    strSql += ",CAST(Stand as int) as Stand \n";
                    strSql += ",Volume \n";
                    strSql += ",BeginStand \n";
                    strSql += ",HerkomstBeginStand \n";
                    strSql += ",CalorificCorrectedVolume \n";
                    strSql += "FROM Car.dbo.MeterStand \n";
                    strSql += "WHERE MeterStandId = @MeterStandId";
                    cmd = new SqlCommand(strSql, conn);
                    cmd.Parameters.AddWithValue("@MeterstandId", intMeterstandId);
                    DataTable dtRegisters = new DataTable();
                    SqlDataAdapter daRegisters = new SqlDataAdapter(cmd);
                    daRegisters.Fill(dtRegisters);


                    Energie.Car.MasterdataClient masterDataClient = new Energie.Car.MasterdataClient(KC.KlantConfig);
                    nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope meteringpoint = masterDataClient.RequestGetMeteringPointMP(drHeader["EanCode"].ToString(), false, "", true, false);
                    nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope_PC portaal_Response = meteringpoint.Portaal_Content;

                    nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope_PC_PMP portaal_MeteringPointResponse = (nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope_PC_PMP)portaal_Response.Item;

                    Meternummer = portaal_MeteringPointResponse.Portaal_EnergyMeter.ID;
                    Product = portaal_MeteringPointResponse.ProductType;
                    if (portaal_MeteringPointResponse.Portaal_EnergyMeter.NrOfRegisters != null)
                    {
                        Aantalregister = portaal_MeteringPointResponse.Portaal_EnergyMeter.NrOfRegisters;
                    }
                    else
                    {
                        Aantalregister = portaal_MeteringPointResponse.Portaal_EnergyMeter.Register.Length.ToString();
                    }
                    foreach (nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register register in portaal_MeteringPointResponse.Portaal_EnergyMeter.Register)
                    {
                        int intNr = 0;
                        if (register.TariffType != null)
                        {
                            if (register.TariffType == "N" && register.MeteringDirection == "LVR") { intNr = 0; }
                            if (register.TariffType == "L" && register.MeteringDirection == "LVR") { intNr = 1; }
                            if (register.TariffType == "N" && register.MeteringDirection == "TLV") { intNr = 2; }
                            if (register.TariffType == "N" && register.MeteringDirection == "TLV") { intNr = 3; }
                        }
                        else
                        {
                            if (register.ID == "1.8.2") { intNr = 0; }
                            if (register.ID == "1.8.1") { intNr = 1; }
                            if (register.ID == "2.8.2") { intNr = 2; }
                            if (register.ID == "2.8.1") { intNr = 3; }
                        }
                        NrOfDigits[intNr] = register.NrOfDigits;
                    }




                    if (dtRegisters.Rows.Count > 0)
                    {
                        vastGesteldeStand = new VastGesteldeStand();

                        vastGesteldeStand.arrReading = new string[10];
                        vastGesteldeStand.arrReadingDate = new string[10];
                        vastGesteldeStand.arrReadingMethod = new string[10];
                        vastGesteldeStand.arrTarrifType = new string[10];
                        vastGesteldeStand.arrDirection = new string[10];
                        vastGesteldeStand.arrNrDigits = new string[10];
                        vastGesteldeStand.arrMeasureUnit = new string[10];

                        for (int i = 0; i < dtRegisters.Rows.Count; i++)
                        {
                            int intNr = 0;
                            if (dtRegisters.Rows[i]["TarifType"].ToString() == "N" && dtRegisters.Rows[i]["Direction"].ToString() == "LVR") { intNr = 0; }
                            if (dtRegisters.Rows[i]["TarifType"].ToString() == "L" && dtRegisters.Rows[i]["Direction"].ToString() == "LVR") { intNr = 1; }
                            if (dtRegisters.Rows[i]["TarifType"].ToString() == "N" && dtRegisters.Rows[i]["Direction"].ToString() == "TLV") { intNr = 2; }
                            if (dtRegisters.Rows[i]["TarifType"].ToString() == "N" && dtRegisters.Rows[i]["Direction"].ToString() == "TLV") { intNr = 3; }

                            vastGesteldeStand.arrReading[i] = dtRegisters.Rows[i]["Stand"].ToString();
                            vastGesteldeStand.arrReadingDate[i] = ((DateTime)drHeader["BeginD"]).ToString("yyyy-MM-dd");
                            vastGesteldeStand.arrReadingMethod[i] = dtRegisters.Rows[i]["HerkomstBeginStand"].ToString();
                            vastGesteldeStand.arrTarrifType[i] = dtRegisters.Rows[i]["TarifType"].ToString();
                            vastGesteldeStand.arrDirection[i] = dtRegisters.Rows[i]["Direction"].ToString();
                            vastGesteldeStand.arrNrDigits[i] = NrOfDigits[intNr];
                            if (Product == "ELK")
                            {
                                vastGesteldeStand.arrMeasureUnit[i] = "KWH";
                            }
                            else
                            {
                                vastGesteldeStand.arrMeasureUnit[i] = "MTQ";
                            }
                        }

                        vastGesteldeStand.ean = drHeader["EanCode"].ToString();
                        vastGesteldeStand.vanDatum = (DateTime)drHeader["BeginD"];
                        vastGesteldeStand.totDatum = (DateTime)drHeader["BeginD"];
                        vastGesteldeStand.netbeheerder = drHeader["Netbeheerder"].ToString();
                        vastGesteldeStand.dossierID = drHeader["DossierID"].ToString();
                        vastGesteldeStand.meternummer = Meternummer;
                        vastGesteldeStand.aantalregister = Aantalregister;
                        vastGesteldeStand.herkomst = vastGesteldeStand.arrReadingMethod[0];
                        vastGesteldeStand.redenmutatie = drHeader["RedenMutatie"].ToString().Trim();
                        vastGesteldeStand.product = Product;
                        vastGesteldeStand.intNrStanden = dtRegisters.Rows.Count;
                        vastGesteldeStand.intNrRegisters = dtRegisters.Rows.Count;
                        vastGesteldeStand.enrollment_ID = "-1";
                        vastGesteldeStand.meterStand_ID = intMeterstandId;
                    }
                }

            }

            CultureInfo provider = CultureInfo.InvariantCulture;
            string OldFileName = strFileName;


            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope enveloppe = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope();


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "EDSN";
            receiver.ReceiverID = "8712423010208";// netbeheerder;// "1114252022907";
            destination.Receiver = receiver;

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EDSNBusinessDocumentHeader_Source();
            //if (blnPV != true) { source.SenderID = HoofdLV; } else { source.SenderID = HoofdPV; }


            //source.ContactTypeIdentifier = "DDQ_O";
            if (vastGesteldeStand.product == "GAS")
            {
                source.SenderID = KC.HoofdLV.ToString();
                source.ContactTypeIdentifier = "DDQ_M";
            }
            else
            {
                source.SenderID = KC.HoofdLV.ToString();
                source.ContactTypeIdentifier = "DDQ_O";
            }

            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP[] portaal_content = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP[1];
            enveloppe.Portaal_Content = portaal_content;


            portaal_content[0] = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP();
            portaal_content[0].EANID = vastGesteldeStand.ean;
            if (vastGesteldeStand.product == "ELK") { portaal_content[0].ProductType = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyProductPortaalTypeCode.ELK; }
            else { portaal_content[0].ProductType = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyProductPortaalTypeCode.GAS; }
            //portaal_content[0].ValidFromDate = vanDatum;
            //portaal_content[0].ValidToDate = totDatum;
            //portaal_content[0].ValidFromDateSpecified = true;
            //portaal_content[0].ValidToDateSpecified = true;

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter[] portaal_meter = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter[1];

            portaal_content[0].Items = portaal_meter;
            portaal_meter[0] = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter();
            portaal_meter[0].ID = vastGesteldeStand.meternummer;// "1234";
            portaal_meter[0].NrOfRegisters = vastGesteldeStand.aantalregister;// "1";
            int intAantalRegister = int.Parse(vastGesteldeStand.aantalregister);

            int intAantalInsturen = vastGesteldeStand.intNrStanden; //Was voor samenvoegen: intNrRegisters
            //if (intNrStanden == 2 && intNrRegisters == 4)
            //{
            //    intAantalInsturen = 2;
            //}
            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register[] portaal_register = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register[intAantalInsturen];
            portaal_meter[0].Register = portaal_register;
            int nrExtraStand = 0;

            for (int intRegister = 0; intRegister < intAantalInsturen; intRegister++)
            {
                if (intRegister < vastGesteldeStand.intNrStanden)
                {
                    portaal_register[intRegister] = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register();
                    //portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.KWH;
                    portaal_register[intRegister].MeasureUnit = GetMeetEenheid(vastGesteldeStand.arrMeasureUnit[intRegister]);

                    // de direction weer toegevoegd. Moet zeker voor de meterberichten
                    portaal_register[intRegister].MeteringDirection = GetDirection(vastGesteldeStand.arrDirection[intRegister]);
                    portaal_register[intRegister].MeteringDirectionSpecified = true;

                    portaal_register[intRegister].NrOfDigits = vastGesteldeStand.arrNrDigits[intRegister];
                    if (vastGesteldeStand.product == "ELK")
                    {
                        portaal_register[intRegister].TariffType = GetTarifType(vastGesteldeStand.arrTarrifType[intRegister]);
                        portaal_register[intRegister].TariffTypeSpecified = true;
                    }

                    nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading portaal_reading = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading();

                    portaal_register[intRegister].Item = portaal_reading;

                    portaal_reading.Reading = vastGesteldeStand.arrReading[intRegister];
                    portaal_reading.ReadingDate = DateTime.ParseExact(vastGesteldeStand.arrReadingDate[intRegister], "yyyy-MM-dd", provider);
                    //portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003;

                    portaal_reading.ReadingMethod = GetHerkomst(vastGesteldeStand.herkomst);
                }
                else
                {
                    portaal_register[intRegister] = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register();
                    //portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.KWH;
                    portaal_register[intRegister].MeasureUnit = GetMeetEenheid(vastGesteldeStand.arrMeasureUnit[0]);

                    // de direction weer toegevoegd. Moet zeker voor de meterberichten
                    if (vastGesteldeStand.arrDirection[0] == "LVR")
                    {
                        portaal_register[intRegister].MeteringDirection = GetDirection("TLV");
                    }
                    else
                    {
                        portaal_register[intRegister].MeteringDirection = GetDirection("LVR");
                    }
                    portaal_register[intRegister].MeteringDirectionSpecified = true;

                    portaal_register[intRegister].NrOfDigits = vastGesteldeStand.arrNrDigits[0];
                    if (vastGesteldeStand.product == "ELK")
                    {
                        portaal_register[intRegister].TariffType = GetTarifType(vastGesteldeStand.arrTarrifType[nrExtraStand]);
                        nrExtraStand++;
                        portaal_register[intRegister].TariffTypeSpecified = true;
                    }

                    nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading portaal_reading = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading();

                    portaal_register[intRegister].Item = portaal_reading;

                    portaal_reading.Reading = "0";
                    portaal_reading.ReadingDate = DateTime.ParseExact(vastGesteldeStand.arrReadingDate[0], "yyyy-MM-dd", provider);
                    //portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003;

                    portaal_reading.ReadingMethod = GetHerkomst(vastGesteldeStand.herkomst);
                }
            }




            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_PM portaal_mutation = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_PM();
            portaal_content[0].Portaal_Mutation = portaal_mutation;

            if (vastGesteldeStand.product == "GAS")
            {
                portaal_mutation.Initiator = "8714252022926";
            }
            else
            {
                portaal_mutation.Initiator = KC.HoofdLV.ToString();
            }
            //portaal_mutation.Consumer = vastGesteldeStand.netbeheerder;// "1114252022907"; geen robin
            portaal_mutation.Consumer = "1119328025455";// "1114252022907";
            //portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MOVEIN;

            if (vastGesteldeStand.redenmutatie.ToUpper() == "ANNUAL") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.PERMTR; }
            //if (redenmutatie == "BULKPV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.BULKPV; }
            if (vastGesteldeStand.redenmutatie == "ALLMTCHG") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.ALLMTCHG; }
            if (vastGesteldeStand.redenmutatie == "CONNACT") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNACT; }
            if (vastGesteldeStand.redenmutatie == "CONNCHG") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNCHG; }
            if (vastGesteldeStand.redenmutatie == "CONNCRE") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNCRE; }
            if (vastGesteldeStand.redenmutatie == "CONNDACT") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNDACT; }
            if (vastGesteldeStand.redenmutatie == "CONNEND") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNEND; }
            if (vastGesteldeStand.redenmutatie == "CONNUPD") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNUPD; }
            if (vastGesteldeStand.redenmutatie == "CONSMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONSMTR; }
            if (vastGesteldeStand.redenmutatie == "DISPUTE") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.DISPUTE; }
            if (vastGesteldeStand.redenmutatie == "DSTRCONN") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.DSTRCONN; }
            if (vastGesteldeStand.redenmutatie == "DSTRMSTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.DSTRMSTR; }
            if (vastGesteldeStand.redenmutatie == "ENDOFMV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.ENDOFMV; }
            if (vastGesteldeStand.redenmutatie == "EOSUPPLY") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.EOSUPPLY; }
            if (vastGesteldeStand.redenmutatie == "HISTMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.HISTMTR; }
            if (vastGesteldeStand.redenmutatie == "MOVEIN") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MOVEIN; }
            if (vastGesteldeStand.redenmutatie == "MVI") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MOVEIN; }
            if (vastGesteldeStand.redenmutatie == "MOVEOUT" || vastGesteldeStand.redenmutatie.ToUpper() == "FINAL") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MOVEOUT; }
            if (vastGesteldeStand.redenmutatie == "MVO") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MOVEOUT; }
            if (vastGesteldeStand.redenmutatie == "MTREND") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MTREND; }
            if (vastGesteldeStand.redenmutatie == "MTRINST") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MTRINST; }
            if (vastGesteldeStand.redenmutatie == "MTRUPD") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MTRUPD; }
            if (vastGesteldeStand.redenmutatie == "NAMECHG") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.NAMECHG; }
            if (vastGesteldeStand.redenmutatie == "NMCRSCMP") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.NMCRSCMP; }
            if (vastGesteldeStand.redenmutatie == "PERMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.PERMTR; }
            if (vastGesteldeStand.redenmutatie == "PHYSMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.PHYSMTR; }
            if (vastGesteldeStand.redenmutatie == "RESCOMP") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.RESCOMP; }
            if (vastGesteldeStand.redenmutatie == "SWITCHLV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.SWITCHLV; }
            if (vastGesteldeStand.redenmutatie == "SWITCHMV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.SWITCHMV; }
            if (vastGesteldeStand.redenmutatie == "SWITCHPV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.SWITCHPV; }
            if (vastGesteldeStand.redenmutatie == "SWTCHUPD") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.SWTCHUPD; }

            if (vastGesteldeStand.dossierID != "")
            {
                nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_PM_Dossier portaal_dossier = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_PM_Dossier();
                portaal_mutation.Dossier = portaal_dossier;
                portaal_dossier.ID = vastGesteldeStand.dossierID;// "7325724";
            }


            //portaal_meter[0].Register

            nl.Energie.EDSN.MeterReading.MeterReading meterReading = new nl.Energie.EDSN.MeterReading.MeterReading();


            //String certPath = certpath + @"EDSN2013053100006.p12";
            if (strFileName == "")
            {
                meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
            }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            meterReading.Url = KC.CarUrl + @"synchroon/ResponderMeterReadingRespondingActivity";

            meterReading.Timeout = 120000;

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope retour = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope();



            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope));
            TextWriter WriteFileStream;

            if (strFileName == "")
            {

                // 8 nov bestandsaanvulling erbij )
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                { BestandsAanvulling = " LV " + BestandsAanvulling; }

                WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingExchangeNotification" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);

                WriteFileStream.Close();

                //string ftpResponse = "";
                //if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingExchangeNotification" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingExchangeNotification" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                //{
                //    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                //}

                StringWriter swXML = new StringWriter();
                serializer.Serialize(swXML, enveloppe);
                CarShared car = new CarShared();
                //int intOutBoxID = Save_Outbox(header.DocumentID, ean, "Stand", swXML.ToString());
                int intBericht_ID = car.Save_Bericht(4, @"MeterReadingExchangeNotification" + BestandsAanvulling + ".xml", "Verbruik : " + enveloppe.EDSNBusinessDocumentHeader.MessageID.ToString(), true, enveloppe.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false); ;
            }

            int intedinID = 0;

            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                BestandsAanvulling = " LV " + BestandsAanvulling; 
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope));
                XmlDocument _Doc = new XmlDocument();


                if (strFileName == "")
                {


                    retour = meterReading.MeterReadingExchangeNotification(enveloppe);

                    WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingResult" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    //string ftpResponse = "";
                    //if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingResult" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    //{
                    //    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                    //}

                    string ResultFileName = KC.XMLPath + @"MeterReadingResult" + BestandsAanvulling + ".xml";

                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);
                    _Doc.Load(KC.XMLPath + "MeterReadingResult" + BestandsAanvulling + ".xml");

                    //MessageBox.Show("Stand ingediend");
                }
                else
                {
                    retour = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope();

                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope));
                    //Save to file kan weg
                    WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingExchangeResponseEnvelope" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();
                }
                int inboxID = 0;
                //if (retour.Portaal_Content.Item.GetType() == typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope_Portaal_Content_EDSN_SimpleRejection))
                //{
                //    nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope_Portaal_Content_EDSN_SimpleRejection portaal_Rejection = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope_Portaal_Content_EDSN_SimpleRejection();
                //    portaal_Rejection = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope_Portaal_Content_EDSN_SimpleRejection)retour.Portaal_Content.Item;
                //    MessageBox.Show("Fout : " + portaal_Rejection.SimpleRejection.RejectionText);
                //}
                //else
                {
                    //int inboxID = 0;
                    CarShared car = new CarShared();
                    int Bericht_ID = car.Save_Bericht(26, @"MeterReadingExchangeResponseEnvelope" + BestandsAanvulling + ".xml", "Stand : " + header.DocumentID, true, retour.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);

                    //inboxID = Save_Inbox(27, _Doc.InnerXml.ToString(), "Stand : " + retour.EDSNBusinessDocumentHeader.MessageID.ToString(), blnBatch);

                    //intedinID = Save_Edine(inboxID, retour.EDSNBusinessDocumentHeader.Source.SenderID, retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID,
                    //        retour.EDSNBusinessDocumentHeader.CreationTimestamp, retour.EDSNBusinessDocumentHeader.MessageID, "UTILTS", "E11");
                    int intMeterStand_ID = Save_MeterStand_Header(intedinID, true, DateTime.Now, vastGesteldeStand.ean, vastGesteldeStand.vanDatum, vastGesteldeStand.totDatum,
                        vastGesteldeStand.netbeheerder, vastGesteldeStand.dossierID, vastGesteldeStand.redenmutatie, -1, false, "");
                    Boolean blnUpdate = false;
                    //if (meterStand_ID != -1) { blnUpdate = true; }
                    for (int intRegister = 0; intRegister < vastGesteldeStand.intNrStanden; intRegister++)
                    {

                        Save_MeterStand(intMeterStand_ID, Bericht_ID, vastGesteldeStand.herkomst, vastGesteldeStand.arrTarrifType[intRegister], vastGesteldeStand.arrReading[intRegister], vastGesteldeStand.arrDirection[intRegister], "0", "0", "", "0", blnUpdate);
                    }
                    //if (inboxID > 0) { ProcessMessage.processMessage(inboxID, ConnString); }
                }

                //if (inboxID > 0) { ProcessMessage.processMessage(inboxID); }
                //Nog check op rejection
                if (strFileName != "") { File.Delete(strFileName); }

                return inboxID;
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                carShared.SchrijfLog("Fout bij MeterreadingExchange : + " + vastGesteldeStand.ean + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, 0, KC.App_ID);
                strError = "Fout bij MeterreadingExchange : + " + vastGesteldeStand.ean + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();

            }
            catch (WebException exception)
            {
                carShared.SchrijfLog("Fout bij MeterreadingExchange : + " + vastGesteldeStand.ean + " - " + exception.Message, 10, 0, KC.App_ID);
                strError = "Fout bij MeterreadingExchange: " +  exception.Message;
            }
            catch (Exception exception)
            {
                carShared.SchrijfLog("Fout MeterreadingExchange GetMeteringMP : + " + vastGesteldeStand.ean + " - " + exception.Message, 10, 0, KC.App_ID);
                strError = "Fout bij MeterreadingExchange: " + exception.Message;
            }
            if (strFileName != "") { File.Delete(strFileName); }

            return 0;
        }

        public int Save_MeterStand_Header(int BerichtId, Boolean verstuurd, DateTime verstuurd_DT, string eanCode, DateTime begin_D,
            DateTime eind_D, String netbeheerder, string dossierID, string redenMutatie, int pMeterStand_ID, Boolean fout, string Referentie)
        {
            SqlConnection cnPubs = new SqlConnection(KC.ConnString);
            string SQLstatement;
            int meterStand_ID = -1;

            try
            {
                cnPubs.Open();
                if (pMeterStand_ID == -1)
                {
                    SQLstatement =
                            "INSERT INTO [Car].[dbo].[MeterStanden_Header] " +
                            "(BerichtId " +
                            ",[Verstuurd] ";
                    if (verstuurd_DT > DateTime.MinValue) { SQLstatement = SQLstatement + ",[VerstuurdDT]"; }
                    SQLstatement = SQLstatement + ",[EanCode] " +
                            ",[BeginD] " +
                            ",[EindD] " +
                            ",[Netbeheerder] " +
                            ",[DossierID] " +
                            ",[RedenMutatie] " +
                            ",[Fout] " +
                            ",Referentie) " +
                            "VALUES " +
                            "(@BerichtId " +
                            ",@Verstuurd ";
                    if (verstuurd_DT > DateTime.MinValue) { SQLstatement = SQLstatement + ",@VerstuurdDT "; }
                    SQLstatement = SQLstatement + ",@EanCode " +
                            ",@BeginD " +
                            ",@EindD " +
                            ",@Netbeheerder " +
                            ",@DossierID " +
                            ",@RedenMutatie " +
                            ",@Fout " +
                            ",@Referentie); SELECT @MeterstandId = SCOPE_IDENTITY();";
                }
                else
                {
                    SQLstatement = " UPDATE [Car].[dbo].[MeterStanden_Header] " +
                        "SET [BerichtId] = @BerichtId " +
                        ",[EanCode] = @EanCode " +
                        ",[Verstuurd] = @Verstuurd " +
                        ",[VerstuurdDT] = @Verstuurd_DT " +
                        ",[Begin_D] = @Begin_D " +
                        ",[Eind_D] = @Eind_D " +
                        ",[Netbeheerder] = @Netbeheerder " +
                        ",[DossierID] = @DossierID " +
                        ",[RedenMutatie] = @RedenMutatie " +
                        ",[Fout] = @Fout " +
                        ",Referentie = @Referentie " +
                        "WHERE MeterstandId=@MeterstandId";
                }
                SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
                cmdSaveInbox.Parameters.Add(new SqlParameter("@BerichtId", SqlDbType.Int));
                cmdSaveInbox.Parameters["@BerichtId"].Value = BerichtId;
                cmdSaveInbox.Parameters.Add(new SqlParameter("@Verstuurd", SqlDbType.Bit));
                cmdSaveInbox.Parameters["@Verstuurd"].Value = verstuurd;
                if (verstuurd_DT > DateTime.MinValue)
                {
                    cmdSaveInbox.Parameters.Add(new SqlParameter("@VerstuurdDT", SqlDbType.DateTime));
                    cmdSaveInbox.Parameters["@VerstuurdDT"].Value = verstuurd_DT;
                }
                cmdSaveInbox.Parameters.Add(new SqlParameter("@EanCode", SqlDbType.BigInt));
                cmdSaveInbox.Parameters["@EanCode"].Value = eanCode;
                cmdSaveInbox.Parameters.Add(new SqlParameter("@BeginD", SqlDbType.Date));
                cmdSaveInbox.Parameters["@BeginD"].Value = begin_D;
                cmdSaveInbox.Parameters.Add(new SqlParameter("@EindD", SqlDbType.Date));
                cmdSaveInbox.Parameters["@EindD"].Value = eind_D;
                cmdSaveInbox.Parameters.Add(new SqlParameter("@Netbeheerder", SqlDbType.BigInt));
                cmdSaveInbox.Parameters["@Netbeheerder"].Value = netbeheerder;
                cmdSaveInbox.Parameters.Add(new SqlParameter("@DossierID", SqlDbType.VarChar));
                cmdSaveInbox.Parameters["@DossierID"].Value = dossierID;
                cmdSaveInbox.Parameters.Add(new SqlParameter("@RedenMutatie", SqlDbType.VarChar));
                cmdSaveInbox.Parameters["@RedenMutatie"].Value = redenMutatie;
                //cmdSaveInbox.Parameters.Add(new SqlParameter("@DossierID", SqlDbType.VarChar));
                //cmdSaveInbox.Parameters["@dossierID"].Value = dossierID;
                cmdSaveInbox.Parameters.Add(new SqlParameter("@Fout", SqlDbType.Bit));
                cmdSaveInbox.Parameters["@Fout"].Value = fout;
                cmdSaveInbox.Parameters.Add(new SqlParameter("@Referentie", SqlDbType.VarChar));
                cmdSaveInbox.Parameters["@Referentie"].Value = Referentie;
                if (pMeterStand_ID != -1)
                {
                    cmdSaveInbox.Parameters.Add(new SqlParameter("@MeterstandId", SqlDbType.Int));
                    cmdSaveInbox.Parameters["@MeterstandId"].Value = pMeterStand_ID;
                    //cmdSaveInbox.Parameters["@Meterstand_ID"].Direction = ParameterDirection.Output;
                }
                else
                {
                    cmdSaveInbox.Parameters.Add(new SqlParameter("@MeterstandId", SqlDbType.Int));
                    cmdSaveInbox.Parameters["@MeterstandId"].Direction = ParameterDirection.Output;
                }
                try
                {
                    cmdSaveInbox.ExecuteNonQuery();
                    if (pMeterStand_ID == -1) { meterStand_ID = (int)cmdSaveInbox.Parameters["@MeterstandId"].Value; }
                    else { meterStand_ID = pMeterStand_ID; }
                    //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
                }
                catch (Exception ex)
                {
                    carShared.SchrijfLog("Fout bij wegeschrijven MeterStand_Header :  - " + eanCode + " - " + ex.ToString(), 10, 0, KC.App_ID);
                    //WriteLog("Fout bij wegeschrijven MeterStand_Header :  - " + eanCode + " - " + ex.ToString(), 10, BerichtId);
                    //MessageBox.Show("Er is iets fout gegaan met het bewaren van de MeterStand_Header, we adviseren U contact op te nemen met IT");
                    //MessageBox.Show(ex.ToString());
                }

                cnPubs.Close();
            }
            catch (Exception ex)
            {
                carShared.SchrijfLog("Fout bij wegeschrijven MeterStand_Header :  - " + eanCode + " - " + ex.ToString(), 10, 0, KC.App_ID);
                meterStand_ID = -1;
            }
            return meterStand_ID;

        }

        public int Save_MeterStand(int meterStand_ID, int BerichtId, string herkomst, string tarifType, string stand, string direction, string volume,
             string beginStand, string herkomstBeginStand, string calorificCorrectedVolume, Boolean update)
        {
            SqlConnection cnPubs = new SqlConnection(KC.ConnString);
            string SQLstatement;
            int Meterstand_Id = -1;

            cnPubs.Open();
            if (update == false)
            {
                SQLstatement =
                        "INSERT INTO [Car].[dbo].[MeterStand] " +
                        "([MeterstandId] " +
                        ",[BerichtId] " +
                        ",[Direction] " +
                        ",[TarifType] " +
                        ",[Herkomst] " +
                        ",[Stand] " +
                        ",[Volume] " +
                        ",[BeginStand] " +
                        ",[HerkomstBeginStand] " +
                        ",[CalorificCorrectedVolume]) " +
                        "VALUES " +
                        "(@MeterstandId " +
                        ",@BerichtId " +
                        ",@Direction " +
                        ",@TarifType " +
                        ",@Herkomst " +
                        ",@Stand " +
                        ",@Volume " +
                        ",@BeginStand " +
                        ",@HerkomstBeginStand " +
                        ",@CalorificCorrectedVolume)";
            }
            else
            {
                SQLstatement = "UPDATE [Car].[dbo].[MeterStand] " +
                    "SET [BerichtId] = @BerichtId " +
                    ",[Herkomst] = @Herkomst " +
                    ",[Stand] = @Stand " +
                    ",[Volume] = @Volume " +
                    ",[BeginStand] = @BeginStand " +
                    ",[HerkomstBeginStand] = @HerkomstBeginStand " +
                    ",[CalorificCorrectedVolume] = @CalorificCorrectedVolume " +
                    "WHERE [MeterStand_ID] = @MeterStand_ID and [Direction] = @Direction and [TarifType] = @TarifType";
            }
            SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveInbox.Parameters.Add(new SqlParameter("@MeterstandId", SqlDbType.Int));
            cmdSaveInbox.Parameters["@MeterstandId"].Value = meterStand_ID;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@BerichtId", SqlDbType.Int));
            cmdSaveInbox.Parameters["@BerichtId"].Value = BerichtId;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Direction", SqlDbType.Char));
            cmdSaveInbox.Parameters["@Direction"].Value = direction;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@TarifType", SqlDbType.Char));
            cmdSaveInbox.Parameters["@TarifType"].Value = tarifType;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Herkomst", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@Herkomst"].Value = herkomst;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Stand", SqlDbType.Decimal));
            cmdSaveInbox.Parameters["@Stand"].Value = stand;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Volume", SqlDbType.Decimal));
            cmdSaveInbox.Parameters["@Volume"].Value = volume;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@BeginStand", SqlDbType.Decimal));
            cmdSaveInbox.Parameters["@BeginStand"].Value = beginStand;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@HerkomstBeginStand", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@HerkomstBeginStand"].Value = herkomstBeginStand;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@CalorificCorrectedVolume", SqlDbType.Decimal));
            cmdSaveInbox.Parameters["@CalorificCorrectedVolume"].Value = calorificCorrectedVolume;



            try
            {
                cmdSaveInbox.ExecuteNonQuery();
                //inboxID = (int)cmdSaveInbox.Parameters["@inboxID"].Value;
                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
            }
            catch (Exception ex)
            {
                carShared.SchrijfLog("Fout bij wegeschrijven MeterStand (meterstand_ID):  - " + meterStand_ID + " - " + ex.ToString(), 10, 0, KC.App_ID);
                //MessageBox.Show("Er is iets fout gegaan met het bewaren van de MeterStand, we adviseren U contact op te nemen met IT");
                //MessageBox.Show(ex.ToString());
            }

            cnPubs.Close();
            return Meterstand_Id;

        }

        private nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode GetMeetEenheid(string MeetEenheid)
        {
            if (MeetEenheid == "KWH") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.KWH; }
            if (MeetEenheid == "KWT") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.KWT; }
            if (MeetEenheid == "M3N") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.M3N; }
            return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.MTQ;

        }

        private nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode GetHerkomst(string herkomst)
        {
            if (herkomst == "Item001") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item001; }
            if (herkomst == "Item002") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item002; }
            if (herkomst == "Fysieke Opname") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003; }
            if (herkomst == "003") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003; }
            if (herkomst == "P4-stand") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item004; }
            if (herkomst == "004") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item004; }
            if (herkomst == "Berekend") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item005; }
            if (herkomst == "005") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item005; }
            if (herkomst == "Item006") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item006; }
            if (herkomst == "Item009") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item009; }
            if (herkomst == "Overeengekomen") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item102; }
            if (herkomst == "102") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item102; }
            if (herkomst == "Klantstand/P1-Stand") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
            if (herkomst == "022") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
            if (herkomst == "22") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
            if (herkomst == "Item90") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item90; }
            if (herkomst == "Item91") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item91; }
            if (herkomst == "Item92") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item92; }
            if (herkomst == "Item93") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item93; }
            if (herkomst == "E01") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.E01; }
            return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22;
        }

        private nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyFlowDirectionCode GetDirection(String Direction)
        {
            if (Direction == "LVR") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyFlowDirectionCode.LVR; }
            if (Direction == "CMB") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyFlowDirectionCode.CMB; }
            return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyFlowDirectionCode.TLV;
        }
        private nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyTariffTypeCode GetTarifType(String TarifType)
        {
            if (TarifType == "N") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyTariffTypeCode.N; }
            if (TarifType == "H") { return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyTariffTypeCode.N; }
            return nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyTariffTypeCode.L;
        }

        public String  OphalenRejections(string strFileName, string netbeheerder, Boolean blnBatch)//, DateTime vanDatum, DateTime totDatum)
        {
            string OldFileName = strFileName;
            string strError = "";
            
            nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope enveloppe = new nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope();


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "DDM_O";
            receiver.ReceiverID = netbeheerder;// "1114252022907";
            destination.Receiver = receiver;

            nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            source.SenderID = KC.HoofdLV.ToString();

            source.ContactTypeIdentifier = "DDQ_O";
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.MeterReading.MeterReading meterReading = new nl.Energie.EDSN.MeterReading.MeterReading();


            //String certPath = certpath + @"EDSN2013053100006.p12";
            if (strFileName == "")
            {
                meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
                //meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV));
            }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            meterReading.Url = KC.CarUrl + @"synchroon/ResponderMeterReadingRespondingActivity";

            meterReading.Timeout = 120000;

            nl.Energie.EDSN.MeterReading.MeterReadingRejectionResponseEnvelope retour = new nl.Energie.EDSN.MeterReading.MeterReadingRejectionResponseEnvelope();



            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope));
            TextWriter WriteFileStream;

            if (strFileName == "")
            {

                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingRejections" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();



                StringWriter swXML = new StringWriter();
                serializer.Serialize(swXML, enveloppe);
                //Opslaan niet zinvol.
                //int intOutBoxID = carShared.Save_Bericht(1, swXML.ToString(), "OphalenRejection", false, "", true, false);

            }

            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                BestandsAanvulling = " LV " + BestandsAanvulling;
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingRejectionResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();

                if (strFileName == "")
                {
                    retour = meterReading.MeterReadingRejectionRequest(enveloppe);

                    WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingRejections" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();



                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);
                    _Doc.Load(KC.XMLPath + "MeterReadingRejections" + BestandsAanvulling + ".xml");

                    if (retour.Portaal_Content.Length == 0)
                    {
                        File.Delete(KC.XMLPath + "MeterReadingRejections" + BestandsAanvulling + ".xml");
                    }

                }
                else
                {
                    //retour = new nl.Energie.EDSN.MeterReading.MeterReadingRejectionResponseEnvelope();
                    _Doc = new XmlDocument();
                    _Doc.Load(strFileName);

                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingRejectionResponseEnvelope));
                    retour = (nl.Energie.EDSN.MeterReading.MeterReadingRejectionResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
                }





                nl.Energie.EDSN.MeterReading.MeterReadingRejectionResponseEnvelope_PC_PR[] rejections = new nl.Energie.EDSN.MeterReading.MeterReadingRejectionResponseEnvelope_PC_PR[retour.Portaal_Content.Length];
                rejections = retour.Portaal_Content;

                foreach (nl.Energie.EDSN.MeterReading.MeterReadingRejectionResponseEnvelope_PC_PR rejection in rejections)
                {
                    //int inboxID = 0;
                    //inboxID = Save_Inbox(27, _Doc.InnerXml.ToString(), "Rejections : " + retour.EDSNBusinessDocumentHeader.MessageID.ToString(), blnBatch);
                    int BerichtID = carShared.Save_Bericht(2, _Doc.InnerXml.ToString(), "Rejections : " + retour.EDSNBusinessDocumentHeader.MessageID.ToString(), true, retour.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);

                    //int intedinID = Save_Edine(inboxID, retour.EDSNBusinessDocumentHeader.Source.SenderID, retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID,
                    //        retour.EDSNBusinessDocumentHeader.CreationTimestamp, retour.EDSNBusinessDocumentHeader.MessageID, "UTILTS", "E11");

                    string ean = rejection.Portaal_MeteringPoint.EANID.ToString();
                    string product = rejection.Portaal_MeteringPoint.ProductType.ToString();
                    string initiator = rejection.Portaal_MeteringPoint.Portaal_Mutation.Initiator.ToString();
                    string mutationReason = rejection.Portaal_MeteringPoint.Portaal_Mutation.MutationReason.ToString();
                    string dossier = "";
                    if (rejection.Portaal_MeteringPoint.Portaal_Mutation.Dossier != null) { dossier = rejection.Portaal_MeteringPoint.Portaal_Mutation.Dossier.ID.ToString(); }
                    string rejectionCode = rejection.Rejection.RejectionCode.ToString();
                    string rejectionText = "";
                    if (rejection.Rejection.RejectionText != null) { rejectionText = rejection.Rejection.RejectionText.ToString(); }

                    Save_MeterStand_Afwijzing(BerichtID, ean, initiator, product, mutationReason, dossier, rejectionCode, rejectionText);

                    //if (inboxID > 0) { ProcessMessage.processMessage(inboxID); }
                }

                //Nog check op rejection


            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                carShared.SchrijfLog("Fout bij MeterReadingRejection " + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, 0, KC.App_ID);
                strError = "Error in rejections: " + S.ErrorCode.ToString() + " " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();

            }
            catch (WebException exception)
            {
                carShared.SchrijfLog("Fout bij MeterReadingRejection" + " - " + exception.Message, 10, 0, KC.App_ID);
                strError = "Error in recjections: " + exception.Message;
                //strResult = "Fout bij MeterreadingExchange : + " + eancode + " - " + exception.Message;
            }
            catch (Exception exception)
            {
                carShared.SchrijfLog("Fout MeterReadingRejection" + " - " + exception.Message, 10, 0, KC.App_ID);
                strError = "Error in recjections: " + exception.Message;
                //strResult = "Fout bij MeterreadingExchange : + " + eancode + " - " + exception.Message;
            }
            if (strFileName != "") { File.Delete(strFileName); }

            return strError;
        }

        public int Save_MeterStand_Afwijzing(int BerichtId, string ean, string initiator, string product, string mutationReason,
            string dossierID, string rejectionCode, string rejectionText)
        {
            SqlConnection cnPubs = new SqlConnection(KC.ConnString);
            string SQLstatement;
            int inboxID = -1;

            cnPubs.Open();
            SQLstatement =
                    "INSERT INTO [Messages].[dbo].[MeterStand_Afwijzing] " +
                    "([BerichtId] " +
                    ",[Ean18_Code] " +
                    ",[Product] " +
                    ",[Initiator] " +
                    ",[MutationReason] " +
                    ",[DossierID] " +
                    ",[RejectionCode] " +
                    ",[RejectionText]) " +
                    "VALUES " +
                    "(@BerichtId " +
                    ",@Ean18_Code " +
                    ",@Product " +
                    ",@Initiator " +
                    ",@MutationReason " +
                    ",@DossierID " +
                    ",@RejectionCode " +
                    ",@RejectionText)";


            SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveInbox.Parameters.Add(new SqlParameter("@BerichtId", SqlDbType.Int));
            cmdSaveInbox.Parameters["@BerichtId"].Value = BerichtId;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Ean18_Code", SqlDbType.BigInt));
            cmdSaveInbox.Parameters["@Ean18_Code"].Value = ean;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Product", SqlDbType.Char));
            cmdSaveInbox.Parameters["@Product"].Value = product;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Initiator", SqlDbType.BigInt));
            cmdSaveInbox.Parameters["@Initiator"].Value = initiator;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@MutationReason", SqlDbType.NVarChar));
            cmdSaveInbox.Parameters["@MutationReason"].Value = mutationReason;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@DossierID", SqlDbType.NVarChar));
            cmdSaveInbox.Parameters["@DossierID"].Value = dossierID;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@RejectionCode", SqlDbType.Char));
            cmdSaveInbox.Parameters["@RejectionCode"].Value = rejectionCode;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@RejectionText", SqlDbType.NVarChar));
            cmdSaveInbox.Parameters["@RejectionText"].Value = rejectionText;


            try
            {
                cmdSaveInbox.ExecuteNonQuery();
                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
            }
            catch //(Exception ex)
            {
                //MessageBox.Show("Er is iets fout gegaan met het bewaren van het Meterstand rejection, we adviseren U contact op te nemen met IT");
                //MessageBox.Show(ex.ToString());
            }

            cnPubs.Close();
            return inboxID;

        }

        public void IndienenDispuut(Boolean blnPV, string strFileName, Dispuut dispuut, Boolean blnBatch)
        {
            string OldFileName = strFileName;
            if (strFileName != "")
            {
                string text = File.ReadAllText(strFileName);
                text = text.Replace("<EDSNDocument>", @"<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "utf-8" + '"' + "?>");
                text = text.Replace("</EDSNDocument>", "");
                text = text.Replace("xmlns=" + '"' + "urn:nedu:edsn:data:meterreadingexchangeresponse:1:standard" + '"', "xmlns:xsi=" + '"' + "http://www.w3.org/2001/XMLSchema-instance" + '"' + " xmlns:xsd=" + '"' + "http://www.w3.org/2001/XMLSchema" + '"');
                text = text.Replace("<EDSNBusinessDocumentHeader>", "<EDSNBusinessDocumentHeader xmlns=" + '"' + "urn:nedu:edsn:data:meterreadingexchangeresponse:1:standard" + '"' + ">");
                text = text.Replace("<Portaal_Content>", "<Portaal_Content xmlns=" + '"' + "urn:nedu:edsn:data:meterreadingexchangeresponse:1:standard" + '"' + ">");
                strFileName = strFileName.Trim() + ".tmp";
                File.WriteAllText(strFileName, text);
            }

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope enveloppe = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope();


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "DDM_O";
            receiver.ReceiverID = dispuut.netbeheerder;
            destination.Receiver = receiver;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EDSNBusinessDocumentHeader_Source();
            source.SenderID = KC.HoofdLV.ToString();
            source.ContactTypeIdentifier = "DDQ_O";
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC portaal_content = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP portaal_meteringPoint = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP();
            portaal_content.Portaal_MeteringPoint = portaal_meteringPoint;
            portaal_meteringPoint.EANID = dispuut.ean;
            //portaal_meteringPoint.ProductType = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyProductPortaalTypeCode.ELK;
            if (dispuut.product == "ELK") { portaal_meteringPoint.ProductType = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyProductPortaalTypeCode.ELK; }
            else { portaal_meteringPoint.ProductType = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyProductPortaalTypeCode.GAS; }

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter portaal_meter = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter();

            portaal_content.Portaal_MeteringPoint.Portaal_EnergyMeter = portaal_meter;
            portaal_meter.ID = dispuut.meternummer;
            portaal_meter.NrOfRegisters = dispuut.aantalregister;
            int intAantalRegister = int.Parse(dispuut.aantalregister);

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register[] portaal_register = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register[2];

            portaal_meter.Register = portaal_register;

            portaal_register[0] = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register();
            //portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.KWH;

            if (dispuut.meeteindheidNormaal == "KWH") { portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.KWH; }
            if (dispuut.meeteindheidNormaal == "KWT") { portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.KWT; }
            if (dispuut.meeteindheidNormaal == "M3N") { portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.M3N; }
            if (dispuut.meeteindheidNormaal == "MTQ") { portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.MTQ; }

            //portaal_register[0].MeteringDirection = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyFlowDirectionCode.LVR;
            //portaal_register[0].MeteringDirectionSpecified = true;
            portaal_register[0].NrOfDigits = dispuut.nrTelwerkenNormaal;
            portaal_register[0].TariffType = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyTariffTypeCode.N;
            portaal_register[0].TariffTypeSpecified = true;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading portaal_reading = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading();

            portaal_register[0].Reading = portaal_reading;
            portaal_reading.Reading = dispuut.standNormaal;
            portaal_reading.ReadingDate = dispuut.vanDatum;
            //portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003;

            if (dispuut.herkomst == "Item001") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item001; }
            if (dispuut.herkomst == "Item002") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item002; }
            if (dispuut.herkomst == "Fysieke Opname") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003; }
            if (dispuut.herkomst == "003") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003; }
            if (dispuut.herkomst == "P4-stand") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item004; }
            if (dispuut.herkomst == "004") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item004; }
            if (dispuut.herkomst == "Berekend") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item005; }
            if (dispuut.herkomst == "005") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item005; }
            if (dispuut.herkomst == "Item006") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item006; }
            if (dispuut.herkomst == "Item009") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item009; }
            if (dispuut.herkomst == "Overeengekomen") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item102; }
            if (dispuut.herkomst == "102") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item102; }
            if (dispuut.herkomst == "Klantstand/P1-Stand") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
            if (dispuut.herkomst == "022") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
            if (dispuut.herkomst == "22") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
            if (dispuut.herkomst == "Item90") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item90; }
            if (dispuut.herkomst == "Item91") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item91; }
            if (dispuut.herkomst == "Item92") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item92; }
            if (dispuut.herkomst == "Item93") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item93; }
            if (dispuut.herkomst == "E01") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.E01; }
            if (dispuut.herkomst == "E02") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.E02; }

            if (intAantalRegister == 2)
            {
                portaal_register[1] = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register();
                //portaal_register[1].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.KWH;

                if (dispuut.meeteindheidLaag == "KWH") { portaal_register[1].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.KWH; }
                if (dispuut.meeteindheidLaag == "KWT") { portaal_register[1].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.KWT; }
                if (dispuut.meeteindheidLaag == "M3N") { portaal_register[1].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.M3N; }
                if (dispuut.meeteindheidLaag == "MTQ") { portaal_register[1].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.MTQ; }

                //portaal_register[1].MeteringDirection = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyFlowDirectionCode.LVR;
                //portaal_register[1].MeteringDirectionSpecified = true;
                portaal_register[1].NrOfDigits = dispuut.nrTelwerkenLaag;
                portaal_register[1].TariffType = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyTariffTypeCode.L;
                portaal_register[1].TariffTypeSpecified = true;

                nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading portaal_reading2 = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading();

                portaal_register[1].Reading = portaal_reading2;
                portaal_reading2.Reading = dispuut.standLaag;
                portaal_reading2.ReadingDate = dispuut.vanDatum;
                portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003;
                if (dispuut.herkomst == "Item001") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item001; }
                if (dispuut.herkomst == "Item002") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item002; }
                if (dispuut.herkomst == "Fysieke Opname") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003; }
                if (dispuut.herkomst == "003") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003; }
                if (dispuut.herkomst == "P4-stand") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item004; }
                if (dispuut.herkomst == "004") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item004; }
                if (dispuut.herkomst == "Berekend") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item005; }
                if (dispuut.herkomst == "005") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item005; }
                if (dispuut.herkomst == "Item006") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item006; }
                if (dispuut.herkomst == "Item009") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item009; }
                if (dispuut.herkomst == "Overeengekomen") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item102; }
                if (dispuut.herkomst == "102") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item102; }
                if (dispuut.herkomst == "Klantstand/P1-Stand") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
                if (dispuut.herkomst == "022") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
                if (dispuut.herkomst == "22") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
                if (dispuut.herkomst == "Item90") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item90; }
                if (dispuut.herkomst == "Item91") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item91; }
                if (dispuut.herkomst == "Item92") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item92; }
                if (dispuut.herkomst == "Item93") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item93; }
                if (dispuut.herkomst == "E01") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.E01; }
                if (dispuut.herkomst == "E02") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.E02; }
            }

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_PM portaal_mutation = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_PM();
            portaal_content.Portaal_MeteringPoint.Portaal_Mutation = portaal_mutation;
            portaal_mutation.Initiator = KC.HoofdLV.ToString();
            // portaal_mutation.Consumer = "1114252022907"; geen robin
            portaal_mutation.Consumer = "1119328025455";
            //portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.DISPUTE;

            //if (redenmutatie == "BULKPV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.BULKPV; }
            //if (redenmutatie == "CONNACT") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.CONNACT; }
            //if (redenmutatie == "CONNCHG") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.CONNCHG; }
            //if (redenmutatie == "CONNCRE") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.CONNCRE; }
            //if (redenmutatie == "CONNDACT") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.CONNDACT; }
            //if (redenmutatie == "CONNEND") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.CONNEND; }
            //if (redenmutatie == "CONNUPD") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.CONNUPD; }
            //if (redenmutatie == "CONSMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.CONSMTR; }
            //if (redenmutatie == "DISPUTE") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.DISPUTE; }
            //if (redenmutatie == "DSTRCONN") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.DSTRCONN; }
            //if (redenmutatie == "DSTRMSTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.DSTRMSTR; }
            //if (redenmutatie == "ENDOFMV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.ENDOFMV; }
            //if (redenmutatie == "EOSUPPLY") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.EOSUPPLY; }
            //if (redenmutatie == "HISTMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.HISTMTR; }
            //if (redenmutatie == "MOVEIN") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.MOVEIN; }
            //if (redenmutatie == "MOVEOUT") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.MOVEOUT; }
            //if (redenmutatie == "MTREND") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.MTREND; }
            //if (redenmutatie == "MTRINST") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.MTRINST; }
            //if (redenmutatie == "MTRUPD") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.MTRUPD; }
            //if (redenmutatie == "NAMECHG") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.NAMECHG; }
            //if (redenmutatie == "NMCRSCMP") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.NMCRSCMP; }
            //if (redenmutatie == "PERMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.PERMTR; }
            //if (redenmutatie == "PHYSMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.PHYSMTR; }
            //if (redenmutatie == "RESCOMP") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.RESCOMP; }
            //if (redenmutatie == "SWITCHLV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.SWITCHLV; }
            //if (redenmutatie == "SWITCHMV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.SWITCHMV; }
            //if (redenmutatie == "SWITCHPV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.SWITCHPV; }
            //if (redenmutatie == "SWTCHUPD") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.SWTCHUPD; }
            portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MutationReasonPortaalCode.DISPUTE;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_PM_Dossier portaal_dossier = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_PM_Dossier();
            portaal_mutation.Dossier = portaal_dossier;
            portaal_dossier.ID = dispuut.dossierID;


            //portaal_meter[0].Register

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDispute meterReadingDispute = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDispute();


            //String certPath = certpath + @"EDSN2013053100006.p12";
            if (strFileName == "")
            {
                meterReadingDispute.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
            }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            meterReadingDispute.Url = KC.CarUrl + @"synchroon/ResponderMeterReadingDisputeRespondingActivity";

            meterReadingDispute.Timeout = 120000;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeAcknowledgementEnvelope retour = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeAcknowledgementEnvelope();



            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope));
            TextWriter WriteFileStream;

            CarShared car = new CarShared();

            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingDisputeNotification" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();



                StringWriter swXML = new StringWriter();
                serializer.Serialize(swXML, enveloppe);
                //int intOutBoxID = Save_Outbox(header.DocumentID, dispuut.ean, "Dispuut", swXML.ToString());

                int intOutBoxID = car.Save_Bericht(4, @"MeterReadingExchangeNotification" + BestandsAanvulling + ".xml", "Dispuut : " + enveloppe.EDSNBusinessDocumentHeader.MessageID.ToString(), true, enveloppe.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false); ;

            }

            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeAcknowledgementEnvelope));
                XmlDocument _Doc = new XmlDocument();

                if (strFileName == "")
                {

                    retour = meterReadingDispute.MeterReadingDisputeNotification(enveloppe);

                    WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingDisputeNotification" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    //string ftpResponse = "";
                    //if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingDisputeNotification" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingDisputeNotification" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    //{
                    //    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                    //}

                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);
                    _Doc.Load(KC.XMLPath + "MeterReadingDisputeNotification" + BestandsAanvulling + ".xml");

                    //MessageBox.Show("Dispuut verstuurd");
                }
                else
                {
                    retour = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeAcknowledgementEnvelope();

                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeAcknowledgementEnvelope));
                    //Save to file kan weg
                    WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingDisputeNotification" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();
                }


                int intBericht_ID = car.Save_Bericht(26, @"MeterReadingExchangeResponseEnvelope" + BestandsAanvulling + ".xml", "Dispuut : " + header.DocumentID, true, retour.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);

                //inboxID = Save_Inbox(39, _Doc.InnerXml.ToString(), "Dispuut : " + retour.EDSNBusinessDocumentHeader.MessageID.ToString(), blnBatch);
                //int intedinID = Save_Edine(inboxID, retour.EDSNBusinessDocumentHeader.Source.SenderID, retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID,
                //        retour.EDSNBusinessDocumentHeader.CreationTimestamp, retour.EDSNBusinessDocumentHeader.MessageID, "DISPUU", "E11");
                Save_Dispuut(intBericht_ID, DateTime.Now, dispuut, false, header.MessageID, true, "", false, blnBatch);
                //if (inboxID > 0) { ProcessMessage.processMessage(inboxID, ConnString); }
                //Nog check op rejection


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
            if (strFileName != "") { File.Delete(strFileName); }
        }

        public string OphalenDispuut(string strFileName, string netbeheerder, Boolean blnBatch)
        {
            string OldFileName = strFileName;
            string strError = "";

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope enveloppe = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope();

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "DDM_O";
            receiver.ReceiverID = netbeheerder;
            destination.Receiver = receiver;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            source.SenderID = KC.HoofdLV.ToString();
            source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;


            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_content;



            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDispute meterReadingDispute = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDispute();


            if (strFileName == "")
            {
                meterReadingDispute.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
            }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            meterReadingDispute.Url = KC.CarUrl + @"synchroon/ResponderMeterReadingDisputeRespondingActivity";

            meterReadingDispute.Timeout = 120000;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope response = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope();

            CarShared car = new CarShared();

            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope));
            TextWriter WriteFileStream;

            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                BestandsAanvulling = " LV " + BestandsAanvulling;
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();

                if (strFileName == "")
                {

                    response = meterReadingDispute.MeterReadingDisputeRequest(enveloppe);

                    WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingDisputeResponse" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, response);
                    WriteFileStream.Close();


                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, response);
                    _Doc.Load(KC.XMLPath + "MeterReadingDisputeResponse" + BestandsAanvulling + ".xml");

                    //MessageBox.Show("Dispuut opgehaald");
                }
                else
                {
                    response = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope();

                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope));
                    //Save to file kan weg
                    WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingDisputeResponse" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, response);
                    WriteFileStream.Close();
                }



                int BerichtId = 0;
                if (response.Portaal_Content.Length > 0)
                {
                    BerichtId = car.Save_Bericht(9, KC.XMLPath + "MeterReadingDisputeResponse" + BestandsAanvulling + ".xml", "Dispuut : " + header.DocumentID, true, response.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);

                    nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope_PC_PMP[] portaal_MeteringPoints = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope_PC_PMP[response.Portaal_Content.Length];
                    portaal_MeteringPoints = response.Portaal_Content;

                    foreach (nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope_PC_PMP meteringpoint in portaal_MeteringPoints)
                    {
                        string ean = meteringpoint.EANID;

                        string Dossier = meteringpoint.Portaal_Mutation.Dossier.ID;
                        int nrRegisters = int.Parse(meteringpoint.Portaal_EnergyMeter.NrOfRegisters);

                        string StandNormaal = "";
                        string StandLaag = "";
                        string referentie = meteringpoint.Portaal_Mutation.ExternalReference;
                        DateTime Datum = DateTime.MinValue;
                        string herkomstEDSN = "";
                        //string netbeheerder = meteringpoint.Portaal_Mutation.Initiator;

                        foreach (nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register register in meteringpoint.Portaal_EnergyMeter.Register)
                        {
                            if (register.TariffType == nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope_EnergyTariffTypeCode.N ||
                                register.TariffType == nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope_EnergyTariffTypeCode.T)
                            {
                                StandNormaal = register.Reading.Reading;
                                Datum = register.Reading.ReadingDate;
                                herkomstEDSN = register.Reading.ReadingMethod.ToString();

                            }
                            else
                            {
                                StandLaag = register.Reading.Reading;
                                Datum = register.Reading.ReadingDate;
                                herkomstEDSN = register.Reading.ReadingMethod.ToString();
                            }
                        }
                        string herkomst = "";
                        if (herkomstEDSN == "Item003") { herkomst = "Fysieke Opname"; }
                        if (herkomstEDSN == "Item004") { herkomst = "P4-stand"; }
                        if (herkomstEDSN == "Item005") { herkomst = "Berekend"; }
                        if (herkomstEDSN == "Item102") { herkomst = "Overeengekomen"; }
                        if (herkomstEDSN == "Item22") { herkomst = "Klantstand/P1-Stand"; }


                        Save_Dispuut_Inkomend(BerichtId, DateTime.Now, ean, Datum, Datum, netbeheerder, Dossier, meteringpoint.Portaal_Mutation.MutationReason.ToString(),
                            herkomst, StandNormaal, StandLaag, false, referentie, false, "", false, blnBatch);
                    }

                    if (BerichtId > 0) { ProcessMessage.processMessage(BerichtId, ConnString); }
                    //Nog check op rejection


                }
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
                strError = "Error in ophalen dispuut: " + S.ErrorCode.ToString() + "-" + S.ErrorDetails + "-" + S.ErrorText + "-" + ex.Detail.InnerXml.ToString();
                carShared.SchrijfLog("Error in ophalen dispuut: " + S.ErrorCode.ToString() + "-" + S.ErrorDetails + "-" + S.ErrorText + "-" + ex.Detail.InnerXml.ToString(), 10, -1, KC.App_ID);
            }
            catch (WebException exception)
            {
                if (!blnBatch)
                {
                    MessageBox.Show(exception.Message);
                }
                strError = "Error in ophalen dispuut: " + exception.Message;
                carShared.SchrijfLog("Error in ophalen dispuut: " + exception.Message, 10, -1, KC.App_ID);
            }
            catch (Exception exception)
            {
                if (!blnBatch)
                {
                    MessageBox.Show(exception.Message);
                }
                strError = "Error in ophalen dispuut: " + exception.Message;
                carShared.SchrijfLog("Error in ophalen dispuut: " + exception.Message, 10, -1, KC.App_ID);
            }
            if (strFileName != "") { File.Delete(strFileName); }
            return strError;
        }

        public void TerugkoppelenDispuut(Boolean blnPV, string strFileName, string netbeheerder, string ean, string referentie, string Dossier,
            Boolean geaccepteerd, string Opmerking, int BerichtId, Boolean blnBatch)
        {
            
            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope enveloppe = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope();


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "DDM_O";
            // receiver.ReceiverID = netbeheerder; moet volgens mij EDSN zijn
            receiver.ReceiverID = "8712423010208";
            //
            destination.Receiver = receiver;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_EDSNBusinessDocumentHeader_Source();
            if (blnPV != true) { source.SenderID = KC.HoofdLV.ToString(); } else { source.SenderID = KC.HoofdPV.ToString(); }
            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_Portaal_Content_Portaal_MeteringPoint[] portaal_content = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_Portaal_Content_Portaal_MeteringPoint[1];
            enveloppe.Portaal_Content = portaal_content;

            portaal_content[0] = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_Portaal_Content_Portaal_MeteringPoint();
            portaal_content[0].EANID = ean;
            portaal_content[0].Result = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_Portaal_Content_Portaal_MeteringPoint_Result();
            if (geaccepteerd) { portaal_content[0].Result.Accepted = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_YesNoCode.J; }
            else { portaal_content[0].Result.Accepted = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_YesNoCode.N; }
            if (Opmerking != "") { portaal_content[0].Result.CommentResult = Opmerking; }
            portaal_content[0].Portaal_Mutation = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation();
            portaal_content[0].Portaal_Mutation.Consumer = netbeheerder;
            if (referentie != "")
            {
                portaal_content[0].Portaal_Mutation.ExternalReference = referentie;
            }
            portaal_content[0].Portaal_Mutation.Initiator = KC.HoofdLV.ToString();
            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation_Dossier portaal_Dossier = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation_Dossier();
            portaal_Dossier.ID = Dossier;
            portaal_content[0].Portaal_Mutation.Dossier = portaal_Dossier;



            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDispute meterReadingDispute = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDispute();


            //String certPath = certpath + @"EDSN2013053100006.p12";
            if (strFileName == "")
            {
                if (blnPV == true)
                {
                    //certPath = certpath + @"EDSN2013053100007.p12";
                    meterReadingDispute.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertPV, KC.CertPVPassword));
                }
                else
                {
                    meterReadingDispute.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
                    //meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV));
                }
            }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            meterReadingDispute.Url = KC.CarUrl + @"synchroon/ResponderMeterReadingDisputeRespondingActivity";

            meterReadingDispute.Timeout = 120000;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultAcknowledgementEnvelope retour = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultAcknowledgementEnvelope();



            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope));
            TextWriter WriteFileStream;

            CarShared car = new CarShared();

            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingDisputeResult" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();

               

                StringWriter swXML = new StringWriter();
                serializer.Serialize(swXML, enveloppe);

                

            }

            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultAcknowledgementEnvelope));
                XmlDocument _Doc = new XmlDocument();

                if (strFileName == "")
                {

                    retour = meterReadingDispute.MeterReadingDisputeResultNotification(enveloppe);

                    WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingDisputeResult" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    
                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);
                    _Doc.Load(KC.XMLPath + "MeterReadingDisputeResult" + BestandsAanvulling + ".xml");

                    //MessageBox.Show("Dispuut Teruggekoppeld");
                }
                else
                {
                    retour = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultAcknowledgementEnvelope();

                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultAcknowledgementEnvelope));
                    //Save to file kan weg
                    WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingDisputeResult" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();
                }


                //if (retour.Portaal_Content.GetType == typeof(nl.Energie.EDSN.MeterReadingDispute.
                int intBericht_ID = car.Save_Bericht(10, KC.XMLPath + "MeterReadingDisputeResult" + BestandsAanvulling + ".xml", "Dispuut : " + header.DocumentID, true, retour.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);
                int inboxID = 0;

                

                if (inboxID > 0) { ProcessMessage.processMessage(inboxID, ConnString); }
                //Nog check op rejection


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
            if (strFileName != "") { File.Delete(strFileName); }
        }

        public void OphalenTerugKoppelingen(Boolean blnPV, string strFileName, string netbeheerder, Boolean blnBatch)
        {
           
            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope enveloppe = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope();


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(KC.ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "DDM_O";
            //receiver.ReceiverID = netbeheerder;
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            if (blnPV != true) { source.SenderID = KC.HoofdLV.ToString(); } else { source.SenderID = KC.HoofdPV.ToString(); }
            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_content;



            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDispute meterReadingDispute = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDispute();


            //String certPath = certpath + @"EDSN2013053100006.p12";
            if (strFileName == "")
            {
                if (blnPV == true)
                {
                    //certPath = certpath + @"EDSN2013053100007.p12";
                    meterReadingDispute.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertPV, KC.CertPVPassword));
                }
                else
                {
                    meterReadingDispute.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(KC.CertLV, KC.CertLVPassword));
                    //meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV));
                }
            }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            meterReadingDispute.Url = KC.CarUrl + @"synchroon/ResponderMeterReadingDisputeRespondingActivity";

            meterReadingDispute.Timeout = 120000;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultResponseEnvelope retour = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultResponseEnvelope();



            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope));
            TextWriter WriteFileStream;

            CarShared car = new CarShared();

            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingDisputeResultRequest" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();

                StringWriter swXML = new StringWriter();
                serializer.Serialize(swXML, enveloppe);

            }

            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();

                if (strFileName == "")
                {

                    retour = meterReadingDispute.MeterReadingDisputeResultRequest(enveloppe);

                    WriteFileStream = new StreamWriter(KC.XMLPath + @"MeterReadingDisputeResultRequest" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    

                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);
                    _Doc.Load(KC.XMLPath + "MeterReadingDisputeResultRequest" + BestandsAanvulling + ".xml");

                    //MessageBox.Show("TerugKoppelingen opgehaald");
                }
                else
                {
                    retour = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultResponseEnvelope();

                    _Doc = new XmlDocument();
                    _Doc.Load(strFileName);
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultResponseEnvelope));
                    retour = (nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

                    
                }



                int inboxID = 0;
                int intBericht_ID = car.Save_Bericht(39, KC.XMLPath + "Terugkoppeling" + BestandsAanvulling + ".xml", "Dispuut : " + header.DocumentID, true, retour.EDSNBusinessDocumentHeader.MessageID.ToString(), false, false);
                //inboxID = Save_Inbox(39, _Doc.InnerXml.ToString(), "Terugkoppeling : " + retour.EDSNBusinessDocumentHeader.MessageID.ToString(), blnBatch);

                nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint[] portaal_MeteringPoints = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint[retour.Portaal_Content.Length];
                portaal_MeteringPoints = retour.Portaal_Content;

                foreach (nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint meteringpoint in portaal_MeteringPoints)
                {
                    string ean = meteringpoint.EANID;

                    string Dossier = meteringpoint.Portaal_Mutation.Dossier.ID;
                    Boolean blnGeaccepteerd = false;
                    if (meteringpoint.Result.Accepted == nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultResponseEnvelope_YesNoCode.J)
                    { blnGeaccepteerd = true; }

                    String Opmerking = "";
                    try
                    {
                        Opmerking = meteringpoint.Result.CommentResult;
                    }
                    catch { }

                    string referentie = "";
                    try
                    {
                        referentie = meteringpoint.Portaal_Mutation.ExternalReference;
                    }
                    catch { }
                    DateTime Datum = DateTime.MinValue;
                    //string herkomstEDSN = "";
                    int BerichtId = 0;

                    try
                    {
                        SqlConnection cnPubs = new SqlConnection(KC.ConnString);
                        cnPubs.Open();
                        string SQLStatement = "SELECT  BerichtId FROM Car.dbo.Dispuut where EanCode=@EanCode and Afgehandeld=0";
                        SqlCommand cmd = new SqlCommand(SQLStatement, cnPubs);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@EANCode", ean);
                        cmd.CommandTimeout = 12000;
                        BerichtId = int.Parse(cmd.ExecuteScalar().ToString());

                        Update_Dispuut(BerichtId, blnGeaccepteerd, true, blnBatch);

                        cnPubs.Close();
                    }
                    catch { }


                    //Save_Dispuut(inboxID, DateTime.Now, ean, Datum, Datum, netbeheerder, Dossier, meteringpoint.Portaal_Mutation.MutationReason.ToString(),
                    //    herkomst, StandNormaal, StandLaag, false, referentie, false, "", false);
                }

                if (inboxID > 0) { ProcessMessage.processMessage(inboxID, ConnString); }
                //Nog check op rejection


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
            //if (strFileName != "") { File.Delete(strFileName); }
        }

        public int Save_Dispuut(int intBericht_ID, DateTime verstuurd_DT, Dispuut dispuut, Boolean geaccepteerd, String referentie,
            Boolean verstuurd, string opmerking, Boolean afgehandeld, Boolean blnBatch)
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;
            int inboxID = -1;

            cnPubs.Open();
            SQLstatement =
                    "INSERT INTO Messages.[dbo].[Dispuut] " +
                    "(BerichtId " +
                    ",Verstuurd_DT " +
                    ",EanCode " +
                    ",Begin_D " +
                    ",Eind_D " +
                    ",Netbeheerder " +
                    ",DossierID " +
                    ",RedenMutatie " +
                    ",Herkomst " +
                    ",StandNormaal " +
                    ",Geaccepteerd " +
                    ",Referentie " +
                    ",Verstuurd " +
                    ",Opmerking " +
                    ",Afgehandeld ";
            if (dispuut.standLaag.Trim() != "") { SQLstatement = SQLstatement + ",StandLaag) "; } else { SQLstatement = SQLstatement + ") "; }
            SQLstatement = SQLstatement + "VALUES " +
                    "(@BerichtId " +
                    ",@Verstuurd_DT " +
                    ",@EanCode " +
                    ",@Begin_D " +
                    ",@Eind_D " +
                    ",@Netbeheerder " +
                    ",@DossierID " +
                    ",@RedenMutatie " +
                    ",@Herkomst " +
                    ",@StandNormaal " +
                    ",@Geaccepteerd " +
                    ",@Referentie " +
                    ",@Verstuurd " +
                    ",@Opmerking " +
                    ",@Afgehandeld ";
            if (dispuut.standLaag.Trim() != "") { SQLstatement = SQLstatement + ",@StandLaag) "; } else { SQLstatement = SQLstatement + ") "; }



            SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveInbox.Parameters.Add(new SqlParameter("@BerichtId", SqlDbType.Int));
            cmdSaveInbox.Parameters["@BerichtId"].Value = intBericht_ID;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Verstuurd_DT", SqlDbType.DateTime));
            cmdSaveInbox.Parameters["@Verstuurd_DT"].Value = verstuurd_DT;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@EanCode", SqlDbType.BigInt));
            cmdSaveInbox.Parameters["@EanCode"].Value = dispuut.ean;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Begin_D", SqlDbType.Date));
            cmdSaveInbox.Parameters["@Begin_D"].Value = dispuut.vanDatum;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Eind_D", SqlDbType.Date));
            cmdSaveInbox.Parameters["@Eind_D"].Value = dispuut.totDatum;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Netbeheerder", SqlDbType.BigInt));
            cmdSaveInbox.Parameters["@Netbeheerder"].Value = dispuut.netbeheerder;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@DossierID", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@DossierID"].Value = dispuut.dossierID;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@RedenMutatie", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@RedenMutatie"].Value = dispuut.redenmutatie;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Herkomst", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@Herkomst"].Value = dispuut.herkomst;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@StandNormaal", SqlDbType.Decimal));
            cmdSaveInbox.Parameters["@StandNormaal"].Value = dispuut.standNormaal;

            cmdSaveInbox.Parameters.Add(new SqlParameter("@Geaccepteerd", SqlDbType.Bit));
            cmdSaveInbox.Parameters["@Geaccepteerd"].Value = geaccepteerd;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Referentie", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@Referentie"].Value = referentie;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Verstuurd", SqlDbType.Bit));
            cmdSaveInbox.Parameters["@Verstuurd"].Value = verstuurd;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Opmerking", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@Opmerking"].Value = opmerking;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Afgehandeld", SqlDbType.Bit));
            cmdSaveInbox.Parameters["@Afgehandeld"].Value = afgehandeld;
            if (dispuut.standLaag.Trim() != "")
            {
                cmdSaveInbox.Parameters.Add(new SqlParameter("@StandLaag", SqlDbType.VarChar));
                cmdSaveInbox.Parameters["@StandLaag"].Value = dispuut.standLaag;
            }


            try
            {
                cmdSaveInbox.ExecuteNonQuery();
                //inboxID = (int)cmdSaveInbox.Parameters["@inboxID"].Value;
                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
            }
            catch (Exception ex)
            {
                if (!blnBatch)
                {
                    MessageBox.Show("Er is iets fout gegaan met het bewaren van het dispuut, we adviseren U contact op te nemen met IT");
                    MessageBox.Show(ex.ToString());
                }
            }

            cnPubs.Close();
            return inboxID;

        }

        public int Save_Dispuut_Inkomend(int BerichtId, DateTime verstuurd_DT, string eanCode, DateTime begin_D, DateTime eind_D, String netbeheerder,
            string dossierID, string redenMutatie, string herkomst, string StandNormaal, string StandLaag, Boolean geaccepteerd, String referentie,
            Boolean verstuurd, string opmerking, Boolean afgehandeld, Boolean blnBatch)
        {
            SqlConnection cnPubs = new SqlConnection(KC.ConnString);
            string SQLstatement;
            int inboxID = -1;

            cnPubs.Open();
            SQLstatement =
                    "INSERT INTO Car.[dbo].[Dispuut] " +
                    "(BerichtId " +
                    ",Verstuurd_DT " +
                    ",EanCode " +
                    ",Begin_D " +
                    ",Eind_D " +
                    ",Netbeheerder " +
                    ",DossierID " +
                    ",RedenMutatie " +
                    ",Herkomst " +
                    ",StandNormaal " +
                    ",Geaccepteerd " +
                    ",Referentie " +
                    ",Verstuurd " +
                    ",Opmerking " +
                    ",Afgehandeld ";
            if (StandLaag.Trim() != "") { SQLstatement = SQLstatement + ",StandLaag) "; } else { SQLstatement = SQLstatement + ") "; }
            SQLstatement = SQLstatement + "VALUES " +
                    "(@BerichtId " +
                    ",@Verstuurd_DT " +
                    ",@EanCode " +
                    ",@Begin_D " +
                    ",@Eind_D " +
                    ",@Netbeheerder " +
                    ",@DossierID " +
                    ",@RedenMutatie " +
                    ",@Herkomst " +
                    ",@StandNormaal " +
                    ",@Geaccepteerd " +
                    ",@Referentie " +
                    ",@Verstuurd " +
                    ",@Opmerking " +
                    ",@Afgehandeld ";
            if (StandLaag.Trim() != "") { SQLstatement = SQLstatement + ",@StandLaag) "; } else { SQLstatement = SQLstatement + ") "; }



            SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveInbox.Parameters.Add(new SqlParameter("@BerichtId", SqlDbType.Int));
            cmdSaveInbox.Parameters["@BerichtId"].Value = BerichtId;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Verstuurd_DT", SqlDbType.DateTime));
            cmdSaveInbox.Parameters["@Verstuurd_DT"].Value = verstuurd_DT;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@EanCode", SqlDbType.BigInt));
            cmdSaveInbox.Parameters["@EanCode"].Value = eanCode;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Begin_D", SqlDbType.Date));
            cmdSaveInbox.Parameters["@Begin_D"].Value = begin_D;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Eind_D", SqlDbType.Date));
            cmdSaveInbox.Parameters["@Eind_D"].Value = eind_D;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Netbeheerder", SqlDbType.BigInt));
            cmdSaveInbox.Parameters["@Netbeheerder"].Value = netbeheerder;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@DossierID", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@DossierID"].Value = dossierID;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@RedenMutatie", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@RedenMutatie"].Value = redenMutatie;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Herkomst", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@Herkomst"].Value = herkomst;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@StandNormaal", SqlDbType.Decimal));
            cmdSaveInbox.Parameters["@StandNormaal"].Value = StandNormaal;

            cmdSaveInbox.Parameters.Add(new SqlParameter("@Geaccepteerd", SqlDbType.Bit));
            cmdSaveInbox.Parameters["@Geaccepteerd"].Value = geaccepteerd;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Referentie", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@Referentie"].Value = referentie == null?"":referentie;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Verstuurd", SqlDbType.Bit));
            cmdSaveInbox.Parameters["@Verstuurd"].Value = verstuurd;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Opmerking", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@Opmerking"].Value = opmerking;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Afgehandeld", SqlDbType.Bit));
            cmdSaveInbox.Parameters["@Afgehandeld"].Value = afgehandeld;
            if (StandLaag.Trim() != "")
            {
                cmdSaveInbox.Parameters.Add(new SqlParameter("@StandLaag", SqlDbType.VarChar));
                cmdSaveInbox.Parameters["@StandLaag"].Value = StandLaag;
            }


            try
            {
                cmdSaveInbox.ExecuteNonQuery();
                //inboxID = (int)cmdSaveInbox.Parameters["@inboxID"].Value;
                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
            }
            catch (Exception ex)
            {
                if (!blnBatch)
                {
                    MessageBox.Show("Er is iets fout gegaan met het bewaren van het dispuut, we adviseren U contact op te nemen met IT");
                    MessageBox.Show(ex.ToString());
                }
            }

            cnPubs.Close();
            return inboxID;

        }
        public int Update_Dispuut(int BerichtId, Boolean geaccepteerd, Boolean afgehandeld, Boolean blnBatch)
        {
            SqlConnection cnPubs = new SqlConnection(KC.ConnString);
            string SQLstatement;
            int inboxID = -1;

            cnPubs.Open();
            SQLstatement =
                    "Update Car.dbo.Dispuut " +
                    " SET Geaccepteerd=@Geaccepteerd " +
                    ",Afgehandeld=@Afgehandeld " +
                    "WHERE BerichtId=@BerichtId";




            SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveInbox.Parameters.Add(new SqlParameter("@BerichtId", SqlDbType.Int));
            cmdSaveInbox.Parameters["@BerichtId"].Value = BerichtId;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Geaccepteerd", SqlDbType.Bit));
            cmdSaveInbox.Parameters["@Geaccepteerd"].Value = geaccepteerd;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Afgehandeld", SqlDbType.Bit));
            cmdSaveInbox.Parameters["@Afgehandeld"].Value = afgehandeld;


            try
            {
                cmdSaveInbox.ExecuteNonQuery();
                //inboxID = (int)cmdSaveInbox.Parameters["@inboxID"].Value;
                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
            }
            catch (Exception ex)
            {
                if (!blnBatch)
                {
                    MessageBox.Show("Er is iets fout gegaan met het bijwerken van het dispuut, we adviseren U contact op te nemen met IT");
                    MessageBox.Show(ex.ToString());
                }
            }

            cnPubs.Close();
            return inboxID;

        }

    }



    public class VastGesteldeStand
    {
        public string ean;
        public DateTime vanDatum;
        public DateTime totDatum;
        public string netbeheerder;
        public string dossierID;
        public string meternummer;
        public string aantalregister;
        public string herkomst;
        public string redenmutatie;
        public string product;
        public string[] arrReading;
        public string[] arrReadingDate;
        public string[] arrReadingMethod;
        public string[] arrTarrifType;
        public string[] arrDirection;
        public string[] arrNrDigits;
        public string[] arrMeasureUnit;
        public int intNrStanden;
        public int intNrRegisters;
        public string enrollment_ID;
        public int meterStand_ID;
    }

    public class Dispuut
    {
        public string ean;
        public DateTime vanDatum;
        public DateTime totDatum;
        public string netbeheerder;
        public string dossierID;
        public string meternummer;
        public string aantalregister;
        public string herkomst;
        public string redenmutatie;
        public string product;
        public string nrTelwerkenNormaal;
        public string meeteindheidNormaal;
        public string standNormaal;
        public string nrTelwerkenLaag;
        public string meeteindheidLaag;
        public string standLaag;
    }
}
