using ServerOnSocketsAndStreams.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerOnSocketsAndStreams.Controllers
{
    public class AuthorizationController : Controller
    {
        public AuthorizationController(QueryHandler queryHandler) : base(queryHandler)
        { }

        public override byte[] GetViewPage(Dictionary<string, string> RequestUrlAndPostMethodElements)
        {
            if (RequestUrlAndPostMethodElements["Method"] == "GET")
            {
                var htmlVariables = new List<string>();
                htmlVariables.Add("Enter login and password");

                return Views.CreateHtmlByteCode("AuthorizationPage", htmlVariables);
            }

            if (RequestUrlAndPostMethodElements["Method"] == "POST")
            {
                string login = "";
                //проверка на наличие в б/д такого аккаунта
                if (!QueryHandlerContext.currentClient.AccountValidation(RequestUrlAndPostMethodElements, out login))
                {
                    var htmlVariables = new List<string>();
                    htmlVariables.Add("Wrong login and/or password, enter again");
                    return Views.CreateHtmlByteCode("AuthorizationPage", htmlVariables);
                }
                //else if(проверить есть ли уже активный клиент по данному аккаунту)
                //{ 
                //    return ViewsManager.CreateHtmlByteCode("Wrong login and/or password, enter again");
                //}
                else
                {
                    QueryHandlerContext.currentClient.clientStatus = ClientStatus.Authorized;
                    QueryHandlerContext.currentClient.ClientLogin = login;
                    var htmlVariables = new List<string>();
                    htmlVariables.Add(login);
                    return Views.CreateHtmlByteCode("AccountValidationCompletePage", htmlVariables, () =>
                    {
                        return Guid.NewGuid().ToString();
                    });
                }
            }

            return ViewsManager.CreateErrorPageByteCode();
        }
    }
}
