<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" EnableEventValidation="false" Inherits="Switch" Codebehind="Switch.aspx.cs" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="panel">
        <div class="heading">
            Switch
        </div>

    </div>
    <div class="content">


        <div class="form-data-entry-style">

            <div>
                <div class="form-data-entry-style" data-role="input">
                    <br />
                    <br />
                    <asp:TextBox ID="txtEANCode" runat="server" input="EanCode" class="field-divided" placeholder="EAN"></asp:TextBox>
                    <asp:Button ID="ZoekEAN" runat="server" class="form-data-entry-style" data-role="input" input="submit" Text="Zoek" OnClick="ZoekEAN_Click" />
                      
     <label for="Kortenaam">Korte naam (24 posities EDSN)</label>

                    <asp:TextBox ID="strAansluiting_Kortenaam" runat="server" input="text" class="field-divided"  placeholder="Aansluiting Kortenaam"></asp:TextBox>
                    <asp:TextBox ID="liKlantconfig" runat="server" input="text" Visible="false" class="field-divided-klein" placeholder="Klant"></asp:TextBox>
                    <asp:Label ID="lblNetbeheerder" runat="server" Text=""></asp:Label>
                    <asp:Label ID="lblProduct" runat="server" Text=""></asp:Label>
                    <asp:Label ID="SlimmeMeterAllocatie" runat="server" Text=""></asp:Label>
                    <asp:CheckBox ID="chkSlimmeMeterAllocatie" Checked="True" runat="server" Text="" />
                    
                    <br />
                    <asp:TextBox ID="strAansluiting_Straat" runat="server" input="text" class="field-divided" placeholder="Straat" ReadOnly="True"></asp:TextBox>
                    <asp:TextBox ID="strAansluiting_Woonplaats" runat="server" input="text" class="field-divided" placeholder="Woonplaats" ReadOnly="True"></asp:TextBox>
                    <asp:TextBox ID="strAansluiting_Postcode" runat="server" input="text" class="field-divided-klein" placeholder="1234AB"></asp:TextBox>
                    <asp:TextBox ID="strAansluiting_Huisnummer" runat="server" input="text" class="field-divided-klein" placeholder="Huisnummer"></asp:TextBox>
                    <asp:TextBox ID="strAansluiting_Toevoeging" runat="server" input="text" class="field-divided-klein" placeholder="Toev."></asp:TextBox>

                </div>
                <div>

                    <label>Switch Info </label>

                    <asp:DropDownList ID="cmbSwitchSoort" runat="server" input="text" OnSelectedIndexChanged="cmbSwitchSoort_SelectedIndexChanged" AutoPostBack="true" class="field-divided" placeholder="Soort Switch"></asp:DropDownList>


                    <asp:DropDownList ID="cmbPV" runat="server" input="text" class="field-divided" placeholder="PV" ReadOnly="True"></asp:DropDownList>

                    <asp:TextBox ID="PV_EAN13_Code" runat="server" input="text" class="field-divided" placeholder="PV" Visible="false"></asp:TextBox>

                    <div class="input-control text" data-role="datepicker">
                        <asp:TextBox ID="dtpDatumSwitch" runat="server" placeholder="Switch Datum"></asp:TextBox>
                        <button id="btnDatumSwitch" class="button"><span id="spnVanaf" class="mif-calendar"></span></button>



                    </div>

                    <label>
                        Relatie <span class="required"></span>
                    </label>
                    <asp:TextBox ID="strRelatie_Email" runat="server" input="email" class="field-divided" placeholder="Email"></asp:TextBox>
                    <asp:Button ID="ZoekRelatie_Email" runat="server" class="form-data-entry-style" data-role="input" input="submit" Text="Zoek" OnClick="ZoekRelatieEmail_Click" />
                    <asp:TextBox ID="strRelatie_RelatieNaam" runat="server" input="text" class="field-divided" placeholder="Relatie Naam"></asp:TextBox>
                    <asp:TextBox ID="strRelatie_BedrijfsNaam" runat="server" input="text" class="field-divided" placeholder="Bedrijfs Naam"></asp:TextBox>
                   
              
                    <br />
                    <asp:TextBox ID="strRelatie_Postcode" runat="server" input="text" class="field-divided-klein" placeholder="1234AB"></asp:TextBox>
                    <asp:TextBox ID="strRelatie_Huisnummer" runat="server" input="text" class="field-divided-klein" placeholder="Huisnummer"></asp:TextBox>
                    <asp:TextBox ID="strRelatie_Toevoeging" runat="server" input="text" class="field-divided-klein" placeholder="Toev."></asp:TextBox>
                    <asp:Button ID="RelatieZoekPostcode" runat="server" class="form-data-entry-style" data-role="input" input="submit" Text="Zoek" OnClick="ZoekPostcode_Click" />
                    <br />
                    <asp:TextBox ID="strRelatie_Straat" runat="server" input="text" class="field-divided" placeholder="Straat" ReadOnly="True"></asp:TextBox>
                    <asp:TextBox ID="strRelatie_Woonplaats" runat="server" input="text" class="field-divided" placeholder="Woonplaats" ReadOnly="True"></asp:TextBox>
                  
                    <asp:TextBox ID="strRelatie_KvK" runat="server" input="text" placeholder="KvK"></asp:TextBox>
                     <asp:TextBox ID="strRelatie_ID" runat="server" input="text" class="field-divided" Visible="true" placeholder="ID"></asp:TextBox>
                    

      
       
                </div>




            </div>
            <br />

             <label>
                        Meterstanden <span class="required"></span>
                    </label>
            <div class="grid condensed">
                <div class="row cells4">
                    <div class="cell">
                        <asp:Label ID="lblHerkomstStand" runat="server" Text="Herkomst stand" Style="display: inherit;"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:DropDownList ID="cmbHerkomst" runat="server" Style="display: inherit;">
                            <asp:ListItem>Fysieke Opname</asp:ListItem>
                            <asp:ListItem>Berekend</asp:ListItem>
                            <asp:ListItem>Overeengekomen</asp:ListItem>
                            <asp:ListItem>Klantstand/P1-Stand</asp:ListItem>
                            <asp:ListItem>P4-stand</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                 
                    <div class="cell">
                        <asp:Label ID="lblAantalRegister" runat="server" Text="Aantal registers" Style="display: inherit;"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:TextBox ID="txtAantalRegisters" runat="server" Text="1"  Style="display: inherit;" OnTextChanged="txtAantalRegisters_TextChanged" AutoPostBack="true"></asp:TextBox>
                    </div>
                </div>
                <br />
                <div class="row cells5">
                    <div class="cell">
                        <asp:Label ID="lblRegistertype" runat="server" Text="Register type" Visible="True"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblRegisterNormaal" runat="server" Text="Normaal" Visible="True"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblRegisterLaag" runat="server" Text="Laag" Visible="False"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblRegisterNormaalTerugLevering" runat="server" Text="Normaal" Visible="False"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblRegisterLaagTerugLevering" runat="server" Text="Laag" Visible="False"></asp:Label>
                    </div>
                </div>
                <br />
                <%--<div class="row cells5">
                    <div class="cell">
                        <asp:Label ID="lblMeeteenheid" runat="server" Text="Meeteenheid" Visible="True"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblMeeteenheidNormaal" runat="server" Text="KWH" Visible="True"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblMeeteenheidLaag" runat="server" Text="KWH" Visible="True"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblMeeteenheidNormaalTerugLevering" runat="server" Text="KWH" Visible="True"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblMeeteenheidLaagTerugLevering" runat="server" Text="KWH" Visible="True"></asp:Label>
                    </div>
                </div>
                <br />--%>
                <div class="row cells5">
                    <div class="cell">
                        <asp:Label ID="lblMeetrichting" runat="server" Text="Meetrichting" Visible="True"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblMeetrichtingNormaal" runat="server" Text="LVR" Visible="True"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblMeetrichtingLaag" runat="server" Text="LVR" Visible="False"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblMeetRichtingNormaalTerugLevering" runat="server" Text="TLV" Visible="False"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblMeetRichtingLaagTerugLevering" runat="server" Text="TLV" Visible="False"></asp:Label>
                    </div>
                </div>
                <br />
                <%--<div class="row cells5">
                    <div class="cell">
                        <asp:Label ID="lblTelwielen" runat="server" Text="Aantal telwielen" Visible="True"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblTelwielenNormaal" runat="server" Text="5" Visible="True"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblTelwielenLaag" runat="server" Text="5" Visible="True"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblTelwielenNormaalTerugLevering" runat="server" Text="5" Visible="True"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:Label ID="lblTelwielenLaagTerugLevering" runat="server" Text="5" Visible="True"></asp:Label>
                    </div>
                </div>
                <br />--%>
                <div class="row cells5">
                    <div class="cell">
                        <asp:Label ID="lblStand" runat="server" Text="Stand" Visible="True"></asp:Label>
                    </div>
                    <div class="cell">
                        <asp:TextBox ID="txtStandNormaal" runat="server" Visible="True"></asp:TextBox>
                    </div>
                    <div class="cell">
                        <asp:TextBox ID="txtStandLaag" runat="server" Visible="False"></asp:TextBox>
                    </div>
                    <div class="cell">
                        <asp:TextBox ID="txtStandNormaalTerugLevering" runat="server" Visible="False"></asp:TextBox>
                    </div>
                    <div class="cell">
                        <asp:TextBox ID="txtStandLaagTerugLevering" runat="server" Visible="False"></asp:TextBox>
                    </div>
                    <%--<asp:Button ID="Button1" runat="server" Text="Button" OnClick="Button1_Click" />--%>
                </div>
            </div>
            <br />
            <br />
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:Button ID="btnIndienenSwitch" runat="server" class="form-data-entry-style" data-role="input" input="submit" Text="Switch Indienen" OnClick="IndienenSwitch_Click" />
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  
             
                &nbsp;&nbsp;
               
        </div>
    </div>




</asp:Content>


