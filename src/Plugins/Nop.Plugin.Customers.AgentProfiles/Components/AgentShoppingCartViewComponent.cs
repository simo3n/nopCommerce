using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Customers.AgentProfiles.Components
{
    [ViewComponent(Name = "AgentShoppingCart")]
    public class AgentShoppingCartViewComponent : NopViewComponent
    {
        public AgentShoppingCartViewComponent()
        {

        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            return View("~/Plugins/Customers.AgentProfiles/Views/AgentShoppingCart.cshtml");
        }
    }
}
