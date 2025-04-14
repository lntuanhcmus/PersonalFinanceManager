using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.ML;

namespace PersonalFinanceManager.MLTrainer
{
    class Program
    {
        static void Main(string[] args)
        {
			try
			{
                Console.WriteLine("Creating DbContext...");
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer("Server=localhost;Database=PersonalFinanceDB;User Id=sa;Password=123456;MultipleActiveResultSets=true;TrustServerCertificate=True;")
                    .Options;

                using var dbContext = new AppDbContext(options);

                Console.WriteLine("Starting ML training...");
                ModelTrainer.TrainAndSave(
                    dbContext,
                    "categoryModel.zip",
                    "typeModel.zip"
                );
                Console.WriteLine("Training completed!");
            }
			catch (Exception ex)
			{
                Console.WriteLine($"Error during execution: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
            }
        }
    }
}