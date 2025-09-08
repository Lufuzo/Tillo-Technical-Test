<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Transactions.aspx.cs" Inherits="Tillo_Technical_Test_Solution.Transactions" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Financial Transactions</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
</head>
<body>
    <div class="container mt-5">
        <h2>Financial Transactions</h2>

        <!-- Action buttons -->
        <div class="mb-3">
            <button id="btnRefresh" class="btn btn-primary me-2">Refresh</button>
        </div>

        <label id="lblMessage" class="text-success fw-bold"></label>
        <div class="float-end mb-3">
            <label id="lblBalance" class="fw-bold"></label>
        </div>

        <!-- Deposit Form -->
        <div class="card p-3 mb-3">
            <h5>Deposit</h5>
            <div class="input-group mb-2">
                <span class="input-group-text">Amount</span>
                <input type="number" id="depositAmount" class="form-control" placeholder="Enter amount" />
            </div>
            <div class="input-group mb-2">
                <span class="input-group-text">Description</span>
                <input type="text" id="depositDesc" class="form-control" placeholder="Optional description" />
            </div>
            <button id="btnDeposit" class="btn btn-success">Deposit</button>
        </div>

        <!-- Withdraw Form -->
        <div class="card p-3 mb-3">
            <h5>Withdraw</h5>
            <div class="input-group mb-2">
                <span class="input-group-text">Amount</span>
                <input type="number" id="withdrawAmount" class="form-control" placeholder="Enter amount" />
            </div>
            <div class="input-group mb-2">
                <span class="input-group-text">Description</span>
                <input type="text" id="withdrawDesc" class="form-control" placeholder="Optional description" />
            </div>
            <button id="btnWithdraw" class="btn btn-danger">Withdraw</button>
        </div>

        <!-- Transfer Form -->
        <div class="card p-3 mb-3">
            <h5>Transfer</h5>
            <div class="input-group mb-2">
                <span class="input-group-text">Amount</span>
                <input type="number" id="transferAmount" class="form-control" placeholder="Enter amount" />
            </div>
            <div class="input-group mb-2">
                <span class="input-group-text">Receiver</span>
                <input type="text" id="transferReceiver" class="form-control" placeholder="Receiver name" />
            </div>
            <div class="input-group mb-2">
                <span class="input-group-text">Description</span>
                <input type="text" id="transferDesc" class="form-control" placeholder="Optional description" />
            </div>
            <button id="btnTransfer" class="btn btn-warning">Transfer</button>
        </div>

        <!-- Transactions Table -->
        <table id="tblTransactions" class="table table-bordered">
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Date</th>
                    <th>Amount</th>
                    <th>Description</th>
                    <th>Type</th>
                    <th>Status</th>
                </tr>
            </thead>
            <tbody></tbody>
        </table>
    </div>

    <script>
        const apiBaseUrl = "https://localhost:44353/api/transactions";

        function loadTransactions() {
            $.get(apiBaseUrl + "/history", function (data) {
                let rows = "";
                data.forEach(t => {
                    rows += `<tr>
                        <td>${t.TransactionId}</td>
                        <td>${new Date(t.Timestamp).toLocaleString()}</td>
                        <td>${t.Amount.toFixed(2)}</td>
                        <td>${t.Description || ""}</td>
                        <td>${t.Type}</td>
                        <td>${t.Status}</td>
                    </tr>`;
                });
                $("#tblTransactions tbody").html(rows);
            });
        }

        function loadBalance() {
            $.get(apiBaseUrl + "/balance", function (data) {
                $("#lblBalance").text("Current Balance: R" + data.toFixed(2));
            });
        }

        function postTransaction(url, payload, successMsg) {
            $.ajax({
                url: apiBaseUrl + url,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                success: function () {
                    $("#lblMessage").text(successMsg);
                    loadTransactions();
                    loadBalance();
                },
                error: function () {
                    $("#lblMessage").text("Transaction failed.");
                }
            });
        }

        $(document).ready(function () {
            loadTransactions();
            loadBalance();

            $("#btnRefresh").click(function () {
                loadTransactions();
                loadBalance();
            });

            $("#btnDeposit").click(function () {
                let amount = parseFloat($("#depositAmount").val());
                let desc = $("#depositDesc").val();
                if (!amount || amount <= 0) {
                    $("#lblMessage").text("Enter a valid deposit amount.");
                    return;
                }
                postTransaction("/deposit", { Amount: amount, Description: desc }, "Deposit successful!");
            });

            $("#btnWithdraw").click(function () {
                let amount = parseFloat($("#withdrawAmount").val());
                let desc = $("#withdrawDesc").val();
                if (!amount || amount <= 0) {
                    $("#lblMessage").text("Enter a valid withdrawal amount.");
                    return;
                }
                postTransaction("/withdraw", { Amount: amount, Description: desc }, "Withdrawal successful!");
            });

            $("#btnTransfer").click(function () {
                let amount = parseFloat($("#transferAmount").val());
                let receiver = $("#transferReceiver").val();
                let desc = $("#transferDesc").val();
                if (!amount || amount <= 0 || !receiver) {
                    $("#lblMessage").text("Enter valid transfer details.");
                    return;
                }
                postTransaction("/transfer", { Amount: amount, Receiver: receiver, Description: desc }, "Transfer successful!");
            });
        });
    </script>
</body>

</html>
