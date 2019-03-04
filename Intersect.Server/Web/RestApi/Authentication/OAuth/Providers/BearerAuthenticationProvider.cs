﻿using Intersect.Security.Claims;
using Intersect.Server.Classes.Database.PlayerData.Api;
using JetBrains.Annotations;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Threading.Tasks;

namespace Intersect.Server.Web.RestApi.Authentication.OAuth.Providers
{
    public class BearerAuthenticationProvider : OAuthBearerAuthenticationProvider
    {
        public override async Task ValidateIdentity([NotNull] OAuthValidateIdentityContext context)
        {
            var owinContext = context.OwinContext;

            var ticket = context.Ticket;
            var identity = ticket?.Identity;
            if (identity == null || owinContext == null)
            {
                context.Rejected();
                return;
            }

            var claimClientId = identity.FindFirst(IntersectClaimTypes.ClientId);
            if (!Guid.TryParse(claimClientId?.Value, out var clientId))
            {
                context.SetError("invalid_token_client");
                return;
            }


            var claimUserId = identity.FindFirst(IntersectClaimTypes.UserId);
            if (!Guid.TryParse(claimUserId?.Value, out var userId))
            {
                context.SetError("invalid_token_user");
                return;
            }

            var claimTicketId = identity.FindFirst(IntersectClaimTypes.TicketId);

            var refreshToken = await RefreshToken.FindForToken(claimTicketId?.Value);
            if (refreshToken == null)
            {
                context.Rejected();
                return;
            }

            if (ticket.Properties?.ExpiresUtc < DateTime.UtcNow)
            {
                context.SetError("access_token_expired");
            }

            if (refreshToken.ClientId != clientId || refreshToken.UserId != userId)
            {
                await RefreshToken.Remove(refreshToken.Id, true);
                context.Rejected();
                return;
            }

            owinContext.Set("refresh_token", refreshToken);
            context.Validated();
        }
    }
}
