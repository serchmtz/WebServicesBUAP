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

            if (user == null || user.Length == 0) return GetResponse(500);

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
                Status = !((code % 200) < 100) ? "error" : "success",
                Data = DateTime.Now.ToString("s")
            };

        }

        public Respuesta SetUser(string user, string pass, string newuser, string newpass)
        {
            return new Respuesta();
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
            try
            {
                bool isValid = ValidateJSON(userInfoJSON);
                if (!isValid) return GetResponse(304);
            }
            catch (JsonReaderException)
            {

                return GetResponse(305);
            }
           
            if (UserInfoExists(searchedUser)) return GetResponse(506);

            UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(userInfoJSON);

            FirebaseResponse fireRes = client.Set<UserInfo>("usuarios_info/" + searchedUser, userInfo);

            if(fireRes != null)
            {
                res = GetResponse(402);               
            }

            return res;
        }

        public Respuesta UpdateUser(string user, string pass, string oldUser, string newUser)
        {
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
            try
            {
                bool isValid = ValidateJSON(userInfoJSON);
                if (!isValid) return GetResponse(304);
            }
            catch (JsonReaderException)
            {

                return GetResponse(305);
            }

            if (!UserInfoExists(searchedUser)) return GetResponse(507);

            UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(userInfoJSON);

            FirebaseResponse fireRes = client.Update<UserInfo>("usuarios_info/" + searchedUser, userInfo);

            if (fireRes != null)
            {
                res = GetResponse(403);
            }

            return res;
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
        /// <seealso cref="System.Boolean"/>
        /// Verdadero si userInfoJSON es válido o falso si no lo es.
        /// </returns>
        /// <exception cref="Newtonsoft.Json.JsonReaderException">
        /// </exception>
        private bool ValidateJSON(string userInfoJSON)
        {
            string schemaJson = @"{
                'decription': 'User Info',
                'type': 'object',
                'properties': 
                    {
                        'correo': { 'type': 'string', 'minLength': 1 },
                        'nombre': { 'type': 'string', 'minLength': 1 },
                        'rol': { 'type': 'string', 'minLength': 1 },
                        'telefono': { 'type': 'string', 'minLength': 1 },
                    },
                'required': ['correo', 'nombre', 'rol', 'telefono']
                }";
            JSchema schema = JSchema.Parse(schemaJson);
            JObject userInfo = JObject.Parse(userInfoJSON);
            return userInfo.IsValid(schema);
        }

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
