﻿using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using HttpArchivesService.Features.HttpArchives.RenameHarFiles;
using HttpArchivesService.Features.HttpArchives.UploadHarFiles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HttpArchivesService.Features.HttpArchives.GetHarById;
using HttpArchivesService.Features.HttpArchives.GetHarsByCurrentUser;
using HttpArchivesService.Features.HttpArchives.ChangeHarFilesDirectories;

namespace HttpArchivesService.Controllers
{

    [ApiController]
    [Route("http-archives")]
    public class HttpArchivesController : Controller
    {
        private readonly IMediator _mediator;

        public HttpArchivesController(IMediator mediator)
        {
            this._mediator = mediator;
        }

        [Authorize]
        [HttpGet]
        [Route("get-by-current-user")]
        public async Task<ActionResult<GetHarsResponseDto>> GetUserSavedHARs()
        {
            var request = new GetHarsByCurrentUser.GetHarsRequest();
            return await this._mediator.Send(request);
        }

        [Authorize]
        [HttpGet]
        [Route("get-by-id")]
        public async Task<ActionResult<HarResponseDto>> GetdHARsById([Required][FromQuery] GetHarById.GetHarByIdRequest request)
        {
            return await this._mediator.Send(request);
        }

        [Authorize]
        [HttpPost]
        [Route("save-for-current-user")]
        public async Task<ActionResult<HarUploadResponseDto>> SaveHARsForUser([Required] IFormFile[] files)
        {
            var request = new UploadHarFilessFeature.HarUploadRequestDto
            {
                FormFiles = files
            };

            return await this._mediator.Send(request);
        }

        [Authorize]
        [HttpPost]
        [Route("move-multiple")]
        public async Task<ActionResult<HarUploadResponseDto>> MoveHARs([Required] ChangeHarFilesDirectories.ChangeHarFilesDirectoriesRequest request)
        {
            await this._mediator.Send(request);

            return Ok();
        }

        [Authorize]
        [HttpPost]
        [Route("rename-multiple")]
        public async Task<IActionResult> RenameHARsForUser([Required] RenameHarFilesDirectories.RenameHarsRequest renameRequests)
        {
            await this._mediator.Send(renameRequests);

            return Ok();
        }
    }
}