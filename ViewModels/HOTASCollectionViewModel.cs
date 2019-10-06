using SierraHOTAS.Models;
using SierraHOTAS.ViewModel.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SierraHOTAS.ViewModel
{
    public class HOTASCollectionViewModel : IDisposable
    {
        public List<DeviceViewModel> Devices { get; set; }

        public event EventHandler<ButtonPressedViewModelEventArgs> ButtonPressed;

        private HOTASCollection _deviceList;

        private DeviceViewModel _selectedDevice;

        private ICommand _fileSaveCommand;

        public ICommand SaveFileCommand => _fileSaveCommand ?? (_fileSaveCommand = new CommandHandler(FileSave, () => CanExecute));

        private ICommand _fileOpenCommand;

        public ICommand OpenFileCommand => _fileOpenCommand ?? (_fileOpenCommand = new CommandHandler(FileOpen, () => CanExecute));

        private ICommand _selectionChangedCommand;

        public ICommand SelectionChangedCommand => _selectionChangedCommand ?? (_selectionChangedCommand = new RelayCommandWithParameter(SelectionChangedCommandEvent));

        private ICommand _exitApplicationCommand;

        public ICommand ExitApplicationCommand => _exitApplicationCommand ?? (_exitApplicationCommand = new CommandHandler(ExitApplication, () => CanExecute));

        public bool CanExecute => true;

        public HOTASCollectionViewModel()
        {
            _deviceList = new HOTASCollection();
        }

        public void Initialize()
        {
            _deviceList.ButtonPressed += DeviceList_ButtonPressed;
            _deviceList.Start();

            BuildDevicesViewModel();
        }

        public void Dispose()
        {
            _deviceList.Stop();
        }

        private void BuildDevicesViewModel()
        {
            Devices = _deviceList.Devices.Select(device => new DeviceViewModel(device)).ToList();
        }

        private void DeviceList_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            var device = Devices.First(d => d.InstanceId == e.Device.InstanceId);
            ButtonPressed?.Invoke(sender, new ButtonPressedViewModelEventArgs() { ButtonId = e.ButtonId, Device = device });
        }

        private static void ExitApplication()
        {
            Application.Current.Shutdown(0);
        }

        private void FileSave()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Document",
                DefaultExt = ".sh",
                Filter = "Sierra Hotel (.shotas)|*.shotas"
            };

            var result = dlg.ShowDialog();

            if (result == true)
            {
                var filename = dlg.FileName;
                Debug.WriteLine($"Saving profile to :{filename}");

                using (var file = File.CreateText(filename))
                {
                    var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                    serializer.Serialize(file, _deviceList);
                }
            }
        }
        private void FileOpen()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                FileName = "Document",
                DefaultExt = ".sh",
                Filter = "Sierra Hotel (.shotas)|*.shotas"
            };

            var result = dlg.ShowDialog();
            if (result != true) return;

            var filename = dlg.FileName;

            using (var file = File.OpenText(filename))
            {
                Debug.WriteLine($"Reading profile from :{filename}");
                var serializer = new JsonSerializer();

                _deviceList = (HOTASCollection)serializer.Deserialize(file, typeof(HOTASCollection));
                BuildDevicesViewModel();
            }
        }

        public void SelectionChangedCommandEvent(object device)
        {
            var x = device as object[];
            _selectedDevice = x[0] as DeviceViewModel;
            if (_selectedDevice != null) Debug.WriteLine($"Device Selected:{_selectedDevice.Name}");
        }

    }
}
