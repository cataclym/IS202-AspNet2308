﻿// <auto-generated />
using System;
using Kartverket.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Kartverket.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("Kartverket.Database.Models.Messages", b =>
                {
                    b.Property<int>("MessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("MessageId"));

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("ReportId")
                        .HasColumnType("int");

                    b.Property<int?>("ReportsReportId")
                        .HasColumnType("int");

                    b.HasKey("MessageId");

                    b.HasIndex("ReportsReportId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Kartverket.Database.Models.Reports", b =>
                {
                    b.Property<int>("ReportId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("ReportId"));

                    b.Property<string>("GeoJsonString")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int?>("UsersUserId")
                        .HasColumnType("int");

                    b.HasKey("ReportId");

                    b.HasIndex("UsersUserId");

                    b.ToTable("Reports");
                });

            modelBuilder.Entity("Kartverket.Database.Models.Users", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("UserId"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<bool>("isAdmin")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("UserId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Kartverket.Database.Models.Messages", b =>
                {
                    b.HasOne("Kartverket.Database.Models.Reports", null)
                        .WithMany("Messages")
                        .HasForeignKey("ReportsReportId");
                });

            modelBuilder.Entity("Kartverket.Database.Models.Reports", b =>
                {
                    b.HasOne("Kartverket.Database.Models.Users", null)
                        .WithMany("MapReports")
                        .HasForeignKey("UsersUserId");
                });

            modelBuilder.Entity("Kartverket.Database.Models.Reports", b =>
                {
                    b.Navigation("Messages");
                });

            modelBuilder.Entity("Kartverket.Database.Models.Users", b =>
                {
                    b.Navigation("MapReports");
                });
#pragma warning restore 612, 618
        }
    }
}
