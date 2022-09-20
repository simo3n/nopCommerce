using Nop.Web.Framework.Models;

namespace Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents an agent list model
    /// </summary>
    public partial record AgentListModel : BasePagedListModel<AgentModel>
    {
    }
}