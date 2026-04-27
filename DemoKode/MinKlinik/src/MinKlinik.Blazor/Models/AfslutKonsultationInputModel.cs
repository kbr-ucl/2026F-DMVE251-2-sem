using System.ComponentModel.DataAnnotations;

namespace MinKlinik.Blazor.Models;

public class AfslutKonsultationInputModel
{
    [Required]
    public Guid? KonsultationId { get; set; }

    [Required(ErrorMessage = "Notat er påkrævet.")]
    [MinLength(2, ErrorMessage = "Notat skal være mindst 2 tegn.")]
    public string Notat { get; set; } = string.Empty;
}
