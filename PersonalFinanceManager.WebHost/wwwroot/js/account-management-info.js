document.addEventListener('DOMContentLoaded', function () {
    // Xử lý nút Kết nối Gmail
    const connectButton = document.getElementById('connectGmailBtn');
    if (connectButton) {
        connectButton.addEventListener('click', async function () {
            const button = this;
            const spinner = document.getElementById('gmailSpinnerConnect');
            button.disabled = true;
            spinner.style.display = 'inline-block';

            try {
                const response = await fetch('/AccountManagement/InitiateOAuth', {
                    method: 'GET'
                });

                if (!response.ok) {
                    const errorData = await response.json();
                    throw new Error(errorData.error || `Lỗi từ server: ${response.status}`);
                }

                const result = await response.json();
                if (result.authUrl) {
                    window.location.href = result.authUrl; // Chuyển hướng đến Google OAuth
                } else {
                    throw new Error('Không lấy được URL xác thực');
                }
            } catch (error) {
                alert('Lỗi khi kết nối Gmail: ' + error.message);
                button.disabled = false;
                spinner.style.display = 'none';
            }
        });
    }

    // Xử lý nút Dừng kết nối Gmail
    const disconnectButton = document.getElementById('disConnectGmailBtn');
    if (disconnectButton) {
        disconnectButton.addEventListener('click', async function () {
            const button = this;
            const spinner = document.getElementById('gmailSpinnerDisconnect');
            button.disabled = true;
            spinner.style.display = 'inline-block';

            try {
                const response = await fetch('/AccountManagement/DisconnectGmail', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                });

                if (!response.ok) {
                    const errorData = await response.json();
                    throw new Error(errorData.error || `Lỗi từ server: ${response.status}`);
                }

                // Tải lại trang để cập nhật trạng thái
                window.location.reload();
            } catch (error) {
                alert('Lỗi khi dừng kết nối Gmail: ' + error.message);
                button.disabled = false;
                spinner.style.display = 'none';
            }
        });
    }
});