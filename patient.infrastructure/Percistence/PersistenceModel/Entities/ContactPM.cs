using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;


namespace patient.infrastructure.Percistence.PersistenceModel.Entities;

[Table("Contact")]
internal class ContactPM
{
    [Key]
    [Column("ContactId")]
    public Guid Id { get; set; }

    [Column("Direction")]
    [StringLength(500)]
    [Required]
    public string Direction { get; set; } = string.Empty;

    [Column("Reference")]
    [StringLength(500)]
    public string? Reference { get; set; }

    [Column("PhoneNumber")]
    [StringLength(20)]
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("Floor")]
    [StringLength(50)]
    [Required]
    public string Floor { get; set; } = string.Empty;

    [Column("Coords")]
    [StringLength(100)]
    [Required]
    public string Coords { get; set; } = string.Empty;

    [Column("IsActive")]
    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    [Column("PatientId")]
    public Guid PatientId { get; set; }
    
    [ForeignKey(nameof(PatientId))]
    public PatientPM Patient { get; set; } = null!;
}
