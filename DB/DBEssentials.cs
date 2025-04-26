
namespace DBEssentials {
    public enum DBs {
        Users,
        RegistrationForms,
        Feedbacks
    }
    public enum RegistrationFormsFieldsDB {
        None = 0,
    }
    public enum UsersFieldsDB {
        ChatID,
        Username,
        Status,
        Role,
        GroupChatID,
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
        AwaitingAgeChoice,
        AwaitingRating,
        AwaitingFeedback,
        AwaitingContactRequest,
        inregprocMinister,  // inregproc: in registration process
        inregprocCustomer,
        inQueue,            // a participant who waits to be assigned to a small group
        inLearningProc,     // a participant who is assigned to a small group. When small group is terminated the customer gets the 'none' role.
        done                // if the customer has the 'done' status they will be asked if they want to terminate the service
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
        public FillOutFormParticipants() {}
        public FillOutFormParticipants (
            String username,
            Genders sex, int age,
            int frequency, Formats format,
            proficiencyLevels languageProficiency,
            String conductor, String time,
            int duration, Boolean notifications,
            long interestsMask, String otherInterests
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
        }
        new public bool isValid () {
            return duration != -1;
        }
    }
    public class Users : BaseDBModel {
        long chatID, groupChatID;
        Statuses status;
        Roles role;
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
            if (chatID == 0 && this.getUsername() == "")
                return false;
            return true;
        }
    }

}
