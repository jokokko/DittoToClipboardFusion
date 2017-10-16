using System;
using PetaPoco;

namespace DittoToClipboardFusion.Models
{
    [TableName("TblLocalRecentItems")]
    internal sealed class ClipboardFusionItem
    {
        [Column]
        public string ItemId { get; set; }

        [Column]
        public Int64 ItemLastModifiedDate { get; set; }

        [Column]
        public string ItemAPIMachineID { get; set; }

        [Column]
        public string ItemAPIUser { get; set; }

        [Column]
        public string ItemSource { get; set; }

        [Column]
        public int ItemIsDeleted { get; set; }

        [Column]
        public string ItemName { get; set; }

        [Column]
        public byte[] ItemData { get; set; }
    }
}