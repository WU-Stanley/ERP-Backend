using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WUIAM.Models;

namespace WUIAM.Models
{
    /// <summary>
    /// Provides extension methods for configuring soft delete global query filters.
    /// </summary>
    public static class SoftDeleteExtensions
    {
        /// <summary>
        /// Configures a global query filter to exclude soft-deleted entities.
        /// Must be called in OnModelCreating.
        /// </summary>
        public static void ConfigureSoftDelete<T>(this EntityTypeBuilder<T> builder) where T : SoftDeleteEntity
        {
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
