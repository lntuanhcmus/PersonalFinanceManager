using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PersonalFinanceManager.WebHost.Helper
{
    public static class EnumHelper
    {
        /// <summary>
        /// Lấy display name từ enum value
        /// </summary>
        public static string GetDisplayName<TEnum>(TEnum value) where TEnum : Enum
        {
            var field = typeof(TEnum).GetField(value.ToString());
            var displayAttr = field?.GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.Name ?? value.ToString();
        }

        /// <summary>
        /// Lấy display name từ chuỗi string (ví dụ: "Monthly" => "Hàng Tháng")
        /// </summary>
        public static string GetDisplayNameFromString<TEnum>(string value) where TEnum : Enum
        {
            if (string.IsNullOrEmpty(value)) return value;

            try
            {
                var enumValue = (TEnum)Enum.Parse(typeof(TEnum), value);
                return GetDisplayName(enumValue);
            }
            catch
            {
                return value;
            }
        }

        /// <summary>
        /// Tạo danh sách SelectListItem từ enum
        /// </summary>
        public static List<SelectListItem> GetSelectList<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = GetDisplayName(e)
                })
                .ToList();
        }
    }

}
