using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Customers.AgentProfiles.Models
{
    public record AgentShoppingCartModel : BaseNopModel
    {
        public AgentShoppingCartModel()
        {
            Items = new List<AgentShoppingCartItemModel>();
        }

        public IList<AgentShoppingCartItemModel> Items { get; set; }
    }
}
