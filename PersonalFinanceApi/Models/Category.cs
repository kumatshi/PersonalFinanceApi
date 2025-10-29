namespace PersonalFinanceApi.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public decimal MonthlyBudget { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}