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
    public enum statuses {
        NONE,           // The user who has met the bot for the first time. They are welcomed and are suggested to register.
        newcomer,       // Pre-registration status
        inregprocMinister,  // inregproc: in registration process
        inregprocCustomer,
        inQueue,            // a participant who waits to be assigned to a small group
        inLearningProc,     // a participant who is assigned to a small group. When small group is terminated the customer gets the 'none' role.
        done                // if the customer has the 'done' status they will be asked if they want to terminate the service
    }
    public enum roles {
        NONE,           // no role: newcomer
        representative, // ministers
        mms,            // ministers
        host,           // ministers
        customer,       // once assigned this role the customer automatically gets the 'inQueue' status
    }
    public class fillOutForm {
        proficiencyLevels languageProficiency;
        public fillOutForm() {}
        public proficiencyLevels getLanguageProficiencyLevel() {
            return languageProficiency;
        }
        public void setLanguageProficiencyLevel(proficiencyLevels newLevel) {
            languageProficiency = newLevel;
        }
    }
    public class Users {
        long chatID, groupChatID;
        statuses status;
        roles role;
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
        public statuses getStatus() {
            return status;
        }
        public void setStatus(statuses newStatus) {
            status = newStatus;
        }
        public roles getRole() {
            return role;
        }
        public void setRole(roles newRole) {
            role = newRole;
        }
        public Users(long chatID, String username, statuses status, roles role, long groupChatID) {
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
            Users result = new Users(0, "NONE", statuses.NONE, roles.NONE, 0);
            if (DEBUG) Console.WriteLine($"IN PROCESS: IN SEARCH FOR THE USER {username}");
	    // TOBEDELETED
	    foreach (var user in list) {
		if (user.getUsername() == username) {
                    if (DEBUG) Console.WriteLine($"RESULT: THE USER {username} HAS BEEN FOUND");
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
                        statuses Status = (statuses)reader.GetInt32(2);
                        roles Role = (roles)reader.GetInt32(3);
                        long GroupChatID = reader.GetInt64(4);
                        // proficiencyLevels LanguageProficiency = (proficiencyLevels)reader.GetInt32(5);
                        if (Username == username) {
                            result = new Users(ChatID, Username, Status, Role, GroupChatID, LanguageProficiency);
                            if (DEBUG) Console.WriteLine($"RESULT: THE USER {username} HAS BEEN FOUND");
                            break;
                        }
                    }
                }
            }
	    */
            if (!result.isValid() && DEBUG)
                Console.WriteLine($"RESULT: THE USER {username} HAS NOT BEEN FOUND");
            return result;
        }
        public bool Add(Users user) {
            if (DEBUG) Console.WriteLine($"IN PROCESS: ADDING THE USER {user.getUsername()} TO THE DB");
            DEBUG = !DEBUG;
            if (findByUsername(user.getUsername()).isValid()) {
                DEBUG = !DEBUG;
                if (DEBUG) Console.WriteLine($"RESULT: THE USER {user.getUsername()} IS ALREADY IN THE DB");
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
                Console.WriteLine($"RESULT: THE USER {user.getUsername()} HAS BEEN ADDED TO THE DB");
            return result;
        }
        // the username cannot be changed, so we just overwrite the other fields
        public bool Update(Users user) {
            if (DEBUG) Console.WriteLine($"IN PROCESS: UPDATING THE USER {user.getUsername()}");
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
            if (DEBUG && !result) Console.WriteLine($"RESULT: THE USER {user.getUsername()} HAS BEEN UPDATED SUCCESSFULLY");
            return result;
        }
    }
}
