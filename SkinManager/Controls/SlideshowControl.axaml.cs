using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;

namespace SkinManager.Controls;

public partial class SlideshowControl : UserControl{
    private static readonly Timer SlideShowTimer = new();
    private int _currentImageIndex = -1;
    
    public ICommand ChangeImageCommand { get; private set; }
    
    public static readonly StyledProperty<List<Bitmap>> ImagesProperty =
        AvaloniaProperty.Register<SlideshowControl, List<Bitmap>>(nameof(Images), defaultValue: []);

    public List<Bitmap> Images{
        get => Dispatcher.UIThread.Invoke(() => GetValue(ImagesProperty));
        set => Dispatcher.UIThread.Invoke(() => SetValue(ImagesProperty, value));
    }
    
    public static readonly StyledProperty<bool> HasMultipleImagesProperty =
        AvaloniaProperty.Register<SlideshowControl, bool>(nameof(HasMultipleImages), defaultValue: false);
    
    public bool HasMultipleImages {
        get => Dispatcher.UIThread.Invoke(() => GetValue(HasMultipleImagesProperty));
        private set => Dispatcher.UIThread.Invoke(() => SetValue(HasMultipleImagesProperty, value));
    }
    public static readonly StyledProperty<string> ImageProgressProperty =
        AvaloniaProperty.Register<SlideshowControl, string>(nameof(ImageProgress), defaultValue: string.Empty);
    
    public string ImageProgress {
        get => Dispatcher.UIThread.Invoke(() => GetValue(ImageProgressProperty));
        private set => Dispatcher.UIThread.Invoke(() => SetValue(ImageProgressProperty, value));
    }
    
    
    public static readonly StyledProperty<Bitmap> CurrentImageProperty =
        AvaloniaProperty.Register<SlideshowControl, Bitmap>(nameof(CurrentImage), defaultValue: new RenderTargetBitmap(new PixelSize(1, 1)));

    public Bitmap CurrentImage{
        get => Dispatcher.UIThread.Invoke(() => GetValue(CurrentImageProperty));
        set => Dispatcher.UIThread.Invoke(() => SetValue(CurrentImageProperty, value));
    }

    public static readonly StyledProperty<double> TimerSecondsProperty =
        AvaloniaProperty.Register<SlideshowControl, double>(nameof(TimerSeconds), defaultValue: TimeSpan.FromSeconds(3).TotalMilliseconds);

    public double TimerSeconds{
        get => Dispatcher.UIThread.Invoke(() => GetValue(TimerSecondsProperty));
        set => Dispatcher.UIThread.Invoke(() => SetValue(TimerSecondsProperty, TimeSpan.FromSeconds(value).TotalMilliseconds));
    }

    public SlideshowControl(){
        SlideShowTimer.Interval = TimerSeconds;
        SlideShowTimer.Elapsed += SlideShowTimerOnElapsed;
        ChangeImageCommand = new RelayCommand<object?>(ChangeImageHandler);
        InitializeComponent();
    }

    private void ChangeImageHandler(object? possibleStringObject){
        if (possibleStringObject is string boolString && bool.TryParse(boolString, out bool goForward)){
            CurrentImage = ChangeImage(goForward);
            ImageProgress = GetProgressText();
            ResetTimer();
        }
    }

    private void SlideShowTimerOnElapsed(object? sender, ElapsedEventArgs e){
        CurrentImage = ChangeImage(true);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change){
        if (change.Property.Name == nameof(Images)){
            SlideShowTimer.Stop();
            _currentImageIndex = -1;
            ImageProgress = string.Empty;
            CurrentImage = new RenderTargetBitmap(new PixelSize(1, 1));

            if (Images.Any()){
                CurrentImage = ChangeImage(true);
                ImageProgress = GetProgressText();
            }
            
            if (Images.Count > 1){
                HasMultipleImages = true;
                SlideShowTimer.Start();
            }
            else{
                HasMultipleImages = false;
                SlideShowTimer.Stop();
            }
        }else if (change.Property.Name == nameof(TimerSeconds)){
            SlideShowTimer.Interval = TimerSeconds;
        }
        base.OnPropertyChanged(change);
    }
    
    private static void ResetTimer(){
        if (SlideShowTimer.Enabled){
            SlideShowTimer.Stop();
            SlideShowTimer.Start();
        }
    }

    private Bitmap ChangeImage(bool goingForward){
        if (goingForward) _currentImageIndex = _currentImageIndex == Images.Count - 1 ? 0 : ++_currentImageIndex;
        else _currentImageIndex = _currentImageIndex == 0 ? Images.Count - 1 : --_currentImageIndex;
        
        return Images[_currentImageIndex];
    }
    
    private string GetProgressText() => $"Image {_currentImageIndex + 1} of {Images.Count}";
}