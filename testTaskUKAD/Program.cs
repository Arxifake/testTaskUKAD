using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace testTaskUKAD
{
    internal class Program
    {
        public static char[] charsToTrim = { '*', ' ', '\'', '\n', '\t', '\r', };
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            bool retry = true;
            Result result = new Result { allLinks = new List<HtmlNode>(), linkTime = new Dictionary<string, double>()};
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
            
            string sitemapString = webClient.DownloadString(url+"/sitemap.xml");
            XmlDocument urlDoc= new XmlDocument();
            urlDoc.LoadXml(sitemapString);
            foreach(XmlNode node in urlDoc.GetElementsByTagName("url")) 
            {
                if (node["loc"] != null) 
                {
                    sitemapLinks.Add(node["loc"].InnerText);
                }
            }

            result = await CrawlSite(httpClient,url,result);
            Console.WriteLine();
            Console.WriteLine("Urls FOUNDED IN SITEMAP.XML but not founded after crawling a web site\r\n");
            foreach(var sitemapLink in sitemapLinks) 
            {
                if (!result.linkTime.ContainsKey(sitemapLink)) 
                {
                    Console.WriteLine(sitemapLink);
                }
            }
            Console.WriteLine() ;
            Console.WriteLine("Urls FOUNDED BY CRAWLING THE WEBSITE but not in sitemap.xml");
            foreach (var link in result.linkTime.Keys) 
            {
                if (!sitemapLinks.Contains(link))
                {
                    Console.WriteLine(link);
                }
            }
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

                if (url.StartsWith("/"))
                {
                    url = url.Remove(0,1);
                    res.linkTime.Add(httpClient.BaseAddress + url, totalTime);
                }
                else 
                {
                    res.linkTime.Add(url, totalTime);
                }
                
                var newList = doc.DocumentNode.Descendants("a").Where(link => link.GetAttributeValue("href", "").Trim(charsToTrim).StartsWith("/") || link.GetAttributeValue("href", "").Trim(charsToTrim).StartsWith(url)).ToList();
                foreach (var link in newList.ToList())
                {
                    
                    if (res.allLinks.Any(x => x.GetAttributeValue("href", "").Trim(charsToTrim) == link.GetAttributeValue("href", "").Trim(charsToTrim))|| res.linkTime.ContainsKey(link.GetAttributeValue("href", "").Trim(charsToTrim)) || link.GetAttributeValue("href","").Trim(charsToTrim)=="/")
                    {
                        newList.Remove(link);
                    }
                    else
                    {
                        res.allLinks.Add(link);
                    }
                }
                foreach (var link in newList)
                {
                    
                    var addResults = await CrawlSite(httpClient, link.GetAttributeValue("href", "").Trim(charsToTrim), res);
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
            catch(Exception ex) 
            {
                Console.WriteLine(url + " Error status code" + ex.Message);
                res.linkTime.Add(url, 0);
                return res;
            }
        }
    }
}
