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
        public DbSet<Roll> Roll_Lists { get; set; }
        public DbSet<WorkRoll> WorkRoll_Lists { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Item> Items { get; set; }


        //Attendance
        public DbSet<AllShift> All_Shifts { get; set; }
        public DbSet<DayShift> Day_Shifts { get; set; }
        public DbSet<UserShift> User_Shifts { get; set; }
        public DbSet<UserDateShift> User_Date_Shifts { get; set; }
        public DbSet<DateSchedule> Date_Schedules { get; set; }

        //Order
        public DbSet<PurchaseOrder> Purchase_Orders { get; set; }
        public DbSet<OrderItemList> Order_Item_Lists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== Masters 関連 =====

            //User
            modelBuilder.Entity<User>()
             .HasIndex(u => u.Login_Id)
             .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Roll)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.Roll_Cd);

            modelBuilder.Entity<User>()
                .HasOne(u=>u.Store)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.Stores_Cd);

            // ===== Attendance 関連 =====
            //AllShift
                modelBuilder.Entity<AllShift>()
                    .HasOne(alls => alls.Store)
                    .WithMany(s => s.AllShifts)
                    .HasForeignKey(alls => alls.Store_ID);
            //UserShift
                modelBuilder.Entity<UserShift>()
                    .HasOne(us => us.User)
                    .WithMany(uSer => uSer.UserShifts)
                    .HasForeignKey(us => us.User_Id)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<UserShift>()
                    .HasOne(us => us.AllShift)
                    .WithMany(aShifts => aShifts.UserShifts)
                    .HasForeignKey(us => us.Shift_Id)
                    .OnDelete(DeleteBehavior.Cascade);

            //DayShift
                modelBuilder.Entity<DayShift>()
                    .HasOne(ds => ds.AllShift)
                    .WithMany(allS => allS.DayShifts)
                    .HasForeignKey(ds => ds.All_Shift_Id)
                    .OnDelete(DeleteBehavior.Cascade);
                
            //DataSchedule
                modelBuilder.Entity<DateSchedule>()
                    .HasOne(ds => ds.User)
                    .WithMany(u => u.DateSchedules)
                    .HasForeignKey(ds => ds.User_Id)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<DateSchedule>()
                    .HasOne(ds => ds.WorkRoll)
                    .WithMany(wr => wr.DateSchedules)
                    .HasForeignKey(ds => ds.Work_Roll_Id)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<DateSchedule>()
                    .HasOne(ds => ds.DayShift)
                    .WithMany(d => d.DateSchedules)
                    .HasForeignKey(ds => ds.Day_Shift_Id)
                    .OnDelete(DeleteBehavior.Cascade);

            //UserDateShift(中間テーブル)
                modelBuilder.Entity<UserDateShift>(entity =>
                {
                    entity.ToTable("USER_DATE_SHIFTS");

                    entity.HasKey(e => e.Id);

                    entity.HasOne(e => e.UserShift)
                    .WithMany(us => us.UserDateShifts)
                    .HasForeignKey(e => e.User_Shift_Id)
                    .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(e => e.DateSchedule)
                    .WithMany(ds => ds.UserDateShifts)
                    .HasForeignKey(e => e.Date_Schedule_Id)
                    .OnDelete(DeleteBehavior.Restrict);
                });


            // ===== Order 関連 =====
            //PurchaseOrder
                modelBuilder.Entity<PurchaseOrder>()
                    .HasOne(pol => pol.Company)
                    .WithMany(c => c.PurchaseOrders)
                    .HasForeignKey(pol => pol.Company_Cd);

                modelBuilder.Entity<PurchaseOrder>()
                    .HasOne(pol => pol.Store)
                    .WithMany(s => s.PurchaseOrders)
                    .HasForeignKey(pol => pol.Store_Cd);
            //OrderItemList
                modelBuilder.Entity<OrderItemList>()
                    .HasOne(oil => oil.PurchaseOrder)
                    .WithMany(pol => pol.OrderItemLists)
                    .HasForeignKey(o => o.P_Order_List_Id);

                modelBuilder.Entity<OrderItemList>()
                    .HasOne(oil =>oil.Item)
                    .WithMany(item => item.OrderItemLists)
                    .HasForeignKey(o => o.Item_Cd);
        }
    }
}
