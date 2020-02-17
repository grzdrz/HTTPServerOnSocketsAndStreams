using System;

namespace ServerOnSocketsAndStreams.Models
{
    public class Client
    {
        public int Id { get; set; }

        public string login { get; set; }
        public string passwordHash { get; set; }

        public string email { get; set; }
        public string telephoneNumber { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string LastName { get; set; }
    }
}
