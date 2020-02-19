using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Controllers
{
    public class WebSocketController : Controller
    {
        public WebSocketController(QueryHandler queryHandler) : base(queryHandler)
        { }

        public override byte[] GetViewPage(Dictionary<string, string> RequestUrlAndPostMethodElements)
        {
            var htmlVariables = new List<string>();
            htmlVariables.Add("string for test of web socket query 123456 !@#$%^****");
            htmlVariables.Add(Server.Ip.ToString() + ":" + Server.PORT.ToString());

            return Views.CreateHtmlByteCode("WebSocketTest", htmlVariables);
        }
    }
}
