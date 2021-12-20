using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using HttpArchivesService.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HttpArchivesService.Data.Entities;
using HttpArchivesService.Features.Shared.Interfaces;
using HttpArchivesService.Features.Shared.Exceptions;
using MediatR;

namespace HttpArchivesService.Features.HttpArchives.UploadHarFiles
{
    public class UploadHarFilessFeature
    {
        public class HARUploadRequestDto : IRequest<HarUploadResponseDto>
        {
            [Required]
            public HARUploadDto[] Files { get; set; }
        }

        public class UploadHarFileHandle : IRequestHandler<HARUploadRequestDto, HarUploadResponseDto>
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

            public async Task<HarUploadResponseDto> Handle(HARUploadRequestDto request, CancellationToken cancellationToken)
            {
                var user = await this._userProvider.GetCurrentUserExplicit();
                
                ValidateRequestFiles(request);
                ValidateRequestDoesNotCreateDuplicatesInADirectory(request);
                await ValidateRequestFilesDirectories(request, user);
                //todo: validate root level dirs and hars
                //todo validate they are actually .har files

                var harEntities = request.Files.Select(x =>
                {
                    return new HttpArchiveRecord
                    {
                        FileName = x.Name,
                        DirId = x.DirId,
                        UserId = user.Id,
                        //HarFile = x.File todo figure out apropraite save format
                    };
                });

                await this._context.HttpArchiveRecords.AddRangeAsync(harEntities);
                await this._context.SaveChangesAsync();

                var localIdsMapping =
                    request.Files
                    .ToDictionary(x => GenerateVolatileIdFromDirAndFileName(x.DirId, x.Name), x => x.LocalId);

                var localIdsToPersistedIdsMapping = harEntities.ToDictionary(x =>
                {
                    var volatileId = GenerateVolatileIdFromDirAndFileName(x.DirId, x.FileName);
                    return localIdsMapping[volatileId];
                }, x => x.Id);

                return new HarUploadResponseDto
                {
                    HarFilesNameToIdMapping = localIdsToPersistedIdsMapping
                };
            }

            private void ValidateRequestFiles(HARUploadRequestDto request)
            {
                if (request.Files == null || !request.Files.Any())
                {
                    throw new UserFriendlyException(StatusCodes.Status400BadRequest, "No Har files were provided");
                }
            }

            private void ValidateRequestDoesNotCreateDuplicatesInADirectory(HARUploadRequestDto request)
            {
                var containsDuplicateFilesOnADirecotry = request.Files
                    .GroupBy(x => GenerateVolatileIdFromDirAndFileName(x.DirId, x.Name))
                    .Any(group => group.Count() > 1);

                if (containsDuplicateFilesOnADirecotry)
                {
                    throw new UserFriendlyException(StatusCodes.Status400BadRequest, "Cannot save two files with the same name in the same directory.");
                }
            }

            private async Task ValidateRequestFilesDirectories(HARUploadRequestDto request, IdentityUser user)
            {
                var directoryKeys = request.Files
                    .Where(f => f.DirId.HasValue)
                    .Select(f => f.DirId)
                    .ToHashSet();

                if (!directoryKeys.Any())
                {
                    return;
                }

                var dirs = await this._context.Directories
                    .Where(dir => directoryKeys.Contains(dir.Id))
                    .Select(dir => new DIrectorySlimDto
                    {
                        Id = dir.Id,
                        UserId = dir.UserId,
                        HARs = dir.ArchiveRecords.Select(har => new HarFileSlimDto
                        {
                            FileName = har.FileName
                        })
                    })
                    .ToArrayAsync();

                var unauthorizedDirs = dirs.Where(dir => dir.UserId != user.Id);
                if (unauthorizedDirs.Any())
                {
                    throw new UserFriendlyException(StatusCodes.Status401Unauthorized, $"Could not upload files as some directories do not belong to the user");
                }

                var duplicateHarFilesInDirectories = request.Files.Where(requestFile =>
                    {
                        var dir = dirs.Where(dir => dir.Id == requestFile.DirId).FirstOrDefault();
                        if(dir == null)
                        {
                            throw new UserFriendlyException(StatusCodes.Status400BadRequest, $"Could not upload files as some directories do not exist");
                        }

                        return dir.HARs.Any(h => h.FileName == requestFile.Name);

                    }).Select(f => f.Name)
                    .ToArray();

                if (duplicateHarFilesInDirectories.Any())
                {
                    throw new UserFriendlyException(StatusCodes.Status400BadRequest,
                        $"Upload failed. Some files: {string.Join(", ", duplicateHarFilesInDirectories)} , are already present in the directory");
                }
            }

            private static string GenerateVolatileIdFromDirAndFileName(int? dirId, string fileName)
            {
                var dirIdStr = dirId.HasValue ? dirId.ToString() : "root";
                return $"{dirIdStr}-{fileName}"; //account for multiple HAR files with the same name in different dirs
            }
        }
    }
}