using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;
using System.Web.UI.WebControls;
using Energie.Car;

public partial class RegisterLeverancierACM : System.Web.UI.Page

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
            SELECT  Naam_vergunninghouder, E_G, status_vergunning as Status
            FROM   dbo.LeveranciersACM inner JOIN
             dbo.StatusLeveranciersACM ON dbo.LeveranciersACM.VergunninghouderID = dbo.StatusLeveranciersACM.VergunninghouderID
        ;", con))
        using (var da = new SqlDataAdapter(cmd))
        {
            var dt = new DataTable();
            da.Fill(dt);
            gvLeverancierACM.DataSource = dt;
            gvLeverancierACM.DataBind();
        }
    }

    protected void gv_RowEditing(object sender, GridViewEditEventArgs e)
    {
        gvLeverancierACM.EditIndex = e.NewEditIndex;
        BindGrid();
    }

    protected void gv_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        gvLeverancierACM.EditIndex = -1;
        BindGrid();
    }

    protected void gv_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        // Key
        var oldEAN = gvLeverancierACM.DataKeys[e.RowIndex].Value.ToString();

        // Nieuwe waarden
        var row = gvLeverancierACM.Rows[e.RowIndex];
        
        string status_vergunning = ((TextBox)row.FindControl("txtStatus_Edit")).Text.Trim();
        string naam_vergunninghouder = ((TextBox)row.FindControl("txtNaam_vergunninghouder_Edit")).Text.Trim();
        string e_g = ((TextBox)row.FindControl("txtE_G_Edit")).Text.Trim();
    
      


        using (var con = new SqlConnection(ConnString))
        using (var cmd = new SqlCommand(@"
        UPDATE dbo.RegisterLeveranciers
           SET Naam_vergunninghouder = @Naam_vergunninghouder,
               status_vergunning   = @Status_vergunning,
               e_g = @E_G
           
       ;", con))
        {
            
            cmd.Parameters.AddWithValue("@Status_vergunning", (object)status_vergunning ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Naam_vergunninghouder", (object)naam_vergunninghouder ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@E_G", (object)e_g ?? DBNull.Value);
            

            con.Open();
            int affected = cmd.ExecuteNonQuery(); // <-- hoort 1 te zijn
                                                  // optioneel: feedback bij 0
        }

        gvLeverancierACM.EditIndex = -1;
        BindGrid();
    }
    protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Insert")
        {
            GridViewRow footer = gvLeverancierACM.FooterRow;

         
            string status_vergunning = ((TextBox)footer.FindControl("txtStatus_vergunning _New")).Text.Trim();
            string naam_vergunninghouder = ((TextBox)footer.FindControl("txtNaam_vergunninghouder_New")).Text.Trim();
            string e_g = ((TextBox)footer.FindControl("txtE_G_New")).Text.Trim();
           

         



            using (var con = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(@"
                INSERT INTO dbo.RegisterLeveranciersACM (Status_vergunning , Naam, E_G)
                VALUES (@Status_vergunning, @Naam_vergunninghouder, @E_G);", con))
            {

                cmd.Parameters.AddWithValue("@status_vergunning ", (object)status_vergunning ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Naam_vergunninghouder", (object)naam_vergunninghouder ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@E_G", (object)e_g ?? DBNull.Value);
              

                con.Open();
                cmd.ExecuteNonQuery();
            }

            BindGrid();
        }
    }
}
