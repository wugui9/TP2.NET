using System.IO;
using System.IO.Compression;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.WpfClient.Models;
using Gauniv.WpfClient.Services;

namespace Gauniv.WpfClient.ViewModels;

/// <summary>
/// Game Details ViewModel
/// </summary>
public partial class GameDetailsViewModel : ViewModelBase, INavigationAware
{
    private readonly IGameService _gameService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private Game? _game;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowDownloadButton))]
    private bool _isPurchased;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowDownloadButton))]
    private bool _isDownloaded;

    [ObservableProperty]
    private string _downloadPath = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Show download button when purchased but not yet downloaded
    /// </summary>
    public bool ShowDownloadButton => IsPurchased && !IsDownloaded;


    public GameDetailsViewModel(
        IGameService gameService, 
        INavigationService navigationService)
    {
        _gameService = gameService;
        _navigationService = navigationService;
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is int gameId)
        {
            _ = LoadGameAsync(gameId);
        }
    }

    private async Task LoadGameAsync(int gameId)
    {
        IsLoading = true;
        
        try
        {
            Game = await _gameService.GetGameByIdAsync(gameId);
            
            if (Game != null)
            {
                // Use IsOwned field from API response to check ownership
                IsPurchased = Game.IsOwned;

                // Check if locally downloaded
                var filePath = ResolveInstalledGamePath(Game);
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    IsDownloaded = true;
                    DownloadPath = filePath;
                }
                else
                {
                    IsDownloaded = false;
                    DownloadPath = string.Empty;
                }
            }
        }
        catch
        {
            // Handle error
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PurchaseAsync()
    {
        if (Game == null) return;

        IsLoading = true;
        
        try
        {
            var success = await _gameService.PurchaseGameAsync(Game.Id);
            if (success)
            {
                IsPurchased = true;
                StatusMessage = "Purchase successful!";
            }
            else
            {
                StatusMessage = "Purchase failed. Please try again.";
            }
        }
        catch
        {
            // Handle error
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DownloadAsync()
    {
        if (Game == null || !IsPurchased) return;

        IsLoading = true;
        
        try
        {
            var installDirectory = GetGameInstallDirectory(Game);
            var downloadPath = await _gameService.DownloadGameAsync(Game.Id, installDirectory, Game.FileName);
            if (!string.IsNullOrWhiteSpace(downloadPath))
            {
                IsDownloaded = true;
                DownloadPath = downloadPath;
                StatusMessage = "Download successful!";
            }
            else
            {
                StatusMessage = "Download failed. Please try again.";
            }
        }
        catch
        {
            // Handle error
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Launch()
    {
        if (!IsDownloaded || string.IsNullOrEmpty(DownloadPath)) return;

        try
        {
            var extension = Path.GetExtension(DownloadPath).ToLowerInvariant();
            if (extension == ".zip")
            {
                LaunchFromZipPackage();
                return;
            }

            var workingDirectory = Path.GetDirectoryName(DownloadPath) ?? Environment.CurrentDirectory;
            System.Diagnostics.ProcessStartInfo startInfo;

            if (extension == ".ps1")
            {
                startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{DownloadPath}\"",
                    UseShellExecute = false,
                    WorkingDirectory = workingDirectory
                };
            }
            else if (extension is ".cmd" or ".bat")
            {
                startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c \"{DownloadPath}\"",
                    UseShellExecute = false,
                    WorkingDirectory = workingDirectory
                };
            }
            else
            {
                startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = DownloadPath,
                    UseShellExecute = true,
                    WorkingDirectory = workingDirectory
                };
            }

            System.Diagnostics.Process.Start(startInfo);
            StatusMessage = "Game launched.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Launch failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private void DeleteGame()
    {
        if (!IsDownloaded || string.IsNullOrEmpty(DownloadPath)) return;

        try
        {
            if (File.Exists(DownloadPath))
            {
                File.Delete(DownloadPath);
            }

            var extractedDirectory = GetExtractedDirectory(DownloadPath);
            if (Directory.Exists(extractedDirectory))
            {
                Directory.Delete(extractedDirectory, true);
            }

            var gameDirectory = Path.GetDirectoryName(DownloadPath);
            if (!string.IsNullOrWhiteSpace(gameDirectory)
                && Directory.Exists(gameDirectory)
                && !Directory.EnumerateFileSystemEntries(gameDirectory).Any())
            {
                Directory.Delete(gameDirectory);
            }

            IsDownloaded = false;
            DownloadPath = string.Empty;
            StatusMessage = "Game deleted successfully.";
        }
        catch
        {
            StatusMessage = "Failed to delete the game.";
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
        else
        {
            _navigationService.NavigateTo<GameListViewModel>();
        }
    }

    private static string GetGameInstallDirectory(Game game)
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Gauniv",
            "Games");

        return Path.Combine(root, $"game-{game.Id}");
    }

    private static string? ResolveInstalledGamePath(Game game)
    {
        var directory = GetGameInstallDirectory(game);
        if (!Directory.Exists(directory))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(game.FileName))
        {
            var expectedPath = Path.Combine(directory, SanitizeFileName(game.FileName));
            if (File.Exists(expectedPath))
            {
                return expectedPath;
            }
        }

        return Directory
            .GetFiles(directory)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(fileName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "download.bin" : cleaned;
    }

    private void LaunchFromZipPackage()
    {
        var extractedDirectory = PrepareExtractedDirectory(DownloadPath);
        if (string.IsNullOrWhiteSpace(extractedDirectory))
        {
            StatusMessage = "Launch failed: cannot create extraction directory. Please close running game/editor and try again.";
            return;
        }

        ZipFile.ExtractToDirectory(DownloadPath, extractedDirectory, true);

        var executablePath = ResolveExecutableFromExtractedPackage(extractedDirectory);
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            StatusMessage = "Package extracted, but no .exe found. Please upload an exported runnable build.";
            return;
        }

        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(executablePath) ?? extractedDirectory
        };

        System.Diagnostics.Process.Start(startInfo);
        StatusMessage = "Game launched from package.";
    }

    private string? ResolveExecutableFromExtractedPackage(string extractedDirectory)
    {
        var executables = Directory
            .EnumerateFiles(extractedDirectory, "*.exe", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith("vshost.exe", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (executables.Count == 0)
        {
            return null;
        }

        if (Game is not null && !string.IsNullOrWhiteSpace(Game.FileName))
        {
            var expectedName = Path.GetFileName(Game.FileName);
            var exact = executables.FirstOrDefault(path =>
                string.Equals(Path.GetFileName(path), expectedName, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(exact))
            {
                return exact;
            }
        }

        if (Game is not null && !string.IsNullOrWhiteSpace(Game.Name))
        {
            var marker = Game.Name.Trim();
            var matched = executables.FirstOrDefault(path =>
                Path.GetFileNameWithoutExtension(path).Contains(marker, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(matched))
            {
                return matched;
            }
        }

        return executables
            .OrderBy(path => path.Count(c => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar))
            .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    private static string GetExtractedDirectory(string packagePath)
    {
        var packageName = Path.GetFileNameWithoutExtension(packagePath);
        var gameFolder = Path.GetFileName(Path.GetDirectoryName(packagePath)?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) ?? "game";
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Gauniv",
            "ExtractedGames");

        return Path.Combine(root, $"{gameFolder}_{packageName}_extracted");
    }

    private static string? PrepareExtractedDirectory(string packagePath)
    {
        var primary = GetExtractedDirectory(packagePath);
        if (TryResetDirectory(primary))
        {
            return primary;
        }

        var fallback = $"{primary}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        if (TryResetDirectory(fallback))
        {
            return fallback;
        }

        return null;
    }

    private static bool TryResetDirectory(string directoryPath)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }

            Directory.CreateDirectory(directoryPath);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
