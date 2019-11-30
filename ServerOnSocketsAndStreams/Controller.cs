using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams
{
    public class Controller
    {
        int numberOfClientRequestToConnect = 0;//TEST!!!!
        int streamTimer = 0;//таймер ожидания нового запроса

        public Socket clientSocket;
        public NetworkStream stream;
        //public StreamWriter streamWriter;
        public StreamReader streamReader;

        public string Request = "";
        public byte[] byteRequest = new byte[1024];
        //public string Response = "";
        public byte[] byteResponse = new byte[1] { 0 };//дефолтное значение, чтоб на пустые запросы stream.Write не жаловался

        //оперативные данные клиента
        string clientCookie;
        string clientIp;
        string clientKey;

        public Controller(Socket socket, int numberOfClientRequestToConnect)
        {
            this.numberOfClientRequestToConnect = numberOfClientRequestToConnect;
            this.clientSocket = socket;
            stream = new NetworkStream(clientSocket);
            clientIp = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();

            #region "ПРИЕМ ПЕРВОГО ЗАПРОСА"
            //Предпроверка нового клиента
            //Ожидаем запрос от сокета 5 сек. Если его нет, значит сокет бу.
            while (!stream.DataAvailable)
            {
                if (streamTimer > 10)
                    break;
                streamTimer++;
                Thread.Sleep(500);
            }
            if (streamTimer > 10)
            {
                Console.WriteLine("\n------------Broken Task" + this.numberOfClientRequestToConnect + " close------------\n");
                return;
            }
            else
                streamTimer = 0;
            int Count = 0;
            while ((Count = stream.Read(byteRequest, 0, byteRequest.Length)) > 0)
            {
                // Преобразуем эти данные в строку и добавим ее к переменной Request
                Request += Encoding.UTF8.GetString(byteRequest, 0, Count);
                // Запрос должен обрываться последовательностью \r\n\r\n
                if (Request.IndexOf("\r\n\r\n") >= 0)
                {
                    break;
                }
            }
            Console.WriteLine("Запрос: \n{0}", Request);
            #endregion

            #region "Анализ кукей"
            //извлечение из запроса куки
            string requestPattern = "((clientCookie=)([0-9])+)|((clientCookie= ))";
            Regex regex = new Regex(requestPattern);
            MatchCollection matchs = regex.Matches(Request);
            string fullCookie = "";
            foreach (Match e in matchs)
                fullCookie += e.Value;

            //если кукей нет генерим новое куки
            if (matchs.Count == 0)
                clientCookie = new Random().Next().ToString();
            //если есть то выпарсиваем его из запроса
            else
                clientCookie = fullCookie.Split(new string[] { "=", ";" }, StringSplitOptions.None)[1];

            //составляем ключ из айпи адреса и куки
            clientKey = clientIp + ":" + clientCookie;

            //если куки нет -> значит клиент делает первых запрос(новый сеанс), либо давно не заходил и браузер удалил куки
            //или если куки есть, но в списке активных клиентов такого клиента нет, то возможно клиент заходил давно, а куки не удалились
            if (matchs.Count == 0 || !Server.activeClients.ContainsKey(clientKey))
            {
                Server.activeClients[clientKey] = new ClientProfile(clientSocket);
                Server.activeClients[clientKey].clientControllers.AddLast(this);
                Server.activeClients[clientKey].ClientId = clientCookie;
                ClientQueryHandling(Request, Server.activeClients[clientKey].ClientId, Server.activeClients[clientKey]);
            }
            //есть активный клиент с таким ключем в списке -> он недавно заходил
            else
            {
                //ограничение: до 4х одновременно поддерживающихся запросов(socket+nstream) от 1го клиента
                if (Server.activeClients[clientKey].clientControllers.Count >= 4)
                    Server.activeClients[clientKey].clientControllers.RemoveFirst();
                Server.activeClients[clientKey].clientControllers.AddLast(this);
                ClientQueryHandling(Request, Server.activeClients[clientKey].ClientId, Server.activeClients[clientKey]);
            }
            #endregion
        }

        public void ClientQueryHandling(string firstRequest, string cookie, ClientProfile client)
        {
            var views = new Views()
            {
                Cookie = cookie
            };
            string requestPattern = "(GET|POST)(\\S|\\s|/)+";
            Regex regex;
            MatchCollection match;
            string tempRequest;
            string[] requestParsed = null;
            while (true)
            {
                #region "ПАРСИНГ ЗАПРОСА"
                //извлечение из запроса GET, POST строк
                regex = new Regex(requestPattern);
                match = regex.Matches(Request);
                tempRequest = null;
                foreach (var e in match) tempRequest += e;
                requestParsed = tempRequest?.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                #endregion

                #region "ОТПРАВКА ОТВЕТА"
                switch (requestParsed[0])
                {
                    case "GET":
                        {
                            switch (requestParsed[1])
                            {
                                case "/":
                                    byteResponse = views.StartPage();
                                    break;
                                case "/Help":
                                    byteResponse = views.Help();
                                    break;
                                case "/Method1":
                                    {
                                        if (client.clientStatus == ClientStatus.Visitor)
                                            byteResponse = views.WrongStatus();
                                        else
                                        {
                                            byteResponse = views.PageWithImage();
                                        }
                                        break;
                                    }
                                case "/Method2":
                                    {
                                        byteResponse = views.PageWithImage2();
                                    }
                                    break;
                                case "/images/img1.png":
                                    byteResponse = views.Image("img1.png");
                                    break;
                                case "/AuthorizationPage":
                                    {
                                        byteResponse = views.AuthorizationPage("Enter login and password");
                                        break;
                                    }
                                case "/favicon.ico":
                                    byteResponse = new byte[1] { 1 };
                                    break;
                                default:
                                    byteResponse = views.StartPage();
                                    break;
                            }
                            break;
                        }
                    case "POST":
                        {
                            switch (requestParsed[1])
                            {
                                case "/AuthorizationPage":
                                    {
                                        byteResponse = views.LoginVerification(Request);
                                        if (byteResponse == null)//если нул значит чтото в логине и/или парле введено неверно
                                            byteResponse = views.AuthorizationPage("Wrond  login and/or password, enter again");
                                        else
                                            client.ChangeLoginAndStatus(Request);
                                        break;
                                    }
                            }
                            break;
                        }
                    default:
                        byteResponse = views.StartPage();
                        break;
                }
                stream.Write(byteResponse, 0, byteResponse.Length);
                stream.Flush();
                Request = "";
                //Response = "";
                #endregion

                #region "ПРИЕМ ВТОРИЧНЫХ ЗАПРОСОВ(например если на странице есть изображения)"
                //Ожидаем вторичный запрос от сокета 5 сек.
                while (!stream.DataAvailable)
                {
                    if (streamTimer > 10)
                        break;
                    streamTimer++;
                    Thread.Sleep(500);
                }
                if (streamTimer > 10)
                {
                    Console.WriteLine("\n------------Task" + numberOfClientRequestToConnect + " close------------\n");
                    return;
                }
                else
                    streamTimer = 0;
                int Count = 0;
                while ((Count = stream.Read(byteRequest, 0, byteRequest.Length)) > 0)
                {
                    // Преобразуем эти данные в строку и добавим ее к переменной Request
                    Request += Encoding.UTF8.GetString(byteRequest, 0, Count);
                    // Запрос должен обрываться последовательностью \r\n\r\n
                    if (Request.IndexOf("\r\n\r\n") >= 0)
                    {
                        break;
                    }
                }
                Console.WriteLine("Запрос: \n{0}", Request);
                #endregion
            }
        }
    }
}
