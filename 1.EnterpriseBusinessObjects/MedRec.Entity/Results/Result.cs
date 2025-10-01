namespace MedRec.Entity.Results;
public static class Result
{
    public static Result<Unit> Ok() => new(true, default, null);
}
