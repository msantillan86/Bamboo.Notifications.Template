namespace Bamboo.Notifications.Template.Application.Contracts;

public interface ICommand
{
    Task<bool> Handle(string request);
}