using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Customers.AgentProfiles.Domains;

namespace Nop.Plugin.Customers.AgentProfiles.Services
{
    public interface IAgentService
    {
        Task<IPagedList<Agent>> GetAllAgentsAsync();
    }
}