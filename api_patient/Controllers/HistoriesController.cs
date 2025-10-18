using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using patient.application.Histories.CreateHistory;

namespace api_patient.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoriesController : ControllerBase
    {
        private readonly IMediator mediator;

        public HistoriesController(IMediator _mediator)
        {
            mediator = _mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateHistory([FromBody] CreateHistoryCommand request)
            => Ok(await mediator.Send(request));

        [HttpPost]
        [Route("Backgrouds")]
        public async Task<IActionResult> CreateBackgroundHistory([FromBody] CreateBackgroudCommand request)
            => Ok(await mediator.Send(request));

        [HttpPost]
        [Route("Evolutions")]
        public async Task<IActionResult> CreateEvolutionHistory([FromBody] CreateEvolutionCommand request)
            => Ok(await mediator.Send(request));
    }
}
