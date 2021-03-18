using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WebServicesBUAP
{


    [ServiceContract]
    public interface IUserService
    {
        Respuesta Authenticate(string user, string pass);
        Respuesta GetResponse(int code);

        [OperationContract(Name = "setUser")]
        Respuesta SetUser(string user, string pass, string newuser, string newpass);

        [OperationContract(Name = "updateUser")]
        Respuesta UpdateUser(string user, string pass, string oldUser, string newUser);


        [OperationContract(Name = "setUserInfo")]
        Respuesta SetUserInfo(string user, string pass, string searchedUser, string userInfoJSON);

        [OperationContract(Name = "updateUserInfo")]
        Respuesta UpdateUserInfo(string user, string pass, string searchedUser, string userInfoJSON);

        
    }

}
