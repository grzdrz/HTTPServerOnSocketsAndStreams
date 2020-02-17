using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Controllers
{
    public class MainPageController : Controller
    {
        public MainPageController(QueryHandler queryHandler) : base(queryHandler)
        { }

        public override byte[] GetViewPage(Dictionary<string, string> requestUrlElements)
        {
            //if (requestUrlElements.ContainsKey("Parameters"))
            //{
            //    ...
            //}

            var htmlVariables = new List<string>();
            if (QueryHandlerContext.currentClient.clientStatus == ClientStatus.Visitor)
                htmlVariables.Add("visitor");
            else
                htmlVariables.Add(QueryHandlerContext.currentClient.ClientLogin);
            htmlVariables.Add(QueryHandlerContext.clientIp);

            return Views.CreateHtmlByteCode("MainPage", htmlVariables);
        }
    }
}
