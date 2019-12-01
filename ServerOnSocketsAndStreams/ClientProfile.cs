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

namespace ServerOnSocketsAndStreams
{
    public enum ClientStatus
    {
        Visitor,
        User
    };

    //сущность клиента в б/д текущих клиентов(клиенты-сокеты полученные при подключении к серверу)
    public class ClientProfile
    {
        //ограничение на количество одновременно поддерживающихся запросов(socket+nstream)
        public LinkedList<Controller> clientControllers = new LinkedList<Controller>();

        public string ClientId;

        //главный критерий клиента
        public Socket currentClientSocket;

        public string Name;
        public string Password;
        public ClientStatus clientStatus;

        public ClientProfile(Socket currentClientSocket)
        {
            this.currentClientSocket = currentClientSocket;
            clientStatus = ClientStatus.Visitor;
        }

        //проверка на наличие 2х одинаковых паролей во 2й и 3й строках
        //и корректного логина
        public bool AccountVerification1(string nameAndPasswords)
        {
            string PasswordPattern = "((Password=)(\\s|\\S)+)(&)\\1";
            string LoginPattern = "(Name=)([a-z]|[0-9])+";
            Regex regex1 = new Regex(PasswordPattern);
            Regex regex2 = new Regex(LoginPattern);
            MatchCollection matchs1 = regex1.Matches(nameAndPasswords);
            MatchCollection matchs2 = regex2.Matches(nameAndPasswords);
            return matchs1.Count != 0 && matchs2.Count != 0;
        }

        //проверка на наличие введенного логина в б/д клиентов
        public bool AccountVerification2(string nameAndPasswords)
        {
            string LoginPattern = "(Name=)([a-z]|[0-9])+";
            Regex regex = new Regex(LoginPattern);
            MatchCollection matchs = regex.Matches(nameAndPasswords);
            string login = "";
            foreach (Match e in matchs) login += e.Value;
            login = login.Split('=')[1];
            int loginHesh = login.GetHashCode();

            //запрос к б/д
            var db = new Context();
            var clientLogin = db.Clients.Where(a => a.loginHesh == loginHesh).ToList();

            return clientLogin.Count != 0;//есть такой логин
        }

        public void AddAccountToDB(string nameAndPasswords)
        {
            string Login = "";
            string Password = "";

            string NamePattern = "(Name=)([a-z]|[0-9])+";
            Regex regex = new Regex(NamePattern);
            MatchCollection matchs = regex.Matches(nameAndPasswords);
            foreach (Match e in matchs)
            {
                Login += e.Value;
                break;
            }
            Login = Login.Split('=')[1];

            string PasswordPattern = "(&Password=)(\\s|\\S)+(&)";
            regex = new Regex(PasswordPattern);
            matchs = regex.Matches(nameAndPasswords);
            foreach (Match e in matchs)
            {
                Password += e.Value;
                break;
            }
            Password = Password.Split('=', '&')[2];

            //запрос к б/д
            var db = new Context();
            var newClient = new Client()
            {
                loginHesh = Login.GetHashCode(),
                passwordHesh = Password.GetHashCode()
            };
            db.Clients.Attach(newClient);
            db.Entry(newClient).State = EntityState.Added;
            db.SaveChanges();
        }

        //проверка на наличие введенного логина в б/д клиентов
        public bool AccountValidation(string nameAndPasswords)
        {
            string Login = "";
            string Password = "";

            string NamePattern = "(Name=)([a-z]|[0-9])+";
            Regex regex = new Regex(NamePattern);
            MatchCollection matchs = regex.Matches(nameAndPasswords);
            foreach (Match e in matchs)
            {
                Login += e.Value;
                break;
            }
            Login = Login.Split('=')[1];

            string PasswordPattern = "(&Password=)(\\s|\\S)+";
            regex = new Regex(PasswordPattern);
            matchs = regex.Matches(nameAndPasswords);
            foreach (Match e in matchs)
            {
                Password += e.Value;
                break;
            }
            Password = Password.Split('=')[1];

            int loginHesh = Login.GetHashCode();
            int passwordHesh = Password.GetHashCode();

            //запрос к б/д
            var db = new Context();
            var clientLogin = db.Clients.Where(a => a.loginHesh == loginHesh && a.passwordHesh == passwordHesh).ToList();

            return clientLogin.Count != 0;//есть такой логин
        }
    }
}
