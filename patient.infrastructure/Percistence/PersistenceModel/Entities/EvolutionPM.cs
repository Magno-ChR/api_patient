using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace patient.infrastructure.Percistence.PersistenceModel.Entities;

[Table("Evolution")]
internal class EvolutionPM
{
    [Column("EvolutionId")]
    [Key]
    public Guid Id { get; set; }

    [Column("RegisterDate")]
    [Required]
    public DateTime RegisterDate { get; set; }

    [Column("Description")]
    [StringLength(1000)]
    [Required]
    public string Description { get; set; } = string.Empty;

    [Column("Observations")]
    [StringLength(1000)]
    public string? Observations { get; set; }

    [Column("MedicOrder")]
    [StringLength(1000)]
    [Required]
    public string MedicOrder { get; set; } = string.Empty;

    [Column("HistoryId")]
    [Required]
    public Guid? HistoryId { get; set; }

    [ForeignKey(nameof(HistoryId))]
    public HistoryPM History { get; set; } = null!;

}
