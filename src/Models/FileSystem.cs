using Newtonsoft.Json;
using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SierraHOTAS
{
    public class FileSystem : IFileSystem
    {
        public string LastSavedFileName { get; set; }

        private static string BuildCurrentPath(string fileName)
        {
            var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var folder = Path.GetDirectoryName(path);
            fileName = Path.Combine(folder, fileName);
            return fileName;
        }

        public void SaveQuickProfilesList(Dictionary<int, QuickProfileItem> list, string fileName)
        {
            fileName = BuildCurrentPath(fileName);
            Debug.WriteLine($"Saving Quick List as :{fileName}");
            using (var file = File.CreateText(fileName))
            {
                var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                serializer.Serialize(file, list);
            }
        }

        public Dictionary<int, QuickProfileItem> LoadQuickProfilesList(string fileName)
        {
            var path = BuildCurrentPath(fileName);

            if (!File.Exists(path)) return null;

            using (var file = File.OpenText(path))
            {
                var serializer = new JsonSerializer();
                serializer.Converters.Add(new CustomJsonConverter());
                try
                {
                    var list = (Dictionary<int, QuickProfileItem>)serializer.Deserialize(file, typeof(Dictionary<int, QuickProfileItem>));
                    return list;
                }
                catch (Exception e)
                {
                    Logging.Log.Error($"Could not deserialize {fileName}\nStack:{e}");
                    throw;
                }
            }
        }

        public string ChooseHotasProfileForQuickLoad()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                FileName = LastSavedFileName ?? "Document",
                DefaultExt = ".json",
                Filter = "Sierra Hotel (.json)|*.json"
            };

            var result = dlg.ShowDialog();
            return result != true ? null : dlg.FileName;
        }

        public void FileSave(IHOTASCollection deviceList)
        {
            if (string.IsNullOrWhiteSpace(LastSavedFileName))
            {
                FileSaveAs(deviceList);
            }
            else
            {
                BaseSave(LastSavedFileName, deviceList);
            }
        }

        public void FileSaveAs(IHOTASCollection deviceList)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "New Mapping",
                DefaultExt = ".json",
                Filter = "Sierra Hotel (.json)|*.json"
            };

            var result = dlg.ShowDialog();

            if (result != true) return;
            LastSavedFileName = dlg.FileName;
            BaseSave(LastSavedFileName, deviceList);
        }

        private static void BaseSave(string fileName, IHOTASCollection deviceList)
        {
            Debug.WriteLine($"Saving profile as :{fileName}");
            using (var file = File.CreateText(fileName))
            {
                var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                serializer.Serialize(file, deviceList);
            }
        }

        public IHOTASCollection FileOpenDialog()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                FileName = LastSavedFileName ?? "Document",
                DefaultExt = ".json",
                Filter = "Sierra Hotel (.json)|*.json"
            };

            var result = dlg.ShowDialog();
            if (result != true) return null;
            
            return FileOpen(dlg.FileName);
        }

        public IHOTASCollection FileOpen(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;

            using (var file = File.OpenText(path))
            {
                Logging.Log.Info($"Reading profile from :{path}");

                var serializer = new JsonSerializer();
                serializer.Converters.Add(new CustomJsonConverter());

                IHOTASCollection collection;
                try
                {
                    collection = (HOTASCollection) serializer.Deserialize(file, typeof(HOTASCollection));
                }
                catch (JsonReaderException readerException)
                {
                    Logging.Log.Error($"Could not deserialize {file}\nPlease verify this a SierraHOTAS compatible JSON file.\n\nStack:{readerException}");
                    return null;
                }
                catch (Exception e)
                {
                    Logging.Log.Error($"Could not deserialize {file}\nStack:{e}");
                    return null;
                }

                LastSavedFileName = path;
                return collection;
            }
        }

        public string GetSoundFileName()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                DefaultExt = ".mp3",
                Filter = "Audio Clip (.mp3)|*.mp3|(all)|*.*",
                InitialDirectory = $@"{Environment.CurrentDirectory}\Sounds"
            };

            var result = dlg.ShowDialog();
            return result != true ? null : dlg.FileName;
        }
    }
}
