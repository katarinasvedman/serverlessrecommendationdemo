using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EcommerceWebApp.Models;
using EcommerceWebApp.Logic;
using System.Collections.Specialized;
using System.Collections;
using System.Web.ModelBinding;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace EcommerceWebApp
{
    public partial class ShoppingCart : System.Web.UI.Page
    {
        public static HttpClient client = new HttpClient();
        protected void Page_Load(object sender, EventArgs e)
        {
            using (ShoppingCartActions usersShoppingCart = new ShoppingCartActions())
            {
                decimal cartTotal = 0;
                cartTotal = usersShoppingCart.GetTotal();
                if (cartTotal > 0)
                {
                    // Display Total.
                    lblTotal.Text = String.Format("{0:c}", cartTotal);
                }
                else
                {
                    LabelTotalText.Text = "";
                    lblTotal.Text = "";
                    ShoppingCartTitle.InnerText = "Shopping Cart is Empty";
                    //UpdateBtn.Visible = false;
                    CheckoutImageBtn.Visible = false;
                }
            }
        }

        public List<CartItem> GetShoppingCartItems()
        {
            ShoppingCartActions actions = new ShoppingCartActions();
            return actions.GetCartItems();
        }
        public List<EntityCartItem> GetEntityCartItems()
        {
            ShoppingCartActions actions = new ShoppingCartActions();
            String cartId = actions.GetCartId();

            //Call Cart Durable Entity Function API
            List<EntityCartItem> items = new List<EntityCartItem>();
            var response = client.GetAsync("http://localhost:7071/api/CartView?CartId=" + cartId).Result;

            if (response.IsSuccessStatusCode)
            {
                var str = response.Content.ReadAsStringAsync();
                EntityCart cart = JsonConvert.DeserializeObject<EntityCart>(str.Result);
                foreach (var item in cart.Items)
                {
                    items.Add(new EntityCartItem { Id = item.Id, Price = item.Price });
                }
            }

            return items;
        }

        public List<CartItem> UpdateCartItems()
        {
            using (ShoppingCartActions usersShoppingCart = new ShoppingCartActions())
            {
                String cartId = usersShoppingCart.GetCartId();

                ShoppingCartActions.ShoppingCartUpdates[] cartUpdates = new ShoppingCartActions.ShoppingCartUpdates[Cartview.Rows.Count];
                for (int i = 0; i < Cartview.Rows.Count; i++)
                {
                    IOrderedDictionary rowValues = new OrderedDictionary();
                    rowValues = GetValues(Cartview.Rows[i]);
                    cartUpdates[i].ProductId = Convert.ToInt32(rowValues["ProductID"]);

                    CheckBox cbRemove = new CheckBox();
                    cbRemove = (CheckBox)Cartview.Rows[i].FindControl("Remove");
                    cartUpdates[i].RemoveItem = cbRemove.Checked;

                    TextBox quantityTextBox = new TextBox();
                    quantityTextBox = (TextBox)Cartview.Rows[i].FindControl("PurchaseQuantity");
                    cartUpdates[i].PurchaseQuantity = Convert.ToInt16(quantityTextBox.Text.ToString());
                }
                usersShoppingCart.UpdateShoppingCartDatabase(cartId, cartUpdates);
                Cartview.DataBind();
                lblTotal.Text = String.Format("{0:c}", usersShoppingCart.GetTotal());
                return usersShoppingCart.GetCartItems();
            }
        }

        public static IOrderedDictionary GetValues(GridViewRow row)
        {
            IOrderedDictionary values = new OrderedDictionary();
            foreach (DataControlFieldCell cell in row.Cells)
            {
                if (cell.Visible)
                {
                    // Extract values from the cell.
                    cell.ContainingField.ExtractValuesFromCell(values, cell, row.RowState, true);
                }
            }
            return values;
        }

        protected void UpdateBtn_Click(object sender, EventArgs e)
        {
            UpdateCartItems();
        }

        /**When user clicks "purchase", iterate through all items in customer's shopping cart and
         * send product id, product name, and unit price to shopping cart action file to create 
         * 'purchased' event in Cosmos DB**/
        protected void CheckoutBtn_Click(object sender, EventArgs e)
        {
            using (ShoppingCartActions usersShoppingCart = new ShoppingCartActions())
            {
                foreach (var item in usersShoppingCart.GetCartItems())
                {
                    int productId = item.ProductId;
                    string productName = item.Product.ProductName;
                    double unitPrice = Convert.ToDouble(item.Product.UnitPrice);
                    for (int k = 0; k < item.Quantity; k++) {
                        usersShoppingCart.PurchaseProduct(productId, productName, unitPrice);
                    }
                    
                }
                usersShoppingCart.EmptyCart();
                /**Redirect user to page that thanks them for their purchase**/
                Response.Redirect("~/PurchasePage.aspx");
                
            }
        }
    }
}