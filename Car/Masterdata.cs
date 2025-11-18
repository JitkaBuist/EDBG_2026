using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Energie.Car
{
    public class Masterdata
    {
        //private string strSQL;
        private SqlConnection conn;
        private CarShared carShared;

        private int masterdataID;
        private int bericht_ID = -1;
        private Int64 ean18_Code;
        private DateTime datumMutatie;
        private Int64 ontvangerEAN;
        private Int64 netbeheerderEAN;
        private Int64 leverancierEAN;
        private Int64 meetverantwoordelijkeEAN;
        private Int64 programmaverantwoordelijkeEAN;
        private Int64 netgebiedEAN;
        private string productSoort = "";
        private string wijzeVanBemetering = "";
        private string profielCategorie = "";
        private string afrekenmaand = "";
        private int contractCapaciteit;
        private string fysiekeCapaciteit = "";
        private string maxVerbruik = "";
        private Int64 capaciteitstariefcode;
        private string fysiekeStatus = "";
        private string leveringsStatus = "";
        private string leveringsRichting = "";
        //private string verblijfsfunctie = "";
        //private string complexbepaling = "";
        private string verbruikSegment = "";
        private int sjvNormaal;
        private int sjvLaag;
        private int sjiNormaal;
        private int sjiLaag;
        private string straatnaam = "";
        private string huisnummer = "";
        private string huisnummerToevoeging = "";
        private string postcode = "";
        private string woonplaats = "";
        private string landcode = "";
        private string locatieOmschrijving = "";
        private string bag = "";
        private string allocatieMethode = "";
        private string administratieveStatusMeter = "";
        private string meternummer = "";
        private string typeMeter = "";
        private string uitleesbaarheidSlimmeMeter = "";
        private Boolean temperatuurCorrectie;
        private int aantalTelwerken;

       

        //MasterdataRegister
        private string telwerkID = "";
        private string meetEenheid = "";
        private string meetRichting = "";
        private string vermenigvuldigingsfactor = "";
        private int aantalTelwielen;
        private string tariefZone = "";

        //MasterdataPM
        private DateTime datumPM;
        private string dossier = "";
        private string reden = "";
        private string referentie = "";

        private Boolean blnBatchProcess = false;
        //private Applicationlog applicationLog = new Applicationlog(ConnString);

        public Masterdata(nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP portaal_MeteringPoint, int Bericht_ID, Boolean pblnBatchProcess, Int64 SenderEAN)
        {
            conn = new SqlConnection(KC.ConnString);
            conn.Open();

            carShared = new CarShared();

            blnBatchProcess = pblnBatchProcess;

            this.Bericht_ID = Bericht_ID;
            EAN18_Code = Int64.Parse(portaal_MeteringPoint.EANID);

            OntvangerEAN = SenderEAN;

            //if (Int64_Code.ToString() == "871685900014370148")
            //{
            //    System.Windows.Forms.MessageBox.Show("Daar heb je hem");
            //}
            if (portaal_MeteringPoint.GridOperator_Company != null)
            {
                NetbeheerderEAN = Int64.Parse(portaal_MeteringPoint.GridOperator_Company.ID);
            }
            if (portaal_MeteringPoint.MPCommercialCharacteristics.BalanceSupplier_Company != null)
            {
                LeverancierEAN = Int64.Parse(portaal_MeteringPoint.MPCommercialCharacteristics.BalanceSupplier_Company.ID);
            }
            if (portaal_MeteringPoint.MPCommercialCharacteristics.MeteringResponsibleParty_Company != null)
            {
                MeetverantwoordelijkeEAN = Int64.Parse(portaal_MeteringPoint.MPCommercialCharacteristics.MeteringResponsibleParty_Company.ID);
            }
            if (portaal_MeteringPoint.MPCommercialCharacteristics.BalanceResponsibleParty_Company != null)
            {
                ProgrammaverantwoordelijkeEAN = Int64.Parse(portaal_MeteringPoint.MPCommercialCharacteristics.BalanceResponsibleParty_Company.ID);
            }
            if (portaal_MeteringPoint.GridArea != null)
            {
                NetgebiedEAN = Int64.Parse(portaal_MeteringPoint.GridArea);
            }

            if (portaal_MeteringPoint.EDSN_AddressSearch != null)
            {
                Postcode = portaal_MeteringPoint.EDSN_AddressSearch.ZIPCode;
                Huisnummer = portaal_MeteringPoint.EDSN_AddressSearch.BuildingNr;
                if (portaal_MeteringPoint.EDSN_AddressSearch.ExBuildingNr != null) { HuisnummerToevoeging = portaal_MeteringPoint.EDSN_AddressSearch.ExBuildingNr; }
                Straatnaam = portaal_MeteringPoint.EDSN_AddressSearch.StreetName;
                Woonplaats = portaal_MeteringPoint.EDSN_AddressSearch.CityName;
                if (portaal_MeteringPoint.EDSN_AddressSearch.Country != null) { Landcode = portaal_MeteringPoint.EDSN_AddressSearch.Country; }
                if (portaal_MeteringPoint.EDSN_AddressSearch.BAG != null) { BAG = portaal_MeteringPoint.EDSN_AddressSearch.BAG.BAGID; }
            }
            if (portaal_MeteringPoint.LocationDescription != null) { LocatieOmschrijving = portaal_MeteringPoint.LocationDescription; }
            ProductSoort = portaal_MeteringPoint.ProductType.ToString();
            VerbruikSegment = portaal_MeteringPoint.MarketSegment.ToString();
            if (portaal_MeteringPoint.MPPhysicalCharacteristics.EnergyFlowDirection != null)
            {
                LeveringsRichting = portaal_MeteringPoint.MPPhysicalCharacteristics.EnergyFlowDirection.ToString();
            }
            FysiekeStatus = portaal_MeteringPoint.MPPhysicalCharacteristics.PhysicalStatus.ToString();
            WijzeVanBemetering = portaal_MeteringPoint.MPPhysicalCharacteristics.MeteringMethod.ToString();
            if (portaal_MeteringPoint.MPPhysicalCharacteristics.ContractedCapacity != null)
            {
                ContractCapaciteit = int.Parse(portaal_MeteringPoint.MPPhysicalCharacteristics.ContractedCapacity.ToString());
            }
            FysiekeStatus = portaal_MeteringPoint.MPPhysicalCharacteristics.PhysicalStatus.ToString();
            if (portaal_MeteringPoint.MPPhysicalCharacteristics.ContractedCapacity != null)
            {
                ContractCapaciteit = int.Parse(portaal_MeteringPoint.MPPhysicalCharacteristics.ContractedCapacity.ToString());
            }
            if (portaal_MeteringPoint.MPPhysicalCharacteristics.PhysicalCapacity != null)
            {
                FysiekeCapaciteit = portaal_MeteringPoint.MPPhysicalCharacteristics.PhysicalCapacity.ToString();
            }
            if (portaal_MeteringPoint.MPPhysicalCharacteristics.CapTarCode != null)
            {
                Capaciteitstariefcode = Int64.Parse(portaal_MeteringPoint.MPPhysicalCharacteristics.CapTarCode);
            }
            if (portaal_MeteringPoint.MPPhysicalCharacteristics.ProfileCategory != null)
            {
                ProfielCategorie = portaal_MeteringPoint.MPPhysicalCharacteristics.ProfileCategory.ToString();
            }

            if (portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedOffPeak != null)
            {
                int intSJVHoog = 0;
                if (portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedPeak != null)
                {
                    int.TryParse(portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedPeak, out intSJVHoog);
                }
                SJVNormaal = intSJVHoog;
                int intSJVLaag = 0;
                if (portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedOffPeak != null)
                {
                    int.TryParse(portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedOffPeak, out intSJVLaag);
                }
                SJVLaag = intSJVLaag;
            }
            else
            {
                int intSJVHoog = 0;
                if (portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedPeak != null)
                {
                    int.TryParse(portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedPeak, out intSJVHoog);
                }
                SJVNormaal = intSJVHoog;
            }

            if (portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyProductionNettedOffPeak != null)
            {
                int intSJIHoog = 0;
                if (portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyProductionNettedPeak != null)
                {
                    int.TryParse(portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyProductionNettedPeak, out intSJIHoog);
                }
                SJINormaal = intSJIHoog;
                int intSJILaag = 0;
                if (portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyProductionNettedOffPeak != null)
                {
                    int.TryParse(portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyProductionNettedOffPeak, out intSJILaag);
                }
                SJILaag = intSJILaag;
            }
            else
            {
                int intSJIHoog = 0;
                if (portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedPeak != null)
                {
                    int.TryParse(portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedPeak, out intSJIHoog);
                }
                SJINormaal = intSJIHoog;
            }

            //if (portaal_MeteringPoint.MPCommercialCharacteristics.Residential != null)
            //{
            //    Verblijfsfunctie = portaal_MeteringPoint.MPCommercialCharacteristics.Residential.ToString();
            //}
            if (portaal_MeteringPoint.MPPhysicalCharacteristics.AllocationMethod != null)
            {
                AllocatieMethode = portaal_MeteringPoint.MPPhysicalCharacteristics.AllocationMethod.ToString();
            }
            if (portaal_MeteringPoint.MPPhysicalCharacteristics.InvoiceMonth != null)
            {
                Afrekenmaand = portaal_MeteringPoint.MPPhysicalCharacteristics.InvoiceMonth.ToString();
            }

            //if (portaal_MeteringPoint.MPCommercialCharacteristics.DeterminationComplex != null)
            //{
            //    Complexbepaling = portaal_MeteringPoint.MPCommercialCharacteristics.DeterminationComplex.ToString();
            //}
            if (portaal_MeteringPoint.MPPhysicalCharacteristics.MaxConsumption != null)
            {
                MaxVerbruik = portaal_MeteringPoint.MPPhysicalCharacteristics.MaxConsumption;
            }

            LeveringsStatus = portaal_MeteringPoint.MPPhysicalCharacteristics.EnergyDeliveryStatus.ToString();

            if (portaal_MeteringPoint.AdministrativeStatusSmartMeter != null)
            {
                AdministratieveStatusMeter = portaal_MeteringPoint.AdministrativeStatusSmartMeter.ToString();
            }

            if (portaal_MeteringPoint.Portaal_EnergyMeter != null)
            {
                if (portaal_MeteringPoint.Portaal_EnergyMeter != null)
                {
                    Meternummer = portaal_MeteringPoint.Portaal_EnergyMeter.ID;
                }
                if (portaal_MeteringPoint.Portaal_EnergyMeter.Type != null)
                {
                    TypeMeter = portaal_MeteringPoint.Portaal_EnergyMeter.Type.ToString();
                    if (portaal_MeteringPoint.Portaal_EnergyMeter.TemperatureCorrection != null)
                    {
                        if (portaal_MeteringPoint.Portaal_EnergyMeter.TemperatureCorrection.ToString() == "J")
                        {
                            TemperatuurCorrectie = true;
                        }
                        else
                        {
                            TemperatuurCorrectie = false;
                        }
                    }
                }
                int intAantalTelwerken = 0;
                if (portaal_MeteringPoint.Portaal_EnergyMeter.NrOfRegisters != null)
                {
                    int.TryParse(portaal_MeteringPoint.Portaal_EnergyMeter.NrOfRegisters.ToString(), out intAantalTelwerken);
                }
                AantalTelwerken = intAantalTelwerken;

                if (portaal_MeteringPoint.Portaal_EnergyMeter.TechnicalCommunicationSM != null)
                {
                    UitleesbaarheidSlimmeMeter = portaal_MeteringPoint.Portaal_EnergyMeter.TechnicalCommunicationSM.ToString();
                }
            }

            if (portaal_MeteringPoint.Portaal_Mutation != null)
            {
                DatumMutatie = portaal_MeteringPoint.Portaal_Mutation[0].MutationDate;
            }
            SchrijfMasterdata(Bericht_ID);
            if (portaal_MeteringPoint.Portaal_EnergyMeter != null)
            {
                if (portaal_MeteringPoint.Portaal_EnergyMeter.Register != null)
                {
                    foreach (nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register register in portaal_MeteringPoint.Portaal_EnergyMeter.Register)
                    {
                        TelwerkID = register.ID;
                        //MeetEenheid = register.MeasureUnit.ToString();
                        if (register.MeteringDirection != null)
                        {
                            MeetRichting = register.MeteringDirection.ToString();
                        }
                        if (register.MultiplicationFactorSpecified)
                        {
                            Vermenigvuldigingsfactor = register.MultiplicationFactor.ToString();
                        }
                        int intAantalTelwielen;
                        int.TryParse(register.NrOfDigits, out intAantalTelwielen);
                        AantalTelwielen = intAantalTelwielen;

                        if (register.TariffType != null)
                        {
                            TariefZone = register.TariffType.ToString();
                        }

                        SchrijfMasterdataRegister();
                    }
                }
            }
            if (portaal_MeteringPoint.Portaal_Mutation != null)
            {
                foreach (nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP_PM mutation in portaal_MeteringPoint.Portaal_Mutation)
                {
                    DatumPM = mutation.MutationDate;
                    if (mutation.Dossier != null)
                    {
                        Dossier = mutation.Dossier.ID;
                    }
                    Reden = mutation.MutationReason.ToString();
                    if (mutation.ExternalReference != null)
                    {
                        Referentie = mutation.ExternalReference;
                    }

                    SchrijfMasterdataPM();
                }
            }

            if (portaal_MeteringPoint.MeteringPointGroup != null)
            {
                nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP_MeteringPointGroup meteringGroup = portaal_MeteringPoint.MeteringPointGroup;

                if (meteringGroup.PAP != null)
                {
                    nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP_MeteringPointGroup_PAP pap = meteringGroup.PAP;
                    SchrijfMasterdataAP(pap.EANID, 1);
                }

                if (meteringGroup.SAP != null)
                {
                    nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP_MeteringPointGroup_SAP[] saps = meteringGroup.SAP;
                    foreach (nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP_MeteringPointGroup_SAP sap in saps)
                    {
                        SchrijfMasterdataAP(sap.EANID, 0);
                    }
                }
            }
        }

        private int MasterdataID
        {
            get { return masterdataID; }
            set { masterdataID = value; }
        }

        private int Bericht_ID
        {
            get { return bericht_ID; }
            set { bericht_ID = value; }
        }

        private Int64 EAN18_Code
        {
            get { return ean18_Code; }
            set { ean18_Code = value; }
        }

        private DateTime DatumMutatie
        {
            get { return datumMutatie; }
            set { datumMutatie = value; }
        }

        private Int64 OntvangerEAN
        {
            get { return ontvangerEAN; }
            set { ontvangerEAN = value; }
        }
        private Int64 NetbeheerderEAN
        {
            get { return netbeheerderEAN; }
            set { netbeheerderEAN = value; }
        }

        private Int64 LeverancierEAN
        {
            get { return leverancierEAN; }
            set { leverancierEAN = value; }
        }

        private Int64 MeetverantwoordelijkeEAN
        {
            get { return meetverantwoordelijkeEAN; }
            set { meetverantwoordelijkeEAN = value; }
        }

        private Int64 ProgrammaverantwoordelijkeEAN
        {
            get { return programmaverantwoordelijkeEAN; }
            set { programmaverantwoordelijkeEAN = value; }
        }

        private Int64 NetgebiedEAN
        {
            get { return netgebiedEAN; }
            set { netgebiedEAN = value; }
        }

        private string ProductSoort
        {
            get { return productSoort; }
            set { productSoort = value; }
        }

        private string WijzeVanBemetering
        {
            get { return wijzeVanBemetering; }
            set { wijzeVanBemetering = value; }
        }

        private string ProfielCategorie
        {
            get { return profielCategorie; }
            set { profielCategorie = value; }
        }

        private string Afrekenmaand
        {
            get { return afrekenmaand; }
            set { afrekenmaand = value; }
        }

        private int ContractCapaciteit
        {
            get { return contractCapaciteit; }
            set { contractCapaciteit = value; }
        }

        private String FysiekeCapaciteit
        {
            get { return fysiekeCapaciteit; }
            set { fysiekeCapaciteit = value; }
        }

        private string MaxVerbruik
        {
            get { return maxVerbruik; }
            set { maxVerbruik = value; }
        }

        private Int64 Capaciteitstariefcode
        {
            get { return capaciteitstariefcode; }
            set { capaciteitstariefcode = value; }
        }

        private string FysiekeStatus
        {
            get { return fysiekeStatus; }
            set { fysiekeStatus = value; }
        }

        private string LeveringsStatus
        {
            get { return leveringsStatus; }
            set { leveringsStatus = value; }
        }

        private string LeveringsRichting
        {
            get { return leveringsRichting; }
            set { leveringsRichting = value; }
        }

        //private string Verblijfsfunctie
        //{
        //    get { return verblijfsfunctie; }
        //    set { verblijfsfunctie = value; }
        //}

        //private string Complexbepaling
        //{
        //    get { return complexbepaling; }
        //    set { complexbepaling = value; }
        //}

        private string VerbruikSegment
        {
            get { return verbruikSegment; }
            set { verbruikSegment = value; }

        }

        private int SJVNormaal
        {
            get { return sjvNormaal; }
            set { sjvNormaal = value; }
        }

        private int SJILaag
        {
            get { return sjiLaag; }
            set { sjiLaag = value; }
        }

        private int SJINormaal
        {
            get { return sjiNormaal; }
            set { sjiNormaal = value; }
        }

        private int SJVLaag
        {
            get { return sjvLaag; }
            set { sjvLaag = value; }
        }

        private string Straatnaam
        {
            get { return straatnaam; }
            set { straatnaam = value; }
        }

        private string Huisnummer
        {
            get { return huisnummer; }
            set { huisnummer = value; }
        }

        private string HuisnummerToevoeging
        {
            get { return huisnummerToevoeging; }
            set { huisnummerToevoeging = value; }
        }

        private string Postcode
        {
            get { return postcode; }
            set { postcode = value; }
        }
        
        private string Woonplaats
        {
            get { return woonplaats; }
            set { woonplaats = value; }
        }

        private string Landcode
        {
            get { return landcode; }
            set { landcode = value; }
        }

        private string LocatieOmschrijving
        {
            get { return locatieOmschrijving; }
            set { locatieOmschrijving = value; }
        }

        private String BAG
        {
            get { return bag; }
            set { bag = value; }
        }

        private string AllocatieMethode
        {
            get { return allocatieMethode; }
            set { allocatieMethode = value; }
        }

        private string AdministratieveStatusMeter
        {
            get { return administratieveStatusMeter; }
            set { administratieveStatusMeter = value; }
        }

        private string Meternummer
        {
            get { return meternummer; }
            set { meternummer = value; }
        }

        private string TypeMeter
        {
            get { return typeMeter; }
            set { typeMeter = value; }
        }

        private string UitleesbaarheidSlimmeMeter
        {
            get { return uitleesbaarheidSlimmeMeter; }
            set { uitleesbaarheidSlimmeMeter = value; }
        }

        private Boolean TemperatuurCorrectie
        {
            get { return temperatuurCorrectie; }
            set { temperatuurCorrectie = value; }
        }

        private int AantalTelwerken
        {
            get { return aantalTelwerken; }
            set { aantalTelwerken = value; }
        }



        

        //MasterdataRegisters

        private string TelwerkID
        {
            get { return telwerkID; }
            set { telwerkID = value; }
        }

        private string MeetEenheid
        {
            get { return meetEenheid; }
            set { meetEenheid = value; }
        }
        private string MeetRichting
        {
            get { return meetRichting; }
            set { meetRichting = value; }
        }
        private string Vermenigvuldigingsfactor
        {
            get { return vermenigvuldigingsfactor; }
            set { vermenigvuldigingsfactor = value; }
        }
        private int AantalTelwielen
        {
            get { return aantalTelwielen; }
            set { aantalTelwielen = value; }
        }
        private string TariefZone
        {
            get { return tariefZone; }
            set { tariefZone = value; }
        }

        //MasterdataPM

        private DateTime DatumPM
        {
            get { return datumPM; }
            set { datumPM = value; }
        }

        private string Dossier
        {
            get { return dossier; }
            set { dossier = value; }
        }

        private string Reden
        {
            get { return reden; }
            set { reden = value; }
        }

        private string Referentie
        {
            get { return referentie; }
            set { referentie = value; }
        }


        public Boolean SchrijfMasterdata(int intInboxID)
        {
            Boolean blnResult = false;

            String strSQL = "INSERT INTO Car.dbo.Masterdata \n";
            strSQL += "(Bericht_ID \n";
            strSQL += ", EAN18_Code \n";
            strSQL += ", DatumMutatie \n";
            strSQL += ", OntvangerEAN \n";
            strSQL += ", NetbeheerderEAN \n";
            strSQL += ", LeverancierEAN \n";
            strSQL += ", ProgrammaverantwoordelijkeEAN \n";
            strSQL += ", MeetverantwoordelijkeEAN \n";
            strSQL += ", NetgebiedEAN \n";
            strSQL += ", ProductSoort \n";
            strSQL += ", WijzeVanBemetering \n";
            strSQL += ", ProfielCategorie \n";
            strSQL += ", Afrekenmaand \n";
            strSQL += ", ContractCapaciteit \n";
            strSQL += ", FysiekeCapaciteit \n";
            strSQL += ", Maxverbruik \n";
            strSQL += ", Capaciteitstariefcode \n";
            strSQL += ", FysiekeStatus \n";
            strSQL += ", LeveringsStatus \n";
            strSQL += ", LeveringsRichting \n";
            //strSQL += ", Verblijfsfunctie \n";
            //strSQL += ", Complexbepaling \n";
            strSQL += ", VerbruikSegment \n";
            strSQL += ", SJVNormaal \n";
            strSQL += ", SJVLaag \n";
            strSQL += ", SJINormaal \n";
            strSQL += ", SJILaag \n";
            strSQL += ", Straatnaam \n";
            strSQL += ", Huisnummer \n";
            strSQL += ", HuisnummerToevoeging \n";
            strSQL += ", Postcode \n";
            strSQL += ", Woonplaats \n";
            strSQL += ", Landcode \n";
            strSQL += ", LocatieOmschrijving \n";
            strSQL += ", BAG \n";
            strSQL += ", Allocatiemethode \n";
            strSQL += ", AdministratieveStatusMeter \n";
            strSQL += ", Meternummer \n";
            strSQL += ", TypeMeter \n";
            strSQL += ", UitleesbaarheidSlimmeMeter \n";
            strSQL += ", TemperatuurCorrectie \n";
            strSQL += ", AantalTelwerken) \n";
            strSQL += "VALUES \n";
            strSQL += "(@Bericht_ID \n";
            strSQL += ", @EAN18_Code \n";
            strSQL += ", @DatumMutatie \n";
            strSQL += ", @OntvangerEAN \n";
            strSQL += ", @NetbeheerderEAN \n";
            strSQL += ", @LeverancierEAN \n";
            strSQL += ", @ProgrammaverantwoordelijkeEAN \n";
            strSQL += ", @MeetverantwoordelijkeEAN \n";
            strSQL += ", @NetgebiedEAN \n";
            strSQL += ", @ProductSoort \n";
            strSQL += ", @WijzeVanBemetering \n";
            strSQL += ", @ProfielCategorie \n";
            strSQL += ", @Afrekenmaand \n";
            strSQL += ", @ContractCapaciteit \n";
            strSQL += ", @FysiekeCapaciteit \n";
            strSQL += ", @Maxverbruik \n";
            strSQL += ", @Capaciteitstariefcode \n";
            strSQL += ", @FysiekeStatus \n";
            strSQL += ", @LeveringsStatus \n";
            strSQL += ", @LeveringsRichting \n";
            //strSQL += ", @Verblijfsfunctie \n";
            //strSQL += ", @Complexbepaling \n";
            strSQL += ", @VerbruikSegment \n";
            strSQL += ", @SJVNormaal \n";
            strSQL += ", @SJVLaag \n";
            strSQL += ", @SJINormaal \n";
            strSQL += ", @SJILaag \n";
            strSQL += ", @Straatnaam \n";
            strSQL += ", @Huisnummer \n";
            strSQL += ", @HuisnummerToevoeging \n";
            strSQL += ", @Postcode \n";
            strSQL += ", @Woonplaats \n";
            strSQL += ", @Landcode \n";
            strSQL += ", @LocatieOmschrijving \n";
            strSQL += ", @BAG \n";
            strSQL += ", @Allocatiemethode \n";
            strSQL += ", @AdministratieveStatusMeter \n";
            strSQL += ", @Meternummer \n";
            strSQL += ", @TypeMeter \n";
            strSQL += ", @UitleesbaarheidSlimmeMeter \n";
            strSQL += ", @TemperatuurCorrectie \n";
            strSQL += ", @AantalTelwerken); SELECT @MasterdataID = SCOPE_IDENTITY();";

            try
            {
                SqlCommand cmd = new SqlCommand(strSQL, conn);
                cmd.Parameters.AddWithValue("@Bericht_ID", intInboxID);
                cmd.Parameters.AddWithValue("@EAN18_Code", EAN18_Code);
                cmd.Parameters.AddWithValue("@DatumMutatie", DatumMutatie);
                cmd.Parameters.AddWithValue("@OntvangerEAN", OntvangerEAN);
                cmd.Parameters.AddWithValue("@NetbeheerderEAN", NetbeheerderEAN);
                cmd.Parameters.AddWithValue("@LeverancierEAN", LeverancierEAN);
                cmd.Parameters.AddWithValue("@ProgrammaverantwoordelijkeEAN", ProgrammaverantwoordelijkeEAN);
                cmd.Parameters.AddWithValue("@MeetverantwoordelijkeEAN", MeetverantwoordelijkeEAN);
                cmd.Parameters.AddWithValue("@NetgebiedEAN", NetgebiedEAN);
                cmd.Parameters.AddWithValue("@ProductSoort", ProductSoort);
                cmd.Parameters.AddWithValue("@WijzeVanBemetering", WijzeVanBemetering);
                cmd.Parameters.AddWithValue("@ProfielCategorie", ProfielCategorie);
                cmd.Parameters.AddWithValue("@Afrekenmaand", Afrekenmaand);
                cmd.Parameters.AddWithValue("@ContractCapaciteit", ContractCapaciteit);
                cmd.Parameters.AddWithValue("@FysiekeCapaciteit", FysiekeCapaciteit);
                cmd.Parameters.AddWithValue("@Maxverbruik", MaxVerbruik);
                cmd.Parameters.AddWithValue("@Capaciteitstariefcode", Capaciteitstariefcode);
                cmd.Parameters.AddWithValue("@FysiekeStatus", FysiekeStatus);
                cmd.Parameters.AddWithValue("@LeveringsStatus", LeveringsStatus);
                cmd.Parameters.AddWithValue("@LeveringsRichting", LeveringsRichting);
                //cmd.Parameters.AddWithValue("@Verblijfsfunctie", Verblijfsfunctie);
                //cmd.Parameters.AddWithValue("@Complexbepaling", Complexbepaling);
                cmd.Parameters.AddWithValue("@VerbruikSegment", VerbruikSegment);
                cmd.Parameters.AddWithValue("@SJVNormaal", SJVNormaal);
                cmd.Parameters.AddWithValue("@SJVLaag", SJVLaag);
                cmd.Parameters.AddWithValue("@SJINormaal", SJINormaal);
                cmd.Parameters.AddWithValue("@SJILaag", SJILaag);
                cmd.Parameters.AddWithValue("@Straatnaam", Straatnaam);
                cmd.Parameters.AddWithValue("@Huisnummer", Huisnummer);
                cmd.Parameters.AddWithValue("@HuisnummerToevoeging", HuisnummerToevoeging);
                cmd.Parameters.AddWithValue("@Postcode", Postcode);
                cmd.Parameters.AddWithValue("@Woonplaats", Woonplaats);
                cmd.Parameters.AddWithValue("@Landcode", Landcode);
                cmd.Parameters.AddWithValue("@LocatieOmschrijving", LocatieOmschrijving);
                cmd.Parameters.AddWithValue("@BAG", BAG);
                cmd.Parameters.AddWithValue("@Allocatiemethode", AllocatieMethode);
                cmd.Parameters.AddWithValue("@AdministratieveStatusMeter", AdministratieveStatusMeter);
                cmd.Parameters.AddWithValue("@Meternummer", Meternummer);
                cmd.Parameters.AddWithValue("@TypeMeter", TypeMeter);
                cmd.Parameters.AddWithValue("@UitleesbaarheidSlimmeMeter", UitleesbaarheidSlimmeMeter);
                cmd.Parameters.AddWithValue("@TemperatuurCorrectie", TemperatuurCorrectie);
                cmd.Parameters.AddWithValue("@AantalTelwerken", AantalTelwerken);

                cmd.Parameters.Add(new SqlParameter("@MasterdataID", SqlDbType.Int));
                cmd.Parameters["@MasterdataID"].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                MasterdataID = (int)cmd.Parameters["@MasterdataID"].Value;


                blnResult = true;
            }
            catch (Exception ex)
            {
                //log
                if (blnBatchProcess)
                {
                    carShared.SchrijfLog("Fout bij opslaan Masterdata : " + ex.Message, 5, Bericht_ID, KC.App_ID);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
            return blnResult;
        }

        private Boolean SchrijfMasterdataRegister()
        {
            Boolean blnResult = false;

            String strSQL = "INSERT INTO Car.dbo.MasterdataTelwerken \n";
            strSQL += "(Masterdata_ID \n";
            strSQL += ",Tewerk_ID \n";
            strSQL += ",Meeteenheid \n";
            strSQL += ",Meetrichting \n";
            strSQL += ",AantalTelwielen \n";
            strSQL += ",TariefZone \n";
            strSQL += ",Factor) \n";
            strSQL += "VALUES \n";
            strSQL += "(@Masterdata_ID \n";
            strSQL += ",@Tewerk_ID \n";
            strSQL += ",@Meeteenheid \n";
            strSQL += ",@Meetrichting \n";
            strSQL += ",@AantalTelwielen \n";
            strSQL += ",@TariefZone \n";
            strSQL += ",@Factor)";

            try
            {
                SqlCommand cmd = new SqlCommand(strSQL, conn);
                cmd.Parameters.AddWithValue("@Masterdata_ID", MasterdataID);
                cmd.Parameters.AddWithValue("@Tewerk_ID", TelwerkID);
                cmd.Parameters.AddWithValue("@Meeteenheid", MeetEenheid);
                cmd.Parameters.AddWithValue("@Meetrichting", MeetRichting);
                cmd.Parameters.AddWithValue("@Factor", Vermenigvuldigingsfactor);
                cmd.Parameters.AddWithValue("@AantalTelwielen", AantalTelwerken);
                cmd.Parameters.AddWithValue("@TariefZone", TariefZone);
                cmd.ExecuteNonQuery();
                blnResult = true;
            }
            catch (Exception ex)
            {
                if (blnBatchProcess)
                {
                    carShared.SchrijfLog("Fout bij opslaan MasterdataTelwerken : " + ex.Message, 5, Bericht_ID, KC.App_ID);
                    
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
            return blnResult;
        }

        private Boolean SchrijfMasterdataPM()
        {
            Boolean blnResult = false;

            String strSQL = "INSERT INTO Car.dbo.MasterdataMutation \n";
            strSQL += "(Masterdata_ID \n";
            strSQL += ",Datum \n";
            strSQL += ",Dossier \n";
            strSQL += ",Reden \n";
            strSQL += ",Referentie) \n";
            strSQL += "VALUES \n";
            strSQL += "(@Masterdata_ID \n";
            strSQL += ",@Datum \n";
            strSQL += ",@Dossier \n";
            strSQL += ",@Reden \n";
            strSQL += ",@Referentie) \n";
            try
            {
                SqlCommand cmd = new SqlCommand(strSQL, conn);
                cmd.Parameters.AddWithValue("@Masterdata_ID", MasterdataID);
                cmd.Parameters.AddWithValue("@Datum", DatumPM);
                cmd.Parameters.AddWithValue("@Dossier", Dossier);
                cmd.Parameters.AddWithValue("@Reden", Reden);
                cmd.Parameters.AddWithValue("@Referentie", Referentie);
                cmd.ExecuteNonQuery();
                blnResult = true;
            }
            catch (Exception ex)
            {
                if (blnBatchProcess)
                {
                    carShared.SchrijfLog("Fout bij opslaan MasterdataMutation : " + ex.Message, 5, Bericht_ID, KC.App_ID);
                    
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }

            return blnResult;
        }
        private Boolean SchrijfMasterdataAP(String AllocatiePunt, int type)
        {
            Boolean blnResult = false;

            

            string strSQL = "INSERT INTO Car.dbo.MasterdataPAPSAP \n";
            strSQL += "(Masterdata_ID \n";
            strSQL += ", EAN18_Code \n";
            strSQL += ", Type) \n";
            strSQL += "VALUES \n";
            strSQL += "(@Masterdata_ID \n";
            strSQL += ", @EAN18_Code \n";
            strSQL += ", @Type) \n";
            try
            {
                SqlCommand cmd = new SqlCommand(strSQL, conn);
                cmd.Parameters.AddWithValue("@Masterdata_ID", MasterdataID);
                cmd.Parameters.AddWithValue("@EAN18_Code", AllocatiePunt);
                cmd.Parameters.AddWithValue("@Type", type);
                cmd.ExecuteNonQuery();
                blnResult = true;
            }
            catch (Exception ex)
            {
                if (blnBatchProcess)
                {
                    carShared.SchrijfLog("Fout bij opslaan MasterdataPAPSAP : " + ex.Message, 5, Bericht_ID, KC.App_ID);
                   
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