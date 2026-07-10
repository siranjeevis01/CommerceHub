using Microsoft.EntityFrameworkCore;
using CommerceHub.Identity.Domain.Entities;

namespace CommerceHub.Identity.Application.Common.Interfaces;

public interface IIdentityDbContext
{
    DbSet<User> Users { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Otp> Otps { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
