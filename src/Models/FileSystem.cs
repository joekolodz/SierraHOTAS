using Newtonsoft.Json;
using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using SierraHOTAS.Factories;

namespace SierraHOTAS
{
    /// <summary>
    /// SierraHOTAS specific library for system file I/O and related operations specific to the app (sounds, HOTAS objects, SierraHOTAS json file format, etc)
    /// </summary>
    public class FileSystem : IFileSystem
    {
        private static IFileIO _fileIo;
        private static FileDialogFactory _fileDialogFactory;
     
        public string LastSavedFileName { get; set; }

        public FileSystem(IFileIO fileIo, FileDialogFactory fileDialogFactory)
        {
             _fileIo = fileIo ?? throw new ArgumentNullException(nameof(fileIo));
            _fileDialogFactory = fileDialogFactory ?? throw new ArgumentNullException(nameof(fileDialogFactory)); ;
        }

        private static string BuildCurrentPath(string fileName)
        {
            var path = _fileIo.GetExecutingAssemblyLocation();
            var folder = _fileIo.GetDirectoryName(path);
            fileName = _fileIo.Combine(folder, fileName);
            return fileName;
        }

        public void SaveQuickProfilesList(Dictionary<int, QuickProfileItem> list, string fileName)
        {
            fileName = BuildCurrentPath(fileName);
            Logging.Log.Debug($"Saving Quick List as :{fileName}");
            using (var file = _fileIo.CreateText(fileName))
            {
                var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                serializer.Serialize(file, list);
            }
        }

        public Dictionary<int, QuickProfileItem> LoadQuickProfilesList(string fileName)
        {
            var path = BuildCurrentPath(fileName);

            if (!_fileIo.FileExists(path)) return null;

            using (var file = _fileIo.OpenText(path))
            {
                var serializer = new JsonSerializer();
                serializer.Converters.Add(new CustomJsonConverter());
                try
                {
                    var list = (Dictionary<int, QuickProfileItem>) serializer.Deserialize(file, typeof(Dictionary<int, QuickProfileItem>));
                    return list;
                }
                catch (JsonException jsonException)
                {
                    Logging.Log.Error($"Could not deserialize {file}\nPlease verify this a SierraHOTAS compatible JSON file.\n\nStack:{jsonException}");
                    throw;
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
            var dlg = _fileDialogFactory.CreateOpenFileDialog();

            dlg.FileName = LastSavedFileName ?? "Document";
            dlg.DefaultExt = ".json";
            dlg.Filter = "Sierra Hotel (.json)|*.json";

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
            var dlg = _fileDialogFactory.CreateSaveFileDialog();
            dlg.FileName = "New Mapping";
            dlg.DefaultExt = ".json";
            dlg.Filter = "Sierra Hotel (.json)|*.json";

            var result = dlg.ShowDialog();

            if (result != true) return;
            LastSavedFileName = dlg.FileName;
            BaseSave(LastSavedFileName, deviceList);
        }

        private static void BaseSave(string fileName, IHOTASCollection deviceList)
        {
            Logging.Log.Debug($"Saving profile as :{fileName}");
            using (var file = _fileIo.CreateText(fileName))
            {
                var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                serializer.Serialize(file, deviceList);
            }
        }

        public IHOTASCollection FileOpenDialog()
        {
            var dlg = _fileDialogFactory.CreateOpenFileDialog();
            dlg.FileName = LastSavedFileName ?? "Document";
            dlg.DefaultExt = ".json";
            dlg.Filter = "Sierra Hotel (.json)|*.json";

            var result = dlg.ShowDialog();
            if (result != true) return null;

            return FileOpen(dlg.FileName);
        }

        public IHOTASCollection FileOpen(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;

            using (var file = _fileIo.OpenText(path))
            {
                Logging.Log.Info($"Reading profile from :{path}");

                var serializer = new JsonSerializer();
                serializer.Converters.Add(new CustomJsonConverter());

                IHOTASCollection collection = null;
                try
                {
                    var o = (JObject)JToken.ReadFrom(new JsonTextReader(file));
                    var version = (string)o["JsonFormatVersion"];
                    
                    if (version == HOTASCollection.FileFormatVersion)
                    {
                        collection = (HOTASCollection)serializer.Deserialize(new JTokenReader(o), typeof(HOTASCollection));
                    }
                    //else - based on version, use factory to get old version and convert/map to latest version, then re-save
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
            var dlg = _fileDialogFactory.CreateOpenFileDialog();

            dlg.DefaultExt = ".mp3";
            dlg.Filter = "Audio Clip (.mp3)|*.mp3|(all)|*.*";
            dlg.InitialDirectory = $@"{_fileIo.GetCurrentDirectory()}\Sounds";

            var result = dlg.ShowDialog();
            return result != true ? null : dlg.FileName;
        }
    }
}
