using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using NearMeAPI.Models;
using log4net;
using System.Reflection;

namespace NearMeAPI.Controllers
{
    public class NearMeController : ApiController
    {
        // Insert API Key here
        private static string userIP = "";

        private static readonly string APIKey = "";
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Route("api/{type}")]
        [HttpGet]
        public IHttpActionResult TypeSearch(string type)
        {
            string userIP = GetIP();
            if (string.IsNullOrEmpty(userIP)) return NotFound();

            JObject geoLocation = GetGeoLocation(userIP);
            if (geoLocation == null) return NotFound();

            string Latitude = geoLocation["lat"].ToString().Replace(",", ".");
            string Longitude = geoLocation["lon"].ToString().Replace(",", ".");

            string restOfUrl = "/maps/api/place/nearbysearch/json?location=" + Latitude +
                ", " + Longitude + "&radius=1000&types=" + type + "&key=" + APIKey;

            JObject placesObject = GetPlaces(restOfUrl);
            if (placesObject == null) return NotFound();

            List<Place> googlePlaces = CreateReturnObject(placesObject);

            return Ok(googlePlaces);
        }

        [Route("api/location/{latitude}/{longitude}/{type}")]
        [HttpGet]
        public IHttpActionResult LocationSearch(string latitude, string longitude, string type)
        {
            string restOfUrl = "/maps/api/place/nearbysearch/json?location=" + latitude +
                ", " + longitude + "&radius=1000&types=" + type + "&key=" + APIKey;

            JObject placesObject = GetPlaces(restOfUrl);
            if (placesObject == null) return NotFound();

            List<Place> googlePlaces = CreateReturnObject(placesObject);

            return Ok(googlePlaces);
        }

        private static JObject GetGeoLocation(string userIP)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://ip-api.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync("json/" + userIP).Result;
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsAsync<JObject>().Result;
                }
                else
                {
                    Log.Debug("Something went wrong, we did not manage to establish a connection to the IP API");
                    return null;
                }
            }
        }

        private static string GetIP()
        {
            if(String.IsNullOrEmpty(userIP))
            {
                userIP = HttpContext.Current.Request.UserHostAddress;
            }

            // Fixing the issue with the localhost loopback address
            if (userIP == "::1")
            {
                using (WebClient client = new WebClient())
                {
                    userIP = client.DownloadString("https://api.ipify.org"); ;
                }
            }

            IPAddress address;

            if (!IPAddress.TryParse(userIP, out address))
            {
                Log.Debug("Problem with user IP. Ip is incorrect. Recieved IP: " + userIP);
                userIP = "";
            }

            return userIP;
        }

        private static JObject GetPlaces(string url)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://maps.googleapis.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsAsync<JObject>().Result;
                }
                else
                {
                    Log.Debug("We did not manage to retrieve information from the Google Places API. Possible problem with key?");
                    return null;
                }
            }
        }

        private static List<Place> CreateReturnObject(JObject placesObject)
        {
            JArray placesArray = placesObject["results"] as JArray;
            List<Place> googlePlaces = new List<Place>();

            foreach (JObject placeObject in placesArray)
            {
                googlePlaces.Add(
                    new Place { Name = placeObject["name"].ToString(), Location = placeObject["vicinity"].ToString() }
                );
            }

            return googlePlaces;
        }

        // An HttpRequest should not be mocked or faked. So this is my solution to not having to mock it in the unit tests
        public string SetIP(string newIP)
        {
            return userIP = newIP;
        }
    }
}
