using System;

namespace ServerlessDemo.Models
{
    public class Cracker
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string Description { get; set; }
        public bool IsRead { get; set; }
    }
}