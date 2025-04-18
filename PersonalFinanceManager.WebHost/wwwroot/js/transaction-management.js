window.TransactionManagement = window.TransactionManagement || {};

document.addEventListener('DOMContentLoaded', function () {

    $('select').select2({
        theme: 'bootstrap-5', // Use Bootstrap 5 theme
        placeholder: '-- Chọn --', // Optional: Customize placeholder
        allowClear: true // Optional: Allow clearing selection
    });

    // Initialize Flatpickr
    flatpickr('.flatpickr', {
        dateFormat: 'd/m/Y',
        allowInput: true,
        onClose: function (selectedDates, dateStr, instance) {
            instance.element.value = dateStr;
        }
    });

    // Delete Confirmation
    document.querySelectorAll('.delete-btn').forEach(function (button) {
        button.addEventListener('click', function () {
            var formId = this.getAttribute('data-form-id');
            var form = document.getElementById(formId);

            Utilities.showConfirmationModal(
                'Bạn có chắc muốn xóa giao dịch này?',
                function () {
                    form.submit();
                }
            );
        });
    });

    const slidePanel = document.getElementById('slidePanel');
    const closePanelBtn = document.getElementById('closePanel');
    const transactionDetails = document.getElementById('transactionDetails');

    // Kiểm tra xem các phần tử có tồn tại
    if (!slidePanel || !closePanelBtn || !transactionDetails) {
        console.error('Không tìm thấy một hoặc nhiều phần tử: slidePanel, closePanelBtn, closePanelBottomBtn, transactionDetails');
        return;
    }

    // Mở panel khi bấm nút "Xem chi tiết"
    document.querySelectorAll('.view-details').forEach(btn => {
        btn.addEventListener('click', async () => {
            try {
                const transactionId = btn.dataset.transactionId;
                const response = await fetch(`/TransactionsManagement/GetTransactionDetails/${transactionId}`);
                const transaction = await response.json();
                transactionDetails.innerHTML = `
                    <li class="list-group-item"><strong>Mã GD:</strong> ${transaction.transactionId}</li>
                    <li class="list-group-item"><strong>Thời gian:</strong> ${transaction.transactionTime}</li>
                    <li class="list-group-item"><strong>Nguồn gửi:</strong> ${transaction.sourceAccount}</li>
                    <li class="list-group-item"><strong>Nguồn nhận:</strong> ${transaction.recipientAccount}</li>
                    <li class="list-group-item"><strong>Người nhận:</strong> ${transaction.recipientName}</li>
                    <li class="list-group-item"><strong>Tổng tiền:</strong> ${transaction.amount.toLocaleString('vi-VN')} VND</li>
                    <li class="list-group-item"><strong>Nội dung:</strong> ${transaction.description}</li>
                    <li class="list-group-item"><strong>Loại GD:</strong> ${transaction.transactionTypeName}</li>
                    <li class="list-group-item"><strong>Danh mục:</strong> ${transaction.categoryName}</li>
                    <li class="list-group-item"><strong>Trạng thái:</strong> ${transaction.status}</li>
                `;
                slidePanel.classList.add('open');
            } catch (error) {
                console.error('Lỗi khi phân tích dữ liệu giao dịch:', error);
            }
        });
    });

    // Đóng panel khi bấm nút "Đóng" (header)
    closePanelBtn.addEventListener('click', () => {
        slidePanel.classList.remove('open');
    });

    // Đóng panel khi bấm ra ngoài
    document.addEventListener('click', (e) => {
        if (slidePanel.classList.contains('open') && !slidePanel.contains(e.target) && !e.target.closest('.view-details')) {
            slidePanel.classList.remove('open');
        }
    });

    // Đóng panel bằng phím ESC
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && slidePanel.classList.contains('open')) {
            slidePanel.classList.remove('open');
        }
    });
});

TransactionManagement.resetForm = function () {
    window.location.href = window.location.origin + window.location.pathname;;
};

TransactionManagement.goToPage = function (pageCount) {
    var page = document.getElementById('goToPage').value;
    if (page && page >= 1 && page <= pageCount) {
        window.location.href = '@Url.Action("Index", new { transactionId = Model.TransactionId, startDate = Model.StartDate, endDate = Model.EndDate, minAmount = Model.MinAmount, maxAmount = Model.MaxAmount, categoryId = Model.CategoryId, sourceAccount = Model.SourceAccount, content = Model.Content, page = "__page__" })'.replace('__page__', page);
    }
};

TransactionManagement.exportToExcel = function (url)
{
    event.preventDefault();
    // Thu thập giá trị từ form
    var transactionId = $('#transactionId').val();
    var startDate = $('#startDate').val();
    var endDate = $('#endDate').val();
    var minAmount = $('#minAmount').val();
    var maxAmount = $('#maxAmount').val();
    var transactionTypeId = $('#TransactionTypeId').val();
    var categoryId = $('#CategoryId').val();
    var status = $('#Status').val();
    var sourceAccount = $('#sourceAccount').val();
    var content = $('#content').val();

    // Tạo query string
    var query = `?transactionId=${encodeURIComponent(transactionId || '')}` +
        `&startDate=${encodeURIComponent(startDate || '')}` +
        `&endDate=${encodeURIComponent(endDate || '')}` +
        `&minAmount=${encodeURIComponent(minAmount || '')}` +
        `&maxAmount=${encodeURIComponent(maxAmount || '')}` +
        `&transactionTypeId=${encodeURIComponent(transactionTypeId || '')}` +
        `&categoryId=${encodeURIComponent(categoryId || '')}` +
        `&status=${encodeURIComponent(status || '')}` +
        `&sourceAccount=${encodeURIComponent(sourceAccount || '')}` +
        `&content=${encodeURIComponent(content || '')}`;

    // Chuyển hướng đến hành động Export
    toastr.success("Xuất file Excel thành công");
    window.location.href = url + query;
    // Khôi phục trạng thái nút sau 5 giây
    setTimeout(function () {
        var button = document.querySelector('.export-btn');

        button.disabled = false;
        button.innerHTML = '<i class="bi bi-file-earmark-spreadsheet me-1"></i> Xuất Excel';
    }, 3000); // 5 giây
};

