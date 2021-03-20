using NUnit.Framework;
using WebServicesBUAP;

namespace TestWebServicesBUAP
{
    [TestFixture]
    public class UserServiceTests
    {
        private IUserService usrSrv;

        public enum JSONType
        {
            Valid,
            Invalid,
            Malformed
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
                                'nombre': 'Fulano López',
                                'rol': 'rh',
                                'telefono': '222-7-18-43-56'
                              }";
                    break;
                case JSONType.Invalid:
                    userInfoJSON = @"{
                                'correo': 'fulano.lopez@mail.net',                              
                                'rol': 'rh',
                                'telefono': '222-7-18-43-56'
                              }";
                    break;
                case JSONType.Malformed:
                    userInfoJSON = @"{
                                'correo': 'fulano.lopez@mail                               
                                'rol': 'rh',
                                'telefono': '222-7-18-43-56'
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

        [TestCase("pruebas1", "12345678a", "pruebas2", JSONType.Valid, ExpectedResult = 504)]
        [TestCase("pruebas2", "12345678b", "pruebas2asdasd", JSONType.Valid, ExpectedResult = 507)]
        [TestCase("pruebas2", "12345678b", "pruebas1", JSONType.Valid, ExpectedResult = 403)]
        [TestCase("pruebas2", "12345678b", "pruebas1", JSONType.Invalid, ExpectedResult = 304)]
        [TestCase("pruebas2", "12345678b", "pruebas1", JSONType.Malformed, ExpectedResult = 305)]
        public int TestUpdateUserInfo(string user, string pass, string searchedUser, JSONType type)
        {
            Respuesta res = usrSrv.UpdateUserInfo(user, pass, searchedUser, getTestJSON(type));
            return res.Code;
        }
    }
}