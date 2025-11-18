using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;
using System.Web.UI.WebControls;
using Energie.Car;

public partial class RegisterAangeslotene : System.Web.UI.Page

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
    private string SearchTerm
    {
        get
        {
            var val = ViewState["SearchTerm"];
            return val == null ? string.Empty : val.ToString();
        }
        set
        {
            ViewState["SearchTerm"] = value;
        }
    }

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
            SELECT [ID]
      ,[Bedrijfsnaam]
      ,[Status]
      
      ,[Voorletters]
      ,[Voornaam]
      ,[Tussenvoegsel]
      ,[Achternaam]

      ,[Email]  
      ,[NieuweEmail]
      ,[Geboortedatum]
      ,[Telefoonnummer]

      ,[Straat]     
      ,[Huisnummer]
      ,[Toevoeging]
      ,[Postcode]
      ,[Woonplaats]

      ,[KvK]
      ,[SlimmeMeterToestemming]

      ,[Bedrijf]
  

            FROM dbo.RegisterAangeslotene
            WHERE (status = 'Aangeslotene' AND ( @q = '' OR Bedrijfsnaam LIKE @like OR Email LIKE @like ) )
          

            ORDER BY Bedrijfsnaam;", con))
        using (var da = new SqlDataAdapter(cmd))
        {
            string q = (SearchTerm == null ? string.Empty : SearchTerm.Trim());


            cmd.Parameters.Add("@q", SqlDbType.NVarChar, 200).Value = q;
            cmd.Parameters.Add("@like", SqlDbType.NVarChar, 200).Value = "%" + q + "%";
            var dt = new DataTable();
            da.Fill(dt);
            gvRegisterAangeslotene.DataSource = dt;
            gvRegisterAangeslotene.DataBind();
        }
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        SearchTerm = txtSearch.Text.Trim();
        gvRegisterAangeslotene.PageIndex = 0;   // vanaf pagina 1 na zoeken
        BindGrid();
    }

    protected void btnClear_Click(object sender, EventArgs e)
    {
        txtSearch.Text = string.Empty;
        SearchTerm = string.Empty;
        gvRegisterAangeslotene.PageIndex = 0;
        BindGrid();
    }

    protected void gv_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvRegisterAangeslotene.PageIndex = e.NewPageIndex;
        BindGrid();
    }
    protected void gv_RowEditing(object sender, GridViewEditEventArgs e)
    {
        gvRegisterAangeslotene.EditIndex = e.NewEditIndex;
        BindGrid();
    }

    protected void gv_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        gvRegisterAangeslotene.EditIndex = -1;
        BindGrid();
    }

    protected void gv_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        // Key
        var oldID = gvRegisterAangeslotene.DataKeys[e.RowIndex].Value.ToString();

        // Nieuwe waarden
        var row = gvRegisterAangeslotene.Rows[e.RowIndex];
        string id = gvRegisterAangeslotene.DataKeys[e.RowIndex].Value.ToString();
        string bedrijfsnaam = ((TextBox)row.FindControl("txtBedrijfsnaam_Edit")).Text.Trim();
        string status = ((DropDownList)row.FindControl("cmbStatus_Edit")).SelectedValue;
        string voorletters = ((TextBox)row.FindControl("txtVoorletters_Edit")).Text.Trim();
        string voornaam = ((TextBox)row.FindControl("txtVoornaam_Edit")).Text.Trim();
        string tussenvoegsel = ((TextBox)row.FindControl("txtTussenvoegsel_Edit")).Text.Trim();
        string achternaam = ((TextBox)row.FindControl("txtAchternaam_Edit")).Text.Trim();
        string straat = ((TextBox)row.FindControl("txtStraat_Edit")).Text.Trim();
        string huisnummer = ((TextBox)row.FindControl("txtHuisnummer_Edit")).Text.Trim();
        string toevoeging = ((TextBox)row.FindControl("txtToevoeging_Edit")).Text.Trim();
        string postcode = ((TextBox)row.FindControl("txtPostcode_Edit")).Text.Trim();
        string woonplaats = ((TextBox)row.FindControl("txtWoonplaats_Edit")).Text.Trim();
        string email = ((TextBox)row.FindControl("txtEmail_Edit")).Text.Trim();
        string geboortedatum = ((TextBox)row.FindControl("txtGeboortedatum_Edit")).Text.Trim();
        string telefoonnummer = ((TextBox)row.FindControl("txtTelefoonnummer_Edit")).Text.Trim();
        string kvk = ((TextBox)row.FindControl("txtKVK_Edit")).Text.Trim();


        using (var con = new SqlConnection(ConnString))
        using (var cmd = new SqlCommand(@"
            UPDATE dbo.RegisterAangeslotene
            SET     Bedrijfsnaam    =   @Bedrijfsnaam,
                    KVK             =   @KVK,
                    Status          =   @Status,
                    Voorletters     =   @Voorletters
                    Voornaam        =   @Voornaam,
                    Tussenvoegsel   =   @Tussenvoegsel,
                    Achternaam      =   @Achternaam,
                    Straat          =   @Straat,
                    Huisnummer      =   @Huisnummer,
                    Toevoeging      =   @Toevoeging
                    Postcode        =   @Postcode,
                    Woonplaats      =   @Woonplaats,
                    Email           =   @Email,
                    Telefoonnummer  =   @Telefoonnummer,
                    Geboortedatum   =   @Geboortedatum
            WHERE   ID = @ID ;", con))
        {
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@Bedrijfsnaam", (object)bedrijfsnaam ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object)status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Voorletters", (object)voorletters ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Voornaam", (object)voornaam ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Tussenvoegsel", (object)tussenvoegsel ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Achternaam", (object)achternaam ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Huisnummer", (object)huisnummer ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Toevoeging", (object)toevoeging ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Straat", (object)straat ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Postcode", (object)postcode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Woonplaats", (object)woonplaats ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Telefoonnummer", (object)telefoonnummer ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Geboortedatum", (object)geboortedatum ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@KVK", (object)kvk ?? DBNull.Value);


            con.Open();
            int affected = cmd.ExecuteNonQuery(); // <-- hoort 1 te zijn
                                                  // optioneel: feedback bij 0
        }

        gvRegisterAangeslotene.EditIndex = -1;
        BindGrid();
    }
    protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Insert")
        {
            GridViewRow footer = gvRegisterAangeslotene.FooterRow;

            //   string id = ((TextBox)footer.FindControl("txtID_New")).Text.Trim();
            string bedrijfsnaam = ((TextBox)footer.FindControl("txtBedrijfsnaam_New")).Text.Trim();
            string status = ((DropDownList)footer.FindControl("cmbStatus_New")).SelectedValue;
            string voorletters = ((TextBox)footer.FindControl("txtVoorletters_New")).Text.Trim();
            string voornaam = ((TextBox)footer.FindControl("txtVoornaam_New")).Text.Trim();
            string tussenvoegsel = ((TextBox)footer.FindControl("txtTussenvoegsel_New")).Text.Trim();
            string naam = ((TextBox)footer.FindControl("txtNaam_New")).Text.Trim();
            string huisnummer = ((TextBox)footer.FindControl("txtHuisnummer_New")).Text.Trim();
            string toevoeging = ((TextBox)footer.FindControl("txtToevoeging_NEW")).Text.Trim();
            string straat = ((TextBox)footer.FindControl("txtStraat_New")).Text.Trim();
            string postcode = ((TextBox)footer.FindControl("txtPostcode_New")).Text.Trim();
            string woonplaats = ((TextBox)footer.FindControl("txtWoonplaats_New")).Text.Trim();
            string email = ((TextBox)footer.FindControl("txtEmail_New")).Text.Trim();
            string geboortedatum = ((TextBox)footer.FindControl("txtGeboortedatum_New")).Text.Trim();
            string telefoonnummer = ((TextBox)footer.FindControl("txtTelefoonnummer_New")).Text.Trim();
            string kvk = ((TextBox)footer.FindControl("txtKVK_New")).Text.Trim();

            // Validaties (minimaal):
            //         if (string.IsNullOrEmpty(id))
            //          {
            // TODO: toon nette melding aan gebruiker
            //               return;
            //          }



            using (var con = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(@"
                INSERT INTO dbo.RegisterAangeslotene (Bedrijfsnaam, Status, Voorletters, Voornaam, Tussenvoegsel, Naam, Straat, Huisnummer, Toevoeging, Woonplaats, Email, Geboortedatum, Telefoonnummer, KVK)
                VALUES ( @Bedrijfsnaam, @Status, @Voorletters, @Voornaam, @Tussenvoegsel, @Naam , @Straat, @Huisnummer, @Toevoeging, @Woonplaats, @Email, @Geboortedatum, @Telefoonnummer, @KVK);", con))
            {

                //        cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = ID;
                cmd.Parameters.AddWithValue("@Bedrijfsnaam", (object)bedrijfsnaam ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", (object)status ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Voorletters", (object)voorletters ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Voornaam", (object)voornaam ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Tussenvoegsel", (object)tussenvoegsel ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Naam", (object)naam ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Huisnummer", (object)huisnummer ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Toevoeging", (object)toevoeging ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Straat", (object)straat ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Postcode", (object)postcode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Woonplaats", (object)woonplaats ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Telefoonnummer", (object)telefoonnummer ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Geboortedatum", (object)geboortedatum ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KVK", (object)kvk ?? DBNull.Value);
                ;

                con.Open();
                cmd.ExecuteNonQuery();
            }

            BindGrid();
        }
    }
}
