using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PersonalFinanceManager.WebHost.Controllers
{
    [Authorize]
    public class GmailManagementController : Controller
    {
        public IActionResult Callback(string success, string error)
        {
            if (!string.IsNullOrEmpty(success) && success == "true")
            {
                TempData["SuccessMessage"] = "Đã kết nối Gmail thành công!";
            }
            else if (!string.IsNullOrEmpty(error))
            {
                TempData["ErrorMessage"] = $"Lỗi khi kết nối Gmail: {error}";
            }
            else
            {
                TempData["ErrorMessage"] = "Xác thực Gmail không thành công.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
