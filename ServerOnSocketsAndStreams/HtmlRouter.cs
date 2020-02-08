using ServerOnSocketsAndStreams.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams
{
    public class HtmlRouter
    {
        Dictionary<string, string> RequestUrlElements;
        QueryHandler QueryHandlerContext;

        public HtmlRouter(QueryHandler queryHandler, Dictionary<string, string[]> parsedRequest)
        {
            QueryHandlerContext = queryHandler;

            RequestUrlElements = new Dictionary<string, string>();

            RequestUrlElements.Add("Method", parsedRequest["MainLine"][0]);//GET POST ...
            RequestUrlElements.Add("Path", parsedRequest["MainLine"][1]);//path/blablabla ...
            //if (parsedRequest["MainLine"].Length > 2)
            //    RequestUrlElements.Add("Parameters", parsedRequest["MainLine"][2]);//?key1=value1&key2=value2&...
            //...
        }

        public byte[] BuildResponse()
        {
            Controller controller;

            switch (RequestUrlElements["Path"])
            {
                case "/favicon.ico":
                    return new byte[1] { 1 };
                case "/":
                    controller = new MainPageController(QueryHandlerContext);
                    break;
                case "/Help":
                    controller = new HelpController(QueryHandlerContext);
                    break;
                case "/SomePage":
                    controller = new SomePageController(QueryHandlerContext);
                    break;
                case "/images/img1.png":
                    return Views.Image("img1.png");
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
                    //controller = new ErrorPageController(QueryHandler);!!!
                    controller = new MainPageController(QueryHandlerContext);
                    break;
            }

            return controller.GetViewPage(RequestUrlElements);
        }
    }
}
