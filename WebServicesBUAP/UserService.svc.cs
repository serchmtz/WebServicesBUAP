using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace WebServicesBUAP
{

    public class UserService : IUserService
    {
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "tFtjVPY7hIDzsF13bkLh5Bol93YaNSKnTzCEEnTz",
            BasePath = "https://classroomws-5b815-default-rtdb.firebaseio.com/"
        };

        IFirebaseClient client;

        public UserService()
        {
            client = new FireSharp.FirebaseClient(config);
        }

        public Respuesta Authenticate(string user, string pass)
        {
            FirebaseResponse fireRes = client.Get("usuarios/" + user);
            if (fireRes == null) return GetResponse(500);

            return new Respuesta();
        }

        public Respuesta GetResponse(int code)
        {
            FirebaseResponse fireRes = client.Get("respuestas/" + code);
            
            if (fireRes == null) return new Respuesta();

            return new Respuesta
            {
                Code = code,
                Message = fireRes.ToString(),
                Status = !((code % 200) < 100) ? "error" : "success"
            };

        }

        public Respuesta SetUser(string user, string pass, string newuser, string newpass)
        {
            return new Respuesta();
        }

        public Respuesta SetUserInfo(string user, string pass, string searchedUser, string userInfoJSON)
        {
            return new Respuesta();
        }

        public Respuesta UpdateUser(string user, string pass, string oldUser, string newUser)
        {
            return new Respuesta();
        }

        public Respuesta UpdateUserInfo(string user, string pass, string searchedUser, string userInfoJSON)
        {
            return new Respuesta();
        }

    }
}
