using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace patient.infrastructure.Percistence.PersistenceModel.Entities;

[Table("Patient")]
internal class PatientPM
{
    [Key]
    [Column("PatientId")]
    public Guid Id { get; set; }

    [Column("FirstName")]
    [StringLength(100)]
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Column("MiddleName")]
    [StringLength(100)]
    [Required]
    public string MiddleName { get; set; } = string.Empty;

    [Column("LastName")]
    [StringLength(100)]
    [Required]
    public string LastName { get; set; } = string.Empty;

    [Column("BloodType")]
    [Required]
    public int BloodType { get; set; }

    [Column("DocumentNumber")]
    [StringLength(50)]
    [Required]
    public string DocumentNumber { get; set; } = string.Empty;

    [Column("DateOfBirth")]
    [Required]
    public DateOnly DateOfBirth { get; set; }

    [Column("Ocupation")]
    [StringLength(100)]
    [Required]
    public string Ocupation { get; set; } = string.Empty;

    [Column("Religion")]
    [StringLength(100)]
    [Required]
    public string Religion { get; set; } = string.Empty;

    [Column("Alergies")]
    [StringLength(250)]
    [Required]
    public string Alergies { get; set; } = string.Empty;

    [InverseProperty(nameof(HistoryPM.Patient))]
    public List<ContactPM> Contacts { get; set; } = new();


}
