using Microsoft.Data.Sqlite;
using DBEssentials;

public class DBApi {
    private String DBName = "";
    private Boolean DEBUG = true;
    public DBApi (String DBName) {
        this.DBName = DBName;
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
    }

    public DBApi () { }

    public Users findByUsername (String username) {
        Users result = new Users(0, "NONE", Statuses.NONE, Roles.NONE, 0);
        if (DEBUG) Console.WriteLine($"\tDataBase:\t\"IN PROCESS: in search for the user {username}\"");
        
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
                        result = new Users(ChatID, Username, Status, Role, GroupChatID);
                        if (DEBUG) Console.WriteLine($"\tDataBase:\t\"RESULT: the user {username} has been found\"");
                        break;
                    }
                }
            }
        }
        if (!result.isValid() && DEBUG)
            Console.WriteLine($"\tDataBase:\t\"RESULT: the user {username} has not been found\"");
        return result;
    }

    public bool Add (Users user) {
        if (DEBUG) Console.WriteLine($"\tDataBase:\t\"IN PROCESS: adding {user.getUsername()} to the DataBase\"");
        DEBUG = !DEBUG;
        if (findByUsername(user.getUsername()).isValid()) {
            DEBUG = !DEBUG;
            if (DEBUG) Console.WriteLine($"\tDataBase:\t\"RESULT: the user {user.getUsername()} is already in the DataBase\"");
            return true; // there is already such a user!
        }
        DEBUG = !DEBUG;
        bool result = false; // no error yet
        
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
	    
        if (!result && DEBUG)
            Console.WriteLine($"\tDataBase\t\"RESULT: the user {user.getUsername()} has been added to the DataBase\"");
        return result;
    }
    // the username cannot be changed, so we just overwrite the other fields
    public bool Update (Users user) {
        if (DEBUG) Console.WriteLine($"\tDataBase:\t\"IN PROCESS: updating the user {user.getUsername()}\"");
        Boolean result = false; // no errors yet
        
	    // open the connection
        using (var connection = new SqliteConnection("Data Source=" + DBName + ".db")) {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"UPDATE Users SET status = {(int)user.getStatus()}, role = {(int)user.getRole()}, groupchatid = {user.getGroupChatID()} WHERE chatid = {user.getChatID()}";
            connection.Open();
            command.ExecuteNonQuery();
        }
	    
        if (DEBUG && !result) Console.WriteLine($"\tDataBase\t\"RESULT: the user {user.getUsername()} has been updated successfully\"");
        return result;
    }
}