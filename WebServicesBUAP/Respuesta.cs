using System;
using System.Runtime.Serialization;

namespace WebServicesBUAP
{
    [DataContract]
    public class Respuesta
    {
        public Respuesta()
        {
            Code = 999;
            Message = "Error desconocido";
            Status = "error";
            Data = DateTime.Now.ToString("s");
        }

        [DataMember(Name = "code")]
        public int Code { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "data")]
        public string Data { get; set; }

        [DataMember(Name = "status")]
        public string  Status{ get; set; }


    }

}
