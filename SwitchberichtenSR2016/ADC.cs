using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using Energie.DataTableHelper;

namespace Energie.SwitchBericht
{
    public class ADC
    {
        private string path = Energie.DataAccess.Configurations.GetApplicationSetting("XMLPATH");

        public void Verplichting(string strFileName)
        {
            string CSVFileName = Path.GetFileNameWithoutExtension(strFileName) + ".CSV";
            string DocumentHeader = "";
            string Portaal_Query = "";
            string Portaal_MeteringPoint = "";
            StringBuilder sb = new StringBuilder();


            Energie.ESDN.ObligationSpecificationEnvelope retour = new Energie.ESDN.ObligationSpecificationEnvelope();
            XmlDocument _Doc = new XmlDocument();
            _Doc.Load(strFileName);

            XmlSerializer serializer = new XmlSerializer(typeof(Energie.ESDN.ObligationSpecificationEnvelope));
            retour = (Energie.ESDN.ObligationSpecificationEnvelope)serializer.Deserialize(new XmlNodeReader(_Doc.DocumentElement));

            DocumentHeader = '"' + retour.EDSNBusinessDocumentHeader.CreationTimestamp.ToUniversalTime().ToString().Trim() + '"' + "," + '"' +
                retour.EDSNBusinessDocumentHeader.MessageID.ToString() + '"' + "," + '"' +
                retour.EDSNBusinessDocumentHeader.Destination.Receiver.ReceiverID.ToString() + '"' + "," + '"' +
                retour.EDSNBusinessDocumentHeader.Source.SenderID + '"';
            sb.AppendLine(DocumentHeader);

            Energie.ESDN.ObligationSpecificationEnvelope_PC pc = new Energie.ESDN.ObligationSpecificationEnvelope_PC();
            pc = retour.Portaal_Content;

            Energie.ESDN.ObligationSpecificationEnvelope_PC_Query query = new Energie.ESDN.ObligationSpecificationEnvelope_PC_Query();
            query = pc.Query;

            Portaal_Query = pc.Query.DateFrom.ToString("yyyy-MM-dd").Trim() + '"' + "," + '"' +
                pc.Query.DateTo.ToString("yyyy-MM-dd").Trim() + '"' + "," + '"' +
                pc.Query.ID.ToString().Trim() + '"' + "," + '"' +
                pc.Query.GridOperator_Company.VATnumber.ToString().Trim() + '"' + "," + '"' +
                pc.Query.BalanceSupplier_Company.ID.ToString().Trim() + '"' + "," + '"' +
                pc.Query.FinancialCharacteristics.Currency.ToString().Trim() + '"' + "," + '"' +
                pc.Query.FinancialCharacteristics.FinancialAmount.AmountExVAT4Decimal.ToString().Replace('.',',').Trim() + '"';
            sb.AppendLine(Portaal_Query);

            Energie.ESDN.ObligationSpecificationEnvelope_PC_Query_PMP[] pmps = query.Portaal_MeteringPoint;

            foreach (Energie.ESDN.ObligationSpecificationEnvelope_PC_Query_PMP pmp in pmps)
            {
                Portaal_MeteringPoint = pmp.EANID.ToString().Trim() + '"' + "," + '"' +
                    pmp.MPCommercialCharacteristics.DayTariff.ToString().Trim() + '"' + "," + '"' +
                    pmp.MPCommercialCharacteristics.TotalDays.ToString().Trim() + '"' + "," + '"' +
                    pmp.MPPhysicalCharacteristics.CapTarCode.ToString().Trim() + '"' + "," + '"' +
                    pmp.MPCommercialCharacteristics.FinancialAmount.AmountExVAT4Decimal.ToString().Replace('.', ',').Trim() + '"';
                sb.AppendLine(Portaal_MeteringPoint);
            }

            using (StreamWriter outfile = new StreamWriter(path + CSVFileName))
            {
                outfile.Write(sb.ToString());
            }

            string strProduct = "";
            string ftpResponse = "";
            if (strFileName.IndexOf("8714252022926") != -1) { strProduct = "Gas/"; } else { strProduct = "Elektra/"; }
            string strFTPPathName = strProduct + GetFTPRootPath(pc.Query.DateFrom.Month) + "/" + GetFTPPath(strFileName) + "/" + CSVFileName;

            
            if (FTPClass.FtpSendFile(@"ftp://services.robinenergie.camelit.nl:21000/" + strFTPPathName, "edbgadc", "ADCPWD45672384>>", path + CSVFileName, out ftpResponse) == false)
            {
                MessageBox.Show("Fout bij verzenden naar robin " + ftpResponse);
            }
        }

        private string GetFTPPath(string strFileName)
        {
            if (strFileName.IndexOf("8716916000004") != -1) { return "Cogas 8716916000004"; }
            if (strFileName.IndexOf("8716902000001") != -1) { return "Delta NWB 8716902000001"; }
            if (strFileName.IndexOf("8717177000000") != -1) { return "Endinet 8717177000000"; }
            if (strFileName.IndexOf("8712423014022") != -1) { return "Enexis 8712423014022"; }
            if (strFileName.IndexOf("8716871000002") != -1) { return "Liander 8716871000002"; }
            if (strFileName.IndexOf("8716912000008") != -1) { return "Rendo 8716912000008"; }
            if (strFileName.IndexOf("8716892000005") != -1) { return "Stedin 8716892000005"; }
            if (strFileName.IndexOf("8716878999996") != -1) { return "Westland Infra 8716878999996"; }
            
            return "";
        }

        private string GetFTPRootPath(int month)
        {
            string strRoot = "";
            switch (month)
            {
                case 1: strRoot =  "1 Januari";
                    break;
                case 2: strRoot =  "2 Februari";
                    break;
                case 3: strRoot = "3 Maart";
                    break;
                case 4: strRoot = "4 April";
                    break;
                case 5: strRoot = "5 Mei";
                    break;
                case 6: strRoot = "6 Juni";
                    break;
                case 7: strRoot = "7 Juli";
                    break;
                case 8: strRoot = "8 Augustus";
                    break;
                case 9: strRoot = "9 September";
                    break;
                case 10: strRoot = "10 Oktober";
                    break;
                case 11: strRoot = "11 November";
                    break;
                case 12: strRoot = "12 December";
                    break;
            }

            return strRoot;
        }
    }
}
