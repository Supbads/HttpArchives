using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HttpArchivesService.Data;
using HttpArchivesService.Data.Entities;
using HttpArchivesService.Features.Shared.Exceptions;
using HttpArchivesService.Features.Shared.Interfaces;
using MediatR;

namespace HttpArchivesService.Features.Directories.RenameDirectories
{
    public class RenameDirectories
    {
        public class RenameDirectoriesRequest : IRequest<Unit>
        {
            public RenameDirectoryDto[] RenameDirectoriesDtos { get; set; }
        }

        public class RenameDirectoriesHandle : IRequestHandler<RenameDirectoriesRequest, Unit>
        {
            private readonly AppDbContext _context;
            private readonly IUserProvider _userProvider;

            public RenameDirectoriesHandle(
                AppDbContext context,
                IUserProvider userProvider)
            {
                this._context = context;
                this._userProvider = userProvider;
            }

            public async Task<Unit> Handle(RenameDirectoriesRequest request, CancellationToken cancellationToken)
            {
                var user = await this._userProvider.GetCurrentUserExplicit();

                ValidateRequestModels(request);

                var dirIdsToRename = request.RenameDirectoriesDtos.Select(dir => dir.DirectoryId).ToHashSet();
                var directories = await this._context.Directories.Where(dir => dirIdsToRename.Contains(dir.Id)).ToListAsync();

                ValidateDirectoriesBelongToUser(user, directories);

                directories.ForEach(dir =>
                {
                    var newName = request.RenameDirectoriesDtos
                        .Where(requestDir => requestDir.DirectoryId == dir.Id)
                        .FirstOrDefault()
                        .NewName;

                    dir.DirectoryName = newName;
                });

                await this._context.SaveChangesAsync();

                return Unit.Value;
            }

            private void ValidateRequestModels(RenameDirectoriesRequest request)
            {
                if(request.RenameDirectoriesDtos == null || !request.RenameDirectoriesDtos.Any())
                {
                    throw new UserFriendlyException(StatusCodes.Status400BadRequest, "Could not rename directories as no rename arguments were sent");
                }
            }

            private void ValidateDirectoriesBelongToUser(IdentityUser user, List<Directory> directories)
            {
                var directoriesNotOwnedByUser = directories.Where(dir => dir.UserId != user.Id).ToList();
                if (directoriesNotOwnedByUser.Any())
                {
                    throw new UserFriendlyException(StatusCodes.Status401Unauthorized, "Some of the directories being renamed are not owned by the user");
                }
            }
        }
    }
}