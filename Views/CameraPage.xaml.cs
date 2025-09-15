// Views/CameraPage.xaml.cs
using Camera.MAUI;
using RefeicoesApp.ViewModels;
using System.ComponentModel;

namespace RefeicoesApp.Views;

public partial class CameraPage : ContentPage
{
    private readonly CameraViewModel _viewModel;
    private bool _isCameraInitialized = false;

    public CameraPage(CameraViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    // O evento Loaded garante que o controle da câmera está pronto.
    private async void CameraView_Loaded(object sender, EventArgs e)
    {
        if (_isCameraInitialized)
            return;

        await Permissions.RequestAsync<Permissions.Camera>();

        // Inicia a câmera e seleciona a frontal
        await cameraView.StartCameraAsync();
        cameraView.Camera = cameraView.Cameras.FirstOrDefault(c => c.Position == CameraPosition.Front);

        _isCameraInitialized = true;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Apenas iniciamos a contagem aqui. A câmera é iniciada no evento Loaded.
        _viewModel.StartCaptureCountdown();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Chamamos StopCameraAsync para liberar a câmera quando a página some.
        cameraView.StopCameraAsync();
        _isCameraInitialized = false;
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