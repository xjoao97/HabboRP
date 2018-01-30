using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Plus.HabboHotel.Roleplay.Web
{
    class WebEvent
    {
        [JsonProperty(PropertyName = "UserId")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "EventName")]
        public string EventName { get; set; }

        [JsonProperty(PropertyName = "ExtraData")]
        public string ExtraData { get; set; }

        [JsonProperty(PropertyName = "Bypass")]
        public bool Bypass { get; set; }

        [JsonProperty(PropertyName = "JSON")]
        public bool IsJSON { get; set; }
        public WebEvent(int UserId, string EventName, string ExtraData, bool Bypass, bool IsJSON)
        {
            this.UserId = UserId;
            this.EventName = EventName;
            this.ExtraData = ExtraData;
            this.Bypass = Bypass;
            this.IsJSON = IsJSON;
        }
    }
}