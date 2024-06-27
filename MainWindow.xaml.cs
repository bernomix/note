﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Note {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow {
    private string _currentFilePath = string.Empty;
    private bool _isSaved = true;
    private Stack<string> _redoStack = new Stack<string>();
    private Stack<string> _undoStack = new Stack<string>();

    public MainWindow() {
      InitializeComponent();
    }

    private bool ConfirmSaveIfNeeded() {
      if (!_isSaved) {
        MessageBoxResult result = MessageBox.Show("現在の内容が保存されていません。保存しますか？", "警告", MessageBoxButton.YesNoCancel,
          MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes) {
          MenuFileSave_OnClick(this, new RoutedEventArgs());
        } else if (result == MessageBoxResult.Cancel) {
          return false;
        }
      }

      return true;
    }

    private void UpdateTextLength() {
      string textForTextLength = Textbox.Text.Replace("\r\n", "").Replace("\n", "");
      int textLength = textForTextLength.Length;
      if (textLength >= 100000) {
        StatusTextLength.Text = $"文字数：{textLength}（文字数制限に到達しました！）";
      } else {
        StatusTextLength.Text = $"文字数：{textLength}";
      }

      StatusProgressbar.Value = textLength;
    }

    private void UpdateFileName() {
      if (_currentFilePath == string.Empty) {
        StatusFileName.Text = "ファイルなし";
      } else {
        StatusFileName.Text = Path.GetFileName(_currentFilePath);
      }
    }

    private void UpdateLineColumn() {
      int lineIndex = Textbox.GetLineIndexFromCharacterIndex(Textbox.CaretIndex) + 1;
      int columnIndex = Textbox.CaretIndex -
                        Textbox.GetCharacterIndexFromLineIndex(
                          Textbox.GetLineIndexFromCharacterIndex(Textbox.CaretIndex));
      StatusCursorLineColumn.Text = $"行 {lineIndex}, 列 {columnIndex}";
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
      UpdateTextLength();
      UpdateFileName();
      UpdateLineColumn();
    }

    private void MainWindow_OnClosing(object sender, CancelEventArgs e) {
      if (!ConfirmSaveIfNeeded()) {
        e.Cancel = true;
      }
    }

    private void Textbox_OnTextChanged(object sender, TextChangedEventArgs e) {
      _isSaved = false;

      if (Textbox.IsFocused) {
        _undoStack.Push(Textbox.Text);
        _redoStack.Clear();
      }

      UpdateTextLength();
      UpdateFileName();
      UpdateLineColumn();
    }

    private void Textbox_OnSelectionChanged(object sender, RoutedEventArgs e) {
      UpdateLineColumn();
    }

    #region Menu

    #region Application

    private void MenuApplicationAbout_OnClick(object sender, RoutedEventArgs e) {
      MessageBox.Show("Note - 0.1.0 \n \u00a9 2024 Bernomix - CC BY-SA 4.0", "Noteについて", MessageBoxButton.OK,
        MessageBoxImage.Information);
    }

    private void MenuApplicationAboutIcon_OnClick(object sender, RoutedEventArgs e) {
      MessageBox.Show(
        "Post it icons created by Saepul Nahwan - Flaticon: https://www.flaticon.com/free-icons/post-it",
        "アイコンについて", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void MenuApplicationExit_OnClick(object sender, RoutedEventArgs e) {
      if (!ConfirmSaveIfNeeded()) {
        return;
      }

      Application.Current.Shutdown();
    }

    #endregion

    #region File

    private void MenuFileNew_OnClick(object sender, RoutedEventArgs e) {
      if (!ConfirmSaveIfNeeded()) {
        return;
      }

      Textbox.Clear();

      _currentFilePath = string.Empty;
      _isSaved = true;

      UpdateFileName();
      UpdateLineColumn();
    }


    private void MenuFileOpen_OnClick(object sender, RoutedEventArgs e) {
      if (!ConfirmSaveIfNeeded()) {
        return;
      }

      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

      if (openFileDialog.ShowDialog() == true) {
        _currentFilePath = openFileDialog.FileName;

        Textbox.Text = File.ReadAllText(_currentFilePath);

        _isSaved = true;
      }

      UpdateFileName();
      UpdateLineColumn();
    }


    private void MenuFileSave_OnClick(object sender, RoutedEventArgs e) {
      if (string.IsNullOrEmpty(_currentFilePath)) {
        MenuFileSaveAs_OnClick(sender, e);
      } else {
        File.WriteAllText(_currentFilePath, Textbox.Text);
        _isSaved = true;
      }

      UpdateFileName();
      UpdateLineColumn();
    }


    private void MenuFileSaveAs_OnClick(object sender, RoutedEventArgs e) {
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

      if (saveFileDialog.ShowDialog() == true) {
        _currentFilePath = saveFileDialog.FileName;

        File.WriteAllText(_currentFilePath, Textbox.Text);

        _isSaved = true;
      }

      UpdateFileName();
      UpdateLineColumn();
    }

    #endregion

    #region Edit

    private void MenuEditUndo_OnClick(object sender, RoutedEventArgs e) {
      if (_undoStack.Count > 0) {
        _redoStack.Push(Textbox.Text);


        Textbox.TextChanged -= Textbox_OnTextChanged;
        Textbox.Text = _undoStack.Pop();
        Textbox.TextChanged += Textbox_OnTextChanged;
      }
    }

    private void MenuEditRedo_OnClick(object sender, RoutedEventArgs e) {
      if (_redoStack.Count > 0) {
        _undoStack.Push(Textbox.Text);

        Textbox.TextChanged -= Textbox_OnTextChanged;
        Textbox.Text = _redoStack.Pop();
        Textbox.TextChanged += Textbox_OnTextChanged;
      }
    }

    private void MenuEditCut_OnClick(object sender, RoutedEventArgs e) {
      if (Textbox.SelectedText.Length > 0) {
        Textbox.Cut();
      }
    }

    private void MenuEditCopy_OnClick(object sender, RoutedEventArgs e) {
      if (Textbox.SelectedText.Length > 0) {
        Textbox.Copy();
      }
    }

    private void MenuEditPaste_OnClick(object sender, RoutedEventArgs e) {
      Textbox.Paste();
    }

    private void MenuEditFont_OnClick(object sender, RoutedEventArgs e) {
      FontDialog fontDialog = new FontDialog();

      DialogResult result = fontDialog.ShowDialog();

      if (result == System.Windows.Forms.DialogResult.OK && fontDialog.Font != null) {
        Textbox.FontFamily = new System.Windows.Media.FontFamily(fontDialog.Font.Name);
        Textbox.FontSize = fontDialog.Font.Size;
        Textbox.FontStyle = fontDialog.Font.Italic ? FontStyles.Italic : FontStyles.Normal;
        Textbox.FontWeight = fontDialog.Font.Bold ? FontWeights.Bold : FontWeights.Normal;
      }
    }

    #endregion

    #region Input

    private void MenuInputDay_OnClick(object sender, RoutedEventArgs e) {
      string dateText = DateTime.Now.ToString("yyyy MM/dd");
      int selectionStart = Textbox.SelectionStart;
      int selectionLength = Textbox.SelectionLength;

      Textbox.Text = Textbox.Text.Remove(selectionStart, selectionLength);
      Textbox.Text = Textbox.Text.Insert(selectionStart, dateText);
      Textbox.SelectionStart = selectionStart + dateText.Length;
    }

    private void MenuInputTime_OnClick(object sender, RoutedEventArgs e) {
      string dateText = DateTime.Now.ToString("HH:mm:ss");
      int selectionStart = Textbox.SelectionStart;
      int selectionLength = Textbox.SelectionLength;

      Textbox.Text = Textbox.Text.Remove(selectionStart, selectionLength);
      Textbox.Text = Textbox.Text.Insert(selectionStart, dateText);
      Textbox.SelectionStart = selectionStart + dateText.Length;
    }

    #endregion

    #endregion
  }
}