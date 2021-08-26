using Newtonsoft.Json;
using NSubstitute;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class FileSystemTests
    {
        [Fact]
        public void basic_constructor()
        {
            Assert.Throws<ArgumentNullException>(() => new FileSystem(null, Substitute.For<FileDialogFactory>()));
            Assert.Throws<ArgumentNullException>(() => new FileSystem(Substitute.For<IFileIO>(), null));
            var fs = new FileSystem(Substitute.For<IFileIO>(), Substitute.For<FileDialogFactory>());
        }

        [Fact]
        public void save_quick_profile_list()
        {
            const string fileName = "some path";
            var subFileIO = Substitute.For<IFileIO>();
            var subDialogFactory = Substitute.For<FileDialogFactory>();

            var list = new Dictionary<int, QuickProfileItem>();

            subFileIO.Combine(Arg.Any<string>(), fileName).Returns(fileName);
            subFileIO.CreateText(Arg.Any<string>()).Returns(new StreamWriter(new MemoryStream()));

            var fs = new FileSystem(subFileIO, subDialogFactory);
            fs.SaveQuickProfilesList(list, fileName);

            subFileIO.Received().GetExecutingAssemblyLocation();
            subFileIO.Received().GetDirectoryName(Arg.Any<string>());
            subFileIO.Received().Combine(Arg.Any<string>(), fileName);
            subFileIO.Received().CreateText(fileName);
        }

        [Fact]
        public void load_quick_profile_list_file_not_exist()
        {
            const string fileName = "some path";
            const bool fileExists = false;
            var subFileIO = Substitute.For<IFileIO>();
            var subDialogFactory = Substitute.For<FileDialogFactory>();

            subFileIO.Combine(Arg.Any<string>(), fileName).Returns(fileName);
            subFileIO.FileExists(fileName).Returns(fileExists);

            var fs = new FileSystem(subFileIO, subDialogFactory);
            fs.LoadQuickProfilesList(fileName);

            subFileIO.Received(0).OpenText(fileName);
        }

        [Fact]
        public void load_quick_profile_list_file_exists()
        {
            const string fileName = "some path";
            const bool fileExists = true;
            const string fileContents = @"{""0"":{""Path"":""C:\\temp\\DCS BS2 - Virpil.json"",""AutoLoad"":false},""4"":{""Path"":""C:\\temp\\Star Citizen.json"",""AutoLoad"":true}}";
            var subFileIO = Substitute.For<IFileIO>();
            var subDialogFactory = Substitute.For<FileDialogFactory>();


            subFileIO.Combine(Arg.Any<string>(), fileName).Returns(fileName);
            subFileIO.FileExists(fileName).Returns(fileExists);

            subFileIO.OpenText(Arg.Any<string>()).Returns(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(fileContents))));

            var fs = new FileSystem(subFileIO, subDialogFactory);
            var list = fs.LoadQuickProfilesList(fileName);

            subFileIO.Received(1).OpenText(fileName);

            Assert.Equal(2, list.Count);
            Assert.NotNull(list[0]);
            Assert.NotNull(list[4]);
            Assert.Equal("c:\\temp\\dcs bs2 - virpil.json", list[0].Path.ToLower());
            Assert.False(list[0].AutoLoad);
            Assert.Equal("c:\\temp\\star citizen.json", list[4].Path.ToLower());
            Assert.True(list[4].AutoLoad);
        }

        [Fact]
        public void load_quick_profile_list_file_throws()
        {
            const string fileName = "some path";
            const bool fileExists = true;
            var fileContents = "";
            var subFileIO = Substitute.For<IFileIO>();
            var subDialogFactory = Substitute.For<FileDialogFactory>();

            subFileIO.Combine(Arg.Any<string>(), fileName).Returns(fileName);
            subFileIO.FileExists(fileName).Returns(fileExists);

            var fs = new FileSystem(subFileIO, subDialogFactory);

            subFileIO.OpenText(Arg.Any<string>()).Returns(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(fileContents))));
            var list = fs.LoadQuickProfilesList(fileName);
            Assert.Null(list);

            fileContents = "x";
            subFileIO.OpenText(Arg.Any<string>()).Returns(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(fileContents))));
            Assert.Throws<JsonReaderException>(() => fs.LoadQuickProfilesList(fileName));

            //missing a bracket
            fileContents = @"{""0"":{""Path"":""C:\\temp\\DCS BS2 - Virpil.json"",""AutoLoad"":false,""4"":{""Path"":""C:\\temp\\Star Citizen.json"",""AutoLoad"":true}}";
            subFileIO.OpenText(Arg.Any<string>()).Returns(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(fileContents))));
            Assert.Throws<JsonSerializationException>(() => fs.LoadQuickProfilesList(fileName));

            subFileIO.OpenText(Arg.Any<string>()).Returns(x => null);
            Assert.Throws<ArgumentNullException>(() => fs.LoadQuickProfilesList(fileName));
        }

        [Fact]
        public void choose_hotas_profile_for_quick_load()
        {
            var subFileIO = Substitute.For<IFileIO>();
            var subDialogFactory = Substitute.For<FileDialogFactory>();
            var subDialog = Substitute.For<IOpenFileDialog>();
            subDialog.FileName.Returns("some path");
            subDialogFactory.CreateOpenFileDialog().Returns(subDialog);

            subDialog.ShowDialog().Returns(true);
            var fs = new FileSystem(subFileIO, subDialogFactory);

            fs.LastSavedFileName = "";
            var fileName = fs.ChooseHotasProfileForQuickLoad();
            Assert.Empty(fileName);

            fs.LastSavedFileName = null;
            fileName = fs.ChooseHotasProfileForQuickLoad();
            Assert.Equal("Document", fileName);

            fs.LastSavedFileName = "last-file-saved";
            fileName = fs.ChooseHotasProfileForQuickLoad();
            Assert.Equal("last-file-saved", fileName);


            subDialog.ShowDialog().Returns(false);
            fs.LastSavedFileName = "last-file-saved";
            fileName = fs.ChooseHotasProfileForQuickLoad();
            Assert.Null(fileName);
        }

        [Fact]
        public void file_save_null()
        {
            var expectedFileName = "New Mapping";

            var subFileIO = Substitute.For<IFileIO>();
            var subDialogFactory = Substitute.For<FileDialogFactory>();
            var subDialog = Substitute.For<ISaveFileDialog>();
            subDialog.FileName.Returns("some path");
            subDialogFactory.CreateSaveFileDialog().Returns(subDialog);
            subDialog.ShowDialog().Returns(true);

            var subList = Substitute.For<IHOTASCollection>();
            var fs = new FileSystem(subFileIO, subDialogFactory);

            fs.LastSavedFileName = "";
            fs.FileSave(subList);
            Assert.Equal(expectedFileName, fs.LastSavedFileName);
            subFileIO.Received().WriteAllText(expectedFileName, Arg.Any<string>());
        }

        [Fact]
        public void file_save()
        {
            var expectedFileName = "not empty";

            var subFileIO = Substitute.For<IFileIO>();
            var subDialogFactory = Substitute.For<FileDialogFactory>();
            var subDialog = Substitute.For<ISaveFileDialog>();
            subDialog.FileName.Returns("some path");
            subDialogFactory.CreateSaveFileDialog().Returns(subDialog);
            subDialog.ShowDialog().Returns(true);

            var subList = Substitute.For<IHOTASCollection>();
            var fs = new FileSystem(subFileIO, subDialogFactory);

            fs.LastSavedFileName = expectedFileName;
            fs.FileSave(subList);
            Assert.Equal(expectedFileName, fs.LastSavedFileName);
            subFileIO.Received().WriteAllText(expectedFileName, Arg.Any<string>());
        }

        [Fact]
        public void file_save_as_true()
        {
            var expectedFileName = "New Mapping";

            var subFileIO = Substitute.For<IFileIO>();
            var subDialogFactory = Substitute.For<FileDialogFactory>();
            var subDialog = Substitute.For<ISaveFileDialog>();
            subDialog.FileName.Returns("some path");
            subDialogFactory.CreateSaveFileDialog().Returns(subDialog);
            subDialog.ShowDialog().Returns(true);

            var subList = Substitute.For<IHOTASCollection>();
            var fs = new FileSystem(subFileIO, subDialogFactory);

            fs.LastSavedFileName = "";
            fs.FileSave(subList);
            Assert.Equal(expectedFileName, fs.LastSavedFileName);
            subFileIO.Received().WriteAllText(expectedFileName, Arg.Any<string>());
        }

        [Fact]
        public void file_save_as_false()
        {
            var notExpectedFileName = "New Mapping";
            var expectedFileName = "some path";

            var subFileIO = Substitute.For<IFileIO>();
            subFileIO.CreateText(Arg.Any<string>()).Returns(new StreamWriter(new MemoryStream()));

            var subDialogFactory = Substitute.For<FileDialogFactory>();
            var subDialog = Substitute.For<ISaveFileDialog>();
            subDialog.FileName.Returns(expectedFileName);
            subDialogFactory.CreateSaveFileDialog().Returns(subDialog);
            subDialog.ShowDialog().Returns(false);

            var subList = Substitute.For<IHOTASCollection>();
            var fs = new FileSystem(subFileIO, subDialogFactory);

            fs.LastSavedFileName = expectedFileName;
            fs.FileSaveAs(subList);
            Assert.NotEqual(notExpectedFileName, fs.LastSavedFileName);
            Assert.Equal(expectedFileName, fs.LastSavedFileName);
            subFileIO.DidNotReceive().CreateText(expectedFileName);
        }

        [Fact]
        public void file_open_dialog_false()
        {
            var subFileIO = Substitute.For<IFileIO>();
            var subDialogFactory = Substitute.For<FileDialogFactory>();
            var subDialog = Substitute.For<IOpenFileDialog>();
            subDialogFactory.CreateOpenFileDialog().Returns(subDialog);
            subDialog.ShowDialog().Returns(false);

            var fs = new FileSystem(subFileIO, subDialogFactory);

            var actual = fs.FileOpenDialog();
            Assert.Null(actual);
        }

        [Fact]
        public void file_open_dialog_true()
        {
            const string fileContents = @"{""JsonFormatVersion"": ""1.0.0"",""Devices"":[{""DeviceId"":""3905f630-ed1c-11e9-8001-444553540000"",},{""DeviceId"":""ffeaf2a0-9145-11ea-8001-444553540000""}]}";
            var subFileIO = Substitute.For<IFileIO>();
            subFileIO.OpenText(Arg.Any<string>()).Returns(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(fileContents))));

            var subFileStream = Substitute.For<IFileStream>();
            subFileIO.CreateFileStream(Arg.Any<string>(), Arg.Any<FileMode>(), Arg.Any<FileAccess>()).Returns(subFileStream);
            var subDialogFactory = Substitute.For<FileDialogFactory>();
            var subDialog = Substitute.For<IOpenFileDialog>();
            subDialogFactory.CreateOpenFileDialog().Returns(subDialog);
            subDialog.ShowDialog().Returns(true);

            var fs = new FileSystem(subFileIO, subDialogFactory);

            var actual = fs.FileOpenDialog();
            Assert.Equal(2, actual.Devices.Count);
            Assert.NotNull(actual.Devices[0]);
            Assert.NotNull(actual.Devices[1]);
            Assert.Equal("3905f630-ed1c-11e9-8001-444553540000", actual.Devices[0].DeviceId.ToString());
            Assert.Equal("ffeaf2a0-9145-11ea-8001-444553540000", actual.Devices[1].DeviceId.ToString());
        }

        [Fact]
        public void file_open_null_path()
        {
            var fs = new FileSystem(Substitute.For<IFileIO>(), Substitute.For<FileDialogFactory>());
            var actual = fs.FileOpen(null);
            Assert.Null(actual);
            actual = fs.FileOpen("");
            Assert.Null(actual);
        }

        [Fact]
        public void file_open_no_exception()
        {
            const string fileContents = @"{""JsonFormatVersion"": ""1.0.0"",""Devices"":[{""DeviceId"":""3905f630-ed1c-11e9-8001-444553540000"",},{""DeviceId"":""ffeaf2a0-9145-11ea-8001-444553540000""}]}";
            var subFileIO = Substitute.For<IFileIO>();
            subFileIO.OpenText(Arg.Any<string>()).Returns(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(fileContents))));

            var subFileStream = Substitute.For<IFileStream>();
            subFileIO.CreateFileStream(Arg.Any<string>(), Arg.Any<FileMode>(), Arg.Any<FileAccess>()).Returns(subFileStream);

            var fs = new FileSystem(subFileIO, Substitute.For<FileDialogFactory>());

            var actual = fs.FileOpen("some path");
            Assert.Equal(2, actual.Devices.Count);
            Assert.NotNull(actual.Devices[0]);
            Assert.NotNull(actual.Devices[1]);
            Assert.Equal("3905f630-ed1c-11e9-8001-444553540000", actual.Devices[0].DeviceId.ToString());
            Assert.Equal("ffeaf2a0-9145-11ea-8001-444553540000", actual.Devices[1].DeviceId.ToString());
        }

        [Fact]
        public void file_open_not_current_version()
        {
            const string fileContents = @"{""JsonFormatVersion"": ""not-current-version"",""Devices"":[{""DeviceId"":""3905f630-ed1c-11e9-8001-444553540000"",},{""DeviceId"":""ffeaf2a0-9145-11ea-8001-444553540000""}]}";
            var subFileIO = Substitute.For<IFileIO>();
            subFileIO.OpenText(Arg.Any<string>()).Returns(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(fileContents))));

            var subFileStream = Substitute.For<IFileStream>();
            subFileIO.CreateFileStream(Arg.Any<string>(), Arg.Any<FileMode>(), Arg.Any<FileAccess>()).Returns(subFileStream);

            var fs = new FileSystem(subFileIO, Substitute.For<FileDialogFactory>());

            var actual = fs.FileOpen("some path");
            Assert.Null(actual);
        }

        [Fact]
        public void file_open_reader_exception()
        {
            const string fileContents = @"{""JsonFormatVersion"": ""1.0.0"",""Devices"":[{""DeviceId"" missing colon ""3905f630-ed1c-11e9-8001-444553540000"",},{""DeviceId"":""ffeaf2a0-9145-11ea-8001-444553540000""}]}";
            var subFileIO = Substitute.For<IFileIO>();
            subFileIO.OpenText(Arg.Any<string>()).Returns(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(fileContents))));

            var subFileStream = Substitute.For<IFileStream>();
            subFileIO.CreateFileStream(Arg.Any<string>(), Arg.Any<FileMode>(), Arg.Any<FileAccess>()).Returns(subFileStream);

            var fs = new FileSystem(subFileIO, Substitute.For<FileDialogFactory>());

            var actual = fs.FileOpen("some path");
            Assert.Null(actual);
        }

        [Fact]
        public void get_sound_file_name_dialog_false()
        {
            var subFileIO = Substitute.For<IFileIO>();
            var subDialogFactory = Substitute.For<FileDialogFactory>();
            var subDialog = Substitute.For<IOpenFileDialog>();
            subDialogFactory.CreateOpenFileDialog().Returns(subDialog);
            subDialog.ShowDialog().Returns(false);

            var fs = new FileSystem(subFileIO, subDialogFactory);

            var actual = fs.GetSoundFileName();
            Assert.Null(actual);
        }

        [Fact]
        public void get_sound_file_name_dialog_true()
        {
            var subFileIO = Substitute.For<IFileIO>();
            var subDialogFactory = Substitute.For<FileDialogFactory>();
            var subDialog = Substitute.For<IOpenFileDialog>();
            subDialogFactory.CreateOpenFileDialog().Returns(subDialog);
            subDialog.FileName.Returns("expected");
            subDialog.ShowDialog().Returns(true);

            var fs = new FileSystem(subFileIO, subDialogFactory);

            var actual = fs.GetSoundFileName();
            Assert.Equal("expected", actual);
        }
    }
}
