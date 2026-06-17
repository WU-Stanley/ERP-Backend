namespace WUIAM.Models
{
    /// <summary>
    /// Base entity with soft delete support.
    /// Entities inheriting from this class will have IsDeleted property and automatic soft delete handling.
    /// </summary>
    public abstract class SoftDeleteEntity
    {
        /// <summary>
        /// Whether this entity has been soft-deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// The timestamp when this entity was soft-deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// The ID of the user who soft-deleted this entity.
        /// </summary>
        public Guid? DeletedBy { get; set; }

        /// <summary>
        /// Marks this entity as soft-deleted.
        /// </summary>
        public void SoftDelete(Guid? deletedBy = null)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
        }

        /// <summary>
        /// Restores this entity from soft deletion.
        /// </summary>
        public void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
        }
    }

    /// <summary>
    /// Base entity with soft delete support and GUID primary key.
    /// </summary>
    public abstract class SoftDeleteGuidEntity : SoftDeleteEntity
    {
        /// <summary>
        /// The primary key of the entity.
        /// </summary>
        public Guid Id { get; set; }
    }

    /// <summary>
    /// Base entity with soft delete support and int primary key.
    /// </summary>
    public abstract class SoftDeleteIntEntity : SoftDeleteEntity
    {
        /// <summary>
        /// The primary key of the entity.
        /// </summary>
        public int Id { get; set; }
    }
}
