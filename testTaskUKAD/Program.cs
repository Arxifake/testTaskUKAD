using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using testTaskUKAD.Logic;
using System.Web;

namespace testTaskUKAD
{
    internal class Program
    {
        public static IURLBuilder Builder = new URLBuilder();
        public static char[] charsToTrim = { '*', ' ', '\'', '\n', '\t', '\r' };
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            bool retry = true;
            Result result = new Result { allLinks = new List<string>(), linkTime = new Dictionary<string, double>()};
            List<string> sitemapLinks = new List<string>();
            Console.WriteLine("Enter website address");
            string url = Console.ReadLine();
            var httpClient = new HttpClient();
            WebClient webClient = new WebClient();
            webClient.Encoding= Encoding.UTF8;
            while (retry)
            {
                try
                {
                    httpClient.BaseAddress = new Uri(url);
                    retry= false;
                }
                catch (UriFormatException ex)
                {
                    Console.WriteLine("Please enter vailid adress");
                    url = Console.ReadLine();
                }
            }
            if (!url.EndsWith("/")) 
            {
                url+= "/";
            }
            result = await CrawlSite(httpClient, url, result);

            try
            {
                string sitemapString = webClient.DownloadString(url + "sitemap.xml");
                XmlDocument urlDoc = new XmlDocument();
                urlDoc.LoadXml(sitemapString);
                foreach (XmlNode node in urlDoc.GetElementsByTagName("url"))
                {
                    if (node["loc"] != null)
                    {
                        sitemapLinks.Add(Builder.BuildURL(url, node["loc"].InnerText));
                    }
                }


                Console.WriteLine();
                Console.WriteLine("Urls FOUNDED IN SITEMAP.XML but not founded after crawling a web site\r\n");
                foreach (var sitemapLink in sitemapLinks)
                {
                    if (!result.linkTime.ContainsKey(sitemapLink))
                    {
                        Console.WriteLine(sitemapLink);
                    }
                }
                Console.WriteLine();
                Console.WriteLine("Urls FOUNDED BY CRAWLING THE WEBSITE but not in sitemap.xml");
                foreach (var link in result.linkTime.Keys)
                {
                    if (!sitemapLinks.Contains(link))
                    {
                        Console.WriteLine(link);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sitemap wasn't found");
            }
            result.linkTime = result.linkTime.OrderBy(x=>x.Value).ToDictionary(x=>x.Key, x=>x.Value);
            Console.WriteLine() ;
            Console.WriteLine("Timing");
            foreach(var link in result.linkTime)
            {
                Console.WriteLine(link.Key +" in time " + link.Value+ " ms");
            }
            Console.WriteLine() ;
            Console.WriteLine("Urls(html documents) found after crawling a website: " + result.linkTime.Count);
            Console.WriteLine("Urls found in sitemap: " + sitemapLinks.Count);
            Console.ReadLine();
            
        }

        public static async Task<Result> CrawlSite(HttpClient httpClient, string url, Result res)
        {
            try
            {
                var timeBefore = DateTime.Now;
                var html = await httpClient.GetStringAsync(url);
                var totalTime = (DateTime.Now - timeBefore).TotalMilliseconds;

                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                //Console.WriteLine(url);
                res.linkTime.Add( url, totalTime);
                
                
                var stringList = doc.DocumentNode.Descendants("a").Where(link => link.GetAttributeValue("href", "").Trim(charsToTrim).StartsWith("/") || link.GetAttributeValue("href", "").Trim(charsToTrim).StartsWith(httpClient.BaseAddress.ToString())).Select(x => x.GetAttributeValue("href", "").Trim(charsToTrim)).ToList();
                var modifyList = new List<string>();
                foreach (var link in stringList)
                {
                   var newUrl = Builder.BuildURLWithoutAttr(link,httpClient.BaseAddress.ToString());
                    //https://asd.com/withoutattribute/

                    if (res.allLinks.Any(x => x == newUrl) ||
                        res.linkTime.ContainsKey(newUrl))
                    {
                        
                    }
                    else 
                    {
                        Console.WriteLine(newUrl);
                        res.allLinks.Add(newUrl);
                        modifyList.Add(newUrl);
                    }
                }
                foreach (var link in modifyList)
                {
                    var addResults = await CrawlSite(httpClient, link, res);
                    foreach (var addResult in addResults.linkTime)
                    {
                        if (!res.linkTime.ContainsKey(addResult.Key))
                        {
                            res.linkTime.Add(addResult.Key, addResult.Value);
                        }
                    }

                }
                return res;
            }
            catch(HttpRequestException ex) 
            {
                if (url.StartsWith("/")) 
                {
                    url = url.Remove(0, 1);
                    Console.WriteLine(httpClient.BaseAddress+ url + " Error status code" + ex.Message);
                    res.linkTime.Add(httpClient.BaseAddress + url, 0);
                }
                else 
                {
                    Console.WriteLine(url + " Error status code" + ex.Message);
                    res.linkTime.Add(url, 0);
                }
                
                return res;
            }
        }
    }
}
