using MedRec.CommonComponents.Views;
using Microsoft.AspNetCore.Components;

namespace MedRec.Patients.Views.Helpers;

/// <summary>
/// Clase auxiliar para gestionar el estado de modales de notificación y confirmación.
/// </summary>
public class ModalNotifier
{
    public event Action? OnStateChanged;

    // Estado común
    public bool IsVisible { get; set; }
    public string Title { get; private set; } = "Confirmar";
    public string Message { get; private set; } = string.Empty;
    public ModalType Type { get; private set; } = ModalType.MessageInfo;
    public RenderFragment? ModalBody { get; private set; }
    public bool CloseOnOverlayClick { get; private set; } = true;

    // Botones
    public bool ShowOk { get; private set; } = true;
    public bool ShowCancel { get; private set; } = false;
    public bool ShowDelete { get; private set; } = false;
    public bool ShowRetry { get; set; } = false;

    // Navegación opcional (solo para mensajes, no para confirmaciones)
    public bool ShouldNavigateAfterClose { get; private set; }
    public string NavigationUrl { get; private set; } = "/";

    // Resultado de confirmación
    public bool? ConfirmationResult { get; private set; } = null;

    // === MÉTODOS PARA MENSAJES SIMPLES ===
    public void ShowMessage(string title, ModalType type, string message = "")
    {
        Reset();
        Title = title;
        Type = type;
        Message = message;
        ShowOk = true;
        ShowCancel = false;
        IsVisible = true;
        OnStateChanged?.Invoke();
    }

    public void ShowMessageAndNavigate(string title, ModalType type, string navigationUrl, string message = "")
    {
        ShowMessage(title, type, message);
        ShouldNavigateAfterClose = true;
        NavigationUrl = navigationUrl;
    }

    // === MÉTODOS AUXILIARES PARA MENSAJES SIMPLES ===

    public void ShowInfoModal(string title, string message)
    {
        Title = title;
        Type = ModalType.MessageInfo;
        Message = message;
        ShowOk = true;
        CloseOnOverlayClick = true;
        IsVisible = true;
        OnStateChanged?.Invoke();
    }

    public void ShowWarningModal(string title, string message)
    {
        Title = title;
        Type = ModalType.MessageWarning;
        Message = message;
        ShowOk = true;
        CloseOnOverlayClick = true;
        IsVisible = true;
        OnStateChanged?.Invoke();
    }

    public void ShowErrorModal(string title, string message)
    {
        Title = title;
        Type = ModalType.MessageError;
        Message = message;
        ShowOk = true;
        CloseOnOverlayClick = true;
        IsVisible = true;
        OnStateChanged?.Invoke();
    }

    // === MÉTODO PARA CONFIRMACIONES DE ELIMINACIÓN ===
    public void ShowDeleteModal(string title, string message)
    {
        Reset();
        Title = title;
        Type = ModalType.MessageDanger;
        Message = message;
        ShowDelete = true;
        ShowCancel = true;
        CloseOnOverlayClick = false;
        IsVisible = true;
        OnStateChanged?.Invoke();
    }

    // === MÉTODOS DE ACCIÓN ===
    public void Confirm() => SetConfirmationResult(true);
    public void Cancel() => SetConfirmationResult(false);
    public void Close() => Reset();

    private void SetConfirmationResult(bool result)
    {
        ConfirmationResult = result;
        IsVisible = false;
        OnStateChanged?.Invoke();
    }

    public void Reset()
    {
        IsVisible = false;
        Message = string.Empty;
        ShowOk = false;
        ShowCancel = false;
        ShowDelete = false;
        ShowRetry = false;
        ShouldNavigateAfterClose = false;
        NavigationUrl = "/";
        ConfirmationResult = null;
        OnStateChanged?.Invoke();
    }
}