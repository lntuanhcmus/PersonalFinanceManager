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

            showConfirmationModal(
                'Bạn có chắc muốn xóa giao dịch này?',
                function () {
                    form.submit();
                }
            );
        });
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

// Custom Confirmation Modal (Bootstrap)
TransactionManagement.showConfirmationModal = function (message, callback) {
    var modalHtml = `
                <div class="modal fade" id="confirmModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title">Xác nhận</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body">
                                ${message}
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
                                <button type="button" class="btn btn-danger" id="confirmBtn">Xóa</button>
                            </div>
                        </div>
                    </div>
                </div>`;
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    var modal = new bootstrap.Modal(document.getElementById('confirmModal'));
    modal.show();

    document.getElementById('confirmBtn').addEventListener('click', function () {
        callback();
        modal.hide();
    });

    document.getElementById('confirmModal').addEventListener('hidden.bs.modal', function () {
        this.remove();
    });
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

