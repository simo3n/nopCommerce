using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Customers.AgentProfiles.Domains;

namespace Nop.Plugin.Customers.AgentProfiles.Mapping
{
    public partial class NameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            { typeof(AddressAgentMapping), "Address_Agent_Mapping" }
        };

        public Dictionary<(Type, string), string> ColumnName => new()
        {
            { (typeof(AddressAgentMapping), "AddressId"), "Address_Id" },
            { (typeof(AddressAgentMapping), "AgentId"), "Agent_Id" },
        };
    }
}