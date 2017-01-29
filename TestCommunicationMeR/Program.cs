using ActiveUp.Net.Mail;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TestCommunicationMeR
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Imap4Client imap = new Imap4Client())
            {
                string Body;
                int ReferenceOrdinalNumber = 96600;
                string ReferenceDate = DateTime.Now.ToString("dd-MMM-yyy hh:mm:ss" + " +0100", CultureInfo.CreateSpecificCulture("en-US"));
                string DeliveryLink;

                imap.Connect("mail.moj-eracun.hr");
                imap.Login("dostava@moj-eracun.hr", "m0j.d05tava");
                Mailbox inbox = imap.SelectMailbox("Inbox");

                int OrdinalNumber = inbox.MessageCount;
                string Date = inbox.Fetch.InternalDate(OrdinalNumber);
                string MerPattern1 = ".*?";
                string MerPattern2 = "((?:http|https)(?::\\/{2}[\\w]+)(?:[\\/|\\.]?)(?:[^\\s\"]*))";
                Regex MerLink = new Regex(MerPattern1 + MerPattern2, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                string MerCheck1 = "(Return)";
                string MerCheck2 = "(-)";
                string MerCheck3 = "(Path)";
                string MerCheck4 = "(:)";
                string MerCheck5 = "( )";
                string MerCheck6 = "(dostava@moj-eracun\\.hr)";
                Regex MerChecker = new Regex(MerCheck1 + MerCheck2 + MerCheck3 + MerCheck4 + MerCheck5 + MerCheck6, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                while (Date != ReferenceDate)
                {
                    OrdinalNumber = inbox.MessageCount;
                    Date = inbox.Fetch.InternalDate(OrdinalNumber);

                    for (OrdinalNumber = inbox.MessageCount; Date != ReferenceDate && OrdinalNumber > ReferenceOrdinalNumber; OrdinalNumber--)
                    {
                        Body = inbox.Fetch.MessageString(OrdinalNumber);
                        try
                        {
                            Match MerLinkMatch = MerLink.Match(Body);
                            Match MerCheckerMatch = MerChecker.Match(Body);
                            if (MerLinkMatch.Success && MerCheckerMatch.Success)
                            {
                                DeliveryLink = MerLinkMatch.Groups[1].ToString();
                                ParseJson(DeliveryLink);
                            }
                        }
                        //TO DO: Add Exceptions
                        catch
                        {
                            
                            throw;
                        }
                    }
                    ReferenceOrdinalNumber = OrdinalNumber;
                }
                imap.Disconnect();
            }
        }

        static void ParseJson(string url)
        {
            // TO DO: Make try-catch for the entire method
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
                int DocumentId = Result.Id;
                int DocumentStatus = Result.Status;
                string BuyerId = Result.BuyerID;
                string SupplierId = Result.SupplierID;
                Console.WriteLine(DocumentStatus + " " + DocumentId + " " + BuyerId + " " + SupplierId);
                Console.ReadLine();
            }
        }

        public class JsonResponse
        {
            [JsonProperty]
            public string BuyerID { get; set; }
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
    }
}