using api_patient.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using patient.application.FoodPlans.CreateFoodPlan;

namespace api_patient.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodPlansController : ControllerBase
    {
        private readonly IMediator mediator;

        public FoodPlansController(IMediator _mediator)
        {
            mediator = _mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFoodPlan([FromBody] CreateFoodPlanCommand request)
            => this.HandleResult(await mediator.Send(request));
    }
}
