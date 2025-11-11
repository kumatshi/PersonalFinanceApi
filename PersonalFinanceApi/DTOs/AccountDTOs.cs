namespace PersonalFinanceApi.DTOs
{
    public class AccountDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
    }

    public class CreateAccountDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public int Type { get; set; }
    }

    public class UpdateAccountDto
    {
        public string Name { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public int Type { get; set; }
    }
}