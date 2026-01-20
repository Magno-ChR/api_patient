using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace patient.test.IntegrationTest;

public class PatientIntegrationTest
{
    private readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("https://localhost:7134")
    };

    private async Task<Guid> CreatePatientAsync()
    {
        var documentNumber = $"DOC-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var payload = new
        {
            firstName = "Juan",
            middleName = "Pedro",
            lastName = "Chuyes Romero",
            bloodType = 1,
            documentNumber,
            dateOfBirth = "2000-08-15",
            ocupation = "Desarrollador",
            religion = "Agnostico",
            alergies = "Ninguna"
        };

        var response = await _client.PostAsJsonAsync("/api/Patients", payload);
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created);

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("isSuccess").GetBoolean());
        Assert.False(root.GetProperty("isFailure").GetBoolean());

        var valueElement = root.GetProperty("value");
        string idString = valueElement.GetString()!;
        Assert.True(Guid.TryParse(idString, out var id));

        var error = root.GetProperty("error");
        Assert.Equal(string.Empty, error.GetProperty("code").GetString());
        Assert.Equal(string.Empty, error.GetProperty("description").GetString());
        Assert.Equal(string.Empty, error.GetProperty("structuredMessage").GetString());
        Assert.Equal(0, error.GetProperty("type").GetInt32());

        return id;
    }

    private async Task<Guid> CreateContactAsync(Guid patientId)
    {
        var payload = new
        {
            patientId,
            direction = "Av. Siempre Viva 742",
            reference = "Frente al colegio",
            phoneNumber = "777-9999",
            floor = "2",
            coords = "-17.7833,-63.1821"
        };

        var response = await _client.PostAsJsonAsync("/api/Patients/Contacts", payload);
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created);

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("isSuccess").GetBoolean());
        Assert.False(root.GetProperty("isFailure").GetBoolean());

        var valueElement = root.GetProperty("value");
        string idString = valueElement.GetString()!;
        Assert.True(Guid.TryParse(idString, out var id));

        return id;
    }

    private async Task<Guid> CreateFoodPlanAsync()
    {
        var payload = new { name = $"Plan-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}" };

        var response = await _client.PostAsJsonAsync("/api/FoodPlans", payload);
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created);

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("isSuccess").GetBoolean());

        var valueElement = root.GetProperty("value");
        string idString = valueElement.GetString()!;
        Assert.True(Guid.TryParse(idString, out var id));

        return id;
    }

    private async Task<Guid> CreateHistoryAsync(Guid patientId, Guid foodPlanId)
    {
        var payload = new
        {
            patientId,
            foodPlanId,
            reason = "Consulta inicial",
            diagnostic = "Diagnóstico preliminar",
            treatment = "Tratamiento recomendado"
        };

        var response = await _client.PostAsJsonAsync("/api/Histories", payload);
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created);

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("isSuccess").GetBoolean());

        var valueElement = root.GetProperty("value");
        string idString = valueElement.GetString()!;
        Assert.True(Guid.TryParse(idString, out var id));

        return id;
    }

    [Fact]
    public async Task CreatePatient_ReturnsSuccessAndValidId()
    {
        var id = await CreatePatientAsync();
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsPatient()
    {
        var id = await CreatePatientAsync();

        var response = await _client.GetAsync($"/api/Patients/GetById?PatientId={id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("isSuccess").GetBoolean());
        Assert.False(root.GetProperty("isFailure").GetBoolean());

        var patient = root.GetProperty("value");
        Assert.Equal(id.ToString(), patient.GetProperty("id").GetString());
        Assert.False(string.IsNullOrWhiteSpace(patient.GetProperty("firstName").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(patient.GetProperty("lastName").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(patient.GetProperty("documentNumber").GetString()));

        var error = root.GetProperty("error");
        Assert.Equal(string.Empty, error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFoundAndError()
    {
        var response = await _client.GetAsync($"/api/Patients/GetById?PatientId={Guid.Empty}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        Assert.False(root.GetProperty("isSuccess").GetBoolean());
        Assert.True(root.GetProperty("isFailure").GetBoolean());

        var error = root.GetProperty("error");
        Assert.Equal("Patient.NotFound", error.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("description").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("structuredMessage").GetString()));
        Assert.True(error.GetProperty("type").GetInt32() > 0);
    }

    [Fact]
    public async Task CreateContact_ReturnsSuccessAndValidId()
    {
        var patientId = await CreatePatientAsync();
        var contactId = await CreateContactAsync(patientId);

        Assert.NotEqual(Guid.Empty, contactId);
    }

    [Fact]
    public async Task CreateHistory_ReturnsSuccessAndValidId()
    {
        var patientId = await CreatePatientAsync();
        var foodPlanId = await CreateFoodPlanAsync();
        var historyId = await CreateHistoryAsync(patientId, foodPlanId);

        Assert.NotEqual(Guid.Empty, historyId);
    }

    [Fact]
    public async Task CompleteWorkflow_PatientContactHistory_Succeeds()
    {
        // 1. Create Patient
        var patientId = await CreatePatientAsync();
        Assert.NotEqual(Guid.Empty, patientId);

        // 2. Verify Patient exists
        var patientResponse = await _client.GetAsync($"/api/Patients/GetById?PatientId={patientId}");
        Assert.Equal(HttpStatusCode.OK, patientResponse.StatusCode);

        // 3. Create Contact for Patient
        var contactId = await CreateContactAsync(patientId);
        Assert.NotEqual(Guid.Empty, contactId);

        // 4. Create Food Plan
        var foodPlanId = await CreateFoodPlanAsync();
        Assert.NotEqual(Guid.Empty, foodPlanId);

        // 5. Create Clinical History for Patient
        var historyId = await CreateHistoryAsync(patientId, foodPlanId);
        Assert.NotEqual(Guid.Empty, historyId);

        // 6. Verify History exists
        var historyResponse = await _client.GetAsync($"/api/Histories/GetById?HistoryId={historyId}");
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);

        using var stream = await historyResponse.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("isSuccess").GetBoolean());
        var history = root.GetProperty("value");
        Assert.Equal(patientId.ToString(), history.GetProperty("patientId").GetString());
        Assert.Equal(foodPlanId.ToString(), history.GetProperty("foodPlanId").GetString());
    }
}
