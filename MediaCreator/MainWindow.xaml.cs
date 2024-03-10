
using FFMpegCore;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using WindowsAPICodePack.Dialogs;
using static MediaToolkit.Model.Metadata;

namespace MediaCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public string SourceFolder
        {
            get { return _SourceFolder; }
            set { _SourceFolder = value; FileList.Clear(); foreach(var file in Directory.GetFiles(value)) FileList.Add(file); OnPropertyChanged("SourceFolder"); OnPropertyChanged("CanOutput"); }
        }
        public string _SourceFolder;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> FileList { get; set; } = new ObservableCollection<string>();
        public string SourceImage { get { return _SourceImage; } set { _SourceImage = value; OnPropertyChanged("SourceImage"); OnPropertyChanged("CanOutput"); } }
        public string _SourceImage;
        public string CurrentFile { get { return _CurrentFile; } set { _CurrentFile = value; OnPropertyChanged("CurrentFile");} }
        public string _CurrentFile;

        public string OutputFolder { get { return _OutputFolder; } set { _OutputFolder = value; OnPropertyChanged("OutputFolder"); OnPropertyChanged("CanOutput"); } }
        public string _OutputFolder { get; set; }

        public bool CanOutput { get { return !string.IsNullOrEmpty(OutputFolder) && !string.IsNullOrEmpty(SourceImage) && FileList.Count > 0; } }

        public MainWindow()
        {
            InitializeComponent();
            //Compile();
        }
        private void SourceSelect_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "Select Audio Source Folder";
            dialog.Multiselect = false;
            if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
                SourceFolder = dialog.FileName;
        }

        private void SourceSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
            dialog.Title = "Select Source Image";
            dialog.Multiselect = false;
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
                SourceImage = dialog.FileName;
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            FileList.RemoveAt(Files.SelectedIndex);
        }

        private void selectOutput_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "Select Output Folder";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                OutputFolder = dialog.FileName;
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            Run.IsEnabled = false;
            Task task = new Task(() =>
            {
                foreach (var file in FileList)
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    CurrentFile = name;
                    Files.Dispatcher.Invoke(new Action(() => { Files.SelectedItem = file; }));
                    FFMpeg.PosterWithAudio(SourceImage, file, $"{OutputFolder}\\{name}.mp4");
                }
                CurrentFile = "";
                Run.Dispatcher.Invoke(new Action(() => { Run.IsEnabled = true; }));
            });
            task.Start();
        }
    }
    public class HasSelectedItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int && ((int)value != -1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
