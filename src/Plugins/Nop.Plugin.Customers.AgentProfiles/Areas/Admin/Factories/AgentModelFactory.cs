using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Models.Customers;
using Nop.Plugin.Customers.AgentProfiles.Domains;
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
        /// Prepare agent model
        /// </summary>
        /// <param name="model">Agent model</param>
        /// <param name="agent">Agent</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the agent model
        /// </returns>
        public virtual async Task<AgentModel> PrepareAgentModelAsync(AgentModel model, Agent agent)
        {
            if (agent != null)
                model = agent.ToModel<AgentModel>();

            //prepare available parent agents
            await PrepareAgentsAsync(model.AvailableAgents);

            return await Task.FromResult(model);
        }

        private async Task PrepareAgentsAsync(IList<SelectListItem> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //prepare available agents
            var availableAgentItems = await GetAgentListAsync();
            foreach (var agentItem in availableAgentItems)
            {
                items.Add(agentItem);
            }
        }

        private async Task<List<SelectListItem>> GetAgentListAsync()
        {
            var agents = await _agentService.GetAllAgentsAsync();
            var listItems = agents.Select(a => new SelectListItem
            {
                Text = a.Name,
                Value = a.Id.ToString()
            });

            var result = new List<SelectListItem>();
            //clone the list to ensure that "selected" property is not set
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            return result;
        }

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

                    var currentAgent = agent;
                    int parentAgentId = currentAgent.ParentAgentId;
                    while (parentAgentId > 0)
                    {
                        var parentAgent = await _agentService.GetAgentByIdAsync(parentAgentId);
                        agentModel.Name = $"{parentAgent.Name} >> {agentModel.Name}";

                        parentAgentId = parentAgent.ParentAgentId;
                    }

                    return await Task.FromResult(agentModel);
                });
            });

            return model;
        }

        #endregion
    }
}