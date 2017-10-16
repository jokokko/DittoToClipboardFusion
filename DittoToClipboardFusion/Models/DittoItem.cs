using PetaPoco;

namespace DittoToClipboardFusion.Models
{
    internal sealed class DittoItem
    {
        [Column]
        // ReSharper disable once InconsistentNaming
        public int LID { get; set; }

        [Column]
        public string MText { get; set; }

        [Column]
        public string StrClipBoardFormat { get; set; }

        [Column]
        public int LDate { get; set; }

        [Column]
        public byte[] OoData { get; set; }
    }
}