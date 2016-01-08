using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NearMeAPI.Models;
using NearMeAPI.Controllers;
using System.Web.Http;
using System.Web.Http.Results;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Web.SessionState;
using System.Reflection;

namespace NearMeAPI.Tests.Controllers
{
    [TestClass]
    public class NearMeControllerTest
    {
        public readonly NearMeController controller = new NearMeController();

        [TestMethod]
        public void TestTypeSearch()
        {
            //HttpContext.Current = FakeHttpContext();
            controller.SetIP("::1");

            IHttpActionResult CorrectSearch = controller.TypeSearch("food");
            OkNegotiatedContentResult<List<Place>> GooglePlaces = CorrectSearch as OkNegotiatedContentResult<List<Place>>;

            Assert.IsNotNull(GooglePlaces);
            Assert.IsNotNull(GooglePlaces.Content);

            IHttpActionResult IncorrectSearch = controller.TypeSearch("munchies");
            OkNegotiatedContentResult<List<Place>> NoPlaces = IncorrectSearch as OkNegotiatedContentResult<List<Place>>;

            Assert.AreNotEqual(NoPlaces, GooglePlaces);
        }

        [TestMethod]
        public void TestLocationSearch()
        {
            IHttpActionResult CorrectSearch = controller.LocationSearch("51.50013", "-0.126305", "food");
            OkNegotiatedContentResult<List<Place>> GooglePlaces = CorrectSearch as OkNegotiatedContentResult<List<Place>>;

            Assert.IsNotNull(GooglePlaces);
            Assert.IsNotNull(GooglePlaces.Content);

            IHttpActionResult IncorrectSearch = controller.LocationSearch("92.43001", "-194.208521", "food");
            OkNegotiatedContentResult<List<Place>> NoPlaces = IncorrectSearch as OkNegotiatedContentResult<List<Place>>;

            int expectedPlaces = 0;
            Assert.AreEqual(expectedPlaces, NoPlaces.Content.Count);
        }

        // Creating a fake HttpContext
        public static HttpContext FakeHttpContext()
        {
            var httpRequest = new HttpRequest("", "http://localhost/", "");
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponse);

            var sessionContainer = new HttpSessionStateContainer("id", new SessionStateItemCollection(),
                                                    new HttpStaticObjectsCollection(), 10, true,
                                                    HttpCookieMode.AutoDetect,
                                                    SessionStateMode.InProc, false);

            httpContext.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
                                        BindingFlags.NonPublic | BindingFlags.Instance,
                                        null, CallingConventions.Standard,
                                        new[] { typeof(HttpSessionStateContainer) },
                                        null)
                                .Invoke(new object[] { sessionContainer });

            return httpContext;
        }
    }
}
