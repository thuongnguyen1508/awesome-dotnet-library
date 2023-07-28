using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TN.EventBus;
using TN.Example.Application.IntegrationEvents.Events;

namespace TN.Example.Controllers
{
    [Route("api/pubsub")]
    [ApiController]
    public class PubSubController : ControllerBase
    {
        private readonly IEventBus _eventBus;
        public PubSubController(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
        [HttpPost("send-message")]
        public async Task<IActionResult> Post([FromBody] string value)
        {
            var event1 = new MessageSentEvent { Message = value };
            var event2 = new MessageDeliveredEvent { Message = value };
            await _eventBus.PublishAsync(event1);
            await _eventBus.PublishAsync(event2);
            return Ok(value);
        }
    }
}
