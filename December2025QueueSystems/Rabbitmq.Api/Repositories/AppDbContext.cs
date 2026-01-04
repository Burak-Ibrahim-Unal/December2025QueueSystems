using Microsoft.EntityFrameworkCore;

namespace Rabbitmq.Api.Repositories
{

    public partial class AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : DbContext(dbContextOptions)
    {
        public DbSet<OutBox> OutBoxes { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Inbox> Inboxes { get; set; }
        public DbSet<Idempotency> Idempotencies { get; set; }
        public DbSet<User> Users { get; set; }

        override protected void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Host=localhost;Port=5432;Database=rabbitmq_api;Username=postgres;Password=yourpassword");
        }

        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OutBox>(entity =>
            {
                entity.ToTable("OutBox");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Created)
                      .IsRequired();
                entity.Property(e => e.EventType)
                      .IsRequired();
                entity.Property(e => e.EventData)
                      .IsRequired()
                      .HasMaxLength(2000);
                entity.Property(e => e.IsSent)
                      .IsRequired();
            });

            modelBuilder.Entity<Discount>(entity =>
            {
                entity.ToTable("Discount"); // Tablo adını "Discount" olarak belirler.
                entity.HasKey(e => e.Id); // Id özelliğini primary key olarak işaretler.
                entity.Property(e => e.UserId)
                      .IsRequired(); // UserId özelliğinin zorunlu olduğunu belirtir.
                entity.Property(e => e.Rate)
                      .IsRequired()
                      .HasPrecision(18, 2); // Rate özelliğini zorunlu yapar ve decimal precision ayarlar (18,2).
                entity.Property(e => e.IsUsed)
                      .IsRequired()
                      .HasDefaultValue(false); // IsUsed özelliğini zorunlu yapar ve varsayılan değerini false olarak ayarlar.
            });

            modelBuilder.Entity<Inbox>(entity =>
            {
                entity.ToTable("Inbox"); // Tablo adını "Inbox" olarak belirler.
                entity.HasKey(e => e.Id); // Id özelliğini primary key olarak işaretler.
                entity.Property(e => e.EventType)
                      .IsRequired()
                      .HasConversion<int>(); // EventType enum'unu integer olarak veritabanında saklar.
                entity.Property(e => e.EventData)
                      .IsRequired()
                      .HasMaxLength(4000); // EventData özelliğini zorunlu yapar ve maksimum uzunluğunu 4000 karakter olarak ayarlar.
                entity.Property(e => e.CreatedAt)
                      .IsRequired(); // CreatedAt özelliğini zorunlu yapar.
                entity.Property(e => e.IsProcess)
                      .IsRequired()
                      .HasDefaultValue(false); // IsProcess özelliğini zorunlu yapar ve varsayılan değerini false olarak ayarlar.
            });

            modelBuilder.Entity<Idempotency>(entity =>
            {
                entity.ToTable("Idempotency"); // Tablo adını "Idempotency" olarak belirler.
                entity.HasKey(e => e.Key); // Key özelliğini primary key olarak işaretler.
                entity.Property(e => e.Key)
                      .ValueGeneratedNever(); // Key özelliğinin veritabanı tarafından otomatik olarak oluşturulmamasını sağlar.
                entity.Property(e => e.EventType)
                      .IsRequired()
                      .HasConversion<int>(); // EventType enum'unu integer olarak veritabanında saklar.
                entity.Property(e => e.Created)
                      .IsRequired(); // Created özelliğini zorunlu yapar.
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User"); // Tablo adını "User" olarak belirler.
                entity.HasKey(e => e.Id); // Id özelliğini primary key olarak işaretler.
                entity.Property(e => e.UserName)
                      .IsRequired()
                      .HasMaxLength(100); // UserName özelliğini zorunlu yapar ve maksimum uzunluğunu 100 karakter olarak ayarlar.
                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(255); // Email özelliğini zorunlu yapar ve maksimum uzunluğunu 255 karakter olarak ayarlar.
            });
        }
    }
}
