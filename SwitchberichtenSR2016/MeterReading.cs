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
using System.Globalization;
using Energie.Car;

namespace Energie.SwitchBericht
{
    public class MeterReading
    {
        //private string SQLstatement;
        private static String ConnString = "";
        //private string urlWebService = "";
        //private string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
        private string path = "";
        private string certPV = "";
        private string certPVPassword = "";
        private string certLV = "";
        private string certLVPassword = "";
        private string HoofdPV = "";
        private string HoofdLV = "";
        private string Klant_Config = "";

        public string ResultFileName = "";
        private string strFTPServer = Energie.DataAccess.Configurations.GetApplicationSetting("FTPSERVER");
        private string strFTPUser = "";
        private string strFTPPassword = "";


        public MeterReading(String klant_Config)
        {
            Klant_Config = klant_Config;
            if (Klant_Config != "")
            {
                ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB_" + Klant_Config);
                //urlWebService = Energie.DataAccess.Configurations.GetApplicationSetting("PORTAAL_" + Klant_Config);//"https://emp.edsn.nl/b2b";
                path = Energie.DataAccess.Configurations.GetApplicationSetting("XMLPATH_" + Klant_Config);//@"c:\test\";
                certPV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPV_" + Klant_Config);
                certPVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPVPASSWORD_" + Klant_Config);
                certLV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLV_" + Klant_Config);
                certLVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLVPASSWORD_" + Klant_Config);
                HoofdPV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV_" + Klant_Config).Trim();
                HoofdLV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV_" + Klant_Config).Trim();
                strFTPUser = Energie.DataAccess.Configurations.GetApplicationSetting("FTPUSER_" + Klant_Config).Trim();
                strFTPPassword = Energie.DataAccess.Configurations.GetApplicationSetting("FTPPASSWORD_" + Klant_Config).Trim();
            }
            else
            {
                ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB");
                //urlWebService = Energie.DataAccess.Configurations.GetApplicationSetting("PORTAAL");//"https://emp.edsn.nl/b2b";
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
        public Boolean RequestMeterReading(Boolean blnPV, string strFileName, Boolean blnBatch)
        {
            Boolean blnData = false;
            string OldFileName = strFileName;
            //if (strFileName != "")
            //{
            //    string text = File.ReadAllText(strFileName);
            //    text = text.Replace("<EDSNDocument>", @"<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "utf-8" + '"' + "?>");
            //    text = text.Replace("</EDSNDocument>", "");
            //    text = text.Replace("xmlns=" + '"' + "urn:nedu:edsn:data:meterreadingexchangeresponse:1:standard" + '"', "xmlns:xsi=" + '"' + "http://www.w3.org/2001/XMLSchema-instance" + '"' + " xmlns:xsd=" + '"' + "http://www.w3.org/2001/XMLSchema" + '"');
            //    text = text.Replace("<EDSNBusinessDocumentHeader>", "<EDSNBusinessDocumentHeader xmlns=" + '"' + "urn:nedu:edsn:data:meterreadingexchangeresponse:1:standard" + '"' + ">");
            //    text = text.Replace("<Portaal_Content>", "<Portaal_Content xmlns=" + '"' + "urn:nedu:edsn:data:meterreadingexchangeresponse:1:standard" + '"' + ">");
            //    strFileName = strFileName.Trim() + ".tmp";
            //    File.WriteAllText(strFileName, text);
            //}

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope enveloppe = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope();


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(ConnString);
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
            if (blnPV != true) { source.SenderID = HoofdLV; } else { source.SenderID = HoofdPV; }
            //if (blnPV != true) { source.SenderID = "7314252022905"; } else { source.SenderID = HoofdPV; }
            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.MeterReading.MeterReading meterReading = new nl.Energie.EDSN.MeterReading.MeterReading();

            //String certPath = certpath + @"EDSN2013053100006.p12";
            if (strFileName == "")
            {
                if (blnPV == true)
                {
                    //certPath = certpath + @"EDSN2013053100007.p12";
                    meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
                }
                else
                {
                    meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
                    //meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV));
                }
            }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            meterReading.Url = KC.CarUrl + @"synchroon/ResponderMeterReadingRespondingActivity";

            meterReading.Timeout = 120000;

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope retour = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope();



            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeRequestEnvelope));
            TextWriter WriteFileStream;

            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                { BestandsAanvulling = " LV " + BestandsAanvulling; }



                //8 nov volledige naam

                WriteFileStream = new StreamWriter(path + @"MeterReadingExchangeRequest" + BestandsAanvulling + ".xml"); ;
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();


                string ftpResponse = "";
                if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingExchangeRequest" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingExchangeRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                {
                    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                }

            }

            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                //if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }
                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();




                if (strFileName == "")
                {
                    retour = meterReading.MeterReadingExchangeRequest(enveloppe);
                    //7 novMeterReadingExchangeResponse 
                    //WriteFileStream = new StreamWriter(path + @"MeterReadingExchangeResponse" + BestandsAanvulling + ".xml");
                    //serializer.Serialize(WriteFileStream, retour);
                    //WriteFileStream.Close();

                    //string ftpResponse = "";
                    //if (blnBatch != true && retour.Portaal_Content.Length > 0)
                    //{
                    //    if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingExchangeResponse" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingExchangeResponse" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    //    {
                    //        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                    //    }
                    //}

                    //StringWriter swXML = new StringWriter();
                    //serializer.Serialize(swXML, retour);
                    //_Doc.Load(path + "MeterReadingExchangeResponse" + BestandsAanvulling + ".xml");

                }
                else
                {





                    retour = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope();



                    _Doc = new XmlDocument();
                    _Doc.Load(strFileName);
                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope));
                    retour = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
                }


                int inboxID = 0;
                if (retour.Portaal_Content.Length > 0)
                {
                    for (int i = 0; i < retour.Portaal_Content.Length; i++)
                    {
                        inboxID = Save_Inbox(27, path + @"MeterReadingExchangeResponse" + BestandsAanvulling + ".xml", "Verbruik : " + retour.EDSNBusinessDocumentHeader.MessageID.ToString(), blnBatch);
                        int intedinID = Save_Edine(inboxID, retour.EDSNBusinessDocumentHeader.Source.SenderID, retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID,
                            retour.EDSNBusinessDocumentHeader.CreationTimestamp, retour.EDSNBusinessDocumentHeader.MessageID, "UTILTS", "E11");

                        nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP portaal_meteringPoint = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP)retour.Portaal_Content[i];

                        nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_PM portaal_Mutation = portaal_meteringPoint.Portaal_Mutation;

                        DateTime dtmBegin = DateTime.MinValue;
                        DateTime dtmEnd = DateTime.MinValue;
                        if (portaal_meteringPoint.ValidFromDate != null)
                        {
                            dtmBegin = portaal_meteringPoint.ValidFromDate.ToUniversalTime();
                        }
                        if (portaal_meteringPoint.ValidToDate != null)
                        {
                            dtmEnd = portaal_meteringPoint.ValidToDate.ToUniversalTime();
                        }

                        string fase = "E01";
                        Boolean blnHeaderSaved = false;
                        DateTime dtmProcessingStart = DateTime.MinValue;
                        DateTime dtmProcessingEnd = DateTime.MinValue;

                        if (portaal_meteringPoint.Items[0].GetType() == typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Volume))
                        //if (1 == 1)
                        {
                            nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Volume portaal_Volume;
                            for (int i2 = 0; i2 < portaal_meteringPoint.Items.Length; i2++)
                            {
                                portaal_Volume = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Volume)portaal_meteringPoint.Items[i2];

                                //Volume
                                fase = "E02";

                                //nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_EnergyMeter_Register_Volume portaal_Volume = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_EnergyMeter_Register_Volume)portaal_Register.Item;

                                string strMeterTimeFrame = "";

                                if (portaal_Volume.Register.TariffType.ToString() == "L") { strMeterTimeFrame = "E10"; } else { strMeterTimeFrame = "E11"; }
                                string strTypeOfMeteringPoint = "";
                                if (portaal_Volume.Register.MeteringDirection.ToString() == "TLV") { strTypeOfMeteringPoint = "E18"; } else { strTypeOfMeteringPoint = "E17"; }


                                if (blnHeaderSaved != true)
                                {
                                    if (portaal_Volume.Reading != null)
                                    {
                                        for (int i4 = 0; i4 < portaal_Volume.Reading.Length; i4++)
                                        {
                                            if (dtmProcessingStart == DateTime.MinValue) { dtmProcessingStart = portaal_Volume.Reading[i4].ReadingDate.ToUniversalTime(); }
                                            if (dtmProcessingEnd == DateTime.MinValue) { dtmProcessingEnd = portaal_Volume.Reading[i4].ReadingDate.ToUniversalTime(); }
                                            if (portaal_Volume.Reading[i4].ReadingDate.ToUniversalTime() > dtmProcessingStart) { dtmProcessingEnd = portaal_Volume.Reading[i4].ReadingDate.ToUniversalTime(); }
                                        }
                                    }
                                    else
                                    {
                                        dtmProcessingStart = portaal_Volume.ValidFromDate.ToUniversalTime();
                                        dtmProcessingEnd = portaal_Volume.ValidToDate.ToUniversalTime();
                                    }

                                    Save_UTILTS_E11_Header(intedinID, portaal_Mutation.Initiator, retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID
                                        , retour.EDSNBusinessDocumentHeader.CreationTimestamp.ToUniversalTime(), dtmProcessingStart, dtmProcessingEnd, fase, blnBatch);
                                    blnHeaderSaved = true;
                                }

                                if (portaal_Volume.Reading != null)
                                {
                                    for (int i4 = 0; i4 < portaal_Volume.Reading.Length; i4++)
                                    {
                                        string strReadingType = "367";
                                        if (portaal_Volume.Reading[i4].ReadingDate.ToUniversalTime() == dtmProcessingEnd) { strReadingType = "368"; }
                                        string strOriginCode = "";

                                        switch (portaal_Volume.Reading[i4].ReadingMethod.ToString())
                                        {
                                            case "Item005":
                                                strOriginCode = "N01";
                                                break;
                                            case "Item102":
                                                strOriginCode = "N03";
                                                break;
                                            case "Item22":
                                                strOriginCode = "E26";
                                                break;
                                            case "Item003":
                                                strOriginCode = "E27";
                                                break;
                                            case "Item004":
                                                strOriginCode = "004"; //Nieuwe XML code
                                                break;
                                        }
                                        Save_UTILTS_E11(intedinID, portaal_Mutation.Dossier.ID, portaal_Mutation.MutationReason.ToString(), portaal_meteringPoint.EANID.ToString(), strReadingType, portaal_Volume.Reading[i4].ReadingDate.ToUniversalTime()
                                                , strTypeOfMeteringPoint, strMeterTimeFrame, strOriginCode, decimal.Parse(portaal_Volume.Reading[i4].Reading), "220", null, "8716867000030", "KWH", blnBatch);
                                    }
                                    Save_UTILTS_E11(intedinID, portaal_Mutation.Dossier.ID, portaal_Mutation.MutationReason.ToString(), portaal_meteringPoint.EANID.ToString(), "", dtmProcessingEnd
                                        , strTypeOfMeteringPoint, strMeterTimeFrame, "", decimal.Parse(portaal_Volume.Volume), "136", null, "8716867000030", "KWH", blnBatch);
                                }
                                else
                                {

                                    Save_UTILTS_E11(intedinID, portaal_Mutation.Dossier.ID, portaal_Mutation.MutationReason.ToString(), portaal_meteringPoint.EANID.ToString(), "", dtmProcessingEnd
                                        , strTypeOfMeteringPoint, strMeterTimeFrame, "", decimal.Parse(portaal_Volume.Volume), "136", null, "8716867000030", "KWH", blnBatch);
                                }

                            }
                        }
                        else
                        {
                            nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter portaal_EnergyMeter;
                            for (int i2 = 0; i2 < portaal_meteringPoint.Items.Length; i2++)
                            {
                                portaal_EnergyMeter = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter)portaal_meteringPoint.Items[i2];


                                nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register portaal_Register = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register();
                                for (int i3 = 0; i3 < portaal_EnergyMeter.Register.Length; i3++)
                                {
                                    portaal_Register = portaal_EnergyMeter.Register[i3];

                                    if (portaal_Register.Item.GetType() == typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Volume_Reading))
                                    {
                                        //Meterstand
                                        nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Volume_Reading portaal_Reading = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Volume_Reading)portaal_Register.Item;
                                        string strMeterTimeFrame = "";
                                        if (portaal_Register.TariffType.ToString() == "L") { strMeterTimeFrame = "E10"; } else { strMeterTimeFrame = "E11"; }
                                        string strTypeOfMeteringPoint = "";
                                        if (portaal_Register.MeteringDirection.ToString() == "TLV") { strTypeOfMeteringPoint = "E18"; } else { strTypeOfMeteringPoint = "E17"; }

                                        string strOriginCode = "";
                                        switch (portaal_Reading.ReadingMethod.ToString())
                                        {
                                            case "Item005":
                                                strOriginCode = "N01";
                                                break;
                                            case "Item102":
                                                strOriginCode = "N03";
                                                break;
                                            case "Item22":
                                                strOriginCode = "E26";
                                                break;
                                            case "Item003":
                                                strOriginCode = "E27";
                                                break;
                                            case "Item004":
                                                strOriginCode = "004"; //Nieuwe XML code
                                                break;
                                        }
                                        if (blnHeaderSaved != true)
                                        {
                                            Save_UTILTS_E11_Header(intedinID, portaal_Mutation.Initiator, retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID
                                                , retour.EDSNBusinessDocumentHeader.CreationTimestamp.ToUniversalTime(), DateTime.MinValue, DateTime.MinValue, fase, blnBatch);
                                            blnHeaderSaved = true;
                                        }
                                        Save_UTILTS_E11(intedinID, portaal_EnergyMeter.ID, portaal_Mutation.MutationReason.ToString(), portaal_meteringPoint.EANID.ToString(), "367", portaal_Reading.ReadingDate.ToUniversalTime()
                                            , strTypeOfMeteringPoint, strMeterTimeFrame, strOriginCode, decimal.Parse(portaal_Reading.Reading), "220", null, "8716867000030", portaal_Register.MeasureUnit.ToString(), blnBatch);

                                    }
                                    if (portaal_Register.Item.GetType() == typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Volume))
                                    {
                                        //Volume
                                        fase = "E02";

                                        nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Volume portaal_Volume = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Volume)portaal_Register.Item;

                                        string strMeterTimeFrame = "";
                                        if (portaal_Register.TariffType.ToString() == "L") { strMeterTimeFrame = "E10"; } else { strMeterTimeFrame = "E11"; }
                                        string strTypeOfMeteringPoint = "";
                                        if (portaal_Register.MeteringDirection.ToString() == "TLV") { strTypeOfMeteringPoint = "E18"; } else { strTypeOfMeteringPoint = "E17"; }


                                        if (blnHeaderSaved != true)
                                        {
                                            for (int i4 = 0; i4 < portaal_Volume.Reading.Length; i4++)
                                            {
                                                if (dtmProcessingStart == DateTime.MinValue) { dtmProcessingStart = portaal_Volume.Reading[i4].ReadingDate.ToUniversalTime(); }
                                                if (dtmProcessingEnd == DateTime.MinValue) { dtmProcessingEnd = portaal_Volume.Reading[i4].ReadingDate.ToUniversalTime(); }
                                                if (portaal_Volume.Reading[i4].ReadingDate.ToUniversalTime() > dtmProcessingStart) { dtmProcessingEnd = portaal_Volume.Reading[i4].ReadingDate.ToUniversalTime(); }
                                            }

                                            Save_UTILTS_E11_Header(intedinID, portaal_Mutation.Initiator, retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID
                                                , retour.EDSNBusinessDocumentHeader.CreationTimestamp.ToUniversalTime(), dtmProcessingStart, dtmProcessingEnd, fase, blnBatch);
                                            blnHeaderSaved = true;
                                        }

                                        Save_UTILTS_E11(intedinID, portaal_EnergyMeter.ID, portaal_Mutation.MutationReason.ToString(), portaal_meteringPoint.EANID.ToString(), "367", dtmProcessingStart
                                            , strTypeOfMeteringPoint, strMeterTimeFrame, "", decimal.Parse(portaal_Volume.Volume), "136", null, "8716867000030", portaal_Register.MeasureUnit.ToString(), blnBatch);

                                        nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Volume_Reading[] portaal_VolumeReading = portaal_Volume.Reading;
                                        for (int i4 = 0; i4 < portaal_VolumeReading.Length; i4++)
                                        {
                                            //begin en eindstand
                                            string strOriginCode = "";
                                            switch (portaal_VolumeReading[i4].ReadingMethod.ToString())
                                            {
                                                case "Item005":
                                                    strOriginCode = "N01";
                                                    break;
                                                case "Item102":
                                                    strOriginCode = "N03";
                                                    break;
                                                case "Item22":
                                                    strOriginCode = "E26";
                                                    break;
                                                case "Item003":
                                                    strOriginCode = "E27";
                                                    break;
                                                case "Item004":
                                                    strOriginCode = "004"; //Nieuwe XML code
                                                    break;
                                            }
                                            string strReadingType = "";
                                            if (portaal_VolumeReading[i4].ReadingDate.ToUniversalTime() == dtmProcessingStart) { strReadingType = "367"; }
                                            if (portaal_VolumeReading[i4].ReadingDate.ToUniversalTime() == dtmProcessingEnd) { strReadingType = "368"; }
                                            Save_UTILTS_E11(intedinID, portaal_EnergyMeter.ID, portaal_Mutation.MutationReason.ToString(), portaal_meteringPoint.EANID.ToString(), strReadingType, portaal_VolumeReading[i4].ReadingDate.ToUniversalTime()
                                            , strTypeOfMeteringPoint, strMeterTimeFrame, strOriginCode, decimal.Parse(portaal_VolumeReading[i4].Reading), "220", null, "8716867000030", portaal_Register.MeasureUnit.ToString(), blnBatch);

                                        }

                                    }
                                    //portaal_Register = portaal_EnergyMeter[i3];
                                    //portaal_Register.
                                }
                            }
                        }

                    }
                    if (inboxID > 0) { ProcessMessage.processMessage(inboxID, ConnString); }
                }
                //Nog check op rejection



            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                if (blnBatch || Klant_Config != "")
                {
                    WriteLog("Fout Meterreading :" + S.ErrorDetails + " " + S.ErrorText, 10, -1);
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
                if (blnBatch || Klant_Config != "")
                {
                    WriteLog("Fout Meterreading :" + exception.Message, 10, -1);
                }
                else
                {
                    MessageBox.Show(exception.Message);
                }
            }
            catch (Exception exception)
            {
                if (blnBatch || Klant_Config != "")
                {
                    WriteLog("Fout Meterreading :" + exception.Message, 10, -1);
                }
                else
                {
                    MessageBox.Show(exception.Message);
                }
            }
            //    File.Delete(strFileName);
            return blnData;
        }

        public void VastGesteldeStand(Boolean blnPV, string strFileName, string ean, DateTime vanDatum, DateTime totDatum, string netbeheerder,
            string dossierID, string meternummer, string aantalregister, string herkomst, string redenmutatie, string product, string nrTelwerkenNormaal,
            string meeteindheidNormaal, string standNormaal, string nrTelwerkenLaag, string meeteindheidLaag, string standLaag, string enrollment_ID, Boolean blnBatch)
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

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope enveloppe = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope();



            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(ConnString);
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
            if (blnPV != true) { source.SenderID = HoofdLV; } else { source.SenderID = HoofdPV; }

            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP[] portaal_content = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP[1];
            enveloppe.Portaal_Content = portaal_content;

            portaal_content[0] = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP();
            portaal_content[0].EANID = ean;
            if (product == "ELK") { portaal_content[0].ProductType = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyProductPortaalTypeCode.ELK; }
            else { portaal_content[0].ProductType = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyProductPortaalTypeCode.GAS; }
            //portaal_content[0].ValidFromDate = vanDatum;
            //portaal_content[0].ValidToDate = totDatum;
            //portaal_content[0].ValidFromDateSpecified = true;
            //portaal_content[0].ValidToDateSpecified = true;

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter[] portaal_meter = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter[1];

            portaal_content[0].Items = portaal_meter;
            portaal_meter[0] = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter();
            portaal_meter[0].ID = meternummer;// "1234";
            portaal_meter[0].NrOfRegisters = aantalregister;// "1";
            int intAantalRegister = int.Parse(aantalregister);

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register[] portaal_register = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register[intAantalRegister];

            portaal_meter[0].Register = portaal_register;

            portaal_register[0] = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register();
            //portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.KWH;
            if (meeteindheidNormaal == "KWH") { portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.KWH; }
            if (meeteindheidNormaal == "KWT") { portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.KWT; }
            if (meeteindheidNormaal == "M3N") { portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.M3N; }
            if (meeteindheidNormaal == "MTQ") { portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.MTQ; }

            // de direction weer toegevoegd. Moet zeker voor de meterberichten
            portaal_register[0].MeteringDirection = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyFlowDirectionCode.LVR;
            portaal_register[0].MeteringDirectionSpecified = true;

            portaal_register[0].NrOfDigits = nrTelwerkenNormaal;
            portaal_register[0].TariffType = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyTariffTypeCode.N;
            portaal_register[0].TariffTypeSpecified = true;

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading portaal_reading = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading();

            portaal_register[0].Item = portaal_reading;
            portaal_reading.Reading = standNormaal;
            portaal_reading.ReadingDate = vanDatum;
            //portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003;

            if (herkomst == "Item001") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item001; }
            if (herkomst == "Item002") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item002; }
            if (herkomst == "Fysieke Opname") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003; }
            if (herkomst == "003") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003; }
            if (herkomst == "P4-stand") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item004; }
            if (herkomst == "004") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item004; }
            if (herkomst == "Berekend") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item005; }
            if (herkomst == "005") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item005; }
            if (herkomst == "Item006") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item006; }
            if (herkomst == "Item009") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item009; }
            if (herkomst == "Overeengekomen") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item102; }
            if (herkomst == "102") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item102; }
            if (herkomst == "Klantstand/P1-Stand") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
            if (herkomst == "022") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
            if (herkomst == "22") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
            if (herkomst == "Item90") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item90; }
            if (herkomst == "Item91") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item91; }
            if (herkomst == "Item92") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item92; }
            if (herkomst == "Item93") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item93; }
            if (herkomst == "E01") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.E01; }
            if (herkomst == "E02") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.E02; }

            if (intAantalRegister == 2)
            {
                portaal_register[1] = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register();
                //portaal_register[1].MeasureUnit = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.KWH;
                if (meeteindheidLaag == "KWH") { portaal_register[1].MeasureUnit = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.KWH; }
                if (meeteindheidLaag == "KWT") { portaal_register[1].MeasureUnit = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.KWT; }
                if (meeteindheidLaag == "M3N") { portaal_register[1].MeasureUnit = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.M3N; }
                if (meeteindheidLaag == "MTQ") { portaal_register[1].MeasureUnit = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.MTQ; }
                // direction weer toegevoegd 
                portaal_register[0].MeteringDirection = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyFlowDirectionCode.LVR;
                portaal_register[0].MeteringDirectionSpecified = true;


                portaal_register[1].NrOfDigits = nrTelwerkenLaag;
                portaal_register[1].TariffType = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyTariffTypeCode.L;
                portaal_register[1].TariffTypeSpecified = true;

                nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading portaal_reading2 = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading();

                portaal_register[1].Item = portaal_reading2;
                portaal_reading2.Reading = standLaag;
                portaal_reading2.ReadingDate = vanDatum;
                //portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003;

                if (herkomst == "Item001") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item001; }
                if (herkomst == "Item002") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item002; }
                if (herkomst == "Fysieke Opname") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003; }
                if (herkomst == "003") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003; }
                if (herkomst == "P4-stand") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item004; }
                if (herkomst == "004") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item004; }
                if (herkomst == "Berekend") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item005; }
                if (herkomst == "005") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item005; }
                if (herkomst == "Item006") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item006; }
                if (herkomst == "Item009") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item009; }
                if (herkomst == "Overeengekomen") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item102; }
                if (herkomst == "102") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item102; }
                if (herkomst == "Klantstand/P1-Stand") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
                if (herkomst == "022") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
                if (herkomst == "22") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
                if (herkomst == "Item90") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item90; }
                if (herkomst == "Item91") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item91; }
                if (herkomst == "Item92") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item92; }
                if (herkomst == "Item93") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item93; }
                if (herkomst == "E01") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.E01; }
                if (herkomst == "E02") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.E02; }
            }

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_PM portaal_mutation = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_PM();
            portaal_content[0].Portaal_Mutation = portaal_mutation;

            portaal_mutation.Initiator = HoofdLV;
            //portaal_mutation.Consumer = netbeheerder;// "1114252022907"; robin is niet goed
            portaal_mutation.Consumer = "1119328025455";
            //portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MOVEIN;

            //if (redenmutatie == "BULKPV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.BULKPV; }
            if (redenmutatie == "ALLMTCHG") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.ALLMTCHG; }
            if (redenmutatie == "CONNACT") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNACT; }
            if (redenmutatie == "CONNCHG") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNCHG; }
            if (redenmutatie == "CONNCRE") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNCRE; }
            if (redenmutatie == "CONNDACT") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNDACT; }
            if (redenmutatie == "CONNEND") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNEND; }
            if (redenmutatie == "CONNUPD") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNUPD; }
            if (redenmutatie == "CONSMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONSMTR; }
            if (redenmutatie == "DISPUTE") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.DISPUTE; }
            if (redenmutatie == "DSTRCONN") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.DSTRCONN; }
            if (redenmutatie == "DSTRMSTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.DSTRMSTR; }
            if (redenmutatie == "ENDOFMV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.ENDOFMV; }
            if (redenmutatie == "EOSUPPLY") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.EOSUPPLY; }
            if (redenmutatie == "HISTMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.HISTMTR; }
            if (redenmutatie == "INDHSE") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.INDHSE; }
            if (redenmutatie == "INDHSH") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.INDHSH; }
            if (redenmutatie == "MDMD") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MDMD; }
            if (redenmutatie == "MDPPMD") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MDPPMD; }
            if (redenmutatie == "MONTHMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MONTHMTR; }
            if (redenmutatie == "MOVEIN") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MOVEIN; }
            if (redenmutatie == "MOVEOUT") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MOVEOUT; }
            if (redenmutatie == "MTREND") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MTREND; }
            if (redenmutatie == "MTRINST") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MTRINST; }
            if (redenmutatie == "MTRUPD") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MTRUPD; }
            if (redenmutatie == "ONRQST") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.ONRQST; }
            if (redenmutatie == "NAMECHG") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.NAMECHG; }
            if (redenmutatie == "NMCRSCMP") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.NMCRSCMP; }
            if (redenmutatie == "PERMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.PERMTR; }
            if (redenmutatie == "PHYSMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.PHYSMTR; }
            if (redenmutatie == "RESCOMP") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.RESCOMP; }
            if (redenmutatie == "SWITCHLV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.SWITCHLV; }
            if (redenmutatie == "SWITCHMV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.SWITCHMV; }
            if (redenmutatie == "SWITCHPV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.SWITCHPV; }
            if (redenmutatie == "SWTCHUPD") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.SWTCHUPD; }

            if (dossierID != "")
            {
                nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_PM_Dossier portaal_dossier = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_PM_Dossier();
                portaal_mutation.Dossier = portaal_dossier;
                portaal_dossier.ID = dossierID;// "7325724";
            }



            //portaal_meter[0].Register

            nl.Energie.EDSN.MeterReading.MeterReading meterReading = new nl.Energie.EDSN.MeterReading.MeterReading();


            //String certPath = certpath + @"EDSN2013053100006.p12";
            if (strFileName == "")
            {
                if (blnPV == true)
                {
                    //certPath = certpath + @"EDSN2013053100007.p12";
                    meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
                }
                else
                {
                    meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
                    //meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV));
                }
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

                WriteFileStream = new StreamWriter(path + @"MeterReadingExchangeNotification" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);

                WriteFileStream.Close();

                string ftpResponse = "";
                if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingExchangeNotification" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingExchangeNotification" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                {
                    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                }


                StringWriter swXML = new StringWriter();
                serializer.Serialize(swXML, enveloppe);
                int intOutBoxID = Save_Outbox(header.DocumentID, ean, "Stand", swXML.ToString());
            }

            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope));
                XmlDocument _Doc = new XmlDocument();

                if (strFileName == "")
                {


                    retour = meterReading.MeterReadingExchangeNotification(enveloppe);

                    WriteFileStream = new StreamWriter(path + @"MeterReadingResult" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    string ftpResponse = "";
                    if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingResult" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    {
                        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                    }

                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);
                    _Doc.Load(path + "MeterReadingResult" + BestandsAanvulling + ".xml");

                    MessageBox.Show("Stand ingediend");
                }
                else
                {
                    retour = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope();

                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope));
                    //Save to file kan weg
                    WriteFileStream = new StreamWriter(path + @"MeterReadingExchangeResponseEnvelope" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();
                }

                //if (retour.Portaal_Content.Item.GetType() == typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope_Portaal_Content_EDSN_SimpleRejection))
                //{
                //    nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope_Portaal_Content_EDSN_SimpleRejection portaal_Rejection = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope_Portaal_Content_EDSN_SimpleRejection();
                //    portaal_Rejection = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope_Portaal_Content_EDSN_SimpleRejection)retour.Portaal_Content.Item;
                //    MessageBox.Show("Fout : " + portaal_Rejection.SimpleRejection.RejectionText);
                //}
                //else
                {
                    int inboxID = 0;
                    inboxID = Save_Inbox(27, _Doc.InnerXml.ToString(), "Stand : " + retour.EDSNBusinessDocumentHeader.MessageID.ToString(), blnBatch);

                    int intedinID = Save_Edine(inboxID, retour.EDSNBusinessDocumentHeader.Source.SenderID, retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID,
                            retour.EDSNBusinessDocumentHeader.CreationTimestamp, retour.EDSNBusinessDocumentHeader.MessageID, "UTILTS", "E11");
                    int intMeterStand_ID = Save_MeterStand_Header(intedinID, true, DateTime.Now, ean, vanDatum, totDatum, netbeheerder, dossierID, redenmutatie, enrollment_ID, -1, false);
                    Save_MeterStand(intMeterStand_ID, intedinID, herkomst, "N", standNormaal, "LVR", "0", "0", "", "0", false);

                    if (intAantalRegister == 2)
                    {
                        Save_MeterStand(intMeterStand_ID, intedinID, herkomst, "L", standLaag, "LVR", "0", "0", "", "0", false);
                    }
                    if (inboxID > 0) { ProcessMessage.processMessage(inboxID, ConnString); }
                }

                //if (inboxID > 0) { ProcessMessage.processMessage(inboxID); }
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
            if (strFileName != "") { File.Delete(strFileName); }
        }

        public int VastGesteldeStand(Boolean blnPV, string strFileName, string ean, DateTime vanDatum, DateTime totDatum, string netbeheerder,
            string dossierID, string meternummer, string aantalregister, string herkomst, string redenmutatie, string product, string[] arrReading,
            string[] arrReadingDate, string[] arrReadingMethod, string[] arrTarrifType, string[] arrDirection, string[] arrNrDigits,
            string[] arrMeasureUnit, int intNrStanden, int intNrRegisters, string enrollment_ID, int meterStand_ID, Boolean blnBatch)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
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

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope enveloppe = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope();


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(ConnString);
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

            if (blnPV != true)
            {
                //source.ContactTypeIdentifier = "DDQ_O";
                if (product == "GAS")
                {
                    source.SenderID = "8714252022926";
                    source.ContactTypeIdentifier = "DDQ_M";
                }
                else
                {
                    source.SenderID = HoofdLV;
                    source.ContactTypeIdentifier = "DDQ_O";
                }
            }
            else
            {
                source.ContactTypeIdentifier = "DDK_O";
                source.SenderID = HoofdPV;
            }
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP[] portaal_content = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP[1];
            enveloppe.Portaal_Content = portaal_content;


            portaal_content[0] = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP();
            portaal_content[0].EANID = ean;
            if (product == "ELK") { portaal_content[0].ProductType = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyProductPortaalTypeCode.ELK; }
            else { portaal_content[0].ProductType = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyProductPortaalTypeCode.GAS; }
            //portaal_content[0].ValidFromDate = vanDatum;
            //portaal_content[0].ValidToDate = totDatum;
            //portaal_content[0].ValidFromDateSpecified = true;
            //portaal_content[0].ValidToDateSpecified = true;

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter[] portaal_meter = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter[1];

            portaal_content[0].Items = portaal_meter;
            portaal_meter[0] = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter();
            portaal_meter[0].ID = meternummer;// "1234";
            portaal_meter[0].NrOfRegisters = aantalregister;// "1";
            int intAantalRegister = int.Parse(aantalregister);

            int intAantalInsturen = intNrStanden; //Was voor samenvoegen: intNrRegisters
            //if (intNrStanden == 2 && intNrRegisters == 4)
            //{
            //    intAantalInsturen = 2;
            //}
            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register[] portaal_register = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register[intAantalInsturen];
            portaal_meter[0].Register = portaal_register;
            int nrExtraStand = 0;

            for (int intRegister = 0; intRegister < intAantalInsturen; intRegister++)
            {
                if (intRegister < intNrStanden)
                {
                    portaal_register[intRegister] = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register();
                    //portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.KWH;
                    portaal_register[intRegister].MeasureUnit = GetMeetEenheid(arrMeasureUnit[intRegister]);

                    // de direction weer toegevoegd. Moet zeker voor de meterberichten
                    portaal_register[intRegister].MeteringDirection = GetDirection(arrDirection[intRegister]);
                    portaal_register[intRegister].MeteringDirectionSpecified = true;

                    portaal_register[intRegister].NrOfDigits = arrNrDigits[intRegister];
                    if (product == "ELK")
                    {
                        portaal_register[intRegister].TariffType = GetTarifType(arrTarrifType[intRegister]);
                        portaal_register[intRegister].TariffTypeSpecified = true;
                    }

                    nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading portaal_reading = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading();

                    portaal_register[intRegister].Item = portaal_reading;

                    portaal_reading.Reading = arrReading[intRegister];
                    portaal_reading.ReadingDate = DateTime.ParseExact(arrReadingDate[intRegister], "yyyy-MM-dd", provider);
                    //portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003;

                    portaal_reading.ReadingMethod = GetHerkomst(herkomst);
                }
                else
                {
                    portaal_register[intRegister] = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register();
                    //portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MeasureUnitPortaalCode.KWH;
                    portaal_register[intRegister].MeasureUnit = GetMeetEenheid(arrMeasureUnit[0]);

                    // de direction weer toegevoegd. Moet zeker voor de meterberichten
                    if (arrDirection[0] == "LVR")
                    {
                        portaal_register[intRegister].MeteringDirection = GetDirection("TLV");
                    }
                    else
                    {
                        portaal_register[intRegister].MeteringDirection = GetDirection("LVR");
                    }
                    portaal_register[intRegister].MeteringDirectionSpecified = true;

                    portaal_register[intRegister].NrOfDigits = arrNrDigits[0];
                    if (product == "ELK")
                    {
                        portaal_register[intRegister].TariffType = GetTarifType(arrTarrifType[nrExtraStand]);
                        nrExtraStand++;
                        portaal_register[intRegister].TariffTypeSpecified = true;
                    }

                    nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading portaal_reading = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading();

                    portaal_register[intRegister].Item = portaal_reading;

                    portaal_reading.Reading = "0";
                    portaal_reading.ReadingDate = DateTime.ParseExact(arrReadingDate[0], "yyyy-MM-dd", provider);
                    //portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003;

                    portaal_reading.ReadingMethod = GetHerkomst(herkomst);
                }
            }




            nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_PM portaal_mutation = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_PM();
            portaal_content[0].Portaal_Mutation = portaal_mutation;

            if (product == "GAS")
            {
                portaal_mutation.Initiator = "8714252022926";
            }
            else
            {
                portaal_mutation.Initiator = HoofdLV;
            }
            //   portaal_mutation.Consumer = netbeheerder;// "1114252022907"; robin is niet goed
            portaal_mutation.Consumer = "1119328025455";
            //portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MOVEIN;

            if (redenmutatie.ToUpper() == "ANNUAL") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.PERMTR; }
            //if (redenmutatie == "BULKPV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.BULKPV; }
            if (redenmutatie == "ALLMTCHG") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.ALLMTCHG; }
            if (redenmutatie == "CONNACT") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNACT; }
            if (redenmutatie == "CONNCHG") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNCHG; }
            if (redenmutatie == "CONNCRE") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNCRE; }
            if (redenmutatie == "CONNDACT") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNDACT; }
            if (redenmutatie == "CONNEND") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNEND; }
            if (redenmutatie == "CONNUPD") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONNUPD; }
            if (redenmutatie == "CONSMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.CONSMTR; }
            if (redenmutatie == "DISPUTE") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.DISPUTE; }
            if (redenmutatie == "DSTRCONN") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.DSTRCONN; }
            if (redenmutatie == "DSTRMSTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.DSTRMSTR; }
            if (redenmutatie == "ENDOFMV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.ENDOFMV; }
            if (redenmutatie == "EOSUPPLY") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.EOSUPPLY; }
            if (redenmutatie == "HISTMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.HISTMTR; }
            if (redenmutatie == "MOVEIN") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MOVEIN; }
            if (redenmutatie == "MVI") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MOVEIN; }
            if (redenmutatie == "MOVEOUT" || redenmutatie.ToUpper() == "FINAL") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MOVEOUT; }
            if (redenmutatie == "MVO") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MOVEOUT; }
            if (redenmutatie == "MTREND") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MTREND; }
            if (redenmutatie == "MTRINST") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MTRINST; }
            if (redenmutatie == "MTRUPD") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.MTRUPD; }
            if (redenmutatie == "NAMECHG") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.NAMECHG; }
            if (redenmutatie == "NMCRSCMP") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.NMCRSCMP; }
            if (redenmutatie == "PERMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.PERMTR; }
            if (redenmutatie == "PHYSMTR") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.PHYSMTR; }
            if (redenmutatie == "RESCOMP") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.RESCOMP; }
            if (redenmutatie == "SWITCHLV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.SWITCHLV; }
            if (redenmutatie == "SWITCHMV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.SWITCHMV; }
            if (redenmutatie == "SWITCHPV") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.SWITCHPV; }
            if (redenmutatie == "SWTCHUPD") { portaal_mutation.MutationReason = nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_MutationReasonPortaalCode.SWTCHUPD; }

            if (dossierID != "")
            {
                nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_PM_Dossier portaal_dossier = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeNotificationEnvelope_PC_PMP_PM_Dossier();
                portaal_mutation.Dossier = portaal_dossier;
                portaal_dossier.ID = dossierID;// "7325724";
            }


            //portaal_meter[0].Register

            nl.Energie.EDSN.MeterReading.MeterReading meterReading = new nl.Energie.EDSN.MeterReading.MeterReading();


            //String certPath = certpath + @"EDSN2013053100006.p12";
            if (strFileName == "")
            {
                if (blnPV == true)
                {
                    //certPath = certpath + @"EDSN2013053100007.p12";
                    meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
                }
                else
                {
                    meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
                    //meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV));
                }
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

                WriteFileStream = new StreamWriter(path + @"MeterReadingExchangeNotification" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);

                WriteFileStream.Close();

                string ftpResponse = "";
                if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingExchangeNotification" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingExchangeNotification" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                {
                    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                }

                StringWriter swXML = new StringWriter();
                serializer.Serialize(swXML, enveloppe);
                int intOutBoxID = Save_Outbox(header.DocumentID, ean, "Stand", swXML.ToString());
            }

            int intedinID = 0;

            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope));
                XmlDocument _Doc = new XmlDocument();


                if (strFileName == "")
                {


                    retour = meterReading.MeterReadingExchangeNotification(enveloppe);

                    WriteFileStream = new StreamWriter(path + @"MeterReadingResult" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    string ftpResponse = "";
                    if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingResult" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    {
                        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                    }

                    ResultFileName = path + @"MeterReadingResult" + BestandsAanvulling + ".xml";

                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);
                    _Doc.Load(path + "MeterReadingResult" + BestandsAanvulling + ".xml");

                    //MessageBox.Show("Stand ingediend");
                }
                else
                {
                    retour = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeAcknowledgementEnvelope();

                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope));
                    //Save to file kan weg
                    WriteFileStream = new StreamWriter(path + @"MeterReadingExchangeResponseEnvelope" + BestandsAanvulling + ".xml");
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
                    inboxID = Save_Inbox(27, _Doc.InnerXml.ToString(), "Stand : " + retour.EDSNBusinessDocumentHeader.MessageID.ToString(), blnBatch);

                    intedinID = Save_Edine(inboxID, retour.EDSNBusinessDocumentHeader.Source.SenderID, retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID,
                            retour.EDSNBusinessDocumentHeader.CreationTimestamp, retour.EDSNBusinessDocumentHeader.MessageID, "UTILTS", "E11");
                    int intMeterStand_ID = Save_MeterStand_Header(intedinID, true, DateTime.Now, ean, vanDatum, totDatum, netbeheerder, dossierID, redenmutatie, enrollment_ID, meterStand_ID, false);
                    Boolean blnUpdate = false;
                    if (meterStand_ID != -1) { blnUpdate = true; }
                    for (int intRegister = 0; intRegister < intNrStanden; intRegister++)
                    {

                        Save_MeterStand(intMeterStand_ID, intedinID, herkomst, arrTarrifType[intRegister], arrReading[intRegister], arrDirection[intRegister], "0", "0", "", "0", blnUpdate);
                    }
                    if (inboxID > 0) { ProcessMessage.processMessage(inboxID, ConnString); }
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


                WriteLog("Fout bij Meterreading : + " + ean + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, 0);
                int intMeterStand_ID = Save_MeterStand_Header(intedinID, true, DateTime.Now, ean, vanDatum, totDatum, netbeheerder, dossierID, redenmutatie, enrollment_ID, meterStand_ID, true);
                //MessageBox.Show(S.ErrorCode.ToString());
                //MessageBox.Show(S.ErrorDetails);
                //MessageBox.Show(S.ErrorText);
                //MessageBox.Show(ex.Detail.InnerXml.ToString());
                return -1;
            }
            catch (WebException exception)
            {
                WriteLog("Fout bij MoveIn : + " + ean + " - " + exception.Message, 10, 0);
                //MessageBox.Show(exception.Message);
                return -1;
            }
            catch (Exception exception)
            {
                WriteLog("Fout bij MoveIn : + " + ean + " - " + exception.Message, 10, 0);
                //MessageBox.Show(exception.Message);
                return -1;
            }
            //if (strFileName != "") { File.Delete(strFileName); }
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
        public void OphalenRejections(Boolean blnPV, string strFileName, string netbeheerder, Boolean blnBatch)//, DateTime vanDatum, DateTime totDatum)
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

            nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope enveloppe = new nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope();


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "DDM_O";

         //   receiver.ReceiverID = netbeheerder;// "1114252022907"; moet volgens mij EDSN zijn
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            if (blnPV != true) { source.SenderID = HoofdLV; } else { source.SenderID = HoofdPV; }

            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.MeterReading.MeterReadingRejectionRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.MeterReading.MeterReading meterReading = new nl.Energie.EDSN.MeterReading.MeterReading();


            //String certPath = certpath + @"EDSN2013053100006.p12";
            if (strFileName == "")
            {
                if (blnPV == true)
                {
                    //certPath = certpath + @"EDSN2013053100007.p12";
                    meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
                }
                else
                {
                    meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
                    //meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV));
                }
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
                if (Klant_Config != "")
                {
                    string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                    WriteFileStream = new StreamWriter(path + @"MeterReadingRejections" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, enveloppe);
                    WriteFileStream.Close();

                    string ftpResponse = "";
                    if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingRejections" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingRejections" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    {
                        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                    }

                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, enveloppe);
                    int intOutBoxID = Save_Outbox(header.DocumentID, "", "OphalenRejection", swXML.ToString());
                }
            }

            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReading.MeterReadingRejectionResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();

                if (strFileName == "")
                {
                    retour = meterReading.MeterReadingRejectionRequest(enveloppe);

                    WriteFileStream = new StreamWriter(path + @"MeterReadingRejections" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    string ftpResponse = "";
                    if (Klant_Config != "" || Klant_Config == "" && blnBatch != true && retour.Portaal_Content.Length > 0)
                    {
                        if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingRejections" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingRejections" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                        {
                            //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                        }
                    }

                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);
                    _Doc.Load(path + "MeterReadingRejections" + BestandsAanvulling + ".xml");

                    if (retour.Portaal_Content.Length == 0)
                    {
                        File.Delete(path + "MeterReadingRejections" + BestandsAanvulling + ".xml");
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
                    int inboxID = 0;
                    inboxID = Save_Inbox(27, _Doc.InnerXml.ToString(), "Rejections : " + retour.EDSNBusinessDocumentHeader.MessageID.ToString(), blnBatch);

                    int intedinID = Save_Edine(inboxID, retour.EDSNBusinessDocumentHeader.Source.SenderID, retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID,
                            retour.EDSNBusinessDocumentHeader.CreationTimestamp, retour.EDSNBusinessDocumentHeader.MessageID, "UTILTS", "E11");

                    string ean = rejection.Portaal_MeteringPoint.EANID.ToString();
                    string product = rejection.Portaal_MeteringPoint.ProductType.ToString();
                    string initiator = rejection.Portaal_MeteringPoint.Portaal_Mutation.Initiator.ToString();
                    string mutationReason = rejection.Portaal_MeteringPoint.Portaal_Mutation.MutationReason.ToString();
                    string dossier = "";
                    if (rejection.Portaal_MeteringPoint.Portaal_Mutation.Dossier != null) { dossier = rejection.Portaal_MeteringPoint.Portaal_Mutation.Dossier.ID.ToString(); }
                    string rejectionCode = rejection.Rejection.RejectionCode.ToString();
                    string rejectionText = "";
                    if (rejection.Rejection.RejectionText != null) { rejectionText = rejection.Rejection.RejectionText.ToString(); }

                    Save_MeterStand_Afwijzing(intedinID, ean, initiator, product, mutationReason, dossier, rejectionCode, rejectionText);

                    //if (inboxID > 0) { ProcessMessage.processMessage(inboxID); }
                }




                //Nog check op rejection


            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
                TextReader tr = new StringReader(ex.Detail.InnerXml);
                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

                if (blnBatch || Klant_Config != "")
                {
                    WriteLog("Fout rejections:  - " + S.ErrorText + S.ErrorDetails, 10, -1);
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
                if (blnBatch || Klant_Config != "")
                {
                    WriteLog("Fout rejections:  - " + exception.Message, 10, -1);
                }
                else
                {
                    MessageBox.Show(exception.Message);
                }
            }
            catch (Exception exception)
            {

                if (blnBatch || Klant_Config != "")
                {
                    WriteLog("Fout rejections:  - " + exception.Message, 10, -1);
                }
                else
                {
                    MessageBox.Show(exception.Message);
                }
            }
            if (strFileName != "") { File.Delete(strFileName); }
        }

        public void IndienenDispuut(Boolean blnPV, string strFileName, string ean, DateTime vanDatum, DateTime totDatum, string netbeheerder,
            string dossierID, string meternummer, string aantalregister, string herkomst, string redenmutatie, string product, string nrTelwerkenNormaal,
            string meeteindheidNormaal, string standNormaal, string nrTelwerkenLaag, string meeteindheidLaag, string standLaag, Boolean blnBatch)
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
            header.DocumentID = GetMessageID.getMessageID(ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "DDM_O";
            //receiver.ReceiverID = netbeheerder; moet volgens mij EDSN zijn
            receiver.ReceiverID = "8712423010208";
            destination.Receiver = receiver;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EDSNBusinessDocumentHeader_Source();
            if (blnPV != true) { source.SenderID = HoofdLV; } else { source.SenderID = HoofdPV; }
            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC portaal_content = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC();
            enveloppe.Portaal_Content = portaal_content;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP portaal_meteringPoint = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP();
            portaal_content.Portaal_MeteringPoint = portaal_meteringPoint;
            portaal_meteringPoint.EANID = ean;
            //portaal_meteringPoint.ProductType = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyProductPortaalTypeCode.ELK;
            if (product == "ELK") { portaal_meteringPoint.ProductType = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyProductPortaalTypeCode.ELK; }
            else { portaal_meteringPoint.ProductType = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyProductPortaalTypeCode.GAS; }

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter portaal_meter = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter();

            portaal_content.Portaal_MeteringPoint.Portaal_EnergyMeter = portaal_meter;
            portaal_meter.ID = meternummer;
            portaal_meter.NrOfRegisters = aantalregister;
            int intAantalRegister = int.Parse(aantalregister);

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register[] portaal_register = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register[2];

            portaal_meter.Register = portaal_register;

            portaal_register[0] = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register();
            //portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.KWH;

            if (meeteindheidNormaal == "KWH") { portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.KWH; }
            if (meeteindheidNormaal == "KWT") { portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.KWT; }
            if (meeteindheidNormaal == "M3N") { portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.M3N; }
            if (meeteindheidNormaal == "MTQ") { portaal_register[0].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.MTQ; }

            //portaal_register[0].MeteringDirection = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyFlowDirectionCode.LVR;
            //portaal_register[0].MeteringDirectionSpecified = true;
            portaal_register[0].NrOfDigits = nrTelwerkenNormaal;
            portaal_register[0].TariffType = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyTariffTypeCode.N;
            portaal_register[0].TariffTypeSpecified = true;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading portaal_reading = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading();

            portaal_register[0].Reading = portaal_reading;
            portaal_reading.Reading = standNormaal;
            portaal_reading.ReadingDate = vanDatum;
            //portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003;

            if (herkomst == "Item001") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item001; }
            if (herkomst == "Item002") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item002; }
            if (herkomst == "Fysieke Opname") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003; }
            if (herkomst == "003") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003; }
            if (herkomst == "P4-stand") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item004; }
            if (herkomst == "004") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item004; }
            if (herkomst == "Berekend") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item005; }
            if (herkomst == "005") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item005; }
            if (herkomst == "Item006") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item006; }
            if (herkomst == "Item009") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item009; }
            if (herkomst == "Overeengekomen") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item102; }
            if (herkomst == "102") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item102; }
            if (herkomst == "Klantstand/P1-Stand") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
            if (herkomst == "022") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
            if (herkomst == "22") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
            if (herkomst == "Item90") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item90; }
            if (herkomst == "Item91") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item91; }
            if (herkomst == "Item92") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item92; }
            if (herkomst == "Item93") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item93; }
            if (herkomst == "E01") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.E01; }
            if (herkomst == "E02") { portaal_reading.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.E02; }

            if (intAantalRegister == 2)
            {
                portaal_register[1] = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register();
                //portaal_register[1].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.KWH;

                if (meeteindheidLaag == "KWH") { portaal_register[1].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.KWH; }
                if (meeteindheidLaag == "KWT") { portaal_register[1].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.KWT; }
                if (meeteindheidLaag == "M3N") { portaal_register[1].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.M3N; }
                if (meeteindheidLaag == "MTQ") { portaal_register[1].MeasureUnit = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_MeasureUnitPortaalCode.MTQ; }

                //portaal_register[1].MeteringDirection = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyFlowDirectionCode.LVR;
                //portaal_register[1].MeteringDirectionSpecified = true;
                portaal_register[1].NrOfDigits = nrTelwerkenLaag;
                portaal_register[1].TariffType = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyTariffTypeCode.L;
                portaal_register[1].TariffTypeSpecified = true;

                nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading portaal_reading2 = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading();

                portaal_register[1].Reading = portaal_reading2;
                portaal_reading2.Reading = standLaag;
                portaal_reading2.ReadingDate = vanDatum;
                portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003;
                if (herkomst == "Item001") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item001; }
                if (herkomst == "Item002") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item002; }
                if (herkomst == "Fysieke Opname") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003; }
                if (herkomst == "003") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item003; }
                if (herkomst == "P4-stand") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item004; }
                if (herkomst == "004") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item004; }
                if (herkomst == "Berekend") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item005; }
                if (herkomst == "005") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item005; }
                if (herkomst == "Item006") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item006; }
                if (herkomst == "Item009") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item009; }
                if (herkomst == "Overeengekomen") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item102; }
                if (herkomst == "102") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item102; }
                if (herkomst == "Klantstand/P1-Stand") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
                if (herkomst == "022") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
                if (herkomst == "22") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item22; }
                if (herkomst == "Item90") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item90; }
                if (herkomst == "Item91") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item91; }
                if (herkomst == "Item92") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item92; }
                if (herkomst == "Item93") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.Item93; }
                if (herkomst == "E01") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.E01; }
                if (herkomst == "E02") { portaal_reading2.ReadingMethod = nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_EnergyMeterReadingMethodCode.E02; }
            }

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_PM portaal_mutation = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope_PC_PMP_PM();
            portaal_content.Portaal_MeteringPoint.Portaal_Mutation = portaal_mutation;
            portaal_mutation.Initiator = HoofdLV;
            //    portaal_mutation.Consumer = "1114252022907"is robin dat is niet goed
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
            portaal_dossier.ID = dossierID;


            //portaal_meter[0].Register

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDispute meterReadingDispute = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDispute();


            //String certPath = certpath + @"EDSN2013053100006.p12";
            if (strFileName == "")
            {
                if (blnPV == true)
                {
                    //certPath = certpath + @"EDSN2013053100007.p12";
                    meterReadingDispute.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
                }
                else
                {
                    meterReadingDispute.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
                    //meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV));
                }
            }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            meterReadingDispute.Url = KC.CarUrl + @"synchroon/ResponderMeterReadingDisputeRespondingActivity";

            meterReadingDispute.Timeout = 120000;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeAcknowledgementEnvelope retour = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeAcknowledgementEnvelope();



            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeNotificationEnvelope));
            TextWriter WriteFileStream;

            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                WriteFileStream = new StreamWriter(path + @"MeterReadingDisputeNotification" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();

                string ftpResponse = "";
                if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingDisputeNotification" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingDisputeNotification" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                {
                    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                }

                StringWriter swXML = new StringWriter();
                serializer.Serialize(swXML, enveloppe);
                int intOutBoxID = Save_Outbox(header.DocumentID, ean, "Dispuut", swXML.ToString());


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

                    WriteFileStream = new StreamWriter(path + @"MeterReadingDisputeNotification" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    string ftpResponse = "";
                    if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingDisputeNotification" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingDisputeNotification" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    {
                        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                    }

                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);
                    _Doc.Load(path + "MeterReadingDisputeNotification" + BestandsAanvulling + ".xml");

                    MessageBox.Show("Dispuut verstuurd");
                }
                else
                {
                    retour = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeAcknowledgementEnvelope();

                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeAcknowledgementEnvelope));
                    //Save to file kan weg
                    //WriteFileStream = new StreamWriter(path + @"MeterReadingDisputeNotification" + BestandsAanvulling + ".xml");
                    //serializer.Serialize(WriteFileStream, retour);
                    //WriteFileStream.Close();
                }


                int inboxID = 0;
                inboxID = Save_Inbox(39, _Doc.InnerXml.ToString(), "Dispuut : " + retour.EDSNBusinessDocumentHeader.MessageID.ToString(), blnBatch);
                int intedinID = Save_Edine(inboxID, retour.EDSNBusinessDocumentHeader.Source.SenderID, retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID,
                        retour.EDSNBusinessDocumentHeader.CreationTimestamp, retour.EDSNBusinessDocumentHeader.MessageID, "DISPUU", "E11");
                Save_Dispuut(intedinID, DateTime.Now, ean, vanDatum, totDatum, netbeheerder, dossierID, redenmutatie, herkomst, standNormaal, standLaag, false, header.MessageID, true, "", false);
                if (inboxID > 0) { ProcessMessage.processMessage(inboxID, ConnString); }
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
            if (strFileName != "") { File.Delete(strFileName); }
        }

        public void OphalenDispuut(Boolean blnPV, string strFileName, string netbeheerder, Boolean blnBatch)
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

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope enveloppe = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope();


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(ConnString);
            header.ExpiresAt = DateTime.Now.AddMinutes(200);
            header.ExpiresAtSpecified = true;
            header.MessageID = System.Guid.NewGuid().ToString();
            enveloppe.EDSNBusinessDocumentHeader = header;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
            header.Destination = destination;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
            receiver.Authority = "";
            receiver.ContactTypeIdentifier = "DDM_O";
            //  receiver.ReceiverID = netbeheerder; moet volgens mij EDSN zijn
            receiver.ReceiverID = "8712423010208";
            
            destination.Receiver = receiver;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_EDSNBusinessDocumentHeader_Source();
            if (blnPV != true) { source.SenderID = HoofdLV; } else { source.SenderID = HoofdPV; }
            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
            //source.ContactTypeIdentifier = "DDQ_O";
            header.Source = source;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope_Portaal_Content();
            enveloppe.Portaal_Content = portaal_content;



            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDispute meterReadingDispute = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDispute();


            //String certPath = certpath + @"EDSN2013053100006.p12";
            if (strFileName == "")
            {
                if (blnPV == true)
                {
                    //certPath = certpath + @"EDSN2013053100007.p12";
                    meterReadingDispute.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
                }
                else
                {
                    meterReadingDispute.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
                    //meterReading.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV));
                }
            }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            meterReadingDispute.Url = KC.CarUrl + @"synchroon/ResponderMeterReadingDisputeRespondingActivity";

            meterReadingDispute.Timeout = 120000;

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope retour = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope();



            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeRequestEnvelope));
            TextWriter WriteFileStream;

            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                WriteFileStream = new StreamWriter(path + @"MeterReadingDisputeRequest" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();

                string ftpResponse = "";
                if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingDisputeRequest" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingDisputeRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                {
                    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                }


                StringWriter swXML = new StringWriter();
                serializer.Serialize(swXML, enveloppe);
                int intOutBoxID = Save_Outbox(header.DocumentID, "", "Ophalen dispuut", swXML.ToString());

            }

            try
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }
                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope));
                XmlDocument _Doc = new XmlDocument();

                if (strFileName == "")
                {

                    retour = meterReadingDispute.MeterReadingDisputeRequest(enveloppe);

                    WriteFileStream = new StreamWriter(path + @"MeterReadingDisputeRequest" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    string ftpResponse = "";
                    if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingDisputeRequest" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingDisputeRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    {
                        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                    }

                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);
                    _Doc.Load(path + "MeterReadingDisputeRequest" + BestandsAanvulling + ".xml");

                    MessageBox.Show("Dispuut opgehaald");
                }
                else
                {
                    retour = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope();

                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope));
                    //Save to file kan weg
                    WriteFileStream = new StreamWriter(path + @"MeterReadingDisputeRequest" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();
                }



                int inboxID = 0;
                if (retour.Portaal_Content.Length > 0)
                {
                    inboxID = Save_Inbox(39, _Doc.InnerXml.ToString(), "Terugkoppeling : " + retour.EDSNBusinessDocumentHeader.MessageID.ToString(), blnBatch);

                    nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope_PC_PMP[] portaal_MeteringPoints = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope_PC_PMP[retour.Portaal_Content.Length];
                    portaal_MeteringPoints = retour.Portaal_Content;

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

                        Save_Dispuut(inboxID, DateTime.Now, ean, Datum, Datum, netbeheerder, Dossier, meteringpoint.Portaal_Mutation.MutationReason.ToString(),
                            herkomst, StandNormaal, StandLaag, false, referentie, false, "", false);
                    }

                    if (inboxID > 0) { ProcessMessage.processMessage(inboxID, ConnString); }
                    //Nog check op rejection


                }
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
            if (strFileName != "") { File.Delete(strFileName); }
        }

        public void TerugkoppelenDispuut(Boolean blnPV, string strFileName, string netbeheerder, string ean, string referentie, string Dossier,
            Boolean geaccepteerd, string Opmerking, int edineID, Boolean blnBatch)
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

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope enveloppe = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope();


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultNotificationEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(ConnString);
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
            if (blnPV != true) { source.SenderID = HoofdLV; } else { source.SenderID = HoofdPV; }
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
            portaal_content[0].Portaal_Mutation.ExternalReference = referentie;
            portaal_content[0].Portaal_Mutation.Initiator = HoofdLV;
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
                    meterReadingDispute.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
                }
                else
                {
                    meterReadingDispute.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
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

            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                WriteFileStream = new StreamWriter(path + @"MeterReadingDisputeResult" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();

                string ftpResponse = "";
                if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingDisputeResult" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingDisputeResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                {
                    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                }


                StringWriter swXML = new StringWriter();
                serializer.Serialize(swXML, enveloppe);
                int intOutBoxID = Save_Outbox(header.DocumentID, "", "RequestTerugkoppelen", swXML.ToString());

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

                    WriteFileStream = new StreamWriter(path + @"MeterReadingDisputeResult" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    string ftpResponse = "";
                    if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingDisputeResult" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingDisputeResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    {
                        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                    }

                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);
                    _Doc.Load(path + "MeterReadingDisputeResult" + BestandsAanvulling + ".xml");

                    MessageBox.Show("Dispuut Teruggekoppeld");
                }
                else
                {
                    retour = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultAcknowledgementEnvelope();

                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultAcknowledgementEnvelope));
                    //Save to file kan weg
                    WriteFileStream = new StreamWriter(path + @"MeterReadingDisputeResult" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();
                }


                //if (retour.Portaal_Content.GetType == typeof(nl.Energie.EDSN.MeterReadingDispute.
                int inboxID = 0;
                inboxID = Save_Inbox(39, _Doc.InnerXml.ToString(), "Terugkoppeling : " + retour.EDSNBusinessDocumentHeader.MessageID.ToString(), blnBatch);

                Update_Dispuut(edineID, geaccepteerd, true);
                //nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope_Portaal_Content_Portaal_MeteringPoint[] portaal_MeteringPoints = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope_Portaal_Content_Portaal_MeteringPoint[retour.Portaal_Content.Length];
                //portaal_MeteringPoints = retour.Portaal_Content;

                //foreach (nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope_Portaal_Content_Portaal_MeteringPoint meteringpoint in portaal_MeteringPoints)
                //{
                //    ean = meteringpoint.EANID;

                //    Dossier = meteringpoint.Portaal_Mutation.Dossier.ID;
                //    int nrRegisters = int.Parse(meteringpoint.Portaal_EnergyMeter.NrOfRegisters);

                //    string StandNormaal = "";
                //    string StandLaag = "";
                //    referentie = meteringpoint.Portaal_Mutation.ExternalReference;
                //    DateTime Datum = DateTime.MinValue;
                //    string herkomstEDSN = "";
                //    //string netbeheerder = meteringpoint.Portaal_Mutation.Initiator;

                //    foreach (nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_EnergyMeter_Register register in meteringpoint.Portaal_EnergyMeter.Register)
                //    {
                //        if (register.TariffType == nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope_EnergyTariffTypeCode.N ||
                //            register.TariffType == nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResponseEnvelope_EnergyTariffTypeCode.T)
                //        {
                //            StandNormaal = register.Reading.Reading;
                //            Datum = register.Reading.ReadingDate;
                //            herkomstEDSN = register.Reading.ReadingMethod.ToString();

                //        }
                //        else
                //        {
                //            StandLaag = register.Reading.Reading;
                //            Datum = register.Reading.ReadingDate;
                //            herkomstEDSN = register.Reading.ReadingMethod.ToString();
                //        }
                //    }
                //    string herkomst = "";
                //    if (herkomstEDSN == "Item003") { herkomst = "Fysieke Opname"; }
                //    if (herkomstEDSN == "Item004") { herkomst = "P4-stand"; }
                //    if (herkomstEDSN == "Item005") { herkomst = "Berekend"; }
                //    if (herkomstEDSN == "Item102") { herkomst = "Overeengekomen"; }
                //    if (herkomstEDSN == "Item22") { herkomst = "Klantstand/P1-Stand"; }

                //    Save_Dispuut(inboxID, DateTime.Now, ean, Datum, Datum, netbeheerder, Dossier, meteringpoint.Portaal_Mutation.MutationReason.ToString(),
                //        herkomst, StandNormaal, StandLaag, false, referentie, false, "", true);
                //}

                if (inboxID > 0) { ProcessMessage.processMessage(inboxID, ConnString); }
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
            if (strFileName != "") { File.Delete(strFileName); }
        }

        public void OphalenTerugKoppelingen(Boolean blnPV, string strFileName, string netbeheerder, Boolean blnBatch)
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

            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope enveloppe = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope();


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultRequestEnvelope_EDSNBusinessDocumentHeader();
            header.ContentHash = "";
            header.CreationTimestamp = DateTime.Now;
            header.DocumentID = GetMessageID.getMessageID(ConnString);
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
            if (blnPV != true) { source.SenderID = HoofdLV; } else { source.SenderID = HoofdPV; }
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
                    meterReadingDispute.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
                }
                else
                {
                    meterReadingDispute.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
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

            if (strFileName == "")
            {
                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
                WriteFileStream = new StreamWriter(path + @"MeterReadingDisputeResultRequest" + BestandsAanvulling + ".xml");
                serializer.Serialize(WriteFileStream, enveloppe);
                WriteFileStream.Close();

                string ftpResponse = "";
                if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingDisputeResultRequest" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingDisputeResultRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                {
                    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                }


                StringWriter swXML = new StringWriter();
                serializer.Serialize(swXML, enveloppe);
                int intOutBoxID = Save_Outbox(header.DocumentID, "", "Ophalen dispuut", swXML.ToString());

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

                    WriteFileStream = new StreamWriter(path + @"MeterReadingDisputeResultRequest" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();

                    string ftpResponse = "";
                    if (FTPClass.FtpSendFile(strFTPServer + @"MeterReadingDisputeResultRequest" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"MeterReadingDisputeResultRequest" + BestandsAanvulling + ".xml", out ftpResponse) == false)
                    {
                        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
                    }

                    StringWriter swXML = new StringWriter();
                    serializer.Serialize(swXML, retour);
                    _Doc.Load(path + "MeterReadingDisputeResultRequest" + BestandsAanvulling + ".xml");

                    MessageBox.Show("TerugKoppelingen opgehaald");
                }
                else
                {
                    retour = new nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultResponseEnvelope();

                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MeterReadingDispute.MeterReadingDisputeResultResponseEnvelope));
                    //Save to file kan weg
                    WriteFileStream = new StreamWriter(path + @"MeterReadingDisputeResultRequest" + BestandsAanvulling + ".xml");
                    serializer.Serialize(WriteFileStream, retour);
                    WriteFileStream.Close();
                }



                int inboxID = 0;
                inboxID = Save_Inbox(39, _Doc.InnerXml.ToString(), "Terugkoppeling : " + retour.EDSNBusinessDocumentHeader.MessageID.ToString(), blnBatch);

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
                    int edineID = 0;

                    try
                    {
                        SqlConnection cnPubs = new SqlConnection(ConnString);
                        cnPubs.Open();
                        string SQLStatement = "SELECT  [EdineId] FROM [Messages].[dbo].[Dispuut] where EanCode=@EanCode and Afgehandeld=0";
                        SqlCommand cmd = new SqlCommand(SQLStatement, cnPubs);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@EANCode", ean);
                        cmd.CommandTimeout = 12000;
                        edineID = int.Parse(cmd.ExecuteScalar().ToString());

                        Update_Dispuut(edineID, blnGeaccepteerd, true);


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
            if (strFileName != "") { File.Delete(strFileName); }
        }

        public int Save_Inbox(int edineMessagetype_ID, string message, string subject, Boolean blnBatch)
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;
            int inboxID = -1;
            string strUID = GetMessageID.getMessageID(ConnString);

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
                    ",@UID " +
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
            cmdSaveInbox.Parameters.AddWithValue("@UID", strUID);
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
                if (blnBatch == false)
                {
                    MessageBox.Show("Er is iets fout gegaan met het bewaren van het verbruik Electra (inbox), we adviseren U contact op te nemen met IT");
                    MessageBox.Show(ex.ToString());
                }
                else
                {
                    WriteLog("Fout opslaan in box Meterreading :" + ex.ToString(), 10, -1);
                }
            }

            cnPubs.Close();
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

            cnPubs.Close();
            return edineID;

        }

        public int Save_UTILTS_E11_Header(int Edine_Id, string sender, string receiver, DateTime messageProcessed, DateTime processingStart, DateTime processingEnd, string fase, Boolean blnBatch)
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
                if (blnBatch || Klant_Config != "")
                {
                    WriteLog("Fout Save_UTILTS_E11_Header :" + ex.Message, 10, -1);
                }
                else
                {
                    MessageBox.Show("Er is iets fout gegaan met het bewaren van het verbruik Electra (inbox), we adviseren U contact op te nemen met IT");
                    MessageBox.Show(ex.ToString());
                }
            }

            cnPubs.Close();
            return inboxID;

        }

        public int Save_UTILTS_E11(int Edine_Id, string transactionId, string transaction_Status, string loc_GC, string readingType, DateTime dtmUTCReading, string typeofMeteringPoint,
            string reading_TimeFrame, string originCode, decimal volume, string volumeType, string volumeStatus, string productID, string measureUnit, Boolean blnBatch)
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
                if (blnBatch || Klant_Config != "")
                {
                    WriteLog("Fout Save_UTILTS_E11 :" + ex.Message, 10, -1);
                }
                else
                {
                    MessageBox.Show("Er is iets fout gegaan met het bewaren van het verbruik Electra (inbox), we adviseren U contact op te nemen met IT");
                    MessageBox.Show(ex.ToString());
                }
            }

            cnPubs.Close();
            return inboxID;

        }

        public int Save_Dispuut(int Edine_Id, DateTime verstuurd_DT, string eanCode, DateTime begin_D, DateTime eind_D, String netbeheerder,
            string dossierID, string redenMutatie, string herkomst, string StandNormaal, string StandLaag, Boolean geaccepteerd, String referentie,
            Boolean verstuurd, string opmerking, Boolean afgehandeld)
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;
            int inboxID = -1;

            cnPubs.Open();
            SQLstatement =
                    "INSERT INTO Messages.[dbo].[Dispuut] " +
                    "(EdineId " +
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
                    "(@EdineId " +
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
            cmdSaveInbox.Parameters.Add(new SqlParameter("@EdineId", SqlDbType.Int));
            cmdSaveInbox.Parameters["@EdineId"].Value = Edine_Id;
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
            cmdSaveInbox.Parameters["@Referentie"].Value = referentie;
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
                MessageBox.Show("Er is iets fout gegaan met het bewaren van het dispuut, we adviseren U contact op te nemen met IT");
                MessageBox.Show(ex.ToString());
            }

            cnPubs.Close();
            return inboxID;

        }

        public int Update_Dispuut(int Edine_Id, Boolean geaccepteerd, Boolean afgehandeld)
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;
            int inboxID = -1;

            cnPubs.Open();
            SQLstatement =
                    "Update Messages.[dbo].[Dispuut] " +
                    " SET Geaccepteerd=@Geaccepteerd " +
                    ",Afgehandeld=@Afgehandeld " +
                    "WHERE EdineID=@EdineID";




            SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveInbox.Parameters.Add(new SqlParameter("@EdineId", SqlDbType.Int));
            cmdSaveInbox.Parameters["@EdineId"].Value = Edine_Id;
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
                MessageBox.Show("Er is iets fout gegaan met het bijwerken van het dispuut, we adviseren U contact op te nemen met IT");
                MessageBox.Show(ex.ToString());
            }

            cnPubs.Close();
            return inboxID;

        }

        //public int Save_MeterStand(int Edine_Id, DateTime verstuurd_DT, string eanCode, DateTime begin_D, DateTime eind_D, String netbeheerder,
        //    string dossierID, string redenMutatie, string herkomst, string StandNormaal, string StandLaag, Boolean Levering)
        //{
        //    SqlConnection cnPubs = new SqlConnection(ConnString);
        //    string SQLstatement;
        //    int inboxID = -1;

        //    cnPubs.Open();
        //    SQLstatement =
        //            "INSERT INTO [dbo].[MeterStand] " +
        //            "([EdineId] " +
        //            ",[Direction] " +
        //            ",[TarifType] " +
        //            ",[Verstuurd_DT] " +
        //            ",[EanCode] " +
        //            ",[Begin_D] " +
        //            ",[Eind_D] " +
        //            ",[Netbeheerder] " +
        //            ",[DossierID] " +
        //            ",[RedenMutatie] " +
        //            ",[Herkomst] " +
        //            ",[Stand]) " +
        //            "VALUES " +
        //            "(@EdineId " +
        //            ",@Direction " +
        //            ",@TarifType " +
        //            ",@Verstuurd_DT " +
        //            ",@EanCode " +
        //            ",@Begin_D " +
        //            ",@Eind_D " +
        //            ",@Netbeheerder " +
        //            ",@DossierID " +
        //            ",@RedenMutatie " +
        //            ",@Herkomst " +
        //            ",@Stand)";

        //    SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
        //    cmdSaveInbox.Parameters.Add(new SqlParameter("@EdineId", SqlDbType.Int));
        //    cmdSaveInbox.Parameters["@EdineId"].Value = Edine_Id;
        //    cmdSaveInbox.Parameters.Add(new SqlParameter("@Verstuurd_DT", SqlDbType.DateTime));
        //    cmdSaveInbox.Parameters["@Verstuurd_DT"].Value = verstuurd_DT;
        //    cmdSaveInbox.Parameters.Add(new SqlParameter("@EanCode", SqlDbType.BigInt));
        //    cmdSaveInbox.Parameters["@EanCode"].Value = eanCode;
        //    cmdSaveInbox.Parameters.Add(new SqlParameter("@Begin_D", SqlDbType.Date));
        //    cmdSaveInbox.Parameters["@Begin_D"].Value = begin_D;
        //    cmdSaveInbox.Parameters.Add(new SqlParameter("@Eind_D", SqlDbType.Date));
        //    cmdSaveInbox.Parameters["@Eind_D"].Value = eind_D;
        //    cmdSaveInbox.Parameters.Add(new SqlParameter("@Netbeheerder", SqlDbType.BigInt));
        //    cmdSaveInbox.Parameters["@Netbeheerder"].Value = netbeheerder;
        //    cmdSaveInbox.Parameters.Add(new SqlParameter("@DossierID", SqlDbType.VarChar));
        //    cmdSaveInbox.Parameters["@DossierID"].Value = dossierID;
        //    cmdSaveInbox.Parameters.Add(new SqlParameter("@RedenMutatie", SqlDbType.VarChar));
        //    cmdSaveInbox.Parameters["@RedenMutatie"].Value = redenMutatie;
        //    cmdSaveInbox.Parameters.Add(new SqlParameter("@Herkomst", SqlDbType.VarChar));
        //    cmdSaveInbox.Parameters["@Herkomst"].Value = herkomst;
        //    cmdSaveInbox.Parameters.Add(new SqlParameter("@StandNormaal", SqlDbType.Decimal));
        //    cmdSaveInbox.Parameters["@StandNormaal"].Value = StandNormaal;
        //    if (StandLaag.Trim() != "")
        //    {
        //        cmdSaveInbox.Parameters.Add(new SqlParameter("@StandLaag", SqlDbType.VarChar));
        //        cmdSaveInbox.Parameters["@StandLaag"].Value = StandLaag;
        //    }


        //    try
        //    {
        //        cmdSaveInbox.ExecuteNonQuery();
        //        //inboxID = (int)cmdSaveInbox.Parameters["@inboxID"].Value;
        //        //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Er is iets fout gegaan met het bewaren van de MeterStand, we adviseren U contact op te nemen met IT");
        //        MessageBox.Show(ex.ToString());
        //    }

        //    cnPubs.Close();
        //    return inboxID;

        //}
        public int Save_MeterStand_Header(int edine_Id, Boolean verstuurd, DateTime verstuurd_DT, string eanCode, DateTime begin_D,
            DateTime eind_D, String netbeheerder, string dossierID, string redenMutatie, string enrollment_ID, int pMeterStand_ID, Boolean fout)
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;
            int meterStand_ID = -1;

            try
            {
                cnPubs.Open();
                if (pMeterStand_ID == -1)
                {
                    SQLstatement =
                            "INSERT INTO [Messages].[dbo].[MeterStanden_Header] " +
                            "([EdineId] " +
                            ",[Verstuurd] ";
                    if (verstuurd_DT > DateTime.MinValue) { SQLstatement = SQLstatement + ",[Verstuurd_DT]"; }
                    SQLstatement = SQLstatement + ",[EanCode] " +
                            ",[Begin_D] " +
                            ",[Eind_D] " +
                            ",[Netbeheerder] " +
                            ",[DossierID] " +
                            ",[RedenMutatie] " +
                            ",[Enrollment_ID] " +
                            ",[Fout]) " +
                            "VALUES " +
                            "(@EdineId " +
                            ",@Verstuurd ";
                    if (verstuurd_DT > DateTime.MinValue) { SQLstatement = SQLstatement + ",@Verstuurd_DT "; }
                    SQLstatement = SQLstatement + ",@EanCode " +
                            ",@Begin_D " +
                            ",@Eind_D " +
                            ",@Netbeheerder " +
                            ",@DossierID " +
                            ",@RedenMutatie " +
                            ",@Enrollment_ID " +
                            ",@Fout ); SELECT @Meterstand_ID = SCOPE_IDENTITY();";
                }
                else
                {
                    SQLstatement = " UPDATE [Messages].[dbo].[MeterStanden_Header] " +
                        "SET [EdineId] = @EdineId " +
                        ",[EanCode] = @EanCode " +
                        ",[Verstuurd] = @Verstuurd " +
                        ",[Verstuurd_DT] = @Verstuurd_DT " +
                        ",[Begin_D] = @Begin_D " +
                        ",[Eind_D] = @Eind_D " +
                        ",[Netbeheerder] = @Netbeheerder " +
                        ",[DossierID] = @DossierID " +
                        ",[RedenMutatie] = @RedenMutatie " +
                        ",[Enrollment_ID] = @Enrollment_ID " +
                        ",[Fout] = @Fout " +
                        "WHERE ID=@MeterStand_ID";
                }
                SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
                cmdSaveInbox.Parameters.Add(new SqlParameter("@EdineId", SqlDbType.Int));
                cmdSaveInbox.Parameters["@EdineId"].Value = edine_Id;
                cmdSaveInbox.Parameters.Add(new SqlParameter("@Verstuurd", SqlDbType.Bit));
                cmdSaveInbox.Parameters["@Verstuurd"].Value = verstuurd;
                if (verstuurd_DT > DateTime.MinValue)
                {
                    cmdSaveInbox.Parameters.Add(new SqlParameter("@Verstuurd_DT", SqlDbType.DateTime));
                    cmdSaveInbox.Parameters["@Verstuurd_DT"].Value = verstuurd_DT;
                }
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
                cmdSaveInbox.Parameters.Add(new SqlParameter("@Enrollment_ID", SqlDbType.VarChar));
                cmdSaveInbox.Parameters["@Enrollment_ID"].Value = enrollment_ID;
                cmdSaveInbox.Parameters.Add(new SqlParameter("@Fout", SqlDbType.Bit));
                cmdSaveInbox.Parameters["@Fout"].Value = fout;
                if (pMeterStand_ID != -1)
                {
                    cmdSaveInbox.Parameters.Add(new SqlParameter("@Meterstand_ID", SqlDbType.Int));
                    cmdSaveInbox.Parameters["@Meterstand_ID"].Value = pMeterStand_ID;
                    //cmdSaveInbox.Parameters["@Meterstand_ID"].Direction = ParameterDirection.Output;
                }
                else
                {
                    cmdSaveInbox.Parameters.Add(new SqlParameter("@Meterstand_ID", SqlDbType.Int));
                    cmdSaveInbox.Parameters["@Meterstand_ID"].Direction = ParameterDirection.Output;
                }
                try
                {
                    cmdSaveInbox.ExecuteNonQuery();
                    if (pMeterStand_ID == -1) { meterStand_ID = (int)cmdSaveInbox.Parameters["@Meterstand_ID"].Value; }
                    else { meterStand_ID = pMeterStand_ID; }
                    //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
                }
                catch (Exception ex)
                {
                    WriteLog("Fout bij wegeschrijven MeterStand_Header :  - " + eanCode + " - " + ex.ToString(), 10, edine_Id);
                    //MessageBox.Show("Er is iets fout gegaan met het bewaren van de MeterStand_Header, we adviseren U contact op te nemen met IT");
                    //MessageBox.Show(ex.ToString());
                }

                cnPubs.Close();
            }
            catch
            { meterStand_ID = -1; }
            return meterStand_ID;

        }

        public int Save_MeterStand(int meterStand_ID, int edine_Id, string herkomst, string tarifType, string stand, string direction, string volume,
             string beginStand, string herkomstBeginStand, string calorificCorrectedVolume, Boolean update)
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;
            int inboxID = -1;

            cnPubs.Open();
            if (update == false)
            {
                SQLstatement =
                        "INSERT INTO [Messages].[dbo].[MeterStand] " +
                        "([MeterStand_ID] " +
                        ",[EdineId] " +
                        ",[Direction] " +
                        ",[TarifType] " +
                        ",[Herkomst] " +
                        ",[Stand] " +
                        ",[Volume] " +
                        ",[BeginStand] " +
                        ",[HerkomstBeginStand] " +
                        ",[CalorificCorrectedVolume]) " +
                        "VALUES " +
                        "(@MeterStand_ID " +
                        ",@EdineId " +
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
                SQLstatement = "UPDATE [Messages].[dbo].[MeterStand] " +
                    "SET [EdineId] = @EdineId " +
                    ",[Herkomst] = @Herkomst " +
                    ",[Stand] = @Stand " +
                    ",[Volume] = @Volume " +
                    ",[BeginStand] = @BeginStand " +
                    ",[HerkomstBeginStand] = @HerkomstBeginStand " +
                    ",[CalorificCorrectedVolume] = @CalorificCorrectedVolume " +
                    "WHERE [MeterStand_ID] = @MeterStand_ID and [Direction] = @Direction and [TarifType] = @TarifType";
            }
            SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveInbox.Parameters.Add(new SqlParameter("@MeterStand_ID", SqlDbType.Int));
            cmdSaveInbox.Parameters["@MeterStand_ID"].Value = meterStand_ID;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@EdineId", SqlDbType.Int));
            cmdSaveInbox.Parameters["@EdineId"].Value = edine_Id;
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
                WriteLog("Fout bij wegeschrijven MeterStand (meterstand_ID):  - " + meterStand_ID + " - " + ex.ToString(), 10, edine_Id);
                //MessageBox.Show("Er is iets fout gegaan met het bewaren van de MeterStand, we adviseren U contact op te nemen met IT");
                //MessageBox.Show(ex.ToString());
            }

            cnPubs.Close();
            return inboxID;

        }

        private int Save_Outbox(string messageID, string ean18_Nummer, string berichtCode, string switchBericht)
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;
            int outboxMessageID = -1;

            cnPubs.Open();
            SQLstatement =
                    "INSERT INTO [Messages].[dbo].[Outbox] " +
                    "([Message_id] " +
                    ",[EdineMessagetype_id] " +
                    ",[Bericht] " +
                    ",[Omschrijving] " +
                    ",[Verzonden] " +
                    ",[EdineMailadres] " +
                    ",[BerichtStatus] " +
                    ",[Afzender] " +
                    ",[Outboxtype_ID]) " +
                    // "OUTPUT @outboxID = inserted.ID " +
                    "VALUES " +
                    "(@id " +
                    ",@Messagetype_id  " +  //11 = UTILMD 392
                    ",@switchBericht " +
                    ",@omschrijving " +
                    ",getdate() " +
                    ",'Energie' " +
                    ",'VERSTUURD' " +
                    ",@EmailAdres " +
                    ",1); SELECT @outboxID = SCOPE_IDENTITY();";



            SqlCommand cmdSaveOutbox = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveOutbox.Parameters.Add(new SqlParameter("@id", SqlDbType.VarChar, 14));
            cmdSaveOutbox.Parameters["@id"].Value = messageID;
            cmdSaveOutbox.Parameters.Add(new SqlParameter("@switchBericht", SqlDbType.NText));
            cmdSaveOutbox.Parameters["@switchBericht"].Value = switchBericht;
            cmdSaveOutbox.Parameters.Add(new SqlParameter("@omschrijving", SqlDbType.NText));
            cmdSaveOutbox.Parameters["@omschrijving"].Value = berichtCode + " " + ean18_Nummer;
            cmdSaveOutbox.Parameters.Add(new SqlParameter("@EmailAdres", SqlDbType.NText));

            cmdSaveOutbox.Parameters["@EmailAdres"].Value = "WebService";//eMailAdres;

            cmdSaveOutbox.Parameters.Add(new SqlParameter("@Messagetype_id", SqlDbType.Int));
            //if (berichtCode == "MoveIn")
            //{
            //    cmdSaveOutbox.Parameters["@Messagetype_id"].Value = "11";
            //}
            //if (berichtCode == "MoveOut")
            //{
            //    cmdSaveOutbox.Parameters["@Messagetype_id"].Value = "13";
            //}
            //if (berichtCode == "EndOfSupply")
            //{
            //    cmdSaveOutbox.Parameters["@Messagetype_id"].Value = "13";
            //}
            //if (berichtCode == "ChangeOfPV")
            //{
            //    cmdSaveOutbox.Parameters["@Messagetype_id"].Value = "11";
            //}
            //else if (berichtCode == "") //????
            //{
            //    cmdSaveOutbox.Parameters["@Messagetype_id"].Value = "14";
            //}
            cmdSaveOutbox.Parameters["@Messagetype_id"].Value = "27";
            cmdSaveOutbox.Parameters.Add(new SqlParameter("@outboxID", SqlDbType.Int));
            cmdSaveOutbox.Parameters["@outboxID"].Direction = ParameterDirection.Output;
            try
            {
                cmdSaveOutbox.ExecuteNonQuery();
                outboxMessageID = (int)cmdSaveOutbox.Parameters["@outboxID"].Value;
                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
            }
            catch (Exception ex)
            {

                MessageBox.Show("Er is iets fout gegaan met het bewaren van het Meterstand/Dispuut (Outbox), we adviseren U contact op te nemen met IT");
                MessageBox.Show(ex.ToString());
            }

            cnPubs.Close();

            return outboxMessageID;

        }
        public int Save_MeterStand_Afwijzing(int intedineID, string ean, string initiator, string product, string mutationReason,
            string dossierID, string rejectionCode, string rejectionText)
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;
            int inboxID = -1;

            cnPubs.Open();
            SQLstatement =
                    "INSERT INTO [Messages].[dbo].[MeterStand_Afwijzing] " +
                    "([EdineId] " +
                    ",[Ean18_Code] " +
                    ",[Product] " +
                    ",[Initiator] " +
                    ",[MutationReason] " +
                    ",[DossierID] " +
                    ",[RejectionCode] " +
                    ",[RejectionText]) " +
                    "VALUES " +
                    "(@EdineId " +
                    ",@Ean18_Code " +
                    ",@Product " +
                    ",@Initiator " +
                    ",@MutationReason " +
                    ",@DossierID " +
                    ",@RejectionCode " +
                    ",@RejectionText)";


            SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveInbox.Parameters.Add(new SqlParameter("@EdineId", SqlDbType.Int));
            cmdSaveInbox.Parameters["@EdineId"].Value = intedineID;
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
            catch (Exception ex)
            {
                MessageBox.Show("Er is iets fout gegaan met het bewaren van het Meterstand rejection, we adviseren U contact op te nemen met IT");
                MessageBox.Show(ex.ToString());
            }

            cnPubs.Close();
            return inboxID;

        }

        public void WriteLog(string description, int LevelID, int inbox_ID)
        {
            try
            {
                SqlConnection conn = new SqlConnection(ConnString);
                conn.Open();
                SqlCommand cmdLog = new SqlCommand();
                cmdLog.Connection = conn;
                string str_SQL = "insert into Messages.dbo.ApplicationLogs (TimeStmp, Description, SourceID, LevelID, Inbox_ID) values(@TimeStmp, @Description, " +
                    " @SourceID, @LevelID, @Inbox_ID)";
                cmdLog.CommandText = str_SQL;
                cmdLog.Parameters.AddWithValue("@TimeStmp", DateTime.Now);
                string strDescription = description;
                if (description.Length > 500) { strDescription = strDescription.Substring(0, 500); }
                cmdLog.Parameters.AddWithValue("@Description", strDescription);
                cmdLog.Parameters.AddWithValue("@SourceID", 4);
                cmdLog.Parameters.AddWithValue("@LevelID", LevelID);
                cmdLog.Parameters.AddWithValue("@Inbox_ID", inbox_ID);
                cmdLog.ExecuteNonQuery();
                conn.Close();
            }
            catch //(Exception ex)
            {
                //EventLog eventlog = new EventLog("Application");
                //eventlog.Source = "Energie App";
                //eventlog.WriteEntry("WriteLog : " + ex.Message, EventLogEntryType.Error, 0);
            }
        }

    }
}
