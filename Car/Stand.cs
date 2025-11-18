using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Energie.Car
{
    public class Stand
    {
        private int stand_ID;
        private Int32 bericht_ID;
        private Int64 ean18_Code;
        private string dossier;
        private string proces;
        private string referentie;
        private Int64 ontvanger;
        private string product;
        private string meternummer;
        private Int32 aantalTelwerken;
        private DateTime standDatum;
        private string herkomst;
        private DateTime berichtDatum;

        public Stand()
        {

        }

        public int Stand_ID
        {
            get { return this.stand_ID; }
            set { this.stand_ID = value; }
        }

        public Int32 Bericht_ID
        {
            get { return this.bericht_ID; }
            set { this.bericht_ID = value; }
        }

        public Int64 EAN18_Code
        {
            get { return this.ean18_Code; }
            set { this.ean18_Code = value; }
        }

        public string Dossier
        {
            get { return this.dossier; }
            set { this.dossier = value; }
        }

        public string Proces
        {
            get { return this.proces; }
            set { this.proces = value; }
        }

        public string Referentie
        {
            get { return this.referentie; }
            set { this.referentie = value; }
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

        public string Meternummer
        {
            get { return this.meternummer; }
            set { this.meternummer = value; }
        }

        public Int32 AantalTelwerken
        {
            get { return this.aantalTelwerken; }
            set { this.aantalTelwerken = value; }
        }

        public DateTime StandDatum
        {
            get { return this.standDatum; }
            set { this.standDatum = value; }
        }

        public String Herkomst
        {
            get { return this.herkomst; }
            set { this.herkomst = value; }
        }

        public DateTime BerichtDatum
        {
            get { return this.berichtDatum; }
            set { this.berichtDatum = value; }
        }

        public Int32 SchrijfStand(SqlConnection conn)
        {
            Int32 intStand_ID = -1;
            //try
            //{
            string strSQL = "INSERT INTO Car.dbo.Stand \n";
            strSQL += "(Bericht_ID \n";
            strSQL += ",EAN18_Code \n";
            strSQL += ",Dossier \n";
            strSQL += ",Proces \n";
            strSQL += ",Referentie \n";
            strSQL += ",Ontvanger \n";
            strSQL += ",Product \n";
            strSQL += ",Meternummer \n";
            strSQL += ",AantalTelwerken \n";
            strSQL += ",StandDatum \n";
            strSQL += ",Herkomst \n";
            strSQL += ",BerichtDatum) \n";
            strSQL += "VALUES \n";
            strSQL += "(@Bericht_ID \n";
            strSQL += ",@EAN18_Code \n";
            strSQL += ",@Dossier \n";
            strSQL += ",@Proces \n";
            strSQL += ",@Referentie \n";
            strSQL += ",@Ontvanger \n";
            strSQL += ",@Product \n";
            strSQL += ",@Meternummer \n";
            strSQL += ",@AantalTelwerken \n";
            strSQL += ",@StandDatum \n";
            strSQL += ",@Herkomst \n";
            strSQL += ",@BerichtDatum); \n";
            strSQL += "  SELECT @StandID = SCOPE_IDENTITY();";
            SqlCommand cmd = new SqlCommand(strSQL, conn);
            cmd.Parameters.AddWithValue("@Bericht_ID", bericht_ID);
            cmd.Parameters.AddWithValue("@EAN18_Code", ean18_Code);
            cmd.Parameters.AddWithValue("@Dossier", dossier);
            cmd.Parameters.AddWithValue("@Proces", herkomst);
            cmd.Parameters.AddWithValue("@Referentie", referentie);
            cmd.Parameters.AddWithValue("@Ontvanger", ontvanger);
            cmd.Parameters.AddWithValue("@Product", product);
            cmd.Parameters.AddWithValue("@Meternummer", meternummer);
            cmd.Parameters.AddWithValue("@AantalTelwerken", aantalTelwerken);
            cmd.Parameters.AddWithValue("@StandDatum", standDatum);
            cmd.Parameters.AddWithValue("@Herkomst", herkomst);
            cmd.Parameters.AddWithValue("@BerichtDatum", berichtDatum);
            cmd.Parameters.Add(new SqlParameter("@StandID", SqlDbType.Int));
            cmd.Parameters["@StandID"].Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();

            intStand_ID = (int)cmd.Parameters["@StandID"].Value;
            //}
            //catch (Exception ex)
            //{

            //}

            return intStand_ID;
        }
    }
}
