using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "bst");

            migrationBuilder.CreateTable(
                name: "BC_AuditLogs",
                schema: "bst",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BC_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BC_Companies",
                schema: "bst",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BC_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BC_IdentityRoles",
                schema: "bst",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BC_IdentityRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BC_IdentityUsers",
                schema: "bst",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BC_IdentityUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BC_IdentityUsers_BC_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "bst",
                        principalTable: "BC_Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BC_Projects",
                schema: "bst",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BC_Projects", x => new { x.Id, x.CompanyId });
                    table.ForeignKey(
                        name: "FK_BC_Projects_BC_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "bst",
                        principalTable: "BC_Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BC_IdentityUserRoleClaims",
                schema: "bst",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BC_IdentityUserRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BC_IdentityUserRoleClaims_BC_IdentityRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "bst",
                        principalTable: "BC_IdentityRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BC_IdentityUserClaims",
                schema: "bst",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BC_IdentityUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BC_IdentityUserClaims_BC_IdentityUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "bst",
                        principalTable: "BC_IdentityUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BC_IdentityUserLogins",
                schema: "bst",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BC_IdentityUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_BC_IdentityUserLogins_BC_IdentityUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "bst",
                        principalTable: "BC_IdentityUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BC_IdentityUserRefreshTokens",
                schema: "bst",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BC_IdentityUserRefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BC_IdentityUserRefreshTokens_BC_IdentityUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "bst",
                        principalTable: "BC_IdentityUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BC_IdentityUserRoles",
                schema: "bst",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BC_IdentityUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_BC_IdentityUserRoles_BC_IdentityRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "bst",
                        principalTable: "BC_IdentityRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BC_IdentityUserRoles_BC_IdentityUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "bst",
                        principalTable: "BC_IdentityUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BC_IdentityUserTokens",
                schema: "bst",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BC_IdentityUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_BC_IdentityUserTokens_BC_IdentityUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "bst",
                        principalTable: "BC_IdentityUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BC_Tasks",
                schema: "bst",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectUid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Progress = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ParentTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkHours = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BC_Tasks", x => new { x.CompanyId, x.ProjectUid, x.Id });
                    table.ForeignKey(
                        name: "FK_BC_Tasks_BC_Projects_CompanyId_ProjectId",
                        columns: x => new { x.CompanyId, x.ProjectId },
                        principalSchema: "bst",
                        principalTable: "BC_Projects",
                        principalColumns: new[] { "Id", "CompanyId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BC_Tasks_BC_Tasks_CompanyId_ProjectId_ParentTaskId",
                        columns: x => new { x.CompanyId, x.ProjectId, x.ParentTaskId },
                        principalSchema: "bst",
                        principalTable: "BC_Tasks",
                        principalColumns: new[] { "CompanyId", "ProjectUid", "Id" });
                });

            migrationBuilder.CreateTable(
                name: "BC_Resources",
                schema: "bst",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectUid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProjectTaskCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectTaskProjectUid = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BC_Resources", x => new { x.CompanyId, x.ProjectUid, x.Id });
                    table.ForeignKey(
                        name: "FK_BC_Resources_BC_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "bst",
                        principalTable: "BC_Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BC_Resources_BC_IdentityUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "bst",
                        principalTable: "BC_IdentityUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BC_Resources_BC_Projects_CompanyId_ProjectUid",
                        columns: x => new { x.CompanyId, x.ProjectUid },
                        principalSchema: "bst",
                        principalTable: "BC_Projects",
                        principalColumns: new[] { "Id", "CompanyId" });
                    table.ForeignKey(
                        name: "FK_BC_Resources_BC_Tasks_ProjectTaskCompanyId_ProjectTaskProjectUid_ProjectTaskId",
                        columns: x => new { x.ProjectTaskCompanyId, x.ProjectTaskProjectUid, x.ProjectTaskId },
                        principalSchema: "bst",
                        principalTable: "BC_Tasks",
                        principalColumns: new[] { "CompanyId", "ProjectUid", "Id" });
                });

            migrationBuilder.CreateTable(
                name: "BC_TaskLinks",
                schema: "bst",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BC_TaskLinks", x => new { x.CompanyId, x.ProjectId, x.Id });
                    table.ForeignKey(
                        name: "FK_BC_TaskLinks_BC_Tasks_CompanyId_ProjectId_FromTaskId",
                        columns: x => new { x.CompanyId, x.ProjectId, x.FromTaskId },
                        principalSchema: "bst",
                        principalTable: "BC_Tasks",
                        principalColumns: new[] { "CompanyId", "ProjectUid", "Id" });
                    table.ForeignKey(
                        name: "FK_BC_TaskLinks_BC_Tasks_CompanyId_ProjectId_ToTaskId",
                        columns: x => new { x.CompanyId, x.ProjectId, x.ToTaskId },
                        principalSchema: "bst",
                        principalTable: "BC_Tasks",
                        principalColumns: new[] { "CompanyId", "ProjectUid", "Id" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_BC_AuditLogs_UserId_Timestamp",
                schema: "bst",
                table: "BC_AuditLogs",
                columns: new[] { "UserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "bst",
                table: "BC_IdentityRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BC_IdentityUserClaims_UserId",
                schema: "bst",
                table: "BC_IdentityUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BC_IdentityUserLogins_UserId",
                schema: "bst",
                table: "BC_IdentityUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BC_IdentityUserRefreshTokens_UserId",
                schema: "bst",
                table: "BC_IdentityUserRefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BC_IdentityUserRoleClaims_RoleId",
                schema: "bst",
                table: "BC_IdentityUserRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_BC_IdentityUserRoles_RoleId",
                schema: "bst",
                table: "BC_IdentityUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "bst",
                table: "BC_IdentityUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_BC_IdentityUsers_CompanyId",
                schema: "bst",
                table: "BC_IdentityUsers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "bst",
                table: "BC_IdentityUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BC_Projects_CompanyId",
                schema: "bst",
                table: "BC_Projects",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BC_Resources_ProjectTaskCompanyId_ProjectTaskProjectUid_ProjectTaskId",
                schema: "bst",
                table: "BC_Resources",
                columns: new[] { "ProjectTaskCompanyId", "ProjectTaskProjectUid", "ProjectTaskId" });

            migrationBuilder.CreateIndex(
                name: "IX_BC_Resources_UserId",
                schema: "bst",
                table: "BC_Resources",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BC_TaskLinks_CompanyId_ProjectId_FromTaskId",
                schema: "bst",
                table: "BC_TaskLinks",
                columns: new[] { "CompanyId", "ProjectId", "FromTaskId" });

            migrationBuilder.CreateIndex(
                name: "IX_BC_TaskLinks_CompanyId_ProjectId_ToTaskId",
                schema: "bst",
                table: "BC_TaskLinks",
                columns: new[] { "CompanyId", "ProjectId", "ToTaskId" });

            migrationBuilder.CreateIndex(
                name: "IX_BC_Tasks_CompanyId_ProjectId_ParentTaskId",
                schema: "bst",
                table: "BC_Tasks",
                columns: new[] { "CompanyId", "ProjectId", "ParentTaskId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BC_AuditLogs",
                schema: "bst");

            migrationBuilder.DropTable(
                name: "BC_IdentityUserClaims",
                schema: "bst");

            migrationBuilder.DropTable(
                name: "BC_IdentityUserLogins",
                schema: "bst");

            migrationBuilder.DropTable(
                name: "BC_IdentityUserRefreshTokens",
                schema: "bst");

            migrationBuilder.DropTable(
                name: "BC_IdentityUserRoleClaims",
                schema: "bst");

            migrationBuilder.DropTable(
                name: "BC_IdentityUserRoles",
                schema: "bst");

            migrationBuilder.DropTable(
                name: "BC_IdentityUserTokens",
                schema: "bst");

            migrationBuilder.DropTable(
                name: "BC_Resources",
                schema: "bst");

            migrationBuilder.DropTable(
                name: "BC_TaskLinks",
                schema: "bst");

            migrationBuilder.DropTable(
                name: "BC_IdentityRoles",
                schema: "bst");

            migrationBuilder.DropTable(
                name: "BC_IdentityUsers",
                schema: "bst");

            migrationBuilder.DropTable(
                name: "BC_Tasks",
                schema: "bst");

            migrationBuilder.DropTable(
                name: "BC_Projects",
                schema: "bst");

            migrationBuilder.DropTable(
                name: "BC_Companies",
                schema: "bst");
        }
    }
}
