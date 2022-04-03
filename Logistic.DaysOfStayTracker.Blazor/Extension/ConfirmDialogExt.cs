using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Blazor.Components;
using MudBlazor;

namespace Logistic.DaysOfStayTracker.Blazor.Extension;

public static class ConfirmDialogExt
{
    public static async Task<bool> ShowConfirmDialog(this IDialogService self, string message, string title = "Внимание", DialogOptions? dialogOptions = null)
    {
        var parameters = ConfirmDialog.CreateParameters(message);
        var dialog = self.Show<ConfirmDialog>(title, parameters, dialogOptions);
        var result = await dialog.Result;
        return !result.Cancelled;
    }
}