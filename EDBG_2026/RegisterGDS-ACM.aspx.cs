using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;
using System.Web.UI.WebControls;
using Energie.Car;

public partial class RegisterGDSACM : System.Web.UI.Page

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
        if (!IsPostBack) BindGrid();

    }



    private void BindGrid()
    {
        using (var con = new SqlConnection(ConnString))
        using (var cmd = new SqlCommand(@"
            SELECT   [Datum_van_beschikking]
            ,[Zaaknummer]
            ,[Partijen]
            ,[Plaats]
            ,[Soort_besluit]
            ,[Soort_E_of_G]
            ,[EAN]
            ,[Status]
            FROM [EnergieDB].[dbo].[RegisterGDSACM]

            ORDER BY Partijen;", con))
        using (var da = new SqlDataAdapter(cmd))
        {
            var dt = new DataTable();
            da.Fill(dt);
            gvGDSACM.DataSource = dt;
            gvGDSACM.DataBind();
        }
    }

    protected void gv_RowEditing(object sender, GridViewEditEventArgs e)
    {
        gvGDSACM.EditIndex = e.NewEditIndex;
        BindGrid();
    }

    protected void gv_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        gvGDSACM.EditIndex = -1;
        BindGrid();
    }

    protected void gv_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        // Key
        var oldEAN = gvGDSACM.DataKeys[e.RowIndex].Value.ToString();

        // Nieuwe waarden
        var row = gvGDSACM.Rows[e.RowIndex];
        string ean = gvGDSACM.DataKeys[e.RowIndex].Value.ToString();
        string status = ((TextBox)row.FindControl("txtStatus_Edit")).Text.Trim();
        string partijen = ((TextBox)row.FindControl("txtPartijen_Edit")).Text.Trim();
        string plaats = ((TextBox)row.FindControl("txtPlaats_Edit")).Text.Trim();
        string zaaknummer = ((TextBox)row.FindControl("txtZaaknummer_Edit")).Text.Trim();
        string soort_E_of_G = ((TextBox)row.FindControl("txtSoort_E_of_G_Edit")).Text.Trim();
        string soort_besluit= ((TextBox)row.FindControl("txtSoort_besluit_Edit")).Text.Trim();
        string datum_van_beschikking = ((TextBox)row.FindControl("txtDatum_van_beschikking_Edit")).Text.Trim();



        using (var con = new SqlConnection(ConnString))
        using (var cmd = new SqlCommand(@"
        UPDATE dbo.RegisterGDSACM
           SET Status = @Status,
               Partijen   = @Partijen,
               Adres  = @Adres,
               Zaaknummer  = @Zaaknummer,
               Soort_E_of_G = @Soort_E_of_G,
               Soort_besluit = @Soort_besluit
               Datum_van_beschikking  = @Datum_van_beschikking
         WHERE Zaaknummer = @Zaaknummer;", con))
        {
            cmd.Parameters.AddWithValue("@EAN", ean);
            cmd.Parameters.AddWithValue("@Status", (object)status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Partijen", (object)partijen ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Plaats", (object)plaats ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Soort_besluit", (object)soort_besluit ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Soort_E_of_G", (object)soort_E_of_G ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Zaaknummer", (object)zaaknummer ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Datum_van_beschikking", (object)datum_van_beschikking ?? DBNull.Value);

            con.Open();
            int affected = cmd.ExecuteNonQuery(); // <-- hoort 1 te zijn
                                                  // optioneel: feedback bij 0
        }

        gvGDSACM.EditIndex = -1;
        BindGrid();
    }
    protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Insert")
        {
            GridViewRow footer = gvGDSACM.FooterRow;

            string ean = ((TextBox)footer.FindControl("txtEAN_New")).Text.Trim();
            string status = ((TextBox)footer.FindControl("txtStatus_New")).Text.Trim();
            string partijen = ((TextBox)footer.FindControl("txtPartijen_New")).Text.Trim();
            string plaats = ((TextBox)footer.FindControl("txtPlaats_New")).Text.Trim();
            string soort_E_of_G = ((TextBox)footer.FindControl("txtSoort_E_of_G_New")).Text.Trim();
            string soort_besluit = ((TextBox)footer.FindControl("txtSoort_besluit_New")).Text.Trim();
            string zaaknummer = ((TextBox)footer.FindControl("txtZaaknummer_New")).Text.Trim();
            string datum_van_beschikking = ((TextBox)footer.FindControl("txtDatum_van_beschikking_New")).Text.Trim();

            // Validaties (minimaal):
            if (string.IsNullOrEmpty(ean))
            {
                // TODO: toon nette melding aan gebruiker
                return;
            }



            using (var con = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(@"
                INSERT INTO dbo.RegisterGDSACM ( [Datum_van_beschikking]
            ,[Zaaknummer]
            ,[Partijen]
            ,[Plaats]
            ,[Soort_besluit]
            ,[Soort_E_of_G]
            ,[EAN]
            ,[Status])
                VALUES (@EAN, @Status, @Naam, @Adres, @Email, @Datum);", con))
            {

             
                cmd.Parameters.AddWithValue("@EAN", ean);
                cmd.Parameters.AddWithValue("@Status", (object)status ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Partijen", (object)partijen ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Plaats", (object)plaats ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Soort_besluit", (object)soort_besluit ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Soort_E_of_G", (object)soort_E_of_G ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Zaaknummer", (object)zaaknummer ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Datum_van_beschikking", (object)datum_van_beschikking ?? DBNull.Value);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            BindGrid();
        }
    }
}
