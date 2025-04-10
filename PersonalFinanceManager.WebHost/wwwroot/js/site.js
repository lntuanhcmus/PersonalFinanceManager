// Hàm hiển thị modal với nội dung và callback khi xác nhận
function showConfirmationModal(content, onConfirm) {
    // Cập nhật nội dung modal
    document.querySelector('#confirmationModal .modal-body').innerText = content;

    // Hiển thị modal
    var modal = new bootstrap.Modal(document.getElementById('confirmationModal'));
    modal.show();

    // Xử lý nút Xác nhận
    var confirmButton = document.getElementById('confirmButton');
    confirmButton.onclick = function () {
        if (onConfirm) onConfirm(); // Thực thi callback
        modal.hide(); // Đóng modal sau khi xác nhận
    };
}

// Đảm bảo modal đóng không giữ lại onclick cũ
document.getElementById('confirmationModal').addEventListener('hidden.bs.modal', function () {
    document.getElementById('confirmButton').onclick = null;
});