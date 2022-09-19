using Nop.Core;

namespace Nop.Plugin.Customers.AgentProfiles.Domains
{
    public partial class Agent : BaseEntity
    {
        public string Name { get; set; }
        public int ParentAgentId { get; set; }
    }
}
