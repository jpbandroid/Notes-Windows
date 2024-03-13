using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Notes.ViewModels;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Notes.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        NoteStorage.ItemsSource = DataAccess.GetData();
    }

    private async void SaveContent()
    {
        DataAccess.AddData(Title.Text);

        NoteStorage.ItemsSource = DataAccess.GetData();
        byte[] bytes;
        StorageFile file = await DownloadsFolder.CreateFileAsync("file.rtf", CreationCollisionOption.GenerateUniqueName);

        if (file != null)
        {
            // write to file
            using (Windows.Storage.Streams.IRandomAccessStream randAccStream =
                await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
            {
                editor.Document.SaveToStream(Microsoft.UI.Text.TextGetOptions.FormatRtf, randAccStream);

                randAccStream.Seek(0);
                var dataReader = new DataReader(randAccStream);
                await dataReader.LoadAsync((uint)randAccStream.Size);
                bytes = new byte[randAccStream.Size];
                dataReader.ReadBytes(bytes);
            }


            SqliteConnection connection = new SqliteConnection("Filename=richEditBox.db");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS myTable (
                content BLOB
            )
            ";
            command.ExecuteNonQuery();

            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText =
            @"
            INSERT INTO myTable (content)
            VALUES (@content)
            ";

            insertCommand.Parameters.Add("@content", SqliteType.Blob, bytes.Length);
            insertCommand.Parameters["@content"].Value = bytes;

            var task1 = insertCommand.ExecuteNonQueryAsync();
        }
    }

    private async void LoadContent()
    {
        Title.Text = NoteStorage.SelectedItem.ToString();

        object[] values = new object[2];

        SqliteConnection connection = new SqliteConnection("Filename=richEditBox.db");
        connection.Open();
        var retrieveCommand = connection.CreateCommand();
        retrieveCommand.CommandText =
            @"SELECT * from myTable";
        var reader = retrieveCommand.ExecuteReader();
        while (reader.Read())
        {
            System.Diagnostics.Debug.WriteLine(reader.GetValues(values));
        }
        var dataBytes = values[0] as byte[];
        var dataBuffer = dataBytes.AsBuffer();
        InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
        await randomAccessStream.WriteAsync(dataBuffer);
        randomAccessStream.Seek(0);
        editor.Document.LoadFromStream(Microsoft.UI.Text.TextSetOptions.FormatRtf, randomAccessStream);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        SaveContent();
    }

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        LoadContent();
    }
}
