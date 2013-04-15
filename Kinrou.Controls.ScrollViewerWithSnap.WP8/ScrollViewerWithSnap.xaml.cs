/// Author:             sebastien jouhans
/// Date:               06/03/2013
/// Description:        
/// 
///                     
/// Updates:   
/// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Input;
using Kinrou.Controls.Events;
using Kinrou.Controls;
using System.Windows.Markup;
using System.Diagnostics;

namespace Kinrou.Controls
{

    [ContentProperty("Children")]
    public partial class ScrollViewerWithSnap : UserControl
    {

        public delegate void SelectedItemChangedEventHandler(object sender, EventArgs data);
        public event SelectedItemChangedEventHandler selectedItemChanged;


        public delegate void TansitionCompleteEventHandler(object sender, EventArgs data);
        public event TansitionCompleteEventHandler transitionComplete;



        // stores the vertical offset of the scrollviewer when the user touches the screen
        private double _originalVerticalOffset = 0;

        // stores the old vertical offset when the vertical offset updates
        private double _oldVerticalOffset = 0;

        // stores the new vertical offset  when the vertical offset updates
        private double _newVerticalOffset = 0;

        // stores the curretly shown item index
        private int _itemIndex = 0;

        // stores the total number of items
        private int _totalNumOfItems;

        // stores whether the user's finger has left the screen
        private bool _hasCompletedFired = false;

        // transition storyboard
        private Storyboard _storyboard;

        // this variable is used to track whether the reset of the scrollviewer was requested
        // it will stop the o.scrollViewer.ScrollToVerticalOffset() to be called in the onVerticalOffsetChanged mehtod
        // it look like scrollViewer.VerticalOffset is not being updated rapidly enough
        // because when calling this method scrollViewer.ScrollToVerticalOffset(0) the event handler onVerticalOffsetChanged 
        // has the previous updated value not 0
        private bool _hasToReset = false;


        public static readonly DependencyProperty verticalOffsetProperty =
            DependencyProperty.Register(
                "verticalOffset",
                typeof(double),
                typeof(ScrollViewerWithSnap),
                new PropertyMetadata(onVerticalOffsetChanged));



        public static readonly DependencyProperty ChildrenProperty = 
            DependencyProperty.Register(
            "Children",
            typeof(UIElementCollection),
            typeof(ScrollViewerWithSnap),
            null);




        public ScrollViewerWithSnap()
        {
            InitializeComponent();

            Children = content.Children;
        }

        public void resetViewScroller()
        {
            scrollViewer.ScrollToVerticalOffset(0);
            _itemIndex = 0;
            _hasToReset = true;
        }
        

        /*
         * Apply Templete event handler
         * 
         * used to initialsed the different part of the new functionality added to the listbox control
         */ 
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            (Parent as UIElement).MouseLeftButtonUp += mouseLeftButtonUp;

            AddHandler(ScrollViewer.ManipulationStartedEvent, (EventHandler<ManipulationStartedEventArgs>)scrollViewerManipulationStarted, true);
            AddHandler(ScrollViewer.ManipulationCompletedEvent, (EventHandler<ManipulationCompletedEventArgs>)scrollViewerManipulationCompleted, true);
            //AddHandler(ListBox.ManipulationDeltaEvent, (EventHandler<ManipulationDeltaEventArgs>)listboxManipulationDelta, true); 


            scrollViewer.ManipulationMode = ManipulationMode.Control;
            scrollViewer.ApplyTemplate();

            ScrollBar verticalBar = ((FrameworkElement)VisualTreeHelper.GetChild(scrollViewer, 0)).FindName("VerticalScrollBar") as ScrollBar;
            verticalBar.ValueChanged += verticalBarValueChanged;

            _totalNumOfItems = Children.Count;
        }


        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty); }
            private set { SetValue(ChildrenProperty, value); }
        }


        /*
         * vertical offset property that is set to the vertical Offset Property
         */
        public double verticalOffset
        {
            get { return (double)GetValue(verticalOffsetProperty); }
            set { SetValue(verticalOffsetProperty, value); }
        }


        /*
       * vertical offset update event handler
       * 
       * is triggered every time the vertical offset property is updated
       * the new value (vertical offset) is then used to update the scroll vertical offset of the listbox's scrollviewer
       */ 
        public static void onVerticalOffsetChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var o = (ScrollViewerWithSnap)sender;
            if (null != o.scrollViewer && !o._hasToReset)
            {
                o.scrollViewer.ScrollToVerticalOffset((double)(e.NewValue));
            }
        }




        /*
         * scrollviewer manipulation started event handler
         * 
         * is triggered every time the user's finger enters the screen
         */ 
        private void scrollViewerManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            //Debug.WriteLine("LB_ManipulationCompleted - " + e.TotalManipulation.Translation.Y.ToString());
            _hasCompletedFired = false;
            _hasToReset = false;
            if (_storyboard != null) _storyboard.Pause();
            _originalVerticalOffset = _oldVerticalOffset;

        }

        /*
         * scrollviewer manipulation complete event handler
         * 
         * is triggered every time the user's finger leaves the screen
         */
        private void scrollViewerManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            //Debug.WriteLine("LB_ManipulationCompleted - " + e.TotalManipulation.Translation.Y.ToString());

            if (!_hasCompletedFired)
            {                
                setItem();
                onSelectedItemChanged(this, new ScrollViewerWithSnapSelectedItemChangedEventArgs(_itemIndex));
            }
            _hasCompletedFired = true;
        }




        /*
         * vertical bar position event handler
         * 
         * is triggered every time the position of the scroll bar changes, this reflects the position of the scrollviewer
         */
        private void verticalBarValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _oldVerticalOffset = _newVerticalOffset;          
            _newVerticalOffset = e.NewValue;

            //Debug.WriteLine(string.Format("new={0} , old={1}" , _newVerticalOffset, _oldVerticalOffset));
        }


        private void onSelectedItemChanged(object sender, EventArgs data)
        {
            if (selectedItemChanged != null)
            {
                selectedItemChanged(this, data);
            }
        }


        private void onTransitionComplete(object sender, EventArgs data)
        {
            if (transitionComplete != null)
            {
                transitionComplete(this, data);
            }
        }


        /*
         * mouse up event handler
         * 
         * is triggered every time the user's finger leaves the screen
         * 
         * this was implemented due to a bug with the list box where the manipulation complete event does not always fire
         */
        private void mouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine(string.Format("mouseLeftButtonUp -- new={0} , old={1}", _newVerticalOffset, _oldVerticalOffset));

            if (!_hasCompletedFired)
            {
                setItem();
                onSelectedItemChanged(this, new ScrollViewerWithSnapSelectedItemChangedEventArgs(_itemIndex));
            }
            _hasCompletedFired = true;
        }


        /*
         * triggers storyboard to animated the scrollviewer depending on the current vertical offset and the current item index
         */
        private void transition()
        {
            DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
            animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = _newVerticalOffset });
            animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(300), Value = _itemIndex * ActualHeight, EasingFunction = new System.Windows.Media.Animation.CubicEase() { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut } });
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath("verticalOffset"));
            animation.Completed += transitionCompleteEventHanlder;

            _storyboard = new Storyboard();
            _storyboard.Children.Add(animation);
            _storyboard.Begin();
        }


        private void transitionCompleteEventHanlder(object sender, EventArgs e)
        {
            (sender as DoubleAnimationUsingKeyFrames).Completed -= transitionCompleteEventHanlder;
            onTransitionComplete(this, new ScrollViewerWithSnapTransitionCompleteEventArg(_itemIndex));
        }


        /*
         * works out wheather the transition animation should go up or down
         */
        private void setItem()
        {
            double diff = _oldVerticalOffset - _newVerticalOffset;

            if (Math.Abs(_originalVerticalOffset - _newVerticalOffset) > .05)
            {
                if (diff > 0)
                {
                    if (_itemIndex > 0) _itemIndex--;
                    //Debug.WriteLine("1");
                }
                else if (diff < 0)
                {
                    if (_itemIndex < _totalNumOfItems) _itemIndex++;
                    //Debug.WriteLine("2");
                }
            }

            transition();

            if (_itemIndex == -1) _itemIndex = 0;
            if (_itemIndex == _totalNumOfItems) _itemIndex = _totalNumOfItems - 1;
        }
    }
    
}
