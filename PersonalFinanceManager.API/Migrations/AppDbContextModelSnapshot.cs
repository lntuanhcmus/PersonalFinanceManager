﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PersonalFinanceManager.Shared.Data;

#nullable disable

namespace PersonalFinanceManager.API.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("PersonalFinanceManager.Shared.Models.Budget", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("Relational:JsonPropertyName", "id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)")
                        .HasAnnotation("Relational:JsonPropertyName", "amount");

                    b.Property<int>("CategoryId")
                        .HasColumnType("int")
                        .HasAnnotation("Relational:JsonPropertyName", "categoryId");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2")
                        .HasAnnotation("Relational:JsonPropertyName", "endDate");

                    b.Property<string>("Period")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "period");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2")
                        .HasAnnotation("Relational:JsonPropertyName", "startDate");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Budgets");
                });

            modelBuilder.Entity("PersonalFinanceManager.Shared.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("Relational:JsonPropertyName", "id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "code");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "name");

                    b.Property<int>("TransactionTypeId")
                        .HasColumnType("int")
                        .HasAnnotation("Relational:JsonPropertyName", "TransactionTypeId");

                    b.HasKey("Id");

                    b.ToTable("Categories");

                    b.HasAnnotation("Relational:JsonPropertyName", "categoryId");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Code = "LU",
                            Name = "Lương",
                            TransactionTypeId = 1
                        },
                        new
                        {
                            Id = 2,
                            Code = "TH",
                            Name = "Thưởng",
                            TransactionTypeId = 1
                        },
                        new
                        {
                            Id = 3,
                            Code = "SH",
                            Name = "Sinh Hoạt",
                            TransactionTypeId = 2
                        },
                        new
                        {
                            Id = 4,
                            Code = "HT-PT",
                            Name = "Học Tập và Phát Triển",
                            TransactionTypeId = 2
                        },
                        new
                        {
                            Id = 5,
                            Code = "GT-PS",
                            Name = "Giải trí và Phát Sinh",
                            TransactionTypeId = 2
                        });
                });

            modelBuilder.Entity("PersonalFinanceManager.Shared.Models.LabelingRule", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Keyword")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TransactionTypeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("LabelingRules");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CategoryId = 3,
                            Keyword = "KOVQR",
                            TransactionTypeId = 2
                        },
                        new
                        {
                            Id = 2,
                            CategoryId = 5,
                            Keyword = "LE NGUYEN TUAN chuyen khoan",
                            TransactionTypeId = 2
                        },
                        new
                        {
                            Id = 3,
                            CategoryId = 3,
                            Keyword = "603 - 60.7 UVK",
                            TransactionTypeId = 3
                        },
                        new
                        {
                            Id = 4,
                            CategoryId = 4,
                            Keyword = "tien cau long",
                            TransactionTypeId = 2
                        },
                        new
                        {
                            Id = 5,
                            CategoryId = 3,
                            Keyword = "Tien Banh Mi",
                            TransactionTypeId = 2
                        },
                        new
                        {
                            Id = 6,
                            CategoryId = 3,
                            Keyword = "Tien com toi",
                            TransactionTypeId = 2
                        },
                        new
                        {
                            Id = 7,
                            CategoryId = 3,
                            Keyword = "Tien com trua",
                            TransactionTypeId = 2
                        },
                        new
                        {
                            Id = 8,
                            CategoryId = 5,
                            Keyword = "GT-PS",
                            TransactionTypeId = 2
                        },
                        new
                        {
                            Id = 9,
                            CategoryId = 4,
                            Keyword = "HT-PT",
                            TransactionTypeId = 2
                        },
                        new
                        {
                            Id = 10,
                            CategoryId = 3,
                            Keyword = "SH - ",
                            TransactionTypeId = 2
                        },
                        new
                        {
                            Id = 11,
                            CategoryId = 1,
                            Keyword = "Lương tháng",
                            TransactionTypeId = 1
                        });
                });

            modelBuilder.Entity("PersonalFinanceManager.Shared.Models.RepaymentTransaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SenderName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TransactionId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("TransactionTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("TransactionId");

                    b.ToTable("RepaymentTransactions");
                });

            modelBuilder.Entity("PersonalFinanceManager.Shared.Models.Transaction", b =>
                {
                    b.Property<string>("TransactionId")
                        .HasColumnType("nvarchar(450)")
                        .HasAnnotation("Relational:JsonPropertyName", "transactionId");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)")
                        .HasAnnotation("Relational:JsonPropertyName", "amount");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int")
                        .HasAnnotation("Relational:JsonPropertyName", "categoryId");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "description");

                    b.Property<string>("RecipientAccount")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "recipientAccount");

                    b.Property<string>("RecipientBank")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "recipientBank");

                    b.Property<string>("RecipientName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "recipientName");

                    b.Property<decimal>("RepaymentAmount")
                        .HasColumnType("decimal(18,2)")
                        .HasAnnotation("Relational:JsonPropertyName", "repayment");

                    b.Property<string>("SourceAccount")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "sourceAccount");

                    b.Property<int>("Status")
                        .HasColumnType("int")
                        .HasAnnotation("Relational:JsonPropertyName", "status");

                    b.Property<DateTime>("TransactionTime")
                        .HasColumnType("datetime2")
                        .HasAnnotation("Relational:JsonPropertyName", "transactionTime");

                    b.Property<int>("TransactionTypeId")
                        .HasColumnType("int")
                        .HasAnnotation("Relational:JsonPropertyName", "transactionTypeId");

                    b.HasKey("TransactionId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("TransactionTypeId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("PersonalFinanceManager.Shared.Models.TransactionType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("Relational:JsonPropertyName", "id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "code");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "name");

                    b.HasKey("Id");

                    b.ToTable("TransactionTypes");

                    b.HasAnnotation("Relational:JsonPropertyName", "transactionType");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Code = "Income",
                            Name = "Thu Nhập"
                        },
                        new
                        {
                            Id = 2,
                            Code = "Expense",
                            Name = "Chi Trả"
                        },
                        new
                        {
                            Id = 3,
                            Code = "Advance",
                            Name = "Tạm Ứng"
                        },
                        new
                        {
                            Id = 4,
                            Code = "Repayment",
                            Name = "Hoàn Trả"
                        });
                });

            modelBuilder.Entity("PersonalFinanceManager.Shared.Models.Budget", b =>
                {
                    b.HasOne("PersonalFinanceManager.Shared.Models.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("PersonalFinanceManager.Shared.Models.RepaymentTransaction", b =>
                {
                    b.HasOne("PersonalFinanceManager.Shared.Models.Transaction", "Transaction")
                        .WithMany("RepaymentTransactions")
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("PersonalFinanceManager.Shared.Models.Transaction", b =>
                {
                    b.HasOne("PersonalFinanceManager.Shared.Models.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId");

                    b.HasOne("PersonalFinanceManager.Shared.Models.TransactionType", "TransactionType")
                        .WithMany()
                        .HasForeignKey("TransactionTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("TransactionType");
                });

            modelBuilder.Entity("PersonalFinanceManager.Shared.Models.Transaction", b =>
                {
                    b.Navigation("RepaymentTransactions");
                });
#pragma warning restore 612, 618
        }
    }
}
