using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.ML.Models;
using System.Text.RegularExpressions;

namespace PersonalFinanceManager.Shared.Helpers
{
    public static class MLDataHelper
    {
        public static IEnumerable<TransactionData> LoadTrainingData(AppDbContext dbContext)
        {
            try
            {
                // Lấy dữ liệu từ Transactions
                Console.WriteLine("Loading transactions...");
                var validTypeIds = new HashSet<int> { 1, 2, 3, 4 };
                var transactions = dbContext.Transactions
                    .Where(t => t.Amount != null && t.CategoryId != 0 && validTypeIds.Contains(t.TransactionTypeId))
                    .Select(t => new TransactionData
                    {
                        Description = CleanText(t.Description) ?? "Unknown",
                        Amount = (float)t.Amount,
                        TransactionTime = t.TransactionTime.ToString("yyyy-MM-dd"),
                        CategoryId = t.CategoryId.ToString(),
                        TransactionTypeId = t.TransactionTypeId.ToString(),
                        DayOfWeek = t.TransactionTime.DayOfWeek.ToString(),
                        HourOfDay = t.TransactionTime.Hour
                    }).ToList()
                    .Where(t => !float.IsNaN(t.Amount) && !float.IsInfinity(t.Amount));
                Console.WriteLine($"Loaded {transactions.Count()} transactions");

                // Kiểm tra Description
                var emptyDescriptions = transactions.Count(t => t.Description == "Unknown");
                if (emptyDescriptions > 0)
                {
                    Console.WriteLine($"Warning: {emptyDescriptions} transactions have empty or null Description, replaced with 'Unknown'.");
                }

                // Lấy dữ liệu từ TransactionCorrections
                Console.WriteLine("Loading corrections...");
                var corrections = dbContext.TransactionCorrections
                    .Where(c => c.Amount != null && c.CategoryId != 0 && c.TransactionTypeId != 0)
                    .Select(c => new TransactionData
                    {
                        Description = CleanText(c.Description) ?? "Unknown",
                        Amount = (float)c.Amount,
                        TransactionTime = c.TransactionTime.ToString("yyyy-MM-dd"),
                        CategoryId = c.CategoryId.ToString(),
                        TransactionTypeId = c.TransactionTypeId.ToString(),
                        DayOfWeek = c.TransactionTime.DayOfWeek.ToString(),
                        HourOfDay = c.TransactionTime.Hour
                    }).ToList()
                    .Where(c => !float.IsNaN(c.Amount) && !float.IsInfinity(c.Amount));
                Console.WriteLine($"Loaded {corrections.Count()} corrections");

                // Lấy dữ liệu từ LabelingRules
                Console.WriteLine("Loading labeling rules...");
                var rules = dbContext.LabelingRules
                    .Where(r => !string.IsNullOrEmpty(r.Keyword))
                    .ToList();
                Console.WriteLine($"Found {rules.Count} labeling rules");

                var ruleBasedData = new List<TransactionData>();
                foreach (var rule in rules)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        ruleBasedData.Add(new TransactionData
                        {
                            Description = CleanText($"{rule.Keyword} sample {i}") ?? "Unknown",
                            Amount = 100000 + i * 10000,
                            TransactionTime = DateTime.Now.AddDays(-i).ToString("yyyy-MM-dd"),
                            CategoryId = rule.CategoryId.ToString(),
                            TransactionTypeId = rule.TransactionTypeId.ToString(),
                            DayOfWeek = DateTime.Now.AddDays(-i).DayOfWeek.ToString(),
                            HourOfDay = 10 + i
                        });
                    }
                }
                Console.WriteLine($"Generated {ruleBasedData.Count} rule-based samples");

                // Kết hợp dữ liệu
                var allData = transactions.Concat(corrections).Concat(ruleBasedData).ToList();
                if (!allData.Any())
                {
                    throw new InvalidOperationException("No valid data for training.");
                }
                Console.WriteLine($"Total {allData.Count()} records before oversampling");

                // Kiểm tra phân phối nhãn
                var categoryCounts = allData.GroupBy(d => d.CategoryId)
                    .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                    .OrderBy(g => g.Count)
                    .ToList();
                Console.WriteLine("Category distribution before oversampling:");
                foreach (var c in categoryCounts)
                {
                    Console.WriteLine($"  CategoryId {c.CategoryId}: {c.Count} samples");
                }

                var typeCounts = allData.GroupBy(d => d.TransactionTypeId)
                    .Select(g => new { TypeId = g.Key, Count = g.Count() })
                    .OrderBy(g => g.Count)
                    .ToList();
                Console.WriteLine("TransactionType distribution before oversampling:");
                foreach (var t in typeCounts)
                {
                    Console.WriteLine($"  TransactionTypeId {t.TypeId}: {t.Count} samples");
                }

                // Oversampling
                var minSampleThreshold = Math.Max(100, categoryCounts.Any() ? (int)(categoryCounts.Average(c => c.Count) * 0.5) : 100);
                var oversampledData = new List<TransactionData>(allData);
                var random = new Random();

                foreach (var group in allData.GroupBy(d => d.CategoryId))
                {
                    var samples = group.ToList();
                    if (samples.Count < minSampleThreshold)
                    {
                        int samplesToAdd = minSampleThreshold - samples.Count;
                        for (int i = 0; i < samplesToAdd; i++)
                        {
                            var sample = samples[random.Next(samples.Count)];
                            oversampledData.Add(new TransactionData
                            {
                                Description = sample.Description + $" (oversampled {i})",
                                Amount = sample.Amount * (float)(1 + random.NextDouble() * 0.1 - 0.05),
                                TransactionTime = sample.TransactionTime,
                                CategoryId = sample.CategoryId,
                                TransactionTypeId = sample.TransactionTypeId,
                                DayOfWeek = sample.DayOfWeek,
                                HourOfDay = sample.HourOfDay
                            });
                        }
                    }
                }

                foreach (var group in allData.GroupBy(d => d.TransactionTypeId))
                {
                    var samples = group.ToList();
                    if (samples.Count < minSampleThreshold)
                    {
                        int samplesToAdd = minSampleThreshold - samples.Count;
                        for (int i = 0; i < samplesToAdd; i++)
                        {
                            var sample = samples[random.Next(samples.Count)];
                            oversampledData.Add(new TransactionData
                            {
                                Description = sample.Description + $" (oversampled {i})",
                                Amount = sample.Amount * (float)(1 + random.NextDouble() * 0.1 - 0.05),
                                TransactionTime = sample.TransactionTime,
                                CategoryId = sample.CategoryId,
                                TransactionTypeId = sample.TransactionTypeId,
                                DayOfWeek = sample.DayOfWeek,
                                HourOfDay = sample.HourOfDay
                            });
                        }
                    }
                }

                Console.WriteLine($"After oversampling: {oversampledData.Count()} records");

                // Kiểm tra phân phối sau oversampling
                categoryCounts = oversampledData.GroupBy(d => d.CategoryId)
                    .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                    .OrderBy(g => g.Count)
                    .ToList();
                Console.WriteLine("Category distribution after oversampling:");
                foreach (var c in categoryCounts)
                {
                    Console.WriteLine($"  CategoryId {c.CategoryId}: {c.Count} samples");
                }

                typeCounts = oversampledData.GroupBy(d => d.TransactionTypeId)
                    .Select(g => new { TypeId = g.Key, Count = g.Count() })
                    .OrderBy(g => g.Count)
                    .ToList();
                Console.WriteLine("TransactionType distribution after oversampling:");
                foreach (var t in typeCounts)
                {
                    Console.WriteLine($"  TransactionTypeId {t.TypeId}: {t.Count} samples");
                }

                return oversampledData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoadTrainingData: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public static string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            text = text.ToLower();
            text = Regex.Replace(text, "[^a-z0-9\\s]", "");
            text = Regex.Replace(text, "\\s+", " ").Trim();
            return text;
        }
    }
}