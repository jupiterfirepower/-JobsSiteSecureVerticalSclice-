using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Jobs.VacancyApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    KeyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.KeyId);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId");
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CompanyDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CompanyLogoPath = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CompanyLink = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.CompanyId);
                });

            migrationBuilder.CreateTable(
                name: "EmploymentTypes",
                columns: table => new
                {
                    EmploymentTypeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmploymentTypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentTypes", x => x.EmploymentTypeId);
                });

            migrationBuilder.CreateTable(
                name: "VacancyEmploymentTypes",
                columns: table => new
                {
                    VacancyId = table.Column<int>(type: "integer", nullable: false),
                    EmploymentTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VacancyEmploymentTypes", x => new { x.VacancyId, x.EmploymentTypeId });
                });

            migrationBuilder.CreateTable(
                name: "VacancyWorkTypes",
                columns: table => new
                {
                    VacancyId = table.Column<int>(type: "integer", nullable: false),
                    WorkTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VacancyWorkTypes", x => new { x.VacancyId, x.WorkTypeId });
                });

            migrationBuilder.CreateTable(
                name: "WorkTypes",
                columns: table => new
                {
                    WorkTypeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkTypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTypes", x => x.WorkTypeId);
                });

            migrationBuilder.CreateTable(
                name: "CompanyOwnerEmails",
                columns: table => new
                {
                    CompanyOwnerEmailsId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    UserEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyOwnerEmails", x => x.CompanyOwnerEmailsId);
                    table.ForeignKey(
                        name: "FK_CompanyOwnerEmails_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vacancies",
                columns: table => new
                {
                    VacancyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    VacancyTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    VacancyDescription = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    SalaryFrom = table.Column<double>(type: "double precision", nullable: true),
                    SalaryTo = table.Column<double>(type: "double precision", nullable: true),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacancies", x => x.VacancyId);
                    table.ForeignKey(
                        name: "FK_Vacancies_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vacancies_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ApiKeys",
                columns: new[] { "KeyId", "Created", "IsActive", "Key", "Modified" },
                values: new object[] { 1, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6734), true, "123456789123456789123", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6729) });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName", "Created", "IsActive", "IsVisible", "Modified", "ParentId" },
                values: new object[] { 1, "Категорії вакансій", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6624), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6625), null });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "CompanyId", "CompanyDescription", "CompanyLink", "CompanyLogoPath", "CompanyName", "Created", "IsActive", "IsVisible", "Modified" },
                values: new object[] { 1, "Test Company Description", "/company/link", "/company/logo.png", "Test Company", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6757), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6754) });

            migrationBuilder.InsertData(
                table: "EmploymentTypes",
                columns: new[] { "EmploymentTypeId", "Created", "EmploymentTypeName", "Modified" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6807), "full-time", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6806) },
                    { 2, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6809), "part-time", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6808) },
                    { 3, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6810), "temporary", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6809) },
                    { 4, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6811), "contract", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6811) },
                    { 5, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6812), "freelance", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6812) }
                });

            migrationBuilder.InsertData(
                table: "WorkTypes",
                columns: new[] { "WorkTypeId", "Created", "Modified", "WorkTypeName" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6780), new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6778), "Office" },
                    { 2, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6782), new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6781), "Remote" },
                    { 3, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6783), new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6782), "Office/Remote" },
                    { 4, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6784), new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6784), "Hybrid" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName", "Created", "IsActive", "IsVisible", "Modified", "ParentId" },
                values: new object[,]
                {
                    { 2, "IT, комп''ютери, інтернет", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6662), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6662), 1 },
                    { 3, "Адмiнiстрацiя, керівництво середньої ланки", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6663), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6664), 1 },
                    { 4, "Будівництво, архітектура", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6665), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6665), 1 },
                    { 5, "Бухгалтерія, аудит", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6666), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6666), 1 },
                    { 6, "Готельно-ресторанний бізнес, туризм", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6667), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6667), 1 },
                    { 7, "Дизайн, творчість", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6668), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6668), 1 },
                    { 8, "ЗМІ, видавництво, поліграфія", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6669), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6669), 1 },
                    { 9, "Краса, фітнес, спорт", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6670), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6670), 1 },
                    { 10, "Культура, музика, шоу-бізнес", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6671), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6671), 1 },
                    { 11, "Логістика, склад, ЗЕД", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6672), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6672), 1 },
                    { 12, "Маркетинг, реклама, PR", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6672), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6673), 1 },
                    { 13, "Медицина, фармацевтика", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6673), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6674), 1 },
                    { 14, "Нерухомість", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6675), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6675), 1 },
                    { 15, "Освіта, наука", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6675), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6676), 1 },
                    { 16, "Охорона, безпека", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6676), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6677), 1 },
                    { 17, "Продаж, закупівля", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6677), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6678), 1 },
                    { 18, "Робочі спеціальності, виробництво", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6678), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6679), 1 },
                    { 19, "Роздрібна торгівля", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6679), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6680), 1 },
                    { 20, "Секретаріат, діловодство, АГВ", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6680), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6681), 1 },
                    { 21, "Сільське господарство, агробізнес", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6681), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6682), 1 },
                    { 22, "Страхування", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6682), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6683), 1 },
                    { 23, "Сфера обслуговування", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6683), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6684), 1 },
                    { 24, "Телекомунікації та зв'язок", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6684), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6684), 1 },
                    { 25, "Топменеджмент, керівництво вищої ланки", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6685), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6685), 1 },
                    { 26, "Транспорт, автобізнес", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6686), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6686), 1 },
                    { 27, "Управління персоналом", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6687), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6687), 1 },
                    { 28, "Фінанси, банк", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6688), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6688), 1 },
                    { 29, "Юриспруденція", new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6689), true, true, new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6689), 1 }
                });

            migrationBuilder.InsertData(
                table: "CompanyOwnerEmails",
                columns: new[] { "CompanyOwnerEmailsId", "CompanyId", "Created", "IsActive", "Modified", "UserEmail" },
                values: new object[] { 1, 1, new DateTime(2024, 10, 9, 15, 11, 46, 972, DateTimeKind.Utc).AddTicks(7898), true, new DateTime(2024, 10, 9, 15, 11, 46, 972, DateTimeKind.Utc).AddTicks(7895), "jupiterfiretetraedr@gmail.com" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId",
                table: "Categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyOwnerEmails_CompanyId",
                table: "CompanyOwnerEmails",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_CategoryId",
                table: "Vacancies",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_CompanyId",
                table: "Vacancies",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "CompanyOwnerEmails");

            migrationBuilder.DropTable(
                name: "EmploymentTypes");

            migrationBuilder.DropTable(
                name: "Vacancies");

            migrationBuilder.DropTable(
                name: "VacancyEmploymentTypes");

            migrationBuilder.DropTable(
                name: "VacancyWorkTypes");

            migrationBuilder.DropTable(
                name: "WorkTypes");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
