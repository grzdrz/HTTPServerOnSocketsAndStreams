using ServerOnSocketsAndStreams.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ServerOnSocketsAndStreams.Views;

namespace ServerOnSocketsAndStreams
{
    public class HtmlRouter
    {
        Dictionary<string, string> RequestUrlElements;
        QueryHandler QueryHandlerContext;

        public HtmlRouter(QueryHandler queryHandler)
        {
            QueryHandlerContext = queryHandler;
            RequestUrlElements = ParseRequestFirstLine(queryHandler.ParsedRequest);
        }

        public Dictionary<string, string> ParseRequestFirstLine(Dictionary<string, string[]> parsedRequest)
        {
            var result = new Dictionary<string, string>();
            string temp;

            result.Add("Method", parsedRequest["MainLine"][0]);//GET POST ...

            temp = new Regex(@"\/.*\?|\/.*").Match(parsedRequest["MainLine"][1])?.ToString();
            if (temp != null)
            {
                if (temp.EndsWith("?"))
                    temp = new String(temp.Take(temp.Length - 1).ToArray());//отрезаем крайние символы
                result.Add("Path", temp);// path/bla/bla/bla ...
            }

            temp = new Regex(@"\?.*").Match(parsedRequest["MainLine"][1])?.ToString();
            if (temp != null)
            {
                temp = new String(temp.Skip(1).ToList().ToArray());
                result.Add("Parameters", temp);// ?key1=value1&key2=value2&...
            }

            return result;
        }

        public byte[] BuildResponse()
        {
            Controller controller;

            switch (RequestUrlElements["Path"])
            {
                case "/favicon.ico":
                    return ViewsManager.CreateImageByteCode("favicon.ico");
                case "/":
                    controller = new MainPageController(QueryHandlerContext);
                    break;
                case "/Help":
                    controller = new HelpController(QueryHandlerContext);
                    break;
                case "/SomePage":
                    controller = new SomePageController(QueryHandlerContext);
                    break;
                case "/images/img1.png":////
                    return ViewsManager.CreateImageByteCode("img1.jpg");
                case "/AuthorizationPage":
                    controller = new AuthorizationController(QueryHandlerContext);
                    break;
                case "/RegistrationPage":
                    controller = new RegistrationController(QueryHandlerContext);
                    break;
                case "/WebSocketView":
                    controller = new WebSocketController(QueryHandlerContext);
                    break;
                default:
                    return ViewsManager.CreateErrorPageByteCode();
            }

            return controller.GetViewPage(RequestUrlElements);
        }
    }
}
