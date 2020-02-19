using ServerOnSocketsAndStreams.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams
{
    public class Validation
    {
        //проверка на совпадение паролей при подтверждении пароля 
        public static bool ComparePasswords(Dictionary<string, string> nameAndPasswords)
        {
            if (nameAndPasswords.ContainsKey("SecondPassword") && nameAndPasswords.ContainsKey("FirstPassword"))
            {
                return nameAndPasswords["FirstPassword"] == nameAndPasswords["SecondPassword"];
            }
            return false;
        }

        //проверка на наличие введенного логина в б/д
        public static bool CheckForSuchLoginInDB(Dictionary<string, string> nameAndPasswords)
        {
            if (nameAndPasswords.ContainsKey("Name"))
            {
                try
                {
                    //запрос к б/д
                    using (ClientsContext db = new ClientsContext())
                    {
                        db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
                        string name = nameAndPasswords["Name"];
                        return db.Clients.Any(a => a.login == name);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            return false;
        }

        //проверка на наличие введенного логина и пароля в б/д клиентов
        public static bool CheckForSuchAccountInDB(Dictionary<string, string> nameAndPasswords, out string Login)
        {
            Login = "";//////
            if (nameAndPasswords.ContainsKey("Name") && nameAndPasswords.ContainsKey("FirstPassword"))
            {
                Login = nameAndPasswords["Name"];/////
                string name = nameAndPasswords["Name"];
                string passwordHash = Program.GetHash(nameAndPasswords["FirstPassword"]);

                try
                {
                    //запрос к б/д
                    using (ClientsContext db = new ClientsContext())
                    {
                        db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
                        return db.Clients.Any(a => a.login == name && a.passwordHash == passwordHash);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            return false;
        }
    }
}
