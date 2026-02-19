using api_patient.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using patient.application.Patients.CreatePatient;
using patient.application.Patients.DeletePatient;
using patient.application.Patients.GetPatient;
using patient.application.Patients.UpdatePatient;

namespace api_patient.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "patient")]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetPatient([FromQuery]GetPatientCommand request)
        => this.HandleResult(await _mediator.Send(request));

    [HttpGet("GetList")]
    public async Task<IActionResult> GetPatients([FromQuery]GetPatientListCommand request)
    {
        var result = await _mediator.Send(request);
        return this.HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePatient([FromBody]CreatePatientCommand request)
    {
        var result = await _mediator.Send(request);
        return this.HandleResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePatient([FromBody] UpdatePatientCommand request)
    {
        var result = await _mediator.Send(request);
        return this.HandleResult(result);
    }

    [HttpPost]
    [Route("Contacts")]
    public async Task<IActionResult> CreatePatientContact([FromBody]CreatePatientContactCommand request)
    {
        var result = await _mediator.Send(request);
        return this.HandleResult(result);
    }

    [HttpPut]
    [Route("Contacts")]
    public async Task<IActionResult> UpdatePatientContact([FromBody]UpdatePatientContactCommand request)
    {
        var result = await _mediator.Send(request);
        return this.HandleResult(result);
    }

    [HttpDelete]
    [Route("Contacts")]
    public async Task<IActionResult> DeletePatientContact([FromQuery ]DeletePatientContactCommand request)
    {
        var result = await _mediator.Send(request);
        return this.HandleResult(result);
    }
}
