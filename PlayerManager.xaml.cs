
namespace tic_tac_toe
{    
    //PlayerManager handles player selection and creation of new players
    public partial class PlayerManager : ContentPage
    {
        private List<MainPage.Player> PickerInfo { get; set; } //List to store the player information for the picker
        private MainPage.Player player1;
        private MainPage.Player player2;
        private bool isComputerSelected = false; //Boolean to check if computer player is selected

        //Constructor for PlayerManager
        public PlayerManager()
        {
            InitializeComponent();

            PickerInfo = new List<MainPage.Player>(); //Initialize PickerInfo list
            LoadPlayersFromJson(); //Load player data from JSON file

            //Set the item source for the pickers to the PickerInfo list
            playerOnePicker.ItemsSource = PickerInfo;
            playerTwoPicker.ItemsSource = PickerInfo;

            //Handle the event when a player is selected from the picker
            playerOnePicker.SelectedIndexChanged += (sender, args) =>
            {
                //If a player is selected, set the text fields to the player's information
                if (playerOnePicker.SelectedItem is MainPage.Player selectedPlayer)
                {
                    //Update the text fields with the selected player's information
                    FirstNameEntry_P1.Text = selectedPlayer.FirstName;
                    LastNameEntry_P1.Text = selectedPlayer.LastName;
                    BirthYearEntry_P1.Text = selectedPlayer.BirthYear.ToString();
                }
            };

            playerTwoPicker.SelectedIndexChanged += (sender, args) =>
            {
                if (playerTwoPicker.SelectedItem is MainPage.Player selectedPlayer)
                {
                    FirstNameEntry_P2.Text = selectedPlayer.FirstName;
                    LastNameEntry_P2.Text = selectedPlayer.LastName;
                    BirthYearEntry_P2.Text = selectedPlayer.BirthYear.ToString();
                }
            };

            BindingContext = this; //Set the binding context to this page for the XAML elements in this page
        }

        //Load players from JSON file and add them to the PickerInfo list
        private void LoadPlayersFromJson()
        {
            List<MainPage.Player> players = MainPage.PlayerInfoSerializer.LoadPlayers();
            PickerInfo.AddRange(players);
        }

        //Handler for when the imagebutton ComputerPlayerSelection_Button is clicked.
        //Changes the background color of the button and disables the text fields for player 2
        //If the button is clicked again, the background color is set back to null and the text fields are enabled
        //Sets the player2 to be the computer player
        private void ComputerPlayerSelected_Clicked(object sender, EventArgs e)
        {
            isComputerSelected = !isComputerSelected;

            if (isComputerSelected)
            {
                //Color the button when computer player is selected and disable the text fields for player 2
                ComputerPlayerSelection_Button.BackgroundColor = Color.FromArgb("#0026be");
                playerTwoPicker.IsEnabled = false;
                playerTwoPicker.SelectedItem = null;
                FirstNameEntry_P2.IsEnabled = false;
                FirstNameEntry_P2.Text = "";
                LastNameEntry_P2.IsEnabled = false;
                LastNameEntry_P2.Text = "";
                BirthYearEntry_P2.IsEnabled = false;
                BirthYearEntry_P2.Text = "";

                //Check if the computer player is already in the list
                MainPage.Player computerPlayer = PickerInfo.FirstOrDefault(player => player.FirstName == "Computer");

                //If the computer player is not in the list, create a new computer player and add it to the list
                if (computerPlayer == null)
                {
                    computerPlayer = new MainPage.Player
                    {
                        FirstName = "Computer",
                        LastName = "",
                        BirthYear = 1945 // First programmable computer was invented in 1945
                    };
                    PickerInfo.Insert(0, computerPlayer); // Insert computer player at the beginning of the list
                }

                player2 = computerPlayer; // Update player2 variable with the computer player
            }
            //If the button is clicked again, set the background color back to null and enable the text fields for player 2
            else
            {
                ComputerPlayerSelection_Button.BackgroundColor = null;
                playerTwoPicker.IsEnabled = true;
                FirstNameEntry_P2.IsEnabled = true;
                LastNameEntry_P2.IsEnabled = true;
                BirthYearEntry_P2.IsEnabled = true;
            }
        }

        //Handler when the Continue button is clicked
        //Checks if the player information is valid, creates the player objects, and navigates to the GamePage
        //If the player information is not valid, display an alert
        //Checs for invalid input such as empty text fields, invalid characters, and invalid birth year
        //Also checks if the player 1 and player 2 information are the same and if the player 1 or player 2 first name is "Computer"
        private async void GamePageButton_Clicked(object sender, EventArgs e)
        {
            //Get the player 1 information from the entry fields
            string player1FirstName = FirstNameEntry_P1.Text;
            string player1LastName = LastNameEntry_P1.Text;
            string player1BirthYearText = BirthYearEntry_P1.Text;

            //Check if the player 1 information is valid
            if (!IsValidName(player1FirstName) || !IsValidName(player1LastName) || !IsValidBirthYear(player1BirthYearText))
            {
                await DisplayAlert("Invalid Input", "Player 1: Please enter only letters for name and a 4-digit birth year.", "OK");
                return;
            }

            // Check if player 1 and player 2 entries are the same
            if (FirstNameEntry_P1.Text.Equals(FirstNameEntry_P2.Text, StringComparison.OrdinalIgnoreCase) &&
                LastNameEntry_P1.Text.Equals(LastNameEntry_P2.Text, StringComparison.OrdinalIgnoreCase) &&
                BirthYearEntry_P1.Text.Equals(BirthYearEntry_P2.Text, StringComparison.OrdinalIgnoreCase))
            {
                await DisplayAlert("Invalid Input", "Player 1 and Player 2 cannot have all same info.", "OK");
                return;
            }

            // Check if either player tries to enter "Computer" as their first name
            if (FirstNameEntry_P1.Text.Equals("Computer", StringComparison.OrdinalIgnoreCase) ||
                FirstNameEntry_P2.Text.Equals("Computer", StringComparison.OrdinalIgnoreCase))
            {
                await DisplayAlert("Invalid Input", "Player 1 and Player 2 cannot have the first name 'Computer'.", "OK");
                return;
            }

            int player1BirthYear = int.Parse(player1BirthYearText);

            //Create new player 1 object with the information
            player1 = new MainPage.Player
            {
                FirstName = player1FirstName,
                LastName = player1LastName,
                BirthYear = player1BirthYear
            };

            //If the computer player is selected, set player 2 to be the computer player
            if (isComputerSelected)
            {
                player2 = PickerInfo.FirstOrDefault(player => player.FirstName == "Computer"); //Find the player with the first name "Computer"
            }

            //If the computer player is not selected, get the player 2 information from the entry fields
            //Check if the player 2 information is valid and create the player 2 object
            else
            {
                string player2FirstName = FirstNameEntry_P2.Text;
                string player2LastName = LastNameEntry_P2.Text;
                string player2BirthYearText = BirthYearEntry_P2.Text;

                if (string.IsNullOrWhiteSpace(player2FirstName) || string.IsNullOrWhiteSpace(player2LastName) || string.IsNullOrWhiteSpace(player2BirthYearText))
                {
                    await DisplayAlert("Invalid Input", "Player 2 must be selected or created.", "OK");
                    return;
                }

                if (!IsValidName(player2FirstName) || !IsValidName(player2LastName) || !IsValidBirthYear(player2BirthYearText))
                {
                    await DisplayAlert("Invalid Input", "Player 2: Please enter valid letters for name and a 4-digit birth year.", "OK");
                    return;
                }

                int player2BirthYear = int.Parse(player2BirthYearText);
                player2 = new MainPage.Player
                {
                    FirstName = player2FirstName,
                    LastName = player2LastName,
                    BirthYear = player2BirthYear
                };
            }

            //Display the selected players in an alert and navigate to the GamePage
            await DisplayAlert("Players Selected", $"Player 1: {player1.FirstName} {player1.LastName}\nPlayer 2: {player2.FirstName} {player2.LastName}", "OK");

            //Navigate to the GamePage with the player objects
            await Navigation.PushAsync(new GamePage(player1, player2));
        }

        //Checks if the name is not null, not empty, does not contain spaces, and only contains letters
        private bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && !name.Contains(" ") && name.All(char.IsLetter);
        }

        //Checks if the birth year is a 4-digit number
        private bool IsValidBirthYear(string birthYear)
        {
            int year;
            return int.TryParse(birthYear, out year) && birthYear.Length == 4;
        }
    }
}