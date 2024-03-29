﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PROAtas.Assets.Styles
{
    public static class CollectionViewStyle
    {
        public static TCollectionView GridStyle<TCollectionView>(this TCollectionView collection, ItemsLayoutOrientation orientation, int span, int spacing) where TCollectionView : CollectionView
        {
            collection.ItemsLayout = new GridItemsLayout(orientation)
            {
                Span = span,
                VerticalItemSpacing = spacing,
                HorizontalItemSpacing = spacing,
            };

            return collection;
        }

        public static TCollectionView VerticalListStyle<TCollectionView>(this TCollectionView collection) where TCollectionView : CollectionView
        {
            collection.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                ItemSpacing = 5,
            };

            return collection;
        }

        public static TCollectionView HorizontalListStyle<TCollectionView>(this TCollectionView collection) where TCollectionView : CollectionView
        {
            collection.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
            {
                ItemSpacing = 5,
            };

            return collection;
        }

        public static TCollectionView SingleSelection<TCollectionView>(this TCollectionView collection) where TCollectionView : CollectionView
        {
            collection.SelectionMode = SelectionMode.Single;

            return collection;
        }

        public static TCollectionView NoSelection<TCollectionView>(this TCollectionView collection) where TCollectionView : CollectionView
        {
            collection.SelectionMode = SelectionMode.None;

            return collection;
        }
    }
}
