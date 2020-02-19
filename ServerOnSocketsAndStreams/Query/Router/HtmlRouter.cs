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
        // ["Method"]="GET|POST|...";["Path"]="controller/action/...";["UrlParameters"]="key1=value1&key2=value2&...";
        // ["Name"]=...;["Password"]=...;[["Password"]=...;]
        public Dictionary<string, string> RequestUrlAndPostMethodElements = new Dictionary<string, string>();

        public QueryHandler QueryContext;

        public HtmlRouter(QueryHandler queryHandler)
        {
            QueryContext = queryHandler;
            ParseRequestFirstLine(queryHandler.ParsedRequest);
            ParsePostRequestLine(queryHandler.ParsedRequest);
        }

        public void ParseRequestFirstLine(Dictionary<string, string[]> parsedRequest)
        {
            string temp;

            //GET POST ...
            RequestUrlAndPostMethodElements.Add("Method", parsedRequest["MainLine"][0]);

            // controller/action/...
            temp = new Regex(@"\/.*\?|\/.*", RegexOptions.IgnoreCase)
                .Match(parsedRequest["MainLine"][1]).ToString();
            if (temp != "")
            {
                if (temp.EndsWith("?"))
                    temp = new String(temp.Take(temp.Length - 1).ToArray());//отрезаем последний символ
                RequestUrlAndPostMethodElements.Add("Path", temp);
            }

            // ?key1=value1&key2=value2&...
            temp = new Regex(@"\?.*", RegexOptions.IgnoreCase)
                .Match(parsedRequest["MainLine"][1]).ToString();
            if (temp != "")
            {
                temp = new String(temp.Skip(1).ToList().ToArray());//отрезаем первый символ
                RequestUrlAndPostMethodElements.Add("UrlParameters", temp);
            }
        }

        public void ParsePostRequestLine(Dictionary<string, string[]> parsedRequest)
        {
            // Name=value1&Password=value2&Password=value2
            string temp = new Regex(@"Name=.*(&Password=.*)+", RegexOptions.IgnoreCase)
                .Match(parsedRequest.Last().Key).ToString();
            if (temp != "")
            {
                string[] temp2 = temp.Split('&');
                RequestUrlAndPostMethodElements.Add("Name", temp2[0].Split('=')[1]);
                RequestUrlAndPostMethodElements.Add("FirstPassword", temp2[1].Split('=')[1]);
                if (temp2.Length > 2)
                    RequestUrlAndPostMethodElements.Add("SecondPassword", temp2[2].Split('=')[1]);
            }
        }

        public byte[] BuildResponse()
        {
            Controller controller;

            switch (RequestUrlAndPostMethodElements["Path"])
            {
                case "/favicon.ico":
                    return ViewsManager.CreateImageByteCode("favicon.ico");
                case "/":
                    controller = new MainPageController(QueryContext);
                    break;
                case "/Help":
                    controller = new HelpController(QueryContext);
                    break;
                case "/SomePage":
                    controller = new SomePageController(QueryContext);
                    break;
                case "/images/img1.png":////
                    return ViewsManager.CreateImageByteCode("img1.jpg");
                case "/AuthorizationPage":
                    controller = new AuthorizationController(QueryContext);
                    break;
                case "/RegistrationPage":
                    controller = new RegistrationController(QueryContext);
                    break;
                case "/WebSocketView":
                    controller = new WebSocketController(QueryContext);
                    break;
                default:
                    return ViewsManager.CreateErrorPageByteCode();
            }

            return controller.GetViewPage(RequestUrlAndPostMethodElements);
        }
    }
}
