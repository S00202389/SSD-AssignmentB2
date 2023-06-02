using System;

namespace BankingApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Bank bank = new Bank();
            bool running = true;

            while (running)
            {
                Console.WriteLine("***Banking Application Menu***");
                Console.WriteLine("1. Add Bank Account");
                Console.WriteLine("2. Close Bank Account");
                Console.WriteLine("3. View Account Information");
                Console.WriteLine("4. Make Lodgement");
                Console.WriteLine("5. Make Withdrawal");
                Console.WriteLine("6. Exit");
                Console.WriteLine("Choose an option:");

                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        Console.WriteLine("Enter Account Type (1 for Current Account, 2 for Savings Account):");
                        AccountType accountType = (AccountType)int.Parse(Console.ReadLine());

                        Console.WriteLine("Enter Name:");
                        string name = Console.ReadLine();

                        Console.WriteLine("Enter Address:");
                        string address = Console.ReadLine();

                        Console.WriteLine("Enter Opening Balance:");
                        decimal balance = decimal.Parse(Console.ReadLine());

                        if (accountType == AccountType.CurrentAccount)
                        {
                            Console.WriteLine("Enter Overdraft Amount:");
                            decimal overdraftAmount = decimal.Parse(Console.ReadLine());
                            bank.AddAccount(new CurrentAccount(name, address, balance, overdraftAmount));
                        }
                        else if (accountType == AccountType.SavingsAccount)
                        {
                            Console.WriteLine("Enter Interest Rate:");
                            double interestRate = double.Parse(Console.ReadLine());
                            bank.AddAccount(new SavingsAccount(name, address, balance, interestRate));
                        }
                        else
                        {
                            Console.WriteLine("Invalid account type.");
                        }
                        break;

                    case "2":
                        Console.WriteLine("Enter Account Number:");
                        string accountNumber = Console.ReadLine();
                        bank.CloseAccount(accountNumber);
                        break;

                    case "3":
                        Console.WriteLine("Enter Account Number:");
                        accountNumber = Console.ReadLine();
                        bank.PrintAccountInformation(accountNumber);
                        break;

                    case "4":
                        Console.WriteLine("Enter Account Number:");
                        accountNumber = Console.ReadLine();
                        Console.WriteLine("Enter Amount to Lodge:");
                        decimal amountToLodge = decimal.Parse(Console.ReadLine());
                        bank.MakeLodgement(accountNumber, amountToLodge);
                        break;

                    case "5":
                        Console.WriteLine("Enter Account Number:");
                        accountNumber = Console.ReadLine();
                        Console.WriteLine("Enter Amount to Withdraw:");
                        decimal amountToWithdraw = decimal.Parse(Console.ReadLine());
                        bank.MakeWithdrawal(accountNumber, amountToWithdraw);
                        break;

                    case "6":
                        running = false;
                        Console.WriteLine("Exiting the application...");
                        break;

                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }

                Console.WriteLine();
            }
        }
    }

    public enum AccountType
    {
        CurrentAccount = 1,
        SavingsAccount = 2
    }

    public class Bank
    {
        private List<BankAccount> accounts;

        public Bank()
        {
            accounts = new List<BankAccount>();
        }

        public void AddAccount(BankAccount account)
        {
            accounts.Add(account);
            Console.WriteLine("Account created successfully. Account Number: " + account.AccountNumber);
        }

        public void CloseAccount(string accountNumber)
        {
            BankAccount account = FindAccountByNumber(accountNumber);

            if (account != null)
            {
                accounts.Remove(account);
                Console.WriteLine("Account closed successfully.");
            }
            else
            {
                Console.WriteLine("Account not found.");
            }
        }

        public void PrintAccountInformation(string accountNumber)
        {
            BankAccount account = FindAccountByNumber(accountNumber);

            if (account != null)
            {
                Console.WriteLine(account);
            }
            else
            {
                Console.WriteLine("Account not found.");
            }
        }

        public void MakeLodgement(string accountNumber, decimal amount)
        {
            BankAccount account = FindAccountByNumber(accountNumber);

            if (account != null)
            {
                account.Deposit(amount);
                Console.WriteLine("Lodgement successful.");
            }
            else
            {
                Console.WriteLine("Account not found.");
            }
        }

        public void MakeWithdrawal(string accountNumber, decimal amount)
        {
            BankAccount account = FindAccountByNumber(accountNumber);

            if (account != null)
            {
                bool success = account.Withdraw(amount);

                if (success)
                {
                    Console.WriteLine("Withdrawal successful.");
                }
                else
                {
                    Console.WriteLine("Insufficient funds.");
                }
            }
            else
            {
                Console.WriteLine("Account not found.");
            }
        }

        private BankAccount FindAccountByNumber(string accountNumber)
        {
            return accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
        }
    }

    public abstract class BankAccount
    {
        private static int accountCount = 1;

        public string AccountNumber { get; }
        public string Name { get; }
        public string Address { get; }
        public decimal Balance { get; protected set; }

        public BankAccount(string name, string address, decimal balance)
        {
            AccountNumber = GenerateAccountNumber();
            Name = name;
            Address = address;
            Balance = balance;
        }

        protected string GenerateAccountNumber()
        {
            string accountNumber = accountCount.ToString("D6");
            accountCount++;
            return accountNumber;
        }

        public abstract void Deposit(decimal amount);

        public abstract bool Withdraw(decimal amount);

        public override string ToString()
        {
            return $"Account Number: {AccountNumber}\n" +
                   $"Name: {Name}\n" +
                   $"Address: {Address}\n" +
                   $"Balance: {Balance:C}\n";
        }
    }

    public class CurrentAccount : BankAccount
    {
        public decimal OverdraftAmount { get; }

        public CurrentAccount(string name, string address, decimal balance, decimal overdraftAmount)
            : base(name, address, balance)
        {
            OverdraftAmount = overdraftAmount;
        }

        public override void Deposit(decimal amount)
        {
            Balance += amount;
        }

        public override bool Withdraw(decimal amount)
        {
            decimal totalFunds = Balance + OverdraftAmount;

            if (totalFunds >= amount)
            {
                Balance -= amount;
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return base.ToString() +
                   $"Overdraft Amount: {OverdraftAmount:C}\n";
        }
    }

    public class SavingsAccount : BankAccount
    {
        public double InterestRate { get; }

        public SavingsAccount(string name, string address, decimal balance, double interestRate)
            : base(name, address, balance)
        {
            InterestRate = interestRate;
        }

        public override void Deposit(decimal amount)
        {
            Balance += amount;
        }

        public override bool Withdraw(decimal amount)
        {
            if (Balance >= amount)
            {
                Balance -= amount;
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return base.ToString() +
                   $"Interest Rate: {InterestRate:P}\n";
        }
    }
}