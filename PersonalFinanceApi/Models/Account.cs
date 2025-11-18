namespace PersonalFinanceApi.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "RUB";
        public AccountType Type { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

    public enum AccountType
    {
        Cash,          
        BankCard,       
        CreditCard,     
        Savings,       
        Investment     
    }
}