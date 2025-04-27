using MySqlX.XDevAPI.Common;
using Telegram.Bot.Types;

namespace DBEssentials {
    public enum DBs {
        Users,
        RegistrationForms,
        Feedbacks
    }

    public class DBInfo {
        private static Dictionary<String, String> NameToFields = new();
        public DBInfo() {
            NameToFields[getDBName(DBs.Users)] =
            """
                ChatID INTEGER NOT NULL,
                Username TINYTEXT NOT NULL,
                Status TINYINT(1) UNSIGNED NOT NULL,
                Role TINYINT(1) UNSIGNED NOT NULL,
                GroupChatID INTEGER
            """;

            NameToFields[getDBName(DBs.RegistrationForms)] =
            """
                Username TINYTEXT NOT NULL,
                Age TINYINT(1) NOT NULL,
                Sex TINYINT(1) NOT NULL,
                LanguageProficiency TINYINT(1),
                Format TINYINT(1) NOT NULL,
                Frequency TINYINT(1) NOT NULL,
                Conductor TEXT NOT NULL,
                Time TEXT NOT NULL,
                Duration TINYINT(1) NOT NULL,
                Notifications BOOL NOT NULL,
                InterestsMask INTEGER NOT NULL,
                OtherInterests TEXT NOT NULL,
                Stage TINYINT(1) NOT NULL
            """;
            NameToFields[getDBName(DBs.Feedbacks)] =
            """
                ID INTEGER NOT NULL AUTO_INCREMENT,
                Username TEXT NOT NULL,
                Rating TINYINT(1) NOT NULL,
                Text TEXT NOT NULL,
                PRIMARY KEY (id)
            """;
        }
        public String DBNameToFields(DBs Name) {
            return NameToFields[getDBName(Name)];
        }
        public String DBNameToFields(String Name) {
            return NameToFields[Name];
        }
        public String getDBName(DBs name) {
            Variables sensitive = new Variables();
            return sensitive.getDBName(name);
        }

        public String elementInsertString(BaseDBModel element) {
            String result = "";
            Users user = element as Users ?? new Users();
            FillOutFormParticipants form = element as FillOutFormParticipants ?? new FillOutFormParticipants();
            feedbackForm feedback = element as feedbackForm ?? new feedbackForm();
            if (user.isValid()) { // The element is a user
                result =
                    $"""
                    
                    VALUES ({user.getChatID()}, '{user.getUsername()}', {(int)user.getStatus()}, {(int)user.getRole()}, {user.getGroupChatID()});
                    """;
            } else if (form.isValid()) { // The element is a form
                result =
                    $"""
                    
                    VALUES (
                    '{form.getUsername()}',
                    {form.age},
                    {(int)form.sex},
                    {(int)form.languageProficiency},
                    {(int)form.format},
                    {form.frequency},
                    '{form.conductor}',
                    '{form.time}',
                    {form.duration},
                    {form.notifications},
                    {form.interestsMask},
                    '{form.otherInterests}',
                    {(int)form.stage});
                    """;
            } else if (feedback.isValid()) { // The element is a feedback form
                result = 
                    $"""
                    (Username,Rating,Text)
                    VALUES ('{feedback.getUsername()}',{feedback.getRating()},'{feedback.getText()}');
                    """;
            }
            return result;
        }

        public String elementUpdateString(BaseDBModel element) {
            String result = "";
            Users user = element as Users ?? new Users();
            FillOutFormParticipants form = element as FillOutFormParticipants ?? new FillOutFormParticipants();
            feedbackForm feedback = element as feedbackForm ?? new feedbackForm();
            if (user.isValid()) { // The element is a user
                result =
                    $"""
                    SET Status = {(int)user.getStatus()}, Role = {(int)user.getRole()}, GroupChatID = {user.getGroupChatID()}
                    """;
            } else if (form.isValid()) { // The element is a form
                result =
                    $"""
                    SET Age = {form.age}, Sex = {(int)form.sex}, LanguageProficiency = {(int)form.languageProficiency}, Format = {(int)form.format}, Frequency = {form.frequency}, Conductor = '{form.conductor}', Time = '{form.time}', Duration = {form.duration}, Notifications = {form.notifications}, InterestsMask = {form.interestsMask}, OtherInterests = '{form.otherInterests}', Stage = {(int)form.stage}
                    """;
            } else if (feedback.isValid()) { // The element is a feedback form
                result =
                    $"""
                    SET Rating = {feedback.getRating()}, Text = {feedback.getText()}
                    """;
            }
            return result;
        }
    }

    public enum RegistrationFormsFieldsDB {
        None = 0,
    }
    public enum FieldsDB {
        // Users DB fields
        ChatID,
        Username,
        Status,
        Role,
        GroupChatID,
        // Registration Forms DB fields
     // Username is already here
        Age,
        Sex,
        LanguageProficiency,
        Format,
        Frequency,
        Conductor,
        Time,
        Duration,
        Notifications,
        InterestsMask,
        OtherInterests,
        // Feedbacks DB fields
        ID,
     // Username is already here
        Rating,
        Text
    }
	public enum proficiencyLevels {
        zero,
        A1,
        A2,
        B1,
        B2,
        C1
    }
    public enum Statuses {
        NONE,               // The user who has met the bot for the first time.
                            // They are welcomed and are suggested to register.
        AwaitingRegistrationChoice,       // Pre-registration status
        ARCMinister,        // ARC: Awaiting Registration Completion
        ARCCustomer,
        AwaitingAssignment, // a participant who waits to be assigned to a small group
        inLearningProcess,  // a participant who is assigned to a small group. When small group is terminated the customer gets the 'none' role.
        done                // if the customer has the 'done' status they will be asked if they want to terminate the service
    }
    public enum Roles {
        NONE,           // no role: newcomer
        Representative, // ministers
        MMS,            // ministers
        Host,           // ministers
        Customer,       // once assigned this role the customer automatically gets the 'inQueue' status
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
    public enum RegistrationStatuses {
        NONE,
        AwaitingRoleChoice,
        AwaitingSexChoice,
        AwaitingAgeChoice,
        AwaitingLevelChoice,
        AwaitingFormatChoice,
        AwaitingConductorChoice,
        AwaitingConductorResponse, // a user proposes their own idea
        InterestsChoice,
        AwaitingInterestsChoice,
        AwaitingInterestsResponse, // a user writes their own interests
        AwaitingTimeChoice,
    }
    public class BaseDBModel {
        String username = "";
        private bool _validity = false;
        public BaseDBModel() { }
        public BaseDBModel(String username) {
            this.username = username;
        }
        public virtual bool isValid() {
            return _validity;
        }
        public String getUsername() {
            return username;
        }
        public virtual Statuses getStatus() {
            return Statuses.NONE;
        }
    }

    // The fill-out form Class for the participants
    public class FillOutFormParticipants : BaseDBModel {
        public Genders sex = Genders.Male;
        public int age = 0, frequency = 0; // frequency: how many times a week
        public Formats format = Formats.Offline;
        public proficiencyLevels languageProficiency = proficiencyLevels.A1;
        public String conductor = ""; // place or the messenger
        public String time = ""; // the format: "Monday:hourMask,Tuesday:hourMask,...,Sunday:hourMask"
        public int duration = -1; // in minutes
        public Boolean notifications = true;
        public long interestsMask = 0; // a bit mask of interests: board games, video games, books reading, BSFN in English, food, lectures, tests, other options
        public String otherInterests = "";
        public RegistrationStatuses stage = RegistrationStatuses.NONE;
        public FillOutFormParticipants() {}
        public FillOutFormParticipants (
            String username,
            Genders sex, int age,
            int frequency, Formats format,
            proficiencyLevels languageProficiency,
            String conductor, String time,
            int duration, Boolean notifications,
            long interestsMask, String otherInterests,
            RegistrationStatuses stage
        ) : base(username) {
            this.sex = sex;
            this.age = age;
            this.frequency = frequency;
            this.format = format;
            this.languageProficiency = languageProficiency;
            this.conductor = conductor;
            this.time = time;
            this.duration = duration;
            this.notifications = notifications;
            this.interestsMask = interestsMask;
            this.otherInterests = otherInterests;
            this.stage = stage;
        }
        new public bool isValid () {
            return (int)stage != 0;
        }
    }
    public class Users : BaseDBModel {
        long chatID = 0, groupChatID = 0;
        Statuses status;
        Roles role;
        public Users() { }
        public long getChatID() {
            return chatID;
        }
        public long getGroupChatID() {
            return groupChatID;
        }
        public void setGroupChatID(int newID) {
            groupChatID = newID;
        }
        public override Statuses getStatus () {
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
        public Users(
            long chatID,
            String username,
            Statuses status,
            Roles role,
            long groupChatID
        ) : base(username) {
            this.chatID = chatID;
            this.status = status;
            this.role = role;
            this.groupChatID = groupChatID;
        }
        // Checks if the user is valid. Not valid user means the user was not found.
        public override bool isValid() {
            if (chatID == 0 || this.getUsername() == "")
                return false;
            return true;
        }
    }

    public class feedbackForm : BaseDBModel {
        private long id = 0, rating = 0;
        private String text = "";
        public feedbackForm() { }
        public feedbackForm(long id, String username, long rating, String text) : base(username) {
            this.id = id;
            this.rating = rating;
            this.text = text;
        }
        public long getId() {
            return id;
        }
        public String getText() {
            return text;
        }
        public long getRating() {
            return rating;
        }
        public void setRating(long rating) {
            this.rating = rating;
        }
        public void setText(String text) {
            this.text = text;
        }

        public override bool isValid() {
            if (id == 0)
                return false;
            return true;
        }

    }

}