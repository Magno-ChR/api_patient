using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace patient.infrastructure.Percistence.PersistenceModel.Entities;

[Table("Background")]
internal class BackgroundPM
{
    [Key]
    [Column("BackgroundId")]
    public Guid Id { get; set; }

    [Required]
    [Column("RegisterDate")]
    public DateTime RegisterDate { get; set; }

    [Required]
    [Column("Description")]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("HistoryId")]
    public Guid HistoryId { get; set; }

    [ForeignKey(nameof(HistoryId))]
    public HistoryPM History { get; set; } = null!;

}
