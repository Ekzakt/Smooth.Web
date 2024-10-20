﻿using IdentityModel;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;

namespace Smooth.Shared.Extensions;

public static class PrincipalExtensions
{
    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <param name="principal">The principal.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static string GetDisplayName(this ClaimsPrincipal principal)
    {
        if (principal == null) return string.Empty;

        var name = principal.FindFirst(JwtClaimTypes.Name)?.Value;
        if (!string.IsNullOrWhiteSpace(name)) return name;

        var sub = principal.FindFirst(JwtClaimTypes.Subject)?.Value;
        if (!string.IsNullOrWhiteSpace(name)) return sub;

        return string.Empty;
    }


    /// <summary>
    /// Determines whether this instance is authenticated.
    /// </summary>
    /// <param name="principal">The principal.</param>
    /// <returns>
    ///   <c>true</c> if the specified principal is authenticated; otherwise, <c>false</c>.
    /// </returns>
    [DebuggerStepThrough]
    public static bool IsAuthenticated(this IPrincipal principal)
    {
        return principal != null && principal.Identity != null && principal.Identity.IsAuthenticated;
    }
}
