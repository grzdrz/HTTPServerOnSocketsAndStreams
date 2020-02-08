using ServerOnSocketsAndStreams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Controllers
{
    public abstract class Controller
    {
        public Views Views;
        public QueryHandler QueryHandlerContext;

        public Controller(QueryHandler queryHandler)
        {
            QueryHandlerContext = queryHandler;
            Views = new Views(QueryHandlerContext.currentClient);
        }

        public abstract byte[] GetViewPage(Dictionary<string, string> requestUrlElements);
    }
}
