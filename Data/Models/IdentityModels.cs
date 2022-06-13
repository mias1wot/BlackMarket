using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace BlackMarket_API.Data.Models
{
	public class CustomUserRole : IdentityUserRole<long> { }
	public class CustomUserClaim : IdentityUserClaim<long> { }
	public class CustomUserLogin : IdentityUserLogin<long> { }

	public class CustomRole : IdentityRole<long, CustomUserRole>
	{
		public CustomRole() { }
		public CustomRole(string name) { Name = name; }
	}

	public class CustomUserStore : UserStore<User, CustomRole, long,
		CustomUserLogin, CustomUserRole, CustomUserClaim>
	{
		public CustomUserStore(BlackMarket context)
			: base(context)
		{
		}
	}

	public class CustomRoleStore : RoleStore<CustomRole, long, CustomUserRole>
	{
		public CustomRoleStore(BlackMarket context)
			: base(context)
		{
		}
	}
}