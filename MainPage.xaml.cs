using System.Collections.ObjectModel;
using System.Text.Json;


namespace tic_tac_toe

{
    public partial class MainPage : ContentPage
    {
        //ObservableCollection to store the list of players
        public ObservableCollection<Player> Players { get; set; }

        //Class for different properties of a player
        public class Player
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int BirthYear { get; set; }
            public int Wins { get; set; }
            public int Losses { get; set; }
            public int Draws { get; set; }
            public TimeSpan TotalTimePlayed { get; set; } //total of all games played

            //wins, losses, draws, and totaltimeplayed are set to 0 when a new player is created
            public Player()
            {
                Wins = 0;
                Losses = 0;
                Draws = 0;
                TotalTimePlayed = new TimeSpan(0, 0, 0);
            }

            // Combine player's name and birth year for display in UI in the picker on PlayerManager page
            public string PickerInfo => $"{FirstName} {LastName} {BirthYear}";
        }

        //Constructor for MainPage
        public MainPage()
        {
            // Initialize components, create ObservableCollection, and load players from JSON
            InitializeComponent();
            Players = new ObservableCollection<Player>();
            LoadPlayersFromJson();
            BindingContext = this;
        }


        // Load players from JSON file, sort them, and add to ObservableCollection for display
        private void LoadPlayersFromJson()
        {
            // Load players from JSON file
            List<Player> players = PlayerInfoSerializer.LoadPlayers();

            //Sort the list of players by computer player first, then wins, lastname, firstname so that the list is in order
            players = players.OrderByDescending(p => p.FirstName == "Computer")
                            .ThenByDescending(p => p.Wins)
                            .ThenBy(p => p.LastName ?? "")
                            .ThenBy(p => p.FirstName ?? "")
                            .ToList();

            // Clear the Players collection before adding players
            Players.Clear();

            // Add players to the Players collection if they don't already exist
            foreach (Player player in players)
            {
                // Check if the player already exists in the Players collection
                bool playerExists = Players.Any(p => p.FirstName == player.FirstName &&
                                                    p.LastName == player.LastName &&
                                                    p.BirthYear == player.BirthYear);

                // Only add the player if it doesn't already exist in the collection
                if (!playerExists)
                {
                    Players.Add(player);
                }
            }
        }


        // Class to serialize and deserialize player info to and from JSON
        public static class PlayerInfoSerializer
        {
            //tictactoe players json file path
            public static string FolderPath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PlayerData");
            public static string FilePath { get; } = Path.Combine(FolderPath, "players.json");

            // Save updated players to JSON file
            public static void SavePlayers(List<Player> players) //players in order, check above
            {
                try
                {
                    // Create directory for player data if it doesn't exist
                    string directoryPath = Path.GetDirectoryName(FilePath);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Load existing players from JSON file
                    List<Player> existingPlayers = LoadPlayers(); 

                    // Iterate through the updated players list
                    foreach (Player player in players)
                    {
                        // Find player with the same first name, last name, and birth year
                        Player existingPlayer = existingPlayers.FirstOrDefault(p =>
                            p.FirstName == player.FirstName &&
                            p.LastName == player.LastName &&
                            p.BirthYear == player.BirthYear);

                        //If the player exists, update the player's data
                        if (existingPlayer != null)
                        {
                            existingPlayer.Wins = player.Wins;
                            existingPlayer.Losses = player.Losses;
                            existingPlayer.Draws = player.Draws;
                            existingPlayer.TotalTimePlayed = player.TotalTimePlayed;
                        }
                        // If the player doesn't exist, add the player to the existing players list
                        else
                        {
                            existingPlayers.Add(player);
                        }
                    }

                    // Save the updated players list to the file
                    string jsonString = JsonSerializer.Serialize(existingPlayers);
                    File.WriteAllText(FilePath, jsonString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in saving players: {ex.Message}");
                    throw;
                }
            }
            
            // Load players from JSON file
            public static List<Player> LoadPlayers()
            {
                // Check if the file exists, and if it does, deserialize the its contents
                if (File.Exists(FilePath))
                {
                    // Read the JSON data from the file and convert it to a list of Player objects
                    string jsonString = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<List<Player>>(jsonString);
                }
                else
                {
                    // If the file doesn't exist, return an empty list (for example, if the user hasn't played any games yet)
                    return new List<Player>();
                }
            }
        }

        //Navigate to RulesPage
        private async void ToRulesPage_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RulesPage());
        }

        //Exit the application
        private void Exit_Clicked(object sender, EventArgs e)
        {
            Application.Current.Quit();
        }
    }
}