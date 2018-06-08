using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;

namespace ySlide
{
    public class DesignerItemDecorator : Control
    {
        private Adorner adorner;

        public bool ShowDecorator
        {
            get { return (bool)GetValue(ShowDecoratorProperty); }
            set { SetValue(ShowDecoratorProperty, value); }
        }

        public static readonly DependencyProperty ShowDecoratorProperty =
            DependencyProperty.Register("ShowDecorator", typeof(bool), typeof(DesignerItemDecorator),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(ShowDecoratorProperty_Changed)));

        public DesignerItemDecorator()
        {
            Unloaded += new RoutedEventHandler(this.DesignerItemDecorator_Unloaded);
        }

        private void HideAdorner()
        {
            if (adorner != null)
            {
                adorner.Visibility = Visibility.Hidden;
                //adorner = null;
            }
        }

        private void ShowAdorner()
        {
            if (adorner == null)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);

                if (adornerLayer != null)
                {
                    ContentControl designerItem = this.DataContext as ContentControl;
                    //Canvas canvas = VisualTreeHelper.GetParent(designerItem) as Canvas;
                    adorner = new ResizeRotateAdorner(designerItem);
                    adornerLayer.Add(adorner);

                    if (this.ShowDecorator)
                    {
                        adorner.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        adorner.Visibility = Visibility.Hidden;
                    }
                }
            }
            else
            {
                adorner.Visibility = Visibility.Visible;
            }
        }

        private void DesignerItemDecorator_Unloaded(object sender, RoutedEventArgs e)
        {
            if (adorner != null)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    adornerLayer.Remove(adorner);
                }

                adorner = null;
            }
        }

        private static void ShowDecoratorProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DesignerItemDecorator decorator = (DesignerItemDecorator)d;
            bool showDecorator = (bool)e.NewValue;

            if (showDecorator)
            {
                decorator.ShowAdorner();
            }
            else
            {
                decorator.HideAdorner();
            }
        }
    }
}
