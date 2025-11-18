using System;
using System.Collections.Generic;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using Energie.Car;

public partial class MasterdataInfo : System.Web.UI.Page
{
    protected String ConnString = "";
    protected SqlConnection conn;
    protected DataSet dsGet;
    protected CultureInfo provider = CultureInfo.InvariantCulture;
    protected SqlCommand cmd;
    protected SqlDataAdapter daSwitch;
    protected DataSet dsSwitch;
    protected DataRow drSwitch;
    protected int Klant_ID_ELK = -1;
    protected int Klant_ID_GAS = -1;

    protected int Klant_ID = -1;
    protected int intFacturatieRelatieID = -1;
    protected int RelatieId = -1;
    //protected int Account_Relatie_ID = -1;
    protected string Klant_Config = "";
    protected string DossierID = "";
    protected DateTime dtmDatum;

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
         
        }
    }
    protected void txtEANCode_TextChanged(object sender, EventArgs e)
    {
        

        conn = new SqlConnection(ConnString);
        conn.Open();
        

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

                if (OldEan != txtEANCode.Text.Trim())
                {

                    Energie.Car.MasterdataClient masterDataClient = new Energie.Car.MasterdataClient(KC.KlantConfig);
                    nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope meteringpoint = masterDataClient.RequestGetMeteringPointMP(txtEANCode.Text, false, "", true, false);

                    if (meteringpoint != null)
                    {
                        nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope_PC portaal_Response = meteringpoint.Portaal_Content;

                        if (portaal_Response.Item.GetType() != typeof(nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope_PC_PR))
                        {
                            nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope_PC_PMP portaal_MeteringPointResponse = (nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope_PC_PMP)portaal_Response.Item;

                            //cmd = new SqlCommand("Select ID from Netbeheerders where EAN13_Code=@EAN13_Code", conn);
                            //cmd.Parameters.AddWithValue("@EAN13_Code", portaal_MeteringPointResponse.GridOperator_Company.ID.ToString());
                            //int intNetbeheerderID = -1;
                            //Object objNetbeheerderID = cmd.ExecuteScalar();
                            Session["NetbeheerderEAN"] = portaal_MeteringPointResponse.GridOperator_Company.ID.ToString();
                            lblNetbeheerder.Text = portaal_MeteringPointResponse.GridOperator_Company.ID.ToString();
                            lblNetgebied.Text = portaal_MeteringPointResponse.GridArea;

                            if (portaal_MeteringPointResponse.MPCommercialCharacteristics.BalanceResponsibleParty_Company != null)
                            {
                                lblPV.Text = portaal_MeteringPointResponse.MPCommercialCharacteristics.BalanceResponsibleParty_Company.ID.ToString();
                            }

                            if (portaal_MeteringPointResponse.MPCommercialCharacteristics.BalanceSupplier_Company != null)
                            {
                                lblLV.Text = portaal_MeteringPointResponse.MPCommercialCharacteristics.BalanceSupplier_Company.ID.ToString();
                            }

                            if (portaal_MeteringPointResponse.EDSN_AddressExtended != null)
                            {
                                txtStraat.Text = portaal_MeteringPointResponse.EDSN_AddressExtended[0].StreetName;
                                txtHuisnummer.Text = portaal_MeteringPointResponse.EDSN_AddressExtended[0].BuildingNr;
                                txtToevoeging.Text = portaal_MeteringPointResponse.EDSN_AddressExtended[0].ExBuildingNr;
                                txtPostcode.Text = portaal_MeteringPointResponse.EDSN_AddressExtended[0].ZIPCode;
                                txtWoonplaats.Text = portaal_MeteringPointResponse.EDSN_AddressExtended[0].CityName;
                            }

                            if (portaal_MeteringPointResponse.MarketSegment != null)
                            {
                                lblVerbruiksegment.Text = portaal_MeteringPointResponse.MarketSegment;
                            }

                            if (portaal_MeteringPointResponse.MPPhysicalCharacteristics != null)
                            {
                                lblLeveringsrichting.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.EnergyFlowDirection;
                                lblAdminStatus.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.EnergyDeliveryStatus;
                                lblBemetering.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.MeteringMethod;
                                lblLeveringstatus.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.PhysicalStatus;
                                if (portaal_MeteringPointResponse.MPPhysicalCharacteristics.EAEnergyConsumptionNettedPeak != null)
                                {
                                    lblSJVNormaal.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.EAEnergyConsumptionNettedPeak;
                                }
                                if (portaal_MeteringPointResponse.MPPhysicalCharacteristics.EAEnergyConsumptionNettedOffPeak != null)
                                {
                                    lblSJVLaag.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.EAEnergyConsumptionNettedOffPeak;
                                }
                                if (portaal_MeteringPointResponse.MPPhysicalCharacteristics.EAEnergyProductionNettedPeak != null)
                                {
                                    lblSJINormaal.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.EAEnergyProductionNettedPeak;
                                }
                                if (portaal_MeteringPointResponse.MPPhysicalCharacteristics.EAEnergyProductionNettedOffPeak != null)
                                {
                                    lblSJILaag.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.EAEnergyProductionNettedOffPeak;
                                }
                                lblAllocatieMethode.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.AllocationMethod;
                                lblProfiel.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.ProfileCategory;

                                if (portaal_MeteringPointResponse.MPPhysicalCharacteristics.CapTarCode != null)
                                {
                                    lblCapaciteitscode.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.CapTarCode;
                                }
                            }
                            //lblEANCode.Text = intStap.ToString();
                            //txtAantalRegisters.Text = portaal_MeteringPointResponse.Portaal_EnergyMeter.NrOfRegisters;
                            //txtMeternummer.Text = portaal_MeteringPointResponse.Portaal_EnergyMeter.ID;
                            if (portaal_MeteringPointResponse.ProductType == "GAS")
                            {
                                Session["Producttype"] = "G";
                            }
                            else
                            {
                                Session["Producttype"] = "E";
                            }
                            //lblEANCode.Text = intStap.ToString();
                            lblProduct.Text = Session["Producttype"].ToString();
                        }
                        else
                        {
                            nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope_PC_PR portaal_MeteringPointResponse = (nl.Energie.EDSN.GetMeteringPointMP.GetMeteringPointResponseEnvelope_PC_PR)portaal_Response.Item;
                            AllertMessage(portaal_MeteringPointResponse.Rejection[0].RejectionText);
                        }
                    }

                }
            }
            DataBind();
        }
        catch (Exception ex)
        {
            AllertMessage("Error bij opvragen EAN : " + " - " + ex.Message);
        }
        conn.Close();
        
    }

    
    protected void btnIndienen_Click(object sender, EventArgs e)
    {
        conn = new SqlConnection(ConnString);
        conn.Open();
        

        conn.Close();
    }


    protected void AllertMessage(string strMessage)
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

    

    protected void SchoonScherm()
    {
        txtEANCode.Text = "";
        //txtNaam.Text = "";
        //dtpDatumSwitch.Text = DateTime.Now.ToString("yyyy.MM.dd");
        Session["Aansluiting_ID"] = "";
        Session["Adres_ID"] = "";
        txtStraat.Text = "";
        txtHuisnummer.Text = "";
        //txtPostcode.Text = "";
        //txtPlaats.Text = "";
        //txtStandHoog.Text = "";
        //txtStandLaag.Text = "";
        txtStraat.Enabled = true;
        txtHuisnummer.Enabled = true;
        //txtToevoeging.Enabled = true;
        //txtPostcode.Enabled = true;
        //txtPlaats.Enabled = true;
        //btnZoekAdres.Enabled = true;
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




    protected void btnZoek_Click(object sender, EventArgs e)
    {
        string strError = "";
        Energie.Car.MasterdataClient masterDataClient = new Energie.Car.MasterdataClient(KC.KlantConfig);
        nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope meteringpoint = masterDataClient.SearchMeteringPointMP(false, txtEANCode.Text, txtPostcode.Text, txtHuisnummer.Text, txtToevoeging.Text, "", "", "", "", "", 0, out strError);

        if (meteringpoint != null)
        {
            nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope_PC portaal_Response = meteringpoint.Portaal_Content;

            if (portaal_Response != null)
            {
                if (portaal_Response.Item.GetType() != typeof(nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope_PC_PR))
                {
                    nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope_PC_Result portaal_ResponseResult = (nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope_PC_Result)portaal_Response.Item;
                    if (portaal_ResponseResult.Portaal_MeteringPoint != null)
                    {
                        nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope_PC_Result_PMP portaal_MeteringPointResponse = (nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope_PC_Result_PMP)portaal_ResponseResult.Portaal_MeteringPoint[0];
                        //cmd = new SqlCommand("Select ID from Netbeheerders where EAN13_Code=@EAN13_Code", conn);
                        //cmd.Parameters.AddWithValue("@EAN13_Code", portaal_MeteringPointResponse.GridOperator_Company.ID.ToString());
                        //int intNetbeheerderID = -1;
                        //Object objNetbeheerderID = cmd.ExecuteScalar();
                        //Session["NetbeheerderEAN"] = portaal_MeteringPointResponse.GridOperator_Company.ID.ToString();
                        lblNetbeheerder.Text = portaal_MeteringPointResponse.GridOperator_Company.ID.ToString();
                        lblNetgebied.Text = portaal_MeteringPointResponse.GridArea;

                        //if (portaal_MeteringPointResponse.MPCommercialCharacteristics. != null)
                        //{
                        //    lblPV.Text = portaal_MeteringPointResponse.MPCommercialCharacteristics.BalanceResponsibleParty_Company.ID.ToString();
                        //}

                        //if (portaal_MeteringPointResponse.MPCommercialCharacteristics.BalanceSupplier_Company != null)
                        //{
                        //    lblLV.Text = portaal_MeteringPointResponse.MPCommercialCharacteristics.BalanceSupplier_Company.ID.ToString();
                        //}
                        txtEANCode.Text = portaal_MeteringPointResponse.EANID;

                        if (portaal_MeteringPointResponse.EDSN_AddressSearch != null)
                        {
                            txtStraat.Text = portaal_MeteringPointResponse.EDSN_AddressSearch.StreetName;
                            txtHuisnummer.Text = portaal_MeteringPointResponse.EDSN_AddressSearch.BuildingNr;
                            txtToevoeging.Text = portaal_MeteringPointResponse.EDSN_AddressSearch.ExBuildingNr;
                            txtPostcode.Text = portaal_MeteringPointResponse.EDSN_AddressSearch.ZIPCode;
                            txtWoonplaats.Text = portaal_MeteringPointResponse.EDSN_AddressSearch.CityName;
                        }

                        if (portaal_MeteringPointResponse.MarketSegment != null)
                        {
                            lblVerbruiksegment.Text = portaal_MeteringPointResponse.MarketSegment;
                        }

                        if (portaal_MeteringPointResponse.MPPhysicalCharacteristics != null)
                        {
                            lblLeveringsrichting.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.EnergyFlowDirection;
                            //lblAdminStatus.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.EnergyDeliveryStatus;
                            lblBemetering.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.MeteringMethod;
                            //lblLeveringstatus.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.PhysicalStatus;
                            //if (portaal_MeteringPointResponse.MPPhysicalCharacteristics.EACPeak != null)
                            //{
                            //    lblSJVNormaal.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.EACPeak;
                            //}
                            //if (portaal_MeteringPointResponse.MPPhysicalCharacteristics.EACOffPeak != null)
                            //{
                            //    lblSJVLaag.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.EACOffPeak;
                            //}
                            lblAllocatieMethode.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.AllocationMethod;
                            lblProfiel.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.ProfileCategory;

                            //if (portaal_MeteringPointResponse.MPPhysicalCharacteristics.CapTarCode != null)
                            //{
                            //    lblCapaciteitscode.Text = portaal_MeteringPointResponse.MPPhysicalCharacteristics.CapTarCode;
                            //}
                        }
                        //lblEANCode.Text = intStap.ToString();
                        //txtAantalRegisters.Text = portaal_MeteringPointResponse.Portaal_EnergyMeter.NrOfRegisters;
                        //txtMeternummer.Text = portaal_MeteringPointResponse.Portaal_EnergyMeter.ID;
                        if (portaal_MeteringPointResponse.ProductType == "GAS")
                        {
                            Session["Producttype"] = "G";
                        }
                        else
                        {
                            Session["Producttype"] = "E";
                        }
                        //lblEANCode.Text = intStap.ToString();
                        lblProduct.Text = Session["Producttype"].ToString();
                    }
                }
                else
                {
                    nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope_PC_PR portaal_MeteringPointResponse = (nl.Energie.EDSN.SearchMeteringPointsMP.SearchMeteringPointsResponseEnvelope_PC_PR)portaal_Response.Item;
                    AllertMessage(portaal_MeteringPointResponse.Rejection[0].RejectionText);
                }
            }
            else
            {
                AllertMessage("Fout bij opvragen/zoeken aansluiting");
            }
        }
    }
}