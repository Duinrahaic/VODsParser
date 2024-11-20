using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace VODsParser.Client.ViewModels;

public class ClientWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> ExitCommand { get; }
    public Interaction<Unit, Unit> ExitInteraction { get; }
    

    public ClientWindowViewModel()
    {
        if(!Avalonia.Controls.Design.IsDesignMode)
        {
        }
        ExitCommand = ReactiveCommand.CreateFromTask(OnExit);
        ExitInteraction = new Interaction<Unit, Unit>();
    }

    private async Task OnExit()
    {
        await ExitInteraction.Handle(Unit.Default);

    }
    
}