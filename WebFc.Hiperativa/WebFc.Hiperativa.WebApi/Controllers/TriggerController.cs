using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UtilityFramework.Application.Core;
using UtilityFramework.Application.Core.ViewModels;
using UtilityFramework.Services.Core.Interface;
using UtilityFramework.Services.Iugu.Core;
using UtilityFramework.Services.Iugu.Core.Enums;
using UtilityFramework.Services.Iugu.Core.Models;
// ReSharper disable NotAccessedField.Local
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable HeuristicUnreachableCode
#pragma warning disable 1998

namespace WebFc.Hiperativa.WebApi.Controllers
{

    [Route("api/v1/[controller]")]
    public class TriggerController : Controller
    {

        private readonly ISenderMailService _senderMailService;
        private readonly ISenderNotificationService _senderNotificationService;
        private readonly string _projectName;
        public TriggerController(ISenderMailService senderMailService, ISenderNotificationService senderNotificationService)
        {
            _senderMailService = senderMailService;
            _senderNotificationService = senderNotificationService;
            _projectName = Assembly.GetEntryAssembly().GetName().Name?.Split('.')[0];

        }


        /// <summary>
        /// GATILHOS DA IUGU
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("ChangeStatusGatway")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> ChangeStatusGatway([FromForm] IuguTriggerModel model)
        {
            try
            {
                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                model.SetAllProperties(Request.Form);

                var registerLog = false;

                switch (model.Event)
                {
                    case TriggerEvents.InvoiceReleased:

                        // todo logica de repasse de valores 


                        break;
                    case TriggerEvents.ReferralsVerification:

                        // todo logica pra notificar validação de dados bancários
                        break;
                }

                if (registerLog)
                {
                    var unused = Task.Run(() =>
                   {
                       var json = JsonConvert.SerializeObject(model, new JsonSerializerSettings() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore }).JsonPrettify();

                       _senderMailService.SendMessageEmail("Gatilho", "fabricio@megaleios.com", json,
                           $"Gatilho {_projectName} - {model.Event}");
                   });
                }
                return Ok(Utilities.ReturnSuccess());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}