using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;
public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever(); // Generado en C#

        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(PatientConstraints.FirstNameMaxLength);

        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(PatientConstraints.LastNameMaxLength);

        builder.Property(p => p.DocumentNumber)
            .IsRequired()
            .HasMaxLength(PatientConstraints.DocumentNumberMaxLength);

        builder.HasIndex(p => p.DocumentNumber).IsUnique();

        builder.Property(p => p.Address)
            .HasMaxLength(PatientConstraints.AddressMaxLength);

        builder.Property(p => p.PhoneNumber)
            .IsRequired()
            .HasMaxLength(PatientConstraints.PhoneNumberMaxLength);

        builder.Property(p => p.Email)
            .HasMaxLength(PatientConstraints.EmailMaxLength);

        builder.Property(p => p.BiologicalSexId)
            .HasDefaultValue(BiologicalSex.Unknown);

        builder.Property(p => p.HealthInsuranceMemberNumber)
            .HasMaxLength(PatientConstraints.HealthInsuranceMemberNumberMaxLength);

        builder.Property(p => p.HealthInsuranceCard)
            .HasMaxLength(PatientConstraints.HealthInsuranceCardMaxLength);

        builder.Property(p => p.HealthInsurancePlan)
            .HasMaxLength(PatientConstraints.HealthInsurancePlanMaxLength);

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);

        // Concurrencia optimista con LastModified
        builder.Property(p => p.RowVersion)
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        // Relación uno a uno
        builder.HasOne<PatientMedicalHistory>()
            .WithOne()
            .HasForeignKey<PatientMedicalHistory>(pmh => pmh.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
