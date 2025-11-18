using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Energie.Car
{
    public class VerbruikTelwerk
    {
        private int verbruik_ID;
        private string richting;
        private string tariefType;
        private string meetEenheid;
        private Int16 aantalTelwielen;
        private int volume;
        private int gecorrigeerdVolume = 0;
        private DateTime beginDatum = DateTime.MinValue;
        private int beginStand;
        private string herkomstBegin;
        private DateTime eindDatum = DateTime.MinValue;
        private int eindStand;
        private string herkomstEind;
        private Boolean gesplitst;

        //private string strSQL;
        private SqlConnection conn;

        public VerbruikTelwerk(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP portaal_meteringPoint, Int32 intBerichtID, Int32 intVerbruik_ID, Verbruik verbruik)
        {
            conn = new SqlConnection(KC.ConnString);
            conn.Open();

            if (portaal_meteringPoint.Items[0].GetType() == typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Volume))
            {
                //Verbruiken

                nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Volume portaal_Volume;
                for (int i2 = 0; i2 < portaal_meteringPoint.Items.Length; i2++)
                {


                    portaal_Volume = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Volume)portaal_meteringPoint.Items[i2];

                    if (portaal_Volume.Reading != null)
                    {
                        for (int i4 = 0; i4 < portaal_Volume.Reading.Length; i4++)
                        {
                            if (beginDatum == DateTime.MinValue) { beginDatum = portaal_Volume.Reading[i4].ReadingDate; }
                            if (eindDatum == DateTime.MinValue) { eindDatum = portaal_Volume.Reading[i4].ReadingDate; }
                            if (portaal_Volume.Reading[i4].ReadingDate > beginDatum) { eindDatum = portaal_Volume.Reading[i4].ReadingDate; }
                        }
                    }
                    else
                    {
                        beginDatum = portaal_Volume.ValidFromDate;
                        eindDatum = portaal_Volume.ValidToDate;
                    }

                    verbruik_ID = intVerbruik_ID;
                    volume = int.Parse(portaal_Volume.Volume);
                    meetEenheid = "";


                    if (portaal_Volume.Register != null)
                    {
                        nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Volume_Register register = portaal_Volume.Register;
                        richting = register.MeteringDirection.ToString();
                        tariefType = register.TariffType.ToString();
                    }

                    if (portaal_Volume.Reading != null)
                    {
                        for (int i4 = 0; i4 < portaal_Volume.Reading.Length; i4++)
                        {
                            if (portaal_Volume.Reading[i4].ReadingDate == eindDatum)
                            {
                                eindStand = int.Parse(portaal_Volume.Reading[i4].Reading);
                                HerkomstEind = portaal_Volume.Reading[i4].ReadingMethod.ToString();
                            }
                            else
                            {
                                beginStand = int.Parse(portaal_Volume.Reading[i4].Reading);
                                HerkomstBegin = portaal_Volume.Reading[i4].ReadingMethod.ToString();
                            }
                        }
                    }
                    else
                    {

                        //Save_UTILTS_E11(intedinID, portaal_Mutation.Dossier.ID, portaal_Mutation.MutationReason.ToString(), portaal_meteringPoint.EANID.ToString(), "", dtmProcessingEnd
                        //    , strTypeOfMeteringPoint, strMeterTimeFrame, "", decimal.Parse(portaal_Volume.Volume), "136", null, "8716867000030", "KWH", blnBatchProcess);
                    }
                    SchrijfVerbruikRegisters();
                }

            }
            else
            {
                //Verbruiken 2
                int intNrMeteringPoints = portaal_meteringPoint.Items.Length;

                if (intNrMeteringPoints > 1) { gesplitst = true; }


                nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter portaal_EnergyMeter;
                for (int i2 = 0; i2 < portaal_meteringPoint.Items.Length; i2++)
                {
                    int intStandID = -1;

                    portaal_EnergyMeter = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter)portaal_meteringPoint.Items[i2];

                    nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register portaal_Register = new nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register();
                    for (int i3 = 0; i3 < portaal_EnergyMeter.Register.Length; i3++)
                    {
                        portaal_Register = portaal_EnergyMeter.Register[i3];

                        if (portaal_Register.Item.GetType() == typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading))
                        {
                            //Meterstand
                            Stand stand = new Stand();
                            stand.EAN18_Code = verbruik.EAN18_Code;
                            stand.Bericht_ID = intBerichtID;
                            stand.Product = verbruik.Product;
                            stand.StandDatum = verbruik.BeginDatum;
                            stand.Referentie = verbruik.Referentie;
                            stand.Dossier = verbruik.Dossier;
                            stand.Proces = verbruik.Process;
                            //stand.Herkomst = verbruik.He
                            stand.Meternummer = verbruik.Meternummer;
                            stand.AantalTelwerken = verbruik.AantalRegisters;
                            stand.Stand_ID = -1;

                            StandTelwerk standTelwerk = new StandTelwerk();
                            standTelwerk.AantalTelwielen = short.Parse(portaal_Register.NrOfDigits.ToString());

                            if (portaal_Register.TariffTypeSpecified == true)
                            {
                                standTelwerk.TariefType = portaal_Register.TariffType.ToString();
                            }
                            else
                            {
                                standTelwerk.TariefType = "N";
                            }

                            if (portaal_Register.MeteringDirectionSpecified == true)
                            {
                                standTelwerk.RegisterType = portaal_Register.MeteringDirection.ToString();
                            }
                            else
                            {
                                standTelwerk.RegisterType = "LVR";
                            }
                            standTelwerk.MeetEenheid = portaal_Register.MeasureUnit.ToString();



                            if (intStandID == -1)
                            {
                                intStandID = stand.SchrijfStand(conn);
                            }
                            standTelwerk.Stand_ID = intStandID;

                            nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading portaal_Reading = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading)portaal_Register.Item;

                            standTelwerk.Datum = portaal_Reading.ReadingDate;
                            standTelwerk.Stand = int.Parse(portaal_Reading.Reading);
                            standTelwerk.Herkomst = portaal_Reading.ReadingMethod.ToString();

                            standTelwerk.SchrijfStandenRegister(conn);


                        }
                        if (portaal_Register.Item.GetType() == typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Volume))
                        {
                            //Volume

                            nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Volume portaal_Volume = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Volume)portaal_Register.Item;

                            aantalTelwielen = short.Parse(portaal_Register.NrOfDigits.ToString());
                            volume = int.Parse(portaal_Volume.Volume);
                            if (portaal_Register.TariffTypeSpecified == true)
                            {
                                tariefType = portaal_Register.TariffType.ToString();
                            }
                            else
                            {
                                tariefType = "N";
                            }

                            if (portaal_Register.MeteringDirectionSpecified == true)
                            {
                                richting = portaal_Register.MeteringDirection.ToString();
                            }
                            else
                            {
                                richting = "LVR";
                            }
                            meetEenheid = portaal_Register.MeasureUnit.ToString();


                            //if (intVerbruik_ID == -1)
                            //{
                            //    for (int i4 = 0; i4 < portaal_Volume.Reading.Length; i4++)
                            //    {
                            //        if (dtmProcessingStart == DateTime.MinValue) { dtmProcessingStart = portaal_Volume.Reading[i4].ReadingDate; }
                            //        if (dtmProcessingEnd == DateTime.MinValue) { dtmProcessingEnd = portaal_Volume.Reading[i4].ReadingDate; }
                            //        if (portaal_Volume.Reading[i4].ReadingDate > dtmProcessingStart) { dtmProcessingEnd = portaal_Volume.Reading[i4].ReadingDate; }
                            //    }

                            //    if (verbruiken.Gesplitst)
                            //    {
                            //        if (dtmProcessingStart == dtmBegin)
                            //        {
                            //            verbruiken.EindDatum = dtmProcessingEnd;
                            //        }
                            //        else
                            //        {
                            //            verbruiken.BeginDatum = dtmProcessingStart;
                            //        }
                            //    }

                            //    intVerbruiID = verbruiken.SchrijfVerbruik(conn);
                            //}

                            verbruik_ID = intVerbruik_ID;

                            nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Volume_Reading[] portaal_VolumeReading = portaal_Volume.Reading;
                            for (int i4 = 0; i4 < portaal_VolumeReading.Length; i4++)
                            {
                                if (portaal_VolumeReading[i4].ReadingDate == verbruik.BeginDatum)
                                {
                                    BeginDatum = verbruik.BeginDatum;
                                    HerkomstBegin = portaal_VolumeReading[i4].ReadingMethod.ToString();
                                    BeginStand = int.Parse(portaal_VolumeReading[i4].Reading);
                                }

                                if (portaal_VolumeReading[i4].ReadingDate == verbruik.EindDatum)
                                {
                                    EindDatum = verbruik.EindDatum;
                                    HerkomstEind = portaal_VolumeReading[i4].ReadingMethod.ToString();
                                    EindStand = int.Parse(portaal_VolumeReading[i4].Reading);
                                }



                                //Save_UTILTS_E11(intedinID, portaal_EnergyMeter.ID, portaal_Mutation.MutationReason.ToString(), portaal_meteringPoint.EANID.ToString(), strReadingType, portaal_VolumeReading[i4].ReadingDate.ToUniversalTime()
                                //, strTypeOfMeteringPoint, strMeterTimeFrame, strOriginCode, decimal.Parse(portaal_VolumeReading[i4].Reading), "220", null, "8716867000030", portaal_Register.MeasureUnit.ToString(), blnBatchProcess);

                            }


                            SchrijfVerbruikRegisters();
                        }
                        //portaal_Register = portaal_EnergyMeter[i3];
                        //portaal_Register.
                    }
                }
            }
        }

        public int Verbruik_ID
        {
            get { return this.verbruik_ID; }
            set { this.verbruik_ID = value; }
        }

        public String Richting
        {
            get { return this.richting; }
            set { this.richting = value; }
        }

        public String TariefType
        {
            get { return this.tariefType; }
            set { this.tariefType = value; }
        }

        public String MeetEenheid
        {
            get { return this.meetEenheid; }
            set { this.meetEenheid = value; }
        }

        public Int16 AantalTelwielen
        {
            get { return this.aantalTelwielen; }
            set { this.aantalTelwielen = value; }
        }

        public Int32 Volume
        {
            get { return this.volume; }
            set { this.volume = value; }
        }

        public Int32 GecorrigeerdVolume
        {
            get { return this.gecorrigeerdVolume; }
            set { this.gecorrigeerdVolume = value; }
        }

        public DateTime BeginDatum
        {
            get { return this.beginDatum; }
            set { this.beginDatum = value; }
        }

        public int BeginStand
        {
            get { return this.beginStand; }
            set { this.beginStand = value; }
        }

        public String HerkomstBegin
        {
            get { return this.herkomstBegin; }
            set
            {
                string strTmp = value;
                this.herkomstBegin = strTmp.Replace("Item", "");
            }
        }

        

        public string strBeginStand
        {
            get { return this.beginStand.ToString(); }
            set
            {
                int.TryParse(value, out beginStand);
            }
        }

        public DateTime EindDatum
        {
            get { return this.eindDatum; }
            set { this.eindDatum = value; }
        }

        public int EindStand
        {
            get { return this.eindStand; }
            set { this.eindStand = value; }
        }

        public String HerkomstEind
        {
            get { return this.herkomstEind; }
            set
            {
                string strTmp = value;
                this.herkomstEind = strTmp.Replace("Item", "").Replace("item", "");
            }
        }

        public Boolean Gesplitst
        {
            get { return this.gesplitst; }
            set { this.gesplitst = value; }
        }


        public void SchrijfVerbruikRegisters()
        {
            string strSQL = "INSERT INTO car.dbo.VerbruikTelwerk \n";
            strSQL += "(Verbruik_ID \n";
            strSQL += ",Richting \n";
            strSQL += ",TariefType \n";
            strSQL += ",MeetEenheid \n";
            strSQL += ",AantalTelwielen \n";
            strSQL += ",Volume \n";
            strSQL += ",GecorrigeerdeVolume \n";
            strSQL += ",BeginDatum \n";
            strSQL += ",BeginStand \n";
            strSQL += ",HerkomstBegin \n";
            strSQL += ",EindDatum \n";
            strSQL += ",EindStand \n";
            strSQL += ",HerkomstEind) \n";
            strSQL += "VALUES \n";
            strSQL += "(@Verbruik_ID \n";
            strSQL += ",@Richting \n";
            strSQL += ",@TariefType \n";
            strSQL += ",@MeetEenheid \n";
            strSQL += ",@AantalTelwielen \n";
            strSQL += ",@Volume \n";
            strSQL += ",@GecorrigeerdeVolume \n";
            strSQL += ",@BeginDatum \n";
            strSQL += ",@BeginStand \n";
            strSQL += ",@HerkomstBegin \n";
            strSQL += ",@EindDatum \n";
            strSQL += ",@EindStand \n";
            strSQL += ",@HerkomstEind) \n";
            SqlCommand cmd = new SqlCommand(strSQL, conn);
            cmd.Parameters.AddWithValue("@Verbruik_ID", verbruik_ID);
            cmd.Parameters.AddWithValue("@Richting", richting);
            cmd.Parameters.AddWithValue("@TariefType", tariefType);
            cmd.Parameters.AddWithValue("@MeetEenheid", meetEenheid);
            cmd.Parameters.AddWithValue("@AantalTelwielen", aantalTelwielen);
            cmd.Parameters.AddWithValue("@Volume", volume);
            cmd.Parameters.AddWithValue("@GecorrigeerdeVolume", gecorrigeerdVolume);
            cmd.Parameters.AddWithValue("@BeginDatum", beginDatum);
            cmd.Parameters.AddWithValue("@BeginStand", BeginStand);
            cmd.Parameters.AddWithValue("@HerkomstBegin", HerkomstBegin);
            cmd.Parameters.AddWithValue("@EindDatum", eindDatum);
            cmd.Parameters.AddWithValue("@EindStand", eindStand);
            cmd.Parameters.AddWithValue("@HerkomstEind", HerkomstEind);
            cmd.ExecuteNonQuery();

        }

    }
}
