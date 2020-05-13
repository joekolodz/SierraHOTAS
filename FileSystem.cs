using Newtonsoft.Json;
using SierraHOTAS.Models;
using System;
using System.Diagnostics;
using System.IO;

namespace SierraHOTAS
{
    public class FileSystem
    {
        public static string LastSavedFileName { get; set; }

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
                var xxx = (HOTASCollection)serializer.Deserialize(file, typeof(HOTASCollection));
                return xxx;
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
