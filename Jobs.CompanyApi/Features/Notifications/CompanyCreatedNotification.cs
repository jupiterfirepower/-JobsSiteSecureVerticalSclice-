using MediatR;

namespace Jobs.CompanyApi.Features.Notifications;

public record CompanyCreatedNotification(int Id) : INotification;