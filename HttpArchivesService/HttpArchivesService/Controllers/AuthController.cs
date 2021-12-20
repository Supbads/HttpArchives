﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using HttpArchivesService.Features.Shared.Exceptions;

namespace HttpArchivesService.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AuthController(
            IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._userManager = userManager;
            this._signInManager = signInManager;
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await this._userManager.FindByNameAsync(username);

            if (user == null)
            {
                throw new UserFriendlyException(StatusCodes.Status400BadRequest, $"A user with the username: {username} does not exist");
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

            if (!result.Succeeded)
            {
                throw new UserFriendlyException(StatusCodes.Status400BadRequest, $"The username and password don't match for user: {username}");
            }

            return Ok();
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(string username, string password)
        {
            var identityUser = new IdentityUser(username);
            var result = await this._userManager.CreateAsync(identityUser, password);

            if (!result.Succeeded)
            {
                var errorMessage = BuildIdentityResultErrorMessage(result);
                throw new UserFriendlyException(StatusCodes.Status400BadRequest, $"Could not register user {username}. Errors: {errorMessage}");

            }

            return Ok();
        }

        [HttpPost]
        [Route("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout()
        {
            await this._signInManager.SignOutAsync();
            await this._httpContextAccessor.HttpContext.SignOutAsync();

            return Ok();
        }

        private string BuildIdentityResultErrorMessage(IdentityResult validationResult)
        {
            var errors = validationResult.Errors
                 .Select(x => x.Description)
                 .ToArray();
            return string.Join(", ", errors);
        }
    }
}