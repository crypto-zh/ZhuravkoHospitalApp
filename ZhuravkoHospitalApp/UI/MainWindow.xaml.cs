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

namespace ZhuravkoHospitalApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ComboSpecialization.ItemsSource = App.DataBase.Specialization.ToList();
            ComboSpecialization.SelectedIndex = 0;
        }

        private void ComboSpecialization_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ComboSpecialization.SelectedItem as Entities.Specialization;
            if (item != null)
            {
                ComboDoctor.ItemsSource = App.DataBase.Doctor.ToList().Where(p => p.Specialization == item).ToList();
            }
        }

        private void ComboDoctor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDoctor = ComboDoctor.SelectedItem as Entities.Doctor;
            if (selectedDoctor != null)
            {
                GenerateShedule(selectedDoctor);
            }
        }

        private void GenerateShedule(Entities.Doctor doctor)
        {
            var startDate = DateTime.Parse("2021-12-12");
            var endDate = startDate.AddDays(5);
            var sheduleGenerator = new Utils.ScheduleGenerator(startDate, endDate, doctor.DoctorSchedule.ToList());
            var headers = sheduleGenerator.GeneratedHeaders();
            var appointments = sheduleGenerator.GenerateAppointments(headers);
            LoadShedule(headers, appointments);
        }

        private void LoadShedule(List<Entities.ScheduleHeader> headers, List<List<Entities.ScheduleAppointment>> appointments)
        {

            DGridShedule.Columns.Clear();
            for (int i = 0; i < headers.Count(); i++)
            {
                FrameworkElementFactory headerFactory = new FrameworkElementFactory(typeof(UserControls.ScheduleHeaderControl));
                headerFactory.SetValue(DataContextProperty, headers[i]);
                var headerTemplate = new DataTemplate
                {
                    VisualTree = headerFactory
                };
                FrameworkElementFactory cellFactory = new FrameworkElementFactory(typeof(UserControls.AppointmentScheduleControl));
                cellFactory.SetBinding(DataContextProperty, new Binding($"[{i}]"));
                cellFactory.AddHandler(MouseDownEvent, new MouseButtonEventHandler(BtnAppointment_MouseDown), true);
                var cellTemplate = new DataTemplate
                {
                    VisualTree = cellFactory
                };
                var columnTemplate = new DataGridTemplateColumn
                {
                    HeaderTemplate = headerTemplate,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    CellTemplate = cellTemplate
                };
                DGridShedule.Columns.Add(columnTemplate);
            }
            DGridShedule.ItemsSource = appointments;
        }

        private void BtnAppointment_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var currentControl = sender as UserControls.AppointmentScheduleControl;
            var currentAppointment = currentControl.DataContext as Entities.ScheduleAppointment;
            if (currentAppointment != null && currentAppointment.SheduleId > 0 && currentAppointment.AppointmentType == Entities.AppointmentType.Free)
            {
                App.DataBase.Appointment.Add(new Entities.Appointment
                {
                    DoctorScheduleId = currentAppointment.SheduleId,
                    StartTime = currentAppointment.StartTime,
                    EndTime = currentAppointment.EndTime,
                    ClientId = 1
                });
                App.DataBase.SaveChanges();
                MessageBox.Show("Вы записаны на прием");
                ComboDoctor_SelectionChanged(ComboDoctor, null);
            }
        }
    }
}
