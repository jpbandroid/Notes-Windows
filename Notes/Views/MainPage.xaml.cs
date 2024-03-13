using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Controls;

using Notes.ViewModels;
using Windows.Storage;

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
    }

    public static void AddData(string inputText)
    {
        string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "NotesDatabase.db");
        using (SqliteConnection db =
          new SqliteConnection($"Filename={dbpath}"))
        {
            db.Open();

            SqliteCommand insertCommand = new SqliteCommand();
            insertCommand.Connection = db;

            // Use parameterized query to prevent SQL injection attacks
            insertCommand.CommandText = "INSERT INTO MyTable VALUES (NULL, @Entry);";
            insertCommand.Parameters.AddWithValue("@Entry", inputText);

            insertCommand.ExecuteReader();
        }

    }

    public static List<String> GetData()
    {
        List<String> entries = new List<string>();

        string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "NotesDatabase.db");
        using (SqliteConnection db =
           new SqliteConnection($"Filename={dbpath}"))
        {
            db.Open();

            SqliteCommand selectCommand = new SqliteCommand
                ("SELECT Text_Entry from MyTable", db);

            SqliteDataReader query = selectCommand.ExecuteReader();

            while (query.Read())
            {
                entries.Add(query.GetString(0));
            }
        }

        return entries;
    }
}
