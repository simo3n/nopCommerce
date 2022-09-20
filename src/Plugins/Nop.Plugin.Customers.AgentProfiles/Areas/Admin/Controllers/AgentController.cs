using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Factories;
using Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Models.Customers;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class AgentController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IAgentModelFactory _agentModelFactory;

        public AgentController(IPermissionService permissionService, IAgentModelFactory agentModelFactory)
        {
            _permissionService = permissionService;
            _agentModelFactory = agentModelFactory;
        }

        #region List

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //prepare model
            var model = await _agentModelFactory.PrepareAgentSearchModelAsync(new AgentSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(AgentSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _agentModelFactory.PrepareAgentListModelAsync(searchModel);

            return Json(model);
        }

        #endregion
    }
}
