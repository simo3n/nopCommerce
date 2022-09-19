using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Customers.AgentProfiles.Domains;

namespace Nop.Plugin.Customers.AgentProfiles.Migrations
{
    [NopMigration("2022-09-19 15:31:00", "AgentProfiles initial migration", MigrationProcessType.NoMatter)]
    public class InitialMigration : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public InitialMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            Create.TableFor<Agent>();
            Create.TableFor<AddressAgentMapping>();
        }
    }
}
