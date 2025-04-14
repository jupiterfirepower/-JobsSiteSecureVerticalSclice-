using Jobs.CompanyApi.Features.Notifications;
using MediatR;

namespace Jobs.CompanyApi.Features.Handlers;

public class CompanyAssignedHandler(ILogger<CompanyAssignedHandler> logger) : INotificationHandler<CompanyCreatedNotification>
{
    public Task Handle(CompanyCreatedNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation($"handling notification for vacancy creation with id : {notification.Id}. assigning.");
        return Task.CompletedTask;
    }
}