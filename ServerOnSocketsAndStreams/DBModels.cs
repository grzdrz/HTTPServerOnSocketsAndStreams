using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServerOnSocketsAndStreams
{
    public class Context : DbContext
    {
        public Context() : base("ServerDB")
        {}

        public DbSet<Client> Clients { get; set; }
    }

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
