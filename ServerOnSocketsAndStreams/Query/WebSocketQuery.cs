using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Query
{
    public class WebSocketQuery : IQuery
    {
        DateTime startTime;
        DateTime currentTime;
        TimeSpan dTime;

        QueryHandler QueryContext;

        public void ProcessQuery(QueryHandler queryContext)
        {
            QueryContext = queryContext;

            Handshake(queryContext.ParsedRequest["Sec-WebSocket-Key"][0]);

            while (true)
            {
                queryContext.Request = "";
                queryContext.byteRequest = new byte[1024];
                #region "ПРИЕМ ЗАПРОСОВ ПО WEB SOCKET"
                //Ожидаем запрос от сокета 5 сек.
                startTime = DateTime.Now;
                while (!queryContext.stream.DataAvailable)
                {
                    currentTime = DateTime.Now;
                    dTime = currentTime - startTime;
                    if (dTime.TotalMilliseconds > 5000)
                    {
                        Console.WriteLine("\n>>Web_Socket_Task" + queryContext.numberOfClientRequestToConnect + " close<<\n");
                        queryContext.stream.Close();
                        queryContext.clientSocket.Dispose();
                        return;
                    }
                    Thread.Sleep(500);
                }

                queryContext.stream.Read(queryContext.byteRequest, 0, queryContext.byteRequest.Length);
                queryContext.Request = DecodeWebSocketMessage(queryContext.byteRequest);
                #endregion

                #region "Парсинг запроса"
                //подразумевается что по веб сокету приходят строки с '\r\n\r\n' в конце
                queryContext.Request = queryContext.Request.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
                Console.WriteLine("Запрос по WebSocket: \n{0}", queryContext.Request);
                #endregion

                #region "Отправка веб сокету ответа"
                queryContext.byteResponse = EncodeWebSocketMessage("BLABLABLA");
                queryContext.stream.Write(queryContext.byteResponse, 0, queryContext.byteResponse.Length);
                queryContext.stream.Flush();
                #endregion
            }
        }

        public string DecodeWebSocketMessage(byte[] bytes)
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

        public byte[] EncodeWebSocketMessage(string message)
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

        public void Handshake(string keyHash)
        {
            string newKeyHash = ComputeWebSocketHandshakeSecurityHash(keyHash);

            string Response = "HTTP/1.1 101 Switching Protocols\r\n" +
                              "Upgrade: websocket\r\n" +
                              "Connection: Upgrade\r\n" +
                              "Sec-WebSocket-Accept: " + newKeyHash + "\r\n\r\n";
            var byteResponse = Encoding.UTF8.GetBytes(Response);

            QueryContext.Request = "";
            QueryContext.byteRequest = new byte[1024];
            QueryContext.stream.Write(byteResponse, 0, byteResponse.Length);
            QueryContext.stream.Flush();
        }

        public string ComputeWebSocketHandshakeSecurityHash(string secWebSocketKey)
        {
            string MagicKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            string secWebSocketAccept = "";

            string ret = secWebSocketKey + MagicKEY;

            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] sha1Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ret));

            secWebSocketAccept = Convert.ToBase64String(sha1Hash);

            return secWebSocketAccept;
        }
    }
}
