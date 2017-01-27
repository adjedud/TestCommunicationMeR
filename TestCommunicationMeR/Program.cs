using ActiveUp.Net.Mail;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp.Parser.Html;

namespace TestCommunicationMeR
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Imap4Client imap = new Imap4Client())
            {
                string body;
                int referenceOrdinalNumber = 96600;
                string referenceDate = DateTime.Now.ToString("dd-MMM-yyy hh:mm:ss" + " +0100", CultureInfo.CreateSpecificCulture("en-US"));
                string deliveryLink;
                var htmlParser = new HtmlParser();

                imap.Connect("mail.moj-eracun.hr");
                imap.Login("dostava@moj-eracun.hr", "m0j.d05tava");
                Mailbox inbox = imap.SelectMailbox("Inbox");

                int ordinalNumber = inbox.MessageCount;
                string date = inbox.Fetch.InternalDate(ordinalNumber);
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


                while (date != referenceDate)
                {
                    ordinalNumber = inbox.MessageCount;
                    date = inbox.Fetch.InternalDate(ordinalNumber);

                    for (ordinalNumber = inbox.MessageCount; date != referenceDate && ordinalNumber > referenceOrdinalNumber; ordinalNumber--)
                    {
                        body = inbox.Fetch.MessageString(ordinalNumber);
                        try
                        {
                            Match MerLinkMatch = MerLink.Match(body);
                            Match MerCheckerMatch = MerChecker.Match(body);
                            //TO DO: Add additional check based on sender
                            if (MerLinkMatch.Success && MerCheckerMatch.Success)
                            {
                                deliveryLink = MerLinkMatch.Groups[1].ToString();
                                GetJson(deliveryLink);
                            }
                        }
                        //TO DO: Add Exceptions
                        catch
                        {
                            
                            throw;
                        }
                    }
                    referenceOrdinalNumber = ordinalNumber;
                }
                imap.Disconnect();
            }
        }

        static void GetJson(string url)
        {
            // TO DO: Make try-catch for the entire method
            //string url = "https://www.moj-eracun.hr/exchange/getstatus?id=1041189&ver=cb590fe1-3138-4287-b8fc-058a56065152";

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
    }
}
