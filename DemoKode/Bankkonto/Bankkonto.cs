var test = new Test();
test.TestSavingsAccount();
test.TestCheckingAccount();


public class Account
{
    public string Owner { get; init; }
    public decimal Balance { get; protected set; }

    public Account(string owner)
    {
        Owner = owner;
    }

    public void Deposit(decimal amount)
    {
        if(amount < 0) throw new ArgumentException($"{nameof(amount)} must be a positive number");

        Balance += amount;

    }

    public virtual void Withdraw(decimal amount)
    {
        if (amount < 0) throw new ArgumentException($"{nameof(amount)} must be a positive number");

        Balance -= amount;
    }
}


public class SavingsAccount : Account
{
    private decimal MinimumSavingsAccountBalance = 0;
    public SavingsAccount(string owner) : base(owner)
    {
    }

    public override void Withdraw(decimal amount)
    {
        if(Balance - amount < MinimumSavingsAccountBalance) throw new ArgumentException($"Balance under {MinimumSavingsAccountBalance} not allowed");
        base.Withdraw(amount);
    }
}

public class CheckingAccount : Account
{
    private decimal CheckingAccountBalance = -1000;
    public CheckingAccount(string owner) : base(owner)
    {
    }

    public override void Withdraw(decimal amount)
    {
        if (Balance - amount < CheckingAccountBalance) throw new ArgumentException($"Balance under {CheckingAccountBalance} not allowed");
        base.Withdraw(amount);
    }
}

public class Test
{
    public void TestCheckingAccount()
    {
        var checkingAccount = new CheckingAccount("CheckingAccount");
        checkingAccount.Withdraw(1000);
        Console.WriteLine($"{checkingAccount.Balance}");
        try
        {
            checkingAccount.Withdraw(1);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine($"{checkingAccount.Balance}");
        }
    }

    public void TestSavingsAccount()
    {
        var savingsAccount = new SavingsAccount("SavingsAccount");
        savingsAccount.Deposit(1000);
        Console.WriteLine($"{savingsAccount.Balance}");
        savingsAccount.Withdraw(1000);
        Console.WriteLine($"{savingsAccount.Balance}");
        try
        {
            savingsAccount.Withdraw(1);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine($"{savingsAccount.Balance}");
        }
    }
}