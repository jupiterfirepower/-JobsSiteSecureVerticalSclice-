using MediatR;

namespace Jobs.VacancyApi.Features.Notifications;

public record VacancyCreatedNotification(int Id) : INotification;