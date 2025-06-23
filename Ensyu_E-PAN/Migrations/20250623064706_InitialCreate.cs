using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ensyu_E_PAN.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "All_Shifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Confirm_Flg = table.Column<bool>(type: "INTEGER", nullable: false),
                    Fixed_Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Sending_Flg = table.Column<bool>(type: "INTEGER", nullable: false),
                    Cost = table.Column<int>(type: "INTEGER", nullable: false),
                    Sum_WorkTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Rec_Flg = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_All_Shifts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    C_Name = table.Column<string>(type: "TEXT", nullable: false),
                    Post_Code = table.Column<int>(type: "INTEGER", nullable: false),
                    Address1 = table.Column<string>(type: "TEXT", nullable: false),
                    Address2 = table.Column<string>(type: "TEXT", nullable: false),
                    Tel = table.Column<int>(type: "INTEGER", nullable: true),
                    Fax = table.Column<int>(type: "INTEGER", nullable: true),
                    Mail = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Item_Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roll_Lists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roll_Lists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    C_Name = table.Column<string>(type: "TEXT", nullable: false),
                    Post_Code = table.Column<int>(type: "INTEGER", nullable: false),
                    Address1 = table.Column<string>(type: "TEXT", nullable: false),
                    Address2 = table.Column<string>(type: "TEXT", nullable: false),
                    Tel = table.Column<int>(type: "INTEGER", nullable: true),
                    Fax = table.Column<int>(type: "INTEGER", nullable: true),
                    Mail = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkRoll_Lists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkRoll_Lists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Day_Shifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    All_Shift_Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Sum_WorkTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Sum_TotalCost = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Day_Shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Day_Shifts_All_Shifts_All_Shift_Id",
                        column: x => x.All_Shift_Id,
                        principalTable: "All_Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Purchase_Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Quotation = table.Column<int>(type: "INTEGER", nullable: false),
                    Tax = table.Column<int>(type: "INTEGER", nullable: true),
                    Order_Date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Delivery_Date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Payment_Date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Payment_Terms = table.Column<string>(type: "TEXT", nullable: false),
                    Confirm_Flg = table.Column<bool>(type: "INTEGER", nullable: false),
                    Company_Cd = table.Column<int>(type: "INTEGER", nullable: false),
                    Manager = table.Column<string>(type: "TEXT", nullable: false),
                    Store_Cd = table.Column<int>(type: "INTEGER", nullable: false),
                    Other = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchase_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purchase_Orders_Companies_Company_Cd",
                        column: x => x.Company_Cd,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Purchase_Orders_Stores_Store_Cd",
                        column: x => x.Store_Cd,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Login_Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Roll_Cd = table.Column<int>(type: "INTEGER", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Stores_Cd = table.Column<int>(type: "INTEGER", nullable: false),
                    TimePrice_D = table.Column<int>(type: "INTEGER", nullable: false),
                    TimePrice_N = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roll_Lists_Roll_Cd",
                        column: x => x.Roll_Cd,
                        principalTable: "Roll_Lists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_Stores_Stores_Cd",
                        column: x => x.Stores_Cd,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Order_Item_Lists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    P_Order_List_Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Item_Cd = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_Item_Lists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Order_Item_Lists_Items_Item_Cd",
                        column: x => x.Item_Cd,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_Item_Lists_Purchase_Orders_P_Order_List_Id",
                        column: x => x.P_Order_List_Id,
                        principalTable: "Purchase_Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Date_Schedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Today = table.Column<DateTime>(type: "TEXT", nullable: false),
                    User_Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Work_Roll_Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Day_Shift_Id = table.Column<int>(type: "INTEGER", nullable: false),
                    P_Start_WorkTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    P_End_WorkTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    U_Start_WorkTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    U_End_WorkTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Start_WorkTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    End_WorkTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Start_BreakTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    End_BreakTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    T_WorkTime_D = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    T_WorkTime_N = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    T_WorkTime_All = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    D_DayPrice = table.Column<int>(type: "INTEGER", nullable: true),
                    N_DayPrice = table.Column<int>(type: "INTEGER", nullable: true),
                    T_DayPrice = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Date_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Date_Schedules_Day_Shifts_Day_Shift_Id",
                        column: x => x.Day_Shift_Id,
                        principalTable: "Day_Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Date_Schedules_Users_User_Id",
                        column: x => x.User_Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Date_Schedules_WorkRoll_Lists_Work_Roll_Id",
                        column: x => x.Work_Roll_Id,
                        principalTable: "WorkRoll_Lists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "User_Shifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    User_Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Shift_Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Day_Shifts_Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Date_Ym = table.Column<DateTime>(type: "TEXT", nullable: false),
                    List_Status = table.Column<bool>(type: "INTEGER", nullable: false),
                    Total_WorkTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Month_Price = table.Column<int>(type: "INTEGER", nullable: false),
                    U_Confirm_Flg = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Shifts_All_Shifts_Shift_Id",
                        column: x => x.Shift_Id,
                        principalTable: "All_Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_Shifts_Day_Shifts_Day_Shifts_Id",
                        column: x => x.Day_Shifts_Id,
                        principalTable: "Day_Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_User_Shifts_Users_User_Id",
                        column: x => x.User_Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USER_DATE_SHIFTS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    User_Shift_Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Shift_Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Date_Schedule_Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_DATE_SHIFTS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_USER_DATE_SHIFTS_Date_Schedules_Date_Schedule_Id",
                        column: x => x.Date_Schedule_Id,
                        principalTable: "Date_Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_USER_DATE_SHIFTS_User_Shifts_User_Shift_Id",
                        column: x => x.User_Shift_Id,
                        principalTable: "User_Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Date_Schedules_Day_Shift_Id",
                table: "Date_Schedules",
                column: "Day_Shift_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Date_Schedules_User_Id",
                table: "Date_Schedules",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Date_Schedules_Work_Roll_Id",
                table: "Date_Schedules",
                column: "Work_Roll_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Day_Shifts_All_Shift_Id",
                table: "Day_Shifts",
                column: "All_Shift_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Item_Lists_Item_Cd",
                table: "Order_Item_Lists",
                column: "Item_Cd");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Item_Lists_P_Order_List_Id",
                table: "Order_Item_Lists",
                column: "P_Order_List_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_Orders_Company_Cd",
                table: "Purchase_Orders",
                column: "Company_Cd");

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_Orders_Store_Cd",
                table: "Purchase_Orders",
                column: "Store_Cd");

            migrationBuilder.CreateIndex(
                name: "IX_USER_DATE_SHIFTS_Date_Schedule_Id",
                table: "USER_DATE_SHIFTS",
                column: "Date_Schedule_Id");

            migrationBuilder.CreateIndex(
                name: "IX_USER_DATE_SHIFTS_User_Shift_Id",
                table: "USER_DATE_SHIFTS",
                column: "User_Shift_Id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Shifts_Day_Shifts_Id",
                table: "User_Shifts",
                column: "Day_Shifts_Id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Shifts_Shift_Id",
                table: "User_Shifts",
                column: "Shift_Id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Shifts_User_Id",
                table: "User_Shifts",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Login_Id",
                table: "Users",
                column: "Login_Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Roll_Cd",
                table: "Users",
                column: "Roll_Cd");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Stores_Cd",
                table: "Users",
                column: "Stores_Cd");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Order_Item_Lists");

            migrationBuilder.DropTable(
                name: "USER_DATE_SHIFTS");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Purchase_Orders");

            migrationBuilder.DropTable(
                name: "Date_Schedules");

            migrationBuilder.DropTable(
                name: "User_Shifts");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "WorkRoll_Lists");

            migrationBuilder.DropTable(
                name: "Day_Shifts");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "All_Shifts");

            migrationBuilder.DropTable(
                name: "Roll_Lists");

            migrationBuilder.DropTable(
                name: "Stores");
        }
    }
}
