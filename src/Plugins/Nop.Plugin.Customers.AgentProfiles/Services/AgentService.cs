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
            var agents = await _agentRepository.GetAllPagedAsync(query =>
            {
                return query;
            });

            return agents;
        }
    }
}
