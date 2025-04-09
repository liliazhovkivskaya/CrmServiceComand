namespace CrmServiceApp.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public List<Appointment> Appointments { get; set; } = new();
    }
}
