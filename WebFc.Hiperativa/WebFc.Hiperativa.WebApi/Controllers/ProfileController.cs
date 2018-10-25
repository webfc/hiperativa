using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using UtilityFramework.Application.Core;
using UtilityFramework.Application.Core.JwtMiddleware;
using UtilityFramework.Application.Core.ViewModels;
using UtilityFramework.Services.Core.Interface;
using WebFc.Hiperativa.Domain;
using WebFc.Hiperativa.Domain.ViewModels;
using WebFc.Hiperativa.Repository.Interface;

namespace WebFc.Hiperativa.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class ProfileController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IProfileRepository _profileRepository;
        private readonly ISenderMailService _senderMailService;

        public ProfileController(IMapper mapper, IProfileRepository profileRepository, ISenderMailService senderMailService)
        {
            _mapper = mapper;
            _profileRepository = profileRepository;
            _senderMailService = senderMailService;
        }


        /// <summary>
        /// DETALHES DO USUÁRIO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("Detail/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Detail([FromRoute] string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out var unused) == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var entity = await _profileRepository.FindByIdAsync(id).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<ProfileViewModel>(entity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// INFORMAÇÕES  DO USUÁRIO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetInfo")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetInfo()
        {
            try
            {
                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var entity = await _profileRepository.FindByIdAsync(userId).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                if (entity.DataBlocked != null)
                    return StatusCode((int)HttpStatusCode.Forbidden, new ReturnViewModel()
                    {
                        Erro = true,
                        Message = $"Usuário bloqueado,motivo {entity.Reason ?? "não informado"}"
                    });

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<ProfileViewModel>(entity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// REMOVER PERFIL
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "id":"string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Delete")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete([FromBody] BaseViewModel model)
        {
            try
            {
                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                if (ObjectId.TryParse(model.Id, out var unused) == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                await _profileRepository.DeleteOneAsync(model.Id).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess("Removido com sucesso"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }


        /// <summary>
        /// REMOVER PERFIL BY E-MAIL
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "email":"string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("DeleteByEmail")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteByEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || email.ValidEmail() == false)
                    return BadRequest(Utilities.ReturnErro("Informe um e-mail valido"));

                await _profileRepository.DeleteAsync(x => x.Email == email.ToLower()).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess("Removido com sucesso"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// REGISTRAR USUÁRIO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "":"string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Register")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Register([FromBody] ProfileRegisterViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                if (string.IsNullOrEmpty(model.FacebookId))
                {
                    if (await _profileRepository.CheckByAsync(x => x.FacebookId == model.FacebookId))
                        return BadRequest(Utilities.ReturnErro(DefaultMessages.FacebookInUse));

                    model.Login = model.Email.ToLower();
                }
                if (model.Cpf.ValidCpf() == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CpfInvalid));

                if (await _profileRepository.CheckByAsync(x => x.Cpf == model.Cpf.OnlyNumbers()).ConfigureAwait(false))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CpfInUse));

                if (await _profileRepository.CheckByAsync(x => x.Login == model.Login).ConfigureAwait(false))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.LoginInUse));

                if (await _profileRepository.CheckByAsync(x => x.Email == model.Email.ToLower()).ConfigureAwait(false))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.EmailInUse));

                var entity = _mapper.Map<Data.Entities.Profile>(model);

                var entityId = await _profileRepository.CreateAsync(entity).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess(data: TokenProviderMiddleware.GenerateToken(entityId)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// LOGIN
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "login":"string", //optional
        ///              "password":"string", //optional
        ///              "facebookId":"string", //optional
        ///              "googleId":"string" //optional
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Token")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Token([FromBody] LoginViewModel model)
        {
            try
            {
                model.TrimStringProperties();

                if (!string.IsNullOrEmpty(model.RefreshToken))
                {
                    var token = new JwtSecurityToken(Regex.Replace(model.RefreshToken, "bearer", "", RegexOptions.IgnoreCase).Trim());

                    if (!string.IsNullOrEmpty(token?.Subject))
                        return Ok(Utilities.ReturnSuccess(data: TokenProviderMiddleware.GenerateToken(token?.Subject)));

                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));
                }


                Data.Entities.Profile entity;
                if (string.IsNullOrEmpty(model.FacebookId) == false)
                {
                    entity = await _profileRepository.FindOneByAsync(x => x.FacebookId == model.FacebookId)
                        .ConfigureAwait(false);


                    if (entity == null)
                        return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));
                }
                else
                {
                    model.TrimStringProperties();
                    var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.Email));

                    if (isInvalidState != null)
                        return BadRequest(isInvalidState);

                    entity = await _profileRepository
                       .FindOneByAsync(x => x.Login == model.Login && x.Password == model.Password).ConfigureAwait(false);

                    if (entity == null)
                        return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidLogin));

                }


                if (entity.DataBlocked != null)
                    return BadRequest(Utilities.ReturnErro($"Usuário bloqueado : {entity.Reason}"));

                return Ok(Utilities.ReturnSuccess(data: TokenProviderMiddleware.GenerateToken(entity._id.ToString())));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// BLOQUEAR / DESBLOQUEAR USUÁRIO - ADMIN
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "id": "string", // required
        ///              "block": true,
        ///              "reason": "" //motivo de bloquear o usuário
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("BlockUnBlock")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> BlockUnBlock([FromBody] BlockViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.TargetId));

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var entity = await _profileRepository.FindByIdAsync(model.TargetId);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                entity.DataBlocked = model.Block ? DateTimeOffset.Now.ToUnixTimeSeconds() : (long?)null;
                entity.Reason = model.Block ? model.Reason : null;

                await _profileRepository.UpdateAsync(entity);

                return Ok(Utilities.ReturnSuccess(model.Block ? "Bloqueado com sucesso" : "Desbloqueado com sucesso"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// ALTERAR PASSWORD
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "currentPassword":"string",
        ///              "newPassword":"string",
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("ChangePassword")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            try
            {
                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro());

                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var entity = await _profileRepository.FindByIdAsync(userId).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                if (!entity.Password.Equals(model.CurrentPassword))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.PasswordNoMatch));

                entity.LastPassword = entity.Password;
                entity.Password = model.NewPassword;

                _profileRepository.Update(entity);

                return Ok(Utilities.ReturnSuccess("Senha alterada com sucesso."));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// METODO DE LISTAGEM COM DATATABLE - TODOS 
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("LoadData")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(typeof(ProfileViewModel), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> LoadData([FromForm] DtParameters model)
        {
            var response = new DtResult<ProfileViewModel>();

            try
            {
                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var totalRecords = await _profileRepository.CountAsync(x => x.Created != null);

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                    ? Builders<Data.Entities.Profile>.Sort.Descending(sortColumn)
                    : Builders<Data.Entities.Profile>.Sort.Ascending(sortColumn);

                var retorno = await _profileRepository
                    .LoadDataTableAsync(x => x.Created != null, model.Search.Value, sortBy, model.Start, model.Length, columns);

                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                    ? (int)await _profileRepository.CountSearchDataTableAsync(x => x.Created != null, model.Search.Value, columns)
                    : totalRecords;

                response.Data = _mapper.Map<List<ProfileViewModel>>(retorno);
                response.Draw = model.Draw;
                response.RecordsFiltered = totalrecordsFiltered;
                response.RecordsTotal = totalRecords;

                return Ok(response);

            }
            catch (Exception)
            {
                return Ok(response);
            }
        }


        /// <summary>
        /// REGISTRAR E REMOVER DEVICE ID ONESIGNAL OU FCM | CHAMAR APOS LOGIN E LOGOUT
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "id":"string"
        ///              "isRegister":true 
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("RegisterUnRegisterDeviceId")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public IActionResult RegisterUnRegisterDeviceId([FromBody] PushViewModel model)
        {
            try
            {
                var userId = Request.GetUserId();

                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                Task.Run(() =>
               {
                   if (model.IsRegister)
                   {
                       _profileRepository.UpdateMultiple(Query<Data.Entities.Profile>.Where(x => x._id == ObjectId.Parse(userId)),
                           new UpdateBuilder<Data.Entities.Profile>().AddToSet(x => x.DeviceId, model.DeviceId), UpdateFlags.None);
                   }
                   else
                   {
                       _profileRepository.UpdateMultiple(Query<Data.Entities.Profile>.Where(x => x._id == ObjectId.Parse(userId)),
                           new UpdateBuilder<Data.Entities.Profile>().Pull(x => x.DeviceId, model.DeviceId), UpdateFlags.None);
                   }
               });

                return Ok(Utilities.ReturnSuccess());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }


        /// <summary>
        /// UPDATE DE USUÁRIO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "fullName":"string",
        ///              "email":"string",
        ///              "photo":"string", //filename
        ///              "phone":"string", //(##) #####-####
        ///              "login":"string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Update")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Update([FromBody] ProfileRegisterViewModel model)
        {
            try
            {

                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                if (await _profileRepository.CheckByAsync(x => x.Email == model.Email.ToLower() && x._id != ObjectId.Parse(model.Email)))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.EmailInUse));


                if (await _profileRepository.CheckByAsync(x => x.Login == model.Login && x._id != ObjectId.Parse(model.Login)))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.LoginInUse));

                var profileEntity = await _profileRepository.FindByIdAsync(userId).ConfigureAwait(false);

                profileEntity.Email = model.Email;
                profileEntity.FullName = model.FullName;
                profileEntity.Phone = model.Phone?.OnlyNumbers();
                profileEntity.Login = model.Login;
                profileEntity.Photo = model.Photo.RemovePathImage().SetPhotoProfile(profileEntity.FacebookId);

                profileEntity = await _profileRepository.UpdateAsync(profileEntity).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<ProfileViewModel>(profileEntity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// ESQUECI A SENHA
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "email":"string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ForgotPassword([FromBody] LoginViewModel model)
        {
            try
            {
                model?.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.Email));

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var profile = await _profileRepository.FindOneByAsync(x => x.Email.Equals(model.Email)).ConfigureAwait(false);

                if (profile == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileUnRegistred));

                var dataBody = new Dictionary<string, string>();

                var newPassword = Utilities.RandomInt(8);


                dataBody.Add("{{title}}", "Lembrete de senha");
                dataBody.Add("{{message}}", $"<p>Caro(a) {profile.FullName.GetFirstName()}</p>" +
                                            $"<p>Segue sua senha de acesso ao {Startup.ApplicationName}</p>" +
                                            $"<p><b>Login</b> : {profile.Login}</p>" +
                                            $"<p><b>Senha</b> :{newPassword}</p>");

                var body = _senderMailService.GerateBody("custom", dataBody);

                var unused = Task.Run(async () =>
                {
                    await _senderMailService.SendMessageEmailAsync(Startup.ApplicationName, profile.Email, body, "Lembrete de senha").ConfigureAwait(false);

                    profile.Password = newPassword;
                    await _profileRepository.UpdateAsync(profile);

                });

                return Ok(Utilities.ReturnSuccess("Verifique seu e-mail"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// VERIFICAR SE E-MAIL ESTÁ EM USO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///             POST
        ///             {
        ///                 "email":"string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("CheckEmail")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CheckEmail([FromBody] ValidationViewModel model)
        {
            try
            {
                model?.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.Email));

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                if (await _profileRepository.CheckByAsync(x => x.Email.Equals(model.Email.ToLower())).ConfigureAwait(false))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.EmailInUse));

                return Ok(Utilities.ReturnSuccess("Disponível"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }


        /// <summary>
        /// VERIFICAR SE E-MAIL ESTÁ EM USO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///             POST
        ///             {
        ///                 "login":"string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("CheckLogin")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CheckLogin([FromBody] ValidationViewModel model)
        {
            try
            {
                model?.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.Email));

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                if (await _profileRepository.CheckByAsync(x => x.Login.Equals(model.Login)).ConfigureAwait(false))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.LoginInUse));

                return Ok(Utilities.ReturnSuccess("Disponível"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }


    }
}