using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using HttpArchivesService.Data;
using HttpArchivesService.Data.Entities;
using HttpArchivesService.Features.HttpArchives.PreviewHarById.PreviewModels;
using HttpArchivesService.Features.Shared.Exceptions;
using HttpArchivesService.Features.Shared.Interfaces;
using MediatR;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Web;

namespace HttpArchivesService.Features.HttpArchives.PreviewHarById
{
    public class PreviewHarById
    {
        public class PreviewHarByIdRequest : IRequest<HarPreview>
        {
            public int Id { get; set; }
        }

        public class PreviewHarByIdHandle : IRequestHandler<PreviewHarByIdRequest, HarPreview>
        {
            private readonly AppDbContext _context;
            private readonly IUserProvider _userProvider;

            public PreviewHarByIdHandle(
                AppDbContext context,
                IUserProvider userProvider)
            {
                this._context = context;
                this._userProvider = userProvider;
            }

            public async Task<HarPreview> Handle(PreviewHarByIdRequest request, CancellationToken cancellationToken)
            {
                var user = await this._userProvider.GetCurrentUserExplicit();

                var har = await this._context.HttpArchiveRecords.FindAsync(request.Id);
                ValidateUserOwnsTheHar(request, user, har);

                var contentAsStr = Encoding.UTF8.GetString(har.Content);

                //contentAsStr = HttpUtility.JavaScriptStringEncode(contentAsStr);

                var preview = JsonSerializer.Deserialize<HarPreview>(contentAsStr, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });                

                return preview;
            }

            private void ValidateUserOwnsTheHar(PreviewHarByIdRequest request, IdentityUser user, HttpArchiveRecord har)
            {
                if (har.UserId != user.Id)
                {
                    throw new UserFriendlyException(StatusCodes.Status401Unauthorized, $"Cannot preview http archive with id: {request.Id} as it is not owned by the user");
                }
            }
        }
    }
}
