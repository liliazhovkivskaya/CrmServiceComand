namespace CrmServiceApp.Models
{
    public enum AppointmentStatus
    {
        Pending,  // Новая заявка
        Accepted, // Принята
        Rejected  // Отклонена
    }

    public class Appointment
    {
        public int Id { get; set; }
        public string Service { get; set; } = string.Empty;
        public DateTime Time { get; set; }

        // Удобнее держать статус enum, чем bool
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        // Связь с клиентом
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        // Если нужно оповещать в Telegram, можно хранить ChatId
        public long? TelegramChatId { get; set; }
    }

}
