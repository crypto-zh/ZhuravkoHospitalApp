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
using Word = Microsoft.Office.Interop.Word;

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
                Entities.Appointment appointment = new Entities.Appointment
                {
                    DoctorScheduleId = currentAppointment.SheduleId,
                    StartTime = currentAppointment.StartTime,
                    EndTime = currentAppointment.EndTime,
                    ClientId = 1
                };
                App.DataBase.Appointment.Add(appointment);
                App.DataBase.SaveChanges();
                if (MessageBox.Show("Вы записаны на прием, хотите распечатать талон?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes) 
                {
                    var doc = FormTicket(appointment);
                    if (doc != null)
                    {
                        doc.Application.Dialogs[Microsoft.Office.Interop.Word.WdWordDialog.wdDialogFilePrint].Show();
                        doc.Application.Quit();
                    }

                }
                ComboDoctor_SelectionChanged(ComboDoctor, null);
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Вы хотите распечатать только свободные записи?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var doc = FormDocument(false);
                if (doc != null)
                {
                    doc.Application.Dialogs[Microsoft.Office.Interop.Word.WdWordDialog.wdDialogFilePrint].Show();
                    doc.Application.Quit();
                }
                else
                {
                    MessageBox.Show("Проверьте соединение с базой и убедитесь, что выбрали врача");
                }
            }
            else
            {
                var doc = FormDocument(true);
                if (doc != null)
                {
                    doc.Application.Dialogs[Microsoft.Office.Interop.Word.WdWordDialog.wdDialogFilePrint].Show();
                    doc.Application.Quit();
                }
                else
                {
                    MessageBox.Show("Проверьте соединение с базой и убедитесь, что выбрали врача");
                }
            }
        }

        private Word.Document FormTicket(Entities.Appointment appointment)
        {
            try
            {
                var app = new Word.Application();
                var document = app.Documents.Add();

                Word.Paragraph heeader = document.Paragraphs.Add();

                Word.Range headerRange = heeader.Range;
                headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                headerRange.Bold = 1;
                headerRange.Text = "Врач: " + appointment.DoctorSchedule.Doctor.FullName;
                headerRange.Font.Size = 16;
                document.Paragraphs.Add();

                Word.Paragraph paragraph = document.Paragraphs.Add();
                Word.Range paragraphRange = paragraph.Range;
                paragraphRange.Text = "Время: ";
                paragraphRange.Text += $"{appointment.StartTime.ToString(@"hh\:mm")} - " +
                                           $"{appointment.EndTime.ToString(@"hh\:mm")}";
                return document;
            }
            catch
            {
                return null;
            }
        }
        private Word.Document FormDocument(bool allEntries)
        {
            try
            {
                var selectedDoctor = ComboDoctor.SelectedItem as Entities.Doctor;
                var startDate = DateTime.Parse("2021-12-12");
                var endDate = startDate.AddDays(5);
                if (selectedDoctor != null)
                {
                    var sheduleGenerator = new Utils.ScheduleGenerator(startDate, endDate, selectedDoctor.DoctorSchedule.ToList());
                    var headers = sheduleGenerator.GeneratedHeaders();
                    var times = sheduleGenerator.GenerateAppointments(headers);

                    var app = new Word.Application();
                    var document = app.Documents.Add();
                    Word.Paragraph heeader = document.Paragraphs.Add();

                    Word.Range headerRange = heeader.Range;
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    headerRange.Bold = 1;
                    headerRange.Text = "Врач: " + selectedDoctor.FullName;
                    headerRange.Font.Size = 16;
                    document.Paragraphs.Add();

                    Word.Paragraph paragraphEntry = document.Paragraphs.Add();
                    Word.Range paragraphEntryRange = paragraphEntry.Range;

                    Dictionary<Entities.ScheduleHeader, List<Entities.ScheduleAppointment>> data = new Dictionary<Entities.ScheduleHeader, List<Entities.ScheduleAppointment>>();
                    foreach(var header in headers)
                    {
                        data.Add(header, new List<Entities.ScheduleAppointment>());
                    }
                    foreach (var time in times)
                    {
                        foreach (var entry in time)
                        {
                            if (allEntries)
                            {
                                if (entry.AppointmentType != Entities.AppointmentType.DayOff)
                                {
                                    data[entry.ScheduleHeader].Add(entry); 
                                }
                            }
                            else
                            {
                                if (entry.AppointmentType == Entities.AppointmentType.Free)
                                {
                                    data[entry.ScheduleHeader].Add(entry);
                                }
                            }
                        }
                    }
                    foreach(var header in headers)
                    {
                        Word.Paragraph paragraphHeaderDay = document.Paragraphs.Add();
                        Word.Range paragraphHeaderDayRange = paragraphHeaderDay.Range;
                        paragraphHeaderDayRange.Text = header.Date.ToString("ddd dd MMMM") + ": ";
                        paragraphHeaderDayRange.Bold = 1;
                        document.Paragraphs.Add();

                        Word.Paragraph paragraph = document.Paragraphs.Add();
                        Word.Range paragraphRange = paragraph.Range;
                        if (data[header].Count != 0)
                        {
                            foreach (var entry in data[header])
                            {
                                paragraphRange.Text += $"{entry.StartTime.ToString(@"hh\:mm")} - " +
                                                                    $"{entry.EndTime.ToString(@"hh\:mm")}\t";
                            }
                        }
                        else
                        {
                            paragraphRange.Text += "Записей нет";
                        }
                        document.Paragraphs.Add();
                        document.Paragraphs.Add();
                    }
                    return document;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
