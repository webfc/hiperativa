using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UtilityFramework.Application.Core;
using UtilityFramework.Application.Core.ViewModels;
// ReSharper disable UnusedMember.Local

namespace WebFc.Hiperativa.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/File")]
    public class FileController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly List<string> _acceptedFiles;
        public FileController(IHostingEnvironment env)
        {
            _env = env;
            _acceptedFiles = new List<string>() { ".png", ".jpeg", ".jpg", ".gif" };

        }

        /// <summary>
        /// Upload one file
        /// </summary>
        /// <remarks>
        /// 
        ///     POST api/v1/profile/Upload
        ///                       
        ///         content-type: multipart/form-data
        ///         file = archive  
        ///     
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Upload")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string path = "")
        {

            object response = null;
            try
            {
                if (file == null)
                    return BadRequest(Utilities.ReturnErro("Nenhuma arquivo encontrado"));

                if (file.Length <= 0)
                    return BadRequest(Utilities.ReturnErro("Nenhuma arquivo encontrado"));

                var totalMb = ConvertBytesToMegabytes(file.Length);

                if (totalMb > 3)
                    return BadRequest(Utilities.ReturnErro("Arquivo muito grande"));


                if (!_acceptedFiles.Contains(Path.GetExtension(file.FileName).ToLower().Trim()))
                    return BadRequest(Utilities.ReturnErro("Arquivo não permitido!"));

                var folder = $"{_env.ContentRootPath}\\content\\upload\\{path}".Trim();

                var exists = Directory.Exists(folder);

                if (!exists)
                    Directory.CreateDirectory(folder);

                var arquivo = Utilities.GetUniqueFileName(Path.GetFileNameWithoutExtension(DateTime.Now.ToString("yy-MM-dd-HH-mm-ss")), folder, Path.GetExtension(file.FileName)).ToLower();

                if (!_acceptedFiles.Contains(Path.GetExtension(file.FileName).ToLower().Trim()))
                    return BadRequest(Utilities.ReturnErro("Arquivo não permitido!"));

                arquivo = $"{arquivo}{Path.GetExtension(file.FileName)}";

                var pathFile = Path.Combine(folder, arquivo);

                using (var destinationStream = System.IO.File.Create(pathFile))
                {
                    await file.CopyToAsync(destinationStream);
                }

                response = new
                {
                    fileName = arquivo
                };


                return Ok(Utilities.ReturnSuccess(data: response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(data: response));
            }
        }

        /// <summary>
        /// UPLOAD FILES 
        /// </summary>
        /// <remarks>
        /// 
        ///     POST api/v1/profile/aploads
        ///                       
        ///         content-type: multipart/form-data
        ///         files = archive  
        ///         files = archive
        ///         path = foldername
        ///     
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Uploads")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Uploads([FromForm] List<IFormFile> files, [FromForm] string path = "")
        {
            object response = null;
            try
            {
                if (!files.Any()) return BadRequest(Utilities.ReturnErro("Nenhuma arquivo encontrado"));
                var filesNames = new List<string>();

                if (files.Any(file => _acceptedFiles.Contains(Path.GetExtension(file.FileName).ToLower().Trim())))
                    return BadRequest(Utilities.ReturnErro("Arquivo não permitido!"));
                if (files.Any(file => ConvertBytesToMegabytes(file.Length) > 3))
                    return BadRequest(Utilities.ReturnErro("Um ou mais arquivos muito grande"));

                foreach (var fileName in files)
                {
                    if (fileName.Length <= 0) continue;

                    var totalMb = ConvertBytesToMegabytes(fileName.Length);

                    if (totalMb > 3)
                        return BadRequest(Utilities.ReturnErro("Arquivo muito grande"));

                    var folder = $"{_env.ContentRootPath}\\content\\upload\\{path}".Trim();

                    var exists = Directory.Exists(folder);

                    if (!exists)
                        Directory.CreateDirectory(folder);

                    var arquivo = Utilities.GetUniqueFileName(Path.GetFileNameWithoutExtension(DateTime.Now.ToString("yy-MM-dd-HH-mm-ss")), folder, Path.GetExtension(fileName.FileName)).ToLower();

                    arquivo = $"{arquivo}{Path.GetExtension(fileName.FileName)}";

                    var pathFile = Path.Combine(folder, arquivo);

                    using (var destinationStream = System.IO.File.Create(pathFile))
                    {
                        await fileName.CopyToAsync(destinationStream);
                    }

                    filesNames.Add(arquivo);
                }

                response = new
                {
                    fileNames = filesNames
                };

                return Ok(Utilities.ReturnSuccess(data: response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(data: response));
            }
        }

        private static double ConvertBytesToMegabytes(long bytes) => (bytes / 1024f) / 1024f;

        private static double ConvertKilobytesToMegabytes(long kilobytes) => kilobytes / 1024f;
    }
}