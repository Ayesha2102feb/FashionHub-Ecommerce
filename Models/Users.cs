namespace EcomWebsite.Models
{
    public class Users
    {
        public int id { get; set; }

        public string? email { get; set; }

        public string?password { get; set; }
        public string? phone { get; set; }

        public string? role { get; set; }
    }
}
