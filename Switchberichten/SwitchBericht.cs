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
//using System.Xml;
//using System.Windows.Forms;
//using nl.Energie.EDSN;
//using System.Web.Services.Protocols;
//using System.Net.Sockets;
//using System.Threading;


//namespace Energie.SwitchBericht
//{


//    public class SwitchBericht
//    {

//        public int aansluitingID;
//        private string SQLstatement;
//        private SqlConnection conn;
//        private static String ConnString = "";
//        static string c_EOF = "'\r\n";
//        private StringBuilder switchBericht = new StringBuilder();
//        private string urlWebService = Energie.DataAccess.Configurations.GetApplicationSetting("PORTAAL");//"https://emp.edsn.nl/b2b";
//        //private string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
//        private string path = "";
//        private string certPV = "";
//        private string certPVPassword = "";
//        private string certLV = "";
//        private string certLVPassword = "";
//        private string HoofdPV = "";
//        private string HoofdLV = "";
//        public string strReceiver = "8712423010208";
//        public string strSender = "";
//        public string strPVSender = "";
//        private string DossierID = "";
//        public string EnrollmentID = "";
//        protected string Klant_Config = "";
//        protected string strError_Message = "";
//        private string strFTPServer = Energie.DataAccess.Configurations.GetApplicationSetting("FTPSERVER");
//        private string strFTPUser = "";
//        private string strFTPPassword = "";
//        private string strTest = Energie.DataAccess.Configurations.GetApplicationSetting("TEST");

//        private Int32 port = 13000;
//        private IPAddress localAddr = IPAddress.Parse("82.139.104.75");
//        private int intService = -1;
//        private TcpListener listener;
//        private Boolean blnContinue = true;
//        private System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();

//        public SwitchBericht(string klant_Config)
//        {
//            Klant_Config = klant_Config;



//            if (Klant_Config != "")
//            {
//                HoofdPV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV_"+Klant_Config).Trim();
//                HoofdLV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV_"+Klant_Config).Trim();
//                ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB_" + Klant_Config);
//                certPV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPV_" + Klant_Config);
//                certPVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPVPASSWORD_" + Klant_Config);
//                certLV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLV_" + Klant_Config);
//                certLVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLVPASSWORD_" + Klant_Config);
//                strSender = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV_" + Klant_Config).Trim();
//                strPVSender = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV_" + Klant_Config).Trim();
//                path = Energie.DataAccess.Configurations.GetApplicationSetting("XMLPATH_" + Klant_Config);//@"c:\test\";
//                strFTPUser = Energie.DataAccess.Configurations.GetApplicationSetting("FTPUSER_" + Klant_Config).Trim();
//                strFTPPassword = Energie.DataAccess.Configurations.GetApplicationSetting("FTPPASSWORD_" + Klant_Config).Trim();
//            }
//            else
//            {
//                HoofdPV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV").Trim();
//                HoofdLV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV").Trim();
//                ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB");
//                certPV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPV");
//                certPVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTPVPASSWORD");
//                certLV = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLV");
//                certLVPassword = Energie.DataAccess.Configurations.GetApplicationSetting("CERTLVPASSWORD");
//                strSender = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV").Trim();
//                strPVSender = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV").Trim();
//                path = Energie.DataAccess.Configurations.GetApplicationSetting("XMLPATH");//@"c:\test\";
//            }
//        }



//        public Boolean SwitchWebService(Boolean blnBatch, string strPV, out string pDossierID, out string strError, Boolean blnToFile)
//        {
//            //if (blnToFile == true)
//            //{
//            //    timer1.Tick += new EventHandler(timer1_Tick);
//            //    timer1.Interval = 100;vw_SwitchBericht
//            //    timer1.Enabled = true;
//            //}

//            Boolean blnOK = false;

//            SQLstatement = "Select Aansluiting_ID, Ean18_code, Bericht_Code " +
//                                    ", Aansluiting_Naam, Aansluiting_type, Factuurmodel_ID " +
//                                    ", Contract_Start_DT, Beginstand_Hoog, Beginstand_Laag " +
//                                    ", Verblijfsfunctie, leverancier_EAN13_code, netbeheerder_EAN13_code, account_Naam, account_KorteNaam " +
//                                    ", Transportkosten_Straat, Transportkosten_Huisnummer, Transportkosten_Toevoeging, Transportkosten_Postcode, Transportkosten_Plaats, Transportkosten_Land_Code " +
//                                    ", Transportkosten_Postbus, Transportkosten_Postbus_Plaats, Transportkosten_Postbus_Postcode " +
//                                    ", Meterstand_Opname_NL_DT,  Transportkosten_naam1, Transportkosten_naam2 , Transportkosten_naam3 " +
//                                    ", Aansluiting_Straat, Aansluiting_Huisnummer, Aansluiting_Toevoeging, Aansluiting_Postcode, Aansluiting_Plaats, Aansluiting_Land_Code, ProductType, EnrollmentID " +
//                                    ", KvKNr, SlimmeMeterAllocatie, GeboorteDatum " +
//                               "From vw_SwitchBerichten " +
//                               "where Aansluiting_id = @aanlsuiting_ID and Bericht_392_432_Outbox_ID = 0 ";//and ProductType = 'E' ";
//            //woz in query maar ik weet niet wat ermee te doen voor switchbericht.. (denk overbodig??)
//            SqlDataAdapter da = new SqlDataAdapter(SQLstatement, ConnString);
//            da.SelectCommand.Parameters.Add(new SqlParameter("@aanlsuiting_ID", SqlDbType.Int));
//            da.SelectCommand.Parameters["@aanlsuiting_ID"].Value = aansluitingID;
//            DataTable dtGet = new DataTable();
//            da.Fill(dtGet);
//            DataRow dr;
//            dr = dtGet.Rows[0];

//            //strError = "Na Query";

//            string strRequestFile = "";


//            switch (dr["Bericht_Code"].ToString())
//            {
//                case "392E03":
//                    if (blnBatch != true) { blnOK = ChangeOfSupplier(dr, blnToFile, strRequestFile); } else { blnOK = ChangeOfSupplierBatch(dtGet); }
//                    break;
//                case "392E01":
//                    if (blnBatch != true) { blnOK = MoveIn(dr, blnToFile, strRequestFile); } else { blnOK = MoveInBatch(dr); }
//                    break;
//                case "432E01":
//                    blnOK = MoveOut(dr);
//                    break;
//                case "432E20":
//                    if (blnBatch != true) { blnOK = EndOfSupply(dr, blnToFile, strRequestFile); } else { blnOK = EndOfSupplyBatch(dr); }
//                    break;
//                case "432E21":
//                    blnOK = ChangeOfPV(dr, strPV, blnToFile, strRequestFile);
//                    break;
//                case "NAMECH":
//                    blnOK = NameChange(dr);
//                    break;
//                case "EOSNOT":
//                    blnOK = EOSNotice(dr, blnToFile, strRequestFile);
//                    break;
//                case "COAMET":
//                    blnOK = ChangeOfAllocationMethod(dr, blnToFile, strRequestFile);
//                    break;
//            }

//            pDossierID = DossierID;
//            strError = strError_Message;
//            return blnOK;

//        }

//        public Boolean MoveIn(DataRow dr, Boolean blnToFile, string strRequestFile)
//        {
//            Boolean blnOK = false;
//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope enveloppe = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope();

//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);// "EDM00010";
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            string sender = "";
//            string identifier = "";
//            if (dr["ProductType"].ToString() == "G")
//            {
//                sender = "8714252022926";
//                identifier = "DDQ_M";
//            }
//            else
//            {
//                sender = strSender;
//                identifier = "DDQ_O";
//            }
//            source.SenderID = sender;
//            source.ContactTypeIdentifier = identifier;
//            header.Source = source;


//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC portaal_content = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC();
//            enveloppe.Portaal_Content = portaal_content;

//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP meteringpoint = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP();

//            meteringpoint.EANID = dr["Ean18_code"].ToString();
//            portaal_content.Portaal_MeteringPoint = meteringpoint;

//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_GridOperator_Company();
//            meteringpointGridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();
//            meteringpoint.GridOperator_Company = meteringpointGridOperator;

//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC meteringpointMPCommercial = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC();
//            meteringpoint.MPCommercialCharacteristics = meteringpointMPCommercial;
//            if (dr["KvKNr"].ToString() != "")
//            {
//                meteringpointMPCommercial.ChamberOfCommerceID = dr["KvKNr"].ToString();
//            }

//            if (dr["Verblijfsfunctie"].ToString() == "00")
//            {
//                meteringpointMPCommercial.Residential = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.N;
//                meteringpointMPCommercial.DeterminationComplex = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.N;
//            }
//            if (dr["Verblijfsfunctie"].ToString() == "01")
//            {
//                meteringpointMPCommercial.Residential = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.J;
//                meteringpointMPCommercial.DeterminationComplex = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.N;
//            }
//            if (dr["Verblijfsfunctie"].ToString() == "10")
//            {
//                meteringpointMPCommercial.Residential = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.N;
//                meteringpointMPCommercial.DeterminationComplex = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.J;
//            }
//            if (dr["Verblijfsfunctie"].ToString() == "11")
//            {
//                meteringpointMPCommercial.Residential = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.J;
//                meteringpointMPCommercial.DeterminationComplex = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.J;
//            }
//            if (dr["Verblijfsfunctie"].ToString() == "22")
//            {
//                meteringpointMPCommercial.Residential = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.NVT;
//                meteringpointMPCommercial.DeterminationComplex = nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_YesNoNaCode.NVT;
//            }

//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_SimpleAddressType contact = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_SimpleAddressType();
//            if (dr["Transportkosten_Postbus"].ToString().Trim() == "")
//            {
//                contact.StreetName = dr["Transportkosten_Straat"].ToString();
//                contact.BuildingNr = dr["Transportkosten_Huisnummer"].ToString();
//                if (dr["Transportkosten_Toevoeging"].ToString().Trim() != "")
//                {
//                    contact.ExBuildingNr = dr["Transportkosten_Toevoeging"].ToString();
//                }
//                contact.ZIPCode = dr["Transportkosten_Postcode"].ToString();
//                contact.CityName = dr["Transportkosten_Plaats"].ToString();
//                contact.Country = "NL";
//            }
//            else
//            {
//                contact.StreetName = "Postbus";
//                contact.BuildingNr = dr["Transportkosten_Postbus"].ToString();
//                //contact.ExBuildingNr = " ";
//                contact.ZIPCode = dr["Transportkosten_Postbus_Postcode"].ToString();
//                contact.CityName = dr["Transportkosten_Postbus_Plaats"].ToString();
//                contact.Country = "NL";
//            }

//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC_GridContractParty meteringpointGridContact = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC_GridContractParty();
//            if (dr["account_KorteNaam"].ToString().Length > 24) { meteringpointGridContact.Surname = dr["account_KorteNaam"].ToString().Substring(0, 24); }
//            else { meteringpointGridContact.Surname = dr["account_KorteNaam"].ToString(); }
//            if (!DBNull.Value.Equals(dr["GeboorteDatum"]))
//            {
//                meteringpointGridContact.BirthDateSpecified = true;
//                meteringpointGridContact.BirthDate = (DateTime)dr["GeboorteDatum"];
//            }
//            meteringpointGridContact.Contact = contact;
//            //meteringpointGridContact.Initials = " ";
//            meteringpointMPCommercial.GridContractParty = meteringpointGridContact;

//            if (dr["ProductType"].ToString() == "G")
//            {
//                sender = "8714252022926";
//            }
//            else
//            {
//                sender = dr["Leverancier_EAN13_Code"].ToString(); ;
//            }

//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company meteringpointBalanceSupplier = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company();
//            meteringpointBalanceSupplier.ID = sender;
//            meteringpointMPCommercial.BalanceSupplier_Company = meteringpointBalanceSupplier;

//            string PV = "";
//            if (dr["ProductType"].ToString() == "G")
//            {
//                PV = "8717185189995";
//            }
//            else
//            {
//                PV = strPVSender;
//            }
//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC_BalanceResponsibleParty_Company meteringpointBalanceResponsibleParty = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_MPCC_BalanceResponsibleParty_Company();
//            meteringpointBalanceResponsibleParty.ID = PV;// "8712423012615";
//            meteringpointMPCommercial.BalanceResponsibleParty_Company = meteringpointBalanceResponsibleParty;



//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_PM meteringpointPortaalMutation = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_PC_PMP_PM();
//            meteringpointPortaalMutation.MutationDate = (DateTime)dr["Contract_Start_DT"];
//            meteringpoint.Portaal_Mutation = meteringpointPortaalMutation;

//            nl.Energie.EDSN.MoveIn.MoveIn moveIn = new nl.Energie.EDSN.MoveIn.MoveIn();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            moveIn.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));



//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //moveIn.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderMoveInRespondingActivity";
//            moveIn.Url = KC.CarUrl + @"synchroon/ResponderMoveInRespondingActivity";

//            moveIn.Timeout = 120000;





//            nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope retour = new nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope();




//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope));
//            StringWriter swXML = new StringWriter();
//            serializer.Serialize(swXML, enveloppe);
//            //MessageBox.Show(swXML.ToString());
//            int intOutBoxID = Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "MoveIn", swXML.ToString());

//            TextWriter WriteFileStream = new StreamWriter(path + @"MoveIn.xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();

//            //TextWriter WriteFileStream = new StreamWriter(@"c:\test\Output\MoveIn.xml");
//            //serializer.Serialize(WriteFileStream, enveloppe);
//            //WriteFileStream.Close();


//            try
//            {
//                //7 nov move in
//                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                { BestandsAanvulling = " LV " + BestandsAanvulling; }

//                if (blnToFile == true && Klant_Config != "")
//                {
//                    string fileName = path + @"MoveIn.xml";

//                    XmlSerializer serializer1 = new XmlSerializer(typeof(nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope));
//                    if (File.Exists(fileName)) { File.Delete(fileName); }
//                    TextWriter WriteFileStream1 = new StreamWriter(fileName);
//                    serializer1.Serialize(WriteFileStream1, enveloppe);
//                    WriteFileStream1.Close();

//                    IPAddress ipAddr = localAddr;
//                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

//                    Socket client = new Socket(AddressFamily.InterNetwork,
//                            SocketType.Stream, ProtocolType.Tcp);

//                    client.Connect(ipEndPoint);

//                    if (File.Exists(path + "RemoteResult.xml") == true) { File.Delete(path + "RemoteResult.xml"); }

//                    client.SendFile(fileName);

//                    client.Shutdown(SocketShutdown.Both);
//                    client.Close();

//                    SqlConnection conn = new SqlConnection(ConnString);
//                    conn.Open();

//                    int intCounter = 0;
//                    int intRecords = 0;
//                    while (intRecords == 0 && intCounter < 200)
//                    {
//                        string strSQL = "select count(*) from Messages.dbo.XMLMessage";
//                        SqlCommand cmd = new SqlCommand(strSQL, conn);
//                        intRecords = (int)cmd.ExecuteScalar();
//                        Application.DoEvents();
//                        Thread.Sleep(100);
//                        intCounter++;
//                    }

//                    if (intRecords > 0)
//                    {
//                        string strSQL = "select * from Messages.dbo.XMLMessage";
//                        SqlCommand cmd = new SqlCommand(strSQL, conn);
//                        string strXML = cmd.ExecuteScalar().ToString();

//                        serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope));
//                        XmlDocument _Doc = new XmlDocument();
//                        File.WriteAllText(path + "RemoteResult.xml", strXML);

//                        _Doc.LoadXml(strXML);
//                        retour = (nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
//                    }
//                    conn.Close();
//                }
//                else
//                {
//                    retour = moveIn.MoveInRequestEnvelope(enveloppe);
//                }
//                nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;

//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope));
//                WriteFileStream = new StreamWriter(path + @"MoveInResponse" + BestandsAanvulling + ".xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();


//                string ftpResponse = "";
//                if (Klant_Config == "ROBIN" && strTest != "JA")
//                {
//                    if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "MoveInResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + @"MoveInResponse" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                    {
//                        WriteLog("Fout bij verzenden naar nexant " + dr["Ean18_code"].ToString() + " - " + ftpResponse, 10, intOutBoxID);

//                        //MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse);
//                    }
//                }

//                // 

//                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection))
//                {
//                    nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
//                    nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
//                    //MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
//                    WriteLog("Fout MoveIn " + dr["Ean18_code"].ToString() + " - " + rejectionPortaalType[0].RejectionText.ToString(), 10, intOutBoxID);
//                    if (Klant_Config != "")
//                    {
//                        strError_Message = "Fout MoveIn " + dr["Ean18_code"].ToString() + " - " + rejectionPortaalType[0].RejectionText.ToString();
//                    }
//                    else
//                    {
//                        WriteEnrollmentLog(rejectionPortaalType[0].RejectionText.ToString(), dr["ProductType"].ToString(),
//                            "MoveIn", "", EnrollmentID, dr["Ean18_code"].ToString());
//                    }
//                }
//                else
//                {
//                    blnOK = true;
//                    nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem = (nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;



//                    //414
//                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
//                    swXML = new StringWriter();
//                    serializer.Serialize(swXML, responseItem);

//                    DossierID = responseItem.Portaal_Mutation.Dossier.ID;

//                    int inboxID_414 = Save_Inbox(12, swXML.ToString(), "MoveIn");
//                    int edineID = Save_Edine(inboxID_414, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "414");
//                    Save_414_Header(edineID, DateTime.Now, strReceiver, strSender);
//                    Save_414(edineID, header.DocumentID, dr["Ean18_code"].ToString(), (DateTime)dr["Contract_Start_DT"], "E01",
//                            meteringpointBalanceResponsibleParty.ID, dr["Transportkosten_naam1"].ToString(), dr["Aansluiting_Naam"].ToString(),
//                            dr["Transportkosten_Straat"].ToString(), dr["Transportkosten_Plaats"].ToString(), dr["Transportkosten_Postcode"].ToString(),
//                            "NL", "E", "E01", "E01", responseItem.Portaal_Mutation.Dossier.ID, dr["EnrollmentID"].ToString());

//                    ProcessMessage.processMessage(inboxID_414, ConnString);
//                    //E09

//                    int inboxID_E09 = Save_Inbox(12, swXML.ToString(), "MoveIn");
//                    edineID = Save_Edine(inboxID_E09, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "E09");
//                    Save_E09_Header(edineID, DateTime.Now, strReceiver, strSender);
//                    Save_E09(edineID, header.DocumentID, dr["Ean18_code"].ToString(), dr["Netbeheerder_EAN13_Code"].ToString(), (DateTime)dr["Contract_Start_DT"], "7", "E01", DossierID);
//                    ProcessMessage.processMessage(inboxID_E09, ConnString);

//                    Save_Switch((int)dr["Aansluiting_ID"], intOutBoxID, inboxID_414, inboxID_E09);
//                    //MessageBox.Show("Accepted - Dossier " + responseItem.Portaal_Mutation.Dossier.ID);

//                    WriteMasterdataRequest(dr["Ean18_code"].ToString(), dr["Netbeheerder_EAN13_Code"].ToString(), (DateTime)dr["Contract_Start_DT"]);
//                    blnOK = true;
//                }
//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                WriteLog("Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID);
//                if (Klant_Config != "")
//                {
//                    strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
//                }
//                else
//                {
//                    WriteEnrollmentLog(S.ErrorCode.ToString() + " - " + S.ErrorDetails + " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), dr["ProductType"].ToString(),
//                    "Movein", "", EnrollmentID, dr["Ean18_code"].ToString());
//                }
//                //MessageBox.Show(S.ErrorCode.ToString());
//                //MessageBox.Show(S.ErrorDetails);
//                //MessageBox.Show(S.ErrorText);
//                //MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }
//            catch (WebException exception)
//            {
//                WriteLog("Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }
//            catch (Exception exception)
//            {
//                WriteLog("Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }

//            return blnOK;
//        }

//        public Boolean MoveInFromXML(string strRequestFile)
//        {
//            Boolean blnOK = false;
//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope enveloppe = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope();

//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader();


//            nl.Energie.EDSN.MoveIn.MoveIn moveIn = new nl.Energie.EDSN.MoveIn.MoveIn();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            moveIn.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));



//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //moveIn.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderMoveInRespondingActivity";
//            moveIn.Url = KC.CarUrl + @"synchroon/ResponderMoveInRespondingActivity";

//            moveIn.Timeout = 120000;


//            nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope retour = new nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope();

//            try
//            {
//                XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope));
//                XmlDocument _Doc = new XmlDocument();
//                _Doc.Load(strRequestFile);
//                enveloppe = (nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

//                retour = moveIn.MoveInRequestEnvelope(enveloppe);

//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope));
//                StreamWriter WriteFileStream = new StreamWriter(path + @"result.xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();

//                blnOK = true;

//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {


//            }
//            catch (WebException exception)
//            {

//            }
//            catch (Exception exception)
//            {

//            }

//            return blnOK;
//        }

//        public Boolean MoveInFromFile(string strFileName)//DataRow dr
//        {
//            String OldFileName = strFileName;
//            string text = File.ReadAllText(strFileName);
//            text = text.Replace("<MoveInResponseEnvelope>", @"<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "utf-8" + '"' + "?><MoveInResponseEnvelope>");
//            text = text.Replace("</EDSNDocument>", "");
//            text = text.Replace("<MoveInResponseEnvelope" + '"', "<MoveInResponseEnvelope xmlns:xsi=" + '"' + "http://www.w3.org/2001/XMLSchema-instance" + '"' + " xmlns:xsd=" + '"' + "http://www.w3.org/2001/XMLSchema" + '"');
//            text = text.Replace("<EDSNBusinessDocumentHeader>", "<EDSNBusinessDocumentHeader xmlns=" + '"' + "urn:nedu:edsn:data:moveinresponse:1:standard" + '"' + ">");
//            text = text.Replace("<Portaal_Content>", "<Portaal_Content xmlns=" + '"' + "urn:nedu:edsn:data:moveinresponse:1:standard" + '"' + ">");
//            strFileName = strFileName.Trim() + ".tmp";
//            File.WriteAllText(strFileName, text);

//            Boolean blnOK = false;
//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope enveloppe = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope();

//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);// "EDM00010";
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MoveIn.MoveInRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            source.SenderID = strSender;
//            source.ContactTypeIdentifier = "DDQ_O";
//            header.Source = source;



//            nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope retour = new nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope();

//            try
//            {
//                XmlDocument _Doc = new XmlDocument();
//                _Doc.Load(strFileName);

//                XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope));
//                retour = (nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

//                //retour = moveIn.CallMoveIn(enveloppe);
//                nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;
//                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection))
//                {
//                    nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
//                    nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
//                    //MessageBox.Show();
//                    if (Klant_Config != "")
//                    {
//                        strError_Message = rejectionPortaalType[0].RejectionText.ToString();
//                    }
//                    else
//                    {
//                        MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
//                    }

//                }
//                else
//                {
//                    nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem = (nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;


//                    //414
//                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveIn.MoveInResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
//                    StringWriter swXML = new StringWriter();
//                    serializer.Serialize(swXML, responseItem);
//                    int inboxID_414 = Save_Inbox(12, swXML.ToString(), "MoveIn");
//                    int edineID = Save_Edine(inboxID_414, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "414");

//                    string strEAN18 = responseItem.EANID.ToString();
//                    DateTime dtmSwitchDatum = responseItem.Portaal_Mutation.MutationDate;

//                    string Naam = "";
//                    conn = new SqlConnection(ConnString);
//                    conn.Open();

//                    string str_SQL = "SELECT [Naam] FROM [Messages].[dbo].[PVSwitches] where EAN18_code=@Ean18_Code";
//                    SqlCommand cmd = new SqlCommand(str_SQL, conn);
//                    cmd = conn.CreateCommand();
//                    cmd.CommandText = str_SQL;
//                    cmd.Parameters.AddWithValue("@Ean18_Code", strEAN18);
//                    SqlDataReader srdr = cmd.ExecuteReader();
//                    if (srdr.HasRows)
//                    {
//                        while (srdr.Read())
//                        {
//                            try
//                            {
//                                Naam = srdr.GetString(0);
//                            }
//                            catch
//                            {
//                                Naam = "";
//                            }

//                        }
//                    }
//                    srdr.Close();

//                    Save_414_Header(edineID, DateTime.Now, strReceiver, strSender);
//                    Save_414(edineID, header.DocumentID, strEAN18, dtmSwitchDatum, "E01",
//                            responseItem.MPCommercialCharacteristics.BalanceResponsibleParty_Company.ID, Naam, Naam,
//                            "", "", "",
//                            "NL", "E", "E01", "E01", responseItem.Portaal_Mutation.Dossier.ID, "");

//                    ProcessMessage.processMessage(inboxID_414, ConnString);
//                    //E09

//                    int inboxID_E09 = Save_Inbox(12, swXML.ToString(), "MoveIn");
//                    edineID = Save_Edine(inboxID_E09, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "E09");
//                    Save_E09_Header(edineID, DateTime.Now, strReceiver, strSender);
//                    Save_E09(edineID, header.DocumentID, strEAN18, responseItem.GridOperator_Company.ID, dtmSwitchDatum, "7", "E01", responseItem.Portaal_Mutation.Dossier.ID);
//                    ProcessMessage.processMessage(inboxID_E09, ConnString);

//                    //Save_Switch((int)dr["Aansluiting_ID"], intOutBoxID, inboxID_414, inboxID_E09);
//                    //MessageBox.Show("Accepted");
//                    blnOK = true;
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
//                //WriteLog("Fout bij MoveIn : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                MessageBox.Show(exception.Message);
//            }
//            catch (Exception exception)
//            {
//                MessageBox.Show(exception.Message);
//            }

//            if (strFileName != "")
//            {
//                File.Delete(strFileName);
//                strFileName = OldFileName;
//            }

//            return blnOK;
//        }

//        public Boolean ChangeOfSupplier(DataRow dr, Boolean blnToFile, string strRequestFile)
//        {
//            Boolean blnOK = false;
//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope enveloppe = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope();

//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader();



//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);// "EDM00010";
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            string sender = "";
//            string identifier = "";
//            if (dr["ProductType"].ToString() == "G")
//            {
//                sender = "8714252022926";
//                identifier = "DDQ_M";
//            }
//            else
//            {
//                sender = strSender;
//                identifier = "DDQ_O";
//            }


//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            source.SenderID = sender;
//            source.ContactTypeIdentifier = identifier;
//            header.Source = source;


//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC portaal_content = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC();
//            enveloppe.Portaal_Content = portaal_content;

//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP meteringpoint = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP();

//            meteringpoint.EANID = dr["Ean18_code"].ToString();
//            portaal_content.Portaal_MeteringPoint = meteringpoint;

//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_GridOperator_Company();
//            meteringpointGridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();
//            meteringpoint.GridOperator_Company = meteringpointGridOperator;

//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC meteringpointMPCommercial = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC();
//            meteringpoint.MPCommercialCharacteristics = meteringpointMPCommercial;
//            if (dr["KvKNr"].ToString() != "")
//            {
//                meteringpointMPCommercial.ChamberOfCommerceID = dr["KvKNr"].ToString();
//            }

//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC_GridContractParty meteringpointGridContact = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC_GridContractParty();
//            meteringpointGridContact.Surname = dr["Aansluiting_Naam"].ToString();
//            if (!DBNull.Value.Equals(dr["GeboorteDatum"]))
//            {
//                meteringpointGridContact.BirthDateSpecified = true;
//                meteringpointGridContact.BirthDate = (DateTime)dr["GeboorteDatum"];
//            }
//                //meteringpointGridContact.Initials = " ";
//                meteringpointMPCommercial.GridContractParty = meteringpointGridContact;

//            if (dr["ProductType"].ToString() == "G")
//            {
//                sender = "8714252022926";
//            }
//            else
//            {
//                sender = dr["Leverancier_EAN13_Code"].ToString(); ;
//            }

//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company meteringpointBalanceSupplier = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company();
//            meteringpointBalanceSupplier.ID = sender;
//            meteringpointMPCommercial.BalanceSupplier_Company = meteringpointBalanceSupplier;

//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC_BalanceResponsibleParty_Company meteringpointBalanceResponsibleParty = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_MPCC_BalanceResponsibleParty_Company();
//            if (dr["ProductType"].ToString() != "G") { meteringpointBalanceResponsibleParty.ID = HoofdPV; } else { meteringpointBalanceResponsibleParty.ID = "8717185189995"; };//"7614252022906"
//            meteringpointMPCommercial.BalanceResponsibleParty_Company = meteringpointBalanceResponsibleParty;

//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_PM meteringpointPortaalMutation = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_PC_PMP_PM();
//            meteringpointPortaalMutation.MutationDate = (DateTime)dr["Contract_Start_DT"];
//            meteringpoint.Portaal_Mutation = meteringpointPortaalMutation;


//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplier changeOfSupplier = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplier();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            changeOfSupplier.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));


//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //moveIn.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderMoveInRespondingActivity";
//            changeOfSupplier.Url = KC.CarUrl + @"synchroon/ResponderChangeOfSupplierRespondingActivity";

//            changeOfSupplier.Timeout = 120000;

//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope retour = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope();


//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope));
//            StringWriter swXML = new StringWriter();
//            serializer.Serialize(swXML, enveloppe);
//            //MessageBox.Show(swXML.ToString());
//            int intOutBoxID = Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "MoveIn", swXML.ToString());


//            // move in ook wegschrijven
//            TextWriter WriteFileStream = new StreamWriter(path + @"ChangeofSupplier.xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();



//            try
//            {
//                // de xml wegschrijven (jitka 7 november)

//                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                { BestandsAanvulling = " LV " + BestandsAanvulling; }

//                if (blnToFile == true && Klant_Config != "")
//                {
//                    string fileName = path + @"ChangeOfSupplier.xml";

//                    // string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                    XmlSerializer serializer1 = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope));
//                    if (File.Exists(fileName)) { File.Delete(fileName); }
//                    TextWriter WriteFileStream1 = new StreamWriter(fileName);
//                    serializer1.Serialize(WriteFileStream1, enveloppe);
//                    WriteFileStream1.Close();

//                    IPAddress ipAddr = localAddr;
//                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

//                    // Create a TCP socket.
//                    Socket client = new Socket(AddressFamily.InterNetwork,
//                            SocketType.Stream, ProtocolType.Tcp);

//                    // Connect the socket to the remote endpoint.
//                    client.Connect(ipEndPoint);

//                    if (File.Exists(path + "RemoteResult.xml") == true) { File.Delete(path + "RemoteResult.xml"); }

//                    client.SendFile(fileName);


//                    // Release the socket.
//                    client.Shutdown(SocketShutdown.Both);
//                    client.Close();

//                    SqlConnection conn = new SqlConnection(ConnString);
//                    conn.Open();

//                    int intCounter = 0;
//                    int intRecords = 0;
//                    //WriteLog("Lus", 10, 0);
//                    while (intRecords == 0 && intCounter < 200)
//                    {
//                        string strSQL = "select count(*) from Messages.dbo.XMLMessage";
//                        SqlCommand cmd = new SqlCommand(strSQL, conn);
//                        intRecords = (int)cmd.ExecuteScalar();
//                        Application.DoEvents();
//                        Thread.Sleep(100);
//                        intCounter++;
//                    }

//                    //WriteLog("EindeLus " + intRecords.ToString(), 10, 0);

//                    if (intRecords > 0)
//                    {
//                        string strSQL = "select * from Messages.dbo.XMLMessage";
//                        SqlCommand cmd = new SqlCommand(strSQL, conn);
//                        string strXML = cmd.ExecuteScalar().ToString();
//                        //WriteLog("Eindelees " + strXML.Length.ToString(), 10, 0);


//                        serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope));
//                        XmlDocument _Doc = new XmlDocument();
//                        File.WriteAllText(path + "RemoteResult.xml", strXML);

//                        _Doc.LoadXml(strXML);
//                        //_Doc.Load(path + "RemoteResult.xml");
//                        //nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope result = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope();
//                        //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
//                        retour = (nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
//                    }
//                    conn.Close();
//                }
//                else
//                {
//                    retour = changeOfSupplier.CallChangeOfSupplier(enveloppe);
//                }
//                nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;

//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope));
//                //Save to file kan weg
//                WriteFileStream = new StreamWriter(path + @"ChangeOfSupplierResponse" + BestandsAanvulling + ".xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();


//                string ftpResponse = "";
//                if (Klant_Config == "ROBIN" && strTest != "JA")
//                {
//                    if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "ChangeOfSupplierResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + @"ChangeOfSupplierResponse" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                    {
//                        //MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse);
//                        WriteLog("Fout bij verzenden naar nexant ChangeOfSupplier - " + dr["Ean18_code"].ToString() + "-" + ftpResponse, 10, intOutBoxID);

//                    }
//                }

//                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_Portaal_Content_Portaal_Rejection))
//                {
//                    nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
//                    nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
//                    //MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
//                    WriteLog("Fout bij  ChangeOfSupplier - " + dr["Ean18_code"].ToString() + "-" + rejectionPortaalType[0].RejectionText.ToString(), 10, intOutBoxID);
//                    if (Klant_Config != "")
//                    {
//                        strError_Message = "Fout bij  ChangeOfSupplier - " + dr["Ean18_code"].ToString() + "-" + rejectionPortaalType[0].RejectionText.ToString();
//                    }
//                    else
//                    {
//                        WriteEnrollmentLog(rejectionPortaalType[0].RejectionText.ToString(), dr["ProductType"].ToString(),
//                            "ChangeOfSupplier", "", EnrollmentID, dr["Ean18_code"].ToString());
//                    }
//                }
//                else
//                {
//                    blnOK = true;
//                    nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem = (nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;


//                    //414
//                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
//                    swXML = new StringWriter();
//                    serializer.Serialize(swXML, responseItem);

//                    DossierID = responseItem.Portaal_Mutation.Dossier.ID;

//                    int inboxID_414 = Save_Inbox(12, swXML.ToString(), "ChangeOfSupplier");
//                    int edineID = Save_Edine(inboxID_414, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "414");
//                    Save_414_Header(edineID, DateTime.Now, strReceiver, strSender);
//                    Save_414(edineID, header.DocumentID, dr["Ean18_code"].ToString(), (DateTime)dr["Contract_Start_DT"], "E01",
//                            strPVSender, dr["Transportkosten_naam1"].ToString(), dr["Aansluiting_Naam"].ToString(),
//                            dr["Transportkosten_Straat"].ToString(), dr["Transportkosten_Plaats"].ToString(), dr["Transportkosten_Postcode"].ToString(),
//                            "NL", "E", "E01", "E01", responseItem.Portaal_Mutation.Dossier.ID, dr["EnrollmentID"].ToString());

//                    ProcessMessage.processMessage(inboxID_414, ConnString);
//                    //E09 Alleen LV

//                    int inboxID_E09 = Save_Inbox(15, swXML.ToString(), "ChangeOfSupplier");
//                    edineID = Save_Edine(inboxID_E09, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "E09");
//                    Save_E09_Header(edineID, DateTime.Now, strReceiver, strSender);
//                    Save_E09(edineID, header.DocumentID, dr["Ean18_code"].ToString(), dr["Netbeheerder_EAN13_Code"].ToString(), (DateTime)dr["Contract_Start_DT"], "7", "E01", responseItem.Portaal_Mutation.Dossier.ID);
//                    ProcessMessage.processMessage(inboxID_E09, ConnString);

//                    Save_Switch((int)dr["Aansluiting_ID"], intOutBoxID, inboxID_414, inboxID_E09);

//                    WriteMasterdataRequest(dr["Ean18_code"].ToString(), dr["Netbeheerder_EAN13_Code"].ToString(), (DateTime)dr["Contract_Start_DT"]);

//                    //MessageBox.Show("Accepted - Dossier " + responseItem.Portaal_Mutation.Dossier.ID);
//                    blnOK = true;
//                }
//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                WriteLog("Fout bij ChangeOfSupplier : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID);
//                if (Klant_Config != "")
//                {
//                    strError_Message = "Fout bij ChangeOfSupplier : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
//                }
//                else
//                {
//                    WriteEnrollmentLog(S.ErrorCode.ToString() + " - " + S.ErrorDetails + " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), dr["ProductType"].ToString(),
//                    "ChangeOfSupplier", "", EnrollmentID, dr["Ean18_code"].ToString());
//                }
//                //MessageBox.Show(S.ErrorCode.ToString());
//                //MessageBox.Show(S.ErrorDetails);
//                //MessageBox.Show(S.ErrorText);
//                //MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }
//            catch (WebException exception)
//            {
//                WriteLog("Fout bij ChangeOfSupplier : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij ChangeOfSupplier : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }
//            catch (Exception exception)
//            {
//                WriteLog("Fout bij ChangeOfSupplier : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij ChangeOfSupplier : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }

//            return blnOK;
//        }


//        public Boolean ChangeOfSupplierFromXML(string strRequestFile)
//        {
//            Boolean blnOK = false;
//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope enveloppe = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope();

//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope_EDSNBusinessDocumentHeader();



//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplier changeOfSupplier = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplier();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            changeOfSupplier.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));


//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //moveIn.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderMoveInRespondingActivity";
//            changeOfSupplier.Url = KC.CarUrl + @"synchroon/ResponderChangeOfSupplierRespondingActivity";

//            changeOfSupplier.Timeout = 120000;

//            nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope retour = new nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope();



//            try
//            {
//                XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope));
//                XmlDocument _Doc = new XmlDocument();
//                _Doc.Load(strRequestFile);
//                enveloppe = (nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierRequestEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

//                retour = changeOfSupplier.CallChangeOfSupplier(enveloppe);

//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfSupplier.ChangeOfSupplierResponseEnvelope));
//                StreamWriter WriteFileStream = new StreamWriter(path + @"result.xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();
//                blnOK = true;

//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//            }
//            catch (WebException exception)
//            {

//            }
//            catch (Exception exception)
//            {

//            }

//            return blnOK;
//        }

//        public Boolean ChangeOfSupplierBatch(DataTable dtGet)
//        {
//            string eanForError = "";
//            Boolean blnOK = false;
//            nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope enveloppe = new nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope();

//            nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);// "EDM00010";
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            source.SenderID = strSender;
//            source.ContactTypeIdentifier = "DDQ_O";
//            header.Source = source;

//            int intRecordnr = 0;
//            nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_PC_PMP[] meteringpoint = new nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_PC_PMP[dtGet.Rows.Count - 1];
//            enveloppe.Portaal_Content = meteringpoint;

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            foreach (DataRow dr in dtGet.Rows)
//            {


//                meteringpoint[intRecordnr].EANID = dr["Ean18_code"].ToString();
//                eanForError = dr["Ean18_code"].ToString();
//                //portaal_content.Portaal_MeteringPoint = meteringpoint;

//                nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_PC_PMP_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_PC_PMP_GridOperator_Company();
//                meteringpointGridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();
//                meteringpoint[intRecordnr].GridOperator_Company = meteringpointGridOperator;

//                nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_PC_PMP_MPCC meteringpointMPCommercial = new nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_PC_PMP_MPCC();
//                meteringpoint[intRecordnr].MPCommercialCharacteristics = meteringpointMPCommercial;
//                if (dr["KvKNr"].ToString() != "")
//                {
//                    meteringpointMPCommercial.ChamberOfCommerceID = dr["KvKNr"].ToString();
//                }

//                nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_PC_PMP_MPCC_GridContractParty meteringpointGridContact = new nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_PC_PMP_MPCC_GridContractParty();
//                meteringpointGridContact.Surname = dr["Aansluiting_Naam"].ToString();
//                if (!DBNull.Value.Equals(dr["GeboorteDatum"]))
//                {
//                    meteringpointGridContact.BirthDateSpecified = true;
//                    meteringpointGridContact.BirthDate = (DateTime)dr["GeboorteDatum"];
//                }
//                //meteringpointGridContact.Initials = " ";
//                meteringpointMPCommercial.GridContractParty = meteringpointGridContact;

//                nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company meteringpointBalanceSupplier = new nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company();
//                meteringpointBalanceSupplier.ID = dr["Leverancier_EAN13_Code"].ToString();
//                meteringpointMPCommercial.BalanceSupplier_Company = meteringpointBalanceSupplier;

//                nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_PC_PMP_MPCC_BalanceResponsibleParty_Company meteringpointBalanceResponsibleParty = new nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_PC_PMP_MPCC_BalanceResponsibleParty_Company();
//                meteringpointBalanceResponsibleParty.ID = HoofdPV;
//                meteringpointMPCommercial.BalanceResponsibleParty_Company = meteringpointBalanceResponsibleParty;

//                nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_PC_PMP_PM meteringpointPortaalMutation = new nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope_PC_PMP_PM();
//                meteringpointPortaalMutation.MutationDate = (DateTime)dr["Contract_Start_DT"];
//                meteringpoint[intRecordnr].Portaal_Mutation = meteringpointPortaalMutation;

//                intRecordnr = intRecordnr + 1;
//            }

//            nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatch changeOfSupplier = new nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatch();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            changeOfSupplier.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));


//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //moveIn.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderMoveInRespondingActivity";
//            changeOfSupplier.Url = KC.CarUrl + @"synchroon/ResponderChangeOfSupplierRespondingActivity";

//            changeOfSupplier.Timeout = 120000;

//            nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchResponseEnvelope retour = new nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchResponseEnvelope();


//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchRequestEnvelope));
//            StringWriter swXML = new StringWriter();
//            serializer.Serialize(swXML, enveloppe);
//            //MessageBox.Show(swXML.ToString());
//            int intOutBoxID = Save_Outbox(header.DocumentID,"C.O.S. Batch", "ChangeOfSupplier", swXML.ToString());

//            //TextWriter WriteFileStream = new StreamWriter(@"c:\test\Output\MoveIn.xml");
//            //serializer.Serialize(WriteFileStream, enveloppe);
//            //WriteFileStream.Close();

//            try
//            {
//                retour = changeOfSupplier.CallChangeOfSupplierBatch(enveloppe);
//                nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;
//                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchResponseEnvelope_Portaal_Content_EDSN_SimpleRejection))
//                {
//                    nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchResponseEnvelope_Portaal_Content_EDSN_SimpleRejection itemRejection = (nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchResponseEnvelope_Portaal_Content_EDSN_SimpleRejection)portaalResponse.Item;
//                    //nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
//                    //MessageBox.Show(itemRejection.SimpleRejection.RejectionText);
//                    WriteLog("Fout bij  ChangeOfSupplierBatch - " + eanForError + "-" + itemRejection.SimpleRejection.RejectionText, 10, intOutBoxID);
//                    strError_Message = "Fout bij  ChangeOfSupplierBatch - " + eanForError + "-" + itemRejection.SimpleRejection.RejectionText;
//                }
//                else
//                {
//                    //nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchResponseEnvelope_Portaal_Content responseItem = (nl.Energie.EDSN.ChangeOfSupplierBatch.ChangeOfSupplierBatchResponseEnvelope_Portaal_Content)portaalResponse.Item;




//                    //Save_Switch((int)dr["Aansluiting_ID"], intOutBoxID, inboxID_414, inboxID_E09);
//                    //MessageBox.Show("Accepted");
//                    blnOK = true;
//                }
//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                //MessageBox.Show(S.ErrorCode.ToString());
//                //MessageBox.Show(S.ErrorDetails);
//                //MessageBox.Show(S.ErrorText);
//                //MessageBox.Show(ex.Detail.InnerXml.ToString());
//                WriteLog("Fout bij  ChangeOfSupplierBatch - " + eanForError + "-" + ex.Detail.InnerXml.ToString(), 10, intOutBoxID);
//                strError_Message = "Fout bij  ChangeOfSupplierBatch - " + eanForError + "-" + ex.Detail.InnerXml.ToString();
//            }
//            catch (WebException exception)
//            {
//                //MessageBox.Show(exception.Message);
//                WriteLog("Fout bij  ChangeOfSupplierBatch - " + eanForError + "-" + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij  ChangeOfSupplierBatch - " + eanForError + "-" + exception.Message;
//            }
//            catch (Exception exception)
//            {
//                MessageBox.Show(exception.Message);
//                WriteLog("Fout bij  ChangeOfSupplierBatch - " + eanForError + "-" + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij  ChangeOfSupplierBatch - " + eanForError + "-" + exception.Message;
//            }
//            return blnOK;
//        }



//        public Boolean MoveOut(DataRow dr)
//        {
//            Boolean blnOK = false;
//            string eanForError = "";
//            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope enveloppe = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            string sender = "";
//            string identifier = "";
//            if (dr["ProductType"].ToString() == "G")
//            {
//                sender = "8714252022926";
//                identifier = "DDQ_M";
//            }
//            else
//            {
//                sender = strSender;
//                identifier = "DDQ_O";
//            }

//            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            source.SenderID = sender;
//            source.ContactTypeIdentifier = identifier;
//            header.Source = source;


//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content();
//            enveloppe.Portaal_Content = portaal_content;

//            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint meteringpoint = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint();

//            meteringpoint.EANID = dr["Ean18_code"].ToString();
//            eanForError = dr["Ean18_code"].ToString();
//            portaal_content.Portaal_MeteringPoint = meteringpoint;

//            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company();
//            meteringpointGridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();
//            meteringpoint.GridOperator_Company = meteringpointGridOperator;

//            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics meteringpointMPCommercial = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics();
//            meteringpoint.MPCommercialCharacteristics = meteringpointMPCommercial;

//            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company meteringpointBalanceSupplier = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company();
//            meteringpointBalanceSupplier.ID = sender;
//            meteringpointMPCommercial.BalanceSupplier_Company = meteringpointBalanceSupplier;

//            nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation meteringpointPortaalMutation = new nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation();
//            meteringpointPortaalMutation.MutationDate = (DateTime)dr["Contract_Start_DT"];
//            meteringpoint.Portaal_Mutation = meteringpointPortaalMutation;

//            nl.Energie.EDSN.MoveOut.MoveOut MoveOut = new nl.Energie.EDSN.MoveOut.MoveOut();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            MoveOut.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));

//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //MoveOut.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderMoveOutRespondingActivity";
//            MoveOut.Url = KC.CarUrl + @"synchroon/ResponderMoveOutRespondingActivity";

//            MoveOut.Timeout = 120000;

//            nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope retour = new nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope();


//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveOut.MoveOutRequestEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + @"MoveOut.xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();

//            StringWriter swXML = new StringWriter();
//            serializer.Serialize(swXML, enveloppe);
//            //MessageBox.Show(swXML.ToString());
//            int intOutBoxID = Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "MoveOut", swXML.ToString());

//            try
//            {
//                // de xml wegschrijven (jitka 7 november)

//                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                { BestandsAanvulling = " LV " + BestandsAanvulling; }
//                // 
//                retour = MoveOut.CallMoveOut(enveloppe);
//                nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;

//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope));

//                WriteFileStream = new StreamWriter(path + @"MoveOutResponse" + BestandsAanvulling + ".xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();

//                string ftpResponse = "";
//                if (Klant_Config == "ROBIN" && strTest != "JA")
//                {
//                    if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "MoveOutResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + @"MoveOutResponse" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                    {
//                        //MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse);
//                        WriteLog("Fout bij verzenden naar nexant MoveOut - " + eanForError + "-" + ftpResponse, 10, intOutBoxID);
//                    }
//                }

//                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_Portaal_Content_Portaal_Rejection))
//                {
//                    nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
//                    nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
//                    //MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
//                    WriteLog("Fout bij  MoveOut - " + eanForError + "-" + rejectionPortaalType[0].RejectionText.ToString(), 10, intOutBoxID);
//                    if (Klant_Config != "")
//                    {
//                        strError_Message = "Fout bij  MoveOut - " + eanForError + "-" + rejectionPortaalType[0].RejectionText.ToString();
//                    }
//                    else
//                    {
//                        WriteEnrollmentLog(rejectionPortaalType[0].RejectionText.ToString(), dr["ProductType"].ToString(),
//                            "MoveOut", "", EnrollmentID, dr["Ean18_code"].ToString());
//                    }
//                }
//                else
//                {
//                    blnOK = true;
//                    nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem = (nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;

//                    //406
//                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveOut.MoveOutResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
//                    swXML = new StringWriter();
//                    serializer.Serialize(swXML, responseItem);

//                    DossierID = responseItem.Portaal_Mutation.Dossier.ID;

//                    int inboxID_406 = Save_Inbox(13, swXML.ToString(), "MoveOut");
//                    int edineID = Save_Edine(inboxID_406, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "406");
//                    Save_406_Header(edineID, DateTime.Now, strReceiver, strSender);
//                    Save_406(edineID, header.DocumentID, dr["Ean18_code"].ToString(), (DateTime)dr["Contract_Start_DT"],
//                        responseItem.Portaal_Mutation.Dossier.ID, dr["EnrollmentID"].ToString(), "");

//                    ProcessMessage.processMessage(inboxID_406, ConnString);

//                    //E09
//                    int inboxID_E09 = Save_Inbox(12, swXML.ToString(), "MoveOut");
//                    edineID = Save_Edine(inboxID_E09, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "E09");
//                    Save_E09_Header(edineID, DateTime.Now, strReceiver, strSender);
//                    Save_E09_End(edineID, header.DocumentID, dr["Ean18_code"].ToString(), dr["Netbeheerder_EAN13_Code"].ToString(), (DateTime)dr["Contract_Start_DT"], "7", "E01");
//                    ProcessMessage.processMessage(inboxID_E09, ConnString);

//                    Save_Switch_End((int)dr["Aansluiting_ID"], intOutBoxID, inboxID_406, inboxID_E09);

//                    //MessageBox.Show("Accepted - Dossier " + responseItem.Portaal_Mutation.Dossier.ID);

//                    WriteMasterdataRequest(dr["Ean18_code"].ToString(), dr["Netbeheerder_EAN13_Code"].ToString(), (DateTime)dr["Contract_Start_DT"]);

//                    blnOK = true;
//                }
//            }

//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                WriteLog("Fout bij MoveOut : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID);
//                if (Klant_Config != "")
//                {
//                    strError_Message = "Fout bij MoveOut : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
//                }
//                else
//                {
//                    WriteEnrollmentLog(S.ErrorCode.ToString() + " - " + S.ErrorDetails + " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), dr["ProductType"].ToString(),
//                    "MoveOut", "", EnrollmentID, dr["Ean18_code"].ToString());
//                }
//                //MessageBox.Show(S.ErrorCode.ToString());
//                //MessageBox.Show(S.ErrorDetails);
//                //MessageBox.Show(S.ErrorText);
//                //MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }

//            catch (WebException exception)
//            {
//                WriteLog("Fout bij MoveOut : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij MoveOut : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }

//            catch (Exception exception)
//            {
//                WriteLog("Fout bij MoveOut : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij MoveOut : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }
//            return blnOK;
//        }

//        private Boolean EndOfSupply(DataRow dr, Boolean blnToFile, string strRequestFile)
//        {
//            Boolean blnOK = false;
//            string eanForError = "";

//            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope enveloppe = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            string sender = "";
//            string identifier = "";
//            if (dr["ProductType"].ToString() == "G")
//            {
//                sender = "8714252022926";
//                identifier = "DDQ_M";
//            }
//            else
//            {
//                sender = strSender;
//                identifier = "DDQ_O";
//            }

//            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            source.SenderID = sender;
//            source.ContactTypeIdentifier = identifier;
//            header.Source = source;


//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content();
//            enveloppe.Portaal_Content = portaal_content;

//            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint meteringpoint = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint();
//            portaal_content.Portaal_MeteringPoint = meteringpoint;
//            meteringpoint.EANID = dr["Ean18_code"].ToString();
//            eanForError = dr["Ean18_code"].ToString();

//            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company();
//            meteringpoint.GridOperator_Company = meteringpointGridOperator;
//            meteringpointGridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();


//            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics meteringpointMPCommercial = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics();
//            meteringpoint.MPCommercialCharacteristics = meteringpointMPCommercial;


//            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics mpcommercialcharacteristicts = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics();
//            meteringpoint.MPCommercialCharacteristics = mpcommercialcharacteristicts;

//            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company balansesupplier_company = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company();
//            mpcommercialcharacteristicts.BalanceSupplier_Company = balansesupplier_company;
//            balansesupplier_company.ID = sender;

//            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation portaal_mutation = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation();
//            meteringpoint.Portaal_Mutation = portaal_mutation;
//            portaal_mutation.MutationDate = (DateTime)dr["Contract_Start_DT"];
//            portaal_mutation.ExternalReference = " test";

//            nl.Energie.EDSN.EndOfSupply.EndOfSupply EndOfSupply = new nl.Energie.EDSN.EndOfSupply.EndOfSupply();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            EndOfSupply.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));

//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //EndOfSupply.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderEndOfSupplyRespondingActivity";
//            EndOfSupply.Url = KC.CarUrl + @"synchroon/ResponderEndOfSupplyRespondingActivity";

//            EndOfSupply.Timeout = 120000;

//            nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope retour = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope();


//            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + @"EndOfSupply" + BestandsAanvulling + ".xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();

//            string ftpResponse = "";
//            if (FTPClass.FtpSendFile(strFTPServer + @"EndOfSupply" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"EndOfSupply" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//            {
//                //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//            }


//            StringWriter swXML = new StringWriter();
//            serializer.Serialize(swXML, enveloppe);
//            //MessageBox.Show(swXML.ToString());
//            int intOutBoxID = Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "EndOfSupply", swXML.ToString());


//            try
//            {

//                // 7 nov 2014




//                BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                { BestandsAanvulling = " LV " + BestandsAanvulling; }


//                //XmlDocument _Doc = new XmlDocument();
//                //_Doc.Load(path + @"EndOfSupplyResponse LV 120914 215836.xml");
//                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope));
//                //retour = (nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

//                if (blnToFile == true && Klant_Config != "")
//                {
//                    string fileName = path + @"EOS.xml";

//                    // string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                    XmlSerializer serializer1 = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope));
//                    if (File.Exists(fileName)) { File.Delete(fileName); }
//                    TextWriter WriteFileStream1 = new StreamWriter(fileName);
//                    serializer1.Serialize(WriteFileStream1, enveloppe);
//                    WriteFileStream1.Close();

//                    IPAddress ipAddr = localAddr;
//                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

//                    // Create a TCP socket.
//                    Socket client = new Socket(AddressFamily.InterNetwork,
//                            SocketType.Stream, ProtocolType.Tcp);

//                    // Connect the socket to the remote endpoint.
//                    client.Connect(ipEndPoint);

//                    if (File.Exists(path + "RemoteResult.xml") == true) { File.Delete(path + "RemoteResult.xml"); }

//                    client.SendFile(fileName);


//                    // Release the socket.
//                    client.Shutdown(SocketShutdown.Both);
//                    client.Close();

//                    SqlConnection conn = new SqlConnection(ConnString);
//                    conn.Open();

//                    int intCounter = 0;
//                    int intRecords = 0;
//                    //WriteLog("Lus", 10, 0);
//                    while (intRecords == 0 && intCounter < 200)
//                    {
//                        string strSQL = "select count(*) from Messages.dbo.XMLMessage";
//                        SqlCommand cmd = new SqlCommand(strSQL, conn);
//                        intRecords = (int)cmd.ExecuteScalar();
//                        Application.DoEvents();
//                        Thread.Sleep(100);
//                        intCounter++;
//                    }

//                    //WriteLog("EindeLus " + intRecords.ToString(), 10, 0);

//                    if (intRecords > 0)
//                    {
//                        string strSQL = "select * from Messages.dbo.XMLMessage";
//                        SqlCommand cmd = new SqlCommand(strSQL, conn);
//                        string strXML = cmd.ExecuteScalar().ToString();
//                        //WriteLog("Eindelees " + strXML.Length.ToString(), 10, 0);


//                        serializer = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope));
//                        XmlDocument _Doc = new XmlDocument();
//                        File.WriteAllText(path + "RemoteResult.xml", strXML);

//                        _Doc.LoadXml(strXML);
//                        //_Doc.Load(path + "RemoteResult.xml");
//                        //nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope result = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope();
//                        //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
//                        retour = (nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
//                    }
//                    conn.Close();
//                }
//                else
//                {
//                    retour = EndOfSupply.CallEndOfSupply(enveloppe);
//                }

//                nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;


//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope));
//                WriteFileStream = new StreamWriter(path + @"EndOfSupplyResponse" + BestandsAanvulling + ".xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();

//                ftpResponse = "";
//                if (FTPClass.FtpSendFile(strFTPServer + @"EndOfSupplyResponse" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"EndOfSupplyResponse" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                {
//                    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//                }

//                ftpResponse = "";
//                if (Klant_Config == "ROBIN" && strTest != "JA")
//                {
//                    if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "EndOfSupplyResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + @"EndOfSupplyResponse" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                    {
//                        //MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse);
//                        WriteLog("Fout bij verzenden naar nexant  EndOfSupply - " + eanForError + "-" + ftpResponse, 10, intOutBoxID);
//                    }
//                }

//                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_Portaal_Content_Portaal_Rejection))
//                {
//                    nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
//                    nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
//                    //MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
//                    WriteLog("Fout bij EndOfSupply - " + eanForError + "-" + rejectionPortaalType[0].RejectionText.ToString(), 10, intOutBoxID);
//                    if (Klant_Config != "")
//                    {
//                        strError_Message = "Fout bij EndOfSupply - " + eanForError + "-" + rejectionPortaalType[0].RejectionText.ToString();
//                    }
//                    else
//                    {
//                        WriteEnrollmentLog(rejectionPortaalType[0].RejectionText.ToString(), dr["ProductType"].ToString(),
//                            "EndOfSupply", "", EnrollmentID, dr["Ean18_code"].ToString());
//                    }
//                }
//                else
//                {
//                    blnOK = true;
//                    nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem = (nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;

//                    //406
//                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
//                    swXML = new StringWriter();
//                    serializer.Serialize(swXML, responseItem);

//                    DossierID = responseItem.Portaal_Mutation.Dossier.ID;

//                    int inboxID_406 = Save_Inbox(13, swXML.ToString(), "EndOfSupply");
//                    int edineID = Save_Edine(inboxID_406, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "406");
//                    Save_406_Header(edineID, DateTime.Now, strReceiver, strSender);
//                    Save_406(edineID, header.DocumentID, dr["Ean18_code"].ToString(), (DateTime)dr["Contract_Start_DT"],
//                        responseItem.Portaal_Mutation.Dossier.ID, dr["EnrollmentID"].ToString(), "");

//                    ProcessMessage.processMessage(inboxID_406, ConnString);

//                    //E09
//                    int inboxID_E09 = Save_Inbox(12, swXML.ToString(), "EndOfSupply");
//                    edineID = Save_Edine(inboxID_E09, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "E09");
//                    Save_E09_Header(edineID, DateTime.Now, strReceiver, strSender);
//                    Save_E09_End(edineID, header.DocumentID, dr["Ean18_code"].ToString(), dr["Netbeheerder_EAN13_Code"].ToString(), (DateTime)dr["Contract_Start_DT"], "7", "E01");
//                    ProcessMessage.processMessage(inboxID_E09, ConnString);

//                    Save_Switch_End((int)dr["Aansluiting_ID"], intOutBoxID, inboxID_406, inboxID_E09);

//                    //MessageBox.Show("Accepted - Dossier " + responseItem.Portaal_Mutation.Dossier.ID);

//                    WriteMasterdataRequest(dr["Ean18_code"].ToString(), dr["Netbeheerder_EAN13_Code"].ToString(), (DateTime)dr["Contract_Start_DT"]);

//                    blnOK = true;
//                }
//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                WriteLog("Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID);
//                if (Klant_Config != "")
//                {
//                    strError_Message = "Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
//                }
//                else
//                {
//                    WriteEnrollmentLog(S.ErrorCode.ToString() + " - " + S.ErrorDetails + " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), dr["ProductType"].ToString(),
//                    "EndOffSupply", "", EnrollmentID, dr["Ean18_code"].ToString());
//                }
//                //MessageBox.Show(S.ErrorCode.ToString());
//                //MessageBox.Show(S.ErrorDetails);
//                //MessageBox.Show(S.ErrorText);
//                //MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }
//            catch (WebException exception)
//            {
//                WriteLog("Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }
//            catch (Exception exception)
//            {
//                WriteLog("Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }

//            return blnOK;
//        }

//        public Boolean EndOfSupplyFromXML(string strRequestFile)
//        {
//            Boolean blnOK = false;
//            //string eanForError = "";

//            nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope enveloppe = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope();


//            nl.Energie.EDSN.EndOfSupply.EndOfSupply EndOfSupply = new nl.Energie.EDSN.EndOfSupply.EndOfSupply();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            EndOfSupply.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));

//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //EndOfSupply.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderEndOfSupplyRespondingActivity";
//            EndOfSupply.Url = KC.CarUrl + @"synchroon/ResponderEndOfSupplyRespondingActivity";

//            EndOfSupply.Timeout = 120000;

//            nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope retour = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope();

//            try
//            {
//                XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope));
//                XmlDocument _Doc = new XmlDocument();
//                _Doc.Load(strRequestFile);
//                //nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope result = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope();
//                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
//                enveloppe = (nl.Energie.EDSN.EndOfSupply.EndOfSupplyRequestEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

//                retour = EndOfSupply.CallEndOfSupply(enveloppe);

//                nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;


//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope));
//                StreamWriter WriteFileStream = new StreamWriter(path + @"result.xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();

//                blnOK = true;

//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                //XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                //TextReader tr = new StringReader(ex.Detail.InnerXml);
//                //SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);


//                //strError_Message = "Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                //    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
//                //MessageBox.Show(S.ErrorCode.ToString());
//                //MessageBox.Show(S.ErrorDetails);
//                //MessageBox.Show(S.ErrorText);
//                //MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }
//            catch (WebException exception)
//            {
//                //WriteLog("Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                //strError_Message = "Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }
//            catch (Exception exception)
//            {
//                //WriteLog("Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                //strError_Message = "Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }

//            return blnOK;
//        }



//        private Boolean ChangeOfPV(DataRow dr, string strNewPV, Boolean blnToFile, string strRequestFile)
//        {
//            Boolean blnOK = false;
//            string eanForError = "";

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope enveloppe = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            source.SenderID = strSender;// strPVSender;// strSender;
//            source.ContactTypeIdentifier = "DDQ_O";// "DDQ_O";
//            header.Source = source;


//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content();
//            enveloppe.Portaal_Content = portaal_content;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint meteringpoint = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint();
//            portaal_content.Portaal_MeteringPoint = meteringpoint;
//            meteringpoint.EANID = dr["Ean18_code"].ToString();
//            eanForError = dr["Ean18_code"].ToString();

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company();
//            meteringpoint.GridOperator_Company = meteringpointGridOperator;
//            meteringpointGridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics meteringpointMPCommercial = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics();
//            meteringpoint.MPCommercialCharacteristics = meteringpointMPCommercial;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics mpcommercialcharacteristicts = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics();
//            meteringpoint.MPCommercialCharacteristics = mpcommercialcharacteristicts;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company balansesupplier_company = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company();
//            mpcommercialcharacteristicts.BalanceSupplier_Company = balansesupplier_company;
//            balansesupplier_company.ID = strSender;//"8714252022919";

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceResponsibleParty_Company balansresponsibleparty_company = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceResponsibleParty_Company();
//            mpcommercialcharacteristicts.BalanceResponsibleParty_Company = balansresponsibleparty_company;
//            balansresponsibleparty_company.ID = strNewPV; // strPVSender;// "8712423012615";

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation portaal_mutation = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation();
//            meteringpoint.Portaal_Mutation = portaal_mutation;
//            portaal_mutation.MutationDate = (DateTime)dr["Contract_Start_DT"];


//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPV changeOfPV = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPV();

//            //String certPath = certpath + @"EDSN2013053100007.p12";
//            changeOfPV.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));


//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //changeOfPV.Url = @"https://portaal-fatn.nl.Energie.EDSN.nl/b2b/synchroon/ResponderChangeOfPVRespondingActivity";
//            changeOfPV.Url = KC.CarUrl + @"synchroon/ResponderChangeOfPVRespondingActivity";

//            changeOfPV.Timeout = 120000;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope retour = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope();


//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + @"changeOfPV.xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();

//            StringWriter swXML = new StringWriter();
//            serializer.Serialize(swXML, enveloppe);
//            //MessageBox.Show(swXML.ToString());
//            int intOutBoxID = Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "ChangeOfPV", swXML.ToString());


//            try
//            {
//                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                { BestandsAanvulling = " LV " + BestandsAanvulling; }



//                if (blnToFile == true && Klant_Config != "")
//                {
//                    string fileName = path + @"ChangeOfPV.xml";

//                    XmlSerializer serializer1 = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope));
//                    if (File.Exists(fileName)) { File.Delete(fileName); }
//                    TextWriter WriteFileStream1 = new StreamWriter(fileName);
//                    serializer1.Serialize(WriteFileStream1, enveloppe);
//                    WriteFileStream1.Close();

//                    IPAddress ipAddr = localAddr;
//                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

//                    Socket client = new Socket(AddressFamily.InterNetwork,
//                            SocketType.Stream, ProtocolType.Tcp);

//                    client.Connect(ipEndPoint);

//                    if (File.Exists(path + "RemoteResult.xml") == true) { File.Delete(path + "RemoteResult.xml"); }

//                    client.SendFile(fileName);

//                    client.Shutdown(SocketShutdown.Both);
//                    client.Close();

//                    SqlConnection conn = new SqlConnection(ConnString);
//                    conn.Open();

//                    int intCounter = 0;
//                    int intRecords = 0;
//                    while (intRecords == 0 && intCounter < 200)
//                    {
//                        string strSQL = "select count(*) from Messages.dbo.XMLMessage";
//                        SqlCommand cmd = new SqlCommand(strSQL, conn);
//                        intRecords = (int)cmd.ExecuteScalar();
//                        Application.DoEvents();
//                        Thread.Sleep(100);
//                        intCounter++;
//                    }

//                    if (intRecords > 0)
//                    {
//                        string strSQL = "select * from Messages.dbo.XMLMessage";
//                        SqlCommand cmd = new SqlCommand(strSQL, conn);
//                        string strXML = cmd.ExecuteScalar().ToString();

//                        serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope));
//                        XmlDocument _Doc = new XmlDocument();
//                        File.WriteAllText(path + "RemoteResult.xml", strXML);

//                        _Doc.LoadXml(strXML);
//                        retour = (nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
//                    }
//                    conn.Close();
//                }
//                else
//                {
//                    retour = changeOfPV.CallChangeOfPV(enveloppe);
//                }

//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope));
//                WriteFileStream = new StreamWriter(path + @"ChangeOfPVResponseEnvelope" + BestandsAanvulling + ".xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();

//                //string ftpResponse = "";
//                //if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "ChangeOfPVResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + @"ChangeOfPVResponseEnvelope" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                //{
//                //    MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse);
//                //}



//                nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;
//                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_Rejection))
//                {
//                    nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
//                    nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
//                    //MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
//                    WriteLog("Fout bij ChangeOfPV - " + eanForError + "-" + rejectionPortaalType[0].RejectionText.ToString(), 10, intOutBoxID);
//                    if (Klant_Config != "")
//                    {
//                        strError_Message = "Fout bij ChangeOfPV - " + eanForError + "-" + rejectionPortaalType[0].RejectionText.ToString();
//                    }
//                    else
//                    {
//                        WriteEnrollmentLog(rejectionPortaalType[0].RejectionText.ToString(), dr["ProductType"].ToString(),
//                            "ChangeOfPV", "", EnrollmentID, dr["Ean18_code"].ToString());
//                    }
//                }
//                else
//                {
//                    blnOK = true;
//                    nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem = (nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;

//                    //414
//                    new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
//                    swXML = new StringWriter();
//                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
//                    serializer.Serialize(swXML, responseItem);
//                    //int inboxID_414 = Save_Inbox(12, swXML.ToString(), "MoveIn");
//                    //int edineID = Save_Edine(inboxID_414, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "414");
//                    //Save_414_Header(edineID, DateTime.Now, strReceiver, strSender);
//                    //Save_414(edineID, header.DocumentID, dr["Ean18_code"].ToString(), (DateTime)dr["Contract_Start_DT"], "E01",
//                    //        "", dr["Transportkosten_naam1"].ToString(), dr["Aansluiting_Naam"].ToString(),
//                    //        dr["Transportkosten_Straat"].ToString(), dr["Transportkosten_Plaats"].ToString(), dr["Transportkosten_Postcode"].ToString(),
//                    //        "NL", "E", "E01", "E01", responseItem.Portaal_Mutation.Dossier.ID, dr["EnrollmentID"].ToString());

//                    //ProcessMessage.processMessage(inboxID_414);
//                    //E09

//                    DossierID = responseItem.Portaal_Mutation.Dossier.ID;

//                    int inboxID_E09 = Save_Inbox(12, swXML.ToString(), "ChangeOfPV");
//                    int edineID = Save_Edine(inboxID_E09, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "E09");
//                    Save_E09_Header(edineID, DateTime.Now, strReceiver, strSender);
//                    Save_E09(edineID, header.DocumentID, dr["Ean18_code"].ToString(), dr["Netbeheerder_EAN13_Code"].ToString(), (DateTime)dr["Contract_Start_DT"], "7", "E01", responseItem.Portaal_Mutation.Dossier.ID);
//                    ProcessMessage.processMessage(inboxID_E09, ConnString);

//                    Save_Switch((int)dr["Aansluiting_ID"], intOutBoxID, -1, inboxID_E09);

//                    //MessageBox.Show("Accepted - Dossier " + responseItem.Portaal_Mutation.Dossier.ID);
//                    blnOK = true;
//                }

//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                WriteLog("Fout bij ChangeOfPV : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID);
//                if (Klant_Config != "")
//                {
//                    strError_Message = "Fout bij ChangeOfPV : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
//                }
//                else
//                {
//                    WriteEnrollmentLog(S.ErrorCode.ToString() + " - " + S.ErrorDetails + " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), dr["ProductType"].ToString(),
//                    "ChangeOfPV", "", EnrollmentID, dr["Ean18_code"].ToString());
//                }
//                //MessageBox.Show(S.ErrorCode.ToString());
//                //MessageBox.Show(S.ErrorDetails);
//                //MessageBox.Show(S.ErrorText);
//                //MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }
//            catch (WebException exception)
//            {
//                WriteLog("Fout bij ChangeOfPV : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij MoveOut : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }
//            catch (Exception exception)
//            {
//                WriteLog("Fout bij ChangeOfPV : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij MoveOut : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }

//            return blnOK;
//        }

//        public Boolean ChangeOfPVFromXML(string strRequestFile)
//        {
//            Boolean blnOK = false;
//            string eanForError = "";

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope enveloppe = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope();




//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPV changeOfPV = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPV();

//            changeOfPV.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));


//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            changeOfPV.Url = KC.CarUrl + @"synchroon/ResponderChangeOfPVRespondingActivity";

//            changeOfPV.Timeout = 120000;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope retour = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope();

//            try
//            {
//                XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope));
//                XmlDocument _Doc = new XmlDocument();
//                _Doc.Load(strRequestFile);
//                enveloppe = (nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

//                retour = changeOfPV.CallChangeOfPV(enveloppe);

//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope));
//                StreamWriter WriteFileStream = new StreamWriter(path + @"result.xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();

//                blnOK = true;


//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//            }
//            catch (WebException exception)
//            {
//            }
//            catch (Exception exception)
//            {

//            }

//            return blnOK;
//        }

//        public Boolean ChangeOfPVDirect(string ean18, string nb_ean, string strNewPV, DateTime switch_dt, out string errorText)
//        {
//            Boolean blnOK = false;
//            errorText = "";

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope enveloppe = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            source.SenderID = strSender;// strPVSender;// strSender;
//            source.ContactTypeIdentifier = "DDQ_O";// "DDQ_O";
//            header.Source = source;


//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content();
//            enveloppe.Portaal_Content = portaal_content;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint meteringpoint = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint();
//            portaal_content.Portaal_MeteringPoint = meteringpoint;
//            meteringpoint.EANID = ean18;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company();
//            meteringpoint.GridOperator_Company = meteringpointGridOperator;
//            meteringpointGridOperator.ID = nb_ean;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics meteringpointMPCommercial = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics();
//            meteringpoint.MPCommercialCharacteristics = meteringpointMPCommercial;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics mpcommercialcharacteristicts = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics();
//            meteringpoint.MPCommercialCharacteristics = mpcommercialcharacteristicts;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company balansesupplier_company = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company();
//            mpcommercialcharacteristicts.BalanceSupplier_Company = balansesupplier_company;
//            balansesupplier_company.ID = strSender;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceResponsibleParty_Company balansresponsibleparty_company = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceResponsibleParty_Company();
//            mpcommercialcharacteristicts.BalanceResponsibleParty_Company = balansresponsibleparty_company;
//            balansresponsibleparty_company.ID = strNewPV; // strPVSender;// "8712423012615";

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation portaal_mutation = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation();
//            meteringpoint.Portaal_Mutation = portaal_mutation;
//            portaal_mutation.MutationDate = switch_dt;


//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPV changeOfPV = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPV();

//            //String certPath = certpath + @"EDSN2013053100007.p12";
//            changeOfPV.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));


//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //changeOfPV.Url = @"https://portaal-fatn.nl.Energie.EDSN.nl/b2b/synchroon/ResponderChangeOfPVRespondingActivity";
//            changeOfPV.Url = KC.CarUrl + @"synchroon/ResponderChangeOfPVRespondingActivity";

//            changeOfPV.Timeout = 120000;

//            nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope retour = new nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope();


//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVRequestEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + ean18 + "_"  + @"changeOfPV.xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();

//            StringWriter swXML = new StringWriter();
//            serializer.Serialize(swXML, enveloppe);
//            //MessageBox.Show(swXML.ToString());
//            //int intOutBoxID = Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "ChangeOfPV", swXML.ToString());


//            try
//            {
//                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                { BestandsAanvulling = " LV " + BestandsAanvulling; }

//                retour = changeOfPV.CallChangeOfPV(enveloppe);

//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope));
//                WriteFileStream = new StreamWriter(path + @"ChangeOfPVResponseEnvelope_" + ean18 + "_" + BestandsAanvulling + ".xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();

//                //string ftpResponse = "";
//                //if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "ChangeOfPVResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + @"ChangeOfPVResponseEnvelope" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                //{
//                //    MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse);
//                //}



//                nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;
//                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_Rejection))
//                {
//                    nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
//                    nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
//                    //MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
//                    blnOK = false;
//                    errorText = rejectionPortaalType[0].RejectionText.ToString();
//                    strError_Message = errorText;
//                }
//                else
//                {
//                    blnOK = true;
//                    nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem = (nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;

//                    //414
//                    //new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfPV.ChangeOfPVResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
//                    //swXML = new StringWriter();
//                    //serializer.Serialize(swXML, responseItem);
//                    //int inboxID_414 = Save_Inbox(12, swXML.ToString(), "MoveIn");
//                    //int edineID = Save_Edine(inboxID_414, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "414");
//                    //Save_414_Header(edineID, DateTime.Now, strReceiver, strSender);
//                    //Save_414(edineID, header.DocumentID, dr["Ean18_code"].ToString(), (DateTime)dr["Contract_Start_DT"], "E01",
//                            //"", dr["Transportkosten_naam1"].ToString(), dr["Aansluiting_Naam"].ToString(),
//                            //dr["Transportkosten_Straat"].ToString(), dr["Transportkosten_Plaats"].ToString(), dr["Transportkosten_Postcode"].ToString(),
//                            //"NL", "E", "E01", "E01", responseItem.Portaal_Mutation.Dossier.ID, dr["EnrollmentID"].ToString());

//                    //ProcessMessage.processMessage(inboxID_414);
//                    //E09

//                    //int inboxID_E09 = Save_Inbox(12, swXML.ToString(), "MoveIn");
//                    //edineID = Save_Edine(inboxID_E09, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "E09");
//                    //Save_E09_Header(edineID, DateTime.Now, strReceiver, strSender);
//                    //Save_E09(edineID, header.DocumentID, dr["Ean18_code"].ToString(), dr["Netbeheerder_EAN13_Code"].ToString(), (DateTime)dr["Contract_Start_DT"], "7", "E01");
//                    //ProcessMessage.processMessage(inboxID_E09);

//                    //Save_Switch((int)dr["Aansluiting_ID"], intOutBoxID, inboxID_414, inboxID_E09);

//                    //MessageBox.Show("Accepted - Dossier " + responseItem.Portaal_Mutation.Dossier.ID);
//                    blnOK = true;
//                }
//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                errorText = S.ErrorCode.ToString() + " " + S.ErrorDetails + " " + S.ErrorText + " " + ex.Detail.InnerXml.ToString(); 
//                //MessageBox.Show(S.ErrorCode.ToString());
//                //MessageBox.Show(S.ErrorDetails);
//                //MessageBox.Show(S.ErrorText);
//                //MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }
//            catch (WebException exception)
//            {
//                errorText = exception.Message;
//            }
//            catch (Exception exception)
//            {
//                errorText = exception.Message;
//            }
//            return blnOK;
//        }




//        private Boolean NameChange(DataRow dr)
//        {
//            Boolean blnOK = false;

//            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope enveloppe = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope();


//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            source.SenderID = strSender;
//            source.ContactTypeIdentifier = "DDQ_O";
//            header.Source = source;


//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content portaal_content = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content();
//            enveloppe.Portaal_Content = portaal_content;

//            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content_Portaal_MeteringPoint meteringpoint = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content_Portaal_MeteringPoint();
//            portaal_content.Portaal_MeteringPoint = meteringpoint;
//            meteringpoint.EANID = dr["Ean18_code"].ToString();


//            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company();
//            meteringpoint.GridOperator_Company = meteringpointGridOperator;
//            meteringpointGridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();


//            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics meteringpointMPCommercial = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics();
//            meteringpoint.MPCommercialCharacteristics = meteringpointMPCommercial;


//            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics mpcommercialcharacteristicts = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics();
//            meteringpoint.MPCommercialCharacteristics = mpcommercialcharacteristicts;

//            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_GridContractParty gridcontractparty = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_GridContractParty();
//            mpcommercialcharacteristicts.GridContractParty = gridcontractparty;


//            //gridcontractparty.Initials = "C";
//            //gridcontractparty.SurnamePrefix = "vd";
//            gridcontractparty.Surname = dr["Aansluiting_Naam"].ToString();


//            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company balansesupplier_company = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company();
//            mpcommercialcharacteristicts.BalanceSupplier_Company = balansesupplier_company;
//            balansesupplier_company.ID = strSender;

//            nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation portaal_mutation = new nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation();
//            meteringpoint.Portaal_Mutation = portaal_mutation;
//            portaal_mutation.MutationReason = nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope_MutationReasonPortaalCode.NAMECHG;
//            //portaal_mutation.ExternalReference = " test";

//            nl.Energie.EDSN.NameChange.NameChange NameChange = new nl.Energie.EDSN.NameChange.NameChange();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            NameChange.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));


//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //NameChange.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderNameChangeRespondingActivity";
//            NameChange.Url = KC.CarUrl + @"synchroon/ResponderNameChangeRespondingActivity";

//            NameChange.Timeout = 120000;



//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + @"NameChange.xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();

//            nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope retour = new nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope();
//            //retour = NameChange.CallNameChange(enveloppe);

//            try
//            {
//                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                { BestandsAanvulling = " LV " + BestandsAanvulling; }

//                retour = NameChange.CallNameChange(enveloppe);

//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.NameChange.NameChangeRequestEnvelope));
//                WriteFileStream = new StreamWriter(path + @"NameChangeRequestEnvelope" + BestandsAanvulling + ".xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();

//                nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;
//                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope_Portaal_Content_Portaal_Rejection))
//                {
//                    nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope_Portaal_Content_Portaal_Rejection itemRejection = (nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope_Portaal_Content_Portaal_Rejection)portaalResponse.Item;
//                    nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope_RejectionPortaalType[] rejectionPortaalType = itemRejection.Rejection;
//                    //MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
//                    if (Klant_Config != "")
//                    {
//                        strError_Message = rejectionPortaalType[0].RejectionText.ToString();
//                    }
//                    else
//                    {
//                        MessageBox.Show(rejectionPortaalType[0].RejectionText.ToString());
//                    }
//                }
//                else
//                {
//                    blnOK = true;
//                    nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope_Portaal_Content_Portaal_MeteringPoint reponseItem = (nl.Energie.EDSN.NameChange.NameChangeResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;

//                    DossierID = reponseItem.Portaal_Mutation.Dossier.ID;

//                    MessageBox.Show("Accepted");
//                    blnOK = true;
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
//            return blnOK;
//        }

//        public Boolean GetGains(Boolean blnPV, Boolean blnBatch)
//        {
//            Boolean blnOK = false;

//            //Masterdata
//            nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope enveloppe = new nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            if (blnPV != true) { source.SenderID = strSender; } else { source.SenderID = strPVSender; }
//            //source.ContactTypeIdentifier = "DDQ_O";
//            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
//            header.Source = source;

//            nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_Portaal_Content portaal_Content = new nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope_Portaal_Content();
//            enveloppe.Portaal_Content = portaal_Content;

//            nl.Energie.EDSN.LossGainRejectUpdate.LossGainRejectUpdate gainRequest = new nl.Energie.EDSN.LossGainRejectUpdate.LossGainRejectUpdate();

//            //String CertPath = certpath + @"EDSN2013053100006.p12";
//            if (blnPV == true)
//            {
//                //CertPath = certpath + @"EDSN2013053100007.p12";
//                gainRequest.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
//            }
//            else
//            {
//                gainRequest.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
//            }

//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //gainRequest.Url = @"https://portaal-fatn.nl.Energie.EDSN.nl/b2b/synchroon/ResponderLossGainRejectUpdateRespondingActivity";
//            gainRequest.Url = KC.CarUrl + @"synchroon/ResponderLossGainRejectUpdateRespondingActivity";

//            gainRequest.Timeout = 120000;

//            nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope retour = new nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope();

//            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.GainResultRequestEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + @"LossGainRejectUpdate" + BestandsAanvulling + ".xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();

//            string ftpResponse = "";

//                if (FTPClass.FtpSendFile(strFTPServer + @"LossGainRejectUpdate" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"LossGainRejectUpdate" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                {
//                    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//                }

//            try
//            {
//                retour = gainRequest.GainResult(enveloppe);
//                try
//                {
//                    BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                    if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }
//                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope));
//                    //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope));
//                    //Save to file kan weg
//                    WriteFileStream = new StreamWriter(path + @"GainResult" + BestandsAanvulling + ".xml");
//                    serializer.Serialize(WriteFileStream, retour);
//                    WriteFileStream.Close();

//                    ftpResponse = "";
//                    if (blnBatch != true && retour.Portaal_Content.Length > 0)
//                    if (FTPClass.FtpSendFile(strFTPServer + @"GainResult" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"GainResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                    {
//                        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//                    }


//                    blnOK = true;


//                    //Save to String
//                    StringWriter swXML = new StringWriter();
//                    serializer.Serialize(swXML, retour);
//                }
//                catch { }

//                //XmlDocument _Doc = new XmlDocument();
//                //_Doc.Load(path + "GainResultReRun.xml");
//                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope));
//                //retour = (nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));


//                if (retour.Portaal_Content.Length > 0)
//                {
//                    nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint[] responseItems = (nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint[])retour.Portaal_Content;

//                    foreach (nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem in responseItems)
//                    {
//                        //414
//                        string strBRP = "";
//                        if (responseItem.MPCommercialCharacteristics.BalanceResponsibleParty_Company != null)
//                        {
//                            strBRP = responseItem.MPCommercialCharacteristics.BalanceResponsibleParty_Company.ID;
//                        }
//                        serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
//                        StringWriter swXML = new StringWriter();
//                        int edineID = 0;
//                        serializer.Serialize(swXML, responseItem);

//                        if (blnPV != true)
//                        {
//                            int inboxID_414 = Save_Inbox(12, swXML.ToString(), "GetGains");
//                            edineID = Save_Edine(inboxID_414, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "414");
//                            Save_414_Header(edineID, DateTime.Now, strReceiver, strSender);
//                            Save_414(edineID, header.DocumentID, responseItem.EANID.ToString(), responseItem.Portaal_Mutation.MutationDate, "E01",
//                                    strBRP, "", "",
//                                    "", "", "",
//                                    "NL", "E", "E01", "E01", responseItem.Portaal_Mutation.Dossier.ID, "");

//                            ProcessMessage.processMessage(inboxID_414, ConnString);
//                        }

//                        if (blnPV == true)
//                        {
//                            //E09

//                            int inboxID_E09 = Save_Inbox(12, swXML.ToString(), "MoveIn");
//                            edineID = Save_Edine(inboxID_E09, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "E09");
//                            Save_E09_Header(edineID, DateTime.Now, strReceiver, strSender);
//                            Save_E09(edineID, header.DocumentID, responseItem.EANID.ToString(), responseItem.GridOperator_Company.ID.ToString(), responseItem.Portaal_Mutation.MutationDate, "7", "E01", responseItem.Portaal_Mutation.Dossier.ID);
//                            ProcessMessage.processMessage(inboxID_E09, ConnString);
//                        }
//                        //Save_Switch((int)dr["Aansluiting_ID"], intOutBoxID, inboxID_414, inboxID_E09);
//                    }
//                }
//                else
//                {
//                    if (Klant_Config == "") { File.Delete(path + @"GainResult" + BestandsAanvulling + ".xml"); }
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
//                blnOK = true;
//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                if (Klant_Config == "" && blnBatch)
//                {
//                    WriteLog("Fout Gains :" + S.ErrorDetails + " " + S.ErrorText, 10, -1);
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
//                if (Klant_Config == "" && blnBatch)
//                {
//                    WriteLog("Fout Gains :" + exception.Message, 10, -1);
//                }
//                else
//                {
//                    MessageBox.Show(exception.Message);
//                }
//            }
//            catch (Exception exception)
//            {
//                if (Klant_Config == "" && blnBatch)
//                {
//                    WriteLog("Fout Gains :" + exception.Message, 10, -1);
//                }
//                else
//                {
//                    MessageBox.Show(exception.Message);
//                }
//            }

//            return blnOK;
//        }

//        public Boolean GetLoss(Boolean blnPV, string strFileName, Boolean blnBatch)
//        {
//            Boolean blnOK = false;

//            //Masterdata
//            nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope enveloppe = new nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            if (blnPV != true) { source.SenderID = strSender; } else { source.SenderID = strPVSender; }
//            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
//            //source.ContactTypeIdentifier = "DDQ_O";
//            header.Source = source;

//            nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_Portaal_Content portaal_Content = new nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope_Portaal_Content();
//            enveloppe.Portaal_Content = portaal_Content;

//            nl.Energie.EDSN.LossGainRejectUpdate.LossGainRejectUpdate lossRequest = new nl.Energie.EDSN.LossGainRejectUpdate.LossGainRejectUpdate();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            if (blnPV == true)
//            {
//                //certPath = certpath + @"EDSN2013053100007.p12";
//                lossRequest.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
//            }
//            else
//            {
//                lossRequest.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
//            }

//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //lossRequest.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderLossGainRejectUpdateRespondingActivity";
//            lossRequest.Url = KC.CarUrl + @"synchroon/ResponderLossGainRejectUpdateRespondingActivity";


//            lossRequest.Timeout = 120000;

//            nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope retour = new nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope();

//            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.LossResultRequestEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + @"LossGainRejectUpdate" + BestandsAanvulling + ".xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();

//            string ftpResponse = "";
//            if (Klant_Config != "")
//            {
//                if (FTPClass.FtpSendFile(strFTPServer + @"LossGainRejectUpdate" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"LossGainRejectUpdate" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                {
//                    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//                }
//            }

//            try
//            {
//                if (strFileName == "")
//                {
//                    retour = lossRequest.LossResult(enveloppe);
//                }
//                else
//                {
//                    XmlDocument _Doc = new XmlDocument();
//                    _Doc.Load(strFileName);

//                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope));
//                    retour = (nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
//                }
//                try
//                {
//                    BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                    if (blnPV == true) { BestandsAanvulling = " PV " + BestandsAanvulling; } else { BestandsAanvulling = " LV " + BestandsAanvulling; }
//                    //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
//                    serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope));
//                    //Save to file kan weg
//                    WriteFileStream = new StreamWriter(path + @"LossResult" + BestandsAanvulling + ".xml");
//                    serializer.Serialize(WriteFileStream, retour);
//                    WriteFileStream.Close();

//                    ftpResponse = "";
//                    if (blnBatch != true && retour.Portaal_Content.Length > 0)
//                    if (FTPClass.FtpSendFile(strFTPServer + @"LossResult" + BestandsAanvulling + ".xml", strFTPUser, strFTPPassword, path + @"LossResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                    {
//                        //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//                    }

//                    ftpResponse = "";
//                    if (Klant_Config == "ROBIN" && strTest != "JA")
//                    {
//                        if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "LossResultResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + @"LossResult" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                        {
//                            if (blnBatch || Klant_Config != "")
//                            {
//                                WriteLog("Fout bij verzenden naar nexant " + ftpResponse, 10, -1);
//                            }
//                            else
//                            {
//                                MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse);
//                            }
//                        }
//                    }
//                    //Save to String
//                    StringWriter swXML = new StringWriter();
//                    serializer.Serialize(swXML, retour);
//                }
//                catch { }

//                //XmlDocument _Doc = new XmlDocument();
//                //_Doc.Load(path + "LossResult LV 121813 95616.xml");
//                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope));
//                //retour = (nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));




//                if (retour.Portaal_Content.Length > 0)
//                {
//                    nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint[] responseItems = (nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint[])retour.Portaal_Content;

//                    foreach (nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem in responseItems)
//                    {
//                        //406
//                        serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.LossResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint));
//                        StringWriter swXML = new StringWriter();
//                        serializer.Serialize(swXML, responseItem);
//                        int edineID = 0;

//                        if (blnPV != true)
//                        {
//                            int inboxID_406 = Save_Inbox(13, swXML.ToString(), "MoveOut");
//                            edineID = Save_Edine(inboxID_406, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "406");
//                            Save_406_Header(edineID, DateTime.Now, strReceiver, strSender);

//                            string NewBalanceSupplier = "";
//                            if (responseItem.MPCommercialCharacteristics.BalanceSupplier_Company != null)
//                            {
//                                NewBalanceSupplier = responseItem.MPCommercialCharacteristics.BalanceSupplier_Company.ID.ToString();
//                            }

//                            Save_406(edineID, header.DocumentID, responseItem.EANID.ToString(), responseItem.Portaal_Mutation.MutationDate,
//                                responseItem.Portaal_Mutation.Dossier.ID, "", NewBalanceSupplier);

//                            ProcessMessage.processMessage(inboxID_406, ConnString);
//                        }
//                        else
//                        {
//                            //E09
//                            int inboxID_E09 = Save_Inbox(12, swXML.ToString(), "MoveIn");
//                            edineID = Save_Edine(inboxID_E09, strReceiver, strSender, DateTime.Now, header.DocumentID, "UTILMD", "E09");
//                            Save_E09_Header(edineID, DateTime.Now, strReceiver, strSender);
//                            Save_E09_End(edineID, header.DocumentID, responseItem.EANID.ToString(), responseItem.GridOperator_Company.ID.ToString(), responseItem.Portaal_Mutation.MutationDate, "7", "E01");
//                            ProcessMessage.processMessage(inboxID_E09, ConnString);
//                        }
//                        //Save_Switch_End((int)dr["Aansluiting_ID"], intOutBoxID, inboxID_406, inboxID_E09);
//                    }
//                }
//                else
//                {
//                    if (Klant_Config == "") { File.Delete(path + @"LossResult" + BestandsAanvulling + ".xml"); }
//                }

//                blnOK = true;
//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                if (blnBatch || Klant_Config != "")
//                {
//                    WriteLog("Fout Losses " + S.ErrorDetails + " " + S.ErrorText, 10, -1);
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

//                if (blnBatch || Klant_Config != "")
//                {
//                    WriteLog("Fout Losses " + exception.Message, 10, -1);
//                }
//                else
//                {
//                    MessageBox.Show(exception.Message);
//                }
//            }
//            catch (Exception exception)
//            {

//                if (blnBatch || Klant_Config != "")
//                {
//                    WriteLog("Fout Losses " + exception.Message, 10, -1);
//                }
//                else
//                {
//                    MessageBox.Show(exception.Message);
//                }
//            }

//            return blnOK;
//        }
//        public Boolean GetReject(Boolean blnPV)
//        {
//            Boolean blnOK = false;

//            //Masterdata
//            nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope enveloppe = new nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            if (blnPV != true) { source.SenderID = strSender; } else { source.SenderID = strPVSender; }
//            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
//            //source.ContactTypeIdentifier = "DDQ_O";
//            header.Source = source;

//            nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_Portaal_Content portaal_Content = new nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope_Portaal_Content();
//            enveloppe.Portaal_Content = portaal_Content;

//            nl.Energie.EDSN.LossGainRejectUpdate.LossGainRejectUpdate rejectRequest = new nl.Energie.EDSN.LossGainRejectUpdate.LossGainRejectUpdate();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            if (blnPV == true)
//            {
//                //certPath = certpath + @"EDSN2013053100007.p12";
//                rejectRequest.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
//            }
//            else
//            {
//                rejectRequest.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
//            }

//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //rejectRequest.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderLossGainRejectUpdateRespondingActivity";
//            rejectRequest.Url = KC.CarUrl + @"synchroon/ResponderLossGainRejectUpdateRespondingActivity";


//            rejectRequest.Timeout = 120000;

//            nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultResponseEnvelope retour = new nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultResponseEnvelope();

//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultRequestEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + @"LossGainRejectUpdate.xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();

//            try
//            {
//                retour = rejectRequest.RejectionResult(enveloppe);

//                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                BestandsAanvulling = " PV " + BestandsAanvulling;
//                XmlSerializer serializer2 = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.RejectionResultResponseEnvelope));
//                TextWriter WriteFileStream2 = new StreamWriter(path + @"RejectionResponse" + BestandsAanvulling + ".xml");
//                serializer2.Serialize(WriteFileStream2, retour);
//                WriteFileStream2.Close();


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
//                blnOK = true;
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

//            return blnOK;
//        }

//        public Boolean GetUpdates(Boolean blnPV)
//        {
//            Boolean blnOK = false;

//            //Masterdata
//            nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultRequestEnvelope enveloppe = new nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            if (blnPV != true) { source.SenderID = strSender; } else { source.SenderID = strPVSender; }
//            if (blnPV != true) { source.ContactTypeIdentifier = "DDQ_O"; } else { source.ContactTypeIdentifier = "DDK_O"; }
//            //source.ContactTypeIdentifier = "DDQ_O";
//            header.Source = source;

//            nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultRequestEnvelope_Portaal_Content portaal_Content = new nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultRequestEnvelope_Portaal_Content();
//            enveloppe.Portaal_Content = portaal_Content;

//            nl.Energie.EDSN.LossGainRejectUpdate.LossGainRejectUpdate rejectUpdate = new nl.Energie.EDSN.LossGainRejectUpdate.LossGainRejectUpdate();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            if (blnPV == true)
//            {
//                //certPath = certpath + @"EDSN2013053100007.p12";
//                rejectUpdate.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certPV, certPVPassword));
//            }
//            else
//            {
//                rejectUpdate.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));
//            }

//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //rejectUpdate.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderLossGainRejectUpdateRespondingActivity";
//            rejectUpdate.Url = KC.CarUrl + @"synchroon/ResponderLossGainRejectUpdateRespondingActivity";


//            rejectUpdate.Timeout = 120000;

//            nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultResponseEnvelope retour = new nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultResponseEnvelope();

//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultResponseEnvelope));

//           // TextWriter WriteFileStream = new StreamWriter(path + @"LossGainRejectUpdate.xml");
//           // serializer.Serialize(WriteFileStream, enveloppe);
//           // WriteFileStream.Close();

//            try
//            {
//                retour = rejectUpdate.UpdateResult(enveloppe);

//                string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                BestandsAanvulling = " PV " + BestandsAanvulling;
//                XmlSerializer serializer2 = new XmlSerializer(typeof(nl.Energie.EDSN.LossGainRejectUpdate.UpdateResultResponseEnvelope));
//                TextWriter WriteFileStream2 = new StreamWriter(path + @"UpdateResponse" + BestandsAanvulling + ".xml");
//                serializer2.Serialize(WriteFileStream2, retour);
//                WriteFileStream2.Close();


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
//                blnOK = true;
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

//            return blnOK;
//        }

//        public Boolean MoveInBatch(DataRow dr)
//        {
//            Boolean blnOK = false;

//            //Move in

//            nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope enveloppe = new nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            source.SenderID = strSender;
//            source.ContactTypeIdentifier = "DDQ_O";
//            header.Source = source;

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP[] MeteringPoint = new nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP[1];
//            enveloppe.Portaal_Content = MeteringPoint;

//            nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP meteringpoint = new nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP();
//            MeteringPoint[0] = meteringpoint;

//            meteringpoint.EANID = dr["Ean18_code"].ToString();
//            //portaal_content.Portaal_MeteringPoint = meteringpoint;

//            nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP_GridOperator_Company();
//            meteringpointGridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();
//            meteringpoint.GridOperator_Company = meteringpointGridOperator;

//            nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP_MPCC meteringpointMPCommercial = new nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP_MPCC();
//            meteringpoint.MPCommercialCharacteristics = meteringpointMPCommercial;
//            if (dr["KvKNr"].ToString() != "")
//            {
//                meteringpointMPCommercial.ChamberOfCommerceID = dr["KvKNr"].ToString();
//            }

//            nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP_MPCC_GridContractParty meteringpointGridContact = new nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP_MPCC_GridContractParty();
//            meteringpointGridContact.Surname = dr["Aansluiting_Naam"].ToString();
//            if (!DBNull.Value.Equals(dr["GeboorteDatum"]))
//            {
//                meteringpointGridContact.BirthDateSpecified = true;
//                meteringpointGridContact.BirthDate = (DateTime)dr["GeboorteDatum"];
//            }
//            meteringpointMPCommercial.GridContractParty = meteringpointGridContact;

//            nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company meteringpointBalanceSupplier = new nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company();
//            meteringpointBalanceSupplier.ID = dr["Leverancier_EAN13_Code"].ToString();
//            meteringpointMPCommercial.BalanceSupplier_Company = meteringpointBalanceSupplier;

//            nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP_MPCC_BalanceResponsibleParty_Company meteringpointBalanceResponsibleParty = new nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP_MPCC_BalanceResponsibleParty_Company();
//            meteringpointBalanceResponsibleParty.ID = "8712423012615";
//            meteringpointMPCommercial.BalanceResponsibleParty_Company = meteringpointBalanceResponsibleParty;

//            nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP_PM meteringpointPortaalMutation = new nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope_PC_PMP_PM();
//            meteringpointPortaalMutation.MutationDate = (DateTime)dr["Contract_Start_DT"];
//            meteringpoint.Portaal_Mutation = meteringpointPortaalMutation;

//            nl.Energie.EDSN.MoveInBatch.MoveInBatch moveIn = new nl.Energie.EDSN.MoveInBatch.MoveInBatch();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            moveIn.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));


//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //moveIn.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderMoveInBatchRespondingActivity";
//            moveIn.Url = KC.CarUrl + @"synchroon/ResponderMoveInBatchRespondingActivity";

//            moveIn.Timeout = 120000;

//            nl.Energie.EDSN.MoveInBatch.MoveInBatchResponseEnvelope retour = new nl.Energie.EDSN.MoveInBatch.MoveInBatchResponseEnvelope();


//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MoveInBatch.MoveInBatchRequestEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + @"MoveInBatch.xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();
//            StringWriter swXML = new StringWriter();
//            serializer.Serialize(swXML, enveloppe);
//            //MessageBox.Show(swXML.ToString());
//            int intOutBoxID = Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "MoveInBatch", swXML.ToString());

//            retour = moveIn.CallMoveInBatch(enveloppe);

//            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//            BestandsAanvulling = " PV " + BestandsAanvulling;
//            XmlSerializer serializer2 = new XmlSerializer(typeof(nl.Energie.EDSN.MoveInBatch.MoveInBatchResponseEnvelope));
//            TextWriter WriteFileStream2 = new StreamWriter(path + @"MoveInBatch2" + BestandsAanvulling + ".xml");
//            serializer2.Serialize(WriteFileStream2, retour);
//            WriteFileStream2.Close();

//            string ftpResponse = "";
//            if (Klant_Config == "ROBIN" && strTest != "JA")
//            {
//                if (FTPClass.FtpSendFile("ftp://services.robinenergie.camelit.nl:21000/PreDelivery/" + "MoveInBatchResponseEnvelope" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xml", "edbg4nexant", "5512-XXSP-KLB", path + @"MoveInBatch2" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//                {
//                    WriteLog("Fout bij verzenden naar nexant " + ftpResponse, 10, -1);
//                    //MessageBox.Show("Fout bij verzenden naar nexant " + ftpResponse);
//                }
//            }



//            try
//            {
//                nl.Energie.EDSN.MoveInBatch.MoveInBatchResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;
//                if (portaalResponse.Item != null)
//                {
//                    nl.Energie.EDSN.MoveInBatch.MoveInBatchResponseEnvelope_Portaal_Content_EDSN_SimpleRejection edsn_simplerejection = new nl.Energie.EDSN.MoveInBatch.MoveInBatchResponseEnvelope_Portaal_Content_EDSN_SimpleRejection();
//                    WriteLog("Fout MoveinBatch " + edsn_simplerejection.SimpleRejection.RejectionText.ToString(), 10, -1);
//                    //MessageBox.Show(edsn_simplerejection.SimpleRejection.RejectionText.ToString());
//                }
//                else
//                {
//                    //EDSN_MoveInBatch.MoveInBatchResponseEnvelope_Portaal_Content_Portaal_MeteringPoint reponseItem = (EDSN_MoveInBatch.MoveInBatchResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaalResponse.Item;
//                    Save_Switch((int)dr["Aansluiting_ID"], intOutBoxID, 0, 0);
//                    //MessageBox.Show("Accepted");
//                }
//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                WriteLog("Fout MoveinBatch " + ex.Detail.InnerXml.ToString(), 10, -1);
//                strError_Message = "Fout MoveinBatch " + ex.Detail.InnerXml.ToString();
//                //MessageBox.Show(S.ErrorCode.ToString());
//                //MessageBox.Show(S.ErrorDetails);
//                //MessageBox.Show(S.ErrorText);
//                //MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }
//            catch (WebException exception)
//            {
//                WriteLog("Fout MoveinBatch " + exception.Message, 10, -1);
//                strError_Message = "Fout bij MoveinBatch : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                MessageBox.Show(exception.Message);
//            }
//            catch (Exception exception)
//            {
//                WriteLog("Fout MoveinBatch " + exception.Message, 10, -1);
//                strError_Message = "Fout bij MoveinBatch : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                MessageBox.Show(exception.Message);
//            }


//            return blnOK;
//        }

//        private Boolean EndOfSupplyBatch(DataRow dr)
//        {
//            Boolean blnOK = false;

//            nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope enveloppe = new nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = "EDM00006";
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = "8712423010208";
//            destination.Receiver = receiver;

//            nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            source.SenderID = HoofdLV;
//            source.ContactTypeIdentifier = "DDQ_O";
//            header.Source = source;


//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint[] MeteringPoint = new nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint[1];
//            enveloppe.Portaal_Content = MeteringPoint;

//            nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint meteringpoint = new nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint();
//            MeteringPoint[0] = meteringpoint;

//            meteringpoint.EANID = "871691687421057305";
//            //portaal_content.Portaal_MeteringPoint = meteringpoint;

//            nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company meteringpointGridOperator = new nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_GridOperator_Company();
//            meteringpointGridOperator.ID = "8716916000004";
//            meteringpoint.GridOperator_Company = meteringpointGridOperator;

//            nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics meteringpointMPCommercial = new nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics();
//            meteringpoint.MPCommercialCharacteristics = meteringpointMPCommercial;

//            nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company meteringpointBalanceSupplier = new nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_MPCommercialCharacteristics_BalanceSupplier_Company();
//            meteringpointBalanceSupplier.ID = HoofdLV;
//            meteringpointMPCommercial.BalanceSupplier_Company = meteringpointBalanceSupplier;

//            nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation meteringpointPortaalMutation = new nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope_Portaal_Content_Portaal_MeteringPoint_Portaal_Mutation();
//            meteringpointPortaalMutation.MutationDate = DateTime.Parse("4-11-2012");
//            meteringpoint.Portaal_Mutation = meteringpointPortaalMutation;


//            nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatch EndOfSupplyBatch = new nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatch();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            EndOfSupplyBatch.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));

//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //EndOfSupplyBatch.Url = @"https://portaal-fatn.nl.Energie.EDSN.nl/b2b/synchroon/ResponderEndOfSupplyBatchRespondingActivity";
//            EndOfSupplyBatch.Url = KC.CarUrl + @"synchroon/ResponderEndOfSupplyBatchRespondingActivity";

//            EndOfSupplyBatch.Timeout = 120000;

//            nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchResponseEnvelope retour = new nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchResponseEnvelope();



//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchRequestEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + @"EndOfSupplyBatch.xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();

//            StringWriter swXML = new StringWriter();
//            serializer.Serialize(swXML, enveloppe);
//            //MessageBox.Show(swXML.ToString());
//            int intOutBoxID = Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "EndOfSupply", swXML.ToString());

//            try
//            {
//                retour = EndOfSupplyBatch.CallEndOfSupplyBatch(enveloppe);
//                nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchResponseEnvelope_Portaal_Content portaalResponse = retour.Portaal_Content;
//                //Is dit goed??
//                if (portaalResponse.Item == null)
//                {
//                    nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchResponseEnvelope_Portaal_Content_EDSN_SimpleRejection edsn_simplerejection = new nl.Energie.EDSN.EndOfSupplyBatch.EndOfSupplyBatchResponseEnvelope_Portaal_Content_EDSN_SimpleRejection();
//                    WriteLog("Fout EndOffSupplyBatch " + edsn_simplerejection.SimpleRejection.RejectionText.ToString(), 10, -1);
//                    //MessageBox.Show(edsn_simplerejection.SimpleRejection.RejectionText.ToString());

//                }
//                else
//                {
//                    blnOK = true;
//                    Save_Switch_End((int)dr["Aansluiting_ID"], intOutBoxID, 0, 0);

//                    //MessageBox.Show("Accepted");
//                    blnOK = true;
//                }
//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                //MessageBox.Show(S.ErrorCode.ToString());
//                //MessageBox.Show(S.ErrorDetails);
//                //MessageBox.Show(S.ErrorText);
//                //MessageBox.Show(ex.Detail.InnerXml.ToString());
//                WriteLog("Fout EndOffSupplyBatch " + ex.Detail.InnerXml.ToString(), 10, -1);
//                strError_Message = "Fout EndOffSupplyBatch " + ex.Detail.InnerXml.ToString();
//            }
//            catch (WebException exception)
//            {
//                //MessageBox.Show(exception.Message);
//                WriteLog("Fout EndOffSupplyBatch " + exception.Message, 10, -1);
//                strError_Message = "Fout bij EndOffSupplyBatch : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//            }
//            catch (Exception exception)
//            {
//                WriteLog("Fout EndOffSupplyBatch " + exception.Message, 10, -1);
//                strError_Message = "Fout bij EndOffSupplyBatch : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }
//            return blnOK;
//        }

//        private Boolean EOSNotice(DataRow dr, Boolean blnToFile, string strRequestFile)
//        {
//            Boolean blnOK = false;

//            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope enveloppe = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            string sender = "";
//            string identifier = "";
//            if (dr["ProductType"].ToString() == "G")
//            {
//                sender = "8714252022926";
//                identifier = "DDQ_M";
//            }
//            else
//            {
//                sender = strSender;
//                identifier = "DDQ_O";
//            }

//            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_EDSNBusinessDocumentHeader_Source();
//            source.SenderID = sender;
//            source.ContactTypeIdentifier = identifier;
//            header.Source = source;


//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP[] pc_pmp = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP[1];

//            pc_pmp[0] = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP();
//            enveloppe.Portaal_Content = pc_pmp;

//            pc_pmp[0].EANID = dr["Ean18_code"].ToString();
//            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_GridOperator_Company gridOperator = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_GridOperator_Company();
//            pc_pmp[0].GridOperator_Company = gridOperator;
//            gridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();
//            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_MPCC mpcc = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_MPCC();
//            pc_pmp[0].MPCommercialCharacteristics = mpcc;
//            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_MPCC_BalanceSupplier_Company balanceSupplier = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_MPCC_BalanceSupplier_Company();
//            mpcc.BalanceSupplier_Company = balanceSupplier;
//            balanceSupplier.ID = sender;
//            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_PM pm = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope_PC_PMP_PM();
//            pc_pmp[0].Portaal_Mutation = pm;
//            pm.MutationDate = (DateTime)dr["Contract_Start_DT"];
//            //nl.Energie.EDSN.NoticeEos.NoticeEOSNotificationEnvelope_PC_PMP_MPCC_GridContractParty gridContact = new nl.Energie.EDSN.NoticeEos.NoticeEOSNotificationEnvelope_PC_PMP_MPCC_GridContractParty();
//            //mpcc.GridContractParty = gridContact;
//            //gridContact.Surname = dr["account_KorteNaam"].ToString();
//            //gridContact.Contact = new nl.Energie.EDSN.NoticeEos.NoticeEOSNotificationEnvelope_BoundAddressType();
//            //gridContact.Contact.StreetName = dr["Transportkosten_Straat"].ToString();
//            //gridContact.Contact.BuildingNr = dr["Transportkosten_Huisnummer"].ToString();
//            //gridContact.Contact.ZIPCode = dr["Transportkosten_Postcode"].ToString();
//            //gridContact.Contact.CityName = dr["Transportkosten_Plaats"].ToString();
//            //gridContact.Contact.Country = "NL";


//            nl.Energie.EDSN.NoticeEOS.NoticeEOS NoticeEOS = new nl.Energie.EDSN.NoticeEOS.NoticeEOS();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            NoticeEOS.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));

//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //EndOfSupply.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderEndOfSupplyRespondingActivity";
//            NoticeEOS.Url = KC.CarUrl + @"synchroon/ResponderNoticeEOSRespondingActivity";

//            NoticeEOS.Timeout = 120000;

//            nl.Energie.EDSN.NoticeEOS.NoticeEOSAcknowledgementEnvelope retour = new nl.Energie.EDSN.NoticeEOS.NoticeEOSAcknowledgementEnvelope();


//            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + @"NoticeEOS" + BestandsAanvulling + ".xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();

//            //string ftpResponse = "";
//            //if (FTPClass.FtpSendFile("ftp://62.148.191.136/" + @"EndOfSupply" + BestandsAanvulling + ".xml", "robin", "Wu!69Z#", path + @"EndOfSupply" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//            //{
//            //    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//            //}


//            StringWriter swXML = new StringWriter();
//            serializer.Serialize(swXML, enveloppe);
//            //MessageBox.Show(swXML.ToString());
//            int intOutBoxID = 0; // Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "NoticeEOS", swXML.ToString());


//            try
//            {

//                BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                { BestandsAanvulling = " LV " + BestandsAanvulling; }


//                //XmlDocument _Doc = new XmlDocument();
//                //_Doc.Load(path + @"EndOfSupplyResponse LV 120914 215836.xml");
//                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope));
//                //retour = (nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

//                if (blnToFile == true && Klant_Config != "")
//                {
//                    string fileName = path + @"EOSNotice.xml";

//                    // string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                    XmlSerializer serializer1 = new XmlSerializer(typeof(nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope));
//                    if (File.Exists(fileName)) { File.Delete(fileName); }
//                    TextWriter WriteFileStream1 = new StreamWriter(fileName);
//                    serializer1.Serialize(WriteFileStream1, enveloppe);
//                    WriteFileStream1.Close();

//                    IPAddress ipAddr = localAddr;
//                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

//                    // Create a TCP socket.
//                    Socket client = new Socket(AddressFamily.InterNetwork,
//                            SocketType.Stream, ProtocolType.Tcp);

//                    // Connect the socket to the remote endpoint.
//                    client.Connect(ipEndPoint);

//                    if (File.Exists(path + "RemoteResult.xml") == true) { File.Delete(path + "RemoteResult.xml"); }

//                    client.SendFile(fileName);


//                    // Release the socket.
//                    client.Shutdown(SocketShutdown.Both);
//                    client.Close();

//                    SqlConnection conn = new SqlConnection(ConnString);
//                    conn.Open();

//                    int intCounter = 0;
//                    int intRecords = 0;
//                    //WriteLog("Lus", 10, 0);
//                    while (intRecords == 0 && intCounter<200)
//                    {
//                        string strSQL = "select count(*) from Messages.dbo.XMLMessage";
//                        SqlCommand cmd = new SqlCommand(strSQL, conn);
//                        intRecords = (int)cmd.ExecuteScalar();
//                        Application.DoEvents();
//                        Thread.Sleep(100);
//                        intCounter++;
//                    }

//                    //WriteLog("EindeLus " + intRecords.ToString() , 10, 0);

//                    if (intRecords > 0)
//                    {
//                        string strSQL = "select * from Messages.dbo.XMLMessage";
//                        SqlCommand cmd = new SqlCommand(strSQL, conn);
//                        string strXML = cmd.ExecuteScalar().ToString();



//                        serializer = new XmlSerializer(typeof(nl.Energie.EDSN.NoticeEOS.NoticeEOSAcknowledgementEnvelope));
//                        XmlDocument _Doc = new XmlDocument();
//                        File.WriteAllText(path + "RemoteResult.xml", strXML);

//                        _Doc.LoadXml(strXML);
//                        //_Doc.Load(path + "RemoteResult.xml");
//                        //nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope result = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope();
//                        //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
//                        retour = (nl.Energie.EDSN.NoticeEOS.NoticeEOSAcknowledgementEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));
//                    }
//                    conn.Close();
//                }
//                else
//                {

//                    retour = NoticeEOS.NoticeEOSNotification(enveloppe);
//                }
//                nl.Energie.EDSN.NoticeEOS.NoticeEOSAcknowledgementEnvelope_PC portaalResponse = retour.Portaal_Content;

//                blnOK = true;

//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.NoticeEOS.NoticeEOSAcknowledgementEnvelope));
//                WriteFileStream = new StreamWriter(path + @"NoticeEos" + BestandsAanvulling + ".xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();


//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                WriteLog("Fout bij EndOfSupply Notice:  " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID);
//                if (Klant_Config != "")
//                {
//                    strError_Message = "Fout bij EndOfSupply Notice : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
//                }
//                else
//                {
//                    WriteEnrollmentLog(S.ErrorCode.ToString() + " - " + S.ErrorDetails + " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), dr["ProductType"].ToString(),
//                    "EOSNotice", "", EnrollmentID, dr["Ean18_code"].ToString());
//                }
//                //MessageBox.Show(S.ErrorCode.ToString());
//                //MessageBox.Show(S.ErrorDetails);
//                //MessageBox.Show(S.ErrorText);
//                //MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }
//            catch (WebException exception)
//            {
//                WriteLog("Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }
//            catch (Exception exception)
//            {
//                WriteLog("Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }


//            return blnOK;
//        }

//        private Boolean ChangeOfAllocationMethod(DataRow dr, Boolean blnToFile, string strRequestFile)
//        {
//            Boolean blnOK = false;

//            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope enveloppe = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope();

//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader header = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader();
//            header.ContentHash = "";
//            header.CreationTimestamp = DateTime.Now;
//            header.DocumentID = GetMessageID.getMessageID(ConnString);
//            header.ExpiresAt = DateTime.Now.AddMinutes(200);
//            header.ExpiresAtSpecified = true;
//            header.MessageID = System.Guid.NewGuid().ToString();
//            enveloppe.EDSNBusinessDocumentHeader = header;

//            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader_Destination destination = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader_Destination();
//            header.Destination = destination;

//            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver receiver = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader_Destination_Receiver();
//            receiver.Authority = "";
//            receiver.ContactTypeIdentifier = "EDSN";
//            receiver.ReceiverID = strReceiver;
//            destination.Receiver = receiver;

//            string sender = "";
//            string identifier = "";
//            if (dr["ProductType"].ToString() == "G")
//            {
//                sender = "8714252022926";
//                identifier = "DDQ_M";
//            }
//            else
//            {
//                sender = strSender;
//                identifier = "DDQ_O";
//            }

//            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader_Source source = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EDSNBusinessDocumentHeader_Source();
//            source.SenderID = sender;
//            source.ContactTypeIdentifier = identifier;
//            header.Source = source;


//            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC pc = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC();
//            enveloppe.Portaal_Content = pc;


//            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP pc_pmp = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP();

//            //pc_pmp = new nl.Energie.EDSN.ChangeOfAllocationMethod.NoticeEOSNotificationEnvelope_PC_PMP();
//            enveloppe.Portaal_Content.Portaal_MeteringPoint = pc_pmp;

//            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_MPPC pc_pmp_mppc = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_MPPC();
//            if ((bool)dr["SlimmeMeterAllocatie"] == true)
//            {
//                pc_pmp_mppc.AllocationMethod = nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EnergyAllocationMethodPortaalCode.SMA;
//            }
//            else
//            {
//                pc_pmp_mppc.AllocationMethod = nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_EnergyAllocationMethodPortaalCode.PRF;
//            }
//                pc_pmp.MPPhysicalCharacteristics = pc_pmp_mppc;

//            pc_pmp.EANID = dr["Ean18_code"].ToString();
//            pc_pmp.ValidFromDate = (DateTime)dr["Contract_Start_DT"];
//            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_GridOperator_Company gridOperator = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_GridOperator_Company();
//            pc_pmp.GridOperator_Company = gridOperator;
//            gridOperator.ID = dr["Netbeheerder_EAN13_Code"].ToString();
//            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_MPCC mpcc = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_MPCC();
//            pc_pmp.MPCommercialCharacteristics = mpcc;


//            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company balanceSupplier = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_MPCC_BalanceSupplier_Company();
//            mpcc.BalanceSupplier_Company = balanceSupplier;
//            balanceSupplier.ID = sender;
//            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_Portaal_Mutation pm = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope_PC_PMP_Portaal_Mutation();
//            pc_pmp.Portaal_Mutation = pm;
//            pm.ExternalReference = dr["Aansluiting_ID"].ToString();

//            //pm.ExternalReference = (DateTime)dr["Contract_Start_DT"];
//            //nl.Energie.EDSN.NoticeEos.NoticeEOSNotificationEnvelope_PC_PMP_MPCC_GridContractParty gridContact = new nl.Energie.EDSN.NoticeEos.NoticeEOSNotificationEnvelope_PC_PMP_MPCC_GridContractParty();
//            //mpcc.GridContractParty = gridContact;
//            //gridContact.Surname = dr["account_KorteNaam"].ToString();
//            //gridContact.Contact = new nl.Energie.EDSN.NoticeEos.NoticeEOSNotificationEnvelope_BoundAddressType();
//            //gridContact.Contact.StreetName = dr["Transportkosten_Straat"].ToString();
//            //gridContact.Contact.BuildingNr = dr["Transportkosten_Huisnummer"].ToString();
//            //gridContact.Contact.ZIPCode = dr["Transportkosten_Postcode"].ToString();
//            //gridContact.Contact.CityName = dr["Transportkosten_Plaats"].ToString();
//            //gridContact.Contact.Country = "NL";


//            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethod ChangeOfAllocationMethod = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethod();


//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            ChangeOfAllocationMethod.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));

//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //EndOfSupply.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderEndOfSupplyRespondingActivity";
//            ChangeOfAllocationMethod.Url = KC.CarUrl + @"synchroon/ResponderChangeOfAllocationMethodRespondingActivity";

//            ChangeOfAllocationMethod.Timeout = 120000;

//            nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope retour = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope();


//            string BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//            XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodRequestEnvelope));
//            TextWriter WriteFileStream = new StreamWriter(path + @"ChangeOfAllocation" + BestandsAanvulling + ".xml");
//            serializer.Serialize(WriteFileStream, enveloppe);
//            WriteFileStream.Close();

//            //string ftpResponse = "";
//            //if (FTPClass.FtpSendFile("ftp://62.148.191.136/" + @"EndOfSupply" + BestandsAanvulling + ".xml", "robin", "Wu!69Z#", path + @"EndOfSupply" + BestandsAanvulling + ".xml", out ftpResponse) == false)
//            //{
//            //    //MessageBox.Show("Fout bij verzenden naar Dentit " + ftpResponse); 
//            //}


//            StringWriter swXML = new StringWriter();
//            serializer.Serialize(swXML, enveloppe);
//            //MessageBox.Show(swXML.ToString());
//            int intOutBoxID = 0; // Save_Outbox(header.DocumentID, dr["Ean18_code"].ToString(), "NoticeEOS", swXML.ToString());


//            try
//            {

//                BestandsAanvulling = DateTime.Now.ToString("MMddyy Hmmssffff");
//                { BestandsAanvulling = " LV " + BestandsAanvulling; }


//                //XmlDocument _Doc = new XmlDocument();
//                //_Doc.Load(path + @"EndOfSupplyResponse LV 120914 215836.xml");
//                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope));
//                //retour = (nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));



//                retour = ChangeOfAllocationMethod.CallChangeOfAllocationMethod(enveloppe);

//                nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_PC portaalResponse = retour.Portaal_Content;

//                blnOK = true;


//                if (portaalResponse.Item.GetType() == typeof(nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_PC_PR))
//                {
//                    nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_PC_PR rejection = (nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_PC_PR)portaalResponse.Item;
//                    nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_RejectionPortaalType rejectionType = (nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_RejectionPortaalType)rejection.Rejection[0];
//                    strError_Message = rejectionType.RejectionText;
//                    blnOK = false;
//                }
//                else
//                {
//                    nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_PC_PMP portaalPMP = new nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_PC_PMP();
//                    portaalPMP = (nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope_PC_PMP)portaalResponse.Item;


//                    SqlConnection cnPubs = new SqlConnection(ConnString);
//                    string SQLstatement;

//                    cnPubs.Open();
//                    SQLstatement =
//                            "INSERT INTO [Messages].[dbo].[SMAChange] ([EAN18_Code],[Datum],[SlimmeMeterAllocatie],[DossierID]) " +
//                            "VALUES(@EAN18_Code, @Datum, @SlimmeMeterAllocatie,@DossierID)";


//                    SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
//                    cmdSaveInbox.Parameters.AddWithValue("@EAN18_Code", dr["Ean18_code"].ToString());
//                    cmdSaveInbox.Parameters.AddWithValue("@Datum", (DateTime)dr["Contract_Start_DT"]);
//                    cmdSaveInbox.Parameters.AddWithValue("@SlimmeMeterAllocatie", (bool)dr["SlimmeMeterAllocatie"]);
//                    cmdSaveInbox.Parameters.AddWithValue("@DossierID", portaalPMP.Portaal_Mutation.Dossier.ID);
//                    cmdSaveInbox.ExecuteNonQuery();
//                    cnPubs.Close();

//                }


//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.ChangeOfAllocationMethod.ChangeOfAllocationMethodResponseEnvelope));
//                WriteFileStream = new StreamWriter(path + @"ChangeOfAllocation" + BestandsAanvulling + ".xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();


//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                TextReader tr = new StringReader(ex.Detail.InnerXml);
//                SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                WriteLog("Fout bij EndOfSupply Notice:  " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID);
//                if (Klant_Config != "")
//                {
//                    strError_Message = "Fout bij EndOfSupply Notice : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                        " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
//                }
//                else
//                {
//                    WriteEnrollmentLog(S.ErrorCode.ToString() + " - " + S.ErrorDetails + " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), dr["ProductType"].ToString(),
//                    "EOSNotice", "", EnrollmentID, dr["Ean18_code"].ToString());
//                }
//                //MessageBox.Show(S.ErrorCode.ToString());
//                //MessageBox.Show(S.ErrorDetails);
//                //MessageBox.Show(S.ErrorText);
//                //MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }
//            catch (WebException exception)
//            {
//                WriteLog("Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }
//            catch (Exception exception)
//            {
//                WriteLog("Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                strError_Message = "Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }


//            return blnOK;
//        }

//        public Boolean EOSNoticeFromRequestXML(string strRequestFile)
//        {
//            Boolean blnOK = false;

//            nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope enveloppe = new nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope();



//            nl.Energie.EDSN.NoticeEOS.NoticeEOS NoticeEOS = new nl.Energie.EDSN.NoticeEOS.NoticeEOS();

//            //String certPath = certpath + @"EDSN2013053100006.p12";
//            NoticeEOS.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(certLV, certLVPassword));

//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

//            //EndOfSupply.Url = @"https://portaal-fatn.edsn.nl/b2b/synchroon/ResponderEndOfSupplyRespondingActivity";
//            NoticeEOS.Url = KC.CarUrl + @"synchroon/ResponderNoticeEOSRespondingActivity";

//            NoticeEOS.Timeout = 120000;

//            nl.Energie.EDSN.NoticeEOS.NoticeEOSAcknowledgementEnvelope retour = new nl.Energie.EDSN.NoticeEOS.NoticeEOSAcknowledgementEnvelope();

//            try
//            {

//                XmlSerializer serializer = new XmlSerializer(typeof(nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope));
//                XmlDocument _Doc = new XmlDocument();
//                _Doc.Load(strRequestFile);
//                //nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope result = new nl.Energie.EDSN.EndOfSupply.EndOfSupplyResponseEnvelope();
//                //serializer = new XmlSerializer(typeof(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope));
//                enveloppe = (nl.Energie.EDSN.NoticeEOS.NoticeEOSNotificationEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

//                retour = NoticeEOS.NoticeEOSNotification(enveloppe);
//                nl.Energie.EDSN.NoticeEOS.NoticeEOSAcknowledgementEnvelope_PC portaalResponse = retour.Portaal_Content;

//                blnOK = true;

//                serializer = new XmlSerializer(typeof(nl.Energie.EDSN.NoticeEOS.NoticeEOSAcknowledgementEnvelope));
//                StreamWriter WriteFileStream = new StreamWriter(path + @"result.xml");
//                serializer.Serialize(WriteFileStream, retour);
//                WriteFileStream.Close();


//            }
//            catch (System.Web.Services.Protocols.SoapException ex)
//            {

//                //XmlSerializer FaultSerializer = new XmlSerializer(typeof(SOAPFault));
//                //TextReader tr = new StringReader(ex.Detail.InnerXml);
//                //SOAPFault S = (SOAPFault)FaultSerializer.Deserialize(tr);

//                //WriteLog("Fout bij EndOfSupply Notice:  " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                //    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString(), 10, intOutBoxID);
//                //strError_Message = "Fout bij EndOfSupply Notice : + " + dr["Ean18_code"].ToString() + " - " + S.ErrorCode.ToString() + " - " + S.ErrorDetails +
//                //    " - " + S.ErrorText + " - " + ex.Detail.InnerXml.ToString();
//                //MessageBox.Show(S.ErrorCode.ToString());
//                //MessageBox.Show(S.ErrorDetails);
//                //MessageBox.Show(S.ErrorText);
//                //MessageBox.Show(ex.Detail.InnerXml.ToString());
//            }
//            catch (WebException exception)
//            {
//                //WriteLog("Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                //strError_Message = "Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }
//            catch (Exception exception)
//            {
//                //WriteLog("Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message, 10, intOutBoxID);
//                //strError_Message = "Fout bij EndOfSupply : + " + dr["Ean18_code"].ToString() + " - " + exception.Message;
//                //MessageBox.Show(exception.Message);
//            }


//            return blnOK;
//        }

//        private void Sent(string Ean18_Nummer, string controlref, string berichtType )
//        {
//            string messageBackupTo = "Marcel.Drost@Energie.nl";
//            string subject = "Switch Bericht type " + berichtType+ " voor EAN " + Ean18_Nummer + " op datum " + controlref;
//            string body = "Backup sent naar adres van Energie van het Switchbericht";
//            MailMessage message = new MailMessage();
//            message.Body = body;
//            message.To.Add(messageBackupTo);
//            message.Subject = subject;
//            MailAddress messageFrom = new MailAddress("Marcel.Drost@Energie.nl");
//            message.From = messageFrom;
//            MemoryStream stream = new MemoryStream(UTF32Encoding.Default.GetBytes(switchBericht.ToString()));
//            stream.Position = 0;
//            DateTime fileNameDate;
//            fileNameDate = DateTime.Now;
//            string fileDate;
//            fileDate = fileNameDate.ToString("yyyyMMddHHmm");
//            Attachment messageAttachment2 = new Attachment(stream, "Switchbericht" + fileDate + ".txt");
//            message.Attachments.Add(messageAttachment2);
//            SmtpClient client = new SmtpClient("mars");
//            client.SendAsync(message, null);
//        }

//        private int Save_392(string messageID , string ean18_Nummer , string berichtCode, int test)
//        {
//            SqlConnection cnPubs = new SqlConnection(ConnString);
//            string SQLstatement;
//            int outbox392_432MessageID = 0;

//            cnPubs.Open();
//            SQLstatement =
//                    "INSERT INTO [Messages].[dbo].[Outbox] " +
//                    "([Message_id] " +
//                    ",[EdineMessagetype_id] " +
//                    ",[Bericht] " +
//                    ",[Omschrijving] " +
//                    ",[Verzonden] " +
//                    ",[EdineMailadres] " +
//                    ",[BerichtStatus] " +
//                    ",[Afzender] "+
//                    ",[Outboxtype_ID]) " +
//                    // "OUTPUT @outboxID = inserted.ID " +
//                    "VALUES " +
//                    "(@id " +
//                    ",@Messagetype_id  " +  //11 = UTILMD 392
//                    ",@switchBericht " +
//                    ",@omschrijving " +
//                    ",null  " +
//                    ",@EmailAdres " +
//                    ",'TE_VERSTUREN' ";
//                    if (test == 1)
//                    {
//                    SQLstatement = SQLstatement + ",'" + HoofdPV + "@cps.testfac.tennet',1); SELECT @outboxID = SCOPE_IDENTITY();";
//                    }
//                    else
//                    {
//                    SQLstatement = SQLstatement + ",'" + HoofdPV + "cps.tennet.org',1); SELECT @outboxID = SCOPE_IDENTITY();";
//                    }


//            SqlCommand cmdSaveOutbox = new SqlCommand(SQLstatement, cnPubs);
//            cmdSaveOutbox.Parameters.Add(new SqlParameter("@id", SqlDbType.VarChar, 14));
//            cmdSaveOutbox.Parameters["@id"].Value = messageID;
//            cmdSaveOutbox.Parameters.Add(new SqlParameter("@switchBericht", SqlDbType.NText));
//            cmdSaveOutbox.Parameters["@switchBericht"].Value = switchBericht.ToString();
//            cmdSaveOutbox.Parameters.Add(new SqlParameter("@omschrijving", SqlDbType.NText));
//            cmdSaveOutbox.Parameters["@omschrijving"].Value = "SwitchEAN_" + ean18_Nummer;
//            cmdSaveOutbox.Parameters.Add(new SqlParameter("@EmailAdres", SqlDbType.NText));
//            if (test == 1)
//            {
//                cmdSaveOutbox.Parameters["@EmailAdres"].Value = "8716867222234@cps.testfac.tennet";//eMailAdres;
//            }
//            else
//            {
//                cmdSaveOutbox.Parameters["@EmailAdres"].Value = "8712423010208@cps.tennet.org";//eMailAdres;
//            }
//            cmdSaveOutbox.Parameters.Add(new SqlParameter("@Messagetype_id", SqlDbType.Int));
//            if (berichtCode == "392")
//            {
//                cmdSaveOutbox.Parameters["@Messagetype_id"].Value = "11";// 11         
//            }
//            else if (berichtCode == "432")
//            {
//                cmdSaveOutbox.Parameters["@Messagetype_id"].Value = "14";// 11         
//            }
//            cmdSaveOutbox.Parameters.Add(new SqlParameter("@outboxID", SqlDbType.Int));
//            cmdSaveOutbox.Parameters["@outboxID"].Direction = ParameterDirection.Output;
//            try
//            {
//                cmdSaveOutbox.ExecuteNonQuery();
//                outbox392_432MessageID = (int)cmdSaveOutbox.Parameters["@outboxID"].Value;
//                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
//            }
//            catch (Exception ex)
//            {
//                //MessageBox.Show("Er is iets fout gegaan met het bewaren van het Switchbericht Electra, we adviseren U contact op te nemen met IT");
//                //MessageBox.Show(ex.ToString());
//                WriteLog("Er is iets fout gegaan met het bewaren van het Switchbericht Electra : + " + ean18_Nummer + " - " + ex.Message, 10, -1);
//            }

//            //updating switchbericht met Outbox ID waar 392 of 432 instaat
//            SQLstatement =
//                    "update [EnergieDB].[dbo].[Switchberichten] set Bericht_392_432_Outbox_ID = @outboxID " +
//                    "where aansluiting_ID = @aansluitingID and Bericht_392_432_Outbox_ID = 0";

//            SqlCommand cmdSave392 = new SqlCommand(SQLstatement, cnPubs);
//            cmdSave392.Parameters.Add(new SqlParameter("@aansluitingID", SqlDbType.VarChar, 14));
//            cmdSave392.Parameters["@aansluitingID"].Value = aansluitingID;
//            cmdSave392.Parameters.Add(new SqlParameter("@outboxID", SqlDbType.Int));
//            cmdSave392.Parameters["@outboxID"].Value = outbox392_432MessageID;
//            cmdSave392.ExecuteNonQuery();
//            cnPubs.Close();

//            return outbox392_432MessageID;

//        }

//        private int Save_Outbox(string messageID, string ean18_Nummer, string berichtCode, string switchBericht)
//        {
//            SqlConnection cnPubs = new SqlConnection(ConnString);
//            string SQLstatement;
//            int outboxMessageID = -1;

//            cnPubs.Open();
//            SQLstatement =
//                    "INSERT INTO [Messages].[dbo].[Outbox] " +
//                    "([Message_id] " +
//                    ",[EdineMessagetype_id] " +
//                    ",[Bericht] " +
//                    ",[Omschrijving] " +
//                    ",[Verzonden] " +
//                    ",[EdineMailadres] " +
//                    ",[BerichtStatus] " +
//                    ",[Afzender] " +
//                    ",[Outboxtype_ID]) " +
//                // "OUTPUT @outboxID = inserted.ID " +
//                    "VALUES " +
//                    "(@id " +
//                    ",@Messagetype_id  " +  //11 = UTILMD 392
//                    ",@switchBericht " +
//                    ",@omschrijving " +
//                    ",getdate() " + 
//                    ",'Energie' " + 
//                    ",'VERSTUURD' " +
//                    ",@EmailAdres " +
//                    ",1); SELECT @outboxID = SCOPE_IDENTITY();";



//            SqlCommand cmdSaveOutbox = new SqlCommand(SQLstatement, cnPubs);
//            cmdSaveOutbox.Parameters.Add(new SqlParameter("@id", SqlDbType.VarChar, 14));
//            cmdSaveOutbox.Parameters["@id"].Value = messageID;
//            cmdSaveOutbox.Parameters.Add(new SqlParameter("@switchBericht", SqlDbType.NText));
//            cmdSaveOutbox.Parameters["@switchBericht"].Value = switchBericht;
//            cmdSaveOutbox.Parameters.Add(new SqlParameter("@omschrijving", SqlDbType.NText));
//            cmdSaveOutbox.Parameters["@omschrijving"].Value = "SwitchEAN_" + ean18_Nummer;
//            cmdSaveOutbox.Parameters.Add(new SqlParameter("@EmailAdres", SqlDbType.NText));

//            cmdSaveOutbox.Parameters["@EmailAdres"].Value = "WebService";//eMailAdres;

//            cmdSaveOutbox.Parameters.Add(new SqlParameter("@Messagetype_id", SqlDbType.Int));
//            if (berichtCode == "MoveIn")
//            {
//                cmdSaveOutbox.Parameters["@Messagetype_id"].Value = "11";
//            }
//            if (berichtCode == "MoveOut")
//            {
//                cmdSaveOutbox.Parameters["@Messagetype_id"].Value = "13";        
//            }
//            if (berichtCode == "EndOfSupply")
//            {
//                cmdSaveOutbox.Parameters["@Messagetype_id"].Value = "13";         
//            }
//            if (berichtCode == "ChangeOfPV")
//            {
//                cmdSaveOutbox.Parameters["@Messagetype_id"].Value = "11";         
//            }
//            else if (berichtCode == "") //????
//            {
//                cmdSaveOutbox.Parameters["@Messagetype_id"].Value = "14";         
//            }
//            cmdSaveOutbox.Parameters.Add(new SqlParameter("@outboxID", SqlDbType.Int));
//            cmdSaveOutbox.Parameters["@outboxID"].Direction = ParameterDirection.Output;
//            try
//            {
//                cmdSaveOutbox.ExecuteNonQuery();
//                outboxMessageID = (int)cmdSaveOutbox.Parameters["@outboxID"].Value;
//                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
//            }
//            catch (Exception ex)
//            {

//                //MessageBox.Show("Er is iets fout gegaan met het bewaren van het Switchbericht Electra (Outbox), we adviseren U contact op te nemen met IT");
//                //MessageBox.Show(ex.ToString());
//                WriteLog("Er is iets fout gegaan met het bewaren van het Switchbericht Electra : + " + ean18_Nummer + " - " + ex.Message, 10, -1);
//            }



//            return outboxMessageID;

//        }

//        public  int Save_Inbox(int edineMessagetype_ID, string message, string subject)
//        {
//            SqlConnection cnPubs = new SqlConnection(ConnString);
//            string SQLstatement;
//            int inboxID = -1;

//            cnPubs.Open();
//            SQLstatement =
//                    "INSERT INTO [Messages].[dbo].[Inbox] " +
//                    "([Ontvangen] " +
//                    ",[EdineMessagetype_ID] " +
//                    ",[Edine_Message] " +
//                    ",[Processed] " +
//                    ",[UID] " +
//                    ",[errors] " +
//                    ",[Subject] " +
//                    ",[ToEnergieDB]) " +
//                    "VALUES " +
//                    "(GetDate() " +
//                    ",@EdineMessagetype_ID " +
//                    ",@Edine_Message " +
//                    ",1 " +
//                    ",convert(varchar, getdate(), 126) " +
//                    ",0 " +
//                    ",@Subject " +
//                    ",1); SELECT @inboxID = SCOPE_IDENTITY();";


//            SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
//            cmdSaveInbox.Parameters.Add(new SqlParameter("@EdineMessagetype_ID", SqlDbType.Int));
//            cmdSaveInbox.Parameters["@EdineMessagetype_ID"].Value = edineMessagetype_ID;
//            cmdSaveInbox.Parameters.Add(new SqlParameter("@Edine_Message", SqlDbType.NText));
//            cmdSaveInbox.Parameters["@Edine_Message"].Value = message;
//            cmdSaveInbox.Parameters.Add(new SqlParameter("@Subject", SqlDbType.VarChar));
//            cmdSaveInbox.Parameters["@Subject"].Value = subject;

//            cmdSaveInbox.Parameters.Add(new SqlParameter("@inboxID", SqlDbType.Int));
//            cmdSaveInbox.Parameters["@inboxID"].Direction = ParameterDirection.Output;
//            try
//            {
//                cmdSaveInbox.ExecuteNonQuery();
//                inboxID = (int)cmdSaveInbox.Parameters["@inboxID"].Value;
//                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
//            }
//            catch (Exception ex)
//            {
//                //MessageBox.Show("Er is iets fout gegaan met het bewaren van het Switchbericht Electra (inbox), we adviseren U contact op te nemen met IT");
//                //MessageBox.Show(ex.ToString());
//                WriteLog("Er is iets fout gegaan met het bewaren van het Switchbericht Electra inbox : + " + subject + " - " + ex.Message, 10, -1);
//            }
//            cnPubs.Close();
//            return inboxID;

//        }
//        public int Save_Edine(int inboxID, string unb_Sender_ID, string unb_Recipient_ID, DateTime unb_Date, string messageNumber, string unh_Message_ID_Type,
//            string bgm_Document_Message_Name)
//        {
//            SqlConnection cnPubs = new SqlConnection(ConnString);
//            string SQLstatement;
//            int edineID = -1;

//            cnPubs.Open();
//            SQLstatement =
//                    "INSERT INTO [Messages].[dbo].[Edine] " +
//                    "([Inbox_ID] " +
//                    ",[Processed] " +
//                    ",[UNB_Syntax_ID] " +
//                    ",[UNB_Syntax_version] " +
//                    ",[UNB_Sender_ID] " +
//                    ",[UNB_Sender_Partner_ID] " +
//                    ",[UNB_Sender_Address] " +
//                    ",[UNB_Recipient_ID] " +
//                    ",[UNB_Recipient_Partner] " +
//                    ",[UNB_Recipient_Address] " +
//                    ",[UNB_Date] " +
//                    ",[UNB_Control_Reference] " +
//                    ",[UNH_Message_Reference] " +
//                    ",[UNH_Message_ID_Type] " +
//                    ",[UNH_Message_ID_Type_Version] " +
//                    ",[UNH_Message_ID_Type_Release] " +
//                    ",[UNH_Message_ID_Agency] " +
//                    ",[UNH_Message_ID_Assigned] " +
//                    ",[BGM_Document_Message_Name] " +
//                    ",[BGM_Document_Message_Number] " +
//                    ",[BGM_Message_Function] " +
//                    ",[BGM_Response_Type] " +
//                    ",[UNT_Segment] " +
//                    ",[UNT_Reference] " +
//                    ",[UNZ_Count] " +
//                    ",[UNZ_reference]) " +
//                    "VALUES " +
//                    "(@Inbox_ID " +
//                    ",1 " +
//                    ",'UNOC' " +
//                    ",'3' " +
//                    ",@UNB_Sender_ID " +
//                    ",'14' " +
//                    ",'' " +
//                    ",@UNB_Recipient_ID " +
//                    ",'14' " +
//                    ",'' " +
//                    ",@UNB_Date " +
//                    ",@MessageNumber " +
//                    ",@MessageNumber " +
//                    ",@UNH_Message_ID_Type " +
//                    ",'D' " +
//                    ",'01C' " +
//                    ",'UN' " +
//                    ",'E4NL20' " +
//                    ",@BGM_Document_Message_Name " +
//                    ",@MessageNumber " +
//                    ",'9' " +
//                    ",'AB' " +
//                    ",'1' " +
//                    ",@MessageNumber " +
//                    ",'1' " +
//                    ",@MessageNumber); SELECT @edineID = SCOPE_IDENTITY();";


//            SqlCommand cmdSaveEdine = new SqlCommand(SQLstatement, cnPubs);
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@Inbox_ID", SqlDbType.Int));
//            cmdSaveEdine.Parameters["@Inbox_ID"].Value = inboxID;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@UNB_Sender_ID", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@UNB_Sender_ID"].Value = unb_Sender_ID;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@UNB_Recipient_ID", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@UNB_Recipient_ID"].Value = unb_Recipient_ID;
//            //cmdSaveEdine.Parameters.Add(new SqlParameter("@UNB_Recipient_ID", SqlDbType.VarChar));
//            //cmdSaveEdine.Parameters["@UNB_Recipient_ID"].Value = unb_Recipient_ID;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@UNB_Date", SqlDbType.DateTime));
//            cmdSaveEdine.Parameters["@UNB_Date"].Value = unb_Date;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@MessageNumber", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@MessageNumber"].Value = messageNumber;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@UNH_Message_ID_Type", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@UNH_Message_ID_Type"].Value = unh_Message_ID_Type;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@BGM_Document_Message_Name", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@BGM_Document_Message_Name"].Value = bgm_Document_Message_Name;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@edineID", SqlDbType.Int));
//            cmdSaveEdine.Parameters["@edineID"].Direction = ParameterDirection.Output;
//            try
//            {
//                cmdSaveEdine.ExecuteNonQuery();
//                edineID = (int)cmdSaveEdine.Parameters["@edineID"].Value;
//                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
//            }
//            catch (Exception ex)
//            {
//                //MessageBox.Show("Er is iets fout gegaan met het bewaren van het Switchbericht Electra (edine), we adviseren U contact op te nemen met IT");
//                //MessageBox.Show(ex.ToString());
//                WriteLog("Er is iets fout gegaan met het bewaren van het Switchbericht Electra edine : + " + inboxID + " - " + ex.Message, 10, -1);
//            }
//            cnPubs.Close();
//            return edineID;

//        }

//        private void Save_414_Header(int edineID, DateTime messageProcessed, string ms, string mr)
//        {
//            SqlConnection cnPubs = new SqlConnection(ConnString);
//            string SQLstatement;

//            cnPubs.Open();
//            SQLstatement =
//                    "INSERT INTO [Messages].[dbo].[UTILMD_414_Header] " +
//                    "([EdineId] " +
//                    ",[MessageProcessed] " +
//                    ",[MS] " +
//                    ",[MR]) " +
//                    "VALUES " +
//                    "(@EdineId " +
//                    ",@MessageProcessed " +
//                    ",@MS " +
//                    ",@MR)";


//            SqlCommand cmdSaveEdine = new SqlCommand(SQLstatement, cnPubs);
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@EdineId", SqlDbType.Int));
//            cmdSaveEdine.Parameters["@EdineId"].Value = edineID;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@MessageProcessed", SqlDbType.DateTime));
//            cmdSaveEdine.Parameters["@MessageProcessed"].Value = messageProcessed;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@MS", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@MS"].Value = ms;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@MR", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@MR"].Value = mr;

//            try
//            {
//                cmdSaveEdine.ExecuteNonQuery();
//                //edineID = (int)cmdSaveEdine.Parameters["@edineID"].Value;
//                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
//            }
//            catch (Exception ex)
//            {
//                //MessageBox.Show("Er is iets fout gegaan met het bewaren van het Switchbericht Electra (414_Header), we adviseren U contact op te nemen met IT");
//                //MessageBox.Show(ex.ToString());
//                WriteLog("Er is iets fout gegaan met het bewaren van het Switchbericht Electra 414_Header : + " + edineID + " - " + ex.Message, 10, -1);
//            }
//            cnPubs.Close();
//        }



//        private void Save_414(int edineID, string transactionId, string loc_GC, DateTime dtmUTCContractStart, string status, string brp,
//                string nameIV, string nameCustomer, string address, string city, string postalCode, string country, string tax,
//                string leveranciersModel, string statusDescription, string DossierID, string EnrollmentID)
//        {
//            SqlConnection cnPubs = new SqlConnection(ConnString);
//            string SQLstatement;

//            cnPubs.Open();
//            SQLstatement =
//                    "INSERT INTO [Messages].[dbo].[UTILMD_414] " +
//                    "([EdineId] " +
//                    ",[TransactionId] " +
//                    ",[LOC_GC] " +
//                    ",[dtmUTCContractStart] " +
//                    ",[StatusCategory] " +
//                    ",[Status] " +
//                    ",[BRP] " +
//                    ",[NameIV] " +
//                    ",[NameCustomer] " +
//                    ",[Address] " +
//                    ",[City] " +
//                    ",[PostalCode] " +
//                    ",[Country] " +
//                    ",[TAX] " +
//                    ",[LeveranciersModel] " +
//                    ",[StatusDescription] " +
//                    ",[DossierID] " +
//                    ",[EnrollmentID]) " +
//                    "VALUES " +
//                    "(@EdineId " +
//                    ",@TransactionId " +
//                    ",@LOC_GC " +
//                    ",@dtmUTCContractStart " +
//                    ",'E01' " +
//                    ",@Status " +
//                    ",@BRP " +
//                    ",@NameIV " +
//                    ",@NameCustomer " +
//                    ",@Address " +
//                    ",@City " +
//                    ",@PostalCode " +
//                    ",@Country " +
//                    ",@TAX " +
//                    ",@LeveranciersModel " +
//                    ",@StatusDescription " +
//                    ",@DossierID " +
//                    ",@EnrollmentID)";


//            SqlCommand cmdSaveEdine = new SqlCommand(SQLstatement, cnPubs);
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@edineID", SqlDbType.Int));
//            cmdSaveEdine.Parameters["@edineID"].Value = edineID;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@TransactionId", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@TransactionId"].Value = transactionId;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@LOC_GC", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@LOC_GC"].Value = loc_GC;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@dtmUTCContractStart", SqlDbType.DateTime));
//            cmdSaveEdine.Parameters["@dtmUTCContractStart"].Value = dtmUTCContractStart;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@Status", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@Status"].Value = status;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@BRP", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@BRP"].Value = brp;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@NameIV", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@NameIV"].Value = nameIV;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@NameCustomer", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@NameCustomer"].Value = nameCustomer;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@Address", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@Address"].Value = address;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@City", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@City"].Value = city;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@PostalCode", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@PostalCode"].Value = postalCode;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@Country", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@Country"].Value = country;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@TAX", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@TAX"].Value = country;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@LeveranciersModel", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@LeveranciersModel"].Value = leveranciersModel;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@StatusDescription", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@StatusDescription"].Value = statusDescription;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@DossierID", SqlDbType.NVarChar));
//            cmdSaveEdine.Parameters["@DossierID"].Value = DossierID;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@EnrollmentID", SqlDbType.NVarChar));
//            cmdSaveEdine.Parameters["@EnrollmentID"].Value = EnrollmentID;

//            try
//            {
//                cmdSaveEdine.ExecuteNonQuery();
//            }
//            catch (Exception ex)
//            {
//                //MessageBox.Show("Er is iets fout gegaan met het bewaren van het Switchbericht Electra (414), we adviseren U contact op te nemen met IT");
//                //MessageBox.Show(ex.ToString());
//                WriteLog("Er is iets fout gegaan met het bewaren van het Switchbericht Electra 414 : + " + edineID + " - " + ex.Message, 10, -1);
//            }
//            cnPubs.Close();
//        }

//        private void Save_406_Header(int edineID, DateTime messageProcessed, string ms, string mr)
//        {
//            SqlConnection cnPubs = new SqlConnection(ConnString);
//            string SQLstatement;

//            cnPubs.Open();
//            SQLstatement =
//                    "INSERT INTO [Messages].[dbo].[UTILMD_406_Header] " +
//                    "([EdineId] " +
//                    ",[MessageProcessed] " +
//                    ",[MS] " +
//                    ",[MR]) " +
//                    "VALUES " +
//                    "(@EdineId " +
//                    ",@MessageProcessed " +
//                    ",@MS " +
//                    ",@MR)";


//            SqlCommand cmdSaveEdine = new SqlCommand(SQLstatement, cnPubs);
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@EdineId", SqlDbType.Int));
//            cmdSaveEdine.Parameters["@EdineId"].Value = edineID;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@MessageProcessed", SqlDbType.DateTime));
//            cmdSaveEdine.Parameters["@MessageProcessed"].Value = messageProcessed;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@MS", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@MS"].Value = ms;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@MR", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@MR"].Value = mr;

//            try
//            {
//                cmdSaveEdine.ExecuteNonQuery();
//                //edineID = (int)cmdSaveEdine.Parameters["@edineID"].Value;
//                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
//            }
//            catch (Exception ex)
//            {
//                //MessageBox.Show("Er is iets fout gegaan met het bewaren van het Switchbericht Electra (406_Header), we adviseren U contact op te nemen met IT");
//                //MessageBox.Show(ex.ToString());
//                WriteLog("Er is iets fout gegaan met het bewaren van het Switchbericht Electra 406_Header : + " + edineID + " - " + ex.Message, 10, -1);
//            }
//            cnPubs.Close();
//        }

//        private void Save_406(int edineID, string transactionId, string loc_GC, DateTime dtmUTCContractStart, string DossierID, 
//            string EnrollmentID, string NewBalanceSupplier)
//        {
//            SqlConnection cnPubs = new SqlConnection(ConnString);
//            string SQLstatement;

//            cnPubs.Open();
//            SQLstatement =
//                    "INSERT INTO [Messages].[dbo].[UTILMD_406] " +
//                    "([EdineId] " +
//                    ",[TransactionId] " +
//                    ",[LOC_GC] " +
//                    ",[dtmUTCContractEnd] " +
//                    ",[StatusCategory] " +
//                    ",[Status] " +
//                    ",[DossierID] " +
//                    ",[EnrollmentID] " +
//                    ",[NewBalanceSupplier])" +
//                    "VALUES " +
//                    "(@EdineId " +
//                    ",@TransactionId " +
//                    ",@LOC_GC " +
//                    ",@dtmUTCContractEnd " +
//                    ",'E01' " +
//                    ",'E01' " +
//                    ",@DossierID " +
//                    ",@EnrollmentID " +
//                    ",@NewBalanceSupplier)";


//            SqlCommand cmdSaveEdine = new SqlCommand(SQLstatement, cnPubs);
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@EdineId", SqlDbType.Int));
//            cmdSaveEdine.Parameters["@EdineId"].Value = edineID;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@TransactionId", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@TransactionId"].Value = transactionId;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@LOC_GC", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@LOC_GC"].Value = loc_GC;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@dtmUTCContractEnd", SqlDbType.DateTime));
//            cmdSaveEdine.Parameters["@dtmUTCContractEnd"].Value = dtmUTCContractStart;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@DossierID", SqlDbType.NVarChar));
//            cmdSaveEdine.Parameters["@DossierID"].Value = DossierID;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@EnrollmentID", SqlDbType.NVarChar));
//            cmdSaveEdine.Parameters["@EnrollmentID"].Value = EnrollmentID;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@NewBalanceSupplier", SqlDbType.NVarChar));
//            cmdSaveEdine.Parameters["@NewBalanceSupplier"].Value = NewBalanceSupplier;


//            try
//            {
//                cmdSaveEdine.ExecuteNonQuery();
//                //edineID = (int)cmdSaveEdine.Parameters["@edineID"].Value;
//                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
//            }
//            catch (Exception ex)
//            {
//                //MessageBox.Show("Er is iets fout gegaan met het bewaren van het Switchbericht Electra (406_Header), we adviseren U contact op te nemen met IT");
//                //MessageBox.Show(ex.ToString());
//                WriteLog("Er is iets fout gegaan met het bewaren van het Switchbericht Electra 406 : + " + edineID + " - " + ex.Message, 10, -1);
//            }
//            cnPubs.Close();
//        }

//        private void Save_E09_Header(int edineID, DateTime messageProcessed, string ms, string mr)
//        {
//            SqlConnection cnPubs = new SqlConnection(ConnString);
//            string SQLstatement;

//            cnPubs.Open();
//            SQLstatement =
//                    "INSERT INTO [Messages].[dbo].[UTILMD_E09_Header] " +
//                    "([EdineId] " +
//                    ",[MessageProcessed] " +
//                    ",[MS] " +
//                    ",[MR]) " +
//                    "VALUES " +
//                    "(@EdineId " +
//                    ",@MessageProcessed " +
//                    ",@MS " +
//                    ",@MR)";


//            SqlCommand cmdSaveEdine = new SqlCommand(SQLstatement, cnPubs);
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@EdineId", SqlDbType.Int));
//            cmdSaveEdine.Parameters["@EdineId"].Value = edineID;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@MessageProcessed", SqlDbType.DateTime));
//            cmdSaveEdine.Parameters["@MessageProcessed"].Value = messageProcessed;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@MS", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@MS"].Value = ms;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@MR", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@MR"].Value = mr;

//            try
//            {
//                cmdSaveEdine.ExecuteNonQuery();
//            }
//            catch (Exception ex)
//            {
//                //MessageBox.Show("Er is iets fout gegaan met het bewaren van het Switchbericht Electra (E09_Header), we adviseren U contact op te nemen met IT");
//                //MessageBox.Show(ex.ToString());
//                WriteLog("Er is iets fout gegaan met het bewaren van het Switchbericht Electra E09_Header : + " + edineID + " - " + ex.Message, 10, -1);
//            }
//            cnPubs.Close();
//        }
//        private void Save_E09(int edineID, string transactionId, string loc_GC, string gridArea, DateTime dtmUTCContractStart, 
//                string statusCategory, string statusReason, string dossierID)
//        {
//            SqlConnection cnPubs = new SqlConnection(ConnString);
//            string SQLstatement;

//            cnPubs.Open();
//            SQLstatement =
//                    "INSERT INTO [Messages].[dbo].[UTILMD_E09] " +
//                    "([EdineId] " +
//                    ",[TransactionId] " +
//                    ",[LOC_GC] " +
//                    ",[GridArea] " +
//                    ",[dtmUTCContractStart] " +
//                    ",[StatusCategory] " +
//                    ",[StatusReason] " +
//                    ",[DossierID]) " +
//                    "VALUES " +
//                    "(@EdineId " +
//                    ",@TransactionId " +
//                    ",@LOC_GC " +
//                    ",@GridArea " +
//                    ",@dtmUTCContractStart " +
//                    ",@StatusCategory " +
//                    ",@StatusReason " +
//                    ",@DossierID)";


//            SqlCommand cmdSaveEdine = new SqlCommand(SQLstatement, cnPubs);
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@edineID", SqlDbType.Int));
//            cmdSaveEdine.Parameters["@edineID"].Value = edineID;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@TransactionId", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@TransactionId"].Value = transactionId;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@LOC_GC", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@LOC_GC"].Value = loc_GC;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@GridArea", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@GridArea"].Value = gridArea;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@dtmUTCContractStart", SqlDbType.DateTime));
//            cmdSaveEdine.Parameters["@dtmUTCContractStart"].Value = dtmUTCContractStart;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@StatusCategory", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@StatusCategory"].Value = statusCategory;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@StatusReason", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@StatusReason"].Value = statusReason;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@DossierID", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@DossierID"].Value = dossierID;

//            try
//            {
//                cmdSaveEdine.ExecuteNonQuery();
//            }
//            catch (Exception ex)
//            {
//                //MessageBox.Show("Er is iets fout gegaan met het bewaren van het Switchbericht Electra (E09), we adviseren U contact op te nemen met IT");
//                //MessageBox.Show(ex.ToString());
//                WriteLog("Er is iets fout gegaan met het bewaren van het Switchbericht Electra E09 : + " + edineID + " - " + ex.Message, 10, -1);
//            }
//            cnPubs.Close();
//        }

//        private void Save_E09_End(int edineID, string transactionId, string loc_GC, string gridArea, DateTime dtmUTCContractEnd,
//                string statusCategory, string statusReason)
//        {
//            SqlConnection cnPubs = new SqlConnection(ConnString);
//            string SQLstatement;

//            cnPubs.Open();
//            SQLstatement =
//                    "INSERT INTO [Messages].[dbo].[UTILMD_E09] " +
//                    "([EdineId] " +
//                    ",[TransactionId] " +
//                    ",[LOC_GC] " +
//                    ",[GridArea] " +
//                    ",[dtmUTCContractEnd] " +
//                    ",[StatusCategory] " +
//                    ",[StatusReason]) " +
//                    "VALUES " +
//                    "(@EdineId " +
//                    ",@TransactionId " +
//                    ",@LOC_GC " +
//                    ",@GridArea " +
//                    ",@dtmUTCContractEnd " +
//                    ",@StatusCategory " +
//                    ",@StatusReason)";


//            SqlCommand cmdSaveEdine = new SqlCommand(SQLstatement, cnPubs);
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@edineID", SqlDbType.Int));
//            cmdSaveEdine.Parameters["@edineID"].Value = edineID;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@TransactionId", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@TransactionId"].Value = transactionId;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@LOC_GC", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@LOC_GC"].Value = loc_GC;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@GridArea", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@GridArea"].Value = gridArea;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@dtmUTCContractEnd", SqlDbType.DateTime));
//            cmdSaveEdine.Parameters["@dtmUTCContractEnd"].Value = dtmUTCContractEnd;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@StatusCategory", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@StatusCategory"].Value = statusCategory;
//            cmdSaveEdine.Parameters.Add(new SqlParameter("@StatusReason", SqlDbType.VarChar));
//            cmdSaveEdine.Parameters["@StatusReason"].Value = statusReason;

//            try
//            {
//                cmdSaveEdine.ExecuteNonQuery();
//            }
//            catch (Exception ex)
//            {
//                //MessageBox.Show("Er is iets fout gegaan met het bewaren van het Switchbericht Electra (E09), we adviseren U contact op te nemen met IT");
//                //MessageBox.Show(ex.ToString());
//                WriteLog("Er is iets fout gegaan met het bewaren van het Switchbericht Electra E09_end : + " + edineID + " - " + ex.Message, 10, -1);
//            }
//            cnPubs.Close();
//        }
//        private void Save_Switch(int aansluitingID, int intOutboxID_switch, int intInboxID_414, int intInboxID_E09)
//        {
//            SqlConnection cnPubs = new SqlConnection(ConnString);
//            string SQLstatement;

//            //updating switchbericht met Outbox ID waar 392 of 432 instaat
//            cnPubs.Open();
//            SQLstatement =
//                    "update [EnergieDB].[dbo].[Switchberichten] set Bericht_392_432_Outbox_ID = @outboxID, Bericht_414_Inbox_ID=@InboxID_414, Bericht_E09_Inbox_ID=@Inbox_E09  " +
//                    "where aansluiting_ID = @aansluitingID and Bericht_392_432_Outbox_ID = 0";

//            SqlCommand cmdSave392 = new SqlCommand(SQLstatement, cnPubs);
//            cmdSave392.Parameters.Add(new SqlParameter("@aansluitingID", SqlDbType.VarChar, 14));
//            cmdSave392.Parameters["@aansluitingID"].Value = aansluitingID;
//            cmdSave392.Parameters.Add(new SqlParameter("@outboxID", SqlDbType.Int));
//            cmdSave392.Parameters["@outboxID"].Value = intOutboxID_switch;
//            cmdSave392.Parameters.Add(new SqlParameter("@InboxID_414", SqlDbType.Int));
//            cmdSave392.Parameters["@InboxID_414"].Value = intInboxID_414;
//            cmdSave392.Parameters.Add(new SqlParameter("@Inbox_E09", SqlDbType.Int));
//            cmdSave392.Parameters["@Inbox_E09"].Value = intInboxID_E09;
//            cmdSave392.ExecuteNonQuery();
//            cnPubs.Close();

//        }

//        private void Save_Switch_End(int aansluitingID, int intOutboxID_switch, int intInboxID_406, int intInboxID_E09)
//        {
//            SqlConnection cnPubs = new SqlConnection(ConnString);
//            string SQLstatement;

//            //updating switchbericht met Outbox ID waar 392 of 432 instaat
//            cnPubs.Open();
//            SQLstatement =
//                    "update [EnergieDB].[dbo].[Switchberichten] set Bericht_392_432_Outbox_ID = @outboxID, Bericht_406_Inbox_ID=@InboxID_406, Bericht_E09_Inbox_ID=@Inbox_E09  " +
//                    "where aansluiting_ID = @aansluitingID and Bericht_392_432_Outbox_ID = 0";

//            SqlCommand cmdSave392 = new SqlCommand(SQLstatement, cnPubs);
//            cmdSave392.Parameters.Add(new SqlParameter("@aansluitingID", SqlDbType.VarChar, 14));
//            cmdSave392.Parameters["@aansluitingID"].Value = aansluitingID;
//            cmdSave392.Parameters.Add(new SqlParameter("@outboxID", SqlDbType.Int));
//            cmdSave392.Parameters["@outboxID"].Value = intOutboxID_switch;
//            cmdSave392.Parameters.Add(new SqlParameter("@InboxID_406", SqlDbType.Int));
//            cmdSave392.Parameters["@InboxID_406"].Value = intInboxID_406;
//            cmdSave392.Parameters.Add(new SqlParameter("@Inbox_E09", SqlDbType.Int));
//            cmdSave392.Parameters["@Inbox_E09"].Value = intInboxID_E09;
//            cmdSave392.ExecuteNonQuery();
//            cnPubs.Close();

//        }

//        public void WriteLog(string description, int LevelID, int inbox_ID)
//        {
//            try
//            {
//                string strDescription = description;
//                if (strDescription.Length > 500) { strDescription = strDescription.Substring(0, 500); }

//                SqlConnection conn = new SqlConnection(ConnString);
//                conn.Open();
//                SqlCommand cmdLog = new SqlCommand();
//                cmdLog.Connection = conn;
//                string str_SQL = "insert into Messages.dbo.ApplicationLogs (TimeStmp, Description, SourceID, LevelID, Inbox_ID) values(@TimeStmp, @Description, " +
//                    " @SourceID, @LevelID, @Inbox_ID)";
//                cmdLog.CommandText = str_SQL;
//                cmdLog.Parameters.AddWithValue("@TimeStmp", DateTime.Now);
//                cmdLog.Parameters.AddWithValue("@Description", strDescription);
//                cmdLog.Parameters.AddWithValue("@SourceID", 4);
//                cmdLog.Parameters.AddWithValue("@LevelID", LevelID);
//                cmdLog.Parameters.AddWithValue("@Inbox_ID", inbox_ID);
//                cmdLog.ExecuteNonQuery();
//                conn.Close();
//            }
//            catch (Exception ex)
//            {
//                EventLog eventlog = new EventLog("Application");
//                eventlog.Source = "Energie App";
//                eventlog.WriteEntry("WriteLog : " + ex.Message, EventLogEntryType.Error, 0);
//            }
//        }

//        public void WriteEnrollmentLog(string description, string product, string switchType, string dossierID, string enrollmentID, string ean18_Code)
//        {
//            try
//            {
//                SqlConnection conn = new SqlConnection(ConnString);
//                conn.Open();
//                SqlCommand cmdLog = new SqlCommand();
//                cmdLog.Connection = conn;
//                string str_SQL = "INSERT INTO [Messages].[dbo].[EnrollmentLog] " +
//                    "([Product] " +
//                    ",[EAN18_Code] " +
//                    ",[SwitchType] " +
//                    ",[DossierID] " +
//                    ",[EnrollmentID] " +
//                    ",[RejectionText]) " +
//                    "VALUES " +
//                    "(@Product " +
//                    ", @EAN18_Code " +
//                    ", @SwitchType " +
//                    ", @DossierID " +
//                    ", @EnrollmentID " +
//                    ", @RejectionText)";
//                cmdLog.CommandText = str_SQL;
//                cmdLog.Parameters.AddWithValue("@Product", product);
//                cmdLog.Parameters.AddWithValue("@EAN18_Code", ean18_Code);
//                cmdLog.Parameters.AddWithValue("@SwitchType", switchType);
//                cmdLog.Parameters.AddWithValue("@DossierID", dossierID);
//                cmdLog.Parameters.AddWithValue("@EnrollmentID", enrollmentID);
//                cmdLog.Parameters.AddWithValue("@RejectionText", description);
//                cmdLog.ExecuteNonQuery();
//                conn.Close();
//            }
//            catch (Exception ex)
//            {
//                EventLog eventlog = new EventLog("Application");
//                eventlog.Source = "Energie App";
//                eventlog.WriteEntry("WriteLog : " + ex.Message, EventLogEntryType.Error, 0);
//            }
//        }

//        private void WriteMasterdataRequest(string Eancode, string EanCode_Netbeheerder, DateTime dt_SwitchDatum)
//        {
//            try
//            {
//                SqlConnection conn = new SqlConnection(ConnString);
//                conn.Open();
//                SqlCommand cmdLog = new SqlCommand();
//                cmdLog.Connection = conn;
//                string str_SQL = "INSERT INTO [Messages].[dbo].[MasterdataRequest] " +
//                    "([EAN18] " +
//                    ",[SwitchDatum] " +
//                    ",[NB_EAN13] " +
//                    ",[Verstuurd]) " +
//                    "VALUES " +
//                    "(@EAN18 " +
//                    ",@SwitchDatum " +
//                    ",@NB_EAN13 " +
//                    ",@Verstuurd)";
//                cmdLog.CommandText = str_SQL;
//                cmdLog.Parameters.AddWithValue("@EAN18", Eancode);
//                cmdLog.Parameters.AddWithValue("@SwitchDatum", dt_SwitchDatum);
//                cmdLog.Parameters.AddWithValue("@NB_EAN13", EanCode_Netbeheerder);
//                cmdLog.Parameters.AddWithValue("@Verstuurd", false);
//                cmdLog.ExecuteNonQuery();
//                conn.Close();
//            }
//            catch (Exception ex)
//            {
//                WriteLog("Fout bij schrijven MasterDataRequest: " + Eancode + " Netbeheerder :" + EanCode_Netbeheerder + " Datum : " + dt_SwitchDatum.ToString() + " - " + ex.Message, 10, 0);
//            }
//        }

//        private void GetMasterData(string Eancode, string EanCode_Netbeheerder)
//        {
//            try
//            {
//                Cursor.Current = Cursors.WaitCursor;
//                Energie.SwitchBericht.MasterdataBericht masterData = new Energie.SwitchBericht.MasterdataBericht(Klant_Config);
//                masterData.RequestMasterdata(Eancode, EanCode_Netbeheerder, false, "", false, true, "");
//                Cursor.Current = Cursors.Default;
//            }
//            catch
//            {
//                WriteLog("Fout bij verzoek masterdata Eancode: " + Eancode + " Netbeheerder :" + EanCode_Netbeheerder,10,0);
//            }
//        }

//        //private void timer1_Tick(object sender, EventArgs e)
//        //{
//        //    timer1.Enabled = false;

//        //    listener = new TcpListener(System.Net.IPAddress.Any, port);
//        //    listener.Start();
//        //    DoBeginAcceptTcpClient(listener);
//        //    WriteLog("Start Timer", 10, 0);
//        //}

//        //public ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

//        //public void DoBeginAcceptTcpClient(TcpListener listener)
//        //{

//        //    // Set the event to nonsignaled state.
//        //    tcpClientConnected.Reset();

//        //    // Start to listen for connections from a client.
//        //    //Console.WriteLine("Waiting for a connection...");

//        //    // Accept the connection. 
//        //    // BeginAcceptSocket() creates the accepted socket.
//        //    listener.BeginAcceptTcpClient(
//        //        new AsyncCallback(DoAcceptTcpClientCallback),
//        //        listener);

//        //    // Wait until a connection is made and processed before 
//        //    // continuing.
//        //    //tcpClientConnected.WaitOne();
//        //    WriteLog("Dobegin", 10, 0);
//        //}

//        //public void DoAcceptTcpClientCallback(IAsyncResult ar)
//        //{
//        //    WriteLog("CallBack", 10, 0);
//        //    // Get the listener that handles the client request.
//        //    listener = (TcpListener)ar.AsyncState;




//        //    // End the operation and display the received data on 
//        //    // the console.
//        //    TcpClient client = listener.EndAcceptTcpClient(ar);
//        //    using (var stream = client.GetStream())
//        //    using (var output = File.Create(path + "RemoteResult.xml"))
//        //    {
//        //        //Console.WriteLine("Client connected. Starting to receive the file");

//        //        // read the file in chunks of 1KB
//        //        var buffer = new byte[1024];
//        //        int bytesRead;
//        //        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
//        //        {
//        //            output.Write(buffer, 0, bytesRead);
//        //        }
//        //    }

//        //    // Process the connection here. (Add the client to a
//        //    // server table, read data, etc.)
//        //    Console.WriteLine("Client connected completed");

//        //    // Signal the calling thread to continue.
//        //    tcpClientConnected.Set();
//        //    if (blnContinue) { DoBeginAcceptTcpClient(listener); }
//        //}
//    }
//}
