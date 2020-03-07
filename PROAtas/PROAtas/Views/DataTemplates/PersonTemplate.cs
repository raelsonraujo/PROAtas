﻿using CSharpForMarkup;
using PROAtas.Assets.Components;
using PROAtas.Assets.Styles;
using PROAtas.Assets.Theme;
using PROAtas.ViewModel;
using PROAtas.ViewModel.Elements;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using static CSharpForMarkup.EnumsForGridRowsAndColumns;

namespace PROAtas.Views.DataTemplates
{
    public class PersonTemplate
    {
        enum Col { Reorder, Content, Action }

        public static DataTemplate New(object viewModel) => new DataTemplate(() =>
        {
            // Outer Grid
            var grid = new Grid
            {
                BackgroundColor = Colors.Primary,
                Padding = 5,

                Children =
                {
                    // Outer Border
                    new Frame
                    {
                        Padding = 5, CornerRadius = 6, BackgroundColor = Colors.Accent,

                        // Frame content
                        Content = new Grid
                        {
                            ColumnDefinitions = Columns.Define(
                                (Col.Reorder, GridLength.Auto),
                                (Col.Content, GridLength.Star),
                                (Col.Action, GridLength.Auto)),

                            RowSpacing = 0, ColumnSpacing = 5,

                            Children =
                            {
                                new Frame { } .FramedCustomEntry(Images.PersonBlack, "Nome da pessoa", nameof(MinuteViewModel.SavePerson), nameof(PersonElement.Name), isSavingPath: nameof(PersonElement.IsSaving), saveSource: viewModel, hasSaveParameter: true)
                                    .Col(Col.Content),

                                new Button { ImageSource = Images.Delete } .Danger() .Round(40) .Center()
                                    .Col(Col.Action)
                                    .Bind(Button.CommandParameterProperty)
                                    .Bind(nameof(MinuteViewModel.DeletePerson), source: viewModel)
                            }
                        }
                    },
                }
            };

            return grid;
        });
    }
}
