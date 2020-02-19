using ServerOnSocketsAndStreams.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams
{
    public class Server
    {
        public static int PORT = 8005;
        public static IPAddress Ip = GetLocalIPAddress();
        public IPEndPoint EndPoint;

        //["IPv4"]:{client_profile_object}
        //список клиентов обратившихся к серверу, составленный по их IP адресам
        public static Dictionary<string, ClientSession> ActiveClients = new Dictionary<string, ClientSession>();

        int numberOfClientRequestToConnect = 0;//TEST

        //запуск сервера
        public Server()
        {
            Database.SetInitializer<ClientsContext>(new ContextInitializer());

            EndPoint = new IPEndPoint(Ip, PORT);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(EndPoint);
            serverSocket.Listen(10);
            Console.WriteLine("SERVER STARTED");
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                Console.WriteLine("\n===>New request accept " + (++numberOfClientRequestToConnect) + "_<===\n");
                Task task = Task.Run(() =>
                {
                    QueryHandler controller = new QueryHandler(clientSocket, numberOfClientRequestToConnect);
                });
            }
        }

        public static IPAddress GetLocalIPAddress()
        {
            return Dns.GetHostAddresses("").FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }
    }
}


//steam.DataAvailable меняется на false при первом считывании из потока, т.е. если прислать несколько пакетов через поток, 
//то цикл while(stream.DataAvailable) для непрерывного считывания этих пакетов за раз не поможет,
//т.к. после первой итерации DataAvailable сменит значение на false и цикл прервется.
//Для счета данных до конца нужно либо знать что из себя представляет последний пакет, чтобы определить когда прервать бесконечный цикл считываний,
//либо знать сколько всего пакетов должно быть прислано и на каждый писать streamReader.ReadLine().

//Браузер присылает запрос на подключение(создание нового клиентского сокета на сервере) при каждом новом вводе адреса в адресной строк 
//или при переходе по ссылке, и соответственно на каждый запрос нужно создавать свой stream-поток,
//НО если браузер запросит html-страницу, на которой есть например изображение, то ПО ЭТОМУ ЖЕ stream'у будет послан запрос на картинку
//(адрес в запросе совпадает с ссылкой в теге картинки)
