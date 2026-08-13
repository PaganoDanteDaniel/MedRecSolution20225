using MedRec.Identity.BusinessObjects.Constants;

namespace MedRec.Identity.UseCases.Tests;

/// <summary>
/// Guarda de sincronización entre <see cref="SystemPermissions"/> (código) y el seed SQL de la
/// migración <c>20260806154747_AddIdentityAndAudit.cs</c> más el seed incremental de
/// <c>20260813113621_AddProfessionalsPermissions.cs</c> (INSERT INTO Permissions ...), que
/// hardcodean independientemente el mismo catálogo de 32 permisos. Si alguno de los dos se edita
/// sin actualizar el otro, un permiso puede faltar en la base y una verificación contra
/// <c>IAuthorizationService.EnsurePermissionAsync</c> se convierte en un deny-by-default silencioso
/// y difícil de diagnosticar en producción.
///
/// IMPORTANTE: la lista de códigos esperados abajo debe mantenerse en sincronía manualmente con
/// AMBOS lugares: <see cref="SystemPermissions"/>.All y el bloque SELECT ... UNION ALL del seed SQL
/// en la migración. Si tenés que tocar uno de los tres (esta lista, la clase, o la migración),
/// actualizá los otros dos en el mismo cambio.
/// </summary>
public class SystemPermissionsCatalogTests
{
    // Debe coincidir exactamente (mismo conjunto, sin faltantes ni sobrantes) con los códigos
    // sembrados por el bloque "INSERT INTO Permissions ... SELECT UUID(), t.Code, t.Description"
    // de 4.Framework&Drivers\MedRec.DataContext.MySql\Migrations\20260806154747_AddIdentityAndAudit.cs
    private static readonly string[] ExpectedMigrationSeedCodes =
    {
        "patients.view",
        "patients.create",
        "patients.edit",
        "patients.delete",
        "medicalvisits.view",
        "medicalvisits.create",
        "medicalvisits.edit",
        "medicalvisits.delete",
        "appointments.view",
        "appointments.create",
        "appointments.edit",
        "appointments.delete",
        "healthinsurance.view",
        "healthinsurance.create",
        "healthinsurance.edit",
        "healthinsurance.delete",
        "dynamictemplates.view",
        "dynamictemplates.create",
        "dynamictemplates.edit",
        "dynamictemplates.delete",
        "users.view",
        "users.create",
        "users.edit",
        "users.delete",
        "roles.view",
        "roles.create",
        "roles.edit",
        "roles.delete",
        "professionals.view",
        "professionals.create",
        "professionals.edit",
        "professionals.delete",
    };

    [Fact]
    public void All_ShouldHaveExactly32Permissions()
    {
        Assert.Equal(32, SystemPermissions.All.Count);
    }

    [Fact]
    public void All_ShouldMatchMigrationSeedCodes_ExactlyNoMoreNoFewer()
    {
        var codesInCode = SystemPermissions.All.Select(p => p.Code).ToHashSet();
        var codesInMigration = ExpectedMigrationSeedCodes.ToHashSet();

        Assert.Equal(codesInMigration, codesInCode);
    }

    [Fact]
    public void All_ShouldNotContainDuplicateCodes()
    {
        var codes = SystemPermissions.All.Select(p => p.Code).ToList();
        Assert.Equal(codes.Count, codes.Distinct().Count());
    }
}
