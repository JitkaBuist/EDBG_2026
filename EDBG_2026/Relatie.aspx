<%@ Page Title="" Language="C#"  MasterPageFile="~/MasterPage.master" AutoEventWireup="true" EnableEventValidation="false" Inherits="Relatie" Codebehind="Relatie.aspx.cs" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    
    <div class="panel">
        <div class="heading">
            Relatie</div>
          
       </div>
        <div class="content">





 <div class="form-data-entry-style">    

       <div>
           <div class="form-data-entry-style" data-role="input">
       
     

               <br />
                <br />
    
        <asp:TextBox ID="strRelatie_Email" runat="server" input="email"  class="field-divided"  AutoPostBack="true" placeholder="Email"  ></asp:TextBox>
        <asp:Button ID="btnZoekRelatie" runat="server" class="form-data-entry-style" data-role="input" input="submit" Text="Zoek" OnClick="ZoekRelatie_Click"  />
         <asp:TextBox ID="strRelatie_Bedrijfsnaam" runat="server" input="text"  visible="false" class="field-divided" placeholder="Bedrijfsnaam" ReadOnly="False" ></asp:TextBox> 
         <asp:TextBox ID="strRelatie_KvK" class="field-divided" visible="false" placeholder="KvK" runat="server"></asp:TextBox> 
           </div>
  
   <div>
        <asp:Label ID="Label2" Text="Bedrijf J/N  " runat="server" Font-Bold="False" AutoPostBack="true" Font-Size="Smaller"></asp:Label> 
           <asp:CheckBox runat="server" ID="chkBedrijf" OnCheckedChanged="chkBedrijf_CheckedChanged"  AutoPostBack="true" Font-Bold="False"  Checked="True" Font-Size="Smaller"></asp:CheckBox> 
       <asp:Label ID="Label4" Text="Toestemming Slimme Meter  " runat="server" Font-Bold="False" Font-Size="Smaller"></asp:Label> 
           <asp:CheckBox runat="server" ID="chkSlimmeMeterToestemming"   AutoPostBack="true" Font-Bold="False" Checked="True" Font-Size="Smaller"></asp:CheckBox>
        
     <br />
   
       <asp:TextBox ID="strRelatie_Relatienaam" runat="server" input="text"  class="field-divided" placeholder="Relatie Naam" ></asp:TextBox>   
       <asp:TextBox ID="strRelatie_Voornaam" runat="server" input="text"  class="field-divided" placeholder="Voornaam" ></asp:TextBox>
        <asp:TextBox ID="strRelatie_Tussenvoegsel" runat="server" input="text"  class="field-divided-klein" placeholder="Tussenv." ></asp:TextBox>
        <asp:TextBox ID="strRelatie_Achternaam" runat="server" input="text"  class="field-divided" placeholder="Achternaam"  ></asp:TextBox>
       
      
     
            <div class="input-control text" data-role="datepicker" data-start-mode="year">
                <button  id="btnGeboortedatum" runat="server" class="button"><span  class="mif-calendar" ></span> </button>
        
  <asp:TextBox ID="strRelatie_Geboortedatum" runat="server"    placeholder="Geboortedatum"></asp:TextBox>
      
  
      
  
   </div>
       <div>
           
               
         
       </div>
       <div>   
        <asp:TextBox ID="strRelatie_Postcode" runat="server" input="text"  class="field-divided-klein" placeholder="1234AB"  ></asp:TextBox>
        <asp:TextBox ID="strRelatie_Huisnummer" runat="server" input="text"  class="field-divided-klein" placeholder="Huisnummer"  ></asp:TextBox>
        <asp:TextBox ID="strRelatie_Toevoeging" runat="server" input="text"  class="field-divided-klein" placeholder="Toev." ></asp:TextBox>
        <asp:Button ID="btnRelatieZoekPostcode" runat="server" class="form-data-entry-style" data-role="input" input="submit" Text="Zoek" OnClick="ZoekRelatiePostcode_Click"  />
             <br />
        <asp:TextBox ID="strRelatie_Straat" runat="server" input="text"  class="field-divided" placeholder="Straat" ReadOnly="True" ></asp:TextBox>  
        <asp:TextBox ID="strRelatie_Woonplaats" runat="server" input="text"  class="field-divided" placeholder="Woonplaats" ReadOnly="True" ></asp:TextBox>
         </div>
          <div>
  
     
   
   
      
              </div>
     
  
            <div>


                <br />
                 <br />
                 <br />

              <asp:Button ID="NieuweRelatieAanmaken" runat="server" class="form-data-entry-style" data-role="input" input="submit" Text="Nieuwe Relatie Toevoegen" OnClick="NieuweRelatieAanmaken_Click"  />  
                <asp:Label ID="lblMeldingAanmaken" runat="server" Font-Bold="False" Font-Size="Smaller" Text= ""/>
                 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  
                 <asp:Button ID="RelatieAanpassen" runat="server" class="form-data-entry-style" data-role="input" input="submit" Text="Relatie Aanpassen" OnClick="RelatieAanpassen_Click"  />
                
                &nbsp;&nbsp;
                <asp:TextBox ID="strNieuwEmail" runat="server" input="email"  class="field-divided" placeholder="Nieuw email adres"    ></asp:TextBox>
                 <asp:Button ID="Email" runat="server" class="form-data-entry-style" data-role="input" input="submit" Text="Email Aanpassen" OnClick="EmailAanpassen_Click"  />

                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;

              <asp:Button ID="NieuweKlachtAanmaken" runat="server" class="form-data-entry-style" data-role="input" input="submit" Text="Klacht Toevoegen" OnClick="NieuweKlachtAanmaken_Click"  />  

                <br />
                <br />
                <br />
 </div>
        </div>
    </div>
              </div>
        <asp:Label ID="Label3" Text="Account:  " runat="server" Font-Bold="True"  ReadOnly="True" Font-Size="Smaller"></asp:Label>
     <asp:TextBox ID="liKlantconfig" runat="server" input="text"  class="field-divided-klein" ReadOnly="True" placeholder="Klant" ></asp:TextBox> 
      <asp:Label ID="Label5" Text="Gebruikersnaam:  " runat="server" Font-Bold="True"  ReadOnly="True" Font-Size="Smaller"></asp:Label>
     <asp:TextBox ID="liGebruikersnaam" runat="server" input="text"  class="field-divided-klein" ReadOnly="True" placeholder="Gebruikersnaam" ></asp:TextBox> 

    </div>
</asp:Content>       



