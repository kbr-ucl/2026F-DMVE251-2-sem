// var test = new Test();
// test.TestSavingsAccount();
// test.TestCheckingAccount();
var test = new CustomerTest();
test.DoTest();

public class Customer
{
    public Account Account { get; private set; }
    public Customer(Account account)
    {
        Account = account;
    }

    public void ChangeAccountType(Account account)
    {
        Account = account;
    }
}

public class CustomerTest
{
    public void DoTest()
    {
        var account = new SavingsAccount("sdf");
        var customer = new Customer(account);
        customer.Account.Deposit(1000);
        customer.Account.Withdraw(1000);
        try
        {
            customer.Account.Withdraw(200);
        }
        catch (System.Exception)
        {

            Console.WriteLine("Overtræk");
        }

        var newAccount = new CheckingAccount(account.Owner, account.Balance);
        customer.ChangeAccountType(newAccount);
        customer.Account.Withdraw(200);
        Console.WriteLine(customer.Account.Balance);
    }
}

public class Account
{
    public string Owner { get; init; }
    public decimal Balance { get; protected set; }

    public Account(string owner, decimal openingBalance = 0)
    {
        Owner = owner;
        Balance = openingBalance;
    }

    public void Deposit(decimal amount)
    {
        if (amount < 0) throw new ArgumentException($"{nameof(amount)} must be a positive number");

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
    public SavingsAccount(string owner, decimal openingBalance = 0) : base(owner, openingBalance)
    {
    }

    public override void Withdraw(decimal amount)
    {
        if (Balance - amount < MinimumSavingsAccountBalance) throw new ArgumentException($"Balance under {MinimumSavingsAccountBalance} not allowed");
        base.Withdraw(amount);
    }
}

public class CheckingAccount : Account
{
    private decimal CheckingAccountBalance = -1000;
    public CheckingAccount(string owner, decimal openingBalance = 0) : base(owner, openingBalance)
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