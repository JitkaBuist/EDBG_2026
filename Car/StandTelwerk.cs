using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Energie.Car
{
    public class StandTelwerk
    {
        private int stand_ID;
        private string registerType;
        private string tariefType;
        private string meetEenheid;
        private Int16 aantalTelwielen;
        private DateTime datum;
        private int stand;
        private string herkomst;

        public StandTelwerk()
        {

        }

        public int Stand_ID
        {
            get { return this.stand_ID; }
            set { this.stand_ID = value; }
        }
        public String RegisterType
        {
            get { return this.registerType; }
            set { this.registerType = value; }
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

        public String strAantalTelwielen
        {
            get { return this.aantalTelwielen.ToString(); }
            set
            {
                Int16 intAantalTelWielen = 0;
                Int16.TryParse(value, out intAantalTelWielen);
                this.aantalTelwielen = intAantalTelWielen;
            }
        }

        public DateTime Datum
        {
            get { return this.datum; }
            set { this.datum = value; }
        }

        public int Stand
        {
            get { return this.stand; }
            set { this.stand = value; }
        }

        public String strStand
        {
            get { return this.stand.ToString(); }
            set
            {
                int intStand = 0;
                int.TryParse(value, out intStand);
                this.stand = intStand;
            }
        }

        public String Herkomst
        {
            get { return this.herkomst; }
            set
            {
                string strTmp = value;

                this.herkomst = strTmp.Replace("Item", "");
            }
        }

        public void SchrijfStandenRegister(SqlConnection conn)
        {
            string strSQL = "INSERT INTO Car.dbo.StandTelwerk \n";
            strSQL += "(Stand_ID \n";
            strSQL += ",RegisterType \n";
            strSQL += ",TariefType \n";
            strSQL += ",MeetEenheid \n";
            strSQL += ",AantalTelWielen \n";
            strSQL += ",Datum \n";
            strSQL += ",Stand \n";
            strSQL += ",Herkomst) \n";
            strSQL += "VALUES \n";
            strSQL += "(@Stand_ID \n";
            strSQL += ",@RegisterType \n";
            strSQL += ",@TariefType \n";
            strSQL += ",@MeetEenheid \n";
            strSQL += ",@AantalTelWielen \n";
            strSQL += ",@Datum \n";
            strSQL += ",@Stand \n";
            strSQL += ",@Herkomst) \n";
            SqlCommand cmd = new SqlCommand(strSQL, conn);
            cmd.Parameters.AddWithValue("@Stand_ID", stand_ID);
            cmd.Parameters.AddWithValue("@RegisterType", registerType);
            cmd.Parameters.AddWithValue("@TariefType", tariefType);
            cmd.Parameters.AddWithValue("@MeetEenheid", meetEenheid);
            cmd.Parameters.AddWithValue("@AantalTelwielen", aantalTelwielen);
            cmd.Parameters.AddWithValue("@Datum", datum);
            cmd.Parameters.AddWithValue("@Stand", stand);
            cmd.Parameters.AddWithValue("@Herkomst", herkomst);

            cmd.ExecuteNonQuery();
        }
    }
}
