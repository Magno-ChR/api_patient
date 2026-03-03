namespace patient.infrastructure.Integration;

/// <summary>Opciones de RabbitMQ para el consumidor de eventos FoodPlan.</summary>
public sealed class RabbitMqFoodPlanOptions
{
    public const string SectionName = "RabbitMQ";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    public string FoodPlansExchange { get; set; } = "foodplans";
    public string FoodPlansQueue { get; set; } = "api_patient.foodplans";
    public string FoodPlanCreatedRoutingKey { get; set; } = "foodplan.created";
    public string FoodPlanUpdatedRoutingKey { get; set; } = "foodplan.updated";
}
