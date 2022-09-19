using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Plugins;
using Nop.Web.Framework;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Customers.AgentProfiles
{
    public class AgentProfilesPlugin : BasePlugin, IAdminMenuPlugin
    {
        private readonly ICustomerService _customerService;

        public AgentProfilesPlugin(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public override async Task InstallAsync()
        {
            var agentCustomerRole = await _customerService.GetCustomerRoleBySystemNameAsync("Agents");
            if (agentCustomerRole is null)
            {
                agentCustomerRole = new CustomerRole()
                {
                    SystemName = "Agents",
                    Name = "Agents",
                    Active = true
                };

                await _customerService.InsertCustomerRoleAsync(agentCustomerRole);
            }

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            var agentCustomerRole = await _customerService.GetCustomerRoleBySystemNameAsync("Agents");
            if (agentCustomerRole is not null)
                await _customerService.DeleteCustomerRoleAsync(agentCustomerRole);

            await base.UninstallAsync();
        }

        public Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            string systemName = "Agents";

            var menuItem = new SiteMapNode()
            {
                SystemName = systemName,
                Title = "Agenti",
                IconClass = "far fa-user",
                Visible = true,
            };

            var subItem1 = new SiteMapNode()
            {
                SystemName = "Provvigioni",
                Title = "Provvigioni",
                ControllerName = "Provvigioni",
                ActionName = "List",
                IconClass = "far fa-dot-circle",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "area", AreaNames.Admin } }
            };

            var subItem2 = new SiteMapNode()
            {
                SystemName = "Ordini clienti",
                Title = "Ordini clienti",
                ControllerName = "Provvigioni",
                ActionName = "List",
                IconClass = "far fa-dot-circle",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "area", AreaNames.Admin } }
            };

            menuItem.ChildNodes.Add(subItem1);
            menuItem.ChildNodes.Add(subItem2);

            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == systemName);

            if (pluginNode != null)
                pluginNode.ChildNodes.Add(menuItem);
            else
                rootNode.ChildNodes.Add(menuItem);

            return Task.CompletedTask;
        }
    }
}
