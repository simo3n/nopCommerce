using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Customers.AgentProfiles.Domains;

namespace Nop.Plugin.Customers.AgentProfiles.Services
{
    public class AgentService : IAgentService
    {
        private readonly IRepository<Agent> _agentRepository;

        public AgentService(IRepository<Agent> agentRepository)
        {
            _agentRepository = agentRepository;
        }

        public async Task<IPagedList<Agent>> GetAllAgentsAsync()
        {
            var agents = await _agentRepository.GetAllPagedAsync(query => query);
            return agents;
        }

        /// <summary>
        /// Gets an agent
        /// </summary>
        /// <param name="agentId">Agent identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the agent
        /// </returns>
        public async Task<Agent> GetAgentByIdAsync(int agentId)
        {
            return await _agentRepository.GetByIdAsync(agentId);
        }

        /// <summary>
        /// Gets all agents filtered by parent agent identifier
        /// </summary>
        /// <param name="parentAgentId">Parent agent identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the agents
        /// </returns>
        public async Task<IList<Agent>> GetAllAgentsByParentAgentIdAsync(int parentAgentId)
        {
            var agents = await _agentRepository.GetAllAsync(query =>
            {
                query = query.Where(a => a.ParentAgentId == parentAgentId);
                return query.OrderBy(c => c.Id);
            });

            return agents;
        }

        /// <summary>
        /// Insert an agent
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertAgentAsync(Agent agent)
        {
            await _agentRepository.InsertAsync(agent);
        }

        /// <summary>
        /// Updates the agent
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateAgentAsync(Agent agent)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));

            //validate agent hierarchy
            var parentAgent = await GetAgentByIdAsync(agent.ParentAgentId);
            while (parentAgent != null)
            {
                if (agent.Id == parentAgent.Id)
                {
                    agent.Id = 0;
                    break;
                }

                parentAgent = await _agentRepository.GetByIdAsync(parentAgent.ParentAgentId);
            }

            await _agentRepository.UpdateAsync(agent);
        }

        /// <summary>
        /// Delete agent
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteAgentAsync(Agent agent)
        {
            await _agentRepository.DeleteAsync(agent);

            //reset a "Parent category" property of all child subcategories
            var subagents = await GetAllAgentsByParentAgentIdAsync(agent.Id);
            foreach (var subagent in subagents)
            {
                subagent.ParentAgentId = 0;
                await UpdateAgentAsync(subagent);
            }
        }
    }
}
