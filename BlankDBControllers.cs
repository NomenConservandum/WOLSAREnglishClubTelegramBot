// Temporary blank DB controllers. FOR TESTING PURPOSES ONLY

namespace BDBC { // Blank DataBase Class
    public class Users {
        String username;
        public String getUsername() {
            return username;
        }
        public Users(String username) {
            this.username = username;
        }
        // Checks if the user is valid. Not valid user means the user was not found.
        public bool isValid() {
            if (username == "NONE")
                return false;
            return true;
        }
    }
    public class PseudoDB {
        List<Users> list;
        public PseudoDB() {
            list = new List<Users>() {};
        }
        public Users findByUsername(String username) {
            foreach (var user in list) {
                if (user.getUsername() == username)
                    return user;
            }
            return new Users("NONE");
        }
        public bool Add(Users user) { // must have a check, but I'm not gonna implement it for this, but it will be there 
            list.Add(user);
            return false; // no error
        }
    }
}
