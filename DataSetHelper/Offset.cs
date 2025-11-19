using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Energie.DataTableHelper
{
    public class Offset
    {

        private String ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB");
            
        public int getOffset(DateTime utcDate)
        {
            /*
             * Note: It is not permitted to send two different time zones within one message.
             * Note: All dates in a message shall be given with the same time zone. E. g. both “Message date” and
             *       “Processing start date” should be given with the same time zone.
             *
             * The moment of preparation/transmission of the message (message date) defines the choice 
             * between summer- and wintertime 
             */

            utcDate = DateTime.Parse(utcDate.ToShortDateString());
            int offs = 0;
            string SQLstatement;
            SqlConnection cnPubs = new SqlConnection(ConnString);

            //  Voor offset nemen we (NL_DT - UTC_DT) van het begiin van de dag van het E-programma) 
            SQLstatement = "select @offset = DATEDIFF(hh, utc_dt,NL_DT) from Energiedb.dbo.Tijden " +
                           "where NL_DT = @date";
            SqlCommand cmdGetOffset = new SqlCommand(SQLstatement, cnPubs);
            cmdGetOffset.Parameters.AddWithValue("@date", utcDate);
            cmdGetOffset.Parameters.Add(new SqlParameter("@offset", SqlDbType.Int));
            cmdGetOffset.Parameters["@offset"].Direction = ParameterDirection.Output;
            cnPubs.Open();
            cmdGetOffset.ExecuteNonQuery();
            offs = (int)cmdGetOffset.Parameters["@offset"].Value;
            cnPubs.Close();
            return offs;
        }

        public int getOffset()
        {
            return this.getOffset(DateTime.Now);
        }
}
}
