#nullable enable

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using OpenSense.Components;
using OpenSense.Pipeline;

namespace OpenSense.WPF.Pipeline {
    public partial class CreateComponentConfigurationWindow : Window {

        private static readonly Regex WhitespaceRegex = new Regex(@"\s+");

        private readonly IComponentMetadata[] _components;

        public CreateComponentConfigurationWindow() {
            InitializeComponent();
            _components = new ComponentManager().Components
                .OrderBy(c => c.Name)
                .ToArray();
            DataGridComponents.ItemsSource = _components;
        }

        public ComponentConfiguration? Result { get; private set; }

        private void ButtonYes_Click(object sender, RoutedEventArgs e) {
            if (DataGridComponents.SelectedItem is not IComponentMetadata metadata) {
                return;
            }
            var config = metadata.CreateConfiguration();
            config.Name = metadata.Name;
            config.Description = "";
            Result = config;
            DialogResult = true;
        }

        private void TextBoxFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            IComponentMetadata[] data;
            var text = WhitespaceRegex.Replace(TextBoxFilter.Text, "");
            if (string.IsNullOrEmpty(text)) {
                data = _components;
            } else {
                data = _components
                    .Select(c => new MatchInfo(c, text))
                    .Where(t => t.NameDist < t.Name.Length && t.DescDist < t.Desc.Length)
                    .OrderByDescending(t => t.NameSeqLen)
                    .ThenByDescending(t => t.NameSubLen)
                    .ThenBy(t => t.NameDist)
                    .ThenByDescending(t => t.DescSeqLen)
                    .ThenByDescending(t => t.DescSubLen)
                    .ThenBy(t => t.DescDist)
                    .ThenBy(t => t.Component.Name)
                    .Select(t => t.Component)
                    .ToArray();
            }
            DataGridComponents.ItemsSource = data;
            DataGridComponents.SelectedIndex = 0;
        }

        #region Helpers
        private static int MinDistance(string a, string b) {
            a = a.ToLowerInvariant();
            b = b.ToLowerInvariant();

            var dp = new int[a.Length + 1, b.Length + 1];

            for (var i = 0; i <= a.Length; i++) {
                for (var j = 0; j <= b.Length; j++) {
                    if (i == 0) {
                        dp[i, j] = j;  // If first string is empty, only option is to insert all characters of second string
                    } else if (j == 0) {
                        dp[i, j] = i;  // If second string is empty, only option is to remove all characters of first string
                    } else if (a[i - 1] == b[j - 1]) {
                        dp[i, j] = dp[i - 1, j - 1];  // If last characters are same, ignore them and get count for remaining strings.
                    } else {
                        dp[i, j] = 1 + Math.Min(dp[i - 1, j],          // Remove
                                        Math.Min(dp[i, j - 1],          // Insert
                                                 dp[i - 1, j - 1]));    // Replace
                    }
                }
            }

            return dp[a.Length, b.Length];
        }

        private static int LongestCommonSubstringLength(string a, string b) {
            a = a.ToLowerInvariant();
            b = b.ToLowerInvariant();

            var dp = new int[a.Length + 1, b.Length + 1];
            int maxLength = 0;

            for (var i = 1; i <= a.Length; i++) {
                for (var j = 1; j <= b.Length; j++) {
                    if (a[i - 1] == b[j - 1]) {
                        dp[i, j] = dp[i - 1, j - 1] + 1;
                        maxLength = Math.Max(maxLength, dp[i, j]);
                    } else {
                        dp[i, j] = 0; // Unlike classic LCS problem, we reset the count for longest common substring
                    }
                }
            }

            return maxLength;
        }

        private static int LongestCommonSubsequenceLength(string a, string b) {
            a = a.ToLowerInvariant();
            b = b.ToLowerInvariant();

            var dp = new int[a.Length + 1, b.Length + 1];

            for (var i = 0; i <= a.Length; i++) {
                for (var j = 0; j <= b.Length; j++) {
                    if (i == 0 || j == 0) {
                        dp[i, j] = 0;
                    } else if (a[i - 1] == b[j - 1]) {
                        dp[i, j] = dp[i - 1, j - 1] + 1;
                    } else {
                        dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                    }
                }
            }

            return dp[a.Length, b.Length];
        } 
        #endregion

        #region Classes
        private readonly struct MatchInfo {

            public IComponentMetadata Component { get; }

            public string Name { get; }

            public int NameDist { get; }

            public int NameSeqLen { get; }

            public int NameSubLen { get; }

            public string Desc { get; }

            public int DescDist { get; }

            public int DescSeqLen { get; }

            public int DescSubLen { get; }

            public MatchInfo(IComponentMetadata component, string text) {
                Component = component;
                Name = WhitespaceRegex.Replace(component.Name, "");
                NameDist = MinDistance(Name, text);
                NameSeqLen = LongestCommonSubsequenceLength(Name, text);
                NameSubLen = LongestCommonSubstringLength(Name, text);
                Desc = WhitespaceRegex.Replace(component.Description, "");
                DescDist = MinDistance(Desc, text);
                DescSeqLen = LongestCommonSubsequenceLength(Desc, text);
                DescSubLen = LongestCommonSubstringLength(Desc, text);
            }

        }
        #endregion
    }
}
