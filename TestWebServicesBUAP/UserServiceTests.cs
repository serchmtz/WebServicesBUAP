using NUnit.Framework;
using WebServicesBUAP;
using System;

namespace TestWebServicesBUAP
{
    [TestFixture]
    public class UserServiceTests
    {
        private UserService usrSrv;

        public enum JSONType
        {
            Valid,
            Invalid,
            Malformed,
            PartialValid,
            PartialInvalid,
            PartialMalformed
        }

        [OneTimeSetUp]
        public void Init()
        {
            usrSrv = new UserService();
           
        }
        private string getTestJSON(JSONType type)
        {
            string userInfoJSON = "";

            switch (type)
            {
                case JSONType.Valid:
                    userInfoJSON = @"{
                                'correo': 'fulano.lopez@mail.net',
                                'nombre': 'Fulano Lopez',
                                'rol': 'ventas',
                                'telefono': '222-7-18-43-56'
                              }";
                    break;
                case JSONType.Invalid:
                    userInfoJSON = @"{
                                'correo': 'fulano.lopez@mail.net',                              
                                'rol': 'ventas',
                                'telefono': '222-7-18-43-56'
                              }";
                    break;
                case JSONType.Malformed:
                    userInfoJSON = @"{
                                'correo': 'fulano.lopez@mail.net                       
                                'rol': 'ventas',
                                'telefono': '222-7-18-43-56'
                              }";
                    break;
                case JSONType.PartialValid:
                    userInfoJSON = @"{
                                'correo': 'fulano.lopez@newmail.net'
                              }";
                    break;
                case JSONType.PartialInvalid:
                    userInfoJSON = @"{
                                'correo': ''
                              }";
                    break;
                case JSONType.PartialMalformed:
                    userInfoJSON = @"{
                                'correo': '
                              }";
                    break;

            }
            return userInfoJSON;
        }

        [TestCase(500)]
        [TestCase(301)]
        [TestCase(-1)]
        public void TestGetResponse(int code)
        {
            Respuesta res = usrSrv.GetResponse(code);
            Assert.NotNull(res);
            Assert.AreEqual(res.Code, code);
            Assert.IsNotEmpty(res.Message);
        }

        [TestCase("pruebas2", "12345678b", ExpectedResult = 99)]
        [TestCase("pruebas1", "12345678a", ExpectedResult = 504)]
        [TestCase("pruebas2", null, ExpectedResult = 501)]
        [TestCase("as32112", null, ExpectedResult = 500)]
        [TestCase(null, null, ExpectedResult = 500)]
        [TestCase("", null, ExpectedResult = 500)]
        [TestCase("?", null, ExpectedResult = 500)]
        public int TestAuthenticate(string user, string pass)
        {
            Respuesta res = usrSrv.Authenticate(user, pass);
            Assert.NotNull(res);
            return res.Code;
        }


        [TestCase("pruebas1", "12345678a", "pruebas2", JSONType.Valid, ExpectedResult = 504)]
        [TestCase("pruebas2", "12345678b", "pruebas2", JSONType.Valid, ExpectedResult = 506)]
        [TestCase("pruebas2", "12345678b", "pruebas1", JSONType.Valid, ExpectedResult = 402)]
        [TestCase("pruebas2", "12345678b", "pruebas1", JSONType.Invalid, ExpectedResult = 304)]
        [TestCase("pruebas2", "12345678b", "pruebas1", JSONType.Malformed, ExpectedResult = 305)]
        public int TestSetUserInfo(string user, string pass, string searchedUser, JSONType type)
        {
            Respuesta res = usrSrv.SetUserInfo(user, pass, searchedUser, getTestJSON(type));
            return res.Code;
        }

        [TestCase("pruebas1", "12345678a", "pruebas2", JSONType.PartialValid, ExpectedResult = 504)]
        [TestCase("pruebas2", "12345678b", "pruebas2asdasd", JSONType.PartialValid, ExpectedResult = 507)]
        [TestCase("pruebas2", "12345678b", "pruebas1", JSONType.PartialValid, ExpectedResult = 403)]
        [TestCase("pruebas2", "12345678b", "pruebas1", JSONType.PartialInvalid, ExpectedResult = 306)]
        [TestCase("pruebas2", "12345678b", "pruebas1", JSONType.PartialMalformed, ExpectedResult = 305)]
        public int TestUpdateUserInfo(string user, string pass, string searchedUser, JSONType type)
        {
            Respuesta res = usrSrv.UpdateUserInfo(user, pass, searchedUser, getTestJSON(type));
            return res.Code;
        }

        [TestCase(JSONType.Valid, false, ExpectedResult = 0)]
        [TestCase(JSONType.Invalid, false, ExpectedResult = 304)]
        [TestCase(JSONType.Malformed, false, ExpectedResult = 305)]
        [TestCase(JSONType.PartialValid, true, ExpectedResult = 0)]
        [TestCase(JSONType.PartialInvalid, true, ExpectedResult = 306)]
        [TestCase(JSONType.PartialMalformed, true, ExpectedResult = 305)]
        public int TestValidateJSON(JSONType type, bool partial)
        {
            var (isValid, code) = usrSrv.ValidateJSON(getTestJSON(type), partial);
            return code;
        }

        [TestCase("pru", "asd", "asd", "asd", ExpectedResult = 500)]
        [TestCase("pruebas1", "asd", "asd", "asd", ExpectedResult = 501)]
        [TestCase("pruebas1", "12345678a", "asd", "asd", ExpectedResult = 504)]
        [TestCase("pruebas2", "12345678b", "pruebas1", "128a", ExpectedResult = 502)]
        [TestCase("pruebas2", "12345678b", "pruebas1 -23", "128a", ExpectedResult = 503)]
        [TestCase("pruebas2", "12345678b", "pruebas1", "12345678a", ExpectedResult = 508)]
        [TestCase("pruebas2", "12345678b", "pruebas44", "12345678uu", ExpectedResult = 404)]
        public int TestSetUser(string user, string pass, string newUser, string newPass)
        {
            Respuesta res = usrSrv.SetUser(user, pass, newUser, newPass);
            return res.Code;
        }


        [TestCase("pru", "asd", "asd", "asd", "asda", ExpectedResult = 500)]
        [TestCase("pruebas1", "asd", "asd", "asd", "asdasd", ExpectedResult = 501)]
        [TestCase("pruebas1", "12345678a", "asd", "asd", "asdasd", ExpectedResult = 504)]
        [TestCase("pruebas2", "12345678b", "pruebas1", "pruebas1", "128a", ExpectedResult = 502)]
        [TestCase("pruebas2", "12345678b", "pruebas1", "pruebas 1", "128a", ExpectedResult = 503)]
        [TestCase("pruebas2", "12345678b", "pruebas1error", "prubas1", "12345678a", ExpectedResult = 505)]
        [TestCase("pruebas2", "12345678b", "pruebas44", "pruebas44", "12345678uu", ExpectedResult = 401)]
        [TestCase("pruebas2", "12345678b", "pruebas44", "pruebas44new", "12345678uu", ExpectedResult = 401)]
        public int TestUpdateUser(string user, string pass, string oldUser, string newUser, string newPass)
        {
            Respuesta res = usrSrv.UpdateUser(user, pass, oldUser, newUser, newPass);
            return res.Code;
        }
    }
}