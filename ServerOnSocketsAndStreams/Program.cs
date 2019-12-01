using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server();

            ////создание таблицы через CodeFirst
            //var db = new Context();
            //db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
            //Client client = new Client();
            //db.Clients.Attach(client);
            //db.Entry(client).State = System.Data.Entity.EntityState.Added;
            //db.SaveChanges();
            //Console.WriteLine("end");

            Console.ReadKey();
        }
    }
}

//прикрутить к регистрации обязательный email с проверкой
