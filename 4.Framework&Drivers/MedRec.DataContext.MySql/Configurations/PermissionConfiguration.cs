using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Code).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Description).HasMaxLength(250);
        builder.Property(p => p.IsDeleted).HasDefaultValue(false);

        builder.Property(p => p.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
    }
}
