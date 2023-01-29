using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testTaskUKAD.Logic
{
    public interface IURLBuilder
    {
        string BuildURL(string baseUrl, string url);
        string BuildURLWithoutAttr(string baseUrl, string url);
    }
}
