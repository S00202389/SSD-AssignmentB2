using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Banking_Application
{
    public class Data_Access_Layer
    {

        private List<Bank_Account> accounts;
        public static String databaseName = "Banking Database.db";
        private static Data_Access_Layer instance = new Data_Access_Layer();

        private Data_Access_Layer()
        {
            accounts = new List<Bank_Account>();
        }

        public static Data_Access_Layer getInstance()
        {
            return instance;
        }

        private SqliteConnection getDatabaseConnection()
        {

            String databaseConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = Data_Access_Layer.databaseName,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            return new SqliteConnection(databaseConnectionString);

        }

        private void initialiseDatabase()
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS Bank_Accounts(    
                        accountNo TEXT PRIMARY KEY,
                        name TEXT NOT NULL,
                        address_line_1 TEXT,
                        address_line_2 TEXT,
                        address_line_3 TEXT,
                        town TEXT NOT NULL,
                        balance REAL NOT NULL,
                        accountType INTEGER NOT NULL,
                        overdraftAmount REAL,
                        interestRate REAL
                    ) WITHOUT ROWID
                ";

                command.ExecuteNonQuery();

            }
        }

        public void loadBankAccounts()
        {
            if (!File.Exists(Data_Access_Layer.databaseName))
                initialiseDatabase();
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM Bank_Accounts";
                    SqliteDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {

                        int accountType = dr.GetInt16(7);

                        if (accountType == Account_Type.Current_Account)
                        {
                            Current_Account ca = new Current_Account();
                            ca.accountNo = dr.GetString(0);
                            ca.name = dr.GetString(1);
                            ca.address_line_1 = dr.GetString(2);
                            ca.address_line_2 = dr.GetString(3);
                            ca.address_line_3 = dr.GetString(4);
                            ca.town = dr.GetString(5);
                            ca.balance = dr.GetDouble(6);
                            ca.overdraftAmount = dr.GetDouble(8);
                            accounts.Add(ca);
                        }
                        else
                        {
                            Savings_Account sa = new Savings_Account();
                            sa.accountNo = dr.GetString(0);
                            sa.name = dr.GetString(1);
                            sa.address_line_1 = dr.GetString(2);
                            sa.address_line_2 = dr.GetString(3);
                            sa.address_line_3 = dr.GetString(4);
                            sa.town = dr.GetString(5);
                            sa.balance = dr.GetDouble(6);
                            sa.interestRate = dr.GetDouble(9);
                            accounts.Add(sa);
                        }


                    }

                }

            }
        }

        public String addBankAccount(Bank_Account ba)
        {

            if (ba.GetType() == typeof(Current_Account))
                ba = (Current_Account)ba;
            else
                ba = (Savings_Account)ba;

            accounts.Add(ba);

            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO Bank_Accounts VALUES(" +
                    "'" + ba.accountNo + "', " +
                    "'" + ba.name + "', " +
                    "'" + ba.address_line_1 + "', " +
                    "'" + ba.address_line_2 + "', " +
                    "'" + ba.address_line_3 + "', " +
                    "'" + ba.town + "', " +
                    ba.balance + ", " +
                    (ba.GetType() == typeof(Current_Account) ? 1 : 2) + ", ";

                if (ba.GetType() == typeof(Current_Account))
                {
                    Current_Account ca = (Current_Account)ba;
                    command.CommandText += ca.overdraftAmount + ", NULL";
                }
                else
                {
                    Savings_Account sa = (Savings_Account)ba;
                    command.CommandText += "NULL," + sa.interestRate;
                }
                command.CommandText += ", " + "'" + GenerateHMAC(ba.accountNo) + "')";

                // Binding values to parameters
                command.Parameters.AddWithValue("$accountNo", ba.accountNo);
                command.Parameters.AddWithValue("$name", ba.name);
                command.Parameters.AddWithValue("$address_line_1", ba.address_line_1);
                command.Parameters.AddWithValue("$address_line_2", ba.address_line_2);
                command.Parameters.AddWithValue("$address_line_3", ba.address_line_3);
                command.Parameters.AddWithValue("$town", ba.town);
                command.Parameters.AddWithValue("$balance", ba.balance);

                command.ExecuteNonQuery();

            }

            return ba.accountNo;

        }
        public Bank_Account findBankAccountByAccNo(String accNo)

        {

            foreach (Bank_Account ba in accounts)

            {



                if (ba.accountNo.Equals(accNo))

                {

                    if (!ValidateHMAC(ba.accountNo, RetrieveHMAC(ba.accountNo)))
                    {

                        Console.WriteLine("this account is not valid");
                        return null;
                    }

                    return ba;

                }



            }

            return null;

        }

        private string RetrieveHMAC(string accountNumber)
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT hmac FROM Bank_Accounts WHERE accountNo = @accountNumber";
                    command.Parameters.AddWithValue("@accountNumber", accountNumber);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString(0);
                        }
                    }
                }
            }

            return null;
        }

        protected bool ValidateHMAC(string data, string storedHash)
        {
            byte[] storedHashBytes = ConvertHexStringToByteArray(storedHash);

            using (var hmac = new HMACSHA256(GetPrivateKey()))
            {
                byte[] computedHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

                return AreByteArraysEqual(storedHashBytes, computedHashBytes);
            }
        }

        public string GenerateHMAC(string data)
        {
            byte[] dataBuffer = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA256(GetPrivateKey()))
            {
                byte[] hmacBuffer = hmac.ComputeHash(dataBuffer);

                return ConvertByteArrayToHexString(hmacBuffer);
            }
        }

        private static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("Invalid hex string length");
            }

            byte[] byteArray = new byte[hexString.Length / 2];

            for (int i = 0; i < byteArray.Length; i++)
            {
                byteArray[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return byteArray;
        }

        private static bool AreByteArraysEqual(byte[] byteArray1, byte[] byteArray2)
        {
            if (byteArray1.Length != byteArray2.Length)
            {
                return false;
            }

            for (int i = 0; i < byteArray1.Length; i++)
            {
                if (byteArray1[i] != byteArray2[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static string ConvertByteArrayToHexString(byte[] byteArray)
        {
            StringBuilder hexBuilder = new StringBuilder(byteArray.Length * 2);

            for (int i = 0; i < byteArray.Length; i++)
            {
                hexBuilder.AppendFormat("{0:x2}", byteArray[i]);
            }

            return hexBuilder.ToString();
        }

        private static byte[] GetPrivateKey()
        {
            string filePath = @"E:/Final Year-S1/Secure development/SSD-Assignment---Banking-Application-master/SSD-Assignment---Banking-Application-master/SSD Assignment - Banking Application/PrivateKey.txt";
            string fileContents = File.ReadAllText(filePath);
            return Encoding.UTF8.GetBytes(fileContents);
        }

        public bool closeBankAccount(String accNo)
        {

            Bank_Account toRemove = null;

            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    toRemove = ba;
                    break;
                }

            }

            if (toRemove == null)
                return false;
            else
            {
                accounts.Remove(toRemove);

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM Bank_Accounts WHERE accountNo = '" + toRemove.accountNo + "'";
                    command.ExecuteNonQuery();

                }

                return true;
            }

        }

        public bool lodge(String accNo, double amountToLodge)
        {

            Bank_Account toLodgeTo = null;

            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    ba.lodge(amountToLodge);
                    toLodgeTo = ba;
                    break;
                }

            }

            if (toLodgeTo == null)
                return false;
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Bank_Accounts SET balance = " + toLodgeTo.balance + " WHERE accountNo = '" + toLodgeTo.accountNo + "'";
                    command.ExecuteNonQuery();

                }

                return true;
            }

        }

        public bool withdraw(String accNo, double amountToWithdraw)
        {

            Bank_Account toWithdrawFrom = null;
            bool result = false;

            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    result = ba.withdraw(amountToWithdraw);
                    toWithdrawFrom = ba;
                    break;
                }

            }

            if (toWithdrawFrom == null || result == false)
                return false;
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Bank_Accounts SET balance = " + toWithdrawFrom.balance + " WHERE accountNo = '" + toWithdrawFrom.accountNo + "'";
                    command.ExecuteNonQuery();

                }

                return true;
            }

        }

    }
}
