using ServerOnSocketsAndStreams.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Controllers
{
    public class RegistrationController : Controller
    {
        public RegistrationController(QueryHandler queryHandler) : base(queryHandler)
        { }

        public override byte[] GetViewPage(Dictionary<string, string> requestUrlElements)
        {
            //if (requestUrlElements.ContainsKey("Parameters"))
            //{
            //    ...
            //}

            if (requestUrlElements["Method"] == "GET")
            {
                var htmlVariables = new List<string>();
                htmlVariables.Add("Enter login and password");

                return Views.CreateHtmlByteCode("RegistrationPage", htmlVariables);
            }

            if (requestUrlElements["Method"] == "POST")
            {
                //проверка на совпадение паролей во 2й и 3й полях для ввода
                if (!QueryHandlerContext.currentClient.AccountVerification1(QueryHandlerContext.Request))
                {
                    var htmlVariables = new List<string>();
                    htmlVariables.Add("Wrong password, enter data again");
                    return Views.CreateHtmlByteCode("RegistrationPage", htmlVariables);
                }
                //проверка на наличие в б/д такого логина
                else if (QueryHandlerContext.currentClient.AccountVerification2(QueryHandlerContext.Request))
                {
                    var htmlVariables = new List<string>();
                    htmlVariables.Add("Such login already exists, enter data again");
                    return Views.CreateHtmlByteCode("RegistrationPage", htmlVariables);
                }
                else
                {
                    QueryHandlerContext.currentClient.AddAccountToDB(QueryHandlerContext.Request);//отправка логина и пароля в б/д
                    return Views.CreateHtmlByteCode("AccountVerificationCompletePage", null);
                }
            }

            return ViewsManager.CreateErrorPageByteCode();
        }
    }
}
