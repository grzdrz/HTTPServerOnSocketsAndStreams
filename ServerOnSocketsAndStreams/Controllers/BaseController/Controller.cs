using ServerOnSocketsAndStreams;
using ServerOnSocketsAndStreams.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Controllers
{
    public abstract class Controller
    {
        public ViewsManager Views;
        public QueryHandler QueryHandlerContext;

        public Controller(QueryHandler queryHandler)
        {
            QueryHandlerContext = queryHandler;
            Views = new ViewsManager(QueryHandlerContext.currentClient);
        }

        public abstract byte[] GetViewPage(Dictionary<string, string> RequestUrlAndPostMethodElements);
    }
}
