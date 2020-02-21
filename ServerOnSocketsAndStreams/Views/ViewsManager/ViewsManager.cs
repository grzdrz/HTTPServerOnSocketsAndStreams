using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Views
{
    public class ViewsManager
    {
        public ClientSession Client;

        public ViewsManager(ClientSession client)
        {
            Client = client;
        }

        public static byte[] CreateImageByteCode(string img)
        {
            byte[] byteImage;
            using (FileStream fs = new FileStream("..\\..\\Images\\" + img, FileMode.Open))
            {
                byteImage = new byte[fs.Length];
                fs.Read(byteImage, 0, byteImage.Length);
            }

            string headLine = "HTTP/1.1 200 OK" +
                "\nContent-type: image/png" +
                "\nContent-Length:" + byteImage.Length.ToString() +
                "\n\n";
            byte[] byteHeadLine = Encoding.UTF8.GetBytes(headLine);

            byte[] byteImageResponse = new byte[byteImage.Length + byteHeadLine.Length];
            byteHeadLine.CopyTo(byteImageResponse, 0);
            byteImage.CopyTo(byteImageResponse, byteHeadLine.Length);

            return byteImageResponse;
        }

        public byte[] CreateHtmlByteCode(string pageName, List<string> variables, bool setCookie = false)
        {
            string html = "";
            Regex regex = null;
            byte[] byteHttpLine = null;
            using (FileStream fstream = new FileStream("..\\..\\Views\\" + pageName + ".html", FileMode.Open, FileAccess.ReadWrite))
            {
                using (StreamReader streamReader = new StreamReader(fstream))
                {
                    html = streamReader.ReadToEnd();
                }

                //вставляем переменные в соответствующих местах в разметке(если они есть)
                if (!(variables is null))
                    for (int i = 0; i < variables.Count; i++)
                    {
                        regex = new Regex("<variable id=" + i + "></variable>");
                        html = regex.Replace(html, variables[i]);
                    }

                byteHttpLine = Encoding.UTF8.GetBytes(html);
            }

            string cookieHeader = "";
            if (setCookie)
            {
                cookieHeader = "\nSet-Cookie: cookie1=" + Client.ClientCookie;
            }

            string headLine = "HTTP/1.1 200 OK" +
            "\nContent-Type: text/html" +
            cookieHeader +
            "\nContent-Length: " + byteHttpLine.Length.ToString() +
            "\n\n";

            byte[] byteHeadLine = Encoding.UTF8.GetBytes(headLine);
            byte[] byteResponse = new byte[byteHttpLine.Length + byteHeadLine.Length];
            byteHeadLine.CopyTo(byteResponse, 0);
            byteHttpLine.CopyTo(byteResponse, byteHeadLine.Length);

            return byteResponse;
        }

        public static byte[] CreateErrorPageByteCode()
        {
            string headLine = "HTTP/1.1 404 Not Found" +
            "\nContent-Type: text/html" +
            "\nContent-Length: 0" +
            "\n\n";

            byte[] byteHeadLine = Encoding.UTF8.GetBytes(headLine);

            return byteHeadLine;
        }
    }
}
