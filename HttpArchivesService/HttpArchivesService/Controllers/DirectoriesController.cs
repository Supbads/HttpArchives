using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HttpArchivesService.Features.Directories.CreateDirectory;
using HttpArchivesService.Features.Directories.GetDirectories;
using HttpArchivesService.Features.Directories.RenameDirectories;
using Microsoft.AspNetCore.Http;

namespace HttpArchivesService.Controllers
{
    [ApiController]
    [Route("directories")]
    public class DirectoriesController : Controller
    {
        private readonly IMediator _mediator;

        public DirectoriesController(IMediator mediator)
        {
            this._mediator = mediator;
        }

        [Authorize]
        [HttpGet]
        [Route("get-by-current-user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GetDirectoriesResponseDto>> GetDirectoriesByCurrentUser()
        {
            var request = new GetDirectoriesByCurrentUser.GetDirectoriesRequest();
            var result = await this._mediator.Send(request);

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<CreateDirectoryResponseDto>> CreateDirectoryForCurrentUser([Required] CreateDirectoryFeature.CreateDirectoryRequestDto request)
        {
            var result = await this._mediator.Send(request);

            return this.Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Route("rename-multiple")]
        public async Task<ActionResult<string>> RenameDirecotries([Required] RenameDirectories.RenameDirectoriesRequest request)
        {
            var result = await this._mediator.Send(request);

            return this.Ok(result);
        }

        [Authorize]
        [HttpDelete]
        [Route("delete")]
        public async Task<ActionResult<string>> DeleteDirectory([Required] string request)
        {
            var result = await this._mediator.Send(request);

            return this.Ok(result);
        }

    }
}