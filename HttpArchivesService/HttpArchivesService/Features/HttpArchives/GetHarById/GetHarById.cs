using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HttpArchivesService.Data;
using HttpArchivesService.Features.Shared.Interfaces;
using HttpArchivesService.Features.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace HttpArchivesService.Features.HttpArchives.GetHarById
{
    public class GetHarById
    {
        public class GetHarByIdRequest : IRequest<HarResponseDto>
        {
            public int HarId { get; set; }
        }

        public class GetHarByIdRequestHandle : IRequestHandler<GetHarByIdRequest, HarResponseDto>
        {
            private readonly AppDbContext _context;
            private readonly IUserProvider _userProvider;

            public GetHarByIdRequestHandle(
                AppDbContext context,
                IUserProvider userProvider)
            {
                this._context = context;
                this._userProvider = userProvider;
            }

            public async Task<HarResponseDto> Handle(GetHarByIdRequest request, CancellationToken cancellationToken)
            {
                var user = await _userProvider.GetCurrentUserExplicit();

                var har = await this._context.HttpArchiveRecords
                    .Where(har => har.Id == request.HarId)
                    .Select(har => new
                    {
                        har.Id,
                        har.UserId,
                        har.DirId,
                        har.FileName,
                    }).FirstOrDefaultAsync();

                if (har.UserId != user.Id)
                {
                    throw new UserFriendlyException(StatusCodes.Status401Unauthorized, $"User is not authorized to access har with id: {request.HarId}");
                }

                var result = new HarResponseDto
                {
                    Id = har.Id,
                    FileName = har.FileName,
                    DirId = har.DirId
                };

                return result;
            }
        }
    }
}