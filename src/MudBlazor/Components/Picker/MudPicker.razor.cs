using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor.Extensions;
using MudBlazor.Interop;
using MudBlazor.Utilities;

namespace MudBlazor
{
    public partial class MudPicker : MudBasePicker
    {
        enum PickerPaperPosition
        {
            Unknown,
            Below,
            Above,
            Top,
            Bottom
        }


        [Inject]
        private IBrowserWindowSizeProvider WindowSizeListener { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        protected string PickerClass =>
        new CssBuilder("mud-picker")
            .AddClass($"mud-picker-inline", PickerVariant != PickerVariant.Static)
            .AddClass($"mud-picker-static", PickerVariant == PickerVariant.Static)
            .AddClass($"mud-rounded", PickerVariant == PickerVariant.Static && !PickerSquare)
            .AddClass($"mud-elevation-{PickerElevation}", PickerVariant == PickerVariant.Static)
            .AddClass($"mud-picker-input-button", !AllowKeyboardInput && PickerVariant != PickerVariant.Static)
            .AddClass($"mud-picker-input-text", AllowKeyboardInput && PickerVariant != PickerVariant.Static)
            .AddClass($"mud-disabled", Disabled && PickerVariant != PickerVariant.Static)
            .AddClass(Class)
            .Build();

        protected string PickerPaperClass =>
        new CssBuilder("mud-picker-paper")
            .AddClass("mud-picker-view", PickerVariant == PickerVariant.Inline)
            .AddClass("mud-picker-open", IsOpen && PickerVariant == PickerVariant.Inline)
            .AddClass("mud-picker-popover-paper", PickerVariant == PickerVariant.Inline)
            .AddClass("mud-dialog", PickerVariant == PickerVariant.Dialog)
            .AddClass("mud-picker-hidden", pickerPaperPosition == PickerPaperPosition.Unknown && PickerVariant == PickerVariant.Inline)
            .AddClass("mud-picker-pos-top", pickerPaperPosition == PickerPaperPosition.Top)
            .AddClass("mud-picker-pos-above", pickerPaperPosition == PickerPaperPosition.Above)
            .AddClass("mud-picker-pos-bottom", pickerPaperPosition == PickerPaperPosition.Bottom)
            .Build();

        protected string PickerContainerClass =>
        new CssBuilder("mud-picker-container")
        .AddClass("mud-paper-square", PickerSquare)
        .AddClass("mud-picker-container-landscape", Orientation == Orientation.Landscape && PickerVariant == PickerVariant.Static)
        .Build();
        
        protected string PickerInputClass =>
        new CssBuilder("mud-input-input-control").AddClass(Class)
        .Build();

        [Parameter] public bool IsRange { get; set; } = false;
        [Parameter] public string InputIcon { get; set; }
        [Parameter] public RenderFragment ChildContent { get; set; }

        [Parameter] public EventCallback PickerOpened { get; set; }

        private bool PickerSquare { get; set; }
        private int PickerElevation { get; set; }
        private bool isRendered = false;
        private ElementReference PickerContainerRef { get; set; }

        private PickerPaperPosition pickerPaperPosition = PickerPaperPosition.Unknown;

        protected override void OnInitialized()
        {
            if (PickerVariant == PickerVariant.Static)
            {
                IsOpen = true;

                if (Elevation == 8)
                {
                    PickerElevation = 0;
                }
                else
                {
                    PickerElevation = Elevation;
                }

                if (!Rounded)
                {
                    PickerSquare = true;
                }
            }
            else
            {
                PickerSquare = Square;
                PickerElevation = Elevation;
            }
        }

        public override void ToggleOpen()
        {
            base.ToggleOpen();
            if(IsOpen)
                OnPickerOpened();
            else
                StateHasChanged();
        }

        public override void Open()
        {
            base.Open();
            OnPickerOpened();
        }

        protected virtual void OnPickerOpened()
        {
            PickerOpened.InvokeAsync(this);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (PickerVariant == PickerVariant.Inline)
            {
                if (!isRendered && IsOpen)
                {
                    isRendered = true;
                    await DeterminePosition();
                    StateHasChanged();
                }
                else if (isRendered && !IsOpen)
                {
                    isRendered = false;
                    pickerPaperPosition = PickerPaperPosition.Unknown;
                }
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task DeterminePosition()
        {
            if (WindowSizeListener == null || JSRuntime == null)
            {
                pickerPaperPosition = PickerPaperPosition.Below;
                return;
            }

            var size = await WindowSizeListener.GetBrowserWindowSize();
            var clientRect = await JSRuntime.InvokeAsync<BoundingClientRect>("getMudBoundingClientRect", PickerContainerRef);

            if (size == null || clientRect == null)
            {
                pickerPaperPosition = PickerPaperPosition.Below;
                return;
            }

            if (size.Height < clientRect.Height)
            {
                pickerPaperPosition = PickerPaperPosition.Top;
            }
            else if (size.Height < clientRect.Bottom)
            {
                if (clientRect.Top > clientRect.Height)
                {
                    pickerPaperPosition = PickerPaperPosition.Above;
                }
                else if (clientRect.Top > size.Height / 2)
                {
                    pickerPaperPosition = PickerPaperPosition.Bottom;
                }
                else
                {
                    pickerPaperPosition = PickerPaperPosition.Top;
                }
            }
            else if (clientRect.Top < 0)
            {
                pickerPaperPosition = PickerPaperPosition.Top;
            }
            else
            {
                pickerPaperPosition = PickerPaperPosition.Below;
            }
        }
    }
}