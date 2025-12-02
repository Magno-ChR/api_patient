using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using patient.application.Patients.CreatePatient;
using patient.application.Patients.DeletePatient;
using patient.application.Patients.GetPatient;
using patient.application.Patients.UpdatePatient;

namespace api_patient.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetPatient([FromQuery]GetPatientCommand Id)
    {
        var result = await _mediator.Send(Id);
        return Ok(result);
    }

    [HttpGet("GetList")]
    public async Task<IActionResult> GetPatients([FromQuery]GetPatientListCommand request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePatient([FromBody]CreatePatientCommand request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePatient([FromBody] UpdatePatientCommand request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    [HttpPost]
    [Route("Contacts")]
    public async Task<IActionResult> CreatePatientContact([FromBody]CreatePatientContactCommand request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    [HttpPut]
    [Route("Contacts")]
    public async Task<IActionResult> UpdatePatientContact([FromBody]UpdatePatientContactCommand request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    [HttpDelete]
    [Route("Contacts")]
    public async Task<IActionResult> DeletePatientContact([FromQuery ]DeletePatientContactCommand request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
}
