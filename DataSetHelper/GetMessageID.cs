using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Energie.DataTableHelper
{
    public class GetMessageID
    {

        public static string getMessageID(string ConnString)
        {
            //String ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB");
            SqlConnection cnPubs = new SqlConnection(ConnString);
            cnPubs.Open();
            SqlCommand cmdGetID = new SqlCommand("[Messages].[dbo].[p_getMessageNumber]", cnPubs);
            cmdGetID.CommandType = CommandType.StoredProcedure;
            cmdGetID.Parameters.Add("@Messagenumber", SqlDbType.VarChar, 14);
            cmdGetID.Parameters["@Messagenumber"].Direction = ParameterDirection.Output;
            
            cmdGetID.ExecuteNonQuery();
            string messageID;
            messageID = (string)cmdGetID.Parameters["@Messagenumber"].Value;
            cnPubs.Close();
            return messageID;
        }
    }
}