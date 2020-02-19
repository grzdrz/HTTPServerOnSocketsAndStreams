using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Models
{
    public class DBManager
    {
        public static void AddAccountToDB(Dictionary<string, string> nameAndPasswords)
        {
            if (nameAndPasswords.ContainsKey("Name") && nameAndPasswords.ContainsKey("FirstPassword"))
            {
                try
                {
                    //запрос к б/д
                    using (ClientsContext db = new ClientsContext())
                    {
                        db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
                        var newClient = new Client()
                        {
                            login = nameAndPasswords["Name"],
                            passwordHash = Program.GetHash(nameAndPasswords["FirstPassword"])
                        };
                        db.Clients.Attach(newClient);
                        db.Entry(newClient).State = EntityState.Added;
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            else throw new Exception("EMPTY_NAME_OR_PASSWORD_POST_PARAMETERS");
        }
    }
}
