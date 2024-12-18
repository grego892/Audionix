using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using MudBlazor;
using Serilog;
using System.Reflection;
using System.Threading.Tasks;

namespace Audionix.Components.Layout
{
    public partial class MainLayout : LayoutComponentBase
    {
        bool _drawerOpen = true;
        private MudThemeProvider? _mudThemeProvider;
        private bool _isDarkMode = true;
        private string userName = "";
        private Color buttonColor = Color.Error;
        private bool isLoginPage = false;
        private bool streamStatus = false;
        private double streamVolume = 0.5;
        private string streamStatusText = "Stopped";
        [Inject] private NavigationManager? NavigationManager { get; set; }
        [Inject] private IDialogService? DialogService { get; set; }
        [Inject] private IJSRuntime? JSRuntime { get; set; }
        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            Log.Information("--- MainLayout -- OnInitializedAsync() - Method Starting");
            AuthenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
            await UpdateAuthenticationState();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("initializeAudioPlayer", DotNetObjectReference.Create(this));
            }
        }

        private async Task ToggleLoginLogout()
        {
            Log.Information("--- MainLayout -- ToggleLoginLogout() - Method Starting");
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var isAuthenticated = authState.User?.Identity?.IsAuthenticated ?? false;
            Log.Debug("--- MainLayout -- isAuthenticated: " + isAuthenticated);
            if (isAuthenticated)
            {
                Log.Debug("--- MainLayout -- " + userName + "IS LOGGING OUT.");
                // await SignInManager.SignOutAsync();
                NavigationManager?.NavigateTo("/Account/LoggingOut", true);
                Log.Debug("--- MainLayout -- " + userName + "IS LOGGED OUT.");
                NavigationManager?.NavigateTo("/Account/LoggedOut", true);
            }
            else
            {
                NavigationManager?.NavigateTo("/Account/Login", true);
            }
        }

        private async Task UpdateAuthenticationState()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var isAuthenticated = authState.User?.Identity?.IsAuthenticated ?? false;
            userName = isAuthenticated ? authState.User?.Identity?.Name ?? "" : "";
            buttonColor = isAuthenticated ? Color.Primary : Color.Error;
            isLoginPage = HttpContext?.Request.Path.StartsWithSegments("/Account") ?? false;
        }

        private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            await InvokeAsync(async () =>
            {
                await UpdateAuthenticationState();
                StateHasChanged();
            });
        }

        void DrawerToggle()
        {
            _drawerOpen = !_drawerOpen;
            StateHasChanged();
            Log.Information("--- MainLayout -- DrawerToggle() - _drawerOpen: {_drawerOpen}", _drawerOpen);
        }

        MudTheme AudionixTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = Colors.LightBlue.Default,
                Secondary = Colors.Teal.Default,
                AppbarBackground = Colors.LightBlue.Darken4
            },
            PaletteDark = new PaletteDark()
            {
                Primary = Colors.LightBlue.Default,
                Secondary = Colors.Teal.Default,
                AppbarBackground = Colors.LightBlue.Darken4
            },

            LayoutProperties = new LayoutProperties()
            {
                DrawerWidthLeft = "180px",
            }
        };

        private async Task ShowAboutSnackbar()
        {
            bool? result = await DialogService.ShowMessageBox(
                "Audionix",
                (MarkupString)$"Version: {GetVersion()} <br /> Developed by: Greg Davis <br /> djgrego@djgrego.com");
            string? state = result == null ? "Canceled" : "Deleted!";
            StateHasChanged();
        }

        private async void ToggleStreamStatus()
        {
            streamStatus = !streamStatus;
            if (streamStatus)
            {
                await JSRuntime.InvokeVoidAsync("playAudio");
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("pauseAudio");
            }
        }

        private async Task SetStreamVolume(double volume)
        {
            streamVolume = volume;
            await JSRuntime.InvokeVoidAsync("setVolume", volume);
        }

        [JSInvokable]
        public void UpdateStreamStatus(string status)
        {
            streamStatusText = status;
            if (status == "Error" || status == "Stopped")
            {
                streamStatusText += " - Attempting to reconnect...";
            }
            StateHasChanged();
        }

        string GetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version.ToString();
        }
    }
}
