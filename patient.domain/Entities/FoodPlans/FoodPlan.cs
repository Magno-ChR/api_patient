using patient.domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.domain.Entities.FoodPlans;

public class FoodPlan : AggregateRoot
{
    public string Name { get; private set; }
    public FoodPlan(Guid id, string name) : base(id)
    {
        Name = name;
    }

    private FoodPlan() : base() { }

}
