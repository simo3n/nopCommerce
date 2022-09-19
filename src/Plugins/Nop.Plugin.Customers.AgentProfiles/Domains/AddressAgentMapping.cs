using Nop.Core;

namespace Nop.Plugin.Customers.AgentProfiles.Domains
{
    public class AddressAgentMapping : BaseEntity
    {
        public int AddressId { get; set; }
        public int AgentId { get; set; }
    }
}
