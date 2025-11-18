<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" Inherits="_Default" Codebehind="Default.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="main-content clear-float">
        <div class="tile-area no-padding">



            <%--<h3 class="fg-orange text-light margin5"> <span class="mif-chevron-right mif-2x" style="vertical-align: top !important;"></span></h3>--%>
            <div class="tile-group no-margin no-padding" style="width: 100%;">
                <div class="tile-large ol-transparent" data-role="tile">
                    <%--<div class="panel alert" data-role="panel">
                        <div class="heading">
                            <span class="title">Services</span>
                        </div>
                        <div class="content">
                            <h5>Send Service Status : Stop</h5>
                            <h5>Receive Service Status : Stop</h5>
                            <h5>EDSN Service Status : Stop</h5>
                        </div>
                    </div>--%>
                    <div class="panel success" data-role="panel">
                        <div class="heading">
                            <span class="title">Systeem</span>
                        </div>
                        <div class="content">
                            <h5>Database : OK</h5>
                        </div>
                    </div>
                </div>
                <div class="tile-wide ol-transparent" data-role="tile">
                    <div class="panel">
                        <div class="heading">
                            <span class="icon mif-mail"></span>
                            <span class="title">Postvak Uit</span>
                        </div>
                        <div class="content">
                            <asp:Label ID="lblUitAantal" runat="server" Text="Aantal : 0"></asp:Label>
                            <br />
                            <asp:Label ID="lblNietVerzonden" runat="server" Text="Niet verzonden : 0"></asp:Label>
                           
                        </div>
                    </div>
                </div>
               <div class="tile ol-transparent" data-role="tile"></div>
                <div class="tile ol-transparent" data-role="tile"></div>
                <div class="tile ol-transparent" data-role="tile"></div>
      <%--          <div class="tile ol-transparent" data-role="tile"></div> --%>
                <div class="tile-wide ol-transparent" data-role="tile"> 
                    <div class="panel">
                        <div class="heading">
                            <span class="icon mif-mail"></span>
                            <span class="title">Postvak In</span>
                        </div>
                        <div class="content">
                            <asp:Label ID="lblInAantal" runat="server" Text="Aantal : 0"></asp:Label>
                            <br />
                            <asp:Label ID="lblInNietVerwerkt" runat="server" Text="Niet verwerkt : 0"></asp:Label>
                        </div>
                    </div>
                </div>
            </div>

      <%--       <h3 class="fg-blue text-light margin5"> <span class="mif-chevron-right mif-2x" style="vertical-align: top !important;"></span></h3>
            <div class="tile-group no-margin no-padding" style="width: 100%;">
                <div class="tile-large ol-transparent" data-role="tile"></div>
                <div class="tile-wide ol-transparent" data-role="tile"></div>
                <div class="tile ol-transparent" data-role="tile"></div>
                <div class="tile ol-transparent" data-role="tile"></div>
                <div class="tile ol-transparent" data-role="tile"></div>
                <div class="tile ol-transparent" data-role="tile"></div>
                <div class="tile-wide ol-transparent" data-role="tile"></div>
            </div>
        </div>
    </div>
   End of tiles --%>
            </div>
</div>
</asp:Content>

