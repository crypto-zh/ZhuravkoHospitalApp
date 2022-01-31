using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZhuravkoHospitalApp.Entities
{
    class ScheduleAppointment
    {
        public int SheduleId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public AppointmentType AppointmentType { get; set; }
    }
}
