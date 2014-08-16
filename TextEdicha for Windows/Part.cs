using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TextEdicha_Windows
{
    [JsonObject(MemberSerialization.OptIn)]

    public class Part
    {

        [JsonProperty]

        public string mode { get; set; }


        [JsonProperty]

        public int lastid { get; set; }

        [JsonProperty]

        public string name { get; set; }

        [JsonProperty]

        public string comment { get; set; }

        [JsonProperty]

        public string ip { get; set; }

        [JsonProperty]

        public string time { get; set; }

        [JsonProperty]

        public string channel { get; set; }

        [JsonProperty]

        public Boolean rom { get; set; }

        [JsonProperty]

        public string id { get; set; }


        public Part()
        {

        }



        public string ToJsonString()
        {

            return JsonConvert.SerializeObject(this);

        }

        public static Part Deserialize(string jsonString)
        {

            return JsonConvert.DeserializeObject<Part>(jsonString);

        }

    }
}
