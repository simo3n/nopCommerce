using Nop.Web.Framework.Models;

namespace Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents an agent list model
    /// </summary>
    public record AgentListModel : BasePagedListModel<AgentModel>
    {
    }
}