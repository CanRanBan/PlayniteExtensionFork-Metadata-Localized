﻿using Newtonsoft.Json;
using Playnite.SDK;
using PluginCommon;
using PluginCommon.PlayniteResources;
using PluginCommon.PlayniteResources.API;
using PluginCommon.PlayniteResources.Common;
using PluginCommon.PlayniteResources.Converters;
using MetadataLocal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace MetadataLocal.Views
{
    /// <summary>
    /// Logique d'interaction pour MetadataLocalStoreSelection.xaml
    /// </summary>
    public partial class MetadataLocalStoreSelection : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private IPlayniteAPI _PlayniteApi;

        public string _PluginUserDataPath { get; set; }
        public SearchResult StoreResult { get; set; } = new SearchResult();

        public bool IsFirstLoad = true;

        public MetadataLocalStoreSelection(IPlayniteAPI PlayniteApi, string StoreDefault, string GameName, string PluginUserDataPath)
        {
            _PlayniteApi = PlayniteApi;
            _PluginUserDataPath = PluginUserDataPath;

            InitializeComponent();

            PART_DataLoadWishlist.Visibility = Visibility.Collapsed;
            PART_GridData.IsEnabled = true;

            switch (StoreDefault.ToLower())
            {
                case "steam":
                    rbSteam.IsChecked = true;
                    break;

                case "origin":
                    rbOrigin.IsChecked = true;
                    break;

                case "epic":
                    rbEpic.IsChecked = true;
                    break;

                case "xbox":
                    rbXbox.IsChecked = true;
                    break;

                default:
                    rbSteam.IsChecked = true;
                    break;
            }

            SearchElement.Text = GameName;

            SearchElements();
            IsFirstLoad = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lbSelectable.ItemsSource = null;
            lbSelectable.UpdateLayout();
        }


        private void BtCancel_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).Close();
        }

        private void BtOk_Click(object sender, RoutedEventArgs e)
        {
            StoreResult = (SearchResult)lbSelectable.SelectedItem;
            ((Window)this.Parent).Close();
        }


        private void LbSelectable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btOk.IsEnabled = true;
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchElements();
        }

        private void Rb_Check(object sender, RoutedEventArgs e)
        {
            if (!IsFirstLoad)
            {
                RadioButton rb = sender as RadioButton;
                if (rb.Name == "rbSteam" && (bool)rb.IsChecked)
                {
                    SearchElements();
                }

                if (rb.Name == "rbEpic" && (bool)rb.IsChecked)
                {
                    SearchElements();
                }

                if (rb.Name == "rbOrigin" && (bool)rb.IsChecked)
                {
                    SearchElements();
                }

                if (rb.Name == "rbXbox" && (bool)rb.IsChecked)
                {
                    SearchElements();
                }
            }
        }

        private void SearchElement_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ButtonSearch_Click(null, null);
            }
        }


        private void SearchElements()
        {
            bool IsSteam = (bool)rbSteam.IsChecked;
            bool IsOrigin = (bool)rbOrigin.IsChecked;
            bool IsEpic = (bool)rbEpic.IsChecked;
            bool IsXbox = (bool)rbXbox.IsChecked;

            PART_DataLoadWishlist.Visibility = Visibility.Visible;
            PART_GridData.IsEnabled = false;

            string gameSearch = RemoveAccents(SearchElement.Text);

            lbSelectable.ItemsSource = null;
            Task task = Task.Run(() => LoadData(gameSearch, IsSteam, IsOrigin, IsEpic, IsXbox))
                .ContinueWith(antecedent =>
                {
                    this.Dispatcher.Invoke(new Action(() => {
                        if (antecedent.Result != null)
                        {
                            lbSelectable.ItemsSource = antecedent.Result;
                        }
            
                        PART_DataLoadWishlist.Visibility = Visibility.Collapsed;
                        PART_GridData.IsEnabled = true;

#if DEBUG
                        logger.Debug($"MetadataLocal - SearchElements({gameSearch}) - " + JsonConvert.SerializeObject(antecedent.Result));
#endif
                    }));
                });
        }

        private string RemoveAccents(string text)
        {
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        }

        private async Task<List<SearchResult>> LoadData(string SearchElement, bool IsSteam, bool IsOrigin, bool IsEpic, bool IsXbox)
        {
            var results = new List<SearchResult>();

            if (IsSteam)
            {
                results = MetadataLocalProvider.GetMultiSteamData(SearchElement);
            }

            if (IsOrigin)
            {
                results = MetadataLocalProvider.GetMultiOriginData(SearchElement, _PluginUserDataPath);
            }

            if (IsEpic)
            {
                results = MetadataLocalProvider.GetMultiEpicData(SearchElement);
            }

            if (IsXbox)
            {
                results = MetadataLocalProvider.GetMultiXboxData(_PlayniteApi, SearchElement);
            }

            return results;
        }
    }
}
