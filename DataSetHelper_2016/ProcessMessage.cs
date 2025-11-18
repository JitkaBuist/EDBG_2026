using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
namespace Energie.DataTableHelper
{
    public class ProcessMessage
    {
        public static string processMessage(int inbox_ID, string ConnString)
        {
            //String ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("EnergieDB");
            SqlConnection cnPubs = new SqlConnection(ConnString);
            SqlCommand cmdProcessMessage = new SqlCommand("[Messages].[dbo].[ProcessMessages]", cnPubs);
            cmdProcessMessage.CommandType = CommandType.StoredProcedure;
            cmdProcessMessage.Parameters.Add("@inbox_ID", SqlDbType.Int);
            cmdProcessMessage.Parameters["@inbox_ID"].Direction = ParameterDirection.Input;
            cmdProcessMessage.Parameters["@inbox_ID"].Value = inbox_ID;

            cmdProcessMessage.Parameters.Add("@succes", SqlDbType.Bit);
            cmdProcessMessage.Parameters["@succes"].Direction = ParameterDirection.Output;
            cmdProcessMessage.Parameters.Add("@strError", SqlDbType.NVarChar, 4000);
            cmdProcessMessage.Parameters["@strError"].Direction = ParameterDirection.Output;
            cnPubs.Open();
            cmdProcessMessage.ExecuteNonQuery();
            string strError;
            strError = (string)cmdProcessMessage.Parameters["@strError"].Value;
            cnPubs.Close();
            return strError;
        }
    }
}
