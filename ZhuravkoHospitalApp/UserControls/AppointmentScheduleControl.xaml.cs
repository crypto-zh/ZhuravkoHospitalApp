using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZhuravkoHospitalApp.UserControls
{
    /// <summary>
    /// Логика взаимодействия для AppointmentScheduleControl.xaml
    /// </summary>
    public partial class AppointmentScheduleControl : UserControl
    {
        public AppointmentScheduleControl()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Entities.ScheduleAppointment scheduleAppointment)
            {
                BtnAppointment.Content = $"{scheduleAppointment.StartTime.ToString(@"hh\:mm")} - " +
                   $"{scheduleAppointment.EndTime.ToString(@"hh\:mm")}";
                switch (scheduleAppointment.AppointmentType)
                {
                    case Entities.AppointmentType.Free:
                        {
                            BtnAppointment.Visibility = Visibility.Visible;
                            BtnAppointment.IsEnabled = true;
                            BtnAppointment.Foreground = new SolidColorBrush(Colors.White);
                        }
                        break;
                    case Entities.AppointmentType.Busy:
                        {
                            BtnAppointment.IsEnabled = false;
                            BtnAppointment.Foreground = new SolidColorBrush(Colors.Black);
                            BtnAppointment.Visibility = Visibility.Visible;
                        }
                        break;
                    case Entities.AppointmentType.DayOff:
                        {
                            BtnAppointment.Visibility = Visibility.Hidden;
                        }
                        break;
                }
            }
        }
    }
}
