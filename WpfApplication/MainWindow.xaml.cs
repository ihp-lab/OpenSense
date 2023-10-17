﻿#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Pipeline;
using OpenSense.WPF.Pipeline;
using OpenSense.WPF.Views.Runners;
using OpenSense.WPF.Widgets;

namespace OpenSense.WPF {

    public partial class MainWindow : Window {

        private readonly ObservableLogWriter logWriter = new ObservableLogWriter();

        private TextWriter? stdOut;
        private TextWriter? stdErr;

        /// <summary>
        /// Use as binding target, must be public.
        /// </summary>
        public ObservableLogWriter? LogWriter => logWriter;

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e) {
            stdOut = Console.Out;
            stdErr = Console.Error;
            Console.SetOut(logWriter);
            Console.SetError(logWriter);
        }

        private void Window_Closed(object sender, EventArgs e) {
            Debug.Assert(stdOut is not null);
            Debug.Assert(stdErr is not null);
            Console.SetOut(stdOut);
            Console.SetError(stdErr);
            stdOut = null;
            stdErr = null;
        }

        private void ButtonPipelineEditor_Click(object sender, RoutedEventArgs e) {
            var editor = new PipelineEditorWindow() { 
                //Owner = this,
            };
            editor.Show();
        }

        private void ButtonPipelineRunner_Click(object sender, RoutedEventArgs e) {
            var executor = new RunnerWindow(new PipelineConfiguration()) { 
                //Owner = this,
            };
            executor.Show();
        }


        #region widgets
        private void ItemsControlWidgets_Initialized(object sender, System.EventArgs e) {
            ItemsControlWidgets.ItemsSource = new WidgetManager().Widgets;
        }

        private void WidgetItem_Click(object sender, RoutedEventArgs e) {
            var fe = (FrameworkElement)sender;
            var widgetMetadata = (IWidgetMetadata)fe.DataContext;
            var win = widgetMetadata.Create();
            win.ShowDialog();
        }

        #endregion

        private void ButtonClearOutput_Click(object sender, RoutedEventArgs e) {
            logWriter.Clear();
        }

        private bool scrollToEnd = true;

        private void ScrollViewerOutput_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            scrollToEnd = ScrollViewerOutput.VerticalOffset == ScrollViewerOutput.ScrollableHeight;
        }

        private bool byTextUpdate;
        private int selectionStart = 0;
        private int selectionLength = 0;

        private void TextBoxOutput_SelectionChanged(object sender, RoutedEventArgs e) {
            if (byTextUpdate) {
                return;
            }
            selectionStart = TextBoxOutput.SelectionStart;
            selectionLength = TextBoxOutput.SelectionLength;
        }

        private void TextBoxOutput_TextChanged(object sender, TextChangedEventArgs e) {
            if (scrollToEnd) {
                ScrollViewerOutput.ScrollToEnd();
            }
            byTextUpdate = true;
            TextBoxOutput.Select(selectionStart, selectionLength);
            byTextUpdate = false;
        }
    }
}
