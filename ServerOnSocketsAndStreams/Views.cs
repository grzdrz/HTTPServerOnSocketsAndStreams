using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams
{
    public class Views
    {
        public ClientProfile Client;
        public string Cookie;

        public byte[] MainPage(string pageName, List<string> variables)
        {
            string clientLogin = "";
            if (Client.clientStatus == ClientStatus.Visitor)
                clientLogin = "visitor";
            else
                clientLogin = Client.ClientLogin;
            variables.Add(clientLogin);

            return CreateHtmlByteCode(pageName, variables);

            #region "Старый вариант"
            //string ResponseHtml = "<!DOCTYPE html>" +
            //                   "<html>" +
            //                       "<head>" +
            //                           "<meta charset='utf-8'></meta>" +
            //                       "</head>" +
            //                       "<body>" +
            //                           "<div>" +
            //                           "<p>Welcome to the best site in the galaxy " + clientLogin + ", but that's not for sure</p></br></br>" +
            //                           "<a href=\'http://" + IP + ":8005/Help\'>Site navigator</a>" +
            //                           "</dib>" +
            //                       "</body>" +
            //                    "</html>";

            //string Response = "HTTP/1.1 200 OK" +
            //    "\nContent-Type: text/html" +
            //    "\nSet-Cookie: cookie1=" + Cookie +
            //    "\nContent-Length: " + ResponseHtml.Length +
            //    "\n\n" + ResponseHtml;
            //return Encoding.UTF8.GetBytes(Response);
            #endregion
        }

        public byte[] Image(string img)
        {
            byte[] byteImage;
            //var test = Directory.GetCurrentDirectory();
            using (FileStream fs = new FileStream("..\\..\\img2.jpg", FileMode.Open))
            {
                byteImage = new byte[fs.Length];
                fs.Read(byteImage, 0, byteImage.Length);
            }

            string headLine = "HTTP/1.1 200 OK" +
                "\nContent-type: image/png" +
                "\nSet-Cookie: cookie1=" + Cookie +
                "\nContent-Length:" + byteImage.Length.ToString() +
                "\n\n";
            byte[] byteHeadLine = Encoding.UTF8.GetBytes(headLine);

            //байт код всего вышеперечисленного в определенной последовательности(заголовки+картинка)
            byte[] byteImageResponse = new byte[byteImage.Length + byteHeadLine.Length];
            byteHeadLine.CopyTo(byteImageResponse, 0);
            byteImage.CopyTo(byteImageResponse, byteHeadLine.Length);

            return byteImageResponse;
        }

        public byte[] CreateHtmlByteCode(string pageName, List<string> variables)
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
                        regex = new Regex("/--variable" + i + "--/");
                        html = regex.Replace(html, variables[i]);
                    }

                byteHttpLine = Encoding.UTF8.GetBytes(html);
            }

            string headLine = "HTTP/1.1 200 OK" +
            "\nContent-Type: text/html" +
            "\nSet-Cookie: cookie1=" + Cookie +
            "\nContent-Length: " + byteHttpLine.Length.ToString() +
            "\n\n";
            byte[] byteHeadLine = Encoding.UTF8.GetBytes(headLine);

            byte[] byteResponse = new byte[byteHttpLine.Length + byteHeadLine.Length];
            byteHeadLine.CopyTo(byteResponse, 0);
            byteHttpLine.CopyTo(byteResponse, byteHeadLine.Length);

            return byteResponse;
        }

        //public byte[] PageWithImage()
        //{
        //    string ResponseHtml = "<!DOCTYPE html>" +
        //           "<html>" +
        //               "<head>" +
        //                   "<meta charset='utf-8'></meta>" +
        //               "</head>" +
        //               "<body>" +
        //                   "<div>" +
        //                     "<img src='images/img1.png'></img>" +
        //                   "</dib>" +
        //               "</body>" +
        //            "</html>";

        //    string Response = "HTTP/1.1 200 OK" +
        //    "\nContent-Type: text/html" +
        //    "\nSet-Cookie: cookie1=" + Cookie +
        //    "\nContent-Length: " + ResponseHtml.Length +
        //    "\n\n" + ResponseHtml;
        //    return Encoding.UTF8.GetBytes(Response);
        //}

        //public byte[] AuthorizationPage(string phrase)
        //{
        //    string ResponseHtml = "<!DOCTYPE html>" +
        //           "<html>" +
        //               "<head>" +
        //                   "<meta charset='utf-8'></meta>" +
        //               "</head>" +
        //               "<body>" +
        //                   "<div>" +
        //                       "<p>" + phrase + "</p></br>" +
        //                       "<form accept-charset='utf-8' method='post'>" +
        //                           "<p>" +
        //                               "<input type='text' name='Name' />" +
        //                           "</p>" +
        //                           "<p>" +
        //                               "<input type='password' name='Password' />" +
        //                           "</p>" +
        //                           "<p>" +
        //                               "<input type='submit' value='Отправить' />" +
        //                           "</p>" +
        //                       "</form>" +
        //                   "</dib>" +
        //               "</body>" +
        //           "</html>";

        //    string Response = "HTTP/1.1 200 OK" +
        //    "\nContent-Type: text/html" +
        //    "\nSet-Cookie: cookie1=" + Cookie +
        //    "\nContent-Length: " + ResponseHtml.Length +
        //    "\n\n" + ResponseHtml;
        //    return Encoding.UTF8.GetBytes(Response);
        //}

        //public byte[] AccountValidationComplete(string NameAndPassword)
        //{
        //    string ResponseHtml = "<!DOCTYPE html>" +
        //       "<html>" +
        //           "<head>" +
        //               "<meta charset='utf-8'></meta>" +
        //           "</head>" +
        //           "<body>" +
        //               "<div>" +
        //                   "<p>Welcome " + Client.ClientLogin + "</p></br>" +
        //                   "<a href='/'> Main page </a>" +
        //               "</dib>" +
        //           "</body>" +
        //       "</html>";

        //    string Response = "HTTP/1.1 200 OK" +
        //    "\nContent-Type: text/html" +
        //    "\nSet-Cookie: cookie1=" + Cookie +
        //    "\nContent-Length: " + ResponseHtml.Length +
        //    "\n\n" + ResponseHtml;
        //    return Encoding.UTF8.GetBytes(Response);
        //}

        //public byte[] RegistrationPage(string phrase)
        //{
        //    string ResponseHtml = "<!DOCTYPE html>" +
        //           "<html>" +
        //               "<head>" +
        //                   "<meta charset='utf-8'></meta>" +
        //               "</head>" +
        //               "<body>" +
        //                   "<div>" +
        //                       "<p>" + phrase + "</p></br>" +
        //                       "<form accept-charset='utf-8' method='post'>" +
        //                           "<p>" +
        //                               "<input type='text' name='Name' />" +
        //                           "</p>" +
        //                           "<p>" +
        //                               "<input type='password' name='Password' />" +
        //                           "</p>" +
        //                           "<p>" +
        //                               "<input type='password' name='Password' />" +
        //                           "</p>" +
        //                           "<p>" +
        //                               "<input type='submit' value='Отправить' />" +
        //                           "</p>" +
        //                       "</form>" +
        //                   "</dib>" +
        //               "</body>" +
        //           "</html>";

        //    string Response = "HTTP/1.1 200 OK" +
        //    "\nContent-Type: text/html" +
        //    "\nSet-Cookie: cookie1=" + Cookie +
        //    "\nContent-Length: " + ResponseHtml.Length +
        //    "\n\n" + ResponseHtml;
        //    return Encoding.UTF8.GetBytes(Response);
        //}

        //public byte[] AccountVerificationComplete()
        //{
        //    string ResponseHtml = "<!DOCTYPE html>" +
        //       "<html>" +
        //           "<head>" +
        //               "<meta charset='utf-8'></meta>" +
        //           "</head>" +
        //           "<body>" +
        //               "<div>" +
        //                   "<p>Registration complete</p></br>" +
        //                   "<a href='/AuthorizationPage'> Authorization </a></br>" +
        //                   "<a href='/'> Main_Page </a>" +
        //               "</dib>" +
        //           "</body>" +
        //       "</html>";

        //    string Response = "HTTP/1.1 200 OK" +
        //    "\nContent-Type: text/html" +
        //    "\nSet-Cookie: cookie1=" + Cookie +
        //    "\nContent-Length: " + ResponseHtml.Length +
        //    "\n\n" + ResponseHtml;
        //    return Encoding.UTF8.GetBytes(Response);
        //}

        //public byte[] WrongStatus()
        //{
        //    string ResponseHtml = "<!DOCTYPE html>" +
        //           "<html>" +
        //               "<head>" +
        //                   "<meta charset='utf-8'></meta>" +
        //               "</head>" +
        //               "<body>" +
        //                   "<div>" +
        //                   "<p>Visitors can't read this page</p></br>" +
        //                   "<a href='/AuthorizationPage'> Authorization </a>" +
        //                   "</dib>" +
        //               "</body>" +
        //            "</html>";

        //    string Response = "HTTP/1.1 200 OK" +
        //    "\nContent-Type: text/html" +
        //    "\nSet-Cookie: cookie1=" + Cookie +
        //    "\nContent-Length: " + ResponseHtml.Length +
        //    "\n\n" + ResponseHtml;
        //    return Encoding.UTF8.GetBytes(Response);
        //}
    }
}
