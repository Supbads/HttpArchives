using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using HttpArchivesService.Data;
using HttpArchivesService.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HttpArchivesService.Features.Shared.Interfaces;
using MediatR;
using HttpArchivesService.Features.Shared.Exceptions;
using Microsoft.AspNetCore.Http;

namespace HttpArchivesService.Features.HttpArchives.RenameHarFiles
{
    public class RenameHarFilesDirectories
    {
        public class RenameHarsRequest : IRequest<Unit>
        {
            public HarRenameDto[] RenameHarDtos { get; set; }
        }

        public class RenameHarsHandle : IRequestHandler<RenameHarsRequest, Unit>
        {
            private readonly AppDbContext _context;
            private readonly IUserProvider _userProvider;

            public RenameHarsHandle(
                AppDbContext context,
                IUserProvider userProvider)
            {
                this._context = context;
                this._userProvider = userProvider;
            }

            public async Task<Unit> Handle(RenameHarsRequest request, CancellationToken cancellationToken)
            {
                var user = await this._userProvider.GetCurrentUserExplicit();

                ValidateRenameModels(request);

                var harIdsToRename = request.RenameHarDtos.Select(har => har.Id).ToHashSet();
                var hars = await this._context.HttpArchiveRecords.Where(har => harIdsToRename.Contains(har.Id)).ToListAsync();

                ValidateDirectoriesBelongToUser(user, hars);

                hars.ForEach(har =>
                {
                    var newName = request.RenameHarDtos
                        .Where(requestHar => requestHar.Id == har.Id)
                        .FirstOrDefault()
                        .NewName;

                    har.FileName = newName;
                });

                await this._context.SaveChangesAsync();

                return Unit.Value;
            }

            private void ValidateRenameModels(RenameHarsRequest request)
            {
                if (request.RenameHarDtos == null || !request.RenameHarDtos.Any())
                {
                    throw new UserFriendlyException(StatusCodes.Status400BadRequest, "Could not rename http archives as no rename arguments were sent");
                }
            }

            private void ValidateDirectoriesBelongToUser(IdentityUser user, List<HttpArchiveRecord> hars)
            {
                var harsNotOwnedByUser = hars.Where(dir => dir.UserId != user.Id).ToList();
                if (harsNotOwnedByUser.Any())
                {
                    throw new UserFriendlyException(StatusCodes.Status401Unauthorized, "Some of the http archives being renamed are not owned by the user");
                }
            }
        }
    }
}