using System.ComponentModel.DataAnnotations;

namespace WUIAM.Models
{
    /// <summary>
    /// Generic pagination request parameters.
    /// </summary>
    public class PaginationParams
    {
        /// <summary>
        /// Page number (1-based). Defaults to 1.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1.")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Items per page. Defaults to 20, max 100.
        /// </summary>
        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Generic paginated response wrapper.
    /// </summary>
    public class PaginatedResponse<T>
    {
        /// <summary>
        /// The items on the current page.
        /// </summary>
        public IEnumerable<T> Items { get; set; } = [];

        /// <summary>
        /// Total number of items across all pages.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages available.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// Whether there is a previous page.
        /// </summary>
        public bool HasPrevious => PageNumber > 1;

        /// <summary>
        /// Whether there is a next page.
        /// </summary>
        public bool HasNext => PageNumber < TotalPages;
    }
}
