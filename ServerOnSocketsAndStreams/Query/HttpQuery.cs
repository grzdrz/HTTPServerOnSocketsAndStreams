using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Query
{
    public class HttpQuery : IQuery
    {
        DateTime startTime;
        DateTime currentTime;
        TimeSpan dTime;

        public void ProcessQuery(QueryHandler queryContext)
        {
            while (true)
            {
                queryContext.byteResponse = null;
                #region "ОТПРАВКА ОТВЕТА"
                HtmlRouter htmlRouter = new HtmlRouter(queryContext);
                queryContext.byteResponse = htmlRouter.BuildResponse();

                queryContext.stream.Write(queryContext.byteResponse, 0, queryContext.byteResponse.Length);
                queryContext.stream.Flush();
                #endregion

                queryContext.Request = "";
                queryContext.byteRequest = new byte[1024];
                #region "ПРИЕМ ВТОРИЧНЫХ ЗАПРОСОВ(если на странице есть ссылки на внешние файлы)"
                //Ожидаем вторичный запрос от сокета 5 сек.
                startTime = DateTime.Now;
                while (!queryContext.stream.DataAvailable)
                {
                    currentTime = DateTime.Now;
                    dTime = currentTime - startTime;
                    if (dTime.TotalMilliseconds > 5000)
                    {
                        Console.WriteLine("\n>>Task" + queryContext.numberOfClientRequestToConnect + " close<<\n");
                        queryContext.stream.Close();
                        queryContext.clientSocket.Dispose();
                        return;
                    }
                    Thread.Sleep(500);
                }
                int Count = 0;
                while ((Count = queryContext.stream.Read(queryContext.byteRequest, 0, queryContext.byteRequest.Length)) > 0)
                {
                    // Преобразуем эти данные в строку и добавим ее к переменной Request
                    queryContext.Request += Encoding.UTF8.GetString(queryContext.byteRequest, 0, Count);
                    // Запрос должен обрываться последовательностью \r\n\r\n
                    if (queryContext.Request.IndexOf("\r\n\r\n") >= 0)
                    {
                        break;
                    }
                }
                Console.WriteLine("Запрос: \n{0}", queryContext.Request);
                #endregion

                #region "ПАРСИНГ ЗАПРОСА"
                queryContext.ParsedRequest = queryContext.ParseHttpRequest(queryContext.Request);
                #endregion
            }
        }
    }
}
