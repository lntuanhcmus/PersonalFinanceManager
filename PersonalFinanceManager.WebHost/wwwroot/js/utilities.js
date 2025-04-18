// wwwroot/js/utilities.js
window.Utilities = window.Utilities || {};

// Cấu hình domain
Utilities.config = {
    apiBaseUrl: 'http://localhost:8000'
};

// Các hàm tiện ích khác
Utilities.initializeFlatpickr = function () {
    flatpickr(".datetimepicker", {
        enableTime: true,
        dateFormat: "d/m/Y H:i",
        time_24hr: true,
        minuteIncrement: 1,
        defaultDate: document.getElementById('TransactionTime')?.value
    });

    flatpickr(".datetimepicker-modal", {
        enableTime: true,
        dateFormat: "d/m/Y H:i",
        time_24hr: true,
        minuteIncrement: 1
    });
};

Utilities.initializeFormValidation = function () {
    const forms = document.querySelectorAll('.needs-validation');
    Array.from(forms).forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });
};

var Utilities = Utilities || {};

Utilities.showConfirmationModal = function (message, callback, options = {}) {
    // Mặc định các giá trị
    var modalId = `confirmationModal-${Date.now()}`;
    var confirmButtonId = `confirmButton-${Date.now()}`;
    var cancelButtonId = `cancelButton-${Date.now()}`;
    var title = options.title || 'Xác nhận';
    var confirmText = options.confirmText || 'Xác nhận';
    var confirmClass = options.confirmClass || 'btn-primary';
    var cancelText = options.cancelText || 'Hủy';
    var cancelClass = options.cancelClass || 'btn-secondary';

    // Tạo HTML cho modal
    var modalHtml = `
        <style>
            .custom-confirm-modal .modal-content {
                border-radius: 12px;
                box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
                border: none;
                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            }
            .custom-confirm-modal .modal-header {
                border-bottom: 1px solid #e9ecef;
                padding: 1.5rem;
                background-color: #f8f9fa;
                border-top-left-radius: 12px;
                border-top-right-radius: 12px;
            }
            .custom-confirm-modal .modal-title {
                font-size: 1.25rem;
                font-weight: 600;
                color: #1a1a1a;
            }
            .custom-confirm-modal .modal-body {
                padding: 1.5rem;
                font-size: 1rem;
                color: #495057;
                line-height: 1.6;
            }
            .custom-confirm-modal .modal-footer {
                border-top: 1px solid #e9ecef;
                padding: 1rem 1.5rem;
                display: flex;
                gap: 0.5rem;
                justify-content: flex-end;
            }
            .custom-confirm-modal .btn {
                padding: 0.75rem 1.5rem;
                border-radius: 8px;
                font-size: 0.95rem;
                font-weight: 500;
                transition: all 0.2s ease;
            }
            .custom-confirm-modal .btn-danger {
                background-color: #dc3545;
                border-color: #dc3545;
            }
            .custom-confirm-modal .btn-danger:hover {
                background-color: #c82333;
                border-color: #bd2130;
                transform: translateY(-1px);
            }
            .custom-confirm-modal .btn-outline-secondary {
                border-color: #6c757d;
                color: #6c757d;
            }
            .custom-confirm-modal .btn-outline-secondary:hover {
                background-color: #6c757d;
                color: #fff;
                transform: translateY(-1px);
            }
            .custom-confirm-modal .btn-close {
                opacity: 0.8;
                transition: opacity 0.2s ease;
            }
            .custom-confirm-modal .btn-close:hover {
                opacity: 1;
            }
            .custom-confirm-modal .btn i {
                margin-right: 0.5rem;
            }
        </style>
        <div class="modal fade custom-confirm-modal" id="${modalId}" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">${title}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        ${message}
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn ${cancelClass}" id="${cancelButtonId}" data-bs-dismiss="modal">
                            <i class="bi bi-x-circle"></i> ${cancelText}
                        </button>
                        <button type="button" class="btn ${confirmClass}" id="${confirmButtonId}">
                            <i class="bi bi-check-circle"></i> ${confirmText}
                        </button>
                    </div>
                </div>
            </div>
        </div>`;

    // Chèn modal vào DOM
    document.body.insertAdjacentHTML('beforeend', modalHtml);

    // Khởi tạo và hiển thị modal
    var modalElement = document.getElementById(modalId);
    var modal = new bootstrap.Modal(modalElement);
    modal.show();

    // Gắn sự kiện cho nút xác nhận
    document.getElementById(confirmButtonId).addEventListener('click', function () {
        callback();
        modal.hide();
    });

    // Xóa modal khi đóng
    modalElement.addEventListener('hidden.bs.modal', function () {
        this.remove();
    });
};