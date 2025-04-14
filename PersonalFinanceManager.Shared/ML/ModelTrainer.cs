using Microsoft.ML;
using Microsoft.ML.Data;
using PersonalFinanceManager.Shared.Constants;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Data.Entity;
using PersonalFinanceManager.Shared.Helpers;
using PersonalFinanceManager.Shared.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PersonalFinanceManager.Shared.ML
{
    public class ModelTrainer
    {
        public static void UpdateModel(AppDbContext dbContext, string categoryModelPath, string typeModelPath)
        {
            try
            {
                var mlContext = new MLContext(seed: 0);
                Console.WriteLine("Loading new training data...");
                var newData = MLDataHelper.LoadTrainingData(dbContext).ToList();

                if (!newData.Any())
                {
                    Console.WriteLine("No new data to process.");
                    return;
                }

                // Cập nhật mô hình Category
                UpdateCategoryModel(mlContext, newData, categoryModelPath);

                // Cập nhật mô hình Type
                UpdateTypeModel(mlContext, newData, typeModelPath);

                Console.WriteLine("Model update completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateModel: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private static void UpdateCategoryModel(MLContext mlContext, List<TransactionData> newData, string categoryModelPath)
        {
            var dataView = mlContext.Data.LoadFromEnumerable(newData);
            var categoryPipeline = mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "DescriptionFeatures", inputColumnName: "Description")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(
                    outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(mlContext.Transforms.Concatenate(
                    outputColumnName: "Features", "DescriptionFeatures", "Amount", "DayOfWeekEncoded", "HourOfDay"))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "CategoryKey", inputColumnName: "CategoryId"));

            // Tải mô hình hiện tại (nếu có)
            ITransformer categoryModel = LoadModel(mlContext, categoryModelPath);

            // Nếu mô hình không tồn tại, tạo mô hình mới
            if (categoryModel == null)
            {
                Console.WriteLine("Training a new category model...");
                categoryModel = categoryPipeline.Fit(dataView);
            }
            else
            {
                Console.WriteLine("Updating category model with new data...");
                categoryModel = categoryPipeline.Fit(dataView);  // Cập nhật mô hình với dữ liệu mới
            }

            // Lưu mô hình đã cập nhật
            mlContext.Model.Save(categoryModel, dataView.Schema, categoryModelPath);
        }

        private static void UpdateTypeModel(MLContext mlContext, List<TransactionData> newData, string typeModelPath)
        {
            var dataView = mlContext.Data.LoadFromEnumerable(newData);
            var typePipeline = mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "DescriptionFeatures", inputColumnName: "Description")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(
                    outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(mlContext.Transforms.Concatenate(
                    outputColumnName: "Features", "DescriptionFeatures", "Amount", "DayOfWeekEncoded", "HourOfDay"))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "TypeKey", inputColumnName: "TransactionTypeId"));

            // Tải mô hình hiện tại (nếu có)
            ITransformer typeModel = LoadModel(mlContext, typeModelPath);

            // Nếu mô hình không tồn tại, tạo mô hình mới
            if (typeModel == null)
            {
                Console.WriteLine("Training a new transaction type model...");
                typeModel = typePipeline.Fit(dataView);
            }
            else
            {
                Console.WriteLine("Updating transaction type model with new data...");
                typeModel = typePipeline.Fit(dataView);  // Cập nhật mô hình với dữ liệu mới
            }

            // Lưu mô hình đã cập nhật
            mlContext.Model.Save(typeModel, dataView.Schema, typeModelPath);
        }

        private static ITransformer LoadModel(MLContext mlContext, string modelPath)
        {
            try
            {
                return mlContext.Model.Load(modelPath, out _);
            }
            catch (Exception)
            {
                Console.WriteLine($"No model found at {modelPath}, creating a new one.");
                return null;
            }
        }


    }

    public class TransactionDataWeighted
    {
        public string Description { get; set; }
        public float Amount { get; set; }
        public string TransactionTime { get; set; }
        public string DayOfWeek { get; set; }
        public float HourOfDay { get; set; }
        public string CategoryId { get; set; }
        public string TransactionTypeId { get; set; }
        public float CategoryWeight { get; set; }
        public float TypeWeight { get; set; }
    }
}
