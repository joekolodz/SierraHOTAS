using SierraHOTAS.Factories;
using SierraHOTAS.Models;
using SierraJSON;
using System;
using System.Collections.Generic;
using System.IO;

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

            try
            {
                _fileIo.WriteAllText(fileName, Serializer.ToJSON(list, new CustomSierraJsonConverter()));
            }
            catch (Exception e)
            {
                Logging.Log.Debug(e);
                throw;
            }
        }

        public Dictionary<int, QuickProfileItem> LoadQuickProfilesList(string fileName)
        {
            var path = BuildCurrentPath(fileName);

            if (!_fileIo.FileExists(path)) return null;

            try
            {
                var json = _fileIo.ReadAllText(path);
                var list = Serializer.ToObject<Dictionary<int, QuickProfileItem>>(json, null);
                return list;
            }
            catch (Serializer.SierraJSONException e)
            {
                Logging.Log.Error($"Could not deserialize {fileName}\nStack:{e}");
                throw;
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

            try
            {
                _fileIo.WriteAllText(fileName, Serializer.ToJSON(deviceList, new CustomSierraJsonConverter()));
            }
            catch (Exception e)
            {
                Logging.Log.Fatal(e);
                throw;
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

            IHOTASCollection collection;
            try
            {
                var json = _fileIo.ReadAllText(path);
                var version = (string)Serializer.GetToken("JsonFormatVersion", json);

                if (version == HOTASCollection.FileFormatVersion)
                {
                    collection = Serializer.ToObject<HOTASCollection>(json, new CustomSierraJsonConverter());
                    if (collection.ActionCatalog == null)
                    {
                        collection.SetCatalog(new ActionCatalog());
                    }
                    collection.ActionCatalog.PostDeserializeProcess();
                }
                //else - based on version, use factory to get old version and convert/map to latest version, then re-save
                else
                {
                    LastSavedFileName = "Version not supported. Auto conversion not implemented for this version.";
                    Logging.Log.Warn($"Warning opening file: {LastSavedFileName} Path:{path}");
                    return null;
                }
            }
            catch (Exception readerException)
            {
                Logging.Log.Error($"Could not deserialize {path}\nPlease verify this a SierraHOTAS compatible JSON file.\n\nStack:{readerException}");
                return null;
            }

            LastSavedFileName = path;
            return collection;
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

        public void SaveModeOverlayScreenPosition(string path, int x, int y)
        {
            try
            {
                _fileIo.WriteAllText(path, $"{x},{y}");
            }
            catch (IOException ioe)
            {
                Logging.Log.Error(ioe);
            }
            catch (Exception e)
            {
                Logging.Log.Fatal(e);
                throw;
            }
        }

        public string ReadModeOverlayScreenPosition(string path)
        {
            if (!_fileIo.FileExists(path)) return string.Empty;

            try
            {
                return _fileIo.ReadAllText(path);
            }
            catch (IOException ioe)
            {
                Logging.Log.Error(ioe);
                return string.Empty;
            }
            catch (Exception e)
            {
                Logging.Log.Fatal(e);
                throw;
            }
        }
    }
}
