using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.IO;
namespace Energie.DataTableHelper
{
    public class FTPClass
    {

        public static Boolean FtpSendFile(string server, string username, string password, string filename, out string responseDiscription)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(server);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                request.Credentials = new NetworkCredential(username, password);

                StreamReader sourceStream = new StreamReader(filename);
                byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                sourceStream.Close();
                request.ContentLength = fileContents.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                responseDiscription = response.StatusDescription;

                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                responseDiscription = ex.Message;
                return false;
            }
        }
    }
}
