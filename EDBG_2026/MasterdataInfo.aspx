<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" Inherits="MasterdataInfo" EnableEventValidation="false" Codebehind="MasterdataInfo.aspx.cs" %>



<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="panel">
        <div class="heading">
            Info Aansluiting</div>
       </div>
        <div class="content">


 <div class="form-data-entry-style">    

       <div>
           <div class="form-data-entry-style" data-role="input">
          <label>
               <br />
               <br />
               Aansluiting <span class="required">
                </span></label>
                <asp:TextBox ID="txtEANCode" runat="server" input="EanCode"  class="field-divided" OnTextChanged="txtEANCode_TextChanged" AutoPostBack="True" placeholder="EAN"  ></asp:TextBox>
                <asp:Button ID="ZoekEAN" runat="server" class="form-data-entry-style" data-role="input" input="submit" Text="Zoek" OnClick="btnZoek_Click"></asp:Button>
                 <asp:Label ID="lblError" runat="server" Text=""></asp:Label>
                <br />
               <label>Adres </label>
                <asp:TextBox ID="txtStraat" runat="server" input="text"  class="field-divided" placeholder="Straat" ReadOnly="True" ></asp:TextBox>  
                <asp:TextBox ID="txtWoonplaats" runat="server" input="text"  class="field-divided" placeholder="Woonplaats" ReadOnly="True" ></asp:TextBox>
                <asp:TextBox ID="txtPostcode" runat="server" input="text"  class="field-divided-klein" placeholder="1234AB"  ></asp:TextBox>
                <asp:TextBox ID="txtHuisnummer" runat="server" input="text"  class="field-divided-klein" placeholder="Huisnummer"  ></asp:TextBox>
                <asp:TextBox ID="txtToevoeging" runat="server" input="text"  class="field-divided-klein" placeholder="Toev." ></asp:TextBox>
                <br />
               <br />
                
                
           
        </div>
   <div>
       
       

        


                    
                         <label>Netbeheerder </label>        
                        <asp:TextBox ID="lblNetbeheerder" runat="server" Text=""></asp:TextBox>
           <br />
           <br />
                         <label>Netgebied </label>        
                        <asp:TextBox ID="lblNetgebied" runat="server" Text=""></asp:TextBox>
           <br />
           <br />         
                            <label>Verbruiksegment</label>           
                        <asp:TextBox ID="lblVerbruiksegment" runat="server" Text=""></asp:TextBox>
            <br />
            <br />         
                           <label>leverancier </label>                    
                       <asp:TextBox ID="lblLV" runat="server" Text=""></asp:TextBox>
            <br />
            <br />      
                          <label>Leveringsrichting </label>                   
                        <asp:TextBox ID="lblLeveringsrichting" runat="server" Text=""></asp:TextBox>
            <br />
            <br />       
                            <label>PV </label>                
                        <asp:TextBox ID="lblPV" runat="server" Text=""></asp:TextBox>
            <br />
            <br />      
                           <label>AdminStatus </label>         
                       <asp:TextBox ID="lblAdminStatus" runat="server" Text=""></asp:TextBox>
            <br />
            <br />   
                           <label>Netgebied </label>       
                        <asp:TextBox ID="lblProduct" runat="server" Text=""></asp:TextBox>
           <br />
           <br />       
                           <label>Wijze van Bemetering </label>                   
                        <asp:TextBox ID="lblBemetering" runat="server" Text=""></asp:TextBox>
            <br />
            <br />  
                            <label>Levering Status</label>        
                           <asp:TextBox ID="lblLeveringstatus" runat="server" Text=""></asp:TextBox>
            <br />
            <br />
                
                       <label>SJV Normaal </label>        
                        <asp:TextBox ID="lblSJVNormaal" runat="server" Text=""></asp:TextBox>
            <br />
            <br />  
                           <label>AllocatieMethode </label>                
                        <asp:TextBox ID="lblAllocatieMethode" runat="server" Text=""></asp:TextBox>
            <br />
            <br />
                     <label>SJV Laag</label>       
                        <asp:TextBox ID="lblSJVLaag" runat="server" Text=""></asp:TextBox>
            <br />
            <br />
                        <label>Profiel </label>       
                         <asp:TextBox ID="lblProfiel" runat="server" Text=""></asp:TextBox>
            <br />
            <br />
                        <label>Capaciteitscode </label>       
                        <asp:TextBox ID="lblCapaciteitscode" runat="server" Text=""></asp:TextBox>
             <br />
            <br />
                
                       <label>SJI Normaal </label>        
                        <asp:TextBox ID="lblSJINormaal" runat="server" Text=""></asp:TextBox>
            <br />
            <br />  
                     <label>SJI Laag</label>       
                        <asp:TextBox ID="lblSJILaag" runat="server" Text=""></asp:TextBox>            
            
       
        </div>     
       
      
       
        
        </div>    
            <br />
            
               
    </div>
           </div>

    

         
   
</asp:Content>  



