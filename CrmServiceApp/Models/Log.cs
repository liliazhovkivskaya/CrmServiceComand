using CrmServiceApp.Enums;

namespace CrmServiceApp.Models
{
    public class Log
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public virtual Client Client { get; set; }

        public string Discription {  get; set; }

        public LogType LogType { get; set; }

        public string? Exception { get; set; }

    }
}
