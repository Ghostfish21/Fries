# if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using HelpSystem.DataModels;
using HelpSystem.Search;
using HelpSystem.Markdown;

namespace HelpSystem.Editor {
    public class HelpSystemEditorWindow : EditorWindow {
        private List<Post> allPosts = new();
        private string _searchQuery = "";
        private bool _searchTitleOnly = true;
        private List<Post> _searchResults = new List<Post>();
        private Post _selectedPost;
        private int _currentPageIndex = 0;
        private Vector2 _scrollPosition;
        private Vector2 _contentScrollPosition;

        [MenuItem("Window/Help System")]
        public static void ShowWindow() {
            GetWindow<HelpSystemEditorWindow>("Help System");
        }

        private void OnEnable() {
            string[] guids = AssetDatabase.FindAssets("t:Post");
            allPosts = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<Post>(AssetDatabase.GUIDToAssetPath(g)))
                .ToList();
        }

        private void OnGUI() {
            DrawSearchSection();
            EditorGUILayout.Separator();
            DrawContentSection();
        }

        private void DrawSearchSection() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _searchQuery = EditorGUILayout.TextField("Search:", _searchQuery, EditorStyles.toolbarSearchField);
            _searchTitleOnly = GUILayout.Toggle(_searchTitleOnly,
                new GUIContent("Title Only", "Search only in post titles"), EditorStyles.toolbarButton);

            if (GUILayout.Button("Search", EditorStyles.toolbarButton) || Event.current.keyCode == KeyCode.Return) {
                PerformSearch();
            }

            EditorGUILayout.EndHorizontal();

            if (_searchResults.Count > 0) {
                EditorGUILayout.BeginVertical("box", GUILayout.MaxHeight(150));
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                foreach (Post post in _searchResults) {
                    if (GUILayout.Button(post.title, EditorStyles.miniButton)) {
                        _selectedPost = post;
                        _currentPageIndex = 0;
                        MarkdownRenderer.ClearCache(); // Clear image cache when new post is selected
                    }
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            else if (!string.IsNullOrWhiteSpace(_searchQuery)) {
                EditorGUILayout.HelpBox("No results found.", MessageType.Info);
            }
        }

        private void PerformSearch() {
            _searchResults = SearchManager.SearchPosts(allPosts, _searchQuery, _searchTitleOnly);
        }

        private void DrawContentSection() {
            if (!_selectedPost) {
                EditorGUILayout.HelpBox("Select a post from the search results or create a new one.", MessageType.Info);

                return;
            }

            EditorGUILayout.LabelField(_selectedPost.title, EditorStyles.boldLabel);

            if (_selectedPost.pages.Count == 0) {
                EditorGUILayout.HelpBox("This post has no pages.", MessageType.Warning);
                return;
            }

            // Page navigation buttons
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = (_currentPageIndex > 0);
            if (GUILayout.Button("◀ Previous")) {
                _currentPageIndex--;
            }

            GUI.enabled = (_currentPageIndex < _selectedPost.pages.Count - 1);
            if (GUILayout.Button("Next ▶")) {
                _currentPageIndex++;
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"Page {_currentPageIndex + 1} / {_selectedPost.pages.Count}",
                EditorStyles.centeredGreyMiniLabel);

            // Display current page content
            TextAsset currentPage = _selectedPost.pages[_currentPageIndex];

            _contentScrollPosition = EditorGUILayout.BeginScrollView(_contentScrollPosition);
            EditorGUILayout.BeginVertical("box");
            string mdText = "";
            if (currentPage) mdText = currentPage.text;
            MarkdownRenderer.Render(mdText);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }
    }
}
# endif