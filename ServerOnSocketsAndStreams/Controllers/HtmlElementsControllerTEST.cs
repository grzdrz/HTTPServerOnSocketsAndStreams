using ServerOnSocketsAndStreams.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Controllers
{
    public class HtmlElementsController : Controller
    {
        public HtmlElementsController(QueryHandler queryHandler) : base(queryHandler)
        { }

        public override byte[] GetViewPage(Dictionary<string, string> requestUrlElements)
        {
            if (requestUrlElements["Method"] == "GET")
            {
                return Views.CreateHtmlByteCode("HtmlElementsPage", null);
            }
            else if (requestUrlElements["Method"] == "POST")
            {
                return Views.CreateHtmlByteCode("HtmlElementsPage", null);//////////
            }

            return ViewsManager.CreateErrorPageByteCode();
        }
    }
}
