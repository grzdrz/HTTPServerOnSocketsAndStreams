using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Query
{
    public interface IQuery
    {
        void ProcessQuery(QueryHandler queryContext);
    }
}
