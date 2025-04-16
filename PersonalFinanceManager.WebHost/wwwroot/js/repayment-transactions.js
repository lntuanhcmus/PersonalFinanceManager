// repayment-transactions.js
// Namespace để tránh xung đột
window.RepaymentTransactions = window.RepaymentTransactions || {};

RepaymentTransactions.fetchRepaymentTransactions = function (transactionId) {
    fetch(`/TransactionsManagement/GetRepaymentTransactionsByTransactionId?transactionId=${transactionId}`, {
        method: 'GET',
        headers: {
            'Accept': 'application/json'
        }
    })
        .then(response => response.json())
        .then(data => {
            const tableBody = document.querySelector('#repaymentTransactionsTable tbody');
            tableBody.innerHTML = '';

            if (data && data.length > 0) {
                data.forEach(item => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                    <td>${item.id}</td>
                    <td>${item.transactionTime}</td>
                    <td>${item.amount.toLocaleString('vi-VN')}</td>
                    <td>${item.senderName || ''}</td>
                    <td>${item.description || ''}</td>
                    <td>${item.createdAt}</td>
                    <td>
                        <button class="btn btn-warning btn-sm" onclick='RepaymentTransactions.openEditModal(${JSON.stringify(item)})'>Sửa</button>
                        <button class="btn btn-danger btn-sm" onclick="RepaymentTransactions.deleteRepayment(${item.id})">Xóa</button>
                    </td>
                `;
                    tableBody.appendChild(row);
                });
            } else {
                tableBody.innerHTML = '<tr><td colspan="7" class="text-center">Không có giao dịch hoàn trả nào.</td></tr>';
            }
        })
        .catch(error => {
            console.error('Error fetching repayment transactions:', error);
            const tableBody = document.querySelector('#repaymentTransactionsTable tbody');
            tableBody.innerHTML = '<tr><td colspan="7" class="text-center">Lỗi khi tải dữ liệu.</td></tr>';
        });
};

RepaymentTransactions.submitRepaymentForm = function () {
    const form = document.getElementById('addRepaymentForm');
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    const formData = new FormData(form);
    const data = {};
    formData.forEach((value, key) => {
        data[key] = value;
    });

    fetch(`/TransactionsManagement/AddRepaymentTransaction`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        body: JSON.stringify(data)
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => { throw new Error(text || 'Failed to create repayment transaction'); });
            }
            return response.text().then(text => text ? JSON.parse(text) : {});
        })
        .then(() => {
            const modal = bootstrap.Modal.getInstance(document.getElementById('addRepaymentModal'));
            modal.hide();
            form.reset();
            form.classList.remove('was-validated');
            RepaymentTransactions.fetchRepaymentTransactions(document.getElementById('TransactionId').value);
        })
        .catch(error => {
            console.error('Error creating repayment transaction:', error);
            alert('Lỗi khi thêm giao dịch hoàn trả: ' + error.message);
        });
};

RepaymentTransactions.openEditModal = function (item) {
    document.getElementById('editId').value = item.id;
    document.getElementById('editTransactionTime').value = item.transactionTime;
    document.getElementById('editAmount').value = item.amount;
    document.getElementById('editSenderName').value = item.senderName || '';
    document.getElementById('editDescription').value = item.description || '';

    const modal = new bootstrap.Modal(document.getElementById('editRepaymentModal'));
    modal.show();
};

RepaymentTransactions.submitEditRepaymentForm = function () {
    const form = document.getElementById('editRepaymentForm');
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    const formData = new FormData(form);
    const data = {};
    formData.forEach((value, key) => {
        data[key] = value;
    });

    fetch(`/TransactionsManagement/EditRepaymentTransaction/${data.Id}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        body: JSON.stringify(data)
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => { throw new Error(text || 'Failed to update repayment transaction'); });
            }
            return response.text().then(text => text ? JSON.parse(text) : {});
        })
        .then(() => {
            const modal = bootstrap.Modal.getInstance(document.getElementById('editRepaymentModal'));
            modal.hide();
            form.reset();
            form.classList.remove('was-validated');
            RepaymentTransactions.fetchRepaymentTransactions(document.getElementById('TransactionId').value);
        })
        .catch(error => {
            console.error('Error updating repayment transaction:', error);
            alert('Lỗi khi sửa giao dịch hoàn trả: ' + error.message);
        });
};

RepaymentTransactions.deleteRepayment = function (id) {
    if (!confirm('Bạn có chắc muốn xóa giao dịch hoàn trả này?')) {
        return;
    }

    fetch(`/TransactionsManagement/DeleteRepaymentTransaction/${id}`, {
        method: 'DELETE',
        headers: {
            'Accept': 'application/json'
        }
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => { throw new Error(text || 'Failed to delete repayment transaction'); });
            }
            return response.text().then(text => text ? JSON.parse(text) : {});
        })
        .then(() => {
            RepaymentTransactions.fetchRepaymentTransactions(document.getElementById('TransactionId').value);
        })
        .catch(error => {
            console.error('Error deleting repayment transaction:', error);
            alert('Lỗi khi xóa giao dịch hoàn trả: ' + error.message);
        });
};