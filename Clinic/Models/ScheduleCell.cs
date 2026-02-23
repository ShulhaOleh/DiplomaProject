using System.Collections.Generic;
using System.Windows;

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
                if (IsBreak) return (string)Application.Current.FindResource("ScheduleCell_Break");
                if (IsBusy) return (string)Application.Current.FindResource("ScheduleCell_Busy");
                if (IsFree) return (string)Application.Current.FindResource("ScheduleCell_Free");
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
