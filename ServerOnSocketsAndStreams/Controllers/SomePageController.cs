using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Controllers
{
    public class SomePageController : Controller
    {
        public SomePageController(QueryHandler queryHandler) : base(queryHandler)
        { }

        public override byte[] GetViewPage(Dictionary<string, string> RequestUrlAndPostMethodElements)
        {
            if (QueryHandlerContext.currentClient.clientStatus == ClientStatus.Visitor)
                return Views.CreateHtmlByteCode("WrongStatusPage", null);

            return Views.CreateHtmlByteCode("PageWithImage", null);
        }
    }
}
