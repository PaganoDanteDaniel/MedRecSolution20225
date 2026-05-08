namespace MedRec.Shared.Gruards;

public static class Guard
{
    public static GuardBuilderString Against(string value, string paramName)
        => new GuardBuilderString(value, paramName);
    public static GuardBuilderInt Against(int value, string paramName)
        => new GuardBuilderInt(value, paramName);
    public static GuardBuilderDateTime Against(DateTime value, string paramName)
        => new GuardBuilderDateTime(value, paramName);
    public static GuardBuilderEnum<TEnum> Against<TEnum>(TEnum value, string paramName)
        where TEnum : struct, Enum
        => new GuardBuilderEnum<TEnum>(value, paramName);
    public static GuardBuilderGuid Against(Guid value, string paramName)
        => new GuardBuilderGuid(value, paramName);
    public static GuardBuilderDecimal Against(decimal value, string paramName)
        => new GuardBuilderDecimal(value, paramName);
}


