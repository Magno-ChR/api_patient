namespace patient.infrastructure.Integration;

/// <summary>Opciones de RabbitMQ para consumir eventos meal-plan (cola ms-patients-queue de ms-infrastructure).</summary>
public sealed class RabbitMqFoodPlanOptions
{
    public const string SectionName = "RabbitMQ";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    /// <summary>Virtual host (ms-infrastructure usa "/").</summary>
    public string VirtualHost { get; set; } = "/";
    /// <summary>Segundos entre intentos de reconexión (estilo ms-logistic).</summary>
    public int ReconnectDelaySeconds { get; set; } = 5;
    /// <summary>Máximo de intentos de reconexión al arrancar (0 = infinitos).</summary>
    public int MaxReconnectAttempts { get; set; } = 0;

    /// <summary>Exchange definido en ms-infrastructure (meal-plans). Solo referencia; no se declara.</summary>
    public string FoodPlansExchange { get; set; } = "meal-plans";
    /// <summary>Cola definida en ms-infrastructure para ms-patient. No crear; solo consumir.</summary>
    public string FoodPlansQueue { get; set; } = "ms-patients-queue";
    public string FoodPlanCreatedRoutingKey { get; set; } = "meal-plan.created";
    public string FoodPlanUpdatedRoutingKey { get; set; } = "meal-plan.updated";
}
