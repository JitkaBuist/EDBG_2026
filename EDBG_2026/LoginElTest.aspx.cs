using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using Energie.DataTableHelper;
using System.Web.Security;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;


public partial class LoginElTest : System.Web.UI.Page
{
    protected string UserName;



    protected SqlConnection conn;
    protected String ConnString = "";





    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["LoginFoutmelding"] != null)
        {
            string Foutmelding = Session["LoginFoutmelding"].ToString();
            ScriptManager.RegisterClientScriptBlock(this.Page, this.GetType(), "Myscript1", @"alert('" + Foutmelding + "');", true);
            Session["LoginFoutmelding"] = null;
        }

    }

    protected void ValidateUser(object sender, EventArgs e)
    {
        string Wachtwoord = "";
        //string Klantconfig = "";




        ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");
        using (SqlConnection con = new SqlConnection(ConnString))
        {

            string Hash = PasswordHash.CreateHash(LoginEl1.Password);

            using (SqlCommand cmd = new SqlCommand("SELECT Wachtwoord FROM [Klantconfig].[dbo].[Logins] " +
                "where Gebruikersnaam=@Gebruikersnaam "))
            {
                con.Open();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@Gebruikersnaam", LoginEl1.UserName);
                //cmd.Parameters.AddWithValue("@Password", Hash);
                cmd.Connection = con;



                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    Wachtwoord = result.ToString();
                }
                else
                { LoginEl1.FailureText = "Fout mailadres"; }
            }


            if (Wachtwoord != "")
            {
                if (PasswordHash.ValidatePassword(LoginEl1.Password, Wachtwoord))
                {
                    Session["GebruikersNaam"] = LoginEl1.UserName;
                    using (SqlCommand cmd = new SqlCommand("SELECT Klantconfig, LoginID  FROM  [Klantconfig].[dbo].[Logins] " +
                    "where Gebruikersnaam=@Gebruikersnaam "))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;

                        cmd.Parameters.AddWithValue("@Gebruikersnaam", LoginEl1.UserName);

                        cmd.Connection = con;

                        SqlDataReader rdr = cmd.ExecuteReader();

                        Session["KlantConfig"] = "";

                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {

                                Session["KlantConfig"] = (string)rdr["Klantconfig"];
                                Session["GebruikersID"] = rdr.GetInt32(1).ToString();
                            }
                        }
                        rdr.Close();

                        if (Session["KlantConfig"].ToString() != "")
                        {
                            try
                            {
                                String strSql = "SELECT ConnString From KlantConfig Where Klantconfig=@Klantconfig";
                                SqlCommand cmd2 = new SqlCommand(strSql, con);
                                cmd2.Parameters.AddWithValue("@Klantconfig", Session["KlantConfig"]);
                                rdr = cmd2.ExecuteReader();
                                while (rdr.Read())
                                {
                                    Session["ConnString"] = rdr.GetString(0);
                                }

                                rdr.Close();

                               
                            }
                            catch //(Exception ex)
                            {

                            }
                            rdr.Close();
                        }

                        FormsAuthentication.RedirectFromLoginPage(LoginEl1.UserName, LoginEl1.RememberMeSet);

                        //       

                    }
                }
                else
                {
                    LoginEl1.FailureText = "Fout Password";
                }
            }




            con.Close();
        }
    }
    protected void LoginVergeten_Click(object sender, EventArgs e)
    {

        try
        {

            ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");
            using (SqlConnection con = new SqlConnection(ConnString))
            {

                // create password reset token
                string Token = GetUniqueKey(10);
                DateTime ExpirationDate = DateTime.Now.AddDays(+1);


                using (SqlCommand cmd = new SqlCommand("UPDATE [Klantconfig].[dbo].[Logins]  " +
                   "set TokenUsed = @TokenUsed " +
                   ",Token = @Token " +
                   ",ExpirationDate = @ExpirationDate " +
                   "where Gebruikersnaam=@Gebruikersnaam "))


                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.AddWithValue("@Gebruikersnaam", LoginEl1.UserName);
                    cmd.Parameters.AddWithValue("@Token", Token);

                    cmd.Parameters.AddWithValue("ExpirationDate", ExpirationDate);
                    cmd.Parameters.AddWithValue("@TokenUsed", 1);
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();


                    con.Close();
                }

                //create the mail message 
                MailMessage mail = new MailMessage();

                //set the addresses 
                mail.From = new MailAddress("support@edbg.nl");
                mail.To.Add(LoginEl1.UserName);


                //set the content 
                mail.Subject = "Wachtwoord vergeten";
                mail.IsBodyHtml = true;
                mail.Body += "Geachte heer, mevrouw";
                mail.Body += "<br />";
                mail.Body += "<br />";
                mail.Body += "U kunt vandaag een nieuw wachtwoord aanmaken met de volgende link <br>";
                mail.Body += "<br />";
                mail.Body += "https://www.edbgtest.nl/wachtwoordaanpassen.aspx";

                mail.Body += "?Token=" + Token;
                mail.Body += "<br />";
                mail.Body += "<br />";
                mail.Body += "Met vriendelijke groeten";
                mail.Body += "<br />";

                mail.Body += "Support EDBG";



                //send the message smtp2go
                SmtpClient smtpClient = new SmtpClient("mail.smtp2go.com", 587);
                smtpClient.Credentials = new NetworkCredential("support@edbg.nl", "Hf0xV0CFn5kNOI2b"); // Gebruikersnaam en wachtwoord (API-sleutel)
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls13 | System.Net.SecurityProtocolType.Tls12;
                smtpClient.EnableSsl = true;  // Schakel SSL in voor beveiligde verzending



                smtpClient.Send(mail);
                LoginEl1.FailureText = "Reset Password mail verstuurd";
                AlertMessage("Reset password mail verstuurd  ");
                FormsAuthentication.RedirectFromLoginPage(LoginEl1.UserName, LoginEl1.RememberMeSet);
            }
        }
        catch (Exception ex)
        {
            AlertMessage("Mail versturen mislukt : " + ex.Message);
            LoginEl1.FailureText = "Reset Password mail niet verstuurd";
        }
    }


    private void AlertMessage(string strMessage)
    {
        Type cstype = this.GetType();

        // Get a ClientScriptManager reference from the Page class.
        ClientScriptManager cs = Page.ClientScript;

        // Check to see if the startup script is already registered.
        if (!cs.IsStartupScriptRegistered(cstype, "PopupScript"))
        {
            String cstext = "alert('" + strMessage + "');";
            cs.RegisterStartupScript(cstype, "PopupScript", cstext, true);
        }
    }


    public static string GetUniqueKey(int maxSize)
    {
        char[] chars = new char[62];
        chars =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        byte[] data = new byte[1];
        using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
        {
            crypto.GetNonZeroBytes(data);
            data = new byte[maxSize];
            crypto.GetNonZeroBytes(data);
        }
        StringBuilder result = new StringBuilder(maxSize);
        foreach (byte b in data)
        {
            result.Append(chars[b % (chars.Length)]);
        }
        return result.ToString();
    }


    protected void Indienen_Click(object sender, EventArgs e)
    {

    }




    protected void Aanpassen_Click(object sender, EventArgs e)
    {

    }



    protected void EmailAanpassen_Click(object sender, EventArgs e)
    {

    }

}
