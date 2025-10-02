namespace MedRec.Entity.DTOs;
public class PaginationDto
{
    public PaginationDto(int currentPage, int pageSize, string filter = null)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        Filter = filter;
    }

    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public string Filter { get; set; }
}
