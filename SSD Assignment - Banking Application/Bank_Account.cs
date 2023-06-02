using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace Banking_Application
{
    public abstract class Bank_Account
    {
        public String accountNo;
        public String name;
        public String address_line_1;
        public String address_line_2;
        public String address_line_3;
        public String town;
        public double balance;

        public String keyBase64;
        public Bank_Account()
        {

        }
        public Bank_Account(String name, String address_line_1, String address_line_2, String address_line_3, String town, double balance)
        {
            using (Aes aesAlgorithm = Aes.Create())
            {

                aesAlgorithm.KeySize = 128;
                aesAlgorithm.GenerateKey();
                keyBase64 = Convert.ToBase64String(aesAlgorithm.Key);
                this.accountNo = System.Guid.NewGuid().ToString();
                this.name = Encryption.EncryptString(keyBase64, name);
                this.address_line_1 = Encryption.EncryptString(keyBase64, address_line_1); ;
                this.address_line_2 = Encryption.EncryptString(keyBase64, address_line_2); ;
                this.address_line_3 = Encryption.EncryptString(keyBase64, address_line_3); ;
                this.town = Encryption.EncryptString(keyBase64, town);
                this.balance = balance;


                Console.WriteLine(keyBase64);

            }
        }
        public void lodge(double amountIn)
        {
            balance += amountIn;
        }
        public abstract bool withdraw(double amountToWithdraw);
        public abstract double getAvailableFunds();
        public override String ToString()
        {

            return "\nAccount No: " + accountNo + "\n" +
            "Name: " + Encryption.DecryptString(keyBase64, name) + "\n" +
            "Address Line 1: " + Encryption.DecryptString(keyBase64, address_line_1) + "\n" +
            "Address Line 2: " + Encryption.DecryptString(keyBase64, address_line_2) + "\n" +
            "Address Line 3: " + Encryption.DecryptString(keyBase64, address_line_3) + "\n" +
            "Town: " + Encryption.DecryptString(keyBase64, town) + "\n" +
            "Balance: " + balance + "\n";

        }
    }
}