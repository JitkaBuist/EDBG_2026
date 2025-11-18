using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using Energie.DataTableHelper;
using System.Web.Security;
using System.Net.Mail;
using System.Net;
using Energie.Car;
using Newtonsoft.Json;
using RestSharp;


public partial class Relatie : System.Web.UI.Page
{


    protected SqlConnection conn;
    protected String ConnString = "";
    protected CultureInfo provider = CultureInfo.InvariantCulture;

    protected DataSet dsGet;

    protected SqlCommand cmd;
    protected SqlDataAdapter daSwitch;
    protected DataSet dsSwitch;
    protected DataRow drSwitch;

  
   
    
    protected int Relatie_ID = -1;
    protected int Account_Relatie_ID = -1;
    protected string Klant_Config = "";
    
   


    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["KlantConfig"] == null || Session["KlantConfig"].ToString() == "")
        {
            string strUrl = "Login.aspx";

            Response.Redirect(strUrl);
        }

        //}



        Klant_Config = Session["KlantConfig"].ToString().ToUpper();
        KC.KlantConfig = Session["KlantConfig"].ToString().ToUpper();
        KC.ConnStringPortaal = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");
        KC.App_ID = AppID.EDBG;
        string strKlantconfig = Session["Klantconfig"].ToString();
        liKlantconfig.Text = strKlantconfig;
        string strGebruikersnaam = Session["Gebruikersnaam"].ToString();
        liGebruikersnaam.Text = strGebruikersnaam;
         


        
        ConnString = KC.ConnString;

        if (!IsPostBack)
        {
            if (chkBedrijf.Checked)
            {
                strRelatie_Bedrijfsnaam.Visible = true;
                strRelatie_KvK.Visible = true;
                strRelatie_Voornaam.Visible = false;
                strRelatie_Achternaam.Visible = false;
                strRelatie_Relatienaam.Visible = false;
                strRelatie_Geboortedatum.Visible = false;
                strRelatie_Tussenvoegsel.Visible = false;
                btnGeboortedatum.Visible = false;

            }
            else
            {
                strRelatie_Voornaam.Text = strRelatie_Voornaam.Text.Trim();
                strRelatie_Achternaam.Text = strRelatie_Achternaam.Text.Trim();
                strRelatie_Relatienaam.Text = strRelatie_Voornaam.Text + strRelatie_Achternaam.Text;
                strRelatie_Voornaam.Visible = true;
                strRelatie_Achternaam.Visible = true;
                strRelatie_Relatienaam.Visible = true;
                strRelatie_Geboortedatum.Visible = true;
                btnGeboortedatum.Visible = true;
                strRelatie_Tussenvoegsel.Visible = true;
                strRelatie_Bedrijfsnaam.Visible = false;
                strRelatie_KvK.Visible = false;

            }


        }

    }

    protected void chkBedrijf_CheckedChanged(object sender, EventArgs e)
    {
        Bedrijf();
    }

    protected void Bedrijf()
    {
        if (chkBedrijf.Checked)
        {
            strRelatie_Bedrijfsnaam.Visible = true;
            strRelatie_KvK.Visible = true;
            strRelatie_Voornaam.Visible = false;
            strRelatie_Tussenvoegsel.Visible = false;
            strRelatie_Achternaam.Visible = false;
            strRelatie_Relatienaam.Visible = false;
            strRelatie_Geboortedatum.Visible = false;
           

        }
        else
        {
            strRelatie_Voornaam.Text = strRelatie_Voornaam.Text.Trim();
            strRelatie_Achternaam.Text = strRelatie_Achternaam.Text.Trim();
       
            strRelatie_Tussenvoegsel.Visible = true;
            strRelatie_Voornaam.Visible = true;
            strRelatie_Voornaam.Visible = true;
            strRelatie_Achternaam.Visible = true;
            strRelatie_Geboortedatum.Visible = true;
            strRelatie_Relatienaam.Visible = true;
            strRelatie_Bedrijfsnaam.Visible = false;
            strRelatie_KvK.Visible = false;
          
            btnGeboortedatum.Visible = true;
        }
    }

    


    protected void ZoekRelatie_Click(object sender, EventArgs e)
    {
        strNieuwEmail.Text = "";

        strRelatie_Straat.Text = "";
        strRelatie_Huisnummer.Text = "";
        strRelatie_Toevoeging.Text = "";
        strRelatie_Woonplaats.Text = "";
        strRelatie_Postcode.Text = "";
        strRelatie_Geboortedatum.Text= "";
        strRelatie_Voornaam.Text = "";
        strRelatie_Tussenvoegsel.Text = "";
        strRelatie_Achternaam.Text = "";
        strRelatie_Geboortedatum.Text = "";
       

        strRelatie_Bedrijfsnaam.Text = "";
        strRelatie_Relatienaam.Text = "";
       

        strRelatie_KvK.Text = "";
         chkBedrijf.Checked = true;

        chkSlimmeMeterToestemming.Checked = true;







        using (SqlConnection con = new SqlConnection(ConnString))
        {



            string strSQL = ("select " +
      "[Bedrijfsnaam]" +
     
      ",[Relatienaam]" +
      ",[KvK]" +
      ",[Voornaam]" +
      ",[Achternaam] " +
      ",[Tussenvoegsel] " +
      ",[Geboortedatum] " +
      
      ",[NieuweEmail] " +
      ",[Email]" +

      ",[Straat]" +
      ",[Woonplaats]" +
      ",[Huisnummer]" +
      ",[Toevoeging]" +
      ",[Postcode]" +

      ",[Bedrijf]" +
      ",[SlimmeMeterToestemming] " +
 
    
      " FROM  [Relaties] " +
       "where LTrim(Rtrim(Email))=@Email");



            {
                SqlCommand cmd1 = new SqlCommand(strSQL, con);
                cmd1.Connection = con;
                con.Open();
                cmd1.CommandType = System.Data.CommandType.Text;
                cmd1.Parameters.AddWithValue("@Email", strRelatie_Email.Text.Trim());
                SqlDataReader rdr = cmd1.ExecuteReader();

                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        strRelatie_Email.Text = (string)rdr["Email"].ToString();
                        strRelatie_Bedrijfsnaam.Text = (string)rdr["Bedrijfsnaam"].ToString();
                       
                        strRelatie_Relatienaam.Text = (string)rdr["Relatienaam"].ToString();

                        strRelatie_KvK.Text = (string)rdr["KvK"].ToString();

        //                if (strRelatie_KvK.Text.Trim() != "")
         //               {
         //                   chkBedrijf.Checked = true;  
         //               }
         //               else
         //               {
         //                   chkBedrijf.Checked = false;
         //               }
                       
                        strRelatie_Huisnummer.Text = (string)rdr["Huisnummer"].ToString();
                        strRelatie_Toevoeging.Text = (string)rdr["Toevoeging"].ToString();
                        strRelatie_Postcode.Text = rdr["Postcode"].ToString();
                        strRelatie_Straat.Text = (string)rdr["Straat"].ToString();
                        strRelatie_Woonplaats.Text = rdr["Woonplaats"].ToString();

                        strRelatie_Voornaam.Text = (string)rdr["Voornaam"].ToString();
                        strRelatie_Tussenvoegsel.Text = (string)rdr["Tussenvoegsel"].ToString();
                        strRelatie_Achternaam.Text = (string)rdr["Achternaam"].ToString();
                  //      strRelatie_Geboortedatum.Text = (string)rdr["Geboortedatum"].ToString();
                        string datumString = Convert.ToDateTime(rdr["Geboortedatum"]).ToString("dd-MM-yyyy");
                        strRelatie_Geboortedatum.Text = datumString;
                        chkBedrijf.Checked = (bool)rdr["Bedrijf"];
                        chkSlimmeMeterToestemming.Checked = (bool)rdr["SlimmeMeterToestemming"];
                       
                        Bedrijf();
                    }
                }
                rdr.Close();

            }
        }
    }

    public class Adres
    {
        public string postcode { get; set; }
        public string huisnummer { get; set; }
        public string street { get; set; }
        public string city { get; set; }
    }




    protected void ZoekRelatiePostcode_Click(object sender, EventArgs e)
    {


        strRelatie_Straat.Text = "";
        strRelatie_Woonplaats.Text = "";

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12;


        strRelatie_Postcode.Text = strRelatie_Postcode.Text;
        int intHuisnummer = int.Parse(strRelatie_Huisnummer.Text);
        var client = new RestClient("https://postcode.tech/api/v1/postcode?");

        //gratis postcode check van https://postcode.tech/home, zij gaven het API token

        string APIKEY = "fd6ef2ea-3bca-4255-8501-79c1dd5cafc4";

        //var url = "https://postcode.tech/api/v1/postcode?";
        var request = new RestRequest();
        request.Method = Method.Get;

        request.AddHeader("Authorization", string.Format("Bearer {0}", APIKEY));


        request.AddQueryParameter("postcode", strRelatie_Postcode.Text);
        request.AddQueryParameter("number", strRelatie_Huisnummer.Text);

        RestResponse response = (RestResponse)client.Execute(request);

        var content = response.Content;
        Adres adres = JsonConvert.DeserializeObject<Adres>(content);


        strRelatie_Straat.Text = adres.street;
        strRelatie_Woonplaats.Text = adres.city;

    }



    protected void NieuweRelatieAanmaken_Click(object sender, EventArgs e)
    {





        using (SqlConnection con = new SqlConnection(ConnString))
        {
            int nrRecords = 0;
            //            using (SqlCommand cmd = new SqlCommand("SELECT Count(*) FROM [dbo].[Relaties] Where Naam=@Naam and Postcode=@Postcode and Huisnummer=@Huisnummer and Email=@Email"))
                       using (SqlCommand cmd = new SqlCommand("SELECT Count(*) FROM [dbo].[Relaties] Where  Email=@Email"))

            {
                cmd.CommandType = System.Data.CommandType.Text;
              
                cmd.Parameters.AddWithValue("@Email", strRelatie_Email.Text);
      



                cmd.Connection = con;
                con.Open();
                Object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    nrRecords = int.Parse(result.ToString());
                }


                con.Close();
            }

            if (nrRecords== 0)
                using (SqlCommand cmd = new SqlCommand("insert into [EnergieDB].[dbo].[Relaties] " +
                    "(Email,Bedrijfsnaam, Relatienaam,   Voornaam, Tussenvoegsel, Achternaam, Geboortedatum, Straat, Huisnummer, Toevoeging, Postcode, Woonplaats,KvK,  SlimmeMeterToestemming, Bedrijf ) " +
                     "values " +

                    "(@Email " +

                    ",@Bedrijfsnaam " +
                    ",@Relatienaam " +
                   

                    ",@Voornaam " +
                    ",@Tussenvoegsel " +
                    ",@Achternaam " +
                    ",@Geboortedatum " +

                    ",@Straat " +
                    ",@Huisnummer " +
                    ",@Toevoeging " +
                    ",@Postcode " +
                    ",@Woonplaats " +

                    ",@KvK " +
                    ",@Bedrijf " +

                    ",@SlimmeMeterToestemming )"))
                  
                     


                {
                    cmd.CommandType = System.Data.CommandType.Text;
       
                    cmd.Parameters.AddWithValue("@Email", strRelatie_Email.Text);

                    cmd.Parameters.AddWithValue("@Voornaam", strRelatie_Voornaam.Text);
                    cmd.Parameters.AddWithValue("@Tussenvoegsel", strRelatie_Tussenvoegsel.Text);
                    cmd.Parameters.AddWithValue("@Achternaam", strRelatie_Achternaam.Text);
                    cmd.Parameters.AddWithValue("@Geboortedatum", strRelatie_Geboortedatum.Text);


                    cmd.Parameters.AddWithValue("@Bedrijfsnaam", strRelatie_Bedrijfsnaam.Text);
                   
                    cmd.Parameters.AddWithValue("@Relatienaam", strRelatie_Relatienaam.Text);

                    cmd.Parameters.AddWithValue("@Straat", strRelatie_Straat.Text);
                    cmd.Parameters.AddWithValue("@Huisnummer", strRelatie_Huisnummer.Text);
                    cmd.Parameters.AddWithValue("@Toevoeging", strRelatie_Toevoeging.Text);
                    cmd.Parameters.AddWithValue("@Postcode", strRelatie_Postcode.Text);
                    cmd.Parameters.AddWithValue("@Woonplaats", strRelatie_Woonplaats.Text);
                  
                    cmd.Parameters.AddWithValue("@KvK", strRelatie_KvK.Text);
                    cmd.Parameters.AddWithValue("@Bedrijf", chkBedrijf.Checked);

                    cmd.Parameters.AddWithValue("@SlimmeMeterToestemming", chkSlimmeMeterToestemming.Checked);
                


                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();


                    con.Close();
                    AlertMessage("Nieuwe Relatie aangemaakt");
                
           //         string strUrl = "Relatie.aspx";
           //         Response.Redirect(strUrl); ;
                }
            else
            {
               
                AlertMessage("Relatie bestaat al.");
            }
        }
    }



           
    








    protected void RelatieAanpassen_Click(object sender, EventArgs e)
    {

  //      Geboortedatum = DateTime.ParseExact(strRelatie_Geboortedatum.Text.Trim(), "yyyy.MM.dd", provider);



        using (SqlConnection con = new SqlConnection(ConnString))
        {


            using (SqlCommand cmd = new SqlCommand("update [EnergieDB].[dbo].[Relaties] SET " +
                " Email= @Email " +


                ", Voornaam = @Voornaam" +
                ", Tussenvoegsel = @Tussenvoegsel" +
                ", Achternaam = @Achternaam" +
                ", Geboortedatum = @Geboortedatum" +

                 ",Relatienaam = @Relatienaam" +
             
                 ",BedrijfsNaam = @BedrijfsNaam" +

                ", Straat = @Straat" +
                ", Huisnummer = @Huisnummer" +
                ", Toevoeging = @Toevoeging " +    
                 ",Postcode = @Postcode " +
                ", Woonplaats = @Woonplaats " +

                ", KvK = @KvK "+
                ", Bedrijf = @Bedrijf " +

                ",SlimmeMeterToestemming = @SlimmeMeterToestemming " +
                
                "where  Email=@Email"))


            {
                cmd.CommandType = System.Data.CommandType.Text;
                // cmd.Parameters.AddWithValue("@GebruikersAccountID", strAccountID);
                cmd.Parameters.AddWithValue("@Email", strRelatie_Email.Text);

                //    cmd.Parameters.AddWithValue("@Klantconfig", liKlantconfig.Text);
                cmd.Parameters.AddWithValue("@Bedrijfsnaam", strRelatie_Bedrijfsnaam.Text);
               
                cmd.Parameters.AddWithValue("@Relatienaam", strRelatie_Relatienaam.Text);

                cmd.Parameters.AddWithValue("@Voornaam", strRelatie_Voornaam.Text);
                cmd.Parameters.AddWithValue("@Tussenvoegsel", strRelatie_Tussenvoegsel.Text);
                cmd.Parameters.AddWithValue("@Achternaam", strRelatie_Achternaam.Text);

                
  //             Geboortedatum = DateTime.ParseExact(strRelatie_Geboortedatum.Text.Trim(), "yyyy.MM.dd", provider);

                cmd.Parameters.AddWithValue("@Geboortedatum", Convert.ToDateTime(strRelatie_Geboortedatum.Text));

                cmd.Parameters.AddWithValue("@Straat", strRelatie_Straat.Text);
                cmd.Parameters.AddWithValue("@Huisnummer", strRelatie_Huisnummer.Text);
                cmd.Parameters.AddWithValue("@Toevoeging", strRelatie_Toevoeging.Text);
                cmd.Parameters.AddWithValue("@Postcode", strRelatie_Postcode.Text);
                cmd.Parameters.AddWithValue("@Woonplaats", strRelatie_Woonplaats.Text);

                cmd.Parameters.AddWithValue("@KvK", strRelatie_KvK.Text);
                cmd.Parameters.AddWithValue("@Bedrijf", chkBedrijf.Checked);


                cmd.Parameters.AddWithValue("@SlimmeMeterToestemming", chkSlimmeMeterToestemming.Checked);
           
              


                cmd.Connection = con;
                con.Open();
                cmd.ExecuteNonQuery();
                AlertMessage("Relatiegegevens zijn gewijzigd");
                
                
           

                con.Close();
                lblMeldingAanmaken.Text = "";
                
                
                
            }

        }
    }

    protected void EmailAanpassen_Click(object sender, EventArgs e)
    {







        using (SqlConnection con = new SqlConnection(ConnString))
        {


            using (SqlCommand cmd = new SqlCommand("update [EnergieDB].[dbo].[GebruikersAccount] SET " +
                " Email= @NieuwEmail " +

                "where  Email=@Email"))


            {
                cmd.CommandType = System.Data.CommandType.Text;

                cmd.Parameters.AddWithValue("@Email", strRelatie_Email.Text);
                cmd.Parameters.AddWithValue("@nieuwEmail", strNieuwEmail.Text);



                cmd.Connection = con;
                con.Open();
                cmd.ExecuteNonQuery();


                con.Close();

                 AlertMessage("Emailadres gewijzigd ");
            }

        }
        ConnString = Energie.DataAccess.Configurations.GetApplicationSetting("Generic_DEBUG");

        using (SqlConnection con = new SqlConnection(ConnString))
        {
            using (SqlCommand cmd = new SqlCommand("UPDATE [Klantconfig].[dbo].[Logins] SET " +
                  "Gebruikersnaam = @nieuwEmail " +
                  "where Gebruikersnaam=@Email"))
            {

                cmd.CommandType = System.Data.CommandType.Text;

                cmd.Parameters.AddWithValue("@Email", strRelatie_Email.Text);
                cmd.Parameters.AddWithValue("@nieuwEmail", strNieuwEmail.Text);



                cmd.Connection = con;
                con.Open();
                cmd.ExecuteNonQuery();


                con.Close();
                strRelatie_Email.Text = strNieuwEmail.Text;
                strNieuwEmail.Text = "";
                AlertMessage("Emailadres gewijzigd in Account en Login");
              //  string strUrl = "Relatie.aspx";
              //  Response.Redirect(strUrl); ;

            }

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











    protected void NieuweKlachtAanmaken_Click(object sender, EventArgs e)
    {

        string strUrl = "Klachten.aspx";

        Response.Redirect(strUrl);
    }


}









