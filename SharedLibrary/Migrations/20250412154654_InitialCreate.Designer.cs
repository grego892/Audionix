﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SharedLibrary.Data;

#nullable disable

namespace SharedLibrary.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250412154654_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

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

            modelBuilder.Entity("SharedLibrary.Data.ApplicationUser", b =>
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

            modelBuilder.Entity("SharedLibrary.Models.AppSettings", b =>
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
                            DataPath = "C:\\AudionixAudio",
                            IsDatapathSetup = false
                        });
                });

            modelBuilder.Entity("SharedLibrary.Models.AudioMetadata", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Artist")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("integer");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("interval");

                    b.Property<int>("EndDate")
                        .HasColumnType("integer");

                    b.Property<int?>("EnergyLevelId")
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

                    b.Property<bool>("ProtectNextIntro")
                        .HasColumnType("boolean");

                    b.Property<short>("Segue")
                        .HasColumnType("smallint");

                    b.Property<double>("SegueSeconds")
                        .HasColumnType("double precision");

                    b.Property<int?>("SoundCodeId")
                        .HasColumnType("integer");

                    b.Property<int>("StartDate")
                        .HasColumnType("integer");

                    b.Property<int>("StationId")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("EnergyLevelId");

                    b.HasIndex("SoundCodeId");

                    b.HasIndex("StationId");

                    b.ToTable("AudioFiles");
                });

            modelBuilder.Entity("SharedLibrary.Models.Folder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int>("StationId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("StationId");

                    b.ToTable("Folders");
                });

            modelBuilder.Entity("SharedLibrary.Models.MusicSchedule.Category", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CategoryId"));

                    b.Property<string>("CategoryName")
                        .HasColumnType("text");

                    b.Property<int>("StationId")
                        .HasColumnType("integer");

                    b.Property<int?>("TemplateId")
                        .HasColumnType("integer");

                    b.HasKey("CategoryId");

                    b.HasIndex("TemplateId");

                    b.ToTable("Category");
                });

            modelBuilder.Entity("SharedLibrary.Models.MusicSchedule.MusicGridItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Friday")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("FridayPatternId")
                        .HasColumnType("integer");

                    b.Property<string>("Hour")
                        .HasColumnType("text");

                    b.Property<string>("Monday")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("MondayPatternId")
                        .HasColumnType("integer");

                    b.Property<string>("Saturday")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("SaturdayPatternId")
                        .HasColumnType("integer");

                    b.Property<int>("StationId")
                        .HasColumnType("integer");

                    b.Property<string>("Sunday")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("SundayPatternId")
                        .HasColumnType("integer");

                    b.Property<string>("Thursday")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("ThursdayPatternId")
                        .HasColumnType("integer");

                    b.Property<string>("Tuesday")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("TuesdayPatternId")
                        .HasColumnType("integer");

                    b.Property<string>("Wednesday")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("WednesdayPatternId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("MusicGridItems");
                });

            modelBuilder.Entity("SharedLibrary.Models.MusicSchedule.MusicPattern", b =>
                {
                    b.Property<int>("PatternId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("PatternId"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("StationId")
                        .HasColumnType("integer");

                    b.HasKey("PatternId");

                    b.ToTable("MusicPatterns");
                });

            modelBuilder.Entity("SharedLibrary.Models.MusicSchedule.PatternCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("integer");

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("MusicPatternId")
                        .HasColumnType("integer");

                    b.Property<int>("MusicPatternSortOrder")
                        .HasColumnType("integer");

                    b.Property<int>("StationId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("MusicPatternId");

                    b.ToTable("PatternCategories");
                });

            modelBuilder.Entity("SharedLibrary.Models.MusicSchedule.Rules.EnergyLevel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Level")
                        .HasColumnType("text");

                    b.Property<int>("StationId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("EnergyLevels");
                });

            modelBuilder.Entity("SharedLibrary.Models.MusicSchedule.Rules.SongScheduleSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ArtistSeperation")
                        .HasColumnType("integer");

                    b.Property<int>("MaxEnergySeperation")
                        .HasColumnType("integer");

                    b.Property<int>("MaxSoundcodeSeperation")
                        .HasColumnType("integer");

                    b.Property<int>("TitleSeperation")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("SongScheduleSettings");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            ArtistSeperation = 10,
                            MaxEnergySeperation = 3,
                            MaxSoundcodeSeperation = 3,
                            TitleSeperation = 10
                        });
                });

            modelBuilder.Entity("SharedLibrary.Models.MusicSchedule.Rules.SoundCode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<int>("StationId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("SoundCodes");
                });

            modelBuilder.Entity("SharedLibrary.Models.ProgramLogItem", b =>
                {
                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<int>("LogOrderID")
                        .HasColumnType("integer");

                    b.Property<string>("Artist")
                        .HasColumnType("text");

                    b.Property<string>("Cue")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<int?>("Device")
                        .HasColumnType("integer");

                    b.Property<string>("From")
                        .HasColumnType("text");

                    b.Property<short>("Intro")
                        .HasColumnType("smallint");

                    b.Property<TimeSpan>("Length")
                        .HasColumnType("interval");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Passthrough")
                        .HasColumnType("text");

                    b.Property<double>("Progress")
                        .HasColumnType("double precision");

                    b.Property<int?>("RotatorID")
                        .HasColumnType("integer");

                    b.Property<short>("Segue")
                        .HasColumnType("smallint");

                    b.Property<string>("SongCategory")
                        .HasColumnType("text");

                    b.Property<int?>("SoundCodeId")
                        .HasColumnType("integer");

                    b.Property<int>("StationId")
                        .HasColumnType("integer");

                    b.Property<int?>("Status")
                        .HasColumnType("integer");

                    b.Property<TimeOnly>("TimeEstimated")
                        .HasColumnType("time");

                    b.Property<TimeOnly>("TimePlayed")
                        .HasColumnType("time");

                    b.Property<TimeOnly>("TimeScheduled")
                        .HasColumnType("time");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.HasKey("Date", "LogOrderID");

                    b.HasIndex("RotatorID");

                    b.HasIndex("SoundCodeId");

                    b.HasIndex("StationId");

                    b.ToTable("Log");
                });

            modelBuilder.Entity("SharedLibrary.Models.Rotator", b =>
                {
                    b.Property<int>("RotatorID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("RotatorID"));

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<short>("Intro")
                        .HasColumnType("smallint");

                    b.Property<double>("Length")
                        .HasColumnType("double precision");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("RotatorArtist")
                        .HasColumnType("text");

                    b.Property<string>("RotatorTitle")
                        .HasColumnType("text");

                    b.Property<short>("Segue")
                        .HasColumnType("smallint");

                    b.Property<int>("StationId")
                        .HasColumnType("integer");

                    b.HasKey("RotatorID");

                    b.ToTable("Rotator");
                });

            modelBuilder.Entity("SharedLibrary.Models.Station", b =>
                {
                    b.Property<int>("StationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("StationId"));

                    b.Property<string>("AudioDeviceId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CallLetters")
                        .HasColumnType("text");

                    b.Property<DateOnly?>("CurrentPlayingDate")
                        .HasColumnType("date");

                    b.Property<int>("CurrentPlayingId")
                        .HasColumnType("integer");

                    b.Property<DateOnly?>("NextPlayDate")
                        .HasColumnType("date");

                    b.Property<int>("NextPlayId")
                        .HasColumnType("integer");

                    b.Property<string>("Slogan")
                        .HasColumnType("text");

                    b.Property<int>("StationSortOrder")
                        .HasColumnType("integer");

                    b.HasKey("StationId");

                    b.ToTable("Stations");
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
                    b.HasOne("SharedLibrary.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("SharedLibrary.Data.ApplicationUser", null)
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

                    b.HasOne("SharedLibrary.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("SharedLibrary.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SharedLibrary.Models.AudioMetadata", b =>
                {
                    b.HasOne("SharedLibrary.Models.MusicSchedule.Category", "SongCategory")
                        .WithMany()
                        .HasForeignKey("CategoryId");

                    b.HasOne("SharedLibrary.Models.MusicSchedule.Rules.EnergyLevel", "EnergyLevel")
                        .WithMany()
                        .HasForeignKey("EnergyLevelId");

                    b.HasOne("SharedLibrary.Models.MusicSchedule.Rules.SoundCode", "SoundCode")
                        .WithMany()
                        .HasForeignKey("SoundCodeId");

                    b.HasOne("SharedLibrary.Models.Station", "Station")
                        .WithMany("AudioFiles")
                        .HasForeignKey("StationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EnergyLevel");

                    b.Navigation("SongCategory");

                    b.Navigation("SoundCode");

                    b.Navigation("Station");
                });

            modelBuilder.Entity("SharedLibrary.Models.Folder", b =>
                {
                    b.HasOne("SharedLibrary.Models.Station", "Station")
                        .WithMany("Folders")
                        .HasForeignKey("StationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Station");
                });

            modelBuilder.Entity("SharedLibrary.Models.MusicSchedule.Category", b =>
                {
                    b.HasOne("SharedLibrary.Models.MusicSchedule.MusicPattern", "Template")
                        .WithMany()
                        .HasForeignKey("TemplateId");

                    b.Navigation("Template");
                });

            modelBuilder.Entity("SharedLibrary.Models.MusicSchedule.PatternCategory", b =>
                {
                    b.HasOne("SharedLibrary.Models.MusicSchedule.Category", "Category")
                        .WithMany("PatternCategories")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SharedLibrary.Models.MusicSchedule.MusicPattern", "MusicPattern")
                        .WithMany("PatternCategories")
                        .HasForeignKey("MusicPatternId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("MusicPattern");
                });

            modelBuilder.Entity("SharedLibrary.Models.ProgramLogItem", b =>
                {
                    b.HasOne("SharedLibrary.Models.Rotator", "Rotator")
                        .WithMany()
                        .HasForeignKey("RotatorID");

                    b.HasOne("SharedLibrary.Models.MusicSchedule.Rules.SoundCode", "SoundCode")
                        .WithMany()
                        .HasForeignKey("SoundCodeId");

                    b.HasOne("SharedLibrary.Models.Station", "Station")
                        .WithMany("ProgramLogItems")
                        .HasForeignKey("StationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Rotator");

                    b.Navigation("SoundCode");

                    b.Navigation("Station");
                });

            modelBuilder.Entity("SharedLibrary.Models.MusicSchedule.Category", b =>
                {
                    b.Navigation("PatternCategories");
                });

            modelBuilder.Entity("SharedLibrary.Models.MusicSchedule.MusicPattern", b =>
                {
                    b.Navigation("PatternCategories");
                });

            modelBuilder.Entity("SharedLibrary.Models.Station", b =>
                {
                    b.Navigation("AudioFiles");

                    b.Navigation("Folders");

                    b.Navigation("ProgramLogItems");
                });
#pragma warning restore 612, 618
        }
    }
}
