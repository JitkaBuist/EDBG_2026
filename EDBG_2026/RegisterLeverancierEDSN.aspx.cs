using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;
using System.Web.UI.WebControls;
using Energie.Car;

public partial class RegisterLeverancierEDSN : System.Web.UI.Page

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
            SELECT EAN, Status, Naam, Adres, Email, Datum
            FROM dbo.RegisterLeveranciers
            ORDER BY Naam;", con))
        using (var da = new SqlDataAdapter(cmd))
        {
            var dt = new DataTable();
            da.Fill(dt);
            gvLeverancierEDSN.DataSource = dt;
            gvLeverancierEDSN.DataBind();
        }
    }

    protected void gv_RowEditing(object sender, GridViewEditEventArgs e)
    {
        gvLeverancierEDSN.EditIndex = e.NewEditIndex;
        BindGrid();
    }

    protected void gv_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        gvLeverancierEDSN.EditIndex = -1;
        BindGrid();
    }

    protected void gv_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        // Key
        var oldEAN = gvLeverancierEDSN.DataKeys[e.RowIndex].Value.ToString();

        // Nieuwe waarden
        var row = gvLeverancierEDSN.Rows[e.RowIndex];
        string ean = gvLeverancierEDSN.DataKeys[e.RowIndex].Value.ToString();
        string status = ((TextBox)row.FindControl("txtStatus_Edit")).Text.Trim();
        string naam = ((TextBox)row.FindControl("txtNaam_Edit")).Text.Trim();
        string adres = ((TextBox)row.FindControl("txtAdres_Edit")).Text.Trim();
        string email = ((TextBox)row.FindControl("txtEmail_Edit")).Text.Trim();
        string datum = ((TextBox)row.FindControl("txtDatum_Edit")).Text.Trim();


        using (var con = new SqlConnection(ConnString))
        using (var cmd = new SqlCommand(@"
        UPDATE dbo.RegisterLeveranciers
           SET Status = @Status,
               Naam   = @Naam,
               Adres  = @Adres,
               Email  = @Email,
               Datum  = @Datum
         WHERE EAN = @EAN;", con))
        {
            cmd.Parameters.AddWithValue("@EAN", ean);
            cmd.Parameters.AddWithValue("@Status", (object)status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Naam", (object)naam ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Adres", (object)adres ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Datum", (object)datum ?? DBNull.Value);

            con.Open();
            int affected = cmd.ExecuteNonQuery(); // <-- hoort 1 te zijn
                                                  // optioneel: feedback bij 0
        }

        gvLeverancierEDSN.EditIndex = -1;
        BindGrid();
    }
    protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Insert")
        {
            GridViewRow footer = gvLeverancierEDSN.FooterRow;

            string ean = ((TextBox)footer.FindControl("txtEAN_New")).Text.Trim();
            string status = ((TextBox)footer.FindControl("txtStatus_New")).Text.Trim();
            string naam = ((TextBox)footer.FindControl("txtNaam_New")).Text.Trim();
            string adres = ((TextBox)footer.FindControl("txtAdres_New")).Text.Trim();
            string email = ((TextBox)footer.FindControl("txtEmail_New")).Text.Trim();
            string datum = ((TextBox)footer.FindControl("txtDatum_New")).Text.Trim();

            // Validaties (minimaal):
            if (string.IsNullOrEmpty(ean))
            {
                // TODO: toon nette melding aan gebruiker
                return;
            }



            using (var con = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(@"
                INSERT INTO dbo.RegisterLeveranciers (EAN, Status, Naam, Adres, Email, Datum)
                VALUES (@EAN, @Status, @Naam, @Adres, @Email, @Datum);", con))
            {

                cmd.Parameters.Add("@EAN", SqlDbType.BigInt).Value = ean;
                cmd.Parameters.AddWithValue("@Status", (object)status ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Naam", (object)naam ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Adres", (object)adres ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Datum", (object)datum ?? DBNull.Value);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            BindGrid();
        }
    }
}
