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

            //Try this
            //using System.Security.Cryptography;
            //var randomNumber = new byte[32];
            //using (var rng = RandomNumberGenerator.Create())
            //{
            //    rng.GetBytes(randomNumber);
            //    return Convert.ToBase64String(randomNumber);
            //}


            double refreshTokenExpireInMinutes = Double.Parse(ConfigurationManager.AppSettings["RefreshTokenExpireInMinutes"]);

            var guid = Guid.NewGuid().ToString();
			/* Copy claims from previous token
			 ***********************************/
			var refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
			{
				IssuedUtc = context.Ticket.Properties.IssuedUtc,
				ExpiresUtc = DateTime.UtcNow.AddMinutes(refreshTokenExpireInMinutes)
			};
            var i = context.Ticket.Identity;

            var refreshTokenTicket = await Task.Run(() => new AuthenticationTicket(context.Ticket.Identity, refreshTokenProperties));

			_refreshTokens.TryAdd(guid, refreshTokenTicket);

			context.SetToken(guid);
		}

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            AuthenticationTicket ticket;
            if (_refreshTokens.TryRemove(context.Token, out ticket))
            {
				context.SetTicket(ticket);
				//context.SetTicket(new AuthenticationTicket(new System.Security.Claims.ClaimsIdentity(), ticket.Properties));
			}
        }
    }
}