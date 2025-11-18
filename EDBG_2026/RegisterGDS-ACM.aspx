<%@ Page Title="Register GDS ACM" Language="C#" MasterPageFile="~/MasterPage.master"
    AutoEventWireup="true"
    Inherits="RegisterGDSACM" EnableEventValidation="false" Codebehind="RegisterGDS-ACM.aspx.cs" %>

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
        XLSX.writeFile(wb, "RegisterGDSACM.xlsx"); // Echte XLSX, geen warning
    }
</script>
            
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="panel">
        <div class="heading">
            <span class="title">Register GDS-ACM</span>
        </div>

       <div class="btn-bar">
    <button type="button" class="btn-primary" onclick="exportToXlsx()">📤 Exporteer naar Excel</button>
</div>

<asp:GridView ID="gvGDSACM" runat="server"
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

        <asp:TemplateField HeaderText="Partijen">
            <ItemTemplate><%# Eval("Partijen") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtPartijen_Edit" runat="server" Text='<%# Bind("Partijen") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtPartijen_New" runat="server" placeholder="Partijen" /></FooterTemplate>
        </asp:TemplateField>

         <asp:TemplateField HeaderText="Soort_besluit">
            <ItemTemplate><%# Eval("Soort_besluit") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtSoort_besluit_Edit" runat="server" Text='<%# Bind("Soort_besluit") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtSoort_besluit_New" runat="server" placeholder="Soort Besluit" /></FooterTemplate>
        </asp:TemplateField>

         <asp:TemplateField HeaderText="Soort_E_of_G">
            <ItemTemplate><%# Eval("Soort_E_of_G") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtSoort_E_of_G_Edit" runat="server" Text='<%# Bind("Soort_E_of_G") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtSoort_E_of_G_New" runat="server" placeholder="Soort Besluit" /></FooterTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Plaats">
            <ItemTemplate><%# Eval("Plaats") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtPlaats_Edit" runat="server" Text='<%# Bind("Plaats") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtPlaats_New" runat="server" placeholder="Plaats" /></FooterTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Zaaknummer">
            <ItemTemplate><%# Eval("Zaaknummer") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtZaaknummer_Edit" runat="server" Text='<%# Bind("Zaaknummer") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtZaaknummer_New" runat="server" placeholder="Zaaknummer" /></FooterTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Datum_van_beschikking">
            <ItemTemplate><%# Eval("Datum_van_beschikking", "{0:dd-MM-yyyy}") %></ItemTemplate>
            <EditItemTemplate><asp:TextBox ID="txtDatum_van_beschikking_Edit" runat="server" Text='<%# Bind("Datum_van_beschikking","{0:yyyy-MM-dd}") %>' /></EditItemTemplate>
            <FooterTemplate><asp:TextBox ID="txtDatum_van_beschikking_New" runat="server" placeholder="jjjj-MM-dd" /></FooterTemplate>
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
