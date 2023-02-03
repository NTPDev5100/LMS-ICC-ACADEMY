using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace AppZim.ZIM
{
    public class DataApi
    {
        public class DataAPI
        {
            public int Code { get; set; }
            public string Message { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public object Data { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public int TotalResult { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public int PageSize { get; set; }
        }
    }
}