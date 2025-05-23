using System;

namespace WhisperVaultApi.Models
{
    public class Confession
    {
        public Guid Id { get; set; }

        public string Text { get; set; }

        public DateTime SubmittedAt { get; set; } // Needed for timestamping

        public bool IsReleased { get; set; } // Used to determine visibility

          // Navigation property for tags (many-to-many)
    public ICollection<Tag> Tags { get; set; }
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Navigation property for confessions (many-to-many)
    public ICollection<Confession> Confessions { get; set; }
}
    }

