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
                Variables sensitive = new Variables();
                command.CommandText = 
                @$"
                    CREATE TABLE {DBName} (
                        {sensitive.DBNameToFields(DBName)}
                    );
                ";
                command.ExecuteNonQuery();
                if (DEBUG) Console.WriteLine($"RESULT: THE {DBName} DATABASE AND ITS TABLES HAVE BEEN CREATED");
            }
        }
    }

    public DBApi () { }

    public BaseDBModel findByField (UsersFieldsDB fieldName, String value) {
        BaseDBModel result = new BaseDBModel();
        if (DEBUG) Console.WriteLine($"\t{DBName} DataBase:\t\"IN PROCESS: in search for the {DBName} by {fieldName.ToString()}: {value}\"");
        
        // open the connection
        using (var connection = new MySqlConnection(MSQLConnectionString)) {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @$"SELECT * FROM {DBName} WHERE {fieldName.ToString()} = '{value}'";
            
            using (var reader = command.ExecuteReader()) {
                if (!reader.HasRows) // Literally no element with such value
                    return result;
                if (DEBUG) Console.WriteLine($"\t{DBName} DataBase:\t\"RESULT: the {DBName} {value} has been found\"");

                while (reader.Read()) {
                    Variables sensitive = new Variables();
                    if (DBName == sensitive.getDBName(DBs.Users)) { // if it is Users DB
                        long ChatID = reader.GetInt64(0);
                        String Username = reader.GetString(1);
                        Statuses Status = (Statuses)reader.GetByte(2);
                        Roles Role = (Roles)reader.GetByte(3);
                        long GroupChatID = reader.GetInt64(4);
                        result = new Users(ChatID, Username, Status, Role, GroupChatID);
                        return result; // we can break off early, because there is only one user in the db and we need the first encounter
                    } else if (DBName == sensitive.getDBName(DBs.RegistrationForms)) { // if it is Forms DB
                        String Username = reader.GetString(0);
                        int Age = reader.GetByte(1);
                        int Sex = reader.GetByte(2);
                        proficiencyLevels languageProficiency = (proficiencyLevels)reader.GetByte(3);
                        int Format = reader.GetByte(4);
                        int Frequency = reader.GetByte(5);
                        String Conductor = reader.GetString(6);
                        String Time = reader.GetString(7);
                        int Duration = reader.GetByte(8);
                        Boolean Notifications = reader.GetBoolean(9);
                        long InterestsMask = reader.GetInt32(10);
                        String OtherInterests = reader.GetString(11);
                        result = new FillOutFormParticipants(
                            Username,
                            (Genders)Sex, Age,
                            Frequency, (Formats)Format,
                            languageProficiency, Conductor,
                            Time, Duration, Notifications,
                            InterestsMask, OtherInterests
                        );
                        return result; // we can break off early, because there is only one user in the db and we need the first encounter
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