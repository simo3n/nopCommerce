using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents an agent model
    /// </summary>
    public partial record AgentModel : BaseNopEntityModel
    {
        #region Ctor

        public AgentModel()
        {
            if (PageSize < 1)
                PageSize = 5;

            AvailableAgents = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Customers.Agents.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Customers.Agents.Fields.Parent")]
        public int ParentAgentId { get; set; }

        [NopResourceDisplayName("Admin.Customers.Agents.Fields.PageSize")]
        public int PageSize { get; set; }

        public IList<SelectListItem> AvailableAgents { get; set; }

        #endregion
    }

    public partial record AgentLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Customers.Agents.Fields.Name")]
        public string Name { get; set; }
    }
}