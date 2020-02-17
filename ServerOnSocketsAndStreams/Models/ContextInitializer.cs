using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Models
{
    class ContextInitializer : DropCreateDatabaseAlways<ClientsContext>
    {
        MD5 md5 = MD5.Create();
        protected override void Seed(ClientsContext db)
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
