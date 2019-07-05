using System.Collections.Generic;

namespace EcommerceWebApp.Models
{

    /// <summary>
    /// Architects every cart on the website
    /// </summary>
    public class EntityCart
    {

        public string CartId { get; set; }

        public double TotalAmount { get; set; }

        public List<EntityCartItem> Items { get; set; }
    }

    public class EntityCartItem
    {
        public string Id { get; set; }
        public double Price { get; set; }
    }
}