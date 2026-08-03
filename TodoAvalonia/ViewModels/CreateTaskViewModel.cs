using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Messages;

namespace TodoAvalonia.ViewModels;

public partial class CreateTaskViewModel : ViewModelBase
{
    [RelayCommand]
    private void Open() => WeakReferenceMessenger.Default.Send(new ShowModalMessage(new TaskListFormViewModel()));
}