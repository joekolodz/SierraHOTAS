using Newtonsoft.Json;
using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace SierraHOTAS
{
    public class FileSystem
    {
        public static string LastSavedFileName { get; set; }

        public static void SaveQuickProfilesList(Dictionary<int, string> list, string fileName)
        {
            Debug.WriteLine($"Saving Quick List as :{fileName}");
            using (var file = File.CreateText(fileName))
            {
                var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                serializer.Serialize(file, list);
            }
        }

        public static Dictionary<int, string> LoadQuickProfilesList(string fileName)
        {
            using (var file = File.OpenText(fileName))
            {
                var serializer = new JsonSerializer();
                serializer.Converters.Add(new CustomJsonConverter());
                try
                {
                    var list = (Dictionary<int, string>)serializer.Deserialize(file, typeof(Dictionary<int, string>));
                    return list;
                }
                catch (Exception e)
                {
                    Logging.Log.Error($"Could not deserialize {fileName}\nStack:{e}");
                    throw;
                }
            }
        }

        public static void FileSave(HOTASCollection deviceList)
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

        public static void FileSaveAs(HOTASCollection deviceList)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "New Mapping",
                DefaultExt = ".json",
                Filter = "Sierra Hotel (.json)|*.json"
            };

            var result = dlg.ShowDialog();

            if (result == true)
            {
                LastSavedFileName = dlg.FileName;
                Logging.Log.Info($"Saving profile as :{LastSavedFileName}");

                BaseSave(LastSavedFileName, deviceList);
            }
        }

        private static void BaseSave(string fileName, HOTASCollection deviceList)
        {
            Debug.WriteLine($"Saving profile as :{fileName}");
            using (var file = File.CreateText(fileName))
            {
                var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                serializer.Serialize(file, deviceList);
            }
        }

        public static HOTASCollection FileOpen()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                FileName = LastSavedFileName ?? "Document",
                DefaultExt = ".json",
                Filter = "Sierra Hotel (.json)|*.json"
            };

            var result = dlg.ShowDialog();
            if (result != true) return null;

            LastSavedFileName = dlg.FileName;

            using (var file = File.OpenText(LastSavedFileName))
            {
                Logging.Log.Info($"Reading profile from :{LastSavedFileName}");

                var serializer = new JsonSerializer();
                serializer.Converters.Add(new CustomJsonConverter());

                HOTASCollection collection;
                try
                {
                    collection = (HOTASCollection)serializer.Deserialize(file, typeof(HOTASCollection));
                }
                catch (Exception e)
                {
                    Logging.Log.Error($"Could not deserialize {file}\nStack:{e}");
                    throw;
                }

                return collection;
            }
        }

        public static string GetSoundFileName()
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
