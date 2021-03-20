using System;
using System.Runtime.Serialization;

namespace WebServicesBUAP
{
    [DataContract]
    public class Respuesta
    {
        public Respuesta()
        {
            _code = 999;
            Message = "Error desconocido";
            Status = "error";
            Data = "";
        }
        private int _code;
        [DataMember(Name = "code")]
        public int Code 
        {
            get
            {
                return _code;
            }
            set
            {
                _code = value;
                Status = !((value % 200) < 100) ? "error" : "success";
                Data = Status == "success" ? DateTime.Now.ToString("s") : "";
            }
        }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "data")]
        public string Data { get; set; }

        [DataMember(Name = "status")]
        public string  Status{ get; set; }


    }

}
