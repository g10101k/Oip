using System.Collections.Concurrent;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Base.Services;

/// <summary>
/// Service for managing a cache of tags. Provides methods to update, retrieve, and clear the cache of tags.
/// </summary>
public class TagCacheService
{
    private readonly ConcurrentDictionary<uint, TagResponse> _tags = new();

    /// <summary>
    /// Gets the collection of all cached tags.
    /// </summary>
    /// <value>The collection of all cached tags.</value>
    public ICollection<TagResponse> Tags => _tags.Values;

    /// <summary>
    /// Fully updates the tag list: adds new tags, updates existing ones, and removes missing tags.
    /// </summary>
    /// <param name="newTags">The new set of tags to update the cache with.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="newTags"/> is null.</exception>
    public void UpdateTags(IEnumerable<TagResponse> newTags)
    {
        ArgumentNullException.ThrowIfNull(newTags);

        var newTagIds = new HashSet<uint>();

        // Update existing and add new tags
        foreach (var tag in newTags)
        {
            _tags[tag.Id] = tag;
            newTagIds.Add(tag.Id);
        }

        // Remove tags that are not in the new list
        foreach (var existingId in _tags.Keys)
        {
            if (!newTagIds.Contains(existingId))
            {
                _tags.TryRemove(existingId, out _);
            }
        }
    }

    /// <summary>
    /// Attempts to retrieve a tag by its ID from the cache.
    /// </summary>
    /// <param name="id">The ID of the tag to retrieve.</param>
    /// <param name="tag">The tag corresponding to the given ID, if found.</param>
    /// <returns>True if the tag was found, otherwise false.</returns>
    public virtual bool TryGetTag(uint id, out TagResponse? tag)
    {
        return _tags.TryGetValue(id, out tag);
    }

    /// <summary>
    /// Clears all cached tags.
    /// </summary>
    /// <remarks>This method clears the entire cache of tags.</remarks>
    public void Clear() => _tags.Clear();
}