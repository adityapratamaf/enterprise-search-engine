using SearchEngine.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SearchEngine.Infrastructure.Identity.Configurations;

public class RolePermissionConfiguration
    : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(
        EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RoleId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasOne(x => x.Permission)
            .WithMany()
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK ke IdentityRole (AspNetRoles). Restrict untuk menghindari
        // multiple cascade path ke RolePermissions (Permission sudah cascade);
        // pembersihan saat role dihapus ditangani di handler penghapusan role.
        builder.HasOne<IdentityRole>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x =>
            new
            {
                x.RoleId,
                x.PermissionId
            })
            .IsUnique();
    }
}
