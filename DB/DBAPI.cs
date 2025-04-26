using MySql.Data.MySqlClient;
using DBEssentials;

public class DBApi {
    private String DBName = "", MSQLConnectionString = "";
    private Boolean DEBUG = true;
    public DBApi (String DBName, String Password) {
        this.DBName = DBName;
        MSQLConnectionString = $"server=localhost;database={DBName};user=root;password={Password}";
	        
        // open the connection
        using (var connection = new MySqlConnection(MSQLConnectionString)) {
            connection.Open();
            bool result = false; // doesn't exist
            var existenceCommand =
            @$"
                    SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name ='{DBName}';
            ";
            if (connection.State == System.Data.ConnectionState.Open) {
                MySqlCommand innerCommand = new MySqlCommand(existenceCommand, connection);
                var res = innerCommand.ExecuteScalar();
                result = Convert.ToInt32(res) > 0;
            }
            else {
                throw new System.ArgumentException("Data.ConnectionState must be open");
            }
            if (result) {
                if (DEBUG) Console.WriteLine($"RESULT: THE {DBName} DATABASE AND ITS TABLES ALREADY EXIST");
            } else {
                if (DEBUG) Console.WriteLine($"IN PROCESS: THE {DBName} DATABASE AND ITS TABLES ARE BEING CREATED");
                var command = connection.CreateCommand();
                command.CommandText =
                @$"
                    CREATE TABLE {DBName} (
                        ChatID INTEGER NOT NULL,
                        Username TEXT NOT NULL,
                        Status INTEGER NOT NULL,
                        Role INTEGER NOT NULL,
                        GroupChatID INTEGER
                    );
                ";
                command.ExecuteNonQuery();
                if (DEBUG) Console.WriteLine($"RESULT: THE {DBName} DATABASE AND ITS TABLES HAVE BEEN CREATED");
            }
        }
    }

    public DBApi () { }

    // username == "NONE": no such user
    public Users findByField (UsersFieldsDB fieldName, String value) {
        Users result = new Users(0, "NONE", Statuses.NONE, Roles.NONE, 0);
        if (DEBUG) Console.WriteLine($"\t{DBName} DataBase:\t\"IN PROCESS: in search for the {DBName} by {fieldName.ToString()}: {value}\"");
        
        // open the connection
        using (var connection = new MySqlConnection(MSQLConnectionString)) {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @$"SELECT * FROM {DBName} WHERE {fieldName.ToString()} = '{value}'";
            // there is only one user in the db so it's not gonna be a problem
            
            using (var reader = command.ExecuteReader()) {
                if (!reader.HasRows) // Literally no user with such username
                    return result;

                while (reader.Read()) {
                    long ChatID = reader.GetInt64(0);
                    String Username = reader.GetString(1);
                    Statuses Status = (Statuses)reader.GetInt32(2);
                    Roles Role = (Roles)reader.GetInt32(3);
                    long GroupChatID = reader.GetInt64(4);
                    if (Username == value) {
                        result = new Users(ChatID, Username, Status, Role, GroupChatID);
                        if (DEBUG) Console.WriteLine($"\t{DBName} DataBase:\t\"RESULT: the user {value} has been found\"");
                        break; // we can break off early, because there is only one user in the db and we need the first encounter
                    }
                }
            }
        }
        if (!result.isValid() && DEBUG)
            Console.WriteLine($"\t{DBName} DataBase:\t\"RESULT: the user {value} has not been found\"");
        return result;
    }

    // true: an error occurred; false: deleted successfully
    public bool Add (Users user) {
        if (DEBUG) Console.WriteLine($"\t{DBName} DataBase:\t\"IN PROCESS: adding {user.getUsername()} to the DataBase\"");
        DEBUG = !DEBUG;
        if (findByField(UsersFieldsDB.Username, user.getUsername()).isValid()) {
            DEBUG = !DEBUG;
            if (DEBUG) Console.WriteLine($"\t{DBName} DataBase:\t\"RESULT: the user {user.getUsername()} is already in the DataBase\"");
                return true; // there is already such a user!
        }
        DEBUG = !DEBUG;
        bool errors = false; // no error yet
        
	    using (var connection = new MySqlConnection(MSQLConnectionString)) {
            var command = connection.CreateCommand();
            command.CommandText = 
            @$"
                INSERT INTO {DBName}
                VALUES ({user.getChatID()}, '{user.getUsername()}', {(int)user.getStatus()}, {(int)user.getRole()}, {user.getGroupChatID()});
            ";
            connection.Open();
            command.ExecuteNonQuery();
        }
	    
        if (!errors && DEBUG)
            Console.WriteLine($"\tDataBase\t\"RESULT: the user {user.getUsername()} has been added to the DataBase\"");
        return errors;
    }

    // true: an error occurred; false: deleted successfully
    // the username cannot be changed, so we just overwrite the other fields
    public bool Update(Users user) {
        if (DEBUG) Console.WriteLine($"\t{DBName} DataBase:\t\"IN PROCESS: updating the user {user.getUsername()}\"");
        Boolean errors = false; // no errors yet
	    // open the connection
        using (var connection = new MySqlConnection(MSQLConnectionString)) {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"UPDATE {DBName} SET Status = {(int)user.getStatus()}, Role = {(int)user.getRole()}, GroupChatID = {user.getGroupChatID()} WHERE ChatID = {user.getChatID()}";
            command.ExecuteNonQuery();
        }
        if (DEBUG && !errors) Console.WriteLine($"\t{DBName} DataBase\t\"RESULT: the {DBName} {user.getUsername()} has been updated successfully\"");
        return errors;
    }

    // true: an error occurred; false: deleted successfully
    public bool Delete(Users user) {
        return true;
    }
}