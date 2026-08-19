using Menro.Domain.Entities.Blog;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class BlogPostLikeConfiguration : IEntityTypeConfiguration<BlogPostLike>
{
    public void Configure(EntityTypeBuilder<BlogPostLike> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.BlogPost)
            .WithMany()
            .HasForeignKey(x => x.BlogPostId)
            .OnDelete(DeleteBehavior.Cascade); // حذف پست -> لایک‌هاش هم پاک بشن

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade); // حذف اکانت -> لایک‌هاش هم پاک بشن (بدون نگرانی، چون این فقط یه رکورد وضعیته، نه محتوا مثل Author که باید Snapshot بمونه)

        // یه کاربر فقط یه‌بار می‌تونه یه پست رو لایک کنه - همین ایندکس یکتا
        // خودش تضمین می‌کنه، حتی اگه لایه‌ی بالاتر (Service) هم چک کنه.
        builder.HasIndex(x => new { x.BlogPostId, x.UserId }).IsUnique();
    }
}