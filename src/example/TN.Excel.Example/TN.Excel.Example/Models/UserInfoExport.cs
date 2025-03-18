using TN.Excel.Attributes;

namespace TN.Excel.Example.Models
{
    public class UserInfoExport
    {
        [ExcelColumnHeader("DateOfBirth")]
        public string DateOfBirth { get; set; }

        [ExcelColumnHeader("FullName")]
        public string FullName { get; set; }

        [ExcelColumnHeader("Age")]
        public int Age { get; set; }

        [ExcelColumnHeader("Phone Number")]
        public string PhoneNumber { get; set; }

        [ExcelColumnHeader("Email")]
        public string Email { get; set; }
    }
}
