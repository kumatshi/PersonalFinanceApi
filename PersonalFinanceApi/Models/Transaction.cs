using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace PersonalFinanceApi.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Сумма должна быть больше 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Описание не может превышать 200 символов")]
        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        public TransactionType Type { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }

    public enum TransactionType
    {
        Income,     
        Expense    
    }
}