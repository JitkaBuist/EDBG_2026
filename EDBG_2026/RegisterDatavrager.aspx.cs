using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Threading;
using Energie.Car;


public partial class RegisterDatavrager : System.Web.UI.Page
{
    protected String ConnString = "";
    protected SqlConnection conn;
    protected SqlCommand cmd;
    protected string strSql;

    protected DataTable dtPortfolio;
    protected SqlDataAdapter daPortfolio;
    protected DataTable dtReport;
    protected SqlDataAdapter daReport;

    protected CultureInfo provider = CultureInfo.InvariantCulture;
    protected string Klant_Config = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["KlantConfig"] == null || Session["KlantConfig"].ToString() == "")
        {
            string strUrl = "Login.aspx";
            Response.Redirect(strUrl);
        }



        KC.KlantConfig = Session["KlantConfig"].ToString().ToUpper();
        KC.ConnStringPortaal = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");

        ConnString = KC.ConnString;



        if (!IsPostBack)
        {
            conn = new SqlConnection(ConnString);
            conn.Open();

            strSql = " SELECT [Organisatie]      ,CONCAT('''', EAN) AS  EAN     ,[Adres]      FROM [EnergieDB].[dbo].[RegisterDatavrager] ";



            cmd = new SqlCommand(strSql, conn);

            dtReport = new DataTable();
            daReport = new SqlDataAdapter(cmd);

            daReport.Fill(dtReport);

            litOverzicht.Text = "";

            litOverzicht.Text = "";
            foreach (DataRow drReport in dtReport.Rows)
            {
                litOverzicht.Text += "<tr>";
                litOverzicht.Text += "<td>" + drReport["Organisatie"].ToString() + "</td>";
                litOverzicht.Text += "<td>" + drReport["EAN"].ToString() + "</td>";
                litOverzicht.Text += "<td>" + drReport["Adres"].ToString() + "</td>";

                litOverzicht.Text += "</tr>";
            }





            DataBind();

            conn.Close();
        }
    }

}