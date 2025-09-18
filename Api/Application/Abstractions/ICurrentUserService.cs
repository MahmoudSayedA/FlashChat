﻿namespace Api.Application.Abstractions
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? UserName { get; }
        bool IsAuthenticated { get; }
    }
}
