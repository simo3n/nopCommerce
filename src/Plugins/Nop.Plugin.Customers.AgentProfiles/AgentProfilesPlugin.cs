using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Customers.AgentProfiles
{
    public class AgentProfilesPlugin : BasePlugin, IAdminMenuPlugin
    {
        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;

        public AgentProfilesPlugin(ICustomerService customerService, IPermissionService permissionService, ILocalizationService localizationService)
        {
            _customerService = customerService;
            _permissionService = permissionService;
            _localizationService = localizationService;
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

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return;

            var customersNode = rootNode.ChildNodes.FirstOrDefault(n => n.SystemName == "Customers");
            if (customersNode is not null)
            {
                var agentsNode = new SiteMapNode()
                {
                    SystemName = "Agents",
                    Title = await _localizationService.GetResourceAsync("Admin.Customers.Agents"),
                    ControllerName = "Agents",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary() { { "area", AreaNames.Admin } },
                    IconClass = "far fa-user",
                    Visible = true,
                };

                customersNode.ChildNodes.Add(agentsNode);
            }

            //string systemName = "Agents";

            //var menuItem = new SiteMapNode()
            //{
            //    SystemName = systemName,
            //    Title = "Agenti",
            //    IconClass = "far fa-user",
            //    Visible = true,
            //};

            //var subItem1 = new SiteMapNode()
            //{
            //    SystemName = "Provvigioni",
            //    Title = "Regole prov",
            //    ControllerName = "Provvigioni",
            //    ActionName = "List",
            //    IconClass = "far fa-dot-circle",
            //    Visible = true,
            //    RouteValues = new RouteValueDictionary() { { "area", AreaNames.Admin } }
            //};

            //var subItem2 = new SiteMapNode()
            //{
            //    SystemName = "Ordini clienti",
            //    Title = "Ordini clienti",
            //    ControllerName = "Provvigioni",
            //    ActionName = "List",
            //    IconClass = "far fa-dot-circle",
            //    Visible = true,
            //    RouteValues = new RouteValueDictionary() { { "area", AreaNames.Admin } }
            //};

            //menuItem.ChildNodes.Add(subItem1);
            //menuItem.ChildNodes.Add(subItem2);

            //var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == systemName);

            //if (pluginNode != null)
            //    pluginNode.ChildNodes.Add(menuItem);
            //else
            //    rootNode.ChildNodes.Add(menuItem);

            await Task.CompletedTask;
        }
    }
}
