using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace patient.infrastructure.Percistence.PersistenceModel.Entities;

[Table("FoodPlan")]
internal class FoodPlanPM
{
    [Key]
    [Column("FoodPlanId")]
    public Guid Id { get; set; }

    [Column("Name")]
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}
