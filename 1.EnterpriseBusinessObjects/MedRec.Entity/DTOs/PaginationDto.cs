namespace MedRec.Entity.DTOs;
public class PaginationDto
{
    public PaginationDto(int pageNumber, int pageSize, string filter = null)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        Filter = filter;
    }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string Filter { get; set; }
}
