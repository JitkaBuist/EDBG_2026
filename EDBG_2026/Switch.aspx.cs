using Energie.Car;
using Newtonsoft.Json;

using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Switch : System.Web.UI.Page
{
    protected String ConnString = "";
    protected SqlConnection conn;
    protected DataSet dsGet;
    protected CultureInfo provider = CultureInfo.InvariantCulture;
    protected SqlCommand cmd;
    protected SqlDataAdapter daSwitch;
    protected DataSet dsSwitch;
    protected DataRow drSwitch;
  //  protected int Klant_ID_ELK = -1;
  //  protected int Klant_ID_GAS = -1;

    protected int Klant_ID = -1;
    protected int intFacturatieRelatieID = -1;
    //protected int Account_ID = -1;
    protected int Relatie_ID = -1;
    //protected int Account_Relatie_ID = -1;
    protected string Klant_Config = "";
    protected string DossierID = "";
    protected DateTime dtmDatum;
    protected string KvK = "";
    protected CarShared carShared;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["KlantConfig"] == null || Session["KlantConfig"].ToString() == "" )
        {
            string strUrl = "Login.aspx";

            Response.Redirect(strUrl);
        }


        Klant_Config = Session["KlantConfig"].ToString().ToUpper();
        KC.KlantConfig = Session["KlantConfig"].ToString().ToUpper();
        KC.ConnStringPortaal = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");
        KC.App_ID = AppID.EDBG;
        string strKlantconfig = Session["Klantconfig"].ToString();
        liKlantconfig.Text = strKlantconfig;

//       Klant_ID_ELK = KC.KlantIdElk;
 //       Klant_ID_GAS = KC.KlantIdGas;
        ConnString = KC.ConnString;

        carShared = new CarShared();

        if (!IsPostBack)
        {
            conn = new SqlConnection(ConnString);
            conn.Open();

            String strSQL = "select ID, Omschrijving as omschrijving from SwitchBerichtTypes";

            SqlDataAdapter da = new SqlDataAdapter(strSQL, ConnString);
            DataTable dtGet = new DataTable();
            da.Fill(dtGet);

            cmbSwitchSoort.DataSource = dtGet;
            cmbSwitchSoort.DataTextField = "Omschrijving";
            cmbSwitchSoort.DataValueField = "ID";
  //          cmbSwitchSoort.SelectedValue = "8";


            strSQL = "SELECT ID, PV_Code  FROM dbo.PVs order by PV_Code DESC";

            da = new SqlDataAdapter(strSQL, ConnString);
            dtGet = new DataTable();
            da.Fill(dtGet);

            cmbPV.DataSource = dtGet;
            cmbPV.DataTextField = "PV_Code";
            cmbPV.DataValueField = "ID";

            //cmbSwitchSoort.SelectedValue = "5";







            cmbPV.Visible = true;



            DataBind();
            conn.Close();

            Session["Aansluiting_ID"] = "";
     //       Session["Adres_ID"] = "";
        }
    }
    protected void ZoekEAN_Click(object sender, EventArgs e)
    {


        conn = new SqlConnection(ConnString);
        conn.Open();
        int intStap = 0;

        try
        {

            if (txtEANCode.Text.Length == 18)
            {
                String OldEan = "";

                if (Session["OldEan"] == null || Session["OldEan"].ToString() == "")
                {
                    OldEan = "";
                }
                else
                {
                    OldEan = Session["OldEan"].ToString();
                }
                intStap++;

                if (OldEan != txtEANCode.Text.Trim())
                {

                    string SQLStatement = "select MD.EAN18Code as EAN18_Code, MD.AansluitingId as Aansluiting_ID, aan.Aansluitingtype_ID, nb.ID as Netbeheerder_ID, \n";
                    SQLStatement += "MD.RelatieId as RelatieId, aan.Kortenaam, NB.EAN13_Code as NB_EAN, HK.Residential as Verblijfsfunctie, FK.BemeteringsType as Bemeteringtype_ID \n";
                    SQLStatement += "from EnergieDB.masterdata.Masterdata MD \n";
                    SQLStatement += "join EnergieDB.masterdata.HoofdKenmerk HK on HK.HoofdKenmerkId = md.HoofdKenmerkId \n";
                    SQLStatement += "join EnergieDB.masterdata.FysiekeKenmerk FK on FK.FysiekeKenmerkID = md.FysiekeKenmerkID \n";
                    SQLStatement += "join Aansluitingen aan on aan.EAN18_Code = MD.EAN18Code \n";
                    SQLStatement += "join Netbeheerders NB on nb.ID = HK.NetbeheerderId \n";
                    SQLStatement += "where MD.StartDatum <= GETDATE() \n";
                    SQLStatement += "and MD.EindDatum > GETDATE() and MD.EAN18Code = @EANCode ";
                    cmd = new SqlCommand(SQLStatement, conn);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@EANCode", txtEANCode.Text);
                    cmd.CommandTimeout = 12000;
                    daSwitch = new SqlDataAdapter(cmd);
                    dsSwitch = new DataSet();
                    daSwitch.Fill(dsSwitch, "Aansluitingen");
                    intStap++;
                    if (dsSwitch.Tables["Aansluitingen"].Rows.Count > 0)
                    {
                        //Bekende aansluiting met aansluitingdetails
                        drSwitch = dsSwitch.Tables["Aansluitingen"].Rows[0];


                        intStap++;

                        Session["Aansluiting_ID"] = drSwitch["Aansluiting_ID"].ToString();
                        //          Session["Adres_ID"] = drSwitch["Adres_ID"].ToString();
                        Session["Netbeheerder_ID"] = drSwitch["Netbeheerder_ID"].ToString();
                        Session["NetbeheerderEAN"] = drSwitch["NB_EAN"].ToString();
                        lblNetbeheerder.Text = Session["NetbeheerderEAN"].ToString();

                        if (drSwitch["Aansluitingtype_ID"].ToString() != "1")
                        {
                            Session["Leverancier"] = "8714252022926";
                            Session["Producttype"] = "G";
                        }
                        else
                        {
                            Session["Leverancier"] = "8714252022902";
                            Session["Producttype"] = "E";
                        }
                        lblProduct.Text = Session["Producttype"].ToString();

                        //Session["Complexbepaling"] = drSwitch["Verblijfsfunctie"].ToString();

                        if (drSwitch["Bemeteringtype_ID"].ToString() == "7" || drSwitch["Bemeteringtype_ID"].ToString() == "8")
                        {
                            Session["SlimmeMeter"] = "Ja";
                        }
                        else
                        {
                            Session["SlimmeMeter"] = "Nee";
                        }


                        // cmbProduct.Enabled = false;
                    }
                    else
                    {
                        //Bekende aansluiting zonder aansluitingdetails
                        SQLStatement = "select aan.ID, aan.Kortenaam, aan.Aansluitingtype_ID from Aansluitingen aan " +
                        "where aan.EAN18_Code=@EANCode";
                        cmd = new SqlCommand(SQLStatement, conn);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@EANCode", txtEANCode.Text);
                        cmd.CommandTimeout = 12000;
                        daSwitch = new SqlDataAdapter(cmd);
                        if (dsSwitch.Tables.IndexOf("Aansluiting") > -1) { dsSwitch.Tables["Aansluiting"].Rows.Clear(); }
                        daSwitch.Fill(dsSwitch, "Aansluiting");
                        intStap++;
                        if (dsSwitch.Tables["Aansluiting"].Rows.Count > 0)
                        {
                            drSwitch = dsSwitch.Tables["Aansluiting"].Rows[0];
                            Session["Aansluiting_ID"] = drSwitch["ID"].ToString();
                            strAansluiting_Kortenaam.Text = drSwitch["Kortenaam"].ToString();
                            //            txtFactuurRelatieNaam.Text = drSwitch["FactuurRelatieNaam"].ToString();
                            //if (drSwitch["Aansluitingtype_ID"].ToString() == "1")
                            //{
                            //    cmbProduct.Text = "Electriciteit";
                            //}
                            //else
                            //{
                            //    cmbProduct.Text = "Gas";
                            //}

                            SQLStatement = "select * from Switchberichten " +
                                "where Aansluiting_ID=@Aansluiting_ID";
                            cmd = new SqlCommand(SQLStatement, conn);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Aansluiting_ID", drSwitch["ID"]);
                            cmd.CommandTimeout = 12000;
                            daSwitch = new SqlDataAdapter(cmd);
                            if (dsSwitch.Tables.IndexOf("SwitchOud") > -1) { dsSwitch.Tables["SwitchOud"].Rows.Clear(); }
                            daSwitch.Fill(dsSwitch, "SwitchOud");

                            if (dsSwitch.Tables["SwitchOud"].Rows.Count > 0)
                            {

                                drSwitch = dsSwitch.Tables["SwitchOud"].Rows[0];



                                cmbSwitchSoort.SelectedValue = drSwitch["SwitchBerichtType_ID"].ToString();

                            }


                            strAansluiting_Straat.Enabled = true;
                            strAansluiting_Huisnummer.Enabled = true;
                            strAansluiting_Toevoeging.Enabled = true;
                            strAansluiting_Postcode.Enabled = true;
                            strAansluiting_Woonplaats.Enabled = true;
                            strAansluiting_Kortenaam.Enabled = true;
                   


                            //cmbProduct.Enabled = false;
                        }
                        else
                        {
                            drSwitch = null;

                            //cmbFactuurRelatie.Enabled = true;

                            strAansluiting_Straat.Enabled = true;
                            strAansluiting_Huisnummer.Enabled = true;
                            strAansluiting_Toevoeging.Enabled = true;
                            strAansluiting_Postcode.Enabled = true;
                            strAansluiting_Woonplaats.Enabled = true;
                            strAansluiting_Kortenaam.Enabled = true;

                            //cmbProduct.Enabled = true;
                        }
                    }
                    intStap = 6;
                    if (dsSwitch == null || dsSwitch.Tables["Aansluitingen"].Rows.Count == 0)
                    {

                        intStap = 7;
                        String strError = "";

                        intStap = 9;
                        Energie.Car.MasterdataClient masterDataClient = new Energie.Car.MasterdataClient(Session["KlantConfig"].ToString());
                        intStap = 91;
                        nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope meteringpoint = masterDataClient.SearchMeteringPointMP(false, txtEANCode.Text, "", "", "", "", "", "", "", "", 1, out strError);
                        if (strError != "")
                        {
                            AlertMessage(strError);
                        }
                        if (meteringpoint != null)
                        {
                            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope_PC portaal_Response = meteringpoint.Portaal_Content;
                            intStap = 10;
                            //lblEANCode.Text = intStap.ToString();
                            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope_PC_Result portaal_result = (nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope_PC_Result)portaal_Response.Item;
                            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope_PC_Result_PMP[] portaal_MeteringPointResponses = (nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope_PC_Result_PMP[])portaal_result.Portaal_MeteringPoint;
                            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope_PC_Result_PMP portaal_MeteringPointResponse = portaal_MeteringPointResponses[0];
                            intStap = 11;
                            //lblEANCode.Text = intStap.ToString();

                            //if (portaal_MeteringPointResponse.GridOperator_Company != null)
                            //{
                            //    lblEANCode.Text = portaal_MeteringPointResponse.GridOperator_Company.ID.ToString();
                            //}
                            //lblEanNetbeheerder.Text = portaal_MeteringPointResponse.GridOperator_Company.ID.ToString();

                            intStap = 12;
                            //lblEANCode.Text = intStap.ToString();
                            cmd = new SqlCommand("Select ID from Netbeheerders where EAN13_Code=@EAN13_Code", conn);
                            cmd.Parameters.AddWithValue("@EAN13_Code", portaal_MeteringPointResponse.GridOperator_Company.ID.ToString());
                            int intNetbeheerderID = -1;
                            Object objNetbeheerderID = cmd.ExecuteScalar();
                            Session["NetbeheerderEAN"] = portaal_MeteringPointResponse.GridOperator_Company.ID.ToString();
                            lblNetbeheerder.Text = Session["NetbeheerderEAN"].ToString();
                            if (objNetbeheerderID != null)
                            {
                                intNetbeheerderID = (Int32)objNetbeheerderID;
                                Session["Netbeheerder_ID"] = intNetbeheerderID;

                            }
                            else
                            {
                                //Onbekende netbeheerder
                            }

                            intStap = 13;
                            //lblEANCode.Text = intStap.ToString();
                            if (portaal_MeteringPointResponse.ProductType == "GAS")
                            {
                                Session["Leverancier"] = "8714252022926";
                            }
                            else
                            {
                                Session["Leverancier"] = "8714252022902";
                            }
                            intStap = 14;

                            if (portaal_MeteringPointResponse.ProductType == "GAS")
                            {
                                Session["Producttype"] = "G";
                            }
                            else
                            {
                                Session["Producttype"] = "E";
                            }
                            intStap = 15;
                            //lblEANCode.Text = intStap.ToString();
                            lblProduct.Text = Session["Producttype"].ToString();
                            intStap = 16;
                            try
                            {


                            }
                            catch
                            { }
                            //if (portaal_MeteringPointResponse.EDSN_AddressSearch == null) { lblEANCode.Text = "Leeg"; }
                            if (portaal_MeteringPointResponse.EDSN_AddressSearch != null)
                            {
                                if (portaal_MeteringPointResponse.EDSN_AddressSearch.StreetName != null) { strAansluiting_Straat.Text = portaal_MeteringPointResponse.EDSN_AddressSearch.StreetName.ToString(); }
                                if (portaal_MeteringPointResponse.EDSN_AddressSearch.BuildingNr != null) { strAansluiting_Huisnummer.Text = portaal_MeteringPointResponse.EDSN_AddressSearch.BuildingNr.ToString(); }
                                if (portaal_MeteringPointResponse.EDSN_AddressSearch.ExBuildingNr != null) { strAansluiting_Toevoeging.Text = portaal_MeteringPointResponse.EDSN_AddressSearch.ExBuildingNr.ToString(); }
                                if (portaal_MeteringPointResponse.EDSN_AddressSearch.ZIPCode != null) { strAansluiting_Postcode.Text = portaal_MeteringPointResponse.EDSN_AddressSearch.ZIPCode.ToString(); }
                                if (portaal_MeteringPointResponse.EDSN_AddressSearch.CityName != null) { strAansluiting_Woonplaats.Text = portaal_MeteringPointResponse.EDSN_AddressSearch.CityName.ToString(); }
                            }
                        }
                    }
                }


                intStap = 20;
                DataBind();
            }
        }
        catch (Exception ex)
        {
            AlertMessage("Error bij opvragen EAN : " + intStap.ToString() + " - " + ex.Message);
        }
        conn.Close();
    }



    protected void IndienenSwitch_Click(object sender, EventArgs e)
    {
        conn = new SqlConnection(ConnString);
        conn.Open();
        String strError = "1";
        //lblEANCode.Text = strError;
        //if (MessageBox.Show("Wilt u deze switch indienen?", "Bevestigen", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        //{
        string strBericht = "";

        if (strAansluiting_Straat.Text.Trim() == "") { strBericht = strBericht + "Straat, "; }
        //if (txtHuisnummer.Text.Trim() == "") { strBericht = strBericht + "Huisnummer, "; }
        if (strAansluiting_Postcode.Text.Trim() == "") { strBericht = strBericht + "Postcode, "; }
        if (strAansluiting_Woonplaats.Text.Trim() == "") { strBericht = strBericht + "Plaats, "; }
        if (txtEANCode.Text.Trim() == "" || txtEANCode.Text.Trim().Length != 18) { strBericht = strBericht + "Eancode, "; }
        //if (txtHuisnummer.Text.Trim() == "") { strBericht = strBericht + "Huisnummer, "; }
        //if (cmbFactuurModelID1.Text.Trim() == "") { strBericht = strBericht + "Faktuurmodel, "; }
        if (cmbSwitchSoort.Text.Trim() == "") { strBericht = strBericht + "Switch type, "; }
        //if (cmbProduct.Text != "Gas" && cmbVerblijfsfunctie.Text.Trim() == "") { strBericht = strBericht + "Verblijfsfunctie, "; }
        if (Session["Producttype"] == null || Session["Producttype"].ToString().Trim() == "") { strBericht = strBericht + "Product, "; }
        //    if (cmbFactuurRelatie.Text.Trim() == "" && chkAanmakenAccount.Checked == false ) { strBericht = strBericht + "Account, "; } 
        if (Session["Netbeheerder_ID"] == null || Session["Netbeheerder_ID"].ToString() == "") { strBericht = strBericht + "Netbeheerder, "; }

        strError = "2";
        //lblEANCode.Text = strError;

        //lblEANCode.Text = strBericht;
        if (strBericht != "")
        {
            AlertMessage("De volgende velden zijn niet gevuld : " + strBericht);
        }
        else
        {
            //verder met controlle 
            strBericht = "";
            int intCount = 0;

            //Check factuurmodel
            int intFactuurmodelID = 2;// int.Parse(cmbFactuurModelID1.SelectedValue.ToString());
            SqlCommand cmd = new SqlCommand("Select count(*) from Factuurmodel where ID=@ID", conn);
            cmd.Parameters.AddWithValue("@ID", intFactuurmodelID);
            intCount = (Int32)cmd.ExecuteScalar();

            if (intCount != 1)
            {
                strBericht = strBericht + "Factuurmodel, ";
            }

            String strSwitchBerichtCode = "";
            //Check SwitchBerichtTypes
            int intSwitchBerichtTypesID = int.Parse(cmbSwitchSoort.SelectedValue.ToString());
            cmd = new SqlCommand("Select code from SwitchBerichtTypes where ID=@ID", conn);
            cmd.Parameters.AddWithValue("@ID", intSwitchBerichtTypesID);
            object objResult = cmd.ExecuteScalar();

            if (objResult == null)
            {
                strBericht = strBericht + "Switch berichttype, ";
            }
            else
            {
                strSwitchBerichtCode = objResult.ToString();
            }

            if (strSwitchBerichtCode == "COAMET" || strSwitchBerichtCode == "EOSNOT" || strSwitchBerichtCode == "432E21")
            {
                //              chkAanmakenAccount.Checked = false;
            }

            //Check NetbeheerderID
            int intNetbeheerderID = int.Parse(Session["Netbeheerder_ID"].ToString());
            cmd = new SqlCommand("Select count(*) from Netbeheerders where ID=@ID", conn);
            cmd.Parameters.AddWithValue("@ID", intNetbeheerderID);
            intCount = (Int32)cmd.ExecuteScalar();

            if (intCount != 1)
            {
                strBericht = strBericht + "Netbeheerder, ";
            }



            //AansluitingID
            if (Session["Aansluiting_ID"] != null)
            {
                if (Session["Aansluiting_ID"].ToString().Trim() != "")
                {
                    //Check AansluitingID
                    int intAansluitingID = int.Parse(Session["Aansluiting_ID"].ToString().Trim());
                    cmd = new SqlCommand("Select count(*) from Aansluitingen where ID=@ID", conn);
                    cmd.Parameters.AddWithValue("@ID", intAansluitingID);
                    intCount = (Int32)cmd.ExecuteScalar();

                    if (intCount != 1)
                    {
                        strBericht = strBericht + "Aansluiting, ";
                    }

                }
            }

            strError = "4";
            //lblEANCode.Text = strError;

            //Check Verblijfsfunctie
            if (Session["Producttype"].ToString() != "G")
            {
                int intVerblijfsfunctieID = 0;// int.Parse(Session["Complexbepaling"].ToString());
                cmd = new SqlCommand("Select count(*) from Verblijfsfuncties where Code=@ID", conn);
                cmd.Parameters.AddWithValue("@ID", intVerblijfsfunctieID);
                intCount = (Int32)cmd.ExecuteScalar();
                if (intCount != 1)
                {
                    strBericht = strBericht + "Verblijfsfunctie, ";
                }
            }

            strError = "5";
            //lblEANCode.Text = strError;

            //Check Product
            if (Session["Producttype"].ToString().ToUpper().Trim() != "E" && Session["Producttype"].ToString().ToUpper().Trim() != "G")
            {
                strBericht = strBericht + "Product, ";

            }

// Klant_ID_ELK en KLANT_ID_GAS zijn weggehaald 
    //        if (Session["Producttype"].ToString().ToUpper().Trim() != "G") { Klant_ID = Klant_ID_ELK; } else { Klant_ID = Klant_ID_GAS; }

            strError = "6";
            //lblEANCode.Text = strError;

            //nieuwe PV ChangeOfPV
            //  string strPV = Energie.DataAccess.Configurations.GetApplicationSetting("HoofdPV_" + Klant_Config).Trim();
            cmd = new SqlCommand("select code from SwitchBerichtTypes where ID=@ID", conn);
            cmd.Parameters.AddWithValue("@ID", cmbSwitchSoort.SelectedValue);
            string codeBerichtType = cmd.ExecuteScalar().ToString();
            //           if (codeBerichtType == "432E21") dit moet SWITCHPV zijn
            if (codeBerichtType == "SWITCHPV")
            {
                try
                {
                    cmd = new SqlCommand("Select EAN13_Code from PVs where ID=@ID", conn);
                    cmd.Parameters.AddWithValue("@ID", cmbPV.SelectedValue);
                    PV_EAN13_Code.Text = cmd.ExecuteScalar().ToString();
                }
                catch
                {
                    strBericht = strBericht + "PV, ";
                }
            }



            if (strBericht.Trim() != "")
            {
                AlertMessage("De waarde van volgende velden zijn onbekend : " + strBericht);
            }
            else
            {
                Boolean succes = true;

                strError = "7";
                //lblEANCode.Text = strError;

                //Schrijf gegevens
                if (Session["Aansluiting_ID"].ToString().Trim() == "")
                {
                    succes = AanmakenAansluiting();
                }
                //lblEANCode.Text = "Aansluiting";


                strError = "7.1";


                if (succes == true)
                {

                    strError = "8";

                    //Schrijfswitch
                    succes = AanmakenSwitch();

                    //lblEANCode.Text = "Switch";
                    if (succes == true)
                    {
                        strError = "9";
                        Energie.Car.SwitchClient switchClient = new Energie.Car.SwitchClient(Klant_Config);

                        //Energie.SwitchBericht.SwitchBericht switchBericht = new Energie.SwitchBericht.SwitchBericht(Klant_Config);
                        //int testModus = 0;

                        strError = "";
                        try
                        {
                            //switchBericht.createSwitchbericht(testModus);
                            DossierID = "";

                            strError = "10";
                            //lblEANCode.Text = strError;


                            //lblEANCode.Text = "Start";
                            if (switchClient.SwitchWebService(true, out DossierID, out strError, false, int.Parse(Session["Aansluiting_ID"].ToString())))
                            {
                         //       if (cmbSwitchSoort.SelectedValue == "110") 110 = 
                                if  (codeBerichtType == "NAMECH")
                                {
                                    cmd = new SqlCommand("Update EnergieDB.dbo.Aansluitingen set Kortenaam=@KorteNaam where id=@id", conn);
                                    cmd.Parameters.AddWithValue("@ID", Session["Aansluiting_ID"].ToString());
                                    cmd.Parameters.AddWithValue("@Kortenaam", strAansluiting_Kortenaam.Text);
                                    cmd.ExecuteNonQuery();
                                }

                                //lblEANCode.Text = strError;
                                //SchoonScherm();

                                int intLVRNormaal = 0;
                                int intLVRLaag = 0;
                                int intTLVNormaal = 0;
                                int intTLVlaag = 0;
                                int.TryParse(txtStandNormaal.Text, out intLVRNormaal);
                                int.TryParse(txtStandLaag.Text, out intLVRLaag);
                                int.TryParse(txtStandNormaalTerugLevering.Text, out intTLVNormaal);
                                int.TryParse(txtStandLaagTerugLevering.Text, out intTLVlaag);
                                if (intLVRNormaal != 0 || intLVRLaag != 0 || intTLVNormaal != 0 || intTLVlaag != 0)
                                {
                                    int nrRegisters = 0;
                                    int.TryParse(txtAantalRegisters.Text, out nrRegisters);

                                    int intMeterstand_ID = Save_MeterStand_Header(-1, false, DateTime.Now, txtEANCode.Text, dtmDatum, dtmDatum, Session["NetbeheerderEAN"].ToString(), "", strSwitchBerichtCode, -1, false, DossierID);



                                    if (intLVRNormaal != 0)
                                    {
                                        Save_MeterStand(intMeterstand_ID, -1, cmbHerkomst.Text, "N", intLVRNormaal.ToString(), lblMeetrichtingNormaal.Text, "0", intLVRNormaal.ToString(), cmbHerkomst.Text, "0", false);
                                    }
                                    if (intLVRLaag != 0)
                                    {
                                        Save_MeterStand(intMeterstand_ID, -1, cmbHerkomst.Text, "L", intLVRLaag.ToString(), lblMeetrichtingLaag.Text, "0", intLVRLaag.ToString(), cmbHerkomst.Text, "0", false);
                                    }
                                    if (intTLVNormaal != 0)
                                    {
                                        Save_MeterStand(intMeterstand_ID, -1, cmbHerkomst.Text, "N", intTLVNormaal.ToString(), lblMeetRichtingNormaalTerugLevering.Text, "0", intTLVNormaal.ToString(), cmbHerkomst.Text, "0", false);
                                    }
                                    if (intTLVlaag != 0)
                                    {
                                        Save_MeterStand(intMeterstand_ID, -1, cmbHerkomst.Text, "L", intTLVlaag.ToString(), lblMeetRichtingLaagTerugLevering.Text, "0", intTLVlaag.ToString(), cmbHerkomst.Text, "0", false);
                                    }

                                }
                                AlertMessage("Switch uitgevoerd");
                            }
                            else
                            {
                                //remove switch
                              //  AlertMessage(strError);
                                AlertMessage("Fout bij aanmaken switchbericht : " + strError);

                            }
                            //lblEANCode.Text = strError;
                            //MessageBox.Show("Switch staat klaar voor verzenden", "Switch", MessageBoxButtons.OK);
                        }
                         catch (Exception ex)
                        {
                            WriteLog("Fout bij aanmaken switch bericht : " + " - " + ex.Message, 10, 0);
                            AlertMessage("Fout bij aanmaken switchbericht : " + ex.Message);
                        }

                    }
                    //if (succes && Session["SlimmeMeter"] == "Ja")
                    //{
                    //    try
                    //    {
                    //        //Aanmaken P4 Request
                    //        if (DossierID == "") { DossierID = DateTime.Now.ToString("yyyyMMddhhmmsss"); }

                    //        if (cmbSwitchSoort.SelectedValue == "5")
                    //        {
                    //            string SQLStatement = "INSERT INTO [messages].[dbo].[P4_Request] " +
                    //                "([EAN18_Code] " +
                    //                ",[Datum] " +
                    //                ",[Sender] " +
                    //                ",[Receiver] " +
                    //                ",[Reden] " +
                    //                ",[Referentie] " +
                    //                ",[Ingediend] " +
                    //                ",[Aangemaakt_DT] " +
                    //                ",[Response]) " +
                    //                "VALUES " +
                    //                "(@EAN18_Code " +
                    //                ",@Datum " +
                    //                ",@Sender " +
                    //                ",@Receiver " +
                    //                ",@Reden " +
                    //                ",@Referentie " +
                    //                ",@Ingediend " +
                    //                ",GetDate() " +
                    //                ",@Response)";
                    //            cmd = new SqlCommand(SQLStatement, conn);
                    //            cmd.CommandType = CommandType.Text;
                    //            cmd.CommandTimeout = 12000;
                    //            cmd.Parameters.AddWithValue("@EAN18_Code", txtEANCode.Text);
                    //            cmd.Parameters.AddWithValue("@Datum", dtmDatum);
                    //            cmd.Parameters.AddWithValue("@Sender", Session["Leverancier"]);
                    //            cmd.Parameters.AddWithValue("@Receiver", Session["NetbeheerderEAN"]);
                    //            cmd.Parameters.AddWithValue("@Reden", "DAY");
                    //            cmd.Parameters.AddWithValue("@Referentie", DossierID);
                    //            cmd.Parameters.AddWithValue("@Ingediend", false);
                    //            cmd.Parameters.AddWithValue("@Response", false);
                    //            cmd.ExecuteNonQuery();


                    //            SQLStatement = "INSERT INTO [messages].[dbo].[P4_Request] " +
                    //            "([EAN18_Code] " +
                    //            ",[Datum] " +
                    //            ",[Sender] " +
                    //            ",[Receiver] " +
                    //            ",[Reden] " +
                    //            ",[Referentie] " +
                    //            ",[Ingediend] " +
                    //            ",[Aangemaakt_DT] " +
                    //            ",[Response]) " +
                    //            "VALUES " +
                    //            "(@EAN18_Code " +
                    //            ",@Datum " +
                    //            ",@Sender " +
                    //            ",@Receiver " +
                    //            ",@Reden " +
                    //            ",@Referentie " +
                    //            ",@Ingediend " +
                    //            ",GetDate() " +
                    //            ",@Response)";
                    //            cmd = new SqlCommand(SQLStatement, conn);
                    //            cmd.CommandType = CommandType.Text;
                    //            cmd.CommandTimeout = 12000;
                    //            cmd.Parameters.AddWithValue("@EAN18_Code", txtEANCode.Text);
                    //            cmd.Parameters.AddWithValue("@Datum", dtmDatum.AddDays(-1));
                    //            cmd.Parameters.AddWithValue("@Sender", Session["Leverancier"]);
                    //            cmd.Parameters.AddWithValue("@Receiver", Session["NetbeheerderEAN"]);
                    //            cmd.Parameters.AddWithValue("@Reden", "DAY");
                    //            cmd.Parameters.AddWithValue("@Referentie", DossierID + "1");
                    //            cmd.Parameters.AddWithValue("@Ingediend", false);
                    //            cmd.Parameters.AddWithValue("@Response", false);
                    //            cmd.ExecuteNonQuery();

                    //            SQLStatement = "INSERT INTO [messages].[dbo].[P4_Request] " +
                    //            "([EAN18_Code] " +
                    //            ",[Datum] " +
                    //            ",[Sender] " +
                    //            ",[Receiver] " +
                    //            ",[Reden] " +
                    //            ",[Referentie] " +
                    //            ",[Ingediend] " +
                    //            ",[Aangemaakt_DT] " +
                    //            ",[Response]) " +
                    //            "VALUES " +
                    //            "(@EAN18_Code " +
                    //            ",@Datum " +
                    //            ",@Sender " +
                    //            ",@Receiver " +
                    //            ",@Reden " +
                    //            ",@Referentie " +
                    //            ",@Ingediend " +
                    //            ",GetDate() " +
                    //            ",@Response)";
                    //            cmd = new SqlCommand(SQLStatement, conn);
                    //            cmd.CommandType = CommandType.Text;
                    //            cmd.CommandTimeout = 12000;
                    //            cmd.Parameters.AddWithValue("@EAN18_Code", txtEANCode.Text);
                    //            cmd.Parameters.AddWithValue("@Datum", DateTime.Now);
                    //            cmd.Parameters.AddWithValue("@Sender", Session["Leverancier"]);
                    //            cmd.Parameters.AddWithValue("@Receiver", Session["NetbeheerderEAN"]);
                    //            cmd.Parameters.AddWithValue("@Reden", "DAY");
                    //            cmd.Parameters.AddWithValue("@Referentie", DossierID + "2");
                    //            cmd.Parameters.AddWithValue("@Ingediend", false);
                    //            cmd.Parameters.AddWithValue("@Response", false);
                    //            cmd.ExecuteNonQuery();
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        WriteLog("Fout bij opslaan P4 Request : EAN " + txtEANCode.Text + " - " + ex.Message, 10, 0);

                    //    }
                    //}

                }
            }

            conn.Close();
   //         string strUrl = "Switch.aspx";

  //          Response.Redirect(strUrl);
            Session["Adres_ID"] = "";
            txtEANCode.Text = "";
            strRelatie_RelatieNaam.Text = "";
            //}
        }

    }


    protected Boolean AanmakenSwitch()
    {
        try
        {

            dtmDatum = DateTime.ParseExact(dtpDatumSwitch.Text.Trim(), "yyyy.MM.dd", provider);

            SqlCommand cmd;



            string SQLStatement = "delete from [EnergieDB].[dbo].[SwitchBerichten] " +
                "WHERE Aansluiting_ID=@AansluitingID AND BerichtIDSwitch=0";
            cmd = new SqlCommand(SQLStatement, conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 12000;
            cmd.Parameters.AddWithValue("@AansluitingID", Session["Aansluiting_ID"].ToString());
            cmd.ExecuteNonQuery();



            if (cmbSwitchSoort.Text == "8")
                         {
                cmd = new SqlCommand("Select EAN13_Code from PVs where ID=@ID", conn);
                cmd.Parameters.AddWithValue("@ID", cmbPV.SelectedValue);
                PV_EAN13_Code.Text = cmd.ExecuteScalar().ToString();


            }

            SQLStatement = "INSERT INTO [EnergieDB].[dbo].[SwitchBerichten] " +
                                    "([Aansluiting_ID] " +
                                    ",[Contract_Start_DT] " +
                                    ",[Relatie_ID] " +
                                    ",[Factuurmodel_ID] " +
                                    ",[Netbeheerder_ID] " +
                                    ",[SlimmeMeterAllocatie]" +
                                    ",[SwitchBerichtType_ID] " +
                                   
                                   ",[Te_Verzenden_DT] " +
                                   ",[Berichtstatus] " +
                                   ",[ProductType] " +
                                   ",[GebruikersNaam] " +
                                   ",[Aansluiting_Straat] " +
                                    ",[Aansluiting_Huisnummer] " +
                                    ",[Aansluiting_Toevoeging] " +
                                    ",[Aansluiting_Postcode] " +
                                      ",[Aansluiting_Woonplaats] " +
                                      ",[Aansluiting_Kortenaam] " +
                                    ",[Relatie_Straat] " +
                                    ",[Relatie_Huisnummer] " +
                                    ",[Relatie_Toevoeging] " +
                                    ",[Relatie_Postcode] " +
                                    ",[Relatie_Woonplaats] " +
                                    ",[PV] " +
                                    ",[KvKNr] ) " +

                                "VALUES " +
                                "(@Aansluiting_ID " +
                                ",@Contract_Start_DT " +
                                ",@Relatie_ID " +
                                ",@Factuurmodel_ID " +
                                ",@Netbeheerder_ID " +
                                ",@SwitchBerichtType_ID " +
                                ",@SlimmeMeterAllocatie " +
                                ",@Te_Verzenden_DT " +
                                ",@Berichtstatus " +
                                ",@ProductType " +
                                ",@GebruikersNaam " +
                                ",@Aansluiting_Straat " +
                                ",@Aansluiting_Huisnummer" +
                                ",@Aansluiting_Toevoeging " +
                                ",@Aansluiting_Postcode " +
                                ",@Aansluiting_Woonplaats " +
                                 ",@Aansluiting_Kortenaam " +
                                ",@Relatie_Straat " +
                                ",@Relatie_Huisnummer" +
                                ",@Relatie_Toevoeging " +
                                ",@Relatie_Postcode " +
                                ",@Relatie_Woonplaats " +
                                ",@PV " +
                                ",@KvKNr ) ";

            cmd = new SqlCommand(SQLStatement, conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 12000;
            //}

            cmd.Parameters.AddWithValue("@Aansluiting_ID", Session["Aansluiting_ID"].ToString());
            cmd.Parameters.AddWithValue("@Contract_Start_DT", dtmDatum);
            cmd.Parameters.AddWithValue("@Relatie_ID", strRelatie_ID.Text);


            cmd.Parameters.AddWithValue("@Factuurmodel_ID", 2);
            cmd.Parameters.AddWithValue("@Netbeheerder_ID", Session["Netbeheerder_ID"].ToString());
            //if (Session["Producttype"] != "G") { cmd.Parameters.AddWithValue("@Verblijfsfunctie", "01"); }
            cmd.Parameters.AddWithValue("@SwitchBerichtType_ID", cmbSwitchSoort.SelectedValue);
        
            cmd.Parameters.AddWithValue("@SlimmeMeterAllocatie", chkSlimmeMeterAllocatie.Checked);
            cmd.Parameters.AddWithValue("@Te_Verzenden_DT", DateTime.Now);
            cmd.Parameters.AddWithValue("@Berichtstatus", "Start");
            cmd.Parameters.AddWithValue("@ProductType", Session["Producttype"].ToString());
            cmd.Parameters.AddWithValue("@GebruikersNaam", Session["GebruikersNaam"]);

            cmd.Parameters.AddWithValue("@Aansluiting_Straat", strAansluiting_Straat.Text);
            cmd.Parameters.AddWithValue("@Aansluiting_Huisnummer", strAansluiting_Huisnummer.Text);
            cmd.Parameters.AddWithValue("@Aansluiting_Toevoeging", strAansluiting_Toevoeging.Text);
            cmd.Parameters.AddWithValue("@Aansluiting_Postcode", strAansluiting_Postcode.Text);
            cmd.Parameters.AddWithValue("@Aansluiting_Woonplaats", strAansluiting_Woonplaats.Text);
            cmd.Parameters.AddWithValue("@Aansluiting_Kortenaam", strAansluiting_Kortenaam.Text);
            cmd.Parameters.AddWithValue("@Relatie_Straat", strRelatie_Straat.Text);
            cmd.Parameters.AddWithValue("@Relatie_Huisnummer", strRelatie_Huisnummer.Text);
            cmd.Parameters.AddWithValue("@Relatie_Toevoeging", strRelatie_Toevoeging.Text);
            cmd.Parameters.AddWithValue("@Relatie_Postcode", strRelatie_Postcode.Text);
            cmd.Parameters.AddWithValue("@Relatie_Woonplaats", strRelatie_Woonplaats.Text);
            cmd.Parameters.AddWithValue("@PV", PV_EAN13_Code.Text);

            cmd.Parameters.AddWithValue("@KvKNr", strRelatie_KvK.Text);
            //       if (cmbSwitchSoort.Text == "8") { cmd.Parameters.AddWithValue("@PV", intPV); }

            cmd.ExecuteNonQuery();

            return true;
        }
        catch (Exception ex)
        {
            //lblEANCode.Text = ex.Message;
            AlertMessage("Fout bij aanmaken switch : " + ex.Message);

            return false;
        }
    }

    protected Boolean AanmakenAansluiting()
    {
        try
        {
            string SQLStatement = "INSERT INTO [EnergieDB].[dbo].[Aansluitingen] " +
                                  "([EAN18_Code] " +
                                  ",[Aansluitingtype_ID] " +
                                  ",[Relatie_ID]" +
                                  ",[Kortenaam])" +
                               
                                  "VALUES " +
                                  "(@EAN18_Code " +
                                  ",@Aansluitingtype_ID " +
                                  ",@Relatie_ID " +
                                  ",@Kortenaam) " +
                                  "SELECT SCOPE_IDENTITY()";
            SqlCommand cmd = new SqlCommand(SQLStatement, conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 12000;
            cmd.Parameters.AddWithValue("@EAN18_Code", txtEANCode.Text);
            if (Session["Producttype"].ToString().Trim() == "G")
            {
                cmd.Parameters.AddWithValue("@Aansluitingtype_ID", 5);
            }
            else
            {
                cmd.Parameters.AddWithValue("@Aansluitingtype_ID", 1);
            }
            cmd.Parameters.AddWithValue("@Kortenaam", strAansluiting_Kortenaam.Text.Trim());
            cmd.Parameters.AddWithValue("@Relatie_ID", strRelatie_ID.Text.Trim());

            Session["Aansluiting_ID"] = (cmd.ExecuteScalar()).ToString();

            return true;
        }
        catch (Exception ex)
        {
            AlertMessage("Fout bij aanmaken aansluiting : " + ex.Message);
            return false;
        }
    }





    protected void SchoonScherm()
    {
        txtEANCode.Text = "";

        //dtpDatumSwitch.Text = DateTime.Now.ToString("yyyy.MM.dd");
        Session["Aansluiting_ID"] = "";
        Session["Relatie_ID"] = "";
        strRelatie_Email.Text = "";
        strRelatie_RelatieNaam.Text = "";
        strRelatie_BedrijfsNaam.Text = "";
      
        strRelatie_Straat.Text = "";
        strRelatie_Huisnummer.Text = "";
        strRelatie_Toevoeging.Text = "";
        strRelatie_Woonplaats.Text = "";
        strRelatie_Postcode.Text = "";

        strAansluiting_Straat.Text = "";
        strAansluiting_Huisnummer.Text = "";
        strAansluiting_Postcode.Text = "";
        strAansluiting_Woonplaats.Text = "";
        strAansluiting_Kortenaam.Text = "";


    }

    protected void WriteLog(string description, int LevelID, int inbox_ID)
    {
        try
        {
            SqlCommand cmdLog = new SqlCommand();
            cmdLog.Connection = conn;
            string str_SQL = "insert into Messages.dbo.ApplicationLogs (TimeStmp, Description, SourceID, LevelID, Inbox_ID) values(@TimeStmp, @Description, " +
                " @SourceID, @LevelID, @Inbox_ID)";
            cmdLog.CommandText = str_SQL;
            cmdLog.Parameters.AddWithValue("@TimeStmp", DateTime.Now);
            cmdLog.Parameters.AddWithValue("@Description", description);
            cmdLog.Parameters.AddWithValue("@SourceID", 4);
            cmdLog.Parameters.AddWithValue("@LevelID", LevelID);
            cmdLog.Parameters.AddWithValue("@Inbox_ID", inbox_ID);
            cmdLog.ExecuteNonQuery();
            conn.Close();
        }
        catch (Exception ex)
        {
            EventLog eventlog = new EventLog("Application");
            eventlog.Source = "Energie App";
            eventlog.WriteEntry("WriteLog : " + ex.Message, EventLogEntryType.Error, 0);
        }
    }

    protected void cmbSwitchSoort_SelectedIndexChanged(object sender, EventArgs e)
    {
  //      if (cmbSwitchSoort.Text == "8")
        {

            //        lblPV.Visible = true;
        }
  //      else
        {
            //      lblPV.Visible = true;

        }
    }

    protected void cmbPortfolio_SelectedIndexChanged(object sender, EventArgs e)
    {

    }










    protected void ZoekRelatieEmail_Click(object sender, EventArgs e)
    {




        strRelatie_Straat.Text = "";
        strRelatie_ID.Text = "";
        strRelatie_Huisnummer.Text = "";
        strRelatie_Toevoeging.Text = "";
        strRelatie_Woonplaats.Text = "";
        strRelatie_Postcode.Text = "";

        strRelatie_RelatieNaam.Text = "";
        strRelatie_BedrijfsNaam.Text = "";
        

      





        using (SqlConnection con = new SqlConnection(ConnString))
        {


            string RelatieNaam = "";
            string strSQL = ("select " +

               
                "[Straat] " +
                ",[Woonplaats] " +
                ",[Postcode] " +
                ",[Huisnummer] " +
                ",[Toevoeging] " +
                
                ",[RelatieNaam]" +
                ",[BedrijfsNaam]" +
                 ",[ID]" +

                 ",[Email] " +
            
                ",[NieuweEmail] " +
                ",Kvk " +
                "FROM [EnergieDB].[dbo].[relaties]" +
                "where " +
                " Email=@Email");
            {




                SqlCommand cmd1 = new SqlCommand(strSQL, con);
                cmd1.Connection = con;
                con.Open();
                cmd1.CommandType = System.Data.CommandType.Text;
                cmd1.Parameters.AddWithValue("@Email", strRelatie_Email.Text);
                SqlDataReader rdr = cmd1.ExecuteReader();

                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        RelatieNaam = (string)rdr["RelatieNaam"];
                        strRelatie_RelatieNaam.Text = (string)rdr["RelatieNaam"].ToString();

                        strRelatie_Email.Text = (string)rdr["Email"].ToString();
                        strRelatie_ID.Text = (string)rdr["ID"].ToString();
                        strRelatie_BedrijfsNaam.Text = (string)rdr["BedrijfsNaam"].ToString();


                        strRelatie_Straat.Text = (string)rdr["Straat"].ToString();
                        strRelatie_Huisnummer.Text = (string)rdr["Huisnummer"].ToString();
                        strRelatie_Toevoeging.Text = (string)rdr["Toevoeging"].ToString();
                        strRelatie_Postcode.Text = rdr["Postcode"].ToString();
                        strRelatie_Woonplaats.Text = rdr["Woonplaats"].ToString();

                  

                        
                        strRelatie_KvK.Text = (string)rdr["KvK"];


                    }
                }
                rdr.Close();
                if (RelatieNaam == "")
                {
                    AlertMessage("Relatie naam van deze relatie is leeg.");
                }
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


    protected void ZoekPostcode_Click(object sender, EventArgs e)
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

 


    protected void txtAantalRegisters_TextChanged(object sender, EventArgs e)
    {
        int intAantalRegisters = 0;
        if (int.TryParse(txtAantalRegisters.Text, out intAantalRegisters))
        {
            if (intAantalRegisters > 0)
            {
                lblRegisterNormaal.Visible = true;
                lblMeetrichtingNormaal.Visible = true;
                txtStandNormaal.Visible = true;
            }
            else
            {
                lblRegisterNormaal.Visible = false;
                lblMeetrichtingNormaal.Visible = false;
                txtStandNormaal.Visible = false;
            }
            if (intAantalRegisters > 1)
            {
                lblRegisterLaag.Visible = true;
                lblMeetrichtingLaag.Visible = true;
                txtStandLaag.Visible = true;
            }
            else
            {
                lblRegisterLaag.Visible = false;
                lblMeetrichtingLaag.Visible = false;
                txtStandLaag.Visible = false;
            }
            if (intAantalRegisters > 2)
            {
                lblRegisterNormaalTerugLevering.Visible = true;
                lblMeetRichtingNormaalTerugLevering.Visible = true;
                txtStandNormaalTerugLevering.Visible = true;
            }
            else
            {
                lblRegisterNormaalTerugLevering.Visible = false;
                lblMeetRichtingNormaalTerugLevering.Visible = false;
                txtStandNormaalTerugLevering.Visible = false;
            }
            if (intAantalRegisters > 3)
            {
                lblRegisterLaagTerugLevering.Visible = true;
                lblMeetRichtingLaagTerugLevering.Visible = true;
                txtStandLaagTerugLevering.Visible = true;
            }
            else
            {
                lblRegisterLaagTerugLevering.Visible = false;
                lblMeetRichtingLaagTerugLevering.Visible = false;
                txtStandLaagTerugLevering.Visible = false;
            }
        }
    }

    public int Save_MeterStand_Header(int BerichtId, Boolean verstuurd, DateTime verstuurd_DT, string eanCode, DateTime begin_D,
            DateTime eind_D, String netbeheerder, string dossierID, string redenMutatie, int pMeterStand_ID, Boolean fout, string Referentie)
    {
        SqlConnection cnPubs = new SqlConnection(KC.ConnString);
        string SQLstatement;
        int meterStand_ID = -1;

        try
        {
            cnPubs.Open();
            if (pMeterStand_ID == -1)
            {
                SQLstatement =
                        "INSERT INTO [Car].[dbo].[MeterStanden_Header] " +
                        "(BerichtId " +
                        ",[Verstuurd] ";
                if (verstuurd_DT > DateTime.MinValue) { SQLstatement = SQLstatement + ",[VerstuurdDT]"; }
                SQLstatement = SQLstatement + ",[EanCode] " +
                        ",[BeginD] " +
                        ",[EindD] " +
                        ",[Netbeheerder] " +
                        ",[DossierID] " +
                        ",[RedenMutatie] " +
                        ",[Fout] " +
                        ",Referentie " +
                        ",Compleet) " +
                        "VALUES " +
                        "(@BerichtId " +
                        ",@Verstuurd ";
                if (verstuurd_DT > DateTime.MinValue) { SQLstatement = SQLstatement + ",@VerstuurdDT "; }
                SQLstatement = SQLstatement + ",@EanCode " +
                        ",@BeginD " +
                        ",@EindD " +
                        ",@Netbeheerder " +
                        ",@DossierID " +
                        ",@RedenMutatie " +
                        ",@Fout " +
                        ",@Referentie " +
                        ",@Compleet); SELECT @MeterstandId = SCOPE_IDENTITY();";
            }
            else
            {
                SQLstatement = " UPDATE [Car].[dbo].[MeterStanden_Header] " +
                    "SET [BerichtId] = @BerichtId " +
                    ",[EanCode] = @EanCode " +
                    ",[Verstuurd] = @Verstuurd " +
                    ",[VerstuurdDT] = @Verstuurd_DT " +
                    ",[Begin_D] = @Begin_D " +
                    ",[Eind_D] = @Eind_D " +
                    ",[Netbeheerder] = @Netbeheerder " +
                    ",[DossierID] = @DossierID " +
                    ",[RedenMutatie] = @RedenMutatie " +
                    ",[Fout] = @Fout " +
                    ",Referentie = @Referentie " +
                    ",Compleet = @Compleet " +
                    "WHERE MeterstandId=@MeterstandId";
            }
            SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
            cmdSaveInbox.Parameters.Add(new SqlParameter("@BerichtId", SqlDbType.Int));
            cmdSaveInbox.Parameters["@BerichtId"].Value = BerichtId;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Verstuurd", SqlDbType.Bit));
            cmdSaveInbox.Parameters["@Verstuurd"].Value = verstuurd;
            if (verstuurd_DT > DateTime.MinValue)
            {
                cmdSaveInbox.Parameters.Add(new SqlParameter("@VerstuurdDT", SqlDbType.DateTime));
                cmdSaveInbox.Parameters["@VerstuurdDT"].Value = verstuurd_DT;
            }
            cmdSaveInbox.Parameters.Add(new SqlParameter("@EanCode", SqlDbType.BigInt));
            cmdSaveInbox.Parameters["@EanCode"].Value = eanCode;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@BeginD", SqlDbType.Date));
            cmdSaveInbox.Parameters["@BeginD"].Value = begin_D;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@EindD", SqlDbType.Date));
            cmdSaveInbox.Parameters["@EindD"].Value = eind_D;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Netbeheerder", SqlDbType.BigInt));
            cmdSaveInbox.Parameters["@Netbeheerder"].Value = netbeheerder;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@DossierID", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@DossierID"].Value = dossierID;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@RedenMutatie", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@RedenMutatie"].Value = redenMutatie;
            //cmdSaveInbox.Parameters.Add(new SqlParameter("@DossierID", SqlDbType.VarChar));
            //cmdSaveInbox.Parameters["@dossierID"].Value = dossierID;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Fout", SqlDbType.Bit));
            cmdSaveInbox.Parameters["@Fout"].Value = fout;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Referentie", SqlDbType.VarChar));
            cmdSaveInbox.Parameters["@Referentie"].Value = Referentie;
            cmdSaveInbox.Parameters.Add(new SqlParameter("@Compleet", SqlDbType.Bit));
            cmdSaveInbox.Parameters["@Compleet"].Value = false;
            if (pMeterStand_ID != -1)
            {
                cmdSaveInbox.Parameters.Add(new SqlParameter("@MeterstandId", SqlDbType.Int));
                cmdSaveInbox.Parameters["@MeterstandId"].Value = pMeterStand_ID;
                //cmdSaveInbox.Parameters["@Meterstand_ID"].Direction = ParameterDirection.Output;
            }
            else
            {
                cmdSaveInbox.Parameters.Add(new SqlParameter("@MeterstandId", SqlDbType.Int));
                cmdSaveInbox.Parameters["@MeterstandId"].Direction = ParameterDirection.Output;
            }
            try
            {
                cmdSaveInbox.ExecuteNonQuery();
                if (pMeterStand_ID == -1) { meterStand_ID = (int)cmdSaveInbox.Parameters["@MeterstandId"].Value; }
                else { meterStand_ID = pMeterStand_ID; }
                //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
            }
            catch (Exception ex)
            {
                carShared.SchrijfLog("Fout bij wegeschrijven MeterStand_Header :  - " + eanCode + " - " + ex.ToString(), 10, 0, KC.App_ID);
                //WriteLog("Fout bij wegeschrijven MeterStand_Header :  - " + eanCode + " - " + ex.ToString(), 10, BerichtId);
                //MessageBox.Show("Er is iets fout gegaan met het bewaren van de MeterStand_Header, we adviseren U contact op te nemen met IT");
                //MessageBox.Show(ex.ToString());
            }

            cnPubs.Close();
        }
        catch (Exception ex)
        {
            carShared.SchrijfLog("Fout bij wegeschrijven MeterStand_Header :  - " + eanCode + " - " + ex.ToString(), 10, 0, KC.App_ID);
            meterStand_ID = -1;
        }
        return meterStand_ID;

    }

    public int Save_MeterStand(int meterStand_ID, int BerichtId, string herkomst, string tarifType, string stand, string direction, string volume,
         string beginStand, string herkomstBeginStand, string calorificCorrectedVolume, Boolean update)
    {
        SqlConnection cnPubs = new SqlConnection(KC.ConnString);
        string SQLstatement;
        int Meterstand_Id = -1;

        cnPubs.Open();
        if (update == false)
        {
            SQLstatement =
                    "INSERT INTO [Car].[dbo].[MeterStand] " +
                    "([MeterstandId] " +
                    ",[BerichtId] " +
                    ",[Direction] " +
                    ",[TarifType] " +
                    ",[Herkomst] " +
                    ",[Stand] " +
                    ",[Volume] " +
                    ",[BeginStand] " +
                    ",[HerkomstBeginStand] " +
                    ",[CalorificCorrectedVolume]) " +
                    "VALUES " +
                    "(@MeterstandId " +
                    ",@BerichtId " +
                    ",@Direction " +
                    ",@TarifType " +
                    ",@Herkomst " +
                    ",@Stand " +
                    ",@Volume " +
                    ",@BeginStand " +
                    ",@HerkomstBeginStand " +
                    ",@CalorificCorrectedVolume)";
        }
        else
        {
            SQLstatement = "UPDATE [Car].[dbo].[MeterStand] " +
                "SET [BerichtId] = @BerichtId " +
                ",[Herkomst] = @Herkomst " +
                ",[Stand] = @Stand " +
                ",[Volume] = @Volume " +
                ",[BeginStand] = @BeginStand " +
                ",[HerkomstBeginStand] = @HerkomstBeginStand " +
                ",[CalorificCorrectedVolume] = @CalorificCorrectedVolume " +
                "WHERE [MeterStand_ID] = @MeterStand_ID and [Direction] = @Direction and [TarifType] = @TarifType";
        }
        SqlCommand cmdSaveInbox = new SqlCommand(SQLstatement, cnPubs);
        cmdSaveInbox.Parameters.Add(new SqlParameter("@MeterstandId", SqlDbType.Int));
        cmdSaveInbox.Parameters["@MeterstandId"].Value = meterStand_ID;
        cmdSaveInbox.Parameters.Add(new SqlParameter("@BerichtId", SqlDbType.Int));
        cmdSaveInbox.Parameters["@BerichtId"].Value = BerichtId;
        cmdSaveInbox.Parameters.Add(new SqlParameter("@Direction", SqlDbType.Char));
        cmdSaveInbox.Parameters["@Direction"].Value = direction;
        cmdSaveInbox.Parameters.Add(new SqlParameter("@TarifType", SqlDbType.Char));
        cmdSaveInbox.Parameters["@TarifType"].Value = tarifType;
        cmdSaveInbox.Parameters.Add(new SqlParameter("@Herkomst", SqlDbType.VarChar));
        cmdSaveInbox.Parameters["@Herkomst"].Value = herkomst;
        cmdSaveInbox.Parameters.Add(new SqlParameter("@Stand", SqlDbType.Decimal));
        cmdSaveInbox.Parameters["@Stand"].Value = stand;
        cmdSaveInbox.Parameters.Add(new SqlParameter("@Volume", SqlDbType.Decimal));
        cmdSaveInbox.Parameters["@Volume"].Value = volume;
        cmdSaveInbox.Parameters.Add(new SqlParameter("@BeginStand", SqlDbType.Decimal));
        cmdSaveInbox.Parameters["@BeginStand"].Value = beginStand;
        cmdSaveInbox.Parameters.Add(new SqlParameter("@HerkomstBeginStand", SqlDbType.VarChar));
        cmdSaveInbox.Parameters["@HerkomstBeginStand"].Value = herkomstBeginStand;
        cmdSaveInbox.Parameters.Add(new SqlParameter("@CalorificCorrectedVolume", SqlDbType.Decimal));
        cmdSaveInbox.Parameters["@CalorificCorrectedVolume"].Value = calorificCorrectedVolume;



        try
        {
            cmdSaveInbox.ExecuteNonQuery();
            //inboxID = (int)cmdSaveInbox.Parameters["@inboxID"].Value;
            //Console.WriteLine("Switch-Bericht succesvol opgeslagen en verstuurd");
        }
        catch (Exception ex)
        {
            carShared.SchrijfLog("Fout bij wegeschrijven MeterStand (meterstand_ID):  - " + meterStand_ID + " - " + ex.ToString(), 10, 0, KC.App_ID);
            //MessageBox.Show("Er is iets fout gegaan met het bewaren van de MeterStand, we adviseren U contact op te nemen met IT");
            //MessageBox.Show(ex.ToString());
        }

        cnPubs.Close();
        return Meterstand_Id;

    }
}