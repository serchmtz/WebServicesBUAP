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
using System.Security.Cryptography;

namespace WebServicesBUAP
{

    public class UserService : IUserService
    {
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "tFtjVPY7hIDzsF13bkLh5Bol93YaNSKnTzCEEnTz",
            BasePath = "https://classroomws-5b815-default-rtdb.firebaseio.com/"
        };
        readonly IFirebaseClient client;

        public UserService()
        {
            client = new FireSharp.FirebaseClient(config);
        }

        public Respuesta Authenticate(string user, string pass)
        {

            if (user == null || user.Length == 0) return GetResponse(500);

            FirebaseResponse fireRes = client.Get("usuarios/" + user);

            string resPass = fireRes.Body.Trim('"');
            
            if (resPass == "null") return GetResponse(500);
            
            if (pass == null) return GetResponse(501);
        
            if (MD5Hash(pass) == resPass) return GetResponse(99);

            return new Respuesta();
        }

        public Respuesta GetResponse(int code)
        {
            FirebaseResponse fireRes = client.Get("respuestas/" + code);
            
            if (fireRes == null) return new Respuesta();
           
            return new Respuesta
            {
                Code = code,
                Message = fireRes.Body.Trim('"'),
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

        private bool ValidateJSON(string json)
        {
            
            return false;
        }

        private string MD5Hash(string text)
        {
            if (text == null) return text;

            MD5 md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(text));

            byte[] res = md5.Hash;
            StringBuilder strb = new StringBuilder();
            foreach (byte ch in res)
            {
                strb.Append(ch.ToString("x2"));
            }

            return strb.ToString();
           
        }

    }
}
