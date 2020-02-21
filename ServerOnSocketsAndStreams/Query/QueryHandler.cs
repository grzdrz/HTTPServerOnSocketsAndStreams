using ServerOnSocketsAndStreams.Query;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams
{
    public class QueryHandler
    {
        public int numberOfClientRequestToConnect = 0;//TEST!!!!

        public DateTime startTime;
        public DateTime currentTime;
        public TimeSpan dTime;

        public Socket clientSocket;
        public NetworkStream stream;

        public string Request = "";
        public byte[] byteRequest = new byte[1024];
        public byte[] byteResponse;
        public Dictionary<string, string[]> ParsedRequest = new Dictionary<string, string[]>();

        public string clientIp;

        public ClientSession currentClient;

        public IQuery Query;
        public QueryHandler(Socket socket, int numberOfClientRequestToConnect)
        {
            this.numberOfClientRequestToConnect = numberOfClientRequestToConnect;
            this.clientSocket = socket;
            stream = new NetworkStream(clientSocket);
            clientIp = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();

            #region "ПРИЕМ ПЕРВОГО ЗАПРОСА"
            //Предпроверка нового клиента
            //Ожидаем запрос от сокета 5 сек. Если его нет, значит сокет бу.
            startTime = DateTime.Now;
            while (!stream.DataAvailable)
            {
                currentTime = DateTime.Now;
                dTime = currentTime - startTime;
                if (dTime.TotalMilliseconds > 5000)
                {
                    Console.WriteLine("\n>>Broken request" + this.numberOfClientRequestToConnect + " close<<\n");
                    return;
                }
                Thread.Sleep(500);
            }
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

            #region "Парсим запрос"
            ParsedRequest = ParseHttpRequest(Request);
            #endregion

            #region "Выбор способа обработки запроса по типу запроса(обычный http, web socket, ...)"
            if (ParsedRequest.ContainsKey("Upgrade"))//вебсокет
            {
                if (ParsedRequest["Upgrade"][0] == "websocket")
                {
                    Query = new WebSocketQuery();
                    Query.ProcessQuery(this);
                }
            }
            else//обычный http
            {
                if (ParsedRequest.ContainsKey("Cookie"))
                {
                    if (Server.ActiveClients.ContainsKey(ParsedRequest["Cookie"][1]))
                    {
                        //в запросе есть куки и в списке активных клиентов есть ключ с этими куки
                        //значит клиент авторизован
                        currentClient = Server.ActiveClients[ParsedRequest["Cookie"][1]];
                        currentClient.clientStatus = ClientStatus.Authorized;
                        currentClient.queryHandler = this;
                    }
                    else
                    {
                        //в запросе есть куки, но в списке активных клиентов нет ключа с этими куки
                        //значит клиент не авторизован, но мог уже быть авторизован на этом сервере,
                        //а сессия с этими куки по какимто причинам была удалена из словаря
                        currentClient = new ClientSession(clientSocket);
                        currentClient.clientStatus = ClientStatus.Visitor;
                        currentClient.queryHandler = this;
                    }
                }
                else
                {
                    //в запросе по какой то причине нет куки(еще ниразу не авторизовывался, истек срок годности и куки удалились, ...)
                    //значит клиент не авторизован
                    currentClient = new ClientSession(clientSocket);
                    currentClient.clientStatus = ClientStatus.Visitor;
                    currentClient.queryHandler = this;
                }

                Query = new HttpQuery();
                Query.ProcessQuery(this);
            }
            #endregion
        }

        public Dictionary<string, string[]> ParseHttpRequest(string request)
        {
            string[] tempReq = request.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);//массив пакетов строк
            var dict = new Dictionary<string, string[]>();//словарь пар - заголовок: строка после заголовка
            string[] temp1 = null;
            Regex regex = new Regex("(GET )|(POST )");
            foreach (var e in tempReq)
            {
                if (regex.IsMatch(e))
                {
                    temp1 = e.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    dict.Add("MainLine", temp1.ToArray());
                }
                else
                {
                    temp1 = e.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                    dict.Add(temp1[0], temp1.Skip(1).ToArray());
                }
            }

           
            if (dict.ContainsKey("Cookie"))
            {
                regex = new Regex("(cookie1=)(.+)(;*)");
                dict["Cookie"] = regex.Match(dict["Cookie"][0]).Value.Split('=');
            }

            return dict;
        }

        public Dictionary<string, string[]> ParseWebSocketRequest(string request)
        {
            return null;
        }
    }
}
