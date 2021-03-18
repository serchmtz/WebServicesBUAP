using NUnit.Framework;
using WebServicesBUAP;

namespace TestWebServicesBUAP
{
    [TestFixture]
    public class UserServiceTests
    {
        private IUserService usrSrv;
        [OneTimeSetUp]
        public void Init()
        {
            usrSrv = new UserService();
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
        [TestCase("pruebas2", null, ExpectedResult = 501)]
        [TestCase("as32112", null, ExpectedResult = 500)]
        public int TestAuthenticate(string user, string pass)
        {
            Respuesta res = usrSrv.Authenticate(user, pass);
            Assert.NotNull(res);
            return res.Code;
        }
    }
}