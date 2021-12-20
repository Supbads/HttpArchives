using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using HttpArchivesService.Data;
using HttpArchivesService.Data.Entities;
using HttpArchivesService.Features.Shared.Interfaces;
using HttpArchivesService.Features.Shared.Exceptions;
using Microsoft.AspNetCore.Http;

namespace HttpArchivesService.Features.Directories.CreateDirectory
{
    public class CreateDirectoryFeature
    {
        public class CreateDirectoryRequestDto : IRequest<CreateDirectoryResponseDto>
        {
            public string DirectoryName { get; set; }
            public int? ParentDirId { get; set; }
        }

        public class UploadHarFileHandle : IRequestHandler<CreateDirectoryRequestDto, CreateDirectoryResponseDto>
        {
            private readonly AppDbContext _context;
            private readonly IUserProvider _userProvider;

            public UploadHarFileHandle(
                AppDbContext context,
                IUserProvider userProvider)
            {
                this._context = context;
                this._userProvider = userProvider;
            }

            public async Task<CreateDirectoryResponseDto> Handle(CreateDirectoryRequestDto request, CancellationToken cancellationToken)
            {
                var user = await this._userProvider.GetCurrentUserExplicit();

                await ValidateParentDirectory(request, user);

                var directory = new Directory
                {
                    DirectoryName = request.DirectoryName,
                    ParentDirId = request.ParentDirId,
                    UserId = user.Id,
                };

                await this._context.Directories.AddAsync(directory);
                await this._context.SaveChangesAsync();

                return new CreateDirectoryResponseDto
                {
                    DirId = directory.Id
                };
            }

            private async Task ValidateParentDirectory(CreateDirectoryRequestDto request, Microsoft.AspNetCore.Identity.IdentityUser user)
            {
                if (!request.ParentDirId.HasValue)
                {
                    return;
                }

                var dir = await this._context.Directories.FindAsync(request.ParentDirId);
                if (dir == null)
                {
                    throw new UserFriendlyException(StatusCodes.Status400BadRequest,
                        $"Could not create directory {request.DirectoryName} as the parent directory does not exist");
                }

                if (dir.UserId != user.Id)
                {
                    throw new UserFriendlyException(StatusCodes.Status401Unauthorized,
                        $"Could not create directory {request.DirectoryName} as the parent directory does not belong to this user");
                }
            }
        }
    }
}