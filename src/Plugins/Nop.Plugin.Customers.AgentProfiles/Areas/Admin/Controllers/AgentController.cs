﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Factories;
using Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Models.Customers;
using Nop.Plugin.Customers.AgentProfiles.Domains;
using Nop.Plugin.Customers.AgentProfiles.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
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
        private readonly IAgentService _agentService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        public AgentController(IPermissionService permissionService,
                               IAgentModelFactory agentModelFactory,
                               IAgentService agentService,
                               INotificationService notificationService,
                               ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _agentModelFactory = agentModelFactory;
            _agentService = agentService;
            _notificationService = notificationService;
            _localizationService = localizationService;
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

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //prepare model
            var model = await _agentModelFactory.PrepareAgentModelAsync(new AgentModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual async Task<IActionResult> Create(AgentModel model, bool continueEditing, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var agents = await _agentService.GetAllAgentsAsync();
            if (!string.IsNullOrEmpty(model.Name))
            {
                var existingAgent = agents.FirstOrDefault(a => a.Name == model.Name);
                if (existingAgent is not null)
                    ModelState.AddModelError(string.Empty, $"Agent {model.Name} already registered");
            }

            if (ModelState.IsValid)
            {
                //fill entity from model
                var agent = model.ToEntity<Agent>();

                await _agentService.InsertAgentAsync(agent);

                //activity log
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Agents.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = agent.Id });
            }

            //prepare model
            model = await _agentModelFactory.PrepareAgentModelAsync(model, null);

            //if we got this far, something failed, redisplay form
            return View(model);
        }
    }
}
