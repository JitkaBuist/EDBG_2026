using Castle.Components.DictionaryAdapter.Xml;

using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class MasterPage : System.Web.UI.MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {

        if (Session["KlantConfig"] == null || Session["KlantConfig"].ToString() == "")
        {
            string strUrl = "Login.aspx";
            Response.Redirect(strUrl);
        }
        
        
        
        if (Session["KlantConfig"].ToString().ToUpper() == "LV_BASCUUL")
        {

            litLogo.Text = "<img src=\"images/LV_BASCUUL.png\" />";

            liEProgramma.Visible = true;
            liBRP.Visible = true;
            liLeverancier.Visible = true;

            liRegisterBRP.Visible = false;
            liRegisterLeverancierACM.Visible = false;
            liRegisterLeverancierEDSN.Visible = false;
            liRegisterMRP.Visible = false;
            liRegisterNetbeheerder.Visible = false;
            liRegisterDatavrager.Visible = false;
            liRegisterNetbeheerder.Visible = false;
            liRegisterGDSACM.Visible = false;
            liRegisterODA.Visible  = false;
            liRegisterCSP.Visible = false;
            liRegisters.Visible = false;

            liRapportageAansluitingenRelaties.Visible = true;
            liRapportageAansluitingenRelaties.Visible = true;
            liRapportageSwitches.Visible = true;
            liRapportageLosses.Visible = true;
            liRapportageGains.Visible = true;
            liRapportageVerbruiken.Visible = true;
            liTennetRapportage.Visible = true;


            liBerichten.Visible = true;
            liPVOnbalans.Visible = true;
           liPVOnbalansPortfolio.Visible = true;
            liChangeEnrollment.Visible = false;
            lblTennetMenu.Text = "Berichten";
            
            liLeverancier.Visible = true;
            liFacturatie.Visible = true;
            liRelatie.Visible = true;
            liGebruikersnaam.Visible = true;
            liKlantconfig.Visible = true;
            liPortfolio.Visible = false;
            liLoginMenu.Visible = true;
            liKlantgegevens.Visible = true;
            liWebservices.Visible = false;

        }


        if (Session["KlantConfig"].ToString().ToUpper() == "REGISTERS")
        {

            litLogo.Text = "<img src=\"images/normo.png\" />";
            liBRP.Visible = true;
            liLeverancier.Visible = true;
            liBerichten.Visible = true;

            liChangeEnrollment.Visible = false;

            liRegisterBRP.Visible = true;
            liRegisterLeverancierEDSN.Visible = true;
            liRegisterLeverancierACM.Visible = true;
            liRegisterMRP.Visible = true;
            liRegisterNetbeheerder.Visible = true;
            liRegisterDatavrager.Visible = true;
            liRegisterNetbeheerder.Visible = true;
            liRegisterGDSACM.Visible = true;
            liRegisterGDSEDSN.Visible = true;
            liRegisterODA.Visible = true;
            liRegisterCSP.Visible = true;
            liRegisters.Visible = true;


            liRapportageGridgain.Visible = false;
            liRapportageExcel.Visible = true;
            liRapportageAansluitingenRelaties.Visible = true;
            liRapportageAansluitingenRelaties.Visible = true;
            liRapportageSwitches.Visible = true;
            liRapportageLosses.Visible = true;
            liRapportageGains.Visible = true;
            liRapportageVerbruiken.Visible = true;

            liPortfolio.Visible = false;
            liWebservices.Visible = false;
            liVerwerkBericht.Visible = false;
            liImportEtpa.Visible = false;

            liFacturatie.Visible = true;
            liRelatie.Visible = true;
            liGebruikersnaam.Visible = true;
            liKlantconfig.Visible = true;
            
          

            liLoginMenu.Visible = true;
            liKlantgegevens.Visible = true;
           

        }
        if (Session["KlantConfig"].ToString().ToUpper() == "GRID_REPORTS")
        {

            litLogo.Text = "<img src=\"images/GRIDLV_LOGO.jpg\" />";
            liBRP.Visible = false;
            liLeverancier.Visible = true;

            liRegisterBRP.Visible = false;
            liRegisterLeverancierEDSN.Visible = false;
            liRegisterLeverancierACM.Visible = false;
            liRegisterMRP.Visible = false;
            liRegisterNetbeheerder.Visible = false;
            liRegisterDatavrager.Visible = false;
            liRegisterNetbeheerder.Visible = false;
            liRegisterGDSACM.Visible = false;
            liRegisterODA.Visible = false;
            liRegisterCSP.Visible = false;
            liRegisters.Visible = false;

            liEProgramma.Visible = false;
         
            liPVOnbalans.Visible = false;
            liPVOnbalansPortfolio.Visible = false;
            liChangeEnrollment.Visible = false;
            liRapportageGridgain.Visible = true;
            liBerichten.Visible = false;
            lblTennetMenu.Text = "Berichten";
            
            liTennetRapportage.Visible = false;
            liRapportageAansluitingenRelaties.Visible = true;
            liRapportageAansluitingenRelaties.Visible = true;
            liRapportageSwitches.Visible = true;
            liRapportageLosses.Visible = true;
            liRapportageGains.Visible = true;
            liRapportageVerbruiken.Visible = true;

            liFacturatie.Visible = false;
            liRelatie.Visible = false;
            liGebruikersnaam.Visible = false;
            liKlantconfig.Visible = false;
            liPortfolio.Visible = false;
            liLoginMenu.Visible = true;
            liKlantgegevens.Visible = false;
            liWebservices.Visible = false;

        }
        if (Session["KlantConfig"].ToString().ToUpper() == "PV_BASCUUL")
        {

            litLogo.Text = "<img src=\"images/PV_BASCUUL.png\" />";
            liBRP.Visible = true;
            liLeverancier.Visible = true;

            liEProgramma.Visible = true;
            liBerichten.Visible = true;

            liRegisterBRP.Visible = false;
            liRegisterLeverancierEDSN.Visible = false;
            liRegisterLeverancierACM.Visible = false;
            liRegisterMRP.Visible = false;
            liRegisterNetbeheerder.Visible = false;
            liRegisterDatavrager.Visible = false;
            liRegisterNetbeheerder.Visible = false;
            liRegisterGDSACM.Visible = false;
            liRegisterODA.Visible = false;
            liRegisterCSP.Visible = false;
            liRegisters.Visible = false;

            liPVOnbalans.Visible = true;
            liPVOnbalansPortfolio.Visible = true;
            liChangeEnrollment.Visible = false;
            lblTennetMenu.Text = "Berichten";

            liTennetRapportage.Visible = true;
            liRapportageAansluitingenRelaties.Visible = false;
            liRapportageSwitches.Visible = false;
            liRapportageLosses.Visible = false;
            liRapportageGains.Visible = false;
            liRapportageVerbruiken.Visible = false;

            liBerichten.Visible = true;
            liLeverancier.Visible = true;
            liFacturatie.Visible = true;
            liRelatie.Visible = true;
            liGebruikersnaam.Visible = true;
            liKlantconfig.Visible = true;
            liPortfolio.Visible = false;
            liLoginMenu.Visible = true;
            liKlantgegevens.Visible = true;
            liWebservices.Visible = false;
        }
       

        if (Session["KlantConfig"].ToString().ToUpper() == "LV_GRID")
        {
            litLogo.Text = "<img src=\"images/GRIDLV_LOGO.jpg\" />";
            liBRP.Visible = false;
            liLeverancier.Visible = true;

            liRegisterBRP.Visible = false;
            liRegisterLeverancierEDSN.Visible = false;
            liRegisterLeverancierACM.Visible = false;
            liRegisterMRP.Visible = false;
            liRegisterNetbeheerder.Visible = false;
            liRegisterDatavrager.Visible = false;
            liRegisterNetbeheerder.Visible = false;
            liRegisterGDSACM.Visible = false;
            liRegisterODA.Visible = false;
            liRegisterCSP.Visible = false;
            liRegisters.Visible = false;



            liBerichten.Visible = false;
            liTennetRapportage.Visible = false;
            liLeverancier.Visible = true;
            liFacturatie.Visible = true;
            liRelatie.Visible = true;
            liWebservices.Visible = true;
            liGebruikersnaam.Visible = true;
            liKlantconfig.Visible = true;
            liPortfolio.Visible = false;
            liLoginMenu.Visible = true;
            liKlantgegevens.Visible = true;

            liRapportageGridgain.Visible = true;
            liRapportageAansluitingenRelaties.Visible = true;
            liRapportageSwitches.Visible = true;
            liRapportageLosses.Visible = true;
            liRapportageGains.Visible = true;
            liRapportageVerbruiken.Visible = true;
            liPVOnbalans.Visible = false;
            liPVOnbalansPortfolio.Visible = false;
            liChangeEnrollment.Visible = false;
        }

        if (Session["KlantConfig"].ToString().ToUpper() == "EDBG")
        {
           
            litLogo.Text = "<img src=\"images/edbglogo.jpg\" />";

            liBRP.Visible = true;
            liLeverancier.Visible = true;
            liEProgramma.Visible = true;
        //    liChangeEnrollment.Visible = false;

            liRegisterBRP.Visible = true;
            liRegisterLeverancierEDSN.Visible = true;
            liRegisterLeverancierACM.Visible = true;
            liRegisterMRP.Visible = true;
            liRegisterNetbeheerder.Visible = true;
            liRegisterDatavrager.Visible = true;
            liRegisterNetbeheerder.Visible = true;
            liRegisterGDSACM.Visible = true;
            liRegisterODA.Visible = true;
            liRegisterCSP.Visible = true;
            liRegisters.Visible = true;


      
          
            liBerichten.Visible = true;
            liChangeEnrollment.Visible = false;
            lblTennetMenu.Text = "Berichten";
            
            liTennetRapportage.Visible = true;
            liRapportageGridgain.Visible = true;
            liRapportageAansluitingenRelaties.Visible = true;
            liRapportageSwitches.Visible = true;
            liRapportageLosses.Visible = true;
            liRapportageGains.Visible = true;
            liRapportageVerbruiken.Visible = true;
            liRapportageGridgain.Visible = true;
            liPVOnbalans.Visible = true;
            liPVOnbalansPortfolio.Visible = true;

            //  liEDSNRapportage.Visible = true;
         

            liFacturatie.Visible = true;
            liRelatie.Visible = true;
            liWebservices.Visible = true;
            liGebruikersnaam.Visible = true;
            liKlantconfig.Visible = true;      
            liPortfolio.Visible = true;
            liLoginMenu.Visible = true;
            liKlantgegevens.Visible = true;
        }

        if (Session["KlantConfig"].ToString().ToUpper() == "COP")
        {

            litLogo.Text = "<img src=\"images/COP.png\" />";

            liEProgramma.Visible = true;
            liBRP.Visible = true;
            liPVOnbalans.Visible = true;
            liPVOnbalansPortfolio.Visible = true;
            liChangeEnrollment.Visible = false;
            lblTennetMenu.Text = "Berichten";
            liTennetRapportage.Visible = true;

            liRegisterBRP.Visible = false;
            liRegisterLeverancierEDSN.Visible = false;
            liRegisterLeverancierACM.Visible = false;
            liRegisterMRP.Visible = false;
            liRegisterNetbeheerder.Visible = false;
            liRegisterDatavrager.Visible = false;
            liRegisterNetbeheerder.Visible = false;
            liRegisterGDSACM.Visible = false;
            liRegisterODA.Visible = false;
            liRegisterCSP.Visible = false;
            liRegisters.Visible = false;

            liLeverancier.Visible = true;
            liFacturatie.Visible = true;
            liRelatie.Visible = true;
            liWebservices.Visible = true;
            liGebruikersnaam.Visible = true;
            liKlantconfig.Visible = true;
            liPortfolio.Visible = true;
            liLoginMenu.Visible = true;
            liKlantgegevens.Visible = true;
        }
        

        string strGebruikersnaam = Session["Gebruikersnaam"].ToString();
        string strKlantconfig = Session["Klantconfig"].ToString();



        liGebruikersnaam.InnerText = strGebruikersnaam;
        liKlantconfig.InnerText = strKlantconfig;
       
    }
}
