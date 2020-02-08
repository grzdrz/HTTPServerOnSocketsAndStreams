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

        public override byte[] GetViewPage(Dictionary<string, string> requestUrlElements)
        {
            //if (requestUrlElements.ContainsKey("Parameters"))
            //{
            //    ...
            //}

            var htmlVariables = new List<string>();
            htmlVariables.Add("string for test of web socket query 123456 !@#$%^****");

            return Views.CreateHtmlByteCode("WebSocketTest", htmlVariables);
        }
    }
}
