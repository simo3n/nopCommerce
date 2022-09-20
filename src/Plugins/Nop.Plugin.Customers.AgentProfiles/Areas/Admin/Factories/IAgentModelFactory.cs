using System.Threading.Tasks;
using Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Models.Customers;
using Nop.Plugin.Customers.AgentProfiles.Domains;

namespace Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Factories
{
    public interface IAgentModelFactory
    {
        /// <summary>
        /// Prepare agent search model
        /// </summary>
        /// <param name="searchModel">Agent search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the agent search model
        /// </returns>
        Task<AgentSearchModel> PrepareAgentSearchModelAsync(AgentSearchModel searchModel);

        /// <summary>
        /// Prepare paged agent list model
        /// </summary>
        /// <param name="searchModel">Agent search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the agent list model
        /// </returns>
        Task<AgentListModel> PrepareAgentListModelAsync(AgentSearchModel searchModel);

        Task<AgentModel> PrepareAgentModelAsync(AgentModel model, Agent agent);
    }
}
