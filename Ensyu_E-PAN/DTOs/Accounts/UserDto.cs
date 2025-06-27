namespace Ensyu_E_PAN.DTOs.Accounts
{
    namespace Ensyu_E_PAN.DTOs.Masters
    {
        public class UserDto
        {
            public int Id { get; set; }
            public string Login_Id { get; set; }
            public string Name { get; set; }
            public int Roll_Cd { get; set; }
            public string RollName { get; set; }
            public bool IsAdmin { get; set; }
            public int Stores_Cd { get; set; }
            public string StoreName { get; set; }
            public int TimePrice_D { get; set; }
            public int TimePrice_N { get; set; }
        }
    }


}
