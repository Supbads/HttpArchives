using HttpArchivesService.Data;
using HttpArchivesService.Features.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HttpArchivesService.Features.Directories.GetDirectories
{
    public class GetDirectoriesByCurrentUser
    {
        public class GetDirectoriesRequest : IRequest<GetDirectoriesResponseDto>
        {
        }

        public class GetDirectoriesRequestHandle : IRequestHandler<GetDirectoriesRequest, GetDirectoriesResponseDto>
        {
            private readonly AppDbContext _context;
            private readonly IUserProvider _userProvider;

            public GetDirectoriesRequestHandle(
                AppDbContext context,
                IUserProvider userProvider)
            {
                this._context = context;
                this._userProvider = userProvider;
            }

            public async Task<GetDirectoriesResponseDto> Handle(GetDirectoriesRequest request, CancellationToken cancellationToken)
            {
                var user = await this._userProvider.GetCurrentUserExplicit();

                // queries run parallel
                var userHarFilesTask = this._context.HttpArchiveRecords.Where(dir => dir.UserId == user.Id).ToListAsync();
                var userDirectoriesTask = this._context.Directories.Where(dir => dir.UserId == user.Id).ToListAsync();

                var userDirectories = await userDirectoriesTask;
                var userHarFiles = await userHarFilesTask;

                var response = new GetDirectoriesResponseDto();

                var directoriesMapped = userDirectories
                    .Select(dir => new DirectoryViewDto
                    {
                        Id = dir.Id,
                        ParentDirId = dir.ParentDirId,
                        Name = dir.DirectoryName
                    }).ToDictionary(dir => dir.Id, dir => dir);

                var directoriesAtRoot = userDirectories.Where(dir => !dir.ParentDirId.HasValue)
                    .Select(dir => directoriesMapped[dir.Id])
                    .ToList();

                var dirsByParentDir = directoriesMapped.Select(x => x.Value)
                    .Where(dir => dir.ParentDirId.HasValue)
                    .GroupBy(dir => dir.ParentDirId.Value).ToDictionary(g => g.Key, g => g.ToList());

                var harFilesMapped = userHarFiles
                    .Select(har => new HarFileDto
                    {
                        Id = har.Id,
                        DirectoryId = har.DirId,
                        Name = har.FileName
                    }).ToDictionary(har => har.Id, har => har);

                var harsByDir = harFilesMapped.Select(x => x.Value)
                    .Where(har => har.DirectoryId.HasValue)
                    .GroupBy(har => har.DirectoryId.Value).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var directoryAtRoot in directoriesAtRoot)
                {
                    SetupDirectoryTreeRecursively(directoryAtRoot, harsByDir, dirsByParentDir);
                }

                var harFilesAtRoot = userHarFiles.Where(har => !har.DirId.HasValue)
                    .Select(har => harFilesMapped[har.Id])
                    .ToList();

                var rootDir = new RootDirectory
                {
                    InnerDirectories = directoriesAtRoot,
                    HarFiles = harFilesAtRoot,
                };

                response.RootDirectory = rootDir;
                return response;
            }

            private void SetupDirectoryTreeRecursively(DirectoryViewDto directory,
                Dictionary<int, List<HarFileDto>> harsByDir,
                Dictionary<int, List<DirectoryViewDto>> dirsByParentDir)
            {
                if (harsByDir.ContainsKey(directory.Id))
                {
                    directory.HarFilesPreview = harsByDir[directory.Id];
                }

                if (dirsByParentDir.ContainsKey(directory.Id))
                {
                    var dirsToAttach = dirsByParentDir[directory.Id];
                    directory.InnerDirectories = dirsToAttach;

                    foreach (var innerDir in dirsToAttach)
                    {
                        SetupDirectoryTreeRecursively(innerDir, harsByDir, dirsByParentDir);
                    }
                }
            }
        }
    }
}