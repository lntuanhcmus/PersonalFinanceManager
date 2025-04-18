using Microsoft.AspNetCore.Mvc;
using System.Linq;
using PersonalFinanceManager.Shared.Dto; // Giả sử DTO nằm trong namespace này
using PersonalFinanceManager; // Giả sử repository nằm trong namespace này
using Microsoft.AspNetCore.Authorization;

namespace PersonalFinanceManager.WebHost.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "TransactionsManagement");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
    }
}