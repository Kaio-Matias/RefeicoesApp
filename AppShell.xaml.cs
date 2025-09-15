using RefeicoesApp.Views;
namespace RefeicoesApp;
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(CameraPage), typeof(CameraPage));
    }
}