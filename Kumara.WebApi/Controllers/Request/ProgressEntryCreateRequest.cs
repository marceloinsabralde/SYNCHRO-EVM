using System.ComponentModel.DataAnnotations;
using Kumara.Validations;

namespace Kumara.WebApi.Controllers.Requests;

public class ProgressEntryCreateRequest
{
    [NotEmpty]
    public Guid? id { get; set; }

    [NotEmpty]
    public Guid iTwinId { get; set; }

    [NotEmpty]
    public Guid activityId { get; set; }

    [NotEmpty]
    public Guid materialId { get; set; }

    [NotEmpty]
    public Guid quantityUnitOfMeasureId { get; set; }

    [Required]
    public decimal quantityDelta { get; set; }

    [Required]
    public DateOnly progressDate { get; set; }
}
