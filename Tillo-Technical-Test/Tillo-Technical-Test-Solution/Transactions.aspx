<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Transactions.aspx.cs" Inherits="Tillo_Technical_Test_Solution.Transactions" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Financial Transactions</title>
</head>
<body>
    <form id="form1" runat="server">
       <div class="container mt-5">
            <h2 class="mb-3">Financial Transactions</h2>

            <div class="mb-3">
                <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="btn btn-primary me-2" OnClick="btnRefresh_Click" />
                <asp:Button ID="btnSimulate" runat="server" Text="Simulate Transaction" CssClass="btn btn-success" OnClick="btnSimulate_Click" />
            </div>

            <asp:Label ID="lblMessage" runat="server" CssClass="text-success fw-bold"></asp:Label>
            <div class="float-end mb-3">
                <asp:Label ID="lblBalance" runat="server" CssClass="fw-bold"></asp:Label>
            </div>

            <asp:GridView ID="gvTransactions" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="ID" />
                    <asp:BoundField DataField="Date" HeaderText="Date" DataFormatString="{0:MM-dd-yyyy HH:mm}" />
                    <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="{0:C}" />
                    <asp:BoundField DataField="Description" HeaderText="Description" />
                    <asp:BoundField DataField="Type" HeaderText="Type" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>
