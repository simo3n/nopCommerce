using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Extensions;
using Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Models.Customers;
using Nop.Plugin.Customers.AgentProfiles.Services;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the agent model factory implementation
    /// </summary>
    public partial class AgentModelFactory : IAgentModelFactory
    {
        #region Fields

        private readonly IAgentService _agentService;

        #endregion

        #region Ctor

        public AgentModelFactory(IAgentService agentService)
        {
            _agentService = agentService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare agent search model
        /// </summary>
        /// <param name="searchModel">Agent search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the agent search model
        /// </returns>
        public virtual async Task<AgentSearchModel> PrepareAgentSearchModelAsync(AgentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            await Task.CompletedTask;

            return searchModel;
        }

        /// <summary>
        /// Prepare paged agent list model
        /// </summary>
        /// <param name="searchModel">Agent search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the agent list model
        /// </returns>
        public virtual async Task<AgentListModel> PrepareAgentListModelAsync(AgentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get agents
            var agents = await _agentService.GetAllAgentsAsync();

            //prepare grid model
            var model = await new AgentListModel().PrepareToGridAsync(searchModel, agents, () =>
            {
                return agents.SelectAwait(async agent =>
                {
                    //fill in model values from the entity
                    var agentModel = agent.ToModel<AgentModel>();

                    await Task.CompletedTask;

                    return agentModel;
                });
            });

            return model;
        }

        #endregion
    }
}