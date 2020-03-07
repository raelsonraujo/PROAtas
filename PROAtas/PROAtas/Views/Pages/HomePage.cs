﻿using CSharpForMarkup;
using PROAtas.Assets.Styles;
using PROAtas.Assets.Theme;
using PROAtas.Controls;
using PROAtas.Converters;
using PROAtas.Core;
using PROAtas.ViewModel;
using PROAtas.ViewModel.Elements;
using PROAtas.Views.DataTemplates;
using PROAtas.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using static CSharpForMarkup.EnumsForGridRowsAndColumns;

namespace PROAtas.Views.Pages
{
    public class HomePage : BasePage<HomeViewModel>
    {
        enum Row { Content, Banner }

        public HomePage() => Build();

        private void Build()
        {
            Shell.SetFlyoutBehavior(this, FlyoutBehavior.Flyout);

            Title = "PRO Atas";

            var app = App.Current;
            var vm = ViewModel = App.Current.homeViewModel;

            var searchHandler = new CustomSearchHandler
            {
                Placeholder = "Pesquisa...", BackgroundColor = Colors.TextIcons,
            };
            searchHandler.SetBinding(CustomSearchHandler.SearchCommandProperty, nameof(vm.SearchMinute));
            Shell.SetSearchHandler(this, searchHandler);

            Content = new Grid
            {
                Children =
                {
                    new Grid
                    {
                        RowDefinitions = Rows.Define(
                            (Row.Content, GridLength.Star),
                            (Row.Banner, new GridLength(50))),

                        Children =
                        {
                            new CollectionView { ItemTemplate = MinuteTemplate.New() } .VerticalListStyle() .Single()
                                .Assign(out CollectionView optionCollection)
                                .Row(0)
                                .Bind(CollectionView.ItemsSourceProperty, nameof(vm.Minutes))
                                .Bind(CollectionView.SelectedItemProperty, nameof(vm.SelectedMinute), mode: BindingMode.TwoWay),

                            new Button { ImageSource = Images.Add, Margin = 10 } .Standard() .Bottom() .Right() .Round()
                                .Row(0)
                                .Bind(nameof(vm.CreateMinute)),

                            new AdMobView { AdUnitId = Constants.AdHome }
                                .Row(1),
                        }
                    } .Standard(),
                    
                    new OptionDialog("       Opções       ", "       Editar       ", "       Gerar Word       ", "       Gerar PDF       ", "       Deletar       ", Images.Edit, Images.Word, Images.PDF, Images.Delete) { LastWarning = "Deseja mesmo remover este arquivo?", Padding = -5 }
                        .Bind(OptionDialog.IsOpenProperty, nameof(vm.SelectedMinute), converter: new NullToBool())
                        .Bind(OptionDialog.FirstCommandProperty, nameof(vm.EditMinute))
                        .Bind(OptionDialog.SecondCommandProperty, nameof(vm.PrintWord))
                        .Bind(OptionDialog.ThirdCommandProperty, nameof(vm.PrintPDF))
                        .Bind(OptionDialog.LastCommandProperty, nameof(vm.DeleteMinute))
                        .Invoke(l => l.Close += () => vm.SelectedMinute = null),

                    new LoadingDialog()
                        .Bind(LoadingDialog.IsOpenProperty, nameof(vm.IsLoading)),
                }
            } .Standard();
        }
    }
}
