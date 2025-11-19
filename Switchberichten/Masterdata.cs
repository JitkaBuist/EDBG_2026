using System;
using System.Collections.Generic;
using System.Text;
using nl.Energie.EDSN;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;

namespace Energie.SwitchBericht
{
    public class Masterdata
    {
        //private string SQLstatement;
        private static String ConnString = "";
        private string HoofdPV = "";
        private string HoofdLV = "";

        #region fields
        private int edineId;
        private string transactionId;
        private string loc_GC;
        private string gridArea;
        private DateTime dtmUTCValidity;
        private string nextMeterReading;
        private string statusReason;
        private string leveranciersModel = "E01";
        private string settlementMethod;
        private string meteringMethod;
        private string profile;
        private string connectionCapacity;
        private string estimatedAnnualVolume;
        private string estimatedAnnualVolumeLow;
        private string estimatedAnnualVolumeHigh;
        private string nad_DP;
        private string brp;
        private string mv;
        private string nameIT = "";
        private string address;
        private string city;
        private string postalCode;
        private string country;
        //private string tax;
        //private string taxComplexbepaling;
        //private string taxVerblijfsfunctie;
        private string physicalStatus;
        private string administrativeStatus;
        private string consumptionCategory;
        private string typeofMeteringPoint;
        private string productID;
        //private string connectionService;
        private string capaciteitsTariefCode;
        private string physicalcapacity;
        private string houseNr;
        private string houseNrExt;
        private string meternumber;
        private string communicationStatusCode;
        private string energyAllocationMethodCode;
        private int nrOfRegisters;
        private string gridOperator;
        private string dossierID;
        private string Klant_Config;
        #endregion

        #region Constructors
        public Masterdata(
            int edineId,
            nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_PC_PMP portaal_MeteringPoint,
            string transactionId,
            string nad_DP,
            string ms,
            string mr,
            Boolean blnBatch, string klant_Config)
        {
            Klant_Config = klant_Config;
            ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB_" + Klant_Config);
            HoofdPV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV_" + Klant_Config).Trim();
            HoofdLV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV_" + Klant_Config).Trim();

            this.edineId = edineId;
            this.transactionId = transactionId;
            this.nad_DP = nad_DP;
            //EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint portaal_MeteringPoint = new Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint();
            //portaal_MeteringPoint = (nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaal_Content.Item;
            nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_PC_PMP_PM meteringPoint_Mutation = new nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_PC_PMP_PM();
            meteringPoint_Mutation = portaal_MeteringPoint.Portaal_Mutation;
            DossierID = meteringPoint_Mutation.Dossier.ID;

            LOC_GC = portaal_MeteringPoint.EANID;
            if (portaal_MeteringPoint.EDSN_AddressSearch != null)
            {
                Address = portaal_MeteringPoint.EDSN_AddressSearch.StreetName;
                HouseNr = portaal_MeteringPoint.EDSN_AddressSearch.BuildingNr;
                HouseNrExt = portaal_MeteringPoint.EDSN_AddressSearch.ExBuildingNr;
                PostalCode = portaal_MeteringPoint.EDSN_AddressSearch.ZIPCode;
                City = portaal_MeteringPoint.EDSN_AddressSearch.CityName;
                Country = portaal_MeteringPoint.EDSN_AddressSearch.Country;
            }
            GridArea = portaal_MeteringPoint.GridArea;
            if (portaal_MeteringPoint.MarketSegment != null)
            {
                ConsumptionCategory = portaal_MeteringPoint.MarketSegment.ToString();
            }

            gridOperator = portaal_MeteringPoint.GridOperator_Company.ID;
            
            ProductID = portaal_MeteringPoint.ProductType.ToString();
            //TaxComplexbepaling = portaal_MeteringPoint.MPCommercialCharacteristics.DeterminationComplex.ToString();
            //TaxVerblijfsfunctie = portaal_MeteringPoint.MPCommercialCharacteristics.Residential.ToString();
            if (portaal_MeteringPoint.MPCommercialCharacteristics.BalanceResponsibleParty_Company != null)
            {
                BRP = portaal_MeteringPoint.MPCommercialCharacteristics.BalanceResponsibleParty_Company.ID;
            }
            else
            {
                BRP = HoofdPV;
            }
            if (portaal_MeteringPoint.MPCommercialCharacteristics.BalanceSupplier_Company != null)
            {
                this.nad_DP = portaal_MeteringPoint.MPCommercialCharacteristics.BalanceSupplier_Company.ID;
            }
            else
            {
                this.nad_DP = HoofdLV;
            }
            if (portaal_MeteringPoint.MPCommercialCharacteristics.MeteringResponsibleParty_Company != null)
            {
                MV = portaal_MeteringPoint.MPCommercialCharacteristics.MeteringResponsibleParty_Company.ID;
            }
            SettlementMethod = portaal_MeteringPoint.MPPhysicalCharacteristics.AllocationMethod.ToString();
            ConnectionCapacity = portaal_MeteringPoint.MPPhysicalCharacteristics.ContractedCapacity;
            if (portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedOffPeak != "" && portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedOffPeak != null)
            {
                EstimatedAnnualVolumeHigh = portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedPeak;
                EstimatedAnnualVolumeLow = portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedOffPeak;
            }
            else
            {
                EstimatedAnnualVolume = portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedPeak;
            }

            meternumber = "";
            if (portaal_MeteringPoint.Portaal_EnergyMeter != null)
            {
                Meternumber = portaal_MeteringPoint.Portaal_EnergyMeter.ID;
                String strNrOfRegisters = portaal_MeteringPoint.Portaal_EnergyMeter.NrOfRegisters;
                int intNrOfRegisters = 0;
                int.TryParse(strNrOfRegisters, out intNrOfRegisters);
                NrOfRegisters = intNrOfRegisters;
                if (NrOfRegisters == 0 && portaal_MeteringPoint.Portaal_EnergyMeter.Register != null)
                {
                    NrOfRegisters = portaal_MeteringPoint.Portaal_EnergyMeter.Register.Length;
                }
                if (portaal_MeteringPoint.Portaal_EnergyMeter.TechnicalCommunicationSM != null)
                {
                    CommunicationStatusCode = portaal_MeteringPoint.Portaal_EnergyMeter.TechnicalCommunicationSM.ToString();
                }
            }
            AdministrativeStatus = portaal_MeteringPoint.MPPhysicalCharacteristics.EnergyDeliveryStatus.ToString();
            TypeofMeteringPoint = portaal_MeteringPoint.MPPhysicalCharacteristics.EnergyFlowDirection.ToString();
            NextMeterReading = portaal_MeteringPoint.MPPhysicalCharacteristics.InvoiceMonth; //??
            MeteringMethod = portaal_MeteringPoint.MPPhysicalCharacteristics.MeteringMethod.ToString();
            //TODO SR2017 uitzoeken wat er met Portaal_EnergyMeter.Type moet gebeuren
            //if (portaal_MeteringPoint.Portaal_EnergyMeter.Type != null)
            //{
            //    //if (portaal_MeteringPoint.Portaal_EnergyMeter.Type == nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_EnergyMeterTypeCode.DUS) { MeteringMethod = "DUS"; }
            //    //if (portaal_MeteringPoint.Portaal_EnergyMeter.Type == nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_EnergyMeterTypeCode.DUN) { MeteringMethod = "DUN"; }
            //    if (portaal_MeteringPoint.Portaal_EnergyMeter.Type == nl.Energie.EDSN.MasterData.GetMeteringPointResponseEnvelope_EnergyMeterTypeCode) { MeteringMethod = "SLM"; }
            //}
            Physicalcapacity = portaal_MeteringPoint.MPPhysicalCharacteristics.PhysicalCapacity.ToString();
            PhysicalStatus = portaal_MeteringPoint.MPPhysicalCharacteristics.PhysicalStatus.ToString();
            Profile = portaal_MeteringPoint.MPPhysicalCharacteristics.ProfileCategory.ToString();
            DtmUTCValidity = meteringPoint_Mutation.MutationDate.ToUniversalTime();
            StatusReason = meteringPoint_Mutation.MutationReason.ToString();
            if (meteringPoint_Mutation.Dossier != null)
            {
                DossierID = meteringPoint_Mutation.Dossier.ID;
            }
            if (portaal_MeteringPoint.MPPhysicalCharacteristics.AllocationMethod != null)
            {
                EnergyAllocationMethodCode = portaal_MeteringPoint.MPPhysicalCharacteristics.AllocationMethod.ToString();
            }
            if (portaal_MeteringPoint.MPPhysicalCharacteristics != null)
            {
                if (portaal_MeteringPoint.MPPhysicalCharacteristics.CapTarCode != null)
                {
                    CapaciteitsTariefCode = portaal_MeteringPoint.MPPhysicalCharacteristics.CapTarCode.ToString();
                }
            }
            Save_E07_Header(edineId, DateTime.Now, ms, mr, blnBatch);
            Save_E07(blnBatch);
           
        }

        public Masterdata(
            int edineId,
            nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP portaal_MeteringPoint,
            string transactionId,
            string nad_DP,
            string ms,
            string mr,
            Boolean blnBatch, string klant_Config)
        {
            try
            {
                Klant_Config = klant_Config;
                ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB_" + Klant_Config);
                HoofdPV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV_" + Klant_Config).Trim();
                HoofdLV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV_" + Klant_Config).Trim();

                this.edineId = edineId;
                this.transactionId = transactionId;
                this.nad_DP = nad_DP;
                //EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint portaal_MeteringPoint = new Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint();
                //portaal_MeteringPoint = (nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaal_Content.Item;
                nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP_PM meteringPoint_Mutation = new nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_PC_PMP_PM();
                meteringPoint_Mutation = portaal_MeteringPoint.Portaal_Mutation[0];

                LOC_GC = portaal_MeteringPoint.EANID;
                if (portaal_MeteringPoint.EDSN_AddressSearch != null)
                {
                    Address = portaal_MeteringPoint.EDSN_AddressSearch.StreetName;
                    HouseNr = portaal_MeteringPoint.EDSN_AddressSearch.BuildingNr;
                    HouseNrExt = portaal_MeteringPoint.EDSN_AddressSearch.ExBuildingNr;
                    PostalCode = portaal_MeteringPoint.EDSN_AddressSearch.ZIPCode;
                    City = portaal_MeteringPoint.EDSN_AddressSearch.CityName;
                    Country = portaal_MeteringPoint.EDSN_AddressSearch.Country;
                }
                GridArea = portaal_MeteringPoint.GridArea;
                if (portaal_MeteringPoint.MarketSegment != null)
                {
                    ConsumptionCategory = portaal_MeteringPoint.MarketSegment.ToString();
                }

                gridOperator = portaal_MeteringPoint.GridOperator_Company.ID;
                ProductID = portaal_MeteringPoint.ProductType.ToString();
                //if (portaal_MeteringPoint.MPCommercialCharacteristics.DeterminationComplex != null)
                //{
                //    TaxComplexbepaling = portaal_MeteringPoint.MPCommercialCharacteristics.DeterminationComplex.ToString();
                //}
                //if (portaal_MeteringPoint.MPCommercialCharacteristics.Residential != null)
                //{
                //    TaxVerblijfsfunctie = portaal_MeteringPoint.MPCommercialCharacteristics.Residential.ToString();
                //}
                BRP = portaal_MeteringPoint.MPCommercialCharacteristics.BalanceResponsibleParty_Company.ID;
                this.nad_DP = portaal_MeteringPoint.MPCommercialCharacteristics.BalanceSupplier_Company.ID;
                if (portaal_MeteringPoint.MPCommercialCharacteristics.MeteringResponsibleParty_Company != null)
                {
                    MV = portaal_MeteringPoint.MPCommercialCharacteristics.MeteringResponsibleParty_Company.ID;
                }
                SettlementMethod = portaal_MeteringPoint.MPPhysicalCharacteristics.AllocationMethod.ToString();
                ConnectionCapacity = portaal_MeteringPoint.MPPhysicalCharacteristics.ContractedCapacity;
                if (portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedOffPeak != null && portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedOffPeak != "")
                {
                    EstimatedAnnualVolumeHigh = portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedPeak;
                    EstimatedAnnualVolumeLow = portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedOffPeak;
                }
                else
                {
                    if (portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedPeak != null)
                    {
                        EstimatedAnnualVolume = portaal_MeteringPoint.MPPhysicalCharacteristics.EAEnergyConsumptionNettedPeak;
                    }
                }
                AdministrativeStatus = portaal_MeteringPoint.MPPhysicalCharacteristics.EnergyDeliveryStatus.ToString();
                TypeofMeteringPoint = portaal_MeteringPoint.MPPhysicalCharacteristics.EnergyFlowDirection.ToString();
                if (portaal_MeteringPoint.MPPhysicalCharacteristics != null)
                {
                    if (portaal_MeteringPoint.MPPhysicalCharacteristics.InvoiceMonth != null)
                    {
                        NextMeterReading = portaal_MeteringPoint.MPPhysicalCharacteristics.InvoiceMonth; //??
                    }
                }
                MeteringMethod = portaal_MeteringPoint.MPPhysicalCharacteristics.MeteringMethod.ToString();
                Meternumber = "";
                if (portaal_MeteringPoint.Portaal_EnergyMeter != null)
                {
                    Meternumber = portaal_MeteringPoint.Portaal_EnergyMeter.ID;
                    String strNrOfRegisters = portaal_MeteringPoint.Portaal_EnergyMeter.NrOfRegisters;
                    int intNrOfRegisters = 0;
                    int.TryParse(strNrOfRegisters, out intNrOfRegisters);
                    NrOfRegisters = intNrOfRegisters;
                    if (NrOfRegisters == 0 && portaal_MeteringPoint.Portaal_EnergyMeter.Register != null)
                    {
                        NrOfRegisters = portaal_MeteringPoint.Portaal_EnergyMeter.Register.Length;
                    }
                    if (portaal_MeteringPoint.Portaal_EnergyMeter.TechnicalCommunicationSM != null)
                    {
                        CommunicationStatusCode = portaal_MeteringPoint.Portaal_EnergyMeter.TechnicalCommunicationSM.ToString();
                    }
                    //TODO uitzoeken wat er met Portaal_EnergyMeter.Type moet gebeuren
                    //if (portaal_MeteringPoint.Portaal_EnergyMeter.Type != null)
                    //{
                    //    //if (portaal_MeteringPoint.Portaal_EnergyMeter.Type == nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_EnergyMeterTypeCode.DUS) { MeteringMethod = "DUS"; }
                    //    //if (portaal_MeteringPoint.Portaal_EnergyMeter.Type == nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_EnergyMeterTypeCode.DUN) { MeteringMethod = "DUN"; }
                    //    if (portaal_MeteringPoint.Portaal_EnergyMeter.Type == nl.Energie.EDSN.MasterDataUpdate.MasterDataUpdateResponseEnvelope_EnergyMeterTypeCode.SLM) { MeteringMethod = "SLM"; }
                    //}
                }

                if (portaal_MeteringPoint.MPPhysicalCharacteristics.PhysicalCapacity != null)
                {
                    Physicalcapacity = portaal_MeteringPoint.MPPhysicalCharacteristics.PhysicalCapacity.ToString();
                }
                PhysicalStatus = portaal_MeteringPoint.MPPhysicalCharacteristics.PhysicalStatus.ToString();
                if (portaal_MeteringPoint.MPPhysicalCharacteristics.ProfileCategory != null)
                {
                    Profile = portaal_MeteringPoint.MPPhysicalCharacteristics.ProfileCategory.ToString();
                }
                DtmUTCValidity = meteringPoint_Mutation.MutationDate.ToUniversalTime();
                StatusReason = meteringPoint_Mutation.MutationReason.ToString();
                if (meteringPoint_Mutation.Dossier != null)
                {
                    DossierID = meteringPoint_Mutation.Dossier.ID;
                }
                if (portaal_MeteringPoint.MPPhysicalCharacteristics.AllocationMethod != null)
                {
                    EnergyAllocationMethodCode = portaal_MeteringPoint.MPPhysicalCharacteristics.AllocationMethod.ToString();
                }
                if (portaal_MeteringPoint.MPPhysicalCharacteristics.CapTarCode != null)
                {
                    CapaciteitsTariefCode = portaal_MeteringPoint.MPPhysicalCharacteristics.CapTarCode.ToString();
                }
                
                Save_E07_Header(edineId, DateTime.Now, ms, mr, blnBatch);
                Save_E07(blnBatch);
            }
            catch (Exception ex)
            {
                if (blnBatch)
                {
                    WriteLog("Fout in Masterdata class: " + ex.Message, 10, -1);
                }
                else
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public Masterdata(
            int edineId,
            nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope_PC_PMP portaal_MeteringPoint,
            string transactionId,
            string nad_DP,
            string ms,
            string mr,
            Boolean blnBatch, string klant_Config)
        {
            Klant_Config = klant_Config;
            String ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB_" + Klant_Config);
            HoofdPV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV_" + Klant_Config).Trim();
            HoofdLV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdLV_" + Klant_Config).Trim();
            this.edineId = edineId;
            this.transactionId = transactionId;
            this.nad_DP = nad_DP;
            //EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint portaal_MeteringPoint = new Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint();
            //portaal_MeteringPoint = (nl.Energie.EDSN.MasterData.MasterDataResponseEnvelope_Portaal_Content_Portaal_MeteringPoint)portaal_Content.Item;
            nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope_PC_PMP_PM meteringPoint_Mutation = new nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope_PC_PMP_PM();
            meteringPoint_Mutation = portaal_MeteringPoint.Portaal_Mutation;

            LOC_GC = portaal_MeteringPoint.EANID;
            if (portaal_MeteringPoint.EDSN_AddressExtended != null)
            {
                Address = portaal_MeteringPoint.EDSN_AddressExtended.StreetName;
                HouseNr = portaal_MeteringPoint.EDSN_AddressExtended.BuildingNr;
                HouseNrExt = portaal_MeteringPoint.EDSN_AddressExtended.ExBuildingNr;
                PostalCode = portaal_MeteringPoint.EDSN_AddressExtended.ZIPCode;
                City = portaal_MeteringPoint.EDSN_AddressExtended.CityName;
                Country = portaal_MeteringPoint.EDSN_AddressExtended.Country;
            }
            GridArea = portaal_MeteringPoint.GridArea;
            //if (portaal_MeteringPoint.MarketSegment != null)
            //{
            ConsumptionCategory = portaal_MeteringPoint.MarketSegment.ToString();
            //}
            ProductID = portaal_MeteringPoint.ProductType.ToString();
            //TaxComplexbepaling = portaal_MeteringPoint.MPCommercialCharacteristics.DeterminationComplex.ToString();
            //TaxVerblijfsfunctie = portaal_MeteringPoint.MPCommercialCharacteristics.Residential.ToString();
            BRP = portaal_MeteringPoint.MPCommercialCharacteristics.BalanceResponsibleParty_Company.ID;
            this.nad_DP = portaal_MeteringPoint.MPCommercialCharacteristics.BalanceSupplier_Company.ID;
            if (portaal_MeteringPoint.MPCommercialCharacteristics.MeteringResponsibleParty_Company != null)
            {
                MV = portaal_MeteringPoint.MPCommercialCharacteristics.MeteringResponsibleParty_Company.ID;
            }
            SettlementMethod = portaal_MeteringPoint.MPPhysicalCharacteristics.AllocationMethod.ToString();
            ConnectionCapacity = portaal_MeteringPoint.MPPhysicalCharacteristics.ContractedCapacity;
            if (portaal_MeteringPoint.MPPhysicalCharacteristics.EACOffPeak != "")
            {
                EstimatedAnnualVolumeHigh = portaal_MeteringPoint.MPPhysicalCharacteristics.EACPeak;
                EstimatedAnnualVolumeLow = portaal_MeteringPoint.MPPhysicalCharacteristics.EACOffPeak;
            }
            else
            {
                EstimatedAnnualVolume = portaal_MeteringPoint.MPPhysicalCharacteristics.EACPeak;
            }
            AdministrativeStatus = portaal_MeteringPoint.MPPhysicalCharacteristics.EnergyDeliveryStatus.ToString();
            TypeofMeteringPoint = portaal_MeteringPoint.MPPhysicalCharacteristics.EnergyFlowDirection.ToString();
            NextMeterReading = portaal_MeteringPoint.MPPhysicalCharacteristics.InvoiceMonth; //??
            MeteringMethod = portaal_MeteringPoint.MPPhysicalCharacteristics.MeteringMethod.ToString();
            //if (portaal_MeteringPoint.Portaal_EnergyMeter.Type != null)
            //{
                //if (portaal_MeteringPoint.Portaal_EnergyMeter.Type == nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope_EnergyMeterTypeCode.DUS) { MeteringMethod = "DUS"; }
                //if (portaal_MeteringPoint.Portaal_EnergyMeter.Type == nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope_EnergyMeterTypeCode.DUN) { MeteringMethod = "DUN"; }
                if (portaal_MeteringPoint.Portaal_EnergyMeter.Type == nl.Energie.EDSN.MeteringPointInformation.GetMeteringPointResponseEnvelope_EnergyMeterTypeCode.SLM) { MeteringMethod = "SLM"; }
            //}
            Physicalcapacity = portaal_MeteringPoint.MPPhysicalCharacteristics.PhysicalCapacity.ToString();
            PhysicalStatus = portaal_MeteringPoint.MPPhysicalCharacteristics.PhysicalStatus.ToString();
            Profile = portaal_MeteringPoint.MPPhysicalCharacteristics.ProfileCategory.ToString();
            DtmUTCValidity = DateTime.Now;
            dtmUTCValidity = dtmUTCValidity.AddMinutes(-dtmUTCValidity.Minute );
            dtmUTCValidity = dtmUTCValidity.AddSeconds(-dtmUTCValidity.Second );
            dtmUTCValidity = dtmUTCValidity.AddHours(-dtmUTCValidity.Hour);
            dtmUTCValidity = dtmUTCValidity.AddMilliseconds(-dtmUTCValidity.Millisecond);
            StatusReason = "DSTRMSTR";
            if (portaal_MeteringPoint.MPPhysicalCharacteristics != null)
            {
                if (portaal_MeteringPoint.MPPhysicalCharacteristics.CapTarCode != null)
                {
                    CapaciteitsTariefCode = portaal_MeteringPoint.MPPhysicalCharacteristics.CapTarCode.ToString();
                }
            }
            Save_E07_Header(edineId, DateTime.Now, ms, mr, blnBatch);
            Save_E07(blnBatch);

        }

        #endregion  
        public string TransactionId
        {
            get { return this.transactionId; }
            set { this.transactionId = value; }
        }
        public string LOC_GC
        {
            get { return this.loc_GC; }
            set { this.loc_GC = value; }
        }
        public string GridArea
        {
            get { return this.gridArea; }
            set { this.gridArea = value; }
        }
        public DateTime DtmUTCValidity
        {
            get { return this.dtmUTCValidity; }
            set { this.dtmUTCValidity = value; }
        }
        public string NextMeterReading
        {
            get { return this.nextMeterReading; }
            set { this.nextMeterReading = value; }
        }
        public string StatusReason
        {
            //Edine coderingen
            //E01 = Change of party connected to Grid
            //E03 = Change of Balance Supplier
            //E20 = End of Supply
            //E21 = Change of party Connected to Grid
            //E32 = Update Master Data MP
            //E48 = Masterdata for prospects
            //E57 = Switch of metered data responsible
            //E68 = Switch of Transport capacity responsible party
            //E75 = Change of metering method
            //E77 = End of metering
            //E78 = Change of Billing model

            get { return this.statusReason; }
            set
            {
                //Vertalen
                switch (value)
                {
                    case "DSTRMSTR": this.statusReason = "E01";//Niet zeker
                        break;
                }
            }
        }
        public string LeveranciersModel
        {
            //Edine coderingen
            //E01 = Contract directly with customer //Netbeheerdersmodel
            //E03 = Contract via Supplier //Leveranciersmodel
            get { return this.leveranciersModel; }
            //set onbekend nog niet gevonden in model
        }

        public string SettlementMethod
        {
            //E01 Profile
            //E02 Non Profiled
            get { return this.settlementMethod; }
            set
            {
                if (value == "PRF") { this.settlementMethod = "E01"; }
                if (value == "TMT") { this.settlementMethod = "E02"; }
            }
        }


        public string MeteringMethod
        {
            //E13 = Continuous
            //E14 = Non Continuous (Jaar - keuze Energie)
            //E16 = Not Metered 
            //E24 = Calculated 
            //007 = Slimme meter pre-NTA
            //008 = Slimme meter NTA8130 versie 1
            //009 = Maand (zelf gedef. Energie)
            get { return this.meteringMethod;}
            set 
            {
                switch (value)
                {
                    case "JRL": this.meteringMethod = "E14";
                        break;
                    case "MND": this.meteringMethod = "009"; // Nog aanpassen in ProcessMessages
                        break;
                    case "OBM": this.meteringMethod = "E16";
                        break;
                    case "TMT": this.meteringMethod = "E13";
                        break;
                    case "DUS": this.meteringMethod = "008";
                        break;
                    case "DUN": this.meteringMethod = "007";
                        break;
                    
                    //verder als duidelijk is
                }
            }
        }
        public string Profile
        {
            get { return this.profile; }
            set
            {
                if (value.Length >= 2)
                {
                    this.profile = value.Substring(1);
                }
            }
        }
        public string ConnectionCapacity
        {
            get { return this.connectionCapacity; }
            set { this.connectionCapacity = value; }
        }
        public string EstimatedAnnualVolume
        {
            get { return this.estimatedAnnualVolume; }
            set { this.estimatedAnnualVolume = value; }
        }
        public string EstimatedAnnualVolumeLow
        {
            get { return this.estimatedAnnualVolumeLow; }
            set { this.estimatedAnnualVolumeLow = value; }
        }
        public string EstimatedAnnualVolumeHigh
        {
            get { return this.estimatedAnnualVolumeHigh; }
            set { this.estimatedAnnualVolumeHigh = value; }
        }
        public String NAD_DP
        {
            get { return this.nad_DP; }
            set { this.nad_DP = value; }
        }
        public String BRP
        {
            get { return this.brp; }
            set { this.brp = value; }
        }
        public String MV
        {
            get { return this.mv; }
            set { this.mv = value; }
        }
        public string NameIT
        {
            get { return this.nameIT; }
            set { this.nameIT = value; }
        }
        public string Address
        {
            get { return this.address; }
            set { this.address = value; }
        }
        public string City
        {
            get { return this.city; }
            set { this.city = value; }
        }
        public string PostalCode
        {
            get { return this.postalCode; }
            set { this.postalCode = value; }
        }
        public string Country
        {
            get { return this.country; }
            set { this.country = value; }
        }
        //public string Tax //Bestaat dit nog???
        //{
        //    //"00") = geen complexbepaling, geen verblijfsfunctie
        //    //"01") = geen complexbepaling, wel verblijfsfunctie
        //    //"10") = onderdeel van complex , geen verblijfsfunctie
        //    //"11") = onderdeel van complex met verblijfsfunctie
        //    //"22") = nvt, code is alleen bedoeld voor 1x6A geschakeld net
        //    get
        //    {
        //        tax = "00";
        //        if (taxComplexbepaling == "J") { this.tax = "1" + this.tax.Substring(1); }
        //        if (taxVerblijfsfunctie == "J") { this.tax = this.tax.Substring(0, 1) + "1"; }
        //        return tax;
        //    }
        //}
        //private string TaxComplexbepaling
        //{
        //    set { this.taxComplexbepaling = value; }
        //}
        //private string TaxVerblijfsfunctie
        //{
        //    set { this.taxVerblijfsfunctie = value; }
        //}
        public string PhysicalStatus
        {
            //E22 = Operative
            //E23 = Inoperative
            //IAL = In Aanleg
            //IBD = In bedrijf
            //UBD = Uit bedrijf
            //SLP = Gesloopt

            get { return this.physicalStatus; }
            set { this.physicalStatus = value; }
        }
        public string AdministrativeStatus
        {
            //E22 = In gebruik
            //E23 = Uit in gebruik
            get { return this.administrativeStatus; }
            set
            {
                switch (value)
                {
                    case "INA": this.administrativeStatus = "E23";
                        break;
                    case "ACT": this.administrativeStatus = "E22";
                        break;
                }
            }
        }
        public string ConsumptionCategory 
        {
            //"C01" = Retail/KV 
            //"C02" = WholeSale/GV
            get { return this.consumptionCategory; }
            set
            {
                if (value == "KVB") { this.consumptionCategory = "C01"; }
                if (value == "GVB") { this.consumptionCategory = "C02"; }
            }
        }
        public string TypeofMeteringPoint
        {
            //"E17") = Consumption
            //"E18") = Production
            //"E19") = Combined
            get { return this.typeofMeteringPoint; }
            set
            {
                if (value == "LVR") { this.typeofMeteringPoint = "E17"; }
                if (value == "TLV") { this.typeofMeteringPoint = "E18"; }
                if (value == "CMB") { this.typeofMeteringPoint = "E19"; }
            }
        }
        public string ProductID
        {
            //8716867000030 = E
            //5410000100016 = G
            get { return this.productID;}
            set 
            {
                if (value == "GAS") 
                {
                    this.productID = "5410000100016";
                }
                else
                {
                    this.productID = "8716867000030";
                }
            }
        }
        public string ConnectionService; //geen idee
        public string CapaciteitsTariefCode
        {
            get { return this.capaciteitsTariefCode; }
            set { this.capaciteitsTariefCode = value; }
        }
        public string Physicalcapacity
        {
            //zie tabel EnergieDB.dbo.FysiekecapaciteitTypes
            get { return this.physicalcapacity; }
            set
            {
                //In toekomst via tabel?
                switch (value)
                {
                    case "Item1x6": this.physicalcapacity = "E01"; break;
                    case "Item1x25": this.physicalcapacity = "E02"; break;
                    case "Item1x35": this.physicalcapacity = "E03"; break;
                    case "Item1x50": this.physicalcapacity = "E04"; break;
                    case "Item1x63": this.physicalcapacity = "E05"; break;
                    case "Item1x80": this.physicalcapacity = "E06"; break;
                    case "Item3x25": this.physicalcapacity = "E07"; break;
                    case "Item3x35": this.physicalcapacity = "E08"; break;
                    case "Item3x50": this.physicalcapacity = "E09"; break;
                    case "Item3x63": this.physicalcapacity = "E10"; break;
                    case "Item3x80": this.physicalcapacity = "E11"; break;
                    case "G4": this.physicalcapacity = "G4"; break;
                    case "G6": this.physicalcapacity = "G6"; break;
                    case "G10": this.physicalcapacity = "G10"; break;
                    case "G16": this.physicalcapacity = "G16"; break;
                    case "G25": this.physicalcapacity = "G25"; break;
                    case "G40": this.physicalcapacity = "G40"; break;
                    case "G65": this.physicalcapacity = "G65"; break;
                    case "G100": this.physicalcapacity = "G100"; break;
                    case "G160": this.physicalcapacity = "G160"; break;
                    case "G250": this.physicalcapacity = "G250"; break;
                    case "G400": this.physicalcapacity = "G400"; break;
                    case "G650": this.physicalcapacity = "G650"; break;
                    case "G1000": this.physicalcapacity = "G1000"; break;
                    case "G1600": this.physicalcapacity = "G1600"; break;
                    case "G2500": this.physicalcapacity = "G2500"; break;
                    case "OBK": this.physicalcapacity = "E00"; break;
                        //nog geen gas
                }
            }
        }

        public string HouseNr
        {
            get { return this.houseNr; }
            set { this.houseNr = value; }
        }
        public string HouseNrExt
        {
            get { return this.houseNrExt; }
            set { this.houseNrExt = value; }
        }
        public string Meternumber
        {
            get { return this.meternumber; }
            set { this.meternumber = value; }
        }
        public string CommunicationStatusCode
        {
            get { return this.communicationStatusCode; }
            set { this.communicationStatusCode = value; }
        }
        public string EnergyAllocationMethodCode
        {
            get { return this.energyAllocationMethodCode; }
            set { this.energyAllocationMethodCode = value; }
        }
        public int NrOfRegisters
        {
            get { return this.nrOfRegisters; }
            set { this.nrOfRegisters = value; }
        }

        public string GridOperator
        {
            get { return this.gridOperator; }
            set { this.gridOperator = value; }
        }
        public string DossierID
        {
            get { return this.dossierID; }
            set { this.dossierID = value; }
        }

        private void Save_E07(Boolean blnBatch)
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;

            cnPubs.Open();
            SQLstatement =
                    "INSERT INTO [Messages].[dbo].[UTILMD_E07] " +
                    "([EdineId] " +
                    ",[TransactionId] " +
                    ",[LOC_GC] " +
                    ",[GridArea] " +
                    ",[dtmUTCValidity] " +
                    ",[NextMeterReading] " +
                    ",[StatusCategory] " +
                    ",[StatusReason] " +
                    ",[LeveranciersModel] " +
                    ",[SettlementMethod] " +
                    ",[MeteringMethod] " +
                    ",[Profile] " +
                    ",[ConnectionCapacity] " +
                    ",[EstimatedAnnualVolume] " +
                    ",[EstimatedAnnualVolumeLow] " +
                    ",[EstimatedAnnualVolumeHigh] " +
                    ",[NAD_DP] " +
                    ",[BRP] " +
                    ",[MV] " +
                    ",[NameIT] " +
                    ",[Address] " +
                    ",[City] " +
                    ",[PostalCode] " +
                    ",[Country] " +
                    //",[TAX] " +
                    ",[PhysicalStatus] " +
                    ",[AdministrativeStatus] " +
                    ",[ConsumptionCategory] " +
                    ",[TypeofMeteringPoint] " +
                    ",[ProductID] " +
                    ",[ConnectionService] " +
                    ",[CapaciteitsTariefCode] " +
                    ",[physicalcapacity] " +
                    ",[HouseNr] " +
                    ",[HouseNrExt] " +
                    ",[MeterNumber] " +
                    ",[CommunicationStatusCode] " +
                    ",[EnergyAllocationMethod] " +
                    ",[NrOfRegisters] " +
                    ",[GridOperator] " +
                    ",[DossierID]) " +
                    "VALUES " +
                    "(@EdineId " +
                    ",@TransactionId " +
                    ",@LOC_GC " +
                    ",@GridArea " +
                    ",@dtmUTCValidity " +
                    ",@NextMeterReading " +
                    ",'7' " +
                    ",@StatusReason " +
                    ",@LeveranciersModel " +
                    ",@SettlementMethod " +
                    ",@MeteringMethod " +
                    ",@Profile " +
                    ",@ConnectionCapacity " +
                    ",@EstimatedAnnualVolume " +
                    ",@EstimatedAnnualVolumeLow " +
                    ",@EstimatedAnnualVolumeHigh " +
                    ",@NAD_DP " +
                    ",@BRP " +
                    ",@MV " +
                    ",@NameIT " +
                    ",@Address " +
                    ",@City " +
                    ",@PostalCode " +
                    ",@Country " +
                    //",@TAX " +
                    ",@PhysicalStatus " +
                    ",@AdministrativeStatus " +
                    ",@ConsumptionCategory " +
                    ",@TypeofMeteringPoint " +
                    ",@ProductID " +
                    ",@ConnectionService " +
                    ",@CapaciteitsTariefCode " +
                    ",@physicalcapacity " +
                    ",@HouseNr " +
                    ",@HouseNrExt " +
                    ",@MeterNumber " +
                    ",@CommunicationStatusCode " +
                    ",@EnergyAllocationMethod " +
                    ",@NrOfRegisters " +
                    ",@GridOperator " +
                    ",@DossierID) ";


            SqlCommand cmdSaveEdine = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveEdine.Parameters.Add(new SqlParameter("@edineID", SqlDbType.Int));
            cmdSaveEdine.Parameters["@edineID"].Value = edineId;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@TransactionId", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@TransactionId"].Value = TransactionId;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@LOC_GC", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@LOC_GC"].Value = LOC_GC;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@GridArea", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@GridArea"].Value = GridArea;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@dtmUTCValidity", SqlDbType.DateTime));
            cmdSaveEdine.Parameters["@dtmUTCValidity"].Value = DtmUTCValidity;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@NextMeterReading", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@NextMeterReading"].Value = String.IsNullOrEmpty(NextMeterReading) ? (object)DBNull.Value : (object)NextMeterReading;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@StatusReason", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@StatusReason"].Value = String.IsNullOrEmpty(StatusReason) ? (object)DBNull.Value : (object)StatusReason; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@LeveranciersModel", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@LeveranciersModel"].Value = String.IsNullOrEmpty(LeveranciersModel) ? (object)DBNull.Value : (object)LeveranciersModel; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@SettlementMethod", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@SettlementMethod"].Value = String.IsNullOrEmpty(SettlementMethod) ? (object)DBNull.Value : (object)SettlementMethod; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@MeteringMethod", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@MeteringMethod"].Value = String.IsNullOrEmpty(MeteringMethod) ? (object)DBNull.Value : (object)MeteringMethod; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@Profile", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@Profile"].Value = String.IsNullOrEmpty(Profile) ? (object)DBNull.Value : (object)Profile; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@ConnectionCapacity", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@ConnectionCapacity"].Value = String.IsNullOrEmpty(ConnectionCapacity) ? (object)DBNull.Value : (object)ConnectionCapacity; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@EstimatedAnnualVolume", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@EstimatedAnnualVolume"].Value = String.IsNullOrEmpty(EstimatedAnnualVolume) ? (object)DBNull.Value : (object)EstimatedAnnualVolume; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@EstimatedAnnualVolumeLow", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@EstimatedAnnualVolumeLow"].Value = String.IsNullOrEmpty(EstimatedAnnualVolumeLow) ? (object)DBNull.Value : (object)EstimatedAnnualVolumeLow;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@EstimatedAnnualVolumeHigh", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@EstimatedAnnualVolumeHigh"].Value = String.IsNullOrEmpty(EstimatedAnnualVolumeHigh) ? (object)DBNull.Value : (object)EstimatedAnnualVolumeHigh;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@NAD_DP", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@NAD_DP"].Value = NAD_DP;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@BRP", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@BRP"].Value = BRP;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@MV", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@MV"].Value = String.IsNullOrEmpty(MV) ? (object)DBNull.Value : (object)MV; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@NameIT", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@NameIT"].Value = String.IsNullOrEmpty(NameIT) ? (object)DBNull.Value : (object)NameIT; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@Address", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@Address"].Value = String.IsNullOrEmpty(Address) ? (object)DBNull.Value : (object)Address; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@City", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@City"].Value = String.IsNullOrEmpty(City) ? (object)DBNull.Value : (object)City; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@PostalCode", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@PostalCode"].Value = String.IsNullOrEmpty(PostalCode) ? (object)DBNull.Value : (object)PostalCode; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@Country", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@Country"].Value = String.IsNullOrEmpty(Country) ? (object)DBNull.Value : (object)Country; 
            //cmdSaveEdine.Parameters.Add(new SqlParameter("@TAX", SqlDbType.VarChar));
            //cmdSaveEdine.Parameters["@TAX"].Value = String.IsNullOrEmpty(Tax) ? (object)DBNull.Value : (object)Tax; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@PhysicalStatus", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@PhysicalStatus"].Value = String.IsNullOrEmpty(PhysicalStatus) ? (object)DBNull.Value : (object)PhysicalStatus; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@AdministrativeStatus", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@AdministrativeStatus"].Value = String.IsNullOrEmpty(AdministrativeStatus) ? (object)DBNull.Value : (object)AdministrativeStatus; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@ConsumptionCategory", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@ConsumptionCategory"].Value = String.IsNullOrEmpty(ConsumptionCategory) ? (object)DBNull.Value : (object)ConsumptionCategory;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@TypeofMeteringPoint", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@TypeofMeteringPoint"].Value = String.IsNullOrEmpty(TypeofMeteringPoint) ? (object)DBNull.Value : (object)TypeofMeteringPoint; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@ProductID"].Value = String.IsNullOrEmpty(ProductID) ? (object)DBNull.Value : (object)ProductID; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@ConnectionService", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@ConnectionService"].Value = String.IsNullOrEmpty(ConnectionService) ? (object)DBNull.Value : (object)ConnectionService; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@CapaciteitsTariefCode", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@CapaciteitsTariefCode"].Value = String.IsNullOrEmpty(CapaciteitsTariefCode) ? (object)DBNull.Value : (object)CapaciteitsTariefCode; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@physicalcapacity", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@physicalcapacity"].Value = String.IsNullOrEmpty(physicalcapacity) ? (object)DBNull.Value : (object)physicalcapacity; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@HouseNr", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@HouseNr"].Value = String.IsNullOrEmpty(HouseNr) ? (object)DBNull.Value : (object)HouseNr; 
            cmdSaveEdine.Parameters.Add(new SqlParameter("@HouseNrExt", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@HouseNrExt"].Value = String.IsNullOrEmpty(HouseNrExt) ? (object)DBNull.Value : (object)HouseNrExt;
            cmdSaveEdine.Parameters.AddWithValue("@MeterNumber", Meternumber);
            if (CommunicationStatusCode == null) { CommunicationStatusCode = ""; }
            cmdSaveEdine.Parameters.AddWithValue("@CommunicationStatusCode", CommunicationStatusCode);
            if (EnergyAllocationMethodCode == null) { EnergyAllocationMethodCode = ""; }
            cmdSaveEdine.Parameters.AddWithValue("@EnergyAllocationMethod", EnergyAllocationMethodCode);
            cmdSaveEdine.Parameters.AddWithValue("@GridOperator", GridOperator);
            cmdSaveEdine.Parameters.AddWithValue("@DossierID", DossierID);
            
            cmdSaveEdine.Parameters.AddWithValue("@NrOfRegisters", NrOfRegisters);

            try
            {
                cmdSaveEdine.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (blnBatch)
                {
                    WriteLog("Fout bij opslaan E07 : " + ex.Message, 10, edineId);
                }
                else
                {
                    MessageBox.Show("Er is iets fout gegaan met het bewaren van het Masterdata Electra (E07), we adviseren U contact op te nemen met IT");
                    MessageBox.Show(ex.ToString());
                }
            }
            cnPubs.Close();
        }
        private void Save_E07_Header(int edineID, DateTime messageProcessed, string ms, string mr, Boolean blnBatch)
        {
            SqlConnection cnPubs = new SqlConnection(ConnString);
            string SQLstatement;

            cnPubs.Open();
            SQLstatement =
                    "INSERT INTO [Messages].[dbo].[UTILMD_E07_Header] " +
                    "([EdineId] " +
                    ",[MessageProcessed] " +
                    ",[MS] " +
                    ",[MR]) " +
                    "VALUES " +
                    "(@EdineId " +
                    ",@MessageProcessed " +
                    ",@MS " +
                    ",@MR)";


            SqlCommand cmdSaveEdine = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveEdine.Parameters.Add(new SqlParameter("@EdineId", SqlDbType.Int));
            cmdSaveEdine.Parameters["@EdineId"].Value = edineID;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@MessageProcessed", SqlDbType.DateTime));
            cmdSaveEdine.Parameters["@MessageProcessed"].Value = messageProcessed;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@MS", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@MS"].Value = ms;
            cmdSaveEdine.Parameters.Add(new SqlParameter("@MR", SqlDbType.VarChar));
            cmdSaveEdine.Parameters["@MR"].Value = mr;

            try
            {
                cmdSaveEdine.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (blnBatch)
                {
                    WriteLog("Fout bij opslaan E07_Header : " + ex.Message, 10, edineID);
                }
                else
                {
                    MessageBox.Show("Er is iets fout gegaan met het bewaren van het Masterdata Electra (E07_Header), we adviseren U contact op te nemen met IT");
                    MessageBox.Show(ex.ToString());
                }
            }
            cnPubs.Close();
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
