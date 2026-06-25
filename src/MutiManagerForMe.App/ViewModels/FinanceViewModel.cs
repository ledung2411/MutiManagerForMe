using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MutiManagerForMe.App.Data;
using MutiManagerForMe.App.Models;
using MutiManagerForMe.App.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MutiManagerForMe.App.ViewModels;

public partial class FinanceViewModel(DatabaseService database, IUserDialogService dialogs) : PageViewModel
{
    public override string Title => "Thu chi";
    public ObservableCollection<Wallet> Wallets { get; } = [];
    public ObservableCollection<FinanceTransaction> Transactions { get; } = [];
    public IReadOnlyList<string> TransactionTypeOptions => TransactionTypes.All;

    [ObservableProperty] private Wallet? selectedWallet;
    [ObservableProperty] private string selectedType = TransactionTypes.Expense;
    [ObservableProperty] private string selectedCategory = FinanceCategories.Expense[0];
    [ObservableProperty] private IReadOnlyList<string> categoryOptions = FinanceCategories.Expense;
    [ObservableProperty] private string amountText = string.Empty;
    [ObservableProperty] private string transactionNote = string.Empty;
    [ObservableProperty] private DateTime transactionDate = DateTime.Today;
    [ObservableProperty] private decimal monthIncome;
    [ObservableProperty] private decimal monthExpense;
    [ObservableProperty] private decimal totalBalance;
    [ObservableProperty] private decimal budgetLimit;
    [ObservableProperty] private decimal budgetRemaining;
    [ObservableProperty] private double budgetPercent;
    [ObservableProperty] private string budgetText = string.Empty;
    [ObservableProperty] private string newWalletName = string.Empty;
    [ObservableProperty] private string newWalletBalance = "0";
    [ObservableProperty] private string validationMessage = string.Empty;

    public string MonthLabel => DateTime.Today.ToString("MMMM yyyy", new CultureInfo("vi-VN"));

    public override async Task LoadAsync()
    {
        var selectedWalletId = SelectedWallet?.Id;
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var nextMonth = monthStart.AddMonths(1);
        var wallets = await database.GetWalletsAsync();
        var transactions = await database.GetTransactionsAsync(monthStart, nextMonth);
        var budget = await database.GetBudgetAsync(today.ToString("yyyy-MM"));

        Wallets.Clear();
        foreach (var wallet in wallets) Wallets.Add(wallet);
        SelectedWallet = Wallets.FirstOrDefault(w => w.Id == selectedWalletId) ?? Wallets.FirstOrDefault();

        Transactions.Clear();
        foreach (var item in transactions) Transactions.Add(item);

        MonthIncome = transactions.Where(t => !t.IsExpense).Sum(t => t.Amount);
        MonthExpense = transactions.Where(t => t.IsExpense).Sum(t => t.Amount);
        TotalBalance = wallets.Sum(w => w.Balance);
        BudgetLimit = budget.LimitAmount;
        BudgetRemaining = BudgetLimit - MonthExpense;
        BudgetPercent = BudgetLimit <= 0 ? 0 : Math.Min(100, (double)(MonthExpense / BudgetLimit * 100));
        BudgetText = BudgetLimit <= 0 ? string.Empty : BudgetLimit.ToString("0", CultureInfo.InvariantCulture);
    }

    partial void OnSelectedTypeChanged(string value)
    {
        CategoryOptions = value == TransactionTypes.Income ? FinanceCategories.Income : FinanceCategories.Expense;
        SelectedCategory = CategoryOptions[0];
    }

    [RelayCommand]
    private async Task AddTransactionAsync()
    {
        ValidationMessage = string.Empty;
        if (SelectedWallet is null)
        {
            ValidationMessage = "Hãy tạo hoặc chọn một ví.";
            return;
        }
        if (!TryMoney(AmountText, out var amount) || amount <= 0)
        {
            ValidationMessage = "Số tiền phải lớn hơn 0.";
            return;
        }

        await database.AddTransactionAsync(new FinanceTransaction
        {
            WalletId = SelectedWallet.Id,
            Type = SelectedType,
            Amount = amount,
            Category = SelectedCategory,
            Note = TransactionNote,
            OccurredAt = TransactionDate.Date.Add(DateTime.Now.TimeOfDay)
        });
        AmountText = string.Empty;
        TransactionNote = string.Empty;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteTransactionAsync(FinanceTransaction? item)
    {
        if (item is null || !dialogs.Confirm($"Xóa giao dịch {item.Amount:N0} ₫?")) return;
        await database.DeleteTransactionAsync(item.Id);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task SaveBudgetAsync()
    {
        if (!TryMoney(BudgetText, out var amount) || amount < 0)
        {
            ValidationMessage = "Ngân sách không hợp lệ.";
            return;
        }
        await database.SaveBudgetAsync(DateTime.Today.ToString("yyyy-MM"), amount);
        ValidationMessage = string.Empty;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task AddWalletAsync()
    {
        if (string.IsNullOrWhiteSpace(NewWalletName))
        {
            ValidationMessage = "Nhập tên ví.";
            return;
        }
        if (!TryMoney(NewWalletBalance, out var balance))
        {
            ValidationMessage = "Số dư ban đầu không hợp lệ.";
            return;
        }
        var id = await database.AddWalletAsync(NewWalletName, balance);
        NewWalletName = string.Empty;
        NewWalletBalance = "0";
        ValidationMessage = string.Empty;
        await LoadAsync();
        SelectedWallet = Wallets.FirstOrDefault(w => w.Id == id);
    }

    private static bool TryMoney(string value, out decimal result)
    {
        var normalized = value.Trim().Replace(".", string.Empty).Replace(",", string.Empty).Replace(" ", string.Empty);
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }
}
