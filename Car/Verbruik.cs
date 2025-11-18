using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Energie.Car
{
    public class Verbruik
    {
        //private string strSQL;
        private SqlConnection conn;
        //private CarShared carShared;

        private int verbruik_ID;
        private Int64 ean18_Code;
        private Int32 bericht_ID;
        private string dossier;
        private string process;
        private string referentie;
        private Int64 afzender;
        private Int64 ontvanger;
        private string product;
        private DateTime beginDatum;
        private DateTime eindDatum;
        private string meternummer;
        private Int32 aantalRegisters = 0;
        private DateTime berichtDatum;

        public Verbruik(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP portaal_meteringPoint, Int32 intBerichtID, DateTime dtOntvangst)
        {
            conn = new SqlConnection(KC.ConnString);
            conn.Open();

            if (portaal_meteringPoint.ValidFromDate != null)
            {
                beginDatum = portaal_meteringPoint.ValidFromDate;//.ToUniversalTime();
            }
            
            if (portaal_meteringPoint.ValidToDate != null)
            {
                eindDatum = portaal_meteringPoint.ValidToDate;//.ToUniversalTime();
            }

            nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_PM portaal_Mutation = portaal_meteringPoint.Portaal_Mutation;


            ean18_Code = Int64.Parse(portaal_meteringPoint.EANID.ToString());
            bericht_ID = intBerichtID;
            process = portaal_Mutation.MutationReason.ToString();
            afzender = Int64.Parse(portaal_Mutation.Initiator.ToString());
            ontvanger = Int64.Parse(portaal_Mutation.Consumer.ToString());
            BerichtDatum = dtOntvangst;

            if (portaal_Mutation != null)
            {
                dossier = portaal_Mutation.Dossier.ID;
            }
            else
            {
                dossier = "";
            }
            if (portaal_Mutation.ExternalReference != null)
            {
                referentie = portaal_Mutation.ExternalReference;
            }
            else
            {
                referentie = "";
            }

            product = portaal_meteringPoint.ProductType.ToString();

            if (portaal_meteringPoint.Items[0].GetType() == typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Volume))
            {
                nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Volume portaal_Volume = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Volume)portaal_meteringPoint.Items[0];
                if (portaal_Volume.Reading != null)
                {
                    aantalRegisters = portaal_Volume.Reading.Length;
                }

                Verbruik_ID = SchrijfVerbruik();

                VerbruikTelwerk verbruikTelwerk = new VerbruikTelwerk(portaal_meteringPoint, intBerichtID, Verbruik_ID, this);
            }
            else
            {
                nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter portaal_EnergyMeter = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter)portaal_meteringPoint.Items[0];
                meternummer = portaal_EnergyMeter.ID;
                aantalRegisters = Int32.Parse(portaal_EnergyMeter.NrOfRegisters);
                //nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register register = portaal_EnergyMeter.Register[0];

                nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register portaal_Register = portaal_EnergyMeter.Register[0];

                if (portaal_Register.Item.GetType() == typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading))
                {
                    nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading reading = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading)portaal_Register.Item;
                    Car.Stand stand = new Stand();

                    stand.Bericht_ID = Bericht_ID;
                    stand.EAN18_Code = EAN18_Code;
                    stand.Product = Product;
                    stand.StandDatum = reading.ReadingDate ;
                    stand.Referentie = Referentie;
                    stand.Dossier = Dossier;
                    stand.AantalTelwerken = AantalRegisters;
                    stand.Ontvanger = Ontvanger;
                    stand.Herkomst = Process;
                    stand.BerichtDatum = dtOntvangst;
                    stand.Meternummer = Meternummer;
                    int intStand_ID = stand.SchrijfStand(conn);

                    for (int i3 = 0; i3 < portaal_EnergyMeter.Register.Length; i3++)
                    {
                        StandTelwerk standTelwerk = new StandTelwerk();

                        standTelwerk.strAantalTelwielen = portaal_Register.NrOfDigits;

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



                        standTelwerk.Stand_ID = intStand_ID;

                        nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading portaal_Reading = (nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Reading)portaal_Register.Item;

                        standTelwerk.Datum = portaal_Reading.ReadingDate;
                        standTelwerk.strStand = portaal_Reading.Reading;
                        standTelwerk.Herkomst = portaal_Reading.ReadingMethod.ToString();
                        

                        standTelwerk.SchrijfStandenRegister(conn);
                    }

                }
                if (portaal_Register.Item.GetType() == typeof(nl.Energie.EDSN.MeterReading.MeterReadingExchangeResponseEnvelope_PC_PMP_Portaal_EnergyMeter_Register_Volume))
                {
                    Verbruik_ID = SchrijfVerbruik();

                    VerbruikTelwerk verbruikTelwerk = new VerbruikTelwerk(portaal_meteringPoint, intBerichtID, Verbruik_ID, this);
                }
            }

            conn.Close();
        }

        public int Verbruik_ID
        {
            get { return this.verbruik_ID; }
            set { this.verbruik_ID = value; }
        }

        public Int64 EAN18_Code
        {
            get { return this.ean18_Code; }
            set { this.ean18_Code = value; }
        }

        public Int32 Bericht_ID
        {
            get { return this.bericht_ID; }
            set { this.bericht_ID = value; }
        }

        public string Dossier
        {
            get { return this.dossier; }
            set { this.dossier = value; }
        }

        public string Process
        {
            get { return this.process; }
            set { this.process = value; }
        }

        public string Referentie
        {
            get { return this.referentie; }
            set { this.referentie = value; }
        }

        public Int64 Afzender
        {
            get { return this.afzender; }
            set { this.afzender = value; }
        }

        public Int64 Ontvanger
        {
            get { return this.ontvanger; }
            set { this.ontvanger = value; }
        }

        public string Product
        {
            get { return this.product; }
            set { this.product = value; }
        }

        public DateTime BeginDatum
        {
            get { return this.beginDatum; }
            set { this.beginDatum = value; }
        }

        public DateTime EindDatum
        {
            get { return this.eindDatum; }
            set { this.eindDatum = value; }
        }

        public string Meternummer
        {
            get { return this.meternummer == null?"":this.meternummer; }
            set { this.meternummer = value; }
        }

        public Int32 AantalRegisters
        {
            get { return this.aantalRegisters; }
            set { this.aantalRegisters = value; }
        }

        public DateTime BerichtDatum
        {
            get { return this.berichtDatum; }
            set { this.berichtDatum = value; }
        }

        public Int32 SchrijfVerbruik()
        {
            Int32 intVerbruik_ID = -1;
            //try
            //{
            string strSQL = "INSERT INTO Car.dbo.Verbruik \n";
            strSQL += "(EAN18_Code \n";
            strSQL += ", Bericht_ID \n";
            strSQL += ", Dossier \n";
            strSQL += ", Proces \n";
            strSQL += ", Referentie \n";
            strSQL += ", Afzender \n";
            strSQL += ", Ontvanger \n";
            strSQL += ", Product \n";
            strSQL += ", BeginDatum \n";
            strSQL += ", EindDatum \n";
            strSQL += ", Meternummer \n";
            strSQL += ", AantalRegisters \n";
            strSQL += ", BerichtDatum) \n";
            strSQL += "VALUES \n";
            strSQL += "(@EAN18_Code \n";
            strSQL += ", @Bericht_ID \n";
            strSQL += ", @Dossier \n";
            strSQL += ", @Proces \n";
            strSQL += ", @Referentie \n";
            strSQL += ", @Afzender \n";
            strSQL += ", @Ontvanger \n";
            strSQL += ", @Product \n";
            strSQL += ", @BeginDatum \n";
            strSQL += ", @EindDatum \n";
            strSQL += ", @Meternummer \n";
            strSQL += ", @AantalRegisters \n";
            strSQL += ", @BerichtDatum); \n";
            strSQL += " SELECT @Verbruik_ID = SCOPE_IDENTITY();";
            SqlCommand cmd = new SqlCommand(strSQL, conn);
            cmd.Parameters.AddWithValue("@EAN18_Code", ean18_Code);
            cmd.Parameters.AddWithValue("@Dossier", dossier);
            cmd.Parameters.AddWithValue("@Bericht_ID", bericht_ID);
            cmd.Parameters.AddWithValue("@Proces", process);
            cmd.Parameters.AddWithValue("@Referentie", referentie);
            cmd.Parameters.AddWithValue("@Afzender", afzender);
            cmd.Parameters.AddWithValue("@Ontvanger", ontvanger);
            cmd.Parameters.AddWithValue("@Product", product);
            cmd.Parameters.AddWithValue("@BeginDatum", beginDatum);
            cmd.Parameters.AddWithValue("@EindDatum", eindDatum);
            cmd.Parameters.AddWithValue("@Meternummer", Meternummer);
            cmd.Parameters.AddWithValue("@AantalRegisters", aantalRegisters);
            cmd.Parameters.AddWithValue("@BerichtDatum", BerichtDatum);
            cmd.Parameters.Add(new SqlParameter("@Verbruik_ID", SqlDbType.Int));
            cmd.Parameters["@Verbruik_ID"].Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();

            intVerbruik_ID = (int)cmd.Parameters["@Verbruik_ID"].Value;
            //}
            //catch (Exception ex)
            //{

            //}

            return intVerbruik_ID;
        }
    }
}
