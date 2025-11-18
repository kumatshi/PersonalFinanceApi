using Microsoft.EntityFrameworkCore;
using PersonalFinanceApi.Data;
using PersonalFinanceApi.Models;

namespace PersonalFinanceApi.Services
{
    public class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Проверяем и создаем категории
            await SeedCategoriesAsync(context);

            // Проверяем и создаем пользователей
            await SeedUsersAsync(context);
        }

        private static async Task SeedCategoriesAsync(AppDbContext context)
        {
            if (!await context.Categories.AnyAsync())
            {
                var categories = new[]
                {
                    // Категории доходов
                    new Category { Name = "Зарплата", Color = "#4CAF50", Icon = "work", Type = TransactionType.Income, MonthlyBudget = 0 },
                    new Category { Name = "Фриланс", Color = "#8BC34A", Icon = "computer", Type = TransactionType.Income, MonthlyBudget = 0 },
                    new Category { Name = "Инвестиции", Color = "#CDDC39", Icon = "show_chart", Type = TransactionType.Income, MonthlyBudget = 0 },
                    new Category { Name = "Подарки", Color = "#FFEB3B", Icon = "card_giftcard", Type = TransactionType.Income, MonthlyBudget = 0 },
                    new Category { Name = "Премия", Color = "#FFC107", Icon = "military_tech", Type = TransactionType.Income, MonthlyBudget = 0 },
                    new Category { Name = "Дивиденды", Color = "#FF9800", Icon = "account_balance", Type = TransactionType.Income, MonthlyBudget = 0 },
                    
                    // Категории расходов
                    new Category { Name = "Продукты", Color = "#F44336", Icon = "local_grocery_store", Type = TransactionType.Expense, MonthlyBudget = 25000 },
                    new Category { Name = "Транспорт", Color = "#E91E63", Icon = "directions_car", Type = TransactionType.Expense, MonthlyBudget = 8000 },
                    new Category { Name = "Развлечения", Color = "#9C27B0", Icon = "local_movies", Type = TransactionType.Expense, MonthlyBudget = 5000 },
                    new Category { Name = "Жилье", Color = "#673AB7", Icon = "apartment", Type = TransactionType.Expense, MonthlyBudget = 40000 },
                    new Category { Name = "Здоровье", Color = "#3F51B5", Icon = "favorite", Type = TransactionType.Expense, MonthlyBudget = 5000 },
                    new Category { Name = "Одежда", Color = "#2196F3", Icon = "checkroom", Type = TransactionType.Expense, MonthlyBudget = 7000 },
                    new Category { Name = "Рестораны", Color = "#03A9F4", Icon = "restaurant", Type = TransactionType.Expense, MonthlyBudget = 6000 },
                    new Category { Name = "Образование", Color = "#00BCD4", Icon = "school", Type = TransactionType.Expense, MonthlyBudget = 3000 },
                    new Category { Name = "Связь", Color = "#009688", Icon = "smartphone", Type = TransactionType.Expense, MonthlyBudget = 1500 }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Категории созданы");
            }
        }

        private static async Task SeedUsersAsync(AppDbContext context)
        {
            if (!await context.Users.AnyAsync())
            {
                var users = new[]
                {
                    new User
                    {
                        Username = "admin",
                        Email = "admin@finance.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                        Role = UserRoles.Admin,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Username = "demo",
                        Email = "demo@finance.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
                        Role = UserRoles.User,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Пользователи созданы");

                // Создаем счета и транзакции для пользователей
                await SeedUserDataAsync(context);
            }
        }

        private static async Task SeedUserDataAsync(AppDbContext context)
        {
            var adminUser = await context.Users.FirstAsync(u => u.Username == "admin");
            var demoUser = await context.Users.FirstAsync(u => u.Username == "demo");

            // Создаем счета
            var accounts = new[]
            {
                new Account { Name = "Наличные", Balance = 8450.25m, Currency = "RUB", Type = AccountType.Cash, UserId = adminUser.Id },
                new Account { Name = "Тинькофф Black", Balance = 32780.75m, Currency = "RUB", Type = AccountType.BankCard, UserId = adminUser.Id },
                new Account { Name = "Основная карта", Balance = 15000.00m, Currency = "RUB", Type = AccountType.BankCard, UserId = demoUser.Id }
            };

            await context.Accounts.AddRangeAsync(accounts);
            await context.SaveChangesAsync();

            // Создаем транзакции
            var salaryCategory = await context.Categories.FirstAsync(c => c.Name == "Зарплата");
            var productsCategory = await context.Categories.FirstAsync(c => c.Name == "Продукты");
            var adminCardAccount = await context.Accounts.FirstAsync(a => a.Name == "Тинькофф Black" && a.UserId == adminUser.Id);
            var demoCardAccount = await context.Accounts.FirstAsync(a => a.Name == "Основная карта" && a.UserId == demoUser.Id);

            var transactions = new[]
            {
                new Transaction { Amount = 85000.00m, Description = "Зарплата за январь", Date = DateTime.UtcNow.AddDays(-10), Type = TransactionType.Income, CategoryId = salaryCategory.Id, AccountId = adminCardAccount.Id, UserId = adminUser.Id },
                new Transaction { Amount = 30000.00m, Description = "Зарплата", Date = DateTime.UtcNow.AddDays(-8), Type = TransactionType.Income, CategoryId = salaryCategory.Id, AccountId = demoCardAccount.Id, UserId = demoUser.Id }
            };

            await context.Transactions.AddRangeAsync(transactions);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ Счета и транзакции созданы");
            Console.WriteLine("👤 Данные для входа: admin/Admin123! и demo/Demo123!");
        }
    }
}