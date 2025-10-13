namespace MedRec.DataContext.MySql.Options;
public class DBOptionsMySql
{
    public const string SectionKey = nameof(DBOptionsMySql);
    public string ConnectionString { get; set; }
}
