using System.Collections.Generic;

namespace SierraHOTAS.Models
{
    public interface IFileSystem
    {
        string LastSavedFileName { get; set; }
        void SaveQuickProfilesList(Dictionary<int, string> list, string fileName);
        Dictionary<int, string> LoadQuickProfilesList(string fileName);
        string ChooseHotasProfileForQuickLoad();
        void FileSave(IHOTASCollection deviceList);
        void FileSaveAs(IHOTASCollection deviceList);
        IHOTASCollection FileOpenDialog();
        IHOTASCollection FileOpen(string path);
        string GetSoundFileName();
    }
}
