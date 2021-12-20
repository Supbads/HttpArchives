using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using HttpArchivesService.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HttpArchivesService.Data.Entities;
using HttpArchivesService.Features.Shared.Interfaces;
using HttpArchivesService.Features.Shared.Exceptions;
using MediatR;

namespace HttpArchivesService.Features.HttpArchives.UploadHarFiles
{
    public class UploadHarFilessFeature
    {
        public class HarUploadRequestDto : IRequest<HarUploadResponseDto>
        {
            public IFormFile[] FormFiles { get; set; }
        }

        public class UploadHarFileHandle : IRequestHandler<HarUploadRequestDto, HarUploadResponseDto>
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

            public async Task<HarUploadResponseDto> Handle(HarUploadRequestDto request, CancellationToken cancellationToken)
            {
                var user = await this._userProvider.GetCurrentUserExplicit();

                ValidateFileExtension(request);
                ValidateRequestFiles(request);
                ValidateRequestDuplicateNameEntries(request);
                await ValidateRequestDoesNotCreateDuplicatesInRootDirectory(user.Id, request);

                var harEntities = new List<HttpArchiveRecord>(request.FormFiles.Length);

                foreach (var formFile in request.FormFiles)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await formFile.CopyToAsync(memoryStream);

                        var har = new HttpArchiveRecord
                        {
                            FileName = formFile.FileName,
                            DirId = null,
                            UserId = user.Id,
                            Content = memoryStream.ToArray()
                        };

                        harEntities.Add(har);
                    }
                }

                await this._context.HttpArchiveRecords.AddRangeAsync(harEntities);
                await this._context.SaveChangesAsync();

                var entitiesByNameAndId = harEntities.ToDictionary(har => har.FileName, har => har.Id);

                return new HarUploadResponseDto
                {
                    HarFilesNameToIdMapping = entitiesByNameAndId
                };
            }

            private static void ValidateFileExtension(HarUploadRequestDto request)
            {
                var areAllFilesHars = request.FormFiles.All(f => f.FileName.EndsWith(".har"));
                if (!areAllFilesHars)
                {
                    throw new UserFriendlyException(StatusCodes.Status400BadRequest, "Some files are not of type http archive recot (.har)");
                }
            }

            private void ValidateRequestFiles(HarUploadRequestDto request)
            {
                if (request.FormFiles == null || !request.FormFiles.Any())
                {
                    throw new UserFriendlyException(StatusCodes.Status400BadRequest, "No Har files were provided");
                }
            }

            private void ValidateRequestDuplicateNameEntries(HarUploadRequestDto request)
            {
                var containsDuplicates = request.FormFiles
                    .GroupBy(x => x.FileName)
                    .Select(g => g.Count())
                    .Any(count => count > 1);

                if (containsDuplicates)
                {
                    throw new UserFriendlyException(StatusCodes.Status400BadRequest, "Cannot save two files with the same name");
                }
            }

            private async Task ValidateRequestDoesNotCreateDuplicatesInRootDirectory(string userId, HarUploadRequestDto request)
            {
                var userRootLevelHars = (await this._context.HttpArchiveRecords.Where(har => har.UserId == userId && har.DirId == null)
                    .Select(har => har.FileName)
                    .ToArrayAsync())
                    .ToHashSet();

                bool containsRootLevelDuplicates = request.FormFiles.Any(har => userRootLevelHars.Contains(har.FileName));

                if (containsRootLevelDuplicates)
                {
                    throw new UserFriendlyException(StatusCodes.Status400BadRequest, "Failed to save files as some will duplicate by name");
                }
            }
        }
    }
}