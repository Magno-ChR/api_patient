using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace patient.infrastructure.Percistence.PersistenceModel.Entities;

[Table("History")]
internal class HistoryPM
{
    [Key]
    [Column("HistoryId")]
    public Guid Id { get; set; }

    [Column("EntryDate")]
    [Required]
    public DateTime EntryDate { get; set; }

    [Column("Reason")]
    [StringLength(1000)]
    [Required]
    public string Reason { get; set; }

    [Column("Diagnostic")]
    [StringLength(1000)]
    [Required]
    public string Diagnostic { get; set; }

    [Column("Treatment")]
    [StringLength(1000)]
    [Required]
    public string Treatment { get; set; }

    [Column("PatientId")]
    [Required]
    public Guid PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public PatientPM Patient { get; set; }

    [Column("FoodPlanId")]
    [Required]
    public Guid FoodPlanId { get; set; }

    [ForeignKey(nameof(FoodPlanId))]
    public FoodPlanPM FoodPlan { get; set; }

    [InverseProperty(nameof(BackgroundPM.History))]
    public List<BackgroundPM> Backgrounds { get; set; }

    [InverseProperty(nameof(EvolutionPM.History))]
    public List<EvolutionPM> Evolutions { get; set; }

}
