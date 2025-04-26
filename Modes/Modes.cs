using Telegram.Bot.Types;

// The 'Hub' for the modes. Each mode is now moved to a separate file for redacting purposes
public class Modes {
    public Modes() {}

    // gives the menu: gain more info or register
    public void FirstEncounter(
        Update update,
        String usernameTemp, String msgTextTemp, long chatIdTemp,
        Commands commands, DBApi DB
    ) {
        firstEncounter mode = new firstEncounter();
        mode.StartService(update, usernameTemp, msgTextTemp, chatIdTemp, commands, DB);
    }

    // Pre-registration mode. Used when the user has a role 'newcomer.'
    public void RegistrationChoiceMode(
        Update update,
        String usernameTemp, String msgTextTemp, long chatIdTemp,
        Commands commands, DBApi DB
    ) {
		RegistrationChoiceService mode = new RegistrationChoiceService();
		mode.StartService(update, usernameTemp, msgTextTemp, chatIdTemp, commands, DB);
    }
    public void RegistrationCustomer(
        Update update,
        String usernameTemp, String msgTextTemp, long chatIdTemp,
        Commands commands, DBApi DB
    ) {
		RegistrationProcessService mode = new RegistrationProcessService();
		mode.StartServiceCustomer(update, usernameTemp, msgTextTemp, chatIdTemp, commands, DB);
	}
	//
}
