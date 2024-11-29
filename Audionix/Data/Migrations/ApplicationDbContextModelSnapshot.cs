﻿// <auto-generated />
using System;
using Audionix.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Audionix.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Audionix.Data.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Audionix.Models.AppSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("DataPath")
                        .HasColumnType("text");

                    b.Property<bool>("IsDatapathSetup")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("AppSettings");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            DataPath = "C:\\Program Files\\Audionix\\AudionixAudio",
                            IsDatapathSetup = false
                        });
                });

            modelBuilder.Entity("Audionix.Models.AudioDevice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("DeviceID")
                        .HasColumnType("text");

                    b.Property<string>("FriendlyName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("AudioDevices");
                });

            modelBuilder.Entity("Audionix.Models.AudioMetadata", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Artist")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("AudioType")
                        .HasColumnType("integer");

                    b.Property<string>("Category")
                        .HasColumnType("text");

                    b.Property<double>("Duration")
                        .HasColumnType("double precision");

                    b.Property<int>("EndDate")
                        .HasColumnType("integer");

                    b.Property<string>("Filename")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Folder")
                        .HasColumnType("text");

                    b.Property<short>("Intro")
                        .HasColumnType("smallint");

                    b.Property<double>("IntroSeconds")
                        .HasColumnType("double precision");

                    b.Property<bool>("NoFade")
                        .HasColumnType("boolean");

                    b.Property<bool>("ProtectNextIntro")
                        .HasColumnType("boolean");

                    b.Property<short>("Segue")
                        .HasColumnType("smallint");

                    b.Property<double>("SegueSeconds")
                        .HasColumnType("double precision");

                    b.Property<int>("StartDate")
                        .HasColumnType("integer");

                    b.Property<Guid>("StationId")
                        .HasColumnType("uuid");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("StationId");

                    b.ToTable("AudioFiles");
                });

            modelBuilder.Entity("Audionix.Models.Folder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("StationId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("StationId");

                    b.ToTable("Folders");
                });

            modelBuilder.Entity("Audionix.Models.MusicSchedule.Category", b =>
                {
                    b.Property<Guid>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CategoryName")
                        .HasColumnType("text");

                    b.Property<Guid>("StationId")
                        .HasColumnType("uuid");

                    b.Property<int?>("TemplateId")
                        .HasColumnType("integer");

                    b.Property<Guid?>("TemplatePatternId")
                        .HasColumnType("uuid");

                    b.HasKey("CategoryId");

                    b.HasIndex("TemplatePatternId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("Audionix.Models.MusicSchedule.MusicGridItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Friday")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("FridayPatternId")
                        .HasColumnType("uuid");

                    b.Property<string>("Hour")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Monday")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("MondayPatternId")
                        .HasColumnType("uuid");

                    b.Property<string>("Saturday")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("SaturdayPatternId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("StationId")
                        .HasColumnType("uuid");

                    b.Property<string>("Sunday")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("SundayPatternId")
                        .HasColumnType("uuid");

                    b.Property<string>("Thursday")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("ThursdayPatternId")
                        .HasColumnType("uuid");

                    b.Property<string>("Tuesday")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("TuesdayPatternId")
                        .HasColumnType("uuid");

                    b.Property<string>("Wednesday")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("WednesdayPatternId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("MusicGridItems");
                });

            modelBuilder.Entity("Audionix.Models.MusicSchedule.MusicPattern", b =>
                {
                    b.Property<Guid>("PatternId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("StationId")
                        .HasColumnType("uuid");

                    b.HasKey("PatternId");

                    b.ToTable("MusicPatterns");
                });

            modelBuilder.Entity("Audionix.Models.MusicSchedule.PatternCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Guid>("CategoryId")
                        .HasColumnType("uuid");

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("MusicPatternId")
                        .HasColumnType("uuid");

                    b.Property<int>("MusicPatternSortOrder")
                        .HasColumnType("integer");

                    b.Property<Guid>("StationId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("MusicPatternId");

                    b.ToTable("PatternCategories");
                });

            modelBuilder.Entity("Audionix.Models.ProgramLogItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Artist")
                        .HasColumnType("text");

                    b.Property<string>("Cart")
                        .HasColumnType("text");

                    b.Property<string>("Category")
                        .HasColumnType("text");

                    b.Property<string>("Cue")
                        .HasColumnType("text");

                    b.Property<DateOnly?>("Date")
                        .HasColumnType("date");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<int?>("Device")
                        .HasColumnType("integer");

                    b.Property<string>("From")
                        .HasColumnType("text");

                    b.Property<string>("Intro")
                        .HasColumnType("text");

                    b.Property<string>("Length")
                        .HasColumnType("text");

                    b.Property<int>("LogOrderID")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Passthrough")
                        .HasColumnType("text");

                    b.Property<double>("Progress")
                        .HasColumnType("double precision");

                    b.Property<string>("Segue")
                        .HasColumnType("text");

                    b.Property<int?>("States")
                        .HasColumnType("integer");

                    b.Property<Guid>("StationId")
                        .HasColumnType("uuid");

                    b.Property<string>("Status")
                        .HasColumnType("text");

                    b.Property<TimeOnly>("TimeEstimated")
                        .HasColumnType("time");

                    b.Property<TimeOnly>("TimePlayed")
                        .HasColumnType("time");

                    b.Property<TimeOnly>("TimeScheduled")
                        .HasColumnType("time");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.Property<int?>("sID")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("StationId");

                    b.ToTable("Log");
                });

            modelBuilder.Entity("Audionix.Models.Station", b =>
                {
                    b.Property<Guid>("StationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AudioDeviceId")
                        .HasColumnType("uuid");

                    b.Property<string>("CallLetters")
                        .HasColumnType("text");

                    b.Property<int>("CurrentPlaying")
                        .HasColumnType("integer");

                    b.Property<int>("NextPlay")
                        .HasColumnType("integer");

                    b.Property<string>("Slogan")
                        .HasColumnType("text");

                    b.Property<int>("StationSortOrder")
                        .HasColumnType("integer");

                    b.HasKey("StationId");

                    b.HasIndex("AudioDeviceId");

                    b.ToTable("Stations");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

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
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

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

            modelBuilder.Entity("Audionix.Models.Station", b =>
                {
                    b.HasOne("Audionix.Models.AudioDevice", "AudioDevice")
                        .WithMany()
                        .HasForeignKey("AudioDeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AudioDevice");
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
