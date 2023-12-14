namespace tic_tac_toe;

public partial class RulesPage : ContentPage
{
    //Constructor for RulesPage
	public RulesPage()
	{
		InitializeComponent();
	}

    //Event handler for when the NEXT button is clicked, navigates to PlayerManager page
    private async void StartChoosePlayer_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PlayerManager());
    }

    //Method for when user finds the cheat codes button and clicks it
    private int LoadSpecialMoveCountFromTextFile()
    {
        // Variable to store the loaded move count
        int loadedMoveCount = 0;

        try
        {
            // Get the path to the file in the project directory
            // Check if the file exists, and if so, read the data from it. If not, return 0
            // If the file doesn't exist, its creation will be handled in the SaveSpecialMoveCountToTextFile method on GamePage
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "specialMoveCounter_Saved.txt");

            if (File.Exists(filePath))
            {
                string textData = File.ReadAllText(filePath);
                if (int.TryParse(textData, out int amountCheatCount))
                {
                    loadedMoveCount = amountCheatCount;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading special move count: {ex.Message}");
        }

        return loadedMoveCount;
    }

    //Method for when user finds the cheat codes button and clicks it
    private async void OnImageButtonClicked(object sender, EventArgs e)
    {
        // Load the special move count from the text file and display it in the alert
        int? specialMoveCount = LoadSpecialMoveCountFromTextFile();
        string message;

        if (specialMoveCount.HasValue)
        {
            message = $"Cheats been used {specialMoveCount} times.";
        }
        else
        {
            message = "Error loading special move count. Please try again later.";
        }

        // Display cheat codes using DisplayAlert
        await DisplayAlert("Special Cheat Codes",
                           "Enter the code in the small entry box when you feel hopeless. Use them wisely!\n\n" +
                           $"Special Cheat Code for Player 1 (X): S.X\n" +
                           $"Special Cheat Code for Player 2 (O): S.O\n\n" +
                           $"{message}",
                           "OK, I promise not to tell anyone!");
    }


}