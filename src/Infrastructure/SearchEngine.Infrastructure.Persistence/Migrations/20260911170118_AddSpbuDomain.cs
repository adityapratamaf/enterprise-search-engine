using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpbuDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fasilitas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nama = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fasilitas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProdukBbms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nama = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Deskripsi = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Jenis = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ron = table.Column<int>(type: "int", nullable: true),
                    CetaneNumber = table.Column<int>(type: "int", nullable: true),
                    IsSubsidi = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Urutan = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdukBbms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regionals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nomor = table.Column<int>(type: "int", nullable: false),
                    Kode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nama = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regionals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wilayahs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nama = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wilayahs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wilayahs_Regionals_RegionalId",
                        column: x => x.RegionalId,
                        principalTable: "Regionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wilayahs_Wilayahs_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Wilayahs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Spbus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodeSpbu = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nama = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Alamat = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    KodePos = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    WilayahId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    TipeKepemilikan = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    JumlahDispenser = table.Column<int>(type: "int", nullable: false),
                    JumlahNozzle = table.Column<int>(type: "int", nullable: false),
                    TanggalOperasi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NomorTelepon = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spbus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spbus_Wilayahs_WilayahId",
                        column: x => x.WilayahId,
                        principalTable: "Wilayahs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SpbuFasilitas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpbuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FasilitasId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpbuFasilitas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpbuFasilitas_Fasilitas_FasilitasId",
                        column: x => x.FasilitasId,
                        principalTable: "Fasilitas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SpbuFasilitas_Spbus_SpbuId",
                        column: x => x.SpbuId,
                        principalTable: "Spbus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpbuProduks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpbuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProdukBbmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpbuProduks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpbuProduks_ProdukBbms_ProdukBbmId",
                        column: x => x.ProdukBbmId,
                        principalTable: "ProdukBbms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SpbuProduks_Spbus_SpbuId",
                        column: x => x.SpbuId,
                        principalTable: "Spbus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fasilitas_Kode",
                table: "Fasilitas",
                column: "Kode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProdukBbms_Jenis",
                table: "ProdukBbms",
                column: "Jenis");

            migrationBuilder.CreateIndex(
                name: "IX_ProdukBbms_Kode",
                table: "ProdukBbms",
                column: "Kode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Regionals_Kode",
                table: "Regionals",
                column: "Kode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Regionals_Nomor",
                table: "Regionals",
                column: "Nomor",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SpbuFasilitas_FasilitasId",
                table: "SpbuFasilitas",
                column: "FasilitasId");

            migrationBuilder.CreateIndex(
                name: "IX_SpbuFasilitas_SpbuId_FasilitasId",
                table: "SpbuFasilitas",
                columns: new[] { "SpbuId", "FasilitasId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SpbuProduks_ProdukBbmId",
                table: "SpbuProduks",
                column: "ProdukBbmId");

            migrationBuilder.CreateIndex(
                name: "IX_SpbuProduks_SpbuId_ProdukBbmId",
                table: "SpbuProduks",
                columns: new[] { "SpbuId", "ProdukBbmId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Spbus_KodeSpbu",
                table: "Spbus",
                column: "KodeSpbu",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Spbus_Nama",
                table: "Spbus",
                column: "Nama",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Spbus_Status",
                table: "Spbus",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Spbus_WilayahId",
                table: "Spbus",
                column: "WilayahId");

            migrationBuilder.CreateIndex(
                name: "IX_Wilayahs_Kode",
                table: "Wilayahs",
                column: "Kode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Wilayahs_ParentId_Level",
                table: "Wilayahs",
                columns: new[] { "ParentId", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_Wilayahs_RegionalId",
                table: "Wilayahs",
                column: "RegionalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpbuFasilitas");

            migrationBuilder.DropTable(
                name: "SpbuProduks");

            migrationBuilder.DropTable(
                name: "Fasilitas");

            migrationBuilder.DropTable(
                name: "ProdukBbms");

            migrationBuilder.DropTable(
                name: "Spbus");

            migrationBuilder.DropTable(
                name: "Wilayahs");

            migrationBuilder.DropTable(
                name: "Regionals");
        }
    }
}
