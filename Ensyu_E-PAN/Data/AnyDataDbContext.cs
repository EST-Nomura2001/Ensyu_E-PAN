using Ensyu_E_PAN.Models.Attendance;
using Ensyu_E_PAN.Models.Masters;
using Ensyu_E_PAN.Models.Order;
using Microsoft.EntityFrameworkCore;

namespace Ensyu_E_PAN.Data
{
    public class AnyDataDbContext:DbContext
    {
        public AnyDataDbContext(DbContextOptions<AnyDataDbContext> options) : base(options) { }

        //Masters
        public DbSet<User> Users { get; set; }
        public DbSet<Roll> Rolls { get; set; }
        public DbSet<WorkRoll> WorkRolls { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Item> Items { get; set; }


        //Attendance
        public DbSet<AllShift> AllShifts { get; set; }
        public DbSet<DayShift> DayShifts { get; set; }
        public DbSet<UserShift> UserShifts { get; set; }
        public DbSet<UserDateShift> UserDateShifts { get; set; }
        public DbSet<DateSchedule> DateSchedules { get; set; }

        //Order
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<OrderItemList> OrderItemLists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== Masters 関連 =====
            modelBuilder.Entity<User>()
                .HasOne<Roll>()
                .WithMany()
                .HasForeignKey(u => u.Roll_Cd);

            modelBuilder.Entity<User>()
                .HasOne<Store>()
                .WithMany()
                .HasForeignKey(u => u.Stores_Cd);

            // ===== Attendance 関連 =====
            modelBuilder.Entity<UserDateShift>()
                .HasOne<UserShift>()
                .WithMany()
                .HasForeignKey(uds => uds.User_Shift_Id);

            modelBuilder.Entity<UserDateShift>()
                .HasOne<DateSchedule>()
                .WithMany()
                .HasForeignKey(uds => uds.Date_Schedule_Id);

            // ===== Order 関連 =====
            modelBuilder.Entity<OrderItemList>()
                .HasOne<PurchaseOrder>()
                .WithMany()
                .HasForeignKey(o => o.P_Order_List_Id);

            modelBuilder.Entity<OrderItemList>()
                .HasOne<Item>()
                .WithMany()
                .HasForeignKey(o => o.Item_Cd);
        }
    }
}
