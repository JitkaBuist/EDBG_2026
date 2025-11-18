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
using Energie.Car;

public partial class _Default : System.Web.UI.Page
{
    protected String ConnString = "";
    protected SqlConnection conn;
    protected SqlCommand cmd;
    protected string strSql;
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
        //ConnString = Session["ConnString"].ToString();

        if (!IsPostBack)
        {
            if (Session["KlantConfig"].ToString().ToUpper() != "GRID_REPORTS")
            {

                conn = new SqlConnection(ConnString);
                conn.Open();

                strSql = "Select count(*) from messages.dbo.Inbox";
                cmd = new SqlCommand(strSql, conn);
                string result = cmd.ExecuteScalar().ToString();

                lblInAantal.Text = "Aantal : " + result;

                strSql = "Select count(*) from messages.dbo.Inbox where processed=0";
                cmd = new SqlCommand(strSql, conn);
                result = cmd.ExecuteScalar().ToString();

                lblInNietVerwerkt.Text = "Niet verwerkt : " + result;

                strSql = "Select count(*) from messages.dbo.Outbox";
                cmd = new SqlCommand(strSql, conn);
                result = cmd.ExecuteScalar().ToString();

                lblUitAantal.Text = "Aantal : " + result;

                strSql = "Select count(*) from messages.dbo.Outbox where BerichtStatus='TE_VERSTUREN'";
                cmd = new SqlCommand(strSql, conn);
                result = cmd.ExecuteScalar().ToString();

                lblNietVerzonden.Text = "Niet verzonden : " + result;

                //Out
                strSql = "select ISNULL(count(*),0) from Messages.dbo.ApplicationLogs where SourceID=1 and TimeStmp>=DATEADD(MINUTE, -6, GETDATE())";
                cmd = new SqlCommand(strSql, conn);
                int intOut = (int)cmd.ExecuteScalar();
                //Out
                strSql = "select ISNULL(count(*),0) from Messages.dbo.ApplicationLogs where SourceID=2 and TimeStmp>=DATEADD(MINUTE, -6, GETDATE())";
                cmd = new SqlCommand(strSql, conn);
                int intIn = (int)cmd.ExecuteScalar();

                String strServicePanel = "";
                if (intIn == 0 || intOut == 0)
                {
                    strServicePanel = "<div class=\"panel alert\" data-role=\"panel\">";
                }
                else
                {
                    strServicePanel = "<div class=\"panel success\" data-role=\"panel\">";
                }



                if (Session["KlantConfig"].ToString().ToUpper() == "LV_BASCUUL")
                {
                    strServicePanel = strServicePanel + "<div class=\"heading\">";
                    strServicePanel = strServicePanel + "<span class=\"title\">Services</span>";
                    strServicePanel = strServicePanel + "</div>";
                    strServicePanel = strServicePanel + "<div class=\"content\">";
                    if (intOut == 0)
                    {
                        strServicePanel = strServicePanel + "<h5>Send Service Status : Stop</h5>";
                    }
                    else
                    {
                        strServicePanel = strServicePanel + "<h5>Send Service Status : OK</h5>";
                    }
                    if (intIn == 0)
                    {
                        strServicePanel = strServicePanel + "<h5>Receive Service Status : Stop</h5>";
                    }
                    else
                    {
                        strServicePanel = strServicePanel + "<h5>Receive Service Status : OK</h5>";
                    }
                    strServicePanel = strServicePanel + "<h5>EDSN Service Status : Stop</h5>";

                    strServicePanel = strServicePanel + "</div>";
                    strServicePanel = strServicePanel + "</div>";
                }
                if (Session["KlantConfig"].ToString().ToUpper() == "PV_BASCUUL")
                {
                    strServicePanel = strServicePanel + "<div class=\"heading\">";
                    strServicePanel = strServicePanel + "<span class=\"title\">Services</span>";
                    strServicePanel = strServicePanel + "</div>";
                    strServicePanel = strServicePanel + "<div class=\"content\">";
                    if (intOut == 0)
                    {
                        strServicePanel = strServicePanel + "<h5>Send Service Status : Stop</h5>";
                    }
                    else
                    {
                        strServicePanel = strServicePanel + "<h5>Send Service Status : OK</h5>";
                    }
                    if (intIn == 0)
                    {
                        strServicePanel = strServicePanel + "<h5>Receive Service Status : Stop</h5>";
                    }
                    else
                    {
                        strServicePanel = strServicePanel + "<h5>Receive Service Status : OK</h5>";
                    }
                    strServicePanel = strServicePanel + "<h5>EDSN Service Status : Stop</h5>";

                    strServicePanel = strServicePanel + "</div>";
                    strServicePanel = strServicePanel + "</div>";
                }
                if (Session["KlantConfig"].ToString().ToUpper() == "ETPA")
                {
                    strServicePanel = strServicePanel + "<div class=\"heading\">";
                    strServicePanel = strServicePanel + "<span class=\"title\">Services</span>";
                    strServicePanel = strServicePanel + "</div>";
                    strServicePanel = strServicePanel + "<div class=\"content\">";
                    if (intOut == 0)
                    {
                        strServicePanel = strServicePanel + "<h5>Send Service Status : Stop</h5>";
                    }
                    else
                    {
                        strServicePanel = strServicePanel + "<h5>Send Service Status : OK</h5>";
                    }
                    if (intIn == 0)
                    {
                        strServicePanel = strServicePanel + "<h5>Receive Service Status : Stop</h5>";
                    }
                    else
                    {
                        strServicePanel = strServicePanel + "<h5>Receive Service Status : OK</h5>";
                    }
                    strServicePanel = strServicePanel + "<h5>EDSN Service Status : Stop</h5>";

                    strServicePanel = strServicePanel + "</div>";
                    strServicePanel = strServicePanel + "</div>";
                }
                if (Session["KlantConfig"].ToString().ToUpper() == "COP")
                {
                    strServicePanel = strServicePanel + "<div class=\"heading\">";
                    strServicePanel = strServicePanel + "<span class=\"title\">Services</span>";
                    strServicePanel = strServicePanel + "</div>";
                    strServicePanel = strServicePanel + "<div class=\"content\">";
                    if (intOut == 0)
                    {
                        strServicePanel = strServicePanel + "<h5>Send Service Status : Stop</h5>";
                    }
                    else
                    {
                        strServicePanel = strServicePanel + "<h5>Send Service Status : OK</h5>";
                    }
                    if (intIn == 0)
                    {
                        strServicePanel = strServicePanel + "<h5>Receive Service Status : Stop</h5>";
                    }
                    else
                    {
                        strServicePanel = strServicePanel + "<h5>Receive Service Status : OK</h5>";
                    }
                    strServicePanel = strServicePanel + "<h5>EDSN Service Status : Stop</h5>";

                    strServicePanel = strServicePanel + "</div>";
                    strServicePanel = strServicePanel + "</div>";
                }


                if (Session["KlantConfig"].ToString().ToUpper() == "LV_GRID")
                {
                    strServicePanel = strServicePanel + "<div class=\"heading\">";
                    strServicePanel = strServicePanel + "<span class=\"title\">Services</span>";
                    strServicePanel = strServicePanel + "</div>";
                    strServicePanel = strServicePanel + "<div class=\"content\">";
                    if (intOut == 0)
                    {
                        strServicePanel = strServicePanel + "<h5>Send Service Status : Stop</h5>";
                    }
                    else
                    {
                        strServicePanel = strServicePanel + "<h5>Send Service Status : OK</h5>";
                    }
                    if (intIn == 0)
                    {
                        strServicePanel = strServicePanel + "<h5>Receive Service Status : Stop</h5>";
                    }
                    else
                    {
                        strServicePanel = strServicePanel + "<h5>Receive Service Status : OK</h5>";
                    }
                    strServicePanel = strServicePanel + "<h5>EDSN Service Status : Stop</h5>";

                    strServicePanel = strServicePanel + "</div>";
                    strServicePanel = strServicePanel + "</div>";
                }

                if (Session["KlantConfig"].ToString().ToUpper() == "EDBG")
                {
                    strServicePanel = strServicePanel + "<div class=\"heading\">";
                    strServicePanel = strServicePanel + "<span class=\"title\">Services</span>";
                    strServicePanel = strServicePanel + "</div>";
                    strServicePanel = strServicePanel + "<div class=\"content\">";
                    if (intOut == 0)
                    {
                        strServicePanel = strServicePanel + "<h5>Send Service Status : Stop</h5>";
                    }
                    else
                    {
                        strServicePanel = strServicePanel + "<h5>Send Service Status : OK</h5>";
                    }
                    if (intIn == 0)
                    {
                        strServicePanel = strServicePanel + "<h5>Receive Service Status : Stop</h5>";
                    }
                    else
                    {
                        strServicePanel = strServicePanel + "<h5>Receive Service Status : OK</h5>";
                    }
                    strServicePanel = strServicePanel + "<h5>EDSN Service Status : Stop</h5>";

                    strServicePanel = strServicePanel + "</div>";
                    strServicePanel = strServicePanel + "</div>";
                }

                //litServiceStatus.Text = strServicePanel;

                conn.Close();
            }
        }
    }
}