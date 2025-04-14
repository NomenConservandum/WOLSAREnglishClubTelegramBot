using Microsoft.Data.Sqlite;

namespace DBController { // Proper DataBase Class
	public enum proficiencyLevels {
        zero,
        A1,
        A2,
        B1,
        B2,
        C1
    }
    public enum Statuses {
        NONE,           // The user who has met the bot for the first time. They are welcomed and are suggested to register.
        newcomer,       // Pre-registration status
        inregprocMinister,  // inregproc: in registration process
        inregprocCustomer,
        inQueue,            // a participant who waits to be assigned to a small group
        inLearningProc,     // a participant who is assigned to a small group. When small group is terminated the customer gets the 'none' role.
        done                // if the customer has the 'done' status they will be asked if they want to terminate the service
    }
    public enum Roles {
        NONE,           // no role: newcomer
        representative, // ministers
        mms,            // ministers
        host,           // ministers
        customer,       // once assigned this role the customer automatically gets the 'inQueue' status
    }
    public enum Genders { // based
      Male,
      Female
    }
    public enum Formats {
      Offline,
      Online
    }
	public enum Days {
		Monday,
		Tuesday,
		Wednesday,
		Thursday,
		Friday,
		Saturday,
        Sunday
	}
    public enum DaysRussian {
		Понедельник,
		Вторник,
		Среда,
		Четверг,
		Пятница,
		Суббота,
		Воскресенье
	}
    // The fill-out form Class for the participants
    public class FillOutFormParticipants {
        public Genders sex = Genders.Male;
        public int age = 0, frequency = 0; // frequency: how many times a week
        public Formats format = Formats.Offline;
        public proficiencyLevels languageProficiency = proficiencyLevels.A1;
        public String conductor = ""; // place or the messenger
        public String time = ""; // the format: "Monday:hourMask,Tuesday:hourMask,...,Sunday:hourMask"
        public int duration = 60; // in minutes
        public Boolean notifications = true;
        public long interestsMask = 0; // a bit mask of interests: board games, video games, books reading, BSFN in English, food, lectures, tests, other options
        public String otherInterests = "";
        public FillOutFormParticipants() {}
    }
    public class Users {
        long chatID, groupChatID;
        Statuses status;
        Roles role;
        String username;
        public long getChatID() {
            return chatID;
        }
        public String getUsername() {
            return username;
        }
        public long getGroupChatID() {
            return groupChatID;
        }
        public void setGroupChatID(int newID) {
            groupChatID = newID;
        }
        public Statuses getStatus() {
            return status;
        }
        public void setStatus(Statuses newStatus) {
            status = newStatus;
        }
        public Roles getRole() {
            return role;
        }
        public void setRole(Roles newRole) {
            role = newRole;
        }
        public Users(long chatID, String username, Statuses status, Roles role, long groupChatID) {
            this.chatID = chatID;
            this.username = username;
            this.status = status;
            this.role = role;
            this.groupChatID = groupChatID;
        }
        // Checks if the user is valid. Not valid user means the user was not found.
        public bool isValid() {
            if (chatID == 0 && username == "NONE")
                return false;
            return true;
        }
    }
    public class DBApi {
        private String DBName = "";
        private Boolean DEBUG = true;
	      List<Users> list = new List<Users>() {}; // TOBEDELETED
        public DBApi(String DBName) {
            this.DBName = DBName;
	    // while I'm on termux I cannot use a proper DB. TOBEDELETED
            /*
	    // open the connection
            using (var connection = new SqliteConnection($"Data Source={DBName}.db")) {
                connection.Open();
                bool result = false; // doesn't exist
                var existanceCommand =
                @$"
                    SELECT name FROM sqlite_master WHERE type='table' AND name='Users';
                ";
                if(connection.State == System.Data.ConnectionState.Open) {   
                    SqliteCommand innerCommand = new SqliteCommand(existanceCommand, connection);
                    SqliteDataReader reader = innerCommand.ExecuteReader();
                    if (reader.HasRows) {
                        reader.Close();
                        result = true; // exists
                    }
                    reader.Close(); // @al000y: thanks mate
                } else {
                    throw new System.ArgumentException("Data.ConnectionState must be open");
                }
                if (result) {
                    if (DEBUG) Console.WriteLine($"RESULT: THE DATABASE AND ITS TABLES ALREADY EXIST");
                } else {
                    if (DEBUG) Console.WriteLine($"IN PROCESS: THE DATABASE AND ITS TABLES ARE BEING CREATED");
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    CREATE TABLE Users (
                        chatid INTEGER NOT NULL,
                        username TEXT NOT NULL,
                        status INTEGER NOT NULL,
                        role INTEGER NOT NULL,
                        groupchatid INTEGER,
                    );
                ";
                command.ExecuteNonQuery();
                if (DEBUG) Console.WriteLine($"RESULT: THE DATABASE AND ITS TABLES HAVE BEEN CREATED");
                }
            }
	    */
        }
	
        public DBApi() {}

        public Users findByUsername(String username) {
            Users result = new Users(0, "NONE", Statuses.NONE, Roles.NONE, 0);
            if (DEBUG) Console.WriteLine($"\tDataBase:\t\"IN PROCESS: in search for the user {username}\"");
	          // TOBEDELETED
	          foreach (var user in list) {
		          if (user.getUsername() == username) {
                if (DEBUG) Console.WriteLine($"\tDataBase:\t\"RESULT: the user {username} has been found\"");
		              return user;
	              }
	          }
	    /*
            // open the connection
            using (var connection = new SqliteConnection("Data Source=" + DBName + ".db")) {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM Users";
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        long ChatID = reader.GetInt64(0);
                        String Username = reader.GetString(1);
                        Statuses Status = (Statuses)reader.GetInt32(2);
                        Roles Role = (Roles)reader.GetInt32(3);
                        long GroupChatID = reader.GetInt64(4);
                        // proficiencyLevels LanguageProficiency = (proficiencyLevels)reader.GetInt32(5);
                        if (Username == username) {
                            result = new Users(ChatID, Username, Status, Role, GroupChatID, LanguageProficiency);
                            if (DEBUG) Console.WriteLine($"\tDataBase:\t"RESULT: the user {username} has been found\"");
                            break;
                        }
                    }
                }
            }
	    */
            if (!result.isValid() && DEBUG)
                Console.WriteLine($"\tDataBase:\t\"RESULT: the user {username} has not been found\"");
            return result;
        }
        
        public bool Add(Users user) {
            if (DEBUG) Console.WriteLine($"\tDataBase:\t\"IN PROCESS: adding {user.getUsername()} to the DataBase\"");
            DEBUG = !DEBUG;
            if (findByUsername(user.getUsername()).isValid()) {
                DEBUG = !DEBUG;
                if (DEBUG) Console.WriteLine($"\tDataBase:\t\"RESULT: the user {user.getUsername()} is already in the DataBase\"");
                return true; // there is already such a user!
            }
            DEBUG = !DEBUG;
            bool result = false; // no error yet
	          list.Add(user);
            // TOBEDELETED
	    /*
	          using (var connection = new SqliteConnection($"Data Source={DBName}.db")) {
                var command = connection.CreateCommand();
                command.CommandText = 
                @$"
                    INSERT INTO Users
                    VALUES ({user.getChatID()}, '{user.getUsername()}', {(int)user.getStatus()}, {(int)user.getRole()}, {user.getGroupChatID()});
                ";
                // {(int)form.getLanguageProficiencyLevel()}
                connection.Open();
                command.ExecuteNonQuery();
                // possibly here some errors may occur, if they do, we switch result to true.
            }
	    */
            if (!result && DEBUG)
                Console.WriteLine($"\tDataBase\t\"RESULT: the user {user.getUsername()} has been added to the DataBase\"");
            return result;
        }
        // the username cannot be changed, so we just overwrite the other fields
        public bool Update(Users user) {
            if (DEBUG) Console.WriteLine($"\tDataBase:\t\"IN PROCESS: updating the user {user.getUsername()}\"");
            Boolean result = false; // no errors yet
	          for (int i = 0; i < list.Count; ++i) {
		        if (list[i].getUsername() == user.getUsername())
		            list[i] = user;
	          }
	    // TOBEDELETED
	    /*
	    // open the connection
            using (var connection = new SqliteConnection("Data Source=" + DBName + ".db")) {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE Users SET status = {(int)user.getStatus()}, role = {(int)user.getRole()}, groupchatid = {user.getGroupChatID()}, languageProficiency = {(int)user.getLanguageProficiencyLevel()} WHERE chatid = {user.getChatID()}";
                connection.Open();
                command.ExecuteNonQuery();
            }
	    */
            if (DEBUG && !result) Console.WriteLine($"\tDataBase\t\"RESULT: the user {user.getUsername()} has been updated successfully\"");
            return result;
        }
    }
}
