using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HttpArchivesService.Data;
using Microsoft.EntityFrameworkCore;
using HttpArchivesService.Features.Shared.Interfaces;
using MediatR;

namespace HttpArchivesService.Features.HttpArchives.GetHarsByCurrentUser
{
    public class GetHarsByCurrentUser
    {
        public class GetHarsRequest : IRequest<GetHarsResponseDto>
        {
        }

        public class GetHarsRequestHandle : IRequestHandler<GetHarsRequest, GetHarsResponseDto>
        {
            private readonly AppDbContext _context;
            private readonly IUserProvider _userProvider;

            public GetHarsRequestHandle(
                AppDbContext context,
                IUserProvider userProvider)
            {
                this._context = context;
                this._userProvider = userProvider;
            }

            public async Task<GetHarsResponseDto> Handle(GetHarsRequest request, CancellationToken cancellationToken)
            {
                var user = await this._userProvider.GetCurrentUserExplicit();
                
                var userHarFiles = await this._context.HttpArchiveRecords.Where(dir => dir.UserId == user.Id).ToListAsync();

                var harFilesMapped = userHarFiles.Select(har => new HarFileDto
                    {
                        Id = har.Id,
                        DirectoryId = har.DirId,
                        Name = har.FileName
                    }).ToArray();

                return new GetHarsResponseDto
                {
                    HarFiles = harFilesMapped
                };
            }

        }
    }
}