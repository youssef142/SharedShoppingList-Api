using Microsoft.EntityFrameworkCore;
using FamilyShoppingList.Models;

namespace FamilyShoppingList.Data
{
    public class ShoppingListDbContext : DbContext
    {
        public ShoppingListDbContext(DbContextOptions<ShoppingListDbContext> options) : base(options)
        {
        }

        public DbSet<ShoppingItem> ShoppingItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<GroupInvite> GroupInvites { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ItemEditRequest> ItemEditRequests { get; set; }
        
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShoppingItem>()
                .HasOne(item => item.AddedByUser)
                .WithMany(user => user.ItemsAdded)
                .HasForeignKey(item => item.AddedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ShoppingItem>()
                .HasOne(item => item.StatusChangedByUser)
                .WithMany(user => user.ItemsStatusChanged)
                .HasForeignKey(item => item.StatusChangedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.Token)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(r => r.Token)
                      .IsUnique();

                entity.Property(r => r.Token)
                      .HasMaxLength(64)
                      .IsRequired();
            });

            // Group -> OwnerUser
            modelBuilder.Entity<Group>()
                .HasOne(g => g.OwnerUser)
                .WithMany(u => u.OwnedGroups)
                .HasForeignKey(g => g.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // GroupMember -> Group
            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // GroupMember -> User
            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // GroupInvite -> Group
            modelBuilder.Entity<GroupInvite>()
                .HasOne(gi => gi.Group)
                .WithMany()
                .HasForeignKey(gi => gi.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // GroupInvite -> InvitedUser (the two-FK-to-User situation)
            modelBuilder.Entity<GroupInvite>()
                .HasOne(gi => gi.InvitedUser)
                .WithMany()
                .HasForeignKey(gi => gi.InvitedUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupInvite>()
                .HasOne(gi => gi.InvitingUser)
                .WithMany()
                .HasForeignKey(gi => gi.InvitingUserId)
                .OnDelete(DeleteBehavior.NoAction);  // avoids the cascade-path conflict, same as before

            // ShoppingItem -> Group
            modelBuilder.Entity<ShoppingItem>()
                .HasOne(i => i.Group)
                .WithMany(g => g.shoppingItems)
                .HasForeignKey(i => i.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Group)
                .WithMany(g => g.Categories)
                .HasForeignKey(c => c.GroupId)
                .OnDelete(DeleteBehavior.Cascade); // deleting a group deletes its categories

            // Category -> ShoppingItems (one-to-many)
            modelBuilder.Entity<ShoppingItem>()
                .HasOne(i => i.Category)
                .WithMany(c => c.ShoppingItems)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.SetNull); // deleting a category doesn't delete items, just unsets it

            // Uniqueness: no duplicate category names within the same group
            modelBuilder.Entity<Category>()
                .HasIndex(c => new { c.GroupId, c.Name })
                .IsUnique();

            modelBuilder.Entity<ItemEditRequest>()
                .HasOne(r => r.Item)
                .WithMany()
                .HasForeignKey(r => r.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ItemEditRequest>()
                .HasOne(r => r.Group)
                .WithMany()
                .HasForeignKey(r => r.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ItemEditRequest>()
                .HasOne(r => r.RequestedByUser)
                .WithMany()
                .HasForeignKey(r => r.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ItemEditRequest>()
                .HasOne(r => r.ReviewedByUser)
                .WithMany()
                .HasForeignKey(r => r.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
