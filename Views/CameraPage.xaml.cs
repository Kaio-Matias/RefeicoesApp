using Camera.MAUI;
using RefeicoesApp.ViewModels;
using System.ComponentModel;

namespace RefeicoesApp.Views;

public partial class CameraPage : ContentPage
{
    private readonly CameraViewModel _viewModel;

    public CameraPage(CameraViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Permissions.RequestAsync<Permissions.Camera>();

        await cameraView.StartCameraAsync();
        cameraView.Camera = cameraView.Cameras.FirstOrDefault(c => c.Position == CameraPosition.Front);

        _viewModel.StartCaptureCountdown();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // CORREÇÃO DEFINITIVA:
        // A biblioteca permite chamar StopCameraAsync diretamente.
        // A verificação 'if' foi removida.
        cameraView.StopCameraAsync();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is CameraViewModel vm)
        {
            vm.PropertyChanged -= ViewModel_PropertyChanged;
            vm.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CameraViewModel.ShouldTakePhoto) && _viewModel.ShouldTakePhoto)
        {
            _viewModel.ShouldTakePhoto = false;
            await TakePhoto();
        }
    }

    private async Task TakePhoto()
    {
        var photoStream = await cameraView.TakePhotoAsync();
        if (photoStream is not null)
        {
            await _viewModel.HandlePhotoTaken(photoStream);
        }
    }
}