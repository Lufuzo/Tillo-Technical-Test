using Data_Layer.Models;
using NUnit.Framework;
using Repository_Layer.Transaction_Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nunit_Transaction_TestProject
{
    public class NunitTestClass
    {
        [TestFixture]
        public class TransactionRepositoryTests
        {
            private string _testDbPath = string.Empty;
            private string _connectionString = string.Empty;
            private TransactionRepository _repo;

            [SetUp]
            public void Setup()
            {
                // Each test runs on a clean DB file
                _testDbPath = Path.Combine(Path.GetTempPath(), $"transactions_test_{Guid.NewGuid()}.db");
                _connectionString = $"Data Source={_testDbPath};Version=3;BusyTimeout=5000;Journal Mode=WAL;";

                _repo = new TransactionRepository(_connectionString);
                _repo.Initialize();
            }

            [TearDown]
            public void Cleanup()
            {
                if (File.Exists(_testDbPath))
                    File.Delete(_testDbPath);
            }

            [Test]
            public void Initialize_ShouldCreateDefaultAccount()
            {
                var account = _repo.GetOrCreateDefaultAccount();
                Assert.That(account, Is.Not.Null);
                Assert.That(account.Name, Is.EqualTo("Default"));
                Assert.That(account.Balance, Is.EqualTo(0m));
            }

            [Test]
            public void AddTransaction_ShouldInsertRecord()
            {
                var account = _repo.GetOrCreateDefaultAccount();
                var tx = new Transaction
                {
                    AccountId = account.AccountId,
                    Amount = 100,
                    Type = TransactionType.Deposit,
                    Description = "Test deposit",
                    Timestamp = DateTime.UtcNow,
                    Status = TransactionStatus.Success
                };

                _repo.AddTransaction(tx);

                var results = _repo.GetHistory().ToList();
                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Amount, Is.EqualTo(100));
                Assert.That(results[0].Type, Is.EqualTo(TransactionType.Deposit));
            }

            [Test]
            public void UpdateAccount_ShouldChangeBalance()
            {
                var account = _repo.GetOrCreateDefaultAccount();
                account.Balance = 500m;

                _repo.UpdateAccount(account);

                var updated = _repo.GetAccount(account.AccountId);
                Assert.That(updated.Balance, Is.EqualTo(500m));
            }

            [Test]
            public void GetHistory_ShouldReturnRecentTransactions()
            {
                var account = _repo.GetOrCreateDefaultAccount();

                for (int i = 0; i < 3; i++)
                {
                    _repo.AddTransaction(new Transaction
                    {
                        AccountId = account.AccountId,
                        Amount = 10 * (i + 1),
                        Type = TransactionType.Deposit,
                        Description = $"Tx {i}",
                        Timestamp = DateTime.UtcNow.AddMinutes(-i),
                        Status = TransactionStatus.Success
                    });
                }

                var history = _repo.GetHistory(2).ToList();
                Assert.That(history.Count, Is.EqualTo(2));
            }
        }
    }
}
