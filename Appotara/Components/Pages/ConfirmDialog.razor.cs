using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appotara.Components.Pages
{
	public partial class ConfirmDialog
    {
        [CascadingParameter] BlazoredModalInstance BlazoredModal { get; set; } = default!;

        [Parameter] public string? Message { get; set; }
        [Parameter] public string? Title { get; set; }

        private async Task Confirm() => await BlazoredModal.CloseAsync(ModalResult.Ok(true));
        private async Task Cancel() => await BlazoredModal.CancelAsync();

    }
}
