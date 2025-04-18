﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SharedLibrary.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataPath = table.Column<string>(type: "text", nullable: true),
                    IsDatapathSetup = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnergyLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<string>(type: "text", nullable: true),
                    StationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnergyLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MusicGridItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Hour = table.Column<string>(type: "text", nullable: true),
                    Sunday = table.Column<string>(type: "text", nullable: false),
                    Monday = table.Column<string>(type: "text", nullable: false),
                    Tuesday = table.Column<string>(type: "text", nullable: false),
                    Wednesday = table.Column<string>(type: "text", nullable: false),
                    Thursday = table.Column<string>(type: "text", nullable: false),
                    Friday = table.Column<string>(type: "text", nullable: false),
                    Saturday = table.Column<string>(type: "text", nullable: false),
                    SundayPatternId = table.Column<int>(type: "integer", nullable: true),
                    MondayPatternId = table.Column<int>(type: "integer", nullable: true),
                    TuesdayPatternId = table.Column<int>(type: "integer", nullable: true),
                    WednesdayPatternId = table.Column<int>(type: "integer", nullable: true),
                    ThursdayPatternId = table.Column<int>(type: "integer", nullable: true),
                    FridayPatternId = table.Column<int>(type: "integer", nullable: true),
                    SaturdayPatternId = table.Column<int>(type: "integer", nullable: true),
                    StationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicGridItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MusicPatterns",
                columns: table => new
                {
                    PatternId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicPatterns", x => x.PatternId);
                });

            migrationBuilder.CreateTable(
                name: "Rotator",
                columns: table => new
                {
                    RotatorID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RotatorTitle = table.Column<string>(type: "text", nullable: true),
                    RotatorArtist = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Length = table.Column<double>(type: "double precision", nullable: false),
                    Intro = table.Column<short>(type: "smallint", nullable: false),
                    Segue = table.Column<short>(type: "smallint", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rotator", x => x.RotatorID);
                });

            migrationBuilder.CreateTable(
                name: "SongScheduleSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistSeperation = table.Column<int>(type: "integer", nullable: false),
                    TitleSeperation = table.Column<int>(type: "integer", nullable: false),
                    MaxSoundcodeSeperation = table.Column<int>(type: "integer", nullable: false),
                    MaxEnergySeperation = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongScheduleSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SoundCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: true),
                    StationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoundCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    StationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StationSortOrder = table.Column<int>(type: "integer", nullable: false),
                    CallLetters = table.Column<string>(type: "text", nullable: true),
                    Slogan = table.Column<string>(type: "text", nullable: true),
                    AudioDeviceId = table.Column<string>(type: "text", nullable: false),
                    CurrentPlayingId = table.Column<int>(type: "integer", nullable: false),
                    CurrentPlayingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NextPlayId = table.Column<int>(type: "integer", nullable: false),
                    NextPlayDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.StationId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryName = table.Column<string>(type: "text", nullable: true),
                    StationId = table.Column<int>(type: "integer", nullable: false),
                    TemplateId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_Category_MusicPatterns_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "MusicPatterns",
                        principalColumn: "PatternId");
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    StationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Folders_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "StationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Log",
                columns: table => new
                {
                    LogOrderID = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Cue = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Artist = table.Column<string>(type: "text", nullable: true),
                    TimeScheduled = table.Column<TimeOnly>(type: "time", nullable: false),
                    TimeEstimated = table.Column<TimeOnly>(type: "time", nullable: false),
                    TimePlayed = table.Column<TimeOnly>(type: "time", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    RotatorID = table.Column<int>(type: "integer", nullable: true),
                    Length = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Intro = table.Column<short>(type: "smallint", nullable: false),
                    Segue = table.Column<short>(type: "smallint", nullable: false),
                    SongCategory = table.Column<string>(type: "text", nullable: true),
                    From = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Passthrough = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: true),
                    Device = table.Column<int>(type: "integer", nullable: true),
                    Progress = table.Column<double>(type: "double precision", nullable: false),
                    StationId = table.Column<int>(type: "integer", nullable: false),
                    SoundCodeId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => new { x.Date, x.LogOrderID });
                    table.ForeignKey(
                        name: "FK_Log_Rotator_RotatorID",
                        column: x => x.RotatorID,
                        principalTable: "Rotator",
                        principalColumn: "RotatorID");
                    table.ForeignKey(
                        name: "FK_Log_SoundCodes_SoundCodeId",
                        column: x => x.SoundCodeId,
                        principalTable: "SoundCodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Log_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "StationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AudioFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Filename = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Artist = table.Column<string>(type: "text", nullable: false),
                    Intro = table.Column<short>(type: "smallint", nullable: false),
                    Segue = table.Column<short>(type: "smallint", nullable: false),
                    StartDate = table.Column<int>(type: "integer", nullable: false),
                    EndDate = table.Column<int>(type: "integer", nullable: false),
                    ProtectNextIntro = table.Column<bool>(type: "boolean", nullable: false),
                    IntroSeconds = table.Column<double>(type: "double precision", nullable: false),
                    SegueSeconds = table.Column<double>(type: "double precision", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    StationId = table.Column<int>(type: "integer", nullable: false),
                    Folder = table.Column<string>(type: "text", nullable: true),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    SoundCodeId = table.Column<int>(type: "integer", nullable: true),
                    EnergyLevelId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AudioFiles_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "CategoryId");
                    table.ForeignKey(
                        name: "FK_AudioFiles_EnergyLevels_EnergyLevelId",
                        column: x => x.EnergyLevelId,
                        principalTable: "EnergyLevels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AudioFiles_SoundCodes_SoundCodeId",
                        column: x => x.SoundCodeId,
                        principalTable: "SoundCodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AudioFiles_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "StationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatternCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MusicPatternSortOrder = table.Column<int>(type: "integer", nullable: false),
                    MusicPatternId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: false),
                    StationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatternCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatternCategories_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatternCategories_MusicPatterns_MusicPatternId",
                        column: x => x.MusicPatternId,
                        principalTable: "MusicPatterns",
                        principalColumn: "PatternId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Id", "DataPath", "IsDatapathSetup" },
                values: new object[] { 1, "C:\\AudionixAudio", false });

            migrationBuilder.InsertData(
                table: "SongScheduleSettings",
                columns: new[] { "Id", "ArtistSeperation", "MaxEnergySeperation", "MaxSoundcodeSeperation", "TitleSeperation" },
                values: new object[] { 1, 10, 3, 3, 10 });

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_Id",
                table: "AppSettings",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AudioFiles_CategoryId",
                table: "AudioFiles",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AudioFiles_EnergyLevelId",
                table: "AudioFiles",
                column: "EnergyLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_AudioFiles_SoundCodeId",
                table: "AudioFiles",
                column: "SoundCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_AudioFiles_StationId",
                table: "AudioFiles",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_Category_TemplateId",
                table: "Category",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_StationId",
                table: "Folders",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_Log_RotatorID",
                table: "Log",
                column: "RotatorID");

            migrationBuilder.CreateIndex(
                name: "IX_Log_SoundCodeId",
                table: "Log",
                column: "SoundCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Log_StationId",
                table: "Log",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_PatternCategories_CategoryId",
                table: "PatternCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PatternCategories_MusicPatternId",
                table: "PatternCategories",
                column: "MusicPatternId");

            migrationBuilder.CreateIndex(
                name: "IX_SongScheduleSettings_Id",
                table: "SongScheduleSettings",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AudioFiles");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "Log");

            migrationBuilder.DropTable(
                name: "MusicGridItems");

            migrationBuilder.DropTable(
                name: "PatternCategories");

            migrationBuilder.DropTable(
                name: "SongScheduleSettings");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "EnergyLevels");

            migrationBuilder.DropTable(
                name: "Rotator");

            migrationBuilder.DropTable(
                name: "SoundCodes");

            migrationBuilder.DropTable(
                name: "Stations");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "MusicPatterns");
        }
    }
}
