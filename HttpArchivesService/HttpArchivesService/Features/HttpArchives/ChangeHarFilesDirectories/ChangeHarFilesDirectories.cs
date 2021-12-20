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

                //todo validate duplicates in dirs
                //todo validate duplicates at root

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

            //private void ValidateRequestDoesNotCreateDuplicatesInADirectory(HARUploadRequestDto request)
            //{
            //    var containsDuplicateFilesOnADirecotry = request.Files
            //        .GroupBy(x => GenerateVolatileIdFromDirAndFileName(x.DirId, x.Name))
            //        .Any(group => group.Count() > 1);

            //    if (containsDuplicateFilesOnADirecotry)
            //    {
            //        throw new UserFriendlyException(StatusCodes.Status400BadRequest, "Cannot save two files with the same name in the same directory.");
            //    }
            //}

            //private async Task ValidateRequestFilesDirectories(HARUploadRequestDto request, IdentityUser user)
            //{
            //    var directoryKeys = request.Files
            //        .Where(f => f.DirId.HasValue)
            //        .Select(f => f.DirId)
            //        .ToHashSet();

            //    if (!directoryKeys.Any())
            //    {
            //        return;
            //    }

            //    var dirs = await this._context.Directories
            //        .Where(dir => directoryKeys.Contains(dir.Id))
            //        .Select(dir => new DIrectorySlimDto
            //        {
            //            Id = dir.Id,
            //            UserId = dir.UserId,
            //            HARs = dir.ArchiveRecords.Select(har => new HarFileSlimDto
            //            {
            //                FileName = har.FileName
            //            })
            //        })
            //        .ToArrayAsync();

            //    var unauthorizedDirs = dirs.Where(dir => dir.UserId != user.Id);
            //    if (unauthorizedDirs.Any())
            //    {
            //        throw new UserFriendlyException(StatusCodes.Status401Unauthorized, $"Could not upload files as some directories do not belong to the user");
            //    }

            //    var duplicateHarFilesInDirectories = request.Files.Where(requestFile =>
            //    {
            //        var dir = dirs.Where(dir => dir.Id == requestFile.DirId).FirstOrDefault();
            //        if (dir == null)
            //        {
            //            throw new UserFriendlyException(StatusCodes.Status400BadRequest, $"Could not upload files as some directories do not exist");
            //        }

            //        return dir.HARs.Any(h => h.FileName == requestFile.Name);

            //    }).Select(f => f.Name)
            //        .ToArray();

            //    if (duplicateHarFilesInDirectories.Any())
            //    {
            //        throw new UserFriendlyException(StatusCodes.Status400BadRequest,
            //            $"Upload failed. Some files: {string.Join(", ", duplicateHarFilesInDirectories)} , are already present in the directory");
            //    }
            //}

            //private async Task ValidateRequestDoesNotCreateDuplicatesInRootDirectory(string userId, HARUploadRequestDto request)
            //{
            //    var userRootLevelHars = (await this._context.HttpArchiveRecords.Where(har => har.UserId == userId && har.DirId == null)
            //        .Select(har => har.FileName)
            //        .ToArrayAsync())
            //        .ToHashSet();

            //    bool containsRootLevelDuplicates = request.FormFiles.Any(har => userRootLevelHars.Contains(har.FileName));

            //    if (containsRootLevelDuplicates)
            //    {
            //        throw new UserFriendlyException(StatusCodes.Status400BadRequest, "Failed to save files as some will duplicate by name");
            //    }
            //}

            private static string GenerateVolatileIdFromDirAndFileName(int? dirId, string fileName)
            {
                var dirIdStr = dirId.HasValue ? dirId.ToString() : "root";
                return $"{dirIdStr}-{fileName}"; //account for multiple HAR files with the same name in different dirs
            }
        }
    }
}