namespace MedRec.Identity.BusinessObjects.Constants;

public static class SystemPermissions
{
    public const string Patients_View = "patients.view";
    public const string Patients_Create = "patients.create";
    public const string Patients_Edit = "patients.edit";
    public const string Patients_Delete = "patients.delete";

    public const string MedicalVisits_View = "medicalvisits.view";
    public const string MedicalVisits_Create = "medicalvisits.create";
    public const string MedicalVisits_Edit = "medicalvisits.edit";
    public const string MedicalVisits_Delete = "medicalvisits.delete";

    public const string Appointments_View = "appointments.view";
    public const string Appointments_Create = "appointments.create";
    public const string Appointments_Edit = "appointments.edit";
    public const string Appointments_Delete = "appointments.delete";

    public const string HealthInsurance_View = "healthinsurance.view";
    public const string HealthInsurance_Create = "healthinsurance.create";
    public const string HealthInsurance_Edit = "healthinsurance.edit";
    public const string HealthInsurance_Delete = "healthinsurance.delete";

    public const string DynamicTemplates_View = "dynamictemplates.view";
    public const string DynamicTemplates_Create = "dynamictemplates.create";
    public const string DynamicTemplates_Edit = "dynamictemplates.edit";
    public const string DynamicTemplates_Delete = "dynamictemplates.delete";

    public const string Users_View = "users.view";
    public const string Users_Create = "users.create";
    public const string Users_Edit = "users.edit";
    public const string Users_Delete = "users.delete";

    public const string Roles_View = "roles.view";
    public const string Roles_Create = "roles.create";
    public const string Roles_Edit = "roles.edit";
    public const string Roles_Delete = "roles.delete";

    public const string Professionals_View = "professionals.view";
    public const string Professionals_Create = "professionals.create";
    public const string Professionals_Edit = "professionals.edit";
    public const string Professionals_Delete = "professionals.delete";

    public static readonly IReadOnlyList<(string Code, string Description)> All = new[]
    {
        (Patients_View, "Ver pacientes"),
        (Patients_Create, "Crear pacientes"),
        (Patients_Edit, "Editar pacientes"),
        (Patients_Delete, "Eliminar pacientes"),
        (MedicalVisits_View, "Ver visitas médicas"),
        (MedicalVisits_Create, "Crear visitas médicas"),
        (MedicalVisits_Edit, "Editar visitas médicas"),
        (MedicalVisits_Delete, "Eliminar visitas médicas"),
        (Appointments_View, "Ver turnos"),
        (Appointments_Create, "Crear turnos"),
        (Appointments_Edit, "Editar turnos"),
        (Appointments_Delete, "Eliminar turnos"),
        (HealthInsurance_View, "Ver obras sociales"),
        (HealthInsurance_Create, "Crear obras sociales"),
        (HealthInsurance_Edit, "Editar obras sociales"),
        (HealthInsurance_Delete, "Eliminar obras sociales"),
        (DynamicTemplates_View, "Ver plantillas de campos dinámicos"),
        (DynamicTemplates_Create, "Crear plantillas de campos dinámicos"),
        (DynamicTemplates_Edit, "Editar plantillas de campos dinámicos"),
        (DynamicTemplates_Delete, "Eliminar plantillas de campos dinámicos"),
        (Users_View, "Ver usuarios"),
        (Users_Create, "Crear usuarios"),
        (Users_Edit, "Editar usuarios"),
        (Users_Delete, "Eliminar usuarios"),
        (Roles_View, "Ver roles"),
        (Roles_Create, "Crear roles"),
        (Roles_Edit, "Editar roles"),
        (Roles_Delete, "Eliminar roles"),
        (Professionals_View, "Ver profesionales"),
        (Professionals_Create, "Crear profesionales"),
        (Professionals_Edit, "Editar profesionales"),
        (Professionals_Delete, "Eliminar profesionales"),
    };
}
