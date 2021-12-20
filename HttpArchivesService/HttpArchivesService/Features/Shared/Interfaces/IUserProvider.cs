using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace HttpArchivesService.Features.Shared.Interfaces
{
    public interface IUserProvider
    {
        Task<IdentityUser> GetCurrentUserExplicit();
    }
}
