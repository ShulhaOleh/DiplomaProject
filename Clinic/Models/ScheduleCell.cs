using System.Collections.Generic;

namespace Clinic.Models
{
    public class ScheduleCell
    {
        public bool IsFree { get; set; }
        public bool IsBusy { get; set; }
        public bool IsBreak { get; set; }

        public string Display
        {
            get
            {
                if (IsBreak) return "Обід";
                if (IsBusy) return "Зайнято";
                if (IsFree) return "Вільно";
                return "";
            }
        }

        public string CellColor
        {
            get
            {
                if (IsBreak) return "LightGray";
                if (IsBusy) return "IndianRed";
                if (IsFree) return "LightGreen";
                return "White";
            }
        }

    }
}
