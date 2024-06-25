using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDS
{
    public class Common1
    {
        public List<Cookie> setCookie(string cookie)
        {
            var list = new List<Cookie>();

            var cookies = cookie.Split(';').ToList();

            cookies.ForEach(x =>
            {
                var a = new Cookie
                {
                    key = x.Split('=')[0].Trim(),
                    value = x.Split('=')[1].Trim()
                };

                list.Add(a);
            });

            return list;
        }
        public class Cookie
        {
            public string key { get; set; }
            public string value { get; set; }

        }
    }
}
