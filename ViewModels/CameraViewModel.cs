using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RefeicoesApp.Services;

namespace RefeicoesApp.ViewModels;

public partial class CameraViewModel : ObservableObject
{
    private readonly IApiRefeicoes _api;
    private IDispatcherTimer? _captureTimer;
    private IDispatcherTimer? _confirmationTimer;
    private int _captureCountdown = 3;
    private int _confirmationCountdown = 10;
    private Stream? _photoStream;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool isBusy = false;
    public bool IsNotBusy => !IsBusy;

    [ObservableProperty] bool isCameraVisible = true;
    [ObservableProperty] bool isConfirmationVisible = false;
    [ObservableProperty] string? captureCountdownText;
    [ObservableProperty] string? confirmationCountdownText;
    [ObservableProperty] ImageSource? capturedImageSource;
    [ObservableProperty] bool shouldTakePhoto;

    public CameraViewModel(IApiRefeicoes api)
    {
        _api = api;
        InitializeTimers();
    }

    private void InitializeTimers()
    {
        _captureTimer = Application.Current?.Dispatcher.CreateTimer();
        if (_captureTimer is not null)
        {
            _captureTimer.Interval = TimeSpan.FromSeconds(1);
            _captureTimer.Tick += CaptureTimer_Tick;
        }

        _confirmationTimer = Application.Current?.Dispatcher.CreateTimer();
        if (_confirmationTimer is not null)
        {
            _confirmationTimer.Interval = TimeSpan.FromSeconds(1);
            _confirmationTimer.Tick += ConfirmationTimer_Tick;
        }
    }

    public void StartCaptureCountdown()
    {
        IsCameraVisible = true;
        IsConfirmationVisible = false;
        _captureCountdown = 3;
        CaptureCountdownText = _captureCountdown.ToString();
        _captureTimer?.Start();
    }

    private void CaptureTimer_Tick(object? sender, EventArgs e)
    {
        _captureCountdown--;
        CaptureCountdownText = _captureCountdown.ToString();
        if (_captureCountdown == 0)
        {
            _captureTimer?.Stop();
            ShouldTakePhoto = true;
        }
    }

    public async Task HandlePhotoTaken(Stream photoStream)
    {
        var memoryStream = new MemoryStream();
        await photoStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        _photoStream = memoryStream;

        _photoStream.Position = 0;
        CapturedImageSource = ImageSource.FromStream(() => _photoStream);

        IsCameraVisible = false;
        IsConfirmationVisible = true;
        StartConfirmationCountdown();
    }

    private void StartConfirmationCountdown()
    {
        _confirmationCountdown = 10;
        ConfirmationCountdownText = $"Confirmar em {_confirmationCountdown}s";
        _confirmationTimer?.Start();
    }

    private void ConfirmationTimer_Tick(object? sender, EventArgs e)
    {
        _confirmationCountdown--;
        ConfirmationCountdownText = $"Confirmar em {_confirmationCountdown}s";
        if (_confirmationCountdown <= 0)
        {
            _confirmationTimer?.Stop();
            if (ConfirmCommand.CanExecute(null)) ConfirmCommand.Execute(null);
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private void Retake()
    {
        _confirmationTimer?.Stop();
        _photoStream?.Dispose();
        _photoStream = null;
        CapturedImageSource = null;
        StartCaptureCountdown();
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task Confirm()
    {
        IsBusy = true;
        _confirmationTimer?.Stop();

        try
        {
            if (_photoStream is null || _photoStream.Length == 0) throw new InvalidOperationException("Foto não capturada.");

            _photoStream.Position = 0;
            var streamPart = new Refit.StreamPart(_photoStream, "photo.jpg", "image/jpeg");
            var response = await _api.IdentificarEFinalizar(streamPart);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                await Shell.Current.DisplayAlert("Sucesso!", response.Content.Mensagem, "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                string error = response.Error?.Content ?? "Erro desconhecido.";
                await Shell.Current.DisplayAlert("Falha", error, "Tentar Novamente");
                Retake();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Não foi possível conectar: {ex.Message}", "OK");
            Retake();
        }
        finally
        {
            _photoStream?.Dispose();
            _photoStream = null;
            IsBusy = false;
        }
    }
}