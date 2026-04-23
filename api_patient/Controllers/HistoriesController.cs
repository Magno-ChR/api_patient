using api_patient.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using patient.application.Histories.CreateHistory;
using patient.application.Histories.GetHistory;
using patient.application.Histories.UpdateHistory;

namespace api_patient.Controllers
{
    [Route("api/patient/[controller]")]
    [ApiController]
    [Authorize(Roles = "patient,admin,doctor")]
    public class HistoriesController : ControllerBase
    {
        private readonly IMediator mediator;

        public HistoriesController(IMediator _mediator)
        {
            mediator = _mediator;
        }

        [HttpGet("GetById")]
        public async Task<IActionResult> GetHistory([FromQuery] GetHistoryCommand request)
            => this.HandleResult(await mediator.Send(request));

        [HttpGet("GetByPatientId")]
        [HttpGet("GetListByPatientId")]
        public async Task<IActionResult> GetHistoryListByPatientId([FromQuery] GetHistoryListByPatientIdCommand request)
            => this.HandleResult(await mediator.Send(request));

        [HttpPost]
        public async Task<IActionResult> CreateHistory([FromBody] CreateHistoryCommand request)
            => this.HandleResult(await mediator.Send(request));

        [HttpPut]
        public async Task<IActionResult> UpdateHistory([FromBody] UpdateHistoryCommand request)
            => this.HandleResult(await mediator.Send(request));

        [HttpPost]
        [Route("Backgrouds")]
        public async Task<IActionResult> CreateBackgroundHistory([FromBody] CreateBackgroudCommand request)
            => this.HandleResult(await mediator.Send(request));

        [HttpPut]
        [Route("Backgrouds")]
        public async Task<IActionResult> UpdateBackgroundHistory([FromBody] UpdateBackgroundCommand request)
            => this.HandleResult(await mediator.Send(request));

        [HttpPost]
        [Route("Evolutions")]
        public async Task<IActionResult> CreateEvolutionHistory([FromBody] CreateEvolutionCommand request)
            => this.HandleResult(await mediator.Send(request));

        [HttpPut]
        [Route("Evolutions")]
        public async Task<IActionResult> UpdateEvolutionHistory([FromBody] UpdateEvolutionCommand request)
            => this.HandleResult(await mediator.Send(request));
    }
}
