using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

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

    class ContextInitializer : DropCreateDatabaseAlways<Context>
    {
        MD5 md5 = MD5.Create();
        protected override void Seed(Context db)
        {
            Client p1 = new Client 
            { 
                login = "QWERTY",
                passwordHash = Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes("QWERTY")))
            };
            Client p2 = new Client 
            {
                login = "qqqq",
                passwordHash = Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes("qqqq1111")))
            };

            db.Clients.Add(p1);
            db.Clients.Add(p2);
            db.SaveChanges();
            base.Seed(db);
        }
    }
}
