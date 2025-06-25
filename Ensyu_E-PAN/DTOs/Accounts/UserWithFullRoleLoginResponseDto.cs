namespace Ensyu_E_PAN.DTOs.Accounts
{
    public class UserWithFullRoleLoginResponseDto
    {
        public int Id { get; set; }
        public string Login_Id { get; set; }
        public string Name { get; set; }

        public RoleDto Role { get; set; }

        public string StoreName { get; set; }
        public string StoreAddress1 { get; set; }
        public string StoreAddress2 { get; set; }
        public string? StorePostCode { get; set; }
        public string? StoreTel { get; set; }
        public string? StoreFax { get; set; }
        public string StoreMail { get; set; }

        public int TimePrice_D { get; set; }
        public int TimePrice_N { get; set; }


    }
}
