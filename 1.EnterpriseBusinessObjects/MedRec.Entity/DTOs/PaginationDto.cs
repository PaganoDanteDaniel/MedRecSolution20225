namespace MedRec.Entity.DTOs;
public class PaginationDto
{
    public PaginationDto(int currentPage, int pageSize, string filterOne = null, string filterTwo = null)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        FilterOne = filterOne;
        FilterTwo = filterTwo;
    }

    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public string FilterOne { get; set; }
    public string FilterTwo { get; set; }
}
