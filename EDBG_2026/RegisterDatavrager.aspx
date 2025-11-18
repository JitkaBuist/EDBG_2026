<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" Inherits="RegisterDatavrager" EnableEventValidation="false" Codebehind="RegisterDatavrager.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="panel">
        <div class="heading">
            <span class="title">Register Datavrager</span>
        </div>


   
        <button type="button" onclick="exportToXlsx()">Exporteer Excel</button>

<script src="https://cdn.jsdelivr.net/npm/xlsx@0.18.5/dist/xlsx.full.min.js"></script>
<script>
    function exportToXlsx() {
        const table = document.querySelector('.custom-grid');
        const wb = XLSX.utils.table_to_book(table, { sheet: "Sheet1" });
        XLSX.writeFile(wb, "Datavrager.xlsx"); // Echte XLSX, geen warning
    }
</script>
        <div class="content">

 <style>
.custom-grid {
    width: 100%;
    border-collapse: collapse;
    font-family: Arial, sans-serif;
    margin: 20px 0;
    box-shadow: 0 1px 3px rgba(0,0,0,0.1);
}

.custom-grid th, .custom-grid td {
    padding: 12px 15px;
    border: 1px solid #e0e0e0;
    text-align: left;
    vertical-align: top;
}

.custom-grid th {
    background-color: #f8f9fa;
    font-weight: 600;
    color: #333;
    position: sticky;
    top: 0;
}

.custom-grid tr:nth-child(even) {
    background-color: #f9f9f9;
}

.custom-grid tr:hover {
    background-color: #f1f1f1;
}

/* Aangepaste kolombreedtes */
.Organisatie-col { width: 150px; min-width: 150px; }
.EAN-col { width: 200px; min-width: 200px; }
.Adres-col { width: 120px; min-width: 120px; }
.Redispatch-col { width: 200px; min-width: 200px; }
.Capaciteitsbeperking-col { width: 120px; min-width: 120px; }


/* Responsive aanpassingen */
@media (max-width: 1200px) {
    .custom-grid {
        display: block;
        overflow-x: auto;
        white-space: nowrap;
    }
    
    .custom-grid th, 
    .custom-grid td {
        white-space: normal;
    }
}
</style>

<table class="custom-grid">
    <thead>
        <tr>
         
            <th class="Organisatie-col">Organisatie</th>
            <th class="EAN-col">EAN</th>
            <th class="Adres">Adres</th>
         
          
          
        </tr>
    </thead>
    <tbody>
        <asp:Literal ID="litOverzicht" runat="server" />
    </tbody>
</table>
            </div>
           
        </div>

</asp:Content>


