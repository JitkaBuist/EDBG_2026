using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Energie.Car
{
    public class Gain
    {
        private string strSql;
        private SqlConnection conn;
        //private CarShared carShared;

        private int gain_ID;
        private int bericht_ID;
        private DateTime datum;
        private Int64 ontvanger;
        private Int64 ean18_Code;
        private String product;
        private Int64 netbeheerderEAN;
        private Int64 programmaverantwoordelijkeEAN;
        private Int64 leverancierEAN;
        private String dossier;
        private String reden;
        private String referentie = "";

        public Gain(nl.Energie.EDSN.LossGainRejectUpdate.GainResultResponseEnvelope_Portaal_Content_Portaal_MeteringPoint responseItem, int intBerichtID, String Ontvanger)
        {
            conn = new SqlConnection(KC.ConnString);
            conn.Open();

            Bericht_ID = intBerichtID;
            
            strOntvanger = Ontvanger;
            strEAN18_Code = responseItem.EANID;
            Product = responseItem.ProductType.ToString();
            strNetbeheerderEAN = responseItem.GridOperator_Company.ID;
            if (responseItem.MPCommercialCharacteristics != null)
            {
                if (responseItem.MPCommercialCharacteristics.BalanceResponsibleParty_Company != null)
                {
                    strProgrammaverantwoordelijkeEAN = responseItem.MPCommercialCharacteristics.BalanceResponsibleParty_Company.ID;
                }
                if (responseItem.MPCommercialCharacteristics.BalanceSupplier_Company != null)
                {
                    strLeverancierEAN = responseItem.MPCommercialCharacteristics.BalanceSupplier_Company.ID;
                }
            }
            if (responseItem.Portaal_Mutation != null)
            {
                Datum = responseItem.Portaal_Mutation.MutationDate;
                Dossier = responseItem.Portaal_Mutation.Dossier.ID;
                Reden = responseItem.Portaal_Mutation.MutationReason.ToString();
                if (responseItem.Portaal_Mutation.ExternalReference != null)
                {
                    Referentie = responseItem.Portaal_Mutation.ExternalReference;
                }
            }

            SchrijfGain(conn);

            conn.Close();
        }

        public int Gain_ID
        {
            get { return this.gain_ID; }
            set { this.gain_ID = value; }
        }

        public int Bericht_ID
        {
            get { return this.bericht_ID; }
            set { this.bericht_ID = value; }
        }

        public DateTime Datum
        {
            get { return this.datum; }
            set { this.datum = value; }
        }

        public Int64 Ontvanger
        {
            get { return this.ontvanger; }
            set { this.ontvanger = value; }
        }

        public String strOntvanger
        {
            get { return this.ontvanger.ToString(); }
            set
            {
                Int64 intOntvanger = 0;
                Int64.TryParse(value, out intOntvanger);
                this.ontvanger = intOntvanger;
            }
        }
        public Int64 EAN18_Code
        {
            get { return this.ean18_Code; }
            set { this.ean18_Code = value; }
        }

        public String strEAN18_Code
        {
            get { return this.ean18_Code.ToString(); }
            set
            {
                Int64 intEAN18_Code = 0;
                Int64.TryParse(value, out intEAN18_Code);
                this.ean18_Code = intEAN18_Code;
            }
        }

        public String Product
        {
            get { return this.product; }
            set { this.product = value; }
        }

        public Int64 NetbeheerderEAN
        {
            get { return this.netbeheerderEAN; }
            set { this.netbeheerderEAN = value; }
        }

        public String strNetbeheerderEAN
        {
            get { return this.netbeheerderEAN.ToString(); }
            set
            {
                Int64 intNetbeheerderEAN = 0;
                Int64.TryParse(value, out intNetbeheerderEAN);
                this.netbeheerderEAN = intNetbeheerderEAN;
            }
        }

        public Int64 ProgrammaverantwoordelijkeEAN
        {
            get { return this.programmaverantwoordelijkeEAN; }
            set { this.programmaverantwoordelijkeEAN = value; }
        }

        public String strProgrammaverantwoordelijkeEAN
        {
            get { return this.programmaverantwoordelijkeEAN.ToString(); }
            set
            {
                Int64 intProgrammaverantwoordelijkeEAN = 0;
                Int64.TryParse(value, out intProgrammaverantwoordelijkeEAN);
                this.programmaverantwoordelijkeEAN = intProgrammaverantwoordelijkeEAN;
            }
        }

        public Int64 LeverancierEAN
        {
            get { return this.leverancierEAN; }
            set { this.leverancierEAN = value; }
        }

        public String strLeverancierEAN
        {
            get { return this.leverancierEAN.ToString(); }
            set
            {
                Int64 intLeverancierEAN = 0;
                Int64.TryParse(value, out intLeverancierEAN);
                this.leverancierEAN = intLeverancierEAN;
            }
        }

        public String Dossier
        {
            get { return this.dossier; }
            set { this.dossier = value; }
        }

        public String Reden
        {
            get { return this.reden; }
            set { this.reden = value; }
        }

        public String Referentie
        {
            get { return this.referentie; }
            set { this.referentie = value; }
        }

        public Int32 SchrijfGain(SqlConnection conn)
        {
            strSql = "INSERT INTO Car.dbo.Gain \n";
            strSql += "(Bericht_ID \n";
            strSql += ",Datum \n";
            strSql += ",Ontvanger \n";
            strSql += ",EAN18_Code \n";
            strSql += ",Product \n";
            strSql += ",NB\n";
            strSql += ",PV \n";
            strSql += ",LV \n";
            strSql += ",Dossier \n";
            strSql += ",Reden \n";
            strSql += ",Referentie) \n";
            strSql += "VALUES \n";
            strSql += "(@Bericht_ID \n";
            strSql += ",@Datum \n";
            strSql += ",@Ontvanger \n";
            strSql += ",@EAN18_Code \n";
            strSql += ",@Product \n";
            strSql += ",@NB \n";
            strSql += ",@PV \n";
            strSql += ",@LV \n";
            strSql += ",@Dossier \n";
            strSql += ",@Reden \n";
            strSql += ",@Referentie); SELECT @Gain_ID = SCOPE_IDENTITY();";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@Bericht_ID", Bericht_ID);
            cmd.Parameters.AddWithValue("@Datum", Datum);
            cmd.Parameters.AddWithValue("@Ontvanger", Ontvanger);
            cmd.Parameters.AddWithValue("@EAN18_Code", EAN18_Code);
            cmd.Parameters.AddWithValue("@Product", Product);
            cmd.Parameters.AddWithValue("@NB", NetbeheerderEAN);
            cmd.Parameters.AddWithValue("@PV", ProgrammaverantwoordelijkeEAN);
            cmd.Parameters.AddWithValue("@LV", LeverancierEAN);
            cmd.Parameters.AddWithValue("@Dossier", Dossier);
            cmd.Parameters.AddWithValue("@Reden", Reden);
            cmd.Parameters.AddWithValue("@Referentie", Referentie);

            cmd.Parameters.Add(new SqlParameter("@Gain_ID", SqlDbType.Int));
            cmd.Parameters["@Gain_ID"].Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();

            return Gain_ID;
        }

    }
}
