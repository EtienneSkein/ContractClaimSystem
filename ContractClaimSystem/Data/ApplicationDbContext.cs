using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ContractClaimSystem.Models;

// Your data models (entities) should be referenced here.
// For example, if you have a `ClaimSubmission` model, you'll include it as a DbSet property.

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSet for each entity you want to work with in the database
    public DbSet<ClaimSubmission> Claims { get; set; }
}
