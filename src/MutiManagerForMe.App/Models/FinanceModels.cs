namespace MutiManagerForMe.App.Models;

public static class TransactionTypes
{
    public const string Expense = "Chi";
    public const string Income = "Thu";
    public static IReadOnlyList<string> All { get; } = [Expense, Income];
}

public sealed class Wallet
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    public decimal Balance { get; set; }
}

public sealed class FinanceTransaction
{
    public long Id { get; set; }
    public long WalletId { get; set; }
    public string WalletName { get; set; } = string.Empty;
    public string Type { get; set; } = TransactionTypes.Expense;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.Now;

    public bool IsExpense => Type == TransactionTypes.Expense;
    public decimal SignedAmount => IsExpense ? -Amount : Amount;
}

public sealed class Budget
{
    public long Id { get; set; }
    public string Month { get; set; } = DateTime.Today.ToString("yyyy-MM");
    public decimal LimitAmount { get; set; }
}

public static class FinanceCategories
{
    public static IReadOnlyList<string> Expense { get; } =
        ["Ăn uống", "Di chuyển", "Mua sắm", "Hóa đơn", "Sức khỏe", "Giải trí", "Giáo dục", "Khác"];

    public static IReadOnlyList<string> Income { get; } =
        ["Lương", "Thưởng", "Đầu tư", "Bán hàng", "Hoàn tiền", "Khác"];
}
