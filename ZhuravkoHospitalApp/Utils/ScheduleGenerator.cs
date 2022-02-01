using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ZhuravkoHospitalApp.Utils
{
    class ScheduleGenerator
    {
        private DateTime startDate;
        private DateTime endDate;
        private List<Entities.DoctorSchedule> doctorSchedule;

        public ScheduleGenerator(DateTime startDate, DateTime endDate, List<Entities.DoctorSchedule> schedules)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            doctorSchedule = schedules.Where(p => p.Date >= startDate.Date && p.Date <= endDate.Date).ToList();
        }
        public List<Entities.ScheduleHeader> GeneratedHeaders()
        {
            var result = new List<Entities.ScheduleHeader>();

            var beginDate = startDate;
            while (beginDate.Date < endDate)
            {
                result.Add(new Entities.ScheduleHeader()
                {
                    Date = beginDate.Date
                });
                beginDate = beginDate.AddDays(1);
            }
            return result;
        }

        public List<List<Entities.ScheduleAppointment>> GenerateAppointments(List<Entities.ScheduleHeader> headers)
        {
            var result = new List<List<Entities.ScheduleAppointment>>();
            if (doctorSchedule.Count > 0)
            {
                var minStartTime = doctorSchedule.Min(p => p.StartTime);
                var maxEndTime = doctorSchedule.Max(p => p.EndTime);

                var time = minStartTime;
                while (time < maxEndTime)
                {
                    var appointmentsPerInterval = new List<Entities.ScheduleAppointment>();
                    foreach (var header in headers)
                    {
                       
                        var currentShedule = doctorSchedule.FirstOrDefault(p => p.Date == header.Date);
                        var scheduleAppointment = new Entities.ScheduleAppointment
                        {
                            SheduleId = currentShedule?.Id ?? -1,
                            StartTime = time,
                            Date = header.Date,
                            ScheduleHeader = header,
                            EndTime = time.Add(TimeSpan.FromMinutes(30))

                        };
                        if (currentShedule != null)
                        {
                            var busyAppointment = currentShedule.Appointment.FirstOrDefault(p => p.StartTime == time);
                            if (busyAppointment != null)
                            {
                                scheduleAppointment.AppointmentType = Entities.AppointmentType.Busy;
                            }
                            else
                            {
                                if (time < currentShedule.StartTime || time >= currentShedule.EndTime)
                                {
                                    scheduleAppointment.AppointmentType = Entities.AppointmentType.DayOff;
                                }
                                else
                                {
                                    scheduleAppointment.AppointmentType = Entities.AppointmentType.Free;
                                }
                            }
                        }
                        else
                        {
                            scheduleAppointment.AppointmentType = Entities.AppointmentType.DayOff;
                        }
                        appointmentsPerInterval.Add(scheduleAppointment);


                    }
                    result.Add(appointmentsPerInterval);
                    time = time.Add(TimeSpan.FromMinutes(30));
                }
            }

            return result;
        }
    }
}
