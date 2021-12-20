using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using HttpArchivesService.Features.Shared.Interfaces;
using HttpArchivesService.Features.Shared.Exceptions;

namespace HttpArchivesService.Features.Shared
{
    public class UserProvider : IUserProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;
        
        public UserProvider(
            IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._userManager = userManager;
        }

        public async Task<IdentityUser> GetCurrentUserExplicit()
        {
            var user = this._httpContextAccessor.HttpContext.User;
            var identityUser = await this._userManager.GetUserAsync(user);

            if(identityUser == null)
            {
                throw new UserFriendlyException(StatusCodes.Status401Unauthorized, "Action requires the user to be signed in");
            }

            return identityUser;
        }
    }
}