using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System.Security.Cryptography;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.ServiceModel;
using System.Net.Http;


namespace WebServicesBUAP
{
    [ServiceBehavior(
        Namespace  = "http://WebServicesBUAP",
        Name = "UserService"
        )]
    public class UserService : IUserService
    {
        readonly IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "tFtjVPY7hIDzsF13bkLh5Bol93YaNSKnTzCEEnTz",
            BasePath = "https://classroomws-5b815-default-rtdb.firebaseio.com/"
        };
        readonly IFirebaseClient client;

        private readonly string[] allowedRoles = { "rh" };

        public UserService()
        {
            client = new FireSharp.FirebaseClient(config);
        }

        /// <summary>
        /// Authentica y verifica los permisos de un usuario.
        /// </summary>
        /// <param name="user">
        /// El identificador del usuario (nombre de usuario).
        /// </param>
        /// <param name="pass">
        /// La contraseña del usuario.
        /// </param>
        /// <returns>
        /// <seealso cref="WebServicesBUAP.Respuesta"/>
        /// Un objeto con el código, mensaje y estatus de respuesta.
        /// </returns>
        public Respuesta Authenticate(string user, string pass)
        {

            if (string.IsNullOrEmpty(user) || user == "?") return GetResponse(500);

            FirebaseResponse fireRes = client.Get("usuarios/" + user);

            string resPass = fireRes.Body.Trim('"');
        
            if (resPass == "null") return GetResponse(500);
            
            if (pass == null || MD5Hash(pass) != resPass) return GetResponse(501);
        

            UserInfo userInfo = GetUserInfo(user);
            
            if (userInfo == null || !allowedRoles.Contains(userInfo.Rol)) return GetResponse(504);

            return GetResponse(99);
        }

        /// <summary>
        /// Obtiene la Respuesta de acuerdo a un código numérico.
        /// </summary>
        /// <param name="code">
        /// El código de la respuesta.
        /// </param>
        /// <returns>
        /// <seealso cref="WebServicesBUAP.Respuesta"/>
        /// El objeto de respuesta.
        /// </returns>
        public Respuesta GetResponse(int code)
        {
            FirebaseResponse fireRes = client.Get("respuestas/" + code);
            
            if (fireRes == null) return new Respuesta();

            return new Respuesta
            {
                Code = code,
                Message = fireRes.Body.Trim('"'),
            };

        }

        /// <summary>
        /// Insertar nombre y contraseña de una cuenta.
        /// </summary>
        /// <returns>
        /// <seealso cref="WebServicesBUAP.Respuesta"/>
        /// La respuesta de la operación.
        /// </returns>
        /// <param name="user">Nombre de usuario de autenticación.</param>
        /// <param name="pass">La contraseña de autenticación.</param>
        /// <param name="newUser">El nombre de usuario de la cuenta nueva.</param>
        /// <param name="newPass">La contraseña de la cuenta nueva</param>
        public Respuesta SetUser(string user, string pass, string newUser, string newPass)
        {
            Respuesta res = Authenticate(user, pass);
            if (res.Status == "error") return res;

            if (!Regex.Match(newUser, "^[a-zA-Z0-9]+$").Success) return GetResponse(503);

            if (!Regex.Match(newPass, "^(?=.*\\d+).{8,}$").Success) return GetResponse(502);

            if (UserExists(newUser)) return GetResponse(508);

            FirebaseResponse fireRes = client.Set("usuarios/" + newUser, MD5Hash(newPass));

            if (fireRes != null) res = GetResponse(404);

            return res;
        }

        /// <summary>
        /// Actualizar nombre de usuario y contraseña de una cuenta.
        /// </summary>
        /// <returns>
        /// <seealso cref="WebServicesBUAP.Respuesta"/>
        /// La respuesta de la operación.
        /// </returns>
        /// <param name="user">Nombre de usuario de autenticación.</param>
        /// <param name="pass">La contraseña de autenticación.</param>
        /// <param name="oldUser">El nombre de usuario actual de la cuenta a modificar.</param>
        /// <param name="newUser">El nuevo nombre de usuario de la cuenta a modificar.</param>
        /// <param name="newPass">La nueva contraseña de la cuenta a modificar.</param>
        public Respuesta UpdateUser(string user, string pass, string oldUser, string newUser, string newPass)
        {
            Respuesta res = Authenticate(user, pass);
            if (res.Status == "error") return res;

            if (!Regex.Match(newUser, "^[a-zA-Z0-9]+$").Success) return GetResponse(503);

            if (!Regex.Match(newPass, "^(?=.*\\d+).{8,}$").Success) return GetResponse(502);

            if (!UserExists(oldUser)) return GetResponse(505);

            if(oldUser != newUser)
            {
                client.Delete("usuarios/" + oldUser);
            }

            FirebaseResponse fireRes = client.Set("usuarios/" + newUser, MD5Hash(newPass));

            if (fireRes != null) res = GetResponse(401);

            return res;
        }

        /// <summary>
        /// Insertar datos del usuario que son: correo, nombre, rol y teléfono.
        /// </summary>
        /// <param name="user">Nombre de usuario de autenticación.</param>
        /// <param name="pass">La contraseña de autenticación.</param>
        /// <param name="searchedUser">El identificador de los nuevos datos.</param>
        /// <param name="userInfoJSON">
        /// El JSON con la información del usuario.
        /// Formato:
        /// <code>
        /// {
        ///     "correo": "correo@mail.com",
        ///     "rol": "userRol",
        ///     "telefono": "123-2-312-23",
        ///     "nombre": "Full User Name"
        /// }
        /// </code>
        /// </param>
        /// <returns>
        /// <seealso cref="WebServicesBUAP.Respuesta"/>
        /// La respuesta de la operación.
        /// </returns>
        public Respuesta SetUserInfo(string user, string pass, string searchedUser, string userInfoJSON)
        {
            Respuesta res = Authenticate(user, pass);
            
            if (res.Status == "error") return res;
            var (isValid, code) = ValidateJSON(userInfoJSON);
            if (!isValid) return GetResponse(code);
            if (UserInfoExists(searchedUser)) return GetResponse(506);




            UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(userInfoJSON);

            FirebaseResponse fireRes = client.Set("usuarios_info/" + searchedUser, userInfo);

            if (fireRes != null) return GetResponse(402);

            return new Respuesta();
        }

        /// <summary>
        /// Actualizar correo, nombre, rol ó teléfono de una cuenta.
        /// </summary>
        /// <param name="user">Nombre de usuario de autenticación.</param>
        /// <param name="pass">La contraseña de autenticación.</param>
        /// <param name="searchedUser">El identificador de los nuevos datos.</param>
        /// <param name="userInfoJSON">
        /// El JSON con la información del usuario.
        /// Formato:
        /// <code>
        /// {
        ///     "correo": "correo@mail.com",
        ///     "rol": "userRol",
        ///     "telefono": "123-2-312-23",
        ///     "nombre": "Full User Name"
        /// }
        /// </code>
        /// </param>
        /// <returns>
        /// <seealso cref="WebServicesBUAP.Respuesta"/>
        /// La respuesta de la operación.
        /// </returns>
        public Respuesta UpdateUserInfo(string user, string pass, string searchedUser, string userInfoJSON)
        {
            Respuesta res = Authenticate(user, pass);

            if (res.Status == "error") return res;

            var (isValid, code) = ValidateJSON(userInfoJSON, true);
            if (!isValid) return GetResponse(code);

            if (!UserInfoExists(searchedUser)) return GetResponse(507);


            UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(userInfoJSON);
            userInfoJSON = JsonConvert.SerializeObject(userInfo);


            Console.WriteLine(userInfoJSON);
            //Console.WriteLine(userInfo);
            //FirebaseResponse fireRes = client.UpdateAsync("usuarios_info/" + searchedUser, userInfo).Result;


            //if (fireRes != null) return GetResponse(403);
            using (var httpClient = new HttpClient())
            {
                string url = "https://classroomws-5b815-default-rtdb.firebaseio.com/"
                    + "usuarios_info/" + searchedUser + ".json";
                HttpMethod httpMethod = new HttpMethod("PATCH");

                HttpRequestMessage req = new HttpRequestMessage(httpMethod, url)
                {
                    Content = new StringContent(userInfoJSON, Encoding.UTF8, "application/json")
                };
                var result = httpClient.SendAsync(req).Result;
                Console.WriteLine(result.IsSuccessStatusCode);
                Console.WriteLine(result.StatusCode);
                return GetResponse(403);
            }

            return new Respuesta();
        }

        /// <summary>
        /// Verifica si ya existe una entrada de información de usuario en la base de datos.
        /// </summary>
        /// <param name="user">
        /// El identificador del usuario (nombre de usuario)
        /// </param>
        /// <returns>
        /// <seealso cref="System.Boolean"/>
        /// Verdadero si existe de lo contrario falso.
        /// </returns>
        private bool UserInfoExists(string user)
        {
            FirebaseResponse fireRes = client.Get("usuarios_info/" + user);
            return !(fireRes == null || fireRes.Body.Trim('"') == "null");
        }

        /// <summary>
        /// Verifica si ya existe un usuario en la base de datos.
        /// </summary>
        /// <param name="user">
        /// El identificador del usuario (nombre de usuario)
        /// </param>
        /// <returns>
        /// <seealso cref="System.Boolean"/>
        /// Verdadero si existe de lo contrario falso.
        /// </returns>
        private bool UserExists(string user)
        {
            FirebaseResponse fireRes = client.Get("usuarios/" + user);
            return !(fireRes == null || fireRes.Body.Trim('"') == "null");
        }

        /// <summary>
        /// Obtiene la información de un usuario.
        /// </summary>
        /// <param name="user">
        /// El identificador del usuario (nombre de usuario)
        /// </param>
        /// <returns>
        /// <seealso cref="WebServicesBUAP.UserInfo"/>
        /// La instancia con la información del usuario.
        /// </returns>
        private UserInfo GetUserInfo(string user)
        {
            FirebaseResponse fireRes = client.Get("usuarios_info/" + user);
            UserInfo userInfo = fireRes.ResultAs<UserInfo>();
            return userInfo;
        }

        /// <summary>
        /// Valida un JSON que representa la información de un usuario.
        /// </summary>
        /// <param name="userInfoJSON">
        /// El JSON que representa la información del usuario.
        /// </param>
        /// <returns>
        /// Una tupla conteniendo la siguiente información:
        /// <list type="bullet">
        /// <item>
        /// <seealso cref="bool"/> 
        /// isValid, verdadero si el JSON es válido, falso de otra manera.
        /// </item>
        /// <item>
        /// <seealso cref="int"/>
        /// code, código de error o 0 si el JSON es válido.
        /// </item>
        /// </list>
        /// </returns>
        public (bool isValid, int code) ValidateJSON(string userInfoJSON, bool partial = false)
        {
            int invalidCode;
            string jsonSchema = @"{
                'decription': 'User Info',
                'type': 'object',
                'additionalProperties': false,
                'properties': 
                    {
                        'correo': { 
                            'type': 'string', 
                            'format': 'email'
                        },
                        'nombre': { 
                            'type': 'string',
                            'minLength': 1
                        },
                        'rol': {
                            'type': 'string',
                            'minLength': 1
                        },
                        'telefono': {
                            'type': 'string',
                            'minLength': 1
                         },
                    }";
            if(!partial)
            {
                jsonSchema += @",
                'required': ['correo', 'nombre', 'rol', 'telefono']
                }";
                invalidCode = 304;
            }
            else
            {
                jsonSchema += @"
                }";
                invalidCode = 306;
        
            }
                 
            JSchema schema = JSchema.Parse(jsonSchema);
            try
            {
                JObject userInfo = JObject.Parse(userInfoJSON);
                bool isValid = userInfo.IsValid(schema);
                int code = isValid ? 0 : invalidCode;
                return (isValid, code);
            }
            catch (JsonReaderException)
            {
                return (false, 305);
            }


        }

        /// <summary>
        /// Calcula el hash MD5 de una cadena de texto.
        /// </summary>
        /// <returns>
        /// <seealso cref="System.String"/>
        /// El hash MD5 como una cadena de texto.
        /// </returns>
        /// <param name="text">La cadena de texto de entrada.</param>
        private string MD5Hash(string text)
        {
            if (text == null) return text;

            MD5 md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(Encoding.UTF8.GetBytes(text));

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
