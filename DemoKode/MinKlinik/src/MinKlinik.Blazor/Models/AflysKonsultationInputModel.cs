using System.ComponentModel.DataAnnotations;

namespace MinKlinik.Blazor.Models;

public class AflysKonsultationInputModel
{
    [Required]
    public Guid? KonsultationId { get; set; }
}
