// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    public partial class TimeflowViewDisplay : TimeflowViewModuleBase
    {
        private const int _menuSearchButtonLeftPad = 16;
        private const int _menuSearchFieldTopPad = 4;
        private const int _menuSearchFieldPadLeft = 4;
        private const int _menuSearchFieldLineSize = 2;

        #region PUBLIC

        public enum SearchTypeSorting
        {
            Alphabetical,
            Count,
            Prioritized
        }

        public string SearchTerm;

        #endregion

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public bool IsSearching;

        [NonSerialized]
        public bool IsFiltered = false;


        #endregion

        #region PRIVATE NON-SERIALIZED

        [NonSerialized]
        private GUIRect menuSearchButtonRect;

        [NonSerialized]
        private GUIRect menuSearchFieldRect;

        [NonSerialized]
        private GUIRect menuSearchFieldLine;

        [NonSerialized]
        private List<string> searchTypes;


        [NonSerialized]
        private int prioritizedTypesCount;

        private const int _menuSearchTypeWidth = 140;

        [SerializeField]
        private int _SearchTypeIndex = 0; // 0 = None, >0 maps to searchTypes[index-1]

        [NonSerialized]
        private GUIRect menuSearchTypeRect;

        [NonSerialized]
        private List<Type> searchTypeTypes;

        [NonSerialized]
        private List<Component> searchTypeSampleComponents;

        [NonSerialized]
        private List<int> searchTypeCounts;

        [NonSerialized]
        private int searchTypePrioritizedCount = 0;

        [NonSerialized]
        private List<string> channelWords;

        [NonSerialized]
        private List<int> channelWordCounts;

        [NonSerialized]
        private int channelWordPrioritizedCount = 0;

        private const int _menuChannelWordWidth = 140; // width of the channel word dropdown

        // Multi-select support for channel words
        [SerializeField]
        private List<int> _SelectedWordIndices = new List<int>(); // indices into channelWords (0-based)

        [NonSerialized]
        private GUIRect menuChannelWordRect;

        [NonSerialized]
        private GUIRect searchSortingRect;

        [NonSerialized]
        private GUIRect clearSearchRect;

        #endregion

        #region SEARCH

        public void Search()
        {
            IsSearching = !IsSearching;
            if (IsSearching) {
                ApplyFilter();
                BuildSearchMenus();
                string focused = GUI.GetNameOfFocusedControl();
                if (focused != "SearchDisplay") {
                    AxonGUI.FocusControl("SearchDisplay");
                }
            }
            ApplyFilter();
        }

        #endregion

        #region SEARCH & FILTER

        public void ApplyFilter()
        {
            IsFiltered = false;
            if (Objects != null && Objects.Count > 0) {
                foreach (TimeflowObject obj in Objects) {
                    if (obj == null) continue;
                    ApplyFilter(obj);
                }
            }
        }

        private void ApplyFilter(TimeflowObject obj)
        {
            if (obj == null) {
                Timeflow.View.NeedsRefresh = true;
                return;
            }

            obj.IsDisplayed = true;

            // Determine search flags
            bool hasSearchText = IsSearching && !string.IsNullOrEmpty(SearchTerm);
            bool hasTypeDropdown = IsSearching && _SearchTypeIndex > 0 && searchTypes != null && _SearchTypeIndex - 1 < searchTypes.Count;
            bool hasWordSelection = IsSearching && _SelectedWordIndices != null && _SelectedWordIndices.Count > 0 && channelWords != null;
            bool hasSearch = hasSearchText || hasTypeDropdown || hasWordSelection;

            // Clear channel search-displayed flags to neutral "true", will be AND-ed by active filters
            if (obj.AllChannelsForDisplay != null) {
                foreach (var ch in obj.AllChannelsForDisplay) {
                    if (ch == null) continue;
                    ch.IsSearchDisplayed = true;
                }
            }

            // Parse search filters similar to Unity: support type filter prefix "t:TypeName"
            List<string> typeTerms = null;
            List<string> textTerms = null;
            if (hasSearchText) {
                typeTerms = new List<string>();
                textTerms = new List<string>();
                var parts = SearchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++) {
                    var p = parts[i];
                    var lower = p.ToLower();

                    if (lower == "t:" && i + 1 < parts.Length) {
                        var next = parts[++i];
                        var tname = next.Trim();
                        if (!string.IsNullOrEmpty(tname)) typeTerms.Add(tname.ToLower());
                        continue;
                    }

                    if (lower.StartsWith("t:") && lower.Length > 2) {
                        var tname = p.Substring(2).Trim();
                        if (!string.IsNullOrEmpty(tname)) typeTerms.Add(tname.ToLower());
                        continue;
                    }

                    textTerms.Add(lower);
                }
            }

            // Merge dropdown selected type into typeTerms so it acts as an AND filter with any typed t: filters
            if (hasTypeDropdown) {
                if (typeTerms == null) typeTerms = new List<string>();
                string selectedType = searchTypes[_SearchTypeIndex - 1];
                if (!string.IsNullOrEmpty(selectedType)) {
                    string t = selectedType.ToLower();
                    if (!typeTerms.Contains(t)) typeTerms.Add(t);
                }
            }

            if (hasSearch) {
                bool typeOk = true;
                if (typeTerms != null && typeTerms.Count > 0) {
                    typeOk = true;
                    var components = obj.gameObject.GetComponents<Component>();
                    for (int ti = 0; ti < typeTerms.Count; ti++) {
                        string tfilter = typeTerms[ti];
                        bool matchedThisType = false;

                        if (components != null) {
                            for (int ci = 0; ci < components.Length; ci++) {
                                var c = components[ci];
                                if (c == null) continue;
                                var ct = c.GetType();
                                string n = ct.Name.ToLower();
                                string fn = ct.FullName != null ? ct.FullName.ToLower() : n;
                                if (n.Contains(tfilter) || fn.Contains(tfilter)) { matchedThisType = true; break; }
                            }
                        }

                        if (!matchedThisType && obj.AllChannels != null) {
                            foreach (var ch in obj.AllChannels) {
                                if (ch == null) continue;
                                var ct = ch.GetType();
                                string n = ct.Name.ToLower();
                                string fn = ct.FullName != null ? ct.FullName.ToLower() : n;
                                if (n.Contains(tfilter) || fn.Contains(tfilter)) { matchedThisType = true; break; }
                            }
                        }

                        if (!matchedThisType) { typeOk = false; break; }
                    }
                }

                bool textOk = true;
                bool objectMatched = false;
                if (hasSearchText) {
                    if (textTerms != null && textTerms.Count > 0) {
                        textOk = true;
                        string oname = obj.Name != null ? obj.Name.ToLower() : string.Empty;
                        for (int wi = 0; wi < textTerms.Count; wi++) {
                            string token = textTerms[wi];
                            bool found = false;

                            if (!string.IsNullOrEmpty(oname) && oname.Contains(token)) {
                                found = true;
                                objectMatched = true;
                            }
                            else if (obj.AllChannelsForDisplay != null) {
                                foreach (var ch in obj.AllChannelsForDisplay) {
                                    if (ch == null) continue;
                                    string cname = ch.Name != null ? ch.Name.ToLower() : string.Empty;
                                    if (!string.IsNullOrEmpty(cname) && cname.Contains(token)) { found = true; break; }
                                }
                            }

                            if (!found) { textOk = false; break; }
                        }

                        if (obj.AllChannelsForDisplay != null) {
                            foreach (var ch in obj.AllChannelsForDisplay) {
                                if (ch == null) continue;
                                bool chMatch = true;
                                if (objectMatched) {
                                    // Object name matched: leave all channels as matching
                                    chMatch = true;
                                }
                                else
                                if (textTerms.Count > 0) {
                                    string cname = ch.Name != null ? ch.Name.ToLower() : string.Empty;
                                    for (int wi = 0; wi < textTerms.Count; wi++) {
                                        string token = textTerms[wi];
                                        if (string.IsNullOrEmpty(cname) || !cname.Contains(token)) { chMatch = false; break; }
                                    }
                                }
                                else {
                                    chMatch = false;
                                }
                                ch.IsSearchDisplayed = ch.IsSearchDisplayed && chMatch;
                            }
                        }
                    }
                    else {
                        // No text tokens: leave channel flags alone so other filters (like channel word) can apply
                    }
                }
                else {
                    // No text portion to search: leave channel flags alone so other filters (like channel word) can apply
                }

                // Apply channel word multi-select filter (OR logic - match ANY selected word)
                bool wordOk = true;
                if (hasWordSelection) {
                    wordOk = false;

                    // Build list of selected words
                    List<string> selectedWords = new List<string>();
                    foreach (int idx in _SelectedWordIndices) {
                        if (idx >= 0 && idx < channelWords.Count) {
                            string word = channelWords[idx];
                            if (!string.IsNullOrEmpty(word)) {
                                selectedWords.Add(word.ToLower());
                            }
                        }
                    }

                    if (selectedWords.Count > 0) {
                        // Apply to channels-for-display first to control visibility
                        if (obj.AllChannelsForDisplay != null) {
                            foreach (var ch in obj.AllChannelsForDisplay) {
                                if (ch == null) continue;
                                string cname = ch.Name != null ? ch.Name.ToLower() : string.Empty;

                                // Check if channel contains ANY of the selected words
                                bool chWordMatch = false;
                                foreach (string word in selectedWords) {
                                    if (!string.IsNullOrEmpty(cname) && cname.Contains(word)) {
                                        chWordMatch = true;
                                        break;
                                    }
                                }

                                // When no text search is active, set solely based on word match
                                // When text search is active, AND with previous flag
                                ch.IsSearchDisplayed = typeOk || textOk || hasSearchText ? (ch.IsSearchDisplayed && chWordMatch) : chWordMatch;

                                if (chWordMatch) wordOk = true;
                            }
                        }
                        else if (obj.AllChannels != null) {
                            foreach (var ch in obj.AllChannels) {
                                if (ch == null) continue;
                                string cname = ch.Name != null ? ch.Name.ToLower() : string.Empty;

                                // Check if channel contains ANY of the selected words
                                foreach (string word in selectedWords) {
                                    if (!string.IsNullOrEmpty(cname) && cname.Contains(word)) {
                                        wordOk = true;
                                        break;
                                    }
                                }
                                if (wordOk) break;
                            }
                        }
                    }
                }

                IsFiltered = true;
                obj.IsDisplayed = typeOk && textOk && wordOk;
            }

            if (!obj.gameObject.activeInHierarchy && VisibleOnly) {
                obj.IsDisplayed = false;
                IsFiltered = true;
            }
            if ((obj.IsLocked && UnlockedOnly) || (!obj.IsLocked && LockedOnly)) {
                obj.IsDisplayed = false;
                IsFiltered = true;
            }
            if (EnabledOnly) {
                if (!obj.Enabled) {
                    obj.IsDisplayed = false;
                    IsFiltered = true;
                }
            }

            if (obj.IsDisplayed) {
                if (ChannelMode == ChannelModes.Objects) {
                    obj.IsDisplayed = true;
                    IsFiltered = true;
                }
                else
                if (ChannelMode == ChannelModes.Displayed) {
                    obj.IsDisplayed = obj.DisplayChannels;
                    IsFiltered = true;
                }
                else
                if (ChannelMode == ChannelModes.Solo) {
                    obj.IsDisplayed = obj.DisplaySolo;
                    IsFiltered = true;
                    if (obj.AllChannels != null) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (ch.DisplayChannelSolo) {
                                obj.IsDisplayed = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (!obj.IsDisplayed) {
                AnyObjectsHidden = true;
                Timeflow.View.SelectObject(obj, false);
            }
        }

        public void BuildSearchMenus()           
        {
            _BuildSearchTypeMenu();
            _BuildChannelWordMenu();
        }

        private void _BuildSearchTypeMenu()
        {
            if (searchTypes == null) searchTypes = new List<string>();
            if (searchTypeTypes == null) searchTypeTypes = new List<Type>();
            if (searchTypeSampleComponents == null) searchTypeSampleComponents = new List<Component>();
            if (searchTypeCounts == null) searchTypeCounts = new List<int>();

            // Preserve current selection before clearing
            string prevTypeName = null;
            Type prevType = null;
            if (_SearchTypeIndex > 0 && searchTypes.Count >= _SearchTypeIndex) {
                int prevIdx = _SearchTypeIndex - 1;
                if (prevIdx >= 0 && prevIdx < searchTypes.Count) {
                    prevTypeName = searchTypes[prevIdx];
                    if (searchTypeTypes != null && prevIdx < searchTypeTypes.Count) {
                        prevType = searchTypeTypes[prevIdx];
                    }
                }
            }

            searchTypes.Clear();
            searchTypeTypes.Clear();
            searchTypeSampleComponents.Clear();
            searchTypeCounts.Clear();
            searchTypePrioritizedCount = 0;

            if (Objects == null || Objects.Count == 0) {
                _SearchTypeIndex = 0; // reset when nothing to populate
                return;
            }

            int ci = 1;
            List<Component> allComponents = new List<Component>();
            foreach (var obj in Objects) {
                if (obj == null || obj.gameObject == null) continue;
                if (!obj.IsDisplayed) continue; // include only actively displayed objects

                Component[] comps;
                try {
                    comps = obj.gameObject.GetComponents<Component>();
                }
                catch {
                    continue;
                }

                if (comps == null || comps.Length == 0) continue;

                foreach (var c in comps) {
                    allComponents.Add(c);
                    ci++;
                }
            }
            if (allComponents.Count == 0) return;

            var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            var sampleMap = new Dictionary<string, Component>(StringComparer.OrdinalIgnoreCase);
            var countMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var c in allComponents) {
                if (c == null) continue;
                var t = c.GetType();
                if (t == null) continue;

                var name = t.Name;
                if (string.IsNullOrEmpty(name)) continue;

                if (!map.ContainsKey(name)) {
                    map.Add(name, t);
                    sampleMap.Add(name, c);
                }

                if (countMap.TryGetValue(name, out int cnt)) {
                    countMap[name] = cnt + 1;
                }
                else {
                    countMap[name] = 1;
                }
            }

            if (map.Count > 0) {
                // Determine prioritized keys from preferences (in given order)
                List<string> orderedKeys = new List<string>();
                List<string> prioritized = new List<string>();

                if (TimeflowPreferences.Current.SearchTypeSorting != SearchTypeSorting.Prioritized) {
                    // Non-prioritized sorting: alphabetical or by count
                    var keys = new List<string>(map.Keys);
                    if (TimeflowPreferences.Current.SearchTypeSorting == SearchTypeSorting.Alphabetical) {
                        keys.Sort(StringComparer.OrdinalIgnoreCase);
                    }
                    else
                    if (TimeflowPreferences.Current.SearchTypeSorting == SearchTypeSorting.Count) {
                        keys.Sort((a, b) => {
                            int acnt = countMap.TryGetValue(a, out int ac) ? ac : 0;
                            int bcnt = countMap.TryGetValue(b, out int bc) ? bc : 0;
                            return bcnt.CompareTo(acnt); // descending
                        });
                    }
                    orderedKeys.AddRange(keys);
                }
                else {
                    var pref = TimeflowPreferences.Current;
                    if (pref != null && pref.PrioritizedFilterTypes != null && pref.PrioritizedFilterTypes.Count > 0) {
                        foreach (var p in pref.PrioritizedFilterTypes) {
                            if (string.IsNullOrEmpty(p)) continue;
                            // exact name match, case-insensitive
                            foreach (var key in map.Keys) {
                                if (string.Equals(key, p, StringComparison.OrdinalIgnoreCase)) {
                                    if (!prioritized.Contains(key)) prioritized.Add(key);
                                }
                            }
                        }
                    }

                    // Collect non-prioritized keys and sort alphabetically
                    var remaining = new List<string>();
                    foreach (var key in map.Keys) {
                        if (!prioritized.Contains(key)) remaining.Add(key);
                    }
                    remaining.Sort(StringComparer.OrdinalIgnoreCase);

                    // Build final ordered list: prioritized first (keep preference-provided order), then remaining
                    orderedKeys.AddRange(prioritized);
                    orderedKeys.AddRange(remaining);
                }
                prioritizedTypesCount = prioritized.Count;

                // Fill lists in the new order
                foreach (var key in orderedKeys) {
                    searchTypes.Add(key);
                    searchTypeTypes.Add(map[key]);
                    // match sample by key
                    if (sampleMap.TryGetValue(key, out var sample)) searchTypeSampleComponents.Add(sample);
                    else searchTypeSampleComponents.Add(null);
                    // add count
                    searchTypeCounts.Add(countMap.TryGetValue(key, out var c) ? c : 0);
                }

                // Store count of prioritized items for UI divider
                searchTypePrioritizedCount = prioritized.Count;
            }

            // Restore previous selection if possible
            int newIndex = 0; // 0 = All
            if (prevType != null && searchTypeTypes != null && searchTypeTypes.Count > 0) {
                for (int i = 0; i < searchTypeTypes.Count; i++) {
                    if (searchTypeTypes[i] == prevType) { newIndex = i + 1; break; }
                }
            }
            if (newIndex == 0 && !string.IsNullOrEmpty(prevTypeName) && searchTypes != null && searchTypes.Count > 0) {
                for (int i = 0; i < searchTypes.Count; i++) {
                    if (string.Equals(searchTypes[i], prevTypeName, StringComparison.OrdinalIgnoreCase)) { newIndex = i + 1; break; }
                }
            }

            if (searchTypes == null || searchTypes.Count == 0) newIndex = 0;
            _SearchTypeIndex = newIndex;
        }

        private void _BuildChannelWordMenu()
        {
            // Capture previous selections by word values before clearing
            List<string> prevSelectedWords = new List<string>();
            if (channelWords != null && _SelectedWordIndices != null) {
                foreach (int idx in _SelectedWordIndices) {
                    if (idx >= 0 && idx < channelWords.Count) {
                        prevSelectedWords.Add(channelWords[idx]);
                    }
                }
            }

            if (channelWords == null) channelWords = new List<string>();
            if (channelWordCounts == null) channelWordCounts = new List<int>();
            channelWords.Clear();
            channelWordCounts.Clear();
            channelWordPrioritizedCount = 0;

            if (Objects == null || Objects.Count == 0) {
                if (_SelectedWordIndices == null) _SelectedWordIndices = new List<int>();
                _SelectedWordIndices.Clear();
                return;
            }

            // Build counts of words across currently displayed channels
            var countMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int oi = 0; oi < Objects.Count; oi++) {
                var obj = Objects[oi];
                if (obj == null) continue;
                if (!obj.IsDisplayed) continue; // include only actively displayed objects

                var src = obj.AllChannelsForDisplay != null && obj.AllChannelsForDisplay.Count > 0
                    ? obj.AllChannelsForDisplay
                    : obj.AllChannels;

                if (src == null || src.Count == 0) continue;

                for (int ci = 0; ci < src.Count; ci++) {
                    var ch = src[ci];
                    if (ch == null || string.IsNullOrEmpty(ch.Name) || !ch.IsSearchDisplayed) continue;

                    // De-duplicate words per channel
                    var wordsInChannel = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var tokens = ch.Name.Split(new[] { ' ', '-', '_', '/', '.', ':', '[', ']', '(', ')', '{', '}', '|', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int ti = 0; ti < tokens.Length; ti++) {
                        var w = tokens[ti].Trim();
                        if (string.IsNullOrEmpty(w)) continue;
                        if (w == "Unnamed") continue; // skip common meaningless word
                        if (!wordsInChannel.Add(w)) continue; // already counted for this channel
                        if (countMap.TryGetValue(w, out int cnt)) countMap[w] = cnt + 1;
                        else countMap[w] = 1;
                    }
                }
            }

            if (countMap.Count > 0) {

                if (TimeflowPreferences.Current.SearchTypeSorting != SearchTypeSorting.Prioritized) {
                    if (TimeflowPreferences.Current.SearchTypeSorting == SearchTypeSorting.Alphabetical) {
                        var keys = new List<string>(countMap.Keys);
                        keys.Sort(StringComparer.OrdinalIgnoreCase);
                        foreach (var w in keys) {
                            channelWords.Add(w);
                            channelWordCounts.Add(countMap[w]);
                        }
                    }
                    else { // Count
                        var keys = new List<string>(countMap.Keys);
                        keys.Sort((a, b) => {
                            int acnt = countMap.TryGetValue(a, out int ac) ? ac : 0;
                            int bcnt = countMap.TryGetValue(b, out int bc) ? bc : 0;
                            return bcnt.CompareTo(acnt); // descending
                        });
                        foreach (var w in keys) {
                            channelWords.Add(w);
                            channelWordCounts.Add(countMap[w]);
                        }
                    }
                }
                else {
                    // Prioritize words using preferences (supports partial matches)
                    var prefs = TimeflowPreferences.Current;
                    var keys = new List<string>(countMap.Keys);
                    var prioritizedPairs = new List<(int prefIdx, string key)>();
                    var prioritizedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    if (prefs != null && prefs.PrioritizedChannelNames != null && prefs.PrioritizedChannelNames.Count > 0) {
                        foreach (var key in keys) {
                            int bestIdx = int.MaxValue;
                            for (int pi = 0; pi < prefs.PrioritizedChannelNames.Count; pi++) {
                                string p = prefs.PrioritizedChannelNames[pi];
                                if (string.IsNullOrEmpty(p)) continue;
                                if (key.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0) {
                                    if (pi < bestIdx) bestIdx = pi;
                                }
                            }
                            if (bestIdx != int.MaxValue) {
                                prioritizedPairs.Add((bestIdx, key));
                                prioritizedSet.Add(key);
                            }
                        }
                    }

                    // Sort prioritized by preference order, then alphabetically
                    prioritizedPairs.Sort((a, b) => {
                        int cmp = a.prefIdx.CompareTo(b.prefIdx);
                        return cmp != 0 ? cmp : string.Compare(a.key, b.key, StringComparison.OrdinalIgnoreCase);
                    });

                    // Remaining keys (non-prioritized) sorted alphabetically
                    var remaining = new List<string>();
                    foreach (var key in keys) {
                        if (!prioritizedSet.Contains(key)) remaining.Add(key);
                    }
                    remaining.Sort(StringComparer.OrdinalIgnoreCase);

                    // Build final ordered lists
                    foreach (var pair in prioritizedPairs) {
                        string w = pair.key;
                        channelWords.Add(w);
                        channelWordCounts.Add(countMap[w]);
                    }
                    channelWordPrioritizedCount = prioritizedPairs.Count;

                    foreach (var w in remaining) {
                        channelWords.Add(w);
                        channelWordCounts.Add(countMap[w]);
                    }
                }

            }

            // Restore previous selections by value if they still exist
            if (_SelectedWordIndices == null) _SelectedWordIndices = new List<int>();
            _SelectedWordIndices.Clear();

            if (prevSelectedWords.Count > 0 && channelWords.Count > 0) {
                foreach (string word in prevSelectedWords) {
                    int matchIdx = channelWords.FindIndex(w => string.Equals(w, word, StringComparison.OrdinalIgnoreCase));
                    if (matchIdx >= 0 && !_SelectedWordIndices.Contains(matchIdx)) {
                        _SelectedWordIndices.Add(matchIdx);
                    }
                }
            }
        }

        private void GUIMenuSearch()
        {
            if (IsLayout) {
                menuSearchFieldRect.y = _menuSearchFieldTopPad;
                menuSearchFieldRect.x = menuSearchButtonRect.x + menuSearchButtonRect.width + _menuSearchFieldPadLeft;

                // Fit search field and dropdowns within the Hierarchy width
                float rightLimit = Layout.Hierarchy.Left + Layout.Hierarchy.Width - 30;
                float startX = menuSearchFieldRect.x;
                const float rightPadding = 6f;
                const float gap = 4f;
                const float minFieldW = 60f;
                const float minTypeW = 50f;
                const float minWordW = 50f;

                float available = Mathf.Max(0f, rightLimit - startX - rightPadding);

                // Calculate desired widths
                float typeW = _menuSearchTypeWidth;
                float wordW = _menuChannelWordWidth;

                // Dynamically size the type dropdown based on its text content
                float ComputeTypeDropdownWidth()
                {
                    GUIStyle style = EditorStyles.popup;
                    float w = 0f;

                    // Start with "All"
                    Vector2 sizeAll = Vector2.zero;
                    w = Mathf.Max(w, sizeAll.x);

                    // Include types with their counts, if available
                    if (searchTypes != null && searchTypes.Count > 0 && _SearchTypeIndex > 0) {
                        int i = _SearchTypeIndex - 1;
                        int cnt = (searchTypeCounts != null && i < searchTypeCounts.Count) ? searchTypeCounts[i] : 0;
                        string label = $"{searchTypes[i]} ({cnt})";
                        Vector2 sz = style.CalcSize(new GUIContent(label));
                        if (sz.x > w) w = sz.x;
                    }
                    else {
                        style.CalcSize(new GUIContent("All"));
                    }

                    // Add padding for icon + dropdown arrow + margins
                    const float popupExtraPadding = 28f;
                    w += popupExtraPadding;

                    // Clamp to a reasonable maximum so field has room; ensure at least minTypeW
                    float maxType = Mathf.Max(minTypeW, available - (minWordW + minFieldW + (2f * gap)));
                    if (float.IsNaN(maxType) || float.IsInfinity(maxType)) maxType = available;
                    return Mathf.Clamp(w, minTypeW, Mathf.Max(minTypeW, maxType));
                }

                typeW = ComputeTypeDropdownWidth();

                // Remaining width for the search field after dropdowns + gaps
                float remaining = available - (typeW + wordW + (2f * gap));

                // If not enough space for the field, reduce dropdown widths down to their minimums
                if (remaining < minFieldW) {
                    float deficit = (minFieldW - remaining);

                    // Reduce channel word dropdown first
                    float reduce = Mathf.Min(deficit, wordW - minWordW);
                    wordW -= reduce;
                    deficit -= reduce;

                    // Then reduce type dropdown if still needed
                    if (deficit > 0f) {
                        reduce = Mathf.Min(deficit, typeW - minTypeW);
                        typeW -= reduce;
                        deficit -= reduce;
                    }
                }

                // Final field width (allow shrinking below min if absolutely necessary)
                float fieldW = Mathf.Clamp(available - (typeW + wordW + (2f * gap)), 10f, available);

                // Assign rects
                menuSearchFieldRect.width = (int)fieldW;
                menuSearchFieldRect.height = TimeflowViewLayout.SmallIconSize;

                // Type dropdown rect to the right of the text field
                menuSearchTypeRect = menuSearchFieldRect;
                menuSearchTypeRect.x = (int)(menuSearchFieldRect.x + menuSearchFieldRect.width + gap);
                menuSearchTypeRect.width = (int)Mathf.Max(0f, typeW);

                // Channel word dropdown rect to the right of the type dropdown
                menuChannelWordRect = menuSearchTypeRect;
                menuChannelWordRect.x = (int)(menuSearchTypeRect.x + menuSearchTypeRect.width + gap);
                menuChannelWordRect.width = (int)Mathf.Max(0f, wordW);

                // Ensure the last control does not overflow the Hierarchy right edge
                float lastRight = menuChannelWordRect.x + menuChannelWordRect.width;
                if (lastRight > rightLimit) {
                    float overflow = lastRight - rightLimit;
                    menuChannelWordRect.width = (int)Mathf.Max(0f, menuChannelWordRect.width - overflow);
                }

                // Clear button rect after channel word dropdown
                searchSortingRect = menuChannelWordRect;
                searchSortingRect.x = (int)(menuChannelWordRect.x + menuChannelWordRect.width + gap);
                searchSortingRect.y = menuSearchFieldRect.y;
                searchSortingRect.width = searchSortingRect.height = TimeflowViewLayout.SmallIconSize;

                // Make room for clear button if it overflows
                float clearRight = searchSortingRect.x + searchSortingRect.width;
                if (clearRight > rightLimit) {
                    float overflow = clearRight - rightLimit;
                    menuChannelWordRect.width = (int)Mathf.Max(0f, menuChannelWordRect.width - overflow);
                    searchSortingRect.x = (int)(menuChannelWordRect.x + menuChannelWordRect.width + gap);
                }

                // Clear button rect after channel word dropdown
                clearSearchRect = searchSortingRect;
                clearSearchRect.x = searchSortingRect.xMax;

                menuSearchFieldLine = menuDisplayNameRect;
                menuSearchFieldLine.y += menuSearchFieldLine.height + _menuSearchFieldLineSize;
                menuSearchFieldLine.height = _menuSearchFieldLineSize;
            }
            else {
                GUI.color = AxonColor.Default;
                GUIStyle labelStyle = new GUIStyle(GUI.skin.textField);
                labelStyle.alignment = TextAnchor.MiddleLeft;

                // Ensure menus are built
                if (searchTypes == null || searchTypes.Count == 0 || channelWords == null || channelWords.Count == 0) {
                    BuildSearchMenus();
                }

                // Clamp selected indices
                if (_SearchTypeIndex < 0 || (searchTypes != null && _SearchTypeIndex > searchTypes.Count)) {
                    _SearchTypeIndex = 0;
                }

                // Validate selected word indices
                if (_SelectedWordIndices == null) _SelectedWordIndices = new List<int>();
                _SelectedWordIndices.RemoveAll(idx => idx < 0 || idx >= channelWords.Count);

                // Search text field
                GUI.SetNextControlName("SearchDisplay");
                if (SearchTerm == null) SearchTerm = "";
                string search = EditorGUI.TextField(menuSearchFieldRect, SearchTerm, GUI.skin.textField);
                if (search != SearchTerm) {
                    SearchTerm = search;
                    ApplyFilter();
                    BuildSearchMenus();
                }

                // Type dropdown with icons: first option is "All"
                int tcount = (searchTypes != null ? searchTypes.Count : 0);
                bool hasDivider = searchTypePrioritizedCount > 0 && tcount > searchTypePrioritizedCount;
                int totalTypeOptions = 1 + tcount + (hasDivider ? 1 : 0);
                GUIContent[] typeOptions = new GUIContent[totalTypeOptions];
                typeOptions[0] = new GUIContent("All");
                if (tcount > 0) {
                    int opt = 1;
                    for (int i = 0; i < tcount; i++) {
                        if (hasDivider && i == searchTypePrioritizedCount) {
                            // Insert a visual divider (non-selectable handling below)
                            typeOptions[opt++] = new GUIContent("");
                        }
                        Texture icon = AxonUI.Icons.Settings;
                        if (searchTypeTypes != null && i < searchTypeTypes.Count && searchTypeTypes[i] != null) {
                            Component sample = (searchTypeSampleComponents != null && i < searchTypeSampleComponents.Count) ? searchTypeSampleComponents[i] : null;
                            var gc = EditorGUIUtility.ObjectContent(sample, searchTypeTypes[i]);
                            if (gc != null && gc.image != null) icon = gc.image;
                        }
                        int cnt = (searchTypeCounts != null && i < searchTypeCounts.Count) ? searchTypeCounts[i] : 0;
                        string more = TimeflowPreferences.Current.SearchTypeSorting == SearchTypeSorting.Prioritized &&
                            TimeflowPreferences.Current.ShowOtherTypesInSubmenu && i >= prioritizedTypesCount ? "More/" : "";
                        typeOptions[opt++] = new GUIContent($"{more}{searchTypes[i]} ({cnt})", icon);
                    }
                }

                // Map current selection to popup index accounting for divider
                int currentPopupIndex = 0;
                if (_SearchTypeIndex > 0) {
                    int typeIdx = _SearchTypeIndex - 1;
                    if (hasDivider && typeIdx >= searchTypePrioritizedCount) {
                        currentPopupIndex = 1 /*All*/ + 1 /*divider*/ + typeIdx;
                    }
                    else {
                        currentPopupIndex = 1 /*All*/ + typeIdx;
                    }
                }

                int newPopupIndex = EditorGUI.Popup(menuSearchTypeRect, currentPopupIndex, typeOptions);
                if (newPopupIndex != currentPopupIndex) {
                    // If divider selected, ignore and keep previous selection
                    int dividerPopupIndex = hasDivider ? (1 + searchTypePrioritizedCount) : -1;
                    if (newPopupIndex == dividerPopupIndex) {
                        // do nothing
                    }
                    else {
                        int newIndex = 0;
                        if (newPopupIndex == 0) {
                            newIndex = 0; // All
                        }
                        else {
                            int idx = newPopupIndex - 1; // remove All offset
                            if (hasDivider && newPopupIndex > dividerPopupIndex) idx -= 1; // skip divider slot
                            newIndex = idx + 1; // convert to _SearchTypeIndex (1-based)
                        }
                        if (newIndex != _SearchTypeIndex) {
                            _SearchTypeIndex = newIndex;
                            ApplyFilter();
                            BuildSearchMenus();
                            //BuildChannelWordMenu();
                        }
                    }
                }

                // Channel word multi-select dropdown
                // Create display text showing selected count or selected words
                string channelWordDisplay = "All";
                if (_SelectedWordIndices != null && _SelectedWordIndices.Count > 0) {
                    if (_SelectedWordIndices.Count == 1) {
                        int idx = _SelectedWordIndices[0];
                        if (idx >= 0 && idx < channelWords.Count) {
                            int cnt = (channelWordCounts != null && idx < channelWordCounts.Count) ? channelWordCounts[idx] : 0;
                            channelWordDisplay = $"{channelWords[idx]} ({cnt})";
                        }
                    }
                    else {
                        channelWordDisplay = $"Multiple ({_SelectedWordIndices.Count})";
                    }
                }

                // Draw dropdown button
                if (EditorGUI.DropdownButton(menuChannelWordRect, new GUIContent(channelWordDisplay), FocusType.Keyboard)) {
                    // Build and show generic menu for multi-select
                    GenericMenu menu = new GenericMenu();

                    // Add "All (Clear Selection)" option
                    menu.AddItem(new GUIContent("All (Clear Selection)"),
                        _SelectedWordIndices == null || _SelectedWordIndices.Count == 0,
                        () => {
                            _SelectedWordIndices.Clear();
                            ApplyFilter();
                            //BuildChannelWordMenu();
                        });

                    // Add separator after All option
                    menu.AddSeparator("");

                    // Add divider if we have prioritized items
                    bool hasWordDivider = channelWordPrioritizedCount > 0 && channelWords.Count > channelWordPrioritizedCount;

                    // Add each word as a toggle item
                    for (int i = 0; i < channelWords.Count; i++) {
                        // Add divider after prioritized items
                        if (hasWordDivider && i == channelWordPrioritizedCount - 1) {
                            menu.AddSeparator("");
                        }

                        int wordIdx = i; // capture for closure
                        int cnt = (channelWordCounts != null && i < channelWordCounts.Count) ? channelWordCounts[i] : 0;
                        string label = $"{channelWords[i]} ({cnt})";
                        string more = TimeflowPreferences.Current.SearchTypeSorting == SearchTypeSorting.Prioritized &&
                            TimeflowPreferences.Current.ShowOtherTypesInSubmenu && i >= prioritizedTypesCount ? "More/" : "";

                        bool isSelected = _SelectedWordIndices.Contains(wordIdx);

                        menu.AddItem(new GUIContent(more + label), isSelected, () => {
                            if (isSelected) {
                                _SelectedWordIndices.Remove(wordIdx);
                            }
                            else {
                                _SelectedWordIndices.Add(wordIdx);
                            }
                            ApplyFilter();
                            //BuildChannelWordMenu();
                        });
                    }

                    menu.ShowAsContext();
                }

                GUIStyle style = null;
                if (TimeflowPreferences.Current.SearchTypeSorting == SearchTypeSorting.Alphabetical) {
                    style = AxonUI.SearchTypeSortingAlphabeticalStyle;
                }
                else
                if (TimeflowPreferences.Current.SearchTypeSorting == SearchTypeSorting.Prioritized) {
                    style = AxonUI.SearchTypeSortingPrioritizedStyle;
                }
                else {
                    style = AxonUI.SearchTypeSortingCountStyle;
                }

                if (GUI.Button(searchSortingRect, AxonUI.SearchTypeSortingLabel, style)) {
                    if (TimeflowPreferences.Current.SearchTypeSorting == SearchTypeSorting.Alphabetical) {
                        TimeflowPreferences.Current.SearchTypeSorting = SearchTypeSorting.Prioritized;
                        BuildSearchMenus();
                    }
                    else
                    if (TimeflowPreferences.Current.SearchTypeSorting == SearchTypeSorting.Prioritized) {
                        TimeflowPreferences.Current.SearchTypeSorting = SearchTypeSorting.Count;
                        BuildSearchMenus();
                    }
                    else {
                        TimeflowPreferences.Current.SearchTypeSorting = SearchTypeSorting.Alphabetical;
                        BuildSearchMenus();
                    }
                }

                // Clear search button
                bool canClear = (!string.IsNullOrEmpty(SearchTerm)) || _SearchTypeIndex > 0 || (_SelectedWordIndices != null && _SelectedWordIndices.Count > 0);
                EditorGUI.BeginDisabledGroup(!canClear);
                if (GUI.Button(clearSearchRect, AxonUI.ClearSearchLabel, AxonUI.ClearSearchStyle)) {
                    SearchTerm = "";
                    _SearchTypeIndex = 0;
                    if (_SelectedWordIndices != null) _SelectedWordIndices.Clear();
                    AxonGUI.FocusControl("SearchDisplay");
                    ApplyFilter();
                    BuildSearchMenus();
                }
                EditorGUI.EndDisabledGroup();

                GUI.Box(menuSearchFieldLine, "", AxonUI.SolidStyle);
                GUI.color = AxonColor.Default;
            }
        }

        #endregion

    }

}//AxonGenesis
#endif
