﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Infrastructure.Data;
using PersonalFinanceManager.Shared.Data;

namespace PersonalFinanceManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriesApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CategoriesApiController(AppDbContext context)
        {
            _context = context;
        }
        // GET: api/budgets
        [HttpGet]
        public ActionResult<IEnumerable<Category>> GetCategories()
        {
            try
            {
                var categories = _context.Categories.ToList();

                return Ok(categories);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpGet("{id}")]
        public ActionResult<IEnumerable<Category>> GetCategoryById(int id)
        {
            try
            {
                var category = _context.Categories.FirstOrDefault(c => c.Id == id);

                return Ok(category);

            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
