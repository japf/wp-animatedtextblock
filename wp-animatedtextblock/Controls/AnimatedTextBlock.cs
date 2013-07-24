using System;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Windows.Foundation.Metadata;
using Observable = System.Reactive.Linq.Observable;
using ObservableExtensions = System.ObservableExtensions;

namespace Chartreuse.Today.Controls
{
    public class AnimatedTextBlock : Control
    {
        private readonly System.Reactive.Subjects.ISubject<int> subject;

        private readonly Storyboard storyboard1;
        private readonly Storyboard storyboard2;

        private Border border1;
        private Border border2;

        private Border animatedBorder1;
        private Border animatedBorder2;

        private bool isTemplateLoaded;
        private Action templateLoadedAction;
 
        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        public static readonly DependencyProperty CountProperty = DependencyProperty.Register(
            "Count", 
            typeof(int), 
            typeof(AnimatedTextBlock), 
            new PropertyMetadata(0, new PropertyChangedCallback(OnCountChanged)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", 
            typeof(string), 
            typeof(AnimatedTextBlock), 
            new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnTextChanged)));

        public AnimatedTextBlock()
        {
            this.DefaultStyleKey = typeof(AnimatedTextBlock);

            this.subject = new System.Reactive.Subjects.Subject<int>();
            this.storyboard1 = new Storyboard { Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
            this.storyboard2 = new Storyboard { Duration = new Duration(TimeSpan.FromSeconds(0.3)) };

            this.UpdateStoryboard(this.storyboard1, -90.0, 0.0);
            this.UpdateStoryboard(this.storyboard2, 0.0, 90.0);
           
            IObservable<EventPattern<EventArgs>> second = Observable.StartWith<EventPattern<EventArgs>>(Observable
                    .FromEventPattern<EventHandler, EventArgs>(ehea => new EventHandler(ehea.Invoke), eh => this.storyboard1.Completed += eh, eh => this.storyboard1.Completed -= eh), new EventPattern<EventArgs>[] { new EventPattern<EventArgs>(this.storyboard1, EventArgs.Empty) });


            var a = Observable.Zip(this.subject, second, (a0, a1) => a0);
            var b = Microsoft.Phone.Reactive.Observable.ObserveOnDispatcher(a);
            var c = b.Subscribe<int>(this.OnNext, this.OnCompleted);
        }

        private void OnNext(int value)
        {
            this.OnNext(value.ToString(CultureInfo.InvariantCulture));
        }

        private void OnNext(string value)
        {
            if (!this.isTemplateLoaded)
            {
                this.templateLoadedAction = () =>
                    {
                        ((TextBlock)this.border1.Child).Text = value;
                        this.border1.Visibility = Visibility.Visible;
                    };
                return;
            }

            if (this.animatedBorder2 != null)
            {
                this.animatedBorder2.Visibility = Visibility.Collapsed;
                this.animatedBorder1 = this.border1.Visibility != Visibility.Visible ? this.border1 : this.border2;
            }
            else
            {
                this.animatedBorder1 = this.border1.Visibility != Visibility.Visible ? this.border1 : this.border2;
            }

            this.animatedBorder2 = this.animatedBorder1 != this.border1 ? this.border1 : this.border2;

            this.storyboard1.Stop();
            this.storyboard2.Stop();
            
            Storyboard.SetTarget(this.storyboard1.Children[0], this.animatedBorder1.Projection);
            Storyboard.SetTarget(this.storyboard2.Children[0], this.animatedBorder2.Projection);

            ((TextBlock)this.animatedBorder1.Child).Text = value;
            
            this.animatedBorder1.Visibility = Visibility.Visible;
            this.animatedBorder2.Visibility = Visibility.Visible;

            // use a hack to run the animation after 2 dispatcher updates
            // to make sure the phone can handle it
            // its similar to WPF BeginInvoke(DispatcherPriority.Idle)
            // but we don't have priorities in Windows Phone
            this.Dispatcher.BeginInvoke(() =>
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    this.storyboard1.Begin();
                    this.storyboard2.Begin();
                });
            });
        }

        private void OnCompleted()
        {
            if (this.animatedBorder2 != null)
            {
                this.animatedBorder2.Visibility = Visibility.Visible;
            }
        }

        private void UpdateStoryboard(Storyboard s, double start, double end)
        {
            DoubleAnimation animation2 = new DoubleAnimation
            { 
                Duration = s.Duration,
                From = start, 
                To = end 
            };

            DoubleAnimation animation = animation2;
            
            s.Children.Add(animation);
            
            Storyboard.SetTargetProperty(animation, new PropertyPath(PlaneProjection.RotationXProperty));
        }

        private static void OnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimatedTextBlock animatedTextblock = (AnimatedTextBlock) d;
            animatedTextblock.OnCountChanged((int)e.NewValue, (int)e.OldValue);
        }

        private void OnCountChanged(int newValue, int oldValue)
        {
            ObservableExtensions.Subscribe<int>(Observable
                    .Range(oldValue + 1, newValue - oldValue)
                    .TakeLast<int>(5), (v) => this.subject.OnNext(v));
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimatedTextBlock animatedTextblock = (AnimatedTextBlock)d;
            animatedTextblock.OnTextChanged((string)e.NewValue, (string)e.OldValue);
        }

        private void OnTextChanged(string newValue, string oldValue)
        {
            this.OnNext(oldValue);
            this.OnNext(newValue);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.border1 = this.GetTemplateChild("PART_A") as Border;
            if (this.border1 == null || this.border1.Child as TextBlock == null)
                throw new NotSupportedException("Tempate must contain a Border named PART_A with a TextBlock inside");

            this.border2 = this.GetTemplateChild("PART_B") as Border;
            if (this.border2 == null)
                throw new NotSupportedException("Tempate must contain a Border named PART_A");

            this.isTemplateLoaded = true;

            if (this.templateLoadedAction != null)
            {
                this.templateLoadedAction();
                this.templateLoadedAction = null;
            }
        }
    }
}
