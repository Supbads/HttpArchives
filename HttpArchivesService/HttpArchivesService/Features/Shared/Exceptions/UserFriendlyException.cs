using System;

namespace HttpArchivesService.Features.Shared.Exceptions
{
    public class UserFriendlyException : Exception
    {
        public UserFriendlyException(int statusCode, string errorMessage): base(errorMessage)
        {
            this.StatusCode = statusCode;
        }

        public int StatusCode { get; }
    }
}