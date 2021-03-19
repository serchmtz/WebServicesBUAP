using NUnit.Framework;
using WebServicesBUAP;

namespace TestWebServicesBUAP
{
    [TestFixture]
    public class UserServiceTests
    {
        private IUserService usrSrv;
        private string userInfoJSON;
        [OneTimeSetUp]
        public void Init()
        {
            usrSrv = new UserService();
            userInfoJSON = @"{
                                'correo': 'mi.correo@mail.net',
                                'nombre': 'Jorge Luis Borges',
                                'rol': 'ventas',
                                'telefono': '222-7-18-62-98'
                              }";
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

        [TestCase("pruebas1", "12345678b", "pruebas2", ExpectedResult = 402)]
        public int TestSetUserInfo(string user, string pass, string searchedUser)
        {
            Respuesta res = usrSrv.SetUserInfo(user, pass, searchedUser, userInfoJSON);
            return res.Code;
        }
    }
}