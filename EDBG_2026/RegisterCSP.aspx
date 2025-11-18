<%@ Page Title="Register CSP" Language="C#" MasterPageFile="~/MasterPage.master"
    AutoEventWireup="true"
    Inherits="RegisterCSP" EnableEventValidation="false" Codebehind="RegisterCSP.aspx.cs" %>

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

    </style>
             

<script src="https://cdn.jsdelivr.net/npm/xlsx@0.18.5/dist/xlsx.full.min.js"></script>
<script>
    function exportToXlsx() {
        const table = document.querySelector('.custom-grid');
        const wb = XLSX.utils.table_to_book(table, { sheet: "Sheet1" });
        XLSX.writeFile(wb, "RegisterCSP.xlsx"); // Echte XLSX, geen warning
    }
</script>
            
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="panel">
        <div class="heading">
            <span class="title">Register CSP</span>
        </div>

       <div class="btn-bar">
    <button type="button" class="btn-primary" onclick="exportToXlsx()">📤 Exporteer naar Excel</button>
</div>

<asp:GridView ID="gvCSP" runat="server"
    AutoGenerateColumns="False"
    CssClass="custom-grid"
    DataKeyNames="EAN"
    ShowFooter="true"
    OnRowEditing="gv_RowEditing"
    OnRowCancelingEdit="gv_RowCancelingEdit"
    OnRowUpdating="gv_RowUpdating"
    OnRowCommand="gv_RowCommand">
    <Columns>

    
        <asp:TemplateField HeaderText="EAN">
            <ItemTemplate>
                <%# Eval("EAN") %>
            </ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="ean" runat="server" Text='<%# Bind("EAN") %>' CssClass="aspNetDisabled" ReadOnly="true" />
            </EditItemTemplate>
            <FooterTemplate>
                <asp:TextBox ID="txtEAN_New" runat="server" placeholder="13 cijfers" />
            </FooterTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Status">
            <ItemTemplate><%# Eval("Status") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtStatus_Edit" runat="server" Text='<%# Bind("Status") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtStatus_New" runat="server" placeholder="Status" /></FooterTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Naam">
            <ItemTemplate><%# Eval("Naam") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtNaam_Edit" runat="server" Text='<%# Bind("Naam") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtNaam_New" runat="server" placeholder="Naam" /></FooterTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Adres">
            <ItemTemplate><%# Eval("Adres") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtAdres_Edit" runat="server" Text='<%# Bind("Adres") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtAdres_New" runat="server" placeholder="Adres" /></FooterTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Email">
            <ItemTemplate><%# Eval("Email") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtEmail_Edit" runat="server" Text='<%# Bind("Email") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtEmail_New" runat="server" placeholder="naam@domein.nl" /></FooterTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Datum">
            <ItemTemplate><%# Eval("Datum", "{0:dd-MM-yyyy}") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtDatum_Edit" runat="server" Text='<%# Bind("Datum","{0:yyyy-MM-dd}") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtDatum_New" runat="server" placeholder="jjjj-MM-dd" /></FooterTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Redispatch">
            <ItemTemplate><%# Eval("Redispatch") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtRedispatch_Edit" runat="server" Text='<%# Bind("Redispatch") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtRedispatch_New" runat="server" placeholder="Y/N" /></FooterTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Capaciteitsbeperking">
             <ItemTemplate><%# Eval("Capaciteitsbeperking") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtCapaciteitsbeperking_Edit" runat="server" Text='<%# Bind("Capaciteitsbeperking") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtCapaciteitsbeperking_New" runat="server" placeholder="Y/N" /></FooterTemplate>
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
