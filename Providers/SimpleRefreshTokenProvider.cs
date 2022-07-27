using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.Services.Repositories;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BlackMarket_API.Providers
{
    public class SimpleRefreshTokenProvider : IAuthenticationTokenProvider
    {
        private static ConcurrentDictionary<string, AuthenticationTicket> _refreshTokens = new ConcurrentDictionary<string, AuthenticationTicket>();

        public void Create(AuthenticationTokenCreateContext context) => throw new NotImplementedException();
        public void Receive(AuthenticationTokenReceiveContext context) => throw new NotImplementedException();
        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            string clientId = context.Ticket.Properties.Dictionary["ClientId"];
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return;
            }

            double refreshTokenExpireInMinutes = Convert.ToDouble(context.OwinContext.Get<string>("as:clientRefreshTokenLifeTimeInMinutes"));
            

            context.Ticket.Properties.IssuedUtc = DateTime.UtcNow;
            context.Ticket.Properties.ExpiresUtc = DateTime.UtcNow.AddMinutes(refreshTokenExpireInMinutes);

            //Save Ticket to DB
            using (RefreshTokenRepository refreshTokenRepo = new RefreshTokenRepository())
			{
                var refreshTokenId = Guid.NewGuid().ToString();
                long userId = Convert.ToInt64(context.Ticket.Properties.Dictionary["UserId"]);
                string ticket = context.SerializeTicket();

                bool addRefreshTokenRes = await refreshTokenRepo.AddRefreshToken(refreshTokenId, userId, clientId, ticket);
                if(addRefreshTokenRes)
                    context.SetToken(refreshTokenId);
            }
		}

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            //var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");
            //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

            string refreshTokenId = context.Token;

            using (RefreshTokenRepository DBcontext = new RefreshTokenRepository())
            {
                RefreshToken refreshToken = await DBcontext.GetRefreshToken(refreshTokenId);

                if (refreshToken != null)
                {
                    context.DeserializeTicket(refreshToken.Ticket);
                    await DBcontext.RemoveRefreshToken(refreshToken);
                }
            }
        }
    }
}