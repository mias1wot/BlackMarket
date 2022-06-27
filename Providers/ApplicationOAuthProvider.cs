using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.Services.Repositories;

namespace BlackMarket_API.Providers
{
	public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;

        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null)
            {
                throw new ArgumentNullException("publicClientId");
            }

            _publicClientId = publicClientId;
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");
            if (allowedOrigin == null)
                allowedOrigin = "*";

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });


            var userManager = context.OwinContext.GetUserManager<UserManager>();

			User user = await userManager.FindAsync(context.UserName, context.Password);
			if (user == null)
			{
				context.SetError("invalid_grant", "The user name or password is incorrect.");
				return;
			}

			//You need to all needed claims in ClaimsIdentity (oAuthIdentity or your own)

			ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager,
			   Startup.OAuthOptions.AuthenticationType);

			//var identity = new ClaimsIdentity(context.Options.AuthenticationType);
			//identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
			//identity.AddClaim(new Claim("sub", context.UserName));
			//identity.AddClaim(new Claim("role", roles));
			//IList<string> roles = await userManager.GetRolesAsync(user.Id);


			ClaimsIdentity cookiesIdentity = await user.GenerateUserIdentityAsync(userManager,
						 CookieAuthenticationDefaults.AuthenticationType);

            //AuthenticationProperties properties = CreateProperties(user.UserName);
            var propertiesDictionary = new Dictionary<string, string> 
            { 
                { "UserId", user.Id.ToString() },
				{ "ClientId", (context.ClientId == null) ? string.Empty : context.ClientId }
			};
            AuthenticationProperties properties = new AuthenticationProperties(propertiesDictionary);


            AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
			context.Validated(ticket);

            context.Request.Context.Authentication.SignIn(cookiesIdentity);
		}

		public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
		{
			string originalClient = context.Ticket.Properties.Dictionary["ClientId"];
			string currentClient = context.ClientId;

			if (originalClient != currentClient)
			{
				context.SetError("invalid_clientId", "Refresh token is issued to a different clientId.");
				return Task.FromResult<object>(null);
			}

			//Here you can change Identity information
			var newIdentity = new ClaimsIdentity(context.Ticket.Identity);
            //newIdentity.AddClaim(new Claim("newClaim", "newValue"));

            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);

            return Task.FromResult<object>(null);
		}

		public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
			foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
			{
				context.AdditionalResponseParameters.Add(property.Key, property.Value);
			}

			return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            //client_id, client_secret can be passed and checked here

            string clientId = string.Empty;
            string clientSecret = string.Empty;

            //Try to get credentials from 2 different sources of packet
            if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
            {
                context.TryGetFormCredentials(out clientId, out clientSecret);
            }

            if (context.ClientId == null)
            {
                //Remove the comments from the below line context.SetError, and invalidate context 
                //if you want to force sending clientId/secrects once obtain access tokens. 
                context.Validated();
                //context.SetError("invalid_clientId", "ClientId should be sent.");
                return Task.FromResult<object>(null);
            }


            //Checks passed credentials
            Client client = null;
            using (ClientRepository clientRepo = new ClientRepository())
            {
                client = clientRepo.GetClient(context.ClientId);
            }

            if (client == null)
            {
                context.SetError("invalid_clientId", $"Client '{context.ClientId}' is not registered in the system.");
                return Task.FromResult<object>(null);
            }

            if (client.ApplicationType == ApplicationTypes.React || client.ApplicationType == ApplicationTypes.Flutter)
            {
                if (string.IsNullOrWhiteSpace(clientSecret))
                {
                    context.SetError("invalid_clientId", "Client secret must be sent.");
                    return Task.FromResult<object>(null);
                }
                else
                {
                    if (client.Secret != Helper.GetHash(clientSecret))
                    {
                        context.SetError("invalid_clientId", "Client secret is invalid.");
                        return Task.FromResult<object>(null);
                    }
                }
            }

            if (!client.Active)
            {
                context.SetError("invalid_clientId", "Client is inactive (cannot be used).");
                return Task.FromResult<object>(null);
            }

            context.OwinContext.Set<string>("as:clientAllowedOrigin", client.AllowedOrigin);
            context.OwinContext.Set<string>("as:clientRefreshTokenLifeTimeInMinutes", client.RefreshTokenLifeTimeInMinutes.ToString());

            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(string userName)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", userName }
            };
            return new AuthenticationProperties(data);
        }
    }
}