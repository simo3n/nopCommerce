using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents an agent search model
    /// </summary>
    public record AgentSearchModel : BaseSearchModel
    {
        public AgentSearchModel()
        {
        }

        [NopResourceDisplayName("Admin.Customers.Agents.List.SearchAgentName")]
        public string SearchAgentName { get; set; }
    }
}
