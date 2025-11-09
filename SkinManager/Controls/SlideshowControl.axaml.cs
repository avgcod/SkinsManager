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
    private static readonly Random RanRoller = new();
    private static readonly Timer SlideShowTimer = new();
    private int _currentImageIndex = -1;
    
    public ICommand BackCommand { get; private set; }
    public ICommand ForwardCommand { get; private set; }
    
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
        BackCommand = new RelayCommand(GoToPreviousImageCommand);
        ForwardCommand = new RelayCommand(GoToNextImageCommand);
        InitializeComponent();
    }

    private void GoToPreviousImageCommand(){
        ChangeImage(false);
        ResetTimer();
    }
    
    private void GoToNextImageCommand(){
        ChangeImage(true);
        ResetTimer();
    }

    private void SlideShowTimerOnElapsed(object? sender, ElapsedEventArgs e){
        ChangeImage(true);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change){
        if (change.Property.Name == nameof(Images)){
            SlideShowTimer.Stop();
            _currentImageIndex = -1;
            ImageProgress = string.Empty;
            CurrentImage = new RenderTargetBitmap(new PixelSize(1, 1));
            
            if(Images.Any()) ChangeImage(true);
            
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
    
    private void ResetTimer(){
        if (SlideShowTimer.Enabled){
            SlideShowTimer.Stop();
            SlideShowTimer.Start();
        }
    }

    private void ChangeImage(bool goingForward){
        if (goingForward) _currentImageIndex = _currentImageIndex == Images.Count - 1 ? 0 : ++_currentImageIndex;
        else _currentImageIndex = _currentImageIndex == 0 ? Images.Count - 1 : --_currentImageIndex;
        
        CurrentImage = Images[_currentImageIndex];
        ImageProgress = $"Image {_currentImageIndex + 1} of {Images.Count}";
    }
    
}