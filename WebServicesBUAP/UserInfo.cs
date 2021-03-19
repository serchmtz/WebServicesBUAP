using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebServicesBUAP
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserInfo
    {
        [JsonProperty(PropertyName = "correo")]
        public string Correo { get; set; }
        [JsonProperty(PropertyName = "nombre")]
        public string Nombre { get; set; }
        [JsonProperty(PropertyName = "telefono")]
        public string Telefono { get; set; }
        [JsonProperty(PropertyName = "rol")]
        public string Rol { get; set; }

    }
}