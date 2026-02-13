using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.WpfClient.Models;
using Gauniv.WpfClient.Services;

namespace Gauniv.WpfClient.ViewModels;

/// <summary>
/// Profile ViewModel
/// </summary>
public partial class ProfileViewModel : ViewModelBase, INavigationAware
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _registeredAt = string.Empty;

    [ObservableProperty]
    private string _installPath = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ProfileViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;

        // Default install path
        InstallPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Gauniv", "Games");
    }

    public void OnNavigatedTo(object parameter)
    {
        _ = LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        IsLoading = true;

        try
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user != null)
            {
                Email = user.Email;
                FirstName = user.FirstName;
                LastName = user.LastName;
                RegisteredAt = user.RegisteredAt != default
                    ? user.RegisteredAt.ToString("yyyy-MM-dd")
                    : "N/A";
            }
            else if (_authService.CurrentUser != null)
            {
                Email = _authService.CurrentUser.Email;
                FirstName = _authService.CurrentUser.FirstName;
                LastName = _authService.CurrentUser.LastName;
            }
        }
        catch
        {
            StatusMessage = "Failed to load profile.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void BrowseInstallPath()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select install folder"
        };

        if (dialog.ShowDialog() == true)
        {
            InstallPath = dialog.FolderName;
            StatusMessage = "Install path updated.";
        }
    }

    [RelayCommand]
    private void OpenInstallFolder()
    {
        try
        {
            if (Directory.Exists(InstallPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = InstallPath,
                    UseShellExecute = true
                });
            }
            else
            {
                StatusMessage = "Folder does not exist.";
            }
        }
        catch
        {
            StatusMessage = "Failed to open folder.";
        }
    }
}
