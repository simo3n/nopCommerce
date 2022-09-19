using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Customers.AgentProfiles.Domains;

namespace Nop.Plugin.Customers.AgentProfiles.Mapping.Builders
{
    public class AgentBuilder : NopEntityBuilder<Agent>
    {
        #region Methods

        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Agent.Name)).AsString(100).NotNullable();
        }

        #endregion
    }
}