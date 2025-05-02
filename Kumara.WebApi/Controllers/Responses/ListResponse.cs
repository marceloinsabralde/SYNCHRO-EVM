namespace Kumara.WebApi.Controllers.Responses;

public class ListResponse<T>
{
    public required IEnumerable<T> items { get; set; }
}
