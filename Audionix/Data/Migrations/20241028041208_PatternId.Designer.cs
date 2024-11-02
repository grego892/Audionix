﻿// <auto-generated />
using System;
using Audionix.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Audionix.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241028041208_PatternId")]
    partial class PatternId
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("Audionix.Data.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("TEXT");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("TEXT");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Audionix.Models.AudioMetadata", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Artist")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Duration")
                        .HasColumnType("REAL");

                    b.Property<int>("EndDate")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Filename")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Folder")
                        .HasColumnType("TEXT");

                    b.Property<short>("Intro")
                        .HasColumnType("INTEGER");

                    b.Property<double>("IntroSeconds")
                        .HasColumnType("REAL");

                    b.Property<bool>("NoFade")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ProtectNextIntro")
                        .HasColumnType("INTEGER");

                    b.Property<short>("Segue")
                        .HasColumnType("INTEGER");

                    b.Property<double>("SegueSeconds")
                        .HasColumnType("REAL");

                    b.Property<string>("SelectedCategory")
                        .HasColumnType("TEXT");

                    b.Property<int>("StartDate")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("StationId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("StationId");

                    b.ToTable("AudioFiles");
                });

            modelBuilder.Entity("Audionix.Models.Folder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("StationId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("StationId");

                    b.ToTable("Folders");
                });

            modelBuilder.Entity("Audionix.Models.MusicSchedule.Category", b =>
                {
                    b.Property<Guid>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("CategoryName")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("StationId")
                        .HasColumnType("TEXT");

                    b.Property<int?>("TemplateId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("TemplatePatternId")
                        .HasColumnType("TEXT");

                    b.HasKey("CategoryId");

                    b.HasIndex("TemplatePatternId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("Audionix.Models.MusicSchedule.Grid", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Grids");
                });

            modelBuilder.Entity("Audionix.Models.MusicSchedule.MusicPattern", b =>
                {
                    b.Property<Guid>("PatternId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("StationId")
                        .HasColumnType("TEXT");

                    b.HasKey("PatternId");

                    b.ToTable("MusicPatterns");
                });

            modelBuilder.Entity("Audionix.Models.MusicSchedule.PatternCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("CategoryId")
                        .HasColumnType("TEXT");

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("MusicPatternId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("MusicPatternId");

                    b.ToTable("PatternCategories");
                });

            modelBuilder.Entity("Audionix.Models.ProgramLogItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Actual")
                        .HasColumnType("TEXT");

                    b.Property<string>("Artist")
                        .HasColumnType("TEXT");

                    b.Property<string>("Cart")
                        .HasColumnType("TEXT");

                    b.Property<string>("Category")
                        .HasColumnType("TEXT");

                    b.Property<string>("Cue")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<int?>("Device")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Estimated")
                        .HasColumnType("TEXT");

                    b.Property<string>("From")
                        .HasColumnType("TEXT");

                    b.Property<string>("Length")
                        .HasColumnType("TEXT");

                    b.Property<int>("LogID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Passthrough")
                        .HasColumnType("TEXT");

                    b.Property<double>("Progress")
                        .HasColumnType("REAL");

                    b.Property<string>("Scheduled")
                        .HasColumnType("TEXT");

                    b.Property<string>("Segue")
                        .HasColumnType("TEXT");

                    b.Property<string>("States")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("StationId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<int?>("sID")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("StationId");

                    b.ToTable("Log");
                });

            modelBuilder.Entity("Audionix.Models.Station", b =>
                {
                    b.Property<Guid>("StationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("CallLetters")
                        .HasColumnType("TEXT");

                    b.Property<string>("Slogan")
                        .HasColumnType("TEXT");

                    b.Property<int>("StationSortOrder")
                        .HasColumnType("INTEGER");

                    b.HasKey("StationId");

                    b.ToTable("Stations");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Audionix.Models.AudioMetadata", b =>
                {
                    b.HasOne("Audionix.Models.Station", "Station")
                        .WithMany("AudioFiles")
                        .HasForeignKey("StationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Station");
                });

            modelBuilder.Entity("Audionix.Models.Folder", b =>
                {
                    b.HasOne("Audionix.Models.Station", "Station")
                        .WithMany("Folders")
                        .HasForeignKey("StationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Station");
                });

            modelBuilder.Entity("Audionix.Models.MusicSchedule.Category", b =>
                {
                    b.HasOne("Audionix.Models.MusicSchedule.MusicPattern", "Template")
                        .WithMany()
                        .HasForeignKey("TemplatePatternId");

                    b.Navigation("Template");
                });

            modelBuilder.Entity("Audionix.Models.MusicSchedule.PatternCategory", b =>
                {
                    b.HasOne("Audionix.Models.MusicSchedule.Category", "Category")
                        .WithMany("PatternCategories")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Audionix.Models.MusicSchedule.MusicPattern", "MusicPattern")
                        .WithMany("PatternCategories")
                        .HasForeignKey("MusicPatternId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("MusicPattern");
                });

            modelBuilder.Entity("Audionix.Models.ProgramLogItem", b =>
                {
                    b.HasOne("Audionix.Models.Station", "Station")
                        .WithMany("ProgramLogItems")
                        .HasForeignKey("StationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Station");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Audionix.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Audionix.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Audionix.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Audionix.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Audionix.Models.MusicSchedule.Category", b =>
                {
                    b.Navigation("PatternCategories");
                });

            modelBuilder.Entity("Audionix.Models.MusicSchedule.MusicPattern", b =>
                {
                    b.Navigation("PatternCategories");
                });

            modelBuilder.Entity("Audionix.Models.Station", b =>
                {
                    b.Navigation("AudioFiles");

                    b.Navigation("Folders");

                    b.Navigation("ProgramLogItems");
                });
#pragma warning restore 612, 618
        }
    }
}