using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Common;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Customers.AgentProfiles.Domains;

namespace Nop.Plugin.Customers.AgentProfiles.Mapping.Builders
{
    /// <summary>
    /// Represents a address agent mapping entity builder
    /// </summary>
    public partial class AddressAgentMappingBuilder : NopEntityBuilder<AddressAgentMapping>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(AddressAgentMapping), nameof(AddressAgentMapping.AddressId)))
                    .AsInt32().ForeignKey<Address>().PrimaryKey()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(AddressAgentMapping), nameof(AddressAgentMapping.AgentId)))
                    .AsInt32().ForeignKey<Agent>().PrimaryKey();
        }

        #endregion
    }
}
