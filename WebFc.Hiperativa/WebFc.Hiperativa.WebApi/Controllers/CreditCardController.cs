using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using UtilityFramework.Application.Core;
using UtilityFramework.Application.Core.JwtMiddleware;
using UtilityFramework.Application.Core.ViewModels;
using UtilityFramework.Services.Iugu.Core.Entity;
using UtilityFramework.Services.Iugu.Core.Interface;
using UtilityFramework.Services.Iugu.Core.Models;
using WebFc.Hiperativa.Domain;
using WebFc.Hiperativa.Domain.Services.Interface;
using WebFc.Hiperativa.Domain.ViewModels;
using WebFc.Hiperativa.Repository.Interface;

namespace WebFc.Hiperativa.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CreditCardController : Controller
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly bool _isSandBox;
        private readonly IIuguCustomerServices _iuguCustomerServices;
        private readonly IIuguPaymentMethodService _iuguPaymentMethodService;
        private readonly IMapper _mapper;
        private readonly IProfileRepository _profileUserBusiness;
        private readonly IUtilService _utilService;

        public CreditCardController(ICreditCardRepository creditCardRepository,
            IIuguPaymentMethodService iuguPaymentMethodService, IProfileRepository profileUserBusiness,
            IIuguCustomerServices iuguCustomerServices, IMapper mapper, IUtilService utilService)
        {
            _creditCardRepository = creditCardRepository;
            _iuguPaymentMethodService = iuguPaymentMethodService;
            _profileUserBusiness = profileUserBusiness;
            _iuguCustomerServices = iuguCustomerServices;
            _mapper = mapper;
            _utilService = utilService;
            _isSandBox = Utilities.GetConfigurationRoot().GetSection("IUGU:SandBox").Get<bool>();
        }

        /// <summary>
        ///     LISTAR CARTÕES DE CRÉDITO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Get()
        {
            var response = new List<CreditCardViewModel>();
            try
            {
                var userId = Request.GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var entity = await _profileUserBusiness.FindByIdAsync(userId);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound, responseList: true));

                var listCreditCard =
                    await _creditCardRepository.FindByAsync(x => x.ProfileId.Equals(userId)) as List<CreditCard>;

                if (string.IsNullOrEmpty(entity.AccountKey) && _isSandBox == false ||
                    string.IsNullOrEmpty(entity.AccountKeyDev) && _isSandBox)
                    return Ok(Utilities.ReturnSuccess(data: response));

                var listIuguCards =
                    await _iuguPaymentMethodService.ListarCredCardsAsync(_isSandBox
                        ? entity.AccountKeyDev
                        : entity.AccountKey) as List<IuguCreditCard>;

                listCreditCard?.ForEach(card =>
               {
                   var map = _mapper.Map<CreditCardViewModel>(card);

                   var cardIuug = listIuguCards?.Find(x => x.Id == card.Number.ToString());

                   if (cardIuug == null) return;

                   map.Number = cardIuug.Data.DisplayNumber;
                   map.Flag = _utilService.GetFlag(cardIuug.Data.Brand?.ToLower());
                   map.Name = cardIuug.Data.HolderName;
                   map.ExpMonth = cardIuug.Data.Month;
                   map.ExpYear = cardIuug.Data.Year;

                   response.Add(map);
               });

                return Ok(Utilities.ReturnSuccess(data: response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(responseList: true));
            }
        }

        /// <summary>
        ///     DETALHES DO CARTÃO
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Details([FromRoute] string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out var unused) == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidIdentifier));

                var userId = Request.GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var profileEntity = await _profileUserBusiness.FindByIdAsync(userId);

                if (profileEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                var cardEntity = await _creditCardRepository.FindByIdAsync(id).ConfigureAwait(false);

                if (cardEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CreditCardNotFound));

                var creditCardIugu =
                    await _iuguPaymentMethodService.BuscarCredCardsAsync(profileEntity.AccountKey,
                        cardEntity.TokenCard);

                if (creditCardIugu == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CreditCardNotFoundIugu));

                var cardViewModel = _mapper.Map<CreditCardViewModel>(cardEntity);

                if (cardViewModel == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CreditCardNotFound));

                cardViewModel.Number = creditCardIugu.Data.DisplayNumber;
                cardViewModel.Flag = creditCardIugu.Data.Brand;
                cardViewModel.Name = creditCardIugu.Data.HolderName;
                cardViewModel.ExpMonth = creditCardIugu.Data.Month;
                cardViewModel.ExpYear = creditCardIugu.Data.Year;

                return Ok(Utilities.ReturnSuccess(data: cardViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        ///     REGISTRAR CARTÃO DE CREDITO
        /// </summary>
        /// <remarks>
        ///     OBJ DE ENVIO
        ///     POST api/v1/CreditCard
        ///     {
        ///     "name": "string",
        ///     "number": "string", // #### #### #### ####
        ///     "expMonth": 0,
        ///     "expYear": 0, // FORMAT 2019
        ///     "cvv": "string" // MINLENGTH 3 / MAX 4
        ///     }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Register")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Register([FromBody] CreditCardViewModel model)
        {
            try
            {
                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));


                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                if (model.Number?.Length < 14)
                    return BadRequest(Utilities.ReturnErro("Número de cartão inválido."));
                if (model.ExpMonth >= 12 || model.ExpMonth <= 1)
                    return BadRequest(Utilities.ReturnErro("Mês de vencimento inválido."));
                if (model.ExpYear <= DateTime.Now.Year)
                    return BadRequest(Utilities.ReturnErro("Ano de vencimento inválido."));
                if (model.Cvv?.Length < 3)
                    return BadRequest(Utilities.ReturnErro("Código de segurança inválido."));

                var entity = await _profileUserBusiness.FindByIdAsync(userId);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));


                if (_isSandBox == false && string.IsNullOrEmpty(entity.AccountKey) ||
                    _isSandBox && string.IsNullOrEmpty(entity.AccountKeyDev))
                {
                    var iuguCustomerResponse = await _iuguCustomerServices.SaveClientAsync(new IuguCustomerCreated
                    {
                        Email = entity.Email,
                        Name = $"{entity.FullName}"
                    });

                    if (iuguCustomerResponse.HasError)
                        return BadRequest(Utilities.ReturnErro(iuguCustomerResponse.MessageError));

                    if (_isSandBox)
                        entity.AccountKeyDev = iuguCustomerResponse.Id;
                    else
                        entity.AccountKey = iuguCustomerResponse.Id;

                    entity = _profileUserBusiness.Update(entity);
                }

                var tokenCardIugu = _iuguPaymentMethodService.SaveCreditCard(new IuguPaymentMethodToken
                {
                    Method = "credit_card",
                    Test = "false",
                    Data = new IuguDataPaymentMethodToken
                    {
                        FirstName = model.Name.GetFirstName(),
                        LastName = model.Name.GetLastName(),
                        Number = model.Number,
                        Month = $"{model.ExpMonth:00}",
                        VerificationValue = model.Cvv.OnlyNumbers(),
                        Year = $"{model.ExpYear}"
                    }
                });

                if (!string.IsNullOrEmpty(tokenCardIugu.MessageError))
                    return BadRequest(Utilities.ReturnErro(tokenCardIugu.MessageError));

                var iuguCreditCardResponse = _iuguPaymentMethodService.LinkCreditCardClient(
                    new IuguCustomerPaymentMethod
                    {
                        CustomerId = entity.AccountKey,
                        Description = $"Meu {entity.CreditCards.Count + 1} Cartão de credito",
                        SetAsDefault = (!entity.CreditCards.Any()).ToString(),
                        Token = tokenCardIugu.Id
                    }, _isSandBox ? entity.AccountKeyDev : entity.AccountKey);


                if (iuguCreditCardResponse.HasError)
                    return BadRequest(Utilities.ReturnErro(iuguCreditCardResponse.MessageError));

                    var creditCardId = _creditCardRepository.Create(new Data.Entities.CreditCard
                    {
                        ProfileId = userId,
                        TokenCard = iuguCreditCardResponse.Id
                    });

                    entity.CreditCards.Add(creditCardId);

                await _profileUserBusiness.UpdateAsync(entity);

                return Ok(Utilities.ReturnSuccess("Registrado com sucesso"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }


        /// <summary>
        ///     REMOVER CARTÃO DE CRÉDITO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out var unused) == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidIdentifier));


                var userId = Request.GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var entity = await _profileUserBusiness.FindByIdAsync(userId);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                var creditCardLoad = await _creditCardRepository.FindByIdAsync(id).ConfigureAwait(false);

                if (creditCardLoad == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CreditCardNotFound));

                await _iuguPaymentMethodService.RemoverCredCardAsync(
                    _isSandBox ? entity.AccountKeyDev : entity.AccountKey, creditCardLoad.TokenCard);

                _creditCardRepository.DeleteOne(id);

                entity.CreditCards.Remove(id);
                await _profileUserBusiness.UpdateAsync(entity);


                return Ok(Utilities.ReturnSuccess());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}