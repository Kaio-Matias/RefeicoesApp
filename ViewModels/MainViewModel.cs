using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RefeicoesApp.Views;

namespace RefeicoesApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [RelayCommand]
    async Task GoToCamera()
    {
        await Shell.Current.GoToAsync(nameof(CameraPage));
    }
}