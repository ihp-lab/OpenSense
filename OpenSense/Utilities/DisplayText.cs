// !!! REFACTOR !!!

// <copyright file="DisplayText.cs" company="USC ICT">
// Copyright (c) USC ICT. All rights reserved.
// </copyright>

namespace OpenSense.Utilities {
    using System.ComponentModel;

    /// <summary>
    /// DisplayText is a helper class.
    /// </summary>
    public class DisplayText : INotifyPropertyChanged {
        private string text;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayText"/> class.
        /// </summary>
        public DisplayText() {
            this.text = string.Empty;
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text {
            get => this.text;

            set {
                this.text = value;
                this.OnPropertyChanged(nameof(this.Text));
            }
        }

        /// <summary>
        /// Method to update the text.
        /// </summary>
        /// <param name="str">The updated text.</param>
        public void UpdateText(string str) {
            this.Text = str;
        }

        /// <summary>
        /// Helper function for firing an event when the image property changes.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        private void OnPropertyChanged(string propertyName) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}