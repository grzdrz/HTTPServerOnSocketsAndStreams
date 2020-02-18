using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Models
{
    public class ClientsContext : DbContext
    {
        public ClientsContext() : base("ServerDB")
        {
        }

        public DbSet<Client> Clients { get; set; }
    }
}
