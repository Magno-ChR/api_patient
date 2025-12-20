using patient.test.ContractTest.ContractDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace patient.test.ContractTest;

public class PatientContractTest
{
    private readonly HttpClient _client;

    public PatientContractTest()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7134")
        };
    }

    [Fact]
    public async Task GetPatients_Should_Respect_Contract()
    {
        // Act
        var response = await _client.GetAsync(
            "/api/Patients/GetList?page=1&pageSize=10&search=Carlo");

        // Assert – Status code
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GetPatientsResponse>();

        // Assert – Root contract
        Assert.NotNull(body);
        Assert.True(body!.IsSuccess);
        Assert.False(body.IsFailure);

        // Assert – Paging contract
        Assert.NotNull(body.Value);
        Assert.True(body.Value.Page > 0);
        Assert.True(body.Value.PageSize > 0);
        Assert.True(body.Value.TotalPages >= 1);

        // Assert – Items contract
        Assert.NotNull(body.Value.Items);
        Assert.All(body.Value.Items, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(item.FirstName));
            Assert.False(string.IsNullOrWhiteSpace(item.LastName));
            Assert.NotEqual(Guid.Empty, item.Id);
        });
    }

    [Fact]
    public async Task GetPatients_When_NotFound_Should_Respect_Error_Contract()
    {
        // Act
        var response = await _client.GetAsync(
            "/api/Patients/GetList?page=1&pageSize=10&search=PacienteInexistente");

        // Assert – Status code
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponseContractDto>();

        // Assert – Root contract
        Assert.NotNull(body);
        Assert.False(body!.IsSuccess);
        Assert.True(body.IsFailure);

        // Assert – Error contract
        Assert.NotNull(body.Error);
        Assert.Equal("Patient.NotFound", body.Error.Code);
        Assert.False(string.IsNullOrWhiteSpace(body.Error.Description));
        Assert.False(string.IsNullOrWhiteSpace(body.Error.StructuredMessage));
        Assert.True(body.Error.Type > 0);
    }
}
