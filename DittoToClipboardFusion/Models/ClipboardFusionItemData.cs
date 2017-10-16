using Newtonsoft.Json;

namespace DittoToClipboardFusion.Models
{
    internal sealed class ClipboardFusionItemData
    {
        [JsonProperty(PropertyName = "System.String")]
        public string Data { get; set; }

        public string FileDrop { get; set; }

        [JsonProperty(PropertyName = "Preferred DropEffect")]
        public string PreferredDropEffect { get; set; }

        public string OwnerPath { get; set; }
        public string WindowClass { get; set; }
        public string CommandLine { get; set; }

        public byte[] Bitmap { get; set; }
    }
}