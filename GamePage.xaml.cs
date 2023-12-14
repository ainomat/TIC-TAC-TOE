using static tic_tac_toe.MainPage;

namespace tic_tac_toe;

public partial class GamePage : ContentPage
{
    //Variables for gamepage to store player info, game state and grid
    private List<MainPage.Player> players = new List<MainPage.Player>();
    private bool isGameStarted = false; // is the game started
    private DateTime startTime; // start time of the game
    private bool isFirstPlayerTurn = true;
    private string PelaajaX = "cross.png";
    string PelaajaO = "circle.png";
    string currentPlayer = "cross.png";
    private MainPage.Player player1;
    private MainPage.Player player2;

    private string[,] GridButtons = new string[3, 3]
    {
        { "empty.png", "empty.png", "empty.png" },
        { "empty.png", "empty.png", "empty.png" },
        { "empty.png", "empty.png", "empty.png" }
    };

    //Constructor for GamePage, initializes the UI components and player data
    public GamePage(MainPage.Player player1, MainPage.Player player2)
    {
        InitializeComponent();
        this.player1 = player1; //this.player1 is the player1 object in GamePage
        this.player2 = player2;
        DisableGrid();
        players = PlayerInfoSerializer.LoadPlayers();

        //If player 2 is computer, image on the right side is computer, else it's player 2
        if (player2.FirstName == "Computer")
        {
            Player2TurnImage.Source = "computer_logo.png";
        }
        else
        {
            Player2TurnImage.Source = "player2_logo.png";
        }

        // Disable the secret entry by default, enable it when the game starts in Timer
        SecretEntry.IsEnabled = false;
        LoadSpecialMoveCountFromTextFile(); // Load the special move count from the text file
    }

    //When START GAME button is clicked, timer starts, grid is enabled, secret entry is enabled for use
    public void TimerAndGameBoardStart_Clicked(object sender, EventArgs e)
    {
        if (!isGameStarted)
        {
            isGameStarted = true;
            startTime = DateTime.Now; // start time of the game so that time can be calculated
            EnableGrid();

            SecretEntry.IsEnabled = true;
            // Handle the case where the user presses Enter after entering the correct secret code
            SecretEntry.Completed += SecretEntry_Completed;

            // Create a new timer, set the interval to 1 second, and start it
            var dispatcherTimer = Application.Current.Dispatcher.CreateTimer();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Tick += HandleGameTimer;
            dispatcherTimer.Start();
        }
    }

    //Handle game timer, update timer label, update players' total time played
    private void HandleGameTimer(object sender, EventArgs args)
    {
        if (isGameStarted) // Check if the game is still running
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;
            string formattedTime = $"{(int)elapsedTime.TotalHours:D2}:{elapsedTime.Minutes:D2}:{elapsedTime.Seconds:D2}";

            MainThread.BeginInvokeOnMainThread(() =>
            {
                TimerLabel.Text = formattedTime;

                // Update players' total time played
                player1.TotalTimePlayed = elapsedTime;
                player2.TotalTimePlayed = elapsedTime;
            });
        }
    }


    //Game over: check for win and tie conditions, set winner image, disable grid after game is over
    //Save player data
    //Return game over status (true or false)
    private bool isGameOver = false; //game over first set to false
    private bool GameOver()
    {
        // Check if the game is already over, and if so, return false
        if (isGameOver)
        {
            return false;
        }

        // Check for win or tie
        bool gameOver = CheckForWin() || CheckForTie();

        // If the game is over, set the winner image and disable the grid
        if (gameOver)
        {
            isGameOver = true; // Set the flag to prevent further calls to GameOver()

            //Set winner image based on the current player or tie
            if (CheckForWin())
            {
                SetWinnerImage(currentPlayer == PelaajaX ? "Player 1" : "Player 2");
            }
            else if (CheckForTie())
            {
                SetWinnerImage("tie");
            }

            WinnerImage.IsVisible = true;

            // Disable the grid after the game is over
            DisableGrid();

            // Save player data
            SavePlayerData();

            //isGameStarted = false; // Stop the timer
        }
        return gameOver; // Return true if the game is over, false if not
    }

    //Save player data to the players list, update existing player information or add new players
    //Save the updated players list to JSON, overwriting the existing file
    private void SavePlayerData()
    {
        // Find and update existing Player 1 data
        var existingPlayer1 = players.FirstOrDefault(player =>
            player.FirstName == player1.FirstName &&
            player.LastName == player1.LastName &&
            player.BirthYear == player1.BirthYear);

        // If Player 1 exists, update their data
        if (existingPlayer1 != null)
        {
            existingPlayer1.Wins += (CheckForWin() && currentPlayer == PelaajaX) ? 1 : 0;
            existingPlayer1.Losses += (CheckForWin() && currentPlayer == PelaajaO) ? 1 : 0;
            existingPlayer1.Draws += (CheckForTie()) ? 1 : 0;
            existingPlayer1.TotalTimePlayed += player1.TotalTimePlayed; // Update playtime
        }
        else
        {
            // If Player 1 doesn't exist, add them to the list
            players.Add(player1);
        }

        // Find and update existing Player 2 data
        var existingPlayer2 = players.FirstOrDefault(player =>
            player.FirstName == player2.FirstName &&
            player.LastName == player2.LastName &&
            player.BirthYear == player2.BirthYear);

        // If Player 2 exists, update their data
        if (existingPlayer2 != null)
        {
            existingPlayer2.Wins += (CheckForWin() && currentPlayer == PelaajaO) ? 1 : 0; 
            existingPlayer2.Losses += (CheckForWin() && currentPlayer == PelaajaX) ? 1 : 0;
            existingPlayer2.Draws += (CheckForTie()) ? 1 : 0;
            existingPlayer2.TotalTimePlayed += player2.TotalTimePlayed; // Update playtime
        }
        else
        {
            // If Player 2 doesn't exist, add them to the list
            players.Add(player2);
        }

        // Save the updated players list to JSON, overwriting the existing file
        PlayerInfoSerializer.SavePlayers(players);
    }


    //Enable and disable grid, used in timer and gameboard start button
    private void EnableGrid()
    {
        // Enable all ImageButtons in the grid
        foreach (var child in ticTacToeGrid.Children)
        {
            if (child is ImageButton button)
            {
                button.IsEnabled = true;
            }
        }
    }
    private void DisableGrid()
    {
        // Disable all ImageButtons in the grid
        foreach (var child in ticTacToeGrid.Children)
        {
            if (child is ImageButton button)
            {
                button.IsEnabled = false;
            }
        }
    }

    //Struct for special move count, used also in RulesPage.xaml.cs
    public struct SpecialMoveCount
    {
        public int SpecialMove1Count { get; set; }
    }

    private SpecialMoveCount specialMoveCount;

    //Handle secret entry
    //If secret code is correct: first empty the grid, then "S.X" = X to every corner, "S.O" = O to every corner
    //Disable the entry after a valid secret code is entered
    //Increment the SpecialMove1Count by 1
    private void SecretEntry_Completed(object sender, EventArgs e)
    {
        string secretCode = SecretEntry.Text;

        if (secretCode == "S.X")
        {
            ResetGrid(); // Reset all grid places to "empty.png"

            // Update the GridButtons array
            GridButtons[0, 0] = GridButtons[0, 2] = GridButtons[2, 0] = GridButtons[2, 2] = "cross.png";

            // Update the UI based on the GridButtons array
            UpdateGridUI();

            // Disable the entry after a valid secret code is entered
            SecretEntry.IsEnabled = false;

            // Increment the SpecialMove1Count property
            specialMoveCount.SpecialMove1Count++;

            // Save the incremented specialMoveCount to the text file
            SaveSpecialMoveCountToTextFile(specialMoveCount.SpecialMove1Count);
        }
        else if (secretCode == "S.O")
        {
            ResetGrid(); // Reset all grid places to "empty.png"

            // Update the GridButtons array
            GridButtons[0, 0] = GridButtons[0, 2] = GridButtons[2, 0] = GridButtons[2, 2] = "circle.png";

            // Update the UI based on the GridButtons array
            UpdateGridUI();

            // Disable the entry after a valid secret code is entered
            SecretEntry.IsEnabled = false;

            // Increment the SpecialMove1Count property
            specialMoveCount.SpecialMove1Count++;

            // Save the incremented specialMoveCount to the text file
            SaveSpecialMoveCountToTextFile(specialMoveCount.SpecialMove1Count);
        }
        else
        {
            // If user enters an invalid secret code, nothing happens, cheat can be tried again
            return;
        }

        SecretEntry.Text = ""; // Clear the entry after a valid secret code is entered
    }

    // Attempt to load the special move count from the text file
    // If the file doesn't exist, set the special move count to 0
    private void LoadSpecialMoveCountFromTextFile()
    {
        try
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "specialMoveCounter_Saved.txt");

            if (File.Exists(filePath))
            {
                string textData = File.ReadAllText(filePath);
                if (int.TryParse(textData, out int amountCheatCount))
                {
                    specialMoveCount.SpecialMove1Count = amountCheatCount;
                }
            }
            else
            {
                specialMoveCount.SpecialMove1Count = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading special move count: {ex.Message}");
        }
    }

    // Save the special move count to the text file
    // If the file doesn't exist, create it
    private void SaveSpecialMoveCountToTextFile(int count)
    {
        // Convert the special move count to a string for saving
        string textData = $"SpecialMove1Count: {specialMoveCount.SpecialMove1Count}";

        // Attempt to save the special move count to the text file, if doesn't exist, create it
        try
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "specialMoveCounter_Saved.txt");
            File.WriteAllText(filePath, count.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    // Reset all grid places to "empty.png" before making the special move after the secret code is entered
    // Used in SecretEntry_Completed
    private void ResetGrid()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                GridButtons[i, j] = "empty.png";
            }
        }
    }

    // UpdateGridUI method updates the user interface based on the GridButtons array.
    // It iterates through the gridButtons array and sets the source of corresponding ImageButtons
    // to reflect the current state of the game board.
    private void UpdateGridUI()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                ImageButton button = GetGridButtonFromIndices(i, j);
                if (button != null)
                {
                    //Update the visual grid with empty.png, cross.png or circle.png based on GridButtons array positions
                    button.Source = GridButtons[i, j];
                }
            }
        }
    }

    // Handles the event when a grid button is clicked.
    // Checks for win and tie conditions. Increments wins, losses, and ties accordingly.
    // Updates player borders to show current players turn, toggles current player and their mark.
    // Disables grid buttons during AI's turn, then enables them after AI's move.
    private async void GridClicked(object sender, EventArgs e)
    {
        try
        {
            ImageButton GridClicked = (ImageButton)sender; //Get the grid button that was clicked
            int row = Grid.GetRow(GridClicked); //Get the row of clicked button
            int column = Grid.GetColumn(GridClicked); //Get the column of clicked button

            if (GridButtons[row, column] == "empty.png")
            {
                GridClicked.Source = currentPlayer; // Update grid with the current player's mark
                GridButtons[row, column] = currentPlayer; // Update the source of the clicked button in the GridButtons array

                bool isWin = CheckForWin();
                if (isWin)
                {
                    if (currentPlayer == PelaajaX)
                    {
                        player1.Wins++; //increment player 1 wins to the player 1 object
                        player2.Losses++; //increment player 2 losses to the player 2 object
                    }
                    else if (currentPlayer == PelaajaO)
                    {
                        player2.Wins++; //increment player 2 wins to the player 2 object
                        player1.Losses++; //increment player 1 losses to the player 1 object
                    }

                    // if game is won, it's Game Over, disable grid, set winner image, save player data, stop timer
                    isGameStarted = false;

                    GameOver();
                    return;
                }

                // Check for tie
                bool isTie = CheckForTie();
                if (isTie)
                {
                    player1.Draws++; //increment player 1 ties to the player 1 object
                    player2.Draws++; //increment player 2 ties to the player 2 object
                    isGameStarted = false;

                    GameOver();
                    return;
                }

                UpdatePlayerBorder(); // Update the player frames to indicate whose turn it is
                // Toggle the current player and their mark, player 1 is always X and player 2 is always O, player 1 starts
                isFirstPlayerTurn = !isFirstPlayerTurn;
                currentPlayer = isFirstPlayerTurn ? PelaajaX : PelaajaO; // Toggle the playmark, ? = if true, : = if false

            }

            // Disable grid buttons during AI's turn, enable them after AI's move
            if (player2.FirstName == "Computer" && currentPlayer == PelaajaO)
            {
                DisableGridButtons();
                await ComputerAIMove();
                EnableGridButtons();
            }
        }
        //If exception, display error message and go back to mainpage
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            await DisplayAlert("ERROR", "The gameboard doesn't seem to be working properly.\nPress OK to continue back to MainPage", "OK");
            await Navigation.PopAsync(); // Go back to MainPage
        }
    }

    // Disable all ImageButtons in the grid, used with AI
    private void DisableGridButtons()
    {
        foreach (var child in ticTacToeGrid.Children)
        {
            if (child is ImageButton button)
            {
                button.IsEnabled = false;
            }
        }
    }

    // Enable all ImageButtons in the grid, used with AI
    private void EnableGridButtons()
    {
        foreach (var child in ticTacToeGrid.Children)
        {
            if (child is ImageButton button)
            {
                button.IsEnabled = true;
            }
        }
    }

    // AI move: get empty positions, delay between 0.5s to 2s, randomize the position, place the mark, update UI in visual grid
    private async Task ComputerAIMove()
    {
        // Get empty positions by iterating through the GridButtons array
        List<int> emptyPositions = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (GridButtons[i, j] == "empty.png") //position is empty if the image there is "empty.png"
                {
                    emptyPositions.Add(i * 3 + j);
                }
            }
        }

        // If there empty positions, delay move between 0.5s to 2s, randomize the position, place the mark, update UI in visual grid
        if (emptyPositions.Count > 0)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(500, 2000))); // Delay between 0.5s to 2s
            Random random = new Random();
            int randomIndex = random.Next(0, emptyPositions.Count); // Randomize the position
            int position = emptyPositions[randomIndex];
            int row = position / 3;
            int column = position % 3;

            // Ensure the correct mark is placed for the AI's move
            GridButtons[row, column] = PelaajaO;

            // Update the UI based on the GridButtons array
            ImageButton button = GetGridButtonFromIndices(row, column);
            if (button != null)
            {
                button.Source = GridButtons[row, column];
            }

            // Check for win or tie if player 2 is computer
            if (player2.FirstName == "Computer" && CheckForWin())
                //if player 2 is computer and check for win is true
            {
                player2.Wins++; // increment player 2 wins to the player 2 object
                player1.Losses++; // increment player 1 losses to the player 1 object
                isGameStarted = false;

                GameOver();
                return;
            }

            CheckForTie();
            UpdatePlayerBorder();
            currentPlayer = (currentPlayer == PelaajaX) ? PelaajaO : PelaajaX;
            isFirstPlayerTurn = !isFirstPlayerTurn;
        }
    }



    //Get the ImageButton from the grid indices for use in computer AI move
    private ImageButton GetGridButtonFromIndices(int row, int column)
    {
        // Get the ImageButton from the calculated position in the grid
        if (ticTacToeGrid.Children[row * 3 + column] is ImageButton button)
        {
            return button;
        }
        else
        {
            return null;
        }
    }

    //Checks for win by checking rows, columns and diagonals for either player
    private bool CheckForWin()
    {
        // Check rows
        for (int i = 0; i < 3; i++)
        {
            if (GridButtons[i, 0] == PelaajaX && GridButtons[i, 1] == PelaajaX && GridButtons[i, 2] == PelaajaX)
            {
                return true;
            }
            if (GridButtons[i, 0] == PelaajaO && GridButtons[i, 1] == PelaajaO && GridButtons[i, 2] == PelaajaO)
            {
                return true;
            }
        }

        // Check columns
        for (int i = 0; i < 3; i++)
        {
            if (GridButtons[0, i] == PelaajaX && GridButtons[1, i] == PelaajaX && GridButtons[2, i] == PelaajaX)
            {
                return true;
            }
            if (GridButtons[0, i] == PelaajaO && GridButtons[1, i] == PelaajaO && GridButtons[2, i] == PelaajaO)
            {
                return true;
            }
        }

        // Check diagonals
        if (GridButtons[0, 0] == PelaajaX && GridButtons[1, 1] == PelaajaX && GridButtons[2, 2] == PelaajaX)
        {
            return true;
        }
        if (GridButtons[0, 0] == PelaajaO && GridButtons[1, 1] == PelaajaO && GridButtons[2, 2] == PelaajaO)
        {
            return true;
        }
        if (GridButtons[0, 2] == PelaajaX && GridButtons[1, 1] == PelaajaX && GridButtons[2, 0] == PelaajaX)
        {
            return true;
        }
        if (GridButtons[0, 2] == PelaajaO && GridButtons[1, 1] == PelaajaO && GridButtons[2, 0] == PelaajaO)
        {
            return true;
        }

        return false;
    }


    //Check for tie by checking if there are any empty positions left, if not, it's a tie
    private bool CheckForTie()
    {
        //If not a win, check for tie
        if (!CheckForWin())
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (GridButtons[i, j] == "empty.png")
                    {
                        return false;
                    }
                }
            }
            // If no empty positions are found, it's a tie
            return true;
        }
        // If there is a win, it's not a tie
        return false;
    }


    //Set winner or tie image based on the winning/tied players, used in GameOver()
    private void SetWinnerImage(string winner)
    {
        if (winner == "Player 1")
        {
            WinnerImage.Source = "player_one_victory.png";
        }
        else if (winner == "Player 2")
        {
            WinnerImage.Source = "player_two_victory.png";
        }
        else if (winner == "tie")
        {
            WinnerImage.Source = "tie_notification.png";
        }
    }

    //Updates the player image borders to indicate whose turn it is, highlight the current player, clear the other player
    private void UpdatePlayerBorder()
    {
        Player1Frame.Stroke = null;
        Player2Frame.Stroke = null;

        if (isFirstPlayerTurn)
        {
            Player2Frame.Stroke = Color.FromArgb("#0026be");
            Player1Frame.Stroke = Color.FromArgb("#00000000");
        }
        else if (!isFirstPlayerTurn)
        {
            Player1Frame.Stroke = Color.FromArgb("#0026be");
            Player2Frame.Stroke = Color.FromArgb("#00000000");
        }
    }

    //When mainpage is loaded again, the listview of scores is also updated
    private void BackToMainpage_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new MainPage());
    }

}