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

        public void ChangeLoginAndStatus(string nameAndPassword)
        {
            string Name = "";
            string Password = "";

            string NamePattern = "(Name=)([a-z])+";
            Regex regex = new Regex(NamePattern);
            MatchCollection matchs = regex.Matches(nameAndPassword);
            foreach (var e in matchs)
                Name = e.ToString().Split('=')[1];

            string PasswordPattern = "(&Password=)(\\s|\\S)+";
            regex = new Regex(PasswordPattern);
            matchs = regex.Matches(nameAndPassword);
            foreach (var e in matchs)
                Password = e.ToString().Split('=')[1];

            this.clientStatus = ClientStatus.User;
        }
    }
}
