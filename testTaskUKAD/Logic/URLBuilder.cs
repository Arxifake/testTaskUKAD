using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testTaskUKAD.Logic
{
    public class URLBuilder : IURLBuilder
    {
        public string BuildURL(string baseUrl, string url)
        {
            if (baseUrl.EndsWith("/"))
            {
                baseUrl = baseUrl.Remove(baseUrl.Length - 1);
            }
            if (url.StartsWith("/") || !url.EndsWith("/"))
            {
                if (url.StartsWith("/") && !url.EndsWith("/"))
                {
                    return baseUrl + url + "/";
                    //sitemapLinks.Add(url + node["loc"].InnerText + "/");
                }
                else if (url.StartsWith("/"))
                {
                    return baseUrl + url;
                    //sitemapLinks.Add(url + node["loc"].InnerText);
                }
                else return url + "/"; 
                    //sitemapLinks.Add(node["loc"].InnerText + "/");

            }
            else
            {
                return url;
                //sitemapLinks.Add(node["loc"].InnerText);
            }
        }
    }
}
