using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediatR;
using HttpArchivesService.Data;
using HttpArchivesService.Data.Entities;
using HttpArchivesService.Features.Shared.Exceptions;
using HttpArchivesService.Features.Shared.Interfaces;

namespace HttpArchivesService.Features.HttpArchives.ChangeHarFilesDirectories
{
    public class ChangeHarFilesDirectories
    {
        public class ChangeHarFilesDirectoriesRequest : IRequest<Unit>
        {
            public ChangeHarFileDirectoryDto[] ChangeHarDirectoryDtos { get; set; }
        }

        public class ChangeDirectoriesHandle : IRequestHandler<ChangeHarFilesDirectoriesRequest, Unit>
        {
            private readonly AppDbContext _context;
            private readonly IUserProvider _userProvider;

            public ChangeDirectoriesHandle(
                AppDbContext context,
                IUserProvider userProvider)
            {
                this._context = context;
                this._userProvider = userProvider;
            }

            public async Task<Unit> Handle(ChangeHarFilesDirectoriesRequest request, CancellationToken cancellationToken)
            {
                var user = await this._userProvider.GetCurrentUserExplicit();

                ValidateMoveModels(request);

                var harIdsToMove = request.ChangeHarDirectoryDtos
                    .Select(har => har.HarId)
                    .ToHashSet();

                var hars = await this._context.HttpArchiveRecords
                    .Where(har => harIdsToMove.Contains(har.Id))
                    .ToListAsync();
                
                var newDirectoriesIds = request.ChangeHarDirectoryDtos
                    .Where(har => har.NewDirectoryId.HasValue)
                    .Select(har => har.NewDirectoryId.Value)
                    .ToHashSet();

                var directories = await this._context.Directories
                    .Where(dir => newDirectoriesIds.Contains(dir.Id))
                    .ToListAsync();

                ValidateHarFilesBelongToUser(user, hars);
                ValidateDirectoriesBelongToUser(user, directories);

                hars.ForEach(har =>
                {
                    var newDirectoryId = request.ChangeHarDirectoryDtos
                        .Where(requestHar => requestHar.HarId == har.Id)
                        .FirstOrDefault()
                        .NewDirectoryId;

                    har.DirId = newDirectoryId;
                });

                await this._context.SaveChangesAsync();

                return Unit.Value;
            }

            private void ValidateMoveModels(ChangeHarFilesDirectoriesRequest request)
            {
                if (request.ChangeHarDirectoryDtos == null || !request.ChangeHarDirectoryDtos.Any())
                {
                    throw new UserFriendlyException(StatusCodes.Status400BadRequest, "Could not move http archives as no arguments were sent");
                }
            }

            private void ValidateHarFilesBelongToUser(IdentityUser user, List<HttpArchiveRecord> hars)
            {
                var harsNotOwnedByUser = hars.Where(dir => dir.UserId != user.Id).ToList();
                if (harsNotOwnedByUser.Any())
                {
                    throw new UserFriendlyException(StatusCodes.Status401Unauthorized, "Some of the http archives being moved are not owned by the user");
                }
            }

            private void ValidateDirectoriesBelongToUser(IdentityUser user, List<Directory> directories)
            {
                var directoriesNotOwnedByUser = directories.Where(dir => dir.UserId != user.Id);

                if (directoriesNotOwnedByUser.Any())
                {
                    throw new UserFriendlyException(StatusCodes.Status401Unauthorized, "Some of the files are being moved to directories not owned by the user");
                }
            }
        }
    }
}