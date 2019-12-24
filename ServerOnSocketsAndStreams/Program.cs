using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server();

            //Console.WriteLine(ComputeWebSocketHandshakeSecurityHash("Iv8io/9s+lYFgZWcXczP8Q=="));

            Console.ReadKey();
        }
    }
}

//прикрутить к регистрации обязательный email с проверкой
