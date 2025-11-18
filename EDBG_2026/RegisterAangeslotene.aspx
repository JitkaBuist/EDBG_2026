<%@ Page Title="Register Aangeslotene" Language="C#" MasterPageFile="~/MasterPage.master"
    AutoEventWireup="true"
    Inherits="RegisterAangeslotene" EnableEventValidation="false" Codebehind="RegisterAangeslotene.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style>
        /* --- Layout wit/blauw stijl --- */
        body {
            background-color: #f7f9fc;
            font-family: 'Segoe UI', sans-serif;
            color: #333;
        }

        .panel {
            background: #fff;
            border-radius: 10px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
            padding: 20px;
            margin: 20px auto;
            max-width: 1100px;
        }

        .heading {
            border-bottom: 3px solid #0078d7;
            padding-bottom: 10px;
            margin-bottom: 15px;
        }

        .heading .title {
            font-size: 20px;
            color: #0078d7;
            font-weight: 600;
        }

        .btn-bar {
            display: flex;
            gap: 10px;
            margin-bottom: 15px;
        }

        .btn-primary {
            background-color: #0078d7;
            color: #fff;
            border: none;
            border-radius: 5px;
            padding: 8px 16px;
            font-size: 14px;
            cursor: pointer;
            transition: background-color 0.2s ease;
        }

        .btn-primary:hover {
            background-color: #005ea6;
        }

        /* --- Grid styling --- */
        .custom-grid {
            width: 100%;
            border-collapse: collapse;
            background: #fff;
        }

        .custom-grid th {
            background-color: #0078d7;
            color: #fff;
            font-weight: 500;
            padding: 10px;
            text-align: left;
        }

        .custom-grid td {
            border-bottom: 1px solid #e5e5e5;
            padding: 8px 10px;
        }

        .custom-grid tr:nth-child(even) {
            background-color: #f9fbff;
        }

        .custom-grid tr:hover {
            background-color: #eaf3ff;
        }

        .custom-grid input[type=text] {
            width: 95%;
            padding: 4px 6px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }

        .custom-grid .aspNetDisabled {
            background-color: #f5f5f5;
            color: #999;
        }

        .custom-grid a {
            color: #0078d7;
            text-decoration: none;
        }

        .custom-grid a:hover {
            text-decoration: underline;
        }
       
.btn-new {
    background-color: #ffffff;
    color: #0078d7;
    border: 1px solid #0078d7;
    border-radius: 6px;
    padding: 6px 12px;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease-in-out;
    display: inline-flex;
    align-items: center;
    gap: 5px;
}

.btn-new .icon {
    font-size: 16px;
    line-height: 1;
}

.btn-new:hover {
    background-color: #0078d7;
    color: #ffffff;
    border-color: #0078d7;
}
.search-bar {
    display: flex; gap: 10px; align-items: center; margin: 10px 0 15px;
}
.search-input {
    flex: 1; max-width: 360px;
    padding: 8px 10px; border: 1px solid #cfe3f8; border-radius: 6px;
}
.btn-secondary {
    background:#ffffff; color:#555; border:1px solid #cfd8e3;
    border-radius:6px; padding:6px 12px; font-size:14px; transition:.2s;
}
.btn-secondary:hover { background:#f2f6fb; }


    </style>
             

<script src="https://cdn.jsdelivr.net/npm/xlsx@0.18.5/dist/xlsx.full.min.js"></script>
<script>
    function exportToXlsx() {
        const table = document.querySelector('.custom-grid');
        const wb = XLSX.utils.table_to_book(table, { sheet: "Sheet1" });
        XLSX.writeFile(wb, "RegisterAangeslotene.xlsx"); // Echte XLSX, geen warning
    }
</script>
            
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="panel">
        <div class="heading">
            <span class="title">Register Aangeslotene</span>
        </div>

       <div class="btn-bar">
    <button type="button" class="btn-primary" onclick="exportToXlsx()">📤 Exporteer naar Excel</button>
</div>

<div class="search-bar">
    <asp:TextBox ID="txtSearch" runat="server" CssClass="search-input" placeholder="Zoek op naam..." />
    <asp:LinkButton ID="btnSearch" runat="server" CssClass="btn-primary" OnClick="btnSearch_Click">
        <span class="icon">🔎</span> Zoeken
    </asp:LinkButton>
    <asp:LinkButton ID="btnClear" runat="server" CssClass="btn-secondary" OnClick="btnClear_Click">
        Reset
    </asp:LinkButton>
</div>

<<asp:GridView ID="gvRegisterAangeslotene" runat="server"
    AutoGenerateColumns="False"
    CssClass="custom-grid"
    DataKeyNames="ID"
    ShowFooter="true"
    AllowPaging="true" PageSize="200"
    OnPageIndexChanging="gv_PageIndexChanging"
    OnRowEditing="gv_RowEditing"
    OnRowCancelingEdit="gv_RowCancelingEdit"
    OnRowUpdating="gv_RowUpdating"
    OnRowCommand="gv_RowCommand">
    <Columns>
        <asp:TemplateField HeaderText="Bedrijfsnaam">
            <ItemTemplate><%# Eval("Bedrijfsnaam") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtBedrijfsnaam_Edit" runat="server" Text='<%# Bind("Bedrijfsnaam") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtBedrijfsnaam_New" runat="server" placeholder="Bedrijfsnaam" /></FooterTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Status"  Visible="false">
    <ItemTemplate>
        <%# Eval("Status") %>
    </ItemTemplate>
    <EditItemTemplate>
        <asp:DropDownList ID="cmbStatus_Edit" runat="server" Visible="true" SelectedValue='<%# Bind("Status") %>'>
            <asp:ListItem Text="Aangeslotene" Value="Aangeslotene" />
            <asp:ListItem Text="Leverancier" Value="Leverancier" />
            <asp:ListItem Text="Netbeheerder" Value="Netbeheerder" />
            <asp:ListItem Text="BRP" Value="BRP" />
            <asp:ListItem Text="MRP" Value="MRP" />
            <asp:ListItem Text="CSP" Value="CSP" />
            <asp:ListItem Text="GDS" Value="GDS" />
            <asp:ListItem Text="ODA" Value="ODA" />
             <asp:ListItem Text="Systeempartij" Value="Systeempartij" />
        </asp:DropDownList>
    </EditItemTemplate>
    <FooterTemplate>
                <asp:DropDownList ID="cmbStatus_New" runat="server" Visible="true" SelectedValue='<%# Bind("Status") %>'>
            <asp:ListItem Text="Aangeslotene" Value="Aangeslotene" />
            <asp:ListItem Text="Leverancier" Value="Leverancier" />
            <asp:ListItem Text="Netbeheerder" Value="Netbeheerder" />
            <asp:ListItem Text="BRP" Value="BRP" />
            <asp:ListItem Text="MRP" Value="MRP" />
            <asp:ListItem Text="CSP" Value="CSP" />
            <asp:ListItem Text="GDS" Value="GDS" />
            <asp:ListItem Text="ODA" Value="ODA" />
            <asp:ListItem Text="Systeempartij" Value="Systeempartij" />

        </asp:DropDownList>
    </FooterTemplate>
</asp:TemplateField>
        <asp:TemplateField HeaderText="Voorletters">
            <ItemTemplate><%# Eval("Voorletters") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtVoorletters_Edit" runat="server" Text='<%# Bind("Voorletters") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtVoorletters_New" runat="server" placeholder="Voorletters" /></FooterTemplate>
        </asp:TemplateField>
         <asp:TemplateField HeaderText="Voornaam">
     <ItemTemplate><%# Eval("Voornaam") %></ItemTemplate>
     <EditItemTemplate><asp:TextBox ID="txtVoornaam_Edit" runat="server" Text='<%# Bind("Voornaam") %>' /></EditItemTemplate>
     <FooterTemplate><asp:TextBox ID="txtVoornaam_New" runat="server" placeholder="Voornaam" /></FooterTemplate>
 </asp:TemplateField>
        <asp:TemplateField HeaderText="Tussenv">
            <ItemTemplate><%# Eval("Tussenvoegsel") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtTussenvoegsel_Edit" runat="server" Text='<%# Bind("Tussenvoegsel") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtTussenvoegsel_New" runat="server" placeholder="Tussenvoegsel" /></FooterTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Achternaam">
            <ItemTemplate><%# Eval("Achternaam") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtAchternaam_Edit" runat="server" Text='<%# Bind("Achternaam") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtAchternaam_New" runat="server" placeholder="Achternaam" /></FooterTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Straat">
            <ItemTemplate><%# Eval("Straat") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtStraat_Edit" runat="server" Text='<%# Bind("Straat") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtStraat_New" runat="server" placeholder="Straat" /></FooterTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="nr.">
            <ItemTemplate><%# Eval("Huisnummer") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtHuisnummer_Edit" runat="server" Text='<%# Bind("Huisnummer") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtHuisnummer_New" runat="server" placeholder="Huisnummer" /></FooterTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Toev">
            <ItemTemplate><%# Eval("Toevoeging") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtToevoeging_Edit" runat="server" Text='<%# Bind("Toevoeging") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtToevoeging_New" runat="server" placeholder="Toevoeging" /></FooterTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Postcode">
            <ItemTemplate><%# Eval("Postcode") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtPostcode_Edit" runat="server" Text='<%# Bind("Postcode") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtPostcode_New" runat="server" placeholder="Postcode" /></FooterTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Woonplaats">
            <ItemTemplate><%# Eval("Woonplaats") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtWoonplaats_Edit" runat="server" Text='<%# Bind("Woonplaats") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtWoonplaats_New" runat="server" placeholder="Woonplaats" /></FooterTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Email">
            <ItemTemplate><%# Eval("Email") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtEmail_Edit" runat="server" Text='<%# Bind("Email") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtEmail_New" runat="server" placeholder="naam@domein.nl" /></FooterTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Geboortedatum">
            <ItemTemplate><%# Eval("GeboorteDatum", "{0:dd-MM-yyyy}") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtGeboortedatum_Edit" runat="server" Text='<%# Bind("Geboortedatum","{0:yyyy-MM-dd}") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtGeboortedatum_New" runat="server" placeholder="jjjj-MM-dd" /></FooterTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Telefoonnr">
            <ItemTemplate><%# Eval("Telefoonnummer") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtTelefoonnummer_Edit" runat="server" Text='<%# Bind("Telefoonnummer") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtTelefoonnummer_New" runat="server" placeholder="0612345678" /></FooterTemplate>
        </asp:TemplateField>
         <asp:TemplateField HeaderText="KVK">
     <ItemTemplate><%# Eval("KVK") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtKVK_Edit" runat="server" Text='<%# Bind("KVK") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtKVK_New" runat="server" placeholder="KVKnummer" /></FooterTemplate>
     </asp:TemplateField>

     
        <asp:TemplateField>
            <ItemTemplate>
                <asp:LinkButton runat="server" CommandName="Edit" Text="✏️ Wijzig" />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:LinkButton runat="server" CommandName="Update" Text="💾 Opslaan" />
                &nbsp;
                <asp:LinkButton runat="server" CommandName="Cancel" Text="❌ Annuleer" />
            </EditItemTemplate>
            <FooterTemplate>
                <asp:LinkButton runat="server" CssClass="btn-new" CommandName="Insert">
                    <span class="icon">➕</span> Nieuw
                </asp:LinkButton>

            </FooterTemplate>
        </asp:TemplateField>

    </Columns>
</asp:GridView>

</asp:Content>
