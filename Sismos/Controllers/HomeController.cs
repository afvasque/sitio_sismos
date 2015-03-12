using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure;

namespace Sismos.Controllers
{
    public static class JavaScript
    {
        public static string Serialize(object o)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(o);
        }
    }

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string[] pinsLat = new string[20];
            string[] pinsLong = new string[20];

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/2.5_month.csv");
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string results = sr.ReadToEnd();
            sr.Close();

            string[] tempStr = results.Replace('\n', '\r').Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

            string[] tempStr2 = new string[tempStr.Length-1];

            for(int i = 1; i < tempStr.Length; i++)
            {
                tempStr2[i - 1] = tempStr[i]; 
            }


            int num_rows = 20;
            int num_cols = 4;

            string[,] data = new string[num_rows, num_cols];

            for (int r = 0; r < num_rows; r++)
            {
                string[] line_r = tempStr2[r].Split(',');
                for (int c = 0; c < num_cols; c++)
                {
                    if(c == 3){
                        c = 4;
                        data[r, c-1] = line_r[c];
                    }
                    else
                    {
                        data[r, c] = line_r[c];
                    }
                    

                }

                pinsLat[r] = data[r, 1];
                pinsLong[r] = data[r, 2];
            }

            ViewData["pinLat"] = Sismos.Controllers.JavaScript.Serialize(pinsLat);
            ViewData["pinLong"] = Sismos.Controllers.JavaScript.Serialize(pinsLong);

            return View();
        }

        [HttpGet]
        public ActionResult webjob(string latitude, string longitude)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("webjobsqueue");
            queue.CreateIfNotExists();

            string s = latitude +','+ longitude;
            string basecode = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(s));

            CloudQueueMessage message = new CloudQueueMessage(basecode);
            queue.AddMessage(message);

            return Json(new { success = true, responseText = "Your message" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult result(string latitude)
        {
            return Content("Ok");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}