namespace MedRec.Shared.Security;
public class Jwt
{
    public const string SectionKey = nameof(Jwt);
    public string Key { get; set; }
    public int ExpirationMinutes { get; set; } = 240;
}
