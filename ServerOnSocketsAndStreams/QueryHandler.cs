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
        int numberOfClientRequestToConnect = 0;//TEST!!!!

        DateTime startTime;
        DateTime currentTime;
        TimeSpan dTime;

        public Socket clientSocket;
        public NetworkStream stream;

        public string Request = "";
        public byte[] byteRequest = new byte[1024];
        public byte[] byteResponse = new byte[1] { 0 };//дефолтное значение, чтоб на пустые запросы stream.Write не жаловался
        public Dictionary<string, string[]> ParsedRequest = new Dictionary<string, string[]>();

        public string clientIp;

        public ClientSession currentClient;

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
                    Console.WriteLine("\n>>Broken Task" + this.numberOfClientRequestToConnect + " close<<\n");
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
                    Handshake(ParsedRequest["Sec-WebSocket-Key"][0]);
                    ProcessWebSocketQuery();
                }
            }
            else//обычный http
            {
                if (Server.activeClients.ContainsKey(clientIp))
                {//непервый запрос от клиента 
                    if (Server.activeClients[clientIp] != null)
                    {
                        currentClient = Server.activeClients[clientIp];

                        if (Server.activeClients[clientIp].clientControllers.Count >= 4)
                            Server.activeClients[clientIp].clientControllers.RemoveFirst();

                        if (ParsedRequest.ContainsKey("Cookie"))
                            if (ParsedRequest["Cookie"][1] == currentClient.ClientCookie)
                                currentClient.clientStatus = ClientStatus.Authorized;
                        //если куки отсутствуют, либо по какойто причине не совпадают
                            else
                                currentClient.clientStatus = ClientStatus.Visitor;
                        else
                            currentClient.clientStatus = ClientStatus.Visitor;
                    }
                    else
                    {//если ip клиента есть среди активных клиентов, но соответствующего профайла почему то нет
                        currentClient = new ClientSession(clientSocket);
                        Server.activeClients[clientIp] = currentClient;
                        Server.activeClients[clientIp].clientControllers.AddLast(this);
                    }
                }
                else
                {//первый запрос от клиента 
                    currentClient = new ClientSession(clientSocket);
                    Server.activeClients.Add(clientIp, currentClient);
                    Server.activeClients[clientIp].clientControllers.AddLast(this);
                }

                ProcessHttpQuery();
            }
            #endregion
        }

        private void ProcessHttpQuery()
        {
            while (true)
            {
                byteResponse = null;
                #region "ОТПРАВКА ОТВЕТА"
                //htmlVariables = null;
                //switch (ParsedRequest.First().Key)
                //{
                //    case "GET":
                //        {
                //            switch (ParsedRequest.First().Value[0])
                //            {
                //                case "/":
                //                    htmlVariables = new List<string>();
                //                    htmlVariables.Add(((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString());
                //                    byteResponse = views.MainPage("MainPage", htmlVariables);
                //                    break;
                //                case "/favicon.ico":
                //                    byteResponse = new byte[1] { 1 };
                //                    break;
                //                case "/Help":
                //                    byteResponse = views.CreateHtmlByteCode("HelpPage", null);
                //                    break;
                //                case "/Method1":
                //                    {
                //                        if (currentClient.clientStatus == ClientStatus.Visitor)
                //                            byteResponse = views.CreateHtmlByteCode("WrongStatusPage", null);
                //                        else
                //                        {
                //                            byteResponse = views.CreateHtmlByteCode("PageWithImage", null);
                //                        }
                //                        break;
                //                    }
                //                case "/Method2":
                //                    {
                //                        byteResponse = views.CreateHtmlByteCode("PageWithImage", null);
                //                    }
                //                    break;
                //                case "/images/img1.png":
                //                    byteResponse = views.Image("img1.png");
                //                    break;
                //                case "/AuthorizationPage":
                //                    {
                //                        htmlVariables = new List<string>();
                //                        htmlVariables.Add("Enter login and password");
                //                        byteResponse = views.CreateHtmlByteCode("AuthorizationPage", htmlVariables);
                //                        break;
                //                    }
                //                case "/RegistrationPage":
                //                    {
                //                        htmlVariables = new List<string>();
                //                        htmlVariables.Add("Enter login and password");
                //                        byteResponse = views.CreateHtmlByteCode("RegistrationPage", htmlVariables);
                //                        break;
                //                    }
                //                case "/WebSocketView":
                //                    {
                //                        htmlVariables = new List<string>();
                //                        htmlVariables.Add("string for test of web socket query 123456 !@#$%^****");
                //                        byteResponse = views.CreateHtmlByteCode("WebSocketTest", htmlVariables);
                //                        break;
                //                    }
                //                default:
                //                    htmlVariables = new List<string>();
                //                    htmlVariables.Add(((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString());
                //                    byteResponse = views.MainPage("MainPage", htmlVariables);
                //                    break;
                //            }
                //            break;
                //        }
                //    case "POST":
                //        {
                //            switch (ParsedRequest.First().Value[0])
                //            {
                //                case "/AuthorizationPage":
                //                    {
                //                        string login = "";
                //                        //проверка на наличие в б/д такого аккаунта
                //                        if (!currentClient.AccountValidation(Request, out login))
                //                        {
                //                            htmlVariables = new List<string>();
                //                            htmlVariables.Add("Wrong login and/or password, enter again");
                //                            byteResponse = views.CreateHtmlByteCode("AuthorizationPage", htmlVariables);
                //                        }
                //                        //else if(проверить есть ли уже активный клиент по данному аккаунту)
                //                        //{ 
                //                        //    byteResponse = views.AuthorizationPage("Wrong login and/or password, enter again");
                //                        //}
                //                        else
                //                        {
                //                            currentClient.clientStatus = ClientStatus.Authorized;
                //                            currentClient.ClientLogin = login;
                //                            htmlVariables = new List<string>();
                //                            htmlVariables.Add(login);
                //                            byteResponse = views.CreateHtmlByteCode("AccountValidationCompletePage", htmlVariables, () =>
                //                            { 
                //                                return Guid.NewGuid().ToString(); 
                //                            });
                //                        }
                //                        break;
                //                    }
                //                case "/RegistrationPage":
                //                    {
                //                        //проверка на совпадение паролей во 2й и 3й полях для ввода
                //                        if (!currentClient.AccountVerification1(Request))
                //                        {
                //                            htmlVariables = new List<string>();
                //                            htmlVariables.Add("Wrong password, enter data again");
                //                            byteResponse = views.CreateHtmlByteCode("RegistrationPage", htmlVariables);
                //                        }
                //                        //проверка на наличие в б/д такого логина
                //                        else if (currentClient.AccountVerification2(Request))
                //                        {
                //                            htmlVariables = new List<string>();
                //                            htmlVariables.Add("Such login already exists, enter data again");
                //                            byteResponse = views.CreateHtmlByteCode("RegistrationPage", htmlVariables);
                //                        }
                //                        else
                //                        {
                //                            currentClient.AddAccountToDB(Request);//отправка логина и пароля в б/д
                //                            byteResponse = views.CreateHtmlByteCode("AccountVerificationCompletePage", null);
                //                        }
                //                        break;
                //                    }
                //            }
                //            break;
                //        }
                //    default:
                //        htmlVariables = new List<string>();
                //        htmlVariables.Add(((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString());
                //        byteResponse = views.MainPage("MainPage", htmlVariables);
                //        break;
                //}
                //stream.Write(byteResponse, 0, byteResponse.Length);
                //stream.Flush();
                #endregion
                #region "ОТПРАВКА ОТВЕТА TEST"
                HtmlRouter htmlRouter = new HtmlRouter(this, ParsedRequest);
                byteResponse = htmlRouter.BuildResponse();

                stream.Write(byteResponse, 0, byteResponse.Length);
                stream.Flush();
                #endregion

                Request = "";
                byteRequest = new byte[1024];
                #region "ПРИЕМ ВТОРИЧНЫХ ЗАПРОСОВ(если на странице есть ссылки на внешние файлы)"
                //Ожидаем вторичный запрос от сокета 5 сек.
                startTime = DateTime.Now;
                while (!stream.DataAvailable)
                {
                    currentTime = DateTime.Now;
                    dTime = currentTime - startTime;
                    if (dTime.TotalMilliseconds > 5000)
                    {
                        Console.WriteLine("\n>>Task" + numberOfClientRequestToConnect + " close<<\n");
                        stream.Close();
                        clientSocket.Dispose();
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

                #region "ПАРСИНГ ЗАПРОСА"
                ParsedRequest = ParseHttpRequest(Request);
                #endregion
            }
        }

        private void ProcessWebSocketQuery()
        {         
            while (true)
            {
                Request = "";
                byteRequest = new byte[1024];
                #region "ПРИЕМ ЗАПРОСОВ ПО WEB SOCKET"
                //Ожидаем запрос от сокета 5 сек.
                startTime = DateTime.Now;
                while (!stream.DataAvailable)
                {
                    currentTime = DateTime.Now;
                    dTime = currentTime - startTime;
                    if (dTime.TotalMilliseconds > 5000)
                    {
                        Console.WriteLine("\n>>Web_Socket_Task" + numberOfClientRequestToConnect + " close<<\n");
                        stream.Close();
                        clientSocket.Dispose();
                        return;
                    }
                    Thread.Sleep(500);
                }

                stream.Read(byteRequest, 0, byteRequest.Length);
                Request = DecodeWebSocketMessage(byteRequest);
                #endregion

                #region "Парсинг запроса"
                //подразумевается что по веб сокету приходят строки с '\r\n\r\n' в конце
                Request = Request.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
                Console.WriteLine("Запрос по WebSocket: \n{0}", Request);
                #endregion

                #region "Отправка веб сокету ответа"
                byteResponse = EncodeWebSocketMessage("BLABLABLA");
                stream.Write(byteResponse, 0, byteResponse.Length);
                stream.Flush();
                #endregion
            }
        }


        private string DecodeWebSocketMessage(byte[] bytes)
        {
            try
            {
                string incomingData = "";
                byte secondByte = bytes[1];

                int dataLength = secondByte & 127;
                int indexFirstMask = 2;

                if (dataLength == 126) indexFirstMask = 4;
                else if (dataLength == 127) indexFirstMask = 10;

                IEnumerable<byte> keys = bytes.Skip(indexFirstMask).Take(4);
                int indexFirstDataByte = indexFirstMask + 4;

                byte[] decoded = new byte[bytes.Length - indexFirstDataByte];
                for (int i = indexFirstDataByte, j = 0; i < bytes.Length; i++, j++)
                {
                    decoded[j] = (byte)(bytes[i] ^ keys.ElementAt(j % 4));
                }

                return incomingData = Encoding.UTF8.GetString(decoded, 0, decoded.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not decode due to :" + ex.Message);
            }
            return null;
        }

        private byte[] EncodeWebSocketMessage(string message)
        {
            byte[] response;
            byte[] bytesRaw = Encoding.UTF8.GetBytes(message);
            byte[] frame = new byte[10];

            int indexStartRawData = -1;
            int length = bytesRaw.Length;

            frame[0] = (byte)129;
            if (length <= 125)
            {
                frame[1] = (byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = (byte)126;
                frame[2] = (byte)((length >> 8) & 255);
                frame[3] = (byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = (byte)127;
                frame[2] = (byte)((length >> 56) & 255);
                frame[3] = (byte)((length >> 48) & 255);
                frame[4] = (byte)((length >> 40) & 255);
                frame[5] = (byte)((length >> 32) & 255);
                frame[6] = (byte)((length >> 24) & 255);
                frame[7] = (byte)((length >> 16) & 255);
                frame[8] = (byte)((length >> 8) & 255);
                frame[9] = (byte)(length & 255);

                indexStartRawData = 10;
            }

            response = new byte[indexStartRawData + length];

            int i, reponseIdx = 0;

            //Add the frame bytes to the reponse
            for (i = 0; i < indexStartRawData; i++)
            {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            //Add the data bytes to the response
            for (i = 0; i < length; i++)
            {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            return response;
        }

        private void Handshake(string keyHash)
        {
            string newKeyHash = ComputeWebSocketHandshakeSecurityHash(keyHash);

            string Response = "HTTP/1.1 101 Switching Protocols\r\n" +
                              "Upgrade: websocket\r\n" +
                              "Connection: Upgrade\r\n" +
                              "Sec-WebSocket-Accept: " + newKeyHash + "\r\n\r\n";
            var byteResponse = Encoding.UTF8.GetBytes(Response);

            Request = "";
            byteRequest = new byte[1024];
            stream.Write(byteResponse, 0, byteResponse.Length);
            stream.Flush(); 
        }

        private string ComputeWebSocketHandshakeSecurityHash(string secWebSocketKey)
        {
            string MagicKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            string secWebSocketAccept = "";

            string ret = secWebSocketKey + MagicKEY;

            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] sha1Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ret));

            secWebSocketAccept = Convert.ToBase64String(sha1Hash);

            return secWebSocketAccept;
        }

        private Dictionary<string, string[]> ParseHttpRequest(string request)
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

        private Dictionary<string, string[]> ParseWebSocketRequest(string request)
        {
            return null;
        }
    }
}
