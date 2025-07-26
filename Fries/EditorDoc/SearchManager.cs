using System.Collections.Generic;
using UnityEngine;
using HelpSystem.DataModels;
using System.Linq;

namespace HelpSystem.Search {
    public static class SearchManager {
        public static List<Post> SearchPosts(List<Post> allPosts, string query, bool searchTitleOnly) {
            if (string.IsNullOrWhiteSpace(query)) {
                return new List<Post>();
            }

            string lowerQuery = query.ToLower();
            List<Post> results = new List<Post>();

            foreach (Post post in allPosts) {
                // Search by title
                if (post.title.ToLower().Contains(lowerQuery)) {
                    results.Add(post);
                    continue; // If found in title, no need to search content if title only or already found
                }

                // Search by content if not title only
                if (searchTitleOnly) continue;
                foreach (TextAsset page in post.pages) {
                    if (!page.text.ToLower().Contains(lowerQuery)) continue;
                    results.Add(post);
                    break; // Found in content, move to next post
                }
            }

            return results.Distinct().ToList(); // Ensure unique posts
        }
    }
}
