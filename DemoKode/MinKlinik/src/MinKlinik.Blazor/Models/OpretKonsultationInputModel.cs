using System.ComponentModel.DataAnnotations;

namespace MinKlinik.Blazor.Models;

public class OpretKonsultationInputModel : IValidatableObject
{
    [Required]
    public DateTime? Fra { get; set; }

    [Required]
    public DateTime? Til { get; set; }

    [Required]
    public Guid? BehandlingstypeId { get; set; }

    [Required]
    public Guid? PatientId { get; set; }

    [Required]
    public Guid? BehandlerId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Fra is not null && Til is not null && Til <= Fra)
        {
            yield return new ValidationResult(
                "Sluttidspunkt skal være efter starttidspunkt.",
                new[] { nameof(Til) });
        }
    }
}
