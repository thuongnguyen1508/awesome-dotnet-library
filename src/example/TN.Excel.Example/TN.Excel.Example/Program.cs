// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using TN.Excel;
using TN.Excel.Example.Models;
using TN.Excel.Services.Abstractions;

Console.WriteLine("Hello, World!");

var services = new ServiceCollection();
services.AddExcelService();

var serviceProvider = services.BuildServiceProvider();
var excelService = serviceProvider.GetService<IExcelService>();

// Export data to excel
var datas = new List<UserInfoExport>
{
    new UserInfoExport
    {
        DateOfBirth = new DateTime(1990, 1, 1).ToString("yyyy/MM/dd"),
        FullName = "John Doe",
        Age = 30,
        PhoneNumber = "1234567890",
        Email = "JoinDoe@gmail.com"
    },
    new UserInfoExport
    {
        DateOfBirth = new DateTime(1991, 1, 1).ToString("yyyy/MM/dd"),
        FullName = "Alex",
        Age = 30,
        PhoneNumber = "81234567891",
        Email = "alex@gmail.com"
    },
};

var projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
var filePath = Path.Combine(projectRoot, $"users{Guid.NewGuid()}.xlsx");
var stream = excelService.Export(datas, "UserInfo");
using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
{
    stream.CopyTo(fileStream);
}

// Import data from excel
var importDatas = excelService.ReadExcelFromStream<UserInfoImport>(stream, "UserInfo", true);
foreach (var data in importDatas)
{
    Console.WriteLine($"DateOfBirth: {data.DateOfBirth}, FullName: {data.FullName}, Age: {data.Age}, PhoneNumber: {data.PhoneNumber}, Email: {data.Email}");
}

