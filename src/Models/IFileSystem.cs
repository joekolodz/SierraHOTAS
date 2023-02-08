using System;
using System.Collections.Generic;

namespace SierraHOTAS.Models
{
    public interface IFileSystem
    {
        string LastSavedFileName { get; set; }
        void SaveQuickProfilesList(Dictionary<int, QuickProfileItem> list, string fileName);
        Dictionary<int, QuickProfileItem> LoadQuickProfilesList(string fileName);
        string ChooseHotasProfileForQuickLoad();
        void FileSave(IHOTASCollection deviceList);
        void FileSaveAs(IHOTASCollection deviceList);
        IHOTASCollection FileOpenDialog();
        IHOTASCollection FileOpen(string path);
        string GetSoundFileName();
        void SaveModeOverlayScreenPosition(string path, int x, int y);
        string ReadModeOverlayScreenPosition(string path);
    }
}
