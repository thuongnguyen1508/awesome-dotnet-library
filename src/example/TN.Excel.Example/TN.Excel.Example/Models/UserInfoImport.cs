using TN.Excel.Attributes;

namespace TN.Excel.Example.Models
{
    public class UserInfoImport
    {
        [ExcelColumnIndex(0)]
        public string DateOfBirth { get; set; }

        [ExcelColumnIndex(1)]
        public string FullName { get; set; }

        [ExcelColumnIndex(2)]
        public int Age { get; set; }

        [ExcelColumnIndex(3)]
        public string PhoneNumber { get; set; }

        [ExcelColumnIndex(4)]
        public string Email { get; set; }
    }
}
