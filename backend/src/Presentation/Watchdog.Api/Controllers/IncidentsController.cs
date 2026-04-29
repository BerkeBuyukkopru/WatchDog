using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Watchdog.Application.DTOs.Monitoring;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // Geçici olarak test için kapatılabilir veya durabilir
    public class IncidentsController : ControllerBase
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly Watchdog.Application.Interfaces.ExternalClients.IStatusBroadcaster _statusBroadcaster;

        public IncidentsController(IIncidentRepository incidentRepository, Watchdog.Application.Interfaces.ExternalClients.IStatusBroadcaster statusBroadcaster)
        {
            _incidentRepository = incidentRepository;
            _statusBroadcaster = statusBroadcaster;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IncidentDto>>> GetAll([FromQuery] Guid? appId)
        {
            var incidents = await _incidentRepository.GetAllAsync(appId);
            var dtos = incidents.Select(i => new IncidentDto
            {
                Id = i.Id,
                AppId = i.AppId, // SignalR filtresi için gerekli
                AppName = i.App?.Name ?? "Bilinmeyen Uygulama",
                FailedComponent = i.FailedComponent,
                ErrorMessage = i.ErrorMessage,
                StartedAt = i.StartedAt,
                ResolvedAt = i.ResolvedAt
            });

            return Ok(dtos);
        }

        [HttpPatch("{id}/resolve")]
        public async Task<IActionResult> Resolve(Guid id)
        {
            var incident = await _incidentRepository.GetByIdAsync(id);
            if (incident == null)
            {
                return NotFound(new { message = "Olay bulunamadı." });
            }

            if (incident.ResolvedAt.HasValue)
            {
                return BadRequest(new { message = "Bu olay zaten çözülmüş." });
            }

            incident.ResolvedAt = DateTime.UtcNow;
            await _incidentRepository.UpdateAsync(incident);

            // 🚨 CANLI GÜNCELLEME: Tüm dashboardlara bildir
            var dto = new IncidentDto
            {
                Id = incident.Id,
                AppId = incident.AppId,
                AppName = incident.App?.Name ?? "Uygulama",
                FailedComponent = incident.FailedComponent,
                ErrorMessage = incident.ErrorMessage,
                StartedAt = incident.StartedAt,
                ResolvedAt = incident.ResolvedAt
            };
            await _statusBroadcaster.BroadcastResolvedIncidentAsync(dto);

            return Ok(new { message = "Olay çözüldü olarak işaretlendi." });
        }
    }
}
