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
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WebServicesBUAP
{

    public class UserService : IUserService
    {
        readonly IFirebaseConfig config = new FirebaseConfig
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
                Status = !((code % 200) < 100) ? "error" : "success",
                Data = DateTime.Now.ToString("s")
            };

        }

        public Respuesta SetUser(string user, string pass, string newuser, string newpass)
        {
            return new Respuesta();
        }

        public Respuesta SetUserInfo(string user, string pass, string searchedUser, string userInfoJSON)
        {
            Respuesta res = Authenticate(user, pass);
            
            if (res.Status == "error") return res;                        
            if (!ValidateJSON(userInfoJSON)) return GetResponse(305);
            if (UserInfoExists(searchedUser)) return GetResponse(506);

            UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(userInfoJSON);

            FirebaseResponse fireRes = client.Set<UserInfo>("usuarios_info/" + searchedUser, userInfo);

            if(fireRes != null)
            {
                res = GetResponse(402);               
            }

            return res;
        }

        private bool UserInfoExists(string user)
        {
            FirebaseResponse fireRes = client.Get("usuarios_info/" + user);
            return !(fireRes == null || fireRes.Body.Trim('"') == "null");
        }
        public Respuesta UpdateUser(string user, string pass, string oldUser, string newUser)
        {
            return new Respuesta();
        }

        public Respuesta UpdateUserInfo(string user, string pass, string searchedUser, string userInfoJSON)
        {
            return new Respuesta();
        }

        private bool ValidateJSON(string userInfoJSON)
        {
            string schemaJson = @"{
                'decription': 'User Info',
                'type': 'object',
                'properties': 
                    {
                        'correo': { 'type': 'string' },
                        'nombre': { 'type': 'string' },
                        'rol': { 'type': 'string' },
                        'telefono': { 'type': 'string' },
                    }
                }";
            JSchema schema = JSchema.Parse(schemaJson);
            JObject userInfo = JObject.Parse(userInfoJSON);
            return userInfo.IsValid(schema);
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
