using CrmServiceApp.Data;
using CrmServiceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmServiceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Фильтрация записей (по имени клиента, дате и статусу).
        /// Пример: GET /api/appointments/filter?name=Иван&date=2025-03-30&status=Accepted
        /// </summary>
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Appointment>>> FilterAppointments(
            [FromQuery] string? name,
            [FromQuery] DateTime? date,
            [FromQuery] AppointmentStatus? status)
        {
            var query = _context.Appointments
                                .Include(a => a.Client)
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                // Поиск по части имени клиента
                query = query.Where(a => a.Client.FullName.Contains(name));
            }

            if (date.HasValue)
            {
                // Сравниваем только дату без учёта времени
                query = query.Where(a => a.Time.Date == date.Value.Date);
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var result = await query.ToListAsync();
            return Ok(result);
        }

        /// <summary>
        /// Вернуть все записи (без фильтра).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
        {
            return await _context.Appointments
                                 .Include(a => a.Client)
                                 .ToListAsync();
        }

        /// <summary>
        /// Вернуть запись по ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetAppointment(int id)
        {
            var appointment = await _context.Appointments
                                            .Include(a => a.Client)
                                            .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            return appointment;
        }

        /// <summary>
        /// Создать новую запись (Appointment).
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Appointment>> CreateAppointment(Appointment appointment)
        {
            // По умолчанию Status = Pending
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointment);
        }

        /// <summary>
        /// Принять запись (установить Status = Accepted).
        /// </summary>
        [HttpPut("{id}/accept")]
        public async Task<IActionResult> AcceptAppointment(int id)
        {
            var appointment = await _context.Appointments
                                            .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            appointment.Status = AppointmentStatus.Accepted;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Отклонить запись (установить Status = Rejected).
        /// </summary>
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectAppointment(int id)
        {
            var appointment = await _context.Appointments
                                            .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            appointment.Status = AppointmentStatus.Rejected;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Удалить запись по ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
