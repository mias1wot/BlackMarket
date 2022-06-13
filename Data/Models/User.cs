namespace BlackMarket_API.Data.Models
{
	using Microsoft.AspNet.Identity;
	using Microsoft.AspNet.Identity.EntityFramework;
	using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
	using System.Security.Claims;
	using System.Threading.Tasks;

	[Table("User")]
	//public partial class User
	//public partial class User: IdentityUser
	//public partial class User: IdentityUser<int, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>, IUser<int>
	//public partial class User : IdentityUser<long, IdentityUserLogin<long>, IdentityUserRole<long>, IdentityUserClaim<long>>
	public partial class User : IdentityUser<long, CustomUserLogin, CustomUserRole, CustomUserClaim>
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            Cart = new HashSet<Cart>();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User, long> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            //var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        [Required]
        [StringLength(60)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Surname { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Cart> Cart { get; set; }
    }
}
