<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ShoppingCart.aspx.cs" Inherits="EcommerceWebApp.ShoppingCart" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div id="ShoppingCartTitle" runat="server" class="ContentHead"><h1>Shopping Cart</h1></div>
    
    <asp:GridView ID="Cartview" runat="server" AutoGenerateColumns="False" ShowFooter="True" GridLines="Vertical" CellPadding="4"
        ItemType="EcommerceWebApp.Models.EntityCartItem" SelectMethod="GetEntityCartItems" 
        CssClass="table table-striped table-bordered" >   
        <Columns>
        <asp:BoundField DataField="Id" HeaderText="Name" />        
        <asp:BoundField DataField="Price" HeaderText="Price (each)" DataFormatString="{0:c}"/> 
        </Columns>    
    </asp:GridView>
    <div>
        <p></p>
        <strong>
            <asp:Label ID="LabelTotalText" runat="server" Text="Order Total: "></asp:Label>
            <asp:Label ID="lblTotal" runat="server" EnableViewState="false"></asp:Label>
        </strong> 
    </div>
   
    <br />
        <asp:Button ID="CheckoutImageBtn" runat="server" Text="Purchase" OnClick="CheckoutBtn_Click" />
</asp:Content>