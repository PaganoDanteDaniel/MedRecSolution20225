using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedRec.DataContext.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessionalsPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO Permissions (Id, Code, Description, IsDeleted, CreatedAt)
                SELECT UUID(), t.Code, t.Description, 0, UTC_TIMESTAMP(6) FROM (
                    SELECT 'professionals.view' AS Code, 'Ver profesionales' AS Description
                    UNION ALL SELECT 'professionals.create', 'Crear profesionales'
                    UNION ALL SELECT 'professionals.edit', 'Editar profesionales'
                    UNION ALL SELECT 'professionals.delete', 'Eliminar profesionales'
                ) AS t;

                INSERT INTO RolePermissions (RoleId, PermissionId)
                SELECT r.Id, p.Id FROM Roles r CROSS JOIN Permissions p
                WHERE r.Name = 'Administrador'
                  AND p.Code IN ('professionals.view', 'professionals.create', 'professionals.edit', 'professionals.delete');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE rp FROM RolePermissions rp
                INNER JOIN Permissions p ON rp.PermissionId = p.Id
                WHERE p.Code IN ('professionals.view', 'professionals.create', 'professionals.edit', 'professionals.delete');

                DELETE FROM Permissions
                WHERE Code IN ('professionals.view', 'professionals.create', 'professionals.edit', 'professionals.delete');
            ");
        }
    }
}
