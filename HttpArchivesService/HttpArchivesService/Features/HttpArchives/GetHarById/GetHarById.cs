using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HttpArchivesService.Data;
using HttpArchivesService.Features.Shared.Interfaces;

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
                var user = _userProvider.GetCurrentUserExplicit();

                var har = await this._context.HttpArchiveRecords.FindAsync(request.HarId);

                var result = new HarResponseDto
                {
                    Id = har.Id,
                    FileName = har.FileName,
                    DirId = har.DirId,
                    //File = har.File
                };

                return result;
            }
        }
    }
}