using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Data.Entity;
using System.Security.Cryptography;
using ServerOnSocketsAndStreams.Models;

namespace ServerOnSocketsAndStreams
{
    public enum ClientStatus
    {
        Visitor,
        Authorized
    };

    public class ClientSession
    {
        public QueryHandler queryHandler;

        public Socket currentClientSocket;
        public string ClientCookie = "no cookies";
        public string ClientLogin;
        public ClientStatus clientStatus;

        public ClientSession(Socket currentClientSocket)
        {
            this.currentClientSocket = currentClientSocket;
            clientStatus = ClientStatus.Visitor;
        }
    }
}