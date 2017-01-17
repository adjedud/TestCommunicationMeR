using ActiveUp.Net.Mail;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestCommunicationMeR
{
    class Program
    {
        static void Main(string[] args)
        {
            Connection();
            //GetJson();
        }

        static void GetJson()
        {
            //string DomainControllerMethod = "https://www.moj-eracun.hr/exchange/getstatus?";
            string url = "https://www.moj-eracun.hr/exchange/getstatus?id=1041189&ver=cb590fe1-3138-4287-b8fc-058a56065152";

            //string url2 = DomainControllerMethod + ;
            using (var MeR = new WebClient())
            {
                var json = MeR.DownloadString(url);
                Console.WriteLine(json);
                Console.ReadLine();
                JsonResponse Result = JsonConvert.DeserializeObject<JsonResponse>(json);
                using (StreamWriter ResultFile = File.CreateText(@"C:\Temp\TestMerJSON.txt"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(ResultFile, json);
                }
                int DocumentStatus = Result.Status;
                Console.WriteLine(DocumentStatus);
                Console.ReadLine();
            }
        }
        public class JsonResponse
        {
            [JsonProperty]
            public int Id { get; set; }
            public string InterniBroj { get; set; }
            public string SupplierName { get; set; }
            public string SupplierID { get; set; }
            public int Status { get; set; }
            public int Type { get; set; }
            public string ParentDocumentID { get; set; }
            public DateTime IssueDate { get; set; }
            public DateTime UpdateDate { get; set; }
            public string Message { get; set; }
        }

        static void Connection ()
        {
            using (Imap4Client imap = new Imap4Client())
            {
                imap.Connect("mail.moj-eracun.hr");
                imap.Login("dostava@moj-eracun.hr", "m0j.d05tava");
                Mailbox inbox = imap.SelectMailbox("Inbox");
                string body = inbox.Fetch.BodyStructure(90000);
                imap.Disconnect();
            }
        }
    }
}
