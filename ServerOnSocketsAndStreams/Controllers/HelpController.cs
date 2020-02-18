using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Controllers
{
    public class HelpController : Controller
    {
        public HelpController(QueryHandler queryHandler) : base(queryHandler)
        { }

        public override byte[] GetViewPage(Dictionary<string, string> RequestUrlAndPostMethodElements)
        {
            //if (RequestUrlAndPostMethodElements.ContainsKey("Parameters"))
            //{
            //    ...
            //}

            return Views.CreateHtmlByteCode("HelpPage", null);
        }
    }
}
