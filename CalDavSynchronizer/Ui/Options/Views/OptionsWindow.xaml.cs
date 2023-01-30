﻿// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows;
using CalDavSynchronizer.Ui.Options.ViewModels;

namespace CalDavSynchronizer.Ui.Options.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
            DataContextChanged += OptionsWindow_DataContextChanged;
            Closing += OnWindowClosing;
        }

        private void OptionsWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is OptionsCollectionViewModel newViewModel)
            {
                newViewModel.CloseRequested += ViewModel_CloseRequested;
            }

            if (e.OldValue is OptionsCollectionViewModel oldViewModel)
            {
                oldViewModel.CloseRequested -= ViewModel_CloseRequested;
            }
        }

        private void ViewModel_CloseRequested(object sender, CloseEventArgs e)
        {
            DialogResult = e.IsAcceptedByUser;
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (DataContext is OptionsCollectionViewModel viewModel)
            {
                if (!DialogResult.HasValue)
                {
                    var result = MessageBox.Show("Do you want to save profiles?", ComponentContainer.MessageBoxTitle, MessageBoxButton.YesNo);
                    DialogResult = (result == MessageBoxResult.Yes);
                }

                if (DialogResult.Value)
                {
                    e.Cancel = !viewModel.Validate();
                }
            }
        }
    }
}