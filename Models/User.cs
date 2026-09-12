namespace Porjai20.Models
{
    public class User
    {
        public int Emp_ID { get; set; }
        public int Id
        {
            get => Emp_ID;
            set => Emp_ID = value;
        }

        public string Emp_Name { get; set; } = string.Empty;
        public string Name
        {
            get => Emp_Name;
            set => Emp_Name = value;
        }

        public string Emp_Address { get; set; } = string.Empty;

        public string Emp_Tel { get; set; } = string.Empty;
        public string Phone
        {
            get => Emp_Tel;
            set => Emp_Tel = value;
        }

        public string Emp_Username { get; set; } = string.Empty;
        public string Username
        {
            get => Emp_Username;
            set => Emp_Username = value;
        }

        public string Emp_Password { get; set; } = string.Empty;
        public string Password
        {
            get => Emp_Password;
            set => Emp_Password = value;
        }

        public string Emp_Role { get; set; } = "User";
        public string Role
        {
            get => Emp_Role;
            set => Emp_Role = value;
        }
    }
}
