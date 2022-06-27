using BlackMarket_API.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackMarket_API.Data.Services.Interfaces
{
	public interface IRefreshTokenRepository
	{
		Task<RefreshToken> GetRefreshToken(string refreshTokenId);
		Task<bool> AddRefreshToken(string refreshTokenId, long userId, string clientId, string ticket);
		Task<bool> RemoveRefreshToken(string refreshTokenId);
		Task<bool> RemoveRefreshToken(RefreshToken refreshToken);
	}
}
