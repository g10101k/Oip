using System.Net.Mail;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Oip.Notifications.Base;
using Oip.Notifications.Channels;
using Oip.Notifications.Contexts;

namespace Oip.Notifications.Services;

/// <summary>
/// Provides notification functionality by distributing messages to all registered notification channels with error handling and logging capabilities
/// </summary>
[ApiExplorerSettings(GroupName = "grpc")]
public class NotificationService(
    ILogger<NotificationService> logger,
    SmtpChannel smtpChannel,
    NotificationTypeRepository notificationTypeRepository,
    NotificationChannelRepository notificationChannelRepository,
    NotificationTemplateRepository notificationTemplateRepository,
    UserNotificationPreferenceRepository userNotificationPreferenceRepository,
    NotificationRepository notificationRepository,
    NotificationDeliveryRepository notificationDeliveryRepository) : GrpcNotificationService.GrpcNotificationServiceBase
{
    private readonly List<INotificationChannel> _channels = [smtpChannel];

    /// <summary>
    /// Notification via all channels
    /// </summary>
    /// <param name="user">User to notify</param>
    /// <param name="subject">Message subject</param>
    /// <param name="message">Message content</param>
    public void Notify(UserInfoDto user, string subject, string message)
    {
        foreach (var channel in _channels)
        {
            try
            {
                channel.Notify(user, subject, message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while sending notification via channel {channelName}",
                    channel.Name);
            }
        }
    }

    /// <summary>
    /// Notification via all channels with attachments
    /// </summary>
    /// <param name="user">User to notify</param>
    /// <param name="subject">Message subject</param>
    /// <param name="message">Message content</param>
    /// <param name="attachments">File attachments</param>
    public void Notify(UserInfoDto user, string subject, string message, Attachment[] attachments)
    {
        foreach (var channel in _channels)
        {
            try
            {
                channel.Notify(user, subject, message, attachments);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while sending notification via channel {channelName}",
                    channel.Name);
            }
        }
    }

    // Notification Type Management
    public override async Task<CreateNotificationTypeResponse> CreateNotificationType(
        CreateNotificationTypeRequest request, ServerCallContext context)
    {
        try
        {
            var notificationType = new NotificationTypeEntity
            {
                Name = request.Name,
                Description = request.Description,
                Scope = request.Scope
            };

            await notificationTypeRepository.AddAsync(notificationType);

            return new CreateNotificationTypeResponse
            {
                NotificationType = new NotificationType
                {
                    NotificationTypeId = notificationType.NotificationTypeId,
                    Name = notificationType.Name,
                    Description = notificationType.Description,
                    Scope = notificationType.Scope
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating notification type: {Name}", request.Name);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to create notification type"));
        }
    }

    public override async Task<GetNotificationTypeResponse> GetNotificationType(
        GetNotificationTypeRequest request, ServerCallContext context)
    {
        try
        {
            var notificationType = await notificationTypeRepository.GetByIdAsync(request.NotificationTypeId);
            if (notificationType == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification type with ID {request.NotificationTypeId} not found"));
            }

            return new GetNotificationTypeResponse
            {
                NotificationType = new NotificationType
                {
                    NotificationTypeId = notificationType.NotificationTypeId,
                    Name = notificationType.Name,
                    Description = notificationType.Description,
                    Scope = notificationType.Scope
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving notification type: {Id}", request.NotificationTypeId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to retrieve notification type"));
        }
    }

    public override async Task<UpdateNotificationTypeResponse> UpdateNotificationType(
        UpdateNotificationTypeRequest request, ServerCallContext context)
    {
        try
        {
            var notificationType = await notificationTypeRepository.GetByIdAsync(request.NotificationTypeId);
            if (notificationType == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification type with ID {request.NotificationTypeId} not found"));
            }

            notificationType.Name = request.Name;
            notificationType.Description = request.Description;
            notificationType.Scope = request.Scope;

            await notificationTypeRepository.UpdateAsync(notificationType);

            return new UpdateNotificationTypeResponse
            {
                NotificationType = new NotificationType
                {
                    NotificationTypeId = notificationType.NotificationTypeId,
                    Name = notificationType.Name,
                    Description = notificationType.Description,
                    Scope = notificationType.Scope
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating notification type: {Id}", request.NotificationTypeId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to update notification type"));
        }
    }

    public override async Task<DeleteNotificationTypeResponse> DeleteNotificationType(
        DeleteNotificationTypeRequest request, ServerCallContext context)
    {
        try
        {
            await notificationTypeRepository.DeleteAsync(request.NotificationTypeId);
            return new DeleteNotificationTypeResponse { Success = true };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting notification type: {Id}", request.NotificationTypeId);
            return new DeleteNotificationTypeResponse { Success = false };
        }
    }

    public override async Task<ListNotificationTypesResponse> ListNotificationTypes(
        ListNotificationTypesRequest request, ServerCallContext context)
    {
        try
        {
            List<NotificationTypeEntity> notificationTypes;
            if (!string.IsNullOrEmpty(request.ScopeFilter))
            {
                notificationTypes = await notificationTypeRepository.GetByScopeAsync(request.ScopeFilter);
            }
            else
            {
                notificationTypes = await notificationTypeRepository.GetAllAsync();
            }

            var response = new ListNotificationTypesResponse();
            response.NotificationTypes.AddRange(notificationTypes.Select(nt => new NotificationType
            {
                NotificationTypeId = nt.NotificationTypeId,
                Name = nt.Name,
                Description = nt.Description,
                Scope = nt.Scope
            }));

            // In a real implementation, you would implement proper pagination
            response.Pagination = new PaginationResponse
            {
                Page = request.Pagination?.Page ?? 1,
                PageSize = request.Pagination?.PageSize ?? notificationTypes.Count,
                TotalCount = notificationTypes.Count,
                TotalPages = 1
            };

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing notification types");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to list notification types"));
        }
    }

    // Notification Channel Management
    public override async Task<CreateNotificationChannelResponse> CreateNotificationChannel(
        CreateNotificationChannelRequest request, ServerCallContext context)
    {
        try
        {
            var notificationChannel = new NotificationChannelEntity
            {
                Name = request.Name,
                IsActive = request.IsActive,
                RequiresVerification = request.RequiresVerification,
                MaxRetryCount = request.MaxRetryCount
            };

            await notificationChannelRepository.AddAsync(notificationChannel);

            return new CreateNotificationChannelResponse
            {
                NotificationChannel = new NotificationChannel
                {
                    NotificationChannelId = notificationChannel.NotificationChannelId,
                    Name = notificationChannel.Name,
                    IsActive = notificationChannel.IsActive,
                    RequiresVerification = notificationChannel.RequiresVerification,
                    MaxRetryCount = notificationChannel.MaxRetryCount ?? 0
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating notification channel: {Name}", request.Name);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to create notification channel"));
        }
    }

    public override async Task<GetNotificationChannelResponse> GetNotificationChannel(
        GetNotificationChannelRequest request, ServerCallContext context)
    {
        try
        {
            var notificationChannel = await notificationChannelRepository.GetByIdAsync(request.NotificationChannelId);
            if (notificationChannel == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification channel with ID {request.NotificationChannelId} not found"));
            }

            return new GetNotificationChannelResponse
            {
                NotificationChannel = new NotificationChannel
                {
                    NotificationChannelId = notificationChannel.NotificationChannelId,
                    Name = notificationChannel.Name,
                    IsActive = notificationChannel.IsActive,
                    RequiresVerification = notificationChannel.RequiresVerification,
                    MaxRetryCount = notificationChannel.MaxRetryCount ?? 0
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving notification channel: {Id}", request.NotificationChannelId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to retrieve notification channel"));
        }
    }

    public override async Task<UpdateNotificationChannelResponse> UpdateNotificationChannel(
        UpdateNotificationChannelRequest request, ServerCallContext context)
    {
        try
        {
            var notificationChannel = await notificationChannelRepository.GetByIdAsync(request.NotificationChannelId);
            if (notificationChannel == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification channel with ID {request.NotificationChannelId} not found"));
            }

            notificationChannel.Name = request.Name;
            notificationChannel.IsActive = request.IsActive;
            notificationChannel.RequiresVerification = request.RequiresVerification;
            notificationChannel.MaxRetryCount = request.MaxRetryCount;

            await notificationChannelRepository.UpdateAsync(notificationChannel);

            return new UpdateNotificationChannelResponse
            {
                NotificationChannel = new NotificationChannel
                {
                    NotificationChannelId = notificationChannel.NotificationChannelId,
                    Name = notificationChannel.Name,
                    IsActive = notificationChannel.IsActive,
                    RequiresVerification = notificationChannel.RequiresVerification,
                    MaxRetryCount = notificationChannel.MaxRetryCount ?? 0
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating notification channel: {Id}", request.NotificationChannelId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to update notification channel"));
        }
    }

    public override async Task<DeleteNotificationChannelResponse> DeleteNotificationChannel(
        DeleteNotificationChannelRequest request, ServerCallContext context)
    {
        try
        {
            await notificationChannelRepository.DeleteAsync(request.NotificationChannelId);
            return new DeleteNotificationChannelResponse { Success = true };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting notification channel: {Id}", request.NotificationChannelId);
            return new DeleteNotificationChannelResponse { Success = false };
        }
    }

    public override async Task<ListNotificationChannelsResponse> ListNotificationChannels(
        ListNotificationChannelsRequest request, ServerCallContext context)
    {
        try
        {
            List<NotificationChannelEntity> notificationChannels;
            if (request.ActiveOnly)
            {
                notificationChannels = await notificationChannelRepository.GetActiveChannelsAsync();
            }
            else
            {
                notificationChannels = await notificationChannelRepository.GetAllAsync();
            }

            var response = new ListNotificationChannelsResponse();
            response.NotificationChannels.AddRange(notificationChannels.Select(nc => new NotificationChannel
            {
                NotificationChannelId = nc.NotificationChannelId,
                Name = nc.Name,
                IsActive = nc.IsActive,
                RequiresVerification = nc.RequiresVerification,
                MaxRetryCount = nc.MaxRetryCount ?? 0
            }));

            // In a real implementation, you would implement proper pagination
            response.Pagination = new PaginationResponse
            {
                Page = request.Pagination?.Page ?? 1,
                PageSize = request.Pagination?.PageSize ?? notificationChannels.Count,
                TotalCount = notificationChannels.Count,
                TotalPages = 1
            };

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing notification channels");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to list notification channels"));
        }
    }

    // Notification Template Management
    public override async Task<CreateNotificationTemplateResponse> CreateNotificationTemplate(
        CreateNotificationTemplateRequest request, ServerCallContext context)
    {
        try
        {
            var notificationTemplate = new NotificationTemplateEntity
            {
                NotificationTypeId = request.NotificationTypeId,
                SubjectTemplate = request.SubjectTemplate,
                MessageTemplate = request.MessageTemplate,
                IsActive = request.IsActive
            };

            await notificationTemplateRepository.AddAsync(notificationTemplate);

            // Handle channel associations
            foreach (var channelId in request.ChannelIds)
            {
                var templateChannel = new NotificationTemplateChannelEntity
                {
                    NotificationTemplateId = notificationTemplate.NotificationTemplateId,
                    NotificationChannelId = channelId
                };
                // In a real implementation, you would save these associations
            }

            return new CreateNotificationTemplateResponse
            {
                NotificationTemplate = new NotificationTemplate
                {
                    NotificationTemplateId = notificationTemplate.NotificationTemplateId,
                    NotificationTypeId = notificationTemplate.NotificationTypeId,
                    SubjectTemplate = notificationTemplate.SubjectTemplate,
                    MessageTemplate = notificationTemplate.MessageTemplate,
                    IsActive = notificationTemplate.IsActive,
                    ChannelIds = { request.ChannelIds }
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating notification template");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to create notification template"));
        }
    }

    public override async Task<GetNotificationTemplateResponse> GetNotificationTemplate(
        GetNotificationTemplateRequest request, ServerCallContext context)
    {
        try
        {
            var notificationTemplate = await notificationTemplateRepository.GetWithChannelsAsync(
                request.NotificationTemplateId);

            if (notificationTemplate == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification template with ID {request.NotificationTemplateId} not found"));
            }

            return new GetNotificationTemplateResponse
            {
                NotificationTemplate = new NotificationTemplate
                {
                    NotificationTemplateId = notificationTemplate.NotificationTemplateId,
                    NotificationTypeId = notificationTemplate.NotificationTypeId,
                    SubjectTemplate = notificationTemplate.SubjectTemplate,
                    MessageTemplate = notificationTemplate.MessageTemplate,
                    IsActive = notificationTemplate.IsActive,
                    ChannelIds =
                        { notificationTemplate.NotificationTemplateChannels.Select(tc => tc.NotificationChannelId) }
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving notification template: {Id}", request.NotificationTemplateId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to retrieve notification template"));
        }
    }

    public override async Task<UpdateNotificationTemplateResponse> UpdateNotificationTemplate(
        UpdateNotificationTemplateRequest request, ServerCallContext context)
    {
        try
        {
            var notificationTemplate =
                await notificationTemplateRepository.GetByIdAsync(request.NotificationTemplateId);
            if (notificationTemplate == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification template with ID {request.NotificationTemplateId} not found"));
            }

            notificationTemplate.NotificationTypeId = request.NotificationTypeId;
            notificationTemplate.SubjectTemplate = request.SubjectTemplate;
            notificationTemplate.MessageTemplate = request.MessageTemplate;
            notificationTemplate.IsActive = request.IsActive;

            await notificationTemplateRepository.UpdateAsync(notificationTemplate);

            // In a real implementation, you would also update channel associations

            return new UpdateNotificationTemplateResponse
            {
                NotificationTemplate = new NotificationTemplate
                {
                    NotificationTemplateId = notificationTemplate.NotificationTemplateId,
                    NotificationTypeId = notificationTemplate.NotificationTypeId,
                    SubjectTemplate = notificationTemplate.SubjectTemplate,
                    MessageTemplate = notificationTemplate.MessageTemplate,
                    IsActive = notificationTemplate.IsActive,
                    ChannelIds = { request.ChannelIds }
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating notification template: {Id}", request.NotificationTemplateId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to update notification template"));
        }
    }

    public override async Task<DeleteNotificationTemplateResponse> DeleteNotificationTemplate(
        DeleteNotificationTemplateRequest request, ServerCallContext context)
    {
        try
        {
            await notificationTemplateRepository.DeleteAsync(request.NotificationTemplateId);
            return new DeleteNotificationTemplateResponse { Success = true };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting notification template: {Id}", request.NotificationTemplateId);
            return new DeleteNotificationTemplateResponse { Success = false };
        }
    }

    public override async Task<ListNotificationTemplatesResponse> ListNotificationTemplates(
        ListNotificationTemplatesRequest request, ServerCallContext context)
    {
        try
        {
            List<NotificationTemplateEntity> notificationTemplates;
            if (request.HasNotificationTypeId)
            {
                notificationTemplates = await notificationTemplateRepository.GetByTypeWithChannelsAsync(
                    request.NotificationTypeId);
            }
            else
            {
                notificationTemplates = await notificationTemplateRepository.GetAllAsync();
            }

            // Filter by active if requested
            if (request.HasActiveOnly && request.ActiveOnly)
            {
                notificationTemplates = notificationTemplates.Where(t => t.IsActive).ToList();
            }

            var response = new ListNotificationTemplatesResponse();
            response.NotificationTemplates.AddRange(notificationTemplates.Select(nt => new NotificationTemplate
            {
                NotificationTemplateId = nt.NotificationTemplateId,
                NotificationTypeId = nt.NotificationTypeId,
                SubjectTemplate = nt.SubjectTemplate,
                MessageTemplate = nt.MessageTemplate,
                IsActive = nt.IsActive,
                ChannelIds = { nt.NotificationTemplateChannels.Select(tc => tc.NotificationChannelId) }
            }));

            // In a real implementation, you would implement proper pagination
            response.Pagination = new PaginationResponse
            {
                Page = request.Pagination?.Page ?? 1,
                PageSize = request.Pagination?.PageSize ?? notificationTemplates.Count,
                TotalCount = notificationTemplates.Count,
                TotalPages = 1
            };

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing notification templates");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to list notification templates"));
        }
    }

    // User Notification Preferences
    public override async Task<SetUserNotificationPreferenceResponse> SetUserNotificationPreference(
        SetUserNotificationPreferenceRequest request, ServerCallContext context)
    {
        try
        {
            var preference = new UserNotificationPreferenceEntity
            {
                UserId = request.UserId,
                NotificationTypeId = request.NotificationTypeId,
                NotificationChannelId = request.NotificationChannelId,
                IsEnabled = request.IsEnabled
            };

            var result = await userNotificationPreferenceRepository.UpsertPreferenceAsync(preference);

            return new SetUserNotificationPreferenceResponse
            {
                Preference = new UserNotificationPreference
                {
                    UserNotificationPreferenceId = result.UserNotificationPreferenceId,
                    UserId = result.UserId,
                    NotificationTypeId = result.NotificationTypeId,
                    NotificationChannelId = result.NotificationChannelId,
                    IsEnabled = result.IsEnabled
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting user notification preference");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to set user notification preference"));
        }
    }

    public override async Task<GetUserNotificationPreferenceResponse> GetUserNotificationPreference(
        GetUserNotificationPreferenceRequest request, ServerCallContext context)
    {
        try
        {
            var preference = await userNotificationPreferenceRepository.GetPreferenceAsync(
                request.UserId, request.NotificationTypeId, request.NotificationChannelId);

            if (preference == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "User notification preference not found"));
            }

            return new GetUserNotificationPreferenceResponse
            {
                Preference = new UserNotificationPreference
                {
                    UserNotificationPreferenceId = preference.UserNotificationPreferenceId,
                    UserId = preference.UserId,
                    NotificationTypeId = preference.NotificationTypeId,
                    NotificationChannelId = preference.NotificationChannelId,
                    IsEnabled = preference.IsEnabled
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user notification preference");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to retrieve user notification preference"));
        }
    }

    public override async Task<ListUserNotificationPreferencesResponse> ListUserNotificationPreferences(
        ListUserNotificationPreferencesRequest request, ServerCallContext context)
    {
        try
        {
            var preferences = await userNotificationPreferenceRepository.GetByUserIdAsync(request.UserId);

            var response = new ListUserNotificationPreferencesResponse();
            response.Preferences.AddRange(preferences.Select(p => new UserNotificationPreference
            {
                UserNotificationPreferenceId = p.UserNotificationPreferenceId,
                UserId = p.UserId,
                NotificationTypeId = p.NotificationTypeId,
                NotificationChannelId = p.NotificationChannelId,
                IsEnabled = p.IsEnabled
            }));

            // In a real implementation, you would implement proper pagination
            response.Pagination = new PaginationResponse
            {
                Page = request.Pagination?.Page ?? 1,
                PageSize = request.Pagination?.PageSize ?? preferences.Count,
                TotalCount = preferences.Count,
                TotalPages = 1
            };

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing user notification preferences");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to list user notification preferences"));
        }
    }

    // Notification Management
    public override async Task<CreateNotificationResponse> CreateNotification(
        CreateNotificationRequest request, ServerCallContext context)
    {
        try
        {
            var notification = new NotificationEntity
            {
                NotificationTypeId = request.NotificationTypeId,
                Importance = (Oip.Notifications.Contexts.ImportanceLevel)(int)request.Importance,
                CreatedAt = DateTimeOffset.UtcNow,
                DataJson = request.DataJson
            };

            await notificationRepository.AddAsync(notification);

            return new CreateNotificationResponse
            {
                Notification = new Notification
                {
                    NotificationId = notification.NotificationId,
                    NotificationTypeId = notification.NotificationTypeId,
                    Importance = (ImportanceLevel)(int)request.Importance,
                    CreatedAt = notification.CreatedAt.ToString("o"),
                    DataJson = notification.DataJson
                    // NotificationUsers would be populated in a real implementation
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating notification");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to create notification"));
        }
    }

    public override async Task<GetNotificationResponse> GetNotification(
        GetNotificationRequest request, ServerCallContext context)
    {
        try
        {
            var notification = await notificationRepository.GetWithUsersAsync(request.NotificationId);
            if (notification == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification with ID {request.NotificationId} not found"));
            }

            var notificationUsers = notification.NotificationUsers.Select(nu => new NotificationUser
            {
                NotificationUserId = nu.NotificationUserId,
                NotificationId = nu.NotificationId,
                UserId = nu.UserId,
                Subject = nu.Subject,
                Message = nu.Message
            }).ToList();

            return new GetNotificationResponse
            {
                Notification = new Notification
                {
                    NotificationId = notification.NotificationId,
                    NotificationTypeId = notification.NotificationTypeId,
                    Importance = (ImportanceLevel)(int)notification.Importance,
                    CreatedAt = notification.CreatedAt.ToString("o"),
                    DataJson = notification.DataJson,
                    NotificationUsers = { notificationUsers }
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving notification: {Id}", request.NotificationId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to retrieve notification"));
        }
    }

    public override async Task<ListNotificationsResponse> ListNotifications(
        ListNotificationsRequest request, ServerCallContext context)
    {
        try
        {
            List<NotificationEntity> notifications;

            if (request.HasUserId)
            {
                notifications = await notificationRepository.GetByUserIdAsync(request.UserId);
            }
            else if (request.HasNotificationTypeId)
            {
                notifications = await notificationRepository.GetByTypeAsync(request.NotificationTypeId);
            }
            else if (request.HasImportanceFilter)
            {
                notifications = await notificationRepository.GetByImportanceAsync(
                    (Oip.Notifications.Contexts.ImportanceLevel)(int)request.ImportanceFilter);
            }
            else if (!string.IsNullOrEmpty(request.CreatedAfter))
            {
                if (DateTimeOffset.TryParse(request.CreatedAfter, out var date))
                {
                    notifications = await notificationRepository.GetCreatedAfterAsync(date);
                }
                else
                {
                    notifications = await notificationRepository.GetAllAsync();
                }
            }
            else
            {
                notifications = await notificationRepository.GetAllAsync();
            }

            var response = new ListNotificationsResponse();
            response.Notifications.AddRange(notifications.Select(n => new Notification
            {
                NotificationId = n.NotificationId,
                NotificationTypeId = n.NotificationTypeId,
                Importance = (ImportanceLevel)(int)n.Importance,
                CreatedAt = n.CreatedAt.ToString("o"),
                DataJson = n.DataJson
                // NotificationUsers would be populated in a real implementation
            }));

            // In a real implementation, you would implement proper pagination
            response.Pagination = new PaginationResponse
            {
                Page = request.Pagination?.Page ?? 1,
                PageSize = request.Pagination?.PageSize ?? notifications.Count,
                TotalCount = notifications.Count,
                TotalPages = 1
            };

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing notifications");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to list notifications"));
        }
    }

    // Notification Delivery Management
    public override async Task<GetNotificationDeliveryResponse> GetNotificationDelivery(
        GetNotificationDeliveryRequest request, ServerCallContext context)
    {
        try
        {
            var delivery = await notificationDeliveryRepository.GetByIdAsync(request.NotificationDeliveryId);
            if (delivery == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification delivery with ID {request.NotificationDeliveryId} not found"));
            }

            return new GetNotificationDeliveryResponse
            {
                Delivery = new NotificationDelivery
                {
                    NotificationDeliveryId = delivery.NotificationDeliveryId,
                    NotificationUserId = delivery.NotificationUserId,
                    UserId = delivery.UserId,
                    NotificationChannelId = delivery.NotificationChannelId,
                    Status = (DeliveryStatus)(int)delivery.Status,
                    ExternalId = delivery.ExternalId,
                    ErrorMessage = delivery.ErrorMessage,
                    RetryCount = delivery.RetryCount,
                    CreatedAt = delivery.CreatedAt.ToString("o"),
                    SentAt = delivery.SentAt?.ToString("o"),
                    DeliveredAt = delivery.DeliveredAt?.ToString("o")
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving notification delivery: {Id}", request.NotificationDeliveryId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to retrieve notification delivery"));
        }
    }

    public override async Task<UpdateNotificationDeliveryStatusResponse> UpdateNotificationDeliveryStatus(
        UpdateNotificationDeliveryStatusRequest request, ServerCallContext context)
    {
        try
        {
            await notificationDeliveryRepository.UpdateStatusAsync(
                request.NotificationDeliveryId,
                (Oip.Notifications.Contexts.DeliveryStatus)(int)request.Status,
                request.ExternalId,
                request.ErrorMessage);

            var delivery = await notificationDeliveryRepository.GetByIdAsync(request.NotificationDeliveryId);

            if (delivery != null)
            {
                return new UpdateNotificationDeliveryStatusResponse
                {
                    Delivery = new NotificationDelivery
                    {
                        NotificationDeliveryId = delivery.NotificationDeliveryId,
                        NotificationUserId = delivery.NotificationUserId,
                        UserId = delivery.UserId,
                        NotificationChannelId = delivery.NotificationChannelId,
                        Status = (DeliveryStatus)(int)delivery.Status,
                        ExternalId = delivery.ExternalId,
                        ErrorMessage = delivery.ErrorMessage,
                        RetryCount = delivery.RetryCount,
                        CreatedAt = delivery.CreatedAt.ToString("o"),
                        SentAt = delivery.SentAt?.ToString("o"),
                        DeliveredAt = delivery.DeliveredAt?.ToString("o")
                    }
                };
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating notification delivery status: {Id}", request.NotificationDeliveryId);
        }

        throw new RpcException(new Status(StatusCode.Internal, "Failed to update notification delivery status"));
    }

    public override async Task<ListNotificationDeliveriesResponse> ListNotificationDeliveries(
        ListNotificationDeliveriesRequest request, ServerCallContext context)
    {
        try
        {
            List<NotificationDeliveryEntity> deliveries;

            if (request.HasUserId)
            {
                deliveries = await notificationDeliveryRepository.GetByUserIdAsync(request.UserId);
            }
            else if (request.HasNotificationChannelId)
            {
                deliveries =
                    await notificationDeliveryRepository.GetByChannelIdAsync(request.NotificationChannelId);
            }
            else if (request.HasStatusFilter)
            {
                deliveries = await notificationDeliveryRepository.GetByStatusAsync(
                    (Oip.Notifications.Contexts.DeliveryStatus)(int)request.StatusFilter);
            }
            else
            {
                deliveries = await notificationDeliveryRepository.GetAllAsync();
            }

            var response = new ListNotificationDeliveriesResponse();
            response.Deliveries.AddRange(deliveries.Select(d => new NotificationDelivery
            {
                NotificationDeliveryId = d.NotificationDeliveryId,
                NotificationUserId = d.NotificationUserId,
                UserId = d.UserId,
                NotificationChannelId = d.NotificationChannelId,
                Status = (DeliveryStatus)(int)d.Status,
                ExternalId = d.ExternalId,
                ErrorMessage = d.ErrorMessage,
                RetryCount = d.RetryCount,
                CreatedAt = d.CreatedAt.ToString("o"),
                SentAt = d.SentAt?.ToString("o"),
                DeliveredAt = d.DeliveredAt?.ToString("o")
            }));

            // In a real implementation, you would implement proper pagination
            response.Pagination = new PaginationResponse
            {
                Page = request.Pagination?.Page ?? 1,
                PageSize = request.Pagination?.PageSize ?? deliveries.Count,
                TotalCount = deliveries.Count,
                TotalPages = 1
            };

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing notification deliveries");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to list notification deliveries"));
        }
    }

    // Event Handling
    public override async Task<HandleNotificationEventResponse> HandleNotificationEvent(
        HandleNotificationEventRequest request, ServerCallContext context)
    {
        try
        {
            // In a real implementation, you would process the event and create notifications
            // based on the event type and data
            var createdNotificationIds = new List<long>();

            // For now, we'll just return an empty response
            return new HandleNotificationEventResponse
            {
                CreatedNotificationIds = { createdNotificationIds }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling notification event: {EventType}", request.EventType);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to handle notification event"));
        }
    }
}