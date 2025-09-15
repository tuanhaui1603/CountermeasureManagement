using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountermeasureManagement
{
    public class DataRecord
    {
        public string No { get; set; }

        public string Date { get; set; }

        public string StatusError { get; set; }

        public string PartName { get; set; }

        public string Area { get; set; }

        public string NccC1 { get; set; }

        public string NccC2 { get; set; }

        public string PicQc { get; set; }

        public string Image { get; set; } // Thường là đường dẫn file hoặc URL

        public string ContentError { get; set; }

        public string OldError { get; set; }

        public string NewError { get; set; }

        public string Rank { get; set; }

        public string Qty { get; set; }

        public string QtyTotal { get; set; }

        public string Solution { get; set; }

        public string Action { get; set; }

        public string PlanComplete { get; set; }
        public string ActualComplete { get; set; }
        public string Reason { get; set; }
        public string Countermesure { get; set; }
    }
}
