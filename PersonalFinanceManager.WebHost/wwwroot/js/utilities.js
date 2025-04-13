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