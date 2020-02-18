using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Data.Entity;
using System.Security.Cryptography;
using ServerOnSocketsAndStreams.Models;

namespace ServerOnSocketsAndStreams
{
    public enum ClientStatus
    {
        Visitor,
        Authorized
    };

    //сущность клиента в б/д текущих клиентов(клиенты-сокеты полученные при подключении к серверу)
    public class ClientSession
    {
        //ограничение на количество одновременно поддерживающихся запросов(socket+nstream)
        public LinkedList<QueryHandler> clientControllers = new LinkedList<QueryHandler>();

        public Socket currentClientSocket;
        public string ClientCookie;
        public string ClientLogin;
        public ClientStatus clientStatus;

        public ClientSession(Socket currentClientSocket)
        {
            this.currentClientSocket = currentClientSocket;
            clientStatus = ClientStatus.Visitor;
        }


        //проверка на наличие 2х одинаковых паролей во 2й и 3й строках
        //и корректного логина
        public bool AccountVerification1(Dictionary<string, string> nameAndPasswords)
        {
            if (nameAndPasswords.ContainsKey("SecondPassword") && nameAndPasswords.ContainsKey("FirstPassword"))
            {
                return nameAndPasswords["FirstPassword"] == nameAndPasswords["SecondPassword"];
            }
            return false;
        }

        //проверка на наличие введенного логина в б/д клиентов
        public bool AccountVerification2(Dictionary<string, string> nameAndPasswords)
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

        public void AddAccountToDB(Dictionary<string, string> nameAndPasswords)
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
                            passwordHash = GetHash(nameAndPasswords["FirstPassword"])
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

        //проверка на наличие введенного логина и пароля в б/д клиентов
        public bool AccountValidation(Dictionary<string, string> nameAndPasswords, out string Login)
        {
            Login = "";//////
            if (nameAndPasswords.ContainsKey("Name") && nameAndPasswords.ContainsKey("FirstPassword"))
            {
                Login = nameAndPasswords["Name"];/////
                string name = nameAndPasswords["Name"];
                string passwordHash = GetHash(nameAndPasswords["FirstPassword"]);

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

        public string GetHash(string input)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }
    }
}

//LINQ to Entity в теле выражений-аргументов ПЕРЕВАРИВАЕТ ТОЛЬКО обычные переменные

//Сработает
//string name = "...";
//db.Clients.Any(a => a.login == name && a.passwordHash == passwordHash); 

//Выкинет ошибку
//dict["Name"]="...";
//db.Clients.Any(a => a.login == dict["Name"] && a.passwordHash == passwordHash);