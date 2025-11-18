using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Energie.DataAccess.DataSet1TableAdapters;
using System.Resources;
using nl.Energie.EDSN;
using System.Runtime.CompilerServices;
using System.Security.Policy;

namespace Energie.Car
{
    public class MeetCorrectie
    {
        private string strSQL;
        private SqlConnection conn;
        private CarShared carShared;

        public int MeetcorrectieId;
        public int BerichtId = -1;
        public Int64 Ean18Code;
        public string Referentie = "";
        public Int64 MV;
        public Int64 Ontvanger;
        public Int64 Netbeheerder;
        public string Beschrijving = "";
        public DateTime DatumConstatering;
        public string Reden = "";
        public DateTime BeginDatum;
        public DateTime EindDatum;
        private Boolean geaccepteerd;
        public Boolean Geaccepteerd_Set = false;
        public Boolean Verzonden = false;
        public string ExterneReferentie;
        public string Dossier;

        //MeetCorrectieVolume
        public int VolumeId;
        public Boolean Correctie;  //True = correctie volumes, False = Oude volumes
        public Int64 Hoeveelheid;
        public string Categorie = "";
        public DateTime BeginDatumVolume;
        public DateTime EindDatumVolume;
        public string Meeteenheid = "";
        public string Meetrichting = "";
        public string Tariefzone = "";
        public string MeetSoort = "";
        public string OpwekId;
        public string Allocatiepunt;

        public Boolean Geaccepteerd
        {
            get { return this.geaccepteerd; }
            set 
            { 
                this.geaccepteerd = value;
                this.Geaccepteerd_Set = true;
            }
        }

        private Boolean blnBatchProcess = false;
        public MeetCorrectie(nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResponseEnvelope_PC_PMP portaal_MeasureCorrection, int Bericht_ID, Boolean pblnBatchProcess, Int64 SenderEAN)
        {
            conn = new SqlConnection(KC.ConnString);
            conn.Open();

            carShared = new CarShared();

            blnBatchProcess = pblnBatchProcess;

            BerichtId = Bericht_ID;
            Ean18Code = Int64.Parse(portaal_MeasureCorrection.EANID);

            Referentie = portaal_MeasureCorrection.Portaal_Mutation.ExternalReference;
            MV = Int64.Parse(portaal_MeasureCorrection.Portaal_Mutation.Initiator);
            Ontvanger = Int64.Parse(portaal_MeasureCorrection.Portaal_Mutation.Consumer);
            Netbeheerder = Int64.Parse(portaal_MeasureCorrection.GridOperator_Company.ID);
            Beschrijving = portaal_MeasureCorrection.MeasureCorrection.Description;
            DatumConstatering = portaal_MeasureCorrection.MeasureCorrection.ObservationDate;
            Reden = string.Join(",", portaal_MeasureCorrection.MeasureCorrection.Reason);
            BeginDatum = portaal_MeasureCorrection.MeasureCorrection.ValidFromDate;
            EindDatum = portaal_MeasureCorrection.MeasureCorrection.ValidToDate;
            if (portaal_MeasureCorrection.MPPhysicalCharacteristics != null) { Allocatiepunt = portaal_MeasureCorrection.MPPhysicalCharacteristics.AllocationPoint; }
            Dossier = portaal_MeasureCorrection.Portaal_Mutation.Dossier.ID;
            ExterneReferentie = portaal_MeasureCorrection.Portaal_Mutation.ExternalReference;

            
            SchrijfMeetCorrectie();


            foreach (nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResponseEnvelope_PC_PMP_Correction_Volume pmp_cor in portaal_MeasureCorrection.Correction_Volume)
            {
                Correctie = true;
                Hoeveelheid = Int64.Parse(pmp_cor.Volume);
                Categorie = pmp_cor.Category;
                BeginDatumVolume = pmp_cor.ValidFromDate;
                EindDatumVolume = pmp_cor.ValidToDate;
                Meeteenheid = pmp_cor.Register.MeasureUnit;
                Meetrichting = pmp_cor.Register.MeteringDirection;
                Tariefzone = pmp_cor.Register.TariffType;
                MeetSoort = pmp_cor.Register.EnergyMeasurement;
                if (pmp_cor.Register.Portaal_EnergyMeter != null)
                {
                    OpwekId = pmp_cor.Register.Portaal_EnergyMeter.mktGeneratingUnit.mRID;
                }

                SchrijfMeetCorrectieVolume();
            }

            foreach (nl.Energie.EDSN.MeasureCorrection.MeasureCorrectionResponseEnvelope_PC_PMP_Original_Volume pmp_org in portaal_MeasureCorrection.Original_Volume)
            {
                Correctie = false;
                Hoeveelheid = Int64.Parse(pmp_org.Volume);
                Categorie = pmp_org.Category;
                BeginDatumVolume = pmp_org.ValidFromDate;
                EindDatumVolume = pmp_org.ValidToDate;
                Meeteenheid = pmp_org.Register.MeasureUnit;
                Meetrichting = pmp_org.Register.MeteringDirection;
                Tariefzone = pmp_org.Register.TariffType;
                MeetSoort = pmp_org.Register.EnergyMeasurement;
                if (pmp_org.Register.Portaal_EnergyMeter != null)
                {
                    OpwekId = pmp_org.Register.Portaal_EnergyMeter.mktGeneratingUnit.mRID;
                }

                SchrijfMeetCorrectieVolume();
            }
        }

        public Boolean SchrijfMeetCorrectie()
        {
            Boolean blnResult = false;
            try
            {
                strSQL = "INSERT INTO Car.dbo.Meetcorrecties \n";
                strSQL += "(BerichtId \n";
                strSQL += ",Ean18Code \n";
                if (Referentie != null){ strSQL += ",Referentie \n"; }
                strSQL += ",MV \n";
                strSQL += ",Ontvanger \n";
                strSQL += ",Netbeheerder \n";
                strSQL += ",Beschrijving \n";
                strSQL += ",DatumConstatering \n";
                strSQL += ",Reden \n";
                strSQL += ",BeginDatum \n";
                strSQL += ",EindDatum \n";
                if (Allocatiepunt != null) { strSQL += ",Allocatiepunt \n"; }
                strSQL += ",Dossier \n";
                if (ExterneReferentie != null) { strSQL += ",ExterneReferentie \n"; }
                if (Geaccepteerd_Set) { strSQL += ",Geaccepteerd \n"; }
                strSQL += ",Verzonden) \n";
                strSQL += "VALUES \n";
                strSQL += "(@BerichtId \n";
                strSQL += ",@Ean18Code \n";
                if (Referentie != null) { strSQL += ",@Referentie \n"; }
                strSQL += ",@MV \n";
                strSQL += ",@Ontvanger \n";
                strSQL += ",@Netbeheerder \n";
                strSQL += ",@Beschrijving \n";
                strSQL += ",@DatumConstatering \n";
                strSQL += ",@Reden \n";
                strSQL += ",@BeginDatum \n";
                strSQL += ",@EindDatum \n";
                if (Allocatiepunt != null) { strSQL += ",@Allocatiepunt \n"; }
                strSQL += ",@Dossier \n";
                if (ExterneReferentie != null) { strSQL += ",@ExterneReferentie \n"; }
                if (Geaccepteerd_Set) { strSQL += ",@Geaccepteerd \n"; }
                strSQL += ",@Verzonden); SELECT @MeetcorrectieId = SCOPE_IDENTITY(); \n";
                SqlCommand cmd = new SqlCommand(strSQL, conn);
                cmd.Parameters.AddWithValue("@BerichtId", BerichtId);
                cmd.Parameters.AddWithValue("@Ean18Code", Ean18Code);
                if (Referentie != null) { cmd.Parameters.AddWithValue("@Referentie", Referentie); }
                cmd.Parameters.AddWithValue("@MV", MV);
                cmd.Parameters.AddWithValue("@Ontvanger", Ontvanger);
                cmd.Parameters.AddWithValue("@Netbeheerder", Netbeheerder);
                cmd.Parameters.AddWithValue("@Beschrijving", Beschrijving);
                cmd.Parameters.AddWithValue("@DatumConstatering", DatumConstatering);
                cmd.Parameters.AddWithValue("@Reden", Reden);
                cmd.Parameters.AddWithValue("@BeginDatum", BeginDatum);
                cmd.Parameters.AddWithValue("@EindDatum", EindDatum);
                if (Allocatiepunt != null) { cmd.Parameters.AddWithValue("@Allocatiepunt", Allocatiepunt); }
                cmd.Parameters.AddWithValue("@Dossier", Dossier);
                if (ExterneReferentie != null) { cmd.Parameters.AddWithValue("@ExterneReferentie", ExterneReferentie); }
                if (Geaccepteerd_Set) { cmd.Parameters.AddWithValue("@Geaccepteerd", Geaccepteerd); }
                cmd.Parameters.AddWithValue("@Verzonden", Verzonden);

                cmd.Parameters.Add(new SqlParameter("@MeetcorrectieId", SqlDbType.Int));
                cmd.Parameters["@MeetcorrectieId"].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                MeetcorrectieId = (int)cmd.Parameters["@MeetcorrectieId"].Value;
                blnResult = true;
            }

            catch (Exception ex)
            {
                //log
                if (blnBatchProcess)
                {
                    carShared.SchrijfLog("Fout bij opslaan Meetcorrectie : " + ex.Message, 5, BerichtId, KC.App_ID);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
            return blnResult;
        }

        public Boolean SchrijfMeetCorrectieVolume()
        {
            Boolean blnResult = false;
            try
            {
                String strSQL = "INSERT INTO Car.dbo.MeetCorrectieVolumes \n";
                strSQL += "(MeetcorrectieId \n";
                strSQL += ",Correctie \n";
                strSQL += ",Hoeveelheid \n";
                strSQL += ",Categorie \n";
                strSQL += ",BeginDatum \n";
                strSQL += ",EindDatum \n";
                strSQL += ",Meeteenheid \n";
                strSQL += ",Meetrichting \n";
                strSQL += ",Tariefzone \n";
                strSQL += ",MeetSoort \n";
                if (OpwekId != null) { strSQL += ",OpwekId \n"; }
                strSQL += ") \n";
                strSQL += "VALUES \n";
                strSQL += "(@MeetcorrectieId \n";
                strSQL += ",@Correctie \n";
                strSQL += ",@Hoeveelheid \n";
                strSQL += ",@Categorie \n";
                strSQL += ",@BeginDatum \n";
                strSQL += ",@EindDatum \n";
                strSQL += ",@Meeteenheid \n";
                strSQL += ",@Meetrichting \n";
                strSQL += ",@Tariefzone \n";
                strSQL += ",@MeetSoort \n";
                if (OpwekId != null) { strSQL += ",@OpwekId \n"; }
                strSQL += "); SELECT @VolumeId = SCOPE_IDENTITY();";
                SqlCommand cmd = new SqlCommand(strSQL, conn);
                cmd.Parameters.AddWithValue("@MeetcorrectieId", MeetcorrectieId);
                cmd.Parameters.AddWithValue("@Correctie", Correctie);
                cmd.Parameters.AddWithValue("@Hoeveelheid", Hoeveelheid);
                cmd.Parameters.AddWithValue("@Categorie", Categorie);
                cmd.Parameters.AddWithValue("@BeginDatum", BeginDatumVolume);
                cmd.Parameters.AddWithValue("@EindDatum", EindDatumVolume);
                cmd.Parameters.AddWithValue("@Meeteenheid", Meeteenheid);
                cmd.Parameters.AddWithValue("@Meetrichting", Meetrichting);
                cmd.Parameters.AddWithValue("@Tariefzone", Tariefzone);
                cmd.Parameters.AddWithValue("@Meetsoort", MeetSoort);
                if (OpwekId != null) { cmd.Parameters.AddWithValue("@OpwekId", OpwekId); }

                cmd.Parameters.Add(new SqlParameter("@VolumeId", SqlDbType.Int));
                cmd.Parameters["@VolumeId"].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                VolumeId = (int)cmd.Parameters["@VolumeId"].Value;
                blnResult = true;
            }

            catch (Exception ex)
            {
                //log
                if (blnBatchProcess)
                {
                    carShared.SchrijfLog("Fout bij opslaan MeetcorrectieVolume : " + ex.Message, 5, BerichtId, KC.App_ID);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
            return blnResult;
        }
    }
}
