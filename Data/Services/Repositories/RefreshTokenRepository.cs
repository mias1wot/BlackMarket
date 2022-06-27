using BlackMarket_API.Data.Models;
using BlackMarket_API.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BlackMarket_API.Data.Services.Repositories
{
	public class RefreshTokenRepository: IRefreshTokenRepository, IDisposable
	{
		BlackMarket context;
		public RefreshTokenRepository()
		{
			context = new BlackMarket();
		}

		public async Task<RefreshToken> GetRefreshToken(string refreshTokenId)
		{
			string hashedRefreshTokenId = Helper.GetHash(refreshTokenId);

			return await context.RefreshToken.FindAsync(hashedRefreshTokenId);
		}

		public async Task<bool> AddRefreshToken(string refreshTokenId, long userId, string clientId, string ticket)
		{
			RefreshToken existingToken = context.RefreshToken
				.SingleOrDefault(refreshTokenDB => refreshTokenDB.UserId == userId && refreshTokenDB.ClientId == clientId);

			if (existingToken != null)
			{
				await RemoveRefreshToken(existingToken);
			}

			//Creates refresh token
			 var refreshToken = new RefreshToken()
			 {
				 RefreshTokenId = Helper.GetHash(refreshTokenId),
				 UserId = userId,
				 ClientId = clientId,
				 Ticket = ticket,
			 };

			context.RefreshToken.Add(refreshToken);

			return context.SaveChanges() > 0;
		}


		public async Task<bool> RemoveRefreshToken(string refreshTokenId)
		{
			string hashedRefreshTokenId = Helper.GetHash(refreshTokenId);
			var refreshToken = await context.RefreshToken.FindAsync(hashedRefreshTokenId);

			if (refreshToken == null)
				return false;

			context.RefreshToken.Remove(refreshToken);
			return await context.SaveChangesAsync() > 0;

			
		}

		public async Task<bool> RemoveRefreshToken(RefreshToken refreshToken)
		{
			context.RefreshToken.Remove(refreshToken);
			return await context.SaveChangesAsync() > 0;
		}



		public void Dispose()
		{
			context.Dispose();
		}
	}
}