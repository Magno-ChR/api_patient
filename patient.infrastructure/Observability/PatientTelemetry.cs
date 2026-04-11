using System.Diagnostics;
using OpenTelemetry.Context.Propagation;

namespace patient.infrastructure.Observability;

public static class PatientTelemetry
{
    public const string ActivitySourceName = "patient.infrastructure.integration";
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    public static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;
}
