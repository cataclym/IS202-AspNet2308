﻿// <auto-generated />
using System;
using Kartverket.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Kartverket.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241024102026_Nullable phone")]
    partial class Nullablephone
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

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

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("GeoJsonString")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("varchar(2000)");

                    b.Property<DateTime?>("ResolvedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Status")
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

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(45)
                        .HasColumnType("varchar(45)");

                    b.Property<string>("Phone")
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(45)
                        .HasColumnType("varchar(45)");

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
